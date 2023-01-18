using System.Collections.Generic;
using NUnit.Framework;
#if UNITY_EDITOR
using UnityEditor.AddressableAssets.Settings;
#endif
using UnityEngine;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.Diagnostics;
using UnityEngine.TestTools;

namespace DiagnosticEventCollectorIntegrationTests
{
    [UnityPlatform(exclude = new[] { RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor })]
    abstract class DiagnosticEventCollectorIntegrationTests : AddressablesTestFixture
    {
        protected abstract bool PostProfilerEvents();

#if UNITY_EDITOR
        protected override void RunBuilder(AddressableAssetSettings settings)
        {
            bool oldState = ProjectConfigData.PostProfilerEvents;
            ProjectConfigData.PostProfilerEvents = PostProfilerEvents();
            base.RunBuilder(settings);
            ProjectConfigData.PostProfilerEvents = oldState;
        }

#endif

        protected override TestBuildScriptMode BuildScriptMode => TestBuildScriptMode.Packed;
    }

    class DiagnosticEventCollectorTests : DiagnosticEventCollectorIntegrationTests
    {
        protected override bool PostProfilerEvents()
        {
            return true;
        }

        private void DummyHandler(DiagnosticEvent evt)
        {
        }

        private void DummyHandler2(DiagnosticEvent evt)
        {
        }

        private void TrackingHandler(DiagnosticEvent evt)
        {
            Debug.Log(evt.Frame);
        }

        private DiagnosticEvent createGenericDiagnosticEvent(int id, int stream, int frame)
        {
            return new DiagnosticEvent("N/A", "DummyEvent" + id, id, stream, frame, 1, new int[]{});
        }

        int opCreate = (int)ResourceManager.DiagnosticEventType.AsyncOperationCreate;
        int opRefcount = (int)ResourceManager.DiagnosticEventType.AsyncOperationReferenceCount;


        [Test]
        public void RegisterEventHandler_ReturnsTrueOnNonexistentSingletonAndCreateEqualsTrue()
        {
            //Prepare
            DiagnosticEventCollectorSingleton.DestroySingleton();


            //Act/Assert
            Assert.AreEqual(false, DiagnosticEventCollectorSingleton.Exists, "Singleton exists when it should not have been initialized.");
            Assert.AreEqual(true, DiagnosticEventCollectorSingleton.RegisterEventHandler(DummyHandler, true, true), "RegisterEventHandler returning false, not registering handler when registration was expected. ");

            //Cleanup
            DiagnosticEventCollectorSingleton.DestroySingleton();
        }

        [Test]
        public void RegisterEventHandler_ReturnsFalseOnNonexistentSingletonAndCreateEqualsFalse()
        {
            //Prepare
            DiagnosticEventCollectorSingleton.DestroySingleton();

            //Act/Assert
            Assert.AreEqual(false, DiagnosticEventCollectorSingleton.Exists, "Singleton exists when it should not have been initialized.");
            Assert.AreEqual(false, DiagnosticEventCollectorSingleton.RegisterEventHandler(DummyHandler, true, false), "RegisterEventHandler returning true when Exists and create should both be false. ");

            //Cleanup
            DiagnosticEventCollectorSingleton.DestroySingleton();
        }

        [Test]
        public void RegisterEventHandler_ReturnsFalseOnUnregisterOnNonexistentHandler()
        {
            //Prepare
            //Create a brand new singleton, ensure it initializes
            DiagnosticEventCollectorSingleton.DestroySingleton();

            var handlerCount = DiagnosticEventCollectorSingleton.Instance.s_EventHandlers.Count;

            //Act/Assert
            Assert.AreEqual(false, DiagnosticEventCollectorSingleton.RegisterEventHandler(DummyHandler, false, false), "Event handler returned registered a handler despite register being false.");
            Assert.AreEqual(handlerCount, DiagnosticEventCollectorSingleton.Instance.s_EventHandlers.Count, "Event handler was registered when no registration should've occurred. ");

            //cleanup
            DiagnosticEventCollectorSingleton.DestroySingleton();
        }

