namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.InteropServices;

    internal class UriCache
    {
        private int count;
        private Entry[] entries = new Entry[8];
        private const int MaxEntries = 8;
        private const int MaxKeyLength = 0x80;

        public Uri CreateUri(string uriString)
        {
            Uri uri = this.Get(uriString);
            if (uri == null)
            {
                uri = new Uri(uriString);
                this.Set(uriString, uri);
            }
            return uri;
        }

        private Uri Get(string key)
        {
            if (key.Length <= 0x80)
            {
                for (int i = this.count - 1; i >= 0; i--)
                {
                    if (this.entries[i].Key == key)
                    {
                        return this.entries[i].Value;
                    }
                }
            }
            return null;
        }

        private void Set(string key, Uri value)
        {
            if (key.Length <= 0x80)
            {
                if (this.count < this.entries.Length)
                {
                    this.entries[this.count++] = new Entry(key, value);
                }
                else
                {
                    Array.Copy(this.entries, 1, this.entries, 0, this.entries.Length - 1);
                    this.entries[this.count - 1] = new Entry(key, value);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Entry
        {
            private string key;
            private Uri value;
            public Entry(string key, Uri value)
            {
                this.key = key;
                this.value = value;
            }

            public string Key
            {
                get
                {
                    return this.key;
                }
            }
            public Uri Value
            {
                get
                {
                    return this.value;
                }
            }
        }
    }
}

