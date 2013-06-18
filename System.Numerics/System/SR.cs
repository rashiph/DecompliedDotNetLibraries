namespace System
{
    using System.Globalization;
    using System.Resources;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class SR
    {
        internal const string Argument_ByteArrayLengthMustBeAMultipleOf4 = "Argument_ByteArrayLengthMustBeAMultipleOf4";
        internal const string Argument_InvalidCharactersInString = "Argument_InvalidCharactersInString";
        internal const string Argument_InvalidHexStyle = "Argument_InvalidHexStyle";
        internal const string Argument_InvalidNumberStyles = "Argument_InvalidNumberStyles";
        internal const string Argument_MustBeBigInt = "Argument_MustBeBigInt";
        internal const string Argument_ParsedStringWasInvalid = "Argument_ParsedStringWasInvalid";
        internal const string ArgumentOutOfRange_MustBeLessThanUInt32MaxValue = "ArgumentOutOfRange_MustBeLessThanUInt32MaxValue";
        internal const string ArgumentOutOfRange_MustBeNonNeg = "ArgumentOutOfRange_MustBeNonNeg";
        internal const string Format_InvalidFormatSpecifier = "Format_InvalidFormatSpecifier";
        internal const string Format_TooLarge = "Format_TooLarge";
        private static SR loader;
        internal const string NotSupported_NumberStyle = "NotSupported_NumberStyle";
        internal const string Overflow_BigIntInfinity = "Overflow_BigIntInfinity";
        internal const string Overflow_Decimal = "Overflow_Decimal";
        internal const string Overflow_Int32 = "Overflow_Int32";
        internal const string Overflow_Int64 = "Overflow_Int64";
        internal const string Overflow_NotANumber = "Overflow_NotANumber";
        internal const string Overflow_ParseBigInteger = "Overflow_ParseBigInteger";
        internal const string Overflow_UInt32 = "Overflow_UInt32";
        internal const string Overflow_UInt64 = "Overflow_UInt64";
        private ResourceManager resources;

        internal SR()
        {
            this.resources = new ResourceManager("System.Numerics", base.GetType().Assembly);
        }

        private static SR GetLoader()
        {
            if (loader == null)
            {
                SR sr = new SR();
                Interlocked.CompareExchange<SR>(ref loader, sr, null);
            }
            return loader;
        }

        public static object GetObject(string name)
        {
            SR loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetObject(name, Culture);
        }

        public static string GetString(string name)
        {
            SR loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetString(name, Culture);
        }

        public static string GetString(string name, params object[] args)
        {
            SR loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            string format = loader.resources.GetString(name, Culture);
            if ((args == null) || (args.Length <= 0))
            {
                return format;
            }
            for (int i = 0; i < args.Length; i++)
            {
                string str2 = args[i] as string;
                if ((str2 != null) && (str2.Length > 0x400))
                {
                    args[i] = str2.Substring(0, 0x3fd) + "...";
                }
            }
            return string.Format(CultureInfo.CurrentCulture, format, args);
        }

        public static string GetString(string name, out bool usedFallback)
        {
            usedFallback = false;
            return GetString(name);
        }

        private static CultureInfo Culture
        {
            get
            {
                return null;
            }
        }

        public static ResourceManager Resources
        {
            get
            {
                return GetLoader().resources;
            }
        }
    }
}

