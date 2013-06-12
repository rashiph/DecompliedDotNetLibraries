namespace System.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Security;

    internal static class Associates
    {
        [SecurityCritical]
        private static RuntimeMethodInfo AssignAssociates(int tkMethod, RuntimeType declaredType, RuntimeType reflectedType)
        {
            if (MetadataToken.IsNullToken(tkMethod))
            {
                return null;
            }
            bool flag = declaredType != reflectedType;
            IntPtr[] typeInstantiationContext = null;
            int typeInstCount = 0;
            RuntimeType[] instantiationInternal = declaredType.GetTypeHandleInternal().GetInstantiationInternal();
            if (instantiationInternal != null)
            {
                typeInstCount = instantiationInternal.Length;
                typeInstantiationContext = new IntPtr[instantiationInternal.Length];
                for (int i = 0; i < instantiationInternal.Length; i++)
                {
                    typeInstantiationContext[i] = instantiationInternal[i].GetTypeHandleInternal().Value;
                }
            }
            RuntimeMethodHandleInternal method = ModuleHandle.ResolveMethodHandleInternalCore(RuntimeTypeHandle.GetModule(declaredType), tkMethod, typeInstantiationContext, typeInstCount, null, 0);
            if (flag)
            {
                MethodAttributes attributes = RuntimeMethodHandle.GetAttributes(method);
                if ((attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Private)
                {
                    return null;
                }
                if (((attributes & MethodAttributes.Virtual) != MethodAttributes.PrivateScope) && ((RuntimeTypeHandle.GetAttributes(declaredType) & TypeAttributes.ClassSemanticsMask) == TypeAttributes.AnsiClass))
                {
                    int slot = RuntimeMethodHandle.GetSlot(method);
                    method = RuntimeTypeHandle.GetMethodAt(reflectedType, slot);
                }
            }
            RuntimeMethodInfo methodBase = RuntimeType.GetMethodBase(reflectedType, method) as RuntimeMethodInfo;
            if (methodBase == null)
            {
                methodBase = reflectedType.Module.ResolveMethod(tkMethod, null, null) as RuntimeMethodInfo;
            }
            return methodBase;
        }

        [SecurityCritical]
        internal static unsafe void AssignAssociates(AssociateRecord* associates, int cAssociates, RuntimeType declaringType, RuntimeType reflectedType, out RuntimeMethodInfo addOn, out RuntimeMethodInfo removeOn, out RuntimeMethodInfo fireOn, out RuntimeMethodInfo getter, out RuntimeMethodInfo setter, out MethodInfo[] other, out bool composedOfAllPrivateMethods, out BindingFlags bindingFlags)
        {
            RuntimeMethodInfo info2;
            RuntimeMethodInfo info3;
            RuntimeMethodInfo info4;
            setter = (RuntimeMethodInfo) (info2 = null);
            getter = info3 = info2;
            fireOn = info4 = info3;
            addOn = removeOn = info4;
            other = null;
            Attributes attributes = Attributes.ComposedOfNoStaticMembers | Attributes.ComposedOfNoPublicMembers | Attributes.ComposedOfAllPrivateMethods | Attributes.ComposedOfAllVirtualMethods;
            while (RuntimeTypeHandle.IsGenericVariable(reflectedType))
            {
                reflectedType = (RuntimeType) reflectedType.BaseType;
            }
            bool isInherited = declaringType != reflectedType;
            List<MethodInfo> list = new List<MethodInfo>(cAssociates);
            for (int i = 0; i < cAssociates; i++)
            {
                RuntimeMethodInfo item = AssignAssociates(associates[i].MethodDefToken, declaringType, reflectedType);
                if (item != null)
                {
                    MethodAttributes attributes2 = item.Attributes;
                    bool flag2 = (attributes2 & MethodAttributes.MemberAccessMask) == MethodAttributes.Private;
                    bool flag3 = (attributes2 & MethodAttributes.Virtual) != MethodAttributes.PrivateScope;
                    MethodAttributes attributes3 = attributes2 & MethodAttributes.MemberAccessMask;
                    bool flag4 = attributes3 == MethodAttributes.Public;
                    bool flag5 = (attributes2 & MethodAttributes.Static) != MethodAttributes.PrivateScope;
                    if (flag4)
                    {
                        attributes &= ~Attributes.ComposedOfNoPublicMembers;
                        attributes &= ~Attributes.ComposedOfAllPrivateMethods;
                    }
                    else if (!flag2)
                    {
                        attributes &= ~Attributes.ComposedOfAllPrivateMethods;
                    }
                    if (flag5)
                    {
                        attributes &= ~Attributes.ComposedOfNoStaticMembers;
                    }
                    if (!flag3)
                    {
                        attributes &= ~Attributes.ComposedOfAllVirtualMethods;
                    }
                    if (associates[i].Semantics == MethodSemanticsAttributes.Setter)
                    {
                        setter = item;
                    }
                    else if (associates[i].Semantics == MethodSemanticsAttributes.Getter)
                    {
                        getter = item;
                    }
                    else if (associates[i].Semantics == MethodSemanticsAttributes.Fire)
                    {
                        fireOn = item;
                    }
                    else if (associates[i].Semantics == MethodSemanticsAttributes.AddOn)
                    {
                        addOn = item;
                    }
                    else if (associates[i].Semantics == MethodSemanticsAttributes.RemoveOn)
                    {
                        removeOn = item;
                    }
                    else
                    {
                        list.Add(item);
                    }
                }
            }
            bool isPublic = (attributes & Attributes.ComposedOfNoPublicMembers) == 0;
            bool isStatic = (attributes & Attributes.ComposedOfNoStaticMembers) == 0;
            bindingFlags = RuntimeType.FilterPreCalculate(isPublic, isInherited, isStatic);
            composedOfAllPrivateMethods = (attributes & Attributes.ComposedOfAllPrivateMethods) != 0;
            other = list.ToArray();
        }

        internal static bool IncludeAccessor(MethodInfo associate, bool nonPublic)
        {
            if (associate == null)
            {
                return false;
            }
            return (nonPublic || associate.IsPublic);
        }

        [Flags]
        internal enum Attributes
        {
            ComposedOfAllPrivateMethods = 2,
            ComposedOfAllVirtualMethods = 1,
            ComposedOfNoPublicMembers = 4,
            ComposedOfNoStaticMembers = 8
        }
    }
}

