using System;

internal static class SRETW
{
    internal const string ArgumentOutOfRange_MaxArgExceeded = "ArgumentOutOfRange_MaxArgExceeded";
    internal const string ArgumentOutOfRange_MaxStringsExceeded = "ArgumentOutOfRange_MaxStringsExceeded";
    internal const string ArgumentOutOfRange_NeedNonNegNum = "ArgumentOutOfRange_NeedNonNegNum";
    internal const string ArgumentOutOfRange_NeedValidId = "ArgumentOutOfRange_NeedValidId";

    public static string GetString(string name)
    {
        return name;
    }

    public static string GetString(string name, params object[] args)
    {
        return name;
    }
}

