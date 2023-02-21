﻿namespace GameFrame
{
    public class Destroy : IECSComponent
    {
        public void Clear()
        {
        
        }
    }

    public static class DestroyOperation
    {
        public static Destroy AddDestroy(this ECSEntity ecsEntity)
        {
            Destroy p = ecsEntity.AddComponent<Destroy>();
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

        public static Destroy SetDestroy(this ECSEntity ecsEntity)
        {
            Destroy p = ecsEntity.GetComponent<Destroy>();
            if (p == null)
            {
                return null;
            }
            return p;
        }
    }
}