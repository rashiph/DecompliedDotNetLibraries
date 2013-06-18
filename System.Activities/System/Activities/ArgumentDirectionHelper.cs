namespace System.Activities
{
    using System;
    using System.ComponentModel;

    internal static class ArgumentDirectionHelper
    {
        internal static bool IsDefined(ArgumentDirection direction)
        {
            if ((direction != ArgumentDirection.In) && (direction != ArgumentDirection.Out))
            {
                return (direction == ArgumentDirection.InOut);
            }
            return true;
        }

        public static bool IsIn(Argument argument)
        {
            return IsIn(argument.Direction);
        }

        public static bool IsIn(ArgumentDirection direction)
        {
            if (direction != ArgumentDirection.In)
            {
                return (direction == ArgumentDirection.InOut);
            }
            return true;
        }

        public static bool IsOut(Argument argument)
        {
            return IsOut(argument.Direction);
        }

        public static bool IsOut(ArgumentDirection direction)
        {
            if (direction != ArgumentDirection.Out)
            {
                return (direction == ArgumentDirection.InOut);
            }
            return true;
        }

        public static void Validate(ArgumentDirection direction, string argumentName)
        {
            if (!IsDefined(direction))
            {
                throw FxTrace.Exception.AsError(new InvalidEnumArgumentException(argumentName, (int) direction, typeof(ArgumentDirection)));
            }
        }
    }
}

