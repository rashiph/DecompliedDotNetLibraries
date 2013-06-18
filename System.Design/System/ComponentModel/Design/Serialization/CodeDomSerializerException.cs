namespace System.ComponentModel.Design.Serialization
{
    using System;
    using System.CodeDom;
    using System.Runtime.Serialization;

    [Serializable]
    public class CodeDomSerializerException : SystemException
    {
        private CodeLinePragma linePragma;

        public CodeDomSerializerException(Exception ex, CodeLinePragma linePragma) : base(ex.Message, ex)
        {
            this.linePragma = linePragma;
        }

        public CodeDomSerializerException(Exception ex, IDesignerSerializationManager manager) : base(ex.Message, ex)
        {
            this.FillLinePragmaFromContext(manager);
        }

        protected CodeDomSerializerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.linePragma = (CodeLinePragma) info.GetValue("linePragma", typeof(CodeLinePragma));
        }

        public CodeDomSerializerException(string message, CodeLinePragma linePragma) : base(message)
        {
            this.linePragma = linePragma;
        }

        public CodeDomSerializerException(string message, IDesignerSerializationManager manager) : base(message)
        {
            this.FillLinePragmaFromContext(manager);
        }

        private void FillLinePragmaFromContext(IDesignerSerializationManager manager)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            info.AddValue("linePragma", this.linePragma);
            base.GetObjectData(info, context);
        }

        public CodeLinePragma LinePragma
        {
            get
            {
                return this.linePragma;
            }
        }
    }
}

