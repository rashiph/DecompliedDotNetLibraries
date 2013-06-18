namespace System.Activities.Expressions
{
    using System;
    using System.Activities;
    using System.Activities.Statements;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime;
    using System.Runtime.Collections;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;

    [ContentProperty("Parameters")]
    public sealed class InvokeMethod<TResult> : AsyncCodeActivity<TResult>
    {
        private Collection<Type> genericTypeArguments;
        private MethodExecutor methodExecutor;
        private MethodResolver methodResolver;
        private Collection<Argument> parameters;
        private RuntimeArgument resultArgument;

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            return this.methodExecutor.BeginExecuteMethod(context, callback, state);
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            Collection<RuntimeArgument> arguments = new Collection<RuntimeArgument>();
            Type objectType = TypeHelper.ObjectType;
            if (this.TargetObject != null)
            {
                objectType = this.TargetObject.ArgumentType;
            }
            RuntimeArgument argument = new RuntimeArgument("TargetObject", objectType, ArgumentDirection.In);
            metadata.Bind(this.TargetObject, argument);
            arguments.Add(argument);
            this.resultArgument = new RuntimeArgument("Result", typeof(TResult), ArgumentDirection.Out);
            metadata.Bind(base.Result, this.resultArgument);
            arguments.Add(this.resultArgument);
            this.methodResolver = this.CreateMethodResolver();
            this.methodResolver.DetermineMethodInfo(metadata, out this.methodExecutor);
            this.methodResolver.RegisterParameters(arguments);
            metadata.SetArgumentsCollection(arguments);
            this.methodResolver.Trace();
            if (this.methodExecutor != null)
            {
                this.methodExecutor.Trace(this);
            }
        }

        private MethodResolver CreateMethodResolver()
        {
            return new MethodResolver { MethodName = this.MethodName, RunAsynchronously = this.RunAsynchronously, TargetType = this.TargetType, TargetObject = this.TargetObject, GenericTypeArguments = this.GenericTypeArguments, Parameters = this.Parameters, Result = this.resultArgument, ResultType = typeof(TResult), Parent = this };
        }

        protected override TResult EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            this.methodExecutor.EndExecuteMethod(context, result);
            return base.Result.Get(context);
        }

        public Collection<Type> GenericTypeArguments
        {
            get
            {
                if (this.genericTypeArguments == null)
                {
                    ValidatingCollection<Type> validatings = new ValidatingCollection<Type> {
                        OnAddValidationCallback = delegate (Type item) {
                            if (item == null)
                            {
                                throw FxTrace.Exception.ArgumentNull("item");
                            }
                        }
                    };
                    this.genericTypeArguments = validatings;
                }
                return this.genericTypeArguments;
            }
        }

        public string MethodName { get; set; }

        public Collection<Argument> Parameters
        {
            get
            {
                if (this.parameters == null)
                {
                    ValidatingCollection<Argument> validatings = new ValidatingCollection<Argument> {
                        OnAddValidationCallback = delegate (Argument item) {
                            if (item == null)
                            {
                                throw FxTrace.Exception.ArgumentNull("item");
                            }
                        }
                    };
                    this.parameters = validatings;
                }
                return this.parameters;
            }
        }

        [DefaultValue(false)]
        public bool RunAsynchronously { get; set; }

        [DefaultValue((string) null)]
        public InArgument TargetObject { get; set; }

        [DefaultValue((string) null)]
        public Type TargetType { get; set; }
    }
}

