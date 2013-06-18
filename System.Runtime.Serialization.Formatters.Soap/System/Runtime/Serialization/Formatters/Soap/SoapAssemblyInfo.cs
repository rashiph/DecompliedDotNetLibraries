namespace System.Runtime.Serialization.Formatters.Soap
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.Serialization;

    internal sealed class SoapAssemblyInfo
    {
        private Assembly assembly;
        internal string assemblyString;

        internal SoapAssemblyInfo(string assemblyString)
        {
            this.assemblyString = assemblyString;
        }

        internal SoapAssemblyInfo(string assemblyString, Assembly assembly)
        {
            this.assemblyString = assemblyString;
            this.assembly = assembly;
        }

        internal Assembly GetAssembly(ObjectReader objectReader)
        {
            if (this.assembly == null)
            {
                this.assembly = objectReader.LoadAssemblyFromString(this.assemblyString);
                if (this.assembly == null)
                {
                    throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_AssemblyString"), new object[] { this.assemblyString }));
                }
            }
            return this.assembly;
        }
    }
}

