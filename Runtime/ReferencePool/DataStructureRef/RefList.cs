﻿using System;
using System.Collections.Generic;

namespace GameFrame.Runtime.DataStructureRef
{
    public class RefList<T> : IDisposable
    {
        public List<T> List = new List<T>(64);
        public void Dispose()
        {
            List.Clear();
        }
    }
}