# VSCode编辑器变体注册系统

## 简介

为了简化添加新的VSCode变体编辑器的过程，我们开发了一个可配置的注册系统。现在添加新的编辑器变体只需要几行代码即可完成。

## 快速开始

### 1. 基本注册

```csharp
// 在任何地方调用以下代码来注册新的编辑器变体
VSCodeVariantRegistry.RegisterVariant(new VSCodeVariantConfig
{
    Id = "your-editor-id",
    DisplayName = "Your Editor Name",
    PathKeywords = new[] { "keyword1", "keyword2" },
    ExecutablePattern = @"youreditor.*\.exe$"
});
```

### 2. 完整配置示例

```csharp
VSCodeVariantRegistry.RegisterVariant(new VSCodeVariantConfig
{
    Id = "Devin",                           // 唯一标识符
    DisplayName = "Devin",                  // 显示名称
    PathKeywords = new[] { "Devin", "surf" }, // 路径匹配关键字
    ExecutablePattern = @"Devin.*\.exe$",   // 可执行文件模式
    PrereleaseKeywords = new[] { "insider", "preview", "beta" }, // 预发布版本关键字
    CustomVersionDetector = (versionString) => // 自定义版本检测（可选）
    {
        // 自定义版本解析逻辑
        if (versionString.StartsWith("v"))
        {
            Version.TryParse(versionString.Substring(1).Split('-')[0], out var version);
            return version;
        }
        return new Version(1, 0, 0);
    }
});
```

## 配置参数详解

### 必需参数

- **Id**: 编辑器的唯一标识符（推荐使用小写字母和连字符）
- **DisplayName**: 在UI中显示的编辑器名称
- **PathKeywords**: 用于自动发现的关键字数组（至少一个）

### 可选参数

- **ExecutablePattern**: 可执行文件名的正则表达式模式
- **PrereleaseKeywords**: 预发布版本检测的关键字数组
- **CustomVersionDetector**: 自定义版本检测委托函数

## 使用场景

### 场景1: 添加简单的编辑器变体

```csharp
VSCodeVariantRegistry.RegisterVariant(new VSCodeVariantConfig
{
    Id = "zed",
    DisplayName = "Zed",
    PathKeywords = new[] { "zed" },
    ExecutablePattern = @"zed.*\.exe$"
});
```

### 场景2: 复杂的版本检测需求

```csharp
VSCodeVariantRegistry.RegisterVariant(new VSCodeVariantConfig
{
    Id = "code-oss",
    DisplayName = "Code - OSS",
    PathKeywords = new[] { "code-oss", "vscode-oss" },
    ExecutablePattern = @"code-oss.*\.exe$",
    CustomVersionDetector = (versionStr) =>
    {
        // 处理特殊的版本格式
        var parts = versionStr.Split('.');
        if (parts.Length >= 3)
        {
            return new Version(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
        }
        return new Version(1, 0, 0);
    }
});
```

### 场景3: 批量注册多个编辑器

```csharp
var editors = new[]
{
    new VSCodeVariantConfig { Id = "lapce", DisplayName = "Lapce", PathKeywords = new[] { "lapce" } },
    new VSCodeVariantConfig { Id = "helix", DisplayName = "Helix", PathKeywords = new[] { "hx", "helix" } },
    new VSCodeVariantConfig { Id = "zed-preview", DisplayName = "Zed Preview", PathKeywords = new[] { "zed-preview" } }
};

foreach (var editor in editors)
{
    VSCodeVariantRegistry.RegisterVariant(editor);
}
```

## API参考

### VSCodeVariantRegistry 类

#### 静态方法

- `InitializeDefaultVariants()`: 初始化默认的编辑器变体
- `RegisterVariant(VSCodeVariantConfig config)`: 注册新的编辑器变体
- `GetAllVariants()`: 获取所有已注册的编辑器变体
- `DetectVariantByPath(string path)`: 根据路径检测编辑器类型
- `IsCandidateForDiscovery(string path)`: 检查是否为候选发现路径

### VSCodeVariantConfig 类

#### 属性

- `Id`: 编辑器唯一标识符
- `DisplayName`: 显示名称
- `PathKeywords`: 路径匹配关键字数组
- `ExecutablePattern`: 可执行文件模式
- `PrereleaseKeywords`: 预发布版本关键字
- `CustomVersionDetector`: 自定义版本检测函数

## 最佳实践

### 1. 命名规范
- Id使用小写字母和连字符（如：`my-editor`）
- DisplayName使用清晰的英文名称
- PathKeywords使用编辑器特有的关键字

### 2. 关键字选择
- 选择具有唯一性的关键字
- 避免使用过于通用的词汇
- 考虑不同操作系统下的路径差异

### 3. 错误处理
```csharp
try
{
    VSCodeVariantRegistry.RegisterVariant(config);
    Debug.Log($"成功注册编辑器: {config.DisplayName}");
}
catch (ArgumentException ex)
{
    Debug.LogError($"注册编辑器失败: {ex.Message}");
}
```

## 调试和测试

### 查看已注册的编辑器
```csharp
foreach (var variant in VSCodeVariantRegistry.GetAllVariants())
{
    Debug.Log($"已注册: {variant.DisplayName} (ID: {variant.Id})");
}
```

### 测试路径匹配
```csharp
var testPath = @"C:\Program Files\MyEditor\myeditor.exe";
var detected = VSCodeVariantRegistry.DetectVariantByPath(testPath);
if (detected != null)
{
    Debug.Log($"匹配到编辑器: {detected.DisplayName}");
}
```

## 常见问题

### Q: 如何处理同名但不同版本的编辑器？
A: 可以在PathKeywords中添加版本相关信息，或使用CustomVersionDetector进行区分。

### Q: 注册的编辑器没有被发现怎么办？
A: 检查PathKeywords是否准确匹配实际路径，确保ExecutablePattern正确。

### Q: 可以动态加载编辑器配置吗？
A: 是的，可以在运行时通过脚本或配置文件动态注册编辑器变体。

## 扩展示例

完整的扩展示例请参考 `Assets/Examples/AddNewEditorVariantExample.cs` 文件。