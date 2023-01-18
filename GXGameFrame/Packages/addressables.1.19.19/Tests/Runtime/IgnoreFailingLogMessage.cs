using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

public class IgnoreFailingLogMessage : IDisposable
{
    private bool m_SavedIgnoreFailingMessagesFlag;
    public IgnoreFailingLogMessage()
    {
        m_SavedIgnoreFailingMessagesFlag = LogAssert.ignoreFailingMessages;
        LogAssert.ignoreFailingMessages = true;
    }
    public void Dispose()
    {
        LogAssert.ignoreFailingMessages = m_SavedIgnoreFailingMessagesFlag;
    }
}
