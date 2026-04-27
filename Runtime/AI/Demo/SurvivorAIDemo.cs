using UnityEngine;
using UnityGXGameFrame.AI.Utility;
using UnityGXGameFrame.AI.GOAP;
using UnityGXGameFrame.AI.Hybrid;

namespace UnityGXGameFrame.AI.Demo
{
    /// <summary>
    /// 幸存者AI Demo
    /// 演示Utility + GOAP混合框架的实际使用
    /// 
    /// 场景设定：
    /// - 一个幸存者NPC需要管理饥饿、口渴、疲劳、安全等需求
    /// - Utility系统负责评估各项需求的紧急程度（决定目标优先级）
    /// - GOAP系统负责规划如何满足选中的目标（找食物、喝水、休息、逃跑等）
    /// </summary>
    public class SurvivorAIDemo : MonoBehaviour
    {
        [Header("Survivor Stats (0-100)")]
        [Range(0f, 100f)] public float Hunger = 50f;
        [Range(0f, 100f)] public float Thirst = 50f;
        [Range(0f, 100f)] public float Fatigue = 30f;
        [Range(0f, 100f)] public float Health = 100f;
        [Range(0f, 100f)] public float Safety = 80f;

        [Header("World Objects")]
        public Transform FoodSource;
        public Transform WaterSource;
        public Transform Bed;
        public Transform Enemy;

        [Header("Settings")]
        public float HungerDecay = 2f;
        public float ThirstDecay = 3f;
        public float FatigueGrowth = 1.5f;

        private UtilityGOAPAgentComponent _agentComponent;
        private bool _hasFood;
        private bool _hasWater;
        private bool _isRested;
        private bool _isSafe = true;

        void Start()
        {
            _agentComponent = GetComponent<UtilityGOAPAgentComponent>();
            if (_agentComponent == null)
            {
                _agentComponent = gameObject.AddComponent<UtilityGOAPAgentComponent>();
                _agentComponent.AgentName = "Survivor";
                _agentComponent.DebugMode = true;
            }

            InitializeWorldStates();
            SetupGoals();
            SetupActions();
        }

        void Update()
        {
            // 模拟属性变化
            Hunger = Mathf.Min(100f, Hunger + HungerDecay * Time.deltaTime);
            Thirst = Mathf.Min(100f, Thirst + ThirstDecay * Time.deltaTime);
            Fatigue = Mathf.Min(100f, Fatigue + FatigueGrowth * Time.deltaTime);

            // 更新到AI上下文
            UpdateContext();

            // 更新世界状态（基于实际条件）
            UpdateWorldState();

            // 在Inspector中显示AI状态
            DrawDebugInfo();
        }

        /// <summary>
        /// 初始化世界状态注册
        /// </summary>
        void InitializeWorldStates()
        {
            GOAPWorldStateRegistry.Clear();
            GOAPWorldStateRegistry.Register("HasFood");
            GOAPWorldStateRegistry.Register("HasWater");
            GOAPWorldStateRegistry.Register("IsRested");
            GOAPWorldStateRegistry.Register("IsSafe");
            GOAPWorldStateRegistry.Register("NearFood");
            GOAPWorldStateRegistry.Register("NearWater");
            GOAPWorldStateRegistry.Register("NearBed");
            GOAPWorldStateRegistry.Register("NearEnemy");
        }

        /// <summary>
        /// 设置Utility驱动的GOAP目标
        /// </summary>
        void SetupGoals()
        {
            // 1. 进食目标：饥饿越高优先级越高
            var eatGoal = UtilityGOAPGoal.CreateSimple(
                "Eat",
                "Hunger",       // 上下文键
                0f, 100f,       // 输入范围
                UtilityCurveType.Quadratic, // 饥饿越高，效用增长越快
                1.0f,           // 权重
                "HasFood"       // 期望达到的世界状态
            );
            eatGoal.ActivationThreshold = 0.15f;
            eatGoal.MinDuration = 1f;
            _agentComponent.RegisterGoal(eatGoal);

            // 2. 喝水目标：口渴越高优先级越高
            var drinkGoal = UtilityGOAPGoal.CreateSimple(
                "Drink",
                "Thirst",
                0f, 100f,
                UtilityCurveType.Quadratic,
                1.2f,           // 口渴权重略高于饥饿
                "HasWater"
            );
            drinkGoal.ActivationThreshold = 0.2f;
            drinkGoal.MinDuration = 1f;
            _agentComponent.RegisterGoal(drinkGoal);

            // 3. 休息目标：疲劳越高优先级越高
            var restGoal = UtilityGOAPGoal.CreateSimple(
                "Rest",
                "Fatigue",
                0f, 100f,
                UtilityCurveType.Sigmoid, // S型曲线，中等疲劳时开始显著上升
                0.8f,
                "IsRested"
            );
            restGoal.ActivationThreshold = 0.3f;
            restGoal.MinDuration = 2f;
            _agentComponent.RegisterGoal(restGoal);

            // 4. 安全目标：安全值越低优先级越高（反比曲线）
            var safetyGoal = UtilityGOAPGoal.CreateSimple(
                "BeSafe",
                "Safety",
                0f, 100f,
                UtilityCurveType.Inverse, // 安全值越低，优先级越高
                2.0f,           // 安全权重最高
                "IsSafe"
            );
            safetyGoal.ActivationThreshold = 0.1f;
            safetyGoal.MinDuration = 1f;
            _agentComponent.RegisterGoal(safetyGoal);
        }

