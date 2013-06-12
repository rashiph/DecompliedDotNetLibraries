namespace System.Web.UI
{
    using System;
    using System.Collections;

    internal class TagNamespaceRegisterEntryTable : Hashtable
    {
        public TagNamespaceRegisterEntryTable() : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public override object Clone()
        {
            TagNamespaceRegisterEntryTable table = new TagNamespaceRegisterEntryTable();
            foreach (DictionaryEntry entry in this)
            {
                table[entry.Key] = ((ArrayList) entry.Value).Clone();
            }
            return table;
        }
    }
}

