using UnityEngine;

namespace GameFrame.Runtime.UnityUI
{
    /// <summary>Preserves the global input switch across Canvas.overrideSorting boundaries.</summary>
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    public sealed class PanelInputGate : MonoBehaviour, ICanvasRaycastFilter
    {
        internal bool InputEnabled { get; set; } = true;
        public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera) => InputEnabled;
    }
}
