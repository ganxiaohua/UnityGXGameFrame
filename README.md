# UnityGXGameFrame
* 需要odin插件 yooassets插件 unitask插件 fgui插件
* 测试案例：https://github.com/ganxiaohua/GXGame
* 详情知乎：https://zhuanlan.zhihu.com/p/2022215951286899930

集成UI框架(基于fgui),资源加载框架(基于yooAssets)

# 框架总结

## 概述

GameFrame 是一个基于 Unity 的数据驱动的游戏开发框架，提供了完整的游戏运行时基础设施和编辑器工具支持。框架分为两个主要部分：

- **GameFrame.Runtime**: 运行时核心框架，提供游戏运行时的所有基础设施
- **GameFrame.Editor**: 编辑器扩展工具，提供开发阶段的辅助工具和可视化编辑器

---

## 一、GameFrame.Runtime 运行时框架

### 1. 核心架构 (Core)

#### 1.1 实体组件系统 

| 组件 | 说明 |
|------|------|
| `IEntity` / `Entity` | 实体基类，支持组件模式和子实体模式 |
| `EffEntity` | 用于高性能场景，支持非托管组件 |
| `World` |理实体和组件的生命周期 |
| `ComponentArray` | 组件数组，高效存储和访问组件数据 UnsafeUtility.Malloc的方式创建内存连续 |

**特性：**
- 支持两种实体模式：普通 `Entity` 和 ECS `EffEntity`
- 组件可嵌套（Entity 可以包含其他 Entity 作为组件）
- 支持实体版本控制（Versions），防止过期引用
- 自动生命周期管理（通过 `ReferencePool`）

#### 1.2 实体生命周期管理

```
EntityHouse (实体管理中心)
├── 管理所有活跃实体的更新
├── 支持 Update / LateUpdate / FixedUpdate
└── 与 ReferencePool 配合实现对象复用
```

#### 1.3 场景管理

- `SceneBase`: 场景基类，定义场景生命周期
- `SceneFactory`: 场景工厂，负责场景的创建和切换
- `IScene`: 场景接口，支持自定义场景实现

### 2. 能力系统 (Capability)

高级行为系统：

| 类 | 功能 |
|----|------|
| `CapabilityBase` | 能力基类，定义激活/停用逻辑 |
| `CapabilityCollector` | 能力收集器，管理实体能力集合 |
| `CapabilitySystem` | 能力系统，负责能力调度 |
| `ECCWorld` | ECC (Entity-Capability-Component) 世界 |

**特性：**
- 能力支持优先级排序（TickGroupOrder）
- 支持多种更新模式（Update / FixedUpdate）
- 条件激活机制（ShouldActivate/ShouldDeactivate）

### 3. 资源管理 (Assets)

基于 **YooAsset** 的资源管理模块：

| 类 | 功能 |
|----|------|
| `AssetManager` | 资源管理器单例，统一资源加载入口 |
| `IAssetHandle` | 资源句柄接口 |
| `DefaultAssetHandle` | 默认资源句柄实现 |
| `SceneHandle` | 场景加载专用句柄 |
| `TextAssetHandle` | 文本资源句柄 |
| `IAssetReference` | 资源引用接口，管理资源生命周期 |

**特性：**
- 异步资源加载（基于 UniTask）
- 引用计数自动管理
- 延迟释放机制（Dying Asset Cache）
- 支持 Addressables 和 Resources 两种加载方式

### 4. UI 系统 (UI)

基于 **FairyGUI** 的 UI 管理模块：

| 类 | 功能 |
|----|------|
| `UISystem` | UI 系统单例，管理所有面板 |
| `Panel` | 面板基类，定义面板生命周期 |
| `PanelMode` | 面板模式（Normal / Mono）|
| `PanelFlag` | 面板标志（Persistent / NonTopmost 等）|
| `TransitionPanel` | 支持过渡动画的面板 |
| `LoopList` | 循环列表组件 |

**特性：**
- 面板栈管理（支持 Mono 模式弹窗）
- 面板状态管理（Open / Hide / Destroy）
- 自动层级排序
- 键盘事件处理支持
- 面板缓存和延迟销毁

### 5. 状态机 (FSM)

有限状态机实现：

| 类 | 功能 |
|----|------|
| `FsmController` | 状态机控制器 |
| `FsmState` | 状态基类 |
| `FsmTaskController` | 支持 UniTask 的状态机 |

### 6. 对象池系统

#### 6.1 引用池 (ReferencePool)
用于管理脚本对象的复用：
- 自动扩容和缩容
- 定期清理未使用的引用
- 线程安全实现

