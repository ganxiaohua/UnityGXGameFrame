using System.Collections.Generic;
using GameFrame.Runtime;
using UnityEditor;
using UnityEngine;
using OdinEditorWindow = Sirenix.OdinInspector.Editor.OdinEditorWindow;

namespace GameFrame.Editor
{
    public class CapabilityView : OdinEditorWindow
    {
        private enum CapabilityType
        {
            NotAction = 1,
            Action,
            Lock
        }

        private static CapabilityView sWindow;
        private EffEntity effEntity;
        private ECCWorld eccWorld;
        private List<CapabilityBase> capabilityBaseUpdateMode = new List<CapabilityBase>();
        private List<CapabilityBase> capabilityBaseFixUpdateMode = new List<CapabilityBase>();
        private ArrayExSimilar[] updateMode;
        private ArrayExSimilar[] fixUpdateMode;
        private int tailFrame = 0;
        private int headFrame = 0;
        private int FrameSize = 500;
        private Vector2 scrollPosition = Vector2.zero;
        private Color[] stateColor = new Color[] {Color.white, Color.gray, Color.cyan, Color.yellow};

        public static void Init(EffEntity effEntity)
        {
            sWindow ??= GetWindow<CapabilityView>();
            sWindow.titleContent.text = string.IsNullOrEmpty(effEntity.Name) ? "Entity" : effEntity.Name;
            sWindow.effEntity = effEntity;
            sWindow.eccWorld = (ECCWorld) effEntity.world;
            sWindow.Start();
            sWindow.Show();
        }

        private void Start()
        {
            capabilityBaseUpdateMode.Clear();
            capabilityBaseFixUpdateMode.Clear();
            eccWorld.GetCapability(effEntity, capabilityBaseUpdateMode, capabilityBaseFixUpdateMode);
            updateMode = new ArrayExSimilar[capabilityBaseUpdateMode.Count];
            fixUpdateMode = new ArrayExSimilar[capabilityBaseFixUpdateMode.Count];
        }

        protected override void OnBeginDrawEditors()
        {
            base.OnBeginDrawEditors();
            if (effEntity == null)
                return;
            if (effEntity.State == IEntity.EntityState.IsClear)
            {
                return;
            }

            EditorGUILayout.LabelField("蓝色表示激活，白色表示未激活，黄色表示被锁住了");
            if (EditorApplication.isPlaying && !EditorApplication.isPaused)
            {
                SetData(capabilityBaseUpdateMode, updateMode);
                SetData(capabilityBaseFixUpdateMode, fixUpdateMode);
                tailFrame++;
                if (tailFrame >= FrameSize)
                {
                    headFrame = (tailFrame + 1) % FrameSize;
                }
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.LabelField("___________________________update");
            if (updateMode != null)
                Draw(updateMode);
            EditorGUILayout.Space(2 * updateMode.Length);
            EditorGUILayout.LabelField("___________________________fixedUpdate");
            if (fixUpdateMode != null)
                Draw(fixUpdateMode);
            EditorGUILayout.EndScrollView();
        }

        private void SetData(List<CapabilityBase> baseMode, ArrayExSimilar[] capabilityTypes)
        {
            int count = baseMode.Count;
            for (int i = 0; i < count; i++)
            {
                var capability = baseMode[i];
                capabilityTypes[i] ??= new ArrayExSimilar(FrameSize, capability.GetType().Name);
                bool islock = false;
                if (capability.TagList != null)
                    islock = eccWorld.IsBindCapability(effEntity, capability.TagList);
                capabilityTypes[i].Add(tailFrame % FrameSize, (int) (islock ? CapabilityType.Lock : (capability.IsActive ? CapabilityType.Action : CapabilityType.NotAction)), headFrame);
            }
        }

        private void Draw(ArrayExSimilar[] capabilityTypes)
        {
            var oldColor = GUI.contentColor;
            for (int j = 0; j < capabilityTypes.Length; j++)
            {
                var exSimilar = capabilityTypes[j];
                if (exSimilar == null)
                    continue;
                var datas = exSimilar.GetData();
                EditorGUILayout.BeginHorizontal();
                var pos = EditorGUILayout.GetControlRect();
                int offsetX = 0;
                pos.y += 2 * j;
                for (int i = 0; i < datas.Count; i++)
                {
                    var data = datas[i];
                    Color color = stateColor[data.State];
                    Rect rect = new Rect(offsetX, pos.y, data.Count, 20);
                    offsetX += data.Count;
                    GUI.color = color;
                    EditorGUI.DrawRect(rect, GUI.color);
                    GUI.color = color;
                }

                pos.x = FrameSize;
                EditorGUI.LabelField(pos, exSimilar.Name);
                EditorGUILayout.EndVertical();
            }

            GUI.color = oldColor;
        }


        protected override void OnImGUI()
        {
            base.OnImGUI();
            Repaint();
        }

        public static void Destroy()
        {
            sWindow?.Close();
        }

        protected override void OnDestroy()
        {
            sWindow = null;
        }
    }
}