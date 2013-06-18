namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;

    [ContentProperty("Delegate")]
    public sealed class InvokeDelegate : NativeActivity
    {
        private IDictionary<string, Argument> delegateArguments = new Dictionary<string, Argument>();
        private bool hasOutputArguments;

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            Collection<RuntimeArgument> arguments = new Collection<RuntimeArgument>();
            foreach (KeyValuePair<string, Argument> pair in this.DelegateArguments)
            {
                RuntimeArgument argument = new RuntimeArgument(pair.Key, pair.Value.ArgumentType, pair.Value.Direction);
                metadata.Bind(pair.Value, argument);
                arguments.Add(argument);
            }
            metadata.SetArgumentsCollection(arguments);
            metadata.AddDelegate(this.Delegate);
            if (this.Delegate != null)
            {
                IList<RuntimeDelegateArgument> runtimeDelegateArguments = this.Delegate.RuntimeDelegateArguments;
                if (this.DelegateArguments.Count != runtimeDelegateArguments.Count)
                {
                    metadata.AddValidationError(System.Activities.SR.WrongNumberOfArgumentsForActivityDelegate);
                }
                for (int i = 0; i < runtimeDelegateArguments.Count; i++)
                {
                    RuntimeDelegateArgument argument2 = runtimeDelegateArguments[i];
                    Argument argument3 = null;
                    string name = argument2.Name;
                    if (this.DelegateArguments.TryGetValue(name, out argument3))
                    {
                        if (argument3.Direction != argument2.Direction)
                        {
                            metadata.AddValidationError(System.Activities.SR.DelegateParameterDirectionalityMismatch(name, argument3.Direction, argument2.Direction));
                        }
                        if (argument2.Direction == ArgumentDirection.In)
                        {
                            if (!TypeHelper.AreTypesCompatible(argument3.ArgumentType, argument2.Type))
                            {
                                metadata.AddValidationError(System.Activities.SR.DelegateInArgumentTypeMismatch(name, argument2.Type, argument3.ArgumentType));
                            }
                        }
                        else if (!TypeHelper.AreTypesCompatible(argument2.Type, argument3.ArgumentType))
                        {
                            metadata.AddValidationError(System.Activities.SR.DelegateOutArgumentTypeMismatch(name, argument2.Type, argument3.ArgumentType));
                        }
                    }
                    else
                    {
                        metadata.AddValidationError(System.Activities.SR.InputParametersMissing(argument2.Name));
                    }
                    if (!this.hasOutputArguments && ArgumentDirectionHelper.IsOut(argument2.Direction))
                    {
                        this.hasOutputArguments = true;
                    }
                }
            }
        }

        protected override void Execute(NativeActivityContext context)
        {
            if ((this.Delegate != null) && (this.Delegate.Handler != null))
            {
                Dictionary<string, object> inputParameters = new Dictionary<string, object>();
                if (this.DelegateArguments.Count > 0)
                {
                    foreach (KeyValuePair<string, Argument> pair in this.DelegateArguments)
                    {
                        if (ArgumentDirectionHelper.IsIn(pair.Value.Direction))
                        {
                            inputParameters.Add(pair.Key, pair.Value.Get(context));
                        }
                    }
                }
                context.ScheduleDelegate(this.Delegate, inputParameters, new DelegateCompletionCallback(this.OnHandlerComplete), null);
            }
        }

        private void OnHandlerComplete(NativeActivityContext context, System.Activities.ActivityInstance completedInstance, IDictionary<string, object> outArguments)
        {
            if (this.hasOutputArguments)
            {
                foreach (KeyValuePair<string, object> pair in outArguments)
                {
                    Argument argument = null;
                    if (this.DelegateArguments.TryGetValue(pair.Key, out argument) && ArgumentDirectionHelper.IsOut(argument.Direction))
                    {
                        this.DelegateArguments[pair.Key].Set(context, pair.Value);
                    }
                }
            }
        }

        [DefaultValue((string) null)]
        public ActivityDelegate Delegate { get; set; }

        public IDictionary<string, Argument> DelegateArguments
        {
            get
            {
                return this.delegateArguments;
            }
        }
    }
}

