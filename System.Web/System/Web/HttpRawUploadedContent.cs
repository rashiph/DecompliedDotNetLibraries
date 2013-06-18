namespace System.Web
{
    using System;
    using System.CodeDom.Compiler;
    using System.IO;
    using System.Reflection;
    using System.Security.Permissions;

    internal class HttpRawUploadedContent : IDisposable
    {
        private int _chunkLength;
        private int _chunkOffset;
        private bool _completed;
        private byte[] _data;
        private int _expectedLength;
        private TempFile _file;
        private int _fileThreshold;
        private int _length;

        internal HttpRawUploadedContent(int fileThreshold, int expectedLength)
        {
            this._fileThreshold = fileThreshold;
            this._expectedLength = expectedLength;
            if ((this._expectedLength >= 0) && (this._expectedLength < this._fileThreshold))
            {
                this._data = new byte[this._expectedLength];
            }
            else
            {
                this._data = new byte[this._fileThreshold];
            }
        }

        internal void AddBytes(byte[] data, int offset, int length)
        {
            if (this._completed)
            {
                throw new InvalidOperationException();
            }
            if (length > 0)
            {
                if (this._file == null)
                {
                    if ((this._length + length) <= this._data.Length)
                    {
                        Array.Copy(data, offset, this._data, this._length, length);
                        this._length += length;
                        return;
                    }
                    if ((this._length + length) <= this._fileThreshold)
                    {
                        byte[] destinationArray = new byte[this._fileThreshold];
                        if (this._length > 0)
                        {
                            Array.Copy(this._data, 0, destinationArray, 0, this._length);
                        }
                        Array.Copy(data, offset, destinationArray, this._length, length);
                        this._data = destinationArray;
                        this._length += length;
                        return;
                    }
                    this._file = new TempFile();
                    this._file.AddBytes(this._data, 0, this._length);
                }
                this._file.AddBytes(data, offset, length);
                this._length += length;
            }
        }

        internal void CopyBytes(int offset, byte[] buffer, int bufferOffset, int length)
        {
            if (!this._completed)
            {
                throw new InvalidOperationException();
            }
            if (this._file != null)
            {
                if ((offset >= this._chunkOffset) && ((offset + length) < (this._chunkOffset + this._chunkLength)))
                {
                    Array.Copy(this._data, offset - this._chunkOffset, buffer, bufferOffset, length);
                }
                else if (length <= this._data.Length)
                {
                    this._chunkLength = this._file.GetBytes(offset, this._data.Length, this._data, 0);
                    this._chunkOffset = offset;
                    Array.Copy(this._data, offset - this._chunkOffset, buffer, bufferOffset, length);
                }
                else
                {
                    this._file.GetBytes(offset, length, buffer, bufferOffset);
                }
            }
            else
            {
                Array.Copy(this._data, offset, buffer, bufferOffset, length);
            }
        }

        public void Dispose()
        {
            if (this._file != null)
            {
                this._file.Dispose();
            }
        }

        internal void DoneAddingBytes()
        {
            if (this._data == null)
            {
                this._data = new byte[0];
            }
            if (this._file != null)
            {
                this._file.DoneAddingBytes();
            }
            this._completed = true;
        }

        internal byte[] GetAsByteArray()
        {
            if ((this._file == null) && (this._length == this._data.Length))
            {
                return this._data;
            }
            return this.GetAsByteArray(0, this._length);
        }

        internal byte[] GetAsByteArray(int offset, int length)
        {
            if (!this._completed)
            {
                throw new InvalidOperationException();
            }
            if (length == 0)
            {
                return new byte[0];
            }
            byte[] buffer = new byte[length];
            this.CopyBytes(offset, buffer, 0, length);
            return buffer;
        }

        internal void WriteBytes(int offset, int length, Stream stream)
        {
            if (!this._completed)
            {
                throw new InvalidOperationException();
            }
            if (this._file != null)
            {
                int num = offset;
                int num2 = length;
                byte[] buffer = new byte[(num2 > this._fileThreshold) ? this._fileThreshold : num2];
                while (num2 > 0)
                {
                    int num3 = (num2 > this._fileThreshold) ? this._fileThreshold : num2;
                    int count = this._file.GetBytes(num, num3, buffer, 0);
                    if (count == 0)
                    {
                        return;
                    }
                    stream.Write(buffer, 0, count);
                    num += count;
                    num2 -= count;
                }
            }
            else
            {
                stream.Write(this._data, offset, length);
            }
        }

        internal byte this[int index]
        {
            get
            {
                if (!this._completed)
                {
                    throw new InvalidOperationException();
                }
                if (this._file == null)
                {
                    return this._data[index];
                }
                if ((index >= this._chunkOffset) && (index < (this._chunkOffset + this._chunkLength)))
                {
                    return this._data[index - this._chunkOffset];
                }
                if ((index < 0) || (index >= this._length))
                {
                    throw new ArgumentOutOfRangeException("index");
                }
                this._chunkLength = this._file.GetBytes(index, this._data.Length, this._data, 0);
                this._chunkOffset = index;
                return this._data[0];
            }
        }

        internal int Length
        {
            get
            {
                return this._length;
            }
        }

        private class TempFile : IDisposable
        {
            private string _filename;
            private Stream _filestream;
            private TempFileCollection _tempFiles;

            internal TempFile()
            {
                using (new ApplicationImpersonationContext())
                {
                    string path = Path.Combine(HttpRuntime.CodegenDirInternal, "uploads");
                    new FileIOPermission(FileIOPermissionAccess.AllAccess, path).Assert();
                    if (!Directory.Exists(path))
                    {
                        try
                        {
                            Directory.CreateDirectory(path);
                        }
                        catch
                        {
                        }
                    }
                    this._tempFiles = new TempFileCollection(path, false);
                    this._filename = this._tempFiles.AddExtension("post", false);
                    this._filestream = new FileStream(this._filename, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 0x1000, FileOptions.DeleteOnClose);
                }
            }

            internal void AddBytes(byte[] data, int offset, int length)
            {
                if (this._filestream == null)
                {
                    throw new InvalidOperationException();
                }
                this._filestream.Write(data, offset, length);
            }

            public void Dispose()
            {
                ApplicationImpersonationContext context = new ApplicationImpersonationContext();
                try
                {
                    if (this._filestream != null)
                    {
                        this._filestream.Close();
                    }
                    this._tempFiles.Delete();
                    ((IDisposable) this._tempFiles).Dispose();
                }
                catch
                {
                }
                finally
                {
                    if (context != null)
                    {
                        ((IDisposable) context).Dispose();
                    }
                }
            }

            internal void DoneAddingBytes()
            {
                if (this._filestream == null)
                {
                    throw new InvalidOperationException();
                }
                this._filestream.Flush();
                this._filestream.Seek(0L, SeekOrigin.Begin);
            }

            internal int GetBytes(int offset, int length, byte[] buffer, int bufferOffset)
            {
                if (this._filestream == null)
                {
                    throw new InvalidOperationException();
                }
                this._filestream.Seek((long) offset, SeekOrigin.Begin);
                return this._filestream.Read(buffer, bufferOffset, length);
            }
        }
    }
}

