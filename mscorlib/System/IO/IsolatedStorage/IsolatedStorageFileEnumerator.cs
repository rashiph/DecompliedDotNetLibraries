namespace System.IO.IsolatedStorage
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;

    internal sealed class IsolatedStorageFileEnumerator : IEnumerator
    {
        private IsolatedStorageFile m_Current;
        private bool m_fEnd;
        private TwoLevelFileEnumerator m_fileEnum;
        private FileIOPermission m_fiop;
        private bool m_fReset;
        private string m_rootDir;
        private IsolatedStorageScope m_Scope;
        private const char s_SepExternal = '\\';

        [SecurityCritical]
        internal IsolatedStorageFileEnumerator(IsolatedStorageScope scope)
        {
            this.m_Scope = scope;
            this.m_fiop = IsolatedStorageFile.GetGlobalFileIOPerm(scope);
            this.m_rootDir = IsolatedStorageFile.GetRootDir(scope);
            this.m_fileEnum = new TwoLevelFileEnumerator(this.m_rootDir);
            this.Reset();
        }

        private bool GetIDStream(string path, out Stream s)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(this.m_rootDir);
            builder.Append(path);
            builder.Append('\\');
            builder.Append("identity.dat");
            s = null;
            try
            {
                byte[] buffer;
                using (FileStream stream = new FileStream(builder.ToString(), FileMode.Open))
                {
                    int length = (int) stream.Length;
                    buffer = new byte[length];
                    int offset = 0;
                    while (length > 0)
                    {
                        int num3 = stream.Read(buffer, offset, length);
                        if (num3 == 0)
                        {
                            __Error.EndOfFile();
                        }
                        offset += num3;
                        length -= num3;
                    }
                }
                s = new MemoryStream(buffer);
            }
            catch
            {
                return false;
            }
            return true;
        }

        [SecuritySafeCritical]
        public bool MoveNext()
        {
            IsolatedStorageScope scope;
            string str;
            string str2;
            string str3;
            this.m_fiop.Assert();
            this.m_fReset = false;
        Label_0012:
            if (!this.m_fileEnum.MoveNext())
            {
                this.m_fEnd = true;
                return false;
            }
            IsolatedStorageFile file = new IsolatedStorageFile();
            TwoPaths current = (TwoPaths) this.m_fileEnum.Current;
            bool flag = false;
            if (IsolatedStorageFile.NotAssemFilesDir(current.Path2) && IsolatedStorageFile.NotAppFilesDir(current.Path2))
            {
                flag = true;
            }
            Stream s = null;
            Stream stream2 = null;
            Stream stream3 = null;
            if (flag)
            {
                if (!this.GetIDStream(current.Path1, out s) || !this.GetIDStream(current.Path1 + '\\' + current.Path2, out stream2))
                {
                    goto Label_0012;
                }
                s.Position = 0L;
                if (System.IO.IsolatedStorage.IsolatedStorage.IsRoaming(this.m_Scope))
                {
                    scope = IsolatedStorageScope.Roaming | IsolatedStorageScope.Assembly | IsolatedStorageScope.Domain | IsolatedStorageScope.User;
                }
                else if (System.IO.IsolatedStorage.IsolatedStorage.IsMachine(this.m_Scope))
                {
                    scope = IsolatedStorageScope.Machine | IsolatedStorageScope.Assembly | IsolatedStorageScope.Domain;
                }
                else
                {
                    scope = IsolatedStorageScope.Assembly | IsolatedStorageScope.Domain | IsolatedStorageScope.User;
                }
                str = current.Path1;
                str2 = current.Path2;
                str3 = null;
            }
            else if (IsolatedStorageFile.NotAppFilesDir(current.Path2))
            {
                if (!this.GetIDStream(current.Path1, out stream2))
                {
                    goto Label_0012;
                }
                if (System.IO.IsolatedStorage.IsolatedStorage.IsRoaming(this.m_Scope))
                {
                    scope = IsolatedStorageScope.Roaming | IsolatedStorageScope.Assembly | IsolatedStorageScope.User;
                }
                else if (System.IO.IsolatedStorage.IsolatedStorage.IsMachine(this.m_Scope))
                {
                    scope = IsolatedStorageScope.Machine | IsolatedStorageScope.Assembly;
                }
                else
                {
                    scope = IsolatedStorageScope.Assembly | IsolatedStorageScope.User;
                }
                str = null;
                str2 = current.Path1;
                str3 = null;
                stream2.Position = 0L;
            }
            else
            {
                if (!this.GetIDStream(current.Path1, out stream3))
                {
                    goto Label_0012;
                }
                if (System.IO.IsolatedStorage.IsolatedStorage.IsRoaming(this.m_Scope))
                {
                    scope = IsolatedStorageScope.Application | IsolatedStorageScope.Roaming | IsolatedStorageScope.User;
                }
                else if (System.IO.IsolatedStorage.IsolatedStorage.IsMachine(this.m_Scope))
                {
                    scope = IsolatedStorageScope.Application | IsolatedStorageScope.Machine;
                }
                else
                {
                    scope = IsolatedStorageScope.Application | IsolatedStorageScope.User;
                }
                str = null;
                str2 = null;
                str3 = current.Path1;
                stream3.Position = 0L;
            }
            if (!file.InitStore(scope, s, stream2, stream3, str, str2, str3) || !file.InitExistingStore(scope))
            {
                goto Label_0012;
            }
            this.m_Current = file;
            return true;
        }

        public void Reset()
        {
            this.m_Current = null;
            this.m_fReset = true;
            this.m_fEnd = false;
            this.m_fileEnum.Reset();
        }

        public object Current
        {
            get
            {
                if (this.m_fReset)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumNotStarted"));
                }
                if (this.m_fEnd)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EnumEnded"));
                }
                return this.m_Current;
            }
        }
    }
}

