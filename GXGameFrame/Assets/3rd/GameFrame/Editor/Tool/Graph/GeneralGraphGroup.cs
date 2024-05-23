using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameFrame.Editor
{
    public class GeneralGraphGroup: EditorEntity
    {
        private UnityEditor.Experimental.GraphView.Group group;
        
        public override void Init(object obj)
        {
            group = new UnityEditor.Experimental.GraphView.Group();
            group.layer = 2;
            ((GraphView)obj).AddElement(group);
        }
        
        public void CreateList(List<string> dataList,Rect pos)
        {
            group.Clear();
            var listContainer = new VisualElement();
            listContainer.style.flexDirection = FlexDirection.Column;
            foreach (var data in dataList)
            {
                var item = new Label(data);
                listContainer.Add(item);
            }
            group.Add(listContainer);
            group.SetPosition(new Rect(pos.x+pos.width+10, pos.y, group.resolvedStyle.width, group.resolvedStyle.height));
        }
        
        public override void Clear()
        {
            base.Clear();
            group?.Clear();
            group = null;
        }
    }
}