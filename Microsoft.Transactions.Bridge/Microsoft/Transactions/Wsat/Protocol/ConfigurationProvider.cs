namespace Microsoft.Transactions.Wsat.Protocol
{
    using System;
    using System.Runtime;

    internal abstract class ConfigurationProvider : IDisposable
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected ConfigurationProvider()
        {
        }

        public abstract void Dispose();
        public abstract ConfigurationProvider OpenKey(string key);
        public abstract int ReadInteger(string value, int defaultValue);
        public abstract string[] ReadMultiString(string value, string[] defaultValue);
        public abstract string ReadString(string value, string defaultValue);
    }
}

