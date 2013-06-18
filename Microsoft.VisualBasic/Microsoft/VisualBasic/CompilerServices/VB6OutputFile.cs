namespace Microsoft.VisualBasic.CompilerServices
{
    using Microsoft.VisualBasic;
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Security;

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class VB6OutputFile : VB6File
    {
        internal VB6OutputFile()
        {
        }

        internal VB6OutputFile(string FileName, OpenShare share, bool fAppend) : base(FileName, OpenAccess.Write, share, -1)
        {
            base.m_fAppend = fAppend;
        }

        internal override bool CanWrite()
        {
            return true;
        }

        internal override bool EOF()
        {
            return true;
        }

        public override OpenMode GetMode()
        {
            if (base.m_fAppend)
            {
                return OpenMode.Append;
            }
            return OpenMode.Output;
        }

        internal override long LOC()
        {
            return ((base.m_position + 0x7fL) / 0x80L);
        }

        internal override void OpenFile()
        {
            try
            {
                if (base.m_fAppend)
                {
                    if (File.Exists(base.m_sFullPath))
                    {
                        base.m_file = new FileStream(base.m_sFullPath, FileMode.Open, (FileAccess) base.m_access, (FileShare) base.m_share);
                    }
                    else
                    {
                        base.m_file = new FileStream(base.m_sFullPath, FileMode.Create, (FileAccess) base.m_access, (FileShare) base.m_share);
                    }
                }
                else
                {
                    base.m_file = new FileStream(base.m_sFullPath, FileMode.Create, (FileAccess) base.m_access, (FileShare) base.m_share);
                }
            }
            catch (FileNotFoundException exception)
            {
                throw ExceptionUtils.VbMakeException(exception, 0x35);
            }
            catch (SecurityException exception2)
            {
                throw ExceptionUtils.VbMakeException(exception2, 0x35);
            }
            catch (DirectoryNotFoundException exception3)
            {
                throw ExceptionUtils.VbMakeException(exception3, 0x4c);
            }
            catch (IOException exception4)
            {
                throw ExceptionUtils.VbMakeException(exception4, 0x4b);
            }
            base.m_Encoding = Utils.GetFileIOEncoding();
            base.m_sw = new StreamWriter(base.m_file, base.m_Encoding);
            base.m_sw.AutoFlush = true;
            if (base.m_fAppend)
            {
                long length = base.m_file.Length;
                base.m_file.Position = length;
                base.m_position = length;
            }
        }

        internal override void WriteLine(string s)
        {
            if (s == null)
            {
                base.m_sw.WriteLine();
                base.m_position += 2L;
            }
            else
            {
                if ((base.m_bPrint && (base.m_lWidth != 0)) && (base.m_lCurrentColumn >= base.m_lWidth))
                {
                    base.m_sw.WriteLine();
                    base.m_position += 2L;
                }
                base.m_sw.WriteLine(s);
                base.m_position += base.m_Encoding.GetByteCount(s) + 2;
            }
            base.m_lCurrentColumn = 0;
        }

        internal override void WriteString(string s)
        {
            if ((s != null) && (s.Length != 0))
            {
                if ((base.m_bPrint && (base.m_lWidth != 0)) && ((base.m_lCurrentColumn >= base.m_lWidth) || ((base.m_lCurrentColumn != 0) && ((base.m_lCurrentColumn + s.Length) > base.m_lWidth))))
                {
                    base.m_sw.WriteLine();
                    base.m_position += 2L;
                    base.m_lCurrentColumn = 0;
                }
                base.m_sw.Write(s);
                int byteCount = base.m_Encoding.GetByteCount(s);
                base.m_position += byteCount;
                base.m_lCurrentColumn += s.Length;
            }
        }
    }
}

