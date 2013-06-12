namespace System.Linq
{
    using System;

    internal static class Error
    {
        internal static Exception ArgumentArrayHasTooManyElements(object p0)
        {
            return new ArgumentException(Strings.ArgumentArrayHasTooManyElements(p0));
        }

        internal static Exception ArgumentNotIEnumerableGeneric(object p0)
        {
            return new ArgumentException(Strings.ArgumentNotIEnumerableGeneric(p0));
        }

        internal static Exception ArgumentNotLambda(object p0)
        {
            return new ArgumentException(Strings.ArgumentNotLambda(p0));
        }

        internal static Exception ArgumentNotSequence(object p0)
        {
            return new ArgumentException(Strings.ArgumentNotSequence(p0));
        }

        internal static Exception ArgumentNotValid(object p0)
        {
            return new ArgumentException(Strings.ArgumentNotValid(p0));
        }

        internal static Exception ArgumentNull(string paramName)
        {
            return new ArgumentNullException(paramName);
        }

        internal static Exception ArgumentOutOfRange(string paramName)
        {
            return new ArgumentOutOfRangeException(paramName);
        }

        internal static Exception IncompatibleElementTypes()
        {
            return new ArgumentException(Strings.IncompatibleElementTypes);
        }

        internal static Exception MoreThanOneElement()
        {
            return new InvalidOperationException(Strings.MoreThanOneElement);
        }

        internal static Exception MoreThanOneMatch()
        {
            return new InvalidOperationException(Strings.MoreThanOneMatch);
        }

        internal static Exception NoArgumentMatchingMethodsInQueryable(object p0)
        {
            return new InvalidOperationException(Strings.NoArgumentMatchingMethodsInQueryable(p0));
        }

        internal static Exception NoElements()
        {
            return new InvalidOperationException(Strings.NoElements);
        }

        internal static Exception NoMatch()
        {
            return new InvalidOperationException(Strings.NoMatch);
        }

        internal static Exception NoMethodOnType(object p0, object p1)
        {
            return new InvalidOperationException(Strings.NoMethodOnType(p0, p1));
        }

        internal static Exception NoMethodOnTypeMatchingArguments(object p0, object p1)
        {
            return new InvalidOperationException(Strings.NoMethodOnTypeMatchingArguments(p0, p1));
        }

        internal static Exception NoNameMatchingMethodsInQueryable(object p0)
        {
            return new InvalidOperationException(Strings.NoNameMatchingMethodsInQueryable(p0));
        }

        internal static Exception NotImplemented()
        {
            return new NotImplementedException();
        }

        internal static Exception NotSupported()
        {
            return new NotSupportedException();
        }
    }
}

