namespace Microsoft.JScript
{
    using System;
    using System.Runtime.InteropServices;

    public class ErrorObject : JSObject
    {
        public object description;
        internal object exception;
        public object message;
        public object number;

        internal ErrorObject(ErrorPrototype parent, object[] args) : base(parent)
        {
            this.exception = null;
            this.description = "";
            this.number = 0;
            if (args.Length == 1)
            {
                if ((args[0] == null) || Microsoft.JScript.Convert.IsPrimitiveNumericType(args[0].GetType()))
                {
                    this.number = Microsoft.JScript.Convert.ToNumber(args[0]);
                }
                else
                {
                    this.description = Microsoft.JScript.Convert.ToString(args[0]);
                }
            }
            else if (args.Length > 1)
            {
                this.number = Microsoft.JScript.Convert.ToNumber(args[0]);
                this.description = Microsoft.JScript.Convert.ToString(args[1]);
            }
            this.message = this.description;
            base.noExpando = false;
        }

        internal ErrorObject(ErrorPrototype parent, object e) : base(parent)
        {
            this.exception = e;
            this.number = -2146823266;
            if (e is Exception)
            {
                if (e is JScriptException)
                {
                    this.number = ((JScriptException) e).Number;
                }
                else if (e is ExternalException)
                {
                    this.number = ((ExternalException) e).ErrorCode;
                }
                this.description = ((Exception) e).Message;
                if (((string) this.description).Length == 0)
                {
                    this.description = e.GetType().FullName;
                }
            }
            this.message = this.description;
            base.noExpando = false;
        }

        internal override string GetClassName()
        {
            return "Error";
        }

        public static explicit operator Exception(ErrorObject err)
        {
            return (err.exception as Exception);
        }

        public static Exception ToException(ErrorObject err)
        {
            return (Exception) err;
        }

        internal string Message
        {
            get
            {
                return Microsoft.JScript.Convert.ToString(this.message);
            }
        }
    }
}

