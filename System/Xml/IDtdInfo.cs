namespace System.Xml
{
    using System;
    using System.Collections.Generic;

    internal interface IDtdInfo
    {
        IEnumerable<IDtdAttributeListInfo> GetAttributeLists();
        IDtdAttributeListInfo LookupAttributeList(string prefix, string localName);
        IDtdEntityInfo LookupEntity(string name);

        bool HasDefaultAttributes { get; }

        bool HasNonCDataAttributes { get; }

        string InternalDtdSubset { get; }

        XmlQualifiedName Name { get; }
    }
}

