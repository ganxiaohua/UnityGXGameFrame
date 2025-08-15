#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using FairyGUI;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameFrame.Runtime
{
    [HideMonoScript]
    internal class DebugInspector : MonoBehaviour
    {
        [ShowInInspector]
        public class LoopDataWrap
        {
            [HideInInspector] public int index;
            [HideLabel] [FoldoutGroup("@index")] public object data;
        }

        [ShowInInspector]
        [InlineButton(nameof(OpenScript), "Open")]
        private string Script
        {
            get => itemType.Name;
            set => _ = value;
        }

        [ShowInInspector]
        public List<LoopDataWrap> Datas
        {
            get
            {
                var list = new List<LoopDataWrap>();
                var dataField = itemType.GetProperty("Data");
                foreach (ILoopItem item in (IEnumerable) loopList)
                {
                    var wrap = new LoopDataWrap
                    {
                            index = item.GlobalIndex,
                            data = dataField?.GetValue(item)
                    };
                    list.Add(wrap);
                }

                list.Sort((a, b) => a.index - b.index);
                return list;
            }
        }

        private ILoopList loopList;
        private Type itemType;

        private void OpenScript()
        {
            DebugHelper.OpenScript(Script);
        }

        public static void Register(ILoopList loopList, GList list, Type itemType)
        {
            if (!Application.isPlaying) return;

            var go = list.displayObject.gameObject;
            if (!go) return;

            if (!go.TryGetComponent(out DebugInspector debugInspector))
                debugInspector = go.AddComponent<DebugInspector>();

            debugInspector.loopList = loopList;
            debugInspector.itemType = itemType;
        }
    }
}
#endif