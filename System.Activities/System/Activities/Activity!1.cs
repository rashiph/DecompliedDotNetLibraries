namespace System.Activities
{
    using System;
    using System.Activities.Expressions;
    using System.Activities.Validation;
    using System.Activities.XamlIntegration;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Windows.Markup;

    [ValueSerializer(typeof(ActivityWithResultValueSerializer)), TypeConverter(typeof(ActivityWithResultConverter))]
    public abstract class Activity<TResult> : ActivityWithResult
    {
        protected Activity()
        {
        }

        internal TResult ExecuteWithTryGetValue(ActivityContext context)
        {
            TResult local;
            this.TryGetValue(context, out local);
            return local;
        }

        public static Activity<TResult> FromValue(TResult constValue)
        {
            return new Literal<TResult> { Value = constValue };
        }

        public static Activity<TResult> FromVariable(Variable variable)
        {
            Type type;
            if (variable == null)
            {
                throw FxTrace.Exception.ArgumentNull("variable");
            }
            if (TypeHelper.AreTypesCompatible(variable.Type, typeof(TResult)))
            {
                return new VariableValue<TResult> { Variable = variable };
            }
            if (!ActivityUtilities.IsLocationGenericType(typeof(TResult), out type) || (type != variable.Type))
            {
                throw FxTrace.Exception.Argument("variable", System.Activities.SR.ConvertVariableToValueExpressionFailed(variable.GetType().FullName, typeof(Activity<TResult>).FullName));
            }
            return (Activity<TResult>) ActivityUtilities.CreateVariableReference(variable);
        }

        public static Activity<TResult> FromVariable(Variable<TResult> variable)
        {
            if (variable == null)
            {
                throw FxTrace.Exception.ArgumentNull("variable");
            }
            return new VariableValue<TResult>(variable);
        }

        private bool IsBoundArgumentCorrect(RuntimeArgument argument, bool createEmptyBindings)
        {
            if (createEmptyBindings)
            {
                return object.ReferenceEquals(argument.BoundArgument, this.Result);
            }
            if (this.Result != null)
            {
                return object.ReferenceEquals(argument.BoundArgument, this.Result);
            }
            return true;
        }

        internal override bool IsResultArgument(RuntimeArgument argument)
        {
            return object.ReferenceEquals(argument, base.ResultRuntimeArgument);
        }

        internal sealed override void OnInternalCacheMetadata(bool createEmptyBindings)
        {
            this.OnInternalCacheMetadataExceptResult(createEmptyBindings);
            bool flag = false;
            IList<RuntimeArgument> runtimeArguments = base.RuntimeArguments;
            int count = 0;
            if (runtimeArguments != null)
            {
                count = runtimeArguments.Count;
                for (int i = 0; i < count; i++)
                {
                    RuntimeArgument argument = runtimeArguments[i];
                    if (argument.Name == "Result")
                    {
                        flag = true;
                        if ((argument.Type != typeof(TResult)) || (argument.Direction != ArgumentDirection.Out))
                        {
                            base.AddTempValidationError(new ValidationError(System.Activities.SR.ResultArgumentHasRequiredTypeAndDirection(typeof(TResult), argument.Direction, argument.Type)));
                        }
                        else if (!this.IsBoundArgumentCorrect(argument, createEmptyBindings))
                        {
                            base.AddTempValidationError(new ValidationError(System.Activities.SR.ResultArgumentMustBeBoundToResultProperty));
                        }
                        else
                        {
                            base.ResultRuntimeArgument = argument;
                        }
                        break;
                    }
                }
            }
            if (!flag)
            {
                base.ResultRuntimeArgument = new RuntimeArgument("Result", typeof(TResult), ArgumentDirection.Out);
                if ((this.Result == null) && createEmptyBindings)
                {
                    this.Result = new OutArgument<TResult>();
                }
                Argument.Bind(this.Result, base.ResultRuntimeArgument);
                base.AddArgument(base.ResultRuntimeArgument, createEmptyBindings);
            }
        }

        internal virtual void OnInternalCacheMetadataExceptResult(bool createEmptyBindings)
        {
            base.OnInternalCacheMetadata(createEmptyBindings);
        }

        public static implicit operator Activity<TResult>(TResult constValue)
        {
            return Activity<TResult>.FromValue(constValue);
        }

        public static implicit operator Activity<TResult>(Variable variable)
        {
            return Activity<TResult>.FromVariable(variable);
        }

        public static implicit operator Activity<TResult>(Variable<TResult> variable)
        {
            return Activity<TResult>.FromVariable(variable);
        }

        internal virtual bool TryGetValue(ActivityContext context, out TResult value)
        {
            value = default(TResult);
            return false;
        }

        internal override Type InternalResultType
        {
            get
            {
                return typeof(TResult);
            }
        }

        [DefaultValue((string) null)]
        public OutArgument<TResult> Result { get; set; }

        internal override OutArgument ResultCore
        {
            get
            {
                return this.Result;
            }
            set
            {
                this.Result = value as OutArgument<TResult>;
                if ((this.Result == null) && (value != null))
                {
                    throw FxTrace.Exception.Argument("value", System.Activities.SR.ResultArgumentMustBeSpecificType(typeof(TResult)));
                }
            }
        }
    }
}

