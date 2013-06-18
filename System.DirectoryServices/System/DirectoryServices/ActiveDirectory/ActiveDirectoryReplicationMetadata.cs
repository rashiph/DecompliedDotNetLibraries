namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class ActiveDirectoryReplicationMetadata : DictionaryBase
    {
        private ReadOnlyStringCollection dataNameCollection = new ReadOnlyStringCollection();
        private AttributeMetadataCollection dataValueCollection = new AttributeMetadataCollection();
        private Hashtable nameTable;
        private DirectoryServer server;

        internal ActiveDirectoryReplicationMetadata(DirectoryServer server)
        {
            this.server = server;
            Hashtable table = new Hashtable();
            this.nameTable = Hashtable.Synchronized(table);
        }

        private void Add(string name, AttributeMetadata value)
        {
            base.Dictionary.Add(name.ToLower(CultureInfo.InvariantCulture), value);
            this.dataNameCollection.Add(name);
            this.dataValueCollection.Add(value);
        }

        internal void AddHelper(int count, IntPtr info, bool advanced)
        {
            IntPtr zero = IntPtr.Zero;
            for (int i = 0; i < count; i++)
            {
                if (advanced)
                {
                    zero = (IntPtr) ((((long) info) + (Marshal.SizeOf(typeof(int)) * 2)) + (i * Marshal.SizeOf(typeof(DS_REPL_ATTR_META_DATA_2))));
                    AttributeMetadata metadata = new AttributeMetadata(zero, true, this.server, this.nameTable);
                    this.Add(metadata.Name, metadata);
                }
                else
                {
                    zero = (IntPtr) ((((long) info) + (Marshal.SizeOf(typeof(int)) * 2)) + (i * Marshal.SizeOf(typeof(DS_REPL_ATTR_META_DATA))));
                    AttributeMetadata metadata2 = new AttributeMetadata(zero, false, this.server, this.nameTable);
                    this.Add(metadata2.Name, metadata2);
                }
            }
        }

        public bool Contains(string attributeName)
        {
            string key = attributeName.ToLower(CultureInfo.InvariantCulture);
            return base.Dictionary.Contains(key);
        }

        public void CopyTo(AttributeMetadata[] array, int index)
        {
            base.Dictionary.Values.CopyTo(array, index);
        }

        public ReadOnlyStringCollection AttributeNames
        {
            get
            {
                return this.dataNameCollection;
            }
        }

        public AttributeMetadata this[string name]
        {
            get
            {
                string attributeName = name.ToLower(CultureInfo.InvariantCulture);
                if (this.Contains(attributeName))
                {
                    return (AttributeMetadata) base.InnerHashtable[attributeName];
                }
                return null;
            }
        }

        public AttributeMetadataCollection Values
        {
            get
            {
                return this.dataValueCollection;
            }
        }
    }
}

