namespace System.Net
{
    using System;
    using System.Globalization;

    internal class FtpMethodInfo
    {
        internal FtpMethodFlags Flags;
        internal string HttpCommand;
        private static readonly FtpMethodInfo[] KnownMethodInfo = new FtpMethodInfo[] { new FtpMethodInfo("RETR", FtpOperation.DownloadFile, FtpMethodFlags.HasHttpCommand | FtpMethodFlags.TakesParameter | FtpMethodFlags.IsDownload, "GET"), new FtpMethodInfo("NLST", FtpOperation.ListDirectory, FtpMethodFlags.MustChangeWorkingDirectoryToPath | FtpMethodFlags.HasHttpCommand | FtpMethodFlags.MayTakeParameter | FtpMethodFlags.IsDownload, "GET"), new FtpMethodInfo("LIST", FtpOperation.ListDirectoryDetails, FtpMethodFlags.MustChangeWorkingDirectoryToPath | FtpMethodFlags.HasHttpCommand | FtpMethodFlags.MayTakeParameter | FtpMethodFlags.IsDownload, "GET"), new FtpMethodInfo("STOR", FtpOperation.UploadFile, FtpMethodFlags.TakesParameter | FtpMethodFlags.IsUpload, null), new FtpMethodInfo("STOU", FtpOperation.UploadFileUnique, FtpMethodFlags.MustChangeWorkingDirectoryToPath | FtpMethodFlags.ShouldParseForResponseUri | FtpMethodFlags.DoesNotTakeParameter | FtpMethodFlags.IsUpload, null), new FtpMethodInfo("APPE", FtpOperation.AppendFile, FtpMethodFlags.TakesParameter | FtpMethodFlags.IsUpload, null), new FtpMethodInfo("DELE", FtpOperation.DeleteFile, FtpMethodFlags.TakesParameter, null), new FtpMethodInfo("MDTM", FtpOperation.GetDateTimestamp, FtpMethodFlags.TakesParameter, null), new FtpMethodInfo("SIZE", FtpOperation.GetFileSize, FtpMethodFlags.TakesParameter, null), new FtpMethodInfo("RENAME", FtpOperation.Rename, FtpMethodFlags.TakesParameter, null), new FtpMethodInfo("MKD", FtpOperation.MakeDirectory, FtpMethodFlags.ParameterIsDirectory | FtpMethodFlags.TakesParameter, null), new FtpMethodInfo("RMD", FtpOperation.RemoveDirectory, FtpMethodFlags.ParameterIsDirectory | FtpMethodFlags.TakesParameter, null), new FtpMethodInfo("PWD", FtpOperation.PrintWorkingDirectory, FtpMethodFlags.DoesNotTakeParameter, null) };
        internal string Method;
        internal FtpOperation Operation;

        internal FtpMethodInfo(string method, FtpOperation operation, FtpMethodFlags flags, string httpCommand)
        {
            this.Method = method;
            this.Operation = operation;
            this.Flags = flags;
            this.HttpCommand = httpCommand;
        }

        internal static FtpMethodInfo GetMethodInfo(string method)
        {
            method = method.ToUpper(CultureInfo.InvariantCulture);
            foreach (FtpMethodInfo info in KnownMethodInfo)
            {
                if (method == info.Method)
                {
                    return info;
                }
            }
            throw new ArgumentException(SR.GetString("net_ftp_unsupported_method"), "method");
        }

        internal bool HasFlag(FtpMethodFlags flags)
        {
            return ((this.Flags & flags) != FtpMethodFlags.None);
        }

        internal bool HasHttpCommand
        {
            get
            {
                return ((this.Flags & FtpMethodFlags.HasHttpCommand) != FtpMethodFlags.None);
            }
        }

        internal bool IsCommandOnly
        {
            get
            {
                return ((this.Flags & (FtpMethodFlags.IsUpload | FtpMethodFlags.IsDownload)) == FtpMethodFlags.None);
            }
        }

        internal bool IsDownload
        {
            get
            {
                return ((this.Flags & FtpMethodFlags.IsDownload) != FtpMethodFlags.None);
            }
        }

        internal bool IsUpload
        {
            get
            {
                return ((this.Flags & FtpMethodFlags.IsUpload) != FtpMethodFlags.None);
            }
        }

        internal bool ShouldParseForResponseUri
        {
            get
            {
                return ((this.Flags & FtpMethodFlags.ShouldParseForResponseUri) != FtpMethodFlags.None);
            }
        }
    }
}

