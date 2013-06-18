namespace Microsoft.Build.Shared
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System.Text.RegularExpressions;

    internal static class FileMatcher
    {
        private static readonly string altDirectorySeparator = new string(Path.AltDirectorySeparatorChar, 1);
        private static readonly Microsoft.Build.Shared.DirectoryExists defaultDirectoryExists = new Microsoft.Build.Shared.DirectoryExists(Directory.Exists);
        private static readonly GetFileSystemEntries defaultGetFileSystemEntries = new GetFileSystemEntries(Microsoft.Build.Shared.FileMatcher.GetAccessibleFileSystemEntries);
        private static readonly string directorySeparator = new string(Path.DirectorySeparatorChar, 1);
        internal static readonly char[] directorySeparatorCharacters = new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
        private const string dotdot = "..";
        private const string recursiveDirectoryMatch = "**";
        private static readonly char[] wildcardAndSemicolonCharacters = new char[] { '*', '?', ';' };
        private static readonly char[] wildcardCharacters = new char[] { '*', '?' };

        internal static Result FileMatch(string filespec, string fileToMatch)
        {
            Regex regex;
            Result result = new Result();
            fileToMatch = GetLongPathName(fileToMatch, defaultGetFileSystemEntries);
            GetFileSpecInfo(filespec, out regex, out result.isFileSpecRecursive, out result.isLegalFileSpec, defaultGetFileSystemEntries);
            if (result.isLegalFileSpec)
            {
                Match match = regex.Match(fileToMatch);
                result.isMatch = match.Success;
                if (result.isMatch)
                {
                    result.fixedDirectoryPart = match.Groups["FIXEDDIR"].Value;
                    result.wildcardDirectoryPart = match.Groups["WILDCARDDIR"].Value;
                    result.filenamePart = match.Groups["FILENAME"].Value;
                }
            }
            return result;
        }

        private static string[] GetAccessibleDirectories(string path, string pattern)
        {
            try
            {
                string[] paths = null;
                if (pattern == null)
                {
                    paths = Directory.GetDirectories((path.Length == 0) ? @".\" : path);
                }
                else
                {
                    paths = Directory.GetDirectories((path.Length == 0) ? @".\" : path, pattern);
                }
                if (!path.StartsWith(@".\", StringComparison.Ordinal))
                {
                    RemoveInitialDotSlash(paths);
                }
                return paths;
            }
            catch (SecurityException)
            {
                return new string[0];
            }
            catch (UnauthorizedAccessException)
            {
                return new string[0];
            }
        }

        private static string[] GetAccessibleFiles(string path, string filespec, string projectDirectory, bool stripProjectDirectory)
        {
            try
            {
                string str = (path.Length == 0) ? @".\" : path;
                string[] paths = (filespec == null) ? Directory.GetFiles(str) : Directory.GetFiles(str, filespec);
                if (stripProjectDirectory)
                {
                    RemoveProjectDirectory(paths, projectDirectory);
                }
                else if (!path.StartsWith(@".\", StringComparison.Ordinal))
                {
                    RemoveInitialDotSlash(paths);
                }
                return paths;
            }
            catch (SecurityException)
            {
                return new string[0];
            }
            catch (UnauthorizedAccessException)
            {
                return new string[0];
            }
        }

        private static string[] GetAccessibleFilesAndDirectories(string path, string pattern)
        {
            string[] fileSystemEntries = null;
            if (Directory.Exists(path))
            {
                try
                {
                    fileSystemEntries = Directory.GetFileSystemEntries(path, pattern);
                }
                catch (UnauthorizedAccessException)
                {
                }
                catch (SecurityException)
                {
                }
            }
            if (fileSystemEntries == null)
            {
                fileSystemEntries = new string[0];
            }
            return fileSystemEntries;
        }

        private static string[] GetAccessibleFileSystemEntries(FileSystemEntity entityType, string path, string pattern, string projectDirectory, bool stripProjectDirectory)
        {
            switch (entityType)
            {
                case FileSystemEntity.Files:
                    return GetAccessibleFiles(path, pattern, projectDirectory, stripProjectDirectory);

                case FileSystemEntity.Directories:
                    return GetAccessibleDirectories(path, pattern);

                case FileSystemEntity.FilesAndDirectories:
                    return GetAccessibleFilesAndDirectories(path, pattern);
            }
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(false, "Unexpected filesystem entity type.");
            return null;
        }

        internal static string[] GetFiles(string projectDirectoryUnescaped, string filespecUnescaped)
        {
            return GetFiles(projectDirectoryUnescaped, filespecUnescaped, defaultGetFileSystemEntries, defaultDirectoryExists);
        }

        internal static string[] GetFiles(string projectDirectoryUnescaped, string filespecUnescaped, GetFileSystemEntries getFileSystemEntries, Microsoft.Build.Shared.DirectoryExists directoryExists)
        {
            string str;
            string str2;
            string str3;
            string str4;
            bool flag;
            bool flag2;
            if (!HasWildcards(filespecUnescaped))
            {
                return new string[] { filespecUnescaped };
            }
            ArrayList list = new ArrayList();
            IList listOfFiles = list;
            GetFileSpecInfo(filespecUnescaped, out str, out str2, out str3, out str4, out flag, out flag2, getFileSystemEntries);
            if (!flag2)
            {
                return new string[] { filespecUnescaped };
            }
            bool stripProjectDirectory = false;
            if (projectDirectoryUnescaped != null)
            {
                if (str != null)
                {
                    string b = str;
                    try
                    {
                        str = Path.Combine(projectDirectoryUnescaped, str);
                    }
                    catch (ArgumentException)
                    {
                        return new string[0];
                    }
                    stripProjectDirectory = !string.Equals(str, b, StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    str = projectDirectoryUnescaped;
                    stripProjectDirectory = true;
                }
            }
            if ((str.Length > 0) && !directoryExists(str))
            {
                return new string[0];
            }
            bool flag4 = (str2.Length > 0) && (str2 != ("**" + directorySeparator));
            string str6 = flag4 ? null : Path.GetExtension(str3);
            bool flag5 = ((str6 != null) && (str6.IndexOf('*') == -1)) && (str6.EndsWith("?", StringComparison.Ordinal) || ((str6.Length == 4) && (str3.IndexOf('*') != -1)));
            GetFilesRecursive(listOfFiles, str, str2, flag4 ? null : str3, flag5 ? str6.Length : 0, flag4 ? new Regex(str4, RegexOptions.IgnoreCase) : null, flag, projectDirectoryUnescaped, stripProjectDirectory, getFileSystemEntries);
            return (string[]) list.ToArray(typeof(string));
        }

        internal static void GetFileSpecInfo(string filespec, out Regex regexFileMatch, out bool needsRecursion, out bool isLegalFileSpec, GetFileSystemEntries getFileSystemEntries)
        {
            string str;
            string str2;
            string str3;
            string str4;
            GetFileSpecInfo(filespec, out str, out str2, out str3, out str4, out needsRecursion, out isLegalFileSpec, getFileSystemEntries);
            if (isLegalFileSpec)
            {
                regexFileMatch = new Regex(str4, RegexOptions.IgnoreCase);
            }
            else
            {
                regexFileMatch = null;
            }
        }

        private static void GetFileSpecInfo(string filespec, out string fixedDirectoryPart, out string wildcardDirectoryPart, out string filenamePart, out string matchFileExpression, out bool needsRecursion, out bool isLegalFileSpec, GetFileSystemEntries getFileSystemEntries)
        {
            isLegalFileSpec = true;
            needsRecursion = false;
            fixedDirectoryPart = string.Empty;
            wildcardDirectoryPart = string.Empty;
            filenamePart = string.Empty;
            matchFileExpression = null;
            if (-1 != filespec.IndexOfAny(Path.GetInvalidPathChars()))
            {
                isLegalFileSpec = false;
            }
            else if (-1 != filespec.IndexOf("...", StringComparison.Ordinal))
            {
                isLegalFileSpec = false;
            }
            else
            {
                int num = filespec.LastIndexOf(":", StringComparison.Ordinal);
                if ((-1 != num) && (1 != num))
                {
                    isLegalFileSpec = false;
                }
                else
                {
                    SplitFileSpec(filespec, out fixedDirectoryPart, out wildcardDirectoryPart, out filenamePart, getFileSystemEntries);
                    matchFileExpression = RegularExpressionFromFileSpec(fixedDirectoryPart, wildcardDirectoryPart, filenamePart, out isLegalFileSpec);
                    if (isLegalFileSpec)
                    {
                        needsRecursion = wildcardDirectoryPart.Length != 0;
                    }
                }
            }
        }

        private static void GetFilesRecursive(IList listOfFiles, string baseDirectory, string remainingWildcardDirectory, string filespec, int extensionLengthToEnforce, Regex regexFileMatch, bool needsRecursion, string projectDirectory, bool stripProjectDirectory, GetFileSystemEntries getFileSystemEntries)
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrow((filespec == null) || (regexFileMatch == null), "File-spec overrides the regular expression -- pass null for file-spec if you want to use the regular expression.");
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrow((filespec != null) || (regexFileMatch != null), "Need either a file-spec or a regular expression to match files.");
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(remainingWildcardDirectory != null, "Expected non-null remaning wildcard directory.");
            bool flag = false;
            if (remainingWildcardDirectory.Length == 0)
            {
                flag = true;
            }
            else if (remainingWildcardDirectory.IndexOf("**", StringComparison.Ordinal) == 0)
            {
                flag = true;
            }
            if (flag)
            {
                foreach (string str in getFileSystemEntries(FileSystemEntity.Files, baseDirectory, filespec, projectDirectory, stripProjectDirectory))
                {
                    if (((filespec != null) || regexFileMatch.IsMatch(str)) && (((filespec == null) || (extensionLengthToEnforce == 0)) || (Path.GetExtension(str).Length == extensionLengthToEnforce)))
                    {
                        listOfFiles.Add(str);
                    }
                }
            }
            if (needsRecursion && (remainingWildcardDirectory.Length > 0))
            {
                string pattern = null;
                if (remainingWildcardDirectory != "**")
                {
                    int length = remainingWildcardDirectory.IndexOfAny(directorySeparatorCharacters);
                    Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(length != -1, "Slash should be guaranteed.");
                    pattern = remainingWildcardDirectory.Substring(0, length);
                    remainingWildcardDirectory = remainingWildcardDirectory.Substring(length + 1);
                    if (pattern == "**")
                    {
                        pattern = null;
                        remainingWildcardDirectory = "**";
                    }
                }
                foreach (string str3 in getFileSystemEntries(FileSystemEntity.Directories, baseDirectory, pattern, null, false))
                {
                    GetFilesRecursive(listOfFiles, str3, remainingWildcardDirectory, filespec, extensionLengthToEnforce, regexFileMatch, true, projectDirectory, stripProjectDirectory, getFileSystemEntries);
                }
            }
        }

        internal static string GetLongPathName(string path)
        {
            return GetLongPathName(path, defaultGetFileSystemEntries);
        }

        internal static string GetLongPathName(string path, GetFileSystemEntries getFileSystemEntries)
        {
            string str;
            if (path.IndexOf("~", StringComparison.Ordinal) == -1)
            {
                return path;
            }
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(!HasWildcards(path), "GetLongPathName does not handle wildcards and was passed '{0}'.", path);
            string[] strArray = path.Split(directorySeparatorCharacters);
            int num = 0;
            if (path.StartsWith(directorySeparator + directorySeparator, StringComparison.Ordinal))
            {
                str = ((directorySeparator + directorySeparator) + strArray[2] + directorySeparator) + strArray[3] + directorySeparator;
                num = 4;
            }
            else if ((path.Length > 2) && (path[1] == ':'))
            {
                str = strArray[0] + directorySeparator;
                num = 1;
            }
            else
            {
                str = string.Empty;
                num = 0;
            }
            string[] strArray2 = new string[strArray.Length - num];
            string str2 = str;
            for (int i = num; i < strArray.Length; i++)
            {
                if (strArray[i].Length == 0)
                {
                    strArray2[i - num] = string.Empty;
                }
                else if (strArray[i].IndexOf("~", StringComparison.Ordinal) == -1)
                {
                    strArray2[i - num] = strArray[i];
                    str2 = Path.Combine(str2, strArray[i]);
                }
                else
                {
                    string[] strArray3 = getFileSystemEntries(FileSystemEntity.FilesAndDirectories, str2, strArray[i], null, false);
                    if (strArray3.Length == 0)
                    {
                        for (int j = i; j < strArray.Length; j++)
                        {
                            strArray2[j - num] = strArray[j];
                        }
                        break;
                    }
                    Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(strArray3.Length == 1, "Unexpected number of entries ({3}) found when enumerating '{0}' under '{1}'. Original path was '{2}'", strArray[i], str2, path, strArray3.Length);
                    str2 = strArray3[0];
                    strArray2[i - num] = Path.GetFileName(str2);
                }
            }
            return (str + string.Join(directorySeparator, strArray2));
        }

        internal static bool HasWildcards(string filespec)
        {
            return (-1 != filespec.IndexOfAny(wildcardCharacters));
        }

        internal static bool HasWildcardsSemicolonItemOrPropertyReferences(string filespec)
        {
            if ((-1 == filespec.IndexOfAny(wildcardAndSemicolonCharacters)) && !filespec.Contains("$("))
            {
                return filespec.Contains("@(");
            }
            return true;
        }

        internal static bool IsDirectorySeparator(char c)
        {
            if (c != Path.DirectorySeparatorChar)
            {
                return (c == Path.AltDirectorySeparatorChar);
            }
            return true;
        }

        private static void PreprocessFileSpecForSplitting(string filespec, out string fixedDirectoryPart, out string wildcardDirectoryPart, out string filenamePart)
        {
            int num = filespec.LastIndexOfAny(directorySeparatorCharacters);
            if (-1 == num)
            {
                fixedDirectoryPart = string.Empty;
                wildcardDirectoryPart = string.Empty;
                filenamePart = filespec;
            }
            else
            {
                int length = filespec.IndexOfAny(wildcardCharacters);
                if ((-1 == length) || (length > num))
                {
                    fixedDirectoryPart = filespec.Substring(0, num + 1);
                    wildcardDirectoryPart = string.Empty;
                    filenamePart = filespec.Substring(num + 1);
                }
                else
                {
                    int num3 = filespec.Substring(0, length).LastIndexOfAny(directorySeparatorCharacters);
                    if (-1 == num3)
                    {
                        fixedDirectoryPart = string.Empty;
                        wildcardDirectoryPart = filespec.Substring(0, num + 1);
                        filenamePart = filespec.Substring(num + 1);
                    }
                    else
                    {
                        fixedDirectoryPart = filespec.Substring(0, num3 + 1);
                        wildcardDirectoryPart = filespec.Substring(num3 + 1, num - num3);
                        filenamePart = filespec.Substring(num + 1);
                    }
                }
            }
        }

        private static string RegularExpressionFromFileSpec(string fixedDirectoryPart, string wildcardDirectoryPart, string filenamePart, out bool isLegalFileSpec)
        {
            int length;
            isLegalFileSpec = true;
            if ((((fixedDirectoryPart.IndexOf("<:", StringComparison.Ordinal) != -1) || (fixedDirectoryPart.IndexOf(":>", StringComparison.Ordinal) != -1)) || ((wildcardDirectoryPart.IndexOf("<:", StringComparison.Ordinal) != -1) || (wildcardDirectoryPart.IndexOf(":>", StringComparison.Ordinal) != -1))) || ((filenamePart.IndexOf("<:", StringComparison.Ordinal) != -1) || (filenamePart.IndexOf(":>", StringComparison.Ordinal) != -1)))
            {
                isLegalFileSpec = false;
                return string.Empty;
            }
            if (wildcardDirectoryPart.Contains(".."))
            {
                isLegalFileSpec = false;
                return string.Empty;
            }
            if (filenamePart.EndsWith(".", StringComparison.Ordinal))
            {
                filenamePart = filenamePart.Replace("*", "<:anythingbutdot:>");
                filenamePart = filenamePart.Replace("?", "<:anysinglecharacterbutdot:>");
                filenamePart = filenamePart.Substring(0, filenamePart.Length - 1);
            }
            StringBuilder builder = new StringBuilder();
            builder.Append("<:bol:>");
            builder.Append("<:fixeddir:>").Append(fixedDirectoryPart).Append("<:endfixeddir:>");
            builder.Append("<:wildcarddir:>").Append(wildcardDirectoryPart).Append("<:endwildcarddir:>");
            builder.Append("<:filename:>").Append(filenamePart).Append("<:endfilename:>");
            builder.Append("<:eol:>");
            builder.Replace(directorySeparator, "<:dirseparator:>");
            builder.Replace(altDirectorySeparator, "<:dirseparator:>");
            builder.Replace("<:fixeddir:><:dirseparator:><:dirseparator:>", "<:fixeddir:><:uncslashslash:>");
            do
            {
                length = builder.Length;
                builder.Replace("<:dirseparator:>.<:dirseparator:>", "<:dirseparator:>");
                builder.Replace("<:dirseparator:><:dirseparator:>", "<:dirseparator:>");
                builder.Replace("<:fixeddir:>.<:dirseparator:>.<:dirseparator:>", "<:fixeddir:>.<:dirseparator:>");
                builder.Replace("<:dirseparator:>.<:endfilename:>", "<:endfilename:>");
                builder.Replace("<:filename:>.<:endfilename:>", "<:filename:><:endfilename:>");
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(builder.Length <= length, "Expression reductions cannot increase the length of the expression.");
            }
            while (builder.Length < length);
            do
            {
                length = builder.Length;
                builder.Replace("**<:dirseparator:>**", "**");
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(builder.Length <= length, "Expression reductions cannot increase the length of the expression.");
            }
            while (builder.Length < length);
            do
            {
                length = builder.Length;
                builder.Replace("<:dirseparator:>**<:dirseparator:>", "<:middledirs:>");
                builder.Replace("<:wildcarddir:>**<:dirseparator:>", "<:wildcarddir:><:leftdirs:>");
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(builder.Length <= length, "Expression reductions cannot increase the length of the expression.");
            }
            while (builder.Length < length);
            if (builder.Length > builder.Replace("**", null).Length)
            {
                isLegalFileSpec = false;
                return string.Empty;
            }
            builder.Replace("*.*", "<:anynonseparator:>");
            builder.Replace("*", "<:anynonseparator:>");
            builder.Replace("?", "<:singlecharacter:>");
            builder.Replace(@"\", @"\\");
            builder.Replace("$", @"\$");
            builder.Replace("(", @"\(");
            builder.Replace(")", @"\)");
            builder.Replace("*", @"\*");
            builder.Replace("+", @"\+");
            builder.Replace(".", @"\.");
            builder.Replace("[", @"\[");
            builder.Replace("?", @"\?");
            builder.Replace("^", @"\^");
            builder.Replace("{", @"\{");
            builder.Replace("|", @"\|");
            builder.Replace("<:middledirs:>", @"((/)|(\\)|(/.*/)|(/.*\\)|(\\.*\\)|(\\.*/))");
            builder.Replace("<:leftdirs:>", @"((.*/)|(.*\\)|())");
            builder.Replace("<:rightdirs:>", ".*");
            builder.Replace("<:anything:>", ".*");
            builder.Replace("<:anythingbutdot:>", @"[^\.]*");
            builder.Replace("<:anysinglecharacterbutdot:>", @"[^\.].");
            builder.Replace("<:anynonseparator:>", @"[^/\\]*");
            builder.Replace("<:singlecharacter:>", ".");
            builder.Replace("<:dirseparator:>", @"[/\\]+");
            builder.Replace("<:uncslashslash:>", @"\\\\");
            builder.Replace("<:bol:>", "^");
            builder.Replace("<:eol:>", "$");
            builder.Replace("<:fixeddir:>", "(?<FIXEDDIR>");
            builder.Replace("<:endfixeddir:>", ")");
            builder.Replace("<:wildcarddir:>", "(?<WILDCARDDIR>");
            builder.Replace("<:endwildcarddir:>", ")");
            builder.Replace("<:filename:>", "(?<FILENAME>");
            builder.Replace("<:endfilename:>", ")");
            return builder.ToString();
        }

        private static void RemoveInitialDotSlash(string[] paths)
        {
            for (int i = 0; i < paths.Length; i++)
            {
                if (paths[i].StartsWith(@".\", StringComparison.Ordinal))
                {
                    paths[i] = paths[i].Substring(2);
                }
            }
        }

        internal static void RemoveProjectDirectory(string[] paths, string projectDirectory)
        {
            bool flag = IsDirectorySeparator(projectDirectory[projectDirectory.Length - 1]);
            for (int i = 0; i < paths.Length; i++)
            {
                if (paths[i].StartsWith(projectDirectory, StringComparison.Ordinal))
                {
                    if (!flag)
                    {
                        if ((paths[i].Length > projectDirectory.Length) && IsDirectorySeparator(paths[i][projectDirectory.Length]))
                        {
                            paths[i] = paths[i].Substring(projectDirectory.Length + 1);
                        }
                    }
                    else
                    {
                        paths[i] = paths[i].Substring(projectDirectory.Length);
                    }
                }
            }
        }

        internal static void SplitFileSpec(string filespec, out string fixedDirectoryPart, out string wildcardDirectoryPart, out string filenamePart, GetFileSystemEntries getFileSystemEntries)
        {
            PreprocessFileSpecForSplitting(filespec, out fixedDirectoryPart, out wildcardDirectoryPart, out filenamePart);
            if ("**" == filenamePart)
            {
                wildcardDirectoryPart = wildcardDirectoryPart + "**";
                wildcardDirectoryPart = wildcardDirectoryPart + directorySeparator;
                filenamePart = "*.*";
            }
            fixedDirectoryPart = GetLongPathName(fixedDirectoryPart, getFileSystemEntries);
        }

        internal enum FileSystemEntity
        {
            Files,
            Directories,
            FilesAndDirectories
        }

        internal delegate string[] GetFileSystemEntries(Microsoft.Build.Shared.FileMatcher.FileSystemEntity entityType, string path, string pattern, string projectDirectory, bool stripProjectDirectory);

        internal sealed class Result
        {
            internal string filenamePart = string.Empty;
            internal string fixedDirectoryPart = string.Empty;
            internal bool isFileSpecRecursive;
            internal bool isLegalFileSpec;
            internal bool isMatch;
            internal string wildcardDirectoryPart = string.Empty;

            internal Result()
            {
            }
        }
    }
}

