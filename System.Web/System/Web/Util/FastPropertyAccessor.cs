namespace System.Web.Util
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Web;
    using System.Web.UI;

    internal class FastPropertyAccessor
    {
        private const BindingFlags _declaredFlags = (BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        private ModuleBuilder _dynamicModule;
        private static MethodInfo _getPropertyMethod = typeof(IWebPropertyAccessor).GetMethod("GetProperty");
        private static Type[] _getPropertyParameterList = new Type[] { typeof(object) };
        private static Type[] _interfacesToImplement = new Type[] { typeof(IWebPropertyAccessor) };
        private static MethodInfo _setPropertyMethod = typeof(IWebPropertyAccessor).GetMethod("SetProperty");
        private static Type[] _setPropertyParameterList = new Type[] { typeof(object), typeof(object) };
        private static int _uniqueId;
        private static Hashtable s_accessorCache;
        private static FastPropertyAccessor s_accessorGenerator;
        private static object s_lockObject = new object();

        internal static object GetProperty(object target, string propName, bool inDesigner)
        {
            Type type;
            if (!inDesigner)
            {
                return GetPropertyAccessor(target.GetType(), propName).GetProperty(target);
            }
            FieldInfo fieldInfo = null;
            PropertyInfo propInfo = null;
            GetPropertyInfo(target.GetType(), propName, out propInfo, out fieldInfo, out type);
            if (propInfo != null)
            {
                return propInfo.GetValue(target, null);
            }
            if (fieldInfo == null)
            {
                throw new ArgumentException();
            }
            return fieldInfo.GetValue(target);
        }

        private static IWebPropertyAccessor GetPropertyAccessor(Type type, string propertyName)
        {
            if ((s_accessorGenerator == null) || (s_accessorCache == null))
            {
                lock (s_lockObject)
                {
                    if ((s_accessorGenerator == null) || (s_accessorCache == null))
                    {
                        s_accessorGenerator = new FastPropertyAccessor();
                        s_accessorCache = new Hashtable();
                    }
                }
            }
            int num = HashCodeCombiner.CombineHashCodes(type.GetHashCode(), propertyName.GetHashCode());
            IWebPropertyAccessor accessor = (IWebPropertyAccessor) s_accessorCache[num];
            if (accessor == null)
            {
                Type type2;
                FieldInfo fieldInfo = null;
                PropertyInfo propInfo = null;
                GetPropertyInfo(type, propertyName, out propInfo, out fieldInfo, out type2);
                int num2 = 0;
                if (type2 != type)
                {
                    num2 = HashCodeCombiner.CombineHashCodes(type2.GetHashCode(), propertyName.GetHashCode());
                    accessor = (IWebPropertyAccessor) s_accessorCache[num2];
                    if (accessor != null)
                    {
                        lock (s_accessorCache.SyncRoot)
                        {
                            s_accessorCache[num] = accessor;
                        }
                        return accessor;
                    }
                }
                if (accessor == null)
                {
                    Type type3;
                    lock (s_accessorGenerator)
                    {
                        type3 = s_accessorGenerator.GetPropertyAccessorTypeWithAssert(type2, propertyName, propInfo, fieldInfo);
                    }
                    accessor = (IWebPropertyAccessor) HttpRuntime.CreateNonPublicInstance(type3);
                }
                lock (s_accessorCache.SyncRoot)
                {
                    s_accessorCache[num] = accessor;
                    if (num2 != 0)
                    {
                        s_accessorCache[num2] = accessor;
                    }
                }
            }
            return accessor;
        }

        private Type GetPropertyAccessorTypeWithAssert(Type type, string propertyName, PropertyInfo propInfo, FieldInfo fieldInfo)
        {
            MethodInfo methodInfo = null;
            MethodInfo setMethod = null;
            Type propertyType;
            if (propInfo != null)
            {
                methodInfo = propInfo.GetGetMethod();
                setMethod = propInfo.GetSetMethod();
                propertyType = propInfo.PropertyType;
            }
            else
            {
                propertyType = fieldInfo.FieldType;
            }
            if (this._dynamicModule == null)
            {
                lock (this)
                {
                    if (this._dynamicModule == null)
                    {
                        string uniqueCompilationName = GetUniqueCompilationName();
                        AssemblyName name = new AssemblyName {
                            Name = "A_" + uniqueCompilationName
                        };
                        this._dynamicModule = Thread.GetDomain().DefineDynamicAssembly(name, AssemblyBuilderAccess.Run, null, true, null).DefineDynamicModule("M_" + uniqueCompilationName);
                    }
                }
            }
            TypeBuilder builder2 = this._dynamicModule.DefineType("T_" + string.Concat(new object[] { Util.MakeValidTypeNameFromString(type.Name), "_", propertyName, "_", _uniqueId++ }), TypeAttributes.Public, typeof(object), _interfacesToImplement);
            MethodBuilder methodInfoBody = builder2.DefineMethod("GetProperty", MethodAttributes.Virtual | MethodAttributes.Public, typeof(object), _getPropertyParameterList);
            ILGenerator iLGenerator = methodInfoBody.GetILGenerator();
            if (methodInfo != null)
            {
                iLGenerator.Emit(OpCodes.Ldarg_1);
                iLGenerator.Emit(OpCodes.Castclass, type);
                if (propInfo != null)
                {
                    iLGenerator.EmitCall(OpCodes.Callvirt, methodInfo, null);
                }
                else
                {
                    iLGenerator.Emit(OpCodes.Ldfld, fieldInfo);
                }
                iLGenerator.Emit(OpCodes.Box, propertyType);
                iLGenerator.Emit(OpCodes.Ret);
                builder2.DefineMethodOverride(methodInfoBody, _getPropertyMethod);
            }
            else
            {
                ConstructorInfo constructor = typeof(InvalidOperationException).GetConstructor(Type.EmptyTypes);
                iLGenerator.Emit(OpCodes.Newobj, constructor);
                iLGenerator.Emit(OpCodes.Throw);
            }
            methodInfoBody = builder2.DefineMethod("SetProperty", MethodAttributes.Virtual | MethodAttributes.Public, null, _setPropertyParameterList);
            iLGenerator = methodInfoBody.GetILGenerator();
            if ((fieldInfo != null) || (setMethod != null))
            {
                iLGenerator.Emit(OpCodes.Ldarg_1);
                iLGenerator.Emit(OpCodes.Castclass, type);
                iLGenerator.Emit(OpCodes.Ldarg_2);
                if (propertyType.IsPrimitive)
                {
                    iLGenerator.Emit(OpCodes.Unbox, propertyType);
                    if (propertyType == typeof(sbyte))
                    {
                        iLGenerator.Emit(OpCodes.Ldind_I1);
                    }
                    else if (propertyType == typeof(byte))
                    {
                        iLGenerator.Emit(OpCodes.Ldind_U1);
                    }
                    else if (propertyType == typeof(short))
                    {
                        iLGenerator.Emit(OpCodes.Ldind_I2);
                    }
                    else if (propertyType == typeof(ushort))
                    {
                        iLGenerator.Emit(OpCodes.Ldind_U2);
                    }
                    else if (propertyType == typeof(uint))
                    {
                        iLGenerator.Emit(OpCodes.Ldind_U4);
                    }
                    else if (propertyType == typeof(int))
                    {
                        iLGenerator.Emit(OpCodes.Ldind_I4);
                    }
                    else if (propertyType == typeof(long))
                    {
                        iLGenerator.Emit(OpCodes.Ldind_I8);
                    }
                    else if (propertyType == typeof(ulong))
                    {
                        iLGenerator.Emit(OpCodes.Ldind_I8);
                    }
                    else if (propertyType == typeof(bool))
                    {
                        iLGenerator.Emit(OpCodes.Ldind_I1);
                    }
                    else if (propertyType == typeof(char))
                    {
                        iLGenerator.Emit(OpCodes.Ldind_U2);
                    }
                    else if (propertyType == typeof(decimal))
                    {
                        iLGenerator.Emit(OpCodes.Ldobj, propertyType);
                    }
                    else if (propertyType == typeof(float))
                    {
                        iLGenerator.Emit(OpCodes.Ldind_R4);
                    }
                    else if (propertyType == typeof(double))
                    {
                        iLGenerator.Emit(OpCodes.Ldind_R8);
                    }
                    else
                    {
                        iLGenerator.Emit(OpCodes.Ldobj, propertyType);
                    }
                }
                else if (propertyType.IsValueType)
                {
                    iLGenerator.Emit(OpCodes.Unbox, propertyType);
                    iLGenerator.Emit(OpCodes.Ldobj, propertyType);
                }
                else
                {
                    iLGenerator.Emit(OpCodes.Castclass, propertyType);
                }
                if (propInfo != null)
                {
                    iLGenerator.EmitCall(OpCodes.Callvirt, setMethod, null);
                }
                else
                {
                    iLGenerator.Emit(OpCodes.Stfld, fieldInfo);
                }
            }
            iLGenerator.Emit(OpCodes.Ret);
            builder2.DefineMethodOverride(methodInfoBody, _setPropertyMethod);
            return builder2.CreateType();
        }

        private static void GetPropertyInfo(Type type, string propertyName, out PropertyInfo propInfo, out FieldInfo fieldInfo, out Type declaringType)
        {
            propInfo = GetPropertyMostSpecific(type, propertyName);
            fieldInfo = null;
            if (propInfo != null)
            {
                MethodInfo getMethod = propInfo.GetGetMethod();
                if (getMethod == null)
                {
                    getMethod = propInfo.GetSetMethod();
                }
                declaringType = getMethod.GetBaseDefinition().DeclaringType;
                if (declaringType.IsGenericType)
                {
                    declaringType = type;
                }
                if (declaringType != type)
                {
                    propInfo = declaringType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                }
            }
            else
            {
                fieldInfo = type.GetField(propertyName);
                if (fieldInfo == null)
                {
                    throw new ArgumentException();
                }
                declaringType = fieldInfo.DeclaringType;
            }
        }

        private static PropertyInfo GetPropertyMostSpecific(Type type, string name)
        {
            for (Type type2 = type; type2 != null; type2 = type2.BaseType)
            {
                PropertyInfo property = type2.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (property != null)
                {
                    return property;
                }
            }
            return null;
        }

        private static string GetUniqueCompilationName()
        {
            return Guid.NewGuid().ToString().Replace('-', '_');
        }

        internal static void SetProperty(object target, string propName, object val, bool inDesigner)
        {
            if (!inDesigner)
            {
                GetPropertyAccessor(target.GetType(), propName).SetProperty(target, val);
            }
            else
            {
                FieldInfo fieldInfo = null;
                PropertyInfo propInfo = null;
                Type declaringType = null;
                GetPropertyInfo(target.GetType(), propName, out propInfo, out fieldInfo, out declaringType);
                if (propInfo != null)
                {
                    propInfo.SetValue(target, val, null);
                }
                else
                {
                    if (fieldInfo == null)
                    {
                        throw new ArgumentException();
                    }
                    fieldInfo.SetValue(target, val);
                }
            }
        }
    }
}

