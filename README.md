# UnityCoder 编辑器集成包

## 简介

UnityCoder是一个专门为Unity 2021.3及以上版本设计的代码编辑器集成包，支持Visual Studio Code及其流行的变体编辑器（包括Cursor、Antigravity、Trae、Trae CN、Kiro、Qoder、Qoder CN、Lingma、Devin等），同时排除了Visual Studio的自动发现功能。

**当前版本：1.0.4**

## 主要特性

- ✅ 支持Visual Studio Code及主流变体编辑器
- ✅ 自动发现编辑器安装位置
- ✅ 手动指定编辑器路径功能
- ✅ 自动生成.csproj和.sln项目文件
- ❌ 排除Visual Studio自动发现（按需求）
- ✅ 保持与官方Visual Studio Editor相似的用户体验
- ✅ 支持Unity 2021.3及以上版本

## 支持的编辑器

### 自动发现的编辑器
- Visual Studio Code (标准版和Insiders版)
- Cursor
- Antigravity
- Trae (包括Trae CN中国版本)
- Kiro
- Qoder (包括Trae CN中国版本)
- Lingma
- Devin

### 平台支持
- Windows
- macOS
- Linux

## 安装方法

### 方法1：通过Package Manager安装
1. 打开Unity编辑器
2. 进入 `Window > Package Manager`
3. 点击左上角的 `+` 按钮
4. 选择 `Add package from git URL...`
5. 输入包的Git地址: `[https://gitcode.com/Mosaulse/UnityCoder.git](https://github.com/Mosaulse/Mosaulse_UnityCoder.git)`
6. 点击 `Add` 按钮

### 方法2：手动安装
1. 下载或克隆本仓库
2. 将 `UnityCoder` 文件夹复制到项目的 `Packages` 目录下
3. Unity会自动识别并加载包

## 使用方法

### 自动发现编辑器
包会自动扫描系统中安装的支持的编辑器，并在Unity的外部工具设置中显示。

### 手动指定编辑器路径
1. 在Unity中进入 `Edit > Preferences > External Tools`
2. 在UnityCoder设置区域找到"手动指定编辑器路径"
3. 输入编辑器可执行文件的完整路径，或点击"浏览"按钮选择
4. 包会自动检测编辑器版本信息

### 项目文件生成
- 在Preferences中可以配置哪些类型的包需要生成项目文件
- 支持的包类型：嵌入式包、本地包、注册表包、Git包、内置包等
- 可以随时点击"重新生成项目文件"按钮

## 配置选项

### 项目生成设置
- **嵌入式包**：为项目内的包生成项目文件
- **本地包**：为本地文件系统的包生成项目文件
- **注册表包**：为包管理器注册表中的包生成项目文件
- **Git包**：为Git仓库中的包生成项目文件
- **内置包**：为Unity内置包生成项目文件
- **播放器项目**：为播放器项目生成额外的csproj文件

### 编辑器设置
- 可以在Preferences中查看当前选中的编辑器详细信息
- 支持版本检测和预发布版本标识

## 技术特点

### 架构设计
- 基于Unity的 `IExternalCodeEditor` 接口实现
- 模块化设计，易于扩展支持新的编辑器
- 异步安装发现，不影响Unity启动性能

### 版本兼容性
- 最低支持Unity 2021.3
- 支持最新的Unity LTS版本
- 跨平台兼容（Windows/macOS/Linux）

### 性能优化
- 智能缓存已发现的编辑器安装
- 增量式项目文件更新
- 高效的文件系统监控

## 开发指南

### 添加新的编辑器支持
1. 继承 `CodeEditorInstallation` 基类
2. 实现必要的抽象方法
3. 在 `Discovery` 类中注册新的发现逻辑
4. 更新安装路径检测逻辑

### 扩展项目生成功能
1. 实现 `IGenerator` 接口
2. 在 `GeneratorFactory` 中注册新的生成器
3. 提供相应的配置选项

## 故障排除

### 常见问题

**Q: 编辑器没有被自动发现**
A: 检查编辑器是否安装在标准位置，或者使用手动指定路径功能

**Q: 项目文件生成失败**
A: 确保有足够的磁盘空间和文件写入权限

**Q: IntelliSense不工作**
A: 确保已正确生成项目文件，并在编辑器中重新加载项目

### 日志调试
包会在Unity控制台输出详细的发现和生成日志，可以帮助诊断问题。

## 许可证

本项目采用MIT许可证，详情请查看LICENSE文件。

## 贡献

欢迎提交Issue和Pull Request来改进这个项目！

## 致谢

感谢Unity官方的Visual Studio Editor包提供的参考实现。
