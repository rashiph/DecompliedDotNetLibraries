namespace System.Xaml.Schema
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Xaml;

    public class XamlTypeInvoker
    {
        private Dictionary<XamlType, MethodInfo> _addMethods;
        [SecurityCritical]
        private Action<object> _constructorDelegate;
        [SecurityCritical]
        private ThreeValuedBool _isInSystemXaml;
        [SecurityCritical]
        private ThreeValuedBool _isPublic;
        private XamlType _xamlType;
        private static object[] s_emptyObjectArray = new object[0];
        private static XamlTypeInvoker s_Unknown;

        protected XamlTypeInvoker()
        {
        }

        public XamlTypeInvoker(XamlType type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            this._xamlType = type;
        }

        public virtual void AddToCollection(object instance, object item)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            IList list = instance as IList;
            if (list != null)
            {
                list.Add(item);
            }
            else
            {
                XamlType xamlType;
                this.ThrowIfUnknown();
                if (!this._xamlType.IsCollection)
                {
                    throw new NotSupportedException(System.Xaml.SR.Get("OnlySupportedOnCollections"));
                }
                if (item != null)
                {
                    xamlType = this._xamlType.SchemaContext.GetXamlType(item.GetType());
                }
                else
                {
                    xamlType = this._xamlType.ItemType;
                }
                MethodInfo addMethod = this.GetAddMethod(xamlType);
                if (addMethod == null)
                {
                    throw new XamlSchemaException(System.Xaml.SR.Get("NoAddMethodFound", new object[] { this._xamlType, xamlType }));
                }
                SafeReflectionInvoker.InvokeMethod(addMethod, instance, new object[] { item });
            }
        }

        public virtual void AddToDictionary(object instance, object key, object item)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            IDictionary dictionary = instance as IDictionary;
            if (dictionary != null)
            {
                dictionary.Add(key, item);
            }
            else
            {
                XamlType xamlType;
                this.ThrowIfUnknown();
                if (!this._xamlType.IsDictionary)
                {
                    throw new NotSupportedException(System.Xaml.SR.Get("OnlySupportedOnDictionaries"));
                }
                if (item != null)
                {
                    xamlType = this._xamlType.SchemaContext.GetXamlType(item.GetType());
                }
                else
                {
                    xamlType = this._xamlType.ItemType;
                }
                MethodInfo addMethod = this.GetAddMethod(xamlType);
                if (addMethod == null)
                {
                    throw new XamlSchemaException(System.Xaml.SR.Get("NoAddMethodFound", new object[] { this._xamlType, xamlType }));
                }
                SafeReflectionInvoker.InvokeMethod(addMethod, instance, new object[] { key, item });
            }
        }

        public virtual object CreateInstance(object[] arguments)
        {
            this.ThrowIfUnknown();
            if (!this._xamlType.UnderlyingType.IsValueType && ((arguments == null) || (arguments.Length == 0)))
            {
                object obj2 = DefaultCtorXamlActivator.CreateInstance(this);
                if (obj2 != null)
                {
                    return obj2;
                }
            }
            return this.CreateInstanceWithActivator(this._xamlType.UnderlyingType, arguments);
        }

        [SecuritySafeCritical]
        private object CreateInstanceWithActivator(Type type, object[] arguments)
        {
            return SafeReflectionInvoker.CreateInstance(type, arguments);
        }

        public virtual MethodInfo GetAddMethod(XamlType contentType)
        {
            if (contentType == null)
            {
                throw new ArgumentNullException("contentType");
            }
            if (!this.IsUnknown && (this._xamlType.ItemType != null))
            {
                MethodInfo addMethod;
                if ((contentType == this._xamlType.ItemType) || ((this._xamlType.AllowedContentTypes.Count == 1) && contentType.CanAssignTo(this._xamlType.ItemType)))
                {
                    return this._xamlType.AddMethod;
                }
                if (!this._xamlType.IsCollection)
                {
                    return null;
                }
                if (this._addMethods == null)
                {
                    Dictionary<XamlType, MethodInfo> dictionary = new Dictionary<XamlType, MethodInfo>();
                    dictionary.Add(this._xamlType.ItemType, this._xamlType.AddMethod);
                    foreach (XamlType type in this._xamlType.AllowedContentTypes)
                    {
                        addMethod = CollectionReflector.GetAddMethod(this._xamlType.UnderlyingType, type.UnderlyingType);
                        if (addMethod != null)
                        {
                            dictionary.Add(type, addMethod);
                        }
                    }
                    this._addMethods = dictionary;
                }
                if (this._addMethods.TryGetValue(contentType, out addMethod))
                {
                    return addMethod;
                }
                foreach (KeyValuePair<XamlType, MethodInfo> pair in this._addMethods)
                {
                    if (contentType.CanAssignTo(pair.Key))
                    {
                        return pair.Value;
                    }
                }
            }
            return null;
        }

        public virtual MethodInfo GetEnumeratorMethod()
        {
            return this._xamlType.GetEnumeratorMethod;
        }

        public virtual IEnumerator GetItems(object instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            IEnumerable enumerable = instance as IEnumerable;
            if (enumerable != null)
            {
                return enumerable.GetEnumerator();
            }
            this.ThrowIfUnknown();
            if (!this._xamlType.IsCollection && !this._xamlType.IsDictionary)
            {
                throw new NotSupportedException(System.Xaml.SR.Get("OnlySupportedOnCollectionsAndDictionaries"));
            }
            return (IEnumerator) SafeReflectionInvoker.InvokeMethod(this.GetEnumeratorMethod(), instance, s_emptyObjectArray);
        }

        private void ThrowIfUnknown()
        {
            if (this.IsUnknown)
            {
                throw new NotSupportedException(System.Xaml.SR.Get("NotSupportedOnUnknownType"));
            }
        }

        internal MethodInfo EnumeratorMethod { get; set; }

        private bool IsInSystemXaml
        {
            [SecuritySafeCritical]
            get
            {
                if (this._isInSystemXaml == ThreeValuedBool.NotSet)
                {
                    bool flag = SafeReflectionInvoker.IsInSystemXaml(this._xamlType.UnderlyingType.UnderlyingSystemType);
                    this._isInSystemXaml = flag ? ThreeValuedBool.True : ThreeValuedBool.False;
                }
                return (this._isInSystemXaml == ThreeValuedBool.True);
            }
        }

        private bool IsPublic
        {
            [SecuritySafeCritical]
            get
            {
                if (this._isPublic == ThreeValuedBool.NotSet)
                {
                    Type underlyingSystemType = this._xamlType.UnderlyingType.UnderlyingSystemType;
                    this._isPublic = underlyingSystemType.IsVisible ? ThreeValuedBool.True : ThreeValuedBool.False;
                }
                return (this._isPublic == ThreeValuedBool.True);
            }
        }

        private bool IsUnknown
        {
            get
            {
                if (this._xamlType != null)
                {
                    return (this._xamlType.UnderlyingType == null);
                }
                return true;
            }
        }

        public EventHandler<XamlSetMarkupExtensionEventArgs> SetMarkupExtensionHandler
        {
            get
            {
                if (this._xamlType == null)
                {
                    return null;
                }
                return this._xamlType.SetMarkupExtensionHandler;
            }
        }

        public EventHandler<XamlSetTypeConverterEventArgs> SetTypeConverterHandler
        {
            get
            {
                if (this._xamlType == null)
                {
                    return null;
                }
                return this._xamlType.SetTypeConverterHandler;
            }
        }

        public static XamlTypeInvoker UnknownInvoker
        {
            get
            {
                if (s_Unknown == null)
                {
                    s_Unknown = new XamlTypeInvoker();
                }
                return s_Unknown;
            }
        }

        private static class DefaultCtorXamlActivator
        {
            private static ConstructorInfo s_actionCtor = typeof(Action<object>).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) });
            private static ThreeValuedBool s_securityFailureWithCtorDelegate;

            [SecuritySafeCritical]
            private static object CallCtorDelegate(XamlTypeInvoker type)
            {
                object uninitializedObject = FormatterServices.GetUninitializedObject(type._xamlType.UnderlyingType);
                InvokeDelegate(type._constructorDelegate, uninitializedObject);
                return uninitializedObject;
            }

            public static object CreateInstance(XamlTypeInvoker type)
            {
                if (!EnsureConstructorDelegate(type))
                {
                    return null;
                }
                return CallCtorDelegate(type);
            }

            [SecuritySafeCritical]
            private static bool EnsureConstructorDelegate(XamlTypeInvoker type)
            {
                if (type._constructorDelegate != null)
                {
                    return true;
                }
                if (!type.IsPublic)
                {
                    return false;
                }
                if (s_securityFailureWithCtorDelegate == ThreeValuedBool.NotSet)
                {
                    s_securityFailureWithCtorDelegate = !AppDomain.CurrentDomain.PermissionSet.IsUnrestricted() ? ThreeValuedBool.True : ThreeValuedBool.False;
                }
                if (s_securityFailureWithCtorDelegate == ThreeValuedBool.True)
                {
                    return false;
                }
                try
                {
                    Type underlyingSystemType = type._xamlType.UnderlyingType.UnderlyingSystemType;
                    ConstructorInfo constructor = underlyingSystemType.GetConstructor(Type.EmptyTypes);
                    if (constructor == null)
                    {
                        throw new MissingMethodException(System.Xaml.SR.Get("NoDefaultConstructor", new object[] { underlyingSystemType.FullName }));
                    }
                    if ((constructor.IsSecurityCritical && !constructor.IsSecuritySafeCritical) || (((constructor.Attributes & MethodAttributes.HasSecurity) == MethodAttributes.HasSecurity) || ((underlyingSystemType.Attributes & TypeAttributes.HasSecurity) == TypeAttributes.HasSecurity)))
                    {
                        type._isPublic = ThreeValuedBool.False;
                        return false;
                    }
                    IntPtr functionPointer = constructor.MethodHandle.GetFunctionPointer();
                    object[] parameters = new object[2];
                    parameters[1] = functionPointer;
                    type._constructorDelegate = (Action<object>) s_actionCtor.Invoke(parameters);
                    return true;
                }
                catch (SecurityException)
                {
                    s_securityFailureWithCtorDelegate = ThreeValuedBool.True;
                    return false;
                }
            }

            private static void InvokeDelegate(Action<object> action, object argument)
            {
                action(argument);
            }
        }

        private class UnknownTypeInvoker : XamlTypeInvoker
        {
            public override void AddToCollection(object instance, object item)
            {
                throw new NotSupportedException(System.Xaml.SR.Get("NotSupportedOnUnknownType"));
            }

            public override void AddToDictionary(object instance, object key, object item)
            {
                throw new NotSupportedException(System.Xaml.SR.Get("NotSupportedOnUnknownType"));
            }

            public override object CreateInstance(object[] arguments)
            {
                throw new NotSupportedException(System.Xaml.SR.Get("NotSupportedOnUnknownType"));
            }

            public override IEnumerator GetItems(object instance)
            {
                throw new NotSupportedException(System.Xaml.SR.Get("NotSupportedOnUnknownType"));
            }
        }
    }
}

