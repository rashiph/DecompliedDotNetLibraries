namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class CodeRemoveEventStatement : CodeStatement
    {
        private CodeEventReferenceExpression eventRef;
        private CodeExpression listener;

        public CodeRemoveEventStatement()
        {
        }

        public CodeRemoveEventStatement(CodeEventReferenceExpression eventRef, CodeExpression listener)
        {
            this.eventRef = eventRef;
            this.listener = listener;
        }

        public CodeRemoveEventStatement(CodeExpression targetObject, string eventName, CodeExpression listener)
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
                    this.eventRef = new CodeEventReferenceExpression();
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

