using System;
using System.Collections;
using System.Reflection;

namespace RoslynCSharp
{
    /// <summary>
    /// The method calling convention used when invoking a method.
    /// </summary>
    public enum ProxyCallConvention
    {
        /// <summary>
        /// Call the method as normal.
        /// </summary>
        StandardMethod,
        /// <summary>
        /// Call the method as a Unity coroutine. 
        /// The method should return an 'IEnumerator' to be invoked as a coroutine.
        /// The method will be invoked and managed by a game object and updated every frame.
        /// </summary>
        UnityCoroutine,
        /// <summary>
        /// Call the method based on its return type. 
        /// Methods that return 'IEnumerator' will be automatically invoked as a Unity coroutine.
        /// </summary>
        Any,
    }

    public abstract class ScriptProxy : IDisposable
    {
        // Private
        private IScriptDataProxy fields = null;
        private IScriptDataProxy safeFields = null;
        private IScriptDataProxy properties = null;
        private IScriptDataProxy safeProperties = null;
        private IScriptEventProxy events = null;
        private IScriptEventProxy safeEvents = null;

        // Properties
        public abstract ScriptAssembly Assembly { get; }

        public abstract ScriptType ScriptType { get; }

        /// <summary>
        /// Returns the <see cref="IScriptMemberProxy"/> that provides access to the fields of the wrapped instance. 
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The proxy has already been disposed</exception>
        public virtual IScriptDataProxy Fields
        {
            get
            {
                CheckDisposed();
                if (fields == null)
                    fields = new ScriptFieldDataProxy(ScriptType, this, false, true);

                return fields;
            }
        }

        /// <summary>
        /// Returns the <see cref="IScriptMemberProxy"/> that provides access to the fields of the wrapped instance. 
        /// Any exceptions thrown when locating or accessing the propery will be handled.
        /// </summary>
        public virtual IScriptDataProxy SafeFields
        {
            get
            {
                CheckDisposed();
                if (safeFields == null)
                    safeFields = new ScriptFieldDataProxy(ScriptType, this, false, false);

                return safeFields;
            }
        }

        /// <summary>
        /// Returns the <see cref="IScriptMemberProxy"/> that provides access to the properties of the wrapped instance. 
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The proxy has already been disposed</exception>
        public virtual IScriptDataProxy Properties
        {
            get
            {
                CheckDisposed();
                if (properties == null)
                    properties = new ScriptPropertyDataProxy(ScriptType, this, false, true);

                return properties;
            }
        }

        /// <summary>
        /// Returns the <see cref="IScriptMemberProxy"/> that provides access to the properties of the wrapped instance.
        /// Any exceptions thrown when locating or accessing the propery will be handled.
        /// </summary>
        public virtual IScriptDataProxy SafeProperties
        {
            get
            {
                CheckDisposed();
                if (safeProperties == null)
                    safeProperties = new ScriptPropertyDataProxy(ScriptType, this, false, false);

                return safeProperties;
            }
        }

        public virtual IScriptEventProxy Events
        {
            get
            {
                CheckDisposed();
                if (events == null)
                    events = new ScriptEventHandlerProxy(ScriptType, this, false, true);

                return events;
            }
        }

        public virtual IScriptEventProxy SafeEvents
        {
            get
            {
                CheckDisposed();
                if (safeEvents == null)
                    safeEvents = new ScriptEventHandlerProxy(ScriptType, this, false, true);

                return safeEvents;
            }
        }

        public abstract object Instance { get; }

        public virtual UnityEngine.Object UnityInstance
        {
            get
            {
                CheckDisposed();
                return GetInstanceAs<UnityEngine.Object>(false);
            }
        }

        public virtual UnityEngine.MonoBehaviour MonoBehaviourInstance
        {
            get
            {
                CheckDisposed();
                return GetInstanceAs<UnityEngine.MonoBehaviour>(false);
            }
        }

        public virtual UnityEngine.ScriptableObject ScriptableObjectInstance
        {
            get
            {
                CheckDisposed();
                return GetInstanceAs<UnityEngine.ScriptableObject>(false);
            }
        }

        public virtual bool IsUnityObject
        {
            get { return ScriptType.IsUnityObject; }
        }

        public virtual bool IsMonoBehaviour
        {
            get { return ScriptType.IsMonoBehaviour; }
        }

        public virtual bool IsScriptableObject
        {
            get { return ScriptType.IsScriptableObject; }
        }

        public abstract bool IsDisposed { get; }

        // Construction
        protected abstract void ConstructInstance(ScriptType type, object instance);

        // Methods
        /// <summary>
        /// Attempt to call a method on the managed instance with the specifie name.
        /// This works in a similar way as <see cref="UnityEngine.GameObject.SendMessage(string)"/> where the method name is specified.
        /// The target method must not accept any arguments.
        /// The target method will be called using <see cref="ProxyCallConvention.Any"/>
        /// </summary>
        /// <param name="methodName">The name of the method to call</param>
        /// <returns>The value returned from the target method or null if the target method does not return a value</returns>
        /// <returns>The value returned from the target method or null if the target method does not return a value</returns>
        /// <exception cref="ObjectDisposedException">The proxy has already been disposed</exception>
        /// <exception cref="TargetException">The target method could not be found on the managed type</exception>
        public object Call(string methodName)
        {
            return Call(methodName, ProxyCallConvention.Any);
        }

