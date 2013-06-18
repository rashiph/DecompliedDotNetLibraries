namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Drawing;
    using System.Reflection;
    using System.Runtime;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    public class ActivityCodeDomSerializationManager : IDesignerSerializationManager, IServiceProvider
    {
        private IDesignerSerializationManager serializationManager;
        private ServiceContainer serviceContainer;

        public event ResolveNameEventHandler ResolveName;

        public event EventHandler SerializationComplete;

        public ActivityCodeDomSerializationManager(IDesignerSerializationManager manager)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            this.serializationManager = manager;
            this.serviceContainer = new ServiceContainer();
            this.serializationManager.ResolveName += new ResolveNameEventHandler(this.OnResolveName);
            this.serializationManager.SerializationComplete += new EventHandler(this.OnSerializationComplete);
            if (this.serializationManager is DesignerSerializationManager)
            {
                ((DesignerSerializationManager) this.serializationManager).SessionDisposed += new EventHandler(this.OnSessionDisposed);
            }
        }

        public void AddSerializationProvider(IDesignerSerializationProvider provider)
        {
            this.serializationManager.AddSerializationProvider(provider);
        }

        public object CreateInstance(Type type, ICollection arguments, string name, bool addToContainer)
        {
            return this.serializationManager.CreateInstance(type, arguments, name, false);
        }

        public object GetInstance(string name)
        {
            return this.serializationManager.GetInstance(name);
        }

        public string GetName(object value)
        {
            string name = null;
            Activity activity = value as Activity;
            if (activity != null)
            {
                if (activity.Parent == null)
                {
                    name = activity.GetValue(WorkflowMarkupSerializer.XClassProperty) as string;
                    if ((name != null) && (name.LastIndexOf(".") > 0))
                    {
                        name = name.Substring(name.LastIndexOf('.') + 1);
                    }
                }
                else
                {
                    name = activity.QualifiedName.Replace('.', '_');
                }
            }
            if (name == null)
            {
                name = this.serializationManager.GetName(value);
            }
            return name;
        }

        public object GetSerializer(Type objectType, Type serializerType)
        {
            if (objectType == typeof(string))
            {
                return System.Workflow.ComponentModel.Serialization.PrimitiveCodeDomSerializer.Default;
            }
            if (((objectType != null) && TypeProvider.IsAssignable(typeof(ICollection<string>), objectType)) && (!objectType.IsArray && (serializerType == typeof(CodeDomSerializer))))
            {
                PropertyDescriptor descriptor = this.Context[typeof(PropertyDescriptor)] as PropertyDescriptor;
                if (descriptor != null)
                {
                    if (string.Equals(descriptor.Name, "SynchronizationHandles", StringComparison.Ordinal) && TypeProvider.IsAssignable(typeof(Activity), descriptor.ComponentType))
                    {
                        return new SynchronizationHandlesCodeDomSerializer();
                    }
                }
                else
                {
                    ExpressionContext context = this.Context[typeof(ExpressionContext)] as ExpressionContext;
                    if (((context != null) && (context.Expression is CodePropertyReferenceExpression)) && string.Equals(((CodePropertyReferenceExpression) context.Expression).PropertyName, "SynchronizationHandles", StringComparison.Ordinal))
                    {
                        return new SynchronizationHandlesCodeDomSerializer();
                    }
                }
            }
            object serializer = this.serializationManager.GetSerializer(objectType, serializerType);
            if (!this.UseUserDefinedSerializer(objectType, serializerType))
            {
                serializer = new SerializableTypeCodeDomSerializer(serializer as CodeDomSerializer);
            }
            return serializer;
        }

        public object GetService(Type serviceType)
        {
            object service = null;
            if (serviceType == typeof(IReferenceService))
            {
                service = new ActivityCodeDomReferenceService(this.serializationManager.GetService(serviceType) as IReferenceService);
            }
            if (serviceType == typeof(IServiceContainer))
            {
                service = this.serializationManager.GetService(serviceType);
                if (service == null)
                {
                    service = this.serviceContainer;
                }
            }
            if (service == null)
            {
                service = this.serializationManager.GetService(serviceType);
            }
            if (service == null)
            {
                service = this.serviceContainer.GetService(serviceType);
            }
            return service;
        }

        public Type GetType(string typeName)
        {
            Type type = this.serializationManager.GetType(typeName);
            if (type == null)
            {
                ITypeProvider service = this.GetService(typeof(ITypeProvider)) as ITypeProvider;
                if (service != null)
                {
                    type = service.GetType(typeName);
                }
            }
            return type;
        }

        private void OnResolveName(object sender, ResolveNameEventArgs e)
        {
            if (this.resolveNameEventHandler != null)
            {
                this.resolveNameEventHandler(this, e);
            }
        }

        private void OnSerializationComplete(object sender, EventArgs e)
        {
            if (this.serializationCompleteEventHandler != null)
            {
                this.serializationCompleteEventHandler(this, e);
            }
        }

        private void OnSessionDisposed(object sender, EventArgs e)
        {
            try
            {
                if (this.serializationCompleteEventHandler != null)
                {
                    this.serializationCompleteEventHandler(this, EventArgs.Empty);
                }
            }
            finally
            {
                this.resolveNameEventHandler = null;
                this.serializationCompleteEventHandler = null;
            }
        }

        public void RemoveSerializationProvider(IDesignerSerializationProvider provider)
        {
            this.serializationManager.RemoveSerializationProvider(provider);
        }

        public void ReportError(object errorInformation)
        {
            this.serializationManager.ReportError(errorInformation);
        }

        public void SetName(object instance, string name)
        {
            if (this.GetInstance(name) != instance)
            {
                this.serializationManager.SetName(instance, name);
            }
        }

        private bool UseUserDefinedSerializer(Type objectType, Type serializerType)
        {
            if ((objectType == null) || (serializerType == null))
            {
                return true;
            }
            if (!objectType.IsSerializable || (serializerType != typeof(CodeDomSerializer)))
            {
                return true;
            }
            if ((objectType.IsPrimitive || objectType.IsEnum) || ((objectType == typeof(string)) || typeof(Activity).IsAssignableFrom(objectType)))
            {
                return true;
            }
            TypeConverter converter = TypeDescriptor.GetConverter(objectType);
            if ((converter != null) && converter.CanConvertTo(typeof(InstanceDescriptor)))
            {
                return true;
            }
            object serializer = this.serializationManager.GetSerializer(objectType, serializerType);
            if (((serializer.GetType().Assembly != typeof(CodeDomSerializer).Assembly) && (serializer.GetType().Assembly != Assembly.GetExecutingAssembly())) && (serializer.GetType().Assembly != Assembly.Load("System.Workflow.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")))
            {
                return true;
            }
            Activity activity = this.serializationManager.Context[typeof(Activity)] as Activity;
            return ((((activity != null) && (activity.Site != null)) && ((activity.Site.Container != null) && (objectType.Namespace != null))) && objectType.Namespace.Equals(typeof(Image).Namespace));
        }

        public ContextStack Context
        {
            get
            {
                return this.serializationManager.Context;
            }
        }

        public PropertyDescriptorCollection Properties
        {
            get
            {
                return this.serializationManager.Properties;
            }
        }

        protected IDesignerSerializationManager SerializationManager
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.serializationManager;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.serializationManager = value;
            }
        }
    }
}

