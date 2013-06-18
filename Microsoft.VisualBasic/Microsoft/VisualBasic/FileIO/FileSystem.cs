namespace Microsoft.VisualBasic.FileIO
{
    using Microsoft.VisualBasic.CompilerServices;
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;

    [HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.ExternalProcessMgmt)]
    public class FileSystem
    {
        private const int m_MOVEFILEEX_FLAGS = 11;
        private static readonly char[] m_SeparatorChars = new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar, Path.VolumeSeparatorChar };
        private const Microsoft.VisualBasic.CompilerServices.NativeMethods.ShFileOperationFlags m_SHELL_OPERATION_FLAGS_BASE = (Microsoft.VisualBasic.CompilerServices.NativeMethods.ShFileOperationFlags.FOF_NO_CONNECTED_ELEMENTS | Microsoft.VisualBasic.CompilerServices.NativeMethods.ShFileOperationFlags.FOF_NOCONFIRMMKDIR);
        private const Microsoft.VisualBasic.CompilerServices.NativeMethods.ShFileOperationFlags m_SHELL_OPERATION_FLAGS_HIDE_UI = (Microsoft.VisualBasic.CompilerServices.NativeMethods.ShFileOperationFlags.FOF_NOCONFIRMATION | Microsoft.VisualBasic.CompilerServices.NativeMethods.ShFileOperationFlags.FOF_SILENT);

        private static void AddToStringCollection(Collection<string> StrCollection, string[] StrArray)
        {
            if (StrArray != null)
            {
                foreach (string str in StrArray)
                {
                    if (!StrCollection.Contains(str))
                    {
                        StrCollection.Add(str);
                    }
                }
            }
        }

        internal static void CheckFilePathTrailingSeparator(string path, string paramName)
        {
            if (path == "")
            {
                throw ExceptionUtils.GetArgumentNullException(paramName);
            }
            if (path.EndsWith(Conversions.ToString(Path.DirectorySeparatorChar), StringComparison.Ordinal) | path.EndsWith(Conversions.ToString(Path.AltDirectorySeparatorChar), StringComparison.Ordinal))
            {
                throw ExceptionUtils.GetArgumentExceptionWithArgName(paramName, "IO_FilePathException", new string[0]);
            }
        }

        public static string CombinePath(string baseDirectory, string relativePath)
        {
            if (baseDirectory == "")
            {
                throw ExceptionUtils.GetArgumentNullException("baseDirectory", "General_ArgumentEmptyOrNothing_Name", new string[] { "baseDirectory" });
            }
            if (relativePath == "")
            {
                return baseDirectory;
            }
            baseDirectory = Path.GetFullPath(baseDirectory);
            return NormalizePath(Path.Combine(baseDirectory, relativePath));
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static void CopyDirectory(string sourceDirectoryName, string destinationDirectoryName)
        {
            CopyOrMoveDirectory(CopyOrMove.Copy, sourceDirectoryName, destinationDirectoryName, false, UIOptionInternal.NoUI, UICancelOption.ThrowException);
        }

        public static void CopyDirectory(string sourceDirectoryName, string destinationDirectoryName, UIOption showUI)
        {
            CopyOrMoveDirectory(CopyOrMove.Copy, sourceDirectoryName, destinationDirectoryName, false, ToUIOptionInternal(showUI), UICancelOption.ThrowException);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static void CopyDirectory(string sourceDirectoryName, string destinationDirectoryName, bool overwrite)
        {
            CopyOrMoveDirectory(CopyOrMove.Copy, sourceDirectoryName, destinationDirectoryName, overwrite, UIOptionInternal.NoUI, UICancelOption.ThrowException);
        }

        public static void CopyDirectory(string sourceDirectoryName, string destinationDirectoryName, UIOption showUI, UICancelOption onUserCancel)
        {
            CopyOrMoveDirectory(CopyOrMove.Copy, sourceDirectoryName, destinationDirectoryName, false, ToUIOptionInternal(showUI), onUserCancel);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static void CopyFile(string sourceFileName, string destinationFileName)
        {
            CopyOrMoveFile(CopyOrMove.Copy, sourceFileName, destinationFileName, false, UIOptionInternal.NoUI, UICancelOption.ThrowException);
        }

        public static void CopyFile(string sourceFileName, string destinationFileName, UIOption showUI)
        {
            CopyOrMoveFile(CopyOrMove.Copy, sourceFileName, destinationFileName, false, ToUIOptionInternal(showUI), UICancelOption.ThrowException);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static void CopyFile(string sourceFileName, string destinationFileName, bool overwrite)
        {
            CopyOrMoveFile(CopyOrMove.Copy, sourceFileName, destinationFileName, overwrite, UIOptionInternal.NoUI, UICancelOption.ThrowException);
        }

        public static void CopyFile(string sourceFileName, string destinationFileName, UIOption showUI, UICancelOption onUserCancel)
        {
            CopyOrMoveFile(CopyOrMove.Copy, sourceFileName, destinationFileName, false, ToUIOptionInternal(showUI), onUserCancel);
        }

        [SecuritySafeCritical]
        private static void CopyOrMoveDirectory(CopyOrMove operation, string sourceDirectoryName, string destinationDirectoryName, bool overwrite, UIOptionInternal showUI, UICancelOption onUserCancel)
        {
            VerifyUICancelOption("onUserCancel", onUserCancel);
            string fullDirectoryPath = NormalizePath(sourceDirectoryName);
            string str2 = NormalizePath(destinationDirectoryName);
            FileIOPermissionAccess read = FileIOPermissionAccess.Read;
            if (operation == CopyOrMove.Move)
            {
                read |= FileIOPermissionAccess.Write;
            }
            DemandDirectoryPermission(fullDirectoryPath, read);
            DemandDirectoryPermission(str2, FileIOPermissionAccess.Write | FileIOPermissionAccess.Read);
            ThrowIfDevicePath(fullDirectoryPath);
            ThrowIfDevicePath(str2);
            if (!Directory.Exists(fullDirectoryPath))
            {
                throw ExceptionUtils.GetDirectoryNotFoundException("IO_DirectoryNotFound_Path", new string[] { sourceDirectoryName });
            }
            if (IsRoot(fullDirectoryPath))
            {
                throw ExceptionUtils.GetIOException("IO_DirectoryIsRoot_Path", new string[] { sourceDirectoryName });
            }
            if (File.Exists(str2))
            {
                throw ExceptionUtils.GetIOException("IO_FileExists_Path", new string[] { destinationDirectoryName });
            }
            if (str2.Equals(fullDirectoryPath, StringComparison.OrdinalIgnoreCase))
            {
                throw ExceptionUtils.GetIOException("IO_SourceEqualsTargetDirectory", new string[0]);
            }
            if (((str2.Length > fullDirectoryPath.Length) && str2.Substring(0, fullDirectoryPath.Length).Equals(fullDirectoryPath, StringComparison.OrdinalIgnoreCase)) && (str2[fullDirectoryPath.Length] == Path.DirectorySeparatorChar))
            {
                throw ExceptionUtils.GetInvalidOperationException("IO_CyclicOperation", new string[0]);
            }
            if ((showUI != UIOptionInternal.NoUI) && Environment.UserInteractive)
            {
                ShellCopyOrMove(operation, FileOrDirectory.Directory, fullDirectoryPath, str2, showUI, onUserCancel);
            }
            else
            {
                FxCopyOrMoveDirectory(operation, fullDirectoryPath, str2, overwrite);
            }
        }

        private static void CopyOrMoveDirectoryNode(CopyOrMove Operation, DirectoryNode SourceDirectoryNode, bool Overwrite, ListDictionary Exceptions)
        {
            try
            {
                if (!Directory.Exists(SourceDirectoryNode.TargetPath))
                {
                    Directory.CreateDirectory(SourceDirectoryNode.TargetPath);
                }
            }
            catch (Exception exception)
            {
                if ((!(exception is IOException) && !(exception is UnauthorizedAccessException)) && ((!(exception is DirectoryNotFoundException) && !(exception is NotSupportedException)) && !(exception is SecurityException)))
                {
                    throw;
                }
                Exceptions.Add(SourceDirectoryNode.Path, exception.Message);
                return;
            }
            if (!Directory.Exists(SourceDirectoryNode.TargetPath))
            {
                Exceptions.Add(SourceDirectoryNode.TargetPath, ExceptionUtils.GetDirectoryNotFoundException("IO_DirectoryNotFound_Path", new string[] { SourceDirectoryNode.TargetPath }));
            }
            else
            {
                foreach (string str in Directory.GetFiles(SourceDirectoryNode.Path))
                {
                    try
                    {
                        CopyOrMoveFile(Operation, str, Path.Combine(SourceDirectoryNode.TargetPath, Path.GetFileName(str)), Overwrite, UIOptionInternal.NoUI, UICancelOption.ThrowException);
                    }
                    catch (Exception exception2)
                    {
                        if ((!(exception2 is IOException) && !(exception2 is UnauthorizedAccessException)) && (!(exception2 is SecurityException) && !(exception2 is NotSupportedException)))
                        {
                            throw;
                        }
                        Exceptions.Add(str, exception2.Message);
                    }
                }
                foreach (DirectoryNode node in SourceDirectoryNode.SubDirs)
                {
                    CopyOrMoveDirectoryNode(Operation, node, Overwrite, Exceptions);
                }
                if (Operation == CopyOrMove.Move)
                {
                    try
                    {
                        Directory.Delete(SourceDirectoryNode.Path, false);
                    }
                    catch (Exception exception3)
                    {
                        if ((!(exception3 is IOException) && !(exception3 is UnauthorizedAccessException)) && (!(exception3 is SecurityException) && !(exception3 is DirectoryNotFoundException)))
                        {
                            throw;
                        }
                        Exceptions.Add(SourceDirectoryNode.Path, exception3.Message);
                    }
                }
            }
        }

        [SecuritySafeCritical]
        private static void CopyOrMoveFile(CopyOrMove operation, string sourceFileName, string destinationFileName, bool overwrite, UIOptionInternal showUI, UICancelOption onUserCancel)
        {
            VerifyUICancelOption("onUserCancel", onUserCancel);
            string path = NormalizeFilePath(sourceFileName, "sourceFileName");
            string str = NormalizeFilePath(destinationFileName, "destinationFileName");
            FileIOPermissionAccess read = FileIOPermissionAccess.Read;
            if (operation == CopyOrMove.Move)
            {
                read |= FileIOPermissionAccess.Write;
            }
            new FileIOPermission(read, path).Demand();
            new FileIOPermission(FileIOPermissionAccess.Write, str).Demand();
            ThrowIfDevicePath(path);
            ThrowIfDevicePath(str);
            if (!File.Exists(path))
            {
                throw ExceptionUtils.GetFileNotFoundException(sourceFileName, "IO_FileNotFound_Path", new string[] { sourceFileName });
            }
            if (Directory.Exists(str))
            {
                throw ExceptionUtils.GetIOException("IO_DirectoryExists_Path", new string[] { destinationFileName });
            }
            Directory.CreateDirectory(GetParentPath(str));
            if ((showUI != UIOptionInternal.NoUI) && Environment.UserInteractive)
            {
                ShellCopyOrMove(operation, FileOrDirectory.File, path, str, showUI, onUserCancel);
            }
            else if ((operation == CopyOrMove.Copy) || path.Equals(str, StringComparison.OrdinalIgnoreCase))
            {
                File.Copy(path, str, overwrite);
            }
            else if (overwrite)
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();
                    try
                    {
                        if (!Microsoft.VisualBasic.CompilerServices.NativeMethods.MoveFileEx(path, str, 11))
                        {
                            ThrowWinIOError(Marshal.GetLastWin32Error());
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                }
                else
                {
                    File.Delete(str);
                    File.Move(path, str);
                }
            }
            else
            {
                File.Move(path, str);
            }
        }

        public static void CreateDirectory(string directory)
        {
            directory = Path.GetFullPath(directory);
            if (File.Exists(directory))
            {
                throw ExceptionUtils.GetIOException("IO_FileExists_Path", new string[] { directory });
            }
            Directory.CreateDirectory(directory);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static void DeleteDirectory(string directory, DeleteDirectoryOption onDirectoryNotEmpty)
        {
            DeleteDirectoryInternal(directory, onDirectoryNotEmpty, UIOptionInternal.NoUI, RecycleOption.DeletePermanently, UICancelOption.ThrowException);
        }

        public static void DeleteDirectory(string directory, UIOption showUI, RecycleOption recycle)
        {
            DeleteDirectoryInternal(directory, DeleteDirectoryOption.DeleteAllContents, ToUIOptionInternal(showUI), recycle, UICancelOption.ThrowException);
        }

        public static void DeleteDirectory(string directory, UIOption showUI, RecycleOption recycle, UICancelOption onUserCancel)
        {
            DeleteDirectoryInternal(directory, DeleteDirectoryOption.DeleteAllContents, ToUIOptionInternal(showUI), recycle, onUserCancel);
        }

        [SecuritySafeCritical]
        private static void DeleteDirectoryInternal(string directory, DeleteDirectoryOption onDirectoryNotEmpty, UIOptionInternal showUI, RecycleOption recycle, UICancelOption onUserCancel)
        {
            VerifyDeleteDirectoryOption("onDirectoryNotEmpty", onDirectoryNotEmpty);
            VerifyRecycleOption("recycle", recycle);
            VerifyUICancelOption("onUserCancel", onUserCancel);
            string fullPath = Path.GetFullPath(directory);
            DemandDirectoryPermission(fullPath, FileIOPermissionAccess.Write);
            ThrowIfDevicePath(fullPath);
            if (!Directory.Exists(fullPath))
            {
                throw ExceptionUtils.GetDirectoryNotFoundException("IO_DirectoryNotFound_Path", new string[] { directory });
            }
            if (IsRoot(fullPath))
            {
                throw ExceptionUtils.GetIOException("IO_DirectoryIsRoot_Path", new string[] { directory });
            }
            if ((showUI != UIOptionInternal.NoUI) && Environment.UserInteractive)
            {
                ShellDelete(fullPath, showUI, recycle, onUserCancel, FileOrDirectory.Directory);
            }
            else
            {
                Directory.Delete(fullPath, onDirectoryNotEmpty == DeleteDirectoryOption.DeleteAllContents);
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static void DeleteFile(string file)
        {
            DeleteFileInternal(file, UIOptionInternal.NoUI, RecycleOption.DeletePermanently, UICancelOption.ThrowException);
        }

        public static void DeleteFile(string file, UIOption showUI, RecycleOption recycle)
        {
            DeleteFileInternal(file, ToUIOptionInternal(showUI), recycle, UICancelOption.ThrowException);
        }

        public static void DeleteFile(string file, UIOption showUI, RecycleOption recycle, UICancelOption onUserCancel)
        {
            DeleteFileInternal(file, ToUIOptionInternal(showUI), recycle, onUserCancel);
        }

        [SecuritySafeCritical]
        private static void DeleteFileInternal(string file, UIOptionInternal showUI, RecycleOption recycle, UICancelOption onUserCancel)
        {
            VerifyRecycleOption("recycle", recycle);
            VerifyUICancelOption("onUserCancel", onUserCancel);
            string path = NormalizeFilePath(file, "file");
            new FileIOPermission(FileIOPermissionAccess.Write, path).Demand();
            ThrowIfDevicePath(path);
            if (!File.Exists(path))
            {
                throw ExceptionUtils.GetFileNotFoundException(file, "IO_FileNotFound_Path", new string[] { file });
            }
            if ((showUI != UIOptionInternal.NoUI) && Environment.UserInteractive)
            {
                ShellDelete(path, showUI, recycle, onUserCancel, FileOrDirectory.File);
            }
            else
            {
                File.Delete(path);
            }
        }

        [SecuritySafeCritical]
        private static void DemandDirectoryPermission(string fullDirectoryPath, FileIOPermissionAccess access)
        {
            if (!(fullDirectoryPath.EndsWith(Conversions.ToString(Path.DirectorySeparatorChar), StringComparison.Ordinal) | fullDirectoryPath.EndsWith(Conversions.ToString(Path.AltDirectorySeparatorChar), StringComparison.Ordinal)))
            {
                fullDirectoryPath = fullDirectoryPath + Conversions.ToString(Path.DirectorySeparatorChar);
            }
            new FileIOPermission(access, fullDirectoryPath).Demand();
        }

        public static bool DirectoryExists(string directory)
        {
            return Directory.Exists(directory);
        }

        private static void EnsurePathNotExist(string Path)
        {
            if (File.Exists(Path))
            {
                throw ExceptionUtils.GetIOException("IO_FileExists_Path", new string[] { Path });
            }
            if (Directory.Exists(Path))
            {
                throw ExceptionUtils.GetIOException("IO_DirectoryExists_Path", new string[] { Path });
            }
        }

        private static bool FileContainsText(string FilePath, string Text, bool IgnoreCase)
        {
            bool flag;
            int num = 0x400;
            FileStream stream = null;
            try
            {
                stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                Encoding currentEncoding = Encoding.Default;
                byte[] array = new byte[(num - 1) + 1];
                int count = 0;
                count = stream.Read(array, 0, array.Length);
                if (count > 0)
                {
                    MemoryStream stream2 = new MemoryStream(array, 0, count);
                    StreamReader reader = new StreamReader(stream2, currentEncoding, true);
                    reader.ReadLine();
                    currentEncoding = reader.CurrentEncoding;
                }
                int num2 = Math.Max(currentEncoding.GetMaxByteCount(Text.Length), num);
                TextSearchHelper helper = new TextSearchHelper(currentEncoding, Text, IgnoreCase);
                if (num2 > num)
                {
                    array = (byte[]) Utils.CopyArray((Array) array, new byte[(num2 - 1) + 1]);
                    int num5 = stream.Read(array, count, array.Length - count);
                    count += num5;
                }
                do
                {
                    if ((count > 0) && helper.IsTextFound(array, count))
                    {
                        return true;
                    }
                    count = stream.Read(array, 0, array.Length);
                }
                while (count > 0);
                flag = false;
            }
            catch (Exception exception)
            {
                if (!((((exception is IOException) | (exception is NotSupportedException)) | (exception is SecurityException)) | (exception is UnauthorizedAccessException)))
                {
                    throw;
                }
                flag = false;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
            return flag;
        }

        public static bool FileExists(string file)
        {
            if (!string.IsNullOrEmpty(file) && (file.EndsWith(Conversions.ToString(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase) | file.EndsWith(Conversions.ToString(Path.AltDirectorySeparatorChar), StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
            return File.Exists(file);
        }

        private static ReadOnlyCollection<string> FindFilesOrDirectories(FileOrDirectory FileOrDirectory, string directory, Microsoft.VisualBasic.FileIO.SearchOption searchType, string[] wildcards)
        {
            Collection<string> results = new Collection<string>();
            FindFilesOrDirectories(FileOrDirectory, directory, searchType, wildcards, results);
            return new ReadOnlyCollection<string>(results);
        }

        private static void FindFilesOrDirectories(FileOrDirectory FileOrDirectory, string directory, Microsoft.VisualBasic.FileIO.SearchOption searchType, string[] wildcards, Collection<string> Results)
        {
            VerifySearchOption("searchType", searchType);
            directory = NormalizePath(directory);
            if (wildcards != null)
            {
                foreach (string str in wildcards)
                {
                    if (str.TrimEnd(new char[0]) == "")
                    {
                        throw ExceptionUtils.GetArgumentNullException("wildcards", "IO_GetFiles_NullPattern", new string[0]);
                    }
                }
            }
            if ((wildcards == null) || (wildcards.Length == 0))
            {
                AddToStringCollection(Results, FindPaths(FileOrDirectory, directory, null));
            }
            else
            {
                foreach (string str2 in wildcards)
                {
                    AddToStringCollection(Results, FindPaths(FileOrDirectory, directory, str2));
                }
            }
            if (searchType == Microsoft.VisualBasic.FileIO.SearchOption.SearchAllSubDirectories)
            {
                foreach (string str3 in Directory.GetDirectories(directory))
                {
                    FindFilesOrDirectories(FileOrDirectory, str3, searchType, wildcards, Results);
                }
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static ReadOnlyCollection<string> FindInFiles(string directory, string containsText, bool ignoreCase, Microsoft.VisualBasic.FileIO.SearchOption searchType)
        {
            return FindInFiles(directory, containsText, ignoreCase, searchType, null);
        }

        public static ReadOnlyCollection<string> FindInFiles(string directory, string containsText, bool ignoreCase, Microsoft.VisualBasic.FileIO.SearchOption searchType, params string[] fileWildcards)
        {
            ReadOnlyCollection<string> onlys2 = FindFilesOrDirectories(FileOrDirectory.File, directory, searchType, fileWildcards);
            if (containsText == "")
            {
                return onlys2;
            }
            Collection<string> list = new Collection<string>();
            foreach (string str in onlys2)
            {
                if (FileContainsText(str, containsText, ignoreCase))
                {
                    list.Add(str);
                }
            }
            return new ReadOnlyCollection<string>(list);
        }

        private static string[] FindPaths(FileOrDirectory FileOrDirectory, string directory, string wildCard)
        {
            if (FileOrDirectory == FileSystem.FileOrDirectory.Directory)
            {
                if (wildCard == "")
                {
                    return Directory.GetDirectories(directory);
                }
                return Directory.GetDirectories(directory, wildCard);
            }
            if (wildCard == "")
            {
                return Directory.GetFiles(directory);
            }
            return Directory.GetFiles(directory, wildCard);
        }

        private static void FxCopyOrMoveDirectory(CopyOrMove operation, string sourceDirectoryPath, string targetDirectoryPath, bool overwrite)
        {
            if (((operation == CopyOrMove.Move) & !Directory.Exists(targetDirectoryPath)) & IsOnSameDrive(sourceDirectoryPath, targetDirectoryPath))
            {
                Directory.CreateDirectory(GetParentPath(targetDirectoryPath));
                try
                {
                    Directory.Move(sourceDirectoryPath, targetDirectoryPath);
                    return;
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
            }
            Directory.CreateDirectory(targetDirectoryPath);
            DirectoryNode sourceDirectoryNode = new DirectoryNode(sourceDirectoryPath, targetDirectoryPath);
            ListDictionary exceptions = new ListDictionary();
            CopyOrMoveDirectoryNode(operation, sourceDirectoryNode, overwrite, exceptions);
            if (exceptions.Count > 0)
            {
                IOException exception3 = new IOException(Utils.GetResourceString("IO_CopyMoveRecursive"));
                IDictionaryEnumerator enumerator = exceptions.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    DictionaryEntry current = (DictionaryEntry) enumerator.Current;
                    exception3.Data.Add(current.Key, current.Value);
                }
                throw exception3;
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static ReadOnlyCollection<string> GetDirectories(string directory)
        {
            return FindFilesOrDirectories(FileOrDirectory.Directory, directory, Microsoft.VisualBasic.FileIO.SearchOption.SearchTopLevelOnly, null);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static ReadOnlyCollection<string> GetDirectories(string directory, Microsoft.VisualBasic.FileIO.SearchOption searchType, params string[] wildcards)
        {
            return FindFilesOrDirectories(FileOrDirectory.Directory, directory, searchType, wildcards);
        }

        public static DirectoryInfo GetDirectoryInfo(string directory)
        {
            return new DirectoryInfo(directory);
        }

        public static DriveInfo GetDriveInfo(string drive)
        {
            return new DriveInfo(drive);
        }

        public static FileInfo GetFileInfo(string file)
        {
            file = NormalizeFilePath(file, "file");
            return new FileInfo(file);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static ReadOnlyCollection<string> GetFiles(string directory)
        {
            return FindFilesOrDirectories(FileOrDirectory.File, directory, Microsoft.VisualBasic.FileIO.SearchOption.SearchTopLevelOnly, null);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static ReadOnlyCollection<string> GetFiles(string directory, Microsoft.VisualBasic.FileIO.SearchOption searchType, params string[] wildcards)
        {
            return FindFilesOrDirectories(FileOrDirectory.File, directory, searchType, wildcards);
        }

        private static string GetFullPathFromNewName(string Path, string NewName, string ArgumentName)
        {
            if (NewName.IndexOfAny(m_SeparatorChars) >= 0)
            {
                throw ExceptionUtils.GetArgumentExceptionWithArgName(ArgumentName, "IO_ArgumentIsPath_Name_Path", new string[] { ArgumentName, NewName });
            }
            string path = RemoveEndingSeparator(Path.GetFullPath(Path.Combine(Path, NewName)));
            if (!GetParentPath(path).Equals(Path, StringComparison.OrdinalIgnoreCase))
            {
                throw ExceptionUtils.GetArgumentExceptionWithArgName(ArgumentName, "IO_ArgumentIsPath_Name_Path", new string[] { ArgumentName, NewName });
            }
            return path;
        }

        private static string GetLongPath(string FullPath)
        {
            try
            {
                if (!IsRoot(FullPath))
                {
                    DirectoryInfo info = new DirectoryInfo(GetParentPath(FullPath));
                    if (File.Exists(FullPath))
                    {
                        return info.GetFiles(Path.GetFileName(FullPath))[0].FullName;
                    }
                    if (Directory.Exists(FullPath))
                    {
                        return info.GetDirectories(Path.GetFileName(FullPath))[0].FullName;
                    }
                }
                return FullPath;
            }
            catch (Exception exception)
            {
                if (((!(exception is ArgumentException) && !(exception is ArgumentNullException)) && (!(exception is PathTooLongException) && !(exception is NotSupportedException))) && ((!(exception is DirectoryNotFoundException) && !(exception is SecurityException)) && !(exception is UnauthorizedAccessException)))
                {
                    throw;
                }
                return FullPath;
            }
        }

        public static string GetName(string path)
        {
            return Path.GetFileName(path);
        }

        private static Microsoft.VisualBasic.CompilerServices.NativeMethods.ShFileOperationFlags GetOperationFlags(UIOptionInternal ShowUI)
        {
            Microsoft.VisualBasic.CompilerServices.NativeMethods.ShFileOperationFlags flags2 = Microsoft.VisualBasic.CompilerServices.NativeMethods.ShFileOperationFlags.FOF_NO_CONNECTED_ELEMENTS | Microsoft.VisualBasic.CompilerServices.NativeMethods.ShFileOperationFlags.FOF_NOCONFIRMMKDIR;
            if (ShowUI == UIOptionInternal.OnlyErrorDialogs)
            {
                flags2 |= Microsoft.VisualBasic.CompilerServices.NativeMethods.ShFileOperationFlags.FOF_NOCONFIRMATION | Microsoft.VisualBasic.CompilerServices.NativeMethods.ShFileOperationFlags.FOF_SILENT;
            }
            return flags2;
        }

        public static string GetParentPath(string path)
        {
            Path.GetFullPath(path);
            if (IsRoot(path))
            {
                throw ExceptionUtils.GetArgumentExceptionWithArgName("path", "IO_GetParentPathIsRoot_Path", new string[] { path });
            }
            return Path.GetDirectoryName(path.TrimEnd(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }));
        }

        [SecurityCritical]
        private static Microsoft.VisualBasic.CompilerServices.NativeMethods.SHFILEOPSTRUCT GetShellOperationInfo(Microsoft.VisualBasic.CompilerServices.NativeMethods.SHFileOperationType OperationType, Microsoft.VisualBasic.CompilerServices.NativeMethods.ShFileOperationFlags OperationFlags, string SourcePath, string TargetPath = null)
        {
            return GetShellOperationInfo(OperationType, OperationFlags, new string[] { SourcePath }, TargetPath);
        }

        [SecurityCritical]
        private static Microsoft.VisualBasic.CompilerServices.NativeMethods.SHFILEOPSTRUCT GetShellOperationInfo(Microsoft.VisualBasic.CompilerServices.NativeMethods.SHFileOperationType OperationType, Microsoft.VisualBasic.CompilerServices.NativeMethods.ShFileOperationFlags OperationFlags, string[] SourcePaths, string TargetPath = null)
        {
            Microsoft.VisualBasic.CompilerServices.NativeMethods.SHFILEOPSTRUCT shfileopstruct2;
            shfileopstruct2.wFunc = (uint) OperationType;
            shfileopstruct2.fFlags = (ushort) OperationFlags;
            shfileopstruct2.pFrom = GetShellPath(SourcePaths);
            if (TargetPath == null)
            {
                shfileopstruct2.pTo = null;
            }
            else
            {
                shfileopstruct2.pTo = GetShellPath(TargetPath);
            }
            shfileopstruct2.hNameMappings = IntPtr.Zero;
            try
            {
                shfileopstruct2.hwnd = Process.GetCurrentProcess().MainWindowHandle;
            }
            catch (Exception exception)
            {
                if ((!(exception is SecurityException) && !(exception is InvalidOperationException)) && !(exception is NotSupportedException))
                {
                    throw;
                }
                shfileopstruct2.hwnd = IntPtr.Zero;
            }
            shfileopstruct2.lpszProgressTitle = string.Empty;
            return shfileopstruct2;
        }

        private static string GetShellPath(string FullPath)
        {
            return GetShellPath(new string[] { FullPath });
        }

        private static string GetShellPath(string[] FullPaths)
        {
            StringBuilder builder = new StringBuilder();
            foreach (string str2 in FullPaths)
            {
                builder.Append(str2 + "\0");
            }
            return builder.ToString();
        }

        public static string GetTempFileName()
        {
            return Path.GetTempFileName();
        }

        private static bool IsOnSameDrive(string Path1, string Path2)
        {
            Path1 = Path1.TrimEnd(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
            Path2 = Path2.TrimEnd(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
            return (string.Compare(Path.GetPathRoot(Path1), Path.GetPathRoot(Path2), StringComparison.OrdinalIgnoreCase) == 0);
        }

        private static bool IsRoot(string Path)
        {
            if (!Path.IsPathRooted(Path))
            {
                return false;
            }
            Path = Path.TrimEnd(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
            return (string.Compare(Path, Path.GetPathRoot(Path), StringComparison.OrdinalIgnoreCase) == 0);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static void MoveDirectory(string sourceDirectoryName, string destinationDirectoryName)
        {
            CopyOrMoveDirectory(CopyOrMove.Move, sourceDirectoryName, destinationDirectoryName, false, UIOptionInternal.NoUI, UICancelOption.ThrowException);
        }

        public static void MoveDirectory(string sourceDirectoryName, string destinationDirectoryName, UIOption showUI)
        {
            CopyOrMoveDirectory(CopyOrMove.Move, sourceDirectoryName, destinationDirectoryName, false, ToUIOptionInternal(showUI), UICancelOption.ThrowException);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static void MoveDirectory(string sourceDirectoryName, string destinationDirectoryName, bool overwrite)
        {
            CopyOrMoveDirectory(CopyOrMove.Move, sourceDirectoryName, destinationDirectoryName, overwrite, UIOptionInternal.NoUI, UICancelOption.ThrowException);
        }

        public static void MoveDirectory(string sourceDirectoryName, string destinationDirectoryName, UIOption showUI, UICancelOption onUserCancel)
        {
            CopyOrMoveDirectory(CopyOrMove.Move, sourceDirectoryName, destinationDirectoryName, false, ToUIOptionInternal(showUI), onUserCancel);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static void MoveFile(string sourceFileName, string destinationFileName)
        {
            CopyOrMoveFile(CopyOrMove.Move, sourceFileName, destinationFileName, false, UIOptionInternal.NoUI, UICancelOption.ThrowException);
        }

        public static void MoveFile(string sourceFileName, string destinationFileName, UIOption showUI)
        {
            CopyOrMoveFile(CopyOrMove.Move, sourceFileName, destinationFileName, false, ToUIOptionInternal(showUI), UICancelOption.ThrowException);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static void MoveFile(string sourceFileName, string destinationFileName, bool overwrite)
        {
            CopyOrMoveFile(CopyOrMove.Move, sourceFileName, destinationFileName, overwrite, UIOptionInternal.NoUI, UICancelOption.ThrowException);
        }

        public static void MoveFile(string sourceFileName, string destinationFileName, UIOption showUI, UICancelOption onUserCancel)
        {
            CopyOrMoveFile(CopyOrMove.Move, sourceFileName, destinationFileName, false, ToUIOptionInternal(showUI), onUserCancel);
        }

        internal static string NormalizeFilePath(string Path, string ParamName)
        {
            CheckFilePathTrailingSeparator(Path, ParamName);
            return NormalizePath(Path);
        }

        internal static string NormalizePath(string Path)
        {
            return GetLongPath(RemoveEndingSeparator(Path.GetFullPath(Path)));
        }

        public static TextFieldParser OpenTextFieldParser(string file)
        {
            return new TextFieldParser(file);
        }

        public static TextFieldParser OpenTextFieldParser(string file, params int[] fieldWidths)
        {
            TextFieldParser parser2 = new TextFieldParser(file);
            parser2.SetFieldWidths(fieldWidths);
            parser2.TextFieldType = FieldType.FixedWidth;
            return parser2;
        }

        public static TextFieldParser OpenTextFieldParser(string file, params string[] delimiters)
        {
            TextFieldParser parser2 = new TextFieldParser(file);
            parser2.SetDelimiters(delimiters);
            parser2.TextFieldType = FieldType.Delimited;
            return parser2;
        }

        public static StreamReader OpenTextFileReader(string file)
        {
            return OpenTextFileReader(file, Encoding.UTF8);
        }

        public static StreamReader OpenTextFileReader(string file, Encoding encoding)
        {
            file = NormalizeFilePath(file, "file");
            return new StreamReader(file, encoding, true);
        }

        public static StreamWriter OpenTextFileWriter(string file, bool append)
        {
            return OpenTextFileWriter(file, append, Encoding.UTF8);
        }

        public static StreamWriter OpenTextFileWriter(string file, bool append, Encoding encoding)
        {
            file = NormalizeFilePath(file, "file");
            return new StreamWriter(file, append, encoding);
        }

        public static byte[] ReadAllBytes(string file)
        {
            return File.ReadAllBytes(file);
        }

        public static string ReadAllText(string file)
        {
            return File.ReadAllText(file);
        }

        public static string ReadAllText(string file, Encoding encoding)
        {
            return File.ReadAllText(file, encoding);
        }

        private static string RemoveEndingSeparator(string Path)
        {
            if (Path.IsPathRooted(Path) && Path.Equals(Path.GetPathRoot(Path), StringComparison.OrdinalIgnoreCase))
            {
                return Path;
            }
            return Path.TrimEnd(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
        }

        public static void RenameDirectory(string directory, string newName)
        {
            directory = Path.GetFullPath(directory);
            ThrowIfDevicePath(directory);
            if (IsRoot(directory))
            {
                throw ExceptionUtils.GetIOException("IO_DirectoryIsRoot_Path", new string[] { directory });
            }
            if (!Directory.Exists(directory))
            {
                throw ExceptionUtils.GetDirectoryNotFoundException("IO_DirectoryNotFound_Path", new string[] { directory });
            }
            if (newName == "")
            {
                throw ExceptionUtils.GetArgumentNullException("newName", "General_ArgumentEmptyOrNothing_Name", new string[] { "newName" });
            }
            string path = GetFullPathFromNewName(GetParentPath(directory), newName, "newName");
            EnsurePathNotExist(path);
            Directory.Move(directory, path);
        }

        public static void RenameFile(string file, string newName)
        {
            file = NormalizeFilePath(file, "file");
            ThrowIfDevicePath(file);
            if (!File.Exists(file))
            {
                throw ExceptionUtils.GetFileNotFoundException(file, "IO_FileNotFound_Path", new string[] { file });
            }
            if (newName == "")
            {
                throw ExceptionUtils.GetArgumentNullException("newName", "General_ArgumentEmptyOrNothing_Name", new string[] { "newName" });
            }
            string path = GetFullPathFromNewName(GetParentPath(file), newName, "newName");
            EnsurePathNotExist(path);
            File.Move(file, path);
        }

        [SecurityCritical]
        private static void ShellCopyOrMove(CopyOrMove Operation, FileOrDirectory TargetType, string FullSourcePath, string FullTargetPath, UIOptionInternal ShowUI, UICancelOption OnUserCancel)
        {
            Microsoft.VisualBasic.CompilerServices.NativeMethods.SHFileOperationType type;
            if (Operation == CopyOrMove.Copy)
            {
                type = Microsoft.VisualBasic.CompilerServices.NativeMethods.SHFileOperationType.FO_COPY;
            }
            else
            {
                type = Microsoft.VisualBasic.CompilerServices.NativeMethods.SHFileOperationType.FO_MOVE;
            }
            Microsoft.VisualBasic.CompilerServices.NativeMethods.ShFileOperationFlags operationFlags = GetOperationFlags(ShowUI);
            string fullSource = FullSourcePath;
            if (TargetType == FileOrDirectory.Directory)
            {
                if (Directory.Exists(FullTargetPath))
                {
                    fullSource = Path.Combine(FullSourcePath, "*");
                }
                else
                {
                    Directory.CreateDirectory(GetParentPath(FullTargetPath));
                }
            }
            ShellFileOperation(type, operationFlags, fullSource, FullTargetPath, OnUserCancel, TargetType);
            if ((((Operation == CopyOrMove.Move) & (TargetType == FileOrDirectory.Directory)) && Directory.Exists(FullSourcePath)) && ((Directory.GetDirectories(FullSourcePath).Length == 0) && (Directory.GetFiles(FullSourcePath).Length == 0)))
            {
                Directory.Delete(FullSourcePath, false);
            }
        }

        [SecurityCritical]
        private static void ShellDelete(string FullPath, UIOptionInternal ShowUI, RecycleOption recycle, UICancelOption OnUserCancel, FileOrDirectory FileOrDirectory)
        {
            Microsoft.VisualBasic.CompilerServices.NativeMethods.ShFileOperationFlags operationFlags = GetOperationFlags(ShowUI);
            if (recycle == RecycleOption.SendToRecycleBin)
            {
                operationFlags |= Microsoft.VisualBasic.CompilerServices.NativeMethods.ShFileOperationFlags.FOF_ALLOWUNDO;
            }
            ShellFileOperation(Microsoft.VisualBasic.CompilerServices.NativeMethods.SHFileOperationType.FO_DELETE, operationFlags, FullPath, null, OnUserCancel, FileOrDirectory);
        }

        [SecurityCritical, HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.ExternalProcessMgmt, UI=true)]
        private static void ShellFileOperation(Microsoft.VisualBasic.CompilerServices.NativeMethods.SHFileOperationType OperationType, Microsoft.VisualBasic.CompilerServices.NativeMethods.ShFileOperationFlags OperationFlags, string FullSource, string FullTarget, UICancelOption OnUserCancel, FileOrDirectory FileOrDirectory)
        {
            int num;
            new UIPermission(UIPermissionWindow.SafeSubWindows).Demand();
            FileIOPermissionAccess noAccess = FileIOPermissionAccess.NoAccess;
            if (OperationType == Microsoft.VisualBasic.CompilerServices.NativeMethods.SHFileOperationType.FO_COPY)
            {
                noAccess = FileIOPermissionAccess.Read;
            }
            else if (OperationType == Microsoft.VisualBasic.CompilerServices.NativeMethods.SHFileOperationType.FO_MOVE)
            {
                noAccess = FileIOPermissionAccess.Write | FileIOPermissionAccess.Read;
            }
            else if (OperationType == Microsoft.VisualBasic.CompilerServices.NativeMethods.SHFileOperationType.FO_DELETE)
            {
                noAccess = FileIOPermissionAccess.Write;
            }
            string fullDirectoryPath = FullSource;
            if (((OperationType == Microsoft.VisualBasic.CompilerServices.NativeMethods.SHFileOperationType.FO_COPY) || (OperationType == Microsoft.VisualBasic.CompilerServices.NativeMethods.SHFileOperationType.FO_MOVE)) && fullDirectoryPath.EndsWith("*", StringComparison.Ordinal))
            {
                fullDirectoryPath = RemoveEndingSeparator(FullSource.TrimEnd(new char[] { '*' }));
            }
            if (FileOrDirectory == FileSystem.FileOrDirectory.Directory)
            {
                DemandDirectoryPermission(fullDirectoryPath, noAccess);
            }
            else
            {
                new FileIOPermission(noAccess, fullDirectoryPath).Demand();
            }
            if (OperationType != Microsoft.VisualBasic.CompilerServices.NativeMethods.SHFileOperationType.FO_DELETE)
            {
                if (FileOrDirectory == FileSystem.FileOrDirectory.Directory)
                {
                    DemandDirectoryPermission(FullTarget, FileIOPermissionAccess.Write);
                }
                else
                {
                    new FileIOPermission(FileIOPermissionAccess.Write, FullTarget).Demand();
                }
            }
            Microsoft.VisualBasic.CompilerServices.NativeMethods.SHFILEOPSTRUCT lpFileOp = GetShellOperationInfo(OperationType, OperationFlags, FullSource, FullTarget);
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();
            try
            {
                num = Microsoft.VisualBasic.CompilerServices.NativeMethods.SHFileOperation(ref lpFileOp);
                Microsoft.VisualBasic.CompilerServices.NativeMethods.SHChangeNotify(0x2381f, 3, IntPtr.Zero, IntPtr.Zero);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            if (lpFileOp.fAnyOperationsAborted)
            {
                if (OnUserCancel == UICancelOption.ThrowException)
                {
                    throw new OperationCanceledException();
                }
            }
            else if (num != 0)
            {
                ThrowWinIOError(num);
            }
        }

        private static void ThrowIfDevicePath(string path)
        {
            if (path.StartsWith(@"\\.\", StringComparison.Ordinal))
            {
                throw ExceptionUtils.GetArgumentExceptionWithArgName("path", "IO_DevicePath", new string[0]);
            }
        }

        [SecurityCritical]
        private static void ThrowWinIOError(int errorCode)
        {
            int num = errorCode;
            switch (num)
            {
                case 2:
                    throw new FileNotFoundException();

                case 3:
                    throw new DirectoryNotFoundException();

                case 5:
                    throw new UnauthorizedAccessException();

                case 0xce:
                    throw new PathTooLongException();

                case 15:
                    throw new DriveNotFoundException();
            }
            if ((num != 0x3e3) && (num != 0x4c7))
            {
                throw new IOException(new Win32Exception(errorCode).Message, Marshal.GetHRForLastWin32Error());
            }
            throw new OperationCanceledException();
        }

        private static UIOptionInternal ToUIOptionInternal(UIOption showUI)
        {
            switch (showUI)
            {
                case UIOption.OnlyErrorDialogs:
                    return UIOptionInternal.OnlyErrorDialogs;

                case UIOption.AllDialogs:
                    return UIOptionInternal.AllDialogs;
            }
            throw new InvalidEnumArgumentException("showUI", (int) showUI, typeof(UIOption));
        }

        private static void VerifyDeleteDirectoryOption(string argName, DeleteDirectoryOption argValue)
        {
            if ((argValue != DeleteDirectoryOption.DeleteAllContents) && (argValue != DeleteDirectoryOption.ThrowIfDirectoryNonEmpty))
            {
                throw new InvalidEnumArgumentException(argName, (int) argValue, typeof(DeleteDirectoryOption));
            }
        }

        private static void VerifyRecycleOption(string argName, RecycleOption argValue)
        {
            if ((argValue != RecycleOption.DeletePermanently) && (argValue != RecycleOption.SendToRecycleBin))
            {
                throw new InvalidEnumArgumentException(argName, (int) argValue, typeof(RecycleOption));
            }
        }

        private static void VerifySearchOption(string argName, Microsoft.VisualBasic.FileIO.SearchOption argValue)
        {
            if ((argValue != Microsoft.VisualBasic.FileIO.SearchOption.SearchAllSubDirectories) && (argValue != Microsoft.VisualBasic.FileIO.SearchOption.SearchTopLevelOnly))
            {
                throw new InvalidEnumArgumentException(argName, (int) argValue, typeof(Microsoft.VisualBasic.FileIO.SearchOption));
            }
        }

        private static void VerifyUICancelOption(string argName, UICancelOption argValue)
        {
            if ((argValue != UICancelOption.DoNothing) && (argValue != UICancelOption.ThrowException))
            {
                throw new InvalidEnumArgumentException(argName, (int) argValue, typeof(UICancelOption));
            }
        }

        public static void WriteAllBytes(string file, byte[] data, bool append)
        {
            CheckFilePathTrailingSeparator(file, "file");
            FileStream stream = null;
            try
            {
                FileMode create;
                if (append)
                {
                    create = FileMode.Append;
                }
                else
                {
                    create = FileMode.Create;
                }
                stream = new FileStream(file, create, FileAccess.Write, FileShare.Read);
                stream.Write(data, 0, data.Length);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
        }

        public static void WriteAllText(string file, string text, bool append)
        {
            WriteAllText(file, text, append, Encoding.UTF8);
        }

        public static void WriteAllText(string file, string text, bool append, Encoding encoding)
        {
            CheckFilePathTrailingSeparator(file, "file");
            StreamWriter writer = null;
            try
            {
                if (append && File.Exists(file))
                {
                    StreamReader reader = null;
                    try
                    {
                        reader = new StreamReader(file, encoding, true);
                        char[] buffer = new char[10];
                        reader.Read(buffer, 0, 10);
                        encoding = reader.CurrentEncoding;
                    }
                    catch (IOException)
                    {
                    }
                    finally
                    {
                        if (reader != null)
                        {
                            reader.Close();
                        }
                    }
                }
                writer = new StreamWriter(file, append, encoding);
                writer.Write(text);
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                }
            }
        }

        public static string CurrentDirectory
        {
            get
            {
                return NormalizePath(Directory.GetCurrentDirectory());
            }
            set
            {
                Directory.SetCurrentDirectory(value);
            }
        }

        public static ReadOnlyCollection<DriveInfo> Drives
        {
            get
            {
                Collection<DriveInfo> list = new Collection<DriveInfo>();
                foreach (DriveInfo info in DriveInfo.GetDrives())
                {
                    list.Add(info);
                }
                return new ReadOnlyCollection<DriveInfo>(list);
            }
        }

        private enum CopyOrMove
        {
            Copy,
            Move
        }

        private class DirectoryNode
        {
            private string m_Path;
            private Collection<FileSystem.DirectoryNode> m_SubDirs;
            private string m_TargetPath;

            internal DirectoryNode(string DirectoryPath, string TargetDirectoryPath)
            {
                this.m_Path = DirectoryPath;
                this.m_TargetPath = TargetDirectoryPath;
                this.m_SubDirs = new Collection<FileSystem.DirectoryNode>();
                foreach (string str in Directory.GetDirectories(this.m_Path))
                {
                    string targetDirectoryPath = System.IO.Path.Combine(this.m_TargetPath, System.IO.Path.GetFileName(str));
                    this.m_SubDirs.Add(new FileSystem.DirectoryNode(str, targetDirectoryPath));
                }
            }

            internal string Path
            {
                get
                {
                    return this.m_Path;
                }
            }

            internal Collection<FileSystem.DirectoryNode> SubDirs
            {
                get
                {
                    return this.m_SubDirs;
                }
            }

            internal string TargetPath
            {
                get
                {
                    return this.m_TargetPath;
                }
            }
        }

        private enum FileOrDirectory
        {
            File,
            Directory
        }

        private class TextSearchHelper
        {
            private bool m_CheckPreamble;
            private System.Text.Decoder m_Decoder;
            private bool m_IgnoreCase;
            private byte[] m_Preamble;
            private char[] m_PreviousCharBuffer;
            private string m_SearchText;

            private TextSearchHelper()
            {
                this.m_PreviousCharBuffer = new char[0];
                this.m_CheckPreamble = true;
            }

            internal TextSearchHelper(Encoding Encoding, string Text, bool IgnoreCase)
            {
                this.m_PreviousCharBuffer = new char[0];
                this.m_CheckPreamble = true;
                this.m_Decoder = Encoding.GetDecoder();
                this.m_Preamble = Encoding.GetPreamble();
                this.m_IgnoreCase = IgnoreCase;
                if (this.m_IgnoreCase)
                {
                    this.m_SearchText = Text.ToUpper(CultureInfo.CurrentCulture);
                }
                else
                {
                    this.m_SearchText = Text;
                }
            }

            private static bool BytesMatch(byte[] BigBuffer, byte[] SmallBuffer)
            {
                if ((BigBuffer.Length < SmallBuffer.Length) | (SmallBuffer.Length == 0))
                {
                    return false;
                }
                int num2 = SmallBuffer.Length - 1;
                for (int i = 0; i <= num2; i++)
                {
                    if (BigBuffer[i] != SmallBuffer[i])
                    {
                        return false;
                    }
                }
                return true;
            }

            internal bool IsTextFound(byte[] ByteBuffer, int Count)
            {
                int index = 0;
                if (this.m_CheckPreamble)
                {
                    if (BytesMatch(ByteBuffer, this.m_Preamble))
                    {
                        index = this.m_Preamble.Length;
                        Count -= this.m_Preamble.Length;
                    }
                    this.m_CheckPreamble = false;
                    if (Count <= 0)
                    {
                        return false;
                    }
                }
                int num3 = this.m_Decoder.GetCharCount(ByteBuffer, index, Count);
                char[] destinationArray = new char[((this.m_PreviousCharBuffer.Length + num3) - 1) + 1];
                Array.Copy(this.m_PreviousCharBuffer, 0, destinationArray, 0, this.m_PreviousCharBuffer.Length);
                int num2 = this.m_Decoder.GetChars(ByteBuffer, index, Count, destinationArray, this.m_PreviousCharBuffer.Length);
                if (destinationArray.Length > this.m_SearchText.Length)
                {
                    if (this.m_PreviousCharBuffer.Length != this.m_SearchText.Length)
                    {
                        this.m_PreviousCharBuffer = new char[(this.m_SearchText.Length - 1) + 1];
                    }
                    Array.Copy(destinationArray, destinationArray.Length - this.m_SearchText.Length, this.m_PreviousCharBuffer, 0, this.m_SearchText.Length);
                }
                else
                {
                    this.m_PreviousCharBuffer = destinationArray;
                }
                if (this.m_IgnoreCase)
                {
                    return new string(destinationArray).ToUpper(CultureInfo.CurrentCulture).Contains(this.m_SearchText);
                }
                return new string(destinationArray).Contains(this.m_SearchText);
            }
        }

        private enum UIOptionInternal
        {
            AllDialogs = 3,
            NoUI = 4,
            OnlyErrorDialogs = 2
        }
    }
}

