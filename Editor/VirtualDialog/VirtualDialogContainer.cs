using System;
using UnityEditor;
using UnityEngine;

namespace GameFrame.Editor
{
    public class VirtualDialogContainer : EditorWindow
    {
        public static VirtualDialogContainer temp { get; private set; }

        public static VirtualDialogContainer Open<T>(Action<T> awake) where T : VirtualDialog, new()
        {
            return Open(awake, false);
        }

        public static VirtualDialogContainer Open<T>(Action<T> awake, bool asDropdown) where T : VirtualDialog, new()
        {
            VirtualDialogContainer container = null;
            string ID = typeof(T).FullName;
            UnityEngine.Object[] array = Resources.FindObjectsOfTypeAll(typeof(VirtualDialogContainer));
            foreach (var obj in array)
            {
                var c = (VirtualDialogContainer) obj;
                if (c.ID == ID)
                {
                    container = c;
                    break;
                }
            }

            if (!container)
            {
                container = CreateInstance(typeof(VirtualDialogContainer)) as VirtualDialogContainer;
                container.ID = ID;
            }

            container.modified = false;
            container.dialog?.Close();
            try
            {
                temp = container;
                container.dialog = VirtualDialog.CreateDialog<T>(null);
            }
            finally
            {
                temp = null;
            }

            var dialog = container.dialog;
            dialog.container = container;
            awake.Invoke((T) dialog);
            if (dialog.minSize.sqrMagnitude > 1)
                container.minSize = dialog.minSize;
            if (dialog.maxSize.sqrMagnitude > 1)
                container.maxSize = dialog.maxSize;
            if (dialog.position.sqrMagnitude > 1 && dialog.size.sqrMagnitude > 1)
            {
                container.position = new Rect(dialog.position, dialog.size);
                container.SaveLocation();
            }
            else if (container.IsFirstOpen())
            {
                container.CenterToTarget(SceneView.lastActiveSceneView);
            }

            container.asDropdown = asDropdown;
            if (asDropdown)
            {
                container.ShowAsDropDown(container.position, container.position.size);
            }
            else
            {
                container.Show();
                container.Focus();
            }

            container.LoadLocation();
            container.saveChangesMessage = $"数据已发生变动";
            return container;
        }

        [SerializeField] private string _ID = "Unknown";

        public string ID
        {
            get => _ID;
            private set => _ID = value;
        }

        [SerializeField] private VirtualDialog _dialog = null;

        public VirtualDialog dialog
        {
            get => _dialog;
            private set => _dialog = value;
        }

        public bool modified
        {
            get => hasUnsavedChanges;
            set => hasUnsavedChanges = value;
        }

        private Rect prevPosition = Rect.zero;

        public bool asDropdown { get; private set; }

        public void CenterToTarget(EditorWindow target)
        {
            if (target)
            {
                var rc = position;
                var trc = target.position;
                rc.center = trc.center;
                position = rc;
                SaveLocation();
            }
        }

        public void RightToTarget(EditorWindow target)
        {
            if (target)
            {
                var rc = position;
                var trc = target.position;
                rc.center = trc.center;
                rc.x += (trc.width - rc.width) / 2;
                position = rc;
                SaveLocation();
            }
        }

        private void OnGUI()
        {
            if (Event.current.type == EventType.Layout && IsLocationDirty())
            {
                SaveLocation();
            }

            if (dialog == null)
            {
                Close();
                return;
            }

            titleContent = dialog.titleContent;
            if (dialog.minSize.sqrMagnitude > 1)
                minSize = dialog.minSize;
            if (dialog.maxSize.sqrMagnitude > 1)
                maxSize = dialog.maxSize;

            dialog.titlebar = false;
            dialog.dragable = false;
            dialog.position = Vector2.zero;
            dialog.size = position.size;
            dialog.Draw();
        }

        private void OnDestroy()
        {
            dialog?.Close();
        }

        private void LoadLocation()
        {
            position = new Rect(
                    EditorPrefs.GetFloat($"VDC_{ID}_X", position.x),
                    EditorPrefs.GetFloat($"VDC_{ID}_Y", position.y),
                    EditorPrefs.GetFloat($"VDC_{ID}_W", position.width),
                    EditorPrefs.GetFloat($"VDC_{ID}_H", position.height)
            );
            prevPosition = position;
        }

        private void SaveLocation()
        {
            EditorPrefs.SetFloat($"VDC_{ID}_X", position.x);
            EditorPrefs.SetFloat($"VDC_{ID}_Y", position.y);
            EditorPrefs.SetFloat($"VDC_{ID}_W", position.width);
            EditorPrefs.SetFloat($"VDC_{ID}_H", position.height);
            prevPosition = position;
        }

        private bool IsLocationDirty()
        {
            var curPosition = position;
            return !Mathf.Approximately(curPosition.x, prevPosition.x) || !Mathf.Approximately(curPosition.y, prevPosition.y) ||
                   !Mathf.Approximately(curPosition.width, prevPosition.width) || !Mathf.Approximately(curPosition.height, prevPosition.height);
        }

        private bool IsFirstOpen()
        {
            return !EditorPrefs.HasKey($"VDC_{ID}_X");
        }

        public override void SaveChanges()
        {
            base.SaveChanges();

            var onSave = dialog.GetType().GetMethod($"OnSave", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            onSave.Invoke(dialog, null);
        }
    }
}