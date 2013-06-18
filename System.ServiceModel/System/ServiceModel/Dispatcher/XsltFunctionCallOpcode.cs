namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    internal class XsltFunctionCallOpcode : Opcode
    {
        private int argCount;
        private IXsltContextFunction function;
        private List<NodeSequenceIterator> iterList;
        private static object[] NullArgs = new object[0];
        private XsltContext xsltContext;

        internal XsltFunctionCallOpcode(XsltContext context, IXsltContextFunction function, int argCount) : base(OpcodeID.XsltFunction)
        {
            this.xsltContext = context;
            this.function = function;
            this.argCount = argCount;
            for (int i = 0; i < function.Maxargs; i++)
            {
                if (function.ArgTypes[i] == XPathResultType.NodeSet)
                {
                    this.iterList = new List<NodeSequenceIterator>();
                    break;
                }
            }
            switch (this.function.ReturnType)
            {
                case XPathResultType.Number:
                case XPathResultType.String:
                case XPathResultType.Boolean:
                case XPathResultType.NodeSet:
                    return;
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryCompileException(QueryCompileError.InvalidType, System.ServiceModel.SR.GetString("QueryFunctionTypeNotSupported", new object[] { this.function.ReturnType.ToString() })));
        }

        internal override bool Equals(Opcode op)
        {
            return false;
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            XPathNavigator contextNode = context.Processor.ContextNode;
            if ((contextNode != null) && (context.Processor.ContextMessage != null))
            {
                ((SeekableMessageNavigator) contextNode).Atomize();
            }
            if (this.argCount == 0)
            {
                context.PushFrame();
                int iterationCount = context.IterationCount;
                if (iterationCount > 0)
                {
                    object obj2 = this.function.Invoke(this.xsltContext, NullArgs, contextNode);
                    switch (this.function.ReturnType)
                    {
                        case XPathResultType.Number:
                            context.Push((double) obj2, iterationCount);
                            goto Label_03F6;

                        case XPathResultType.String:
                            context.Push((string) obj2, iterationCount);
                            goto Label_03F6;

                        case XPathResultType.Boolean:
                            context.Push((bool) obj2, iterationCount);
                            goto Label_03F6;

                        case XPathResultType.NodeSet:
                        {
                            NodeSequence sequence = context.CreateSequence();
                            XPathNodeIterator iter = (XPathNodeIterator) obj2;
                            sequence.Add(iter);
                            context.Push(sequence, iterationCount);
                            goto Label_03F6;
                        }
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.Unexpected, System.ServiceModel.SR.GetString("QueryFunctionTypeNotSupported", new object[] { this.function.ReturnType.ToString() })));
                }
            }
            else
            {
                object[] args = new object[this.argCount];
                int count = context.TopArg.Count;
                for (int i = 0; i < count; i++)
                {
                    for (int k = 0; k < this.argCount; k++)
                    {
                        StackFrame frame = context[k];
                        switch (this.function.ArgTypes[k])
                        {
                            case XPathResultType.Number:
                                args[k] = context.PeekDouble(frame[i]);
                                break;

                            case XPathResultType.String:
                                args[k] = context.PeekString(frame[i]);
                                break;

                            case XPathResultType.Boolean:
                                args[k] = context.PeekBoolean(frame[i]);
                                break;

                            case XPathResultType.NodeSet:
                            {
                                NodeSequenceIterator item = new NodeSequenceIterator(context.PeekSequence(frame[i]));
                                args[k] = item;
                                this.iterList.Add(item);
                                break;
                            }
                            default:
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.Unexpected, System.ServiceModel.SR.GetString("QueryFunctionTypeNotSupported", new object[] { this.function.ArgTypes[k].ToString() })));
                        }
                    }
                    object obj3 = this.function.Invoke(this.xsltContext, args, contextNode);
                    if (this.iterList != null)
                    {
                        for (int m = 0; m < this.iterList.Count; m++)
                        {
                            this.iterList[m].Clear();
                        }
                        this.iterList.Clear();
                    }
                    switch (this.function.ReturnType)
                    {
                        case XPathResultType.Number:
                        {
                            StackFrame frame4 = context[this.argCount - 1];
                            context.SetValue(context, frame4[i], (double) obj3);
                            break;
                        }
                        case XPathResultType.String:
                        {
                            StackFrame frame3 = context[this.argCount - 1];
                            context.SetValue(context, frame3[i], (string) obj3);
                            break;
                        }
                        case XPathResultType.Boolean:
                        {
                            StackFrame frame5 = context[this.argCount - 1];
                            context.SetValue(context, frame5[i], (bool) obj3);
                            break;
                        }
                        case XPathResultType.NodeSet:
                        {
                            NodeSequence val = context.CreateSequence();
                            XPathNodeIterator iterator3 = (XPathNodeIterator) obj3;
                            val.Add(iterator3);
                            StackFrame frame6 = context[this.argCount - 1];
                            context.SetValue(context, frame6[i], val);
                            break;
                        }
                        default:
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.Unexpected, System.ServiceModel.SR.GetString("QueryFunctionTypeNotSupported", new object[] { this.function.ReturnType.ToString() })));
                    }
                }
                for (int j = 0; j < (this.argCount - 1); j++)
                {
                    context.PopFrame();
                }
            }
        Label_03F6:
            return base.next;
        }
    }
}

