namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Activities.Runtime;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.Collections;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Windows.Markup;

    public sealed class TryCatch : NativeActivity
    {
        private CatchList catches;
        private FaultCallback exceptionFromCatchOrFinallyHandler;
        internal const string FaultContextId = "{35ABC8C3-9AF1-4426-8293-A6DDBB6ED91D}";
        private Variable<TryCatchState> state = new Variable<TryCatchState>();
        private Collection<Variable> variables;

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            if (this.Try != null)
            {
                metadata.AddChild(this.Try);
            }
            if (this.Finally != null)
            {
                metadata.AddChild(this.Finally);
            }
            Collection<ActivityDelegate> delegates = new Collection<ActivityDelegate>();
            if (this.catches != null)
            {
                foreach (Catch @catch in this.catches)
                {
                    ActivityDelegate action = @catch.GetAction();
                    if (action != null)
                    {
                        delegates.Add(action);
                    }
                }
            }
            metadata.AddImplementationVariable(this.state);
            metadata.SetDelegatesCollection(delegates);
            metadata.SetVariablesCollection(this.Variables);
            if ((this.Finally == null) && (this.Catches.Count == 0))
            {
                metadata.AddValidationError(System.Activities.SR.CatchOrFinallyExpected(base.DisplayName));
            }
        }

        protected override void Cancel(NativeActivityContext context)
        {
            if (!this.state.Get(context).SuppressCancel)
            {
                context.CancelChildren();
            }
        }

        protected override void Execute(NativeActivityContext context)
        {
            this.state.Set(context, new TryCatchState());
            if (this.Try != null)
            {
                context.ScheduleActivity(this.Try, new CompletionCallback(this.OnTryComplete), new FaultCallback(this.OnExceptionFromTry));
            }
            else
            {
                this.OnTryComplete(context, null);
            }
        }

        private Catch FindCatch(Exception exception)
        {
            Type c = exception.GetType();
            Catch @catch = null;
            foreach (Catch catch2 in this.Catches)
            {
                if (catch2.ExceptionType == c)
                {
                    return catch2;
                }
                if (catch2.ExceptionType.IsAssignableFrom(c))
                {
                    if (@catch != null)
                    {
                        if (catch2.ExceptionType.IsSubclassOf(@catch.ExceptionType))
                        {
                            @catch = catch2;
                        }
                    }
                    else
                    {
                        @catch = catch2;
                    }
                }
            }
            return @catch;
        }

        internal static Catch FindCatchActivity(Type typeToMatch, IList<Catch> catches)
        {
            foreach (Catch @catch in catches)
            {
                if (@catch.ExceptionType == typeToMatch)
                {
                    return @catch;
                }
            }
            return null;
        }

        private void OnCatchComplete(NativeActivityContext context, System.Activities.ActivityInstance completedInstance)
        {
            TryCatchState state = this.state.Get(context);
            state.SuppressCancel = true;
            if ((completedInstance != null) && (completedInstance.State != ActivityInstanceState.Closed))
            {
                state.ExceptionHandled = false;
            }
            context.Properties.Remove("{35ABC8C3-9AF1-4426-8293-A6DDBB6ED91D}");
            if (this.Finally != null)
            {
                context.ScheduleActivity(this.Finally, new CompletionCallback(this.OnFinallyComplete), this.ExceptionFromCatchOrFinallyHandler);
            }
            else
            {
                this.OnFinallyComplete(context, null);
            }
        }

        private void OnExceptionFromCatchOrFinally(NativeActivityFaultContext context, Exception propagatedException, System.Activities.ActivityInstance propagatedFrom)
        {
            if (TD.TryCatchExceptionFromCatchOrFinallyIsEnabled())
            {
                TD.TryCatchExceptionFromCatchOrFinally(base.DisplayName);
            }
            this.state.Get(context).SuppressCancel = false;
        }

        private void OnExceptionFromTry(NativeActivityFaultContext context, Exception propagatedException, System.Activities.ActivityInstance propagatedFrom)
        {
            if (propagatedFrom.IsCancellationRequested)
            {
                if (TD.TryCatchExceptionDuringCancelationIsEnabled())
                {
                    TD.TryCatchExceptionDuringCancelation(base.DisplayName);
                }
                context.Abort(propagatedException);
                context.HandleFault();
            }
            else if (this.FindCatch(propagatedException) != null)
            {
                if (TD.TryCatchExceptionFromTryIsEnabled())
                {
                    TD.TryCatchExceptionFromTry(base.DisplayName, propagatedException.GetType().ToString());
                }
                context.CancelChild(propagatedFrom);
                this.state.Get(context).CaughtException = context.CreateFaultContext();
                context.HandleFault();
            }
        }

        private void OnFinallyComplete(NativeActivityContext context, System.Activities.ActivityInstance completedInstance)
        {
            TryCatchState state = this.state.Get(context);
            if (context.IsCancellationRequested && !state.ExceptionHandled)
            {
                context.MarkCanceled();
            }
        }

        private void OnTryComplete(NativeActivityContext context, System.Activities.ActivityInstance completedInstance)
        {
            TryCatchState state = this.state.Get(context);
            state.SuppressCancel = true;
            if (state.CaughtException != null)
            {
                Catch @catch = this.FindCatch(state.CaughtException.Exception);
                if (@catch != null)
                {
                    state.ExceptionHandled = true;
                    if (@catch.GetAction() != null)
                    {
                        context.Properties.Add("{35ABC8C3-9AF1-4426-8293-A6DDBB6ED91D}", state.CaughtException, true);
                        @catch.ScheduleAction(context, state.CaughtException.Exception, new CompletionCallback(this.OnCatchComplete), this.ExceptionFromCatchOrFinallyHandler);
                        return;
                    }
                }
            }
            this.OnCatchComplete(context, null);
        }

        [DependsOn("Try")]
        public Collection<Catch> Catches
        {
            get
            {
                if (this.catches == null)
                {
                    this.catches = new CatchList();
                }
                return this.catches;
            }
        }

        private FaultCallback ExceptionFromCatchOrFinallyHandler
        {
            get
            {
                if (this.exceptionFromCatchOrFinallyHandler == null)
                {
                    this.exceptionFromCatchOrFinallyHandler = new FaultCallback(this.OnExceptionFromCatchOrFinally);
                }
                return this.exceptionFromCatchOrFinallyHandler;
            }
        }

        [DefaultValue((string) null), DependsOn("Catches")]
        public Activity Finally { get; set; }

        [DefaultValue((string) null), DependsOn("Variables")]
        public Activity Try { get; set; }

        public Collection<Variable> Variables
        {
            get
            {
                if (this.variables == null)
                {
                    ValidatingCollection<Variable> validatings = new ValidatingCollection<Variable> {
                        OnAddValidationCallback = delegate (Variable item) {
                            if (item == null)
                            {
                                throw FxTrace.Exception.ArgumentNull("item");
                            }
                        }
                    };
                    this.variables = validatings;
                }
                return this.variables;
            }
        }

        private class CatchList : ValidatingCollection<Catch>
        {
            public CatchList()
            {
                base.OnAddValidationCallback = delegate (Catch item) {
                    if (item == null)
                    {
                        throw FxTrace.Exception.ArgumentNull("item");
                    }
                };
            }

            protected override void InsertItem(int index, Catch item)
            {
                if (item == null)
                {
                    throw FxTrace.Exception.ArgumentNull("item");
                }
                if (TryCatch.FindCatchActivity(item.ExceptionType, base.Items) != null)
                {
                    throw FxTrace.Exception.Argument("item", System.Activities.SR.DuplicateCatchClause(item.ExceptionType.FullName));
                }
                base.InsertItem(index, item);
            }

            protected override void SetItem(int index, Catch item)
            {
                if (item == null)
                {
                    throw FxTrace.Exception.ArgumentNull("item");
                }
                Catch objB = TryCatch.FindCatchActivity(item.ExceptionType, base.Items);
                if ((objB != null) && !object.ReferenceEquals(base[index], objB))
                {
                    throw FxTrace.Exception.Argument("item", System.Activities.SR.DuplicateCatchClause(item.ExceptionType.FullName));
                }
                base.SetItem(index, item);
            }
        }

        [DataContract]
        internal class TryCatchState
        {
            [DataMember(EmitDefaultValue=false)]
            public FaultContext CaughtException { get; set; }

            [DataMember(EmitDefaultValue=false)]
            public bool ExceptionHandled { get; set; }

            [DataMember(EmitDefaultValue=false)]
            public bool SuppressCancel { get; set; }
        }
    }
}

