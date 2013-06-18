namespace System.Workflow.ComponentModel
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;

    [SRCategory("Standard"), Designer(typeof(ThrowDesigner), typeof(IDesigner)), SRDescription("FaultActivityDescription"), ToolboxBitmap(typeof(ThrowActivity), "Resources.Throw.png"), ToolboxItem(typeof(ActivityToolboxItem))]
    public sealed class ThrowActivity : Activity, ITypeFilterProvider, IDynamicPropertyTypeProvider
    {
        [Browsable(false)]
        public static readonly DependencyProperty FaultProperty = DependencyProperty.Register("Fault", typeof(Exception), typeof(ThrowActivity));
        [Browsable(false)]
        public static readonly DependencyProperty FaultTypeProperty = DependencyProperty.Register("FaultType", typeof(Type), typeof(ThrowActivity));

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ThrowActivity()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ThrowActivity(string name) : base(name)
        {
        }

        protected internal sealed override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if ((this.Fault == null) && (this.FaultType == null))
            {
                throw new InvalidOperationException(SR.GetString(CultureInfo.CurrentCulture, "Error_PropertyNotSet", new object[] { FaultProperty.Name }));
            }
            if ((this.FaultType != null) && !typeof(Exception).IsAssignableFrom(this.FaultType))
            {
                throw new InvalidOperationException(SR.GetString(CultureInfo.CurrentCulture, "Error_ExceptionTypeNotException", new object[] { this.FaultType, FaultTypeProperty.Name }));
            }
            if (((this.Fault != null) && (this.FaultType != null)) && !this.FaultType.IsInstanceOfType(this.Fault))
            {
                throw new InvalidOperationException(SR.GetString(CultureInfo.CurrentCulture, "Error_FaultIsNotOfFaultType"));
            }
            if (this.Fault != null)
            {
                throw this.Fault;
            }
            ConstructorInfo constructor = this.FaultType.GetConstructor(new Type[0]);
            if (constructor != null)
            {
                throw ((Exception) constructor.Invoke(null));
            }
            throw new InvalidOperationException(SR.GetString(CultureInfo.CurrentCulture, "Error_FaultTypeNoDefaultConstructor", new object[] { this.FaultType }));
        }

        protected internal override void Initialize(IServiceProvider provider)
        {
            if (base.Parent == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_MustHaveParent"));
            }
            base.Initialize(provider);
        }

        bool ITypeFilterProvider.CanFilterType(Type type, bool throwOnError)
        {
            bool flag = TypeProvider.IsAssignable(typeof(Exception), type);
            if (throwOnError && !flag)
            {
                throw new Exception(SR.GetString("Error_ExceptionTypeNotException", new object[] { type, "Type" }));
            }
            return flag;
        }

        AccessTypes IDynamicPropertyTypeProvider.GetAccessType(IServiceProvider serviceProvider, string propertyName)
        {
            return AccessTypes.Read;
        }

        Type IDynamicPropertyTypeProvider.GetPropertyType(IServiceProvider serviceProvider, string propertyName)
        {
            if (!string.IsNullOrEmpty(propertyName) && propertyName.Equals("Fault", StringComparison.Ordinal))
            {
                return this.FaultType;
            }
            return null;
        }

        [DefaultValue((string) null), SRDescription("FaultDescription"), MergableProperty(false), TypeConverter(typeof(FaultConverter)), SRCategory("Handlers")]
        public Exception Fault
        {
            get
            {
                return (base.GetValue(FaultProperty) as Exception);
            }
            set
            {
                base.SetValue(FaultProperty, value);
            }
        }

        [SRCategory("Handlers"), TypeConverter(typeof(FaultTypeConverter)), Editor(typeof(TypeBrowserEditor), typeof(UITypeEditor)), SRDescription("FaultTypeDescription"), MergableProperty(false), DefaultValue((string) null)]
        public Type FaultType
        {
            get
            {
                return (base.GetValue(FaultTypeProperty) as Type);
            }
            set
            {
                base.SetValue(FaultTypeProperty, value);
            }
        }

        string ITypeFilterProvider.FilterDescription
        {
            get
            {
                return SR.GetString("FilterDescription_FaultHandlerActivity");
            }
        }

        private sealed class FaultConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                if (sourceType == typeof(string))
                {
                    return false;
                }
                return base.CanConvertFrom(context, sourceType);
            }
        }

        private sealed class FaultTypeConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
            }

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                return ((destinationType == typeof(string)) || base.CanConvertTo(context, destinationType));
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                object obj2 = value;
                string str = value as string;
                ITypeProvider service = context.GetService(typeof(ITypeProvider)) as ITypeProvider;
                if (((context != null) && (service != null)) && !string.IsNullOrEmpty(str))
                {
                    Type type = service.GetType(str, false);
                    if (type == null)
                    {
                        return obj2;
                    }
                    ITypeFilterProvider instance = context.Instance as ITypeFilterProvider;
                    if (instance != null)
                    {
                        instance.CanFilterType(type, true);
                    }
                    return type;
                }
                if ((str != null) && (str.Length == 0))
                {
                    obj2 = null;
                }
                return obj2;
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == typeof(string))
                {
                    Type type = value as Type;
                    if (type != null)
                    {
                        return type.FullName;
                    }
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }
    }
}

