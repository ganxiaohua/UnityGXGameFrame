using UnityEngine;

namespace RoslynCSharp.Modding
{
    public interface IModScriptReplacedReceiver
    {
        // Methods
        void OnWillReplaceScript(MonoBehaviour replacementBehaviour);
    }
}