        /// <summary>
        /// Attempt to call a method on the managed instance with the specified name.
        /// This works in a similar way as <see cref="UnityEngine.GameObject.SendMessage(string)"/> where the method name is specified.
        /// The target method must not accept any arguments.
        /// </summary>
        /// <param name="methodName">The name of the method to call</param>
        /// <param name="callConvention">The method calling convention</param>
        /// <returns>The value returned from the target method or null if the target method does not return a value</returns>
        /// <exception cref="ObjectDisposedException">The proxy has already been disposed</exception>
        /// <exception cref="TargetException">The target method could not be found on the managed type</exception>
        public object Call(string methodName, ProxyCallConvention callConvention)
        {
            // Make sure the object has not already been disposed
            CheckDisposed();

            // Find the method
            MethodInfo method = ScriptType.FindCachedMethod(methodName, false);

            // Check for error
            if (method == null)
                throw new TargetException(string.Format("Type '{0}' does not define a method called '{1}'", ScriptType, methodName));

            // Call the method
            object result = method.Invoke(Instance, null);

            // Check for coroutine
            if ((result is IEnumerator) && (callConvention == ProxyCallConvention.Any || callConvention == ProxyCallConvention.UnityCoroutine) == true)
            {
                // Get the coroutine method
                IEnumerator routine = result as IEnumerator;

                // Check if the calling object is a mono behaviour
                if (IsMonoBehaviour == true)
                {
                    // Get the proxy as a mono behaviour
                    UnityEngine.MonoBehaviour mono = GetInstanceAs<UnityEngine.MonoBehaviour>(false);

                    // Register the coroutine with the behaviour so that it will be called after yield
                    mono.StartCoroutine(routine);
                }
            }

            // Get the return value
            return result;
        }

        /// <summary>
        /// Attempt to call a method on the managed instance with the specified name and arguments.
        /// This works in a similar was as <see cref="UnityEngine.GameObject.SendMessage(string)"/> where the method name is specified.
        /// Any number of arguments may be specified but the target method must expect the arguments.
        /// The target method will be called using <see cref="ProxyCallConvention.Any"/>
        /// </summary>
        /// <param name="methodName">The name of the method to call</param>
        /// <param name="arguments">The arguments passed to the method</param>
        /// <returns>The value returned from the target method or null if the target method does not return a value</returns>
        /// <exception cref="ObjectDisposedException">The proxy has already been disposed</exception>
        /// <exception cref="TargetException">The target method could not be found on the managed type</exception>
        public object Call(string methodName, params object[] arguments)
        {
            return Call(methodName, ProxyCallConvention.Any, arguments);
        }

        /// <summary>
        /// Attempt to call a method on the managed instance with the specified name and arguments.
        /// This works in a similar was as <see cref="UnityEngine.GameObject.SendMessage(string)"/> where the method name is specified.
        /// Any number of arguments may be specified but the target method must expect the arguments.
        /// </summary>
        /// <param name="methodName">The name of the method to call</param>
        /// <param name="callConvention">The method calling convention</param>
        /// <param name="arguments">The arguments passed to the method</param>
        /// <returns>The value returned from the target method or null if the target method does not return a value</returns>
        /// <exception cref="ObjectDisposedException">The proxy has already been disposed</exception>
        /// <exception cref="TargetException">The target method could not be found on the managed type</exception>
        public object Call(string methodName, ProxyCallConvention callConvention, params object[] arguments)
        {
            // Make sure the object has not already been disposed
            CheckDisposed();

            // Find the method
            MethodInfo method = ScriptType.FindCachedMethod(methodName, false);

            // Check for error
            if (method == null)
                throw new TargetException(string.Format("Type '{0}' does not define a method called '{1}'", ScriptType, methodName));

            // Call the method
            object result = method.Invoke(Instance, arguments);

            // Check for coroutine
            if ((result is IEnumerator) && (callConvention == ProxyCallConvention.Any || callConvention == ProxyCallConvention.UnityCoroutine) == true)
            {
                // Get the coroutine method
                IEnumerator routine = result as IEnumerator;

                // Check if the calling object is a mono behaviour
                if (IsMonoBehaviour == true)
                {
                    // Get the proxy as a mono behaviour
                    UnityEngine.MonoBehaviour mono = GetInstanceAs<UnityEngine.MonoBehaviour>(false);

                    // Register the coroutine with the behaviour so that it will be called after yield
                    mono.StartCoroutine(routine);
                }
            }

            // Get the return value
            return result;
        }

