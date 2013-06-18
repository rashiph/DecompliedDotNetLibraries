namespace System.Workflow.ComponentModel
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;

    public class PropertyMetadata
    {
        private Attribute[] attributes;
        private object defaultValue;
        private System.Workflow.ComponentModel.GetValueOverride getValueOverride;
        private DependencyPropertyOptions options;
        private bool propertySealed;
        private System.Workflow.ComponentModel.SetValueOverride setValueOverride;
        private bool shouldAlwaysCallOverride;

        public PropertyMetadata()
        {
            this.options = DependencyPropertyOptions.Default;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public PropertyMetadata(object defaultValue) : this(defaultValue, 0)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public PropertyMetadata(DependencyPropertyOptions options) : this(null, options)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public PropertyMetadata(params Attribute[] attributes) : this(null, 0, null, null, attributes)
        {
        }

        public PropertyMetadata(object defaultValue, DependencyPropertyOptions options) : this(defaultValue, options, null, null, new Attribute[0])
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public PropertyMetadata(object defaultValue, params Attribute[] attributes) : this(defaultValue, 0, null, null, attributes)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public PropertyMetadata(DependencyPropertyOptions options, params Attribute[] attributes) : this(null, options, null, null, attributes)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public PropertyMetadata(object defaultValue, DependencyPropertyOptions options, params Attribute[] attributes) : this(defaultValue, options, null, null, attributes)
        {
        }

        public PropertyMetadata(object defaultValue, DependencyPropertyOptions options, System.Workflow.ComponentModel.GetValueOverride getValueOverride, System.Workflow.ComponentModel.SetValueOverride setValueOverride) : this(defaultValue, options, getValueOverride, setValueOverride, new Attribute[0])
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public PropertyMetadata(object defaultValue, DependencyPropertyOptions options, System.Workflow.ComponentModel.GetValueOverride getValueOverride, System.Workflow.ComponentModel.SetValueOverride setValueOverride, params Attribute[] attributes) : this(defaultValue, options, getValueOverride, setValueOverride, false, attributes)
        {
        }

        internal PropertyMetadata(object defaultValue, DependencyPropertyOptions options, System.Workflow.ComponentModel.GetValueOverride getValueOverride, System.Workflow.ComponentModel.SetValueOverride setValueOverride, bool shouldAlwaysCallOverride, params Attribute[] attributes)
        {
            this.options = DependencyPropertyOptions.Default;
            this.defaultValue = defaultValue;
            this.options = options;
            this.getValueOverride = getValueOverride;
            this.setValueOverride = setValueOverride;
            this.shouldAlwaysCallOverride = shouldAlwaysCallOverride;
            this.attributes = attributes;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public Attribute[] GetAttributes()
        {
            return this.GetAttributes(null);
        }

        public Attribute[] GetAttributes(Type attributeType)
        {
            List<Attribute> list = new List<Attribute>();
            if (this.attributes != null)
            {
                foreach (Attribute attribute in this.attributes)
                {
                    if ((attribute != null) && ((attributeType == null) || (attribute.GetType() == attributeType)))
                    {
                        list.Add(attribute);
                    }
                }
            }
            return list.ToArray();
        }

        protected virtual void OnApply(DependencyProperty dependencyProperty, Type targetType)
        {
        }

        internal void Seal(DependencyProperty dependencyProperty, Type targetType)
        {
            this.OnApply(dependencyProperty, targetType);
            this.propertySealed = true;
        }

        public object DefaultValue
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.defaultValue;
            }
            set
            {
                if (this.propertySealed)
                {
                    throw new InvalidOperationException(SR.GetString("Error_SealedPropertyMetadata"));
                }
                this.defaultValue = value;
            }
        }

        public System.Workflow.ComponentModel.GetValueOverride GetValueOverride
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.getValueOverride;
            }
            set
            {
                if (this.propertySealed)
                {
                    throw new InvalidOperationException(SR.GetString("Error_SealedPropertyMetadata"));
                }
                this.getValueOverride = value;
            }
        }

        public bool IsMetaProperty
        {
            get
            {
                return (((byte) (this.options & DependencyPropertyOptions.Metadata)) > 0);
            }
        }

        public bool IsNonSerialized
        {
            get
            {
                return (((byte) (this.options & DependencyPropertyOptions.NonSerialized)) > 0);
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return (((byte) (this.options & DependencyPropertyOptions.ReadOnly)) > 0);
            }
        }

        protected bool IsSealed
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.propertySealed;
            }
        }

        public DependencyPropertyOptions Options
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.options;
            }
            set
            {
                if (this.propertySealed)
                {
                    throw new InvalidOperationException(SR.GetString("Error_SealedPropertyMetadata"));
                }
                this.options = value;
            }
        }

        public System.Workflow.ComponentModel.SetValueOverride SetValueOverride
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.setValueOverride;
            }
            set
            {
                if (this.propertySealed)
                {
                    throw new InvalidOperationException(SR.GetString("Error_SealedPropertyMetadata"));
                }
                this.setValueOverride = value;
            }
        }

        internal bool ShouldAlwaysCallOverride
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.shouldAlwaysCallOverride;
            }
        }
    }
}

