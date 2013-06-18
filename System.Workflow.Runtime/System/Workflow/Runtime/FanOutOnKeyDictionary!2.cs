namespace System.Workflow.Runtime
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal class FanOutOnKeyDictionary<K, V> : IEnumerable<Dictionary<K, V>>, IEnumerable
    {
        private Dictionary<int, Dictionary<K, V>> dictionaryDictionary;

        public FanOutOnKeyDictionary(int fanDegree)
        {
            this.dictionaryDictionary = new Dictionary<int, Dictionary<K, V>>(fanDegree);
            for (int i = 0; i < fanDegree; i++)
            {
                this.dictionaryDictionary.Add(i, new Dictionary<K, V>());
            }
        }

        public IEnumerator<Dictionary<K, V>> GetEnumerator()
        {
            return this.dictionaryDictionary.Values.GetEnumerator();
        }

        public bool SafeTryGetValue(K key, out V value)
        {
            Dictionary<K, V> dictionary = this[key];
            lock (dictionary)
            {
                return dictionary.TryGetValue(key, out value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.dictionaryDictionary.Values.GetEnumerator();
        }

        public Dictionary<K, V> this[K key]
        {
            get
            {
                return this.dictionaryDictionary[Math.Abs((int) (key.GetHashCode() % this.dictionaryDictionary.Count))];
            }
        }
    }
}

