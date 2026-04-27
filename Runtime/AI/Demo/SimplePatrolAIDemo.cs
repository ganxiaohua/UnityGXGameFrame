using UnityEngine;
using UnityGXGameFrame.AI.Utility;
using UnityGXGameFrame.AI.GOAP;
using UnityGXGameFrame.AI.Hybrid;

namespace UnityGXGameFrame.AI.Demo
{
    /// <summary>
    /// 简单巡逻AI Demo
    /// 演示最基础的Utility + GOAP用法
    /// </summary>
    public class SimplePatrolAIDemo : MonoBehaviour
    {
        [Header("Patrol Points")]
        public Transform[] PatrolPoints;

        [Header("AI Stats")]
        [Range(0f, 100f)] public float AlertLevel = 0f;
        public float AlertDecay = 5f;

        [Header("Detection")]
        public float DetectionRadius = 5f;
        public LayerMask PlayerLayer;

        private UtilityGOAPAgentComponent _agent;
        private int _currentPatrolIndex = 0;
        private bool _isAtPatrolPoint;
        private bool _playerDetected;

        void Start()
        {
            _agent = GetComponent<UtilityGOAPAgentComponent>();
            if (_agent == null)
                _agent = gameObject.AddComponent<UtilityGOAPAgentComponent>();

            // 注册状态
            GOAPWorldStateRegistry.Register("AtPatrolPoint");
            GOAPWorldStateRegistry.Register("PlayerFound");
            GOAPWorldStateRegistry.Register("AlertHigh");

            SetupGoals();
            SetupActions();
        }

        void Update()
        {
            // 检测玩家
            Collider[] hits = Physics.OverlapSphere(transform.position, DetectionRadius, PlayerLayer);
            _playerDetected = hits.Length > 0;
            if (_playerDetected)
                AlertLevel = Mathf.Min(100f, AlertLevel + 30f * Time.deltaTime);
            else
                AlertLevel = Mathf.Max(0f, AlertLevel - AlertDecay * Time.deltaTime);

            // 更新上下文
            _agent.SetContextValue("AlertLevel", AlertLevel);
            _agent.SetContextValue("PlayerDetected", _playerDetected);

            // 更新世界状态
            _isAtPatrolPoint = false;
            if (PatrolPoints.Length > 0)
            {
                float dist = Vector3.Distance(transform.position, PatrolPoints[_currentPatrolIndex].position);
                _isAtPatrolPoint = dist < 0.5f;
            }

            _agent.SetWorldState("AtPatrolPoint", _isAtPatrolPoint);
            _agent.SetWorldState("PlayerFound", _playerDetected);
            _agent.SetWorldState("AlertHigh", AlertLevel > 50f);
        }

        void SetupGoals()
        {
            // 巡逻目标：低警戒时优先级高
            var patrolGoal = UtilityGOAPGoal.CreateSimple(
                "Patrol",
                "AlertLevel",
                0f, 100f,
                UtilityCurveType.Inverse, // 警戒越低，越倾向于巡逻
                1.0f,
                "AtPatrolPoint"
            );
            patrolGoal.ActivationThreshold = 0.2f;
            _agent.RegisterGoal(patrolGoal);

            // 警戒目标：警戒高时优先级高
            var alertGoal = UtilityGOAPGoal.CreateSimple(
                "Alert",
                "AlertLevel",
                0f, 100f,
                UtilityCurveType.Quadratic,
                1.5f,
                "AlertHigh"
            );
            alertGoal.ActivationThreshold = 0.3f;
            _agent.RegisterGoal(alertGoal);
        }

        void SetupActions()
        {
            // 移动到巡逻点
            _agent.RegisterAction(new PatrolMoveAction(this));
            // 等待/观察
            _agent.RegisterAction(new LookAroundAction(this));
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, DetectionRadius);
        }

        // ==================== 动作实现 ====================

        private class PatrolMoveAction : GOAPAction
        {
            private SimplePatrolAIDemo _demo;
            private float _moveSpeed = 2f;

            public PatrolMoveAction(SimplePatrolAIDemo demo) : base("MoveToPatrolPoint")
            {
                _demo = demo;
                Cost = 1f;
                AddEffect("AtPatrolPoint");
            }

            public override bool CheckProceduralPrecondition(IAIContext context)
            {
                return _demo.PatrolPoints.Length > 0;
            }

            public override bool Perform(IAIContext context, float deltaTime)
            {
                var target = _demo.PatrolPoints[_demo._currentPatrolIndex];
                Vector3 dir = (target.position - _demo.transform.position).normalized;
                _demo.transform.position += dir * _moveSpeed * deltaTime;
                _demo.transform.LookAt(target.position);

                float dist = Vector3.Distance(_demo.transform.position, target.position);
                if (dist < 0.5f)
                {
                    _demo._currentPatrolIndex = (_demo._currentPatrolIndex + 1) % _demo.PatrolPoints.Length;
                    IsRunning = false;
                    return true;
                }
                return true;
            }
        }

        private class LookAroundAction : GOAPContinuousAction
        {
            private SimplePatrolAIDemo _demo;
            private float _rotateAmount = 0f;

            public LookAroundAction(SimplePatrolAIDemo demo) : base("LookAround", 2f)
            {
                _demo = demo;
                Cost = 0.5f;
                AddPrecondition("AtPatrolPoint");
                AddEffect("AlertHigh");
            }

            protected override void OnUpdate(IAIContext context, float deltaTime)
            {
                _rotateAmount += 90f * deltaTime;
                _demo.transform.Rotate(0f, 90f * deltaTime, 0f);
            }

            protected override bool OnFinish(IAIContext context)
            {
                _demo.AlertLevel = Mathf.Max(0f, _demo.AlertLevel - 20f);
                return true;
            }
        }
    }
}
