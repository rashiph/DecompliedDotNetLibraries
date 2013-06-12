namespace System.Runtime.Serialization.Formatters.Binary
{
    using System;
    using System.Runtime.Serialization;

    internal sealed class ParseRecord
    {
        internal static int parseRecordIdCount = 1;
        internal Type PRarrayElementType;
        internal InternalPrimitiveTypeE PRarrayElementTypeCode;
        internal string PRarrayElementTypeString;
        internal InternalArrayTypeE PRarrayTypeEnum;
        internal Type PRdtType;
        internal InternalPrimitiveTypeE PRdtTypeCode;
        internal long PRheaderId;
        internal long PRidRef;
        internal int[] PRindexMap;
        internal bool PRisArrayVariant;
        internal bool PRisEnum;
        internal bool PRisLowerBound;
        internal bool PRisRegistered;
        internal bool PRisValueTypeFixup;
        internal bool PRisVariant;
        internal string PRkeyDt;
        internal int[] PRlengthA;
        internal int PRlinearlength;
        internal int[] PRlowerBoundA;
        internal object[] PRmemberData;
        internal int PRmemberIndex;
        internal InternalMemberTypeE PRmemberTypeEnum;
        internal InternalMemberValueE PRmemberValueEnum;
        internal string PRname;
        internal object PRnewObj;
        internal int PRnullCount;
        internal object[] PRobjectA;
        internal long PRobjectId;
        internal ReadObjectInfo PRobjectInfo;
        internal InternalObjectPositionE PRobjectPositionEnum;
        internal InternalObjectTypeE PRobjectTypeEnum;
        internal int PRparseRecordId;
        internal InternalParseTypeE PRparseTypeEnum;
        internal int[] PRpositionA;
        internal PrimitiveArray PRprimitiveArray;
        internal int PRrank;
        internal int[] PRrectangularMap;
        internal SerializationInfo PRsi;
        internal long PRtopId;
        internal int[] PRupperBoundA;
        internal string PRvalue;
        internal object PRvarValue;

        internal ParseRecord()
        {
        }

        internal void Init()
        {
            this.PRparseTypeEnum = InternalParseTypeE.Empty;
            this.PRobjectTypeEnum = InternalObjectTypeE.Empty;
            this.PRarrayTypeEnum = InternalArrayTypeE.Empty;
            this.PRmemberTypeEnum = InternalMemberTypeE.Empty;
            this.PRmemberValueEnum = InternalMemberValueE.Empty;
            this.PRobjectPositionEnum = InternalObjectPositionE.Empty;
            this.PRname = null;
            this.PRvalue = null;
            this.PRkeyDt = null;
            this.PRdtType = null;
            this.PRdtTypeCode = InternalPrimitiveTypeE.Invalid;
            this.PRisEnum = false;
            this.PRobjectId = 0L;
            this.PRidRef = 0L;
            this.PRarrayElementTypeString = null;
            this.PRarrayElementType = null;
            this.PRisArrayVariant = false;
            this.PRarrayElementTypeCode = InternalPrimitiveTypeE.Invalid;
            this.PRrank = 0;
            this.PRlengthA = null;
            this.PRpositionA = null;
            this.PRlowerBoundA = null;
            this.PRupperBoundA = null;
            this.PRindexMap = null;
            this.PRmemberIndex = 0;
            this.PRlinearlength = 0;
            this.PRrectangularMap = null;
            this.PRisLowerBound = false;
            this.PRtopId = 0L;
            this.PRheaderId = 0L;
            this.PRisValueTypeFixup = false;
            this.PRnewObj = null;
            this.PRobjectA = null;
            this.PRprimitiveArray = null;
            this.PRobjectInfo = null;
            this.PRisRegistered = false;
            this.PRmemberData = null;
            this.PRsi = null;
            this.PRnullCount = 0;
        }
    }
}

