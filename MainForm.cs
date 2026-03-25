using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace 自动化计算工具
{
    public partial class MainForm : Form
    {
        private Dictionary<string, FaultData> firstWordMap = new();
        private Dictionary<string, FaultData> secondWordMap = new();
        private string firstName = "第一个字";
        private string secondName = "第二个字";
        private Dictionary<string, GroupData> groupsData = new();
        private string? currentGroup;
        private Dictionary<string, string> statusWordMap = new();
        private Dictionary<string, GroupData> statusGroupsData = new();
        private string? currentStatusGroup;
        private string currentStatusName = "状态字";

        public MainForm()
        {
            InitializeComponent();
            this.Font = new Font("微软雅黑", 9.5F);
            SwitchMemoryCalcType(1);
        }

        private void ImportButton_Click(object? sender, EventArgs e)
        {
            using var openFileDialog = new OpenFileDialog
            {
                Filter = "JSON文件|*.json|所有文件|*.*",
                Title = "选择配置JSON文件",
                InitialDirectory = "."
            };

            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                string content = File.ReadAllText(openFileDialog.FileName, Encoding.UTF8);
                var (data, decodeMethod) = LoadJsonFile(content);
                ProcessImportedData(data, decodeMethod);
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("请选择有效的JSON文件", "文件不存在", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导入失败：{ex.Message}", "未知错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private (Dictionary<string, JsonElement>, string) LoadJsonFile(string content)
        {
            content = content.Trim();
            
            try
            {
                var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);
                if (data != null)
                    return (data, "明文格式");
            }
            catch (JsonException)
            {
                try
                {
                    byte[] decodedBytes = Convert.FromBase64String(content);
                    string decodedContent = Encoding.UTF8.GetString(decodedBytes);
                    var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(decodedContent);
                    if (data != null)
                        return (data, "Base64编码格式");
                }
                catch (Exception decodeError)
                {
                    throw new Exception(
                        $"无法解析文件内容。\n既不是有效的JSON格式，也不是有效的Base64编码JSON格式。\n\n错误详情：{decodeError.Message}");
                }
            }

            throw new Exception("无法解析JSON文件");
        }

        private void ProcessImportedData(Dictionary<string, JsonElement> data, string decodeMethod)
        {
            if (data.ContainsKey("groups"))
            {
                ProcessGroupedData(data["groups"], decodeMethod);
            }
            else
            {
                ProcessUngroupedData(data, decodeMethod);
            }
        }

        private void ProcessGroupedData(JsonElement groupsElement, string decodeMethod)
        {
            var groupsData = JsonSerializer.Deserialize<Dictionary<string, GroupData>>(groupsElement);
            if (groupsData == null) return;

            bool isStatusFormat = false;
            foreach (var group in groupsData.Values)
            {
                if (group.StatusMap != null || !string.IsNullOrEmpty(group.StatusName))
                {
                    isStatusFormat = true;
                    break;
                }
            }

            if (isStatusFormat)
            {
                statusGroupsData = groupsData;
                var groupNames = new List<string>(statusGroupsData.Keys);
                if (groupNames.Count > 0)
                    LoadStatusGroup(groupNames[0]);
            }
            else
            {
                this.groupsData = groupsData;
                var groupNames = new List<string>(this.groupsData.Keys);
                resultText!.AppendText($"成功导入 {groupNames.Count} 个分组！({decodeMethod})\n", Color.Green);
                resultText!.AppendText($"分组列表：{string.Join(", ", groupNames)}\n");
                if (groupNames.Count > 0)
                    LoadGroup(groupNames[0]);
            }
        }

        private void ProcessUngroupedData(Dictionary<string, JsonElement> data, string decodeMethod)
        {
            bool isStatusFormat = true;
            foreach (var value in data.Values)
            {
                if (value.ValueKind != JsonValueKind.String)
                {
                    isStatusFormat = false;
                    break;
                }
            }

            if (isStatusFormat)
            {
                statusWordMap = data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.GetString() ?? "");
                var statusKeys = new List<string>(statusWordMap.Keys);
                if (statusKeys.Count > 0)
                    LoadStatusWord(statusKeys[0]);
            }
            else
            {
                ValidateUngroupedStructure(data);
                LoadUngroupedFaultData(data, decodeMethod);
            }
        }

        private void ValidateUngroupedStructure(Dictionary<string, JsonElement> data)
        {
            if (!data.ContainsKey("first_word") || data["first_word"].ValueKind != JsonValueKind.Object)
                throw new Exception("JSON中缺少或错误的'first_word'字段（需为对象类型）");
            if (!data.ContainsKey("second_word") || data["second_word"].ValueKind != JsonValueKind.Object)
                throw new Exception("JSON中缺少或错误的'second_word'字段（需为对象类型）");
        }

        private void LoadUngroupedFaultData(Dictionary<string, JsonElement> data, string decodeMethod)
        {
            if (data.ContainsKey("first_name"))
                firstName = data["first_name"].GetString() ?? "第一个字";
            if (data.ContainsKey("second_name"))
                secondName = data["second_name"].GetString() ?? "第二个字";

            firstWordMap = JsonSerializer.Deserialize<Dictionary<string, FaultData>>(data["first_word"]) ?? new();
            secondWordMap = JsonSerializer.Deserialize<Dictionary<string, FaultData>>(data["second_word"]) ?? new();

            firstNameLabel!.Text = firstName;
            secondNameLabel!.Text = secondName;
            currentGroupLabel!.Text = "当前选择的打包字：默认（无分组）";
            resultText!.AppendText($"打包数据对应表导入完成！({decodeMethod})\n", Color.Green);
        }

        private void ListButton_Click(object? sender, EventArgs e)
        {
            if (tabControl?.SelectedTab == dataTabPage)
            {
                if (groupsData.Count == 0)
                {
                    MessageBox.Show("请先导入打包数据JSON配置文件", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                ShowPopupMenu("选择打包字", new List<string>(groupsData.Keys), LoadGroup);
            }
            else if (tabControl?.SelectedTab == statusTabPage)
            {
                if (statusGroupsData.Count == 0 && statusWordMap.Count == 0)
                {
                    MessageBox.Show("请先导入状态字JSON配置文件", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                ShowStatusMenu();
            }
            else
            {
                MessageBox.Show("请在'打包数据查看'或'状态字查看'标签页中使用列表功能", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ShowPopupMenu(string title, List<string> items, Action<string, Form?> loadCommand)
        {
            var popup = new Form
            {
                Text = title,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                StartPosition = FormStartPosition.Manual,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            var mainPanel = new Panel
            {
                Padding = new Padding(10),
                AutoSize = true
            };

            var flowPanel = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = true,
                Width = 300
            };

            foreach (var item in items)
            {
                var btn = new Button
                {
                    Text = item,
                    Width = 140,
                    Height = 35,
                    Margin = new Padding(5)
                };
                btn.Click += (s, e) =>
                {
                    loadCommand(item, popup);
                    popup.Close();
                };
                flowPanel.Controls.Add(btn);
            }

            mainPanel.Controls.Add(flowPanel);
            popup.Controls.Add(mainPanel);

            var location = listButton?.PointToScreen(new Point(0, listButton.Height)) ?? new Point(100, 100);
            popup.Location = location;
            popup.ShowDialog();
        }

        private void ShowStatusMenu()
        {
            var popup = new Form
            {
                Text = "选择状态字",
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                StartPosition = FormStartPosition.Manual,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            var mainPanel = new Panel
            {
                Padding = new Padding(10),
                AutoSize = true
            };

            var flowPanel = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = true,
                Width = 300
            };

            if (statusGroupsData.Count > 0)
            {
                foreach (var groupName in statusGroupsData.Keys)
                {
                    var btn = new Button
                    {
                        Text = groupName,
                        Width = 140,
                        Height = 35,
                        Margin = new Padding(5)
                    };
                    btn.Click += (s, e) =>
                    {
                        LoadStatusGroup(groupName);
                        popup.Close();
                    };
                    flowPanel.Controls.Add(btn);
                }
            }
            else if (statusWordMap.Count > 0)
            {
                foreach (var statusKey in statusWordMap.Keys)
                {
                    var btn = new Button
                    {
                        Text = statusKey,
                        Width = 140,
                        Height = 35,
                        Margin = new Padding(5)
                    };
                    btn.Click += (s, e) =>
                    {
                        LoadStatusWord(statusKey);
                        popup.Close();
                    };
                    flowPanel.Controls.Add(btn);
                }
            }

            mainPanel.Controls.Add(flowPanel);
            popup.Controls.Add(mainPanel);

            var location = listButton?.PointToScreen(new Point(0, listButton.Height)) ?? new Point(100, 100);
            popup.Location = location;
            popup.ShowDialog();
        }

        private void LoadGroup(string groupName, Form? popup = null)
        {
            if (!groupsData.ContainsKey(groupName))
            {
                MessageBox.Show($"分组 '{groupName}' 不存在", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var groupData = groupsData[groupName];
            if (groupData.FirstWord == null || groupData.SecondWord == null)
            {
                MessageBox.Show($"分组 '{groupName}' 中缺少或错误的数据字段", "结构错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            firstName = groupData.FirstName ?? "第一个字";
            secondName = groupData.SecondName ?? "第二个字";
            firstWordMap = groupData.FirstWord;
            secondWordMap = groupData.SecondWord;
            currentGroup = groupName;

            firstNameLabel!.Text = firstName;
            secondNameLabel!.Text = secondName;
            currentGroupLabel!.Text = $"当前选择的打包字：{groupName}";

            resultText!.Clear();
            resultText!.AppendText($"已加载打包字：{groupName}\n", Color.Blue);
            resultText.AppendText($"第一个字名称：{firstName}\n");
            resultText.AppendText($"第二个字名称：{secondName}\n");
            resultText.AppendText("════════════════════════\n", Color.Gray);
            resultText.AppendText("请在输入框中输入十进制数值进行查询\n");

            popup?.Close();
        }

        private string DecimalToBinary(int num, int? maxBits = null)
        {
            string binary;
            if (num >= 0)
            {
                binary = Convert.ToString(num, 2);
            }
            else
            {
                int bits = maxBits ?? 64;
                binary = Convert.ToString((1L << bits) + num, 2);
            }

            if (maxBits == null)
            {
                int maxKey = 0;
                foreach (var key in firstWordMap.Keys)
                {
                    if (int.TryParse(key, out int keyInt) && keyInt > maxKey)
                        maxKey = keyInt;
                }
                maxBits = maxKey + 1;
            }

            return binary.PadLeft(maxBits.Value, '0');
        }

        private void CheckFault(int wordType)
        {
            resultText!.Clear();
            try
            {
                int num;
                Dictionary<string, FaultData> faultMap;
                string title;

                if (wordType == 1)
                {
                    num = int.Parse(firstWordEntry?.Text ?? "0");
                    faultMap = firstWordMap;
                    title = "第一个字检查结果：";
                }
                else
                {
                    num = int.Parse(secondWordEntry?.Text ?? "0");
                    faultMap = secondWordMap;
                    title = "第二个字检查结果：";
                }

                int maxKey = 0;
                foreach (var key in faultMap.Keys)
                {
                    if (int.TryParse(key, out int keyInt) && keyInt > maxKey)
                        maxKey = keyInt;
                }

                string binaryStr = DecimalToBinary(num, maxKey + 1);
                char[] reversedBits = binaryStr.ToCharArray();
                Array.Reverse(reversedBits);
                string formattedBits = string.Join(" ", ChunkString(new string(reversedBits), 4));

                resultText.AppendText($"{title}\n", Color.Blue);
                resultText.AppendText($"十进制值：{num}\n");
                resultText.AppendText($"二进制表示（低位在前）：{formattedBits}\n");
                resultText.AppendText("════════════════════════\n", Color.Gray);
                resultText.AppendText("你查询的结果如下：\n", Color.Blue);

                bool found = false;
                for (int bitPos = 0; bitPos < reversedBits.Length; bitPos++)
                {
                    if (faultMap.ContainsKey(bitPos.ToString()))
                    {
                        var faultData = faultMap[bitPos.ToString()];
                        char displayOn = faultData.DisplayOn ?? '1';
                        if (reversedBits[bitPos] == displayOn && !string.IsNullOrEmpty(faultData.Description))
                        {
                            resultText.AppendText($"  • 第{bitPos}位: {faultData.Description}\n", Color.DarkRed);
                            found = true;
                        }
                    }
                }

                if (!found)
                    resultText.AppendText("无对应数据位或打包数据表未导入");
            }
            catch (FormatException)
            {
                MessageBox.Show("请输入有效的十进制整数", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string[] ChunkString(string str, int chunkSize)
        {
            int chunkCount = (int)Math.Ceiling((double)str.Length / chunkSize);
            var chunks = new string[chunkCount];
            for (int i = 0; i < chunkCount; i++)
            {
                chunks[i] = str.Substring(i * chunkSize, Math.Min(chunkSize, str.Length - i * chunkSize));
            }
            return chunks;
        }

        private void AboutButton_Click(object? sender, EventArgs e)
        {
            string aboutText = @"自动化计算工具-王国强-202603

版本：3.0
开发者：德龙轧钢自动化团队

本软件用于查看和分析打包数据及状态字，
支持导入JSON格式的配置文件，
帮助工程师快速定位和诊断设备状态。

功能说明：
• 导入：导入JSON格式的打包数据或状态字配置文件
• 列表：选择不同的打包字分组或状态字分组
• 计算器：打开系统计算器
• 关于：显示本帮助信息

支持的文件格式：
• 明文JSON格式
• Base64编码JSON格式

© 2026 德龙轧钢自动化团队";
            MessageBox.Show(aboutText, "关于", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LoadStatusGroup(string groupName, Form? popup = null)
        {
            if (!statusGroupsData.ContainsKey(groupName))
            {
                MessageBox.Show($"状态字分组 '{groupName}' 不存在", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var groupData = statusGroupsData[groupName];
            currentStatusName = groupData.StatusName ?? groupData.FirstName ?? "状态字";
            statusWordMap = groupData.StatusMap ?? new();
            currentStatusGroup = groupName;

            currentStatusLabel!.Text = $"当前选择的状态字分组：{groupName}";

            statusResultText!.Clear();
            statusResultText!.AppendText($"已加载状态字分组：{groupName}\n", Color.Blue);
            statusResultText.AppendText($"状态字名称：{currentStatusName}\n");
            statusResultText.AppendText("════════════════════════\n", Color.Gray);
            statusResultText.AppendText("状态字映射列表：\n\n", Color.Blue);

            var sortedKeys = new List<string>(statusWordMap.Keys);
            sortedKeys.Sort((a, b) =>
            {
                if (int.TryParse(a, out int aInt) && int.TryParse(b, out int bInt))
                    return aInt.CompareTo(bInt);
                return string.Compare(a, b, StringComparison.Ordinal);
            });

            foreach (var key in sortedKeys)
            {
                statusResultText.AppendText($"        \"{key}\": \"{statusWordMap[key]}\",\n", Color.DarkGreen);
            }

            popup?.Close();
        }

        private void LoadStatusWord(string statusKey, Form? popup = null)
        {
            if (!statusWordMap.ContainsKey(statusKey))
            {
                MessageBox.Show($"状态字 '{statusKey}' 不存在", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            currentStatusLabel!.Text = currentStatusGroup != null
                ? $"当前选择的状态字分组：{currentStatusGroup}"
                : $"当前选择的状态字：{statusKey}";

            statusResultText!.Clear();
            statusResultText.AppendText($"已加载状态字：{statusKey}\n", Color.Blue);
            statusResultText.AppendText($"状态值：{statusKey}\n");
            statusResultText.AppendText($"描述：{statusWordMap[statusKey]}\n", Color.DarkGreen);
            statusResultText.AppendText("════════════════════════\n", Color.Gray);

            popup?.Close();
        }

        private void CalculatorButton_Click(object? sender, EventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "calc.exe",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法打开计算器：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SwitchMemoryCalcType(int calcType)
        {
            memoryCalcType = calcType;
            memoryEntries.Clear();

            if (memoryInputGroupBox == null) return;

            memoryInputGroupBox.Controls.Clear();

            var configs = GetCalcTypeConfigs(calcType);

            foreach (var (labelText, key, fgColor) in configs)
            {
                var fieldPanel = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 40,
                    Padding = new Padding(0, 5, 0, 5)
                };

                var label = new Label
                {
                    Text = labelText,
                    Dock = DockStyle.Left,
                    Width = 180,
                    Height = 23,
                    Font = new Font("微软雅黑", 9F),
                    TextAlign = ContentAlignment.MiddleLeft
                };

                var entry = new TextBox
                {
                    Dock = DockStyle.Fill,
                    Height = 23,
                    Font = new Font("微软雅黑", 9F),
                    ForeColor = fgColor,
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle
                };
                entry.KeyPress += (s, e) => { if (e.KeyChar == (char)Keys.Enter) { MemoryCalculateButton_Click(null, EventArgs.Empty); e.Handled = true; } };

                memoryEntries[key] = entry;
                fieldPanel.Controls.Add(label);
                fieldPanel.Controls.Add(entry);
                memoryInputGroupBox.Controls.Add(fieldPanel);
            }
        }

        private List<(string, string, Color)> GetCalcTypeConfigs(int calcType)
        {
            return calcType switch
            {
                1 => new List<(string, string, Color)>
                {
                    ("初始内存映象网地址:", "initial_mem_address", Color.FromArgb(231, 76, 60)),
                    ("初始R地址:", "initial_r_address", Color.FromArgb(52, 152, 219)),
                    ("终止R地址:", "end_r_address", Color.FromArgb(44, 62, 80))
                },
                2 => new List<(string, string, Color)>
                {
                    ("初始内存映象网地址:", "initial_mem_address", Color.FromArgb(231, 76, 60)),
                    ("终止内存映象网地址:", "end_mem_address", Color.FromArgb(44, 62, 80)),
                    ("初始R地址:", "initial_r_address", Color.FromArgb(52, 152, 219))
                },
                3 => new List<(string, string, Color)>
                {
                    ("初始内存映象网地址:", "initial_mem_address", Color.FromArgb(231, 76, 60)),
                    ("初始m地址:", "initial_m_address", Color.FromArgb(39, 174, 96)),
                    ("终止m地址:", "end_m_address", Color.FromArgb(44, 62, 80))
                },
                4 => new List<(string, string, Color)>
                {
                    ("初始内存映象网地址:", "initial_mem_address", Color.FromArgb(231, 76, 60)),
                    ("终止内存映象网地址:", "end_mem_address", Color.FromArgb(44, 62, 80)),
                    ("初始m地址:", "initial_m_address", Color.FromArgb(39, 174, 96)),
                    ("位编号:", "bit_position", Color.FromArgb(44, 62, 80))
                },
                _ => new List<(string, string, Color)>()
            };
        }

        private void MemoryCalculateButton_Click(object? sender, EventArgs e)
        {
            try
            {
                ValidateMemoryInputs();
                string result = CalculateMemoryResult();
                memoryResultLabel!.Text = result;
            }
            catch (Exception ex)
            {
                memoryResultLabel!.Text = $"错误: {ex.Message}";
            }
        }

        private void ValidateMemoryInputs()
        {
            var requiredFields = memoryCalcType switch
            {
                1 => new[] { "initial_mem_address", "initial_r_address", "end_r_address" },
                2 => new[] { "initial_mem_address", "initial_r_address", "end_mem_address" },
                3 => new[] { "initial_mem_address", "initial_m_address", "end_m_address" },
                4 => new[] { "initial_mem_address", "initial_m_address", "end_mem_address", "bit_position" },
                _ => Array.Empty<string>()
            };

            foreach (var field in requiredFields)
            {
                if (string.IsNullOrWhiteSpace(memoryEntries[field].Text))
                    throw new Exception("所有输入框必须填写");
            }
        }

        private string CalculateMemoryResult()
        {
            return memoryCalcType switch
            {
                1 => CalculateAnalogEndMemAddress(),
                2 => CalculateAnalogEndRAddress(),
                3 => CalculateDigitalEndMemAddressAndBit(),
                4 => CalculateDigitalEndMAddress(),
                _ => ""
            };
        }

        private string CalculateAnalogEndMemAddress()
        {
            string initialMemAddress = memoryEntries["initial_mem_address"].Text.ToUpper();
            int initialRAddress = int.Parse(memoryEntries["initial_r_address"].Text);
            int endRAddress = int.Parse(memoryEntries["end_r_address"].Text);

            int initialMem = Convert.ToInt32(initialMemAddress, 16);
            int offset = (endRAddress - initialRAddress) * 2;
            int endMem = initialMem + offset;

            return $"终止内存映象网地址: {endMem:X4}";
        }

        private string CalculateAnalogEndRAddress()
        {
            string initialMemAddress = memoryEntries["initial_mem_address"].Text.ToUpper();
            int initialRAddress = int.Parse(memoryEntries["initial_r_address"].Text);
            string endMemAddress = memoryEntries["end_mem_address"].Text.ToUpper();

            int initialMem = Convert.ToInt32(initialMemAddress, 16);
            int endMem = Convert.ToInt32(endMemAddress, 16);
            int offset = (endMem - initialMem) / 2;
            int endR = initialRAddress + offset;

            return $"终止R地址: {endR}";
        }

        private string CalculateDigitalEndMemAddressAndBit()
        {
            string initialMemAddress = memoryEntries["initial_mem_address"].Text.ToUpper();
            int initialMAddress = int.Parse(memoryEntries["initial_m_address"].Text);
            int endMAddress = int.Parse(memoryEntries["end_m_address"].Text);

            int initialMem = Convert.ToInt32(initialMemAddress, 16);
            int offset = (endMAddress - initialMAddress) / 8;
            int bitPosition = (endMAddress - initialMAddress) % 8;
            int endMem = initialMem + offset;

            return $"终止内存映象网地址: {endMem:X4}, 位编号: {bitPosition}";
        }

        private string CalculateDigitalEndMAddress()
        {
            string initialMemAddress = memoryEntries["initial_mem_address"].Text.ToUpper();
            int initialMAddress = int.Parse(memoryEntries["initial_m_address"].Text);
            string endMemAddress = memoryEntries["end_mem_address"].Text.ToUpper();
            int bitPosition = int.Parse(memoryEntries["bit_position"].Text);

            int initialMem = Convert.ToInt32(initialMemAddress, 16);
            int endMem = Convert.ToInt32(endMemAddress, 16);
            int offset = (endMem - initialMem) * 8;
            int endM = initialMAddress + offset + bitPosition;

            return $"终止m地址: {endM}";
        }

        public class FaultData
        {
            public string? Description { get; set; }
            public char? DisplayOn { get; set; }
        }

        public class GroupData
        {
            public string? FirstName { get; set; }
            public string? SecondName { get; set; }
            public Dictionary<string, FaultData>? FirstWord { get; set; }
            public Dictionary<string, FaultData>? SecondWord { get; set; }
            public string? StatusName { get; set; }
            public Dictionary<string, string>? StatusMap { get; set; }
        }
    }

    public static class RichTextBoxExtensions
    {
        public static void AppendText(this RichTextBox box, string text, Color color)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;
            box.SelectionColor = color;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }
    }
}
