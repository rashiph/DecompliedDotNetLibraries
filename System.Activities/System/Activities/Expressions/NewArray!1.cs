namespace System.Activities.Expressions
{
    using System;
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Runtime.Collections;
    using System.Windows.Markup;

    [ContentProperty("Bounds")]
    public sealed class NewArray<TResult> : CodeActivity<TResult>
    {
        private Collection<Argument> bounds;
        private ConstructorInfo constructorInfo;

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (!typeof(TResult).IsArray)
            {
                metadata.AddValidationError(System.Activities.SR.NewArrayRequiresArrayTypeAsResultType);
            }
            else
            {
                bool flag = false;
                Type[] types = new Type[this.Bounds.Count];
                for (int i = 0; i < this.Bounds.Count; i++)
                {
                    Argument argument = this.Bounds[i];
                    if ((argument == null) || argument.IsEmpty)
                    {
                        metadata.AddValidationError(System.Activities.SR.ArgumentRequired("Bounds", typeof(NewArray<TResult>)));
                        flag = true;
                    }
                    else if (!this.isIntegralType(argument.ArgumentType))
                    {
                        metadata.AddValidationError(System.Activities.SR.NewArrayBoundsRequiresIntegralArguments);
                        flag = true;
                    }
                    else
                    {
                        RuntimeArgument argument2 = new RuntimeArgument("Argument" + i, this.Bounds[i].ArgumentType, this.bounds[i].Direction, true);
                        metadata.Bind(this.Bounds[i], argument2);
                        metadata.AddArgument(argument2);
                        types[i] = argument.ArgumentType;
                    }
                }
                if (!flag)
                {
                    this.constructorInfo = typeof(TResult).GetConstructor(types);
                    if (this.constructorInfo == null)
                    {
                        metadata.AddValidationError(System.Activities.SR.ConstructorInfoNotFound(typeof(TResult).Name));
                    }
                }
            }
        }

        protected override TResult Execute(CodeActivityContext context)
        {
            object[] parameters = new object[this.Bounds.Count];
            int index = 0;
            foreach (Argument argument in this.Bounds)
            {
                parameters[index] = argument.Get(context);
                index++;
            }
            return (TResult) this.constructorInfo.Invoke(parameters);
        }

        private bool isIntegralType(Type type)
        {
            if (((!(type == typeof(sbyte)) && !(type == typeof(byte))) && (!(type == typeof(char)) && !(type == typeof(short)))) && ((!(type == typeof(ushort)) && !(type == typeof(int))) && ((!(type == typeof(uint)) && !(type == typeof(long))) && !(type == typeof(ulong)))))
            {
                return false;
            }
            return true;
        }

        public Collection<Argument> Bounds
        {
            get
            {
                if (this.bounds == null)
                {
                    ValidatingCollection<Argument> validatings = new ValidatingCollection<Argument> {
                        OnAddValidationCallback = delegate (Argument item) {
                            if (item == null)
                            {
                                throw FxTrace.Exception.ArgumentNull("item");
                            }
                        }
                    };
                    this.bounds = validatings;
                }
                return this.bounds;
            }
        }
    }
}

