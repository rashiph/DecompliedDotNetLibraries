namespace System.Management.Instrumentation
{
    using System;
    using System.Collections;
    using System.Management;
    using System.Reflection;
    using System.Runtime;
    using System.Text.RegularExpressions;

    [AttributeUsage(AttributeTargets.Assembly)]
    public class InstrumentedAttribute : Attribute
    {
        private string namespaceName;
        private string securityDescriptor;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstrumentedAttribute() : this(null, null)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstrumentedAttribute(string namespaceName) : this(namespaceName, null)
        {
        }

        public InstrumentedAttribute(string namespaceName, string securityDescriptor)
        {
            if (namespaceName != null)
            {
                namespaceName = namespaceName.Replace('/', '\\');
            }
            if ((namespaceName == null) || (namespaceName.Length == 0))
            {
                namespaceName = @"root\default";
            }
            bool flag = true;
            foreach (string str in namespaceName.Split(new char[] { '\\' }))
            {
                if (((str.Length == 0) || (flag && (string.Compare(str, "root", StringComparison.OrdinalIgnoreCase) != 0))) || ((!Regex.Match(str, "^[a-z,A-Z]").Success || Regex.Match(str, "_$").Success) || Regex.Match(str, @"[^a-z,A-Z,0-9,_,\u0080-\uFFFF]").Success))
                {
                    ManagementException.ThrowWithExtendedInfo(ManagementStatus.InvalidNamespace);
                }
                flag = false;
            }
            this.namespaceName = namespaceName;
            this.securityDescriptor = securityDescriptor;
        }

        internal static InstrumentedAttribute GetAttribute(Assembly assembly)
        {
            object[] customAttributes = assembly.GetCustomAttributes(typeof(InstrumentedAttribute), false);
            if (customAttributes.Length > 0)
            {
                return (InstrumentedAttribute) customAttributes[0];
            }
            return new InstrumentedAttribute();
        }

        private static void GetInstrumentedParentTypes(ArrayList types, Type childType)
        {
            if (!types.Contains(childType))
            {
                Type baseInstrumentationType = InstrumentationClassAttribute.GetBaseInstrumentationType(childType);
                if (baseInstrumentationType != null)
                {
                    GetInstrumentedParentTypes(types, baseInstrumentationType);
                }
                types.Add(childType);
            }
        }

        internal static Type[] GetInstrumentedTypes(Assembly assembly)
        {
            ArrayList types = new ArrayList();
            foreach (Type type in assembly.GetTypes())
            {
                if (IsInstrumentationClass(type))
                {
                    GetInstrumentedParentTypes(types, type);
                }
            }
            return (Type[]) types.ToArray(typeof(Type));
        }

        private static bool IsInstrumentationClass(Type type)
        {
            return (null != InstrumentationClassAttribute.GetAttribute(type));
        }

        public string NamespaceName
        {
            get
            {
                if (this.namespaceName != null)
                {
                    return this.namespaceName;
                }
                return string.Empty;
            }
        }

        public string SecurityDescriptor
        {
            get
            {
                if ((this.securityDescriptor != null) && (this.securityDescriptor.Length != 0))
                {
                    return this.securityDescriptor;
                }
                return null;
            }
        }
    }
}

