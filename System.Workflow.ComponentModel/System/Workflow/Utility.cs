namespace System.Workflow
{
    using System;
    using System.Runtime.InteropServices;

    internal static class Utility
    {
        internal static Guid CreateGuid(string guidString)
        {
            bool flag = false;
            Guid empty = Guid.Empty;
            try
            {
                empty = new Guid(guidString);
                flag = true;
            }
            finally
            {
            }
            return empty;
        }

        internal static bool TryCreateGuid(string guidString, out Guid result)
        {
            bool flag = false;
            result = Guid.Empty;
            try
            {
                result = new Guid(guidString);
                flag = true;
            }
            catch (ArgumentException)
            {
            }
            catch (FormatException)
            {
            }
            catch (OverflowException)
            {
            }
            return flag;
        }
    }
}

