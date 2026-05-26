using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;

namespace SWAutoAttributes
{
    internal enum RuleType
    {
        FileNameRange = 0,
        FixedValue = 1
    }

    internal sealed class PropertyRule
    {
        public string PropertyName { get; set; } = string.Empty;
        public RuleType Type { get; set; } = RuleType.FileNameRange;
        public int StartIndex { get; set; } = 1;
        public int EndIndex { get; set; } = 1;
        public string FixedValue { get; set; } = string.Empty;

        public PropertyRule Clone()
        {
            return new PropertyRule
            {
                PropertyName = PropertyName,
                Type = Type,
                StartIndex = StartIndex,
                EndIndex = EndIndex,
                FixedValue = FixedValue
            };
        }
    }

    internal sealed class AddinSettings
    {
        public List<PropertyRule> Rules { get; set; } = new List<PropertyRule>();

        public AddinSettings Clone()
        {
            return new AddinSettings
            {
                Rules = Rules.Select(r => r.Clone()).ToList()
            };
        }
    }

    internal static class SettingsStore
    {
        private const string BaseKey = @"Software\\SWAutoAttributes";
        private const string RulesValueName = "Rules";

        public static AddinSettings Load()
        {
            var settings = new AddinSettings();
            using (var key = Registry.CurrentUser.OpenSubKey(BaseKey, false))
            {
                if (key == null)
                {
                    settings.Rules.Add(DefaultRule());
                    return settings;
                }

                var raw = ReadString(key, RulesValueName, string.Empty);
                settings.Rules = DeserializeRules(raw);
                if (settings.Rules.Count == 0)
                {
                    settings.Rules.Add(DefaultRule());
                }
            }
            return settings;
        }

        public static void Save(AddinSettings settings)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(BaseKey))
            {
                if (key == null)
                {
                    return;
                }

                var raw = SerializeRules(settings.Rules ?? new List<PropertyRule>());
                key.SetValue(RulesValueName, raw, RegistryValueKind.String);
            }
        }

        private static string SerializeRules(List<PropertyRule> rules)
        {
            if (rules == null || rules.Count == 0)
            {
                return string.Empty;
            }

            return string.Join(";", rules.Select(r =>
                string.Join("|",
                    (int)r.Type,
                    Escape(r.PropertyName),
                    r.StartIndex,
                    r.EndIndex,
                    Escape(r.FixedValue))));
        }

        private static List<PropertyRule> DeserializeRules(string raw)
        {
            var rules = new List<PropertyRule>();
            if (string.IsNullOrWhiteSpace(raw))
            {
                return rules;
            }

            foreach (var ruleChunk in raw.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = ruleChunk.Split('|');
                if (parts.Length < 5)
                {
                    continue;
                }

                if (!int.TryParse(parts[0], out var typeValue))
                {
                    continue;
                }

                if (!int.TryParse(parts[2], out var startIndex))
                {
                    startIndex = 1;
                }

                if (!int.TryParse(parts[3], out var endIndex))
                {
                    endIndex = 1;
                }

                var rule = new PropertyRule
                {
                    Type = (RuleType)typeValue,
                    PropertyName = Unescape(parts[1]),
                    StartIndex = startIndex,
                    EndIndex = endIndex,
                    FixedValue = Unescape(parts[4])
                };

                if (!string.IsNullOrWhiteSpace(rule.PropertyName))
                {
                    rules.Add(rule);
                }
            }

            return rules;
        }

        private static PropertyRule DefaultRule()
        {
            return new PropertyRule
            {
                PropertyName = "文件名片段",
                Type = RuleType.FileNameRange,
                StartIndex = 1,
                EndIndex = 1,
                FixedValue = string.Empty
            };
        }

        private static string ReadString(RegistryKey key, string name, string defaultValue)
        {
            var value = key.GetValue(name) as string;
            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }
            return defaultValue;
        }

        private static string Escape(string value)
        {
            return Uri.EscapeDataString(value ?? string.Empty);
        }

        private static string Unescape(string value)
        {
            return Uri.UnescapeDataString(value ?? string.Empty);
        }
    }
}
