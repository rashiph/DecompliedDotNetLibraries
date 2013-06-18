namespace System.Activities
{
    using System;

    public abstract class OutArgument : Argument
    {
        internal OutArgument()
        {
            base.Direction = ArgumentDirection.Out;
        }

        public static OutArgument CreateReference(InOutArgument argumentToReference, string referencedArgumentName)
        {
            if (argumentToReference == null)
            {
                throw FxTrace.Exception.ArgumentNull("argumentToReference");
            }
            if (string.IsNullOrEmpty(referencedArgumentName))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("referencedArgumentName");
            }
            return (OutArgument) ActivityUtilities.CreateReferenceArgument(argumentToReference.ArgumentType, ArgumentDirection.Out, referencedArgumentName);
        }

        public static OutArgument CreateReference(OutArgument argumentToReference, string referencedArgumentName)
        {
            if (argumentToReference == null)
            {
                throw FxTrace.Exception.ArgumentNull("argumentToReference");
            }
            if (string.IsNullOrEmpty(referencedArgumentName))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("referencedArgumentName");
            }
            return (OutArgument) ActivityUtilities.CreateReferenceArgument(argumentToReference.ArgumentType, ArgumentDirection.Out, referencedArgumentName);
        }
    }
}

