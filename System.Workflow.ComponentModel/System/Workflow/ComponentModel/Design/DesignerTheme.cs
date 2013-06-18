namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Runtime;
    using System.Workflow.ComponentModel.Serialization;

    [TypeConverter(typeof(ThemeTypeConverter)), DesignerSerializer(typeof(DesignerTheme.ThemeSerializer), typeof(WorkflowMarkupSerializer))]
    public abstract class DesignerTheme : IDisposable, IPropertyValueProvider
    {
        private Type designerType;
        private string designerTypeName = string.Empty;
        private WorkflowTheme workflowTheme;

        protected DesignerTheme(WorkflowTheme theme)
        {
            this.workflowTheme = theme;
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        ~DesignerTheme()
        {
            this.Dispose(false);
        }

        internal virtual ICollection GetPropertyValues(ITypeDescriptorContext context)
        {
            return new object[0];
        }

        public virtual void Initialize()
        {
        }

        public virtual void OnAmbientPropertyChanged(AmbientProperty ambientProperty)
        {
        }

        void IDisposable.Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        ICollection IPropertyValueProvider.GetPropertyValues(ITypeDescriptorContext context)
        {
            return this.GetPropertyValues(context);
        }

        [Browsable(false)]
        public virtual string ApplyTo
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.designerTypeName;
            }
            set
            {
                if (this.ReadOnly)
                {
                    throw new InvalidOperationException(DR.GetString("ThemePropertyReadOnly", new object[0]));
                }
                this.designerTypeName = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        protected WorkflowTheme ContainingTheme
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.workflowTheme;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual Type DesignerType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.designerType;
            }
            set
            {
                if (this.ReadOnly)
                {
                    throw new InvalidOperationException(DR.GetString("ThemePropertyReadOnly", new object[0]));
                }
                this.designerType = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ReadOnly
        {
            get
            {
                return ((this.workflowTheme != null) && this.workflowTheme.ReadOnly);
            }
            internal set
            {
                if (this.workflowTheme != null)
                {
                    this.workflowTheme.ReadOnly = value;
                }
            }
        }

        private class ThemeSerializer : WorkflowMarkupSerializer
        {
            protected override object CreateInstance(WorkflowMarkupSerializationManager serializationManager, Type type)
            {
                if (typeof(DesignerTheme).IsAssignableFrom(type))
                {
                    return Activator.CreateInstance(type, new object[] { serializationManager.Context[typeof(WorkflowTheme)] });
                }
                return base.CreateInstance(serializationManager, type);
            }
        }
    }
}

