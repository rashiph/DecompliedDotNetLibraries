namespace Microsoft.Build.Shared
{
    using Microsoft.Build.Collections;
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;

    internal static class FileUtilities
    {
        internal static string cacheDirectory = null;
        private static string executablePath;
        internal const string FileTimeFormat = "yyyy'-'MM'-'dd HH':'mm':'ss'.'fffffff";
        internal const int MaxPath = 260;

        internal static string AttemptToShortenPath(string path)
        {
            if ((path.Length >= NativeMethodsShared.MAX_PATH) || (!IsRootedNoThrow(path) && (((Environment.CurrentDirectory.Length + path.Length) + 1) >= NativeMethodsShared.MAX_PATH)))
            {
                path = GetFullPathNoThrow(path);
            }
            return path;
        }

        internal static void ClearCacheDirectory()
        {
            if (Directory.Exists(GetCacheDirectory()))
            {
                Directory.Delete(GetCacheDirectory(), true);
            }
        }

        private static Uri CreateUriFromPath(string path)
        {
            ErrorUtilities.VerifyThrowArgumentLength(path, "path");
            Uri result = null;
            if (!Uri.TryCreate(path, UriKind.Absolute, out result))
            {
                result = new Uri(path, UriKind.Relative);
            }
            return result;
        }

        internal static void DeleteNoThrow(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (Exception exception)
            {
                if (ExceptionHandling.NotExpectedException(exception))
                {
                    throw;
                }
            }
        }

        internal static bool DirectoryExistsNoThrow(string fullPath)
        {
            fullPath = AttemptToShortenPath(fullPath);
            NativeMethodsShared.WIN32_FILE_ATTRIBUTE_DATA lpFileInformation = new NativeMethodsShared.WIN32_FILE_ATTRIBUTE_DATA();
            return (NativeMethodsShared.GetFileAttributesEx(fullPath, 0, ref lpFileInformation) && ((lpFileInformation.fileAttributes & 0x10) != 0));
        }

        internal static bool EndsWithSlash(string fileSpec)
        {
            if (fileSpec.Length <= 0)
            {
                return false;
            }
            return IsSlash(fileSpec[fileSpec.Length - 1]);
        }

        internal static string EnsureNoLeadingSlash(string path)
        {
            if ((path.Length > 0) && IsSlash(path[0]))
            {
                path = path.Substring(1);
            }
            return path;
        }

        internal static string EnsureNoTrailingSlash(string path)
        {
            if (EndsWithSlash(path))
            {
                path = path.Substring(0, path.Length - 1);
            }
            return path;
        }

        internal static string EnsureTrailingSlash(string fileSpec)
        {
            if ((fileSpec.Length > 0) && !EndsWithSlash(fileSpec))
            {
                fileSpec = fileSpec + Path.DirectorySeparatorChar;
            }
            return fileSpec;
        }

        internal static bool FileExistsNoThrow(string fullPath)
        {
            fullPath = AttemptToShortenPath(fullPath);
            NativeMethodsShared.WIN32_FILE_ATTRIBUTE_DATA lpFileInformation = new NativeMethodsShared.WIN32_FILE_ATTRIBUTE_DATA();
            return (NativeMethodsShared.GetFileAttributesEx(fullPath, 0, ref lpFileInformation) && ((lpFileInformation.fileAttributes & 0x10) == 0));
        }

        internal static bool FileOrDirectoryExistsNoThrow(string fullPath)
        {
            fullPath = AttemptToShortenPath(fullPath);
            NativeMethodsShared.WIN32_FILE_ATTRIBUTE_DATA lpFileInformation = new NativeMethodsShared.WIN32_FILE_ATTRIBUTE_DATA();
            return NativeMethodsShared.GetFileAttributesEx(fullPath, 0, ref lpFileInformation);
        }

        internal static string GetCacheDirectory()
        {
            if (cacheDirectory == null)
            {
                cacheDirectory = Path.Combine(Path.GetTempPath(), string.Format(Thread.CurrentThread.CurrentUICulture, "MSBuild{0}", new object[] { Process.GetCurrentProcess().Id }));
            }
            return cacheDirectory;
        }

        internal static string GetDirectory(string fileSpec)
        {
            string directoryName = Path.GetDirectoryName(fileSpec);
            if (directoryName == null)
            {
                return fileSpec;
            }
            if ((directoryName.Length > 0) && !EndsWithSlash(directoryName))
            {
                directoryName = directoryName + Path.DirectorySeparatorChar;
            }
            return directoryName;
        }

        internal static string GetDirectoryNameOfFullPath(string fullPath)
        {
            if (fullPath == null)
            {
                return null;
            }
            int length = fullPath.Length;
            while (((length > 0) && (fullPath[--length] != Path.DirectorySeparatorChar)) && (fullPath[length] != Path.AltDirectorySeparatorChar))
            {
            }
            return fullPath.Substring(0, length);
        }

        internal static FileInfo GetFileInfoNoThrow(string filePath)
        {
            FileInfo info;
            filePath = AttemptToShortenPath(filePath);
            try
            {
                info = new FileInfo(filePath);
            }
            catch (Exception exception)
            {
                if (ExceptionHandling.NotExpectedException(exception))
                {
                    throw;
                }
                return null;
            }
            if (info.Exists)
            {
                return info;
            }
            return null;
        }

        internal static string GetFullPath(string fileSpec, string currentDirectory)
        {
            fileSpec = EscapingUtilities.UnescapeAll(fileSpec);
            string str = EscapingUtilities.Escape(NormalizePath(Path.Combine(currentDirectory, fileSpec)));
            if (EndsWithSlash(str))
            {
                return str;
            }
            Match match = FileUtilitiesRegex.DrivePattern.Match(fileSpec);
            Match match2 = FileUtilitiesRegex.UNCPattern.Match(str);
            if ((!match.Success || (match.Length != fileSpec.Length)) && (!match2.Success || (match2.Length != str.Length)))
            {
                return str;
            }
            return (str + Path.DirectorySeparatorChar);
        }

        internal static string GetFullPathNoThrow(string path)
        {
            try
            {
                path = NormalizePath(path);
            }
            catch (Exception exception)
            {
                if (ExceptionHandling.NotExpectedException(exception))
                {
                    throw;
                }
            }
            return path;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal static string GetTemporaryFile()
        {
            return GetTemporaryFile(".tmp");
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal static string GetTemporaryFile(string extension)
        {
            return GetTemporaryFile(null, extension);
        }

        internal static string GetTemporaryFile(string directory, string extension)
        {
            ErrorUtilities.VerifyThrowArgumentLengthIfNotNull(directory, "directory");
            ErrorUtilities.VerifyThrowArgumentLength(extension, "extension");
            if (extension[0] != '.')
            {
                extension = '.' + extension;
            }
            string path = null;
            try
            {
                directory = directory ?? Path.GetTempPath();
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                path = Path.Combine(directory, Guid.NewGuid().ToString("N") + extension);
                ErrorUtilities.VerifyThrow(!File.Exists(path), "Guid should be unique");
                File.WriteAllText(path, string.Empty);
            }
            catch (Exception exception)
            {
                if (ExceptionHandling.NotExpectedException(exception))
                {
                    throw;
                }
                throw new IOException(ResourceUtilities.FormatResourceString("Shared.FailedCreatingTempFile", new object[] { exception.Message }), exception);
            }
            return path;
        }

        internal static bool HasExtension(string fileName, string[] allowedExtensions)
        {
            string extension = Path.GetExtension(fileName);
            foreach (string str2 in allowedExtensions)
            {
                if (string.Compare(extension, str2, true, CultureInfo.CurrentCulture) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        internal static bool IsMetaprojectFilename(string filename)
        {
            return string.Equals(Path.GetExtension(filename), ".metaproj", StringComparison.OrdinalIgnoreCase);
        }

        internal static bool IsRootedNoThrow(string path)
        {
            try
            {
                return Path.IsPathRooted(path);
            }
            catch (Exception exception)
            {
                if (ExceptionHandling.NotExpectedException(exception))
                {
                    throw;
                }
                return false;
            }
        }

        internal static bool IsSlash(char c)
        {
            if (c != Path.DirectorySeparatorChar)
            {
                return (c == Path.AltDirectorySeparatorChar);
            }
            return true;
        }

        internal static bool IsSolutionFilename(string filename)
        {
            return string.Equals(Path.GetExtension(filename), ".sln", StringComparison.OrdinalIgnoreCase);
        }

        internal static bool IsVCProjFilename(string filename)
        {
            return string.Equals(Path.GetExtension(filename), ".vcproj", StringComparison.OrdinalIgnoreCase);
        }

        internal static string MakeRelative(string basePath, string path)
        {
            ErrorUtilities.VerifyThrowArgumentNull(basePath, "basePath");
            ErrorUtilities.VerifyThrowArgumentLength(path, "path");
            if (basePath.Length == 0)
            {
                return path;
            }
            Uri baseUri = new Uri(EnsureTrailingSlash(basePath), UriKind.Absolute);
            Uri relativeUri = CreateUriFromPath(path);
            if (!relativeUri.IsAbsoluteUri)
            {
                relativeUri = new Uri(baseUri, relativeUri);
            }
            Uri uri3 = baseUri.MakeRelativeUri(relativeUri);
            return Uri.UnescapeDataString(uri3.IsAbsoluteUri ? uri3.LocalPath : uri3.ToString()).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

        internal static string NormalizePath(string path)
        {
            ErrorUtilities.VerifyThrowArgumentLength(path, "path");
            int errorCode = 0;
            int num2 = NativeMethodsShared.MAX_PATH;
            StringBuilder buffer = new StringBuilder(num2 + 1);
            int num3 = NativeMethodsShared.GetFullPathName(path, buffer.Capacity, buffer, IntPtr.Zero);
            errorCode = Marshal.GetLastWin32Error();
            if (num3 > num2)
            {
                num2 = num3;
                buffer = new StringBuilder(num2 + 1);
                num3 = NativeMethodsShared.GetFullPathName(path, buffer.Capacity, buffer, IntPtr.Zero);
                errorCode = Marshal.GetLastWin32Error();
                ErrorUtilities.VerifyThrow((num3 + 1) < buffer.Capacity, "Final buffer capacity should be sufficient for full path name and null terminator.");
            }
            if (num3 <= 0)
            {
                errorCode = -2147024896 | errorCode;
                Marshal.ThrowExceptionForHR(errorCode);
                return null;
            }
            string message = buffer.ToString();
            if (message.Length >= 260)
            {
                throw new PathTooLongException(message);
            }
            message = Path.Combine(message, string.Empty);
            if (message.StartsWith(@"\\", StringComparison.OrdinalIgnoreCase))
            {
                int num4 = 2;
                while (num4 < message.Length)
                {
                    char ch = message[num4];
                    if (ch.Equals('\\'))
                    {
                        num4++;
                        break;
                    }
                    num4++;
                }
                if ((num4 == message.Length) || (message.IndexOf(@"\\?\globalroot", StringComparison.OrdinalIgnoreCase) != -1))
                {
                    message = Path.GetFullPath(message);
                }
            }
            if (string.Equals(message, path, StringComparison.Ordinal))
            {
                message = path;
            }
            return message;
        }

        internal static string TrimAndStripAnyQuotes(string path)
        {
            path = path.Trim();
            path = path.Trim(new char[] { '"' });
            return path;
        }

        internal static string CurrentExecutableConfigurationFilePath
        {
            get
            {
                return (CurrentExecutablePath + ".config");
            }
        }

        internal static string CurrentExecutableDirectory
        {
            get
            {
                return Path.GetDirectoryName(CurrentExecutablePath);
            }
        }

        internal static string CurrentExecutablePath
        {
            get
            {
                if (executablePath == null)
                {
                    StringBuilder buffer = new StringBuilder(NativeMethodsShared.MAX_PATH);
                    if (NativeMethodsShared.GetModuleFileName(NativeMethodsShared.NullHandleRef, buffer, buffer.Capacity) == 0)
                    {
                        throw new Win32Exception();
                    }
                    executablePath = buffer.ToString();
                }
                return executablePath;
            }
        }

        internal static class ItemSpecModifiers
        {
            internal const string AccessedTime = "AccessedTime";
            internal static readonly string[] All = new string[] { "FullPath", "RootDir", "Filename", "Extension", "RelativeDir", "Directory", "RecursiveDir", "Identity", "ModifiedTime", "CreatedTime", "AccessedTime" };
            internal const string CreatedTime = "CreatedTime";
            internal const string Directory = "Directory";
            internal const string Extension = "Extension";
            internal const string Filename = "Filename";
            internal const string FullPath = "FullPath";
            internal const string Identity = "Identity";
            internal const string ModifiedTime = "ModifiedTime";
            internal const string RecursiveDir = "RecursiveDir";
            internal const string RelativeDir = "RelativeDir";
            internal const string RootDir = "RootDir";
            private static Hashtable tableOfItemSpecModifiers;

            internal static string GetItemSpecModifier(string currentDirectory, string itemSpec, string modifier, ref CopyOnWriteDictionary<string, string> cachedModifiers)
            {
                ErrorUtilities.VerifyThrow(itemSpec != null, "Need item-spec to modify.");
                ErrorUtilities.VerifyThrow(modifier != null, "Need modifier to apply to item-spec.");
                string fullPath = null;
                if (cachedModifiers != null)
                {
                    cachedModifiers.TryGetValue(modifier, out fullPath);
                }
                if (fullPath == null)
                {
                    bool flag = true;
                    try
                    {
                        if (string.Compare(modifier, "FullPath", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            if (currentDirectory == null)
                            {
                                currentDirectory = string.Empty;
                            }
                            fullPath = FileUtilities.GetFullPath(itemSpec, currentDirectory);
                            ThrowForUrl(fullPath, itemSpec, currentDirectory);
                        }
                        else if (string.Compare(modifier, "RootDir", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            string str2;
                            if (currentDirectory == null)
                            {
                                currentDirectory = string.Empty;
                            }
                            if ((cachedModifiers == null) || !cachedModifiers.TryGetValue("FullPath", out str2))
                            {
                                str2 = FileUtilities.GetFullPath(itemSpec, currentDirectory);
                                ThrowForUrl(str2, itemSpec, currentDirectory);
                            }
                            fullPath = Path.GetPathRoot(str2);
                            if (!FileUtilities.EndsWithSlash(fullPath))
                            {
                                ErrorUtilities.VerifyThrow(FileUtilitiesRegex.UNCPattern.IsMatch(fullPath), "Only UNC shares should be missing trailing slashes.");
                                fullPath = fullPath + Path.DirectorySeparatorChar;
                            }
                        }
                        else if (string.Compare(modifier, "Filename", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            if (Path.GetDirectoryName(itemSpec) == null)
                            {
                                fullPath = string.Empty;
                            }
                            else
                            {
                                fullPath = Path.GetFileNameWithoutExtension(itemSpec);
                            }
                        }
                        else if (string.Compare(modifier, "Extension", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            if (Path.GetDirectoryName(itemSpec) == null)
                            {
                                fullPath = string.Empty;
                            }
                            else
                            {
                                fullPath = Path.GetExtension(itemSpec);
                            }
                        }
                        else if (string.Compare(modifier, "RelativeDir", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            fullPath = FileUtilities.GetDirectory(itemSpec);
                        }
                        else if (string.Compare(modifier, "Directory", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            string str3;
                            if (currentDirectory == null)
                            {
                                currentDirectory = string.Empty;
                            }
                            if ((cachedModifiers == null) || !cachedModifiers.TryGetValue("FullPath", out str3))
                            {
                                str3 = FileUtilities.GetFullPath(itemSpec, currentDirectory);
                                ThrowForUrl(str3, itemSpec, currentDirectory);
                            }
                            fullPath = FileUtilities.GetDirectory(str3);
                            Match match = FileUtilitiesRegex.DrivePattern.Match(fullPath);
                            if (!match.Success)
                            {
                                match = FileUtilitiesRegex.UNCPattern.Match(fullPath);
                            }
                            if (match.Success)
                            {
                                ErrorUtilities.VerifyThrow((fullPath.Length > match.Length) && FileUtilities.IsSlash(fullPath[match.Length]), "Root directory must have a trailing slash.");
                                fullPath = fullPath.Substring(match.Length + 1);
                            }
                        }
                        else if (string.Compare(modifier, "RecursiveDir", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            fullPath = string.Empty;
                        }
                        else if (string.Compare(modifier, "Identity", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            flag = cachedModifiers != null;
                            fullPath = itemSpec;
                        }
                        else if (string.Compare(modifier, "ModifiedTime", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            flag = false;
                            FileInfo fileInfoNoThrow = FileUtilities.GetFileInfoNoThrow(EscapingUtilities.UnescapeAll(itemSpec));
                            if (fileInfoNoThrow != null)
                            {
                                fullPath = fileInfoNoThrow.LastWriteTime.ToString("yyyy'-'MM'-'dd HH':'mm':'ss'.'fffffff", null);
                            }
                            else
                            {
                                fullPath = string.Empty;
                            }
                        }
                        else if (string.Compare(modifier, "CreatedTime", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            flag = false;
                            string path = EscapingUtilities.UnescapeAll(itemSpec);
                            if (File.Exists(path))
                            {
                                fullPath = File.GetCreationTime(path).ToString("yyyy'-'MM'-'dd HH':'mm':'ss'.'fffffff", null);
                            }
                            else
                            {
                                fullPath = string.Empty;
                            }
                        }
                        else if (string.Compare(modifier, "AccessedTime", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            flag = false;
                            string str6 = EscapingUtilities.UnescapeAll(itemSpec);
                            if (File.Exists(str6))
                            {
                                fullPath = File.GetLastAccessTime(str6).ToString("yyyy'-'MM'-'dd HH':'mm':'ss'.'fffffff", null);
                            }
                            else
                            {
                                fullPath = string.Empty;
                            }
                        }
                        else
                        {
                            ErrorUtilities.VerifyThrow(false, "\"{0}\" is not a valid item-spec modifier.", modifier);
                        }
                    }
                    catch (Exception exception)
                    {
                        if (ExceptionHandling.NotExpectedException(exception))
                        {
                            throw;
                        }
                        ErrorUtilities.VerifyThrowInvalidOperation(false, "Shared.InvalidFilespecForTransform", modifier, itemSpec, exception.Message);
                    }
                    ErrorUtilities.VerifyThrow(fullPath != null, "The item-spec modifier \"{0}\" was not evaluated.", modifier);
                    if (flag)
                    {
                        if (cachedModifiers == null)
                        {
                            cachedModifiers = new CopyOnWriteDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        }
                        cachedModifiers[modifier] = fullPath;
                    }
                }
                return fullPath;
            }

            internal static bool IsDerivableItemSpecModifier(string name)
            {
                bool flag = IsItemSpecModifier(name);
                return (((!flag || (name.Length != 12)) || ((name[0] != 'R') && (name[0] != 'r'))) && flag);
            }

            internal static bool IsItemSpecModifier(string name)
            {
                if (name != null)
                {
                    switch (name.Length)
                    {
                        case 7:
                            switch (name[0])
                            {
                                case 'R':
                                    if (!(name == "RootDir"))
                                    {
                                        goto Label_01C0;
                                    }
                                    return true;

                                case 'r':
                                    goto Label_01C0;
                            }
                            return false;

                        case 8:
                        {
                            char ch2 = name[0];
                            if (ch2 > 'I')
                            {
                                switch (ch2)
                                {
                                    case 'f':
                                    case 'i':
                                        goto Label_01C0;
                                }
                                break;
                            }
                            switch (ch2)
                            {
                                case 'F':
                                    if ((name != "FullPath") && !(name == "Filename"))
                                    {
                                        goto Label_01C0;
                                    }
                                    return true;

                                case 'I':
                                    if (!(name == "Identity"))
                                    {
                                        goto Label_01C0;
                                    }
                                    return true;
                            }
                            break;
                        }
                        case 9:
                            switch (name[0])
                            {
                                case 'D':
                                    if (!(name == "Directory"))
                                    {
                                        goto Label_01C0;
                                    }
                                    return true;

                                case 'E':
                                    if (!(name == "Extension"))
                                    {
                                        goto Label_01C0;
                                    }
                                    return true;

                                case 'd':
                                case 'e':
                                    goto Label_01C0;
                            }
                            return false;

                        case 10:
                            goto Label_01BE;

                        case 11:
                        {
                            char ch4 = name[0];
                            if (ch4 > 'R')
                            {
                                switch (ch4)
                                {
                                    case 'c':
                                    case 'r':
                                        goto Label_01C0;
                                }
                            }
                            else
                            {
                                switch (ch4)
                                {
                                    case 'C':
                                        if (!(name == "CreatedTime"))
                                        {
                                            goto Label_01C0;
                                        }
                                        return true;

                                    case 'R':
                                        if (!(name == "RelativeDir"))
                                        {
                                            goto Label_01C0;
                                        }
                                        return true;
                                }
                            }
                            return false;
                        }
                        case 12:
                        {
                            char ch5 = name[0];
                            if (ch5 > 'R')
                            {
                                switch (ch5)
                                {
                                    case 'a':
                                    case 'm':
                                    case 'r':
                                        goto Label_01C0;
                                }
                            }
                            else
                            {
                                switch (ch5)
                                {
                                    case 'A':
                                        if (!(name == "AccessedTime"))
                                        {
                                            goto Label_01C0;
                                        }
                                        return true;

                                    case 'M':
                                        if (!(name == "ModifiedTime"))
                                        {
                                            goto Label_01C0;
                                        }
                                        return true;

                                    case 'R':
                                        if (!(name == "RecursiveDir"))
                                        {
                                            goto Label_01C0;
                                        }
                                        return true;
                                }
                            }
                            return false;
                        }
                        default:
                            goto Label_01BE;
                    }
                }
                return false;
            Label_01BE:
                return false;
            Label_01C0:
                return TableOfItemSpecModifiers.ContainsKey(name);
            }

            private static void ThrowForUrl(string fullPath, string itemSpec, string currentDirectory)
            {
                if (fullPath.IndexOf(':') != fullPath.LastIndexOf(':'))
                {
                    fullPath = Path.GetFullPath(Path.Combine(currentDirectory, itemSpec));
                }
            }

            internal static Hashtable TableOfItemSpecModifiers
            {
                get
                {
                    if (tableOfItemSpecModifiers == null)
                    {
                        tableOfItemSpecModifiers = new Hashtable(All.Length, StringComparer.OrdinalIgnoreCase);
                        foreach (string str in All)
                        {
                            tableOfItemSpecModifiers[str] = string.Empty;
                        }
                    }
                    return tableOfItemSpecModifiers;
                }
            }
        }
    }
}

