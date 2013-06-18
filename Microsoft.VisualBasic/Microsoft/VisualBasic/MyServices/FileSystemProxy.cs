namespace Microsoft.VisualBasic.MyServices
{
    using Microsoft.VisualBasic.FileIO;
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Security.Permissions;
    using System.Text;

    [EditorBrowsable(EditorBrowsableState.Never), HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.ExternalProcessMgmt)]
    public class FileSystemProxy
    {
        private SpecialDirectoriesProxy m_SpecialDirectoriesProxy = null;

        internal FileSystemProxy()
        {
        }

        public string CombinePath(string baseDirectory, string relativePath)
        {
            return FileSystem.CombinePath(baseDirectory, relativePath);
        }

        public void CopyDirectory(string sourceDirectoryName, string destinationDirectoryName)
        {
            FileSystem.CopyDirectory(sourceDirectoryName, destinationDirectoryName);
        }

        public void CopyDirectory(string sourceDirectoryName, string destinationDirectoryName, UIOption showUI)
        {
            FileSystem.CopyDirectory(sourceDirectoryName, destinationDirectoryName, showUI);
        }

        public void CopyDirectory(string sourceDirectoryName, string destinationDirectoryName, bool overwrite)
        {
            FileSystem.CopyDirectory(sourceDirectoryName, destinationDirectoryName, overwrite);
        }

        public void CopyDirectory(string sourceDirectoryName, string destinationDirectoryName, UIOption showUI, UICancelOption onUserCancel)
        {
            FileSystem.CopyDirectory(sourceDirectoryName, destinationDirectoryName, showUI, onUserCancel);
        }

        public void CopyFile(string sourceFileName, string destinationFileName)
        {
            FileSystem.CopyFile(sourceFileName, destinationFileName);
        }

        public void CopyFile(string sourceFileName, string destinationFileName, UIOption showUI)
        {
            FileSystem.CopyFile(sourceFileName, destinationFileName, showUI);
        }

        public void CopyFile(string sourceFileName, string destinationFileName, bool overwrite)
        {
            FileSystem.CopyFile(sourceFileName, destinationFileName, overwrite);
        }

        public void CopyFile(string sourceFileName, string destinationFileName, UIOption showUI, UICancelOption onUserCancel)
        {
            FileSystem.CopyFile(sourceFileName, destinationFileName, showUI, onUserCancel);
        }

        public void CreateDirectory(string directory)
        {
            FileSystem.CreateDirectory(directory);
        }

        public void DeleteDirectory(string directory, DeleteDirectoryOption onDirectoryNotEmpty)
        {
            FileSystem.DeleteDirectory(directory, onDirectoryNotEmpty);
        }

        public void DeleteDirectory(string directory, UIOption showUI, RecycleOption recycle)
        {
            FileSystem.DeleteDirectory(directory, showUI, recycle);
        }

        public void DeleteDirectory(string directory, UIOption showUI, RecycleOption recycle, UICancelOption onUserCancel)
        {
            FileSystem.DeleteDirectory(directory, showUI, recycle, onUserCancel);
        }

        public void DeleteFile(string file)
        {
            FileSystem.DeleteFile(file);
        }

        public void DeleteFile(string file, UIOption showUI, RecycleOption recycle)
        {
            FileSystem.DeleteFile(file, showUI, recycle);
        }

        public void DeleteFile(string file, UIOption showUI, RecycleOption recycle, UICancelOption onUserCancel)
        {
            FileSystem.DeleteFile(file, showUI, recycle, onUserCancel);
        }

        public bool DirectoryExists(string directory)
        {
            return FileSystem.DirectoryExists(directory);
        }

        public bool FileExists(string file)
        {
            return FileSystem.FileExists(file);
        }

        public ReadOnlyCollection<string> FindInFiles(string directory, string containsText, bool ignoreCase, Microsoft.VisualBasic.FileIO.SearchOption searchType)
        {
            return FileSystem.FindInFiles(directory, containsText, ignoreCase, searchType);
        }

        public ReadOnlyCollection<string> FindInFiles(string directory, string containsText, bool ignoreCase, Microsoft.VisualBasic.FileIO.SearchOption searchType, params string[] fileWildcards)
        {
            return FileSystem.FindInFiles(directory, containsText, ignoreCase, searchType, fileWildcards);
        }

        public ReadOnlyCollection<string> GetDirectories(string directory)
        {
            return FileSystem.GetDirectories(directory);
        }

        public ReadOnlyCollection<string> GetDirectories(string directory, Microsoft.VisualBasic.FileIO.SearchOption searchType, params string[] wildcards)
        {
            return FileSystem.GetDirectories(directory, searchType, wildcards);
        }

        public DirectoryInfo GetDirectoryInfo(string directory)
        {
            return FileSystem.GetDirectoryInfo(directory);
        }

        public DriveInfo GetDriveInfo(string drive)
        {
            return FileSystem.GetDriveInfo(drive);
        }

        public FileInfo GetFileInfo(string file)
        {
            return FileSystem.GetFileInfo(file);
        }

        public ReadOnlyCollection<string> GetFiles(string directory)
        {
            return FileSystem.GetFiles(directory);
        }

        public ReadOnlyCollection<string> GetFiles(string directory, Microsoft.VisualBasic.FileIO.SearchOption searchType, params string[] wildcards)
        {
            return FileSystem.GetFiles(directory, searchType, wildcards);
        }

        public string GetName(string path)
        {
            return FileSystem.GetName(path);
        }

        public string GetParentPath(string path)
        {
            return FileSystem.GetParentPath(path);
        }

        public string GetTempFileName()
        {
            return FileSystem.GetTempFileName();
        }

        public void MoveDirectory(string sourceDirectoryName, string destinationDirectoryName)
        {
            FileSystem.MoveDirectory(sourceDirectoryName, destinationDirectoryName);
        }

        public void MoveDirectory(string sourceDirectoryName, string destinationDirectoryName, UIOption showUI)
        {
            FileSystem.MoveDirectory(sourceDirectoryName, destinationDirectoryName, showUI);
        }

        public void MoveDirectory(string sourceDirectoryName, string destinationDirectoryName, bool overwrite)
        {
            FileSystem.MoveDirectory(sourceDirectoryName, destinationDirectoryName, overwrite);
        }

        public void MoveDirectory(string sourceDirectoryName, string destinationDirectoryName, UIOption showUI, UICancelOption onUserCancel)
        {
            FileSystem.MoveDirectory(sourceDirectoryName, destinationDirectoryName, showUI, onUserCancel);
        }

        public void MoveFile(string sourceFileName, string destinationFileName)
        {
            FileSystem.MoveFile(sourceFileName, destinationFileName);
        }

        public void MoveFile(string sourceFileName, string destinationFileName, UIOption showUI)
        {
            FileSystem.MoveFile(sourceFileName, destinationFileName, showUI);
        }

        public void MoveFile(string sourceFileName, string destinationFileName, bool overwrite)
        {
            FileSystem.MoveFile(sourceFileName, destinationFileName, overwrite);
        }

        public void MoveFile(string sourceFileName, string destinationFileName, UIOption showUI, UICancelOption onUserCancel)
        {
            FileSystem.MoveFile(sourceFileName, destinationFileName, showUI, onUserCancel);
        }

        public TextFieldParser OpenTextFieldParser(string file)
        {
            return FileSystem.OpenTextFieldParser(file);
        }

        public TextFieldParser OpenTextFieldParser(string file, params int[] fieldWidths)
        {
            return FileSystem.OpenTextFieldParser(file, fieldWidths);
        }

        public TextFieldParser OpenTextFieldParser(string file, params string[] delimiters)
        {
            return FileSystem.OpenTextFieldParser(file, delimiters);
        }

        public StreamReader OpenTextFileReader(string file)
        {
            return FileSystem.OpenTextFileReader(file);
        }

        public StreamReader OpenTextFileReader(string file, Encoding encoding)
        {
            return FileSystem.OpenTextFileReader(file, encoding);
        }

        public StreamWriter OpenTextFileWriter(string file, bool append)
        {
            return FileSystem.OpenTextFileWriter(file, append);
        }

        public StreamWriter OpenTextFileWriter(string file, bool append, Encoding encoding)
        {
            return FileSystem.OpenTextFileWriter(file, append, encoding);
        }

        public byte[] ReadAllBytes(string file)
        {
            return FileSystem.ReadAllBytes(file);
        }

        public string ReadAllText(string file)
        {
            return FileSystem.ReadAllText(file);
        }

        public string ReadAllText(string file, Encoding encoding)
        {
            return FileSystem.ReadAllText(file, encoding);
        }

        public void RenameDirectory(string directory, string newName)
        {
            FileSystem.RenameDirectory(directory, newName);
        }

        public void RenameFile(string file, string newName)
        {
            FileSystem.RenameFile(file, newName);
        }

        public void WriteAllBytes(string file, byte[] data, bool append)
        {
            FileSystem.WriteAllBytes(file, data, append);
        }

        public void WriteAllText(string file, string text, bool append)
        {
            FileSystem.WriteAllText(file, text, append);
        }

        public void WriteAllText(string file, string text, bool append, Encoding encoding)
        {
            FileSystem.WriteAllText(file, text, append, encoding);
        }

        public string CurrentDirectory
        {
            get
            {
                return FileSystem.CurrentDirectory;
            }
            set
            {
                FileSystem.CurrentDirectory = value;
            }
        }

        public ReadOnlyCollection<DriveInfo> Drives
        {
            get
            {
                return FileSystem.Drives;
            }
        }

        public SpecialDirectoriesProxy SpecialDirectories
        {
            get
            {
                if (this.m_SpecialDirectoriesProxy == null)
                {
                    this.m_SpecialDirectoriesProxy = new SpecialDirectoriesProxy();
                }
                return this.m_SpecialDirectoriesProxy;
            }
        }
    }
}

