using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace RoslynCSharp
{
    public abstract class ScriptType
    {
        // Private
        private static List<ScriptType> matchedTypes = new List<ScriptType>();
        private static List<object> matchedAttributes = new List<object>();
        private static readonly object[] emptyObjectArray = Array.Empty<object>();

        private IScriptDataProxy fields = null;
        private IScriptDataProxy safeFields = null;
        private IScriptDataProxy properties = null;
        private IScriptDataProxy safeProperties = null;
        private IScriptEventProxy events = null;
        private IScriptEventProxy safeEvents = null;

        private Dictionary<string, FieldInfo> fieldCache = null;
        private Dictionary<string, PropertyInfo> propertyCache = null;
        private Dictionary<string, MethodInfo> methodCache = null;
        private Dictionary<string, EventInfo> eventCache = null;

        // Public
        public const BindingFlags instanceAttrib = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
        public const BindingFlags staticAttrib = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

        // Properties
        /// <summary>
        /// Get the <see cref="ScriptAssembly"/> that this <see cref="ScriptType"/> is defined in.  
        /// </summary>
        public abstract ScriptAssembly Assembly { get; }

        /// <summary>
        /// Get the <see cref="ScriptType"/> parent for this type. 
        /// The return value will only be valid for nested types otherwise it will be null.
        /// </summary>
        public abstract ScriptType Parent { get; }

        /// <summary>
        /// Get the <see cref="Type"/> that this <see cref="ScriptType"/> wraps.   
        /// </summary>
        public abstract Type SystemType { get; }

        /// <summary>
        /// Get the name of the wrapped type excluding the namespace.
        /// </summary>
        public virtual string Name
        {
            get { return SystemType.Name; }
        }

        /// <summary>
        /// Get the namespace of the wrapped type.
        /// </summary>
        public virtual string Namespace
        {
            get { return SystemType.Namespace; }
        }

        /// <summary>
        /// Get the full name of the wrapped type including namespace.
        /// </summary>
        public virtual string FullName
        {
            get { return SystemType.FullName; }
        }

        /// <summary>
        /// Returns true if the wrapped type is public or false if not.
        /// </summary>
        public virtual bool IsPublic
        {
            get { return SystemType.IsPublic; }
        }

        /// <summary>
        /// Returns true if this <see cref="ScriptType"/> instance is a nested type.
        /// </summary>
        public abstract bool IsNestedType { get; }

        /// <summary>
        /// Returns true if this <see cref="ScriptType"/> defines one or more nested types or false if not.
        /// </summary>
        public abstract bool HasNestedTypes { get; }

        /// <summary>
        /// Get all nested <see cref="ScriptType"/> of this type.
        /// If this type does not define any nested types then the return value will be an empty array.
        /// </summary>
        public abstract ScriptType[] NestedTypes { get; }

        /// <summary>
        /// Returns the <see cref="IScriptDataProxy"/> that provides access to the static fields of the wrapped type. 
        /// </summary>
        public virtual IScriptDataProxy FieldsStatic
        {
            get
            {
                if (fields == null)
                    fields = new ScriptFieldDataProxy(this, null, true, true);

                return fields;
            }
        }

        /// <summary>
        /// Returns the <see cref="IScriptDataProxy"/> that provides access to the static fields of the wrapped type.
        /// Any exceptions thrown by locating or accessing the property will be handled.
        /// </summary>
        public virtual IScriptDataProxy SafeFieldsStatic
        {
            get
            {
                if (safeFields == null)
                    safeFields = new ScriptFieldDataProxy(this, null, true, false);

                return safeFields;
            }
        }

        /// <summary>
        /// Returns the <see cref="IScriptDataProxy"/> that provides access to the static properties of the wrapped type. 
        /// </summary>
        public virtual IScriptDataProxy PropertiesStatic
        {
            get
            {
                if (properties == null)
                    properties = new ScriptPropertyDataProxy(this, null, true, true);

                return properties;
            }
        }

        /// <summary>
        /// Returns the <see cref="IScriptMemberProxy"/> that provides access to the static properties of the wrapped type.
        /// Any exceptions thrown by locating or accessing the property will be handled.
        /// </summary>
        public virtual IScriptDataProxy SafePropertiesStatic
        {
            get
            {
                if (safeProperties == null)
                    safeProperties = new ScriptPropertyDataProxy(this, null, true, false);

                return safeProperties;
            }
        }

        /// <summary>
        /// Returns the <see cref="IScriptEventProxy"/> that provides access to the static events of the wrapped type. 
        /// </summary>
        public virtual IScriptEventProxy EventsStatic
        {
            get
            {
                if (events == null)
                    events = new ScriptEventHandlerProxy(this, null, true, true);

                return events;
            }
        }

        /// <summary>
        /// Returns the <see cref="IScriptEventProxy"/> that provides access to the static events of the wrapped type.
        /// Any exceptions thrown by locating or accessing the event will be handled.
        /// </summary>
        public virtual IScriptEventProxy SafeEventsStatic
        {
            get
            {
                if (safeEvents == null)
                    safeEvents = new ScriptEventHandlerProxy(this, null, true, false);

                return safeEvents;
            }
        }

        /// <summary>
        /// Returns true if this type inherits from <see cref="UnityEngine.Object"/>.
        /// See also <see cref="IsMonoBehaviour"/>.
        /// </summary>
        public bool IsUnityObject
        {
            get { return IsSubTypeOf<UnityEngine.Object>(); }
        }

        /// <summary>
        /// Returns true if this type inherits from <see cref="MonoBehaviour"/>.
        /// </summary>
        public bool IsMonoBehaviour
        {
            get { return IsSubTypeOf<UnityEngine.MonoBehaviour>(); }
        }

        /// <summary>
        /// Returns true if this type inherits from <see cref="ScriptableObject"/> 
        /// </summary>
        public bool IsScriptableObject
        {
            get { return IsSubTypeOf<UnityEngine.ScriptableObject>(); }
        }

        public abstract ICollection<object> CustomAttributes { get; }

        // Construction
        protected abstract void ConstructInstance(ScriptAssembly assembly, ScriptType parent, ScriptType[] nestedTypes, Type systemType);

        // Methods
        public override string ToString()
        {
            return string.Format("{0}({1})", nameof(ScriptType), SystemType);
        }

        #region CreateInstance
        /// <summary>
        /// Creates an instance of this type.
        /// The type will be constructed using the appropriate method (AddComponent, CreateInstance, new).
        /// </summary>
        /// <param name="parent">The <see cref="GameObject"/> to attach the instance to or null if the type is not a <see cref="MonoBehaviour"/></param>
        /// <returns>An instance of <see cref="ScriptProxy"/></returns>
        public virtual ScriptProxy CreateInstance(GameObject parent = null)
        {
            ScriptProxy result = null;

            if(IsMonoBehaviour == true)
            {
                if (parent == null)
                    throw new ArgumentNullException("Cannot create mono behaviour instance because a null parent game object was supplied");

                result = CreateMonoBehaviourInstanceImpl(parent);

                // Register the proxy
                if (result != null)
                    Assembly.Domain.Execution.AddScriptProxy(result);

                return result;
            }
            else if(IsScriptableObject == true)
            {
                result = CreateScriptableObjectInstanceImpl();

                // Register the proxy
                if (result != null)
                    Assembly.Domain.Execution.AddScriptProxy(result);

                return result;
            }

            result = CreateInstanceImpl(emptyObjectArray);

            // Register the proxy
            if (result != null)
                Assembly.Domain.Execution.AddScriptProxy(result);

            return result;
        }

        /// <summary>
        /// Creates an instance of this type.
        /// The type will be constructed using the appropriate method (AddComponent, CreateInstance, new).
        /// </summary>
        /// <param name="parent">The <see cref="GameObject"/> to attach the instance to or null if the type is not a <see cref="MonoBehaviour"/></param>
        /// <param name="parameters">The parameter list for the desired constructor. only used when the type does not inherit from <see cref="UnityEngine.Object"/></param>
        /// <returns>An instance of <see cref="ScriptProxy"/></returns>
        public virtual ScriptProxy CreateInstance(GameObject parent = null, params object[] args)
        {
            ScriptProxy result = null;

            if (IsMonoBehaviour == true)
            {
                if (parent == null)
                    throw new ArgumentNullException("Cannot create mono behaviour instance because a null parent game object was supplied");

                result = CreateMonoBehaviourInstanceImpl(parent);

                // Register the proxy
                if(result != null)
                    Assembly.Domain.Execution.AddScriptProxy(result);

                return result;
            }
            else if (IsScriptableObject == true)
            {
                result = CreateScriptableObjectInstanceImpl();

                // Register the proxy
                if (result != null)
                    Assembly.Domain.Execution.AddScriptProxy(result);

                return result;
            }

            if (args == null)
                args = emptyObjectArray;

            result = CreateInstanceImpl(args);

            // Register the proxy
            if (result != null)
                Assembly.Domain.Execution.AddScriptProxy(result);

            return result;
        }

        /// <summary>
        /// Creates a raw instance of this type.
        /// A raw instance will return the actual instance of the type as opposed to a <see cref="ScriptProxy"/> which allows for more control. 
        /// The type will be constructed using the appropriate method (AddComponent, CreateInstance, new).
        /// </summary>
        /// <param name="parent">The <see cref="GameObject"/> to attach the instance to or null if the type is not a <see cref="MonoBehaviour"/></param>
        /// <returns>A raw instance that can be cast to the desired type</returns>
        public virtual object CreateInstanceRaw(GameObject parent = null)
        {
            return CreateInstance(parent).Instance;
        }

        /// <summary>
        /// Creates an instance of this type and returns the result as the specified generic type.
        /// A raw instance will return the actual instance of the type as opposed to a <see cref="ScriptProxy"/> which allows for more control. 
        /// The type will be constructed using the appropriate method (AddComponent, CreateInstance, new).
        /// </summary>
        /// <typeparam name="T">The generic type to return the instance as</typeparam>
        /// <param name="parent">The <see cref="GameObject"/> to attach the instance to or null if the type is not a <see cref="MonoBehaviour"/></param>
        /// <returns>A raw instance as the specified generic type</returns>
        public virtual T CreateInstanceRaw<T>(GameObject parent = null)
        {
            try
            {
                return CreateInstance(parent)
                    .GetInstanceAs<T>(false);
            }
            catch (NullReferenceException)
            {
                return default;
            }
        }


        /// <summary>
        /// Creates an instance of this type and returns the result as the specified generic type.
        /// A raw instance will return the actual instance of the type as opposed to a <see cref="ScriptProxy"/> which allows for more control. 
        /// The type will be constructed using the appropriate method (AddComponent, CreateInstance, new).
        /// </summary>
        /// <typeparam name="T">The generic type to return the instance as</typeparam>
        /// <param name="parent">The <see cref="GameObject"/> to attach the instance to or null if the type is not a <see cref="MonoBehaviour"/></param>
        /// <param name="parameters">The parameter list for the desired constructor. only used when the type does not inherit from <see cref="UnityEngine.Object"/></param>
        /// <returns>A raw instance as the specified generic type</returns>
        public virtual T CreateInstanceRaw<T>(GameObject parent = null, params object[] args)
        {
            try
            {
                return CreateInstance(parent, args)
                    .GetInstanceAs<T>(false);
            }
            catch (NullReferenceException)
            {
                return default;
            }
        }

        /// <summary>
        /// Creates a raw instance of this type.
        /// A raw instance will return the actual instance of the type as opposed to a <see cref="ScriptProxy"/> which allows for more control. 
        /// The type will be constructed using the appropriate method (AddComponent, CreateInstance, new).
        /// </summary>
        /// <param name="parent">The <see cref="GameObject"/> to attach the instance to or null if the type is not a <see cref="MonoBehaviour"/></param>
        /// <param name="parameters">The parameter list for the desired constructor. only used when the type does not inherit from <see cref="UnityEngine.Object"/></param>
        /// <returns>A raw instance that can be cast to the desired type</returns>
        public virtual object CreateInstanceRaw(GameObject parent = null, params object[] args)
        {
            return CreateInstance(parent, args).Instance;
        }

        protected abstract ScriptProxy CreateMonoBehaviourInstanceImpl(GameObject parent);

        protected abstract ScriptProxy CreateScriptableObjectInstanceImpl();

        protected abstract ScriptProxy CreateInstanceImpl(object[] args);
        #endregion

        /// <summary>
        /// Returns true if this type inherits from the specified type.
        /// </summary>
        /// <param name="subType">The base type</param>
        /// <returns>True if this type inherits from the specified type</returns>
        public virtual bool IsSubTypeOf(Type subType)
        {
            return subType.IsAssignableFrom(SystemType);
        }

        /// <summary>
        /// Returns true if this type inherits from the specified type.
        /// </summary>
        /// <typeparam name="T">The base type</typeparam>
        /// <returns>True if this type inherits from the specified type</returns>
        public bool IsSubTypeOf<T>()
        {
            return IsSubTypeOf(typeof(T));
        }

        #region FindMember
        /// <summary>
        /// Finds a field with the specified name from the cache if possible.
        /// If the field is not present in the cache then it will be added automatically so that subsequent calls will be quicker.
        /// </summary>
        /// <param name="name">The name of the field to find</param>
        /// <param name="isStatic">Is the target field a static or instance field</param>
        /// <returns>The <see cref="FieldInfo"/> for the specified field</returns>
        public FieldInfo FindCachedField(string name, bool isStatic)
        {
            FieldInfo targetField = null;

            // Check for cached field
            if (fieldCache != null && fieldCache.TryGetValue(name, out targetField) == true)
                if (targetField.IsStatic == isStatic)
                    return targetField;

            // Select binding flags
            BindingFlags flags = (isStatic == true)
                ? staticAttrib
                : instanceAttrib;

            // Get field
            targetField = FindFieldImpl(name, flags);

            // Check for null
            if (targetField == null)
                return null;

            // Cache method
            if (fieldCache == null)
                fieldCache = new Dictionary<string, FieldInfo>();

            if (fieldCache.ContainsKey(name) == false)
                fieldCache.Add(name, targetField);

            return targetField;
        }

        protected abstract FieldInfo FindFieldImpl(string name, BindingFlags bindingAttrib);

        /// <summary>
        /// Finds a property with the specified name from the cache if possible.
        /// If the property is not present in the cache then it will be added automatically so that subsequent calls will be quicker.
        /// </summary>
        /// <param name="name">The name of the property to find</param>
        /// <param name="isStatic">Is the target property a static or instance property</param>
        /// <returns>The <see cref="PropertyInfo"/> for the specified property</returns>
        public PropertyInfo FindCachedProperty(string name, bool isStatic)
        {
            PropertyInfo targetProperty = null;

            // Check for cached property
            if (propertyCache != null && propertyCache.TryGetValue(name, out targetProperty) == true)
                if (targetProperty.GetGetMethod().IsStatic == isStatic)
                    return targetProperty;

            // Select binding flags
            BindingFlags flags = (isStatic == true)
                ? staticAttrib
                : instanceAttrib;

            // Get property
            targetProperty = FindPropertyImpl(name, flags);

            // Check for null
            if (targetProperty == null)
                return null;

            // Cache method
            if (propertyCache == null)
                propertyCache = new Dictionary<string, PropertyInfo>();

            if (propertyCache.ContainsKey(name) == false)
                propertyCache.Add(name, targetProperty);

            return targetProperty;
        }

        protected abstract PropertyInfo FindPropertyImpl(string name, BindingFlags bindingAttib);

        /// <summary>
        /// Finds a method with the specified name from the cache if possible.
        /// If the method is not present in the cache then it will be added automatically so that subsequent calls will be quicker.
        /// </summary>
        /// <param name="name">The name of the method to find</param>
        /// <param name="isStatic">Is the target method a static or instance method</param>
        /// <returns>The <see cref="MethodInfo"/> for the specified method</returns>
        public MethodInfo FindCachedMethod(string name, bool isStatic)
        {
            MethodInfo targetMethod = null;

            // Check for cached method
            if (methodCache != null && methodCache.TryGetValue(name, out targetMethod) == true)
                if(targetMethod.IsStatic == isStatic)
                    return targetMethod;

            // Select binding flags
            BindingFlags flags = (isStatic == true)
                ? staticAttrib
                : instanceAttrib;

            // Get method
            targetMethod = FindMethodImpl(name, flags);

            // Check for null
            if (targetMethod == null)
                return null;

            // Cache method
            if (methodCache == null)
                methodCache = new Dictionary<string, MethodInfo>();

            if (methodCache.ContainsKey(name) == false)
                methodCache.Add(name, targetMethod);

            return targetMethod;
        }

        protected abstract MethodInfo FindMethodImpl(string name, BindingFlags bindingAttrib);

        /// <summary>
        /// Finds an event with the specified name from the cache if possible.
        /// If the event is not present in the cache then it will be added automatically so that subsequent calls will be quicker.
        /// </summary>
        /// <param name="name">The name of the event to find</param>
        /// <param name="isStatic">Is the target event a static or instance event</param>
        /// <returns>The <see cref="EventInfo"/> for the specified event</returns>
        public EventInfo FindCachedEvent(string name, bool isStatic)
        {
            EventInfo targetEvent = null;

            // Check for cached event
            if (eventCache != null && eventCache.TryGetValue(name, out targetEvent) == true)
                if (targetEvent.GetAddMethod().IsStatic == isStatic)
                    return targetEvent;

            // Select binding flags
            BindingFlags flags = (isStatic == true)
                ? staticAttrib
                : instanceAttrib;

            // Get event
            targetEvent = FindEventImpl(name, flags);

            // Check for null
            if (targetEvent == null)
                return null;

            // Cache method
            if (eventCache == null)
                eventCache = new Dictionary<string, EventInfo>();

            if (eventCache.ContainsKey(name) == false)
                eventCache.Add(name, targetEvent);

            return targetEvent;
        }

        protected abstract EventInfo FindEventImpl(string name, BindingFlags bindingAttrib);
        #endregion

        #region Call
        /// <summary>
        /// Attempts to call a static method on this <see cref="ScriptType"/> with the specified name.
        /// This works in a similar way as <see cref="UnityEngine.GameObject.SendMessage(string)"/> where the method name is specified.
        /// The target method must be static and not accept any arguments.
        /// </summary>
        /// <param name="methodName">The name of the static method to call</param>
        /// <returns>The value returned from the target method or null if the target method does not return a value</returns>
        /// <exception cref="TargetException">The target method could not be found on the managed type</exception>
        /// <exception cref="TargetException">The target method is not static</exception>
        public virtual object CallStatic(string methodName)
        {
            // Find the method
            MethodInfo method = FindCachedMethod(methodName, true);

            // Check for error
            if (method == null)
                throw new TargetException(string.Format("Type '{0}' does not define a static method called '{1}'", this, methodName));

            // Check for static
            if (method.IsStatic == false)
                throw new TargetException(string.Format("The target method '{0}' is not marked as static and must be called on an object", methodName));

            // Call the method
            return method.Invoke(null, null);
        }

        /// <summary>
        /// Attempts to call a static method on this <see cref="ScriptType"/> with the specified name.
        /// This works in a similar way as <see cref="UnityEngine.GameObject.SendMessage(string)"/> where the method name is specified.
        /// The target method must be static and not accept any arguments.
        /// </summary>
        /// <param name="methodName">The name of the static method to call</param>
        /// <param name="arguments">The arguemnts passed to the method</param>
        /// <returns>The value returned from the target method or null if the target method does not return a value</returns>
        /// <exception cref="TargetException">The target method could not be found on the managed type</exception>
        /// <exception cref="TargetException">The target method is not static</exception>
        public virtual object CallStatic(string methodName, params object[] arguments)
        {
            // Find the method
            MethodInfo method = FindCachedMethod(methodName, true);

            // Check for error
            if (method == null)
                throw new TargetException(string.Format("Type '{0}' does not define a static method called '{1}'", this, methodName));

            // Check for static
            if (method.IsStatic == false)
                throw new TargetException(string.Format("The target method '{0}' is not marked as static and must be called on an object", methodName));

            // Call the method
            return method.Invoke(null, arguments);
        }

        /// <summary>
        /// Attempts to call a static method on this <see cref="ScriptType"/> with the specified name.
        /// Any exceptions throw as a result of locating or calling the method will be caught silently
        /// This works in a similar way as <see cref="UnityEngine.GameObject.SendMessage(string)"/> where the method name is specified.
        /// The target method must be static and not accept any arguments.
        /// </summary>
        /// <param name="method">The name of the static method to call</param>
        /// <returns>The value returned from the target method or null if the target method does not return a value</returns>
        public virtual object SafeCallStatic(string method)
        {
            try
            {
                // Call the method and catch any exceptions
                return CallStatic(method);
            }
            catch
            {
                // Exception - Fail silently
                return null;
            }
        }

        /// <summary>
        /// Attempts to call a static method on this <see cref="ScriptType"/> with the specified name.
        /// Any exceptions throw as a result of locating or calling the method will be caught silently
        /// This works in a similar way as <see cref="UnityEngine.GameObject.SendMessage(string)"/> where the method name is specified.
        /// The target method must be static and not accept any arguments.
        /// </summary>
        /// <param name="method">The name of the static method to call</param>
        /// <param name="arguments">The arguments passed to the method</param>
        /// <returns>The value returned from the target method or null if the target method does not return a value</returns>
        public virtual object SafeCallStatic(string method, params object[] arguments)
        {
            try
            {
                // Call the method and catch any exceptions
                return CallStatic(method, arguments);
            }
            catch
            {
                // Exception - Fail silently
                return null;
            }
        }
        #endregion

        #region CustomAttributes
        /// <summary>
        /// Returns true if this type defines a custom attribute of the specified type.
        /// </summary>
        /// <param name="type">The attribute type to find</param>
        /// <param name="includeSubTypes">True if sub classes of the specified attrbute should be found or false if not</param>
        /// <returns>True if the specified custom attribute type is defined on this type or false if not</returns>
        public virtual bool HasAttribute(Type type, bool includeSubTypes = false)
        {
            foreach (object attribute in CustomAttributes)
            {
                if (includeSubTypes == false)
                {
                    // Check for matching type
                    if (attribute.GetType() == type)
                        return true;
                }
                else
                {
                    // Check for matching type or sub type
                    if (type.IsAssignableFrom(attribute.GetType()) == true)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if this type defines a custom attribute of the specified type.
        /// </summary>
        /// <param name="includeSubTypes">True if sub classes of the specified attrbute should be found or false if not</param>
        /// <typeparam name="T">The generic attribute type to find</typeparam>
        /// <returns>True if the specified custom attribute type is defined on this type or false if not</returns>
        public bool HasAttribute<T>(bool includeSubTypes = false) where T : Attribute
        {
            return HasAttribute(typeof(T), includeSubTypes);
        }

        /// <summary>
        /// Returns a custom attribute instance if this type defines a custom attribute of the specified type.
        /// </summary>
        /// <param name="type">The attribute type to find</param>
        /// <param name="includeSubTypes">True if sub classes of the specified attrbute should be found or false if not</param>
        /// <returns>A custom attribute instance if the specified custom attribute type is defined on this type or null if not</returns>
        public virtual object GetAttribute(Type type, bool includeSubTypes = false)
        {
            foreach (object attribute in CustomAttributes)
            {
                if (includeSubTypes == false)
                {
                    // Get matching attribute
                    if (attribute.GetType() == type)
                        return attribute;
                }
                else
                {
                    // Check for matching type or sub type
                    if (type.IsAssignableFrom(attribute.GetType()) == true)
                        return attribute;
                }
            }
            return null;
        }


        /// <summary>
        /// Returns a custom attribute instance if this type defines a custom attribute of the specified type.
        /// </summary>
        /// <param name="includeSubTypes">True if sub classes of the specified attrbute should be found or false if not</param>
        /// <typeparam name="T">The generic attribute type to find</typeparam>
        /// <returns>A custom attribute instance if the specified custom attribute type is defined on this type or null if not</returns>
        public T GetAttribute<T>(bool includeSubTypes = false) where T : Attribute
        {
            return GetAttribute(typeof(T), includeSubTypes) as T;
        }


        /// <summary>
        /// Returns an array of custom attribute instances if this type defines a custom attribute of the specified type.
        /// </summary>
        /// <param name="type">The attribute type to find</param>
        /// <param name="includeSubTypes">True if sub classes of the specified attrbute should be found or false if not</param>
        /// <returns>An array of custom attribute instance if the specified custom attribute type is defined on this type or an empty array if not</returns>
        public virtual object[] GetAttributes(Type type, bool includeSubTypes = false)
        {
            matchedAttributes.Clear();

            foreach (object attribute in CustomAttributes)
            {
                if (includeSubTypes == false)
                {
                    // Get matching attribute
                    if (attribute.GetType() == type)
                        matchedAttributes.Add(attribute);
                }
                else
                {
                    // Check for matching type or sub type
                    if (type.IsAssignableFrom(attribute.GetType()) == true)
                        matchedAttributes.Add(attribute);
                }
            }
            return matchedAttributes.ToArray();
        }

        /// <summary>
        /// Returns an array of custom attribute instances if this type defines a custom attribute of the specified type.
        /// </summary>
        /// <param name="includeSubTypes">True if sub classes of the specified attrbute should be found or false if not</param>
        /// <typeparam name="T">The generic attribute type to find</typeparam>
        /// <returns>An array of custom attribute instance if the specified custom attribute type is defined on this type or an empty array if not</returns>
        public T[] GetAttributes<T>(bool includeSubTypes = false) where T : Attribute
        {
            return GetAttributes(typeof(T), includeSubTypes) as T[];
        }
        #endregion

        /// <summary>
        /// Attempt to find a nested <see cref="ScriptType"/> of this type with the specified name.
        /// </summary>
        /// <param name="nestedTypeName">The name of the nested type to search for</param>
        /// <returns>A <see cref="ScriptType"/> instance representing the matching nested type if found or null</returns>
        public virtual ScriptType FindNestedType(string nestedTypeName)
        {
            return Array.Find(NestedTypes, t => t.Name == nestedTypeName);
        }

        /// <summary>
        /// Attempt to find a nested <see cref="ScriptType"/> of this type with the specified full name.
        /// Note that the full name is the reflection path to the nested type including parent type names separated by the '+' character as per reflection specification. For example 'MyNamespace.MyParentType+MyNestedType'.
        /// </summary>
        /// <param name="nestedTypeFullName">The full name path of the nested type</param>
        /// <returns>An <see cref="ScriptType"/> instance representing the matching nested type if found or null</returns>
        public virtual ScriptType FindNestedTypeFullName(string nestedTypeFullName)
        {
            return Array.Find(NestedTypes, t => t.FullName == nestedTypeFullName);
        }


        #region FindTypeStatic
        /// <summary>
        /// Attempt to find a type with the specified name in the specified <see cref="ScriptDomain"/>.
        /// </summary>
        /// <param name="typeName">The name of the type to find</param>
        /// <param name="searchDomain">The domain to search or null if the active domain should be used</param>
        /// <returns>A <see cref="ScriptType"/> matching the specified type name or null if the type was not found</returns>
        public static ScriptType FindType(string typeName, ScriptDomain searchDomain = null)
        {
            // Try to resolve domain
            if (ResolveSearchDomain(ref searchDomain) == false)
                return null;

            // Search all assemblies
            foreach (ScriptAssembly assembly in searchDomain.Assemblies)
            {
                // Try to find type
                ScriptType type = assembly.FindType(typeName);

                // Check for success
                if (type != null)
                    return type;
            }

            // Type not found
            return null;
        }

        /// <summary>
        /// Attempt to find a type with the specified name in the specified <see cref="ScriptDomain"/> that inherits from the specified base type.
        /// </summary>
        /// <param name="typeName">The name of the type to find</param>
        /// <param name="subType">The base type that the type must inherit from</param>
        /// <param name="searchDomain">The domain to search or null if the active domain should be used</param>
        /// <returns>A <see cref="ScriptType"/> matching the specified type name and inheritance constraints or null if the type was not found</returns>
        public static ScriptType FindSubTypeOf(string typeName, Type subType, ScriptDomain searchDomain = null)
        {
            // Try to resolve domain
            if (ResolveSearchDomain(ref searchDomain) == false)
                return null;

            // Search all assemblies
            foreach (ScriptAssembly assembly in searchDomain.Assemblies)
            {
                // Try to find type
                ScriptType type = assembly.FindSubTypeOf(subType, typeName);

                // Check for success
                if (type != null)
                    return type;
            }

            // Type not found
            return null;
        }

        /// <summary>
        /// Attempt to find a type with the specified name in the specified <see cref="ScriptDomain"/> that inherits from the specified generic base type.
        /// </summary>
        /// <typeparam name="T">The generic type that the type must inherit from</typeparam>
        /// <param name="typeName">The name of the type to find</param>
        /// <param name="searchDomain">The domain to search or null if the active domain should be used</param>
        /// <returns>A <see cref="ScriptType"/> matching the specified type name and inheritance constranints or null if the type was not found</returns>
        public static ScriptType FindSubTypeOf<T>(string typeName, ScriptDomain searchDomain = null)
        {
            // Try to resolve domain
            if (ResolveSearchDomain(ref searchDomain) == false)
                return null;

            // Search all assemblies
            foreach (ScriptAssembly assembly in searchDomain.Assemblies)
            {
                // Try to find type
                ScriptType type = assembly.FindSubTypeOf<T>(typeName);

                // Check for success
                if (type != null)
                    return type;
            }

            // Type not found
            return null;
        }

        /// <summary>
        /// Attempt to find the first type that inherits from the specfieid sub type.
        /// </summary>
        /// <param name="subType">The base type that the type should inherit from</param>
        /// <param name="searchDomain">The domain to search or null if the active domain should be used</param>
        /// <returns>A <see cref="ScriptType"/> matching the specified inheritance constraints or null if the type was not found</returns>
        public static ScriptType FindSubTypeOf(Type subType, ScriptDomain searchDomain = null)
        {
            // Try to resolve domain
            if (ResolveSearchDomain(ref searchDomain) == false)
                return null;

            // Search all assemblies
            foreach (ScriptAssembly assembly in searchDomain.Assemblies)
            {
                // Try to find type
                ScriptType type = assembly.FindSubTypeOf(subType);

                // Check for success
                if (type != null)
                    return type;
            }

            // Type not found
            return null;
        }

        /// <summary>
        /// Attempt to find the first type that inherits from the specified generic sub type.
        /// </summary>
        /// <typeparam name="T">The generic type that the type must inherit from</typeparam>
        /// <param name="searchDomain">The domain to search or null if the active domain should be used</param>
        /// <returns>A <see cref="ScriptType"/> matching the specified inheritance constraints or null if the type was not found</returns>
        public static ScriptType FindSubTypeOf<T>(ScriptDomain searchDomain = null)
        {
            // Try to resolve domain
            if (ResolveSearchDomain(ref searchDomain) == false)
                return null;

            // Search all assemblies
            foreach (ScriptAssembly assembly in searchDomain.Assemblies)
            {
                // Try to find type
                ScriptType type = assembly.FindSubTypeOf<T>();

                // Check for success
                if (type != null)
                    return type;
            }

            // Type not found
            return null;
        }

        /// <summary>
        /// Attempt to find all types that inherit from the specified sub type.
        /// </summary>
        /// <param name="subType">The base type that the types must inherit from</param>
        /// <param name="includeNonPublic">Should non-public types be included in the search</param>
        /// <param name="searchDomain">The domain to search or null if the active domain should be used</param>
        /// <returns>An array of <see cref="ScriptType"/> that inherit from the specified base type or an empty array if no types were found</returns>
        public static ScriptType[] FindAllSubTypesOf(Type subType, bool includeNonPublic = true, ScriptDomain searchDomain = null)
        {
            // Try to resolve domain
            if (ResolveSearchDomain(ref searchDomain) == false)
                return new ScriptType[0];

            // Use shared types list
            matchedTypes.Clear();

            // Search all assemblies
            foreach (ScriptAssembly assembly in searchDomain.Assemblies)
            {
                // Try to find type
                ScriptType[] types = assembly.FindAllSubTypesOf(subType, includeNonPublic);

                // Add to result
                matchedTypes.AddRange(types);
            }

            // Get types array
            return matchedTypes.ToArray();
        }

        /// <summary>
        /// Attempt to find all the types that inherit from the specified generic sub type.
        /// </summary>
        /// <typeparam name="T">The generic base type that the types must inherit from</typeparam>
        /// <param name="includeNonPublic">Should non-public types be included in the search</param>
        /// <param name="searchDomain">The domain to search or null if the active domain should be used</param>
        /// <returns>An array of <see cref="ScriptType"/> that inherit from the specified base type or an empty array if no types were found</returns>
        public static ScriptType[] FindAllSubTypesOf<T>(bool includeNonPublic = true, ScriptDomain searchDomain = null)
        {
            // Try to resolve domain
            if (ResolveSearchDomain(ref searchDomain) == false)
                return new ScriptType[0];

            // Use shared types list
            matchedTypes.Clear();

            // Search all assemblies
            foreach (ScriptAssembly assembly in searchDomain.Assemblies)
            {
                // Try to find type
                ScriptType[] types = assembly.FindAllSubTypesOf<T>(includeNonPublic);

                // Add to result
                matchedTypes.AddRange(types);
            }

            // Get types array
            return matchedTypes.ToArray();
        }

        /// <summary>
        /// Attempt to find all types in the specified domain.
        /// </summary>
        /// <param name="includeNonPublic">Should non-public types be included in the search</param>
        /// <param name="searchDomain">The domai to search or null if the active domain should be used</param>
        /// <returns>An array of <see cref="ScriptType"/> that exist in the specified domain or an empty array if no types were found</returns>
        public static ScriptType[] FindAllTypes(bool includeNonPublic = true, ScriptDomain searchDomain = null)
        {
            // Try to resolve domain
            if (ResolveSearchDomain(ref searchDomain) == false)
                return new ScriptType[0];

            // Use shared types list
            matchedTypes.Clear();

            // Search all assemblies
            foreach (ScriptAssembly assembly in searchDomain.Assemblies)
            {
                // Find types
                ScriptType[] types = assembly.FindAllTypes(includeNonPublic);

                // Add to result
                matchedTypes.AddRange(types);
            }

            return matchedTypes.ToArray();
        }

        /// <summary>
        /// Attempts to find all types in the specified domain that inherit from <see cref="UnityEngine.Object"/>.
        /// </summary>
        /// <param name="includeNonPublic">Should non-public types be included in the search</param>
        /// <param name="searchDomain">The domain to search or null if the active domain should be used</param>
        /// <returns>An array of <see cref="ScriptType"/> that inherit from <see cref="UnityEngine.Object"/> or an empty array if no types were found</returns>
        public static ScriptType[] FindAllUnityTypes(bool includeNonPublic = true, ScriptDomain searchDomain = null)
        {
            // Try to resolve domain
            if (ResolveSearchDomain(ref searchDomain) == false)
                return new ScriptType[0];

            // Use shared types list
            matchedTypes.Clear();

            // Search all assemblies
            foreach (ScriptAssembly assembly in searchDomain.Assemblies)
            {
                // Find types
                ScriptType[] types = assembly.FindAllUnityTypes(includeNonPublic);

                // Add to result
                matchedTypes.AddRange(types);
            }

            return matchedTypes.ToArray();
        }

        /// <summary>
        /// Attempts to find all types in the specified domain that inherit from <see cref="UnityEngine.MonoBehaviour"/>.
        /// </summary>
        /// <param name="includeNonPublic">Should non-public types be included in the search</param>
        /// <param name="searchDomain">The domain to search or null if the active domain should be used</param>
        /// <returns>An array of <see cref="ScriptType"/> that inherit from <see cref="UnityEngine.MonoBehaviour"/> or an empty array if no types were found</returns>
        public static ScriptType[] FindAllMonoBehaviourTypes(bool includeNonPublic = true, ScriptDomain searchDomain = null)
        {
            // Try to resolve domain
            if (ResolveSearchDomain(ref searchDomain) == false)
                return new ScriptType[0];

            // Use shared types list
            matchedTypes.Clear();

            // Search all assemblies
            foreach (ScriptAssembly assembly in searchDomain.Assemblies)
            {
                // Find types
                ScriptType[] types = assembly.FindAllMonoBehaviourTypes(includeNonPublic);

                // Add to result
                matchedTypes.AddRange(types);
            }

            return matchedTypes.ToArray();
        }

        /// <summary>
        /// Attempts to find all types in the specified domain that inherit from <see cref="UnityEngine.ScriptableObject"/>.
        /// </summary>
        /// <param name="includeNonPublic">Should non-public types be included in the search</param>
        /// <param name="searchDomain">The domain to search or null if the active domain should be used</param>
        /// <returns>An array of <see cref="ScriptType"/> that inherit from <see cref="UnityEngine.ScriptableObject"/> or an empty array if no types were found</returns>
        public static ScriptType[] FindAllScriptableObjectTypes(bool includeNonPublic = true, ScriptDomain searchDomain = null)
        {
            // Try to resolve domain
            if (ResolveSearchDomain(ref searchDomain) == false)
                return new ScriptType[0];

            // Use shared types list
            matchedTypes.Clear();

            // Search all assemblies
            foreach (ScriptAssembly assembly in searchDomain.Assemblies)
            {
                // Find types
                ScriptType[] types = assembly.FindAllScriptableObjectTypes(includeNonPublic);

                // Add to result
                matchedTypes.AddRange(types);
            }

            return matchedTypes.ToArray();
        }

        /// <summary>
        /// Enumerates all types that inherit from the specified sub type.
        /// </summary>
        /// <param name="subType">The base type that the types must inherit from</param>
        /// <param name="includeNonPublic">Should non-public types be included in the search</param>
        /// <param name="searchDomain">The domain to search or null if the active domain should be used</param>
        /// <returns>Enumerable of matching results</returns>
        public static IEnumerable<ScriptType> EnumerateAllSubTypesOf(Type subType, bool includeNonPublic = true, ScriptDomain searchDomain = null)
        {
            // Try to resolve domain
            if (ResolveSearchDomain(ref searchDomain) == false)
                yield break;

            // Search all assemblies
            foreach (ScriptAssembly assembly in searchDomain.Assemblies)
            {
                // Try to find type
                foreach (ScriptType type in assembly.EnumerateAllSubTypesOf(subType, includeNonPublic))
                {
                    // Return the type
                    yield return type;
                }
            }
        }

        /// <summary>
        /// Enumerate all types that inherit from the specified generic sub type.
        /// </summary>
        /// <typeparam name="T">The generic base type that the types must inherit from</typeparam>
        /// <param name="includeNonPublic">Should non-public types be included in the search</param>
        /// <param name="searchDomain">The domain to search or null if the active domain should be used</param>
        /// <returns>Enumerable of matching results</returns>
        public static IEnumerable<ScriptType> EnumerateAllSubTypesOf<T>(bool includeNonPublic = true, ScriptDomain searchDomain = null)
        {
            // Try to resolve domain
            if (ResolveSearchDomain(ref searchDomain) == false)
                yield break;

            // Search all assemblies
            foreach (ScriptAssembly assembly in searchDomain.Assemblies)
            {
                // Try to find type
                foreach (ScriptType type in assembly.EnumerateAllSubTypesOf<T>(includeNonPublic))
                {
                    // Return the type
                    yield return type;
                }
            }
        }

        /// <summary>
        /// Enumerate all types in the specified domain.
        /// </summary>
        /// <param name="includeNonPublic">Should non-public types be included in the search</param>
        /// <param name="searchDomain">The domain to search or null if the active domain should be used</param>
        /// <returns>Enumerable of matching results</returns>
        public static IEnumerable<ScriptType> EnumerateAllTypes(bool includeNonPublic = true, ScriptDomain searchDomain = null)
        {
            // Try to resolve domain
            if (ResolveSearchDomain(ref searchDomain) == false)
                yield break;

            // Search all assemblies
            foreach (ScriptAssembly assembly in searchDomain.Assemblies)
            {
                // Try to find type
                foreach (ScriptType type in assembly.EnumerateAllTypes(includeNonPublic))
                {
                    // Return the type
                    yield return type;
                }
            }
        }

        /// <summary>
        /// Enumerate all types in the specified domain that inherit from <see cref="UnityEngine.Object"/>.
        /// </summary>
        /// <param name="includeNonPublic">Should non-public types be include in the search</param>
        /// <param name="searchDomain">The domain to search or null if the active domain should be used</param>
        /// <returns>Enumerable of matching results</returns>
        public static IEnumerable<ScriptType> EnumerateAllUnityTypes(bool includeNonPublic = true, ScriptDomain searchDomain = null)
        {
            // Try to resolve domain
            if (ResolveSearchDomain(ref searchDomain) == false)
                yield break;

            // Search all assemblies
            foreach (ScriptAssembly assembly in searchDomain.Assemblies)
            {
                // Try to find type
                foreach (ScriptType type in assembly.EnumerateAllUnityTypes(includeNonPublic))
                {
                    // Return the type
                    yield return type;
                }
            }
        }

        /// <summary>
        /// Enumerate all types in the specified domain that inherit from <see cref="UnityEngine.MonoBehaviour"/>.
        /// </summary>
        /// <param name="includeNonPublic">Should non-public types be included in the search</param>
        /// <param name="searchDomain">The domain to search or null if the active domain should be used</param>
        /// <returns>Enumerable of matching results</returns>
        public static IEnumerable<ScriptType> EnumerateAllMonoBehaviourTypes(bool includeNonPublic = true, ScriptDomain searchDomain = null)
        {
            // Try to resolve domain
            if (ResolveSearchDomain(ref searchDomain) == false)
                yield break;

            // Search all assemblies
            foreach (ScriptAssembly assembly in searchDomain.Assemblies)
            {
                // Try to find type
                foreach (ScriptType type in assembly.EnumerateAllMonoBehaviourTypes(includeNonPublic))
                {
                    // Return the type
                    yield return type;
                }
            }
        }

        /// <summary>
        /// Enumerate all types in the specified domain that inherit from <see cref="UnityEngine.ScriptableObject"/>.
        /// </summary>
        /// <param name="includeNonPublic">Should non-public types be include in the search</param>
        /// <param name="searchDomain">The domain to search or null if the active domain should be used</param>
        /// <returns>Enumerable of matching results</returns>
        public static IEnumerable<ScriptType> EnumerateAllScriptableObjectTypes(bool includeNonPublic = true, ScriptDomain searchDomain = null)
        {
            // Try to resolve domain
            if (ResolveSearchDomain(ref searchDomain) == false)
                yield break;

            // Search all assemblies
            foreach (ScriptAssembly assembly in searchDomain.Assemblies)
            {
                // Try to find type
                foreach (ScriptType type in assembly.EnumerateAllScriptableObjectTypes(includeNonPublic))
                {
                    // Return the type
                    yield return type;
                }
            }
        }

        private static bool ResolveSearchDomain(ref ScriptDomain searchDomain)
        {
            // Check for specified domain
            if (searchDomain == null)
            {
                // Get the active domain
                searchDomain = ScriptDomain.Active;

                // No domain found to search
                if (searchDomain == null)
                    return false;
            }
            return true;
        }
        #endregion

        public static ScriptType CreateScriptType(ScriptAssembly assembly, ScriptType parent, Type systemType)
        {
            return CreateScriptType<Implementation.ScriptTypeImpl>(assembly, parent, systemType);
        }

        public static T CreateScriptType<T>(ScriptAssembly assembly, ScriptType parent, Type systemType) where T : ScriptType, new()
        {
            // Create the script type instance
            T type = new T();

            // Build nested types
            Type[] nested = systemType.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic);

            ScriptType[] nestedTypes = new ScriptType[nested.Length];

            for(int i = 0; i < nested.Length; i++)
            {
                nestedTypes[i] = CreateScriptType<T>(assembly, type, nested[i]);
            }
            
            // Create the type instance
            type.ConstructInstance(assembly, parent, nestedTypes, systemType);

            return type;
        }
    }
}
