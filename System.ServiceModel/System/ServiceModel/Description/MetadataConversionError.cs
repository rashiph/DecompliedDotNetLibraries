namespace System.ServiceModel.Description
{
    using System;
    using System.ServiceModel;

    public class MetadataConversionError
    {
        private bool isWarning;
        private string message;

        public MetadataConversionError(string message) : this(message, false)
        {
        }

        public MetadataConversionError(string message, bool isWarning)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            this.message = message;
            this.isWarning = isWarning;
        }

        public override bool Equals(object obj)
        {
            MetadataConversionError error = obj as MetadataConversionError;
            if (error == null)
            {
                return false;
            }
            return ((error.IsWarning == this.IsWarning) && (error.Message == this.Message));
        }

        public override int GetHashCode()
        {
            return this.message.GetHashCode();
        }

        public bool IsWarning
        {
            get
            {
                return this.isWarning;
            }
        }

        public string Message
        {
            get
            {
                return this.message;
            }
        }
    }
}

