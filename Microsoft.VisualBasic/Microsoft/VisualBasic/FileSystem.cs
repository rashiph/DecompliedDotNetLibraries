namespace Microsoft.VisualBasic
{
    using Microsoft.VisualBasic.CompilerServices;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;

    [StandardModule, SecurityCritical, HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.ExternalProcessMgmt)]
    public sealed class FileSystem
    {
        private const int A_ALLBITS = 0x3f;
        private const int A_ARCH = 0x20;
        private const int A_HIDDEN = 2;
        private const int A_NORMAL = 0;
        private const int A_RDONLY = 1;
        private const int A_SUBDIR = 0x10;
        private const int A_SYSTEM = 4;
        private const int A_VOLID = 8;
        private const int ERROR_ACCESS_DENIED = 5;
        private const int ERROR_ALREADY_EXISTS = 0xb7;
        private const int ERROR_BAD_NETPATH = 0x35;
        private const int ERROR_FILE_EXISTS = 80;
        private const int ERROR_FILE_NOT_FOUND = 2;
        private const int ERROR_INVALID_ACCESS = 12;
        private const int ERROR_INVALID_PARAMETER = 0x57;
        private const int ERROR_NOT_SAME_DEVICE = 0x11;
        private const int ERROR_WRITE_PROTECT = 0x13;
        internal const int FIRST_LOCAL_CHANNEL = 1;
        internal const int LAST_LOCAL_CHANNEL = 0xff;
        internal static readonly DateTimeFormatInfo m_WriteDateFormatInfo = InitializeWriteDateFormatInfo();
        internal const string sDateFormat = "d";
        internal const string sDateTimeFormat = "F";
        internal const string sTimeFormat = "T";

        private static void AddFileToList(AssemblyData oAssemblyData, int FileNumber, VB6File oFile)
        {
            if (oFile == null)
            {
                throw ExceptionUtils.VbMakeException(0x33);
            }
            oFile.OpenFile();
            oAssemblyData.SetChannelObj(FileNumber, oFile);
        }

        public static void ChDir(string Path)
        {
            Path = Strings.RTrim(Path);
            if ((Path == null) || (Path.Length == 0))
            {
                throw ExceptionUtils.VbMakeException(new ArgumentException(Utils.GetResourceString("Argument_PathNullOrEmpty")), 0x34);
            }
            if (Path == @"\")
            {
                Path = Directory.GetDirectoryRoot(Directory.GetCurrentDirectory());
            }
            try
            {
                Directory.SetCurrentDirectory(Path);
            }
            catch (FileNotFoundException)
            {
                throw ExceptionUtils.VbMakeException(new FileNotFoundException(Utils.GetResourceString("FileSystem_PathNotFound1", new string[] { Path })), 0x4c);
            }
        }

        public static void ChDrive(char Drive)
        {
            Drive = char.ToUpper(Drive, CultureInfo.InvariantCulture);
            if ((Drive < 'A') || (Drive > 'Z'))
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Drive" }));
            }
            if (!UnsafeValidDrive(Drive))
            {
                throw ExceptionUtils.VbMakeException(new IOException(Utils.GetResourceString("FileSystem_DriveNotFound1", new string[] { Conversions.ToString(Drive) })), 0x44);
            }
            Directory.SetCurrentDirectory(Conversions.ToString(Drive) + Conversions.ToString(Path.VolumeSeparatorChar));
        }

        public static void ChDrive(string Drive)
        {
            if ((Drive != null) && (Drive.Length != 0))
            {
                ChDrive(Drive[0]);
            }
        }

        internal static bool CheckFileOpen(AssemblyData oAssemblyData, string sPath, OpenModeTypes NewFileMode)
        {
            int num3 = 0xff;
            for (int i = 1; i <= num3; i++)
            {
                VB6File channelOrNull = GetChannelOrNull(oAssemblyData, i);
                if (channelOrNull != null)
                {
                    OpenMode mode = channelOrNull.GetMode();
                    if (string.Compare(sPath, channelOrNull.GetAbsolutePath(), StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        if (NewFileMode == OpenModeTypes.Any)
                        {
                            return true;
                        }
                        if (((NewFileMode | ((OpenModeTypes) ((int) mode))) != OpenModeTypes.Input) && ((((NewFileMode | ((OpenModeTypes) ((int) mode))) | OpenModeTypes.Binary) | OpenModeTypes.Random) != (OpenModeTypes.Binary | OpenModeTypes.Random)))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static void CheckInputCapable(VB6File oFile)
        {
            if (!oFile.CanInput())
            {
                throw ExceptionUtils.VbMakeException(0x36);
            }
        }

        internal static void CloseAllFiles(AssemblyData oAssemblyData)
        {
            int fileNumber = 1;
            do
            {
                InternalCloseFile(oAssemblyData, fileNumber);
                fileNumber++;
            }
            while (fileNumber <= 0xff);
        }

        internal static void CloseAllFiles(Assembly assem)
        {
            CloseAllFiles(ProjectData.GetProjectData().GetAssemblyData(assem));
        }

        public static string CurDir()
        {
            return Directory.GetCurrentDirectory();
        }

        public static string CurDir(char Drive)
        {
            Drive = char.ToUpper(Drive, CultureInfo.InvariantCulture);
            if ((Drive < 'A') || (Drive > 'Z'))
            {
                throw ExceptionUtils.VbMakeException(new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Drive" })), 0x44);
            }
            string fullPath = Path.GetFullPath(Conversions.ToString(Drive) + Conversions.ToString(Path.VolumeSeparatorChar) + ".");
            if (!UnsafeValidDrive(Drive))
            {
                throw ExceptionUtils.VbMakeException(new IOException(Utils.GetResourceString("FileSystem_DriveNotFound1", new string[] { Conversions.ToString(Drive) })), 0x44);
            }
            return fullPath;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string Dir()
        {
            return IOUtils.FindNextFile(Assembly.GetCallingAssembly());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string Dir(string PathName, FileAttribute Attributes = 0)
        {
            if (Attributes == FileAttribute.Volume)
            {
                IntPtr ptr;
                StringBuilder lpVolumeNameBuffer = new StringBuilder(0x100);
                string lpRootPathName = null;
                if (PathName.Length > 0)
                {
                    lpRootPathName = Path.GetPathRoot(PathName);
                    if (lpRootPathName[lpRootPathName.Length - 1] != Path.DirectorySeparatorChar)
                    {
                        lpRootPathName = lpRootPathName + Conversions.ToString(Path.DirectorySeparatorChar);
                    }
                }
                int lpVolumeSerialNumber = 0;
                int lpMaximumComponentLength = 0;
                int lpFileSystemFlags = 0;
                if (Microsoft.VisualBasic.CompilerServices.NativeMethods.GetVolumeInformation(lpRootPathName, lpVolumeNameBuffer, 0x100, ref lpVolumeSerialNumber, ref lpMaximumComponentLength, ref lpFileSystemFlags, ptr, 0) != 0)
                {
                    return lpVolumeNameBuffer.ToString();
                }
                return "";
            }
            FileAttributes attributes = ((FileAttributes) Attributes) | FileAttributes.Normal;
            return IOUtils.FindFirstFile(Assembly.GetCallingAssembly(), PathName, attributes);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool EOF(int FileNumber)
        {
            return GetStream(Assembly.GetCallingAssembly(), FileNumber).EOF();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static OpenMode FileAttr(int FileNumber)
        {
            return GetStream(Assembly.GetCallingAssembly(), FileNumber).GetMode();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FileClose(params int[] FileNumbers)
        {
            try
            {
                Assembly callingAssembly = Assembly.GetCallingAssembly();
                AssemblyData assemblyData = ProjectData.GetProjectData().GetAssemblyData(callingAssembly);
                if ((FileNumbers == null) || (FileNumbers.Length == 0))
                {
                    CloseAllFiles(assemblyData);
                }
                else
                {
                    int upperBound = FileNumbers.GetUpperBound(0);
                    for (int i = 0; i <= upperBound; i++)
                    {
                        InternalCloseFile(assemblyData, FileNumbers[i]);
                    }
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FileCopy(string Source, string Destination)
        {
            if ((Source == null) || (Source.Length == 0))
            {
                throw ExceptionUtils.VbMakeException(new ArgumentException(Utils.GetResourceString("Argument_PathNullOrEmpty1", new string[] { "Source" })), 0x34);
            }
            if ((Destination == null) || (Destination.Length == 0))
            {
                throw ExceptionUtils.VbMakeException(new ArgumentException(Utils.GetResourceString("Argument_PathNullOrEmpty1", new string[] { "Destination" })), 0x34);
            }
            if (PathContainsWildcards(Source))
            {
                throw ExceptionUtils.VbMakeException(new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Source" })), 0x34);
            }
            if (PathContainsWildcards(Destination))
            {
                throw ExceptionUtils.VbMakeException(new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Destination" })), 0x34);
            }
            AssemblyData assemblyData = ProjectData.GetProjectData().GetAssemblyData(Assembly.GetCallingAssembly());
            if (CheckFileOpen(assemblyData, Destination, OpenModeTypes.Output))
            {
                throw ExceptionUtils.VbMakeException(new IOException(Utils.GetResourceString("FileSystem_FileAlreadyOpen1", new string[] { Destination })), 0x37);
            }
            if (CheckFileOpen(assemblyData, Source, OpenModeTypes.Input))
            {
                throw ExceptionUtils.VbMakeException(new IOException(Utils.GetResourceString("FileSystem_FileAlreadyOpen1", new string[] { Source })), 0x37);
            }
            try
            {
                File.Copy(Source, Destination, true);
                File.SetAttributes(Destination, FileAttributes.Archive);
            }
            catch (FileNotFoundException exception)
            {
                throw ExceptionUtils.VbMakeException(exception, 0x35);
            }
            catch (IOException exception2)
            {
                throw ExceptionUtils.VbMakeException(exception2, 0x37);
            }
            catch (Exception exception3)
            {
                throw exception3;
            }
        }

        public static DateTime FileDateTime(string PathName)
        {
            if (PathContainsWildcards(PathName))
            {
                throw ExceptionUtils.VbMakeException(new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "PathName" })), 0x34);
            }
            if (!File.Exists(PathName))
            {
                throw new FileNotFoundException(Utils.GetResourceString("FileSystem_FileNotFound1", new string[] { PathName }));
            }
            return new FileInfo(PathName).LastWriteTime;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FileGet(int FileNumber, ref bool Value, long RecordNumber = -1L)
        {
            try
            {
                ValidateGetPutRecordNumber(RecordNumber);
                GetStream(Assembly.GetCallingAssembly(), FileNumber).Get(ref Value, RecordNumber);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FileGet(int FileNumber, ref byte Value, long RecordNumber = -1L)
        {
            try
            {
                ValidateGetPutRecordNumber(RecordNumber);
                GetStream(Assembly.GetCallingAssembly(), FileNumber).Get(ref Value, RecordNumber);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FileGet(int FileNumber, ref char Value, long RecordNumber = -1L)
        {
            try
            {
                ValidateGetPutRecordNumber(RecordNumber);
                GetStream(Assembly.GetCallingAssembly(), FileNumber).Get(ref Value, RecordNumber);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FileGet(int FileNumber, ref DateTime Value, long RecordNumber = -1L)
        {
            try
            {
                ValidateGetPutRecordNumber(RecordNumber);
                GetStream(Assembly.GetCallingAssembly(), FileNumber).Get(ref Value, RecordNumber);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FileGet(int FileNumber, ref decimal Value, long RecordNumber = -1L)
        {
            try
            {
                ValidateGetPutRecordNumber(RecordNumber);
                GetStream(Assembly.GetCallingAssembly(), FileNumber).Get(ref Value, RecordNumber);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FileGet(int FileNumber, ref double Value, long RecordNumber = -1L)
        {
            try
            {
                ValidateGetPutRecordNumber(RecordNumber);
                GetStream(Assembly.GetCallingAssembly(), FileNumber).Get(ref Value, RecordNumber);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FileGet(int FileNumber, ref short Value, long RecordNumber = -1L)
        {
            try
            {
                ValidateGetPutRecordNumber(RecordNumber);
                GetStream(Assembly.GetCallingAssembly(), FileNumber).Get(ref Value, RecordNumber);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FileGet(int FileNumber, ref int Value, long RecordNumber = -1L)
        {
            try
            {
                ValidateGetPutRecordNumber(RecordNumber);
                GetStream(Assembly.GetCallingAssembly(), FileNumber).Get(ref Value, RecordNumber);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FileGet(int FileNumber, ref long Value, long RecordNumber = -1L)
        {
            try
            {
                ValidateGetPutRecordNumber(RecordNumber);
                GetStream(Assembly.GetCallingAssembly(), FileNumber).Get(ref Value, RecordNumber);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FileGet(int FileNumber, ref float Value, long RecordNumber = -1L)
        {
            try
            {
                ValidateGetPutRecordNumber(RecordNumber);
                GetStream(Assembly.GetCallingAssembly(), FileNumber).Get(ref Value, RecordNumber);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FileGet(int FileNumber, ref ValueType Value, long RecordNumber = -1L)
        {
            try
            {
                ValidateGetPutRecordNumber(RecordNumber);
                GetStream(Assembly.GetCallingAssembly(), FileNumber).Get(ref Value, RecordNumber);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FileGet(int FileNumber, ref string Value, long RecordNumber = -1L, bool StringIsFixedLength = false)
        {
            try
            {
                ValidateGetPutRecordNumber(RecordNumber);
                GetStream(Assembly.GetCallingAssembly(), FileNumber).Get(ref Value, RecordNumber, StringIsFixedLength);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FileGet(int FileNumber, ref Array Value, long RecordNumber = -1L, bool ArrayIsDynamic = false, bool StringIsFixedLength = false)
        {
            try
            {
                ValidateGetPutRecordNumber(RecordNumber);
                GetStream(Assembly.GetCallingAssembly(), FileNumber).Get(ref Value, RecordNumber, ArrayIsDynamic, StringIsFixedLength);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FileGetObject(int FileNumber, ref object Value, long RecordNumber = -1L)
        {
            try
            {
                ValidateGetPutRecordNumber(RecordNumber);
                GetStream(Assembly.GetCallingAssembly(), FileNumber).GetObject(ref Value, RecordNumber, true);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public static long FileLen(string PathName)
        {
            if (!File.Exists(PathName))
            {
                throw new FileNotFoundException(Utils.GetResourceString("FileSystem_FileNotFound1", new string[] { PathName }));
            }
            return new FileInfo(PathName).Length;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FileOpen(int FileNumber, string FileName, OpenMode Mode, OpenAccess Access = -1, OpenShare Share = -1, int RecordLength = -1)
        {
            try
            {
                ValidateMode(Mode);
                ValidateAccess(Access);
                ValidateShare(Share);
                if ((FileNumber < 1) || (FileNumber > 0xff))
                {
                    throw ExceptionUtils.VbMakeException(0x34);
                }
                vbIOOpenFile(Assembly.GetCallingAssembly(), FileNumber, FileName, Mode, Access, Share, RecordLength);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FilePut(int FileNumber, bool Value, long RecordNumber = -1L)
        {
            try
            {
                ValidateGetPutRecordNumber(RecordNumber);
                GetStream(Assembly.GetCallingAssembly(), FileNumber, OpenModeTypes.Binary | OpenModeTypes.Random).Put(Value, RecordNumber);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FilePut(int FileNumber, byte Value, long RecordNumber = -1L)
        {
            try
            {
                ValidateGetPutRecordNumber(RecordNumber);
                GetStream(Assembly.GetCallingAssembly(), FileNumber, OpenModeTypes.Binary | OpenModeTypes.Random).Put(Value, RecordNumber);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FilePut(int FileNumber, char Value, long RecordNumber = -1L)
        {
            try
            {
                ValidateGetPutRecordNumber(RecordNumber);
                GetStream(Assembly.GetCallingAssembly(), FileNumber, OpenModeTypes.Binary | OpenModeTypes.Random).Put(Value, RecordNumber);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FilePut(int FileNumber, DateTime Value, long RecordNumber = -1L)
        {
            try
            {
                ValidateGetPutRecordNumber(RecordNumber);
                GetStream(Assembly.GetCallingAssembly(), FileNumber, OpenModeTypes.Binary | OpenModeTypes.Random).Put(Value, RecordNumber);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FilePut(int FileNumber, decimal Value, long RecordNumber = -1L)
        {
            try
            {
                ValidateGetPutRecordNumber(RecordNumber);
                GetStream(Assembly.GetCallingAssembly(), FileNumber, OpenModeTypes.Binary | OpenModeTypes.Random).Put(Value, RecordNumber);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FilePut(int FileNumber, double Value, long RecordNumber = -1L)
        {
            try
            {
                ValidateGetPutRecordNumber(RecordNumber);
                GetStream(Assembly.GetCallingAssembly(), FileNumber, OpenModeTypes.Binary | OpenModeTypes.Random).Put(Value, RecordNumber);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FilePut(int FileNumber, short Value, long RecordNumber = -1L)
        {
            try
            {
                ValidateGetPutRecordNumber(RecordNumber);
                GetStream(Assembly.GetCallingAssembly(), FileNumber, OpenModeTypes.Binary | OpenModeTypes.Random).Put(Value, RecordNumber);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FilePut(int FileNumber, int Value, long RecordNumber = -1L)
        {
            try
            {
                ValidateGetPutRecordNumber(RecordNumber);
                GetStream(Assembly.GetCallingAssembly(), FileNumber, OpenModeTypes.Binary | OpenModeTypes.Random).Put(Value, RecordNumber);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FilePut(int FileNumber, long Value, long RecordNumber = -1L)
        {
            try
            {
                ValidateGetPutRecordNumber(RecordNumber);
                GetStream(Assembly.GetCallingAssembly(), FileNumber, OpenModeTypes.Binary | OpenModeTypes.Random).Put(Value, RecordNumber);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FilePut(int FileNumber, float Value, long RecordNumber = -1L)
        {
            try
            {
                ValidateGetPutRecordNumber(RecordNumber);
                GetStream(Assembly.GetCallingAssembly(), FileNumber, OpenModeTypes.Binary | OpenModeTypes.Random).Put(Value, RecordNumber);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FilePut(int FileNumber, ValueType Value, long RecordNumber = -1L)
        {
            try
            {
                ValidateGetPutRecordNumber(RecordNumber);
                GetStream(Assembly.GetCallingAssembly(), FileNumber, OpenModeTypes.Binary | OpenModeTypes.Random).Put(Value, RecordNumber);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining), Obsolete("This member has been deprecated. Please use FilePutObject to write Object types, or coerce FileNumber and RecordNumber to Integer for writing non-Object types. http://go.microsoft.com/fwlink/?linkid=14202")]
        public static void FilePut(object FileNumber, object Value, object RecordNumber = -1)
        {
            throw new ArgumentException(Utils.GetResourceString("UseFilePutObject"));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FilePut(int FileNumber, string Value, long RecordNumber = -1L, bool StringIsFixedLength = false)
        {
            try
            {
                ValidateGetPutRecordNumber(RecordNumber);
                GetStream(Assembly.GetCallingAssembly(), FileNumber, OpenModeTypes.Binary | OpenModeTypes.Random).Put(Value, RecordNumber, StringIsFixedLength);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FilePut(int FileNumber, Array Value, long RecordNumber = -1L, bool ArrayIsDynamic = false, bool StringIsFixedLength = false)
        {
            try
            {
                ValidateGetPutRecordNumber(RecordNumber);
                GetStream(Assembly.GetCallingAssembly(), FileNumber, OpenModeTypes.Binary | OpenModeTypes.Random).Put(Value, RecordNumber, ArrayIsDynamic, StringIsFixedLength);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FilePutObject(int FileNumber, object Value, long RecordNumber = -1L)
        {
            try
            {
                ValidateGetPutRecordNumber(RecordNumber);
                GetStream(Assembly.GetCallingAssembly(), FileNumber, OpenModeTypes.Binary | OpenModeTypes.Random).PutObject(Value, RecordNumber, true);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FileWidth(int FileNumber, int RecordWidth)
        {
            GetStream(Assembly.GetCallingAssembly(), FileNumber).SetWidth(RecordWidth);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int FreeFile()
        {
            Assembly callingAssembly = Assembly.GetCallingAssembly();
            AssemblyData assemblyData = ProjectData.GetProjectData().GetAssemblyData(callingAssembly);
            int lChannel = 1;
            while (assemblyData.GetChannelObj(lChannel) != null)
            {
                lChannel++;
                if (lChannel > 0xff)
                {
                    throw ExceptionUtils.VbMakeException(0x43);
                }
            }
            return lChannel;
        }

        public static FileAttribute GetAttr(string PathName)
        {
            char[] anyOf = new char[] { '*', '?' };
            if (PathName.IndexOfAny(anyOf) >= 0)
            {
                throw ExceptionUtils.VbMakeException(0x34);
            }
            FileInfo info = new FileInfo(PathName);
            if (info.Exists)
            {
                return (((FileAttribute) info.Attributes) & (FileAttribute.Archive | FileAttribute.Directory | FileAttribute.Volume | FileAttribute.System | FileAttribute.Hidden | FileAttribute.ReadOnly));
            }
            DirectoryInfo info2 = new DirectoryInfo(PathName);
            if (info2.Exists)
            {
                return (((FileAttribute) info2.Attributes) & (FileAttribute.Archive | FileAttribute.Directory | FileAttribute.Volume | FileAttribute.System | FileAttribute.Hidden | FileAttribute.ReadOnly));
            }
            if (Path.GetFileName(PathName).Length == 0)
            {
                throw ExceptionUtils.VbMakeException(0x34);
            }
            throw new FileNotFoundException(Utils.GetResourceString("FileSystem_FileNotFound1", new string[] { PathName }));
        }

        internal static VB6File GetChannelObj(Assembly assem, int FileNumber)
        {
            VB6File channelOrNull = GetChannelOrNull(ProjectData.GetProjectData().GetAssemblyData(assem), FileNumber);
            if (channelOrNull == null)
            {
                throw ExceptionUtils.VbMakeException(0x34);
            }
            return channelOrNull;
        }

        private static VB6File GetChannelOrNull(AssemblyData oAssemblyData, int FileNumber)
        {
            return oAssemblyData.GetChannelObj(FileNumber);
        }

        private static VB6File GetStream(Assembly assem, int FileNumber)
        {
            return GetStream(assem, FileNumber, OpenModeTypes.Binary | OpenModeTypes.Append | OpenModeTypes.Random | OpenModeTypes.Output | OpenModeTypes.Input);
        }

        private static VB6File GetStream(Assembly assem, int FileNumber, OpenModeTypes mode)
        {
            if ((FileNumber < 1) || (FileNumber > 0xff))
            {
                throw ExceptionUtils.VbMakeException(0x34);
            }
            VB6File channelObj = GetChannelObj(assem, FileNumber);
            if ((OpenModeTypesFromOpenMode(channelObj.GetMode()) | mode) == ~OpenModeTypes.Any)
            {
                channelObj = null;
                throw ExceptionUtils.VbMakeException(0x36);
            }
            return channelObj;
        }

        private static DateTimeFormatInfo InitializeWriteDateFormatInfo()
        {
            return new DateTimeFormatInfo { DateSeparator = "-", ShortDatePattern = @"\#yyyy-MM-dd\#", LongTimePattern = @"\#HH:mm:ss\#", FullDateTimePattern = @"\#yyyy-MM-dd HH:mm:ss\#" };
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Input(int FileNumber, ref bool Value)
        {
            try
            {
                GetStream(Assembly.GetCallingAssembly(), FileNumber).Input(ref Value);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Input(int FileNumber, ref byte Value)
        {
            try
            {
                GetStream(Assembly.GetCallingAssembly(), FileNumber).Input(ref Value);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Input(int FileNumber, ref char Value)
        {
            try
            {
                GetStream(Assembly.GetCallingAssembly(), FileNumber).Input(ref Value);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Input(int FileNumber, ref DateTime Value)
        {
            try
            {
                GetStream(Assembly.GetCallingAssembly(), FileNumber).Input(ref Value);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Input(int FileNumber, ref decimal Value)
        {
            try
            {
                GetStream(Assembly.GetCallingAssembly(), FileNumber).Input(ref Value);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Input(int FileNumber, ref double Value)
        {
            try
            {
                GetStream(Assembly.GetCallingAssembly(), FileNumber).Input(ref Value);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Input(int FileNumber, ref short Value)
        {
            try
            {
                GetStream(Assembly.GetCallingAssembly(), FileNumber).Input(ref Value);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Input(int FileNumber, ref int Value)
        {
            try
            {
                GetStream(Assembly.GetCallingAssembly(), FileNumber).Input(ref Value);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Input(int FileNumber, ref long Value)
        {
            try
            {
                GetStream(Assembly.GetCallingAssembly(), FileNumber).Input(ref Value);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Input(int FileNumber, ref object Value)
        {
            try
            {
                GetStream(Assembly.GetCallingAssembly(), FileNumber).Input(ref Value);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Input(int FileNumber, ref float Value)
        {
            try
            {
                GetStream(Assembly.GetCallingAssembly(), FileNumber).Input(ref Value);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Input(int FileNumber, ref string Value)
        {
            try
            {
                GetStream(Assembly.GetCallingAssembly(), FileNumber).Input(ref Value);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string InputString(int FileNumber, int CharCount)
        {
            string str;
            try
            {
                if ((CharCount < 0) || (CharCount > 1073741823.5))
                {
                    throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "CharCount" }));
                }
                VB6File channelObj = GetChannelObj(Assembly.GetCallingAssembly(), FileNumber);
                channelObj.Lock();
                try
                {
                    return channelObj.InputString(CharCount);
                }
                finally
                {
                    channelObj.Unlock();
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return str;
        }

        private static void InternalCloseFile(AssemblyData oAssemblyData, int FileNumber)
        {
            if (FileNumber == 0)
            {
                CloseAllFiles(oAssemblyData);
            }
            else
            {
                VB6File channelOrNull = GetChannelOrNull(oAssemblyData, FileNumber);
                if (channelOrNull != null)
                {
                    oAssemblyData.SetChannelObj(FileNumber, null);
                    if (channelOrNull != null)
                    {
                        channelOrNull.CloseFile();
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Kill(string PathName)
        {
            int num;
            string fileName;
            string directoryName = Path.GetDirectoryName(PathName);
            if ((directoryName == null) || (directoryName.Length == 0))
            {
                directoryName = Environment.CurrentDirectory;
                fileName = PathName;
            }
            else
            {
                fileName = Path.GetFileName(PathName);
            }
            FileInfo[] files = new DirectoryInfo(directoryName).GetFiles(fileName);
            directoryName = directoryName + Conversions.ToString(Path.PathSeparator);
            if (files != null)
            {
                int upperBound = files.GetUpperBound(0);
                for (int i = 0; i <= upperBound; i++)
                {
                    FileInfo info2 = files[i];
                    if ((info2.Attributes & (FileAttributes.System | FileAttributes.Hidden)) == 0)
                    {
                        fileName = info2.FullName;
                        if (CheckFileOpen(ProjectData.GetProjectData().GetAssemblyData(Assembly.GetCallingAssembly()), fileName, OpenModeTypes.Any))
                        {
                            throw ExceptionUtils.VbMakeException(new IOException(Utils.GetResourceString("FileSystem_FileAlreadyOpen1", new string[] { fileName })), 0x37);
                        }
                        try
                        {
                            File.Delete(fileName);
                            num++;
                        }
                        catch (IOException exception)
                        {
                            throw ExceptionUtils.VbMakeException(exception, 0x37);
                        }
                        catch (Exception exception2)
                        {
                            throw exception2;
                        }
                    }
                }
            }
            if (num == 0)
            {
                throw new FileNotFoundException(Utils.GetResourceString("KILL_NoFilesFound1", new string[] { PathName }));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string LineInput(int FileNumber)
        {
            VB6File stream = GetStream(Assembly.GetCallingAssembly(), FileNumber);
            CheckInputCapable(stream);
            if (stream.EOF())
            {
                throw ExceptionUtils.VbMakeException(0x3e);
            }
            return stream.LineInput();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static long Loc(int FileNumber)
        {
            return GetStream(Assembly.GetCallingAssembly(), FileNumber).LOC();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Lock(int FileNumber)
        {
            GetStream(Assembly.GetCallingAssembly(), FileNumber).Lock();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Lock(int FileNumber, long Record)
        {
            GetStream(Assembly.GetCallingAssembly(), FileNumber).Lock(Record);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Lock(int FileNumber, long FromRecord, long ToRecord)
        {
            GetStream(Assembly.GetCallingAssembly(), FileNumber).Lock(FromRecord, ToRecord);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static long LOF(int FileNumber)
        {
            return GetStream(Assembly.GetCallingAssembly(), FileNumber).LOF();
        }

        public static void MkDir(string Path)
        {
            if ((Path == null) || (Path.Length == 0))
            {
                throw ExceptionUtils.VbMakeException(new ArgumentException(Utils.GetResourceString("Argument_PathNullOrEmpty")), 0x34);
            }
            if (Directory.Exists(Path))
            {
                throw ExceptionUtils.VbMakeException(0x4b);
            }
            Directory.CreateDirectory(Path);
        }

        private static OpenModeTypes OpenModeTypesFromOpenMode(OpenMode om)
        {
            if (om == OpenMode.Input)
            {
                return OpenModeTypes.Input;
            }
            if (om == OpenMode.Output)
            {
                return OpenModeTypes.Output;
            }
            if (om == OpenMode.Append)
            {
                return OpenModeTypes.Append;
            }
            if (om == OpenMode.Binary)
            {
                return OpenModeTypes.Binary;
            }
            if (om == OpenMode.Random)
            {
                return OpenModeTypes.Random;
            }
            if (om != ((OpenMode) (-1)))
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue"), "om");
            }
            return OpenModeTypes.Any;
        }

        private static bool PathContainsWildcards(string Path)
        {
            if (Path == null)
            {
                return false;
            }
            return ((Path.IndexOf('*') != -1) || (Path.IndexOf('?') != -1));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Print(int FileNumber, params object[] Output)
        {
            try
            {
                GetStream(Assembly.GetCallingAssembly(), FileNumber).Print(Output);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void PrintLine(int FileNumber, params object[] Output)
        {
            try
            {
                GetStream(Assembly.GetCallingAssembly(), FileNumber).PrintLine(Output);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Rename(string OldPath, string NewPath)
        {
            AssemblyData assemblyData = ProjectData.GetProjectData().GetAssemblyData(Assembly.GetCallingAssembly());
            OldPath = VB6CheckPathname(assemblyData, OldPath, (OpenMode) (-1));
            NewPath = VB6CheckPathname(assemblyData, NewPath, (OpenMode) (-1));
            new FileIOPermission(FileIOPermissionAccess.Write | FileIOPermissionAccess.Read, OldPath).Demand();
            new FileIOPermission(FileIOPermissionAccess.Write, NewPath).Demand();
            if (Microsoft.VisualBasic.CompilerServices.UnsafeNativeMethods.MoveFile(OldPath, NewPath) == 0)
            {
                switch (Marshal.GetLastWin32Error())
                {
                    case 2:
                        throw ExceptionUtils.VbMakeException(0x35);

                    case 80:
                    case 0xb7:
                        throw ExceptionUtils.VbMakeException(0x3a);

                    case 12:
                        throw ExceptionUtils.VbMakeException(0x4b);

                    case 0x11:
                        throw ExceptionUtils.VbMakeException(0x4a);
                }
                throw ExceptionUtils.VbMakeException(5);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Reset()
        {
            CloseAllFiles(Assembly.GetCallingAssembly());
        }

        public static void RmDir(string Path)
        {
            if ((Path == null) || (Path.Length == 0))
            {
                throw ExceptionUtils.VbMakeException(new ArgumentException(Utils.GetResourceString("Argument_PathNullOrEmpty")), 0x34);
            }
            try
            {
                Directory.Delete(Path);
            }
            catch (DirectoryNotFoundException exception)
            {
                throw ExceptionUtils.VbMakeException(exception, 0x4c);
            }
            catch (StackOverflowException exception2)
            {
                throw exception2;
            }
            catch (OutOfMemoryException exception3)
            {
                throw exception3;
            }
            catch (ThreadAbortException exception4)
            {
                throw exception4;
            }
            catch (Exception exception5)
            {
                throw ExceptionUtils.VbMakeException(exception5, 0x4b);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static long Seek(int FileNumber)
        {
            return GetStream(Assembly.GetCallingAssembly(), FileNumber).Seek();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Seek(int FileNumber, long Position)
        {
            GetStream(Assembly.GetCallingAssembly(), FileNumber).Seek(Position);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void SetAttr(string PathName, FileAttribute Attributes)
        {
            if ((PathName == null) || (PathName.Length == 0))
            {
                throw ExceptionUtils.VbMakeException(new ArgumentException(Utils.GetResourceString("Argument_PathNullOrEmpty")), 0x34);
            }
            Assembly callingAssembly = Assembly.GetCallingAssembly();
            VB6CheckPathname(ProjectData.GetProjectData().GetAssemblyData(callingAssembly), PathName, OpenMode.Input);
            if ((Attributes | (FileAttribute.Archive | FileAttribute.System | FileAttribute.Hidden | FileAttribute.ReadOnly)) != (FileAttribute.Archive | FileAttribute.System | FileAttribute.Hidden | FileAttribute.ReadOnly))
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Attributes" }));
            }
            FileAttributes fileAttributes = (FileAttributes) Attributes;
            File.SetAttributes(PathName, fileAttributes);
        }

        public static SpcInfo SPC(short Count)
        {
            SpcInfo info;
            if (Count < 1)
            {
                Count = 0;
            }
            info.Count = Count;
            return info;
        }

        public static TabInfo TAB()
        {
            TabInfo info;
            info.Column = -1;
            return info;
        }

        public static TabInfo TAB(short Column)
        {
            TabInfo info;
            if (Column < 1)
            {
                Column = 1;
            }
            info.Column = Column;
            return info;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Unlock(int FileNumber)
        {
            GetStream(Assembly.GetCallingAssembly(), FileNumber).Unlock();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Unlock(int FileNumber, long Record)
        {
            GetStream(Assembly.GetCallingAssembly(), FileNumber).Unlock(Record);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Unlock(int FileNumber, long FromRecord, long ToRecord)
        {
            GetStream(Assembly.GetCallingAssembly(), FileNumber).Unlock(FromRecord, ToRecord);
        }

        private static bool UnsafeValidDrive(char cDrive)
        {
            int num = cDrive - 'A';
            return ((Microsoft.VisualBasic.CompilerServices.UnsafeNativeMethods.GetLogicalDrives() & ((long) Math.Round(Math.Pow(2.0, (double) num)))) != 0L);
        }

        private static void ValidateAccess(OpenAccess Access)
        {
            if (((Access != OpenAccess.Default) && (Access != OpenAccess.Read)) && ((Access != OpenAccess.ReadWrite) && (Access != OpenAccess.Write)))
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Access" }));
            }
        }

        private static void ValidateGetPutRecordNumber(long RecordNumber)
        {
            if ((RecordNumber < 1L) && (RecordNumber != -1L))
            {
                throw ExceptionUtils.VbMakeException(new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "RecordNumber" })), 0x3f);
            }
        }

        private static void ValidateMode(OpenMode Mode)
        {
            if ((((Mode != OpenMode.Input) && (Mode != OpenMode.Output)) && ((Mode != OpenMode.Random) && (Mode != OpenMode.Append))) && (Mode != OpenMode.Binary))
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Mode" }));
            }
        }

        private static void ValidateShare(OpenShare Share)
        {
            if ((((Share != OpenShare.Default) && (Share != OpenShare.Shared)) && ((Share != OpenShare.LockRead) && (Share != OpenShare.LockReadWrite))) && (Share != OpenShare.LockWrite))
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Share" }));
            }
        }

        internal static string VB6CheckPathname(AssemblyData oAssemblyData, string sPath, OpenMode mode)
        {
            if ((sPath.IndexOf('?') != -1) || (sPath.IndexOf('*') != -1))
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidPathChars1", new string[] { sPath }));
            }
            string fullName = new FileInfo(sPath).FullName;
            if (CheckFileOpen(oAssemblyData, fullName, OpenModeTypesFromOpenMode(mode)))
            {
                throw ExceptionUtils.VbMakeException(0x37);
            }
            return fullName;
        }

        private static void vbIOOpenFile(Assembly assem, int FileNumber, string FileName, OpenMode Mode, OpenAccess Access, OpenShare Share, int RecordLength)
        {
            VB6File file;
            AssemblyData assemblyData = ProjectData.GetProjectData().GetAssemblyData(assem);
            if (GetChannelOrNull(assemblyData, FileNumber) != null)
            {
                throw ExceptionUtils.VbMakeException(0x37);
            }
            if ((FileName == null) || (FileName.Length == 0))
            {
                throw ExceptionUtils.VbMakeException(0x4b);
            }
            FileName = new FileInfo(FileName).FullName;
            if (CheckFileOpen(assemblyData, FileName, OpenModeTypesFromOpenMode(Mode)))
            {
                throw ExceptionUtils.VbMakeException(0x37);
            }
            if ((RecordLength != -1) && (RecordLength <= 0))
            {
                throw ExceptionUtils.VbMakeException(5);
            }
            if (Mode == OpenMode.Binary)
            {
                RecordLength = 1;
            }
            else if (RecordLength == -1)
            {
                if (Mode == OpenMode.Random)
                {
                    RecordLength = 0x80;
                }
                else
                {
                    RecordLength = 0x200;
                }
            }
            if (Share == OpenShare.Default)
            {
                Share = OpenShare.LockReadWrite;
            }
            OpenMode mode = Mode;
            switch (mode)
            {
                case OpenMode.Input:
                    if ((Access != OpenAccess.Read) && (Access != OpenAccess.Default))
                    {
                        throw new ArgumentException(Utils.GetResourceString("FileSystem_IllegalInputAccess"));
                    }
                    file = new VB6InputFile(FileName, Share);
                    break;

                case OpenMode.Output:
                    if ((Access != OpenAccess.Write) && (Access != OpenAccess.Default))
                    {
                        throw new ArgumentException(Utils.GetResourceString("FileSystem_IllegalOutputAccess"));
                    }
                    file = new VB6OutputFile(FileName, Share, false);
                    break;

                case OpenMode.Random:
                    if (Access == OpenAccess.Default)
                    {
                        Access = OpenAccess.ReadWrite;
                    }
                    file = new VB6RandomFile(FileName, Access, Share, RecordLength);
                    break;

                case OpenMode.Append:
                    if (((Access != OpenAccess.Write) && (Access != OpenAccess.ReadWrite)) && (Access != OpenAccess.Default))
                    {
                        throw new ArgumentException(Utils.GetResourceString("FileSystem_IllegalAppendAccess"));
                    }
                    file = new VB6OutputFile(FileName, Share, true);
                    break;

                default:
                    if (mode != OpenMode.Binary)
                    {
                        throw ExceptionUtils.VbMakeException(0x33);
                    }
                    if (Access == OpenAccess.Default)
                    {
                        Access = OpenAccess.ReadWrite;
                    }
                    file = new VB6BinaryFile(FileName, Access, Share);
                    break;
            }
            AddFileToList(assemblyData, FileNumber, file);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Write(int FileNumber, params object[] Output)
        {
            try
            {
                GetStream(Assembly.GetCallingAssembly(), FileNumber).WriteHelper(Output);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteLine(int FileNumber, params object[] Output)
        {
            try
            {
                GetStream(Assembly.GetCallingAssembly(), FileNumber).WriteLineHelper(Output);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        internal enum vbFileType
        {
            vbPrintFile,
            vbWriteFile
        }
    }
}

