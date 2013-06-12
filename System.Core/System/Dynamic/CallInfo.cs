namespace System.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Dynamic.Utils;
    using System.Linq.Expressions;

    public sealed class CallInfo
    {
        private readonly int _argCount;
        private readonly ReadOnlyCollection<string> _argNames;

        public CallInfo(int argCount, params string[] argNames) : this(argCount, (IEnumerable<string>) argNames)
        {
        }

        public CallInfo(int argCount, IEnumerable<string> argNames)
        {
            ContractUtils.RequiresNotNull(argNames, "argNames");
            ReadOnlyCollection<string> array = argNames.ToReadOnly<string>();
            if (argCount < array.Count)
            {
                throw Error.ArgCntMustBeGreaterThanNameCnt();
            }
            ContractUtils.RequiresNotNullItems<string>(array, "argNames");
            this._argCount = argCount;
            this._argNames = array;
        }

        public override bool Equals(object obj)
        {
            CallInfo info = obj as CallInfo;
            return ((this._argCount == info._argCount) && this._argNames.ListEquals<string>(info._argNames));
        }

        public override int GetHashCode()
        {
            return (this._argCount ^ this._argNames.ListHashCode<string>());
        }

        public int ArgumentCount
        {
            get
            {
                return this._argCount;
            }
        }

        public ReadOnlyCollection<string> ArgumentNames
        {
            get
            {
                return this._argNames;
            }
        }
    }
}

