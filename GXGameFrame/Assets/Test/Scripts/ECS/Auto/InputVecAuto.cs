   using GameFrame;
   using UnityEngine;
   public static class AutoInputVec
   {
        
         public static void AddInputVec(this ECSEntity ecsEntity)
         {
              ecsEntity.AddComponent<InputVec>();
         }
         
         public static void AddInputVec(this ECSEntity ecsEntity,Vector2 param)
         {
             var p  =  ecsEntity.AddComponent<InputVec>();
             p.vec = param;
         }
         
         public static InputVec GetInputVec(this ECSEntity ecsEntity)
         {
              return ecsEntity.GetComponent<InputVec>();
         }
         
         public static ECSEntity SetInputVec(this ECSEntity ecsEntity,Vector2 param)
         {
              var p = ecsEntity.GetComponent<InputVec>();
              p.vec = param;
              
              return ecsEntity;
         }
              
   }