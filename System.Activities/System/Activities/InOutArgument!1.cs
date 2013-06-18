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

    [TypeConverter(typeof(InOutArgumentConverter)), ValueSerializer(typeof(ArgumentValueSerializer)), ContentProperty("Expression")]
    public sealed class InOutArgument<T> : InOutArgument
    {
        public InOutArgument()
        {
            base.ArgumentType = typeof(T);
        }

        public InOutArgument(Variable variable) : this()
        {
            if (variable != null)
            {
                VariableReference<T> reference = new VariableReference<T> {
                    Variable = variable
                };
                this.Expression = reference;
            }
        }

        public InOutArgument(Activity<Location<T>> expression) : this()
        {
            this.Expression = expression;
        }

        public InOutArgument(Variable<T> variable) : this()
        {
            if (variable != null)
            {
                VariableReference<T> reference = new VariableReference<T> {
                    Variable = variable
                };
                this.Expression = reference;
            }
        }

        public InOutArgument(Expression<Func<ActivityContext, T>> expression) : this()
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
            targetEnvironment.DeclareTemporaryLocation<Location<T>>(base.RuntimeArgument, activityInstance, false);
        }

        public static InOutArgument<T> FromExpression(Activity<Location<T>> expression)
        {
            if (expression == null)
            {
                throw FxTrace.Exception.ArgumentNull("expression");
            }
            return new InOutArgument<T> { Expression = expression };
        }

        public static InOutArgument<T> FromVariable(Variable<T> variable)
        {
            InOutArgument<T> argument = new InOutArgument<T>();
            VariableReference<T> reference = new VariableReference<T> {
                Variable = variable
            };
            argument.Expression = reference;
            return argument;
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

        public static implicit operator InOutArgument<T>(Activity<Location<T>> expression)
        {
            return InOutArgument<T>.FromExpression(expression);
        }

        public static implicit operator InOutArgument<T>(Variable<T> variable)
        {
            return InOutArgument<T>.FromVariable(variable);
        }

        public void Set(ActivityContext context, T value)
        {
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }
            base.ThrowIfNotInTree();
            context.SetValue<T>((InOutArgument<T>) this, value);
        }

        internal override bool TryPopulateValue(LocationEnvironment targetEnvironment, System.Activities.ActivityInstance targetActivityInstance, ActivityContext resolutionContext)
        {
            Location<T> location;
            if (this.Expression.TryGetValue(resolutionContext, out location))
            {
                targetEnvironment.Declare(base.RuntimeArgument, location.CreateReference(false), targetActivityInstance);
                return true;
            }
            targetEnvironment.DeclareTemporaryLocation<Location<T>>(base.RuntimeArgument, targetActivityInstance, false);
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
                else
                {
                    Activity<Location<T>> activity = value as Activity<Location<T>>;
                    if (activity != null)
                    {
                        this.Expression = activity;
                    }
                    else
                    {
                        this.Expression = new ActivityWithResultWrapper<Location<T>>(value);
                    }
                }
            }
        }
    }
}

