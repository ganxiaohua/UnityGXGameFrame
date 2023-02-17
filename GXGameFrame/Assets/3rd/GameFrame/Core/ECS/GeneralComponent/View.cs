namespace GameFrame
{
    public class View : Entity
    {
        public IView Value;
    }

    public static class ViewOperation
    {
        public static View AddView(this ECSEntity ecsEntity, IView view)
        {
            View p = ecsEntity.AddComponent<View>();
            p.Value = view;
            return p;
        }

        public static View AddView(this ECSEntity ecsEntit)
        {
            View p = ecsEntit.AddComponent<View>();
            return p;
        }

        public static View SetView(this ECSEntity ecsEntit, IView view)
        {
            View p = ecsEntit.GetView();
            p.Value = view;
            return p;
        }

        public static View GetView(this ECSEntity ecsEntity)
        {
            View p = ecsEntity.GetComponent<View>();
            if (p == null)
            {
                return null;
            }
            return p;
        }
    }
}