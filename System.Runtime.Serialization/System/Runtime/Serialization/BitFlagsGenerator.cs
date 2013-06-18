namespace System.Runtime.Serialization
{
    using System;
    using System.Reflection.Emit;

    internal class BitFlagsGenerator
    {
        private int bitCount;
        private CodeGenerator ilg;
        private LocalBuilder[] locals;

        public BitFlagsGenerator(int bitCount, CodeGenerator ilg, string localName)
        {
            this.ilg = ilg;
            this.bitCount = bitCount;
            int num = (bitCount + 7) / 8;
            this.locals = new LocalBuilder[num];
            for (int i = 0; i < this.locals.Length; i++)
            {
                this.locals[i] = ilg.DeclareLocal(typeof(byte), localName + i, (byte) 0);
            }
        }

        public int GetBitCount()
        {
            return this.bitCount;
        }

        private static byte GetBitValue(int bitIndex)
        {
            return (byte) (((int) 1) << (bitIndex & 7));
        }

        private static int GetByteIndex(int bitIndex)
        {
            return (bitIndex >> 3);
        }

        public LocalBuilder GetLocal(int i)
        {
            return this.locals[i];
        }

        public int GetLocalCount()
        {
            return this.locals.Length;
        }

        public static bool IsBitSet(byte[] bytes, int bitIndex)
        {
            int byteIndex = GetByteIndex(bitIndex);
            byte bitValue = GetBitValue(bitIndex);
            return ((bytes[byteIndex] & bitValue) == bitValue);
        }

        public void Load(int bitIndex)
        {
            LocalBuilder builder = this.locals[GetByteIndex(bitIndex)];
            byte bitValue = GetBitValue(bitIndex);
            this.ilg.Load(builder);
            this.ilg.Load(bitValue);
            this.ilg.And();
            this.ilg.Load(bitValue);
            this.ilg.Ceq();
        }

        public void LoadArray()
        {
            LocalBuilder var = this.ilg.DeclareLocal(Globals.TypeOfByteArray, "localArray");
            this.ilg.NewArray(typeof(byte), this.locals.Length);
            this.ilg.Store(var);
            for (int i = 0; i < this.locals.Length; i++)
            {
                this.ilg.StoreArrayElement(var, i, this.locals[i]);
            }
            this.ilg.Load(var);
        }

        public static void SetBit(byte[] bytes, int bitIndex)
        {
            int byteIndex = GetByteIndex(bitIndex);
            byte bitValue = GetBitValue(bitIndex);
            bytes[byteIndex] = (byte) (bytes[byteIndex] | bitValue);
        }

        public void Store(int bitIndex, bool value)
        {
            LocalBuilder builder = this.locals[GetByteIndex(bitIndex)];
            byte bitValue = GetBitValue(bitIndex);
            if (value)
            {
                this.ilg.Load(builder);
                this.ilg.Load(bitValue);
                this.ilg.Or();
                this.ilg.Stloc(builder);
            }
            else
            {
                this.ilg.Load(builder);
                this.ilg.Load(bitValue);
                this.ilg.Not();
                this.ilg.And();
                this.ilg.Stloc(builder);
            }
        }
    }
}

