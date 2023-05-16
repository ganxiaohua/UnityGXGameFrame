using BansheeGz.BGDatabase;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace GameFrame
{
    public class GameObjectObjectBase : ObjectBase
    {
        private GameObject m_Obj;
        private Transform m_Trans;
        private UniTaskCompletionSource<bool> m_Task;
        private string m_LoadPath;
        private bool m_SetLocalPos;
        private bool m_SetLocalRot;
        public string LoadPath => m_LoadPath;

        public bool HasGameObject()
        {
            if (m_Obj != null)
            {
                return true;
            }

            return false;
        }


        public void SetObject(GameObject go)
        {
            if (go == null)
            {
                Debugger.LogError("GameoBject go is null");
                return;
            }
        }

        private Transform m_Parent;

        public Transform Parent
        {
            get { return m_Parent; }
            set
            {
                m_Parent = value;
                if (m_Trans != null)
                {
                    m_Trans.parent = m_Parent;
                }
            }
        }


        private Vector3 m_WorldPos = Vector3.zero;

        public Vector3 WorldPos
        {
            get { return m_WorldPos; }
            set
            {
                m_SetLocalPos = false;
                m_WorldPos = value;
                if (m_Trans != null)
                {
                    m_Trans.position = m_WorldPos;
                }
            }
        }

        private Quaternion m_WorldRot = Quaternion.identity;

        public Quaternion WorldRot
        {
            get { return m_WorldRot; }
            set
            {
                m_SetLocalRot = false;
                m_WorldRot = value;
                if (m_Trans != null)
                {
                    m_Trans.rotation = m_WorldRot;
                }
            }
        }

        private Vector3 m_LocalPos = Vector3.zero;

        public Vector3 LocalPos
        {
            get { return m_LocalPos; }
            set
            {
                m_SetLocalPos = true;
                m_LocalPos = LocalPos;
                if (m_Trans != null)
                {
                    m_Trans.localPosition = m_LocalPos;
                }
            }
        }


        private Quaternion m_LocalRot = Quaternion.identity;

        public Quaternion LocalRot
        {
            get { return m_LocalRot; }
            set
            {
                m_SetLocalRot = true;
                m_LocalRot = value;
                if (m_Trans != null)
                {
                    m_Trans.localRotation = m_LocalRot;
                }
            }
        }

        private Vector3 m_LocalScale = Vector3.one;

        public Vector3 LocalScale
        {
            get { return m_LocalScale; }
            set
            {
                m_LocalScale = value;
                if (m_Trans != null)
                {
                    m_Trans.localScale = m_LocalScale;
                }
            }
        }

        public Vector3 WorldScale
        {
            get
            {
                if (m_Trans != null)
                {
                    return m_Trans.lossyScale;
                }

                return Vector3.zero;
            }
        }

        private bool m_Action = true;

        public bool Action
        {
            get { return m_Action; }
            set
            {
                m_Action = value;
                if (m_Obj != null)
                {
                    m_Obj.SetActive(m_Action);
                }
            }
        }

        //
        internal override async void Initialize(object initObject)
        {
            m_SetLocalPos = false;
            m_SetLocalRot = false;
            m_Task = new UniTaskCompletionSource<bool>();
            base.Initialize(initObject);
            await Load(m_InitData as string);
            m_Task.TrySetResult(true);
        }

        private async UniTask Load(string path)
        {
            m_LoadPath = path;
            var go = await AssetManager.Instance.LoadAsyncTask<GameObject>(path);
            if (go == null)
            {
                return;
            }
            m_Obj = Object.Instantiate(go);
            Parent = m_Parent;
            m_Trans = m_Obj.transform;
            Action = m_Action;
            LocalScale = m_LocalScale;
            if (m_SetLocalPos)
            {
                LocalPos = m_LocalPos;
            }
            else
            {
                WorldPos = m_WorldPos;
            }

            if (m_SetLocalRot)
            {
                LocalRot = m_LocalRot;
            }
            else
            {
                WorldRot = m_WorldRot;
            }
        }

        /// <summary>
        /// 获取对象时的事件。
        /// </summary>
        internal override void OnSpawn()
        {
            Action = true;
        }

        /// <summary>
        /// 回收对象时的事件。
        /// </summary>
        internal override void OnUnspawn()
        {
            Action = false;
        }

        /// <summary>
        /// 清理对象基类。
        /// </summary>
        public override void Clear()
        {
            if (m_Task != null)
            {
                m_Task.TrySetCanceled();
                m_Task = null;
            }

            if (m_Obj != null)
            {
                Object.Destroy(m_Obj);
                m_Obj = null;
            }

            m_Trans = null;
            AssetManager.Instance.UnLoad(m_LoadPath);
        }

        public async void LoadOver()
        {
            await m_Task.Task;
            m_Task = null;
        }
    }
}