namespace System.Net
{
    using System;

    internal enum WebHeaderCollectionType : ushort
    {
        FileWebRequest = 9,
        FileWebResponse = 10,
        FtpWebRequest = 7,
        FtpWebResponse = 8,
        HttpListenerRequest = 5,
        HttpListenerResponse = 6,
        HttpWebRequest = 3,
        HttpWebResponse = 4,
        Unknown = 0,
        WebRequest = 1,
        WebResponse = 2
    }
}