        /// <summary>
        /// 设置GOAP动作
        /// </summary>
        void SetupActions()
        {
            // 移动到食物
            _agentComponent.RegisterAction(new MoveToAction("MoveToFood", "NearFood", FoodSource, this));
            // 进食
            _agentComponent.RegisterAction(new EatAction("EatFood", this));
            // 移动到水源
            _agentComponent.RegisterAction(new MoveToAction("MoveToWater", "NearWater", WaterSource, this));
            // 喝水
            _agentComponent.RegisterAction(new DrinkAction("DrinkWater", this));
            // 移动到床
            _agentComponent.RegisterAction(new MoveToAction("MoveToBed", "NearBed", Bed, this));
            // 休息
            _agentComponent.RegisterAction(new RestAction("Sleep", this));
            // 逃跑（远离敌人）
            _agentComponent.RegisterAction(new FleeAction("Flee", Enemy, this));
        }

        /// <summary>
        /// 更新AI上下文数据
        /// </summary>
        void UpdateContext()
        {
            _agentComponent.SetContextValue("Hunger", Hunger);
            _agentComponent.SetContextValue("Thirst", Thirst);
            _agentComponent.SetContextValue("Fatigue", Fatigue);
            _agentComponent.SetContextValue("Health", Health);
            _agentComponent.SetContextValue("Safety", Safety);
            _agentComponent.SetContextValue("HasFood", _hasFood);
            _agentComponent.SetContextValue("HasWater", _hasWater);
            _agentComponent.SetContextValue("IsRested", _isRested);
            _agentComponent.SetContextValue("IsSafe", _isSafe);
        }

        /// <summary>
        /// 更新世界状态
        /// </summary>
        void UpdateWorldState()
        {
            // 更新位置相关状态
            float foodDist = FoodSource != null ? Vector3.Distance(transform.position, FoodSource.position) : 999f;
            float waterDist = WaterSource != null ? Vector3.Distance(transform.position, WaterSource.position) : 999f;
            float bedDist = Bed != null ? Vector3.Distance(transform.position, Bed.position) : 999f;
            float enemyDist = Enemy != null ? Vector3.Distance(transform.position, Enemy.position) : 999f;

            _agentComponent.SetWorldState("NearFood", foodDist < 1.5f);
            _agentComponent.SetWorldState("NearWater", waterDist < 1.5f);
            _agentComponent.SetWorldState("NearBed", bedDist < 1.5f);
            _agentComponent.SetWorldState("NearEnemy", enemyDist < 5f);

            // 更新物品状态
            _agentComponent.SetWorldState("HasFood", _hasFood);
            _agentComponent.SetWorldState("HasWater", _hasWater);
            _agentComponent.SetWorldState("IsRested", _isRested);
            _agentComponent.SetWorldState("IsSafe", _isSafe);

            // 安全值计算
            Safety = Mathf.Clamp(100f - (enemyDist < 10f ? (10f - enemyDist) * 10f : 0f), 0f, 100f);
            _isSafe = enemyDist > 8f;
        }

        void DrawDebugInfo()
        {
            // 运行时Inspector更新已在Update中完成
        }

        // ============================================================
        // 具体动作实现
        // ============================================================

        /// <summary>
        /// 移动到目标位置
        /// </summary>
        private class MoveToAction : GOAPContinuousAction
        {
            private Transform _target;
            private SurvivorAIDemo _demo;
            private string _arriveStateName;

            public MoveToAction(string name, string arriveState, Transform target, SurvivorAIDemo demo) : base(name, 0f)
            {
                _arriveStateName = arriveState;
                _target = target;
                _demo = demo;
                Cost = 1f;

                // 效果：到达目标位置
                AddEffect(arriveState);
            }

