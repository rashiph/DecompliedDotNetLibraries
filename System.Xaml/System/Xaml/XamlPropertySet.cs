namespace System.Xaml
{
    using System;
    using System.Collections.Generic;

    internal class XamlPropertySet
    {
        private Dictionary<XamlMember, bool> dictionary = new Dictionary<XamlMember, bool>();

        public void Add(XamlMember member)
        {
            this.dictionary.Add(member, true);
        }

        public bool Contains(XamlMember member)
        {
            return this.dictionary.ContainsKey(member);
        }
    }
}

