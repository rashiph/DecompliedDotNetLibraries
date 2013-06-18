namespace System.Runtime.Serialization.Formatters.Soap
{
    using System;
    using System.Reflection;

    internal class ValueFixup : ITrace
    {
        internal Array arrayObj;
        internal int[] indexMap;
        internal string memberName;
        internal object memberObject;
        internal ReadObjectInfo objectInfo;
        internal ValueFixupEnum valueFixupEnum;

        internal ValueFixup(Array arrayObj, int[] indexMap)
        {
            this.valueFixupEnum = ValueFixupEnum.Array;
            this.arrayObj = arrayObj;
            this.indexMap = indexMap;
        }

        internal ValueFixup(object memberObject, string memberName, ReadObjectInfo objectInfo)
        {
            this.valueFixupEnum = ValueFixupEnum.Member;
            this.memberObject = memberObject;
            this.memberName = memberName;
            this.objectInfo = objectInfo;
        }

        internal virtual void Fixup(ParseRecord record, ParseRecord parent)
        {
            object pRnewObj = record.PRnewObj;
            switch (this.valueFixupEnum)
            {
                case ValueFixupEnum.Array:
                    this.arrayObj.SetValue(pRnewObj, this.indexMap);
                    return;

                case ValueFixupEnum.Header:
                    break;

                case ValueFixupEnum.Member:
                    if (!this.objectInfo.isSi)
                    {
                        MemberInfo memberInfo = this.objectInfo.GetMemberInfo(this.memberName);
                        this.objectInfo.objectManager.RecordFixup(parent.PRobjectId, memberInfo, record.PRobjectId);
                        break;
                    }
                    this.objectInfo.objectManager.RecordDelayedFixup(parent.PRobjectId, this.memberName, record.PRobjectId);
                    return;

                default:
                    return;
            }
        }

        public virtual string Trace()
        {
            return ("ValueFixup" + this.valueFixupEnum.ToString());
        }
    }
}

