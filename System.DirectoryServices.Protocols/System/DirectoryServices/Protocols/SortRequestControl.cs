namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Runtime.InteropServices;

    public class SortRequestControl : DirectoryControl
    {
        private SortKey[] keys;

        public SortRequestControl(params SortKey[] sortKeys) : base("1.2.840.113556.1.4.473", null, true, true)
        {
            this.keys = new SortKey[0];
            if (sortKeys == null)
            {
                throw new ArgumentNullException("sortKeys");
            }
            for (int i = 0; i < sortKeys.Length; i++)
            {
                if (sortKeys[i] == null)
                {
                    throw new ArgumentException(Res.GetString("NullValueArray"), "sortKeys");
                }
            }
            this.keys = new SortKey[sortKeys.Length];
            for (int j = 0; j < sortKeys.Length; j++)
            {
                this.keys[j] = new SortKey(sortKeys[j].AttributeName, sortKeys[j].MatchingRule, sortKeys[j].ReverseOrder);
            }
        }

        public SortRequestControl(string attributeName, bool reverseOrder) : this(attributeName, null, reverseOrder)
        {
        }

        public SortRequestControl(string attributeName, string matchingRule, bool reverseOrder) : base("1.2.840.113556.1.4.473", null, true, true)
        {
            this.keys = new SortKey[0];
            SortKey key = new SortKey(attributeName, matchingRule, reverseOrder);
            this.keys = new SortKey[] { key };
        }

        public override byte[] GetValue()
        {
            IntPtr zero = IntPtr.Zero;
            int cb = Marshal.SizeOf(typeof(SortKey));
            int length = this.keys.Length;
            IntPtr keys = Marshal.AllocHGlobal((int) (Marshal.SizeOf(typeof(IntPtr)) * (length + 1)));
            try
            {
                IntPtr ptr = IntPtr.Zero;
                IntPtr ptr4 = IntPtr.Zero;
                int index = 0;
                index = 0;
                while (index < length)
                {
                    ptr4 = Marshal.AllocHGlobal(cb);
                    Marshal.StructureToPtr(this.keys[index], ptr4, false);
                    ptr = (IntPtr) (((long) keys) + (Marshal.SizeOf(typeof(IntPtr)) * index));
                    Marshal.WriteIntPtr(ptr, ptr4);
                    index++;
                }
                ptr = (IntPtr) (((long) keys) + (Marshal.SizeOf(typeof(IntPtr)) * index));
                Marshal.WriteIntPtr(ptr, IntPtr.Zero);
                bool isCritical = base.IsCritical;
                int errorCode = Wldap32.ldap_create_sort_control(UtilityHandle.GetHandle(), keys, isCritical ? ((byte) 1) : ((byte) 0), ref zero);
                if (errorCode != 0)
                {
                    if (Utility.IsLdapError((LdapError) errorCode))
                    {
                        string message = LdapErrorMappings.MapResultCode(errorCode);
                        throw new LdapException(errorCode, message);
                    }
                    throw new LdapException(errorCode);
                }
                LdapControl structure = new LdapControl();
                Marshal.PtrToStructure(zero, structure);
                berval berval = structure.ldctl_value;
                base.directoryControlValue = null;
                if (berval != null)
                {
                    base.directoryControlValue = new byte[berval.bv_len];
                    Marshal.Copy(berval.bv_val, base.directoryControlValue, 0, berval.bv_len);
                }
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Wldap32.ldap_control_free(zero);
                }
                if (keys != IntPtr.Zero)
                {
                    for (int i = 0; i < length; i++)
                    {
                        IntPtr ptr5 = Marshal.ReadIntPtr(keys, Marshal.SizeOf(typeof(IntPtr)) * i);
                        if (ptr5 != IntPtr.Zero)
                        {
                            IntPtr hglobal = Marshal.ReadIntPtr(ptr5);
                            if (hglobal != IntPtr.Zero)
                            {
                                Marshal.FreeHGlobal(hglobal);
                            }
                            hglobal = Marshal.ReadIntPtr(ptr5, Marshal.SizeOf(typeof(IntPtr)));
                            if (hglobal != IntPtr.Zero)
                            {
                                Marshal.FreeHGlobal(hglobal);
                            }
                            Marshal.FreeHGlobal(ptr5);
                        }
                    }
                    Marshal.FreeHGlobal(keys);
                }
            }
            return base.GetValue();
        }

        public SortKey[] SortKeys
        {
            get
            {
                if (this.keys == null)
                {
                    return new SortKey[0];
                }
                SortKey[] keyArray = new SortKey[this.keys.Length];
                for (int i = 0; i < this.keys.Length; i++)
                {
                    keyArray[i] = new SortKey(this.keys[i].AttributeName, this.keys[i].MatchingRule, this.keys[i].ReverseOrder);
                }
                return keyArray;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                for (int i = 0; i < value.Length; i++)
                {
                    if (value[i] == null)
                    {
                        throw new ArgumentException(Res.GetString("NullValueArray"), "value");
                    }
                }
                this.keys = new SortKey[value.Length];
                for (int j = 0; j < value.Length; j++)
                {
                    this.keys[j] = new SortKey(value[j].AttributeName, value[j].MatchingRule, value[j].ReverseOrder);
                }
            }
        }
    }
}

