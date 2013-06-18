namespace System.Web.UI.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing.Design;
    using System.Security.Permissions;
    using System.Web.UI;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class HtmlControlDesigner : ComponentDesigner
    {
        private IHtmlControlDesignerBehavior behavior;
        private bool shouldCodeSerialize = true;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.BehaviorInternal != null))
            {
                this.BehaviorInternal.Designer = null;
                this.BehaviorInternal = null;
            }
            base.Dispose(disposing);
        }

        public override void Initialize(IComponent component)
        {
            ControlDesigner.VerifyInitializeArgument(component, typeof(Control));
            base.Initialize(component);
        }

        [Obsolete("The recommended alternative is ControlDesigner.Tag. http://go.microsoft.com/fwlink/?linkid=14202")]
        protected virtual void OnBehaviorAttached()
        {
        }

        [Obsolete("The recommended alternative is ControlDesigner.Tag. http://go.microsoft.com/fwlink/?linkid=14202")]
        protected virtual void OnBehaviorDetaching()
        {
        }

        [Obsolete("The recommended alternative is to handle the Changed event on the DataBindings collection. The DataBindings collection allows more control of the databindings associated with the control. http://go.microsoft.com/fwlink/?linkid=14202")]
        protected virtual void OnBindingsCollectionChanged(string propName)
        {
        }

        internal void OnBindingsCollectionChangedInternal(string propName)
        {
            this.OnBindingsCollectionChanged(propName);
        }

        public virtual void OnSetParent()
        {
        }

        protected override void PreFilterEvents(IDictionary events)
        {
            base.PreFilterEvents(events);
            if (!this.ShouldCodeSerializeInternal)
            {
                ICollection values = events.Values;
                if ((values != null) && (values.Count != 0))
                {
                    object[] array = new object[values.Count];
                    values.CopyTo(array, 0);
                    for (int i = 0; i < array.Length; i++)
                    {
                        EventDescriptor oldEventDescriptor = (EventDescriptor) array[i];
                        oldEventDescriptor = TypeDescriptor.CreateEvent(oldEventDescriptor.ComponentType, oldEventDescriptor, new Attribute[] { BrowsableAttribute.No });
                        events[oldEventDescriptor.Name] = oldEventDescriptor;
                    }
                }
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) properties["Modifiers"];
            if (oldPropertyDescriptor != null)
            {
                properties["Modifiers"] = TypeDescriptor.CreateProperty(oldPropertyDescriptor.ComponentType, oldPropertyDescriptor, new Attribute[] { BrowsableAttribute.No });
            }
            properties["Expressions"] = TypeDescriptor.CreateProperty(base.GetType(), "Expressions", typeof(ExpressionBindingCollection), new Attribute[] { DesignerSerializationVisibilityAttribute.Hidden, CategoryAttribute.Data, new EditorAttribute(typeof(ExpressionsCollectionEditor), typeof(UITypeEditor)), new TypeConverterAttribute(typeof(ExpressionsCollectionConverter)), new ParenthesizePropertyNameAttribute(true), MergablePropertyAttribute.No, new DescriptionAttribute(System.Design.SR.GetString("Control_Expressions")) });
        }

        [Obsolete("The recommended alternative is ControlDesigner.Tag. http://go.microsoft.com/fwlink/?linkid=14202")]
        public IHtmlControlDesignerBehavior Behavior
        {
            get
            {
                return this.BehaviorInternal;
            }
            set
            {
                this.BehaviorInternal = value;
            }
        }

        internal virtual IHtmlControlDesignerBehavior BehaviorInternal
        {
            get
            {
                return this.behavior;
            }
            set
            {
                if (this.behavior != value)
                {
                    if (this.behavior != null)
                    {
                        this.OnBehaviorDetaching();
                        this.behavior.Designer = null;
                        this.behavior = null;
                    }
                    if (value != null)
                    {
                        this.behavior = value;
                        this.OnBehaviorAttached();
                    }
                }
            }
        }

        public DataBindingCollection DataBindings
        {
            get
            {
                return ((IDataBindingsAccessor) base.Component).DataBindings;
            }
        }

        [Obsolete("Error: This property can no longer be referenced, and is included to support existing compiled applications. The design-time element may not always provide access to the element in the markup. There are alternate methods on WebFormsRootDesigner for handling client script and controls. http://go.microsoft.com/fwlink/?linkid=14202", true)]
        protected object DesignTimeElement
        {
            get
            {
                return this.DesignTimeElementInternal;
            }
        }

        internal object DesignTimeElementInternal
        {
            get
            {
                if (this.behavior == null)
                {
                    return null;
                }
                return this.behavior.DesignTimeElement;
            }
        }

        public ExpressionBindingCollection Expressions
        {
            get
            {
                return ((IExpressionsAccessor) base.Component).Expressions;
            }
        }

        [Obsolete("Use of this property is not recommended because code serialization is not supported. http://go.microsoft.com/fwlink/?linkid=14202")]
        public virtual bool ShouldCodeSerialize
        {
            get
            {
                return this.ShouldCodeSerializeInternal;
            }
            set
            {
                this.ShouldCodeSerializeInternal = value;
            }
        }

        internal virtual bool ShouldCodeSerializeInternal
        {
            get
            {
                return this.shouldCodeSerialize;
            }
            set
            {
                this.shouldCodeSerialize = value;
            }
        }
    }
}

