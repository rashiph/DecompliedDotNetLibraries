namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class XPathFunctionLibrary : IFunctionLibrary
    {
        private static XPathFunction[] functionTable;

        static XPathFunctionLibrary()
        {
            XPathFunction[] functionArray = new XPathFunction[0x24];
            ValueDataType[] argTypes = new ValueDataType[1];
            functionArray[0] = new XPathFunction(XPathFunctionID.Boolean, "boolean", ValueDataType.Boolean, argTypes);
            functionArray[1] = new XPathFunction(XPathFunctionID.False, "false", ValueDataType.Boolean);
            functionArray[2] = new XPathFunction(XPathFunctionID.True, "true", ValueDataType.Boolean);
            functionArray[3] = new XPathFunction(XPathFunctionID.Not, "not", ValueDataType.Boolean, new ValueDataType[] { ValueDataType.Boolean });
            functionArray[4] = new XPathFunction(XPathFunctionID.Lang, "lang", ValueDataType.Boolean, new ValueDataType[] { ValueDataType.String });
            ValueDataType[] typeArray4 = new ValueDataType[1];
            functionArray[5] = new XPathFunction(XPathFunctionID.Number, "number", ValueDataType.Double, typeArray4);
            functionArray[6] = new XPathFunction(XPathFunctionID.NumberDefault, "number", ValueDataType.Double);
            functionArray[7] = new XPathFunction(XPathFunctionID.Sum, "sum", ValueDataType.Double, new ValueDataType[] { ValueDataType.Sequence });
            functionArray[8] = new XPathFunction(XPathFunctionID.Floor, "floor", ValueDataType.Double, new ValueDataType[] { ValueDataType.Double });
            functionArray[9] = new XPathFunction(XPathFunctionID.Ceiling, "ceiling", ValueDataType.Double, new ValueDataType[] { ValueDataType.Double });
            functionArray[10] = new XPathFunction(XPathFunctionID.Round, "round", ValueDataType.Double, new ValueDataType[] { ValueDataType.Double });
            ValueDataType[] typeArray9 = new ValueDataType[1];
            functionArray[11] = new XPathFunction(XPathFunctionID.String, "string", ValueDataType.String, typeArray9);
            functionArray[12] = new XPathFunction(XPathFunctionID.StringDefault, "string", ValueDataType.String, QueryFunctionFlag.UsesContextNode);
            functionArray[13] = new XPathFunction(XPathFunctionID.ConcatTwo, "concat", ValueDataType.String, new ValueDataType[] { ValueDataType.String, ValueDataType.String });
            functionArray[14] = new XPathFunction(XPathFunctionID.ConcatThree, "concat", ValueDataType.String, new ValueDataType[] { ValueDataType.String, ValueDataType.String, ValueDataType.String });
            functionArray[15] = new XPathFunction(XPathFunctionID.ConcatFour, "concat", ValueDataType.String, new ValueDataType[] { ValueDataType.String, ValueDataType.String, ValueDataType.String, ValueDataType.String });
            functionArray[0x10] = new XPathFunction(XPathFunctionID.StartsWith, "starts-with", ValueDataType.Boolean, new ValueDataType[] { ValueDataType.String, ValueDataType.String });
            functionArray[0x11] = new XPathFunction(XPathFunctionID.NormalizeSpace, "normalize-space", ValueDataType.String, new ValueDataType[] { ValueDataType.String });
            functionArray[0x12] = new XPathFunction(XPathFunctionID.NormalizeSpaceDefault, "normalize-space", ValueDataType.String, QueryFunctionFlag.UsesContextNode);
            functionArray[0x13] = new XPathFunction(XPathFunctionID.Contains, "contains", ValueDataType.Boolean, new ValueDataType[] { ValueDataType.String, ValueDataType.String });
            functionArray[20] = new XPathFunction(XPathFunctionID.SubstringBefore, "substring-before", ValueDataType.String, new ValueDataType[] { ValueDataType.String, ValueDataType.String });
            functionArray[0x15] = new XPathFunction(XPathFunctionID.SubstringAfter, "substring-after", ValueDataType.String, new ValueDataType[] { ValueDataType.String, ValueDataType.String });
            functionArray[0x16] = new XPathFunction(XPathFunctionID.Substring, "substring", ValueDataType.String, new ValueDataType[] { ValueDataType.String, ValueDataType.Double });
            functionArray[0x17] = new XPathFunction(XPathFunctionID.SubstringLimit, "substring", ValueDataType.String, new ValueDataType[] { ValueDataType.String, ValueDataType.Double, ValueDataType.Double });
            functionArray[0x18] = new XPathFunction(XPathFunctionID.StringLength, "string-length", ValueDataType.Double, new ValueDataType[] { ValueDataType.String });
            functionArray[0x19] = new XPathFunction(XPathFunctionID.StringLengthDefault, "string-length", ValueDataType.Double, QueryFunctionFlag.UsesContextNode);
            functionArray[0x1a] = new XPathFunction(XPathFunctionID.Translate, "translate", ValueDataType.String, new ValueDataType[] { ValueDataType.String, ValueDataType.String, ValueDataType.String });
            functionArray[0x1b] = new XPathFunction(XPathFunctionID.Last, "last", ValueDataType.Double, QueryFunctionFlag.UsesContextNode);
            functionArray[0x1c] = new XPathFunction(XPathFunctionID.Position, "position", ValueDataType.Double, QueryFunctionFlag.UsesContextNode);
            functionArray[0x1d] = new XPathFunction(XPathFunctionID.Count, "count", ValueDataType.Double, new ValueDataType[] { ValueDataType.Sequence });
            functionArray[30] = new XPathFunction(XPathFunctionID.LocalName, "local-name", ValueDataType.String, new ValueDataType[] { ValueDataType.Sequence });
            functionArray[0x1f] = new XPathFunction(XPathFunctionID.LocalNameDefault, "local-name", ValueDataType.String, QueryFunctionFlag.UsesContextNode);
            functionArray[0x20] = new XPathFunction(XPathFunctionID.Name, "name", ValueDataType.String, new ValueDataType[] { ValueDataType.Sequence });
            functionArray[0x21] = new XPathFunction(XPathFunctionID.NameDefault, "name", ValueDataType.String, QueryFunctionFlag.UsesContextNode);
            functionArray[0x22] = new XPathFunction(XPathFunctionID.NamespaceUri, "namespace-uri", ValueDataType.String, new ValueDataType[] { ValueDataType.Sequence });
            functionArray[0x23] = new XPathFunction(XPathFunctionID.NamespaceUriDefault, "namespace-uri", ValueDataType.String, QueryFunctionFlag.UsesContextNode);
            functionTable = functionArray;
        }

        internal XPathFunctionLibrary()
        {
        }

        public QueryFunction Bind(string functionName, string functionNamespace, XPathExprList args)
        {
            if ((functionName == "concat") && (args.Count > 4))
            {
                ConcatFunction function = new ConcatFunction(args.Count);
                if (function.Bind(functionName, args))
                {
                    return function;
                }
            }
            else
            {
                for (int i = 0; i < functionTable.Length; i++)
                {
                    if (functionTable[i].Bind(functionName, args))
                    {
                        return functionTable[i];
                    }
                }
            }
            return null;
        }
    }
}

