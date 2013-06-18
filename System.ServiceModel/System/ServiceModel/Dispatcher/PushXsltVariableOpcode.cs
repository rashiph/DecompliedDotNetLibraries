namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    internal class PushXsltVariableOpcode : Opcode
    {
        private ValueDataType type;
        private IXsltContextVariable variable;
        private XsltContext xsltContext;

        internal PushXsltVariableOpcode(XsltContext context, IXsltContextVariable variable) : base(OpcodeID.PushXsltVariable)
        {
            this.xsltContext = context;
            this.variable = variable;
            this.type = XPathXsltFunctionExpr.ConvertTypeFromXslt(variable.VariableType);
            switch (this.type)
            {
                case ValueDataType.Boolean:
                case ValueDataType.Double:
                case ValueDataType.Sequence:
                case ValueDataType.String:
                    return;
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryCompileException(QueryCompileError.InvalidType, System.ServiceModel.SR.GetString("QueryVariableTypeNotSupported", new object[] { this.variable.VariableType.ToString() })));
        }

        internal override bool Equals(Opcode op)
        {
            if (base.Equals(op))
            {
                PushXsltVariableOpcode opcode = op as PushXsltVariableOpcode;
                if (opcode != null)
                {
                    return ((this.xsltContext == opcode.xsltContext) && (this.variable == opcode.variable));
                }
            }
            return false;
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            context.PushFrame();
            int iterationCount = context.IterationCount;
            if (iterationCount > 0)
            {
                object obj2 = this.variable.Evaluate(this.xsltContext);
                if (obj2 == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryProcessingException(QueryProcessingError.Unexpected, System.ServiceModel.SR.GetString("QueryVariableNull")));
                }
                switch (this.type)
                {
                    case ValueDataType.Boolean:
                        context.Push((bool) obj2, iterationCount);
                        goto Label_013A;

                    case ValueDataType.Double:
                        context.Push((double) obj2, iterationCount);
                        goto Label_013A;

                    case ValueDataType.Sequence:
                    {
                        XPathNodeIterator iterator = (XPathNodeIterator) obj2;
                        NodeSequence sequence = context.CreateSequence();
                        while (iterator.MoveNext())
                        {
                            SeekableXPathNavigator current = iterator.Current as SeekableXPathNavigator;
                            if (current == null)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryProcessingException(QueryProcessingError.Unexpected, System.ServiceModel.SR.GetString("QueryMustBeSeekable")));
                            }
                            sequence.Add(current);
                        }
                        context.Push(sequence, iterationCount);
                        goto Label_013A;
                    }
                    case ValueDataType.String:
                        context.Push((string) obj2, iterationCount);
                        goto Label_013A;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.Unexpected, System.ServiceModel.SR.GetString("QueryVariableTypeNotSupported", new object[] { this.variable.VariableType.ToString() })));
            }
        Label_013A:
            return base.next;
        }
    }
}

