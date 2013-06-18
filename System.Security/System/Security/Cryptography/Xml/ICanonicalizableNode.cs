namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    internal interface ICanonicalizableNode
    {
        void Write(StringBuilder strBuilder, DocPosition docPos, AncestralNamespaceContextManager anc);
        void WriteHash(HashAlgorithm hash, DocPosition docPos, AncestralNamespaceContextManager anc);

        bool IsInNodeSet { get; set; }
    }
}