        [Test]
        public void RegisterEventHandler_ProperlyRegistersHandlerOnRegisterCreate()
        {
            //Prepare
            //Create a brand new singleton, ensure it initializes
            DiagnosticEventCollectorSingleton.DestroySingleton();

            var handlerCount = DiagnosticEventCollectorSingleton.Instance.s_EventHandlers.Count;

            //Act/Assert
            Assert.AreEqual(true, DiagnosticEventCollectorSingleton.RegisterEventHandler(DummyHandler, true, true), "Handler was not registered despite create and exist both being true.");
            Assert.AreEqual(handlerCount + 1, DiagnosticEventCollectorSingleton.Instance.s_EventHandlers.Count, "Event Handler was not properly assigned in s_EventHandlers.");

            //cleanup
            DiagnosticEventCollectorSingleton.DestroySingleton();
        }

        [Test]
        public void RegisterEventHandler_ProperlyRegistersHandlerOnRegisterTrue()
        {
            //Prepare
            //Create a brand new singleton, ensure it initializes
            DiagnosticEventCollectorSingleton.DestroySingleton();

            var handlerCount = DiagnosticEventCollectorSingleton.Instance.s_EventHandlers.Count;
            //Act/Assert

            Assert.AreEqual(true, DiagnosticEventCollectorSingleton.RegisterEventHandler(DummyHandler, true, false), "Handler was not registered despite register being true and the Singleton being intitalized.");
            Assert.AreEqual(handlerCount + 1, DiagnosticEventCollectorSingleton.Instance.s_EventHandlers.Count, "Event Handler was not properly assigned in s_EventHandlers.");

            //cleanup
            DiagnosticEventCollectorSingleton.DestroySingleton();
        }

        [Test]
        public void RegisterEventHandler_TwoRegisterEmptyRemoval()
        {
            //Prepare
            //Create a brand new singleton, ensure it initializes and has an empty list of eventhandlers
            DiagnosticEventCollectorSingleton.DestroySingleton();

            var handlerCount = DiagnosticEventCollectorSingleton.Instance.s_EventHandlers.Count;

            //Act/Assert
            Assert.AreEqual(true, DiagnosticEventCollectorSingleton.RegisterEventHandler(DummyHandler, true, false), "Handler was not registered despite Exists being true.");
            Assert.AreEqual(handlerCount + 1, DiagnosticEventCollectorSingleton.Instance.s_EventHandlers.Count, "DummyHandler was not properly assigned in s_EventHandlers.");
            Assert.AreEqual(true, DiagnosticEventCollectorSingleton.RegisterEventHandler(DummyHandler2, true, false), "Handler was not registered despite Exists being true");
            Assert.AreEqual(handlerCount + 2, DiagnosticEventCollectorSingleton.Instance.s_EventHandlers.Count, "DummyHandler2 was not properly assigned in s_EventHandlers.");

            //Remove each delegate
            Assert.AreEqual(false, DiagnosticEventCollectorSingleton.RegisterEventHandler(DummyHandler, false, false), "DummyHandler was not unregistered despite register being false.");
            Assert.AreEqual(handlerCount + 1, DiagnosticEventCollectorSingleton.Instance.s_EventHandlers.Count, "DummyHandler was not properly removed from s_EventHandlers.");
            Assert.AreEqual(false, DiagnosticEventCollectorSingleton.RegisterEventHandler(DummyHandler, false, false), "Registration occurred despite register being false.");
            Assert.AreEqual(handlerCount + 1, DiagnosticEventCollectorSingleton.Instance.s_EventHandlers.Count, "DummyHandler2 was removed when no removal should've occurred.");

            //cleanup
            DiagnosticEventCollectorSingleton.DestroySingleton();
        }
        
