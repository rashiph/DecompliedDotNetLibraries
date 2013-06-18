namespace System.Activities
{
    using System.Collections.Generic;

    public sealed class ActivityAction : ActivityDelegate
    {
        private static readonly IList<RuntimeDelegateArgument> EmptyDelegateParameters = new List<RuntimeDelegateArgument>(0);

        internal override IList<RuntimeDelegateArgument> InternalGetRuntimeDelegateArguments()
        {
            return EmptyDelegateParameters;
        }
    }
}

