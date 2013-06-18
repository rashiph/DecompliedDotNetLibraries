namespace Microsoft.VisualBasic.CompilerServices
{
    using Microsoft.VisualBasic;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Threading;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class LateBinding
    {
        private const CallType DefaultCallType = ((CallType) 0);
        private const BindingFlags VBLateBindingFlags = (BindingFlags.OptionalParamBinding | BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase);

        private LateBinding()
        {
        }

        private static void CheckForClassExtendingCOMClass(Type objType)
        {
            if ((objType.IsCOMObject && (objType.FullName != "System.__ComObject")) && (objType.BaseType.FullName != "System.__ComObject"))
            {
                throw new InvalidOperationException(Utils.GetResourceString("LateboundCallToInheritedComClass"));
            }
        }

        internal static bool DoesTargetObjectMatch(object Value, MemberInfo Member)
        {
            if ((Value != null) && !Member.DeclaringType.IsAssignableFrom(Value.GetType()))
            {
                return false;
            }
            return true;
        }

        [DebuggerStepThrough, DebuggerHidden]
        private static object FastCall(object o, MethodBase method, ParameterInfo[] Parameters, object[] args, Type objType, IReflect objIReflect)
        {
            int upperBound = args.GetUpperBound(0);
            for (int i = 0; i <= upperBound; i++)
            {
                ParameterInfo info = Parameters[i];
                object defaultValue = args[i];
                if ((defaultValue is Missing) && info.IsOptional)
                {
                    defaultValue = info.DefaultValue;
                }
                args[i] = ObjectType.CTypeHelper(defaultValue, info.ParameterType);
            }
            VBBinder.SecurityCheckForLateboundCalls(method, objType, objIReflect);
            if (((objType != objIReflect) && !method.IsStatic) && !DoesTargetObjectMatch(o, method))
            {
                return InvokeMemberOnIReflect(objIReflect, method, BindingFlags.InvokeMethod, o, args);
            }
            VerifyObjRefPresentForInstanceCall(o, method);
            return method.Invoke(o, args);
        }

        private static IReflect GetCorrectIReflect(object o, Type objType)
        {
            if (((o != null) && !objType.IsCOMObject) && (!RemotingServices.IsTransparentProxy(o) && !(o is Type)))
            {
                IReflect reflect2 = o as IReflect;
                if (reflect2 != null)
                {
                    return reflect2;
                }
            }
            return objType;
        }

        private static MemberInfo[] GetDefaultMembers(Type typ, IReflect objIReflect, ref string DefaultName)
        {
            MemberInfo[] nonGenericMembers;
            if (typ == objIReflect)
            {
                do
                {
                    object[] customAttributes = typ.GetCustomAttributes(typeof(DefaultMemberAttribute), false);
                    if ((customAttributes != null) && (customAttributes.Length != 0))
                    {
                        DefaultName = ((DefaultMemberAttribute) customAttributes[0]).MemberName;
                        nonGenericMembers = GetNonGenericMembers(typ.GetMember(DefaultName, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase));
                        if ((nonGenericMembers != null) && (nonGenericMembers.Length != 0))
                        {
                            return nonGenericMembers;
                        }
                        DefaultName = "";
                        return null;
                    }
                    typ = typ.BaseType;
                }
                while (typ != null);
                DefaultName = "";
                return null;
            }
            nonGenericMembers = GetNonGenericMembers(objIReflect.GetMember("", BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase));
            if ((nonGenericMembers == null) || (nonGenericMembers.Length == 0))
            {
                DefaultName = "";
                return null;
            }
            DefaultName = nonGenericMembers[0].Name;
            return nonGenericMembers;
        }

        private static MemberInfo[] GetMembersByName(IReflect objIReflect, string name, BindingFlags flags)
        {
            MemberInfo[] nonGenericMembers = GetNonGenericMembers(objIReflect.GetMember(name, flags));
            if ((nonGenericMembers != null) && (nonGenericMembers.Length == 0))
            {
                return null;
            }
            return nonGenericMembers;
        }

        private static MemberInfo GetMostDerivedMemberInfo(IReflect objIReflect, string name, BindingFlags flags)
        {
            MemberInfo[] nonGenericMembers = GetNonGenericMembers(objIReflect.GetMember(name, flags));
            if ((nonGenericMembers == null) || (nonGenericMembers.Length == 0))
            {
                return null;
            }
            MemberInfo info2 = nonGenericMembers[0];
            int upperBound = nonGenericMembers.GetUpperBound(0);
            for (int i = 1; i <= upperBound; i++)
            {
                if (nonGenericMembers[i].DeclaringType.IsSubclassOf(info2.DeclaringType))
                {
                    info2 = nonGenericMembers[i];
                }
            }
            return info2;
        }

        internal static MemberInfo[] GetNonGenericMembers(MemberInfo[] Members)
        {
            if ((Members != null) && (Members.Length > 0))
            {
                int num = 0;
                int upperBound = Members.GetUpperBound(0);
                for (int i = 0; i <= upperBound; i++)
                {
                    if (LegacyIsGeneric(Members[i]))
                    {
                        Members[i] = null;
                    }
                    else
                    {
                        num++;
                    }
                }
                if (num == (Members.GetUpperBound(0) + 1))
                {
                    return Members;
                }
                if (num > 0)
                {
                    MemberInfo[] infoArray2 = new MemberInfo[(num - 1) + 1];
                    int index = 0;
                    int num6 = Members.GetUpperBound(0);
                    for (int j = 0; j <= num6; j++)
                    {
                        if (Members[j] != null)
                        {
                            infoArray2[index] = Members[j];
                            index++;
                        }
                    }
                    return infoArray2;
                }
            }
            return null;
        }

        private static BindingFlags GetPropertyPutFlags(object NewValue)
        {
            if (NewValue == null)
            {
                return BindingFlags.SetProperty;
            }
            if ((((NewValue is ValueType) || (NewValue is string)) || ((NewValue is DBNull) || (NewValue is Missing))) || ((NewValue is Array) || (NewValue is CurrencyWrapper)))
            {
                return BindingFlags.PutDispProperty;
            }
            return BindingFlags.PutRefDispProperty;
        }

        [DebuggerStepThrough, DebuggerHidden]
        internal static object InternalLateCall(object o, Type objType, string name, object[] args, string[] paramnames, bool[] CopyBack, bool IgnoreReturn)
        {
            object obj2;
            BindingFlags flags = BindingFlags.OptionalParamBinding | BindingFlags.InvokeMethod | BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase;
            if (IgnoreReturn)
            {
                flags |= BindingFlags.IgnoreReturn;
            }
            if (objType == null)
            {
                if (o == null)
                {
                    throw ExceptionUtils.VbMakeException(0x5b);
                }
                objType = o.GetType();
            }
            IReflect correctIReflect = GetCorrectIReflect(o, objType);
            if (objType.IsCOMObject)
            {
                CheckForClassExtendingCOMClass(objType);
            }
            if (name == null)
            {
                name = "";
            }
            VBBinder binder = new VBBinder(CopyBack);
            if (!objType.IsCOMObject)
            {
                MemberInfo[] mi = GetMembersByName(correctIReflect, name, flags);
                if ((mi == null) || (mi.Length == 0))
                {
                    throw new MissingMemberException(Utils.GetResourceString("MissingMember_MemberNotFoundOnType2", new string[] { name, Utils.VBFriendlyName(objType, o) }));
                }
                if (MemberIsField(mi))
                {
                    throw new ArgumentException(Utils.GetResourceString("ExpressionNotProcedure", new string[] { name, Utils.VBFriendlyName(objType, o) }));
                }
                if ((mi.Length == 1) && ((paramnames == null) || (paramnames.Length == 0)))
                {
                    MemberInfo getMethod = mi[0];
                    if (getMethod.MemberType == MemberTypes.Property)
                    {
                        getMethod = ((PropertyInfo) getMethod).GetGetMethod();
                        if (getMethod == null)
                        {
                            throw new MissingMemberException(Utils.GetResourceString("MissingMember_MemberNotFoundOnType2", new string[] { name, Utils.VBFriendlyName(objType, o) }));
                        }
                    }
                    MethodBase method = (MethodBase) getMethod;
                    ParameterInfo[] parameters = method.GetParameters();
                    int length = args.Length;
                    int num2 = parameters.Length;
                    if (num2 == length)
                    {
                        if (num2 == 0)
                        {
                            return FastCall(o, method, parameters, args, objType, correctIReflect);
                        }
                        if ((CopyBack == null) && NoByrefs(parameters))
                        {
                            ParameterInfo info2 = parameters[num2 - 1];
                            if (!info2.ParameterType.IsArray)
                            {
                                return FastCall(o, method, parameters, args, objType, correctIReflect);
                            }
                            object[] customAttributes = info2.GetCustomAttributes(typeof(ParamArrayAttribute), false);
                            if ((customAttributes == null) || (customAttributes.Length == 0))
                            {
                                return FastCall(o, method, parameters, args, objType, correctIReflect);
                            }
                        }
                    }
                }
            }
            try
            {
                obj2 = binder.InvokeMember(name, flags, objType, correctIReflect, o, args, paramnames);
            }
            catch (MissingMemberException)
            {
                throw;
            }
            catch when (?)
            {
                throw new MissingMemberException(Utils.GetResourceString("MissingMember_MemberNotFoundOnType2", new string[] { name, Utils.VBFriendlyName(objType, o) }));
            }
            catch (TargetInvocationException exception3)
            {
                throw exception3.InnerException;
            }
            return obj2;
        }

        [DebuggerHidden, DebuggerStepThrough]
        internal static void InternalLateSet(object o, ref Type objType, string name, object[] args, string[] paramnames, bool OptimisticSet, CallType UseCallType)
        {
            BindingFlags invokeAttr = BindingFlags.OptionalParamBinding | BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase;
            if (objType == null)
            {
                if (o == null)
                {
                    throw ExceptionUtils.VbMakeException(0x5b);
                }
                objType = o.GetType();
            }
            IReflect correctIReflect = GetCorrectIReflect(o, objType);
            if (name == null)
            {
                name = "";
            }
            if (objType.IsCOMObject)
            {
                CheckForClassExtendingCOMClass(objType);
                if (UseCallType == CallType.Set)
                {
                    invokeAttr |= BindingFlags.PutRefDispProperty;
                    if (args[args.GetUpperBound(0)] == null)
                    {
                        args[args.GetUpperBound(0)] = new DispatchWrapper(null);
                    }
                }
                else if (UseCallType == CallType.Let)
                {
                    invokeAttr |= BindingFlags.PutDispProperty;
                }
                else
                {
                    invokeAttr |= GetPropertyPutFlags(args[args.GetUpperBound(0)]);
                }
            }
            else
            {
                invokeAttr |= BindingFlags.SetProperty;
                MemberInfo member = GetMostDerivedMemberInfo(correctIReflect, name, invokeAttr | BindingFlags.SetField);
                if ((member != null) && (member.MemberType == MemberTypes.Field))
                {
                    FieldInfo info2 = (FieldInfo) member;
                    if (info2.IsInitOnly)
                    {
                        throw new MissingMemberException(Utils.GetResourceString("MissingMember_ReadOnlyField2", new string[] { name, Utils.VBFriendlyName(objType, o) }));
                    }
                    if ((args == null) || (args.Length == 0))
                    {
                        throw new MissingMemberException(Utils.GetResourceString("MissingMember_MemberNotFoundOnType2", new string[] { name, Utils.VBFriendlyName(objType, o) }));
                    }
                    if (args.Length == 1)
                    {
                        object obj3;
                        object obj2 = args[0];
                        VBBinder.SecurityCheckForLateboundCalls(info2, objType, correctIReflect);
                        if (obj2 == null)
                        {
                            obj3 = null;
                        }
                        else
                        {
                            obj3 = ObjectType.CTypeHelper(args[0], info2.FieldType);
                        }
                        if (((objType == correctIReflect) || info2.IsStatic) || DoesTargetObjectMatch(o, info2))
                        {
                            VerifyObjRefPresentForInstanceCall(o, info2);
                            info2.SetValue(o, obj3);
                            return;
                        }
                        InvokeMemberOnIReflect(correctIReflect, info2, BindingFlags.SetField, o, new object[] { obj3 });
                        return;
                    }
                    if (args.Length > 1)
                    {
                        VBBinder.SecurityCheckForLateboundCalls(member, objType, correctIReflect);
                        object obj4 = null;
                        if (((objType == correctIReflect) || ((FieldInfo) member).IsStatic) || DoesTargetObjectMatch(o, member))
                        {
                            VerifyObjRefPresentForInstanceCall(o, member);
                            obj4 = ((FieldInfo) member).GetValue(o);
                        }
                        else
                        {
                            obj4 = InvokeMemberOnIReflect(correctIReflect, member, BindingFlags.GetField, o, new object[] { obj4 });
                        }
                        LateIndexSet(obj4, args, paramnames);
                        return;
                    }
                }
            }
            VBBinder binder = new VBBinder(null);
            if (OptimisticSet && (args.GetUpperBound(0) > 0))
            {
                BindingFlags bindingAttr = BindingFlags.OptionalParamBinding | BindingFlags.GetProperty | BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase;
                Type[] types = new Type[(args.GetUpperBound(0) - 1) + 1];
                int upperBound = types.GetUpperBound(0);
                for (int i = 0; i <= upperBound; i++)
                {
                    object obj5 = args[i];
                    if (obj5 == null)
                    {
                        types[i] = null;
                    }
                    else
                    {
                        types[i] = obj5.GetType();
                    }
                }
                try
                {
                    PropertyInfo info3 = correctIReflect.GetProperty(name, bindingAttr, binder, typeof(int), types, null);
                    if ((info3 != null) && info3.CanWrite)
                    {
                        goto Label_02BD;
                    }
                }
                catch (MissingMemberException)
                {
                }
                return;
            }
        Label_02BD:
            try
            {
                binder.InvokeMember(name, invokeAttr, objType, correctIReflect, o, args, paramnames);
            }
            catch when (?)
            {
                Exception exception2;
                object obj6;
                if ((args == null) || (args.Length <= 1))
                {
                    throw new MissingMemberException(Utils.GetResourceString("MissingMember_MemberNotFoundOnType2", new string[] { name, Utils.VBFriendlyName(objType, o) }));
                }
                invokeAttr = BindingFlags.OptionalParamBinding | BindingFlags.GetProperty | BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase;
                if (!objType.IsCOMObject)
                {
                    invokeAttr |= BindingFlags.GetField;
                }
                try
                {
                    obj6 = binder.InvokeMember(name, invokeAttr, objType, correctIReflect, o, null, null);
                }
                catch when (?)
                {
                    throw exception2;
                }
                catch (AccessViolationException exception4)
                {
                    throw exception4;
                }
                catch (StackOverflowException exception5)
                {
                    throw exception5;
                }
                catch (OutOfMemoryException exception6)
                {
                    throw exception6;
                }
                catch (ThreadAbortException exception7)
                {
                    throw exception7;
                }
                catch (Exception)
                {
                    obj6 = null;
                }
                if (obj6 == null)
                {
                    throw new MissingMemberException(Utils.GetResourceString("MissingMember_MemberNotFoundOnType2", new string[] { name, Utils.VBFriendlyName(objType, o) }));
                }
                try
                {
                    LateIndexSet(obj6, args, paramnames);
                }
                catch when (?)
                {
                    throw exception2;
                }
            }
            catch (TargetInvocationException exception10)
            {
                if (exception10.InnerException == null)
                {
                    throw exception10;
                }
                if (!(exception10.InnerException is TargetParameterCountException))
                {
                    throw exception10.InnerException;
                }
                if ((invokeAttr & BindingFlags.PutRefDispProperty) != BindingFlags.Default)
                {
                    throw new MissingMemberException(Utils.GetResourceString("MissingMember_MemberSetNotFoundOnType2", new string[] { name, Utils.VBFriendlyName(objType, o) }));
                }
                throw new MissingMemberException(Utils.GetResourceString("MissingMember_MemberLetNotFoundOnType2", new string[] { name, Utils.VBFriendlyName(objType, o) }));
            }
        }

        internal static object InvokeMemberOnIReflect(IReflect objIReflect, MemberInfo member, BindingFlags flags, object target, object[] args)
        {
            VBBinder binder = new VBBinder(null);
            binder.CacheMember(member);
            return objIReflect.InvokeMember(member.Name, (BindingFlags.OptionalParamBinding | BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase) | flags, binder, target, args, null, null, null);
        }

        private static bool IsMissingMemberException(Exception ex)
        {
            if (ex is MissingMemberException)
            {
                return true;
            }
            if (ex is MemberAccessException)
            {
                return true;
            }
            COMException exception = ex as COMException;
            if (exception != null)
            {
                if (exception.ErrorCode == -2147352570)
                {
                    return true;
                }
                if (exception.ErrorCode == -2146827850)
                {
                    return true;
                }
            }
            else if (((ex is TargetInvocationException) && (ex.InnerException is COMException)) && (((COMException) ex.InnerException).ErrorCode == -2147352559))
            {
                return true;
            }
            return false;
        }

        [DebuggerStepThrough, DebuggerHidden]
        public static void LateCall(object o, Type objType, string name, object[] args, string[] paramnames, bool[] CopyBack)
        {
            InternalLateCall(o, objType, name, args, paramnames, CopyBack, true);
        }

        [DebuggerStepThrough, DebuggerHidden]
        public static object LateGet(object o, Type objType, string name, object[] args, string[] paramnames, bool[] CopyBack)
        {
            object obj2;
            BindingFlags invokeAttr = BindingFlags.OptionalParamBinding | BindingFlags.GetProperty | BindingFlags.InvokeMethod | BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase;
            if (objType == null)
            {
                if (o == null)
                {
                    throw ExceptionUtils.VbMakeException(0x5b);
                }
                objType = o.GetType();
            }
            IReflect correctIReflect = GetCorrectIReflect(o, objType);
            if (name == null)
            {
                name = "";
            }
            if (objType.IsCOMObject)
            {
                CheckForClassExtendingCOMClass(objType);
            }
            else
            {
                MemberInfo member = GetMostDerivedMemberInfo(correctIReflect, name, invokeAttr | BindingFlags.GetField);
                if ((member != null) && (member.MemberType == MemberTypes.Field))
                {
                    object obj3;
                    VBBinder.SecurityCheckForLateboundCalls(member, objType, correctIReflect);
                    if (((objType == correctIReflect) || ((FieldInfo) member).IsStatic) || DoesTargetObjectMatch(o, member))
                    {
                        VerifyObjRefPresentForInstanceCall(o, member);
                        obj3 = ((FieldInfo) member).GetValue(o);
                    }
                    else
                    {
                        obj3 = InvokeMemberOnIReflect(correctIReflect, member, BindingFlags.GetField, o, null);
                    }
                    if ((args != null) && (args.Length != 0))
                    {
                        return LateIndexGet(obj3, args, paramnames);
                    }
                    return obj3;
                }
            }
            VBBinder binder = new VBBinder(CopyBack);
            try
            {
                obj2 = binder.InvokeMember(name, invokeAttr, objType, correctIReflect, o, args, paramnames);
            }
            catch when (?)
            {
                if (objType.IsCOMObject || ((args != null) && (args.Length > 0)))
                {
                    object obj4;
                    invokeAttr = BindingFlags.OptionalParamBinding | BindingFlags.GetProperty | BindingFlags.InvokeMethod | BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase;
                    if (!objType.IsCOMObject)
                    {
                        invokeAttr |= BindingFlags.GetField;
                    }
                    try
                    {
                        obj4 = binder.InvokeMember(name, invokeAttr, objType, correctIReflect, o, null, null);
                    }
                    catch (AccessViolationException exception2)
                    {
                        throw exception2;
                    }
                    catch (StackOverflowException exception3)
                    {
                        throw exception3;
                    }
                    catch (OutOfMemoryException exception4)
                    {
                        throw exception4;
                    }
                    catch (ThreadAbortException exception5)
                    {
                        throw exception5;
                    }
                    catch (Exception)
                    {
                        obj4 = null;
                    }
                    if (obj4 == null)
                    {
                        throw new MissingMemberException(Utils.GetResourceString("MissingMember_MemberNotFoundOnType2", new string[] { name, Utils.VBFriendlyName(objType, o) }));
                    }
                    try
                    {
                        return LateIndexGet(obj4, args, paramnames);
                    }
                    catch when (?)
                    {
                        Exception exception;
                        throw exception;
                    }
                }
                throw new MissingMemberException(Utils.GetResourceString("MissingMember_MemberNotFoundOnType2", new string[] { name, Utils.VBFriendlyName(objType, o) }));
            }
            catch (TargetInvocationException exception7)
            {
                throw exception7.InnerException;
            }
            return obj2;
        }

        [DebuggerStepThrough, DebuggerHidden]
        public static object LateIndexGet(object o, object[] args, string[] paramnames)
        {
            string defaultName = null;
            object obj2;
            if (o == null)
            {
                throw ExceptionUtils.VbMakeException(0x5b);
            }
            Type objType = o.GetType();
            IReflect correctIReflect = GetCorrectIReflect(o, objType);
            if (objType.IsArray)
            {
                if ((paramnames != null) && (paramnames.Length != 0))
                {
                    throw new ArgumentException(Utils.GetResourceString("Argument_InvalidNamedArgs"));
                }
                Array array = (Array) o;
                int length = args.Length;
                if (length != array.Rank)
                {
                    throw new RankException();
                }
                if (length == 1)
                {
                    return array.GetValue(Conversions.ToInteger(args[0]));
                }
                if (length == 2)
                {
                    return array.GetValue(Conversions.ToInteger(args[0]), Conversions.ToInteger(args[1]));
                }
                int[] indices = new int[(length - 1) + 1];
                int num5 = length - 1;
                for (int i = 0; i <= num5; i++)
                {
                    indices[i] = Conversions.ToInteger(args[i]);
                }
                return array.GetValue(indices);
            }
            MethodBase[] match = null;
            BindingFlags invokeAttr = BindingFlags.OptionalParamBinding | BindingFlags.GetProperty | BindingFlags.InvokeMethod | BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase;
            if (!objType.IsCOMObject)
            {
                int num3;
                int num4;
                if ((args == null) || (args.Length == 0))
                {
                    invokeAttr |= BindingFlags.GetField;
                }
                MemberInfo[] infoArray = GetDefaultMembers(objType, correctIReflect, ref defaultName);
                if (infoArray != null)
                {
                    int upperBound = infoArray.GetUpperBound(0);
                    for (num3 = 0; num3 <= upperBound; num3++)
                    {
                        MemberInfo getMethod = infoArray[num3];
                        if (getMethod.MemberType == MemberTypes.Property)
                        {
                            getMethod = ((PropertyInfo) getMethod).GetGetMethod();
                        }
                        if ((getMethod != null) && (getMethod.MemberType != MemberTypes.Field))
                        {
                            infoArray[num4] = getMethod;
                            num4++;
                        }
                    }
                }
                if ((infoArray == null) | (num4 == 0))
                {
                    throw new MissingMemberException(Utils.GetResourceString("MissingMember_NoDefaultMemberFound1", new string[] { Utils.VBFriendlyName(objType, o) }));
                }
                match = new MethodBase[(num4 - 1) + 1];
                int num7 = num4 - 1;
                for (num3 = 0; num3 <= num7; num3++)
                {
                    try
                    {
                        match[num3] = (MethodBase) infoArray[num3];
                    }
                    catch (StackOverflowException exception)
                    {
                        throw exception;
                    }
                    catch (OutOfMemoryException exception2)
                    {
                        throw exception2;
                    }
                    catch (ThreadAbortException exception3)
                    {
                        throw exception3;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            else
            {
                CheckForClassExtendingCOMClass(objType);
            }
            VBBinder binder = new VBBinder(null);
            try
            {
                object obj3;
                if (objType.IsCOMObject)
                {
                    return binder.InvokeMember("", invokeAttr, objType, correctIReflect, o, args, paramnames);
                }
                object objState = null;
                binder.m_BindToName = defaultName;
                binder.m_objType = objType;
                MethodBase member = binder.BindToMethod(invokeAttr, match, ref args, null, null, paramnames, ref objState);
                VBBinder.SecurityCheckForLateboundCalls(member, objType, correctIReflect);
                if (((objType == correctIReflect) || member.IsStatic) || DoesTargetObjectMatch(o, member))
                {
                    VerifyObjRefPresentForInstanceCall(o, member);
                    obj3 = member.Invoke(o, args);
                }
                else
                {
                    obj3 = InvokeMemberOnIReflect(correctIReflect, member, BindingFlags.InvokeMethod, o, args);
                }
                binder.ReorderArgumentArray(ref args, objState);
                obj2 = obj3;
            }
            catch when (?)
            {
                throw new MissingMemberException(Utils.GetResourceString("MissingMember_NoDefaultMemberFound1", new string[] { Utils.VBFriendlyName(objType, o) }));
            }
            catch (TargetInvocationException exception6)
            {
                throw exception6.InnerException;
            }
            return obj2;
        }

        [DebuggerStepThrough, DebuggerHidden]
        public static void LateIndexSet(object o, object[] args, string[] paramnames)
        {
            string defaultName = null;
            if (o == null)
            {
                throw ExceptionUtils.VbMakeException(0x5b);
            }
            Type objType = o.GetType();
            IReflect correctIReflect = GetCorrectIReflect(o, objType);
            if (objType.IsArray)
            {
                if ((paramnames != null) && (paramnames.Length != 0))
                {
                    throw new ArgumentException(Utils.GetResourceString("Argument_InvalidNamedArgs"));
                }
                Array array = (Array) o;
                int index = args.Length - 1;
                object obj2 = args[index];
                if (obj2 != null)
                {
                    Type elementType = objType.GetElementType();
                    if (obj2.GetType() != elementType)
                    {
                        obj2 = ObjectType.CTypeHelper(obj2, elementType);
                    }
                }
                if (index != array.Rank)
                {
                    throw new RankException();
                }
                if (index == 1)
                {
                    array.SetValue(obj2, Conversions.ToInteger(args[0]));
                }
                else if (index == 2)
                {
                    array.SetValue(obj2, Conversions.ToInteger(args[0]), Conversions.ToInteger(args[1]));
                }
                else
                {
                    int[] indices = new int[(index - 1) + 1];
                    int num5 = index - 1;
                    for (int i = 0; i <= num5; i++)
                    {
                        indices[i] = Conversions.ToInteger(args[i]);
                    }
                    array.SetValue(obj2, indices);
                }
            }
            else
            {
                MethodBase[] match = null;
                BindingFlags invokeAttr = BindingFlags.OptionalParamBinding | BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase;
                if (objType.IsCOMObject)
                {
                    CheckForClassExtendingCOMClass(objType);
                    invokeAttr |= GetPropertyPutFlags(args[args.GetUpperBound(0)]);
                }
                else
                {
                    int num3;
                    int num4;
                    invokeAttr |= BindingFlags.SetProperty;
                    if (args.Length == 1)
                    {
                        invokeAttr |= BindingFlags.SetField;
                    }
                    MemberInfo[] infoArray = GetDefaultMembers(objType, correctIReflect, ref defaultName);
                    if (infoArray != null)
                    {
                        int upperBound = infoArray.GetUpperBound(0);
                        for (num3 = 0; num3 <= upperBound; num3++)
                        {
                            MemberInfo setMethod = infoArray[num3];
                            if (setMethod.MemberType == MemberTypes.Property)
                            {
                                setMethod = ((PropertyInfo) setMethod).GetSetMethod();
                            }
                            if ((setMethod != null) && (setMethod.MemberType != MemberTypes.Field))
                            {
                                infoArray[num4] = setMethod;
                                num4++;
                            }
                        }
                    }
                    if ((infoArray == null) | (num4 == 0))
                    {
                        throw new MissingMemberException(Utils.GetResourceString("MissingMember_NoDefaultMemberFound1", new string[] { Utils.VBFriendlyName(objType, o) }));
                    }
                    match = new MethodBase[(num4 - 1) + 1];
                    int num7 = num4 - 1;
                    for (num3 = 0; num3 <= num7; num3++)
                    {
                        try
                        {
                            match[num3] = (MethodBase) infoArray[num3];
                        }
                        catch (StackOverflowException exception)
                        {
                            throw exception;
                        }
                        catch (OutOfMemoryException exception2)
                        {
                            throw exception2;
                        }
                        catch (ThreadAbortException exception3)
                        {
                            throw exception3;
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                VBBinder binder = new VBBinder(null);
                try
                {
                    if (objType.IsCOMObject)
                    {
                        binder.InvokeMember("", invokeAttr, objType, correctIReflect, o, args, paramnames);
                    }
                    else
                    {
                        object objState = null;
                        binder.m_BindToName = defaultName;
                        binder.m_objType = objType;
                        MethodBase member = binder.BindToMethod(invokeAttr, match, ref args, null, null, paramnames, ref objState);
                        VBBinder.SecurityCheckForLateboundCalls(member, objType, correctIReflect);
                        if (((objType == correctIReflect) || member.IsStatic) || DoesTargetObjectMatch(o, member))
                        {
                            VerifyObjRefPresentForInstanceCall(o, member);
                            member.Invoke(o, args);
                        }
                        else
                        {
                            InvokeMemberOnIReflect(correctIReflect, member, BindingFlags.InvokeMethod, o, args);
                        }
                        binder.ReorderArgumentArray(ref args, objState);
                    }
                }
                catch when (?)
                {
                    throw new MissingMemberException(Utils.GetResourceString("MissingMember_NoDefaultMemberFound1", new string[] { Utils.VBFriendlyName(objType, o) }));
                }
                catch (TargetInvocationException exception5)
                {
                    throw exception5.InnerException;
                }
            }
        }

        [DebuggerStepThrough, DebuggerHidden]
        public static void LateIndexSetComplex(object o, object[] args, string[] paramnames, bool OptimisticSet, bool RValueBase)
        {
            try
            {
                LateIndexSet(o, args, paramnames);
                if (RValueBase && o.GetType().IsValueType)
                {
                    throw new Exception(Utils.GetResourceString("RValueBaseForValueType", new string[] { o.GetType().Name, o.GetType().Name }));
                }
            }
            catch when (?)
            {
            }
        }

        [DebuggerStepThrough, DebuggerHidden]
        public static void LateSet(object o, Type objType, string name, object[] args, string[] paramnames)
        {
            InternalLateSet(o, ref objType, name, args, paramnames, false, (CallType) 0);
        }

        [DebuggerStepThrough, DebuggerHidden]
        public static void LateSetComplex(object o, Type objType, string name, object[] args, string[] paramnames, bool OptimisticSet, bool RValueBase)
        {
            try
            {
                InternalLateSet(o, ref objType, name, args, paramnames, OptimisticSet, (CallType) 0);
                if (RValueBase && objType.IsValueType)
                {
                    throw new Exception(Utils.GetResourceString("RValueBaseForValueType", new string[] { Utils.VBFriendlyName(objType, o), Utils.VBFriendlyName(objType, o) }));
                }
            }
            catch when (?)
            {
            }
        }

        internal static bool LegacyIsGeneric(MemberInfo Member)
        {
            MethodBase base2 = Member as MethodBase;
            if (base2 == null)
            {
                return false;
            }
            return base2.IsGenericMethod;
        }

        private static bool MemberIsField(MemberInfo[] mi)
        {
            int upperBound = mi.GetUpperBound(0);
            for (int i = 0; i <= upperBound; i++)
            {
                info = mi[i];
                if ((info != null) && (info.MemberType == MemberTypes.Field))
                {
                    int num4 = mi.GetUpperBound(0);
                    for (int j = 0; j <= num4; j++)
                    {
                        if (((i != j) && (mi[j] != null)) && info.DeclaringType.IsSubclassOf(mi[j].DeclaringType))
                        {
                            mi[j] = null;
                        }
                    }
                }
            }
            foreach (MemberInfo info in mi)
            {
                if ((info != null) && (info.MemberType != MemberTypes.Field))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool NoByrefs(ParameterInfo[] parameters)
        {
            int num2 = parameters.Length - 1;
            for (int i = 0; i <= num2; i++)
            {
                if (parameters[i].ParameterType.IsByRef)
                {
                    return false;
                }
            }
            return true;
        }

        internal static void VerifyObjRefPresentForInstanceCall(object Value, MemberInfo Member)
        {
            if (Value == null)
            {
                bool isStatic = true;
                switch (Member.MemberType)
                {
                    case MemberTypes.Constructor:
                        isStatic = ((ConstructorInfo) Member).IsStatic;
                        break;

                    case MemberTypes.Field:
                        isStatic = ((FieldInfo) Member).IsStatic;
                        break;

                    case MemberTypes.Method:
                        isStatic = ((MethodInfo) Member).IsStatic;
                        break;
                }
                if (!isStatic)
                {
                    throw new NullReferenceException(Utils.GetResourceString("NullReference_InstanceReqToAccessMember1", new string[] { Utils.MemberToString(Member) }));
                }
            }
        }
    }
}

