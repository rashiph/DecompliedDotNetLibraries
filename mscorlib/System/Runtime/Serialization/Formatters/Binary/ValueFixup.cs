namespace System.Runtime.Serialization.Formatters.Binary
{
    using System;
    using System.Reflection;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Serialization;
    using System.Security;

    internal sealed class ValueFixup
    {
        internal Array arrayObj;
        internal object header;
        internal int[] indexMap;
        internal string memberName;
        internal object memberObject;
        internal ReadObjectInfo objectInfo;
        internal ValueFixupEnum valueFixupEnum;
        internal static MemberInfo valueInfo;

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

        [SecurityCritical]
        internal void Fixup(ParseRecord record, ParseRecord parent)
        {
            object pRnewObj = record.PRnewObj;
            switch (this.valueFixupEnum)
            {
                case ValueFixupEnum.Array:
                    this.arrayObj.SetValue(pRnewObj, this.indexMap);
                    return;

                case ValueFixupEnum.Header:
                {
                    Type type = typeof(Header);
                    if (valueInfo == null)
                    {
                        MemberInfo[] member = type.GetMember("Value");
                        if (member.Length != 1)
                        {
                            throw new SerializationException(Environment.GetResourceString("Serialization_HeaderReflection", new object[] { member.Length }));
                        }
                        valueInfo = member[0];
                        break;
                    }
                    break;
                }
                case ValueFixupEnum.Member:
                    if (!this.objectInfo.isSi)
                    {
                        MemberInfo memberInfo = this.objectInfo.GetMemberInfo(this.memberName);
                        if (memberInfo != null)
                        {
                            this.objectInfo.objectManager.RecordFixup(parent.PRobjectId, memberInfo, record.PRobjectId);
                        }
                        return;
                    }
                    this.objectInfo.objectManager.RecordDelayedFixup(parent.PRobjectId, this.memberName, record.PRobjectId);
                    return;

                default:
                    return;
            }
            FormatterServices.SerializationSetValue(valueInfo, this.header, pRnewObj);
        }
    }
}

