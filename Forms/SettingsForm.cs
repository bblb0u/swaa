using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SWAutoAttributes
{
    internal partial class SettingsForm : Form
    {
        private const string TypeRange = "文件名区间";
        private const string TypeFixed = "固定值";
        private readonly Action<AddinSettings> _applyHandler;

        public SettingsForm(AddinSettings settings, Action<AddinSettings> applyHandler)
        {
            _applyHandler = applyHandler;
            InitializeComponent();

            if (settings?.Rules != null && settings.Rules.Count > 0)
            {
                foreach (var rule in settings.Rules)
                {
                    AddRuleRow(rule);
                }
            }
            else
            {
                AddRuleRow(new PropertyRule
                {
                    PropertyName = "文件名片段",
                    Type = RuleType.FileNameRange,
                    StartIndex = 1,
                    EndIndex = 1
                });
            }
        }

        private void AddRuleRow(PropertyRule rule)
        {
            int rowIndex = dgvRules.Rows.Add();
            var row = dgvRules.Rows[rowIndex];
            row.Cells[colName.Name].Value = rule.PropertyName ?? string.Empty;
            row.Cells[colType.Name].Value = rule.Type == RuleType.FixedValue ? TypeFixed : TypeRange;
            row.Cells[colStart.Name].Value = rule.StartIndex;
            row.Cells[colEnd.Name].Value = rule.EndIndex == 0 ? "0" : rule.EndIndex.ToString();
            row.Cells[colFixed.Name].Value = rule.FixedValue ?? string.Empty;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            AddRuleRow(new PropertyRule
            {
                PropertyName = "新属性",
                Type = RuleType.FileNameRange,
                StartIndex = 1,
                EndIndex = 1
            });
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            if (dgvRules.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in dgvRules.SelectedRows)
                {
                    if (!row.IsNewRow)
                    {
                        dgvRules.Rows.Remove(row);
                    }
                }
                return;
            }

            if (dgvRules.Rows.Count > 0)
            {
                var row = dgvRules.Rows[dgvRules.Rows.Count - 1];
                if (!row.IsNewRow)
                {
                    dgvRules.Rows.Remove(row);
                }
            }
        }

        private void BtnApply_Click(object sender, EventArgs e)
        {
            var rules = new List<PropertyRule>();

            foreach (DataGridViewRow row in dgvRules.Rows)
            {
                if (row.IsNewRow)
                {
                    continue;
                }

                var name = Convert.ToString(row.Cells[colName.Name].Value)?.Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    MessageBox.Show(this, "属性名不能为空。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var typeText = Convert.ToString(row.Cells[colType.Name].Value);
                var type = typeText == TypeFixed ? RuleType.FixedValue : RuleType.FileNameRange;

                int start = 1;
                int end = 0;
                var startText = Convert.ToString(row.Cells[colStart.Name].Value);
                var endText = Convert.ToString(row.Cells[colEnd.Name].Value);

                if (type == RuleType.FileNameRange)
                {
                    if (!int.TryParse(startText, out start) || start < 1)
                    {
                        MessageBox.Show(this, "起始位必须是>=1的整数。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(endText))
                    {
                        MessageBox.Show(this, "结束位必须填写，0表示到末尾。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (!int.TryParse(endText, out end) || end < 0)
                    {
                        MessageBox.Show(this, "结束位必须是>=0的整数（0表示到末尾）。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                var fixedValue = Convert.ToString(row.Cells[colFixed.Name].Value) ?? string.Empty;

                rules.Add(new PropertyRule
                {
                    PropertyName = name,
                    Type = type,
                    StartIndex = start,
                    EndIndex = end,
                    FixedValue = fixedValue
                });
            }

            if (rules.Count == 0)
            {
                MessageBox.Show(this, "请至少添加一条规则。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var settings = new AddinSettings
            {
                Rules = rules
            };

            _applyHandler?.Invoke(settings);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
