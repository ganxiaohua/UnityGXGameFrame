   using GameFrame;
   using UnityEngine;
   public static class AutoView
   {
        
         public static void AddView(this ECSEntity ecsEntity)
         {
              ecsEntity.AddComponent<View>();
         }
         
         public static void AddView(this ECSEntity ecsEntity,IEceView param)
         {
             var p  =  ecsEntity.AddComponent<View>();
             p.Value = param;
         }
         
         public static View GetView(this ECSEntity ecsEntity)
         {
              return ecsEntity.GetComponent<View>();
         }
         
         public static ECSEntity SetView(this ECSEntity ecsEntity,IEceView param)
         {
              var p = ecsEntity.GetComponent<View>();
              p.Value = param;
              
              return ecsEntity;
         }
              
   }