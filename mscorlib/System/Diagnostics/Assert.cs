namespace System.Diagnostics
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Security;

    internal static class Assert
    {
        private const int COR_E_FAILFAST = -2146232797;
        private static int iFilterArraySize;
        private static int iNumOfFilters;
        private static AssertFilter[] ListOfFilters;

        static Assert()
        {
            AddFilter(new DefaultFilter());
        }

        internal static void AddFilter(AssertFilter filter)
        {
            if (iFilterArraySize <= iNumOfFilters)
            {
                AssertFilter[] destinationArray = new AssertFilter[iFilterArraySize + 2];
                if (iNumOfFilters > 0)
                {
                    Array.Copy(ListOfFilters, destinationArray, iNumOfFilters);
                }
                iFilterArraySize += 2;
                ListOfFilters = destinationArray;
            }
            ListOfFilters[iNumOfFilters++] = filter;
        }

        internal static void Check(bool condition, string conditionString, string message)
        {
            if (!condition)
            {
                Fail(conditionString, message);
            }
        }

        [SecuritySafeCritical]
        internal static void Fail(string conditionString, string message)
        {
            StackTrace location = new StackTrace();
            int iNumOfFilters = Assert.iNumOfFilters;
            while (iNumOfFilters > 0)
            {
                AssertFilters filters = ListOfFilters[--iNumOfFilters].AssertFailure(conditionString, message, location);
                if (filters == AssertFilters.FailDebug)
                {
                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                        return;
                    }
                    if (!Debugger.Launch())
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_DebuggerLaunchFailed"));
                    }
                    break;
                }
                if (filters == AssertFilters.FailTerminate)
                {
                    if (Debugger.IsAttached)
                    {
                        Environment.Exit(-2146232797);
                    }
                    else
                    {
                        Environment.FailFast(message);
                    }
                }
                else if (filters == AssertFilters.FailIgnore)
                {
                    return;
                }
            }
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern int ShowDefaultAssertDialog(string conditionString, string message, string stackTrace);
    }
}

