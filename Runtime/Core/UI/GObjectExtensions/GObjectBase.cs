using System;
using Cysharp.Threading.Tasks;
using FairyGUI;

namespace GameFrame.Runtime
{
    public class GObjectBaseData
    {
        public string Package;
        public string Name;
        public bool FromResources;

        public GObjectBaseData(string package, string name, bool forResources)
        {
            Package = package;
            Name = name;
            FromResources = forResources;
        }
    }

    public class GObjectBase : ObjectBase
    {
        protected GObjectBaseData gObjectBaseData;
        protected GObject Obj;
        protected object spawnData;

        /// <summary>
        /// 初始化对象基类。
        /// </summary>
        public override void Initialize(object initData)
        {
            base.Initialize(initData);
            gObjectBaseData = (GObjectBaseData) initData;
        }


        /// <summary>
        /// 获取对象时的事件。
        /// </summary>
        public override void OnSpawn(object obj)
        {
            spawnData = obj;
            base.OnSpawn(obj);
            LoadObject().Forget();
        }


        /// <summary>
        /// 回收对象时的事件。
        /// </summary>
        public override void OnUnspawn()
        {
            Obj.visible = false;
            base.OnUnspawn();
        }

        public override void Dispose()
        {
            Obj?.Dispose();
            Obj = null;
            base.Dispose();
        }

        private async UniTask LoadObject()
        {
            var ver = Versions;
            Obj ??= await UISystem.Instance.CreateGObjectAsync(gObjectBaseData.Package, gObjectBaseData.Name, gObjectBaseData.FromResources);
            if (Obj != null && ver != Versions)
            {
                Obj.Dispose();
                Obj = null;
                return;
            }

            Obj.visible = true;
            OnLoadOver();
        }

        public virtual void OnLoadOver()
        {
        }
    }
}