        [Test]
        public void HandleUnhandledEvents_EventsHandledInProperOrder()
        {
            //Prepare
            //Create a brand new singleton, ensure it initializes and has an empty list of eventhandlers
            DiagnosticEventCollectorSingleton.DestroySingleton();

            DiagnosticEventCollectorSingleton.Instance.m_UnhandledEvents = new List<DiagnosticEvent>();
            DiagnosticEventCollectorSingleton.Instance.m_UnhandledEvents.Add(createGenericDiagnosticEvent(0, opCreate, 0));
            DiagnosticEventCollectorSingleton.Instance.m_UnhandledEvents.Add(createGenericDiagnosticEvent(0, opCreate, 1));
            
            DiagnosticEventCollectorSingleton.Instance.m_CreatedEvents = new Dictionary<int, DiagnosticEvent>();
            DiagnosticEventCollectorSingleton.Instance.m_CreatedEvents.Add(0, createGenericDiagnosticEvent(0, opCreate, 0));
            DiagnosticEventCollectorSingleton.Instance.m_CreatedEvents.Add(1, createGenericDiagnosticEvent(1, opCreate, 1));
            
            DiagnosticEventCollectorSingleton.Instance.RegisterEventHandler(TrackingHandler);
            
            //This is ensuring that the events are handled in the proper order
            LogAssert.Expect(UnityEngine.LogType.Log, "0");
            LogAssert.Expect(UnityEngine.LogType.Log, "0");
            LogAssert.Expect(UnityEngine.LogType.Log, "1");
            LogAssert.Expect(UnityEngine.LogType.Log, "1");
            
            Assert.AreEqual(0, DiagnosticEventCollectorSingleton.Instance.m_UnhandledEvents.Count, "Unhandled event queue should be completely emptied after HandleUnhandledEvents is called.");
            Assert.AreEqual(2, DiagnosticEventCollectorSingleton.Instance.m_CreatedEvents.Count, "Number of events in CreatedEvents should not be affected. ");
            //cleanup
            DiagnosticEventCollectorSingleton.DestroySingleton();
        }

        [Test]
        public void HandleUnhandledEvents_LargeNumberOfUnhandledEvents()
        {
            //Prepare
            //Create a brand new singleton, ensure it initializes and has an empty list of eventhandlers
            DiagnosticEventCollectorSingleton.DestroySingleton();

            DiagnosticEventCollectorSingleton.Instance.m_UnhandledEvents = new List<DiagnosticEvent>();
            DiagnosticEventCollectorSingleton.Instance.m_UnhandledEvents.Add(createGenericDiagnosticEvent(0, opCreate, 0));
            DiagnosticEventCollectorSingleton.Instance.m_UnhandledEvents.Add(createGenericDiagnosticEvent(1, opCreate, 1));
            DiagnosticEventCollectorSingleton.Instance.m_UnhandledEvents.Add(createGenericDiagnosticEvent(1, opRefcount, 2));
            DiagnosticEventCollectorSingleton.Instance.m_UnhandledEvents.Add(createGenericDiagnosticEvent(1, opRefcount, 3));
            DiagnosticEventCollectorSingleton.Instance.m_UnhandledEvents.Add(createGenericDiagnosticEvent(1, opRefcount, 4));
            DiagnosticEventCollectorSingleton.Instance.m_UnhandledEvents.Add(createGenericDiagnosticEvent(1, opRefcount, 5));
            
            DiagnosticEventCollectorSingleton.Instance.m_CreatedEvents = new Dictionary<int, DiagnosticEvent>();
            DiagnosticEventCollectorSingleton.Instance.m_CreatedEvents.Add(0, createGenericDiagnosticEvent(0, opCreate, 0));
            DiagnosticEventCollectorSingleton.Instance.m_CreatedEvents.Add(1, createGenericDiagnosticEvent(1, opCreate, 1));

            DiagnosticEventCollectorSingleton.Instance.RegisterEventHandler(TrackingHandler);
            
            //This is ensuring that the events are handled in the proper order
            LogAssert.Expect(UnityEngine.LogType.Log, "0");
            LogAssert.Expect(UnityEngine.LogType.Log, "0");
            LogAssert.Expect(UnityEngine.LogType.Log, "1");
            LogAssert.Expect(UnityEngine.LogType.Log, "1");
            LogAssert.Expect(UnityEngine.LogType.Log, "2");
            LogAssert.Expect(UnityEngine.LogType.Log, "3");
            LogAssert.Expect(UnityEngine.LogType.Log, "4");
            LogAssert.Expect(UnityEngine.LogType.Log, "5");
            
            Assert.AreEqual(0, DiagnosticEventCollectorSingleton.Instance.m_UnhandledEvents.Count, "Unhandled event queue should be completely emptied after HandleUnhandledEvents is called.");
            Assert.AreEqual(2, DiagnosticEventCollectorSingleton.Instance.m_CreatedEvents.Count, "Number of events in CreatedEvents should not be affected. ");
            //cleanup
            DiagnosticEventCollectorSingleton.DestroySingleton();
        }
        
