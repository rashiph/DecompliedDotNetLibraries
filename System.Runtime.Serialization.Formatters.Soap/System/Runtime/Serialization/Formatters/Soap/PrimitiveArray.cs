namespace System.Runtime.Serialization.Formatters.Soap
{
    using System;
    using System.Globalization;

    internal sealed class PrimitiveArray
    {
        private bool[] booleanA;
        private char[] charA;
        private InternalPrimitiveTypeE code;
        private double[] doubleA;
        private short[] int16A;
        private int[] int32A;
        private long[] int64A;
        private sbyte[] sbyteA;
        private float[] singleA;
        private ushort[] uint16A;
        private uint[] uint32A;
        private ulong[] uint64A;

        internal PrimitiveArray(InternalPrimitiveTypeE code, Array array)
        {
            this.Init(code, array);
        }

        internal string GetValue(int index)
        {
            string str = null;
            switch (this.code)
            {
                case InternalPrimitiveTypeE.Boolean:
                    return this.booleanA[index].ToString();

                case InternalPrimitiveTypeE.Byte:
                case InternalPrimitiveTypeE.Currency:
                case InternalPrimitiveTypeE.Decimal:
                case InternalPrimitiveTypeE.TimeSpan:
                case InternalPrimitiveTypeE.DateTime:
                    return str;

                case InternalPrimitiveTypeE.Char:
                    if (this.charA[index] != '\0')
                    {
                        return char.ToString(this.charA[index]);
                    }
                    return "_0x00_";

                case InternalPrimitiveTypeE.Double:
                    if (!double.IsPositiveInfinity(this.doubleA[index]))
                    {
                        if (double.IsNegativeInfinity(this.doubleA[index]))
                        {
                            return "-INF";
                        }
                        return this.doubleA[index].ToString("R", CultureInfo.InvariantCulture);
                    }
                    return "INF";

                case InternalPrimitiveTypeE.Int16:
                    return this.int16A[index].ToString(CultureInfo.InvariantCulture);

                case InternalPrimitiveTypeE.Int32:
                    return this.int32A[index].ToString(CultureInfo.InvariantCulture);

                case InternalPrimitiveTypeE.Int64:
                    return this.int64A[index].ToString(CultureInfo.InvariantCulture);

                case InternalPrimitiveTypeE.SByte:
                    return this.sbyteA[index].ToString(CultureInfo.InvariantCulture);

                case InternalPrimitiveTypeE.Single:
                    if (!float.IsPositiveInfinity(this.singleA[index]))
                    {
                        if (float.IsNegativeInfinity(this.singleA[index]))
                        {
                            return "-INF";
                        }
                        return this.singleA[index].ToString("R", CultureInfo.InvariantCulture);
                    }
                    return "INF";

                case InternalPrimitiveTypeE.UInt16:
                    return this.uint16A[index].ToString(CultureInfo.InvariantCulture);

                case InternalPrimitiveTypeE.UInt32:
                    return this.uint32A[index].ToString(CultureInfo.InvariantCulture);

                case InternalPrimitiveTypeE.UInt64:
                    return this.uint64A[index].ToString(CultureInfo.InvariantCulture);
            }
            return str;
        }

        internal void Init(InternalPrimitiveTypeE code, Array array)
        {
            this.code = code;
            switch (code)
            {
                case InternalPrimitiveTypeE.Boolean:
                    this.booleanA = (bool[]) array;
                    return;

                case InternalPrimitiveTypeE.Byte:
                case InternalPrimitiveTypeE.Currency:
                case InternalPrimitiveTypeE.Decimal:
                case InternalPrimitiveTypeE.TimeSpan:
                case InternalPrimitiveTypeE.DateTime:
                    break;

                case InternalPrimitiveTypeE.Char:
                    this.charA = (char[]) array;
                    return;

                case InternalPrimitiveTypeE.Double:
                    this.doubleA = (double[]) array;
                    return;

                case InternalPrimitiveTypeE.Int16:
                    this.int16A = (short[]) array;
                    return;

                case InternalPrimitiveTypeE.Int32:
                    this.int32A = (int[]) array;
                    return;

                case InternalPrimitiveTypeE.Int64:
                    this.int64A = (long[]) array;
                    return;

                case InternalPrimitiveTypeE.SByte:
                    this.sbyteA = (sbyte[]) array;
                    return;

                case InternalPrimitiveTypeE.Single:
                    this.singleA = (float[]) array;
                    return;

                case InternalPrimitiveTypeE.UInt16:
                    this.uint16A = (ushort[]) array;
                    return;

                case InternalPrimitiveTypeE.UInt32:
                    this.uint32A = (uint[]) array;
                    return;

                case InternalPrimitiveTypeE.UInt64:
                    this.uint64A = (ulong[]) array;
                    break;

                default:
                    return;
            }
        }

        internal void SetValue(string value, int index)
        {
            switch (this.code)
            {
                case InternalPrimitiveTypeE.Boolean:
                    this.booleanA[index] = bool.Parse(value);
                    return;

                case InternalPrimitiveTypeE.Byte:
                case InternalPrimitiveTypeE.Currency:
                case InternalPrimitiveTypeE.Decimal:
                case InternalPrimitiveTypeE.TimeSpan:
                case InternalPrimitiveTypeE.DateTime:
                    break;

                case InternalPrimitiveTypeE.Char:
                    if ((value[0] != '_') || !value.Equals("_0x00_"))
                    {
                        this.charA[index] = char.Parse(value);
                        return;
                    }
                    this.charA[index] = '\0';
                    return;

                case InternalPrimitiveTypeE.Double:
                    if (!(value == "INF"))
                    {
                        if (value == "-INF")
                        {
                            this.doubleA[index] = double.NegativeInfinity;
                            return;
                        }
                        this.doubleA[index] = double.Parse(value, CultureInfo.InvariantCulture);
                        return;
                    }
                    this.doubleA[index] = double.PositiveInfinity;
                    return;

                case InternalPrimitiveTypeE.Int16:
                    this.int16A[index] = short.Parse(value, CultureInfo.InvariantCulture);
                    return;

                case InternalPrimitiveTypeE.Int32:
                    this.int32A[index] = int.Parse(value, CultureInfo.InvariantCulture);
                    return;

                case InternalPrimitiveTypeE.Int64:
                    this.int64A[index] = long.Parse(value, CultureInfo.InvariantCulture);
                    return;

                case InternalPrimitiveTypeE.SByte:
                    this.sbyteA[index] = sbyte.Parse(value, CultureInfo.InvariantCulture);
                    return;

                case InternalPrimitiveTypeE.Single:
                    if (!(value == "INF"))
                    {
                        if (value == "-INF")
                        {
                            this.singleA[index] = float.NegativeInfinity;
                            return;
                        }
                        this.singleA[index] = float.Parse(value, CultureInfo.InvariantCulture);
                        return;
                    }
                    this.singleA[index] = float.PositiveInfinity;
                    return;

                case InternalPrimitiveTypeE.UInt16:
                    this.uint16A[index] = ushort.Parse(value, CultureInfo.InvariantCulture);
                    return;

                case InternalPrimitiveTypeE.UInt32:
                    this.uint32A[index] = uint.Parse(value, CultureInfo.InvariantCulture);
                    return;

                case InternalPrimitiveTypeE.UInt64:
                    this.uint64A[index] = ulong.Parse(value, CultureInfo.InvariantCulture);
                    break;

                default:
                    return;
            }
        }
    }
}

