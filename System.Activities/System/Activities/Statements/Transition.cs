namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;

    public sealed class Transition
    {
        [DefaultValue((string) null), DependsOn("To")]
        public Activity Action { get; set; }

        internal Activity ActiveTrigger
        {
            get
            {
                if (this.Trigger == null)
                {
                    return this.Source.NullTrigger;
                }
                return this.Trigger;
            }
        }

        [DefaultValue((string) null), DependsOn("Action")]
        public Activity<bool> Condition { get; set; }

        public string DisplayName { get; set; }

        internal string Id { get; set; }

        internal System.Activities.Statements.State Source { get; set; }

        [DefaultValue((string) null), DependsOn("Trigger")]
        public System.Activities.Statements.State To { get; set; }

        [DefaultValue((string) null)]
        public Activity Trigger { get; set; }
    }
}

