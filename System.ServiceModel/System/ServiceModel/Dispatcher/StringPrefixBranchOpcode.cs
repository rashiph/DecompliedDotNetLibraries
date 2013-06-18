namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class StringPrefixBranchOpcode : QueryConditionalBranchOpcode
    {
        internal StringPrefixBranchOpcode() : base(OpcodeID.StringPrefixBranch, new TrieBranchIndex())
        {
        }
    }
}

