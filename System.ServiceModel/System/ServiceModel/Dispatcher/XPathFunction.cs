namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel;
    using System.Text;

    internal class XPathFunction : QueryFunction
    {
        private XPathFunctionID functionID;

        internal XPathFunction(XPathFunctionID functionID, string name, ValueDataType returnType) : base(name, returnType)
        {
            this.functionID = functionID;
        }

        internal XPathFunction(XPathFunctionID functionID, string name, ValueDataType returnType, QueryFunctionFlag flags) : base(name, returnType, flags)
        {
            this.functionID = functionID;
        }

        internal XPathFunction(XPathFunctionID functionID, string name, ValueDataType returnType, ValueDataType[] argTypes) : base(name, returnType, argTypes)
        {
            this.functionID = functionID;
        }

        internal static void BooleanBoolean(ProcessingContext context)
        {
            StackFrame topArg = context.TopArg;
            Value[] values = context.Values;
            while (topArg.basePtr <= topArg.endPtr)
            {
                values[topArg.basePtr++].ConvertTo(context, ValueDataType.Boolean);
            }
        }

        internal static void BooleanFalse(ProcessingContext context)
        {
            context.PushFrame();
            int iterationCount = context.IterationCount;
            if (iterationCount > 0)
            {
                context.Push(false, iterationCount);
            }
        }

        internal static void BooleanLang(ProcessingContext context)
        {
            StackFrame topArg = context.TopArg;
            StackFrame topSequenceArg = context.TopSequenceArg;
            Value[] sequences = context.Sequences;
            while (topSequenceArg.basePtr <= topSequenceArg.endPtr)
            {
                NodeSequence sequence = sequences[topSequenceArg.basePtr++].Sequence;
                for (int i = 0; i < sequence.Count; i++)
                {
                    string strA = context.PeekString(topArg.basePtr).ToUpperInvariant();
                    QueryNode node = sequence.Items[i].Node;
                    long currentPosition = node.Node.CurrentPosition;
                    node.Node.CurrentPosition = node.Position;
                    string strB = node.Node.XmlLang.ToUpperInvariant();
                    node.Node.CurrentPosition = currentPosition;
                    if ((strA.Length == strB.Length) && (string.CompareOrdinal(strA, strB) == 0))
                    {
                        context.SetValue(context, topArg.basePtr++, true);
                    }
                    else if (((strB.Length > 0) && (strA.Length < strB.Length)) && (strB.StartsWith(strA, StringComparison.Ordinal) && (strB[strA.Length] == '-')))
                    {
                        context.SetValue(context, topArg.basePtr++, true);
                    }
                    else
                    {
                        context.SetValue(context, topArg.basePtr++, false);
                    }
                }
                topSequenceArg.basePtr++;
            }
        }

        internal static void BooleanNot(ProcessingContext context)
        {
            StackFrame topArg = context.TopArg;
            Value[] values = context.Values;
            while (topArg.basePtr <= topArg.endPtr)
            {
                values[topArg.basePtr++].Not();
            }
        }

        internal static void BooleanTrue(ProcessingContext context)
        {
            context.PushFrame();
            int iterationCount = context.IterationCount;
            if (iterationCount > 0)
            {
                context.Push(true, iterationCount);
            }
        }

        private static void ConvertFirstArg(ProcessingContext context, ValueDataType type)
        {
            StackFrame topArg = context.TopArg;
            Value[] values = context.Values;
            while (topArg.basePtr <= topArg.endPtr)
            {
                values[topArg.basePtr++].ConvertTo(context, type);
            }
        }

        internal override bool Equals(QueryFunction function)
        {
            XPathFunction function2 = function as XPathFunction;
            if (function2 == null)
            {
                return false;
            }
            return (function2.ID == this.ID);
        }

        internal override void Eval(ProcessingContext context)
        {
            switch (this.functionID)
            {
                case XPathFunctionID.IterateSequences:
                    IterateAndPushSequences(context);
                    return;

                case XPathFunctionID.Count:
                    NodesetCount(context);
                    return;

                case XPathFunctionID.Position:
                    NodesetPosition(context);
                    return;

                case XPathFunctionID.Last:
                    NodesetLast(context);
                    return;

                case XPathFunctionID.LocalName:
                    NodesetLocalName(context);
                    return;

                case XPathFunctionID.LocalNameDefault:
                    NodesetLocalNameDefault(context);
                    return;

                case XPathFunctionID.Name:
                    NodesetName(context);
                    return;

                case XPathFunctionID.NameDefault:
                    NodesetNameDefault(context);
                    return;

                case XPathFunctionID.NamespaceUri:
                    NodesetNamespaceUri(context);
                    return;

                case XPathFunctionID.NamespaceUriDefault:
                    NodesetNamespaceUriDefault(context);
                    return;

                case XPathFunctionID.Boolean:
                    BooleanBoolean(context);
                    return;

                case XPathFunctionID.Not:
                    BooleanNot(context);
                    return;

                case XPathFunctionID.True:
                    BooleanTrue(context);
                    return;

                case XPathFunctionID.False:
                    BooleanFalse(context);
                    return;

                case XPathFunctionID.Lang:
                    BooleanLang(context);
                    return;

                case XPathFunctionID.Number:
                    NumberNumber(context);
                    return;

                case XPathFunctionID.NumberDefault:
                    NumberNumberDefault(context);
                    return;

                case XPathFunctionID.Ceiling:
                    NumberCeiling(context);
                    return;

                case XPathFunctionID.Floor:
                    NumberFloor(context);
                    return;

                case XPathFunctionID.Round:
                    NumberRound(context);
                    return;

                case XPathFunctionID.Sum:
                    NumberSum(context);
                    return;

                case XPathFunctionID.String:
                    StringString(context);
                    return;

                case XPathFunctionID.StringDefault:
                    StringStringDefault(context);
                    return;

                case XPathFunctionID.StartsWith:
                    StringStartsWith(context);
                    return;

                case XPathFunctionID.ConcatTwo:
                    StringConcatTwo(context);
                    return;

                case XPathFunctionID.ConcatThree:
                    StringConcatThree(context);
                    return;

                case XPathFunctionID.ConcatFour:
                    StringConcatFour(context);
                    return;

                case XPathFunctionID.Contains:
                    StringContains(context);
                    return;

                case XPathFunctionID.NormalizeSpace:
                    NormalizeSpace(context);
                    return;

                case XPathFunctionID.NormalizeSpaceDefault:
                    NormalizeSpaceDefault(context);
                    return;

                case XPathFunctionID.StringLength:
                    StringLength(context);
                    return;

                case XPathFunctionID.StringLengthDefault:
                    StringLengthDefault(context);
                    return;

                case XPathFunctionID.SubstringBefore:
                    SubstringBefore(context);
                    return;

                case XPathFunctionID.SubstringAfter:
                    SubstringAfter(context);
                    return;

                case XPathFunctionID.Substring:
                    Substring(context);
                    return;

                case XPathFunctionID.SubstringLimit:
                    SubstringLimit(context);
                    return;

                case XPathFunctionID.Translate:
                    Translate(context);
                    return;
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException(System.ServiceModel.SR.GetString("QueryNotImplemented", new object[] { base.name })));
        }

        internal static void IterateAndPushSequences(ProcessingContext context)
        {
            StackFrame topSequenceArg = context.TopSequenceArg;
            Value[] sequences = context.Sequences;
            context.PushFrame();
            while (topSequenceArg.basePtr <= topSequenceArg.endPtr)
            {
                NodeSequence sequence = sequences[topSequenceArg.basePtr++].Sequence;
                if (sequence.Count == 0)
                {
                    context.PushSequence(NodeSequence.Empty);
                }
                else
                {
                    for (int i = 0; i < sequence.Count; i++)
                    {
                        NodeSequence sequence2 = context.CreateSequence();
                        sequence2.StartNodeset();
                        sequence2.Add(ref sequence.Items[i]);
                        sequence2.StopNodeset();
                        context.Push(sequence2);
                    }
                }
            }
        }

        internal static void NodesetCount(ProcessingContext context)
        {
            StackFrame topArg = context.TopArg;
            while (topArg.basePtr <= topArg.endPtr)
            {
                context.SetValue(context, topArg.basePtr, (double) context.PeekSequence(topArg.basePtr).Count);
                topArg.basePtr++;
            }
        }

        internal static void NodesetLast(ProcessingContext context)
        {
            context.TransferSequenceSize();
        }

        internal static void NodesetLocalName(ProcessingContext context)
        {
            StackFrame topArg = context.TopArg;
            while (topArg.basePtr <= topArg.endPtr)
            {
                NodeSequence sequence = context.PeekSequence(topArg.basePtr);
                context.SetValue(context, topArg.basePtr, sequence.LocalName);
                topArg.basePtr++;
            }
        }

        internal static void NodesetLocalNameDefault(ProcessingContext context)
        {
            IterateAndPushSequences(context);
            NodesetLocalName(context);
        }

        internal static void NodesetName(ProcessingContext context)
        {
            StackFrame topArg = context.TopArg;
            while (topArg.basePtr <= topArg.endPtr)
            {
                NodeSequence sequence = context.PeekSequence(topArg.basePtr);
                context.SetValue(context, topArg.basePtr, sequence.Name);
                topArg.basePtr++;
            }
        }

        internal static void NodesetNameDefault(ProcessingContext context)
        {
            IterateAndPushSequences(context);
            NodesetName(context);
        }

        internal static void NodesetNamespaceUri(ProcessingContext context)
        {
            StackFrame topArg = context.TopArg;
            while (topArg.basePtr <= topArg.endPtr)
            {
                NodeSequence sequence = context.PeekSequence(topArg.basePtr);
                context.SetValue(context, topArg.basePtr, sequence.Namespace);
                topArg.basePtr++;
            }
        }

        internal static void NodesetNamespaceUriDefault(ProcessingContext context)
        {
            IterateAndPushSequences(context);
            NodesetNamespaceUri(context);
        }

        internal static void NodesetPosition(ProcessingContext context)
        {
            context.TransferSequencePositions();
        }

        internal static void NormalizeSpace(ProcessingContext context)
        {
            StackFrame topArg = context.TopArg;
            StringBuilder builder = new StringBuilder();
            while (topArg.basePtr <= topArg.endPtr)
            {
                char[] trimChars = new char[] { ' ', '\t', '\r', '\n' };
                string str = context.PeekString(topArg.basePtr).Trim(trimChars);
                bool flag = false;
                builder.Length = 0;
                for (int i = 0; i < str.Length; i++)
                {
                    char c = str[i];
                    if (XPathCharTypes.IsWhitespace(c))
                    {
                        if (!flag)
                        {
                            builder.Append(' ');
                            flag = true;
                        }
                    }
                    else
                    {
                        builder.Append(c);
                        flag = false;
                    }
                }
                context.SetValue(context, topArg.basePtr, builder.ToString());
                topArg.basePtr++;
            }
        }

        internal static void NormalizeSpaceDefault(ProcessingContext context)
        {
            IterateAndPushSequences(context);
            ConvertFirstArg(context, ValueDataType.String);
            NormalizeSpace(context);
        }

        internal static void NumberCeiling(ProcessingContext context)
        {
            StackFrame topArg = context.TopArg;
            while (topArg.basePtr <= topArg.endPtr)
            {
                context.SetValue(context, topArg.basePtr, Math.Ceiling(context.PeekDouble(topArg.basePtr)));
                topArg.basePtr++;
            }
        }

        internal static void NumberFloor(ProcessingContext context)
        {
            StackFrame topArg = context.TopArg;
            while (topArg.basePtr <= topArg.endPtr)
            {
                context.SetValue(context, topArg.basePtr, Math.Floor(context.PeekDouble(topArg.basePtr)));
                topArg.basePtr++;
            }
        }

        internal static void NumberNumber(ProcessingContext context)
        {
            StackFrame topArg = context.TopArg;
            Value[] values = context.Values;
            while (topArg.basePtr <= topArg.endPtr)
            {
                values[topArg.basePtr++].ConvertTo(context, ValueDataType.Double);
            }
        }

        internal static void NumberNumberDefault(ProcessingContext context)
        {
            IterateAndPushSequences(context);
            NumberNumber(context);
        }

        internal static void NumberRound(ProcessingContext context)
        {
            StackFrame topArg = context.TopArg;
            while (topArg.basePtr <= topArg.endPtr)
            {
                context.PeekDouble(topArg.basePtr);
                context.SetValue(context, topArg.basePtr, QueryValueModel.Round(context.PeekDouble(topArg.basePtr)));
                topArg.basePtr++;
            }
        }

        internal static void NumberSum(ProcessingContext context)
        {
            StackFrame topArg = context.TopArg;
            while (topArg.basePtr <= topArg.endPtr)
            {
                NodeSequence sequence = context.PeekSequence(topArg.basePtr);
                double val = 0.0;
                for (int i = 0; i < sequence.Count; i++)
                {
                    NodeSequenceItem item = sequence[i];
                    val += QueryValueModel.Double(item.StringValue());
                }
                context.SetValue(context, topArg.basePtr, val);
                topArg.basePtr++;
            }
        }

        internal static void StringConcatFour(ProcessingContext context)
        {
            StackFrame frame = context[0];
            StackFrame frame2 = context[1];
            StackFrame frame3 = context[2];
            StackFrame frame4 = context[3];
            while (frame.basePtr <= frame.endPtr)
            {
                context.SetValue(context, frame4.basePtr, context.PeekString(frame.basePtr) + context.PeekString(frame2.basePtr) + context.PeekString(frame3.basePtr) + context.PeekString(frame4.basePtr));
                frame.basePtr++;
                frame2.basePtr++;
                frame3.basePtr++;
                frame4.basePtr++;
            }
            context.PopFrame();
            context.PopFrame();
            context.PopFrame();
        }

        internal static void StringConcatThree(ProcessingContext context)
        {
            StackFrame frame = context[0];
            StackFrame frame2 = context[1];
            StackFrame frame3 = context[2];
            while (frame.basePtr <= frame.endPtr)
            {
                context.SetValue(context, frame3.basePtr, context.PeekString(frame.basePtr) + context.PeekString(frame2.basePtr) + context.PeekString(frame3.basePtr));
                frame.basePtr++;
                frame2.basePtr++;
                frame3.basePtr++;
            }
            context.PopFrame();
            context.PopFrame();
        }

        internal static void StringConcatTwo(ProcessingContext context)
        {
            StackFrame frame = context[0];
            StackFrame frame2 = context[1];
            while (frame.basePtr <= frame.endPtr)
            {
                context.SetValue(context, frame2.basePtr, context.PeekString(frame.basePtr) + context.PeekString(frame2.basePtr));
                frame.basePtr++;
                frame2.basePtr++;
            }
            context.PopFrame();
        }

        internal static void StringContains(ProcessingContext context)
        {
            StackFrame topArg = context.TopArg;
            StackFrame secondArg = context.SecondArg;
            while (topArg.basePtr <= topArg.endPtr)
            {
                string str = context.PeekString(topArg.basePtr);
                string str2 = context.PeekString(secondArg.basePtr);
                context.SetValue(context, secondArg.basePtr, -1 != str.IndexOf(str2, StringComparison.Ordinal));
                topArg.basePtr++;
                secondArg.basePtr++;
            }
            context.PopFrame();
        }

        internal static void StringLength(ProcessingContext context)
        {
            StackFrame topArg = context.TopArg;
            while (topArg.basePtr <= topArg.endPtr)
            {
                context.SetValue(context, topArg.basePtr, (double) context.PeekString(topArg.basePtr).Length);
                topArg.basePtr++;
            }
        }

        internal static void StringLengthDefault(ProcessingContext context)
        {
            IterateAndPushSequences(context);
            ConvertFirstArg(context, ValueDataType.String);
            StringLength(context);
        }

        internal static void StringStartsWith(ProcessingContext context)
        {
            StackFrame topArg = context.TopArg;
            StackFrame secondArg = context.SecondArg;
            while (topArg.basePtr <= topArg.endPtr)
            {
                string str = context.PeekString(topArg.basePtr);
                string str2 = context.PeekString(secondArg.basePtr);
                context.SetValue(context, secondArg.basePtr, str.StartsWith(str2, StringComparison.Ordinal));
                topArg.basePtr++;
                secondArg.basePtr++;
            }
            context.PopFrame();
        }

        internal static void StringString(ProcessingContext context)
        {
            StackFrame topArg = context.TopArg;
            Value[] values = context.Values;
            while (topArg.basePtr <= topArg.endPtr)
            {
                values[topArg.basePtr++].ConvertTo(context, ValueDataType.String);
            }
        }

        internal static void StringStringDefault(ProcessingContext context)
        {
            IterateAndPushSequences(context);
            StringString(context);
        }

        internal static void Substring(ProcessingContext context)
        {
            StackFrame topArg = context.TopArg;
            StackFrame secondArg = context.SecondArg;
            while (topArg.basePtr <= topArg.endPtr)
            {
                string str = context.PeekString(topArg.basePtr);
                int startIndex = ((int) Math.Round(context.PeekDouble(secondArg.basePtr))) - 1;
                if (startIndex < 0)
                {
                    startIndex = 0;
                }
                context.SetValue(context, secondArg.basePtr, (startIndex >= str.Length) ? string.Empty : str.Substring(startIndex));
                topArg.basePtr++;
                secondArg.basePtr++;
            }
            context.PopFrame();
        }

        internal static void SubstringAfter(ProcessingContext context)
        {
            StackFrame topArg = context.TopArg;
            StackFrame secondArg = context.SecondArg;
            while (topArg.basePtr <= topArg.endPtr)
            {
                string str = context.PeekString(topArg.basePtr);
                string str2 = context.PeekString(secondArg.basePtr);
                int index = str.IndexOf(str2, StringComparison.Ordinal);
                context.SetValue(context, secondArg.basePtr, (index == -1) ? string.Empty : str.Substring(index + str2.Length));
                topArg.basePtr++;
                secondArg.basePtr++;
            }
            context.PopFrame();
        }

        internal static void SubstringBefore(ProcessingContext context)
        {
            StackFrame topArg = context.TopArg;
            StackFrame secondArg = context.SecondArg;
            while (topArg.basePtr <= topArg.endPtr)
            {
                string str = context.PeekString(topArg.basePtr);
                string str2 = context.PeekString(secondArg.basePtr);
                int index = str.IndexOf(str2, StringComparison.Ordinal);
                context.SetValue(context, secondArg.basePtr, (index == -1) ? string.Empty : str.Substring(0, index));
                topArg.basePtr++;
                secondArg.basePtr++;
            }
            context.PopFrame();
        }

        internal static void SubstringLimit(ProcessingContext context)
        {
            StackFrame topArg = context.TopArg;
            StackFrame secondArg = context.SecondArg;
            StackFrame frame3 = context[2];
            while (topArg.basePtr <= topArg.endPtr)
            {
                string str2;
                string str = context.PeekString(topArg.basePtr);
                int startIndex = ((int) Math.Round(context.PeekDouble(secondArg.basePtr))) - 1;
                if (startIndex < 0)
                {
                    startIndex = 0;
                }
                int length = (int) Math.Round(context.PeekDouble(frame3.basePtr));
                if ((length < 1) || ((startIndex + length) >= str.Length))
                {
                    str2 = string.Empty;
                }
                else
                {
                    str2 = str.Substring(startIndex, length);
                }
                context.SetValue(context, frame3.basePtr, str2);
                secondArg.basePtr++;
                topArg.basePtr++;
                frame3.basePtr++;
            }
            context.PopFrame();
            context.PopFrame();
        }

        internal static void Translate(ProcessingContext context)
        {
            StackFrame topArg = context.TopArg;
            StackFrame secondArg = context.SecondArg;
            StackFrame frame3 = context[2];
            StringBuilder builder = new StringBuilder();
            while (topArg.basePtr <= topArg.endPtr)
            {
                builder.Length = 0;
                string str = context.PeekString(topArg.basePtr);
                string str2 = context.PeekString(secondArg.basePtr);
                string str3 = context.PeekString(frame3.basePtr);
                for (int i = 0; i < str.Length; i++)
                {
                    char ch = str[i];
                    int index = str2.IndexOf(ch);
                    if (index < 0)
                    {
                        builder.Append(ch);
                    }
                    else if (index < str3.Length)
                    {
                        builder.Append(str3[index]);
                    }
                }
                context.SetValue(context, frame3.basePtr, builder.ToString());
                topArg.basePtr++;
                secondArg.basePtr++;
                frame3.basePtr++;
            }
            context.PopFrame();
            context.PopFrame();
        }

        internal XPathFunctionID ID
        {
            get
            {
                return this.functionID;
            }
        }
    }
}

