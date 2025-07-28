using System;
using UnityEditor;
using UnityEngine;

namespace GameFrame.Runtime.Editor
{
    public abstract class VirtualDialog : ScriptableObject
    {
        const int TITLE_HEIGHT = 20;

        public GUIContent titleContent;

        private Rect rect = Rect.zero;

        public Vector2 size
        {
            get => rect.size;
            set => rect.size = value;
        }

        public Vector2 position
        {
            get => rect.position;
            set => rect.position = value;
        }

        public Vector2 center
        {
            get => rect.center;
            set => rect.center = value;
        }

        public float width
        {
            get => rect.width;
            set => rect.width = value;
        }

        public float height
        {
            get => rect.height;
            set => rect.height = value;
        }

        private Vector2 _minSize = Vector2.zero;

        public Vector2 minSize
        {
            get => _minSize;
            set
            {
                _minSize = value;
                size = Vector2.Max(_minSize, size);
            }
        }

        private Vector2 _maxSize = Vector2.zero;

        public Vector2 maxSize
        {
            get => _maxSize;
            set
            {
                _maxSize = value;
                size = Vector2.Min(_maxSize, size);
            }
        }

        public Rect clientRect
        {
            get
            {
                var rc = rect;
                if (titlebar)
                {
                    rc.y += TITLE_HEIGHT;
                    rc.height -= TITLE_HEIGHT;
                }

                rc.position += Vector2.one * 2;
                rc.size -= Vector2.one * 4;
                return rc;
            }
        }

        public bool titlebar = true;

        public bool dragable = true;

        public bool visible = true;

        public bool top => child == null;

        public bool focus => top && container && EditorWindow.focusedWindow == container;

        public VirtualDialog parent { get; private set; }
        public VirtualDialog child  { get; private set; }

        private VirtualDialogContainer _container;

        public VirtualDialogContainer container
        {
            get
            {
                if (_container == null)
                    return VirtualDialogContainer.temp;
                return _container;
            }
            set { _container = value; }
        }

        public Action delayCall;

        public bool modified
        {
            get => container?.modified ?? false;
            protected set
            {
                if (container) container.modified = value;
            }
        }

        public static T CreateDialog<T>(VirtualDialog parent) where T : VirtualDialog
        {
            T dialog = CreateInstance<T>();
            dialog.hideFlags = HideFlags.DontSave;
            dialog.SetParent(parent);
            return dialog;
        }

        public void FixedSize(float width, float height)
        {
            var fixedSize = new Vector2(width, height);
            if (minSize == fixedSize && maxSize == fixedSize && size == fixedSize)
                return;

            minSize = fixedSize;
            maxSize = fixedSize;
            size = fixedSize;

            Repaint();
        }

        public void SetParent(VirtualDialog parent)
        {
            if (this.parent != null)
            {
                this.parent.child = null;
                this.parent = null;
                this.container = null;
            }

            if (parent != null)
            {
                this.parent = parent;
                this.container = parent.container;
                parent.child = this;
            }

            Repaint();
        }

        public void CenterToParent()
        {
            if (parent != null)
            {
                center = parent.center;
            }
        }

        public void CenterToTarget(EditorWindow target)
        {
            if (target)
            {
                var trc = target.position;
                center = trc.center;
            }
        }

        public void RightToParent()
        {
            if (parent != null)
            {
                center = parent.center + new Vector2((parent.width - width) / 2, 0);
            }
        }

        public void RightToTarget(EditorWindow target)
        {
            if (target)
            {
                var trc = target.position;
                center = trc.center + new Vector2((trc.width - width) / 2, 0);
            }
        }

        public void Close()
        {
            visible = false;
            child?.Close();
            SetParent(null);
            DestroyImmediate(this, true);
        }

        public void Repaint()
        {
            container?.Repaint();
        }

        public void SendEvent(Event e)
        {
            container?.SendEvent(e);
        }

        public void Draw()
        {
            if (!visible)
            {
                Repaint();
                Close();
                return;
            }

            if (top && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                if (CheckCloseWarning())
                    return;
                Repaint();
                Close();
                return;
            }

            //EditorGUIUtility.AddCursorRect(rect, MouseCursor.Arrow);

            using (new EditorGUI.DisabledScope(!top))
            {
                GUI.color = Color.white;

                DrawBlackground();

                DrawTitle();

                DrawGUI();
            }

            child?.Draw();

            Action temp = delayCall;
            delayCall = null;
            temp?.Invoke();
        }

        private void DrawBlackground()
        {
            Rect rc = rect;
            EditorGUI.DrawRect(rc, new Color(0.15f, 0.15f, 0.15f));
            rc.x += 1;
            rc.width -= 2;
            if (titlebar)
            {
                rc.y += TITLE_HEIGHT;
                rc.height -= TITLE_HEIGHT + 1;
            }
            else
            {
                rc.y += 1;
                rc.height -= 2;
            }

            EditorGUI.DrawRect(rc, new Color(0.25f, 0.25f, 0.25f));
        }

        private void DrawTitle()
        {
            if (!titlebar) return;

            var style = EditorStylesHelper.GetLabelStyle(TextAnchor.MiddleLeft);
            Rect rc = rect;
            rc.x += 1;
            rc.y += 1;
            rc.width = Mathf.Max(60, style.CalcSize(titleContent).x + 10);
            rc.height = TITLE_HEIGHT;
            EditorGUI.DrawRect(rc, new Color(0.25f, 0.25f, 0.25f));
            rc.x += 4;
            rc.width -= 8;
            GUI.Label(rc, titleContent, style);

            if (dragable)
            {
                rc.x -= 4;
                rc.width += 8;
                rc.x += rc.width;
                rc.width = rect.width - rc.width;
                if (MouseEventHelper.TryGetMouseDrag(titleContent.text, rc, out _, out var delta))
                {
                    position += delta;
                }
            }
        }

        private void DrawGUI()
        {
            GUILayout.BeginArea(clientRect);
            this.OnGUI();
            GUILayout.EndArea();
        }

        protected virtual void OnEnable()
        {
        }

        protected virtual void OnDisable()
        {
        }

        protected virtual void OnDestroy()
        {
        }

        protected virtual void OnGUI()
        {
        }

        protected virtual void OnSave()
        {
        }

        protected bool CheckCloseWarning()
        {
            if (visible && modified)
            {
                int ret = EditorUtility.DisplayDialogComplex($"{titleContent.text} - Unsaved Changes Detected",
                        "数据已发生变动", "Save", "Discard", "Cancel");
                if (ret == 0)
                {
                    OnSave();
                }
                else if (ret == 2)
                {
                    return true;
                }
            }

            return false;
        }
    }
}