        /// <summary>
        /// Attempt to call a method on the managed instance with the specified name.
        /// Any exceptions thrown as a result of locating or calling the method will be caught silently.
        /// This works in a similar was as <see cref="UnityEngine.GameObject.SendMessage(string)"/> where the target method name is specified.
        /// The target method must not accept any arguments.
        /// The target method will be called using <see cref="ProxyCallConvention.Any"/>
        /// </summary>
        /// <param name="method">The name of the method to call</param>
        /// <returns>The value returned from the target method or null if the target method does not return a value</returns>
        public object SafeCall(string method)
        {
            return SafeCall(method, ProxyCallConvention.Any);
        }

        /// <summary>
        /// Attempt to call a method on the managed instance with the specified name.
        /// Any exceptions thrown as a result of locating or calling the method will be caught silently.
        /// This works in a similar was as <see cref="UnityEngine.GameObject.SendMessage(string)"/> where the target method name is specified.
        /// The target method must not accept any arguments.
        /// </summary>
        /// <param name="method">The name of the method to call</param>
        /// <param name="callConvention">The method calling convention</param>
        /// <returns>The value returned from the target method or null if the target method does not return a value</returns>
        public object SafeCall(string method, ProxyCallConvention callConvention)
        {
            try
            {
                // Make sure the object has not already been disposed
                CheckDisposed();

                // Catch any exceptions
                return Call(method, callConvention);
            }
            catch
            {
                // Exception - Maybe caused by the target method
                return null;
            }
        }

        /// <summary>
        /// Attempt to call a method on the managed instance with the specified name.
        /// Any exceptions thrown as a result of locating or calling the method will be caught silently.
        /// This works in a similar was as <see cref="UnityEngine.GameObject.SendMessage(string)"/> where the target method name is specified.
        /// Any number of arguments may be specified but the target method must expect the arguments.
        /// The target method will be called using <see cref="ProxyCallConvention.Any"/>
        /// </summary>
        /// <param name="method">The name of the method to call</param>
        /// <param name="arguments">The arguments passed to the method</param>
        /// <returns>The value returned from the target method or null if the target method does not return a value</returns>
        public object SafeCall(string method, params object[] arguments)
        {
            return SafeCall(method, ProxyCallConvention.Any, arguments);
        }

        /// <summary>
        /// Attempt to call a method on the managed instance with the specified name.
        /// Any exceptions thrown as a result of locating or calling the method will be caught silently.
        /// This works in a similar was as <see cref="UnityEngine.GameObject.SendMessage(string)"/> where the target method name is specified.
        /// Any number of arguments may be specified but the target method must expect the arguments.
        /// </summary>
        /// <param name="method">The name of the method to call</param>
        /// <param name="callConvention">The method calling convention</param>
        /// <param name="arguments">The arguments passed to the method</param>
        /// <returns>The value returned from the target method or null if the target method does not return a value</returns>
        public object SafeCall(string method, ProxyCallConvention callConvention, params object[] arguments)
        {
            try
            {
                // Make sure the object has not already been disposed
                CheckDisposed();

                // Catch any exceptions
                return Call(method, callConvention, arguments);
            }
            catch
            {
                // Exception - Maybe cause by the target method
                return null;
            }
        }

        public Type GetInstanceType()
        {
            return Instance.GetType();
        }

        /// <summary>
        /// Attempts to get the managed instance as the specified generic type.
        /// </summary>
        /// <typeparam name="T">The generic type to return the instance as</typeparam>
        /// <param name="throwOnError">When false, any exceptions caused by the conversion will be caught and will result in a default value being returned. When true, any exceptions will not be handled.</param>
        /// <param name="errorValue">The value to return when 'throwOnError' is false and an error occurs"/></param>
        /// <returns>The managed instance as the specified generic type or the default value for the generic type if an error occured</returns>
        public virtual T GetInstanceAs<T>(bool throwOnError, T errorValue = default(T))
        {
            // Try a direct cast
            if (throwOnError == true)
                return (T)Instance;

            try
            {
                // Try to cast and catch any InvalidCast exceptions.
                T result = (T)Instance;

                // Return the result
                return result;
            }
            catch
            {
                // Error value
                return errorValue;
            }
        }

        public abstract void Dispose();

        /// <summary>
        /// If the managed object is a Unity type then this method will call 'DontDestroyOnLoad' to ensure that the object is able to survie scene loads.
        /// </summary>
        public virtual void MakePersistent()
        {
            // Make the instance survive scene loads
            if (IsUnityObject == true)
                UnityEngine.Object.DontDestroyOnLoad(UnityInstance);
        }

        protected virtual void CheckDisposed()
        {
            if(IsDisposed == true)
                throw new ObjectDisposedException("The script has already been disposed. Unity types can be disposed automatically when the wrapped type is destroyed");
        }

        public static ScriptProxy CreateScriptProxy(ScriptType type, object instance)
        {
            return CreateScriptProxy<Implementation.ScriptProxyImpl>(type, instance);
        }

        public static T CreateScriptProxy<T>(ScriptType type, object instance) where T : ScriptProxy, new()
        {
            T proxy = new T();
            proxy.ConstructInstance(type, instance);

            return proxy;
        }
    }
}
