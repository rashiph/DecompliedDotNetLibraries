namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Runtime;

    internal abstract class MethodExecutor
    {
        private Activity invokingActivity;
        private Collection<Argument> parameters;
        private RuntimeArgument returnObject;
        private InArgument targetObject;
        private Type targetType;

        public MethodExecutor(Activity invokingActivity, Type targetType, InArgument targetObject, Collection<Argument> parameters, RuntimeArgument returnObject)
        {
            this.invokingActivity = invokingActivity;
            this.targetType = targetType;
            this.targetObject = targetObject;
            this.parameters = parameters;
            this.returnObject = returnObject;
        }

        public IAsyncResult BeginExecuteMethod(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            object target = null;
            if (!this.MethodIsStatic)
            {
                target = this.targetObject.Get(context);
                if (target == null)
                {
                    throw FxTrace.Exception.ArgumentNull("TargetObject");
                }
            }
            return this.BeginMakeMethodCall(context, target, callback, state);
        }

        protected abstract IAsyncResult BeginMakeMethodCall(AsyncCodeActivityContext context, object target, AsyncCallback callback, object state);
        public void EndExecuteMethod(AsyncCodeActivityContext context, IAsyncResult result)
        {
            this.EndMakeMethodCall(context, result);
        }

        protected abstract void EndMakeMethodCall(AsyncCodeActivityContext context, IAsyncResult result);
        protected object[] EvaluateAndPackParameters(CodeActivityContext context, MethodInfo method, bool usingAsyncPattern)
        {
            ParameterInfo[] parameters = method.GetParameters();
            int length = parameters.Length;
            object[] objArray = new object[length];
            if (usingAsyncPattern)
            {
                length -= 2;
            }
            bool flag = HaveParameterArray(parameters);
            for (int i = 0; i < length; i++)
            {
                if (((i == (length - 1)) && !usingAsyncPattern) && flag)
                {
                    int num3 = (this.parameters.Count - length) + 1;
                    if ((num3 == 1) && TypeHelper.AreTypesCompatible(this.parameters[i].ArgumentType, parameters[i].ParameterType))
                    {
                        objArray[i] = this.parameters[i].Get<object>(context);
                    }
                    else
                    {
                        objArray[i] = Activator.CreateInstance(parameters[i].ParameterType, new object[] { num3 });
                        for (int j = 0; j < num3; j++)
                        {
                            ((object[]) objArray[i])[j] = this.parameters[i + j].Get<object>(context);
                        }
                    }
                }
                else
                {
                    objArray[i] = this.parameters[i].Get<object>(context);
                }
            }
            return objArray;
        }

        private static bool HaveParameterArray(ParameterInfo[] parameters)
        {
            if (parameters.Length > 0)
            {
                ParameterInfo info = parameters[parameters.Length - 1];
                return (info.GetCustomAttributes(typeof(ParamArrayAttribute), true).GetLength(0) > 0);
            }
            return false;
        }

        internal object InvokeAndUnwrapExceptions(MethodInfo methodToInvoke, object targetInstance, object[] actualParameters)
        {
            object obj2;
            try
            {
                obj2 = methodToInvoke.Invoke(targetInstance, actualParameters);
            }
            catch (TargetInvocationException exception)
            {
                if (TD.InvokedMethodThrewExceptionIsEnabled())
                {
                    TD.InvokedMethodThrewException(this.invokingActivity.DisplayName, exception.InnerException.ToString());
                }
                throw FxTrace.Exception.AsError(exception.InnerException);
            }
            return obj2;
        }

        public void SetOutArgumentAndReturnValue(ActivityContext context, object state, object[] actualParameters)
        {
            for (int i = 0; i < this.parameters.Count; i++)
            {
                if (this.parameters[i].Direction != ArgumentDirection.In)
                {
                    this.parameters[i].Set(context, actualParameters[i]);
                }
            }
            if (this.returnObject != null)
            {
                this.returnObject.Set(context, state);
            }
        }

        public void Trace(Activity parent)
        {
            if (this.MethodIsStatic)
            {
                if (TD.InvokeMethodIsStaticIsEnabled())
                {
                    TD.InvokeMethodIsStatic(parent.DisplayName);
                }
            }
            else if (TD.InvokeMethodIsNotStaticIsEnabled())
            {
                TD.InvokeMethodIsNotStatic(parent.DisplayName);
            }
        }

        public abstract bool MethodIsStatic { get; }
    }
}

