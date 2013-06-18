namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.Collections;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;

    [ContentProperty("Action")]
    public sealed class InvokeAction : NativeActivity
    {
        private IList<Argument> actionArguments;

        public InvokeAction()
        {
            ValidatingCollection<Argument> validatings = new ValidatingCollection<Argument> {
                OnAddValidationCallback = delegate (Argument item) {
                    if (item == null)
                    {
                        throw FxTrace.Exception.ArgumentNull("item");
                    }
                }
            };
            this.actionArguments = validatings;
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            metadata.AddDelegate(this.Action);
        }

        protected override void Execute(NativeActivityContext context)
        {
            if ((this.Action != null) && (this.Action.Handler != null))
            {
                context.ScheduleAction(this.Action, null, null);
            }
        }

        [DefaultValue((string) null)]
        public ActivityAction Action { get; set; }
    }
}

