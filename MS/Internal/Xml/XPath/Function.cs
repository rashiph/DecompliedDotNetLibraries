namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Collections;
    using System.Xml.XPath;

    internal class Function : AstNode
    {
        private ArrayList argumentList;
        private FunctionType functionType;
        private string name;
        private string prefix;
        internal static XPathResultType[] ReturnTypes;

        static Function()
        {
            XPathResultType[] typeArray = new XPathResultType[0x1c];
            typeArray[3] = XPathResultType.NodeSet;
            typeArray[4] = XPathResultType.String;
            typeArray[5] = XPathResultType.String;
            typeArray[6] = XPathResultType.String;
            typeArray[7] = XPathResultType.String;
            typeArray[8] = XPathResultType.Boolean;
            typeArray[10] = XPathResultType.Boolean;
            typeArray[11] = XPathResultType.Boolean;
            typeArray[12] = XPathResultType.Boolean;
            typeArray[13] = XPathResultType.String;
            typeArray[14] = XPathResultType.Boolean;
            typeArray[15] = XPathResultType.Boolean;
            typeArray[0x10] = XPathResultType.String;
            typeArray[0x11] = XPathResultType.String;
            typeArray[0x12] = XPathResultType.String;
            typeArray[20] = XPathResultType.String;
            typeArray[0x15] = XPathResultType.String;
            typeArray[0x16] = XPathResultType.Boolean;
            typeArray[0x1b] = XPathResultType.Any;
            ReturnTypes = typeArray;
        }

        public Function(FunctionType ftype)
        {
            this.functionType = ftype;
        }

        public Function(FunctionType ftype, AstNode arg)
        {
            this.functionType = ftype;
            this.argumentList = new ArrayList();
            this.argumentList.Add(arg);
        }

        public Function(FunctionType ftype, ArrayList argumentList)
        {
            this.functionType = ftype;
            this.argumentList = new ArrayList(argumentList);
        }

        public Function(string prefix, string name, ArrayList argumentList)
        {
            this.functionType = FunctionType.FuncUserDefined;
            this.prefix = prefix;
            this.name = name;
            this.argumentList = new ArrayList(argumentList);
        }

        public ArrayList ArgumentList
        {
            get
            {
                return this.argumentList;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public string Prefix
        {
            get
            {
                return this.prefix;
            }
        }

        public override XPathResultType ReturnType
        {
            get
            {
                return ReturnTypes[(int) this.functionType];
            }
        }

        public override AstNode.AstType Type
        {
            get
            {
                return AstNode.AstType.Function;
            }
        }

        public FunctionType TypeOfFunction
        {
            get
            {
                return this.functionType;
            }
        }

        public enum FunctionType
        {
            FuncLast,
            FuncPosition,
            FuncCount,
            FuncID,
            FuncLocalName,
            FuncNameSpaceUri,
            FuncName,
            FuncString,
            FuncBoolean,
            FuncNumber,
            FuncTrue,
            FuncFalse,
            FuncNot,
            FuncConcat,
            FuncStartsWith,
            FuncContains,
            FuncSubstringBefore,
            FuncSubstringAfter,
            FuncSubstring,
            FuncStringLength,
            FuncNormalize,
            FuncTranslate,
            FuncLang,
            FuncSum,
            FuncFloor,
            FuncCeiling,
            FuncRound,
            FuncUserDefined
        }
    }
}

