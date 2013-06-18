namespace Microsoft.VisualBasic.CompilerServices
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Reflection;
    using System.Security;
    using System.Threading;

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class IOUtils
    {
        private IOUtils()
        {
        }

        private static string FindFileFilter(AssemblyData oAssemblyData)
        {
            FileSystemInfo[] dirFiles = oAssemblyData.m_DirFiles;
            int dirNextFileIndex = oAssemblyData.m_DirNextFileIndex;
            while (true)
            {
                if (dirNextFileIndex > dirFiles.GetUpperBound(0))
                {
                    oAssemblyData.m_DirFiles = null;
                    oAssemblyData.m_DirNextFileIndex = 0;
                    return null;
                }
                FileSystemInfo info = dirFiles[dirNextFileIndex];
                if (((info.Attributes & (FileAttributes.Directory | FileAttributes.System | FileAttributes.Hidden)) == 0) || ((info.Attributes & oAssemblyData.m_DirAttributes) != 0))
                {
                    oAssemblyData.m_DirNextFileIndex = dirNextFileIndex + 1;
                    return dirFiles[dirNextFileIndex].Name;
                }
                dirNextFileIndex++;
            }
        }

        [SecurityCritical]
        internal static string FindFirstFile(Assembly assem, string PathName, FileAttributes Attributes)
        {
            string fullPath = null;
            string fileName;
            FileSystemInfo[] fileSystemInfos;
            if ((PathName.Length > 0) && (PathName[PathName.Length - 1] == Path.DirectorySeparatorChar))
            {
                fullPath = Path.GetFullPath(PathName);
                fileName = "*.*";
            }
            else
            {
                if (PathName.Length == 0)
                {
                    fileName = "*.*";
                }
                else
                {
                    fileName = Path.GetFileName(PathName);
                    fullPath = Path.GetDirectoryName(PathName);
                    if (((fileName == null) || (fileName.Length == 0)) || (fileName == "."))
                    {
                        fileName = "*.*";
                    }
                }
                if ((fullPath == null) || (fullPath.Length == 0))
                {
                    if (Path.IsPathRooted(PathName))
                    {
                        fullPath = Path.GetPathRoot(PathName);
                    }
                    else
                    {
                        fullPath = Environment.CurrentDirectory;
                        if (fullPath[fullPath.Length - 1] != Path.DirectorySeparatorChar)
                        {
                            fullPath = fullPath + Conversions.ToString(Path.DirectorySeparatorChar);
                        }
                    }
                }
                else if (fullPath[fullPath.Length - 1] != Path.DirectorySeparatorChar)
                {
                    fullPath = fullPath + Conversions.ToString(Path.DirectorySeparatorChar);
                }
                if (fileName == "..")
                {
                    fullPath = fullPath + @"..\";
                    fileName = "*.*";
                }
            }
            try
            {
                fileSystemInfos = Directory.GetParent(fullPath + fileName).GetFileSystemInfos(fileName);
            }
            catch (SecurityException exception)
            {
                throw exception;
            }
            catch when (?)
            {
                throw ExceptionUtils.VbMakeException(0x34);
            }
            catch (StackOverflowException exception3)
            {
                throw exception3;
            }
            catch (OutOfMemoryException exception4)
            {
                throw exception4;
            }
            catch (ThreadAbortException exception5)
            {
                throw exception5;
            }
            catch (Exception)
            {
                return "";
            }
            AssemblyData assemblyData = ProjectData.GetProjectData().GetAssemblyData(assem);
            assemblyData.m_DirFiles = fileSystemInfos;
            assemblyData.m_DirNextFileIndex = 0;
            assemblyData.m_DirAttributes = Attributes;
            if ((fileSystemInfos != null) && (fileSystemInfos.Length != 0))
            {
                return FindFileFilter(assemblyData);
            }
            return "";
        }

        internal static string FindNextFile(Assembly assem)
        {
            AssemblyData assemblyData = ProjectData.GetProjectData().GetAssemblyData(assem);
            if (assemblyData.m_DirFiles == null)
            {
                throw new ArgumentException(Utils.GetResourceString("DIR_IllegalCall"));
            }
            if (assemblyData.m_DirNextFileIndex > assemblyData.m_DirFiles.GetUpperBound(0))
            {
                assemblyData.m_DirFiles = null;
                assemblyData.m_DirNextFileIndex = 0;
                return null;
            }
            return FindFileFilter(assemblyData);
        }
    }
}

