namespace System.Reflection.Emit
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Resources;

    internal class ResWriterData
    {
        internal ResourceAttributes m_attribute;
        internal Stream m_memoryStream;
        internal ResWriterData m_nextResWriter;
        internal ResourceWriter m_resWriter;
        internal string m_strFileName;
        internal string m_strFullFileName;
        internal string m_strName;

        internal ResWriterData(ResourceWriter resWriter, Stream memoryStream, string strName, string strFileName, string strFullFileName, ResourceAttributes attribute)
        {
            this.m_resWriter = resWriter;
            this.m_memoryStream = memoryStream;
            this.m_strName = strName;
            this.m_strFileName = strFileName;
            this.m_strFullFileName = strFullFileName;
            this.m_nextResWriter = null;
            this.m_attribute = attribute;
        }
    }
}

