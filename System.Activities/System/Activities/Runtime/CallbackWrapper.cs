namespace System.Activities.Runtime
{
    using System;
    using System.Activities;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [DataContract]
    internal class CallbackWrapper
    {
        private static BindingFlags bindingFlags = (BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        private Delegate callback;
        [DataMember]
        private string callbackName;
        [DataMember(EmitDefaultValue=false)]
        private string declaringAssemblyName;
        [DataMember(EmitDefaultValue=false)]
        private string declaringTypeName;

        public CallbackWrapper(Delegate callback, System.Activities.ActivityInstance owningInstance)
        {
            this.ActivityInstance = owningInstance;
            this.callback = callback;
        }

        protected void EnsureCallback(Type delegateType, Type[] parameters)
        {
            if (this.callback == null)
            {
                Type type;
                MethodInfo matchingMethod = this.GetMatchingMethod(parameters, out type);
                this.callback = this.RecreateCallback(delegateType, matchingMethod);
            }
        }

        protected void EnsureCallback(Type delegateType, Type[] parameterTypes, Type genericParameter)
        {
            if (this.callback == null)
            {
                this.callback = this.GenerateCallback(delegateType, parameterTypes, genericParameter);
            }
        }

        private Delegate GenerateCallback(Type delegateType, Type[] parameterTypes, Type genericParameter)
        {
            Type type;
            MethodInfo matchingMethod = this.GetMatchingMethod(parameterTypes, out type);
            if (matchingMethod == null)
            {
                foreach (MethodInfo info2 in type.GetMethods(bindingFlags))
                {
                    if ((!info2.IsGenericMethod || !(info2.Name == this.callbackName)) || (info2.GetGenericArguments().Length != 1))
                    {
                        continue;
                    }
                    info2 = info2.MakeGenericMethod(new Type[] { genericParameter });
                    ParameterInfo[] parameters = info2.GetParameters();
                    bool flag = true;
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        ParameterInfo info3 = parameters[i];
                        if ((info3.IsOut || info3.IsOptional) || (info3.ParameterType != parameterTypes[i]))
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag)
                    {
                        matchingMethod = info2;
                        break;
                    }
                }
            }
            if (matchingMethod == null)
            {
                return null;
            }
            return this.RecreateCallback(delegateType, matchingMethod);
        }

        private MethodInfo GetMatchingMethod(Type[] parameters, out Type declaringType)
        {
            object activity = this.ActivityInstance.Activity;
            if (this.declaringTypeName == null)
            {
                declaringType = activity.GetType();
            }
            else
            {
                Assembly assembly;
                if (this.declaringAssemblyName != null)
                {
                    assembly = Assembly.Load(this.declaringAssemblyName);
                }
                else
                {
                    assembly = activity.GetType().Assembly;
                }
                declaringType = assembly.GetType(this.declaringTypeName);
            }
            return declaringType.GetMethod(this.callbackName, bindingFlags, null, parameters, null);
        }

        public static bool IsValidCallback(Delegate callback, System.Activities.ActivityInstance owningInstance)
        {
            object target = callback.Target;
            return ((target == null) || object.ReferenceEquals(target, owningInstance.Activity));
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            if ((this.callbackName == null) && !this.IsCallbackNull)
            {
                MethodInfo method = this.callback.Method;
                this.callbackName = method.Name;
                Type declaringType = method.DeclaringType;
                Type type = this.ActivityInstance.Activity.GetType();
                if (declaringType != type)
                {
                    this.declaringTypeName = declaringType.FullName;
                    if (declaringType.Assembly != type.Assembly)
                    {
                        this.declaringAssemblyName = declaringType.Assembly.FullName;
                    }
                }
                if (method.IsGenericMethod)
                {
                    this.OnSerializingGenericCallback();
                }
            }
        }

        protected virtual void OnSerializingGenericCallback()
        {
            throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.InvalidExecutionCallback(this.callback.Method, null)));
        }

        private Delegate RecreateCallback(Type delegateType, MethodInfo callbackMethod)
        {
            object firstArgument = null;
            if (!callbackMethod.IsStatic)
            {
                firstArgument = this.ActivityInstance.Activity;
            }
            return Delegate.CreateDelegate(delegateType, firstArgument, callbackMethod);
        }

        protected void ValidateCallbackResolution(Type delegateType, Type[] parameterTypes, Type genericParameter)
        {
            if (!this.callback.Equals(this.GenerateCallback(delegateType, parameterTypes, genericParameter)))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.InvalidExecutionCallback(this.callback.Method, null)));
            }
        }

        [DataMember]
        public System.Activities.ActivityInstance ActivityInstance { get; private set; }

        protected Delegate Callback
        {
            get
            {
                return this.callback;
            }
        }

        protected bool IsCallbackNull
        {
            get
            {
                return ((this.callback == null) && (this.callbackName == null));
            }
        }
    }
}

