namespace Microsoft.Build.Tasks
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    internal sealed class InvalidReferenceAssemblyNameException : Exception
    {
        private string sourceItemSpec;

        private InvalidReferenceAssemblyNameException()
        {
        }

        internal InvalidReferenceAssemblyNameException(string sourceItemSpec)
        {
            this.sourceItemSpec = sourceItemSpec;
        }

        private InvalidReferenceAssemblyNameException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        internal string SourceItemSpec
        {
            get
            {
                return this.sourceItemSpec;
            }
        }
    }
}

