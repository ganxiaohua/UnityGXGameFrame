using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FairyGUI;

namespace GameFrame
{
    public class DependentUIResources : Entity, IStart, IClear
    {
        public DefaultAssetReference DefaultAssetReference;
        public string PackageName;
        public GObject Window;
        public UniTaskCompletionSource waitLoadTask;
    }
}