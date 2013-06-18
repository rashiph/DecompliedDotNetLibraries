namespace System.Activities.Expressions
{
    using System;
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Runtime.Collections;
    using System.Windows.Markup;

    [ContentProperty("Arguments")]
    public sealed class New<TResult> : CodeActivity<TResult>
    {
        private Collection<Argument> arguments;
        private ConstructorInfo constructorInfo;

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            bool flag = false;
            Type[] types = new Type[this.Arguments.Count];
            for (int i = 0; i < this.Arguments.Count; i++)
            {
                Argument argument = this.Arguments[i];
                if ((argument == null) || (argument.Expression == null))
                {
                    metadata.AddValidationError(System.Activities.SR.ArgumentRequired("Arguments", typeof(New<TResult>)));
                    flag = true;
                }
                else
                {
                    RuntimeArgument argument2 = new RuntimeArgument("Argument" + i, this.arguments[i].ArgumentType, this.arguments[i].Direction, true);
                    metadata.Bind(this.arguments[i], argument2);
                    metadata.AddArgument(argument2);
                    types[i] = (this.Arguments[i].Direction == ArgumentDirection.In) ? this.Arguments[i].ArgumentType : this.Arguments[i].ArgumentType.MakeByRefType();
                }
            }
            if (!flag)
            {
                this.constructorInfo = typeof(TResult).GetConstructor(types);
                if ((this.constructorInfo == null) && (!typeof(TResult).IsValueType || (types.Length > 0)))
                {
                    metadata.AddValidationError(System.Activities.SR.ConstructorInfoNotFound(typeof(TResult).Name));
                }
            }
        }

        protected override TResult Execute(CodeActivityContext context)
        {
            TResult local;
            object[] parameters = new object[this.Arguments.Count];
            for (int i = 0; i < this.Arguments.Count; i++)
            {
                parameters[i] = this.Arguments[i].Get(context);
            }
            if (this.constructorInfo != null)
            {
                local = (TResult) this.constructorInfo.Invoke(parameters);
            }
            else
            {
                local = (TResult) Activator.CreateInstance(typeof(TResult));
            }
            for (int j = 0; j < this.Arguments.Count; j++)
            {
                Argument argument = this.Arguments[j];
                if ((argument.Direction == ArgumentDirection.InOut) || (argument.Direction == ArgumentDirection.Out))
                {
                    argument.Set(context, parameters[j]);
                }
            }
            return local;
        }

        public Collection<Argument> Arguments
        {
            get
            {
                if (this.arguments == null)
                {
                    ValidatingCollection<Argument> validatings = new ValidatingCollection<Argument> {
                        OnAddValidationCallback = delegate (Argument item) {
                            if (item == null)
                            {
                                throw FxTrace.Exception.ArgumentNull("item");
                            }
                        }
                    };
                    this.arguments = validatings;
                }
                return this.arguments;
            }
        }
    }
}

