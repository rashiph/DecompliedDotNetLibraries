namespace System.Xml
{
    using System;
    using System.Data;

    internal interface IXmlDataVirtualNode
    {
        bool IsInUse();
        bool IsOnColumn(DataColumn col);
        bool IsOnNode(XmlNode nodeToCheck);
        void OnFoliated(XmlNode foliatedNode);
    }
}

