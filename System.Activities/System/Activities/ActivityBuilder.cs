namespace System.Activities
{
    using System;
    using System.Activities.Debugger;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;
    using System.Xaml;

    [ContentProperty("Implementation")]
    public sealed class ActivityBuilder : IDebuggableWorkflowTree
    {
        private Collection<Attribute> attributes;
        private Collection<Constraint> constraints;
        private KeyedCollection<string, DynamicActivityProperty> properties;
        private static AttachableMemberIdentifier propertyReferencePropertyID = new AttachableMemberIdentifier(typeof(ActivityBuilder), "PropertyReference");

        internal static KeyedCollection<string, DynamicActivityProperty> CreateActivityPropertyCollection()
        {
            return new ActivityPropertyCollection();
        }

        public static ActivityPropertyReference GetPropertyReference(object target)
        {
            ActivityPropertyReference reference;
            if (!AttachablePropertyServices.TryGetProperty<ActivityPropertyReference>(target, propertyReferencePropertyID, out reference))
            {
                return null;
            }
            return reference;
        }

        public static void SetPropertyReference(object target, ActivityPropertyReference value)
        {
            AttachablePropertyServices.SetProperty(target, propertyReferencePropertyID, value);
        }

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

        [DefaultValue((string) null), Browsable(false), DependsOn("Constraints")]
        public Activity Implementation { get; set; }

        public string Name { get; set; }

        [DependsOn("Attributes"), Browsable(false)]
        public KeyedCollection<string, DynamicActivityProperty> Properties
        {
            get
            {
                if (this.properties == null)
                {
                    this.properties = new ActivityPropertyCollection();
                }
                return this.properties;
            }
        }

        private class ActivityPropertyCollection : KeyedCollection<string, DynamicActivityProperty>
        {
            protected override string GetKeyForItem(DynamicActivityProperty item)
            {
                return item.Name;
            }
        }
    }
}

