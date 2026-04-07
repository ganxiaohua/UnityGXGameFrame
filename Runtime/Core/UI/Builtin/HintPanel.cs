using System.Threading;
using Cysharp.Threading.Tasks;
using FairyGUI;

namespace GameFrame.Runtime
{
    public sealed partial class HintPanel : TransitionPanel
    {
        public override string Package => "Common";
        public override string Name => "AreaText";
        public override PanelFlag Flags =>   PanelFlag.Persistent;
        public override int SortingOrder => BuiltinPanelOrder.Hint;

        public class Input
        {
            public string content;
            public bool alert = false;
            public bool touchable = false;
        }

        private GTextField content;

        private Input input;

        public override async UniTask OnInitializeAsync(GComponent root, CancellationToken cancelToken = default)
        {
            await base.OnInitializeAsync(root, cancelToken);

            content = (GTextField) root.GetChild("Content");
        }

        public override void OnShow(object args = null)
        {
            base.OnShow(args);

            input = (Input) args;

            Assert.IsTrue(input != null, "Input params can't be null");

            content.text = input.content;

            Root.sortingOrder = input.alert ? BuiltinPanelOrder.Alert : BuiltinPanelOrder.Hint;

            Root.touchable = input.touchable;
        }

        protected override void OnShowTransitionComplete()
        {
            UISystem.Instance.HidePanel(this);
        }
    }
}