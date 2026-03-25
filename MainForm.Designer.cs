namespace 自动化计算工具
{
#nullable enable
    partial class MainForm
    {
        private System.ComponentModel.IContainer? components = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Text = "自动化计算工具-王国强-202603";
            this.ClientSize = new System.Drawing.Size(533, 550);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            
            CreateTopButtons();
            CreateTabControl();
            CreateDataTabPage();
            CreateStatusTabPage();
            CreateMemoryTabPage();
        }

        private System.Windows.Forms.Panel? topButtonPanel;
        private System.Windows.Forms.Button? importButton;
        private System.Windows.Forms.Button? listButton;
        private System.Windows.Forms.Button? calculatorButton;
        private System.Windows.Forms.Button? aboutButton;
        private System.Windows.Forms.TabControl? tabControl;
        private System.Windows.Forms.TabPage? dataTabPage;
        private System.Windows.Forms.TabPage? statusTabPage;
        private System.Windows.Forms.TabPage? memoryTabPage;
        private System.Windows.Forms.Label? currentGroupLabel;
        private System.Windows.Forms.Label? firstWordLabel;
        private System.Windows.Forms.TextBox? firstWordEntry;
        private System.Windows.Forms.Label? firstNameLabel;
        private System.Windows.Forms.Button? firstCheckButton;
        private System.Windows.Forms.Label? secondWordLabel;
        private System.Windows.Forms.TextBox? secondWordEntry;
        private System.Windows.Forms.Label? secondNameLabel;
        private System.Windows.Forms.Button? secondCheckButton;
        private System.Windows.Forms.RichTextBox? resultText;
        private System.Windows.Forms.Label? currentStatusLabel;
        private System.Windows.Forms.RichTextBox? statusResultText;
        private System.Windows.Forms.Panel? memoryLeftPanel;
        private System.Windows.Forms.Panel? memoryRightPanel;
        private System.Windows.Forms.GroupBox? calcTypeGroupBox;
        private System.Windows.Forms.RadioButton? calcType1;
        private System.Windows.Forms.RadioButton? calcType2;
        private System.Windows.Forms.RadioButton? calcType3;
        private System.Windows.Forms.RadioButton? calcType4;
        private System.Windows.Forms.GroupBox? memoryInputGroupBox;
        private System.Windows.Forms.Button? memoryCalculateButton;
        private System.Windows.Forms.GroupBox? memoryResultGroupBox;
        private System.Windows.Forms.Label? memoryResultLabel;

        private void CreateTopButtons()
        {
            topButtonPanel = new System.Windows.Forms.Panel
            {
                Dock = System.Windows.Forms.DockStyle.Top,
                Height = 50,
                Padding = new System.Windows.Forms.Padding(20, 10, 20, 10)
            };

            importButton = new System.Windows.Forms.Button
            {
                Text = "导入",
                Width = 80,
                Height = 30,
                Margin = new System.Windows.Forms.Padding(0, 0, 5, 0)
            };
            importButton.Click += ImportButton_Click;

            listButton = new System.Windows.Forms.Button
            {
                Text = "列表",
                Width = 80,
                Height = 30,
                Margin = new System.Windows.Forms.Padding(0, 0, 5, 0)
            };
            listButton.Click += ListButton_Click;

            calculatorButton = new System.Windows.Forms.Button
            {
                Text = "计算器",
                Width = 80,
                Height = 30,
                Margin = new System.Windows.Forms.Padding(0, 0, 5, 0)
            };
            calculatorButton.Click += CalculatorButton_Click;

            aboutButton = new System.Windows.Forms.Button
            {
                Text = "关于",
                Width = 80,
                Height = 30
            };
            aboutButton.Click += AboutButton_Click;

            topButtonPanel.Controls.Add(importButton);
            topButtonPanel.Controls.Add(listButton);
            topButtonPanel.Controls.Add(calculatorButton);
            topButtonPanel.Controls.Add(aboutButton);

            this.Controls.Add(topButtonPanel);
        }

        private void CreateTabControl()
        {
            tabControl = new System.Windows.Forms.TabControl
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                Margin = new System.Windows.Forms.Padding(10, 5, 10, 10),
                Appearance = System.Windows.Forms.TabAppearance.Normal
            };

            dataTabPage = new System.Windows.Forms.TabPage("  打包数据查看  ");
            statusTabPage = new System.Windows.Forms.TabPage("  状态字查看  ");
            memoryTabPage = new System.Windows.Forms.TabPage("  内存映象网计算  ");

            tabControl.TabPages.Add(dataTabPage);
            tabControl.TabPages.Add(statusTabPage);
            tabControl.TabPages.Add(memoryTabPage);

            this.Controls.Add(tabControl);
        }

        private void CreateDataTabPage()
        {
            currentGroupLabel = new System.Windows.Forms.Label
            {
                Text = "当前选择的打包字：未选择",
                Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular),
                ForeColor = System.Drawing.Color.Blue,
                AutoSize = true,
                Location = new System.Drawing.Point(20, 10)
            };
            dataTabPage!.Controls.Add(currentGroupLabel!);

            var inputPanel = new System.Windows.Forms.Panel
            {
                Location = new System.Drawing.Point(30, 40),
                Width = 473,
                Height = 120,
                AutoSize = true
            };

            firstWordLabel = new System.Windows.Forms.Label
            {
                Text = "第一个字(十进制):",
                Location = new System.Drawing.Point(0, 8),
                Width = 120,
                Height = 23
            };

            firstNameLabel = new System.Windows.Forms.Label
            {
                Text = "第一个字",
                Font = new System.Drawing.Font("微软雅黑", 8F),
                ForeColor = System.Drawing.Color.Gray,
                Location = new System.Drawing.Point(125, 8),
                Width = 80,
                Height = 23
            };

            firstWordEntry = new System.Windows.Forms.TextBox
            {
                Location = new System.Drawing.Point(210, 8),
                Width = 200,
                Height = 23
            };

            firstCheckButton = new System.Windows.Forms.Button
            {
                Text = "转换并查询",
                Location = new System.Drawing.Point(420, 8),
                Width = 100,
                Height = 23
            };
            firstCheckButton.Click += (s, e) => CheckFault(1);

            secondWordLabel = new System.Windows.Forms.Label
            {
                Text = "第二个字(十进制):",
                Location = new System.Drawing.Point(0, 48),
                Width = 120,
                Height = 23
            };

            secondNameLabel = new System.Windows.Forms.Label
            {
                Text = "第二个字",
                Font = new System.Drawing.Font("微软雅黑", 8F),
                ForeColor = System.Drawing.Color.Gray,
                Location = new System.Drawing.Point(125, 48),
                Width = 80,
                Height = 23
            };

            secondWordEntry = new System.Windows.Forms.TextBox
            {
                Location = new System.Drawing.Point(210, 48),
                Width = 200,
                Height = 23
            };

            secondCheckButton = new System.Windows.Forms.Button
            {
                Text = "转换并查询",
                Location = new System.Drawing.Point(420, 48),
                Width = 100,
                Height = 23
            };
            secondCheckButton.Click += (s, e) => CheckFault(2);

            inputPanel.Controls.Add(firstWordLabel);
            inputPanel.Controls.Add(firstNameLabel);
            inputPanel.Controls.Add(firstWordEntry);
            inputPanel.Controls.Add(firstCheckButton);
            inputPanel.Controls.Add(secondWordLabel);
            inputPanel.Controls.Add(secondNameLabel);
            inputPanel.Controls.Add(secondWordEntry);
            inputPanel.Controls.Add(secondCheckButton);

            dataTabPage!.Controls.Add(inputPanel);

            resultText = new System.Windows.Forms.RichTextBox
            {
                Location = new System.Drawing.Point(20, 170),
                Size = new System.Drawing.Size(493, 320),
                ReadOnly = true,
                BackColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("微软雅黑", 9F),
                ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical
            };
            dataTabPage!.Controls.Add(resultText);
        }

        private void CreateStatusTabPage()
        {
            currentStatusLabel = new System.Windows.Forms.Label
            {
                Text = "当前选择的状态字：未选择",
                Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular),
                ForeColor = System.Drawing.Color.Blue,
                AutoSize = true,
                Location = new System.Drawing.Point(20, 10)
            };
            statusTabPage!.Controls.Add(currentStatusLabel!);

            statusResultText = new System.Windows.Forms.RichTextBox
            {
                Location = new System.Drawing.Point(20, 40),
                Size = new System.Drawing.Size(493, 450),
                ReadOnly = true,
                BackColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("微软雅黑", 9F),
                ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical
            };
            statusTabPage!.Controls.Add(statusResultText);
        }

        private void CreateMemoryTabPage()
        {
            var mainPanel = new System.Windows.Forms.Panel
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                Padding = new System.Windows.Forms.Padding(10)
            };

            memoryLeftPanel = new System.Windows.Forms.Panel
            {
                Dock = System.Windows.Forms.DockStyle.Left,
                Width = 150
            };

            memoryRightPanel = new System.Windows.Forms.Panel
            {
                Dock = System.Windows.Forms.DockStyle.Fill
            };

            calcTypeGroupBox = new System.Windows.Forms.GroupBox
            {
                Text = "计算类型",
                Dock = System.Windows.Forms.DockStyle.Top,
                Height = 200
            };

            calcType1 = new System.Windows.Forms.RadioButton
            {
                Text = "模拟量计算\r\n16进制地址",
                Location = new System.Drawing.Point(10, 20),
                Width = 130,
                Height = 40,
                Appearance = System.Windows.Forms.Appearance.Button,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };
            calcType1.CheckedChanged += (s, e) => { if (calcType1.Checked) SwitchMemoryCalcType(1); };

            calcType2 = new System.Windows.Forms.RadioButton
            {
                Text = "模拟量计算\r\nR地址",
                Location = new System.Drawing.Point(10, 65),
                Width = 130,
                Height = 40,
                Appearance = System.Windows.Forms.Appearance.Button,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };
            calcType2.CheckedChanged += (s, e) => { if (calcType2.Checked) SwitchMemoryCalcType(2); };

            calcType3 = new System.Windows.Forms.RadioButton
            {
                Text = "数字量计算\r\n16进制地址和位数",
                Location = new System.Drawing.Point(10, 110),
                Width = 130,
                Height = 40,
                Appearance = System.Windows.Forms.Appearance.Button,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };
            calcType3.CheckedChanged += (s, e) => { if (calcType3.Checked) SwitchMemoryCalcType(3); };

            calcType4 = new System.Windows.Forms.RadioButton
            {
                Text = "数字量计算\r\nM地址",
                Location = new System.Drawing.Point(10, 155),
                Width = 130,
                Height = 40,
                Appearance = System.Windows.Forms.Appearance.Button,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };
            calcType4.CheckedChanged += (s, e) => { if (calcType4.Checked) SwitchMemoryCalcType(4); };

            calcTypeGroupBox.Controls.Add(calcType1);
            calcTypeGroupBox.Controls.Add(calcType2);
            calcTypeGroupBox.Controls.Add(calcType3);
            calcTypeGroupBox.Controls.Add(calcType4);

            memoryLeftPanel.Controls.Add(calcTypeGroupBox);

            memoryInputGroupBox = new System.Windows.Forms.GroupBox
            {
                Text = "输入参数",
                Dock = System.Windows.Forms.DockStyle.Top,
                Height = 150,
                Padding = new System.Windows.Forms.Padding(15)
            };

            memoryCalculateButton = new System.Windows.Forms.Button
            {
                Text = "开始计算",
                Width = 150,
                Height = 35
            };
            memoryCalculateButton.Click += MemoryCalculateButton_Click;

            var buttonPanel = new System.Windows.Forms.Panel
            {
                Dock = System.Windows.Forms.DockStyle.Top,
                Height = 50
            };
            buttonPanel.Controls.Add(memoryCalculateButton);
            memoryCalculateButton.Anchor = System.Windows.Forms.AnchorStyles.Top;
            memoryCalculateButton.Location = new System.Drawing.Point(10, 8);

            memoryResultGroupBox = new System.Windows.Forms.GroupBox
            {
                Text = "计算结果",
                Dock = System.Windows.Forms.DockStyle.Fill,
                Padding = new System.Windows.Forms.Padding(15)
            };

            memoryResultLabel = new System.Windows.Forms.Label
            {
                Text = "",
                Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.FromArgb(231, 76, 60),
                AutoSize = true,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Padding = new System.Windows.Forms.Padding(0, 10, 0, 10)
            };

            memoryResultGroupBox.Controls.Add(memoryResultLabel);

            memoryRightPanel.Controls.Add(memoryInputGroupBox);
            memoryRightPanel.Controls.Add(buttonPanel);
            memoryRightPanel.Controls.Add(memoryResultGroupBox);

            mainPanel.Controls.Add(memoryLeftPanel);
            mainPanel.Controls.Add(memoryRightPanel);

            memoryTabPage!.Controls.Add(mainPanel);

            calcType1.Checked = true;
        }

        private System.Collections.Generic.Dictionary<string, System.Windows.Forms.TextBox> memoryEntries = new();
        private int memoryCalcType = 1;
    }
}
