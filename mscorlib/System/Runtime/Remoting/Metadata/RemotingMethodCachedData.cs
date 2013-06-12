namespace System.Runtime.Remoting.Metadata
{
    using System;
    using System.Reflection;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Messaging;
    using System.Security;

    internal class RemotingMethodCachedData : RemotingCachedData
    {
        private int[] _inRefArgMap;
        private int[] _marshalRequestMap;
        private int[] _marshalResponseMap;
        private string _methodName;
        private int[] _nonRefOutArgMap;
        private int[] _outOnlyArgMap;
        private int[] _outRefArgMap;
        private ParameterInfo[] _parameters;
        private Type _returnType;
        private string _typeAndAssemblyName;
        private MethodCacheFlags flags;

        internal RemotingMethodCachedData(RuntimeConstructorInfo ri) : base(ri)
        {
        }

        internal RemotingMethodCachedData(RuntimeMethodInfo ri) : base(ri)
        {
        }

        private void GetArgMaps()
        {
            lock (this)
            {
                if (this._inRefArgMap == null)
                {
                    int[] inRefArgMap = null;
                    int[] outRefArgMap = null;
                    int[] outOnlyArgMap = null;
                    int[] nonRefOutArgMap = null;
                    int[] marshalRequestMap = null;
                    int[] marshalResponseMap = null;
                    ArgMapper.GetParameterMaps(this.Parameters, out inRefArgMap, out outRefArgMap, out outOnlyArgMap, out nonRefOutArgMap, out marshalRequestMap, out marshalResponseMap);
                    this._inRefArgMap = inRefArgMap;
                    this._outRefArgMap = outRefArgMap;
                    this._outOnlyArgMap = outOnlyArgMap;
                    this._nonRefOutArgMap = nonRefOutArgMap;
                    this._marshalRequestMap = marshalRequestMap;
                    this._marshalResponseMap = marshalResponseMap;
                }
            }
        }

        internal bool IsOneWayMethod()
        {
            if ((this.flags & MethodCacheFlags.CheckedOneWay) != MethodCacheFlags.None)
            {
                return ((this.flags & MethodCacheFlags.IsOneWay) != MethodCacheFlags.None);
            }
            MethodCacheFlags checkedOneWay = MethodCacheFlags.CheckedOneWay;
            object[] customAttributes = ((ICustomAttributeProvider) base.RI).GetCustomAttributes(typeof(OneWayAttribute), true);
            if ((customAttributes != null) && (customAttributes.Length > 0))
            {
                checkedOneWay |= MethodCacheFlags.IsOneWay;
            }
            this.flags |= checkedOneWay;
            return ((checkedOneWay & MethodCacheFlags.IsOneWay) != MethodCacheFlags.None);
        }

        internal bool IsOverloaded()
        {
            if ((this.flags & MethodCacheFlags.CheckedOverloaded) == MethodCacheFlags.None)
            {
                MethodCacheFlags checkedOverloaded = MethodCacheFlags.CheckedOverloaded;
                MethodBase rI = (MethodBase) base.RI;
                RuntimeMethodInfo info = null;
                RuntimeConstructorInfo info2 = null;
                info = rI as RuntimeMethodInfo;
                if (info != null)
                {
                    if (info.IsOverloaded)
                    {
                        checkedOverloaded |= MethodCacheFlags.IsOverloaded;
                    }
                }
                else
                {
                    info2 = rI as RuntimeConstructorInfo;
                    if (info2 == null)
                    {
                        throw new NotSupportedException(Environment.GetResourceString("InvalidOperation_Method"));
                    }
                    if (info2.IsOverloaded)
                    {
                        checkedOverloaded |= MethodCacheFlags.IsOverloaded;
                    }
                }
                this.flags |= checkedOverloaded;
            }
            return ((this.flags & MethodCacheFlags.IsOverloaded) != MethodCacheFlags.None);
        }

        [SecurityCritical]
        private void UpdateNames()
        {
            MethodBase rI = (MethodBase) base.RI;
            this._methodName = rI.Name;
            if (rI.DeclaringType != null)
            {
                this._typeAndAssemblyName = RemotingServices.GetDefaultQualifiedTypeName((RuntimeType) rI.DeclaringType);
            }
        }

        internal int[] MarshalRequestArgMap
        {
            get
            {
                if (this._marshalRequestMap == null)
                {
                    this.GetArgMaps();
                }
                return this._marshalRequestMap;
            }
        }

        internal int[] MarshalResponseArgMap
        {
            get
            {
                if (this._marshalResponseMap == null)
                {
                    this.GetArgMaps();
                }
                return this._marshalResponseMap;
            }
        }

        internal string MethodName
        {
            [SecurityCritical]
            get
            {
                if (this._methodName == null)
                {
                    this.UpdateNames();
                }
                return this._methodName;
            }
        }

        internal int[] NonRefOutArgMap
        {
            get
            {
                if (this._nonRefOutArgMap == null)
                {
                    this.GetArgMaps();
                }
                return this._nonRefOutArgMap;
            }
        }

        internal int[] OutOnlyArgMap
        {
            get
            {
                if (this._outOnlyArgMap == null)
                {
                    this.GetArgMaps();
                }
                return this._outOnlyArgMap;
            }
        }

        internal int[] OutRefArgMap
        {
            get
            {
                if (this._outRefArgMap == null)
                {
                    this.GetArgMaps();
                }
                return this._outRefArgMap;
            }
        }

        internal ParameterInfo[] Parameters
        {
            get
            {
                if (this._parameters == null)
                {
                    this._parameters = ((MethodBase) base.RI).GetParameters();
                }
                return this._parameters;
            }
        }

        internal Type ReturnType
        {
            get
            {
                if ((this.flags & MethodCacheFlags.CheckedForReturnType) == MethodCacheFlags.None)
                {
                    MethodInfo rI = base.RI as MethodInfo;
                    if (rI != null)
                    {
                        Type returnType = rI.ReturnType;
                        if (returnType != typeof(void))
                        {
                            this._returnType = returnType;
                        }
                    }
                    this.flags |= MethodCacheFlags.CheckedForReturnType;
                }
                return this._returnType;
            }
        }

        internal string TypeAndAssemblyName
        {
            [SecurityCritical]
            get
            {
                if (this._typeAndAssemblyName == null)
                {
                    this.UpdateNames();
                }
                return this._typeAndAssemblyName;
            }
        }

        [Serializable, Flags]
        private enum MethodCacheFlags
        {
            CheckedForAsync = 0x10,
            CheckedForReturnType = 0x20,
            CheckedOneWay = 1,
            CheckedOverloaded = 4,
            IsOneWay = 2,
            IsOverloaded = 8,
            None = 0
        }
    }
}

