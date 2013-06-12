namespace System.Reflection.Emit
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;

    internal sealed class InternalAssemblyBuilder : RuntimeAssembly
    {
        private InternalAssemblyBuilder()
        {
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj is InternalAssemblyBuilder)
            {
                return (this == obj);
            }
            return obj.Equals(this);
        }

        public override Type[] GetExportedTypes()
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicAssembly"));
        }

        public override FileStream GetFile(string name)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicAssembly"));
        }

        public override FileStream[] GetFiles(bool getResourceModules)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicAssembly"));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override ManifestResourceInfo GetManifestResourceInfo(string resourceName)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicAssembly"));
        }

        public override string[] GetManifestResourceNames()
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicAssembly"));
        }

        public override Stream GetManifestResourceStream(string name)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicAssembly"));
        }

        public override Stream GetManifestResourceStream(Type type, string name)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicAssembly"));
        }

        public override string CodeBase
        {
            get
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicAssembly"));
            }
        }

        public override string ImageRuntimeVersion
        {
            [SecuritySafeCritical]
            get
            {
                return RuntimeEnvironment.GetSystemVersion();
            }
        }

        public override string Location
        {
            get
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_DynamicAssembly"));
            }
        }
    }
}

