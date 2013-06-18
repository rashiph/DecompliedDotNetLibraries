namespace System.ServiceModel.Description
{
    using System;

    internal static class MessageDirectionHelper
    {
        internal static bool IsDefined(MessageDirection value)
        {
            if (value != MessageDirection.Input)
            {
                return (value == MessageDirection.Output);
            }
            return true;
        }

        internal static MessageDirection Opposite(MessageDirection d)
        {
            if (d != MessageDirection.Input)
            {
                return MessageDirection.Input;
            }
            return MessageDirection.Output;
        }
    }
}

