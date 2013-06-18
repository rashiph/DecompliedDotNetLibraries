namespace System.Activities
{
    using System;
    using System.Activities.Runtime;
    using System.Activities.Validation;
    using System.Activities.XamlIntegration;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Windows.Markup;

    public abstract class Argument
    {
        private ArgumentDirection direction;
        private int evaluationOrder = UnspecifiedEvaluationOrder;
        public const string ResultValue = "Result";
        private System.Activities.RuntimeArgument runtimeArgument;
        public static readonly int UnspecifiedEvaluationOrder = -1;

        internal Argument()
        {
        }

        internal static void Bind(Argument binding, System.Activities.RuntimeArgument argument)
        {
            if (binding != null)
            {
                binding.RuntimeArgument = argument;
            }
            argument.BoundArgument = binding;
        }

        internal bool CanConvertToString(IValueSerializerContext context)
        {
            return (this.WasDesignTimeNull || ((this.EvaluationOrder == UnspecifiedEvaluationOrder) && ActivityWithResultValueSerializer.CanConvertToStringWrapper(this.Expression, context)));
        }

        internal string ConvertToString(IValueSerializerContext context)
        {
            if (this.WasDesignTimeNull)
            {
                return null;
            }
            return ActivityWithResultValueSerializer.ConvertToStringWrapper(this.Expression, context);
        }

        public static Argument Create(Type type, ArgumentDirection direction)
        {
            return ActivityUtilities.CreateArgument(type, direction);
        }

        internal abstract Location CreateDefaultLocation();
        internal static Location<T> CreateLocation<T>()
        {
            return new Location<T>();
        }

        public static Argument CreateReference(Argument argumentToReference, string referencedArgumentName)
        {
            if (argumentToReference == null)
            {
                throw FxTrace.Exception.ArgumentNull("argumentToReference");
            }
            if (string.IsNullOrEmpty(referencedArgumentName))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("referencedArgumentName");
            }
            return ActivityUtilities.CreateReferenceArgument(argumentToReference.ArgumentType, argumentToReference.Direction, referencedArgumentName);
        }

        internal abstract void Declare(LocationEnvironment targetEnvironment, System.Activities.ActivityInstance activityInstance);
        public object Get(ActivityContext context)
        {
            return this.Get<object>(context);
        }

        public T Get<T>(ActivityContext context)
        {
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }
            this.ThrowIfNotInTree();
            return context.GetValue<T>(this.RuntimeArgument);
        }

        public Location GetLocation(ActivityContext context)
        {
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }
            this.ThrowIfNotInTree();
            return this.runtimeArgument.GetLocation(context);
        }

        public void Set(ActivityContext context, object value)
        {
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }
            this.ThrowIfNotInTree();
            context.SetValue<object>(this.RuntimeArgument, value);
        }

        internal void ThrowIfNotInTree()
        {
            if (!this.IsInTree)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.ArgumentNotInTree(this.ArgumentType)));
            }
        }

        internal static void TryBind(Argument binding, System.Activities.RuntimeArgument argument, Activity violationOwner)
        {
            if (argument == null)
            {
                throw FxTrace.Exception.ArgumentNull("argument");
            }
            bool flag = true;
            if (binding != null)
            {
                if (binding.Direction != argument.Direction)
                {
                    violationOwner.AddTempValidationError(new ValidationError(System.Activities.SR.ArgumentDirectionMismatch(argument.Name, argument.Direction, binding.Direction)));
                    flag = false;
                }
                if (binding.ArgumentType != argument.Type)
                {
                    violationOwner.AddTempValidationError(new ValidationError(System.Activities.SR.ArgumentTypeMismatch(argument.Name, argument.Type, binding.ArgumentType)));
                    flag = false;
                }
            }
            if (flag)
            {
                Bind(binding, argument);
            }
        }

        internal abstract bool TryPopulateValue(LocationEnvironment targetEnvironment, System.Activities.ActivityInstance targetActivityInstance, ActivityContext resolutionContext);
        internal void Validate(Activity owner, ref IList<ValidationError> validationErrors)
        {
            if (this.Expression != null)
            {
                if ((this.Expression.Result != null) && !this.Expression.Result.IsEmpty)
                {
                    ValidationError data = new ValidationError(System.Activities.SR.ResultCannotBeSetOnArgumentExpressions, false, this.RuntimeArgument.Name, owner);
                    ActivityUtilities.Add<ValidationError>(ref validationErrors, data);
                }
                ActivityWithResult expression = this.Expression;
                if (expression is IExpressionWrapper)
                {
                    expression = ((IExpressionWrapper) expression).InnerExpression;
                }
                switch (this.Direction)
                {
                    case ArgumentDirection.In:
                        if (!(expression.ResultType != this.ArgumentType))
                        {
                            break;
                        }
                        ActivityUtilities.Add<ValidationError>(ref validationErrors, new ValidationError(System.Activities.SR.ArgumentValueExpressionTypeMismatch(this.ArgumentType, expression.ResultType), false, this.RuntimeArgument.Name, owner));
                        return;

                    case ArgumentDirection.Out:
                    case ArgumentDirection.InOut:
                        Type type;
                        if (!ActivityUtilities.IsLocationGenericType(expression.ResultType, out type) || (type != this.ArgumentType))
                        {
                            Type type2 = ActivityUtilities.CreateActivityWithResult(ActivityUtilities.CreateLocation(this.ArgumentType));
                            ActivityUtilities.Add<ValidationError>(ref validationErrors, new ValidationError(System.Activities.SR.ArgumentLocationExpressionTypeMismatch(type2.FullName, expression.GetType().FullName), false, this.RuntimeArgument.Name, owner));
                        }
                        break;

                    default:
                        return;
                }
            }
        }

        public Type ArgumentType { get; internal set; }

        public ArgumentDirection Direction
        {
            get
            {
                return this.direction;
            }
            internal set
            {
                ArgumentDirectionHelper.Validate(value, "value");
                this.direction = value;
            }
        }

        [DefaultValue(-1)]
        public int EvaluationOrder
        {
            get
            {
                return this.evaluationOrder;
            }
            set
            {
                if ((value < 0) && (value != UnspecifiedEvaluationOrder))
                {
                    throw FxTrace.Exception.ArgumentOutOfRange("EvaluationOrder", value, System.Activities.SR.InvalidEvaluationOrderValue);
                }
                this.evaluationOrder = value;
            }
        }

        [DefaultValue((string) null), IgnoreDataMember]
        public ActivityWithResult Expression
        {
            get
            {
                return this.ExpressionCore;
            }
            set
            {
                this.ExpressionCore = value;
            }
        }

        internal abstract ActivityWithResult ExpressionCore { get; set; }

        internal int Id
        {
            get
            {
                return this.runtimeArgument.Id;
            }
        }

        internal bool IsEmpty
        {
            get
            {
                return (this.Expression == null);
            }
        }

        internal bool IsInTree
        {
            get
            {
                return ((this.runtimeArgument != null) && this.runtimeArgument.IsInTree);
            }
        }

        internal System.Activities.RuntimeArgument RuntimeArgument
        {
            get
            {
                return this.runtimeArgument;
            }
            set
            {
                this.runtimeArgument = value;
            }
        }

        internal bool WasDesignTimeNull { get; set; }

        internal interface IExpressionWrapper
        {
            ActivityWithResult InnerExpression { get; }
        }
    }
}

