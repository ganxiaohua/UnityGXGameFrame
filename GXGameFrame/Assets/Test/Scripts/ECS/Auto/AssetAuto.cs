   using GameFrame;
   using UnityEngine;
   public static class AutoAsset
   {
        
         public static void AddAsset(this ECSEntity ecsEntity)
         {
              ecsEntity.AddComponent<Asset>();
         }
         
         public static void AddAsset(this ECSEntity ecsEntity,string param)
         {
             var p  =  ecsEntity.AddComponent<Asset>();
             p.Path = param;
         }
         
         public static Asset GetAsset(this ECSEntity ecsEntity)
         {
              return ecsEntity.GetComponent<Asset>();
         }
         
         public static ECSEntity SetAsset(this ECSEntity ecsEntity,string param)
         {
              var p = ecsEntity.GetComponent<Asset>();
              p.Path = param;
              return ecsEntity;
         }
              
   }