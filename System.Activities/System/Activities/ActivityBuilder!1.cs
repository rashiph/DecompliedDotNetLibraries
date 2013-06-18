namespace System.Activities
{
    using System;
    using System.Activities.Debugger;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;

    [ContentProperty("Implementation")]
    public sealed class ActivityBuilder<TResult> : IDebuggableWorkflowTree
    {
        private Collection<Attribute> attributes;
        private Collection<Constraint> constraints;
        private KeyedCollection<string, DynamicActivityProperty> properties;

        Activity IDebuggableWorkflowTree.GetWorkflowRoot()
        {
            return this.Implementation;
        }

        [DependsOn("Name")]
        public Collection<Attribute> Attributes
        {
            get
            {
                if (this.attributes == null)
                {
                    this.attributes = new Collection<Attribute>();
                }
                return this.attributes;
            }
        }

        [Browsable(false), DependsOn("Properties")]
        public Collection<Constraint> Constraints
        {
            get
            {
                if (this.constraints == null)
                {
                    this.constraints = new Collection<Constraint>();
                }
                return this.constraints;
            }
        }

        [DependsOn("Constraints"), DefaultValue((string) null), Browsable(false)]
        public Activity Implementation { get; set; }

        public string Name { get; set; }

        [DependsOn("Attributes"), Browsable(false)]
        public KeyedCollection<string, DynamicActivityProperty> Properties
        {
            get
            {
                if (this.properties == null)
                {
                    this.properties = ActivityBuilder.CreateActivityPropertyCollection();
                }
                return this.properties;
            }
        }
    }
}

