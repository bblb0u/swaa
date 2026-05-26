using System.Windows.Forms;

namespace SWAutoAttributes
{
    internal partial class SettingsForm
    {
        private DataGridView dgvRules;
        private DataGridViewTextBoxColumn colName;
        private DataGridViewComboBoxColumn colType;
        private DataGridViewTextBoxColumn colStart;
        private DataGridViewTextBoxColumn colEnd;
        private DataGridViewTextBoxColumn colFixed;
        private Label lblHint;
        private Button btnAdd;
        private Button btnRemove;
        private Button btnApply;
        private Button btnCancel;

        private void InitializeComponent()
        {
            dgvRules = new DataGridView();
            colName = new DataGridViewTextBoxColumn();
            colType = new DataGridViewComboBoxColumn();
            colStart = new DataGridViewTextBoxColumn();
            colEnd = new DataGridViewTextBoxColumn();
            colFixed = new DataGridViewTextBoxColumn();
            lblHint = new Label();
            btnAdd = new Button();
            btnRemove = new Button();
            btnApply = new Button();
            btnCancel = new Button();

            SuspendLayout();

            dgvRules.AllowUserToAddRows = true;
            dgvRules.AllowUserToDeleteRows = true;
            dgvRules.AllowUserToResizeRows = false;
            dgvRules.BackgroundColor = System.Drawing.SystemColors.Window;
            dgvRules.BorderStyle = BorderStyle.FixedSingle;
            dgvRules.ColumnHeadersHeight = 28;
            dgvRules.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvRules.EnableHeadersVisualStyles = false;
            dgvRules.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(242, 242, 242);
            dgvRules.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(40, 40, 40);
            dgvRules.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(248, 248, 248);
            dgvRules.Columns.AddRange(new DataGridViewColumn[]
            {
                colName,
                colType,
                colStart,
                colEnd,
                colFixed
            });
            dgvRules.Location = new System.Drawing.Point(16, 16);
            dgvRules.MultiSelect = true;
            dgvRules.Name = "dgvRules";
            dgvRules.RowHeadersVisible = false;
            dgvRules.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRules.Size = new System.Drawing.Size(820, 230);
            dgvRules.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            colName.HeaderText = "属性名";
            colName.Name = "colName";
            colName.Width = 160;

            colType.HeaderText = "类型";
            colType.Name = "colType";
            colType.Width = 120;
            colType.Items.AddRange(new object[] { "文件名区间", "固定值" });

            colStart.HeaderText = "起始位";
            colStart.Name = "colStart";
            colStart.Width = 80;

            colEnd.HeaderText = "结束位";
            colEnd.Name = "colEnd";
            colEnd.Width = 80;

            colFixed.HeaderText = "固定值";
            colFixed.Name = "colFixed";
            colFixed.Width = 320;

            lblHint.AutoSize = true;
            lblHint.Location = new System.Drawing.Point(16, 255);
            lblHint.Name = "lblHint";
            lblHint.Size = new System.Drawing.Size(240, 15);
            lblHint.Text = "结束位=0 表示到文件名末尾。";

            btnAdd.Location = new System.Drawing.Point(16, 285);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new System.Drawing.Size(90, 28);
            btnAdd.Text = "添加规则";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += BtnAdd_Click;

            btnRemove.Location = new System.Drawing.Point(120, 285);
            btnRemove.Name = "btnRemove";
            btnRemove.Size = new System.Drawing.Size(90, 28);
            btnRemove.Text = "删除规则";
            btnRemove.UseVisualStyleBackColor = true;
            btnRemove.Click += BtnRemove_Click;

            btnApply.AutoSize = true;
            btnApply.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnApply.Padding = new System.Windows.Forms.Padding(12, 0, 12, 0);
            btnApply.Location = new System.Drawing.Point(600, 283);
            btnApply.Name = "btnApply";
            btnApply.Text = "保存并应用";
            btnApply.UseVisualStyleBackColor = true;
            btnApply.Click += BtnApply_Click;
            btnApply.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            btnCancel.AutoSize = true;
            btnCancel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnCancel.Padding = new System.Windows.Forms.Padding(12, 0, 12, 0);
            btnCancel.Location = new System.Drawing.Point(716, 283);
            btnCancel.Name = "btnCancel";
            btnCancel.Text = "关闭";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += BtnCancel_Click;
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(860, 340);
            Controls.Add(dgvRules);
            Controls.Add(lblHint);
            Controls.Add(btnAdd);
            Controls.Add(btnRemove);
            Controls.Add(btnApply);
            Controls.Add(btnCancel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "文件名属性设置";

            ResumeLayout(false);
            PerformLayout();
        }
    }
}
