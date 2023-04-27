
namespace RoslynCSharp
{
    public interface IScriptEventProxy
    {
        // Properties
        ScriptEventHandler this[string name] { get; }

        // Methods
        ScriptEventHandler GetEvent(string name);
    }
}
