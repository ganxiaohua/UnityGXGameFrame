   using GameFrame;
   using UnityEngine;
   public static class AutoRotate
   {
        
         public static void AddRotate(this ECSEntity ecsEntity)
         {
              ecsEntity.AddComponent<Rotate>();
         }
         
         public static void AddRotate(this ECSEntity ecsEntity,Vector2 param)
         {
             var p  =  ecsEntity.AddComponent<Rotate>();
             p.vec = param;
         }
         
         public static Rotate GetRotate(this ECSEntity ecsEntity)
         {
              return ecsEntity.GetComponent<Rotate>();
         }
         
         public static ECSEntity SetRotate(this ECSEntity ecsEntity,Vector2 param)
         {
              var p = ecsEntity.GetComponent<Rotate>();
              p.vec = param;
              ViewBindEventClass.RotateEntityComponentNumericalChange?.Invoke(p,ecsEntity);
              return ecsEntity;
         }
              
   }