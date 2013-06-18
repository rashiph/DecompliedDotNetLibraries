namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.Collections;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;

    public sealed class State
    {
        private System.Activities.Statements.InternalState internalState;
        private NoOp nullTrigger;
        private Collection<Transition> transitions;
        private Collection<Variable> variables;

        internal void ClearInternalState()
        {
            this.internalState = null;
        }

        public string DisplayName { get; set; }

        [DefaultValue((string) null)]
        public Activity Entry { get; set; }

        [DefaultValue((string) null), DependsOn("Entry")]
        public Activity Exit { get; set; }

        internal System.Activities.Statements.InternalState InternalState
        {
            get
            {
                if (this.internalState == null)
                {
                    this.internalState = new System.Activities.Statements.InternalState(this);
                }
                return this.internalState;
            }
        }

        [DefaultValue(false)]
        public bool IsFinal { get; set; }

        internal NoOp NullTrigger
        {
            get
            {
                if (this.nullTrigger == null)
                {
                    NoOp op = new NoOp {
                        DisplayName = "Null Trigger"
                    };
                    this.nullTrigger = op;
                }
                return this.nullTrigger;
            }
        }

        internal uint PassNumber { get; set; }

        internal bool Reachable { get; set; }

        internal string StateId { get; set; }

        internal string StateMachineName { get; set; }

        [DependsOn("Exit")]
        public Collection<Transition> Transitions
        {
            get
            {
                if (this.transitions == null)
                {
                    ValidatingCollection<Transition> validatings = new ValidatingCollection<Transition> {
                        OnAddValidationCallback = delegate (Transition item) {
                            if (item == null)
                            {
                                throw FxTrace.Exception.AsError(new ArgumentNullException("item"));
                            }
                        }
                    };
                    this.transitions = validatings;
                }
                return this.transitions;
            }
        }

        [DependsOn("Transitions")]
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
                                throw FxTrace.Exception.AsError(new ArgumentNullException("item"));
                            }
                        }
                    };
                    this.variables = validatings;
                }
                return this.variables;
            }
        }

        internal sealed class NoOp : CodeActivity
        {
            protected override void Execute(CodeActivityContext context)
            {
            }
        }
    }
}

