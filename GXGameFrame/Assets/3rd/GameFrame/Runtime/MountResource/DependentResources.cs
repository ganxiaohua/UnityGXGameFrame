using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace GameFrame
{
    public class DependentResources : Entity, IStart, IClear
    {
        public string AssetPath;
        public Object Asset;
        public UniTaskCompletionSource<bool> Task;
    }
}