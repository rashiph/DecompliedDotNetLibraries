namespace System.Runtime.Serialization.Formatters.Soap
{
    using System;
    using System.Diagnostics;

    internal sealed class NameInfo
    {
        internal long NIassemId;
        internal SoapAttributeInfo NIattributeInfo;
        internal string NIheaderPrefix;
        internal bool NIisArray;
        internal bool NIisArrayItem;
        internal bool NIisHeader;
        internal bool NIisMustUnderstand;
        internal bool NIisNestedObject;
        internal bool NIisParentTypeOnObject;
        internal bool NIisRemoteRecord;
        internal bool NIisSealed;
        internal bool NIisTopLevelObject;
        internal string NIitemName;
        internal string NIname;
        internal string NInamespace;
        internal InternalNameSpaceE NInameSpaceEnum;
        internal long NIobjectId;
        internal InternalPrimitiveTypeE NIprimitiveTypeEnum;
        internal bool NItransmitTypeOnMember;
        internal bool NItransmitTypeOnObject;
        internal Type NItype;

        [Conditional("SER_LOGGING")]
        internal void Dump(string value)
        {
            SoapAttributeInfo nIattributeInfo = this.NIattributeInfo;
        }

        internal void Init()
        {
            this.NInameSpaceEnum = InternalNameSpaceE.None;
            this.NIname = null;
            this.NIobjectId = 0L;
            this.NIassemId = 0L;
            this.NIprimitiveTypeEnum = InternalPrimitiveTypeE.Invalid;
            this.NItype = null;
            this.NIisSealed = false;
            this.NItransmitTypeOnObject = false;
            this.NItransmitTypeOnMember = false;
            this.NIisParentTypeOnObject = false;
            this.NIisMustUnderstand = false;
            this.NInamespace = null;
            this.NIheaderPrefix = null;
            this.NIitemName = null;
            this.NIisArray = false;
            this.NIisArrayItem = false;
            this.NIisTopLevelObject = false;
            this.NIisNestedObject = false;
            this.NIisHeader = false;
            this.NIisRemoteRecord = false;
            this.NIattributeInfo = null;
        }
    }
}

