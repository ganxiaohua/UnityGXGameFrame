using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

public class ChainOperationTests
{
    [Test]
    public void ChainOperationWithTypedDependency_DoesNotReturnInvalidDependencyHandles()
    {
        //Setup
        ChainOperation<object, object> chainOp = new ChainOperation<object, object>();
        AsyncOperationHandle<object> chainOpHandle = new AsyncOperationHandle<object>(new ProviderOperation<object>());
        chainOp.Init(chainOpHandle, null, false);
        
        //Test
        List<AsyncOperationHandle> dependencies = new List<AsyncOperationHandle>();
        AsyncOperationHandle handle = new AsyncOperationHandle(chainOp);
        chainOpHandle.m_InternalOp.m_Version = 1;
        handle.GetDependencies(dependencies);

        //Assert
        Assert.AreEqual(0, dependencies.Count);
    }

    [Test]
    public void ChainOperationWithTypelessDependency_DoesNotReturnInvalidDependencyHandles()
    {
        //Setup
        ChainOperationTypelessDepedency<object> chainOp = new ChainOperationTypelessDepedency<object>();
        AsyncOperationHandle<object> chainOpHandle = new AsyncOperationHandle<object>(new ProviderOperation<object>());
        chainOp.Init(chainOpHandle, null, false);

        //Test
        List<AsyncOperationHandle> dependencies = new List<AsyncOperationHandle>();
        AsyncOperationHandle handle = new AsyncOperationHandle(chainOp);
        chainOpHandle.m_InternalOp.m_Version = 1;
        handle.GetDependencies(dependencies);

        //Assert
        Assert.AreEqual(0, dependencies.Count);
    }
}
