using System;
using System.Collections.Generic;

namespace GameFrame
{
    /// <summary>
    /// ECSEntity挂载的一定是Context
    /// </summary>
    public abstract class ECSEntity : Entity
    {
        protected override void ThisInit()
        {
            InitComponent();
        }


        public abstract void InitComponent();

        public T AddComponent<T>() where T : Entity
        {
            var component = base.AddComponent<T>();
            if (ComponentParent is Context context)
            {
                context.ChangeAddRomoveChildOrCompone(this);
            }
            else
            {
                throw new Exception("EscEntity ComponentParent must is Context");
            }

            return component;
        }

        public void RemoveComponent<T>() where T : Entity
        {
            base.RemoveComponent<T>();
            if (ComponentParent is Context context)
            {
                context.ChangeAddRomoveChildOrCompone(this);
            }
            else
            {
                throw new Exception("EscEntity ComponentParent must is Context");
            }
        }


        public override void Clear()
        {
            View view = this.GetView();
            if (view != null)
            {
                ReferencePool.Release(view.Value);
            }

            base.Clear();
        }
    }
}