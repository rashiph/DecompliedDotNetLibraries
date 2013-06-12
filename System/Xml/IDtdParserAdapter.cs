namespace System.Xml
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    internal interface IDtdParserAdapter
    {
        void OnNewLine(int pos);
        void OnPublicId(string publicId, LineInfo keywordLineInfo, LineInfo publicLiteralLineInfo);
        void OnSystemId(string systemId, LineInfo keywordLineInfo, LineInfo systemLiteralLineInfo);
        void ParseComment(StringBuilder sb);
        int ParseNamedCharRef(bool expand, StringBuilder internalSubsetBuilder);
        int ParseNumericCharRef(StringBuilder internalSubsetBuilder);
        void ParsePI(StringBuilder sb);
        bool PopEntity(out IDtdEntityInfo oldEntity, out int newEntityId);
        bool PushEntity(IDtdEntityInfo entity, out int entityId);
        bool PushExternalSubset(string systemId, string publicId);
        void PushInternalDtd(string baseUri, string internalDtd);
        int ReadData();
        void Throw(Exception e);

        Uri BaseUri { get; }

        int CurrentPosition { get; set; }

        int EntityStackLength { get; }

        bool IsEntityEolNormalized { get; }

        bool IsEof { get; }

        int LineNo { get; }

        int LineStartPosition { get; }

        IXmlNamespaceResolver NamespaceResolver { get; }

        XmlNameTable NameTable { get; }

        char[] ParsingBuffer { get; }

        int ParsingBufferLength { get; }
    }
}

