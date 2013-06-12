namespace System.Data.SqlTypes
{
    using System;
    using System.Data;

    internal sealed class SQLResource
    {
        internal static readonly string AlreadyFilledMessage = Res.GetString("SqlMisc_AlreadyFilledMessage");
        internal static readonly string ArithOverflowMessage = Res.GetString("SqlMisc_ArithOverflowMessage");
        internal static readonly string ClosedXmlReaderMessage = Res.GetString("SqlMisc_ClosedXmlReaderMessage");
        internal static readonly string CompareDiffCollationMessage = Res.GetString("SqlMisc_CompareDiffCollationMessage");
        internal static readonly string ConcatDiffCollationMessage = Res.GetString("SqlMisc_ConcatDiffCollationMessage");
        internal static readonly string ConversionOverflowMessage = Res.GetString("SqlMisc_ConversionOverflowMessage");
        internal static readonly string DateTimeOverflowMessage = Res.GetString("SqlMisc_DateTimeOverflowMessage");
        internal static readonly string DivideByZeroMessage = Res.GetString("SqlMisc_DivideByZeroMessage");
        internal static readonly string FormatMessage = Res.GetString("SqlMisc_FormatMessage");
        internal static readonly string InvalidArraySizeMessage = Res.GetString("SqlMisc_InvalidArraySizeMessage");
        internal static readonly string InvalidDateTimeMessage = Res.GetString("SqlMisc_InvalidDateTimeMessage");
        internal static readonly string InvalidFlagMessage = Res.GetString("SqlMisc_InvalidFlagMessage");
        internal static readonly string InvalidPrecScaleMessage = Res.GetString("SqlMisc_InvalidPrecScaleMessage");
        internal static readonly string MessageString = Res.GetString("SqlMisc_MessageString");
        internal static readonly string NotFilledMessage = Res.GetString("SqlMisc_NotFilledMessage");
        internal static readonly string NullString = Res.GetString("SqlMisc_NullString");
        internal static readonly string NullValueMessage = Res.GetString("SqlMisc_NullValueMessage");
        internal static readonly string NumeToDecOverflowMessage = Res.GetString("SqlMisc_NumeToDecOverflowMessage");
        internal static readonly string TimeZoneSpecifiedMessage = Res.GetString("SqlMisc_TimeZoneSpecifiedMessage");
        internal static readonly string TruncationMessage = Res.GetString("SqlMisc_TruncationMessage");

        private SQLResource()
        {
        }

        internal static string InvalidOpStreamClosed(string method)
        {
            return Res.GetString("SqlMisc_InvalidOpStreamClosed", new object[] { method });
        }

        internal static string InvalidOpStreamNonReadable(string method)
        {
            return Res.GetString("SqlMisc_InvalidOpStreamNonReadable", new object[] { method });
        }

        internal static string InvalidOpStreamNonSeekable(string method)
        {
            return Res.GetString("SqlMisc_InvalidOpStreamNonSeekable", new object[] { method });
        }

        internal static string InvalidOpStreamNonWritable(string method)
        {
            return Res.GetString("SqlMisc_InvalidOpStreamNonWritable", new object[] { method });
        }
    }
}

