namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ClassInterface(ClassInterfaceType.AutoDispatch), ComVisible(true)]
    public class CodeAttachEventStatement : CodeStatement
    {
        private CodeEventReferenceExpression eventRef;
        private CodeExpression listener;

        public CodeAttachEventStatement()
        {
        }

        public CodeAttachEventStatement(CodeEventReferenceExpression eventRef, CodeExpression listener)
        {
            this.eventRef = eventRef;
            this.listener = listener;
        }

        public CodeAttachEventStatement(CodeExpression targetObject, string eventName, CodeExpression listener)
        {
            this.eventRef = new CodeEventReferenceExpression(targetObject, eventName);
            this.listener = listener;
        }

        public CodeEventReferenceExpression Event
        {
            get
            {
                if (this.eventRef == null)
                {
                    return new CodeEventReferenceExpression();
                }
                return this.eventRef;
            }
            set
            {
                this.eventRef = value;
            }
        }

        public CodeExpression Listener
        {
            get
            {
                return this.listener;
            }
            set
            {
                this.listener = value;
            }
        }
    }
}

