using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using SolidWorks.Interop.swpublished;

namespace SWAutoAttributes
{
    [ComVisible(true)]
    [Guid("b28bf6d5-6185-bbed-0e53-9b04a8317385")]
    [ProgId("SWAutoAttributes.Addin")]
    [ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class Addin : ISwAddin
    {
        private const int MainCmdGroupId = 9123;
        private const int MainCmdId = 1;

        private SldWorks _swApp;
        private int _addinId;
        private CommandManager _cmdMgr;
        private AddinSettings _settings;

        static Addin()
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssemblyFromAddinFolder;
        }

        public bool ConnectToSW(object ThisSW, int cookie)
        {
            try
            {
                _swApp = (SldWorks)ThisSW;
                _addinId = cookie;

                int hInst = 0;
                try
                {
                    hInst = Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().ManifestModule).ToInt32();
                }
                catch
                {
                    hInst = 0;
                }

                _swApp.SetAddinCallbackInfo2(hInst, this, _addinId);
                _settings = SettingsStore.Load();

                CreateCommandMgr();
                _swApp.ActiveDocChangeNotify += OnActiveDocChangeNotify;
                ApplyToActiveDoc(true);
                return true;
            }
            catch (Exception ex)
            {
                LogError("ConnectToSW failed.", ex);
                MessageBox.Show($"插件加载失败：{ex.Message}\n\n日志：{GetLogPath()}", "文件名属性", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool DisconnectFromSW()
        {
            try
            {
                if (_swApp != null)
                {
                    _swApp.ActiveDocChangeNotify -= OnActiveDocChangeNotify;
                }
                RemoveCommandMgr();
                _cmdMgr = null;
                _swApp = null;
                return true;
            }
            catch (Exception ex)
            {
                LogError("DisconnectFromSW failed.", ex);
                return false;
            }
        }

        private void CreateCommandMgr()
        {
            _cmdMgr = _swApp.GetCommandManager(_addinId);
            int cmdGroupErr = 0;
            var cmdGroup = _cmdMgr.CreateCommandGroup2(
                MainCmdGroupId,
                "文件名属性",
                "从文件名提取字符并写入自定义属性",
                string.Empty,
                -1,
                true,
                ref cmdGroupErr);

            cmdGroup.AddCommandItem2(
                "设置/执行",
                -1,
                "设置并执行",
                "设置/执行",
                0,
                nameof(OnShowSettings),
                string.Empty,
                MainCmdId,
                (int)(swCommandItemType_e.swMenuItem | swCommandItemType_e.swToolbarItem));

            cmdGroup.HasToolbar = true;
            cmdGroup.HasMenu = true;
            cmdGroup.Activate();
        }

        private void RemoveCommandMgr()
        {
            if (_cmdMgr != null)
            {
                _cmdMgr.RemoveCommandGroup(MainCmdGroupId);
            }
        }

        private int OnActiveDocChangeNotify()
        {
            ApplyToActiveDoc(true);
            return 0;
        }

        public void OnShowSettings()
        {
            using (var form = new SettingsForm(_settings?.Clone(), ApplySettingsAndRun))
            {
                var frame = _swApp.IFrameObject();
                if (frame != null)
                {
                    var mainHwnd = new IntPtr(frame.GetHWnd());
                    var window = new NativeWindow();
                    window.AssignHandle(mainHwnd);
                    form.ShowDialog(window);
                    window.ReleaseHandle();
                }
                else
                {
                    form.ShowDialog();
                }
            }
        }

        private void ApplySettingsAndRun(AddinSettings newSettings)
        {
            _settings = newSettings.Clone();
            SettingsStore.Save(_settings);
            ApplyToActiveDoc(false);
        }

        private void ApplyToActiveDoc(bool silent)
        {
            if (_swApp == null)
            {
                return;
            }

            var model = _swApp.IActiveDoc2 as ModelDoc2;
            if (model == null)
            {
                if (!silent)
                {
                    SendMessage("没有可处理的文档。", swMessageBoxIcon_e.swMbInformation);
                }
                return;
            }

            ApplyToModel(model, silent);
        }

        private void ApplyToModel(ModelDoc2 model, bool silent)
        {
            if (!IsSupportedDoc(model))
            {
                if (!silent)
                {
                    SendMessage("仅支持零件和装配体。", swMessageBoxIcon_e.swMbInformation);
                }
                return;
            }

            if (_settings?.Rules == null || _settings.Rules.Count == 0)
            {
                if (!silent)
                {
                    SendMessage("未配置规则。", swMessageBoxIcon_e.swMbInformation);
                }
                return;
            }

            if (!TryGetFileNameBase(model, out var fileName, out var nameError))
            {
                if (!silent)
                {
                    SendMessage(nameError, swMessageBoxIcon_e.swMbStop);
                }
                else
                {
                    LogError("GetFileNameBase failed.", new InvalidOperationException(nameError));
                }
                return;
            }

            int processed = 0;
            int failed = 0;
            var errors = new List<string>();

            foreach (var rule in _settings.Rules)
            {
                string value;
                string error;

                if (rule.Type == RuleType.FixedValue)
                {
                    value = rule.FixedValue ?? string.Empty;
                }
                else
                {
                    if (!TryGetValueFromFileName(fileName, rule, out value, out error))
                    {
                        failed++;
                        errors.Add($"{rule.PropertyName}: {error}");
                        continue;
                    }
                }

                if (!TrySetCustomProperty(model, rule.PropertyName, value, out error))
                {
                    failed++;
                    errors.Add($"{rule.PropertyName}: {error}");
                    continue;
                }

                processed++;
            }

            if (silent)
            {
                if (errors.Count > 0)
                {
                    LogError("Auto apply failed: " + string.Join("; ", errors), new InvalidOperationException("AutoApply"));
                }
                return;
            }

            var summary = $"完成：{processed}，失败：{failed}";
            if (errors.Count > 0)
            {
                summary += "\n" + string.Join("\n", errors);
            }

            SendMessage(summary, failed > 0 ? swMessageBoxIcon_e.swMbStop : swMessageBoxIcon_e.swMbInformation);
        }

        private static bool IsSupportedDoc(ModelDoc2 model)
        {
            int type = model.GetType();
            return type == (int)swDocumentTypes_e.swDocPART || type == (int)swDocumentTypes_e.swDocASSEMBLY;
        }

        private static bool TryGetFileNameBase(ModelDoc2 model, out string name, out string error)
        {
            name = string.Empty;
            error = string.Empty;

            var path = model.GetPathName();
            name = string.IsNullOrWhiteSpace(path)
                ? model.GetTitle()
                : Path.GetFileName(path);

            if (string.IsNullOrWhiteSpace(name))
            {
                error = "文件名为空。";
                return false;
            }

            name = Path.GetFileNameWithoutExtension(name);
            name = name.TrimEnd('*');
            if (string.IsNullOrEmpty(name))
            {
                error = "无法解析文件名。";
                return false;
            }

            return true;
        }

        private static bool TryGetValueFromFileName(string fileName, PropertyRule rule, out string value, out string error)
        {
            value = string.Empty;
            error = string.Empty;

            if (string.IsNullOrEmpty(fileName))
            {
                error = "文件名为空。";
                return false;
            }

            int start = rule.StartIndex;
            int end = rule.EndIndex;

            if (start < 1)
            {
                error = "起始位必须>=1。";
                return false;
            }

            if (end < 0)
            {
                error = "结束位不能小于0。";
                return false;
            }

            if (end != 0 && end < start)
            {
                error = "结束位必须>=起始位，或为0表示到末尾。";
                return false;
            }

            if (start > fileName.Length)
            {
                value = string.Empty;
                return true;
            }

            int actualEnd = end == 0 || end > fileName.Length ? fileName.Length : end;
            int length = actualEnd - start + 1;
            if (length <= 0)
            {
                value = string.Empty;
                return true;
            }

            value = fileName.Substring(start - 1, length);
            return true;
        }

        private static bool TrySetCustomProperty(ModelDoc2 model, string propertyName, string value, out string error)
        {
            error = string.Empty;
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                error = "属性名为空。";
                return false;
            }

            var mgr = model.Extension.CustomPropertyManager[string.Empty];
            if (mgr == null)
            {
                error = "无法获取自定义属性管理器。";
                return false;
            }

            int result = mgr.Add3(
                propertyName,
                (int)swCustomInfoType_e.swCustomInfoText,
                value,
                (int)swCustomPropertyAddOption_e.swCustomPropertyReplaceValue);

            if (result != (int)swCustomInfoAddResult_e.swCustomInfoAddResult_AddedOrChanged)
            {
                error = "写入属性失败。";
                return false;
            }

            return true;
        }

        private void SendMessage(string message, swMessageBoxIcon_e icon)
        {
            _swApp.SendMsgToUser2(message, (int)icon, (int)swMessageBoxBtn_e.swMbOk);
        }

        private static Assembly ResolveAssemblyFromAddinFolder(object sender, ResolveEventArgs args)
        {
            try
            {
                var name = new AssemblyName(args.Name).Name + ".dll";
                var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (string.IsNullOrWhiteSpace(baseDir))
                {
                    return null;
                }

                var candidate = Path.Combine(baseDir, name);
                return File.Exists(candidate) ? Assembly.LoadFrom(candidate) : null;
            }
            catch
            {
                return null;
            }
        }

        private static void LogError(string message, Exception ex)
        {
            try
            {
                File.AppendAllText(GetLogPath(),
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}{System.Environment.NewLine}{ex}{System.Environment.NewLine}");
            }
            catch
            {
                // ignore logging failures
            }
        }

        private static string GetLogPath()
        {
            var dir = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "SWAutoAttributes");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, "addin.log");
        }

