namespace <CrtImplementationDetails>
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    internal class ModuleLoadException : System.Exception
    {
        public const string Nested = "A nested exception occurred after the primary exception that caused the C++ module to fail to load.\n";

        public ModuleLoadException(string message) : base(message)
        {
        }

        protected ModuleLoadException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ModuleLoadException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }
}

