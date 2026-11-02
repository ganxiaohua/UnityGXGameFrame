using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace GameFrame.Runtime.UnityUI.Samples
{
    public sealed class SamplePanel : TransitionPanel
    {
        public override string Package => "UnityUI/Samples";
        public override string Name => "SamplePanel";
        public override string PrefabPath => "UnityUI/Samples/SamplePanel";
        public override PanelFlag Flags => PanelFlag.Builtin;
        public override float ShowDuration => 0.15f;
        public override float HideDuration => 0.15f;

        private Text message;
        private Button close;

        public override async UniTask OnInitializeAsync(RectTransform root, CancellationToken cancelToken = default)
        {
            await base.OnInitializeAsync(root, cancelToken);
            message = root.Find("Message").GetComponent<Text>();
            close = root.Find("Close").GetComponent<Button>();
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            message.font = font;
            close.GetComponentInChildren<Text>().font = font;
            close.onClick.AddListener(Close);
            Debug.Log("UnityUI sample: initialized once per instance.");
        }

        public override void OnShow(object args = null)
        {
            base.OnShow(args);
            message.text = args as string ?? "UnityUI panel";
        }

        private void Close() => UISystem.Instance.HidePanel(this);

        public override void Dispose()
        {
            if (close != null) close.onClick.RemoveListener(Close);
            message = null;
            close = null;
            base.Dispose();
        }
    }
}
