   using GameFrame;
   using UnityEngine;
   public static class AutoPos
   {
        
         public static void AddPos(this ECSEntity ecsEntity)
         {
              ecsEntity.AddComponent<Pos>();
         }
         
         public static void AddPos(this ECSEntity ecsEntity,Vector2 param)
         {
             var p  =  ecsEntity.AddComponent<Pos>();
             p.vec = param;
         }
         
         public static Pos GetPos(this ECSEntity ecsEntity)
         {
              return ecsEntity.GetComponent<Pos>();
         }
         
         public static ECSEntity SetPos(this ECSEntity ecsEntity,Vector2 param)
         {
              var p = ecsEntity.GetComponent<Pos>();
              p.vec = param;
              ViewBindEventClass.PosEntityComponentNumericalChange?.Invoke(p,ecsEntity);
              return ecsEntity;
         }
              
   }