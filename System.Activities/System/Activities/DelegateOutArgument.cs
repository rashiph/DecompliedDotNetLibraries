namespace System.Activities
{
    using System;

    public abstract class DelegateOutArgument : DelegateArgument
    {
        internal DelegateOutArgument()
        {
            base.Direction = ArgumentDirection.Out;
        }
    }
}

