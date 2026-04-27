using System;
using UnityEngine;

namespace UnityGXGameFrame.AI.Utility
{
    /// <summary>
    /// 基础考虑因素接口
    /// </summary>
    public interface IUtilityConsideration
    {
        float Evaluate(IAIContext context);
    }

    /// <summary>
    /// AI上下文接口，用于传递运行时数据
    /// </summary>
    public interface IAIContext
    {
        T Get<T>(string key);
        void Set<T>(string key, T value);
        bool Has(string key);
    }

    /// <summary>
    /// 简单的AI上下文实现
    /// </summary>
    public class SimpleAIContext : IAIContext
    {
        private System.Collections.Generic.Dictionary<string, object> _data = new();

        public T Get<T>(string key)
        {
            if (_data.TryGetValue(key, out var value) && value is T t)
                return t;
            return default;
        }

        public void Set<T>(string key, T value)
        {
            _data[key] = value;
        }

        public bool Has(string key)
        {
            return _data.ContainsKey(key);
        }
    }

    /// <summary>
    /// 曲线类型枚举
    /// </summary>
    public enum UtilityCurveType
    {
        Linear,         // 线性
        Quadratic,      // 二次方
        Sqrt,           // 平方根
        Inverse,        // 反比
        Sigmoid,        // S型曲线
        Custom          // 自定义曲线
    }

    /// <summary>
    /// 带曲线映射的考虑因素
    /// </summary>
    [Serializable]
    public class UtilityConsideration : IUtilityConsideration
    {
        [Tooltip("考虑因素名称")]
        public string Name;
        
        [Tooltip("从上下文中读取的键")]
        public string ContextKey;
        
        [Tooltip("输入值的最小范围")]
        public float InputMin = 0f;
        
        [Tooltip("输入值的最大范围")]
        public float InputMax = 1f;
        
        [Tooltip("曲线类型")]
        public UtilityCurveType CurveType = UtilityCurveType.Linear;
        
        [Tooltip("权重")]
        [Range(0f, 1f)]
        public float Weight = 1f;

        [Tooltip("自定义动画曲线（当CurveType为Custom时使用）")]
        public AnimationCurve CustomCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        public float Evaluate(IAIContext context)
        {
            if (!context.Has(ContextKey))
                return 0f;

            float rawValue = context.Get<float>(ContextKey);
            float normalized = Mathf.InverseLerp(InputMin, InputMax, rawValue);
            normalized = Mathf.Clamp01(normalized);
            
            float curvedValue = ApplyCurve(normalized);
            return curvedValue * Weight;
        }

        private float ApplyCurve(float t)
        {
            switch (CurveType)
            {
                case UtilityCurveType.Linear:
                    return t;
                case UtilityCurveType.Quadratic:
                    return t * t;
                case UtilityCurveType.Sqrt:
                    return Mathf.Sqrt(t);
                case UtilityCurveType.Inverse:
                    return 1f - t;
                case UtilityCurveType.Sigmoid:
                    return 1f / (1f + Mathf.Exp(-10f * (t - 0.5f)));
                case UtilityCurveType.Custom:
                    return CustomCurve.Evaluate(t);
                default:
                    return t;
            }
        }
    }

    /// <summary>
    /// 复合考虑因素（使用聚合模式组合多个子考虑因素）
    /// </summary>
    public class CompositeConsideration : IUtilityConsideration
    {
        public enum AggregationType
        {
            Average,    // 平均值
            Min,        // 最小值（短板效应）
            Max,        // 最大值
            Multiply    // 乘积
        }

        public AggregationType Aggregation = AggregationType.Average;
        public System.Collections.Generic.List<IUtilityConsideration> Considerations = new();

        public float Evaluate(IAIContext context)
        {
            if (Considerations.Count == 0)
                return 0f;

            float result = 0f;
            switch (Aggregation)
            {
                case AggregationType.Average:
                    float sum = 0f;
                    foreach (var c in Considerations)
                        sum += c.Evaluate(context);
                    result = sum / Considerations.Count;
                    break;
                case AggregationType.Min:
                    result = float.MaxValue;
                    foreach (var c in Considerations)
                        result = Mathf.Min(result, c.Evaluate(context));
                    break;
                case AggregationType.Max:
                    result = float.MinValue;
                    foreach (var c in Considerations)
                        result = Mathf.Max(result, c.Evaluate(context));
                    break;
                case AggregationType.Multiply:
                    result = 1f;
                    foreach (var c in Considerations)
                        result *= c.Evaluate(context);
                    break;
            }
            return result;
        }
    }
}
