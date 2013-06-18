namespace Microsoft.Build.Tasks
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    internal sealed class InvalidParameterValueException : Exception
    {
        private string actualValue;
        private string paramName;

        private InvalidParameterValueException()
        {
        }

        private InvalidParameterValueException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        internal InvalidParameterValueException(string paramName, string actualValue, string message) : this(paramName, actualValue, message, null)
        {
        }

        internal InvalidParameterValueException(string paramName, string actualValue, string message, Exception innerException) : base(message, innerException)
        {
            this.ParamName = paramName;
            this.ActualValue = actualValue;
        }

        public string ActualValue
        {
            get
            {
                return this.actualValue;
            }
            set
            {
                this.actualValue = value;
            }
        }

        public string ParamName
        {
            get
            {
                return this.paramName;
            }
            set
            {
                this.paramName = value;
            }
        }
    }
}

