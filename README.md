# 自动化计算工具-王国强-202603

## 项目简介

自动化计算工具是一款用于查看和分析打包数据及状态字的Windows桌面应用程序。该工具支持导入JSON格式的配置文件，帮助工程师快速定位和诊断设备状态。

## 主要功能

### 1. 打包数据查看
- 支持导入JSON格式的打包数据配置文件
- 输入十进制数值查询对应的故障描述
- 支持明文JSON和Base64编码JSON格式
- 支持分组管理，可切换不同的打包字分组

### 2. 状态字查看
- 查看状态字映射关系
- 支持分组管理状态字
- 显示状态值和描述信息

### 3. 内存映象网计算
提供4种计算类型：
- **模拟量计算（16进制地址）**：根据初始内存地址和R地址范围计算终止内存地址
- **模拟量计算（R地址）**：根据内存地址范围计算终止R地址
- **数字量计算（16进制地址和位数）**：计算终止内存地址和位编号
- **数字量计算（M地址）**：根据内存地址和位编号计算终止M地址

### 4. 辅助功能
- **导入**：导入JSON配置文件
- **列表**：选择不同的分组
- **计算器**：打开系统计算器
- **关于**：显示版本和帮助信息

## 技术栈

- **开发语言**：C# (.NET 8.0)
- **UI框架**：Windows Forms
- **JSON处理**：System.Text.Json
- **目标平台**：Windows x64

## 项目结构

```
自动化计算工具-5/
├── 自动化计算工具.csproj    # 项目配置文件
├── Program.cs                 # 程序入口
├── MainForm.cs                # 主窗体逻辑
├── MainForm.Designer.cs       # 主窗体UI设计
├── 自动化计算工具.py          # 原Python版本（参考）
├── .github/
│   └── workflows/
│       └── build.yml          # GitHub Actions CI/CD配置
└── README.md                  # 项目说明文档
```

## 开发环境要求

- .NET 8.0 SDK
- Windows 10/11
- Visual Studio 2022 或 Visual Studio Code（推荐）

## 本地开发

### 1. 克隆项目

```bash
git clone <repository-url>
cd 自动化计算工具-5
```

### 2. 恢复依赖

```bash
dotnet restore
```

### 3. 构建项目

```bash
dotnet build --configuration Release
```

### 4. 运行程序

```bash
dotnet run
```

## 发布为可执行文件

### 方式一：使用命令行发布

```bash
# 发布为单文件可执行程序
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:PublishTrimmed=true -p:TrimMode=link -p:PublishReadyToRun=true -o ./publish
```

发布后的文件位于 `./publish` 目录，主要文件为 `自动化计算工具.exe`。

### 方式二：使用Visual Studio

1. 打开解决方案
2. 右键项目 → 发布
3. 选择目标：文件夹
4. 配置发布设置：
   - 目标框架：net8.0-windows
   - 目标运行时：win-x64
   - 部署模式：自包含
   - 生成单个文件：是
   - 裁剪程序集：是
   - 准备运行时：是
5. 点击发布

## GitHub Actions自动构建

项目配置了GitHub Actions，当以下情况发生时会自动构建：

- 推送到 `main` 或 `master` 分支
- 创建或更新标签（格式：`v*`）
- 手动触发工作流

### 创建发布版本

1. 创建并推送标签：
```bash
git tag v1.0.0
git push origin v1.0.0
```

2. GitHub Actions会自动：
   - 构建项目
   - 发布为单文件exe
   - 创建GitHub Release
   - 上传构建产物

## 配置文件格式

### 打包数据JSON格式

```json
{
  "groups": {
    "分组1": {
      "first_name": "第一个字名称",
      "second_name": "第二个字名称",
      "first_word": {
        "0": {
          "description": "描述信息",
          "display_on": "1"
        }
      },
      "second_word": {
        "0": {
          "description": "描述信息",
          "display_on": "1"
        }
      }
    }
  }
}
```

### 状态字JSON格式

```json
{
  "groups": {
    "状态字分组1": {
      "status_name": "状态字名称",
      "status_map": {
        "0": "描述信息",
        "1": "描述信息"
      }
    }
  }
}
```

## 编译输出大小

使用推荐的发布配置，生成的exe文件大小约为：
- **基础大小**：3-5 MB
- **包含所有依赖**：5-8 MB（单文件发布）

## 版本历史

### v3.0.0 (2026-03)
- 使用C#重写，生成更小的exe文件
- 完全复刻原Python版本的功能和界面
- 添加GitHub Actions自动构建和发布
- 优化性能和启动速度

## 许可证

© 2026 德龙轧钢自动化团队

## 联系方式

如有问题或建议，请联系开发团队。

## 贡献

欢迎提交Issue和Pull Request来改进这个项目。
