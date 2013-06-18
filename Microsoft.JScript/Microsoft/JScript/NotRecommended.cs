namespace Microsoft.JScript
{
    using System;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method)]
    public class NotRecommended : Attribute
    {
        private string message;

        public NotRecommended(string message)
        {
            this.message = message;
        }

        public bool IsError
        {
            get
            {
                return false;
            }
        }

        public string Message
        {
            get
            {
                return JScriptException.Localize(this.message, null);
            }
        }
    }
}

