namespace System.Runtime.Serialization.Formatters.Binary
{
    using System;
    using System.Security;

    internal sealed class BinaryObjectWithMapTyped : IStreamable
    {
        internal int assemId;
        internal BinaryHeaderEnum binaryHeaderEnum;
        internal BinaryTypeEnum[] binaryTypeEnumA;
        internal int[] memberAssemIds;
        internal string[] memberNames;
        internal string name;
        internal int numMembers;
        internal int objectId;
        internal object[] typeInformationA;

        internal BinaryObjectWithMapTyped()
        {
        }

        internal BinaryObjectWithMapTyped(BinaryHeaderEnum binaryHeaderEnum)
        {
            this.binaryHeaderEnum = binaryHeaderEnum;
        }

        [SecurityCritical]
        public void Read(__BinaryParser input)
        {
            this.objectId = input.ReadInt32();
            this.name = input.ReadString();
            this.numMembers = input.ReadInt32();
            this.memberNames = new string[this.numMembers];
            this.binaryTypeEnumA = new BinaryTypeEnum[this.numMembers];
            this.typeInformationA = new object[this.numMembers];
            this.memberAssemIds = new int[this.numMembers];
            for (int i = 0; i < this.numMembers; i++)
            {
                this.memberNames[i] = input.ReadString();
            }
            for (int j = 0; j < this.numMembers; j++)
            {
                this.binaryTypeEnumA[j] = (BinaryTypeEnum) input.ReadByte();
            }
            for (int k = 0; k < this.numMembers; k++)
            {
                if ((this.binaryTypeEnumA[k] != BinaryTypeEnum.ObjectUrt) && (this.binaryTypeEnumA[k] != BinaryTypeEnum.ObjectUser))
                {
                    this.typeInformationA[k] = BinaryConverter.ReadTypeInfo(this.binaryTypeEnumA[k], input, out this.memberAssemIds[k]);
                }
                else
                {
                    BinaryConverter.ReadTypeInfo(this.binaryTypeEnumA[k], input, out this.memberAssemIds[k]);
                }
            }
            if (this.binaryHeaderEnum == BinaryHeaderEnum.ObjectWithMapTypedAssemId)
            {
                this.assemId = input.ReadInt32();
            }
        }

        internal void Set(int objectId, string name, int numMembers, string[] memberNames, BinaryTypeEnum[] binaryTypeEnumA, object[] typeInformationA, int[] memberAssemIds, int assemId)
        {
            this.objectId = objectId;
            this.assemId = assemId;
            this.name = name;
            this.numMembers = numMembers;
            this.memberNames = memberNames;
            this.binaryTypeEnumA = binaryTypeEnumA;
            this.typeInformationA = typeInformationA;
            this.memberAssemIds = memberAssemIds;
            this.assemId = assemId;
            if (assemId > 0)
            {
                this.binaryHeaderEnum = BinaryHeaderEnum.ObjectWithMapTypedAssemId;
            }
            else
            {
                this.binaryHeaderEnum = BinaryHeaderEnum.ObjectWithMapTyped;
            }
        }

        public void Write(__BinaryWriter sout)
        {
            sout.WriteByte((byte) this.binaryHeaderEnum);
            sout.WriteInt32(this.objectId);
            sout.WriteString(this.name);
            sout.WriteInt32(this.numMembers);
            for (int i = 0; i < this.numMembers; i++)
            {
                sout.WriteString(this.memberNames[i]);
            }
            for (int j = 0; j < this.numMembers; j++)
            {
                sout.WriteByte((byte) this.binaryTypeEnumA[j]);
            }
            for (int k = 0; k < this.numMembers; k++)
            {
                BinaryConverter.WriteTypeInfo(this.binaryTypeEnumA[k], this.typeInformationA[k], this.memberAssemIds[k], sout);
            }
            if (this.assemId > 0)
            {
                sout.WriteInt32(this.assemId);
            }
        }
    }
}

