namespace System.Web.Configuration
{
    using System;
    using System.Runtime.InteropServices;

    [Guid("A99B591A-23C6-4238-8452-C7B0E895063D"), ComVisible(true)]
    public interface IRemoteWebConfigurationHostServer
    {
        byte[] GetData(string fileName, bool getReadTimeOnly, out long readTime);
        void WriteData(string fileName, string templateFileName, byte[] data, ref long readTime);
        string GetFilePaths(int webLevel, string path, string site, string locationSubPath);
        string DoEncryptOrDecrypt(bool doEncrypt, string xmlString, string protectionProviderName, string protectionProviderType, string[] parameterKeys, string[] parameterValues);
        void GetFileDetails(string name, out bool exists, out long size, out long createDate, out long lastWriteDate);
    }
}

