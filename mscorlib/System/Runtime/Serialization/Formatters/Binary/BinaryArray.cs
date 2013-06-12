namespace System.Runtime.Serialization.Formatters.Binary
{
    using System;
    using System.Security;

    internal sealed class BinaryArray : IStreamable
    {
        internal int assemId;
        internal BinaryArrayTypeEnum binaryArrayTypeEnum;
        private BinaryHeaderEnum binaryHeaderEnum;
        internal BinaryTypeEnum binaryTypeEnum;
        internal int[] lengthA;
        internal int[] lowerBoundA;
        internal int objectId;
        internal int rank;
        internal object typeInformation;

        internal BinaryArray()
        {
        }

        internal BinaryArray(BinaryHeaderEnum binaryHeaderEnum)
        {
            this.binaryHeaderEnum = binaryHeaderEnum;
        }

        [SecurityCritical]
        public void Read(__BinaryParser input)
        {
            switch (this.binaryHeaderEnum)
            {
                case BinaryHeaderEnum.ArraySinglePrimitive:
                    this.objectId = input.ReadInt32();
                    this.lengthA = new int[] { input.ReadInt32() };
                    this.binaryArrayTypeEnum = BinaryArrayTypeEnum.Single;
                    this.rank = 1;
                    this.lowerBoundA = new int[this.rank];
                    this.binaryTypeEnum = BinaryTypeEnum.Primitive;
                    this.typeInformation = (InternalPrimitiveTypeE) input.ReadByte();
                    return;

                case BinaryHeaderEnum.ArraySingleObject:
                    this.objectId = input.ReadInt32();
                    this.lengthA = new int[] { input.ReadInt32() };
                    this.binaryArrayTypeEnum = BinaryArrayTypeEnum.Single;
                    this.rank = 1;
                    this.lowerBoundA = new int[this.rank];
                    this.binaryTypeEnum = BinaryTypeEnum.Object;
                    this.typeInformation = null;
                    return;

                case BinaryHeaderEnum.ArraySingleString:
                    this.objectId = input.ReadInt32();
                    this.lengthA = new int[] { input.ReadInt32() };
                    this.binaryArrayTypeEnum = BinaryArrayTypeEnum.Single;
                    this.rank = 1;
                    this.lowerBoundA = new int[this.rank];
                    this.binaryTypeEnum = BinaryTypeEnum.String;
                    this.typeInformation = null;
                    return;
            }
            this.objectId = input.ReadInt32();
            this.binaryArrayTypeEnum = (BinaryArrayTypeEnum) input.ReadByte();
            this.rank = input.ReadInt32();
            this.lengthA = new int[this.rank];
            this.lowerBoundA = new int[this.rank];
            for (int i = 0; i < this.rank; i++)
            {
                this.lengthA[i] = input.ReadInt32();
            }
            if (((this.binaryArrayTypeEnum == BinaryArrayTypeEnum.SingleOffset) || (this.binaryArrayTypeEnum == BinaryArrayTypeEnum.JaggedOffset)) || (this.binaryArrayTypeEnum == BinaryArrayTypeEnum.RectangularOffset))
            {
                for (int j = 0; j < this.rank; j++)
                {
                    this.lowerBoundA[j] = input.ReadInt32();
                }
            }
            this.binaryTypeEnum = (BinaryTypeEnum) input.ReadByte();
            this.typeInformation = BinaryConverter.ReadTypeInfo(this.binaryTypeEnum, input, out this.assemId);
        }

        internal void Set(int objectId, int rank, int[] lengthA, int[] lowerBoundA, BinaryTypeEnum binaryTypeEnum, object typeInformation, BinaryArrayTypeEnum binaryArrayTypeEnum, int assemId)
        {
            this.objectId = objectId;
            this.binaryArrayTypeEnum = binaryArrayTypeEnum;
            this.rank = rank;
            this.lengthA = lengthA;
            this.lowerBoundA = lowerBoundA;
            this.binaryTypeEnum = binaryTypeEnum;
            this.typeInformation = typeInformation;
            this.assemId = assemId;
            this.binaryHeaderEnum = BinaryHeaderEnum.Array;
            if (binaryArrayTypeEnum == BinaryArrayTypeEnum.Single)
            {
                if (binaryTypeEnum == BinaryTypeEnum.Primitive)
                {
                    this.binaryHeaderEnum = BinaryHeaderEnum.ArraySinglePrimitive;
                }
                else if (binaryTypeEnum == BinaryTypeEnum.String)
                {
                    this.binaryHeaderEnum = BinaryHeaderEnum.ArraySingleString;
                }
                else if (binaryTypeEnum == BinaryTypeEnum.Object)
                {
                    this.binaryHeaderEnum = BinaryHeaderEnum.ArraySingleObject;
                }
            }
        }

        public void Write(__BinaryWriter sout)
        {
            switch (this.binaryHeaderEnum)
            {
                case BinaryHeaderEnum.ArraySinglePrimitive:
                    sout.WriteByte((byte) this.binaryHeaderEnum);
                    sout.WriteInt32(this.objectId);
                    sout.WriteInt32(this.lengthA[0]);
                    sout.WriteByte((byte) ((InternalPrimitiveTypeE) this.typeInformation));
                    return;

                case BinaryHeaderEnum.ArraySingleObject:
                    sout.WriteByte((byte) this.binaryHeaderEnum);
                    sout.WriteInt32(this.objectId);
                    sout.WriteInt32(this.lengthA[0]);
                    return;

                case BinaryHeaderEnum.ArraySingleString:
                    sout.WriteByte((byte) this.binaryHeaderEnum);
                    sout.WriteInt32(this.objectId);
                    sout.WriteInt32(this.lengthA[0]);
                    return;
            }
            sout.WriteByte((byte) this.binaryHeaderEnum);
            sout.WriteInt32(this.objectId);
            sout.WriteByte((byte) this.binaryArrayTypeEnum);
            sout.WriteInt32(this.rank);
            for (int i = 0; i < this.rank; i++)
            {
                sout.WriteInt32(this.lengthA[i]);
            }
            if (((this.binaryArrayTypeEnum == BinaryArrayTypeEnum.SingleOffset) || (this.binaryArrayTypeEnum == BinaryArrayTypeEnum.JaggedOffset)) || (this.binaryArrayTypeEnum == BinaryArrayTypeEnum.RectangularOffset))
            {
                for (int j = 0; j < this.rank; j++)
                {
                    sout.WriteInt32(this.lowerBoundA[j]);
                }
            }
            sout.WriteByte((byte) this.binaryTypeEnum);
            BinaryConverter.WriteTypeInfo(this.binaryTypeEnum, this.typeInformation, this.assemId, sout);
        }
    }
}

