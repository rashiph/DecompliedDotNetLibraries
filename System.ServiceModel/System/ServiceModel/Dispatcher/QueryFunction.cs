namespace System.ServiceModel.Dispatcher
{
    using System;

    internal abstract class QueryFunction
    {
        private static ValueDataType[] emptyParams = new ValueDataType[0];
        private QueryFunctionFlag flags;
        protected string name;
        private ValueDataType[] paramTypes;
        private ValueDataType returnType;

        internal QueryFunction(string name, ValueDataType returnType) : this(name, returnType, emptyParams, QueryFunctionFlag.None)
        {
        }

        internal QueryFunction(string name, ValueDataType returnType, QueryFunctionFlag flags) : this(name, returnType, emptyParams, flags)
        {
        }

        internal QueryFunction(string name, ValueDataType returnType, ValueDataType[] paramTypes) : this(name, returnType, paramTypes, QueryFunctionFlag.None)
        {
        }

        internal QueryFunction(string name, ValueDataType returnType, ValueDataType[] paramTypes, QueryFunctionFlag flags)
        {
            this.name = name;
            this.returnType = returnType;
            this.paramTypes = paramTypes;
            this.flags = flags;
        }

        internal bool Bind(string name, XPathExprList args)
        {
            return (((string.CompareOrdinal(this.name, name) == 0) && (this.paramTypes.Length == args.Count)) && (this.paramTypes.Length == args.Count));
        }

        internal abstract bool Equals(QueryFunction function);
        internal abstract void Eval(ProcessingContext context);
        internal bool TestFlag(QueryFunctionFlag flag)
        {
            return (QueryFunctionFlag.None != (this.flags & flag));
        }

        internal ValueDataType[] ParamTypes
        {
            get
            {
                return this.paramTypes;
            }
        }

        internal ValueDataType ReturnType
        {
            get
            {
                return this.returnType;
            }
        }
    }
}

