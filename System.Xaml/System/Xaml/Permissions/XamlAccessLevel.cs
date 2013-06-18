namespace System.Xaml.Permissions
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Xaml;

    [Serializable]
    public class XamlAccessLevel
    {
        private XamlAccessLevel(string assemblyName, string typeName)
        {
            this.AssemblyNameString = assemblyName;
            this.PrivateAccessToTypeName = typeName;
        }

        public static XamlAccessLevel AssemblyAccessTo(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }
            return new XamlAccessLevel(assembly.FullName, null);
        }

        public static XamlAccessLevel AssemblyAccessTo(AssemblyName assemblyName)
        {
            if (assemblyName == null)
            {
                throw new ArgumentNullException("assemblyName");
            }
            ValidateAssemblyName(assemblyName, "assemblyName");
            return new XamlAccessLevel(assemblyName.FullName, null);
        }

        internal XamlAccessLevel AssemblyOnly()
        {
            return new XamlAccessLevel(this.AssemblyNameString, null);
        }

        internal static XamlAccessLevel FromXml(SecurityElement elem)
        {
            if (elem.Tag != "XamlAccessLevel")
            {
                throw new ArgumentException(System.Xaml.SR.Get("SecurityXmlUnexpectedTag", new object[] { elem.Tag, "XamlAccessLevel" }), "elem");
            }
            string assemblyName = elem.Attribute("AssemblyName");
            if (assemblyName == null)
            {
                throw new ArgumentException(System.Xaml.SR.Get("SecurityXmlMissingAttribute", new object[] { "AssemblyName" }), "elem");
            }
            AssemblyName name = new AssemblyName(assemblyName);
            ValidateAssemblyName(name, "elem");
            string typeName = elem.Attribute("TypeName");
            if (typeName != null)
            {
                typeName = typeName.Trim();
            }
            return new XamlAccessLevel(name.FullName, typeName);
        }

        internal bool Includes(XamlAccessLevel other)
        {
            if (!(other.AssemblyNameString == this.AssemblyNameString))
            {
                return false;
            }
            if (other.PrivateAccessToTypeName != null)
            {
                return (other.PrivateAccessToTypeName == this.PrivateAccessToTypeName);
            }
            return true;
        }

        public static XamlAccessLevel PrivateAccessTo(string assemblyQualifiedTypeName)
        {
            if (assemblyQualifiedTypeName == null)
            {
                throw new ArgumentNullException("assemblyQualifiedTypeName");
            }
            int index = assemblyQualifiedTypeName.IndexOf(',');
            if (index < 0)
            {
                throw new ArgumentException(System.Xaml.SR.Get("ExpectedQualifiedTypeName", new object[] { assemblyQualifiedTypeName }), "assemblyQualifiedTypeName");
            }
            string typeName = assemblyQualifiedTypeName.Substring(0, index).Trim();
            AssemblyName assemblyName = new AssemblyName(assemblyQualifiedTypeName.Substring(index + 1).Trim());
            ValidateAssemblyName(assemblyName, "assemblyQualifiedTypeName");
            return new XamlAccessLevel(assemblyName.FullName, typeName);
        }

        public static XamlAccessLevel PrivateAccessTo(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            return new XamlAccessLevel(type.Assembly.FullName, type.FullName);
        }

        internal SecurityElement ToXml()
        {
            SecurityElement element = new SecurityElement("XamlAccessLevel");
            element.AddAttribute("AssemblyName", this.AssemblyNameString);
            if (this.PrivateAccessToTypeName != null)
            {
                element.AddAttribute("TypeName", this.PrivateAccessToTypeName);
            }
            return element;
        }

        private static void ValidateAssemblyName(AssemblyName assemblyName, string argName)
        {
            if (((assemblyName.Name == null) || (assemblyName.Version == null)) || ((assemblyName.CultureInfo == null) || (assemblyName.GetPublicKeyToken() == null)))
            {
                throw new ArgumentException(System.Xaml.SR.Get("ExpectedQualifiedAssemblyName", new object[] { assemblyName.FullName }), argName);
            }
        }

        public AssemblyName AssemblyAccessToAssemblyName
        {
            get
            {
                return new AssemblyName(this.AssemblyNameString);
            }
        }

        internal string AssemblyNameString { get; private set; }

        public string PrivateAccessToTypeName { get; private set; }

        private static class XmlConstants
        {
            public const string AssemblyName = "AssemblyName";
            public const string TypeName = "TypeName";
            public const string XamlAccessLevel = "XamlAccessLevel";
        }
    }
}

