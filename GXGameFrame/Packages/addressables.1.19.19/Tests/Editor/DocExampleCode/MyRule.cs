
namespace AddressableAssets.DocExampleCode
{
    //Prevent Unity from actually registering the rule in this example
    using InitializeOnLoadAttribute = Dummy; 

#if UNITY_EDITOR
    #region doc_CustomRule
    using UnityEditor;
    using UnityEditor.AddressableAssets.Build;
    using UnityEditor.AddressableAssets.Build.AnalyzeRules;

    class MyRule : AnalyzeRule 
    { 
        // Rule code...
    }
    
    // Register rule
    [InitializeOnLoad]
    class RegisterMyRule
    {
        static RegisterMyRule() {
            AnalyzeSystem.RegisterNewRule<MyRule>();
        }
    }
    #endregion

#endif
    internal class Dummy : System.Attribute { 
        public Dummy() { }
        public Dummy(string parameter) { }
    }
}
