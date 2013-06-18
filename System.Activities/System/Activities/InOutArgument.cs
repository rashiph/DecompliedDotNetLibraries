namespace System.Activities
{
    using System;

    public abstract class InOutArgument : Argument
    {
        internal InOutArgument()
        {
            base.Direction = ArgumentDirection.InOut;
        }

        public static InOutArgument CreateReference(InOutArgument argumentToReference, string referencedArgumentName)
        {
            if (argumentToReference == null)
            {
                throw FxTrace.Exception.ArgumentNull("argumentToReference");
            }
            if (string.IsNullOrEmpty(referencedArgumentName))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("referencedArgumentName");
            }
            return (InOutArgument) ActivityUtilities.CreateReferenceArgument(argumentToReference.ArgumentType, ArgumentDirection.InOut, referencedArgumentName);
        }
    }
}

