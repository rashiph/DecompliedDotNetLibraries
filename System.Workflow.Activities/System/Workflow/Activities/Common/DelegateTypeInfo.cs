namespace System.Workflow.Activities.Common
{
    using System;
    using System.CodeDom;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;

    internal class DelegateTypeInfo
    {
        private CodeParameterDeclarationExpression[] parameters;
        private Type[] parameterTypes;
        private CodeTypeReference returnType;

        internal DelegateTypeInfo(Type delegateClass)
        {
            this.Resolve(delegateClass);
        }

        public override bool Equals(object other)
        {
            if (other == null)
            {
                return false;
            }
            DelegateTypeInfo info = other as DelegateTypeInfo;
            if (info == null)
            {
                return false;
            }
            if ((this.ReturnType.BaseType != info.ReturnType.BaseType) || (this.Parameters.Length != info.Parameters.Length))
            {
                return false;
            }
            for (int i = 0; i < this.Parameters.Length; i++)
            {
                CodeParameterDeclarationExpression expression = info.Parameters[i];
                if (expression.Type.BaseType != this.Parameters[i].Type.BaseType)
                {
                    return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        private void Resolve(MethodInfo method)
        {
            ParameterInfo[] parameters = method.GetParameters();
            this.parameters = new CodeParameterDeclarationExpression[parameters.Length];
            this.parameterTypes = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                string name = parameters[i].Name;
                Type parameterType = parameters[i].ParameterType;
                if ((name == null) || (name.Length == 0))
                {
                    name = "param" + i.ToString(CultureInfo.InvariantCulture);
                }
                FieldDirection @in = FieldDirection.In;
                if (parameterType.IsByRef)
                {
                    if (parameterType.FullName.EndsWith("&"))
                    {
                        parameterType = parameterType.Assembly.GetType(parameterType.FullName.Substring(0, parameterType.FullName.Length - 1), true);
                    }
                    @in = FieldDirection.Ref;
                }
                if (parameters[i].IsOut)
                {
                    if (parameters[i].IsIn)
                    {
                        @in = FieldDirection.Ref;
                    }
                    else
                    {
                        @in = FieldDirection.Out;
                    }
                }
                this.parameters[i] = new CodeParameterDeclarationExpression(new CodeTypeReference(parameterType), name);
                this.parameters[i].Direction = @in;
                this.parameterTypes[i] = parameterType;
            }
            this.returnType = new CodeTypeReference(method.ReturnType);
        }

        private void Resolve(Type delegateClass)
        {
            MethodInfo method = delegateClass.GetMethod("Invoke");
            if (method == null)
            {
                throw new ArgumentException("delegateClass");
            }
            this.Resolve(method);
        }

        internal CodeParameterDeclarationExpression[] Parameters
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.parameters;
            }
        }

        internal Type[] ParameterTypes
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.parameterTypes;
            }
        }

        internal CodeTypeReference ReturnType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.returnType;
            }
        }
    }
}