#### 6.2 对象池 (ObjectPool)
```
ObjectPoolManager
├── CreateObjectPool<T>() - 创建对象池
├── GetObjectPool<T>() - 获取对象池
└── DeleteObjectPool<T>() - 删除对象池
```

#### 6.3 GameObject 池 (GameObjectProxy)
```
GameObjectPool
├── 基于 Prefab 的对象池
├── 异步加载支持
└── 自动回收机制
```

### 7. 定时器系统 (Timer)

| 类 | 功能 |
|----|------|
| `TimerSystem` | 基于优先队列的定时器管理 |
| `UniqueTimer` | 可取消的定时器 |
| `WaitTime` | 等待时间工具类 |

**特性：**
- 使用可变优先队列（MutablePriorityQueue）
- 支持定时器的取消和重新调度

### 8. 数据结构 (DataStructure)

框架提供了多种高效数据结构：

| 数据结构 | 说明 |
|----------|------|
| `ArrayEx` | 扩展数组，支持快速操作 |
| `BitList` | 位列表，节省内存 |
| `BulkList` | 批量列表 |
| `DDictionary` | 双向字典 |
| `DoubleMap` | 双键映射 |
| `MutablePriorityQueue` | 可变优先队列 |
| `JumpIndexArray` | 跳跃索引数组 |

### 9. 事件系统 (EventPool)

- `IEvent` / `EventData`: 事件接口和数据
- `AssetEvent`: 资源事件

### 10. 调试工具 (Debug)

| 类 | 功能 |
|----|------|
| `Debugger` | 日志系统 |
| `Profiler` / `TimeProfiler` | 性能分析 |
| `MemoryProfiler` | 内存分析 |
| `AssertHelp` | 断言帮助类 |

### 11. 任务系统 (Task)

- 基于 **UniTask** 的异步编程支持
- `AwaiterTask`: 等待任务
- `IHandle`: 句柄接口

---

## 二、GameFrame.Editor 编辑器框架

### 1. 实体可视化工具 (EnitiyGraph)

运行时实体结构可视化编辑器：

| 类 | 功能 |
|----|------|
| `EnitiyGraphWindow` | 实体图形窗口 |
| `EnitiyGraphView` | 实体图形视图 |
| `EntityView` | 实体节点视图 |
| `ComponentView` | 组件节点视图 |
| `CapabilityView` | 能力节点视图 |

**功能：**
- 实时显示运行时实体层次结构
- 支持搜索组件和能力
- 双击节点快速定位
- 运行时调试专用

### 2. UI 工具 (UI)

#### 2.1 FGUI 导出工具
| 类 | 功能 |
|----|------|
| `FGUIExporter` | FGUI 组件导出工具 |
| `FGUIChecker` | FGUI 资源检查器 |
| `FGUIPanelCodeGen` | UI 面板代码生成 |

**功能：**
- 导出 FGUI 组件及其依赖资源
- 检查无效引用和重复图片
- 自动生成 UI 面板绑定代码

#### 2.2 UI 预览编辑器
- `FGUIPanelPreviewEditor`: UI 面板预览

### 3. 代码生成工具 (Tool/AutoCreate)

| 类 | 功能 |
|----|------|
| `AutoCreate` | 自动生成脚本 |
| `AutoCreate.Capabilys` | 生成能力系统脚本 |
| `CreateFiles` | 文件创建工具 |
| `GamePlayAssembly` | 游戏程序集管理 |

### 4. 数据库工具 (EditorDatabase)

| 类 | 功能 |
|----|------|
| `DynamicAssemblyBuilder` | 动态程序集构建 |
| `EventBindEditor` | 事件绑定编辑器 |

### 5. 实用工具

#### 5.1 通用工具
| 类 | 功能 |
|----|------|
| `ToolMenu` | 工具菜单入口 |
| `GamePathData` | 游戏路径数据 |
| `EditorPrefs` | 编辑器偏好设置 |
| `TypeNameHelper` | 类型名称帮助 |
| `OpFile` | 文件操作工具 |
| `PictureDispose` | 图片处理工具 |

#### 5.2 Shell 工具
| 类 | 功能 |
|----|------|
| `ShellHelper` | Shell 命令执行 |
| `Git` | Git 操作封装 |

#### 5.3 C# 项目工具
| 类 | 功能 |
|----|------|
| `CSharpProjectHelper` | C# 项目辅助 |
| `ConsoleCSharp` | C# 控制台执行 |
| `CSharpRunner` | C# 代码运行器 |

