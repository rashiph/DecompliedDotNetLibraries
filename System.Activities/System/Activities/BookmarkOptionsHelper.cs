namespace System.Activities
{
    using System;
    using System.ComponentModel;

    internal static class BookmarkOptionsHelper
    {
        private static bool IsDefined(BookmarkOptions options)
        {
            if (options != BookmarkOptions.None)
            {
                return ((options & (BookmarkOptions.NonBlocking | BookmarkOptions.MultipleResume)) == options);
            }
            return true;
        }

        public static bool IsNonBlocking(BookmarkOptions options)
        {
            return ((options & BookmarkOptions.NonBlocking) == BookmarkOptions.NonBlocking);
        }

        public static bool SupportsMultipleResumes(BookmarkOptions options)
        {
            return ((options & BookmarkOptions.MultipleResume) == BookmarkOptions.MultipleResume);
        }

        public static void Validate(BookmarkOptions options, string argumentName)
        {
            if (!IsDefined(options))
            {
                throw FxTrace.Exception.AsError(new InvalidEnumArgumentException(argumentName, (int) options, typeof(BookmarkOptions)));
            }
        }
    }
}

