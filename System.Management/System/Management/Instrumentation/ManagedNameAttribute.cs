namespace System.Management.Instrumentation
{
    using System;
    using System.Reflection;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Struct | AttributeTargets.Class)]
    public class ManagedNameAttribute : Attribute
    {
        private string name;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ManagedNameAttribute(string name)
        {
            this.name = name;
        }

        internal static string GetBaseClassName(Type type)
        {
            InstrumentationClassAttribute attribute = InstrumentationClassAttribute.GetAttribute(type);
            string managedBaseClassName = attribute.ManagedBaseClassName;
            if (managedBaseClassName != null)
            {
                return managedBaseClassName;
            }
            if (InstrumentationClassAttribute.GetAttribute(type.BaseType) == null)
            {
                switch (attribute.InstrumentationType)
                {
                    case InstrumentationType.Instance:
                        return null;

                    case InstrumentationType.Event:
                        return "__ExtrinsicEvent";

                    case InstrumentationType.Abstract:
                        return null;
                }
            }
            return GetMemberName(type.BaseType);
        }

        internal static string GetMemberName(MemberInfo member)
        {
            object[] customAttributes = member.GetCustomAttributes(typeof(ManagedNameAttribute), false);
            if (customAttributes.Length > 0)
            {
                ManagedNameAttribute attribute = (ManagedNameAttribute) customAttributes[0];
                if ((attribute.name != null) && (attribute.name.Length != 0))
                {
                    return attribute.name;
                }
            }
            return member.Name;
        }

        public string Name
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.name;
            }
        }
    }
}

