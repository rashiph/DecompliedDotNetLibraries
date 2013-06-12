namespace System.Runtime.Serialization.Formatters.Binary
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters;
    using System.Security;
    using System.Text;

    internal sealed class __BinaryWriter
    {
        internal BinaryArray binaryArray;
        internal BinaryAssembly binaryAssembly;
        internal BinaryCrossAppDomainString binaryCrossAppDomainString;
        internal BinaryMethodCall binaryMethodCall;
        internal BinaryMethodReturn binaryMethodReturn;
        internal BinaryObject binaryObject;
        internal BinaryObjectString binaryObjectString;
        internal BinaryObjectWithMap binaryObjectWithMap;
        internal BinaryObjectWithMapTyped binaryObjectWithMapTyped;
        private byte[] byteBuffer;
        private int chunkSize = 0x1000;
        internal BinaryCrossAppDomainAssembly crossAppDomainAssembly;
        internal BinaryWriter dataWriter;
        internal FormatterTypeStyle formatterTypeStyle;
        internal int m_nestedObjectCount;
        internal MemberPrimitiveTyped memberPrimitiveTyped;
        internal MemberPrimitiveUnTyped memberPrimitiveUnTyped;
        internal MemberReference memberReference;
        private int nullCount;
        internal Hashtable objectMapTable;
        internal ObjectNull objectNull;
        internal ObjectWriter objectWriter;
        internal Stream sout;

        internal __BinaryWriter(Stream sout, ObjectWriter objectWriter, FormatterTypeStyle formatterTypeStyle)
        {
            this.sout = sout;
            this.formatterTypeStyle = formatterTypeStyle;
            this.objectWriter = objectWriter;
            this.m_nestedObjectCount = 0;
            this.dataWriter = new BinaryWriter(sout, Encoding.UTF8);
        }

        private void InternalWriteItemNull()
        {
            if (this.nullCount > 0)
            {
                if (this.objectNull == null)
                {
                    this.objectNull = new ObjectNull();
                }
                this.objectNull.SetNullCount(this.nullCount);
                this.objectNull.Dump();
                this.objectNull.Write(this);
                this.nullCount = 0;
            }
        }

        [SecurityCritical]
        private void WriteArrayAsBytes(Array array, int typeLength)
        {
            this.InternalWriteItemNull();
            int length = array.Length;
            int num = 0;
            if (this.byteBuffer == null)
            {
                this.byteBuffer = new byte[this.chunkSize];
            }
            while (num < array.Length)
            {
                int num2 = Math.Min((int) (this.chunkSize / typeLength), (int) (array.Length - num));
                int byteCount = num2 * typeLength;
                Buffer.InternalBlockCopy(array, num * typeLength, this.byteBuffer, 0, byteCount);
                this.WriteBytes(this.byteBuffer, 0, byteCount);
                num += num2;
            }
        }

        internal void WriteAssembly(Type type, string assemblyString, int assemId, bool isNew)
        {
            this.InternalWriteItemNull();
            if (assemblyString == null)
            {
                assemblyString = string.Empty;
            }
            if (isNew)
            {
                if (this.binaryAssembly == null)
                {
                    this.binaryAssembly = new BinaryAssembly();
                }
                this.binaryAssembly.Set(assemId, assemblyString);
                this.binaryAssembly.Dump();
                this.binaryAssembly.Write(this);
            }
        }

        internal void WriteBegin()
        {
        }

        internal void WriteBoolean(bool value)
        {
            this.dataWriter.Write(value);
        }

        internal void WriteByte(byte value)
        {
            this.dataWriter.Write(value);
        }

        private void WriteBytes(byte[] value)
        {
            this.dataWriter.Write(value);
        }

        private void WriteBytes(byte[] byteA, int offset, int size)
        {
            this.dataWriter.Write(byteA, offset, size);
        }

        internal object[] WriteCallArray(string uri, string methodName, string typeName, Type[] instArgs, object[] args, object methodSignature, object callContext, object[] properties)
        {
            if (this.binaryMethodCall == null)
            {
                this.binaryMethodCall = new BinaryMethodCall();
            }
            return this.binaryMethodCall.WriteArray(uri, methodName, typeName, instArgs, args, methodSignature, callContext, properties);
        }

        internal void WriteChar(char value)
        {
            this.dataWriter.Write(value);
        }

        internal void WriteChars(char[] value)
        {
            this.dataWriter.Write(value);
        }

        internal void WriteDateTime(DateTime value)
        {
            this.WriteInt64(value.ToBinaryRaw());
        }

        internal void WriteDecimal(decimal value)
        {
            this.WriteString(value.ToString(CultureInfo.InvariantCulture));
        }

        internal void WriteDelayedNullItem()
        {
            this.nullCount++;
        }

        internal void WriteDouble(double value)
        {
            this.dataWriter.Write(value);
        }

        internal void WriteEnd()
        {
            this.dataWriter.Flush();
        }

        internal void WriteInt16(short value)
        {
            this.dataWriter.Write(value);
        }

        internal void WriteInt32(int value)
        {
            this.dataWriter.Write(value);
        }

        internal void WriteInt64(long value)
        {
            this.dataWriter.Write(value);
        }

        internal void WriteItem(NameInfo itemNameInfo, NameInfo typeNameInfo, object value)
        {
            this.InternalWriteItemNull();
            this.WriteMember(itemNameInfo, typeNameInfo, value);
        }

        internal void WriteItemEnd()
        {
            this.InternalWriteItemNull();
        }

        internal void WriteItemObjectRef(NameInfo nameInfo, int idRef)
        {
            this.InternalWriteItemNull();
            this.WriteMemberObjectRef(nameInfo, idRef);
        }

        internal void WriteJaggedArray(NameInfo memberNameInfo, NameInfo arrayNameInfo, WriteObjectInfo objectInfo, NameInfo arrayElemTypeNameInfo, int length, int lowerBound)
        {
            BinaryArrayTypeEnum jagged;
            this.InternalWriteItemNull();
            int[] lengthA = new int[] { length };
            int[] lowerBoundA = null;
            object typeInformation = null;
            int assemId = 0;
            if (lowerBound == 0)
            {
                jagged = BinaryArrayTypeEnum.Jagged;
            }
            else
            {
                jagged = BinaryArrayTypeEnum.JaggedOffset;
                lowerBoundA = new int[] { lowerBound };
            }
            BinaryTypeEnum binaryTypeEnum = BinaryConverter.GetBinaryTypeInfo(arrayElemTypeNameInfo.NItype, objectInfo, arrayElemTypeNameInfo.NIname, this.objectWriter, out typeInformation, out assemId);
            if (this.binaryArray == null)
            {
                this.binaryArray = new BinaryArray();
            }
            this.binaryArray.Set((int) arrayNameInfo.NIobjectId, 1, lengthA, lowerBoundA, binaryTypeEnum, typeInformation, jagged, assemId);
            long nIobjectId = arrayNameInfo.NIobjectId;
            this.binaryArray.Write(this);
        }

        internal void WriteMember(NameInfo memberNameInfo, NameInfo typeNameInfo, object value)
        {
            this.InternalWriteItemNull();
            InternalPrimitiveTypeE nIprimitiveTypeEnum = typeNameInfo.NIprimitiveTypeEnum;
            if (memberNameInfo.NItransmitTypeOnMember)
            {
                if (this.memberPrimitiveTyped == null)
                {
                    this.memberPrimitiveTyped = new MemberPrimitiveTyped();
                }
                this.memberPrimitiveTyped.Set(nIprimitiveTypeEnum, value);
                bool nIisArrayItem = memberNameInfo.NIisArrayItem;
                this.memberPrimitiveTyped.Dump();
                this.memberPrimitiveTyped.Write(this);
            }
            else
            {
                if (this.memberPrimitiveUnTyped == null)
                {
                    this.memberPrimitiveUnTyped = new MemberPrimitiveUnTyped();
                }
                this.memberPrimitiveUnTyped.Set(nIprimitiveTypeEnum, value);
                bool flag2 = memberNameInfo.NIisArrayItem;
                this.memberPrimitiveUnTyped.Dump();
                this.memberPrimitiveUnTyped.Write(this);
            }
        }

        internal void WriteMemberNested(NameInfo memberNameInfo)
        {
            this.InternalWriteItemNull();
            bool nIisArrayItem = memberNameInfo.NIisArrayItem;
        }

        internal void WriteMemberObjectRef(NameInfo memberNameInfo, int idRef)
        {
            this.InternalWriteItemNull();
            if (this.memberReference == null)
            {
                this.memberReference = new MemberReference();
            }
            this.memberReference.Set(idRef);
            bool nIisArrayItem = memberNameInfo.NIisArrayItem;
            this.memberReference.Dump();
            this.memberReference.Write(this);
        }

        internal void WriteMemberString(NameInfo memberNameInfo, NameInfo typeNameInfo, string value)
        {
            this.InternalWriteItemNull();
            bool nIisArrayItem = memberNameInfo.NIisArrayItem;
            this.WriteObjectString((int) typeNameInfo.NIobjectId, value);
        }

        internal void WriteMethodCall()
        {
            if (this.binaryMethodCall == null)
            {
                this.binaryMethodCall = new BinaryMethodCall();
            }
            this.binaryMethodCall.Dump();
            this.binaryMethodCall.Write(this);
        }

        internal void WriteMethodReturn()
        {
            if (this.binaryMethodReturn == null)
            {
                this.binaryMethodReturn = new BinaryMethodReturn();
            }
            this.binaryMethodReturn.Dump();
            this.binaryMethodReturn.Write(this);
        }

        internal void WriteNullItem(NameInfo itemNameInfo, NameInfo typeNameInfo)
        {
            this.nullCount++;
            this.InternalWriteItemNull();
        }

        internal void WriteNullMember(NameInfo memberNameInfo, NameInfo typeNameInfo)
        {
            this.InternalWriteItemNull();
            if (this.objectNull == null)
            {
                this.objectNull = new ObjectNull();
            }
            if (!memberNameInfo.NIisArrayItem)
            {
                this.objectNull.SetNullCount(1);
                this.objectNull.Dump();
                this.objectNull.Write(this);
                this.nullCount = 0;
            }
        }

        internal void WriteObject(NameInfo nameInfo, NameInfo typeNameInfo, int numMembers, string[] memberNames, Type[] memberTypes, WriteObjectInfo[] memberObjectInfos)
        {
            this.InternalWriteItemNull();
            int nIobjectId = (int) nameInfo.NIobjectId;
            string name = null;
            if (nIobjectId < 0)
            {
                name = typeNameInfo.NIname;
            }
            else
            {
                name = nameInfo.NIname;
            }
            if (this.objectMapTable == null)
            {
                this.objectMapTable = new Hashtable();
            }
            ObjectMapInfo info = (ObjectMapInfo) this.objectMapTable[name];
            if ((info != null) && info.isCompatible(numMembers, memberNames, memberTypes))
            {
                if (this.binaryObject == null)
                {
                    this.binaryObject = new BinaryObject();
                }
                this.binaryObject.Set(nIobjectId, info.objectId);
                this.binaryObject.Write(this);
            }
            else
            {
                int nIassemId;
                if (!typeNameInfo.NItransmitTypeOnObject)
                {
                    if (this.binaryObjectWithMap == null)
                    {
                        this.binaryObjectWithMap = new BinaryObjectWithMap();
                    }
                    nIassemId = (int) typeNameInfo.NIassemId;
                    this.binaryObjectWithMap.Set(nIobjectId, name, numMembers, memberNames, nIassemId);
                    this.binaryObjectWithMap.Dump();
                    this.binaryObjectWithMap.Write(this);
                    if (info == null)
                    {
                        this.objectMapTable.Add(name, new ObjectMapInfo(nIobjectId, numMembers, memberNames, memberTypes));
                    }
                }
                else
                {
                    BinaryTypeEnum[] binaryTypeEnumA = new BinaryTypeEnum[numMembers];
                    object[] typeInformationA = new object[numMembers];
                    int[] memberAssemIds = new int[numMembers];
                    for (int i = 0; i < numMembers; i++)
                    {
                        object typeInformation = null;
                        binaryTypeEnumA[i] = BinaryConverter.GetBinaryTypeInfo(memberTypes[i], memberObjectInfos[i], null, this.objectWriter, out typeInformation, out nIassemId);
                        typeInformationA[i] = typeInformation;
                        memberAssemIds[i] = nIassemId;
                    }
                    if (this.binaryObjectWithMapTyped == null)
                    {
                        this.binaryObjectWithMapTyped = new BinaryObjectWithMapTyped();
                    }
                    nIassemId = (int) typeNameInfo.NIassemId;
                    this.binaryObjectWithMapTyped.Set(nIobjectId, name, numMembers, memberNames, binaryTypeEnumA, typeInformationA, memberAssemIds, nIassemId);
                    this.binaryObjectWithMapTyped.Write(this);
                    if (info == null)
                    {
                        this.objectMapTable.Add(name, new ObjectMapInfo(nIobjectId, numMembers, memberNames, memberTypes));
                    }
                }
            }
        }

        [SecurityCritical]
        internal void WriteObjectByteArray(NameInfo memberNameInfo, NameInfo arrayNameInfo, WriteObjectInfo objectInfo, NameInfo arrayElemTypeNameInfo, int length, int lowerBound, byte[] byteA)
        {
            this.InternalWriteItemNull();
            this.WriteSingleArray(memberNameInfo, arrayNameInfo, objectInfo, arrayElemTypeNameInfo, length, lowerBound, byteA);
        }

        internal void WriteObjectEnd(NameInfo memberNameInfo, NameInfo typeNameInfo)
        {
        }

        internal void WriteObjectString(int objectId, string value)
        {
            this.InternalWriteItemNull();
            if (this.binaryObjectString == null)
            {
                this.binaryObjectString = new BinaryObjectString();
            }
            this.binaryObjectString.Set(objectId, value);
            this.binaryObjectString.Write(this);
        }

        internal void WriteRectangleArray(NameInfo memberNameInfo, NameInfo arrayNameInfo, WriteObjectInfo objectInfo, NameInfo arrayElemTypeNameInfo, int rank, int[] lengthA, int[] lowerBoundA)
        {
            this.InternalWriteItemNull();
            BinaryArrayTypeEnum rectangular = BinaryArrayTypeEnum.Rectangular;
            object typeInformation = null;
            int assemId = 0;
            BinaryTypeEnum binaryTypeEnum = BinaryConverter.GetBinaryTypeInfo(arrayElemTypeNameInfo.NItype, objectInfo, arrayElemTypeNameInfo.NIname, this.objectWriter, out typeInformation, out assemId);
            if (this.binaryArray == null)
            {
                this.binaryArray = new BinaryArray();
            }
            for (int i = 0; i < rank; i++)
            {
                if (lowerBoundA[i] != 0)
                {
                    rectangular = BinaryArrayTypeEnum.RectangularOffset;
                    break;
                }
            }
            this.binaryArray.Set((int) arrayNameInfo.NIobjectId, rank, lengthA, lowerBoundA, binaryTypeEnum, typeInformation, rectangular, assemId);
            long nIobjectId = arrayNameInfo.NIobjectId;
            this.binaryArray.Write(this);
        }

        internal object[] WriteReturnArray(object returnValue, object[] args, Exception exception, object callContext, object[] properties)
        {
            if (this.binaryMethodReturn == null)
            {
                this.binaryMethodReturn = new BinaryMethodReturn();
            }
            return this.binaryMethodReturn.WriteArray(returnValue, args, exception, callContext, properties);
        }

        internal void WriteSByte(sbyte value)
        {
            this.WriteByte((byte) value);
        }

        internal void WriteSerializationHeader(int topId, int headerId, int minorVersion, int majorVersion)
        {
            SerializationHeaderRecord record = new SerializationHeaderRecord(BinaryHeaderEnum.SerializedStreamHeader, topId, headerId, minorVersion, majorVersion);
            record.Dump();
            record.Write(this);
        }

        internal void WriteSerializationHeaderEnd()
        {
            MessageEnd end = new MessageEnd();
            end.Dump(this.sout);
            end.Write(this);
        }

        internal void WriteSingle(float value)
        {
            this.dataWriter.Write(value);
        }

        [SecurityCritical]
        internal void WriteSingleArray(NameInfo memberNameInfo, NameInfo arrayNameInfo, WriteObjectInfo objectInfo, NameInfo arrayElemTypeNameInfo, int length, int lowerBound, Array array)
        {
            BinaryArrayTypeEnum single;
            int num;
            this.InternalWriteItemNull();
            int[] lengthA = new int[] { length };
            int[] lowerBoundA = null;
            object typeInformation = null;
            if (lowerBound == 0)
            {
                single = BinaryArrayTypeEnum.Single;
            }
            else
            {
                single = BinaryArrayTypeEnum.SingleOffset;
                lowerBoundA = new int[] { lowerBound };
            }
            BinaryTypeEnum binaryTypeEnum = BinaryConverter.GetBinaryTypeInfo(arrayElemTypeNameInfo.NItype, objectInfo, arrayElemTypeNameInfo.NIname, this.objectWriter, out typeInformation, out num);
            if (this.binaryArray == null)
            {
                this.binaryArray = new BinaryArray();
            }
            this.binaryArray.Set((int) arrayNameInfo.NIobjectId, 1, lengthA, lowerBoundA, binaryTypeEnum, typeInformation, single, num);
            long nIobjectId = arrayNameInfo.NIobjectId;
            this.binaryArray.Write(this);
            if (Converter.IsWriteAsByteArray(arrayElemTypeNameInfo.NIprimitiveTypeEnum) && (lowerBound == 0))
            {
                if (arrayElemTypeNameInfo.NIprimitiveTypeEnum == InternalPrimitiveTypeE.Byte)
                {
                    this.WriteBytes((byte[]) array);
                }
                else if (arrayElemTypeNameInfo.NIprimitiveTypeEnum == InternalPrimitiveTypeE.Char)
                {
                    this.WriteChars((char[]) array);
                }
                else
                {
                    this.WriteArrayAsBytes(array, Converter.TypeLength(arrayElemTypeNameInfo.NIprimitiveTypeEnum));
                }
            }
        }

        internal void WriteString(string value)
        {
            this.dataWriter.Write(value);
        }

        internal void WriteTimeSpan(TimeSpan value)
        {
            this.WriteInt64(value.Ticks);
        }

        internal void WriteUInt16(ushort value)
        {
            this.dataWriter.Write(value);
        }

        internal void WriteUInt32(uint value)
        {
            this.dataWriter.Write(value);
        }

        internal void WriteUInt64(ulong value)
        {
            this.dataWriter.Write(value);
        }

        internal void WriteValue(InternalPrimitiveTypeE code, object value)
        {
            switch (code)
            {
                case InternalPrimitiveTypeE.Boolean:
                    this.WriteBoolean(Convert.ToBoolean(value, CultureInfo.InvariantCulture));
                    return;

                case InternalPrimitiveTypeE.Byte:
                    this.WriteByte(Convert.ToByte(value, CultureInfo.InvariantCulture));
                    return;

                case InternalPrimitiveTypeE.Char:
                    this.WriteChar(Convert.ToChar(value, CultureInfo.InvariantCulture));
                    return;

                case InternalPrimitiveTypeE.Decimal:
                    this.WriteDecimal(Convert.ToDecimal(value, CultureInfo.InvariantCulture));
                    return;

                case InternalPrimitiveTypeE.Double:
                    this.WriteDouble(Convert.ToDouble(value, CultureInfo.InvariantCulture));
                    return;

                case InternalPrimitiveTypeE.Int16:
                    this.WriteInt16(Convert.ToInt16(value, CultureInfo.InvariantCulture));
                    return;

                case InternalPrimitiveTypeE.Int32:
                    this.WriteInt32(Convert.ToInt32(value, CultureInfo.InvariantCulture));
                    return;

                case InternalPrimitiveTypeE.Int64:
                    this.WriteInt64(Convert.ToInt64(value, CultureInfo.InvariantCulture));
                    return;

                case InternalPrimitiveTypeE.SByte:
                    this.WriteSByte(Convert.ToSByte(value, CultureInfo.InvariantCulture));
                    return;

                case InternalPrimitiveTypeE.Single:
                    this.WriteSingle(Convert.ToSingle(value, CultureInfo.InvariantCulture));
                    return;

                case InternalPrimitiveTypeE.TimeSpan:
                    this.WriteTimeSpan((TimeSpan) value);
                    return;

                case InternalPrimitiveTypeE.DateTime:
                    this.WriteDateTime((DateTime) value);
                    return;

                case InternalPrimitiveTypeE.UInt16:
                    this.WriteUInt16(Convert.ToUInt16(value, CultureInfo.InvariantCulture));
                    return;

                case InternalPrimitiveTypeE.UInt32:
                    this.WriteUInt32(Convert.ToUInt32(value, CultureInfo.InvariantCulture));
                    return;

                case InternalPrimitiveTypeE.UInt64:
                    this.WriteUInt64(Convert.ToUInt64(value, CultureInfo.InvariantCulture));
                    return;
            }
            throw new SerializationException(Environment.GetResourceString("Serialization_TypeCode", new object[] { code.ToString() }));
        }
    }
}

