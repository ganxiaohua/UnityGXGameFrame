using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace GameFrame
{
    public class DependentUIResources : Entity, IStart, IClear
    {
        public int Cur;
        public int All;
        public List<string> AssetPaths;
        public UniTaskCompletionSource Task;
    }
}