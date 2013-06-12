namespace Microsoft.SqlServer.Server
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlTypes;
    using System.Globalization;

    internal class SmiExtendedMetaData : SmiMetaData
    {
        private string _name;
        private string _typeSpecificNamePart1;
        private string _typeSpecificNamePart2;
        private string _typeSpecificNamePart3;

        internal SmiExtendedMetaData(SqlDbType dbType, long maxLength, byte precision, byte scale, long localeId, SqlCompareOptions compareOptions, Type userDefinedType, string name, string typeSpecificNamePart1, string typeSpecificNamePart2, string typeSpecificNamePart3) : this(dbType, maxLength, precision, scale, localeId, compareOptions, userDefinedType, false, null, null, name, typeSpecificNamePart1, typeSpecificNamePart2, typeSpecificNamePart3)
        {
        }

        [Obsolete("Not supported as of SMI v2.  Will be removed when v1 support dropped. Use ctor without columns param.")]
        internal SmiExtendedMetaData(SqlDbType dbType, long maxLength, byte precision, byte scale, long localeId, SqlCompareOptions compareOptions, Type userDefinedType, SmiMetaData[] columns, string name, string typeSpecificNamePart1, string typeSpecificNamePart2, string typeSpecificNamePart3) : this(dbType, maxLength, precision, scale, localeId, compareOptions, userDefinedType, name, typeSpecificNamePart1, typeSpecificNamePart2, typeSpecificNamePart3)
        {
        }

        internal SmiExtendedMetaData(SqlDbType dbType, long maxLength, byte precision, byte scale, long localeId, SqlCompareOptions compareOptions, Type userDefinedType, bool isMultiValued, IList<SmiExtendedMetaData> fieldMetaData, SmiMetaDataPropertyCollection extendedProperties, string name, string typeSpecificNamePart1, string typeSpecificNamePart2, string typeSpecificNamePart3) : this(dbType, maxLength, precision, scale, localeId, compareOptions, userDefinedType, null, isMultiValued, fieldMetaData, extendedProperties, name, typeSpecificNamePart1, typeSpecificNamePart2, typeSpecificNamePart3)
        {
        }

        internal SmiExtendedMetaData(SqlDbType dbType, long maxLength, byte precision, byte scale, long localeId, SqlCompareOptions compareOptions, Type userDefinedType, string udtAssemblyQualifiedName, bool isMultiValued, IList<SmiExtendedMetaData> fieldMetaData, SmiMetaDataPropertyCollection extendedProperties, string name, string typeSpecificNamePart1, string typeSpecificNamePart2, string typeSpecificNamePart3) : base(dbType, maxLength, precision, scale, localeId, compareOptions, userDefinedType, udtAssemblyQualifiedName, isMultiValued, fieldMetaData, extendedProperties)
        {
            this._name = name;
            this._typeSpecificNamePart1 = typeSpecificNamePart1;
            this._typeSpecificNamePart2 = typeSpecificNamePart2;
            this._typeSpecificNamePart3 = typeSpecificNamePart3;
        }

        internal override string TraceString(int indent)
        {
            return string.Format(CultureInfo.InvariantCulture, "{2}                 Name={0}{1}{2}TypeSpecificNamePart1='{3}'\n\t{2}TypeSpecificNamePart2='{4}'\n\t{2}TypeSpecificNamePart3='{5}'\n\t", new object[] { (this._name != null) ? this._name : "<null>", base.TraceString(indent), new string(' ', indent), (this.TypeSpecificNamePart1 != null) ? this.TypeSpecificNamePart1 : "<null>", (this.TypeSpecificNamePart2 != null) ? this.TypeSpecificNamePart2 : "<null>", (this.TypeSpecificNamePart3 != null) ? this.TypeSpecificNamePart3 : "<null>" });
        }

        internal string Name
        {
            get
            {
                return this._name;
            }
        }

        internal string TypeSpecificNamePart1
        {
            get
            {
                return this._typeSpecificNamePart1;
            }
        }

        internal string TypeSpecificNamePart2
        {
            get
            {
                return this._typeSpecificNamePart2;
            }
        }

        internal string TypeSpecificNamePart3
        {
            get
            {
                return this._typeSpecificNamePart3;
            }
        }
    }
}

