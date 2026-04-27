# Utility + GOAP 混合AI框架

一个轻量级、自包含的 **Utility（效用）+ GOAP（目标导向行动计划）** 混合AI框架，适用于Unity游戏开发。

## 目录结构

```
AI/
├── Utility/          # 效用系统
│   └── UtilityConsideration.cs
├── GOAP/             # GOAP系统
│   ├── GOAPWorldState.cs
│   ├── GOAPAction.cs
│   ├── GOAPGoal.cs
│   └── GOAPPlanner.cs
├── Hybrid/           # 混合系统
│   ├── UtilityGOAPGoal.cs
│   ├── UtilityGOAPAgent.cs
│   └── UtilityGOAPAgentComponent.cs
├── Demo/             # 示例
│   ├── SurvivorAIDemo.cs
│   └── SimplePatrolAIDemo.cs
└── README.md
```

## 核心概念

### 1. Utility System（效用系统）

用于**评估**各种行为的**优先级/欲望强度**。

- `IUtilityConsideration`: 考虑因素接口，输出0-1的效用值
- `UtilityConsideration`: 带曲线映射的基础实现
  - 支持多种曲线类型：Linear, Quadratic, Sqrt, Inverse, Sigmoid, Custom
  - 可配置输入范围和权重
- `CompositeConsideration`: 复合考虑因素，支持Average/Min/Max/Multiply聚合

### 2. GOAP System（目标导向行动计划）

用于**规划**如何达成目标的具体**动作序列**。

- `GOAPWorldState`: 世界状态（128位位掩码，高效存储）
- `IGOAPAction`: 动作接口（含前提条件、效果、代价）
- `IGOAPGoal`: 目标接口（含期望世界状态、优先级）
- `GOAPPlanner`: 反向A*规划器

### 3. Hybrid（混合系统）

**Utility 决定 "想要什么"**，**GOAP 决定 "怎么做"**。

```
每帧流程：
1. Utility评估所有Goal的优先级
2. 选择优先级最高的有效Goal
3. GOAP为此Goal制定行动计划（A*搜索）
4. 逐帧执行计划中的动作
5. 定期重新评估和重新规划
```

## 快速开始

### 步骤1：定义世界状态

```csharp
// 注册世界状态名称（只需执行一次）
GOAPWorldStateRegistry.Register("HasFood");
GOAPWorldStateRegistry.Register("NearFood");
GOAPWorldStateRegistry.Register("IsSafe");
```

### 步骤2：创建目标（Goal）

```csharp
// 使用Utility驱动的目标
var eatGoal = UtilityGOAPGoal.CreateSimple(
    "Eat",                          // 目标名称
    "Hunger",                       // 上下文键（用于Utility评估）
    0f, 100f,                       // 输入值范围
    UtilityCurveType.Quadratic,     // 曲线类型
    1.0f,                           // 权重
    "HasFood"                       // 期望达到的世界状态
);
eatGoal.ActivationThreshold = 0.15f; // 最低激活阈值
```

### 步骤3：创建动作（Action）

```csharp
// 继承GOAPAction实现自定义动作
public class EatAction : GOAPContinuousAction
{
    public EatAction() : base("Eat", 2f) // 名称，持续时间
    {
        Cost = 0.5f;
        AddPrecondition("NearFood");  // 前提：必须在食物附近
        AddEffect("HasFood");         // 效果：获得食物状态
    }

    protected override bool OnFinish(IAIContext context)
    {
        // 动作完成时的逻辑
        hunger -= 40f;
        return true;
    }
}
```

### 步骤4：组装Agent

```csharp
public class MyAI : MonoBehaviour
{
    private UtilityGOAPAgentComponent _agent;

    void Start()
    {
        _agent = GetComponent<UtilityGOAPAgentComponent>();
        
        // 注册目标和动作
        _agent.RegisterGoal(eatGoal);
        _agent.RegisterGoal(sleepGoal);
        _agent.RegisterAction(new EatAction());
        _agent.RegisterAction(new MoveToFoodAction());
        
        // 设置初始上下文值
        _agent.SetContextValue("Hunger", 50f);
    }

    void Update()
    {
        // 更新上下文数据（供Utility评估使用）
        _agent.SetContextValue("Hunger", hunger);
        
        // Agent.Update() 已由组件自动调用
    }
}
```

## 框架特性

| 特性 | 说明 |
|------|------|
| 位掩码状态 | `GOAPWorldState` 使用128位位掩码，高效且零GC |
| 反向A*规划 | 从目标反向搜索到当前状态，标准GOAP算法 |
| 曲线映射 | Utility支持6种曲线类型 + 自定义AnimationCurve |
| 动态重规划 | 可配置重新规划间隔，应对环境变化 |
| 目标切换冷却 | 防止目标频繁切换，避免AI"抽搐" |
| 最低持续时间 | 目标激活后至少执行一段时间 |
| 纯代码驱动 | 不依赖任何Unity编辑器工具，完全代码配置 |

## Demo说明

### SurvivorAIDemo（幸存者AI）

一个完整的NPC AI示例，模拟幸存者管理需求：

- **状态**: 饥饿、口渴、疲劳、健康、安全
- **目标**: 进食(Eat)、喝水(Drink)、休息(Rest)、保持安全(BeSafe)
- **动作**: 移动到食物/水源/床、进食、喝水、休息、逃跑

### SimplePatrolAIDemo（简单巡逻AI）

最简化的示例，演示巡逻逻辑：
- **状态**: 警戒值
- **目标**: 巡逻(Patrol)、警戒(Alert)
- **动作**: 移动到巡逻点、观察四周

## 扩展建议

1. **添加更多曲线类型**: 在 `UtilityConsideration.ApplyCurve()` 中扩展
2. **更复杂的世界状态**: 如需超过128个状态，可扩展 `GOAPWorldState` 使用更多字段
3. **并行动作**: 当前框架串行执行，可扩展支持并行动作组
4. **计划中断**: 框架已支持 `OnAbort()`，可扩展更复杂的中断逻辑
5. **感知系统**: 将感知层（视觉、听觉）独立出来，写入AI上下文

## 注意事项

- 世界状态名称需在首次使用前通过 `GOAPWorldStateRegistry.Register()` 注册
- `GOAPWorldStateRegistry.Clear()` 会清空所有注册，谨慎在运行时使用
- 规划器有最大搜索节点限制（默认1000），复杂场景可能需要调整
- 所有动作的前提条件和效果在构造函数中配置
