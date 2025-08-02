using System;
using System.Reflection;

namespace GameFrame.Editor
{
    public static class GamePlayAssembly
    {
        
        private static Assembly gamePlayAssembly;
        
        
        public static Assembly GetAssembly()
        {
            if (gamePlayAssembly != null)
                return gamePlayAssembly;
            var assemblys = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblys)
            {
                if (assembly.GetType("AllComponents") != null)
                {
                    gamePlayAssembly = assembly;
                    return gamePlayAssembly;
                }
            }
            throw new Exception("Please generate a AllComponents script");
        }
    }
}