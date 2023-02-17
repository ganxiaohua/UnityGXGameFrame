namespace GameFrame
{
    public class Destroy : Entity
    {
        public bool Value;
    }

    public static class DestroyOperation
    {
        public static Destroy AddDestroy(this ECSEntity ecsEntity)
        {
            Destroy p = ecsEntity.AddComponent<Destroy>();
            p.Value = false;
            return p;
        }

        public static Destroy GetDestroy(this ECSEntity ecsEntity)
        {
            Destroy p = ecsEntity.GetComponent<Destroy>();
            if (p == null)
            {
                return null;
            }

            return p;
        }

        public static Destroy SetDestroy(this ECSEntity ecsEntity, bool destroy)
        {
            Destroy p = ecsEntity.GetComponent<Destroy>();
            if (p == null)
            {
                return null;
            }

            p.Value = destroy;
            return p;
        }
    }
}