
using Cysharp.Threading.Tasks;
using GameFrame;
namespace GameFrame
{
    public class AssetInitComponent : Entity, IStart,IClear
    {
        public CheckUpdate CheckUpdate;
        public UniTaskCompletionSource Task;
    }
}
