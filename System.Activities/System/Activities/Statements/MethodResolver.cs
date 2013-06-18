namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal sealed class MethodResolver
    {
        private MethodInfo beginMethod;
        private MethodInfo endMethod;
        private static readonly BindingFlags instanceBindingFlags = (BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance);
        private static readonly string instanceString = "instance";
        private static readonly BindingFlags staticBindingFlags = (BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static);
        private static readonly string staticString = "static";
        private MethodInfo syncMethod;

        public void DetermineMethodInfo(CodeActivityMetadata metadata, out MethodExecutor methodExecutor)
        {
            bool flag = false;
            methodExecutor = null;
            if (string.IsNullOrEmpty(this.MethodName))
            {
                metadata.AddValidationError(System.Activities.SR.ActivityPropertyMustBeSet("MethodName", this.Parent.DisplayName));
                flag = true;
            }
            Type targetType = this.TargetType;
            if (((targetType != null) && (this.TargetObject != null)) && !this.TargetObject.IsEmpty)
            {
                metadata.AddValidationError(System.Activities.SR.TargetTypeAndTargetObjectAreMutuallyExclusive(this.Parent.GetType().Name, this.Parent.DisplayName));
                flag = true;
            }
            BindingFlags bindingFlags = (this.TargetType != null) ? staticBindingFlags : instanceBindingFlags;
            string str = (bindingFlags == staticBindingFlags) ? staticString : instanceString;
            if (targetType == null)
            {
                if ((this.TargetObject != null) && !this.TargetObject.IsEmpty)
                {
                    targetType = this.TargetObject.ArgumentType;
                }
                else
                {
                    metadata.AddValidationError(System.Activities.SR.OneOfTwoPropertiesMustBeSet("TargetObject", "TargetType", this.Parent.GetType().Name, this.Parent.DisplayName));
                    flag = true;
                }
            }
            if (!flag)
            {
                MethodInfo info;
                Type[] parameterTypes = this.Parameters.Select<Argument, Type>(delegate (Argument argument) {
                    if (argument.Direction != ArgumentDirection.In)
                    {
                        return argument.ArgumentType.MakeByRefType();
                    }
                    return argument.ArgumentType;
                }).ToArray<Type>();
                Type[] genericTypeArguments = this.GenericTypeArguments.ToArray<Type>();
                InheritanceAndParamArrayAwareBinder methodBinder = new InheritanceAndParamArrayAwareBinder(targetType, genericTypeArguments, this.Parent);
                Type resultType = this.ResultType;
                if (this.RunAsynchronously)
                {
                    int length = parameterTypes.Length;
                    Type[] typeArray3 = new Type[length + 2];
                    for (int i = 0; i < length; i++)
                    {
                        typeArray3[i] = parameterTypes[i];
                    }
                    typeArray3[length] = typeof(AsyncCallback);
                    typeArray3[length + 1] = typeof(object);
                    Type[] typeArray4 = new Type[] { typeof(IAsyncResult) };
                    this.beginMethod = this.Resolve(targetType, "Begin" + this.MethodName, bindingFlags, methodBinder, typeArray3, genericTypeArguments, true);
                    if ((this.beginMethod != null) && !this.beginMethod.ReturnType.Equals(typeof(IAsyncResult)))
                    {
                        this.beginMethod = null;
                    }
                    this.endMethod = this.Resolve(targetType, "End" + this.MethodName, bindingFlags, methodBinder, typeArray4, genericTypeArguments, true);
                    if (((this.endMethod != null) && (resultType != null)) && !System.Runtime.TypeHelper.AreTypesCompatible(this.endMethod.ReturnType, resultType))
                    {
                        this.endMethod = null;
                        metadata.AddValidationError(System.Activities.SR.ReturnTypeIncompatible(this.endMethod.ReturnType.Name, this.MethodName, targetType.Name, this.Parent.DisplayName, resultType.Name));
                        return;
                    }
                    if (((this.beginMethod != null) && (this.endMethod != null)) && (this.beginMethod.IsStatic == this.endMethod.IsStatic))
                    {
                        methodExecutor = new AsyncPatternMethodExecutor(this.beginMethod, this.endMethod, this.Parent, this.TargetType, this.TargetObject, this.Parameters, this.Result);
                        return;
                    }
                }
                try
                {
                    info = this.Resolve(targetType, this.MethodName, bindingFlags, methodBinder, parameterTypes, genericTypeArguments, false);
                }
                catch (AmbiguousMatchException)
                {
                    metadata.AddValidationError(System.Activities.SR.DuplicateMethodFound(targetType.Name, str, this.MethodName, this.Parent.DisplayName));
                    return;
                }
                if (info != null)
                {
                    if ((resultType != null) && !System.Runtime.TypeHelper.AreTypesCompatible(info.ReturnType, resultType))
                    {
                        metadata.AddValidationError(System.Activities.SR.ReturnTypeIncompatible(info.ReturnType.Name, this.MethodName, targetType.Name, this.Parent.DisplayName, resultType.Name));
                    }
                    else
                    {
                        this.syncMethod = info;
                        if (this.RunAsynchronously)
                        {
                            methodExecutor = new AsyncWaitCallbackMethodExecutor(info, this.Parent, this.TargetType, this.TargetObject, this.Parameters, this.Result);
                        }
                        else
                        {
                            methodExecutor = new SyncMethodExecutor(info, this.Parent, this.TargetType, this.TargetObject, this.Parameters, this.Result);
                        }
                    }
                }
                else
                {
                    metadata.AddValidationError(System.Activities.SR.PublicMethodWithMatchingParameterDoesNotExist(targetType.Name, str, this.MethodName, this.Parent.DisplayName));
                }
            }
        }

        private static bool HaveParameterArray(ParameterInfo[] parameters)
        {
            if (parameters.Length > 0)
            {
                ParameterInfo info = parameters[parameters.Length - 1];
                return (info.GetCustomAttributes(typeof(ParamArrayAttribute), true).Length > 0);
            }
            return false;
        }

        private static MethodInfo Instantiate(MethodInfo method, Type[] genericTypeArguments)
        {
            if (method.ContainsGenericParameters && (method.GetGenericArguments().Length == genericTypeArguments.Length))
            {
                try
                {
                    return method.MakeGenericMethod(genericTypeArguments);
                }
                catch (ArgumentException)
                {
                    return null;
                }
            }
            return null;
        }

        public void RegisterParameters(IList<RuntimeArgument> arguments)
        {
            bool flag = (this.RunAsynchronously && (this.beginMethod != null)) && (this.endMethod != null);
            if ((this.syncMethod != null) || flag)
            {
                ParameterInfo[] parameters;
                int length;
                string name = "";
                bool flag2 = false;
                if (flag)
                {
                    parameters = this.beginMethod.GetParameters();
                    length = parameters.Length - 2;
                }
                else
                {
                    parameters = this.syncMethod.GetParameters();
                    flag2 = HaveParameterArray(parameters);
                    if (flag2)
                    {
                        length = parameters.Length - 1;
                        name = parameters[length].Name;
                    }
                    else
                    {
                        length = parameters.Length;
                    }
                }
                for (int i = 0; i < length; i++)
                {
                    int num3;
                    string str2 = parameters[i].Name;
                    if (string.IsNullOrEmpty(str2))
                    {
                        str2 = "Parameter" + i;
                    }
                    RuntimeArgument argument = new RuntimeArgument(str2, this.Parameters[i].ArgumentType, this.Parameters[i].Direction, true);
                    Argument.Bind(this.Parameters[i], argument);
                    arguments.Add(argument);
                    if ((!flag && flag2) && (str2.StartsWith(name, false, null) && int.TryParse(str2.Substring(name.Length), NumberStyles.Integer, NumberFormatInfo.CurrentInfo, out num3)))
                    {
                        name = name + "_";
                    }
                }
                if (!flag && flag2)
                {
                    int num4 = this.Parameters.Count - length;
                    for (int j = 0; j < num4; j++)
                    {
                        string str3 = name + j;
                        int num6 = length + j;
                        RuntimeArgument argument2 = new RuntimeArgument(str3, this.Parameters[num6].ArgumentType, this.Parameters[num6].Direction, true);
                        Argument.Bind(this.Parameters[num6], argument2);
                        arguments.Add(argument2);
                    }
                }
            }
            else
            {
                for (int k = 0; k < this.Parameters.Count; k++)
                {
                    RuntimeArgument argument3 = new RuntimeArgument("argument" + k, this.Parameters[k].ArgumentType, this.Parameters[k].Direction, true);
                    Argument.Bind(this.Parameters[k], argument3);
                    arguments.Add(argument3);
                }
            }
        }

        private MethodInfo Resolve(Type targetType, string methodName, BindingFlags bindingFlags, InheritanceAndParamArrayAwareBinder methodBinder, Type[] parameterTypes, Type[] genericTypeArguments, bool suppressAmbiguityException)
        {
            MethodInfo info;
            try
            {
                methodBinder.SelectMethodCalled = false;
                info = targetType.GetMethod(methodName, bindingFlags, methodBinder, CallingConventions.Any, parameterTypes, null);
            }
            catch (AmbiguousMatchException)
            {
                if (!suppressAmbiguityException)
                {
                    throw;
                }
                return null;
            }
            if (((info != null) && !methodBinder.SelectMethodCalled) && (genericTypeArguments.Length > 0))
            {
                info = Instantiate(info, genericTypeArguments);
            }
            return info;
        }

        public void Trace()
        {
            if ((this.RunAsynchronously && (this.beginMethod != null)) && (this.endMethod != null))
            {
                if (TD.InvokeMethodUseAsyncPatternIsEnabled())
                {
                    TD.InvokeMethodUseAsyncPattern(this.Parent.DisplayName, this.beginMethod.ToString(), this.endMethod.ToString());
                }
            }
            else if (this.RunAsynchronously && TD.InvokeMethodDoesNotUseAsyncPatternIsEnabled())
            {
                TD.InvokeMethodDoesNotUseAsyncPattern(this.Parent.DisplayName);
            }
        }

        public Collection<Type> GenericTypeArguments { get; set; }

        public string MethodName { get; set; }

        public Collection<Argument> Parameters { get; set; }

        public Activity Parent { get; set; }

        public RuntimeArgument Result { get; set; }

        internal Type ResultType { get; set; }

        public bool RunAsynchronously { get; set; }

        public InArgument TargetObject { get; set; }

        public Type TargetType { get; set; }

        private class AsyncPatternMethodExecutor : MethodExecutor
        {
            private MethodInfo beginMethod;
            private MethodInfo endMethod;

            public AsyncPatternMethodExecutor(MethodInfo beginMethod, MethodInfo endMethod, Activity invokingActivity, Type targetType, InArgument targetObject, Collection<Argument> parameters, RuntimeArgument returnObject) : base(invokingActivity, targetType, targetObject, parameters, returnObject)
            {
                this.beginMethod = beginMethod;
                this.endMethod = endMethod;
            }

            protected override IAsyncResult BeginMakeMethodCall(AsyncCodeActivityContext context, object target, AsyncCallback callback, object state)
            {
                MethodResolver.InvokeMethodInstanceData data = new MethodResolver.InvokeMethodInstanceData {
                    TargetObject = target,
                    ActualParameters = base.EvaluateAndPackParameters(context, this.beginMethod, true)
                };
                int length = data.ActualParameters.Length;
                data.ActualParameters[length - 2] = callback;
                data.ActualParameters[length - 1] = state;
                context.UserState = data;
                return (IAsyncResult) base.InvokeAndUnwrapExceptions(this.beginMethod, target, data.ActualParameters);
            }

            protected override void EndMakeMethodCall(AsyncCodeActivityContext context, IAsyncResult result)
            {
                MethodResolver.InvokeMethodInstanceData userState = (MethodResolver.InvokeMethodInstanceData) context.UserState;
                userState.ReturnValue = base.InvokeAndUnwrapExceptions(this.endMethod, userState.TargetObject, new object[] { result });
                base.SetOutArgumentAndReturnValue(context, userState.ReturnValue, userState.ActualParameters);
            }

            public override bool MethodIsStatic
            {
                get
                {
                    return this.beginMethod.IsStatic;
                }
            }
        }

        private class AsyncWaitCallbackMethodExecutor : MethodExecutor
        {
            private MethodInfo asyncMethod;

            public AsyncWaitCallbackMethodExecutor(MethodInfo asyncMethod, Activity invokingActivity, Type targetType, InArgument targetObject, Collection<Argument> parameters, RuntimeArgument returnObject) : base(invokingActivity, targetType, targetObject, parameters, returnObject)
            {
                this.asyncMethod = asyncMethod;
            }

            protected override IAsyncResult BeginMakeMethodCall(AsyncCodeActivityContext context, object target, AsyncCallback callback, object state)
            {
                MethodResolver.InvokeMethodInstanceData instance = new MethodResolver.InvokeMethodInstanceData {
                    TargetObject = target,
                    ActualParameters = base.EvaluateAndPackParameters(context, this.asyncMethod, false)
                };
                return new ExecuteAsyncResult(instance, this, callback, state);
            }

            protected override void EndMakeMethodCall(AsyncCodeActivityContext context, IAsyncResult result)
            {
                MethodResolver.InvokeMethodInstanceData data = ExecuteAsyncResult.End(result);
                if (data.ExceptionWasThrown)
                {
                    throw FxTrace.Exception.AsError(data.Exception);
                }
                base.SetOutArgumentAndReturnValue(context, data.ReturnValue, data.ActualParameters);
            }

            public override bool MethodIsStatic
            {
                get
                {
                    return this.asyncMethod.IsStatic;
                }
            }

            private class ExecuteAsyncResult : AsyncResult
            {
                private static Action<object> asyncExecute = new Action<object>(MethodResolver.AsyncWaitCallbackMethodExecutor.ExecuteAsyncResult.AsyncExecute);
                private MethodResolver.AsyncWaitCallbackMethodExecutor executor;
                private MethodResolver.InvokeMethodInstanceData instance;

                public ExecuteAsyncResult(MethodResolver.InvokeMethodInstanceData instance, MethodResolver.AsyncWaitCallbackMethodExecutor executor, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.instance = instance;
                    this.executor = executor;
                    ActionItem.Schedule(asyncExecute, this);
                }

                private static void AsyncExecute(object state)
                {
                    ((MethodResolver.AsyncWaitCallbackMethodExecutor.ExecuteAsyncResult) state).AsyncExecuteCore();
                }

                private void AsyncExecuteCore()
                {
                    try
                    {
                        this.instance.ReturnValue = this.executor.InvokeAndUnwrapExceptions(this.executor.asyncMethod, this.instance.TargetObject, this.instance.ActualParameters);
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        this.instance.Exception = exception;
                        this.instance.ExceptionWasThrown = true;
                    }
                    base.Complete(false);
                }

                public static MethodResolver.InvokeMethodInstanceData End(IAsyncResult result)
                {
                    return AsyncResult.End<MethodResolver.AsyncWaitCallbackMethodExecutor.ExecuteAsyncResult>(result).instance;
                }
            }
        }

        private class InheritanceAndParamArrayAwareBinder : Binder
        {
            private Type declaringType;
            private Type[] genericTypeArguments;
            private Activity parentActivity;
            internal bool SelectMethodCalled;

            public InheritanceAndParamArrayAwareBinder(Type declaringType, Type[] genericTypeArguments, Activity parentActivity)
            {
                this.declaringType = declaringType;
                this.genericTypeArguments = genericTypeArguments;
                this.parentActivity = parentActivity;
            }

            public override FieldInfo BindToField(BindingFlags bindingAttr, FieldInfo[] match, object value, CultureInfo culture)
            {
                throw FxTrace.Exception.AsError(new NotImplementedException());
            }

            public override MethodBase BindToMethod(BindingFlags bindingAttr, MethodBase[] match, ref object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] names, out object state)
            {
                throw FxTrace.Exception.AsError(new NotImplementedException());
            }

            public override object ChangeType(object value, Type type, CultureInfo culture)
            {
                throw FxTrace.Exception.AsError(new NotImplementedException());
            }

            private MethodBase FindMatch(MethodBase[] methodCandidates, BindingFlags bindingAttr, Type[] types, ParameterModifier[] modifiers)
            {
                MethodBase base2 = Type.DefaultBinder.SelectMethod(bindingAttr, methodCandidates, types, modifiers);
                if (base2 == null)
                {
                    foreach (MethodBase base3 in methodCandidates)
                    {
                        MethodInfo info = base3 as MethodInfo;
                        ParameterInfo[] parameters = info.GetParameters();
                        if (MethodResolver.HaveParameterArray(parameters))
                        {
                            Type elementType = parameters[parameters.Length - 1].ParameterType.GetElementType();
                            bool flag = true;
                            for (int i = parameters.Length - 1; i < (types.Length - 1); i++)
                            {
                                if (!System.Runtime.TypeHelper.AreTypesCompatible(types[i], elementType))
                                {
                                    flag = false;
                                    break;
                                }
                            }
                            if (flag)
                            {
                                Type[] typeArray = new Type[parameters.Length];
                                for (int j = 0; j < (typeArray.Length - 1); j++)
                                {
                                    typeArray[j] = types[j];
                                }
                                typeArray[typeArray.Length - 1] = elementType.MakeArrayType();
                                MethodBase base4 = Type.DefaultBinder.SelectMethod(bindingAttr, new MethodBase[] { info }, typeArray, modifiers);
                                if ((base2 != null) && (base4 != null))
                                {
                                    string name = base4.ReflectedType.Name;
                                    string str2 = base4.Name;
                                    string str3 = (bindingAttr == MethodResolver.staticBindingFlags) ? MethodResolver.staticString : MethodResolver.instanceString;
                                    throw FxTrace.Exception.AsError(new AmbiguousMatchException(System.Activities.SR.DuplicateMethodFound(name, str3, str2, this.parentActivity.DisplayName)));
                                }
                                base2 = base4;
                            }
                        }
                    }
                }
                return base2;
            }

            public override void ReorderArgumentArray(ref object[] args, object state)
            {
                throw FxTrace.Exception.AsError(new NotImplementedException());
            }

            public override MethodBase SelectMethod(BindingFlags bindingAttr, MethodBase[] match, Type[] types, ParameterModifier[] modifiers)
            {
                MethodBase[] baseArray;
                Func<MethodBase, bool> predicate = null;
                this.SelectMethodCalled = true;
                if (this.genericTypeArguments.Length > 0)
                {
                    Collection<MethodBase> source = new Collection<MethodBase>();
                    foreach (MethodBase base2 in match)
                    {
                        MethodInfo item = MethodResolver.Instantiate((MethodInfo) base2, this.genericTypeArguments);
                        if (item != null)
                        {
                            source.Add(item);
                        }
                    }
                    baseArray = source.ToArray<MethodBase>();
                }
                else
                {
                    baseArray = (from m in match
                        where !m.ContainsGenericParameters
                        select m).ToArray<MethodBase>();
                }
                if (baseArray.Length == 0)
                {
                    return null;
                }
                Type declaringType = this.declaringType;
                MethodBase base3 = null;
                do
                {
                    if (predicate == null)
                    {
                        predicate = mb => mb.DeclaringType == declaringType;
                    }
                    MethodBase[] methodCandidates = baseArray.Where<MethodBase>(predicate).ToArray<MethodBase>();
                    if (methodCandidates.Length > 0)
                    {
                        base3 = this.FindMatch(methodCandidates, bindingAttr, types, modifiers);
                    }
                    declaringType = declaringType.BaseType;
                }
                while ((declaringType != null) && (base3 == null));
                return base3;
            }

            public override PropertyInfo SelectProperty(BindingFlags bindingAttr, PropertyInfo[] match, Type returnType, Type[] indexes, ParameterModifier[] modifiers)
            {
                throw FxTrace.Exception.AsError(new NotImplementedException());
            }
        }

        private class InvokeMethodInstanceData
        {
            public object[] ActualParameters { get; set; }

            public System.Exception Exception { get; set; }

            public bool ExceptionWasThrown { get; set; }

            public object ReturnValue { get; set; }

            public object TargetObject { get; set; }
        }

        private class SyncMethodExecutor : MethodExecutor
        {
            private MethodInfo syncMethod;

            public SyncMethodExecutor(MethodInfo syncMethod, Activity invokingActivity, Type targetType, InArgument targetObject, Collection<Argument> parameters, RuntimeArgument returnObject) : base(invokingActivity, targetType, targetObject, parameters, returnObject)
            {
                this.syncMethod = syncMethod;
            }

            protected override IAsyncResult BeginMakeMethodCall(AsyncCodeActivityContext context, object target, AsyncCallback callback, object state)
            {
                object[] actualParameters = base.EvaluateAndPackParameters(context, this.syncMethod, false);
                object obj2 = base.InvokeAndUnwrapExceptions(this.syncMethod, target, actualParameters);
                base.SetOutArgumentAndReturnValue(context, obj2, actualParameters);
                return new CompletedAsyncResult(callback, state);
            }

            protected override void EndMakeMethodCall(AsyncCodeActivityContext context, IAsyncResult result)
            {
                CompletedAsyncResult.End(result);
            }

            public override bool MethodIsStatic
            {
                get
                {
                    return this.syncMethod.IsStatic;
                }
            }
        }
    }
}

