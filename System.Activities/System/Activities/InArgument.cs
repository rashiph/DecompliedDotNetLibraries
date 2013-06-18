namespace System.Activities
{
    using System;

    public abstract class InArgument : Argument
    {
        internal InArgument()
        {
            base.Direction = ArgumentDirection.In;
        }

        public static InArgument CreateReference(InArgument argumentToReference, string referencedArgumentName)
        {
            if (argumentToReference == null)
            {
                throw FxTrace.Exception.ArgumentNull("argumentToReference");
            }
            if (string.IsNullOrEmpty(referencedArgumentName))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("referencedArgumentName");
            }
            return (InArgument) ActivityUtilities.CreateReferenceArgument(argumentToReference.ArgumentType, ArgumentDirection.In, referencedArgumentName);
        }

        public static InArgument CreateReference(InOutArgument argumentToReference, string referencedArgumentName)
        {
            if (argumentToReference == null)
            {
                throw FxTrace.Exception.ArgumentNull("argumentToReference");
            }
            if (string.IsNullOrEmpty(referencedArgumentName))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("referencedArgumentName");
            }
            return (InArgument) ActivityUtilities.CreateReferenceArgument(argumentToReference.ArgumentType, ArgumentDirection.In, referencedArgumentName);
        }
    }
}

