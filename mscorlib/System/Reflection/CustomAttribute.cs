namespace System.Reflection
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    internal static class CustomAttribute
    {
        private static RuntimeType Type_RuntimeType = ((RuntimeType) typeof(RuntimeType));
        private static RuntimeType Type_Type = ((RuntimeType) typeof(Type));

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern unsafe object _CreateCaObject(RuntimeModule pModule, IRuntimeMethodInfo pCtor, byte** ppBlob, byte* pEndBlob, int* pcNamedArgs);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern unsafe void _GetPropertyOrFieldData(RuntimeModule pModule, byte** ppBlobStart, byte* pBlobEnd, out string name, out bool bIsProperty, out RuntimeType type, out object value);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void _ParseAttributeUsageAttribute(IntPtr pCa, int cCa, out int targets, out bool inherited, out bool allowMultiple);
        private static bool AllowCriticalCustomAttributes(MethodBase method)
        {
            if (method.IsSecurityTransparent)
            {
                return SpecialAllowCriticalAttributes((RuntimeType) method.DeclaringType);
            }
            return true;
        }

        private static bool AllowCriticalCustomAttributes(RuntimeFieldInfo field)
        {
            if (field.IsSecurityTransparent)
            {
                return SpecialAllowCriticalAttributes((RuntimeType) field.DeclaringType);
            }
            return true;
        }

        private static bool AllowCriticalCustomAttributes(RuntimeParameterInfo parameter)
        {
            return AllowCriticalCustomAttributes(parameter.DefiningMethod);
        }

        private static bool AllowCriticalCustomAttributes(RuntimeType type)
        {
            if (type.IsGenericParameter)
            {
                MethodBase declaringMethod = type.DeclaringMethod;
                if (declaringMethod != null)
                {
                    return AllowCriticalCustomAttributes(declaringMethod);
                }
                type = type.DeclaringType as RuntimeType;
            }
            if (type.IsSecurityTransparent)
            {
                return SpecialAllowCriticalAttributes(type);
            }
            return true;
        }

        [SecurityCritical]
        private static bool AttributeUsageCheck(RuntimeType attributeType, bool mustBeInheritable, object[] attributes, IList derivedAttributes)
        {
            AttributeUsageAttribute attributeUsage = null;
            if (mustBeInheritable)
            {
                attributeUsage = GetAttributeUsage(attributeType);
                if (!attributeUsage.Inherited)
                {
                    return false;
                }
            }
            if (derivedAttributes != null)
            {
                for (int i = 0; i < derivedAttributes.Count; i++)
                {
                    if (derivedAttributes[i].GetType() == attributeType)
                    {
                        if (attributeUsage == null)
                        {
                            attributeUsage = GetAttributeUsage(attributeType);
                        }
                        return attributeUsage.AllowMultiple;
                    }
                }
            }
            return true;
        }

        [SecuritySafeCritical]
        private static object[] CreateAttributeArrayHelper(Type elementType, int elementCount)
        {
            return (object[]) Array.UnsafeCreateInstance(elementType, elementCount);
        }

        [SecurityCritical]
        private static unsafe object CreateCaObject(RuntimeModule module, IRuntimeMethodInfo ctor, ref IntPtr blob, IntPtr blobEnd, out int namedArgs)
        {
            int num;
            byte* ppBlob = (byte*) blob;
            byte* pEndBlob = (byte*) blobEnd;
            object obj2 = _CreateCaObject(module, ctor, &ppBlob, pEndBlob, &num);
            blob = (IntPtr) ppBlob;
            namedArgs = num;
            return obj2;
        }

        [SecurityCritical]
        private static unsafe bool FilterCustomAttributeRecord(CustomAttributeRecord caRecord, MetadataImport scope, ref Assembly lastAptcaOkAssembly, RuntimeModule decoratedModule, MetadataToken decoratedToken, RuntimeType attributeFilterType, bool mustBeInheritable, object[] attributes, IList derivedAttributes, out RuntimeType attributeType, out IRuntimeMethodInfo ctor, out bool ctorHasParameters, out bool isVarArg)
        {
            ctor = null;
            attributeType = null;
            ctorHasParameters = false;
            isVarArg = false;
            IntPtr ptr1 = (IntPtr) (((void*) caRecord.blob.Signature) + caRecord.blob.Length);
            attributeType = decoratedModule.ResolveType(scope.GetParentToken((int) caRecord.tkCtor), null, null) as RuntimeType;
            if (!attributeFilterType.IsAssignableFrom(attributeType))
            {
                return false;
            }
            if (!AttributeUsageCheck(attributeType, mustBeInheritable, attributes, derivedAttributes))
            {
                return false;
            }
            RuntimeAssembly targetAssembly = (RuntimeAssembly) attributeType.Assembly;
            RuntimeAssembly assembly = (RuntimeAssembly) decoratedModule.Assembly;
            if ((targetAssembly != lastAptcaOkAssembly) && !RuntimeAssembly.AptcaCheck(targetAssembly, assembly))
            {
                return false;
            }
            lastAptcaOkAssembly = assembly;
            ConstArray methodSignature = scope.GetMethodSignature(caRecord.tkCtor);
            isVarArg = (methodSignature[0] & 5) != 0;
            ctorHasParameters = methodSignature[1] != 0;
            if (ctorHasParameters)
            {
                ctor = ModuleHandle.ResolveMethodHandleInternal(decoratedModule.GetNativeHandle(), (int) caRecord.tkCtor);
            }
            else
            {
                ctor = attributeType.GetTypeHandleInternal().GetDefaultConstructor();
                if ((ctor == null) && !attributeType.IsValueType)
                {
                    throw new MissingMethodException(".ctor");
                }
            }
            if (ctor == null)
            {
                if (!attributeType.IsVisible && !attributeType.TypeHandle.IsVisibleFromModule(decoratedModule))
                {
                    return false;
                }
                return true;
            }
            if (RuntimeMethodHandle.IsVisibleFromModule(ctor, decoratedModule))
            {
                return true;
            }
            MetadataToken token = new MetadataToken();
            if (decoratedToken.IsParamDef)
            {
                token = new MetadataToken(scope.GetParentToken((int) decoratedToken));
                token = new MetadataToken(scope.GetParentToken((int) token));
            }
            else if ((decoratedToken.IsMethodDef || decoratedToken.IsProperty) || (decoratedToken.IsEvent || decoratedToken.IsFieldDef))
            {
                token = new MetadataToken(scope.GetParentToken((int) decoratedToken));
            }
            else if (decoratedToken.IsTypeDef)
            {
                token = decoratedToken;
            }
            return (token.IsTypeDef && RuntimeMethodHandle.IsVisibleFromType(ctor, decoratedModule.ModuleHandle.ResolveTypeHandle((int) token)));
        }

        [SecurityCritical]
        internal static AttributeUsageAttribute GetAttributeUsage(RuntimeType decoratedAttribute)
        {
            RuntimeModule runtimeModule = decoratedAttribute.GetRuntimeModule();
            MetadataImport metadataImport = runtimeModule.MetadataImport;
            CustomAttributeRecord[] customAttributeRecords = CustomAttributeData.GetCustomAttributeRecords(runtimeModule, decoratedAttribute.MetadataToken);
            AttributeUsageAttribute attribute = null;
            for (int i = 0; i < customAttributeRecords.Length; i++)
            {
                CustomAttributeRecord record = customAttributeRecords[i];
                RuntimeType type = runtimeModule.ResolveType(metadataImport.GetParentToken((int) record.tkCtor), null, null) as RuntimeType;
                if (type == ((RuntimeType) typeof(AttributeUsageAttribute)))
                {
                    AttributeTargets targets;
                    bool flag;
                    bool flag2;
                    if (attribute != null)
                    {
                        throw new FormatException(string.Format(CultureInfo.CurrentUICulture, Environment.GetResourceString("Format_AttributeUsage"), new object[] { type }));
                    }
                    ParseAttributeUsageAttribute(record.blob, out targets, out flag, out flag2);
                    attribute = new AttributeUsageAttribute(targets, flag2, flag);
                }
            }
            if (attribute == null)
            {
                return AttributeUsageAttribute.Default;
            }
            return attribute;
        }

        [SecuritySafeCritical]
        internal static object[] GetCustomAttributes(RuntimeAssembly assembly, RuntimeType caType)
        {
            int count = 0;
            Attribute[] sourceArray = PseudoCustomAttribute.GetCustomAttributes(assembly, caType, true, out count);
            int token = RuntimeAssembly.GetToken(assembly.GetNativeHandle());
            bool isDecoratedTargetSecurityTransparent = assembly.IsAllSecurityTransparent();
            object[] destinationArray = GetCustomAttributes(assembly.ManifestModule as RuntimeModule, token, count, caType, isDecoratedTargetSecurityTransparent);
            if (count > 0)
            {
                Array.Copy(sourceArray, 0, destinationArray, destinationArray.Length - count, count);
            }
            return destinationArray;
        }

        [SecuritySafeCritical]
        internal static object[] GetCustomAttributes(RuntimeConstructorInfo ctor, RuntimeType caType)
        {
            int count = 0;
            Attribute[] sourceArray = PseudoCustomAttribute.GetCustomAttributes(ctor, caType, true, out count);
            object[] destinationArray = GetCustomAttributes(ctor.GetRuntimeModule(), ctor.MetadataToken, count, caType, !AllowCriticalCustomAttributes(ctor));
            if (count > 0)
            {
                Array.Copy(sourceArray, 0, destinationArray, destinationArray.Length - count, count);
            }
            return destinationArray;
        }

        [SecuritySafeCritical]
        internal static object[] GetCustomAttributes(RuntimeEventInfo e, RuntimeType caType)
        {
            int count = 0;
            Attribute[] sourceArray = PseudoCustomAttribute.GetCustomAttributes(e, caType, out count);
            bool isDecoratedTargetSecurityTransparent = e.GetRuntimeModule().GetRuntimeAssembly().IsAllSecurityTransparent();
            object[] destinationArray = GetCustomAttributes(e.GetRuntimeModule(), e.MetadataToken, count, caType, isDecoratedTargetSecurityTransparent);
            if (count > 0)
            {
                Array.Copy(sourceArray, 0, destinationArray, destinationArray.Length - count, count);
            }
            return destinationArray;
        }

        [SecuritySafeCritical]
        internal static object[] GetCustomAttributes(RuntimeFieldInfo field, RuntimeType caType)
        {
            int count = 0;
            Attribute[] sourceArray = PseudoCustomAttribute.GetCustomAttributes(field, caType, out count);
            object[] destinationArray = GetCustomAttributes(field.GetRuntimeModule(), field.MetadataToken, count, caType, !AllowCriticalCustomAttributes(field));
            if (count > 0)
            {
                Array.Copy(sourceArray, 0, destinationArray, destinationArray.Length - count, count);
            }
            return destinationArray;
        }

        [SecuritySafeCritical]
        internal static object[] GetCustomAttributes(RuntimeModule module, RuntimeType caType)
        {
            int count = 0;
            Attribute[] sourceArray = PseudoCustomAttribute.GetCustomAttributes(module, caType, out count);
            bool isDecoratedTargetSecurityTransparent = module.GetRuntimeAssembly().IsAllSecurityTransparent();
            object[] destinationArray = GetCustomAttributes(module, module.MetadataToken, count, caType, isDecoratedTargetSecurityTransparent);
            if (count > 0)
            {
                Array.Copy(sourceArray, 0, destinationArray, destinationArray.Length - count, count);
            }
            return destinationArray;
        }

        [SecuritySafeCritical]
        internal static object[] GetCustomAttributes(RuntimeParameterInfo parameter, RuntimeType caType)
        {
            int count = 0;
            Attribute[] sourceArray = PseudoCustomAttribute.GetCustomAttributes(parameter, caType, out count);
            object[] destinationArray = GetCustomAttributes(parameter.GetRuntimeModule(), parameter.MetadataToken, count, caType, !AllowCriticalCustomAttributes(parameter));
            if (count > 0)
            {
                Array.Copy(sourceArray, 0, destinationArray, destinationArray.Length - count, count);
            }
            return destinationArray;
        }

        [SecuritySafeCritical]
        internal static object[] GetCustomAttributes(RuntimePropertyInfo property, RuntimeType caType)
        {
            int count = 0;
            Attribute[] sourceArray = PseudoCustomAttribute.GetCustomAttributes(property, caType, out count);
            bool isDecoratedTargetSecurityTransparent = property.GetRuntimeModule().GetRuntimeAssembly().IsAllSecurityTransparent();
            object[] destinationArray = GetCustomAttributes(property.GetRuntimeModule(), property.MetadataToken, count, caType, isDecoratedTargetSecurityTransparent);
            if (count > 0)
            {
                Array.Copy(sourceArray, 0, destinationArray, destinationArray.Length - count, count);
            }
            return destinationArray;
        }

        [SecurityCritical]
        internal static object[] GetCustomAttributes(RuntimeMethodInfo method, RuntimeType caType, bool inherit)
        {
            if (method.IsGenericMethod && !method.IsGenericMethodDefinition)
            {
                method = method.GetGenericMethodDefinition() as RuntimeMethodInfo;
            }
            int count = 0;
            Attribute[] sourceArray = PseudoCustomAttribute.GetCustomAttributes(method, caType, true, out count);
            if (!inherit || (caType.IsSealed && !GetAttributeUsage(caType).Inherited))
            {
                object[] objArray = GetCustomAttributes(method.GetRuntimeModule(), method.MetadataToken, count, caType, !AllowCriticalCustomAttributes(method));
                if (count > 0)
                {
                    Array.Copy(sourceArray, 0, objArray, objArray.Length - count, count);
                }
                return objArray;
            }
            List<object> derivedAttributes = new List<object>();
            bool mustBeInheritable = false;
            Type elementType = (((caType == null) || caType.IsValueType) || caType.ContainsGenericParameters) ? typeof(object) : caType;
            while (count > 0)
            {
                derivedAttributes.Add(sourceArray[--count]);
            }
            while (method != null)
            {
                object[] objArray2 = GetCustomAttributes(method.GetRuntimeModule(), method.MetadataToken, 0, caType, mustBeInheritable, derivedAttributes, !AllowCriticalCustomAttributes(method));
                mustBeInheritable = true;
                for (int i = 0; i < objArray2.Length; i++)
                {
                    derivedAttributes.Add(objArray2[i]);
                }
                method = method.GetParentDefinition();
            }
            object[] destinationArray = CreateAttributeArrayHelper(elementType, derivedAttributes.Count);
            Array.Copy(derivedAttributes.ToArray(), 0, destinationArray, 0, derivedAttributes.Count);
            return destinationArray;
        }

        [SecurityCritical]
        internal static object[] GetCustomAttributes(RuntimeType type, RuntimeType caType, bool inherit)
        {
            if (type.GetElementType() != null)
            {
                if (!caType.IsValueType)
                {
                    return CreateAttributeArrayHelper(caType, 0);
                }
                return new object[0];
            }
            if (type.IsGenericType && !type.IsGenericTypeDefinition)
            {
                type = type.GetGenericTypeDefinition() as RuntimeType;
            }
            int count = 0;
            Attribute[] sourceArray = PseudoCustomAttribute.GetCustomAttributes(type, caType, true, out count);
            if (!inherit || (caType.IsSealed && !GetAttributeUsage(caType).Inherited))
            {
                object[] objArray = GetCustomAttributes(type.GetRuntimeModule(), type.MetadataToken, count, caType, !AllowCriticalCustomAttributes(type));
                if (count > 0)
                {
                    Array.Copy(sourceArray, 0, objArray, objArray.Length - count, count);
                }
                return objArray;
            }
            List<object> derivedAttributes = new List<object>();
            bool mustBeInheritable = false;
            Type elementType = (((caType == null) || caType.IsValueType) || caType.ContainsGenericParameters) ? typeof(object) : caType;
            while (count > 0)
            {
                derivedAttributes.Add(sourceArray[--count]);
            }
            while ((type != ((RuntimeType) typeof(object))) && (type != null))
            {
                object[] objArray2 = GetCustomAttributes(type.GetRuntimeModule(), type.MetadataToken, 0, caType, mustBeInheritable, derivedAttributes, !AllowCriticalCustomAttributes(type));
                mustBeInheritable = true;
                for (int i = 0; i < objArray2.Length; i++)
                {
                    derivedAttributes.Add(objArray2[i]);
                }
                type = type.BaseType as RuntimeType;
            }
            object[] destinationArray = CreateAttributeArrayHelper(elementType, derivedAttributes.Count);
            Array.Copy(derivedAttributes.ToArray(), 0, destinationArray, 0, derivedAttributes.Count);
            return destinationArray;
        }

        [SecurityCritical]
        private static object[] GetCustomAttributes(RuntimeModule decoratedModule, int decoratedMetadataToken, int pcaCount, RuntimeType attributeFilterType, bool isDecoratedTargetSecurityTransparent)
        {
            return GetCustomAttributes(decoratedModule, decoratedMetadataToken, pcaCount, attributeFilterType, false, null, isDecoratedTargetSecurityTransparent);
        }

        [SecurityCritical]
        private static unsafe object[] GetCustomAttributes(RuntimeModule decoratedModule, int decoratedMetadataToken, int pcaCount, RuntimeType attributeFilterType, bool mustBeInheritable, IList derivedAttributes, bool isDecoratedTargetSecurityTransparent)
        {
            if (decoratedModule.Assembly.ReflectionOnly)
            {
                throw new InvalidOperationException(Environment.GetResourceString("Arg_ReflectionOnlyCA"));
            }
            MetadataImport metadataImport = decoratedModule.MetadataImport;
            CustomAttributeRecord[] customAttributeRecords = CustomAttributeData.GetCustomAttributeRecords(decoratedModule, decoratedMetadataToken);
            Type elementType = (((attributeFilterType == null) || attributeFilterType.IsValueType) || attributeFilterType.ContainsGenericParameters) ? typeof(object) : attributeFilterType;
            if ((attributeFilterType == null) && (customAttributeRecords.Length == 0))
            {
                return CreateAttributeArrayHelper(elementType, 0);
            }
            object[] attributes = CreateAttributeArrayHelper(elementType, customAttributeRecords.Length);
            int length = 0;
            SecurityContextFrame frame = new SecurityContextFrame();
            frame.Push(decoratedModule.GetRuntimeAssembly());
            Assembly lastAptcaOkAssembly = null;
            for (int i = 0; i < customAttributeRecords.Length; i++)
            {
                bool flag2;
                bool flag3;
                object obj2 = null;
                CustomAttributeRecord caRecord = customAttributeRecords[i];
                IRuntimeMethodInfo ctor = null;
                RuntimeType attributeType = null;
                int namedArgs = 0;
                IntPtr signature = caRecord.blob.Signature;
                IntPtr blobEnd = (IntPtr) (((void*) signature) + caRecord.blob.Length);
                int num4 = (int) ((long) ((((void*) blobEnd) - ((void*) signature)) / 1));
                if (FilterCustomAttributeRecord(caRecord, metadataImport, ref lastAptcaOkAssembly, decoratedModule, decoratedMetadataToken, attributeFilterType, mustBeInheritable, attributes, derivedAttributes, out attributeType, out ctor, out flag2, out flag3))
                {
                    if (ctor != null)
                    {
                        RuntimeMethodHandle.CheckLinktimeDemands(ctor, decoratedModule, isDecoratedTargetSecurityTransparent);
                    }
                    RuntimeConstructorInfo.CheckCanCreateInstance(attributeType, flag3);
                    if (flag2)
                    {
                        obj2 = CreateCaObject(decoratedModule, ctor, ref signature, blobEnd, out namedArgs);
                    }
                    else
                    {
                        obj2 = RuntimeTypeHandle.CreateCaInstance(attributeType, ctor);
                        if (num4 == 0)
                        {
                            namedArgs = 0;
                        }
                        else
                        {
                            if (Marshal.ReadInt16(signature) != 1)
                            {
                                throw new CustomAttributeFormatException();
                            }
                            signature = (IntPtr) (((void*) signature) + 2);
                            namedArgs = Marshal.ReadInt16(signature);
                            signature = (IntPtr) (((void*) signature) + 2);
                        }
                    }
                    for (int j = 0; j < namedArgs; j++)
                    {
                        string str;
                        bool flag4;
                        RuntimeType type;
                        object obj3;
                        IntPtr ptr1 = caRecord.blob.Signature;
                        GetPropertyOrFieldData(decoratedModule, ref signature, blobEnd, out str, out flag4, out type, out obj3);
                        try
                        {
                            if (flag4)
                            {
                                if ((type == null) && (obj3 != null))
                                {
                                    type = (RuntimeType) obj3.GetType();
                                    if (type == Type_RuntimeType)
                                    {
                                        type = Type_Type;
                                    }
                                }
                                RuntimePropertyInfo property = null;
                                if (type == null)
                                {
                                    property = attributeType.GetProperty(str) as RuntimePropertyInfo;
                                }
                                else
                                {
                                    property = attributeType.GetProperty(str, type, Type.EmptyTypes) as RuntimePropertyInfo;
                                }
                                if (property == null)
                                {
                                    throw new CustomAttributeFormatException(string.Format(CultureInfo.CurrentUICulture, Environment.GetResourceString(flag4 ? "RFLCT.InvalidPropFail" : "RFLCT.InvalidFieldFail"), new object[] { str }));
                                }
                                RuntimeMethodInfo setMethod = property.GetSetMethod(true) as RuntimeMethodInfo;
                                if (setMethod.IsPublic)
                                {
                                    RuntimeMethodHandle.CheckLinktimeDemands(setMethod, decoratedModule, isDecoratedTargetSecurityTransparent);
                                    setMethod.Invoke(obj2, BindingFlags.Default, null, new object[] { obj3 }, null, true);
                                }
                            }
                            else
                            {
                                RtFieldInfo field = attributeType.GetField(str) as RtFieldInfo;
                                if (isDecoratedTargetSecurityTransparent)
                                {
                                    RuntimeFieldHandle.CheckAttributeAccess(field.FieldHandle, decoratedModule.GetNativeHandle());
                                }
                                field.InternalSetValue(obj2, obj3, BindingFlags.Default, Type.DefaultBinder, null, false);
                            }
                        }
                        catch (Exception exception)
                        {
                            throw new CustomAttributeFormatException(string.Format(CultureInfo.CurrentUICulture, Environment.GetResourceString(flag4 ? "RFLCT.InvalidPropFail" : "RFLCT.InvalidFieldFail"), new object[] { str }), exception);
                        }
                    }
                    if (!signature.Equals(blobEnd))
                    {
                        throw new CustomAttributeFormatException();
                    }
                    attributes[length++] = obj2;
                }
            }
            frame.Pop();
            if ((length == customAttributeRecords.Length) && (pcaCount == 0))
            {
                return attributes;
            }
            object[] destinationArray = CreateAttributeArrayHelper(elementType, length + pcaCount);
            Array.Copy(attributes, 0, destinationArray, 0, length);
            return destinationArray;
        }

        [SecurityCritical]
        private static unsafe void GetPropertyOrFieldData(RuntimeModule module, ref IntPtr blobStart, IntPtr blobEnd, out string name, out bool isProperty, out RuntimeType type, out object value)
        {
            byte* ppBlobStart = (byte*) blobStart;
            _GetPropertyOrFieldData(module.GetNativeHandle(), &ppBlobStart, (byte*) blobEnd, out name, out isProperty, out type, out value);
            blobStart = (IntPtr) ppBlobStart;
        }

        [SecurityCritical]
        private static bool IsCustomAttributeDefined(RuntimeModule decoratedModule, int decoratedMetadataToken, RuntimeType attributeFilterType)
        {
            return IsCustomAttributeDefined(decoratedModule, decoratedMetadataToken, attributeFilterType, false);
        }

        [SecurityCritical]
        private static bool IsCustomAttributeDefined(RuntimeModule decoratedModule, int decoratedMetadataToken, RuntimeType attributeFilterType, bool mustBeInheritable)
        {
            if (decoratedModule.Assembly.ReflectionOnly)
            {
                throw new InvalidOperationException(Environment.GetResourceString("Arg_ReflectionOnlyCA"));
            }
            MetadataImport metadataImport = decoratedModule.MetadataImport;
            CustomAttributeRecord[] customAttributeRecords = CustomAttributeData.GetCustomAttributeRecords(decoratedModule, decoratedMetadataToken);
            Assembly lastAptcaOkAssembly = null;
            for (int i = 0; i < customAttributeRecords.Length; i++)
            {
                RuntimeType type;
                IRuntimeMethodInfo info;
                bool flag;
                bool flag2;
                CustomAttributeRecord caRecord = customAttributeRecords[i];
                if (FilterCustomAttributeRecord(caRecord, metadataImport, ref lastAptcaOkAssembly, decoratedModule, decoratedMetadataToken, attributeFilterType, mustBeInheritable, null, null, out type, out info, out flag, out flag2))
                {
                    return true;
                }
            }
            return false;
        }

        [SecuritySafeCritical]
        internal static bool IsDefined(RuntimeAssembly assembly, RuntimeType caType)
        {
            return (PseudoCustomAttribute.IsDefined(assembly, caType) || IsCustomAttributeDefined(assembly.ManifestModule as RuntimeModule, RuntimeAssembly.GetToken(assembly.GetNativeHandle()), caType));
        }

        [SecurityCritical]
        internal static bool IsDefined(RuntimeConstructorInfo ctor, RuntimeType caType)
        {
            return (PseudoCustomAttribute.IsDefined(ctor, caType) || IsCustomAttributeDefined(ctor.GetRuntimeModule(), ctor.MetadataToken, caType));
        }

        [SecurityCritical]
        internal static bool IsDefined(RuntimeEventInfo e, RuntimeType caType)
        {
            return (PseudoCustomAttribute.IsDefined(e, caType) || IsCustomAttributeDefined(e.GetRuntimeModule(), e.MetadataToken, caType));
        }

        [SecurityCritical]
        internal static bool IsDefined(RuntimeFieldInfo field, RuntimeType caType)
        {
            return (PseudoCustomAttribute.IsDefined(field, caType) || IsCustomAttributeDefined(field.GetRuntimeModule(), field.MetadataToken, caType));
        }

        [SecurityCritical]
        internal static bool IsDefined(RuntimeModule module, RuntimeType caType)
        {
            return (PseudoCustomAttribute.IsDefined(module, caType) || IsCustomAttributeDefined(module, module.MetadataToken, caType));
        }

        [SecurityCritical]
        internal static bool IsDefined(RuntimeParameterInfo parameter, RuntimeType caType)
        {
            return (PseudoCustomAttribute.IsDefined(parameter, caType) || IsCustomAttributeDefined(parameter.GetRuntimeModule(), parameter.MetadataToken, caType));
        }

        [SecurityCritical]
        internal static bool IsDefined(RuntimePropertyInfo property, RuntimeType caType)
        {
            return (PseudoCustomAttribute.IsDefined(property, caType) || IsCustomAttributeDefined(property.GetRuntimeModule(), property.MetadataToken, caType));
        }

        [SecuritySafeCritical]
        internal static bool IsDefined(RuntimeMethodInfo method, RuntimeType caType, bool inherit)
        {
            if (PseudoCustomAttribute.IsDefined(method, caType))
            {
                return true;
            }
            if (IsCustomAttributeDefined(method.GetRuntimeModule(), method.MetadataToken, caType))
            {
                return true;
            }
            if (inherit)
            {
                method = method.GetParentDefinition();
                while (method != null)
                {
                    if (IsCustomAttributeDefined(method.GetRuntimeModule(), method.MetadataToken, caType, inherit))
                    {
                        return true;
                    }
                    method = method.GetParentDefinition();
                }
            }
            return false;
        }

        [SecurityCritical]
        internal static bool IsDefined(RuntimeType type, RuntimeType caType, bool inherit)
        {
            if (type.GetElementType() == null)
            {
                if (PseudoCustomAttribute.IsDefined(type, caType))
                {
                    return true;
                }
                if (IsCustomAttributeDefined(type.GetRuntimeModule(), type.MetadataToken, caType))
                {
                    return true;
                }
                if (!inherit)
                {
                    return false;
                }
                type = type.BaseType as RuntimeType;
                while (type != null)
                {
                    if (IsCustomAttributeDefined(type.GetRuntimeModule(), type.MetadataToken, caType, inherit))
                    {
                        return true;
                    }
                    type = type.BaseType as RuntimeType;
                }
            }
            return false;
        }

        [SecurityCritical]
        private static void ParseAttributeUsageAttribute(ConstArray ca, out AttributeTargets targets, out bool inherited, out bool allowMultiple)
        {
            int num;
            _ParseAttributeUsageAttribute(ca.Signature, ca.Length, out num, out inherited, out allowMultiple);
            targets = (AttributeTargets) num;
        }

        private static bool SpecialAllowCriticalAttributes(RuntimeType type)
        {
            return (type.Assembly.IsFullyTrusted && RuntimeTypeHandle.IsEquivalentType(type));
        }
    }
}

