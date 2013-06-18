namespace System.Messaging.Interop
{
    using System;
    using System.Runtime.InteropServices;

    internal class Restrictions
    {
        public const int PREQ = 4;
        public const int PRGE = 3;
        public const int PRGT = 2;
        public const int PRLE = 1;
        public const int PRLT = 0;
        public const int PRNE = 5;
        private MQRESTRICTION restrictionStructure;

        public Restrictions(int maxRestrictions)
        {
            this.restrictionStructure = new MQRESTRICTION(maxRestrictions);
        }

        public virtual void AddGuid(int propertyId, int op)
        {
            IntPtr data = Marshal.AllocHGlobal(0x10);
            this.AddItem(propertyId, op, 0x48, data);
        }

        public virtual void AddGuid(int propertyId, int op, Guid value)
        {
            IntPtr destination = Marshal.AllocHGlobal(0x10);
            Marshal.Copy(value.ToByteArray(), 0, destination, 0x10);
            this.AddItem(propertyId, op, 0x48, destination);
        }

        public virtual void AddI4(int propertyId, int op, int value)
        {
            this.AddItem(propertyId, op, 3, (IntPtr) value);
        }

        private void AddItem(int id, int op, short vt, IntPtr data)
        {
            Marshal.WriteInt32(this.restrictionStructure.GetNextValidPtr(0), op);
            Marshal.WriteInt32(this.restrictionStructure.GetNextValidPtr(4), id);
            Marshal.WriteInt16(this.restrictionStructure.GetNextValidPtr(8), vt);
            Marshal.WriteInt16(this.restrictionStructure.GetNextValidPtr(10), (short) 0);
            Marshal.WriteInt16(this.restrictionStructure.GetNextValidPtr(12), (short) 0);
            Marshal.WriteInt16(this.restrictionStructure.GetNextValidPtr(14), (short) 0);
            Marshal.WriteIntPtr(this.restrictionStructure.GetNextValidPtr(0x10), data);
            Marshal.WriteIntPtr(this.restrictionStructure.GetNextValidPtr(0x10 + IntPtr.Size), IntPtr.Zero);
            this.restrictionStructure.restrictionCount++;
        }

        public virtual void AddString(int propertyId, int op, string value)
        {
            if (value == null)
            {
                this.AddItem(propertyId, op, 1, IntPtr.Zero);
            }
            else
            {
                IntPtr data = Marshal.StringToHGlobalUni(value);
                this.AddItem(propertyId, op, 0x1f, data);
            }
        }

        public virtual MQRESTRICTION GetRestrictionsRef()
        {
            return this.restrictionStructure;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class MQRESTRICTION
        {
            public int restrictionCount;
            public IntPtr restrinctions;
            ~MQRESTRICTION()
            {
                if (this.restrinctions != IntPtr.Zero)
                {
                    for (int i = 0; i < this.restrictionCount; i++)
                    {
                        if (Marshal.ReadInt16((IntPtr) ((((long) this.restrinctions) + (i * GetRestrictionSize())) + 8L)) != 3)
                        {
                            IntPtr ptr = (IntPtr) ((((long) this.restrinctions) + (i * GetRestrictionSize())) + 0x10L);
                            Marshal.FreeHGlobal(Marshal.ReadIntPtr(ptr));
                        }
                    }
                    Marshal.FreeHGlobal(this.restrinctions);
                    this.restrinctions = IntPtr.Zero;
                }
            }

            public IntPtr GetNextValidPtr(int offset)
            {
                return (IntPtr) ((((long) this.restrinctions) + (this.restrictionCount * GetRestrictionSize())) + offset);
            }

            public MQRESTRICTION(int maxCount)
            {
                this.restrinctions = Marshal.AllocHGlobal((int) (maxCount * GetRestrictionSize()));
            }

            public static int GetRestrictionSize()
            {
                return (0x10 + (IntPtr.Size * 2));
            }
        }
    }
}