        [Test]
        public void HandleUnhandledEvents_UnaffectedByIncorrectEventOrder()
        {
            //Prepare
            //Create a brand new singleton, ensure it initializes and has an empty list of eventhandlers
            DiagnosticEventCollectorSingleton.DestroySingleton();

            DiagnosticEventCollectorSingleton.Instance.m_UnhandledEvents = new List<DiagnosticEvent>();
            DiagnosticEventCollectorSingleton.Instance.m_UnhandledEvents.Add(createGenericDiagnosticEvent(1, opRefcount, 2));
            DiagnosticEventCollectorSingleton.Instance.m_UnhandledEvents.Add(createGenericDiagnosticEvent(1, opRefcount, 4));
            DiagnosticEventCollectorSingleton.Instance.m_UnhandledEvents.Add(createGenericDiagnosticEvent(1, opCreate, 1));
            DiagnosticEventCollectorSingleton.Instance.m_UnhandledEvents.Add(createGenericDiagnosticEvent(1, opRefcount, 5));
            DiagnosticEventCollectorSingleton.Instance.m_UnhandledEvents.Add(createGenericDiagnosticEvent(1, opRefcount, 3));
            DiagnosticEventCollectorSingleton.Instance.m_UnhandledEvents.Add(createGenericDiagnosticEvent(0, opCreate, 0));
            
            DiagnosticEventCollectorSingleton.Instance.m_CreatedEvents = new Dictionary<int, DiagnosticEvent>();
            DiagnosticEventCollectorSingleton.Instance.m_CreatedEvents.Add(1, createGenericDiagnosticEvent(1, opCreate, 1));
            DiagnosticEventCollectorSingleton.Instance.m_CreatedEvents.Add(0, createGenericDiagnosticEvent(0, opCreate, 0));
            
            
            DiagnosticEventCollectorSingleton.Instance.RegisterEventHandler(TrackingHandler);
            
            //This is ensuring that the events are handled in the proper order
            LogAssert.Expect(UnityEngine.LogType.Log, "0");
            LogAssert.Expect(UnityEngine.LogType.Log, "0");
            LogAssert.Expect(UnityEngine.LogType.Log, "1");
            LogAssert.Expect(UnityEngine.LogType.Log, "1");
            LogAssert.Expect(UnityEngine.LogType.Log, "2");
            LogAssert.Expect(UnityEngine.LogType.Log, "3");
            LogAssert.Expect(UnityEngine.LogType.Log, "4");
            LogAssert.Expect(UnityEngine.LogType.Log, "5");
            
            Assert.AreEqual(0, DiagnosticEventCollectorSingleton.Instance.m_UnhandledEvents.Count, "Unhandled event queue should be completely emptied after HandleUnhandledEvents is called.");
            Assert.AreEqual(2, DiagnosticEventCollectorSingleton.Instance.m_CreatedEvents.Count, "Number of events in CreatedEvents should not be affected. ");
            //cleanup
            DiagnosticEventCollectorSingleton.DestroySingleton();
        }
    }

    class DiagnosticEventCollectorIntegrationTestsPostProfilerEventsIsTrue : DiagnosticEventCollectorIntegrationTests
    {
        protected override bool PostProfilerEvents() => true;

        [Test]
        public void WhenPostProfilerEventsIsTrue_DiagnosticEventsCollectorIsCreated()
        {
            Assert.AreEqual(1, Resources.FindObjectsOfTypeAll(typeof(DiagnosticEventCollectorSingleton)).Length);
        }
    }

    class DiagnosticEventCollectorIntegrationTestsPostProfilerEventsIsFalse : DiagnosticEventCollectorIntegrationTests
    {
        protected override bool PostProfilerEvents() => false;

        [Test]
        public void WhenPostProfilerEventsIsFalse_DiagnosticEventsCollectorIsNotCreated()
        {
            Assert.AreEqual(0, Resources.FindObjectsOfTypeAll(typeof(DiagnosticEventCollectorSingleton)).Length);
        }
    }
}
