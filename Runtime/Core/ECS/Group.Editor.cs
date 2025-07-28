#if UNITY_EDITOR
namespace GameFrame.Runtime
{
    public partial class Group
    {
        private void EditorDisPose()
        {
            var data = CallerAnalysis.Analysis(6);
            if (!string.Equals("Group.cs", data.scriptName))
            {
                Debugger.LogError("Group不允许外部释放 " + data.scriptName);
            }
        }
    }
}
#endif