### 6. GUI 扩展 (GUI)

| 类 | 功能 |
|----|------|
| `EditorGUIHelper` | GUI 辅助工具 |
| `EditorStylesHelper` | 编辑器样式 |
| `ColorScope` / `LabelWidthScope` / `FieldWidthScope` | GUI 作用域 |
| `RichSearchWindow` | 富文本搜索窗口 |
| `ToolbarExtensions` | 工具栏扩展 |

### 7. 虚拟对话框 (VirtualDialog)

| 类 | 功能 |
|----|------|
| `VirtualDialog` | 虚拟对话框基类 |
| `VirtualBaseDialog` | 基础对话框实现 |
| `VirtualDialogContainer` | 对话框容器 |
| `MouseEventHelper` | 鼠标事件辅助 |

### 8. 宏定义工具 (Utils)

| 类 | 功能 |
|----|------|
| `Macro` | 宏定义管理 |
| `MacroDefineHelper` | 宏定义帮助类 |

### 9. YooAsset 扩展

| 类 | 功能 |
|----|------|
| `YooAssetExpand` | YooAsset 扩展功能 |
| `YooAssetPath` | YooAsset 路径管理 |

---

## 三、框架特点总结

### 3.1 架构优势

1. **数据驱动架构**
   - 数据与逻辑分离
   - 高性能缓存友好

2. **模块化设计**
   - 各系统独立运作
   - 通过事件和接口通信
   - 易于扩展和维护

3. **内存管理**
   - 多层次对象池（引用池、对象池、GameObject 池）
   - 自动内存回收
   - 引用计数管理资源生命周期

4. **异步编程**
   - 基于 UniTask 的异步支持
   - 取消令牌支持
   - 零分配异步操作

### 3.2 性能优化

- **组件数组**: 连续内存布局，提高缓存命中率
- **对象池**: 减少 GC 压力
- **延迟释放**: 避免频繁加载卸载资源
- **优先队列**: 高效的定时器调度

### 3.3 开发效率

- **代码生成**: 自动生成重复性代码
- **可视化工具**: 实时查看实体结构
- **调试支持**: 丰富的调试和性能分析工具
- **编辑器扩展**: 集成到 Unity Editor 的工作流

---

## 四、技术栈

| 技术 | 用途 |
|------|------|
| Unity | 游戏引擎 |
| UniTask | 异步编程 |
| FairyGUI | UI 框架 |
| YooAsset | 资源管理 |
| Odin Inspector | 编辑器扩展 |
| NodeCanvas | 可视化脚本（第三方）|

---

## 五、目录结构

```
Assets/3rd/UnityGXGameFrame/
├── Editor/                          # 编辑器工具
│   ├── Customize/                   # 自定义编辑器
│   ├── EditorDatabase/              # 编辑器数据库
│   ├── EditorFrame/                 # 编辑器框架
│   ├── GUI/                         # GUI 扩展
│   ├── Tool/                        # 工具集合
│   │   ├── EnitiyGraph/             # 实体可视化
│   │   ├── Graph/                   # 通用图形工具
│   │   └── ShellHelper/             # Shell 工具
│   ├── UI/                          # UI 工具
│   ├── Utils/                       # 工具类
│   ├── VirtualDialog/               # 虚拟对话框
│   └── YooAssetExpand/              # YooAsset 扩展
│
└── Runtime/                         # 运行时框架
    ├── Arena/                       # 竞技场/战斗场景
    ├── Assets/                      # 资源管理
    ├── Config/                      # 配置管理
    ├── Core/                        # 核心 ECS
    ├── DataStructure/               # 数据结构
    ├── Debug/                       # 调试工具
    ├── EventPool/                   # 事件池
    ├── EXFGUI/                      # FGUI 扩展
    ├── Expand/                      # 扩展方法
    ├── FSM/                         # 状态机
    ├── GameObjectProxy/             # GameObject 代理
    ├── GeneralComponent/            # 通用组件
    ├── GOAP/                        # GOAP AI
    ├── InspectorEditor/             # 运行时检查器
    ├── ObjectPool/                  # 对象池
    ├── ReferencePool/               # 引用池
    ├── Task/                        # 任务系统
    └── Timer/                       # 定时器系统
```

---

## 六、总结

GameFrame 是一个功能完善、架构清晰的 Unity 游戏框架，提供了从资源管理、实体系统、UI 系统到 AI 系统的完整解决方案。数据驱动架构设计兼顾了性能和灵活性，配套的编辑器工具也大大提高了开发效率。适合用于中大型游戏项目的开发。


 

