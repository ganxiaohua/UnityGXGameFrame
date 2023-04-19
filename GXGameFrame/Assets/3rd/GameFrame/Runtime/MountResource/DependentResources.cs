using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameFrame
{
    public class DependentResources : Entity, IStart, IClear
    {
        public List<string> Path;
        public int CurLoadAmount;
        public bool LoadOver;
        public TaskCompletionSource<bool> Task;
    }
}