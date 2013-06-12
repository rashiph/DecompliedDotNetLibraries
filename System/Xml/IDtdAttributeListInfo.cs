namespace System.Xml
{
    using System;
    using System.Collections.Generic;

    internal interface IDtdAttributeListInfo
    {
        IDtdAttributeInfo LookupAttribute(string prefix, string localName);
        IEnumerable<IDtdDefaultAttributeInfo> LookupDefaultAttributes();
        IDtdAttributeInfo LookupIdAttribute();

        bool HasNonCDataAttributes { get; }

        string LocalName { get; }

        string Prefix { get; }
    }
}

