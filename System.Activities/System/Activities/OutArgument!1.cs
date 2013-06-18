namespace System.Activities
{
    using System;
    using System.Activities.Expressions;
    using System.Activities.Runtime;
    using System.Activities.XamlIntegration;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;

    [ContentProperty("Expression"), TypeConverter(typeof(OutArgumentConverter)), ValueSerializer(typeof(ArgumentValueSerializer))]
    public sealed class OutArgument<T> : OutArgument
    {
        public OutArgument()
        {
            base.ArgumentType = typeof(T);
        }

        public OutArgument(DelegateArgument delegateArgument) : this()
        {
            if (delegateArgument != null)
            {
                DelegateArgumentReference<T> reference = new DelegateArgumentReference<T> {
                    DelegateArgument = delegateArgument
                };
                this.Expression = reference;
            }
        }

        public OutArgument(Variable variable) : this()
        {
            if (variable != null)
            {
                VariableReference<T> reference = new VariableReference<T> {
                    Variable = variable
                };
                this.Expression = reference;
            }
        }

        public OutArgument(Activity<Location<T>> expression) : this()
        {
            this.Expression = expression;
        }

        public OutArgument(Expression<Func<ActivityContext, T>> expression) : this()
        {
            if (expression != null)
            {
                this.Expression = new LambdaReference<T>(expression);
            }
        }

        internal override Location CreateDefaultLocation()
        {
            return Argument.CreateLocation<T>();
        }

        internal override void Declare(LocationEnvironment targetEnvironment, System.Activities.ActivityInstance activityInstance)
        {
            targetEnvironment.DeclareTemporaryLocation<Location<T>>(base.RuntimeArgument, activityInstance, true);
        }

        public static OutArgument<T> FromDelegateArgument(DelegateArgument delegateArgument)
        {
            if (delegateArgument == null)
            {
                throw FxTrace.Exception.ArgumentNull("delegateArgument");
            }
            return new OutArgument<T>(delegateArgument);
        }

        public static OutArgument<T> FromExpression(Activity<Location<T>> expression)
        {
            if (expression == null)
            {
                throw FxTrace.Exception.ArgumentNull("expression");
            }
            return new OutArgument<T>(expression);
        }

        public static OutArgument<T> FromVariable(Variable variable)
        {
            if (variable == null)
            {
                throw FxTrace.Exception.ArgumentNull("variable");
            }
            return new OutArgument<T>(variable);
        }

        public T Get(ActivityContext context)
        {
            return base.Get<T>(context);
        }

        public Location<T> GetLocation(ActivityContext context)
        {
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }
            base.ThrowIfNotInTree();
            return context.GetLocation<T>(base.RuntimeArgument);
        }

        public static implicit operator OutArgument<T>(Activity<Location<T>> expression)
        {
            return OutArgument<T>.FromExpression(expression);
        }

        public static implicit operator OutArgument<T>(DelegateArgument delegateArgument)
        {
            return OutArgument<T>.FromDelegateArgument(delegateArgument);
        }

        public static implicit operator OutArgument<T>(Variable variable)
        {
            return OutArgument<T>.FromVariable(variable);
        }

        public void Set(ActivityContext context, T value)
        {
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }
            base.ThrowIfNotInTree();
            context.SetValue<T>((OutArgument<T>) this, value);
        }

        internal override bool TryPopulateValue(LocationEnvironment targetEnvironment, System.Activities.ActivityInstance targetActivityInstance, ActivityContext resolutionContext)
        {
            Location<T> location;
            if (this.Expression.TryGetValue(resolutionContext, out location))
            {
                targetEnvironment.Declare(base.RuntimeArgument, location.CreateReference(true), targetActivityInstance);
                return true;
            }
            targetEnvironment.DeclareTemporaryLocation<Location<T>>(base.RuntimeArgument, targetActivityInstance, true);
            return false;
        }

        [DefaultValue((string) null)]
        public Activity<Location<T>> Expression { get; set; }

        internal override ActivityWithResult ExpressionCore
        {
            get
            {
                return this.Expression;
            }
            set
            {
                if (value == null)
                {
                    this.Expression = null;
                }
                else if (value is Activity<Location<T>>)
                {
                    this.Expression = (Activity<Location<T>>) value;
                }
                else
                {
                    this.Expression = new ActivityWithResultWrapper<Location<T>>(value);
                }
            }
        }
    }
}

