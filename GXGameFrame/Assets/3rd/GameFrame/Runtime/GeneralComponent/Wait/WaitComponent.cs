using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace GameFrame
{
    public class WaitComponent : Entity, IStart,IClear
    {
        public UniTaskCompletionSource UniTaskCompletionSource;
    }
}
