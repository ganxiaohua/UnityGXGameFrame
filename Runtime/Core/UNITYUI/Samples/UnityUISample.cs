using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameFrame.Runtime.UnityUI.Samples
{
    /// <summary>Attach to an empty scene. Enable GAMEFRAME_USE_UNITYUI before entering Play mode.</summary>
    public sealed class UnityUISample : MonoBehaviour
    {
        private void Start() => StartAsync().Forget();

        private async UniTask StartAsync()
        {
            if (!GXGameFrame.UsesUnityUI)
            {
                Debug.LogError("Enable GAMEFRAME_USE_UNITYUI to run the UnityUI sample.", this);
                enabled = false;
                return;
            }
            if (GXGameFrame.Instance.RootEntity == null) GXGameFrame.Instance.Init();
            await UISystem.Instance.ShowUniquePanelAsync<SamplePanel>("UnityUI / Init + OnShow\nClick Close, then press Space to reopen.",
                cancelToken: this.GetCancellationTokenOnDestroy());
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) ShowSample();
        }

        [ContextMenu("Show sample")]
        public void ShowSample()
        {
            if (!Application.isPlaying || !GXGameFrame.UsesUnityUI) return;
            UISystem.Instance.ShowUniquePanelAsync<SamplePanel>("Opened again through ShowUniquePanelAsync.",
                cancelToken: this.GetCancellationTokenOnDestroy()).Forget();
        }

        private void OnDestroy()
        {
            if (Application.isPlaying && GXGameFrame.UsesUnityUI) UISystem.Instance.DestroyUniquePanel<SamplePanel>();
        }
    }
}
