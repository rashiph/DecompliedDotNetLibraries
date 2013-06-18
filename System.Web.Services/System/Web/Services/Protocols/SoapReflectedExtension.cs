namespace System.Web.Services.Protocols
{
    using System;
    using System.Web.Services;

    internal class SoapReflectedExtension : IComparable
    {
        private SoapExtensionAttribute attribute;
        private int priority;
        private Type type;

        internal SoapReflectedExtension(Type type, SoapExtensionAttribute attribute) : this(type, attribute, attribute.Priority)
        {
        }

        internal SoapReflectedExtension(Type type, SoapExtensionAttribute attribute, int priority)
        {
            if (priority < 0)
            {
                throw new ArgumentException(Res.GetString("WebConfigInvalidExtensionPriority", new object[] { priority }), "priority");
            }
            this.type = type;
            this.attribute = attribute;
            this.priority = priority;
        }

        public int CompareTo(object o)
        {
            return (this.priority - ((SoapReflectedExtension) o).priority);
        }

        internal SoapExtension CreateInstance(object initializer)
        {
            SoapExtension extension = (SoapExtension) Activator.CreateInstance(this.type);
            extension.Initialize(initializer);
            return extension;
        }

        internal object GetInitializer(Type serviceType)
        {
            SoapExtension extension = (SoapExtension) Activator.CreateInstance(this.type);
            return extension.GetInitializer(serviceType);
        }

        internal object GetInitializer(LogicalMethodInfo methodInfo)
        {
            SoapExtension extension = (SoapExtension) Activator.CreateInstance(this.type);
            return extension.GetInitializer(methodInfo, this.attribute);
        }

        internal static object[] GetInitializers(Type serviceType, SoapReflectedExtension[] extensions)
        {
            object[] objArray = new object[extensions.Length];
            for (int i = 0; i < objArray.Length; i++)
            {
                objArray[i] = extensions[i].GetInitializer(serviceType);
            }
            return objArray;
        }

        internal static object[] GetInitializers(LogicalMethodInfo methodInfo, SoapReflectedExtension[] extensions)
        {
            object[] objArray = new object[extensions.Length];
            for (int i = 0; i < objArray.Length; i++)
            {
                objArray[i] = extensions[i].GetInitializer(methodInfo);
            }
            return objArray;
        }
    }
}

