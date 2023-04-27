
namespace RoslynCSharp
{
    public interface IScriptDataProxy
    {
        // Properties
        object this[string name] { get; set; }

        // Methods
        void SetValue(string name, object value);

        object GetValue(string name);
    }
}
