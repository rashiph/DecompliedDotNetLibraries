namespace System.Activities.ExpressionParser
{
    using System;
    using System.Activities;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.Serialization;

    [Serializable]
    public class SourceExpressionException : Exception, ISerializable
    {
        private CompilerError[] errors;

        public SourceExpressionException() : base(System.Activities.SR.CompilerError)
        {
        }

        public SourceExpressionException(string message) : base(message)
        {
        }

        protected SourceExpressionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            if (info == null)
            {
                throw FxTrace.Exception.ArgumentNull("info");
            }
            int num = info.GetInt32("count");
            this.errors = new CompilerError[num];
            for (int i = 0; i < num; i++)
            {
                string str = i.ToString(CultureInfo.InvariantCulture);
                string fileName = info.GetString("file" + str);
                int line = info.GetInt32("line" + str);
                int column = info.GetInt32("column" + str);
                string errorNumber = info.GetString("number" + str);
                string errorText = info.GetString("text" + str);
                this.errors[i] = new CompilerError(fileName, line, column, errorNumber, errorText);
            }
        }

        public SourceExpressionException(string message, CompilerErrorCollection errors) : base(message)
        {
            this.errors = new CompilerError[errors.Count];
            errors.CopyTo(this.errors, 0);
        }

        public SourceExpressionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw FxTrace.Exception.ArgumentNull("info");
            }
            if (this.errors == null)
            {
                info.AddValue("count", 0);
            }
            else
            {
                info.AddValue("count", this.errors.Length);
                for (int i = 0; i < this.errors.Length; i++)
                {
                    CompilerError error = this.errors[i];
                    string str = i.ToString(CultureInfo.InvariantCulture);
                    info.AddValue("file" + str, error.FileName);
                    info.AddValue("line" + str, error.Line);
                    info.AddValue("column" + str, error.Column);
                    info.AddValue("number" + str, error.ErrorNumber);
                    info.AddValue("text" + str, error.ErrorText);
                }
            }
            base.GetObjectData(info, context);
        }

        public IEnumerable<CompilerError> Errors
        {
            get
            {
                if (this.errors == null)
                {
                    this.errors = new CompilerError[0];
                }
                return this.errors;
            }
        }
    }
}

