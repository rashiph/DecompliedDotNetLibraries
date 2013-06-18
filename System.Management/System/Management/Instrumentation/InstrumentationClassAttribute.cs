namespace System.Management.Instrumentation
{
    using System;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
    public class InstrumentationClassAttribute : Attribute
    {
        private System.Management.Instrumentation.InstrumentationType instrumentationType;
        private string managedBaseClassName;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstrumentationClassAttribute(System.Management.Instrumentation.InstrumentationType instrumentationType)
        {
            this.instrumentationType = instrumentationType;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstrumentationClassAttribute(System.Management.Instrumentation.InstrumentationType instrumentationType, string managedBaseClassName)
        {
            this.instrumentationType = instrumentationType;
            this.managedBaseClassName = managedBaseClassName;
        }

        internal static InstrumentationClassAttribute GetAttribute(Type type)
        {
            if ((type != typeof(BaseEvent)) && (type != typeof(Instance)))
            {
                object[] customAttributes = type.GetCustomAttributes(typeof(InstrumentationClassAttribute), true);
                if (customAttributes.Length > 0)
                {
                    return (InstrumentationClassAttribute) customAttributes[0];
                }
            }
            return null;
        }

        internal static Type GetBaseInstrumentationType(Type type)
        {
            if (GetAttribute(type.BaseType) != null)
            {
                return type.BaseType;
            }
            return null;
        }

        public System.Management.Instrumentation.InstrumentationType InstrumentationType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.instrumentationType;
            }
        }

        public string ManagedBaseClassName
        {
            get
            {
                if ((this.managedBaseClassName != null) && (this.managedBaseClassName.Length != 0))
                {
                    return this.managedBaseClassName;
                }
                return null;
            }
        }
    }
}

