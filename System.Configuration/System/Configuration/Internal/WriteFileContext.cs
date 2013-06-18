namespace System.Configuration.Internal
{
    using Microsoft.Win32;
    using System;
    using System.CodeDom.Compiler;
    using System.Configuration;
    using System.IO;
    using System.Security.AccessControl;
    using System.Threading;

    internal class WriteFileContext
    {
        private static PlatformID _osPlatform;
        private static bool _osPlatformDetermined = false;
        private TempFileCollection _tempFiles;
        private string _templateFilename;
        private string _tempNewFilename;
        private const int SAVING_RETRY_INTERVAL = 100;
        private const int SAVING_TIMEOUT = 0x2710;

        internal WriteFileContext(string filename, string templateFilename)
        {
            string directoryOrRootName = UrlPath.GetDirectoryOrRootName(filename);
            this._templateFilename = templateFilename;
            this._tempFiles = new TempFileCollection(directoryOrRootName);
            try
            {
                this._tempNewFilename = this._tempFiles.AddExtension("newcfg");
            }
            catch
            {
                ((IDisposable) this._tempFiles).Dispose();
                this._tempFiles = null;
                throw;
            }
        }

        private bool AttemptMove(string Source, string Target)
        {
            if (this.IsWinNT)
            {
                return Microsoft.Win32.UnsafeNativeMethods.MoveFileEx(Source, Target, 1);
            }
            try
            {
                File.Copy(Source, Target, true);
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal void Complete(string filename, bool success)
        {
            try
            {
                if (success)
                {
                    if (File.Exists(filename))
                    {
                        this.ValidateWriteAccess(filename);
                        this.DuplicateFileAttributes(filename, this._tempNewFilename);
                    }
                    else if (this._templateFilename != null)
                    {
                        this.DuplicateTemplateAttributes(this._templateFilename, this._tempNewFilename);
                    }
                    this.ReplaceFile(this._tempNewFilename, filename);
                    this._tempFiles.KeepFiles = true;
                }
            }
            finally
            {
                ((IDisposable) this._tempFiles).Dispose();
                this._tempFiles = null;
            }
        }

        private void DuplicateFileAttributes(string source, string destination)
        {
            FileAttributes fileAttributes = File.GetAttributes(source);
            File.SetAttributes(destination, fileAttributes);
            DateTime creationTimeUtc = File.GetCreationTimeUtc(source);
            File.SetCreationTimeUtc(destination, creationTimeUtc);
            this.DuplicateTemplateAttributes(source, destination);
        }

        private void DuplicateTemplateAttributes(string source, string destination)
        {
            if (this.IsWinNT)
            {
                FileSecurity accessControl = File.GetAccessControl(source, AccessControlSections.Access);
                accessControl.SetAccessRuleProtection(accessControl.AreAccessRulesProtected, true);
                File.SetAccessControl(destination, accessControl);
            }
            else
            {
                FileAttributes fileAttributes = File.GetAttributes(source);
                File.SetAttributes(destination, fileAttributes);
            }
        }

        private bool FileIsWriteLocked(string FileName)
        {
            Stream stream = null;
            bool flag = true;
            if (!FileUtil.FileExists(FileName, true))
            {
                return false;
            }
            try
            {
                FileShare read = FileShare.Read;
                if (this.IsWinNT)
                {
                    read |= FileShare.Delete;
                }
                stream = new FileStream(FileName, FileMode.Open, FileAccess.Read, read);
                flag = false;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream = null;
                }
            }
            return flag;
        }

        private void ReplaceFile(string Source, string Target)
        {
            bool flag = false;
            int num = 0;
            flag = this.AttemptMove(Source, Target);
            while ((!flag && (num < 0x2710)) && (File.Exists(Target) && !this.FileIsWriteLocked(Target)))
            {
                Thread.Sleep(100);
                num += 100;
                flag = this.AttemptMove(Source, Target);
            }
            if (!flag)
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_write_failed", new object[] { Target }));
            }
        }

        private void ValidateWriteAccess(string filename)
        {
            FileStream stream = null;
            try
            {
                stream = new FileStream(filename, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (IOException)
            {
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
        }

        private bool IsWinNT
        {
            get
            {
                if (!_osPlatformDetermined)
                {
                    _osPlatform = Environment.OSVersion.Platform;
                    _osPlatformDetermined = true;
                }
                return (_osPlatform == PlatformID.Win32NT);
            }
        }

        internal string TempNewFilename
        {
            get
            {
                return this._tempNewFilename;
            }
        }
    }
}

