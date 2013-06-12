namespace System.Runtime.Remoting
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [Serializable]
    internal class TypeInfo : IRemotingTypeInfo
    {
        private string[] interfacesImplemented;
        private string[] serverHierarchy;
        private string serverType;

        [SecurityCritical]
        internal TypeInfo(RuntimeType typeOfObj)
        {
            this.ServerType = GetQualifiedTypeName(typeOfObj);
            RuntimeType baseType = (RuntimeType) typeOfObj.BaseType;
            int num = 0;
            while ((baseType != typeof(MarshalByRefObject)) && (baseType != null))
            {
                baseType = (RuntimeType) baseType.BaseType;
                num++;
            }
            string[] strArray = null;
            if (num > 0)
            {
                strArray = new string[num];
                baseType = (RuntimeType) typeOfObj.BaseType;
                for (int i = 0; i < num; i++)
                {
                    strArray[i] = GetQualifiedTypeName(baseType);
                    baseType = (RuntimeType) baseType.BaseType;
                }
            }
            this.ServerHierarchy = strArray;
            Type[] interfaces = typeOfObj.GetInterfaces();
            string[] strArray2 = null;
            bool isInterface = typeOfObj.IsInterface;
            if ((interfaces.Length > 0) || isInterface)
            {
                strArray2 = new string[interfaces.Length + (isInterface ? 1 : 0)];
                for (int j = 0; j < interfaces.Length; j++)
                {
                    strArray2[j] = GetQualifiedTypeName((RuntimeType) interfaces[j]);
                }
                if (isInterface)
                {
                    strArray2[strArray2.Length - 1] = GetQualifiedTypeName(typeOfObj);
                }
            }
            this.InterfacesImplemented = strArray2;
        }

        [SecurityCritical]
        public virtual bool CanCastTo(Type castType, object o)
        {
            if (null != castType)
            {
                if ((castType == typeof(MarshalByRefObject)) || (castType == typeof(object)))
                {
                    return true;
                }
                if (castType.IsInterface)
                {
                    return ((this.interfacesImplemented != null) && this.CanCastTo(castType, this.InterfacesImplemented));
                }
                if (castType.IsMarshalByRef)
                {
                    if (this.CompareTypes(castType, this.serverType))
                    {
                        return true;
                    }
                    if ((this.serverHierarchy != null) && this.CanCastTo(castType, this.ServerHierarchy))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        [SecurityCritical]
        private bool CanCastTo(Type castType, string[] types)
        {
            if (null != castType)
            {
                for (int i = 0; i < types.Length; i++)
                {
                    if (this.CompareTypes(castType, types[i]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        [SecurityCritical]
        private bool CompareTypes(Type type1, string type2)
        {
            Type typeFromQualifiedTypeName = RemotingServices.InternalGetTypeFromQualifiedTypeName(type2);
            return (type1 == typeFromQualifiedTypeName);
        }

        [SecurityCritical]
        internal static string GetQualifiedTypeName(RuntimeType type)
        {
            if (type == null)
            {
                return null;
            }
            return RemotingServices.GetDefaultQualifiedTypeName(type);
        }

        internal static bool ParseTypeAndAssembly(string typeAndAssembly, out string typeName, out string assemName)
        {
            if (typeAndAssembly == null)
            {
                typeName = null;
                assemName = null;
                return false;
            }
            int index = typeAndAssembly.IndexOf(',');
            if (index == -1)
            {
                typeName = typeAndAssembly;
                assemName = null;
                return true;
            }
            typeName = typeAndAssembly.Substring(0, index);
            assemName = typeAndAssembly.Substring(index + 1).Trim();
            return true;
        }

        private string[] InterfacesImplemented
        {
            get
            {
                return this.interfacesImplemented;
            }
            set
            {
                this.interfacesImplemented = value;
            }
        }

        private string[] ServerHierarchy
        {
            get
            {
                return this.serverHierarchy;
            }
            set
            {
                this.serverHierarchy = value;
            }
        }

        internal string ServerType
        {
            get
            {
                return this.serverType;
            }
            set
            {
                this.serverType = value;
            }
        }

        public virtual string TypeName
        {
            [SecurityCritical]
            get
            {
                return this.serverType;
            }
            [SecurityCritical]
            set
            {
                this.serverType = value;
            }
        }
    }
}

