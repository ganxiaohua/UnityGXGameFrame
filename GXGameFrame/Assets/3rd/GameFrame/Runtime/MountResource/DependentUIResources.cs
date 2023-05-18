using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace GameFrame
{
    public class DependentUIResources : Entity, IStart, IClear
    {
        public List<string> AssetPaths;
        public int CurLoadAmount;
        public UniTaskCompletionSource Task;
    }
}