            protected override void OnUpdate(IAIContext context, float deltaTime)
            {
                if (_target == null) return;

                Vector3 dir = (_target.position - _demo.transform.position).normalized;
                _demo.transform.position += dir * 3f * deltaTime;
                _demo.transform.LookAt(_target.position);
            }

            protected override bool OnFinish(IAIContext context)
            {
                return _target != null && Vector3.Distance(_demo.transform.position, _target.position) < 1.5f;
            }

            public override bool CheckProceduralPrecondition(IAIContext context)
            {
                return _target != null;
            }
        }

        /// <summary>
        /// 进食动作
        /// </summary>
        private class EatAction : GOAPContinuousAction
        {
            private SurvivorAIDemo _demo;

            public EatAction(string name, SurvivorAIDemo demo) : base(name, 2f)
            {
                _demo = demo;
                Cost = 0.5f;
                AddPrecondition("NearFood");
                AddEffect("HasFood");
            }

            protected override void OnUpdate(IAIContext context, float deltaTime)
            {
                // 进食中...
            }

            protected override bool OnFinish(IAIContext context)
            {
                _demo.Hunger = Mathf.Max(0f, _demo.Hunger - 40f);
                _demo._hasFood = true;
                Debug.Log($"[{_demo.name}] 进食完成，饥饿值降至 {_demo.Hunger:F1}");
                return true;
            }

            public override bool CheckProceduralPrecondition(IAIContext context)
            {
                return context.Get<bool>("NearFood");
            }
        }

        /// <summary>
        /// 喝水动作
        /// </summary>
        private class DrinkAction : GOAPContinuousAction
        {
            private SurvivorAIDemo _demo;

            public DrinkAction(string name, SurvivorAIDemo demo) : base(name, 1.5f)
            {
                _demo = demo;
                Cost = 0.5f;
                AddPrecondition("NearWater");
                AddEffect("HasWater");
            }

            protected override bool OnFinish(IAIContext context)
            {
                _demo.Thirst = Mathf.Max(0f, _demo.Thirst - 50f);
                _demo._hasWater = true;
                Debug.Log($"[{_demo.name}] 喝水完成，口渴值降至 {_demo.Thirst:F1}");
                return true;
            }

            public override bool CheckProceduralPrecondition(IAIContext context)
            {
                return context.Get<bool>("NearWater");
            }
        }

        /// <summary>
        /// 休息动作
        /// </summary>
        private class RestAction : GOAPContinuousAction
        {
            private SurvivorAIDemo _demo;

            public RestAction(string name, SurvivorAIDemo demo) : base(name, 3f)
            {
                _demo = demo;
                Cost = 0.3f;
                AddPrecondition("NearBed");
                AddEffect("IsRested");
            }

            protected override void OnUpdate(IAIContext context, float deltaTime)
            {
                // 休息中...
            }

            protected override bool OnFinish(IAIContext context)
            {
                _demo.Fatigue = Mathf.Max(0f, _demo.Fatigue - 60f);
                _demo._isRested = true;
                Debug.Log($"[{_demo.name}] 休息完成，疲劳值降至 {_demo.Fatigue:F1}");
                return true;
            }

            public override bool CheckProceduralPrecondition(IAIContext context)
            {
                return context.Get<bool>("NearBed");
            }
        }

        /// <summary>
        /// 逃跑动作
        /// </summary>
        private class FleeAction : GOAPContinuousAction
        {
            private Transform _enemy;
            private SurvivorAIDemo _demo;
            private float _fleeDistance = 12f;

            public FleeAction(string name, Transform enemy, SurvivorAIDemo demo) : base(name, 0f)
            {
                _enemy = enemy;
                _demo = demo;
                Cost = 2f; // 逃跑代价较高，但如果Utility评分高仍会被选中
                AddPrecondition("NearEnemy");
                AddEffect("IsSafe");
            }

            protected override void OnUpdate(IAIContext context, float deltaTime)
            {
                if (_enemy == null) return;

                Vector3 fleeDir = (_demo.transform.position - _enemy.position).normalized;
                _demo.transform.position += fleeDir * 5f * deltaTime;
            }

            protected override bool OnFinish(IAIContext context)
            {
                return _enemy == null || Vector3.Distance(_demo.transform.position, _enemy.position) >= _fleeDistance;
            }

            public override bool CheckProceduralPrecondition(IAIContext context)
            {
                return _enemy != null && Vector3.Distance(_demo.transform.position, _enemy.position) < 8f;
            }
        }
    }
}