        [ComRegisterFunction]
        public static void RegisterFunction(Type t)
        {
            try
            {
                using (var hkLm = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\\SolidWorks\\Addins\\" + t.GUID.ToString("B")))
                {
                    if (hkLm != null)
                    {
                        hkLm.SetValue(null, 0, RegistryValueKind.DWord);
                        hkLm.SetValue("Description", "从文件名提取字符并写入自定义属性", RegistryValueKind.String);
                        hkLm.SetValue("Title", "文件名属性", RegistryValueKind.String);
                    }
                }

                using (var hkCu = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\\SolidWorks\\AddinsStartup\\" + t.GUID.ToString("B")))
                {
                    if (hkCu != null)
                    {
                        hkCu.SetValue(null, 1, RegistryValueKind.DWord);
                    }
                }
            }
            catch
            {
                // 需要管理员权限时，SolidWorks 仍可手动加载 DLL
            }
        }

        [ComUnregisterFunction]
        public static void UnregisterFunction(Type t)
        {
            try
            {
                Registry.LocalMachine.DeleteSubKeyTree(@"SOFTWARE\\SolidWorks\\Addins\\" + t.GUID.ToString("B"), false);
                Registry.CurrentUser.DeleteSubKeyTree(@"SOFTWARE\\SolidWorks\\AddinsStartup\\" + t.GUID.ToString("B"), false);
            }
            catch
            {
                // ignore
            }
        }
    }
}
