namespace System.Runtime.Serialization.Formatters.Binary
{
    using System;
    using System.Reflection;
    using System.Runtime.Serialization;

    internal sealed class BinaryAssemblyInfo
    {
        private Assembly assembly;
        internal string assemblyString;

        internal BinaryAssemblyInfo(string assemblyString)
        {
            this.assemblyString = assemblyString;
        }

        internal BinaryAssemblyInfo(string assemblyString, Assembly assembly)
        {
            this.assemblyString = assemblyString;
            this.assembly = assembly;
        }

        internal Assembly GetAssembly()
        {
            if (this.assembly == null)
            {
                this.assembly = FormatterServices.LoadAssemblyFromStringNoThrow(this.assemblyString);
                if (this.assembly == null)
                {
                    throw new SerializationException(Environment.GetResourceString("Serialization_AssemblyNotFound", new object[] { this.assemblyString }));
                }
            }
            return this.assembly;
        }
    }
}

