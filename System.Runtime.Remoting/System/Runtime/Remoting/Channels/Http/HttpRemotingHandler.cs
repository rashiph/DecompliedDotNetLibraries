namespace System.Runtime.Remoting.Channels.Http
{
    using System;
    using System.IO;
    using System.Runtime;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;
    using System.Text;
    using System.Web;

    public class HttpRemotingHandler : IHttpHandler
    {
        private static string ApplicationConfigurationFile = "web.config";
        private static bool bLoadedConfiguration = false;
        private static Exception s_fatalException = null;
        private static HttpHandlerTransportSink s_transportSink = null;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public HttpRemotingHandler()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public HttpRemotingHandler(Type type, object srvID)
        {
        }

        private bool CanServiceRequest(HttpContext context)
        {
            string requestUriForCurrentRequest = this.GetRequestUriForCurrentRequest(context);
            string objectUriFromRequestUri = HttpChannelHelper.GetObjectUriFromRequestUri(requestUriForCurrentRequest);
            context.Items["__requestUri"] = requestUriForCurrentRequest;
            if (string.Compare(context.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase) != 0)
            {
                if (RemotingServices.GetServerTypeForUri(requestUriForCurrentRequest) != null)
                {
                    return true;
                }
            }
            else
            {
                if (context.Request.QueryString.Count != 1)
                {
                    return false;
                }
                string[] values = context.Request.QueryString.GetValues(0);
                if ((values.Length != 1) || (string.Compare(values[0], "wsdl", StringComparison.OrdinalIgnoreCase) != 0))
                {
                    return false;
                }
                if (string.Compare(objectUriFromRequestUri, "RemoteApplicationMetadata.rem", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }
                int length = requestUriForCurrentRequest.LastIndexOf('?');
                if (length != -1)
                {
                    requestUriForCurrentRequest = requestUriForCurrentRequest.Substring(0, length);
                }
                if (RemotingServices.GetServerTypeForUri(requestUriForCurrentRequest) != null)
                {
                    return true;
                }
            }
            return File.Exists(context.Request.PhysicalPath);
        }

        private string ComposeContentType(string contentType, Encoding encoding)
        {
            if (encoding != null)
            {
                StringBuilder builder = new StringBuilder(contentType);
                builder.Append("; charset=");
                builder.Append(encoding.WebName);
                return builder.ToString();
            }
            return contentType;
        }

        internal static bool CustomErrorsEnabled(HttpContext context)
        {
            try
            {
                if (!context.IsCustomErrorEnabled)
                {
                    return false;
                }
                return RemotingConfiguration.CustomErrorsEnabled(IsLocal(context));
            }
            catch
            {
                return true;
            }
        }

        private string GenerateFaultString(HttpContext context, Exception e)
        {
            if (!CustomErrorsEnabled(context))
            {
                return e.ToString();
            }
            return CoreChannel.GetResourceString("Remoting_InternalError");
        }

        private string GetRequestUriForCurrentRequest(HttpContext context)
        {
            string str3;
            string rawUrl = context.Request.RawUrl;
            if (HttpChannelHelper.ParseURL(rawUrl, out str3) == null)
            {
                str3 = rawUrl;
            }
            string applicationName = RemotingConfiguration.ApplicationName;
            if (((applicationName != null) && (applicationName.Length > 0)) && (str3.Length > applicationName.Length))
            {
                str3 = str3.Substring(applicationName.Length + 1);
            }
            return str3;
        }

        private void InternalProcessRequest(HttpContext context)
        {
            try
            {
                HttpRequest request = context.Request;
                if (!bLoadedConfiguration)
                {
                    lock (ApplicationConfigurationFile)
                    {
                        if (!bLoadedConfiguration)
                        {
                            IisHelper.Initialize();
                            if (RemotingConfiguration.ApplicationName == null)
                            {
                                RemotingConfiguration.ApplicationName = request.ApplicationPath;
                            }
                            string path = request.PhysicalApplicationPath + ApplicationConfigurationFile;
                            if (File.Exists(path))
                            {
                                try
                                {
                                    RemotingConfiguration.Configure(path, false);
                                }
                                catch (Exception exception)
                                {
                                    s_fatalException = exception;
                                    this.WriteException(context, exception);
                                    return;
                                }
                            }
                            try
                            {
                                IChannelReceiverHook hook = null;
                                foreach (IChannel channel in ChannelServices.RegisteredChannels)
                                {
                                    IChannelReceiverHook hook2 = channel as IChannelReceiverHook;
                                    if (((hook2 != null) && (string.Compare(hook2.ChannelScheme, "http", StringComparison.OrdinalIgnoreCase) == 0)) && hook2.WantsToListen)
                                    {
                                        hook = hook2;
                                        break;
                                    }
                                }
                                if (hook == null)
                                {
                                    HttpChannel chnl = new HttpChannel();
                                    ChannelServices.RegisterChannel(chnl, false);
                                    hook = chnl;
                                }
                                string str2 = null;
                                if (IisHelper.IsSslRequired)
                                {
                                    str2 = "https";
                                }
                                else
                                {
                                    str2 = "http";
                                }
                                string channelUri = str2 + "://" + CoreChannel.GetMachineIp();
                                int port = context.Request.Url.Port;
                                string str4 = string.Concat(new object[] { ":", port, "/", RemotingConfiguration.ApplicationName });
                                channelUri = channelUri + str4;
                                hook.AddHookChannelUri(channelUri);
                                ChannelDataStore channelData = ((IChannelReceiver) hook).ChannelData as ChannelDataStore;
                                if (channelData != null)
                                {
                                    channelUri = channelData.ChannelUris[0];
                                }
                                IisHelper.ApplicationUrl = channelUri;
                                ChannelServices.UnregisterChannel(null);
                                s_transportSink = new HttpHandlerTransportSink(hook.ChannelSinkChain);
                            }
                            catch (Exception exception2)
                            {
                                s_fatalException = exception2;
                                this.WriteException(context, exception2);
                                return;
                            }
                            bLoadedConfiguration = true;
                        }
                    }
                }
                if (s_fatalException == null)
                {
                    if (!this.CanServiceRequest(context))
                    {
                        this.WriteException(context, new RemotingException(CoreChannel.GetResourceString("Remoting_ChnlSink_UriNotPublished")));
                    }
                    else
                    {
                        s_transportSink.HandleRequest(context);
                    }
                }
                else
                {
                    this.WriteException(context, s_fatalException);
                }
            }
            catch (Exception exception3)
            {
                this.WriteException(context, exception3);
            }
        }

        internal static bool IsLocal(HttpContext context)
        {
            string str = context.Request.ServerVariables["LOCAL_ADDR"];
            string userHostAddress = context.Request.UserHostAddress;
            return (context.Request.Url.IsLoopback || (((str != null) && (userHostAddress != null)) && (str == userHostAddress)));
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void ProcessRequest(HttpContext context)
        {
            this.InternalProcessRequest(context);
        }

        private void WriteException(HttpContext context, Exception e)
        {
            Stream outputStream = context.Response.OutputStream;
            context.Response.Clear();
            context.Response.ClearHeaders();
            context.Response.ContentType = this.ComposeContentType("text/plain", Encoding.UTF8);
            context.Response.StatusCode = 500;
            context.Response.StatusDescription = CoreChannel.GetResourceString("Remoting_InternalError");
            StreamWriter writer = new StreamWriter(outputStream, new UTF8Encoding(false));
            writer.WriteLine(this.GenerateFaultString(context, e));
            writer.Flush();
        }

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }
    }
}

