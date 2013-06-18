namespace System.Runtime.Serialization.Formatters.Soap
{
    using System;
    using System.Diagnostics;

    internal sealed class ParseRecord : ITrace
    {
        internal static int parseRecordIdCount = 1;
        internal Type PRarrayElementType;
        internal InternalPrimitiveTypeE PRarrayElementTypeCode;
        internal string PRarrayElementTypeString;
        internal InternalArrayTypeE PRarrayTypeEnum;
        internal string PRassemblyName;
        internal Type PRdtType;
        internal InternalPrimitiveTypeE PRdtTypeCode;
        internal long PRheaderId;
        internal long PRidRef;
        internal int[] PRindexMap;
        internal bool PRisArrayVariant;
        internal bool PRisAttributesProcessed;
        internal bool PRisEnum;
        internal bool PRisHeaderRoot;
        internal bool PRisLowerBound;
        internal bool PRisMustUnderstand;
        internal bool PRisParsed;
        internal bool PRisProcessAttributes;
        internal bool PRisRegistered;
        internal bool PRisValueTypeFixup;
        internal bool PRisVariant;
        internal bool PRisWaitingForNestedObject;
        internal bool PRisXmlAttribute;
        internal string PRkeyDt;
        internal int[] PRlengthA;
        internal int PRlinearlength;
        internal int[] PRlowerBoundA;
        internal int PRmemberIndex;
        internal InternalMemberTypeE PRmemberTypeEnum;
        internal InternalMemberValueE PRmemberValueEnum;
        internal string PRname;
        internal string PRnameXmlKey;
        internal object PRnewObj;
        internal object[] PRobjectA;
        internal long PRobjectId;
        internal ReadObjectInfo PRobjectInfo;
        internal InternalObjectPositionE PRobjectPositionEnum;
        internal InternalObjectTypeE PRobjectTypeEnum;
        internal int PRparseRecordId;
        internal InternalParseStateE PRparseStateEnum;
        internal InternalParseTypeE PRparseTypeEnum;
        internal int[] PRpositionA;
        internal PrimitiveArray PRprimitiveArray;
        internal string PRprimitiveArrayTypeString;
        internal int PRrank;
        internal int[] PRrectangularMap;
        internal long PRtopId;
        internal string PRtypeXmlKey;
        internal int[] PRupperBoundA;
        internal string PRvalue;
        internal object PRvarValue;
        internal string PRxmlNameSpace;

        internal ParseRecord()
        {
            this.Counter();
        }

        internal ParseRecord Copy()
        {
            return new ParseRecord { 
                PRparseTypeEnum = this.PRparseTypeEnum, PRobjectTypeEnum = this.PRobjectTypeEnum, PRarrayTypeEnum = this.PRarrayTypeEnum, PRmemberTypeEnum = this.PRmemberTypeEnum, PRmemberValueEnum = this.PRmemberValueEnum, PRobjectPositionEnum = this.PRobjectPositionEnum, PRname = this.PRname, PRisParsed = this.PRisParsed, PRisProcessAttributes = this.PRisProcessAttributes, PRnameXmlKey = this.PRnameXmlKey, PRxmlNameSpace = this.PRxmlNameSpace, PRvalue = this.PRvalue, PRkeyDt = this.PRkeyDt, PRdtType = this.PRdtType, PRassemblyName = this.PRassemblyName, PRdtTypeCode = this.PRdtTypeCode, 
                PRisEnum = this.PRisEnum, PRobjectId = this.PRobjectId, PRidRef = this.PRidRef, PRarrayElementTypeString = this.PRarrayElementTypeString, PRarrayElementType = this.PRarrayElementType, PRisArrayVariant = this.PRisArrayVariant, PRarrayElementTypeCode = this.PRarrayElementTypeCode, PRprimitiveArrayTypeString = this.PRprimitiveArrayTypeString, PRrank = this.PRrank, PRlengthA = this.PRlengthA, PRpositionA = this.PRpositionA, PRlowerBoundA = this.PRlowerBoundA, PRupperBoundA = this.PRupperBoundA, PRindexMap = this.PRindexMap, PRmemberIndex = this.PRmemberIndex, PRlinearlength = this.PRlinearlength, 
                PRrectangularMap = this.PRrectangularMap, PRisLowerBound = this.PRisLowerBound, PRtopId = this.PRtopId, PRheaderId = this.PRheaderId, PRisHeaderRoot = this.PRisHeaderRoot, PRisAttributesProcessed = this.PRisAttributesProcessed, PRisMustUnderstand = this.PRisMustUnderstand, PRparseStateEnum = this.PRparseStateEnum, PRisWaitingForNestedObject = this.PRisWaitingForNestedObject, PRisValueTypeFixup = this.PRisValueTypeFixup, PRnewObj = this.PRnewObj, PRobjectA = this.PRobjectA, PRprimitiveArray = this.PRprimitiveArray, PRobjectInfo = this.PRobjectInfo, PRisRegistered = this.PRisRegistered, PRisXmlAttribute = this.PRisXmlAttribute
             };
        }

        private void Counter()
        {
            lock (typeof(ParseRecord))
            {
                this.PRparseRecordId = parseRecordIdCount++;
            }
        }

        [Conditional("SER_LOGGING")]
        internal void Dump()
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
            this.PRnameXmlKey = null;
            this.PRxmlNameSpace = null;
            this.PRisParsed = false;
            this.PRisProcessAttributes = false;
            this.PRvalue = null;
            this.PRkeyDt = null;
            this.PRdtType = null;
            this.PRassemblyName = null;
            this.PRdtTypeCode = InternalPrimitiveTypeE.Invalid;
            this.PRisEnum = false;
            this.PRobjectId = 0L;
            this.PRidRef = 0L;
            this.PRarrayElementTypeString = null;
            this.PRarrayElementType = null;
            this.PRisArrayVariant = false;
            this.PRarrayElementTypeCode = InternalPrimitiveTypeE.Invalid;
            this.PRprimitiveArrayTypeString = null;
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
            this.PRisHeaderRoot = false;
            this.PRisAttributesProcessed = false;
            this.PRisMustUnderstand = false;
            this.PRparseStateEnum = InternalParseStateE.Initial;
            this.PRisWaitingForNestedObject = false;
            this.PRisValueTypeFixup = false;
            this.PRnewObj = null;
            this.PRobjectA = null;
            this.PRprimitiveArray = null;
            this.PRobjectInfo = null;
            this.PRisRegistered = false;
            this.PRisXmlAttribute = false;
        }

        public string Trace()
        {
            return string.Concat(new object[] { "ParseRecord", this.PRparseRecordId, " ParseType ", this.PRparseTypeEnum.ToString(), " name ", this.PRname, " keyDt ", Util.PString(this.PRkeyDt) });
        }
    }
}

