# 更新日志

## [1.0.3] - 2026-02-19

### 优化改进
- 🔇 注释所有日志输出语句，减少控制台噪音
- 🎯 优化编辑器集成体验，提供更清爽的开发环境
- 📝 保留日志代码以便调试需要时快速恢复

### 代码质量
- 统一注释所有 Debug.Log、Debug.LogWarning、Debug.LogError、Debug.LogException 调用
- 保持代码结构完整，仅注释日志输出部分
- 不影响核心功能和错误处理逻辑

### 影响范围
- VSCodeVariantInstallation.cs
- VSCodeVariantRegistry.cs
- VisualStudioEditor.cs
- UnityCoderEditor.cs
- ProjectGenerationBase.cs
- FileUtility.cs
- VisualStudioIntegration.cs
- ProjectGeneration.cs
- VisualStudioForWindowsInstallation.cs
- ProcessRunner.cs

### 版本信息
- 版本号更新至1.0.3

## [1.0.2] - 2026-02-18

### 新增功能
- 🎉 新增 VSCodeVariantRegistry 编辑器变体注册管理系统
- 📦 新增 VSCodeVariantConfig 编辑器配置信息类
- 🚀 实现模块化的编辑器变体注册机制
- 🔍 支持动态注册新的编辑器变体
- 📝 智能路径检测和编辑器类型识别
- 🎯 优化编辑器发现算法，提高匹配准确性

### 编辑器支持
- 标准版 Visual Studio Code
- Cursor
- Antigravity
- Trae
- Trae CN
- Kiro
- Qoder
- Lingma
- Windsurf

### 技术改进
- 实现可扩展的编辑器配置系统
- 支持自定义版本检测逻辑
- 跨平台路径匹配（Windows/macOS/Linux）
- 优先级排序的编辑器识别算法
- 正则表达式模式匹配优化

### API改进
- 新增 RegisterVariant 方法用于注册编辑器变体
- 新增 DetectVariantByPath 方法用于路径检测
- 新增 IsCandidateForDiscovery 方法用于发现候选
- 新增 GetAllVariants 方法获取所有已注册变体

### 文档更新
- 更新README.md，完善项目说明和使用指南
- 更新CHANGELOG.md，记录版本历史
- 更新package.json，优化包元数据信息

### 版本信息
- 版本号更新至1.0.2

## [1.0.1] - 2026-02-08

### 文档更新
- 更新README.md，添加完整的编辑器支持列表
- 更新CHANGELOG.md，保持版本历史记录
- 更新package.json，包含所有支持的编辑器信息
- 增加keywords，提高包的可发现性

### 版本信息
- 版本号更新至1.0.1

## [1.0.0] - 2026-02-01

### 新增功能
- 🎉 初始版本发布
- 支持Visual Studio Code及主流变体编辑器（Cursor、Antigravity、Trae、Trae CN、Kiro、Qoder、Lingma、Windsurf）
- 自动发现编辑器安装位置
- 手动指定编辑器路径功能
- 自动生成.csproj和.sln项目文件
- 排除Visual Studio自动发现功能
- 支持Unity 2021.3及以上版本
- 保持与官方Visual Studio Editor相似的用户体验

### 技术特性
- 基于Unity `IExternalCodeEditor` 接口实现
- 模块化架构设计
- 异步安装发现机制
- 跨平台支持（Windows/macOS/Linux）
- 智能版本检测和预发布版本标识
- 高效的项目文件生成和更新机制

### 配置选项
- 可配置的项目生成标志（嵌入式包、本地包、注册表包等）
- 编辑器详细信息显示
- 手动路径指定和版本自动识别
- 项目文件重新生成功能

### 开发者功能
- 可扩展的编辑器支持架构
- 灵活的项目生成器接口
- 详细的日志输出和错误处理
- 完整的API文档注释

---

**注意**: 这是初始版本，可能存在一些待优化的地方。欢迎反馈和建议！
