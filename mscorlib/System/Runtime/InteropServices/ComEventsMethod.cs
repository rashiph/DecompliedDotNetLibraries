namespace System.Runtime.InteropServices
{
    using System;
    using System.Reflection;

    internal class ComEventsMethod
    {
        private DelegateWrapper[] _delegateWrappers = null;
        private int _dispid;
        private ComEventsMethod _next;

        internal ComEventsMethod(int dispid)
        {
            this._dispid = dispid;
        }

        internal static ComEventsMethod Add(ComEventsMethod methods, ComEventsMethod method)
        {
            method._next = methods;
            return method;
        }

        internal void AddDelegate(Delegate d)
        {
            int index = 0;
            if (this._delegateWrappers != null)
            {
                index = this._delegateWrappers.Length;
            }
            for (int i = 0; i < index; i++)
            {
                if (this._delegateWrappers[i].Delegate.GetType() == d.GetType())
                {
                    this._delegateWrappers[i].Delegate = Delegate.Combine(this._delegateWrappers[i].Delegate, d);
                    return;
                }
            }
            DelegateWrapper[] array = new DelegateWrapper[index + 1];
            if (index > 0)
            {
                this._delegateWrappers.CopyTo(array, 0);
            }
            array[index] = new DelegateWrapper(d);
            this._delegateWrappers = array;
        }

        internal static ComEventsMethod Find(ComEventsMethod methods, int dispid)
        {
            while ((methods != null) && (methods._dispid != dispid))
            {
                methods = methods._next;
            }
            return methods;
        }

        internal object Invoke(object[] args)
        {
            object obj2 = null;
            foreach (DelegateWrapper wrapper in this._delegateWrappers)
            {
                if ((wrapper != null) && (wrapper.Delegate != null))
                {
                    obj2 = wrapper.Invoke(args);
                }
            }
            return obj2;
        }

        internal static ComEventsMethod Remove(ComEventsMethod methods, ComEventsMethod method)
        {
            if (methods == method)
            {
                methods = methods._next;
                return methods;
            }
            ComEventsMethod method2 = methods;
            while ((method2 != null) && (method2._next != method))
            {
                method2 = method2._next;
            }
            if (method2 != null)
            {
                method2._next = method._next;
            }
            return methods;
        }

        internal void RemoveDelegate(Delegate d)
        {
            int length = this._delegateWrappers.Length;
            int index = -1;
            for (int i = 0; i < length; i++)
            {
                if (this._delegateWrappers[i].Delegate.GetType() == d.GetType())
                {
                    index = i;
                    break;
                }
            }
            if (index >= 0)
            {
                Delegate delegate2 = Delegate.Remove(this._delegateWrappers[index].Delegate, d);
                if (delegate2 != null)
                {
                    this._delegateWrappers[index].Delegate = delegate2;
                }
                else if (length == 1)
                {
                    this._delegateWrappers = null;
                }
                else
                {
                    DelegateWrapper[] wrapperArray = new DelegateWrapper[length - 1];
                    int num4 = 0;
                    while (num4 < index)
                    {
                        wrapperArray[num4] = this._delegateWrappers[num4];
                        num4++;
                    }
                    while (num4 < (length - 1))
                    {
                        wrapperArray[num4] = this._delegateWrappers[num4 + 1];
                        num4++;
                    }
                    this._delegateWrappers = wrapperArray;
                }
            }
        }

        internal int DispId
        {
            get
            {
                return this._dispid;
            }
        }

        internal bool Empty
        {
            get
            {
                if (this._delegateWrappers != null)
                {
                    return (this._delegateWrappers.Length == 0);
                }
                return true;
            }
        }

        internal class DelegateWrapper
        {
            private Type[] _cachedTargetTypes;
            private System.Delegate _d;
            private int _expectedParamsCount;
            private bool _once;

            public DelegateWrapper(System.Delegate d)
            {
                this._d = d;
            }

            public object Invoke(object[] args)
            {
                if (this._d == null)
                {
                    return null;
                }
                if (!this._once)
                {
                    this.PreProcessSignature();
                    this._once = true;
                }
                if ((this._cachedTargetTypes != null) && (this._expectedParamsCount == args.Length))
                {
                    for (int i = 0; i < this._expectedParamsCount; i++)
                    {
                        if (this._cachedTargetTypes[i] != null)
                        {
                            args[i] = Enum.ToObject(this._cachedTargetTypes[i], args[i]);
                        }
                    }
                }
                return this._d.DynamicInvoke(args);
            }

            private void PreProcessSignature()
            {
                ParameterInfo[] parameters = this._d.Method.GetParameters();
                this._expectedParamsCount = parameters.Length;
                Type[] typeArray = new Type[this._expectedParamsCount];
                bool flag = false;
                for (int i = 0; i < this._expectedParamsCount; i++)
                {
                    ParameterInfo info = parameters[i];
                    if ((info.ParameterType.IsByRef && info.ParameterType.HasElementType) && info.ParameterType.GetElementType().IsEnum)
                    {
                        flag = true;
                        typeArray[i] = info.ParameterType.GetElementType();
                    }
                }
                if (flag)
                {
                    this._cachedTargetTypes = typeArray;
                }
            }

            public System.Delegate Delegate
            {
                get
                {
                    return this._d;
                }
                set
                {
                    this._d = value;
                }
            }
        }
    }
}

