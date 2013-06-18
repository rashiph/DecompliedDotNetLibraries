namespace System.Xml
{
    using System;
    using System.IO;

    public interface IFragmentCapableXmlDictionaryWriter
    {
        void EndFragment();
        void StartFragment(Stream stream, bool generateSelfContainedTextFragment);
        void WriteFragment(byte[] buffer, int offset, int count);

        bool CanFragment { get; }
    }
}

