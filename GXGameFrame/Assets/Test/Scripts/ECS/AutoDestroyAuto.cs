   using GameFrame;
   using UnityEngine;
   public static class AutoDestroy
   {
        
         public static void AddDestroy(this ECSEntity ecsEntity)
         {
              ecsEntity.AddComponent<Destroy>();
         }
         
         public static Destroy GetDestroy(this ECSEntity ecsEntity)
         {
              return ecsEntity.GetComponent<Destroy>();
         }
              
   }