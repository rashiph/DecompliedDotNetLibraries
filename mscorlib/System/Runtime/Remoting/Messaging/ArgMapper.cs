namespace System.Runtime.Remoting.Messaging
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Metadata;
    using System.Security;

    internal class ArgMapper
    {
        private int[] _map;
        private RemotingMethodCachedData _methodCachedData;
        private IMethodMessage _mm;

        [SecurityCritical]
        internal ArgMapper(MethodBase mb, bool fOut)
        {
            this._methodCachedData = InternalRemotingServices.GetReflectionCachedData(mb);
            if (fOut)
            {
                this._map = this._methodCachedData.MarshalResponseArgMap;
            }
            else
            {
                this._map = this._methodCachedData.MarshalRequestArgMap;
            }
        }

        [SecurityCritical]
        internal ArgMapper(IMethodMessage mm, bool fOut)
        {
            this._mm = mm;
            MethodBase methodBase = this._mm.MethodBase;
            this._methodCachedData = InternalRemotingServices.GetReflectionCachedData(methodBase);
            if (fOut)
            {
                this._map = this._methodCachedData.MarshalResponseArgMap;
            }
            else
            {
                this._map = this._methodCachedData.MarshalRequestArgMap;
            }
        }

        internal static object[] ExpandAsyncEndArgsToSyncArgs(RemotingMethodCachedData syncMethod, object[] asyncEndArgs)
        {
            object[] objArray = new object[syncMethod.Parameters.Length];
            int[] outRefArgMap = syncMethod.OutRefArgMap;
            for (int i = 0; i < outRefArgMap.Length; i++)
            {
                objArray[outRefArgMap[i]] = asyncEndArgs[i];
            }
            return objArray;
        }

        [SecurityCritical]
        internal object GetArg(int argNum)
        {
            if (((this._map == null) || (argNum < 0)) || (argNum >= this._map.Length))
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_InternalState"));
            }
            return this._mm.GetArg(this._map[argNum]);
        }

        [SecurityCritical]
        internal string GetArgName(int argNum)
        {
            if (((this._map == null) || (argNum < 0)) || (argNum >= this._map.Length))
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_InternalState"));
            }
            return this._mm.GetArgName(this._map[argNum]);
        }

        internal static void GetParameterMaps(ParameterInfo[] parameters, out int[] inRefArgMap, out int[] outRefArgMap, out int[] outOnlyArgMap, out int[] nonRefOutArgMap, out int[] marshalRequestMap, out int[] marshalResponseMap)
        {
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            int num5 = 0;
            int length = 0;
            int num7 = 0;
            int[] sourceArray = new int[parameters.Length];
            int[] numArray2 = new int[parameters.Length];
            int index = 0;
            foreach (ParameterInfo info in parameters)
            {
                bool isIn = info.IsIn;
                bool isOut = info.IsOut;
                bool isByRef = info.ParameterType.IsByRef;
                if (!isByRef)
                {
                    num2++;
                    if (isOut)
                    {
                        num5++;
                    }
                }
                else if (isOut)
                {
                    num3++;
                    num4++;
                }
                else
                {
                    num2++;
                    num3++;
                }
                bool flag4 = false;
                bool flag5 = false;
                if (isByRef)
                {
                    if (isIn == isOut)
                    {
                        flag4 = true;
                        flag5 = true;
                    }
                    else
                    {
                        flag4 = isIn;
                        flag5 = isOut;
                    }
                }
                else
                {
                    flag4 = true;
                    flag5 = isOut;
                }
                if (flag4)
                {
                    sourceArray[length++] = index;
                }
                if (flag5)
                {
                    numArray2[num7++] = index;
                }
                index++;
            }
            inRefArgMap = new int[num2];
            outRefArgMap = new int[num3];
            outOnlyArgMap = new int[num4];
            nonRefOutArgMap = new int[num5];
            num2 = 0;
            num3 = 0;
            num4 = 0;
            num5 = 0;
            for (index = 0; index < parameters.Length; index++)
            {
                ParameterInfo info2 = parameters[index];
                bool flag6 = info2.IsOut;
                if (!info2.ParameterType.IsByRef)
                {
                    inRefArgMap[num2++] = index;
                    if (flag6)
                    {
                        nonRefOutArgMap[num5++] = index;
                    }
                }
                else if (flag6)
                {
                    outRefArgMap[num3++] = index;
                    outOnlyArgMap[num4++] = index;
                }
                else
                {
                    inRefArgMap[num2++] = index;
                    outRefArgMap[num3++] = index;
                }
            }
            marshalRequestMap = new int[length];
            Array.Copy(sourceArray, marshalRequestMap, length);
            marshalResponseMap = new int[num7];
            Array.Copy(numArray2, marshalResponseMap, num7);
        }

        internal int ArgCount
        {
            get
            {
                if (this._map == null)
                {
                    return 0;
                }
                return this._map.Length;
            }
        }

        internal string[] ArgNames
        {
            get
            {
                string[] strArray = null;
                if (this._map != null)
                {
                    ParameterInfo[] parameters = this._methodCachedData.Parameters;
                    strArray = new string[this._map.Length];
                    for (int i = 0; i < this._map.Length; i++)
                    {
                        strArray[i] = parameters[this._map[i]].Name;
                    }
                }
                return strArray;
            }
        }

        internal object[] Args
        {
            [SecurityCritical]
            get
            {
                if (this._map == null)
                {
                    return null;
                }
                object[] objArray = new object[this._map.Length];
                for (int i = 0; i < this._map.Length; i++)
                {
                    objArray[i] = this._mm.GetArg(this._map[i]);
                }
                return objArray;
            }
        }

        internal Type[] ArgTypes
        {
            get
            {
                Type[] typeArray = null;
                if (this._map != null)
                {
                    ParameterInfo[] parameters = this._methodCachedData.Parameters;
                    typeArray = new Type[this._map.Length];
                    for (int i = 0; i < this._map.Length; i++)
                    {
                        typeArray[i] = parameters[this._map[i]].ParameterType;
                    }
                }
                return typeArray;
            }
        }

        internal int[] Map
        {
            get
            {
                return this._map;
            }
        }
    }
}

