namespace System.Diagnostics
{
    using System;
    using System.Collections;
    using System.Reflection;

    [AttributeUsage(AttributeTargets.Event | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class | AttributeTargets.Assembly)]
    public sealed class SwitchAttribute : Attribute
    {
        private string description;
        private string name;
        private Type type;

        public SwitchAttribute(string switchName, Type switchType)
        {
            this.SwitchName = switchName;
            this.SwitchType = switchType;
        }

        public static SwitchAttribute[] GetAll(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }
            ArrayList switchAttribs = new ArrayList();
            object[] customAttributes = assembly.GetCustomAttributes(typeof(SwitchAttribute), false);
            switchAttribs.AddRange(customAttributes);
            Type[] types = assembly.GetTypes();
            for (int i = 0; i < types.Length; i++)
            {
                GetAllRecursive(types[i], switchAttribs);
            }
            SwitchAttribute[] array = new SwitchAttribute[switchAttribs.Count];
            switchAttribs.CopyTo(array, 0);
            return array;
        }

        private static void GetAllRecursive(MemberInfo member, ArrayList switchAttribs)
        {
            object[] customAttributes = member.GetCustomAttributes(typeof(SwitchAttribute), false);
            switchAttribs.AddRange(customAttributes);
        }

        private static void GetAllRecursive(Type type, ArrayList switchAttribs)
        {
            GetAllRecursive((MemberInfo) type, switchAttribs);
            MemberInfo[] members = type.GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            for (int i = 0; i < members.Length; i++)
            {
                if (!(members[i] is Type))
                {
                    GetAllRecursive(members[i], switchAttribs);
                }
            }
        }

        public string SwitchDescription
        {
            get
            {
                return this.description;
            }
            set
            {
                this.description = value;
            }
        }

        public string SwitchName
        {
            get
            {
                return this.name;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (value.Length == 0)
                {
                    throw new ArgumentException(SR.GetString("InvalidNullEmptyArgument", new object[] { "value" }), "value");
                }
                this.name = value;
            }
        }

        public Type SwitchType
        {
            get
            {
                return this.type;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.type = value;
            }
        }
    }
}

