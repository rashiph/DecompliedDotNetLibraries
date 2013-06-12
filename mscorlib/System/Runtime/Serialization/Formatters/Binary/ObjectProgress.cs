namespace System.Runtime.Serialization.Formatters.Binary
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    internal sealed class ObjectProgress
    {
        internal BinaryTypeEnum binaryTypeEnum;
        internal BinaryTypeEnum[] binaryTypeEnumA;
        internal int count;
        internal Type dtType;
        internal BinaryTypeEnum expectedType = BinaryTypeEnum.ObjectUrt;
        internal object expectedTypeInformation;
        internal bool isInitial;
        internal int memberLength;
        internal string[] memberNames;
        internal InternalMemberTypeE memberTypeEnum;
        internal Type[] memberTypes;
        internal InternalMemberValueE memberValueEnum;
        internal string name;
        internal int nullCount;
        internal int numItems;
        internal InternalObjectTypeE objectTypeEnum;
        internal int opRecordId;
        internal static int opRecordIdCount = 1;
        internal ParseRecord pr = new ParseRecord();
        internal object typeInformation;
        internal object[] typeInformationA;

        internal ObjectProgress()
        {
        }

        internal void ArrayCountIncrement(int value)
        {
            this.count += value;
        }

        [Conditional("SER_LOGGING")]
        private void Counter()
        {
            lock (this)
            {
                this.opRecordId = opRecordIdCount++;
                if (opRecordIdCount > 0x3e8)
                {
                    opRecordIdCount = 1;
                }
            }
        }

        internal bool GetNext(out BinaryTypeEnum outBinaryTypeEnum, out object outTypeInformation)
        {
            outBinaryTypeEnum = BinaryTypeEnum.Primitive;
            outTypeInformation = null;
            if (this.objectTypeEnum == InternalObjectTypeE.Array)
            {
                if (this.count == this.numItems)
                {
                    return false;
                }
                outBinaryTypeEnum = this.binaryTypeEnum;
                outTypeInformation = this.typeInformation;
                if (this.count == 0)
                {
                    this.isInitial = false;
                }
                this.count++;
                return true;
            }
            if ((this.count == this.memberLength) && !this.isInitial)
            {
                return false;
            }
            outBinaryTypeEnum = this.binaryTypeEnumA[this.count];
            outTypeInformation = this.typeInformationA[this.count];
            if (this.count == 0)
            {
                this.isInitial = false;
            }
            this.name = this.memberNames[this.count];
            Type[] memberTypes = this.memberTypes;
            this.dtType = this.memberTypes[this.count];
            this.count++;
            return true;
        }

        internal void Init()
        {
            this.isInitial = false;
            this.count = 0;
            this.expectedType = BinaryTypeEnum.ObjectUrt;
            this.expectedTypeInformation = null;
            this.name = null;
            this.objectTypeEnum = InternalObjectTypeE.Empty;
            this.memberTypeEnum = InternalMemberTypeE.Empty;
            this.memberValueEnum = InternalMemberValueE.Empty;
            this.dtType = null;
            this.numItems = 0;
            this.nullCount = 0;
            this.typeInformation = null;
            this.memberLength = 0;
            this.binaryTypeEnumA = null;
            this.typeInformationA = null;
            this.memberNames = null;
            this.memberTypes = null;
            this.pr.Init();
        }
    }
}

