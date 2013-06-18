namespace Microsoft.VisualBasic.CompilerServices
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.IO;

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal sealed class AssemblyData
    {
        internal FileAttributes m_DirAttributes;
        internal FileSystemInfo[] m_DirFiles;
        internal int m_DirNextFileIndex;
        public ArrayList m_Files;

        internal AssemblyData()
        {
            ArrayList list = new ArrayList(0x100);
            object obj2 = null;
            int num = 0;
            do
            {
                list.Add(obj2);
                num++;
            }
            while (num <= 0xff);
            this.m_Files = list;
        }

        internal VB6File GetChannelObj(int lChannel)
        {
            object obj2;
            if (lChannel < this.m_Files.Count)
            {
                obj2 = this.m_Files[lChannel];
            }
            else
            {
                obj2 = null;
            }
            return (VB6File) obj2;
        }

        internal void SetChannelObj(int lChannel, VB6File oFile)
        {
            if (this.m_Files == null)
            {
                this.m_Files = new ArrayList(0x100);
            }
            if (oFile == null)
            {
                VB6File file = (VB6File) this.m_Files[lChannel];
                if (file != null)
                {
                    file.CloseFile();
                }
                this.m_Files[lChannel] = null;
            }
            else
            {
                object obj2 = oFile;
                this.m_Files[lChannel] = obj2;
            }
        }
    }
}

