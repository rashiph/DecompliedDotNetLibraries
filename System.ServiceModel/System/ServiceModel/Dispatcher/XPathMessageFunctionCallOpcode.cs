namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Xml.XPath;

    internal class XPathMessageFunctionCallOpcode : Opcode
    {
        private int argCount;
        private XPathMessageFunction function;

        internal XPathMessageFunctionCallOpcode(XPathMessageFunction fun, int argCount) : base(OpcodeID.XsltInternalFunction)
        {
            this.function = fun;
            this.argCount = argCount;
        }

        internal override bool Equals(Opcode op)
        {
            if (base.Equals(op))
            {
                XPathMessageFunctionCallOpcode opcode = op as XPathMessageFunctionCallOpcode;
                if (opcode != null)
                {
                    return (this.function == opcode.function);
                }
            }
            return false;
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            this.function.InvokeInternal(context, this.argCount);
            return base.next;
        }

        internal int ArgCount
        {
            get
            {
                return this.argCount;
            }
        }

        internal XPathResultType ReturnType
        {
            get
            {
                return this.function.ReturnType;
            }
        }
    }
}

