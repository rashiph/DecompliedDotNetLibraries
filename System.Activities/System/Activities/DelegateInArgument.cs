namespace System.Activities
{
    using System;

    public abstract class DelegateInArgument : DelegateArgument
    {
        internal DelegateInArgument()
        {
            base.Direction = ArgumentDirection.In;
        }
    }
}

