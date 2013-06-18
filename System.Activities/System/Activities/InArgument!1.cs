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

    [TypeConverter(typeof(InArgumentConverter)), ContentProperty("Expression"), ValueSerializer(typeof(ArgumentValueSerializer))]
    public sealed class InArgument<T> : InArgument
    {
        public InArgument()
        {
            base.ArgumentType = typeof(T);
        }

        public InArgument(DelegateArgument delegateArgument) : this()
        {
            if (delegateArgument != null)
            {
                DelegateArgumentValue<T> value2 = new DelegateArgumentValue<T> {
                    DelegateArgument = delegateArgument
                };
                this.Expression = value2;
            }
        }

        public InArgument(Variable variable) : this()
        {
            if (variable != null)
            {
                VariableValue<T> value2 = new VariableValue<T> {
                    Variable = variable
                };
                this.Expression = value2;
            }
        }

        public InArgument(T constValue) : this()
        {
            Literal<T> literal = new Literal<T> {
                Value = constValue
            };
            this.Expression = literal;
        }

        public InArgument(Activity<T> expression) : this()
        {
            this.Expression = expression;
        }

        public InArgument(Expression<Func<ActivityContext, T>> expression) : this()
        {
            if (expression != null)
            {
                this.Expression = new LambdaValue<T>(expression);
            }
        }

        internal override Location CreateDefaultLocation()
        {
            return Argument.CreateLocation<T>();
        }

        internal override void Declare(LocationEnvironment targetEnvironment, System.Activities.ActivityInstance targetActivityInstance)
        {
            targetEnvironment.Declare(base.RuntimeArgument, this.CreateDefaultLocation(), targetActivityInstance);
        }

        public static InArgument<T> FromDelegateArgument(DelegateArgument delegateArgument)
        {
            if (delegateArgument == null)
            {
                throw FxTrace.Exception.ArgumentNull("delegateArgument");
            }
            return new InArgument<T>(delegateArgument);
        }

        public static InArgument<T> FromExpression(Activity<T> expression)
        {
            if (expression == null)
            {
                throw FxTrace.Exception.ArgumentNull("expression");
            }
            return new InArgument<T>(expression);
        }

        public static InArgument<T> FromValue(T constValue)
        {
            InArgument<T> argument = new InArgument<T>();
            Literal<T> literal = new Literal<T> {
                Value = constValue
            };
            argument.Expression = literal;
            return argument;
        }

        public static InArgument<T> FromVariable(Variable variable)
        {
            if (variable == null)
            {
                throw FxTrace.Exception.ArgumentNull("variable");
            }
            return new InArgument<T>(variable);
        }

        public T Get(ActivityContext context)
        {
            return base.Get<T>(context);
        }

        public static implicit operator InArgument<T>(Variable variable)
        {
            return InArgument<T>.FromVariable(variable);
        }

        public static implicit operator InArgument<T>(Activity<T> expression)
        {
            return InArgument<T>.FromExpression(expression);
        }

        public static implicit operator InArgument<T>(DelegateArgument delegateArgument)
        {
            return InArgument<T>.FromDelegateArgument(delegateArgument);
        }

        public static implicit operator InArgument<T>(T constValue)
        {
            return InArgument<T>.FromValue(constValue);
        }

        public void Set(ActivityContext context, T value)
        {
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }
            context.SetValue<T>((InArgument<T>) this, value);
        }

        internal override bool TryPopulateValue(LocationEnvironment targetEnvironment, System.Activities.ActivityInstance activityInstance, ActivityContext resolutionContext)
        {
            T local;
            Location<T> location = Argument.CreateLocation<T>();
            targetEnvironment.Declare(base.RuntimeArgument, location, activityInstance);
            if (this.Expression.TryGetValue(resolutionContext, out local))
            {
                location.Value = local;
                return true;
            }
            return false;
        }

        [DefaultValue((string) null)]
        public Activity<T> Expression { get; set; }

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
                else if (value is Activity<T>)
                {
                    this.Expression = (Activity<T>) value;
                }
                else
                {
                    this.Expression = new ActivityWithResultWrapper<T>(value);
                }
            }
        }
    }
}

