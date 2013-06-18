namespace Microsoft.SqlServer.Server
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlTypes;
    using System.Globalization;

    internal class SmiMetaData
    {
        private static SmiMetaData[] __defaultValues = new SmiMetaData[] { 
            DefaultBigInt, DefaultBinary, DefaultBit, DefaultChar_NoCollation, DefaultDateTime, DefaultDecimal, DefaultFloat, DefaultImage, DefaultInt, DefaultMoney, DefaultNChar_NoCollation, DefaultNText_NoCollation, DefaultNVarChar_NoCollation, DefaultReal, DefaultUniqueIdentifier, DefaultSmallDateTime, 
            DefaultSmallInt, DefaultSmallMoney, DefaultText_NoCollation, DefaultTimestamp, DefaultTinyInt, DefaultVarBinary, DefaultVarChar_NoCollation, DefaultVariant, DefaultNVarChar_NoCollation, DefaultXml, DefaultNVarChar_NoCollation, DefaultNVarChar_NoCollation, DefaultNVarChar_NoCollation, DefaultUdt_NoType, DefaultStructured, DefaultDate, 
            DefaultTime, DefaultDateTime2, DefaultDateTimeOffset
         };
        private static readonly IList<SmiExtendedMetaData> __emptyFieldList = new List<SmiExtendedMetaData>().AsReadOnly();
        private static byte[] __maxLenFromPrecision = new byte[] { 
            5, 5, 5, 5, 5, 5, 5, 5, 5, 9, 9, 9, 9, 9, 9, 9, 
            9, 9, 9, 13, 13, 13, 13, 13, 13, 13, 13, 13, 0x11, 0x11, 0x11, 0x11, 
            0x11, 0x11, 0x11, 0x11, 0x11, 0x11
         };
        private static byte[] __maxVarTimeLenOffsetFromScale = new byte[] { 2, 2, 2, 1, 1, 0, 0, 0 };
        private static string[] __typeNameByDatabaseType;
        private System.Type _clrType;
        private SqlCompareOptions _compareOptions;
        private System.Data.SqlDbType _databaseType;
        private SmiMetaDataPropertyCollection _extendedProperties;
        private IList<SmiExtendedMetaData> _fieldMetaData;
        private bool _isMultiValued;
        private long _localeId;
        private long _maxLength;
        private byte _precision;
        private byte _scale;
        private string _udtAssemblyQualifiedName;
        internal static readonly SmiMetaData DefaultBigInt = new SmiMetaData(System.Data.SqlDbType.BigInt, 8L, 0x13, 0, SqlCompareOptions.None);
        internal static readonly SmiMetaData DefaultBinary = new SmiMetaData(System.Data.SqlDbType.Binary, 1L, 0, 0, SqlCompareOptions.None);
        internal static readonly SmiMetaData DefaultBit = new SmiMetaData(System.Data.SqlDbType.Bit, 1L, 1, 0, SqlCompareOptions.None);
        internal static readonly SmiMetaData DefaultChar_NoCollation = new SmiMetaData(System.Data.SqlDbType.Char, 1L, 0, 0, SqlCompareOptions.IgnoreWidth | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreCase);
        internal static readonly SmiMetaData DefaultDate = new SmiMetaData(System.Data.SqlDbType.Date, 3L, 10, 0, SqlCompareOptions.None);
        internal static readonly SmiMetaData DefaultDateTime = new SmiMetaData(System.Data.SqlDbType.DateTime, 8L, 0x17, 3, SqlCompareOptions.None);
        internal static readonly SmiMetaData DefaultDateTime2 = new SmiMetaData(System.Data.SqlDbType.DateTime2, 8L, 0, 7, SqlCompareOptions.None);
        internal static readonly SmiMetaData DefaultDateTimeOffset = new SmiMetaData(System.Data.SqlDbType.DateTimeOffset, 10L, 0, 7, SqlCompareOptions.None);
        internal static readonly SmiMetaData DefaultDecimal = new SmiMetaData(System.Data.SqlDbType.Decimal, 9L, 0x12, 0, SqlCompareOptions.None);
        internal static readonly SmiMetaData DefaultFloat = new SmiMetaData(System.Data.SqlDbType.Float, 8L, 0x35, 0, SqlCompareOptions.None);
        internal static readonly SmiMetaData DefaultImage = new SmiMetaData(System.Data.SqlDbType.Image, -1L, 0, 0, SqlCompareOptions.None);
        internal static readonly SmiMetaData DefaultInt = new SmiMetaData(System.Data.SqlDbType.Int, 4L, 10, 0, SqlCompareOptions.None);
        internal static readonly SmiMetaData DefaultMoney = new SmiMetaData(System.Data.SqlDbType.Money, 8L, 0x13, 4, SqlCompareOptions.None);
        internal static readonly SmiMetaData DefaultNChar_NoCollation = new SmiMetaData(System.Data.SqlDbType.NChar, 1L, 0, 0, SqlCompareOptions.IgnoreWidth | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreCase);
        internal static readonly SmiMetaData DefaultNText_NoCollation = new SmiMetaData(System.Data.SqlDbType.NText, -1L, 0, 0, SqlCompareOptions.IgnoreWidth | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreCase);
        internal static readonly SmiMetaData DefaultNVarChar_NoCollation = new SmiMetaData(System.Data.SqlDbType.NVarChar, 0xfa0L, 0, 0, SqlCompareOptions.IgnoreWidth | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreCase);
        internal static readonly SmiMetaData DefaultReal = new SmiMetaData(System.Data.SqlDbType.Real, 4L, 0x18, 0, SqlCompareOptions.None);
        internal static readonly SmiMetaData DefaultSmallDateTime = new SmiMetaData(System.Data.SqlDbType.SmallDateTime, 4L, 0x10, 0, SqlCompareOptions.None);
        internal static readonly SmiMetaData DefaultSmallInt = new SmiMetaData(System.Data.SqlDbType.SmallInt, 2L, 5, 0, SqlCompareOptions.None);
        internal static readonly SmiMetaData DefaultSmallMoney = new SmiMetaData(System.Data.SqlDbType.SmallMoney, 4L, 10, 4, SqlCompareOptions.None);
        internal const SqlCompareOptions DefaultStringCompareOptions = (SqlCompareOptions.IgnoreWidth | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreCase);
        internal static readonly SmiMetaData DefaultStructured = new SmiMetaData(System.Data.SqlDbType.Structured, 0L, 0, 0, SqlCompareOptions.None);
        internal static readonly SmiMetaData DefaultText_NoCollation = new SmiMetaData(System.Data.SqlDbType.Text, -1L, 0, 0, SqlCompareOptions.IgnoreWidth | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreCase);
        internal static readonly SmiMetaData DefaultTime = new SmiMetaData(System.Data.SqlDbType.Time, 5L, 0, 7, SqlCompareOptions.None);
        internal static readonly SmiMetaData DefaultTimestamp = new SmiMetaData(System.Data.SqlDbType.Timestamp, 8L, 0, 0, SqlCompareOptions.None);
        internal static readonly SmiMetaData DefaultTinyInt = new SmiMetaData(System.Data.SqlDbType.TinyInt, 1L, 3, 0, SqlCompareOptions.None);
        internal static readonly SmiMetaData DefaultUdt_NoType = new SmiMetaData(System.Data.SqlDbType.Udt, 0L, 0, 0, SqlCompareOptions.None);
        internal static readonly SmiMetaData DefaultUniqueIdentifier = new SmiMetaData(System.Data.SqlDbType.UniqueIdentifier, 0x10L, 0, 0, SqlCompareOptions.None);
        internal static readonly SmiMetaData DefaultVarBinary = new SmiMetaData(System.Data.SqlDbType.VarBinary, 0x1f40L, 0, 0, SqlCompareOptions.None);
        internal static readonly SmiMetaData DefaultVarChar_NoCollation = new SmiMetaData(System.Data.SqlDbType.VarChar, 0x1f40L, 0, 0, SqlCompareOptions.IgnoreWidth | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreCase);
        internal static readonly SmiMetaData DefaultVariant = new SmiMetaData(System.Data.SqlDbType.Variant, 0x1f50L, 0, 0, SqlCompareOptions.None);
        internal static readonly SmiMetaData DefaultXml = new SmiMetaData(System.Data.SqlDbType.Xml, -1L, 0, 0, SqlCompareOptions.IgnoreWidth | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreCase);
        internal const long MaxANSICharacters = 0x1f40L;
        internal const long MaxBinaryLength = 0x1f40L;
        internal const long MaxNameLength = 0x80L;
        internal static readonly DateTime MaxSmallDateTime = new DateTime(0x81f, 6, 6, 0x17, 0x3b, 0x1d, 0x3e6);
        internal static readonly SqlMoney MaxSmallMoney = new SqlMoney(214748.3647M);
        internal const int MaxTimeScale = 7;
        internal const long MaxUnicodeCharacters = 0xfa0L;
        internal const int MinPrecision = 1;
        internal const int MinScale = 0;
        internal static readonly DateTime MinSmallDateTime = new DateTime(0x76b, 12, 0x1f, 0x17, 0x3b, 0x1d, 0x3e7);
        internal static readonly SqlMoney MinSmallMoney = new SqlMoney(-214748.3648M);
        internal const long UnlimitedMaxLengthIndicator = -1L;

        static SmiMetaData()
        {
            string[] strArray = new string[0x23];
            strArray[0] = "bigint";
            strArray[1] = "binary";
            strArray[2] = "bit";
            strArray[3] = "char";
            strArray[4] = "datetime";
            strArray[5] = "decimal";
            strArray[6] = "float";
            strArray[7] = "image";
            strArray[8] = "int";
            strArray[9] = "money";
            strArray[10] = "nchar";
            strArray[11] = "ntext";
            strArray[12] = "nvarchar";
            strArray[13] = "real";
            strArray[14] = "uniqueidentifier";
            strArray[15] = "smalldatetime";
            strArray[0x10] = "smallint";
            strArray[0x11] = "smallmoney";
            strArray[0x12] = "text";
            strArray[0x13] = "timestamp";
            strArray[20] = "tinyint";
            strArray[0x15] = "varbinary";
            strArray[0x16] = "varchar";
            strArray[0x17] = "sql_variant";
            strArray[0x19] = "xml";
            strArray[0x1d] = string.Empty;
            strArray[30] = string.Empty;
            strArray[0x1f] = "date";
            strArray[0x20] = "time";
            strArray[0x21] = "datetime2";
            strArray[0x22] = "datetimeoffset";
            __typeNameByDatabaseType = strArray;
        }

        private SmiMetaData(System.Data.SqlDbType sqlDbType, long maxLength, byte precision, byte scale, SqlCompareOptions compareOptions)
        {
            this._databaseType = sqlDbType;
            this._maxLength = maxLength;
            this._precision = precision;
            this._scale = scale;
            this._compareOptions = compareOptions;
            this._localeId = 0L;
            this._clrType = null;
            this._isMultiValued = false;
            this._fieldMetaData = __emptyFieldList;
            this._extendedProperties = SmiMetaDataPropertyCollection.EmptyInstance;
        }

        internal SmiMetaData(System.Data.SqlDbType dbType, long maxLength, byte precision, byte scale, long localeId, SqlCompareOptions compareOptions, System.Type userDefinedType) : this(dbType, maxLength, precision, scale, localeId, compareOptions, userDefinedType, false, null, null)
        {
        }

        [Obsolete("Not supported as of SMI v2.  Will be removed when v1 support dropped. Use ctor without columns param.")]
        internal SmiMetaData(System.Data.SqlDbType dbType, long maxLength, byte precision, byte scale, long localeId, SqlCompareOptions compareOptions, System.Type userDefinedType, SmiMetaData[] columns) : this(dbType, maxLength, precision, scale, localeId, compareOptions, userDefinedType)
        {
        }

        internal SmiMetaData(System.Data.SqlDbType dbType, long maxLength, byte precision, byte scale, long localeId, SqlCompareOptions compareOptions, System.Type userDefinedType, bool isMultiValued, IList<SmiExtendedMetaData> fieldTypes, SmiMetaDataPropertyCollection extendedProperties) : this(dbType, maxLength, precision, scale, localeId, compareOptions, userDefinedType, null, isMultiValued, fieldTypes, extendedProperties)
        {
        }

        internal SmiMetaData(System.Data.SqlDbType dbType, long maxLength, byte precision, byte scale, long localeId, SqlCompareOptions compareOptions, System.Type userDefinedType, string udtAssemblyQualifiedName, bool isMultiValued, IList<SmiExtendedMetaData> fieldTypes, SmiMetaDataPropertyCollection extendedProperties)
        {
            this.SetDefaultsForType(dbType);
            switch (dbType)
            {
                case System.Data.SqlDbType.Binary:
                case System.Data.SqlDbType.VarBinary:
                    this._maxLength = maxLength;
                    goto Label_01BF;

                case System.Data.SqlDbType.Char:
                case System.Data.SqlDbType.NChar:
                case System.Data.SqlDbType.NVarChar:
                case System.Data.SqlDbType.VarChar:
                    this._maxLength = maxLength;
                    this._localeId = localeId;
                    this._compareOptions = compareOptions;
                    goto Label_01BF;

                case System.Data.SqlDbType.Decimal:
                    this._precision = precision;
                    this._scale = scale;
                    this._maxLength = __maxLenFromPrecision[precision - 1];
                    goto Label_01BF;

                case System.Data.SqlDbType.NText:
                case System.Data.SqlDbType.Text:
                    this._localeId = localeId;
                    this._compareOptions = compareOptions;
                    goto Label_01BF;

                case System.Data.SqlDbType.Udt:
                    this._clrType = userDefinedType;
                    if (null == userDefinedType)
                    {
                        this._maxLength = maxLength;
                        break;
                    }
                    this._maxLength = SerializationHelperSql9.GetUdtMaxLength(userDefinedType);
                    break;

                case System.Data.SqlDbType.Structured:
                    if (fieldTypes != null)
                    {
                        this._fieldMetaData = new List<SmiExtendedMetaData>(fieldTypes).AsReadOnly();
                    }
                    this._isMultiValued = isMultiValued;
                    this._maxLength = this._fieldMetaData.Count;
                    goto Label_01BF;

                case System.Data.SqlDbType.Time:
                    this._scale = scale;
                    this._maxLength = 5 - __maxVarTimeLenOffsetFromScale[scale];
                    goto Label_01BF;

                case System.Data.SqlDbType.DateTime2:
                    this._scale = scale;
                    this._maxLength = 8 - __maxVarTimeLenOffsetFromScale[scale];
                    goto Label_01BF;

                case System.Data.SqlDbType.DateTimeOffset:
                    this._scale = scale;
                    this._maxLength = 10 - __maxVarTimeLenOffsetFromScale[scale];
                    goto Label_01BF;

                default:
                    goto Label_01BF;
            }
            this._udtAssemblyQualifiedName = udtAssemblyQualifiedName;
        Label_01BF:
            if (extendedProperties != null)
            {
                extendedProperties.SetReadOnly();
                this._extendedProperties = extendedProperties;
            }
        }

        internal static SmiMetaData GetDefaultForType(System.Data.SqlDbType dbType)
        {
            return __defaultValues[(int) dbType];
        }

        internal static bool IsSupportedDbType(System.Data.SqlDbType dbType)
        {
            return (((System.Data.SqlDbType.BigInt <= dbType) && (System.Data.SqlDbType.Xml >= dbType)) || ((System.Data.SqlDbType.Udt <= dbType) && (System.Data.SqlDbType.DateTimeOffset >= dbType)));
        }

        internal bool IsValidMaxLengthForCtorGivenType(System.Data.SqlDbType dbType, long maxLength)
        {
            bool flag = true;
            switch (dbType)
            {
                case System.Data.SqlDbType.BigInt:
                case System.Data.SqlDbType.Bit:
                case System.Data.SqlDbType.DateTime:
                case System.Data.SqlDbType.Decimal:
                case System.Data.SqlDbType.Float:
                case System.Data.SqlDbType.Image:
                case System.Data.SqlDbType.Int:
                case System.Data.SqlDbType.Money:
                case System.Data.SqlDbType.NText:
                case System.Data.SqlDbType.Real:
                case System.Data.SqlDbType.UniqueIdentifier:
                case System.Data.SqlDbType.SmallDateTime:
                case System.Data.SqlDbType.SmallInt:
                case System.Data.SqlDbType.SmallMoney:
                case System.Data.SqlDbType.Text:
                case System.Data.SqlDbType.Timestamp:
                case System.Data.SqlDbType.TinyInt:
                case System.Data.SqlDbType.Variant:
                case (System.Data.SqlDbType.SmallInt | System.Data.SqlDbType.Int):
                case System.Data.SqlDbType.Xml:
                case (System.Data.SqlDbType.Text | System.Data.SqlDbType.Int):
                case (System.Data.SqlDbType.Xml | System.Data.SqlDbType.Bit):
                case (System.Data.SqlDbType.TinyInt | System.Data.SqlDbType.Int):
                case System.Data.SqlDbType.Udt:
                case System.Data.SqlDbType.Structured:
                case System.Data.SqlDbType.Date:
                case System.Data.SqlDbType.Time:
                case System.Data.SqlDbType.DateTime2:
                case System.Data.SqlDbType.DateTimeOffset:
                    return flag;

                case System.Data.SqlDbType.Binary:
                    return ((0L < maxLength) && (0x1f40L >= maxLength));

                case System.Data.SqlDbType.Char:
                    return ((0L < maxLength) && (0x1f40L >= maxLength));

                case System.Data.SqlDbType.NChar:
                    return ((0L < maxLength) && (0xfa0L >= maxLength));

                case System.Data.SqlDbType.NVarChar:
                    return ((-1L == maxLength) || ((0L < maxLength) && (0xfa0L >= maxLength)));

                case System.Data.SqlDbType.VarBinary:
                    return ((-1L == maxLength) || ((0L < maxLength) && (0x1f40L >= maxLength)));

                case System.Data.SqlDbType.VarChar:
                    return ((-1L == maxLength) || ((0L < maxLength) && (0x1f40L >= maxLength)));
            }
            return flag;
        }

        private void SetDefaultsForType(System.Data.SqlDbType dbType)
        {
            SmiMetaData defaultForType = GetDefaultForType(dbType);
            this._databaseType = dbType;
            this._maxLength = defaultForType.MaxLength;
            this._precision = defaultForType.Precision;
            this._scale = defaultForType.Scale;
            this._localeId = defaultForType.LocaleId;
            this._compareOptions = defaultForType.CompareOptions;
            this._clrType = null;
            this._isMultiValued = defaultForType._isMultiValued;
            this._fieldMetaData = defaultForType._fieldMetaData;
            this._extendedProperties = defaultForType._extendedProperties;
        }

        internal string TraceString()
        {
            return this.TraceString(0);
        }

        internal virtual string TraceString(int indent)
        {
            string str3 = new string(' ', indent);
            string str2 = string.Empty;
            if (this._fieldMetaData != null)
            {
                foreach (SmiMetaData data in this._fieldMetaData)
                {
                    str2 = string.Format(CultureInfo.InvariantCulture, "{0}{1}\n\t", new object[] { str2, data.TraceString(indent + 5) });
                }
            }
            string str = string.Empty;
            if (this._extendedProperties != null)
            {
                foreach (SmiMetaDataProperty property in this._extendedProperties.Values)
                {
                    str = string.Format(CultureInfo.InvariantCulture, "{0}{1}                   {2}\n\t", new object[] { str, str3, property.TraceString() });
                }
            }
            return string.Format(CultureInfo.InvariantCulture, "\n\t{0}            SqlDbType={1:g}\n\t{0}            MaxLength={2:d}\n\t{0}            Precision={3:d}\n\t{0}                Scale={4:d}\n\t{0}             LocaleId={5:x}\n\t{0}       CompareOptions={6:g}\n\t{0}                 Type={7}\n\t{0}          MultiValued={8}\n\t{0}               fields=\n\t{9}{0}           properties=\n\t{10}", new object[] { str3, this.SqlDbType, this.MaxLength, this.Precision, this.Scale, this.LocaleId, this.CompareOptions, (null != this.Type) ? this.Type.ToString() : "<null>", this.IsMultiValued, str2, str });
        }

        internal string AssemblyQualifiedName
        {
            get
            {
                string str = null;
                if (System.Data.SqlDbType.Udt != this._databaseType)
                {
                    return str;
                }
                if ((this._udtAssemblyQualifiedName == null) && (this._clrType != null))
                {
                    this._udtAssemblyQualifiedName = this._clrType.AssemblyQualifiedName;
                }
                return this._udtAssemblyQualifiedName;
            }
        }

        internal SqlCompareOptions CompareOptions
        {
            get
            {
                return this._compareOptions;
            }
        }

        internal static SmiMetaData DefaultChar
        {
            get
            {
                return new SmiMetaData(DefaultChar_NoCollation.SqlDbType, DefaultChar_NoCollation.MaxLength, DefaultChar_NoCollation.Precision, DefaultChar_NoCollation.Scale, (long) CultureInfo.CurrentCulture.LCID, SqlCompareOptions.IgnoreWidth | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreCase, null);
            }
        }

        internal static SmiMetaData DefaultNChar
        {
            get
            {
                return new SmiMetaData(DefaultNChar_NoCollation.SqlDbType, DefaultNChar_NoCollation.MaxLength, DefaultNChar_NoCollation.Precision, DefaultNChar_NoCollation.Scale, (long) CultureInfo.CurrentCulture.LCID, SqlCompareOptions.IgnoreWidth | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreCase, null);
            }
        }

        internal static SmiMetaData DefaultNText
        {
            get
            {
                return new SmiMetaData(DefaultNText_NoCollation.SqlDbType, DefaultNText_NoCollation.MaxLength, DefaultNText_NoCollation.Precision, DefaultNText_NoCollation.Scale, (long) CultureInfo.CurrentCulture.LCID, SqlCompareOptions.IgnoreWidth | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreCase, null);
            }
        }

        internal static SmiMetaData DefaultNVarChar
        {
            get
            {
                return new SmiMetaData(DefaultNVarChar_NoCollation.SqlDbType, DefaultNVarChar_NoCollation.MaxLength, DefaultNVarChar_NoCollation.Precision, DefaultNVarChar_NoCollation.Scale, (long) CultureInfo.CurrentCulture.LCID, SqlCompareOptions.IgnoreWidth | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreCase, null);
            }
        }

        internal static SmiMetaData DefaultText
        {
            get
            {
                return new SmiMetaData(DefaultText_NoCollation.SqlDbType, DefaultText_NoCollation.MaxLength, DefaultText_NoCollation.Precision, DefaultText_NoCollation.Scale, (long) CultureInfo.CurrentCulture.LCID, SqlCompareOptions.IgnoreWidth | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreCase, null);
            }
        }

        internal static SmiMetaData DefaultVarChar
        {
            get
            {
                return new SmiMetaData(DefaultVarChar_NoCollation.SqlDbType, DefaultVarChar_NoCollation.MaxLength, DefaultVarChar_NoCollation.Precision, DefaultVarChar_NoCollation.Scale, (long) CultureInfo.CurrentCulture.LCID, SqlCompareOptions.IgnoreWidth | SqlCompareOptions.IgnoreKanaType | SqlCompareOptions.IgnoreCase, null);
            }
        }

        internal SmiMetaDataPropertyCollection ExtendedProperties
        {
            get
            {
                return this._extendedProperties;
            }
        }

        internal IList<SmiExtendedMetaData> FieldMetaData
        {
            get
            {
                return this._fieldMetaData;
            }
        }

        internal bool IsMultiValued
        {
            get
            {
                return this._isMultiValued;
            }
        }

        internal long LocaleId
        {
            get
            {
                return this._localeId;
            }
        }

        internal long MaxLength
        {
            get
            {
                return this._maxLength;
            }
        }

        internal byte Precision
        {
            get
            {
                return this._precision;
            }
        }

        internal byte Scale
        {
            get
            {
                return this._scale;
            }
        }

        internal System.Data.SqlDbType SqlDbType
        {
            get
            {
                return this._databaseType;
            }
        }

        internal System.Type Type
        {
            get
            {
                if (((null == this._clrType) && (System.Data.SqlDbType.Udt == this._databaseType)) && (this._udtAssemblyQualifiedName != null))
                {
                    this._clrType = System.Type.GetType(this._udtAssemblyQualifiedName, true);
                }
                return this._clrType;
            }
        }

        internal string TypeName
        {
            get
            {
                if (System.Data.SqlDbType.Udt == this._databaseType)
                {
                    return this.Type.FullName;
                }
                return __typeNameByDatabaseType[(int) this._databaseType];
            }
        }

        internal System.Type TypeWithoutThrowing
        {
            get
            {
                if (((null == this._clrType) && (System.Data.SqlDbType.Udt == this._databaseType)) && (this._udtAssemblyQualifiedName != null))
                {
                    this._clrType = System.Type.GetType(this._udtAssemblyQualifiedName, false);
                }
                return this._clrType;
            }
        }
    }
}

