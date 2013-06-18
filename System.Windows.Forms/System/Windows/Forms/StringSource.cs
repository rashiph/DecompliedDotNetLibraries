namespace System.Windows.Forms
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;

    internal class StringSource : IEnumString
    {
        private static Guid autoCompleteClsid = new Guid("{00BB2763-6A77-11D0-A535-00C04FD7D062}");
        private UnsafeNativeMethods.IAutoComplete2 autoCompleteObject2;
        private int current;
        private int size;
        private string[] strings;

        public StringSource(string[] strings)
        {
            Array.Clear(strings, 0, this.size);
            if (strings != null)
            {
                this.strings = strings;
            }
            this.current = 0;
            this.size = (strings == null) ? 0 : strings.Length;
            Guid gUID = typeof(UnsafeNativeMethods.IAutoComplete2).GUID;
            object obj2 = UnsafeNativeMethods.CoCreateInstance(ref autoCompleteClsid, null, 1, ref gUID);
            this.autoCompleteObject2 = (UnsafeNativeMethods.IAutoComplete2) obj2;
        }

        public bool Bind(HandleRef edit, int options)
        {
            bool flag = false;
            if (this.autoCompleteObject2 == null)
            {
                return flag;
            }
            try
            {
                this.autoCompleteObject2.SetOptions(options);
                this.autoCompleteObject2.Init(edit, this, null, null);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void RefreshList(string[] newSource)
        {
            Array.Clear(this.strings, 0, this.size);
            if (this.strings != null)
            {
                this.strings = newSource;
            }
            this.current = 0;
            this.size = (this.strings == null) ? 0 : this.strings.Length;
        }

        public void ReleaseAutoComplete()
        {
            if (this.autoCompleteObject2 != null)
            {
                Marshal.ReleaseComObject(this.autoCompleteObject2);
                this.autoCompleteObject2 = null;
            }
        }

        void IEnumString.Clone(out IEnumString ppenum)
        {
            ppenum = new StringSource(this.strings);
        }

        int IEnumString.Next(int celt, string[] rgelt, IntPtr pceltFetched)
        {
            if (celt < 0)
            {
                return -2147024809;
            }
            int index = 0;
            while ((this.current < this.size) && (celt > 0))
            {
                rgelt[index] = this.strings[this.current];
                this.current++;
                index++;
                celt--;
            }
            if (pceltFetched != IntPtr.Zero)
            {
                Marshal.WriteInt32(pceltFetched, index);
            }
            if (celt != 0)
            {
                return 1;
            }
            return 0;
        }

        void IEnumString.Reset()
        {
            this.current = 0;
        }

        int IEnumString.Skip(int celt)
        {
            this.current += celt;
            if (this.current >= this.size)
            {
                return 1;
            }
            return 0;
        }
    }
}

