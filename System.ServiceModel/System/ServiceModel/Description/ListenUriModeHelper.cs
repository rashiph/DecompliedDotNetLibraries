namespace System.ServiceModel.Description
{
    using System;

    internal static class ListenUriModeHelper
    {
        public static bool IsDefined(ListenUriMode mode)
        {
            if (mode != ListenUriMode.Explicit)
            {
                return (mode == ListenUriMode.Unique);
            }
            return true;
        }
    }
}

