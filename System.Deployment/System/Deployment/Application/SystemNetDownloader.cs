namespace System.Deployment.Application
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Cache;
    using System.Text;

    internal class SystemNetDownloader : FileDownloader
    {
        protected override void DownloadAllFiles()
        {
            FileDownloader.DownloadQueueItem item;
        Label_0000:
            item = null;
            lock (base._fileQueue)
            {
                if (base._fileQueue.Count > 0)
                {
                    item = (FileDownloader.DownloadQueueItem) base._fileQueue.Dequeue();
                }
            }
            if (item != null)
            {
                this.DownloadSingleFile(item);
                if (!base._fCancelPending)
                {
                    goto Label_0000;
                }
            }
            if (base._fCancelPending)
            {
                throw new DownloadCancelledException();
            }
        }

        protected void DownloadSingleFile(FileDownloader.DownloadQueueItem next)
        {
            Logger.AddMethodCall("DownloadSingleFile called");
            Logger.AddInternalState("DownloadQueueItem : " + ((next != null) ? next.ToString() : "null"));
            WebRequest request = WebRequest.Create(next._sourceUri);
            request.Credentials = CredentialCache.DefaultCredentials;
            RequestCachePolicy policy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
            request.CachePolicy = policy;
            HttpWebRequest httpreq = request as HttpWebRequest;
            if (httpreq != null)
            {
                httpreq.UnsafeAuthenticatedConnectionSharing = true;
                httpreq.AutomaticDecompression = DecompressionMethods.GZip;
                httpreq.CookieContainer = GetUriCookieContainer(httpreq.RequestUri);
                IWebProxy defaultWebProxy = WebRequest.DefaultWebProxy;
                if (defaultWebProxy != null)
                {
                    defaultWebProxy.Credentials = CredentialCache.DefaultCredentials;
                }
                Logger.AddInternalState("HttpWebRequest=" + Logger.Serialize(httpreq));
            }
            if (!base._fCancelPending)
            {
                WebResponse response = null;
                try
                {
                    response = request.GetResponse();
                    UriHelper.ValidateSupportedScheme(response.ResponseUri);
                    if (!base._fCancelPending)
                    {
                        base._eventArgs._fileSourceUri = next._sourceUri;
                        base._eventArgs._fileResponseUri = response.ResponseUri;
                        base._eventArgs.FileLocalPath = next._targetPath;
                        base._eventArgs.Cookie = null;
                        if (response.ContentLength > 0L)
                        {
                            base.CheckForSizeLimit((ulong) response.ContentLength, false);
                            base._accumulatedBytesTotal += response.ContentLength;
                        }
                        base.SetBytesTotal();
                        base.OnModified();
                        Stream responseStream = null;
                        Stream outputFileStream = null;
                        int tickCount = Environment.TickCount;
                        try
                        {
                            responseStream = response.GetResponseStream();
                            Directory.CreateDirectory(Path.GetDirectoryName(next._targetPath));
                            outputFileStream = GetOutputFileStream(next._targetPath);
                            if (outputFileStream != null)
                            {
                                int num3;
                                long num2 = 0L;
                                if (response.ContentLength > 0L)
                                {
                                    outputFileStream.SetLength(response.ContentLength);
                                }
                                do
                                {
                                    if (base._fCancelPending)
                                    {
                                        return;
                                    }
                                    num3 = responseStream.Read(base._buffer, 0, base._buffer.Length);
                                    if (num3 > 0)
                                    {
                                        outputFileStream.Write(base._buffer, 0, num3);
                                    }
                                    base._eventArgs._bytesCompleted += num3;
                                    if (response.ContentLength <= 0L)
                                    {
                                        base._accumulatedBytesTotal += num3;
                                        base.SetBytesTotal();
                                    }
                                    num2 += num3;
                                    if ((next._maxFileSize != -1) && (num2 > next._maxFileSize))
                                    {
                                        throw new InvalidDeploymentException(ExceptionTypes.FileSizeValidation, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_FileBeingDownloadedTooLarge"), new object[] { next._sourceUri.ToString(), next._maxFileSize }));
                                    }
                                    base.CheckForSizeLimit((ulong) num3, true);
                                    if (base._eventArgs._bytesTotal > 0L)
                                    {
                                        base._eventArgs._progress = (int) ((base._eventArgs._bytesCompleted * 100L) / base._eventArgs._bytesTotal);
                                    }
                                    base.OnModifiedWithThrottle(ref tickCount);
                                }
                                while (num3 > 0);
                                if (response.ContentLength != num2)
                                {
                                    outputFileStream.SetLength(num2);
                                }
                            }
                        }
                        finally
                        {
                            if (responseStream != null)
                            {
                                responseStream.Close();
                            }
                            if (outputFileStream != null)
                            {
                                outputFileStream.Close();
                            }
                        }
                        base._eventArgs.Cookie = next._cookie;
                        base._eventArgs._filesCompleted++;
                        Logger.AddInternalState("HttpWebResponse=" + Logger.Serialize(response));
                        base.OnModified();
                        DownloadResult result = new DownloadResult {
                            ResponseUri = response.ResponseUri
                        };
                        result.ServerInformation.Server = response.Headers["Server"];
                        result.ServerInformation.PoweredBy = response.Headers["X-Powered-By"];
                        result.ServerInformation.AspNetVersion = response.Headers["X-AspNet-Version"];
                        base._downloadResults.Add(result);
                    }
                }
                catch (InvalidOperationException exception)
                {
                    throw new DeploymentDownloadException(string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_FailedWhileDownloading"), new object[] { next._sourceUri }), exception);
                }
                catch (IOException exception2)
                {
                    throw new DeploymentDownloadException(string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_FailedWhileDownloading"), new object[] { next._sourceUri }), exception2);
                }
                catch (UnauthorizedAccessException exception3)
                {
                    throw new DeploymentDownloadException(string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_FailedWhileDownloading"), new object[] { next._sourceUri }), exception3);
                }
                finally
                {
                    if (response != null)
                    {
                        response.Close();
                    }
                }
            }
        }

        private static Stream GetOutputFileStream(string targetPath)
        {
            return new FileStream(targetPath, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
        }

        private static CookieContainer GetUriCookieContainer(Uri uri)
        {
            CookieContainer container = null;
            uint bytes = 0;
            if (NativeMethods.InternetGetCookieW(uri.ToString(), null, null, ref bytes))
            {
                StringBuilder cookieData = new StringBuilder((int) (bytes / 2));
                if (!NativeMethods.InternetGetCookieW(uri.ToString(), null, cookieData, ref bytes) || (cookieData.Length <= 0))
                {
                    return container;
                }
                try
                {
                    container = new CookieContainer();
                    container.SetCookies(uri, cookieData.ToString().Replace(';', ','));
                }
                catch (CookieException)
                {
                    container = null;
                }
            }
            return container;
        }
    }
}

