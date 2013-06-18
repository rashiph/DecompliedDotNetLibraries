namespace System.Messaging.Interop
{
    using System;
    using System.Runtime.InteropServices;

    internal class MessagePropertyVariants
    {
        private int basePropertyId;
        private object[] handles;
        private GCHandle handleVectorIdentifiers;
        private GCHandle handleVectorProperties;
        private GCHandle handleVectorStatus;
        private int MAX_PROPERTIES;
        private object[] objects;
        private int propertyCount;
        private MQPROPS reference;
        private short[] variantTypes;
        private int[] vectorIdentifiers;
        private MQPROPVARIANTS[] vectorProperties;
        private int[] vectorStatus;
        public const short VT_ARRAY = 0x2000;
        public const short VT_BOOL = 11;
        public const short VT_BSTR = 8;
        public const short VT_CLSID = 0x48;
        public const short VT_CY = 6;
        public const short VT_DATE = 7;
        public const short VT_EMPTY = 0x7fff;
        public const short VT_I1 = 0x10;
        public const short VT_I2 = 2;
        public const short VT_I4 = 3;
        public const short VT_I8 = 20;
        public const short VT_LPSTR = 30;
        public const short VT_LPWSTR = 0x1f;
        public const short VT_NULL = 1;
        public const short VT_R4 = 4;
        public const short VT_R8 = 5;
        public const short VT_STORED_OBJECT = 0x45;
        public const short VT_STREAMED_OBJECT = 0x44;
        public const short VT_UI1 = 0x11;
        public const short VT_UI2 = 0x12;
        public const short VT_UI4 = 0x13;
        public const short VT_UI8 = 0x15;
        private const short VT_UNDEFINED = 0;
        public const short VT_VECTOR = 0x1000;

        public MessagePropertyVariants()
        {
            this.MAX_PROPERTIES = 0x3d;
            this.basePropertyId = 1;
            this.reference = new MQPROPS();
            this.variantTypes = new short[this.MAX_PROPERTIES];
            this.objects = new object[this.MAX_PROPERTIES];
            this.handles = new object[this.MAX_PROPERTIES];
        }

        internal MessagePropertyVariants(int maxProperties, int baseId)
        {
            this.MAX_PROPERTIES = 0x3d;
            this.basePropertyId = 1;
            this.reference = new MQPROPS();
            this.MAX_PROPERTIES = maxProperties;
            this.basePropertyId = baseId;
            this.variantTypes = new short[this.MAX_PROPERTIES];
            this.objects = new object[this.MAX_PROPERTIES];
            this.handles = new object[this.MAX_PROPERTIES];
        }

        public virtual void AdjustSize(int propertyId, int size)
        {
            this.handles[propertyId - this.basePropertyId] = (uint) size;
        }

        public byte[] GetGuid(int propertyId)
        {
            return (byte[]) this.objects[propertyId - this.basePropertyId];
        }

        public short GetI2(int propertyId)
        {
            return (short) this.objects[propertyId - this.basePropertyId];
        }

        public int GetI4(int propertyId)
        {
            return (int) this.objects[propertyId - this.basePropertyId];
        }

        public IntPtr GetIntPtr(int propertyId)
        {
            object obj2 = this.objects[propertyId - this.basePropertyId];
            if (obj2.GetType() == typeof(IntPtr))
            {
                return (IntPtr) obj2;
            }
            return IntPtr.Zero;
        }

        public byte[] GetString(int propertyId)
        {
            return (byte[]) this.objects[propertyId - this.basePropertyId];
        }

        public IntPtr GetStringVectorBasePointer(int propertyId)
        {
            return (IntPtr) this.handles[propertyId - this.basePropertyId];
        }

        public uint GetStringVectorLength(int propertyId)
        {
            return (uint) this.objects[propertyId - this.basePropertyId];
        }

        public byte GetUI1(int propertyId)
        {
            return (byte) this.objects[propertyId - this.basePropertyId];
        }

        public byte[] GetUI1Vector(int propertyId)
        {
            return (byte[]) this.objects[propertyId - this.basePropertyId];
        }

        public short GetUI2(int propertyId)
        {
            return (short) this.objects[propertyId - this.basePropertyId];
        }

        public int GetUI4(int propertyId)
        {
            return (int) this.objects[propertyId - this.basePropertyId];
        }

        public long GetUI8(int propertyId)
        {
            return (long) this.objects[propertyId - this.basePropertyId];
        }

        public virtual void Ghost(int propertyId)
        {
            if (this.variantTypes[propertyId - this.basePropertyId] != 0)
            {
                this.variantTypes[propertyId - this.basePropertyId] = 0;
                this.propertyCount--;
            }
        }

        public virtual MQPROPS Lock()
        {
            int[] numArray = new int[this.propertyCount];
            int[] numArray2 = new int[this.propertyCount];
            MQPROPVARIANTS[] mqpropvariantsArray = new MQPROPVARIANTS[this.propertyCount];
            int index = 0;
            for (int i = 0; i < this.MAX_PROPERTIES; i++)
            {
                short num3 = this.variantTypes[i];
                if (num3 != 0)
                {
                    numArray[index] = i + this.basePropertyId;
                    mqpropvariantsArray[index].vt = num3;
                    switch (num3)
                    {
                        case 0x11:
                        case 0x10:
                            mqpropvariantsArray[index].bVal = (byte) this.objects[i];
                            break;

                        case 0x1011:
                        {
                            if (this.handles[i] == null)
                            {
                                mqpropvariantsArray[index].caub.cElems = (uint) ((byte[]) this.objects[i]).Length;
                            }
                            else
                            {
                                mqpropvariantsArray[index].caub.cElems = (uint) this.handles[i];
                            }
                            GCHandle handle = GCHandle.Alloc(this.objects[i], GCHandleType.Pinned);
                            this.handles[i] = handle;
                            mqpropvariantsArray[index].caub.pElems = handle.AddrOfPinnedObject();
                            break;
                        }
                        case 0x12:
                        case 2:
                            mqpropvariantsArray[index].iVal = (short) this.objects[i];
                            break;

                        case 0x13:
                        case 3:
                            mqpropvariantsArray[index].lVal = (int) this.objects[i];
                            break;

                        case 0x15:
                        case 20:
                            mqpropvariantsArray[index].hVal = (long) this.objects[i];
                            break;

                        case 0x1f:
                        case 0x48:
                        {
                            GCHandle handle2 = GCHandle.Alloc(this.objects[i], GCHandleType.Pinned);
                            this.handles[i] = handle2;
                            mqpropvariantsArray[index].ptr = handle2.AddrOfPinnedObject();
                            break;
                        }
                        default:
                            if (num3 == 0x7fff)
                            {
                                mqpropvariantsArray[index].vt = 0;
                            }
                            break;
                    }
                    index++;
                    if (this.propertyCount == index)
                    {
                        break;
                    }
                }
            }
            this.handleVectorIdentifiers = GCHandle.Alloc(numArray, GCHandleType.Pinned);
            this.handleVectorProperties = GCHandle.Alloc(mqpropvariantsArray, GCHandleType.Pinned);
            this.handleVectorStatus = GCHandle.Alloc(numArray2, GCHandleType.Pinned);
            this.vectorIdentifiers = numArray;
            this.vectorStatus = numArray2;
            this.vectorProperties = mqpropvariantsArray;
            this.reference.propertyCount = this.propertyCount;
            this.reference.propertyIdentifiers = this.handleVectorIdentifiers.AddrOfPinnedObject();
            this.reference.propertyValues = this.handleVectorProperties.AddrOfPinnedObject();
            this.reference.status = this.handleVectorStatus.AddrOfPinnedObject();
            return this.reference;
        }

        public virtual void Remove(int propertyId)
        {
            if (this.variantTypes[propertyId - this.basePropertyId] != 0)
            {
                this.variantTypes[propertyId - this.basePropertyId] = 0;
                this.objects[propertyId - this.basePropertyId] = null;
                this.handles[propertyId - this.basePropertyId] = null;
                this.propertyCount--;
            }
        }

        public virtual void SetEmpty(int propertyId)
        {
            if (this.variantTypes[propertyId - this.basePropertyId] == 0)
            {
                this.variantTypes[propertyId - this.basePropertyId] = 0x7fff;
                this.propertyCount++;
            }
            this.objects[propertyId - this.basePropertyId] = null;
        }

        public void SetGuid(int propertyId, byte[] value)
        {
            if (this.variantTypes[propertyId - this.basePropertyId] == 0)
            {
                this.variantTypes[propertyId - this.basePropertyId] = 0x48;
                this.propertyCount++;
            }
            this.objects[propertyId - this.basePropertyId] = value;
        }

        public void SetI2(int propertyId, short value)
        {
            if (this.variantTypes[propertyId - this.basePropertyId] == 0)
            {
                this.variantTypes[propertyId - this.basePropertyId] = 2;
                this.propertyCount++;
            }
            this.objects[propertyId - this.basePropertyId] = value;
        }

        public void SetI4(int propertyId, int value)
        {
            if (this.variantTypes[propertyId - this.basePropertyId] == 0)
            {
                this.variantTypes[propertyId - this.basePropertyId] = 3;
                this.propertyCount++;
            }
            this.objects[propertyId - this.basePropertyId] = value;
        }

        public virtual void SetNull(int propertyId)
        {
            if (this.variantTypes[propertyId - this.basePropertyId] == 0)
            {
                this.variantTypes[propertyId - this.basePropertyId] = 1;
                this.propertyCount++;
            }
            this.objects[propertyId - this.basePropertyId] = null;
        }

        public void SetString(int propertyId, byte[] value)
        {
            if (this.variantTypes[propertyId - this.basePropertyId] == 0)
            {
                this.variantTypes[propertyId - this.basePropertyId] = 0x1f;
                this.propertyCount++;
            }
            this.objects[propertyId - this.basePropertyId] = value;
        }

        public void SetUI1(int propertyId, byte value)
        {
            if (this.variantTypes[propertyId - this.basePropertyId] == 0)
            {
                this.variantTypes[propertyId - this.basePropertyId] = 0x11;
                this.propertyCount++;
            }
            this.objects[propertyId - this.basePropertyId] = value;
        }

        public void SetUI1Vector(int propertyId, byte[] value)
        {
            if (this.variantTypes[propertyId - this.basePropertyId] == 0)
            {
                this.variantTypes[propertyId - this.basePropertyId] = 0x1011;
                this.propertyCount++;
            }
            this.objects[propertyId - this.basePropertyId] = value;
        }

        public void SetUI2(int propertyId, short value)
        {
            if (this.variantTypes[propertyId - this.basePropertyId] == 0)
            {
                this.variantTypes[propertyId - this.basePropertyId] = 0x12;
                this.propertyCount++;
            }
            this.objects[propertyId - this.basePropertyId] = value;
        }

        public void SetUI4(int propertyId, int value)
        {
            if (this.variantTypes[propertyId - this.basePropertyId] == 0)
            {
                this.variantTypes[propertyId - this.basePropertyId] = 0x13;
                this.propertyCount++;
            }
            this.objects[propertyId - this.basePropertyId] = value;
        }

        public void SetUI8(int propertyId, long value)
        {
            if (this.variantTypes[propertyId - this.basePropertyId] == 0)
            {
                this.variantTypes[propertyId - this.basePropertyId] = 0x15;
                this.propertyCount++;
            }
            this.objects[propertyId - this.basePropertyId] = value;
        }

        public virtual void Unlock()
        {
            for (int i = 0; i < this.vectorIdentifiers.Length; i++)
            {
                short vt = this.vectorProperties[i].vt;
                if (this.variantTypes[this.vectorIdentifiers[i] - this.basePropertyId] == 1)
                {
                    switch (vt)
                    {
                        case 0x1011:
                        case 1:
                        {
                            this.objects[this.vectorIdentifiers[i] - this.basePropertyId] = this.vectorProperties[i].caub.cElems;
                            continue;
                        }
                    }
                    if (vt == 0x101f)
                    {
                        this.objects[this.vectorIdentifiers[i] - this.basePropertyId] = this.vectorProperties[i * 4].caub.cElems;
                        this.handles[this.vectorIdentifiers[i] - this.basePropertyId] = this.vectorProperties[i * 4].caub.pElems;
                    }
                    else
                    {
                        this.objects[this.vectorIdentifiers[i] - this.basePropertyId] = this.vectorProperties[i].ptr;
                    }
                }
                else
                {
                    switch (vt)
                    {
                        case 0x1f:
                        case 0x48:
                        case 0x1011:
                            ((GCHandle) this.handles[this.vectorIdentifiers[i] - this.basePropertyId]).Free();
                            this.handles[this.vectorIdentifiers[i] - this.basePropertyId] = null;
                            break;

                        case 0x11:
                        case 0x10:
                            this.objects[this.vectorIdentifiers[i] - this.basePropertyId] = this.vectorProperties[i].bVal;
                            break;

                        case 0x12:
                        case 2:
                            this.objects[this.vectorIdentifiers[i] - this.basePropertyId] = this.vectorProperties[i].iVal;
                            break;

                        case 0x13:
                        case 3:
                            this.objects[this.vectorIdentifiers[i] - this.basePropertyId] = this.vectorProperties[i].lVal;
                            break;

                        case 0x15:
                        case 20:
                            this.objects[this.vectorIdentifiers[i] - this.basePropertyId] = this.vectorProperties[i].hVal;
                            break;
                    }
                }
            }
            this.handleVectorIdentifiers.Free();
            this.handleVectorProperties.Free();
            this.handleVectorStatus.Free();
        }

        [StructLayout(LayoutKind.Sequential)]
        public class MQPROPS
        {
            internal int propertyCount;
            internal IntPtr propertyIdentifiers;
            internal IntPtr propertyValues;
            internal IntPtr status;
        }
    }
}

