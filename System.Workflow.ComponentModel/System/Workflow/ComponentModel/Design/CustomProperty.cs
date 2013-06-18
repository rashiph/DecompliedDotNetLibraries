namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    internal sealed class CustomProperty
    {
        private bool browseable = true;
        private string category;
        private string description;
        private System.ComponentModel.DesignerSerializationVisibility designerSerializationVisibility = System.ComponentModel.DesignerSerializationVisibility.Visible;
        private bool generateDependencyProperty = true;
        private bool hidden;
        private bool isEvent;
        private string name;
        public string oldPropertyName;
        public string oldPropertyType;
        private IServiceProvider serviceProvider;
        private string type;
        private string uiTypeEditor;

        public CustomProperty(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public static CustomProperty CreateCustomProperty(IServiceProvider serviceProvider, string customPropertyName, PropertyDescriptor propertyDescriptor, object propertyOwner)
        {
            CustomProperty property = new CustomProperty(serviceProvider) {
                Name = customPropertyName
            };
            if (TypeProvider.IsAssignable(typeof(ActivityBind), propertyDescriptor.PropertyType))
            {
                System.Type type = PropertyDescriptorUtils.GetBaseType(propertyDescriptor, propertyOwner, serviceProvider);
                if (type == null)
                {
                    throw new InvalidOperationException(SR.GetString("Error_CantDeterminePropertyBaseType", new object[] { propertyDescriptor.Name }));
                }
                property.Type = type.FullName;
            }
            else
            {
                property.Type = propertyDescriptor.PropertyType.FullName;
            }
            if (propertyDescriptor is ActivityBindPropertyDescriptor)
            {
                DependencyProperty property2 = DependencyProperty.FromName(propertyDescriptor.Name, propertyDescriptor.ComponentType);
                property.IsEvent = (property2 != null) && property2.IsEvent;
            }
            property.Category = propertyDescriptor.Category;
            return property;
        }

        public bool Browseable
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.browseable;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.browseable = value;
            }
        }

        public string Category
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.category;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.category = value;
            }
        }

        public string Description
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.description;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.description = value;
            }
        }

        public System.ComponentModel.DesignerSerializationVisibility DesignerSerializationVisibility
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.designerSerializationVisibility;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.designerSerializationVisibility = value;
            }
        }

        public bool GenerateDependencyProperty
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.generateDependencyProperty;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.generateDependencyProperty = value;
            }
        }

        public bool Hidden
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.hidden;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.hidden = value;
            }
        }

        public bool IsEvent
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.isEvent;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.isEvent = value;
            }
        }

        public string Name
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.name;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.name = value;
            }
        }

        public string Type
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.type;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.type = value;
            }
        }

        public string UITypeEditor
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.uiTypeEditor;
            }
            set
            {
                string name = value;
                if (this.serviceProvider != null)
                {
                    ITypeProvider service = this.serviceProvider.GetService(typeof(ITypeProvider)) as ITypeProvider;
                    if (service == null)
                    {
                        throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(ITypeProvider).FullName }));
                    }
                    System.Type type = service.GetType(name);
                    if (type != null)
                    {
                        name = type.FullName;
                    }
                }
                this.uiTypeEditor = name;
            }
        }
    }
}

