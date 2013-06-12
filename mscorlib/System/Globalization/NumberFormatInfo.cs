namespace System.Globalization
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Threading;

    [Serializable, ComVisible(true)]
    public sealed class NumberFormatInfo : ICloneable, IFormatProvider
    {
        internal string ansiCurrencySymbol;
        internal int currencyDecimalDigits;
        internal string currencyDecimalSeparator;
        internal string currencyGroupSeparator;
        internal int[] currencyGroupSizes;
        internal int currencyNegativePattern;
        internal int currencyPositivePattern;
        internal string currencySymbol;
        [OptionalField(VersionAdded=2)]
        internal int digitSubstitution;
        private const NumberStyles InvalidNumberStyles = ~(NumberStyles.HexNumber | NumberStyles.AllowCurrencySymbol | NumberStyles.AllowExponent | NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint | NumberStyles.AllowParentheses | NumberStyles.AllowTrailingSign | NumberStyles.AllowLeadingSign);
        private static NumberFormatInfo invariantInfo;
        internal bool isReadOnly;
        [OptionalField(VersionAdded=1)]
        internal int m_dataItem;
        [OptionalField(VersionAdded=2)]
        internal bool m_isInvariant;
        [OptionalField(VersionAdded=1)]
        internal bool m_useUserOverride;
        internal string nanSymbol;
        [OptionalField(VersionAdded=2)]
        internal string[] nativeDigits;
        internal string negativeInfinitySymbol;
        internal string negativeSign;
        internal int numberDecimalDigits;
        internal string numberDecimalSeparator;
        internal string numberGroupSeparator;
        internal int[] numberGroupSizes;
        internal int numberNegativePattern;
        internal int percentDecimalDigits;
        internal string percentDecimalSeparator;
        internal string percentGroupSeparator;
        internal int[] percentGroupSizes;
        internal int percentNegativePattern;
        internal int percentPositivePattern;
        internal string percentSymbol;
        internal string perMilleSymbol;
        internal string positiveInfinitySymbol;
        internal string positiveSign;
        [OptionalField(VersionAdded=1)]
        internal bool validForParseAsCurrency;
        [OptionalField(VersionAdded=1)]
        internal bool validForParseAsNumber;

        [SecuritySafeCritical]
        public NumberFormatInfo() : this(null)
        {
        }

        [SecuritySafeCritical]
        internal NumberFormatInfo(CultureData cultureData)
        {
            this.numberGroupSizes = new int[] { 3 };
            this.currencyGroupSizes = new int[] { 3 };
            this.percentGroupSizes = new int[] { 3 };
            this.positiveSign = "+";
            this.negativeSign = "-";
            this.numberDecimalSeparator = ".";
            this.numberGroupSeparator = ",";
            this.currencyGroupSeparator = ",";
            this.currencyDecimalSeparator = ".";
            this.currencySymbol = "\x00a4";
            this.nanSymbol = "NaN";
            this.positiveInfinitySymbol = "Infinity";
            this.negativeInfinitySymbol = "-Infinity";
            this.percentDecimalSeparator = ".";
            this.percentGroupSeparator = ",";
            this.percentSymbol = "%";
            this.perMilleSymbol = "‰";
            this.nativeDigits = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            this.numberDecimalDigits = 2;
            this.currencyDecimalDigits = 2;
            this.numberNegativePattern = 1;
            this.percentDecimalDigits = 2;
            this.digitSubstitution = 1;
            this.validForParseAsNumber = true;
            this.validForParseAsCurrency = true;
            if (cultureData != null)
            {
                cultureData.GetNFIValues(this);
                if (cultureData.IsInvariantCulture)
                {
                    this.m_isInvariant = true;
                }
            }
        }

        internal static void CheckGroupSize(string propName, int[] groupSize)
        {
            for (int i = 0; i < groupSize.Length; i++)
            {
                if (groupSize[i] < 1)
                {
                    if ((i != (groupSize.Length - 1)) || (groupSize[i] != 0))
                    {
                        throw new ArgumentException(propName, Environment.GetResourceString("Argument_InvalidGroupSize"));
                    }
                    return;
                }
                if (groupSize[i] > 9)
                {
                    throw new ArgumentException(propName, Environment.GetResourceString("Argument_InvalidGroupSize"));
                }
            }
        }

        [SecuritySafeCritical]
        public object Clone()
        {
            NumberFormatInfo info = (NumberFormatInfo) base.MemberwiseClone();
            info.isReadOnly = false;
            return info;
        }

        public object GetFormat(Type formatType)
        {
            if (!(formatType == typeof(NumberFormatInfo)))
            {
                return null;
            }
            return this;
        }

        [SecuritySafeCritical]
        public static NumberFormatInfo GetInstance(IFormatProvider formatProvider)
        {
            NumberFormatInfo numInfo;
            CultureInfo info2 = formatProvider as CultureInfo;
            if ((info2 != null) && !info2.m_isInherited)
            {
                numInfo = info2.numInfo;
                if (numInfo != null)
                {
                    return numInfo;
                }
                return info2.NumberFormat;
            }
            numInfo = formatProvider as NumberFormatInfo;
            if (numInfo != null)
            {
                return numInfo;
            }
            if (formatProvider != null)
            {
                numInfo = formatProvider.GetFormat(typeof(NumberFormatInfo)) as NumberFormatInfo;
                if (numInfo != null)
                {
                    return numInfo;
                }
            }
            return CurrentInfo;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext ctx)
        {
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext ctx)
        {
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext ctx)
        {
            if (this.numberDecimalSeparator != this.numberGroupSeparator)
            {
                this.validForParseAsNumber = true;
            }
            else
            {
                this.validForParseAsNumber = false;
            }
            if (((this.numberDecimalSeparator != this.numberGroupSeparator) && (this.numberDecimalSeparator != this.currencyGroupSeparator)) && ((this.currencyDecimalSeparator != this.numberGroupSeparator) && (this.currencyDecimalSeparator != this.currencyGroupSeparator)))
            {
                this.validForParseAsCurrency = true;
            }
            else
            {
                this.validForParseAsCurrency = false;
            }
        }

        [SecuritySafeCritical]
        public static NumberFormatInfo ReadOnly(NumberFormatInfo nfi)
        {
            if (nfi == null)
            {
                throw new ArgumentNullException("nfi");
            }
            if (nfi.IsReadOnly)
            {
                return nfi;
            }
            NumberFormatInfo info = (NumberFormatInfo) nfi.MemberwiseClone();
            info.isReadOnly = true;
            return info;
        }

        internal static void ValidateParseStyleFloatingPoint(NumberStyles style)
        {
            if ((style & ~(NumberStyles.HexNumber | NumberStyles.AllowCurrencySymbol | NumberStyles.AllowExponent | NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint | NumberStyles.AllowParentheses | NumberStyles.AllowTrailingSign | NumberStyles.AllowLeadingSign)) != NumberStyles.None)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidNumberStyles"), "style");
            }
            if ((style & NumberStyles.AllowHexSpecifier) != NumberStyles.None)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_HexStyleNotSupported"));
            }
        }

        internal static void ValidateParseStyleInteger(NumberStyles style)
        {
            if ((style & ~(NumberStyles.HexNumber | NumberStyles.AllowCurrencySymbol | NumberStyles.AllowExponent | NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint | NumberStyles.AllowParentheses | NumberStyles.AllowTrailingSign | NumberStyles.AllowLeadingSign)) != NumberStyles.None)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidNumberStyles"), "style");
            }
            if (((style & NumberStyles.AllowHexSpecifier) != NumberStyles.None) && ((style & ~NumberStyles.HexNumber) != NumberStyles.None))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_InvalidHexStyle"));
            }
        }

        private static void VerifyDecimalSeparator(string decSep, string propertyName)
        {
            if (decSep == null)
            {
                throw new ArgumentNullException(propertyName, Environment.GetResourceString("ArgumentNull_String"));
            }
            if (decSep.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyDecString"));
            }
        }

        private static void VerifyDigitSubstitution(DigitShapes digitSub, string propertyName)
        {
            switch (digitSub)
            {
                case DigitShapes.Context:
                case DigitShapes.None:
                case DigitShapes.NativeNational:
                    return;
            }
            throw new ArgumentException(propertyName, Environment.GetResourceString("Argument_InvalidDigitSubstitution"));
        }

        private static void VerifyGroupSeparator(string groupSep, string propertyName)
        {
            if (groupSep == null)
            {
                throw new ArgumentNullException(propertyName, Environment.GetResourceString("ArgumentNull_String"));
            }
        }

        private static void VerifyNativeDigits(string[] nativeDig, string propertyName)
        {
            if (nativeDig == null)
            {
                throw new ArgumentNullException(propertyName, Environment.GetResourceString("ArgumentNull_Array"));
            }
            if (nativeDig.Length != 10)
            {
                throw new ArgumentException(propertyName, Environment.GetResourceString("Argument_InvalidNativeDigitCount"));
            }
            for (int i = 0; i < nativeDig.Length; i++)
            {
                if (nativeDig[i] == null)
                {
                    throw new ArgumentNullException(propertyName, Environment.GetResourceString("ArgumentNull_ArrayValue"));
                }
                if (nativeDig[i].Length != 1)
                {
                    if (nativeDig[i].Length != 2)
                    {
                        throw new ArgumentException(propertyName, Environment.GetResourceString("Argument_InvalidNativeDigitValue"));
                    }
                    if (!char.IsSurrogatePair(nativeDig[i][0], nativeDig[i][1]))
                    {
                        throw new ArgumentException(propertyName, Environment.GetResourceString("Argument_InvalidNativeDigitValue"));
                    }
                }
                if ((CharUnicodeInfo.GetDecimalDigitValue(nativeDig[i], 0) != i) && (CharUnicodeInfo.GetUnicodeCategory(nativeDig[i], 0) != UnicodeCategory.PrivateUse))
                {
                    throw new ArgumentException(propertyName, Environment.GetResourceString("Argument_InvalidNativeDigitValue"));
                }
            }
        }

        private void VerifyWritable()
        {
            if (this.isReadOnly)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ReadOnly"));
            }
        }

        public int CurrencyDecimalDigits
        {
            get
            {
                return this.currencyDecimalDigits;
            }
            set
            {
                if ((value < 0) || (value > 0x63))
                {
                    throw new ArgumentOutOfRangeException("CurrencyDecimalDigits", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { 0, 0x63 }));
                }
                this.VerifyWritable();
                this.currencyDecimalDigits = value;
            }
        }

        public string CurrencyDecimalSeparator
        {
            get
            {
                return this.currencyDecimalSeparator;
            }
            set
            {
                this.VerifyWritable();
                VerifyDecimalSeparator(value, "CurrencyDecimalSeparator");
                this.currencyDecimalSeparator = value;
            }
        }

        public string CurrencyGroupSeparator
        {
            get
            {
                return this.currencyGroupSeparator;
            }
            set
            {
                this.VerifyWritable();
                VerifyGroupSeparator(value, "CurrencyGroupSeparator");
                this.currencyGroupSeparator = value;
            }
        }

        public int[] CurrencyGroupSizes
        {
            get
            {
                return (int[]) this.currencyGroupSizes.Clone();
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("CurrencyGroupSizes", Environment.GetResourceString("ArgumentNull_Obj"));
                }
                this.VerifyWritable();
                int[] groupSize = (int[]) value.Clone();
                CheckGroupSize("CurrencyGroupSizes", groupSize);
                this.currencyGroupSizes = groupSize;
            }
        }

        public int CurrencyNegativePattern
        {
            get
            {
                return this.currencyNegativePattern;
            }
            set
            {
                if ((value < 0) || (value > 15))
                {
                    throw new ArgumentOutOfRangeException("CurrencyNegativePattern", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { 0, 15 }));
                }
                this.VerifyWritable();
                this.currencyNegativePattern = value;
            }
        }

        public int CurrencyPositivePattern
        {
            get
            {
                return this.currencyPositivePattern;
            }
            set
            {
                if ((value < 0) || (value > 3))
                {
                    throw new ArgumentOutOfRangeException("CurrencyPositivePattern", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { 0, 3 }));
                }
                this.VerifyWritable();
                this.currencyPositivePattern = value;
            }
        }

        public string CurrencySymbol
        {
            get
            {
                return this.currencySymbol;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("CurrencySymbol", Environment.GetResourceString("ArgumentNull_String"));
                }
                this.VerifyWritable();
                this.currencySymbol = value;
            }
        }

        public static NumberFormatInfo CurrentInfo
        {
            [SecuritySafeCritical]
            get
            {
                CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
                if (!currentCulture.m_isInherited)
                {
                    NumberFormatInfo numInfo = currentCulture.numInfo;
                    if (numInfo != null)
                    {
                        return numInfo;
                    }
                }
                return (NumberFormatInfo) currentCulture.GetFormat(typeof(NumberFormatInfo));
            }
        }

        [ComVisible(false)]
        public DigitShapes DigitSubstitution
        {
            get
            {
                return (DigitShapes) this.digitSubstitution;
            }
            set
            {
                this.VerifyWritable();
                VerifyDigitSubstitution(value, "DigitSubstitution");
                this.digitSubstitution = (int) value;
            }
        }

        public static NumberFormatInfo InvariantInfo
        {
            get
            {
                if (invariantInfo == null)
                {
                    NumberFormatInfo nfi = new NumberFormatInfo {
                        m_isInvariant = true
                    };
                    invariantInfo = ReadOnly(nfi);
                }
                return invariantInfo;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return this.isReadOnly;
            }
        }

        public string NaNSymbol
        {
            get
            {
                return this.nanSymbol;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("NaNSymbol", Environment.GetResourceString("ArgumentNull_String"));
                }
                this.VerifyWritable();
                this.nanSymbol = value;
            }
        }

        [ComVisible(false)]
        public string[] NativeDigits
        {
            get
            {
                return (string[]) this.nativeDigits.Clone();
            }
            set
            {
                this.VerifyWritable();
                VerifyNativeDigits(value, "NativeDigits");
                this.nativeDigits = value;
            }
        }

        public string NegativeInfinitySymbol
        {
            get
            {
                return this.negativeInfinitySymbol;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("NegativeInfinitySymbol", Environment.GetResourceString("ArgumentNull_String"));
                }
                this.VerifyWritable();
                this.negativeInfinitySymbol = value;
            }
        }

        public string NegativeSign
        {
            get
            {
                return this.negativeSign;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("NegativeSign", Environment.GetResourceString("ArgumentNull_String"));
                }
                this.VerifyWritable();
                this.negativeSign = value;
            }
        }

        public int NumberDecimalDigits
        {
            get
            {
                return this.numberDecimalDigits;
            }
            set
            {
                if ((value < 0) || (value > 0x63))
                {
                    throw new ArgumentOutOfRangeException("NumberDecimalDigits", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { 0, 0x63 }));
                }
                this.VerifyWritable();
                this.numberDecimalDigits = value;
            }
        }

        public string NumberDecimalSeparator
        {
            get
            {
                return this.numberDecimalSeparator;
            }
            set
            {
                this.VerifyWritable();
                VerifyDecimalSeparator(value, "NumberDecimalSeparator");
                this.numberDecimalSeparator = value;
            }
        }

        public string NumberGroupSeparator
        {
            get
            {
                return this.numberGroupSeparator;
            }
            set
            {
                this.VerifyWritable();
                VerifyGroupSeparator(value, "NumberGroupSeparator");
                this.numberGroupSeparator = value;
            }
        }

        public int[] NumberGroupSizes
        {
            get
            {
                return (int[]) this.numberGroupSizes.Clone();
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("NumberGroupSizes", Environment.GetResourceString("ArgumentNull_Obj"));
                }
                this.VerifyWritable();
                int[] groupSize = (int[]) value.Clone();
                CheckGroupSize("NumberGroupSizes", groupSize);
                this.numberGroupSizes = groupSize;
            }
        }

        public int NumberNegativePattern
        {
            get
            {
                return this.numberNegativePattern;
            }
            set
            {
                if ((value < 0) || (value > 4))
                {
                    throw new ArgumentOutOfRangeException("NumberNegativePattern", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { 0, 4 }));
                }
                this.VerifyWritable();
                this.numberNegativePattern = value;
            }
        }

        public int PercentDecimalDigits
        {
            get
            {
                return this.percentDecimalDigits;
            }
            set
            {
                if ((value < 0) || (value > 0x63))
                {
                    throw new ArgumentOutOfRangeException("PercentDecimalDigits", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { 0, 0x63 }));
                }
                this.VerifyWritable();
                this.percentDecimalDigits = value;
            }
        }

        public string PercentDecimalSeparator
        {
            get
            {
                return this.percentDecimalSeparator;
            }
            set
            {
                this.VerifyWritable();
                VerifyDecimalSeparator(value, "PercentDecimalSeparator");
                this.percentDecimalSeparator = value;
            }
        }

        public string PercentGroupSeparator
        {
            get
            {
                return this.percentGroupSeparator;
            }
            set
            {
                this.VerifyWritable();
                VerifyGroupSeparator(value, "PercentGroupSeparator");
                this.percentGroupSeparator = value;
            }
        }

        public int[] PercentGroupSizes
        {
            get
            {
                return (int[]) this.percentGroupSizes.Clone();
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("PercentGroupSizes", Environment.GetResourceString("ArgumentNull_Obj"));
                }
                this.VerifyWritable();
                int[] groupSize = (int[]) value.Clone();
                CheckGroupSize("PercentGroupSizes", groupSize);
                this.percentGroupSizes = groupSize;
            }
        }

        public int PercentNegativePattern
        {
            get
            {
                return this.percentNegativePattern;
            }
            set
            {
                if ((value < 0) || (value > 11))
                {
                    throw new ArgumentOutOfRangeException("PercentNegativePattern", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { 0, 11 }));
                }
                this.VerifyWritable();
                this.percentNegativePattern = value;
            }
        }

        public int PercentPositivePattern
        {
            get
            {
                return this.percentPositivePattern;
            }
            set
            {
                if ((value < 0) || (value > 3))
                {
                    throw new ArgumentOutOfRangeException("PercentPositivePattern", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { 0, 3 }));
                }
                this.VerifyWritable();
                this.percentPositivePattern = value;
            }
        }

        public string PercentSymbol
        {
            get
            {
                return this.percentSymbol;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("PercentSymbol", Environment.GetResourceString("ArgumentNull_String"));
                }
                this.VerifyWritable();
                this.percentSymbol = value;
            }
        }

        public string PerMilleSymbol
        {
            get
            {
                return this.perMilleSymbol;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("PerMilleSymbol", Environment.GetResourceString("ArgumentNull_String"));
                }
                this.VerifyWritable();
                this.perMilleSymbol = value;
            }
        }

        public string PositiveInfinitySymbol
        {
            get
            {
                return this.positiveInfinitySymbol;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("PositiveInfinitySymbol", Environment.GetResourceString("ArgumentNull_String"));
                }
                this.VerifyWritable();
                this.positiveInfinitySymbol = value;
            }
        }

        public string PositiveSign
        {
            get
            {
                return this.positiveSign;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("PositiveSign", Environment.GetResourceString("ArgumentNull_String"));
                }
                this.VerifyWritable();
                this.positiveSign = value;
            }
        }
    }
}

