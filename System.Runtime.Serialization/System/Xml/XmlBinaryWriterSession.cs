namespace System.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    public class XmlBinaryWriterSession
    {
        private PriorityDictionary<IXmlDictionary, IntArray> maps = new PriorityDictionary<IXmlDictionary, IntArray>();
        private int nextKey = 0;
        private PriorityDictionary<string, int> strings = new PriorityDictionary<string, int>();

        private int Add(string s)
        {
            int num = this.nextKey++;
            this.strings.Add(s, num);
            return num;
        }

        private IntArray AddKeys(IXmlDictionary dictionary, int minCount)
        {
            IntArray array = new IntArray(Math.Max(minCount, 0x10));
            this.maps.Add(dictionary, array);
            return array;
        }

        public void Reset()
        {
            this.nextKey = 0;
            this.maps.Clear();
            this.strings.Clear();
        }

        public virtual bool TryAdd(XmlDictionaryString value, out int key)
        {
            IntArray array;
            if (value == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
            }
            if (this.maps.TryGetValue(value.Dictionary, out array))
            {
                key = array[value.Key] - 1;
                if (key != -1)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("XmlKeyAlreadyExists")));
                }
                key = this.Add(value.Value);
                array[value.Key] = key + 1;
                return true;
            }
            key = this.Add(value.Value);
            this.AddKeys(value.Dictionary, value.Key + 1)[value.Key] = key + 1;
            return true;
        }

        internal bool TryLookup(XmlDictionaryString s, out int key)
        {
            IntArray array;
            if (this.maps.TryGetValue(s.Dictionary, out array))
            {
                key = array[s.Key] - 1;
                if (key != -1)
                {
                    return true;
                }
            }
            if (this.strings.TryGetValue(s.Value, out key))
            {
                if (array == null)
                {
                    array = this.AddKeys(s.Dictionary, s.Key + 1);
                }
                array[s.Key] = key + 1;
                return true;
            }
            key = -1;
            return false;
        }

        private class IntArray
        {
            private int[] array;

            public IntArray(int size)
            {
                this.array = new int[size];
            }

            public int this[int index]
            {
                get
                {
                    if (index >= this.array.Length)
                    {
                        return 0;
                    }
                    return this.array[index];
                }
                set
                {
                    if (index >= this.array.Length)
                    {
                        int[] destinationArray = new int[Math.Max((int) (index + 1), (int) (this.array.Length * 2))];
                        Array.Copy(this.array, destinationArray, this.array.Length);
                        this.array = destinationArray;
                    }
                    this.array[index] = value;
                }
            }
        }

        private class PriorityDictionary<K, V> where K: class
        {
            private Dictionary<K, V> dictionary;
            private Entry<K, V>[] list;
            private int listCount;
            private int now;

            public PriorityDictionary()
            {
                this.list = new Entry<K, V>[0x10];
            }

            public void Add(K key, V value)
            {
                if (this.listCount < this.list.Length)
                {
                    this.list[this.listCount].Key = key;
                    this.list[this.listCount].Value = value;
                    this.listCount++;
                }
                else
                {
                    if (this.dictionary == null)
                    {
                        this.dictionary = new Dictionary<K, V>();
                        for (int i = 0; i < this.listCount; i++)
                        {
                            this.dictionary.Add(this.list[i].Key, this.list[i].Value);
                        }
                    }
                    this.dictionary.Add(key, value);
                }
            }

            public void Clear()
            {
                this.now = 0;
                this.listCount = 0;
                Array.Clear(this.list, 0, this.list.Length);
                if (this.dictionary != null)
                {
                    this.dictionary.Clear();
                }
            }

            private void DecreaseAll()
            {
                for (int i = 0; i < this.listCount; i++)
                {
                    this.list[i].Time /= 2;
                }
                this.now /= 2;
            }

            public bool TryGetValue(K key, out V value)
            {
                for (int i = 0; i < this.listCount; i++)
                {
                    if (this.list[i].Key == key)
                    {
                        value = this.list[i].Value;
                        this.list[i].Time = this.Now;
                        return true;
                    }
                }
                for (int j = 0; j < this.listCount; j++)
                {
                    if (this.list[j].Key.Equals(key))
                    {
                        value = this.list[j].Value;
                        this.list[j].Time = this.Now;
                        return true;
                    }
                }
                if (this.dictionary == null)
                {
                    value = default(V);
                    return false;
                }
                if (!this.dictionary.TryGetValue(key, out value))
                {
                    return false;
                }
                int index = 0;
                int time = this.list[0].Time;
                for (int k = 1; k < this.listCount; k++)
                {
                    if (this.list[k].Time < time)
                    {
                        index = k;
                        time = this.list[k].Time;
                    }
                }
                this.list[index].Key = key;
                this.list[index].Value = value;
                this.list[index].Time = this.Now;
                return true;
            }

            private int Now
            {
                get
                {
                    if (++this.now == 0x7fffffff)
                    {
                        this.DecreaseAll();
                    }
                    return this.now;
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct Entry
            {
                public K Key;
                public V Value;
                public int Time;
            }
        }
    }
}

