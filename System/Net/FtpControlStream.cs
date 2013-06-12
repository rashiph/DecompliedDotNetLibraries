namespace System.Net
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Net.Cache;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;

    internal class FtpControlStream : CommandStream
    {
        private static readonly AsyncCallback m_AcceptCallbackDelegate = new AsyncCallback(FtpControlStream.AcceptCallback);
        private StringBuilder m_BannerMessage;
        private static readonly AsyncCallback m_ConnectCallbackDelegate = new AsyncCallback(FtpControlStream.ConnectCallback);
        private long m_ContentLength;
        private WeakReference m_Credentials;
        private string m_CurrentTypeSetting;
        private bool m_DataHandshakeStarted;
        private Socket m_DataSocket;
        private string m_EstablishedServerDirectory;
        private StringBuilder m_ExitMessage;
        private DateTime m_LastModified;
        private string m_LoginDirectory;
        private FtpLoginState m_LoginState;
        private IPEndPoint m_PassiveEndPoint;
        private string m_RequestedServerDirectory;
        private Uri m_ResponseUri;
        private static readonly AsyncCallback m_SSLHandshakeCallback = new AsyncCallback(FtpControlStream.SSLHandshakeCallback);
        private TlsStream m_TlsStream;
        private StringBuilder m_WelcomeMessage;
        internal FtpStatusCode StatusCode;
        internal string StatusLine;

        internal FtpControlStream(ConnectionPool connectionPool, TimeSpan lifetime, bool checkLifetime) : base(connectionPool, lifetime, checkLifetime)
        {
            this.m_CurrentTypeSetting = string.Empty;
            this.m_ContentLength = -1L;
        }

        internal void AbortConnect()
        {
            Socket dataSocket = this.m_DataSocket;
            if (dataSocket != null)
            {
                try
                {
                    dataSocket.Close();
                }
                catch (ObjectDisposedException)
                {
                }
            }
        }

        private static void AcceptCallback(IAsyncResult asyncResult)
        {
            FtpControlStream asyncState = (FtpControlStream) asyncResult.AsyncState;
            LazyAsyncResult result = asyncResult as LazyAsyncResult;
            Socket asyncObject = (Socket) result.AsyncObject;
            try
            {
                asyncState.m_DataSocket = asyncObject.EndAccept(asyncResult);
                if (!asyncState.ServerAddress.Equals(((IPEndPoint) asyncState.m_DataSocket.RemoteEndPoint).Address))
                {
                    asyncState.m_DataSocket.Close();
                    throw new WebException(SR.GetString("net_ftp_active_address_different"), WebExceptionStatus.ProtocolError);
                }
                asyncState.ContinueCommandPipeline();
            }
            catch (Exception exception)
            {
                asyncState.CloseSocket();
                asyncState.InvokeRequestCallback(exception);
            }
            finally
            {
                asyncObject.Close();
            }
        }

        protected override CommandStream.PipelineEntry[] BuildCommandsList(WebRequest req)
        {
            string str3;
            string str4;
            string str5;
            bool flag = false;
            FtpWebRequest request = (FtpWebRequest) req;
            this.m_ResponseUri = request.RequestUri;
            ArrayList list = new ArrayList();
            if (request.EnableSsl && !base.UsingSecureStream)
            {
                list.Add(new CommandStream.PipelineEntry(this.FormatFtpCommand("AUTH", "TLS")));
                flag = true;
            }
            if (flag)
            {
                this.m_LoginDirectory = null;
                this.m_EstablishedServerDirectory = null;
                this.m_RequestedServerDirectory = null;
                this.m_CurrentTypeSetting = string.Empty;
                if (this.m_LoginState == FtpLoginState.LoggedIn)
                {
                    this.m_LoginState = FtpLoginState.LoggedInButNeedsRelogin;
                }
            }
            if (this.m_LoginState != FtpLoginState.LoggedIn)
            {
                this.Credentials = request.Credentials.GetCredential(request.RequestUri, "basic");
                this.m_WelcomeMessage = new StringBuilder();
                this.m_ExitMessage = new StringBuilder();
                string parameter = string.Empty;
                string password = string.Empty;
                if (this.Credentials != null)
                {
                    parameter = this.Credentials.InternalGetDomainUserName();
                    password = this.Credentials.InternalGetPassword();
                }
                if ((parameter.Length == 0) && (password.Length == 0))
                {
                    parameter = "anonymous";
                    password = "anonymous@";
                }
                list.Add(new CommandStream.PipelineEntry(this.FormatFtpCommand("USER", parameter)));
                list.Add(new CommandStream.PipelineEntry(this.FormatFtpCommand("PASS", password), CommandStream.PipelineEntryFlags.DontLogParameter));
                if (request.EnableSsl && !base.UsingSecureStream)
                {
                    list.Add(new CommandStream.PipelineEntry(this.FormatFtpCommand("PBSZ", "0")));
                    list.Add(new CommandStream.PipelineEntry(this.FormatFtpCommand("PROT", "P")));
                }
                list.Add(new CommandStream.PipelineEntry(this.FormatFtpCommand("OPTS", "utf8 on")));
                list.Add(new CommandStream.PipelineEntry(this.FormatFtpCommand("PWD", null)));
            }
            GetPathOption normal = GetPathOption.Normal;
            if (request.MethodInfo.HasFlag(FtpMethodFlags.DoesNotTakeParameter))
            {
                normal = GetPathOption.AssumeNoFilename;
            }
            else if (request.MethodInfo.HasFlag(FtpMethodFlags.ParameterIsDirectory))
            {
                normal = GetPathOption.AssumeFilename;
            }
            GetPathInfo(normal, request.RequestUri, out str3, out str4, out str5);
            if ((str5.Length == 0) && request.MethodInfo.HasFlag(FtpMethodFlags.TakesParameter))
            {
                throw new WebException(SR.GetString("net_ftp_invalid_uri"));
            }
            if (((this.m_EstablishedServerDirectory != null) && (this.m_LoginDirectory != null)) && (this.m_EstablishedServerDirectory != this.m_LoginDirectory))
            {
                list.Add(new CommandStream.PipelineEntry(this.FormatFtpCommand("CWD", this.m_LoginDirectory), CommandStream.PipelineEntryFlags.UserCommand));
                this.m_RequestedServerDirectory = this.m_LoginDirectory;
            }
            if (request.MethodInfo.HasFlag(FtpMethodFlags.MustChangeWorkingDirectoryToPath) && (str4.Length > 0))
            {
                list.Add(new CommandStream.PipelineEntry(this.FormatFtpCommand("CWD", str4), CommandStream.PipelineEntryFlags.UserCommand));
                this.m_RequestedServerDirectory = str4;
            }
            if (((request.CacheProtocol != null) && (request.CacheProtocol.ProtocolStatus == CacheValidationStatus.DoNotTakeFromCache)) && (request.MethodInfo.Operation == FtpOperation.DownloadFile))
            {
                list.Add(new CommandStream.PipelineEntry(this.FormatFtpCommand("MDTM", str3)));
            }
            if (!request.MethodInfo.IsCommandOnly)
            {
                if ((request.CacheProtocol == null) || (request.CacheProtocol.ProtocolStatus != CacheValidationStatus.Continue))
                {
                    string str6 = request.UseBinary ? "I" : "A";
                    if (this.m_CurrentTypeSetting != str6)
                    {
                        list.Add(new CommandStream.PipelineEntry(this.FormatFtpCommand("TYPE", str6)));
                        this.m_CurrentTypeSetting = str6;
                    }
                    if (request.UsePassive)
                    {
                        string command = (base.ServerAddress.AddressFamily == AddressFamily.InterNetwork) ? "PASV" : "EPSV";
                        list.Add(new CommandStream.PipelineEntry(this.FormatFtpCommand(command, null), CommandStream.PipelineEntryFlags.CreateDataConnection));
                    }
                    else
                    {
                        string str8 = (base.ServerAddress.AddressFamily == AddressFamily.InterNetwork) ? "PORT" : "EPRT";
                        this.CreateFtpListenerSocket(request);
                        list.Add(new CommandStream.PipelineEntry(this.FormatFtpCommand(str8, this.GetPortCommandLine(request))));
                    }
                    if ((request.CacheProtocol != null) && (request.CacheProtocol.ProtocolStatus == CacheValidationStatus.CombineCachedAndServerResponse))
                    {
                        if (request.CacheProtocol.Validator.CacheEntry.StreamSize > 0L)
                        {
                            list.Add(new CommandStream.PipelineEntry(this.FormatFtpCommand("REST", request.CacheProtocol.Validator.CacheEntry.StreamSize.ToString(CultureInfo.InvariantCulture))));
                        }
                    }
                    else if (request.ContentOffset > 0L)
                    {
                        list.Add(new CommandStream.PipelineEntry(this.FormatFtpCommand("REST", request.ContentOffset.ToString(CultureInfo.InvariantCulture))));
                    }
                }
                else
                {
                    list.Add(new CommandStream.PipelineEntry(this.FormatFtpCommand("SIZE", str3)));
                    list.Add(new CommandStream.PipelineEntry(this.FormatFtpCommand("MDTM", str3)));
                }
            }
            if ((request.CacheProtocol == null) || (request.CacheProtocol.ProtocolStatus != CacheValidationStatus.Continue))
            {
                CommandStream.PipelineEntryFlags userCommand = CommandStream.PipelineEntryFlags.UserCommand;
                if (!request.MethodInfo.IsCommandOnly)
                {
                    userCommand |= CommandStream.PipelineEntryFlags.GiveDataStream;
                    if (!request.UsePassive)
                    {
                        userCommand |= CommandStream.PipelineEntryFlags.CreateDataConnection;
                    }
                }
                if (request.MethodInfo.Operation == FtpOperation.Rename)
                {
                    list.Add(new CommandStream.PipelineEntry(this.FormatFtpCommand("RNFR", str4 + "/" + str5), userCommand));
                    list.Add(new CommandStream.PipelineEntry(this.FormatFtpCommand("RNTO", str4 + "/" + request.RenameTo), userCommand));
                }
                else if (request.MethodInfo.HasFlag(FtpMethodFlags.DoesNotTakeParameter))
                {
                    list.Add(new CommandStream.PipelineEntry(this.FormatFtpCommand(request.Method, string.Empty), userCommand));
                }
                else if (request.MethodInfo.HasFlag(FtpMethodFlags.MustChangeWorkingDirectoryToPath))
                {
                    list.Add(new CommandStream.PipelineEntry(this.FormatFtpCommand(request.Method, str5), userCommand));
                }
                else
                {
                    list.Add(new CommandStream.PipelineEntry(this.FormatFtpCommand(request.Method, str3), userCommand));
                }
                if (!request.KeepAlive)
                {
                    list.Add(new CommandStream.PipelineEntry(this.FormatFtpCommand("QUIT", null)));
                }
            }
            return (CommandStream.PipelineEntry[]) list.ToArray(typeof(CommandStream.PipelineEntry));
        }

        protected override bool CheckValid(ResponseDescription response, ref int validThrough, ref int completeLength)
        {
            if (response.StatusBuffer.Length >= 4)
            {
                string str = response.StatusBuffer.ToString();
                if (response.Status == -1)
                {
                    if ((!char.IsDigit(str[0]) || !char.IsDigit(str[1])) || (!char.IsDigit(str[2]) || ((str[3] != ' ') && (str[3] != '-'))))
                    {
                        return false;
                    }
                    response.StatusCodeString = str.Substring(0, 3);
                    response.Status = Convert.ToInt16(response.StatusCodeString, NumberFormatInfo.InvariantInfo);
                    if (str[3] == '-')
                    {
                        response.Multiline = true;
                    }
                }
                int num = 0;
                while ((num = str.IndexOf("\r\n", validThrough)) != -1)
                {
                    int startIndex = validThrough;
                    validThrough = num + 2;
                    if (!response.Multiline)
                    {
                        completeLength = validThrough;
                        return true;
                    }
                    if (((str.Length > (startIndex + 4)) && (str.Substring(startIndex, 3) == response.StatusCodeString)) && (str[startIndex + 3] == ' '))
                    {
                        completeLength = validThrough;
                        return true;
                    }
                }
            }
            return true;
        }

        protected override void ClearState()
        {
            this.m_ContentLength = -1L;
            this.m_LastModified = DateTime.MinValue;
            this.m_ResponseUri = null;
            this.m_DataHandshakeStarted = false;
            this.StatusCode = FtpStatusCode.Undefined;
            this.StatusLine = null;
            this.m_DataSocket = null;
            this.m_PassiveEndPoint = null;
            this.m_TlsStream = null;
            base.ClearState();
        }

        private static void ConnectCallback(IAsyncResult asyncResult)
        {
            FtpControlStream asyncState = (FtpControlStream) asyncResult.AsyncState;
            try
            {
                LazyAsyncResult result = asyncResult as LazyAsyncResult;
                ((Socket) result.AsyncObject).EndConnect(asyncResult);
                asyncState.ContinueCommandPipeline();
            }
            catch (Exception exception)
            {
                asyncState.CloseSocket();
                asyncState.InvokeRequestCallback(exception);
            }
        }

        protected Socket CreateFtpDataSocket(FtpWebRequest request, Socket templateSocket)
        {
            return new Socket(templateSocket.AddressFamily, templateSocket.SocketType, templateSocket.ProtocolType);
        }

        private void CreateFtpListenerSocket(FtpWebRequest request)
        {
            IPEndPoint localEP = new IPEndPoint(((IPEndPoint) base.Socket.LocalEndPoint).Address, 0);
            try
            {
                this.m_DataSocket = this.CreateFtpDataSocket(request, base.Socket);
            }
            catch (ObjectDisposedException)
            {
                throw ExceptionHelper.RequestAbortedException;
            }
            new SocketPermission(PermissionState.Unrestricted).Assert();
            try
            {
                this.m_DataSocket.Bind(localEP);
                this.m_DataSocket.Listen(1);
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
        }

        private string FormatAddress(IPAddress address, int Port)
        {
            byte[] addressBytes = address.GetAddressBytes();
            StringBuilder builder = new StringBuilder(0x20);
            foreach (byte num in addressBytes)
            {
                builder.Append(num);
                builder.Append(',');
            }
            builder.Append((int) (Port / 0x100));
            builder.Append(',');
            builder.Append((int) (Port % 0x100));
            return builder.ToString();
        }

        private string FormatAddressV6(IPAddress address, int port)
        {
            StringBuilder builder = new StringBuilder(0x2b);
            string str = address.ToString();
            builder.Append("|2|");
            builder.Append(str);
            builder.Append('|');
            builder.Append(port.ToString(NumberFormatInfo.InvariantInfo));
            builder.Append('|');
            return builder.ToString();
        }

        private string FormatFtpCommand(string command, string parameter)
        {
            StringBuilder builder = new StringBuilder((command.Length + ((parameter != null) ? parameter.Length : 0)) + 3);
            builder.Append(command);
            if (!ValidationHelper.IsBlankString(parameter))
            {
                builder.Append(' ');
                builder.Append(parameter);
            }
            builder.Append("\r\n");
            return builder.ToString();
        }

        private long GetContentLengthFrom213Response(string responseString)
        {
            string[] strArray = responseString.Split(new char[] { ' ' });
            if (strArray.Length < 2)
            {
                throw new FormatException(SR.GetString("net_ftp_response_invalid_format", new object[] { responseString }));
            }
            return Convert.ToInt64(strArray[1], NumberFormatInfo.InvariantInfo);
        }

        private DateTime GetLastModifiedFrom213Response(string str)
        {
            DateTime lastModified = this.m_LastModified;
            string[] strArray = str.Split(new char[] { ' ', '.' });
            if (strArray.Length >= 2)
            {
                string str2 = strArray[1];
                if (str2.Length < 14)
                {
                    return lastModified;
                }
                int year = Convert.ToInt32(str2.Substring(0, 4), NumberFormatInfo.InvariantInfo);
                int month = Convert.ToInt16(str2.Substring(4, 2), NumberFormatInfo.InvariantInfo);
                int day = Convert.ToInt16(str2.Substring(6, 2), NumberFormatInfo.InvariantInfo);
                int hour = Convert.ToInt16(str2.Substring(8, 2), NumberFormatInfo.InvariantInfo);
                int minute = Convert.ToInt16(str2.Substring(10, 2), NumberFormatInfo.InvariantInfo);
                int second = Convert.ToInt16(str2.Substring(12, 2), NumberFormatInfo.InvariantInfo);
                int millisecond = 0;
                if (strArray.Length > 2)
                {
                    millisecond = Convert.ToInt16(strArray[2], NumberFormatInfo.InvariantInfo);
                }
                try
                {
                    lastModified = new DateTime(year, month, day, hour, minute, second, millisecond);
                    lastModified = lastModified.ToLocalTime();
                }
                catch (ArgumentOutOfRangeException)
                {
                }
                catch (ArgumentException)
                {
                }
            }
            return lastModified;
        }

        private string GetLoginDirectory(string str)
        {
            int index = str.IndexOf('"');
            int num2 = str.LastIndexOf('"');
            if (((index != -1) && (num2 != -1)) && (index != num2))
            {
                return str.Substring(index + 1, (num2 - index) - 1);
            }
            return string.Empty;
        }

        private static void GetPathInfo(GetPathOption pathOption, Uri uri, out string path, out string directory, out string filename)
        {
            path = uri.GetComponents(UriComponents.Path, UriFormat.Unescaped);
            int num = path.LastIndexOf('/');
            if (((pathOption == GetPathOption.AssumeFilename) && (num != -1)) && (num == (path.Length - 1)))
            {
                path = path.Substring(0, path.Length - 1);
                num = path.LastIndexOf('/');
            }
            if (pathOption == GetPathOption.AssumeNoFilename)
            {
                directory = path;
                filename = string.Empty;
            }
            else
            {
                directory = path.Substring(0, num + 1);
                filename = path.Substring(num + 1, path.Length - (num + 1));
            }
            if ((directory.Length > 1) && (directory[directory.Length - 1] == '/'))
            {
                directory = directory.Substring(0, directory.Length - 1);
            }
        }

        private string GetPortCommandLine(FtpWebRequest request)
        {
            string str;
            try
            {
                IPEndPoint localEndPoint = (IPEndPoint) this.m_DataSocket.LocalEndPoint;
                if (base.ServerAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    return this.FormatAddress(localEndPoint.Address, localEndPoint.Port);
                }
                if (base.ServerAddress.AddressFamily != AddressFamily.InterNetworkV6)
                {
                    throw new InternalException();
                }
                str = this.FormatAddressV6(localEndPoint.Address, localEndPoint.Port);
            }
            catch (Exception exception)
            {
                throw base.GenerateException(WebExceptionStatus.ProtocolError, exception);
            }
            return str;
        }

        private int GetPortV4(string responseString)
        {
            string[] strArray = responseString.Split(new char[] { ' ', '(', ',', ')' });
            if (strArray.Length <= 7)
            {
                throw new FormatException(SR.GetString("net_ftp_response_invalid_format", new object[] { responseString }));
            }
            int index = strArray.Length - 1;
            if (!char.IsNumber(strArray[index], 0))
            {
                index--;
            }
            return (Convert.ToByte(strArray[index--], NumberFormatInfo.InvariantInfo) | (Convert.ToByte(strArray[index--], NumberFormatInfo.InvariantInfo) << 8));
        }

        private int GetPortV6(string responseString)
        {
            int num = responseString.LastIndexOf("(");
            int num2 = responseString.LastIndexOf(")");
            if ((num == -1) || (num2 <= num))
            {
                throw new FormatException(SR.GetString("net_ftp_response_invalid_format", new object[] { responseString }));
            }
            string[] strArray = responseString.Substring(num + 1, (num2 - num) - 1).Split(new char[] { '|' });
            if (strArray.Length < 4)
            {
                throw new FormatException(SR.GetString("net_ftp_response_invalid_format", new object[] { responseString }));
            }
            return Convert.ToInt32(strArray[3], NumberFormatInfo.InvariantInfo);
        }

        private System.Net.TriState IsFtpDataStreamWriteable()
        {
            FtpWebRequest request = base.m_Request as FtpWebRequest;
            if (request != null)
            {
                if (request.MethodInfo.IsUpload)
                {
                    return System.Net.TriState.True;
                }
                if (request.MethodInfo.IsDownload)
                {
                    return System.Net.TriState.False;
                }
            }
            return System.Net.TriState.Unspecified;
        }

        protected override CommandStream.PipelineInstruction PipelineCallback(CommandStream.PipelineEntry entry, ResponseDescription response, bool timeout, ref Stream stream)
        {
            if (response == null)
            {
                return CommandStream.PipelineInstruction.Abort;
            }
            FtpStatusCode status = (FtpStatusCode) response.Status;
            if (status != FtpStatusCode.ClosingControl)
            {
                this.StatusCode = status;
                this.StatusLine = response.StatusDescription;
            }
            if (response.InvalidStatusCode)
            {
                throw new WebException(SR.GetString("net_InvalidStatusCode"), WebExceptionStatus.ProtocolError);
            }
            if (base.m_Index == -1)
            {
                switch (status)
                {
                    case FtpStatusCode.SendUserCommand:
                        this.m_BannerMessage = new StringBuilder();
                        this.m_BannerMessage.Append(this.StatusLine);
                        return CommandStream.PipelineInstruction.Advance;

                    case FtpStatusCode.ServiceTemporarilyNotAvailable:
                        return CommandStream.PipelineInstruction.Reread;
                }
                throw base.GenerateException(status, response.StatusDescription, null);
            }
            if (entry.Command == "OPTS utf8 on\r\n")
            {
                if (response.PositiveCompletion)
                {
                    base.Encoding = Encoding.UTF8;
                }
                else
                {
                    base.Encoding = Encoding.Default;
                }
                return CommandStream.PipelineInstruction.Advance;
            }
            if (entry.Command.IndexOf("USER") != -1)
            {
                if (status == FtpStatusCode.LoggedInProceed)
                {
                    this.m_LoginState = FtpLoginState.LoggedIn;
                    base.m_Index++;
                }
                else if ((status == FtpStatusCode.NotLoggedIn) && (this.m_LoginState != FtpLoginState.NotLoggedIn))
                {
                    this.m_LoginState = FtpLoginState.ReloginFailed;
                    throw ExceptionHelper.IsolatedException;
                }
            }
            if (response.TransientFailure || response.PermanentFailure)
            {
                if (status == FtpStatusCode.ServiceNotAvailable)
                {
                    base.MarkAsRecoverableFailure();
                }
                throw base.GenerateException(status, response.StatusDescription, null);
            }
            if ((this.m_LoginState != FtpLoginState.LoggedIn) && (entry.Command.IndexOf("PASS") != -1))
            {
                switch (status)
                {
                    case FtpStatusCode.NeedLoginAccount:
                    case FtpStatusCode.LoggedInProceed:
                        this.m_LoginState = FtpLoginState.LoggedIn;
                        goto Label_017A;
                }
                throw base.GenerateException(status, response.StatusDescription, null);
            }
        Label_017A:
            if (entry.HasFlag(CommandStream.PipelineEntryFlags.CreateDataConnection) && (response.PositiveCompletion || response.PositiveIntermediate))
            {
                bool flag;
                CommandStream.PipelineInstruction instruction = this.QueueOrCreateDataConection(entry, response, timeout, ref stream, out flag);
                if (!flag)
                {
                    return instruction;
                }
            }
            switch (status)
            {
                case FtpStatusCode.OpeningData:
                case FtpStatusCode.DataAlreadyOpen:
                {
                    if (this.m_DataSocket == null)
                    {
                        return CommandStream.PipelineInstruction.Abort;
                    }
                    if (!entry.HasFlag(CommandStream.PipelineEntryFlags.GiveDataStream))
                    {
                        base.m_AbortReason = SR.GetString("net_ftp_invalid_status_response", new object[] { status, entry.Command });
                        return CommandStream.PipelineInstruction.Abort;
                    }
                    this.TryUpdateContentLength(response.StatusDescription);
                    FtpWebRequest request = (FtpWebRequest) base.m_Request;
                    if (request.MethodInfo.ShouldParseForResponseUri)
                    {
                        this.TryUpdateResponseUri(response.StatusDescription, request);
                    }
                    return this.QueueOrCreateFtpDataStream(ref stream);
                }
                case FtpStatusCode.LoggedInProceed:
                    this.m_WelcomeMessage.Append(this.StatusLine);
                    break;

                case FtpStatusCode.ClosingControl:
                    this.m_ExitMessage.Append(response.StatusDescription);
                    base.CloseSocket();
                    break;

                case FtpStatusCode.ServerWantsSecureSession:
                {
                    FtpWebRequest initiatingRequest = (FtpWebRequest) base.m_Request;
                    TlsStream stream2 = new TlsStream(initiatingRequest.RequestUri.Host, base.NetworkStream, initiatingRequest.ClientCertificates, base.Pool.ServicePoint, initiatingRequest, base.m_Async ? initiatingRequest.GetWritingContext().ContextCopy : null);
                    base.NetworkStream = stream2;
                    break;
                }
                case FtpStatusCode.FileStatus:
                {
                    FtpWebRequest request1 = (FtpWebRequest) base.m_Request;
                    if (entry.Command.StartsWith("SIZE "))
                    {
                        this.m_ContentLength = this.GetContentLengthFrom213Response(response.StatusDescription);
                    }
                    else if (entry.Command.StartsWith("MDTM "))
                    {
                        this.m_LastModified = this.GetLastModifiedFrom213Response(response.StatusDescription);
                    }
                    break;
                }
                default:
                    if (status == FtpStatusCode.PathnameCreated)
                    {
                        if ((entry.Command == "PWD\r\n") && !entry.HasFlag(CommandStream.PipelineEntryFlags.UserCommand))
                        {
                            this.m_LoginDirectory = this.GetLoginDirectory(response.StatusDescription);
                        }
                    }
                    else if (entry.Command.IndexOf("CWD") != -1)
                    {
                        this.m_EstablishedServerDirectory = this.m_RequestedServerDirectory;
                    }
                    break;
            }
            if (!response.PositiveIntermediate && (base.UsingSecureStream || !(entry.Command == "AUTH TLS\r\n")))
            {
                return CommandStream.PipelineInstruction.Advance;
            }
            return CommandStream.PipelineInstruction.Reread;
        }

        private CommandStream.PipelineInstruction QueueOrCreateDataConection(CommandStream.PipelineEntry entry, ResponseDescription response, bool timeout, ref Stream stream, out bool isSocketReady)
        {
            CommandStream.PipelineInstruction instruction2;
            isSocketReady = false;
            if (this.m_DataHandshakeStarted)
            {
                isSocketReady = true;
                return CommandStream.PipelineInstruction.Pause;
            }
            this.m_DataHandshakeStarted = true;
            bool flag = false;
            int port = -1;
            if ((entry.Command == "PASV\r\n") || (entry.Command == "EPSV\r\n"))
            {
                if (!response.PositiveCompletion)
                {
                    base.m_AbortReason = SR.GetString("net_ftp_server_failed_passive", new object[] { response.Status });
                    return CommandStream.PipelineInstruction.Abort;
                }
                if (entry.Command == "PASV\r\n")
                {
                    port = this.GetPortV4(response.StatusDescription);
                }
                else
                {
                    port = this.GetPortV6(response.StatusDescription);
                }
                flag = true;
            }
            new SocketPermission(PermissionState.Unrestricted).Assert();
            try
            {
                CommandStream.PipelineInstruction pause;
                if (flag)
                {
                    try
                    {
                        this.m_DataSocket = this.CreateFtpDataSocket((FtpWebRequest) base.m_Request, base.Socket);
                    }
                    catch (ObjectDisposedException)
                    {
                        throw ExceptionHelper.RequestAbortedException;
                    }
                    this.m_PassiveEndPoint = new IPEndPoint(base.ServerAddress, port);
                }
                if (this.m_PassiveEndPoint != null)
                {
                    IPEndPoint passiveEndPoint = this.m_PassiveEndPoint;
                    this.m_PassiveEndPoint = null;
                    if (base.m_Async)
                    {
                        this.m_DataSocket.BeginConnect(passiveEndPoint, m_ConnectCallbackDelegate, this);
                        pause = CommandStream.PipelineInstruction.Pause;
                    }
                    else
                    {
                        this.m_DataSocket.Connect(passiveEndPoint);
                        pause = CommandStream.PipelineInstruction.Advance;
                    }
                }
                else if (base.m_Async)
                {
                    this.m_DataSocket.BeginAccept(m_AcceptCallbackDelegate, this);
                    pause = CommandStream.PipelineInstruction.Pause;
                }
                else
                {
                    Socket dataSocket = this.m_DataSocket;
                    try
                    {
                        this.m_DataSocket = this.m_DataSocket.Accept();
                        if (!base.ServerAddress.Equals(((IPEndPoint) this.m_DataSocket.RemoteEndPoint).Address))
                        {
                            this.m_DataSocket.Close();
                            throw new WebException(SR.GetString("net_ftp_active_address_different"), WebExceptionStatus.ProtocolError);
                        }
                        isSocketReady = true;
                        pause = CommandStream.PipelineInstruction.Pause;
                    }
                    finally
                    {
                        dataSocket.Close();
                    }
                }
                instruction2 = pause;
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            return instruction2;
        }

        private CommandStream.PipelineInstruction QueueOrCreateFtpDataStream(ref Stream stream)
        {
            if (this.m_DataSocket == null)
            {
                throw new InternalException();
            }
            if (this.m_TlsStream != null)
            {
                stream = new FtpDataStream(this.m_TlsStream, (FtpWebRequest) base.m_Request, this.IsFtpDataStreamWriteable());
                this.m_TlsStream = null;
                return CommandStream.PipelineInstruction.GiveStream;
            }
            NetworkStream networkStream = new NetworkStream(this.m_DataSocket, true);
            if (base.UsingSecureStream)
            {
                FtpWebRequest initiatingRequest = (FtpWebRequest) base.m_Request;
                TlsStream stream3 = new TlsStream(initiatingRequest.RequestUri.Host, networkStream, initiatingRequest.ClientCertificates, base.Pool.ServicePoint, initiatingRequest, base.m_Async ? initiatingRequest.GetWritingContext().ContextCopy : null);
                networkStream = stream3;
                if (base.m_Async)
                {
                    this.m_TlsStream = stream3;
                    LazyAsyncResult result = new LazyAsyncResult(null, this, m_SSLHandshakeCallback);
                    stream3.ProcessAuthentication(result);
                    return CommandStream.PipelineInstruction.Pause;
                }
                stream3.ProcessAuthentication(null);
            }
            stream = new FtpDataStream(networkStream, (FtpWebRequest) base.m_Request, this.IsFtpDataStreamWriteable());
            return CommandStream.PipelineInstruction.GiveStream;
        }

        internal void Quit()
        {
            base.CloseSocket();
        }

        private static void SSLHandshakeCallback(IAsyncResult asyncResult)
        {
            FtpControlStream asyncState = (FtpControlStream) asyncResult.AsyncState;
            try
            {
                asyncState.ContinueCommandPipeline();
            }
            catch (Exception exception)
            {
                asyncState.CloseSocket();
                asyncState.InvokeRequestCallback(exception);
            }
        }

        private void TryUpdateContentLength(string str)
        {
            int startIndex = str.LastIndexOf("(");
            if (startIndex != -1)
            {
                int index = str.IndexOf(" bytes).");
                if ((index != -1) && (index > startIndex))
                {
                    long num3;
                    startIndex++;
                    if (long.TryParse(str.Substring(startIndex, index - startIndex), NumberStyles.AllowTrailingWhite | NumberStyles.AllowLeadingWhite, NumberFormatInfo.InvariantInfo, out num3))
                    {
                        this.m_ContentLength = num3;
                    }
                }
            }
        }

        private void TryUpdateResponseUri(string str, FtpWebRequest request)
        {
            Uri requestUri = request.RequestUri;
            int index = str.IndexOf("for ");
            if (index != -1)
            {
                index += 4;
                int length = str.LastIndexOf('(');
                if (length == -1)
                {
                    length = str.Length;
                }
                if (length > index)
                {
                    Uri uri2;
                    string str2 = str.Substring(index, length - index).TrimEnd(new char[] { ' ', '.', '\r', '\n' });
                    string relativeUri = str2.Replace("%", "%25").Replace("#", "%23");
                    string absolutePath = requestUri.AbsolutePath;
                    if ((absolutePath.Length > 0) && (absolutePath[absolutePath.Length - 1] != '/'))
                    {
                        UriBuilder builder = new UriBuilder(requestUri) {
                            Path = absolutePath + "/"
                        };
                        requestUri = builder.Uri;
                    }
                    if (!Uri.TryCreate(requestUri, relativeUri, out uri2))
                    {
                        throw new FormatException(SR.GetString("net_ftp_invalid_response_filename", new object[] { str2 }));
                    }
                    if (!requestUri.IsBaseOf(uri2) || (requestUri.Segments.Length != (uri2.Segments.Length - 1)))
                    {
                        throw new FormatException(SR.GetString("net_ftp_invalid_response_filename", new object[] { str2 }));
                    }
                    this.m_ResponseUri = uri2;
                }
            }
        }

        internal string BannerMessage
        {
            get
            {
                if (this.m_BannerMessage == null)
                {
                    return null;
                }
                return this.m_BannerMessage.ToString();
            }
        }

        internal long ContentLength
        {
            get
            {
                return this.m_ContentLength;
            }
        }

        internal NetworkCredential Credentials
        {
            get
            {
                if ((this.m_Credentials != null) && this.m_Credentials.IsAlive)
                {
                    return (NetworkCredential) this.m_Credentials.Target;
                }
                return null;
            }
            set
            {
                if (this.m_Credentials == null)
                {
                    this.m_Credentials = new WeakReference(null);
                }
                this.m_Credentials.Target = value;
            }
        }

        internal string ExitMessage
        {
            get
            {
                if (this.m_ExitMessage == null)
                {
                    return null;
                }
                return this.m_ExitMessage.ToString();
            }
        }

        internal DateTime LastModified
        {
            get
            {
                return this.m_LastModified;
            }
        }

        internal Uri ResponseUri
        {
            get
            {
                return this.m_ResponseUri;
            }
        }

        internal string WelcomeMessage
        {
            get
            {
                if (this.m_WelcomeMessage == null)
                {
                    return null;
                }
                return this.m_WelcomeMessage.ToString();
            }
        }

        private enum GetPathOption
        {
            Normal,
            AssumeFilename,
            AssumeNoFilename
        }
    }
}

