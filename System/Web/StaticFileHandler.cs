namespace System.Web
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Web.Hosting;
    using System.Web.Util;

    internal class StaticFileHandler : IHttpHandler
    {
        private const string CONTENT_RANGE_FORMAT = "bytes {0}-{1}/{2}";
        private const int ERROR_ACCESS_DENIED = 5;
        private const int MAX_RANGE_ALLOWED = 5;
        private const string MULTIPART_CONTENT_TYPE = "multipart/byteranges; boundary=<q1w2e3r4t5y6u7i8o9p0zaxscdvfbgnhmjklkl>";
        private const string MULTIPART_RANGE_DELIMITER = "--<q1w2e3r4t5y6u7i8o9p0zaxscdvfbgnhmjklkl>\r\n";
        private const string MULTIPART_RANGE_END = "--<q1w2e3r4t5y6u7i8o9p0zaxscdvfbgnhmjklkl>--\r\n\r\n";
        private const string RANGE_BOUNDARY = "<q1w2e3r4t5y6u7i8o9p0zaxscdvfbgnhmjklkl>";

        internal StaticFileHandler()
        {
        }

        private static string GenerateETag(HttpContext context, DateTime lastModified, DateTime now)
        {
            long num = lastModified.ToFileTime();
            long num2 = now.ToFileTime();
            string str = num.ToString("X8", CultureInfo.InvariantCulture);
            if ((num2 - num) <= 0x1c9c380L)
            {
                return ("W/\"" + str + "\"");
            }
            return ("\"" + str + "\"");
        }

        private static FileInfo GetFileInfo(string virtualPathWithPathInfo, string physicalPath, HttpResponse response)
        {
            FileInfo info;
            if (!FileUtil.FileExists(physicalPath))
            {
                throw new HttpException(0x194, System.Web.SR.GetString("File_does_not_exist"));
            }
            if (physicalPath[physicalPath.Length - 1] == '.')
            {
                throw new HttpException(0x194, System.Web.SR.GetString("File_does_not_exist"));
            }
            try
            {
                info = new FileInfo(physicalPath);
            }
            catch (IOException exception)
            {
                if (!HttpRuntime.HasFilePermission(physicalPath))
                {
                    throw new HttpException(0x194, System.Web.SR.GetString("Error_trying_to_enumerate_files"));
                }
                throw new HttpException(0x194, System.Web.SR.GetString("Error_trying_to_enumerate_files"), exception);
            }
            catch (SecurityException exception2)
            {
                if (!HttpRuntime.HasFilePermission(physicalPath))
                {
                    throw new HttpException(0x191, System.Web.SR.GetString("File_enumerator_access_denied"));
                }
                throw new HttpException(0x191, System.Web.SR.GetString("File_enumerator_access_denied"), exception2);
            }
            if ((info.Attributes & FileAttributes.Hidden) != 0)
            {
                throw new HttpException(0x194, System.Web.SR.GetString("File_is_hidden"));
            }
            if ((info.Attributes & FileAttributes.Directory) != 0)
            {
                if (StringUtil.StringEndsWith(virtualPathWithPathInfo, '/'))
                {
                    throw new HttpException(0x193, System.Web.SR.GetString("Missing_star_mapping"));
                }
                response.Redirect(virtualPathWithPathInfo + "/");
            }
            return info;
        }

        private static bool GetLongFromSubstring(string s, ref int startIndex, out long result)
        {
            result = 0L;
            MovePastSpaceCharacters(s, ref startIndex);
            int num = startIndex;
            MovePastDigits(s, ref startIndex);
            int num2 = startIndex - 1;
            if (num2 < num)
            {
                return false;
            }
            long num3 = 1L;
            for (int i = num2; i >= num; i--)
            {
                int num5 = s[i] - '0';
                result += num5 * num3;
                num3 *= 10L;
                if (result < 0L)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool GetNextRange(string rangeHeader, ref int startIndex, long fileLength, out long offset, out long length, out bool isSatisfiable)
        {
            long num;
            offset = 0L;
            length = 0L;
            isSatisfiable = false;
            if (fileLength <= 0L)
            {
                startIndex = rangeHeader.Length;
                return true;
            }
            MovePastSpaceCharacters(rangeHeader, ref startIndex);
            if ((startIndex < rangeHeader.Length) && (rangeHeader[startIndex] == '-'))
            {
                startIndex++;
                if (!GetLongFromSubstring(rangeHeader, ref startIndex, out length))
                {
                    return false;
                }
                if (length > fileLength)
                {
                    offset = 0L;
                    length = fileLength;
                }
                else
                {
                    offset = fileLength - length;
                }
                isSatisfiable = IsRangeSatisfiable(offset, length, fileLength);
                return IncrementToNextRange(rangeHeader, ref startIndex);
            }
            if (GetLongFromSubstring(rangeHeader, ref startIndex, out offset) && ((startIndex < rangeHeader.Length) && (rangeHeader[startIndex] == '-')))
            {
                startIndex++;
            }
            else
            {
                return false;
            }
            if (!GetLongFromSubstring(rangeHeader, ref startIndex, out num))
            {
                length = fileLength - offset;
            }
            else
            {
                if (num > (fileLength - 1L))
                {
                    num = fileLength - 1L;
                }
                length = (num - offset) + 1L;
                if (length < 1L)
                {
                    return false;
                }
            }
            isSatisfiable = IsRangeSatisfiable(offset, length, fileLength);
            return IncrementToNextRange(rangeHeader, ref startIndex);
        }

        private static bool IncrementToNextRange(string s, ref int startIndex)
        {
            MovePastSpaceCharacters(s, ref startIndex);
            if (startIndex < s.Length)
            {
                if (s[startIndex] != ',')
                {
                    return false;
                }
                startIndex++;
            }
            return true;
        }

        private static bool IsOutDated(string ifRangeHeader, DateTime lastModified)
        {
            try
            {
                DateTime time = lastModified.ToUniversalTime();
                return (HttpDate.UtcParse(ifRangeHeader) < time);
            }
            catch
            {
                return true;
            }
        }

        private static bool IsRangeSatisfiable(long offset, long length, long fileLength)
        {
            return ((offset < fileLength) && (length > 0L));
        }

        private static bool IsSecurityError(int ErrorCode)
        {
            return (ErrorCode == 5);
        }

        private static void MovePastDigits(string s, ref int startIndex)
        {
            while (((startIndex < s.Length) && (s[startIndex] <= '9')) && (s[startIndex] >= '0'))
            {
                startIndex++;
            }
        }

        private static void MovePastSpaceCharacters(string s, ref int startIndex)
        {
            while ((startIndex < s.Length) && (s[startIndex] == ' '))
            {
                startIndex++;
            }
        }

        internal static unsafe bool ProcessRangeRequest(HttpContext context, string physicalPath, long fileLength, string rangeHeader, string etag, DateTime lastModified)
        {
            long offset;
            long length;
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;
            bool flag = false;
            if (fileLength <= 0L)
            {
                SendRangeNotSatisfiable(response, fileLength);
                return true;
            }
            string ifRangeHeader = request.Headers["If-Range"];
            if ((ifRangeHeader != null) && (ifRangeHeader.Length > 1))
            {
                if (ifRangeHeader[0] == '"')
                {
                    if (ifRangeHeader != etag)
                    {
                        return flag;
                    }
                }
                else
                {
                    if ((ifRangeHeader[0] == 'W') && (ifRangeHeader[1] == '/'))
                    {
                        return flag;
                    }
                    if (IsOutDated(ifRangeHeader, lastModified))
                    {
                        return flag;
                    }
                }
            }
            int index = rangeHeader.IndexOf('=');
            if ((index == -1) || (index == (rangeHeader.Length - 1)))
            {
                return flag;
            }
            int startIndex = index + 1;
            bool flag2 = true;
            bool flag4 = false;
            ByteRange[] rangeArray = null;
            int num5 = 0;
            long num6 = 0L;
            while ((startIndex < rangeHeader.Length) && flag2)
            {
                bool flag3;
                flag2 = GetNextRange(rangeHeader, ref startIndex, fileLength, out offset, out length, out flag3);
                if (!flag2)
                {
                    break;
                }
                if (flag3)
                {
                    if (rangeArray == null)
                    {
                        rangeArray = new ByteRange[0x10];
                    }
                    if (num5 >= rangeArray.Length)
                    {
                        ByteRange[] rangeArray2 = new ByteRange[rangeArray.Length * 2];
                        int len = rangeArray.Length * Marshal.SizeOf(rangeArray[0]);
                        fixed (ByteRange* rangeRef = rangeArray)
                        {
                            fixed (ByteRange* rangeRef2 = rangeArray2)
                            {
                                StringUtil.memcpyimpl((byte*) rangeRef, (byte*) rangeRef2, len);
                            }
                        }
                        rangeArray = rangeArray2;
                    }
                    rangeArray[num5].Offset = offset;
                    rangeArray[num5].Length = length;
                    num5++;
                    num6 += length;
                    if (num6 > (fileLength * 5L))
                    {
                        flag4 = true;
                        break;
                    }
                }
            }
            if (!flag2)
            {
                return flag;
            }
            if (flag4)
            {
                SendBadRequest(response);
                return true;
            }
            if (num5 == 0)
            {
                SendRangeNotSatisfiable(response, fileLength);
                return true;
            }
            string mimeMapping = MimeMapping.GetMimeMapping(physicalPath);
            if (num5 == 1)
            {
                offset = rangeArray[0].Offset;
                length = rangeArray[0].Length;
                response.ContentType = mimeMapping;
                string str3 = string.Format(CultureInfo.InvariantCulture, "bytes {0}-{1}/{2}", new object[] { offset, (offset + length) - 1L, fileLength });
                response.AppendHeader("Content-Range", str3);
                SendFile(physicalPath, offset, length, fileLength, context);
            }
            else
            {
                response.ContentType = "multipart/byteranges; boundary=<q1w2e3r4t5y6u7i8o9p0zaxscdvfbgnhmjklkl>";
                string s = "Content-Type: " + mimeMapping + "\r\n";
                for (int i = 0; i < num5; i++)
                {
                    offset = rangeArray[i].Offset;
                    length = rangeArray[i].Length;
                    response.Write("--<q1w2e3r4t5y6u7i8o9p0zaxscdvfbgnhmjklkl>\r\n");
                    response.Write(s);
                    response.Write("Content-Range: ");
                    string str4 = string.Format(CultureInfo.InvariantCulture, "bytes {0}-{1}/{2}", new object[] { offset, (offset + length) - 1L, fileLength });
                    response.Write(str4);
                    response.Write("\r\n\r\n");
                    SendFile(physicalPath, offset, length, fileLength, context);
                    response.Write("\r\n");
                }
                response.Write("--<q1w2e3r4t5y6u7i8o9p0zaxscdvfbgnhmjklkl>--\r\n\r\n");
            }
            response.StatusCode = 0xce;
            response.AppendHeader("Last-Modified", HttpUtility.FormatHttpDateTime(lastModified));
            response.AppendHeader("Accept-Ranges", "bytes");
            response.AppendHeader("ETag", etag);
            response.AppendHeader("Cache-Control", "public");
            return true;
        }

        public void ProcessRequest(HttpContext context)
        {
            string overrideVirtualPath = null;
            ProcessRequestInternal(context, overrideVirtualPath);
        }

        private static bool ProcessRequestForNonMapPathBasedVirtualFile(HttpRequest request, HttpResponse response, string overrideVirtualPath)
        {
            bool flag = false;
            if (HostingEnvironment.UsingMapPathBasedVirtualPathProvider)
            {
                return flag;
            }
            VirtualFile vf = null;
            string virtualPath = (overrideVirtualPath == null) ? request.FilePath : overrideVirtualPath;
            if (HostingEnvironment.VirtualPathProvider.FileExists(virtualPath))
            {
                vf = HostingEnvironment.VirtualPathProvider.GetFile(virtualPath);
            }
            if (vf == null)
            {
                throw new HttpException(0x194, System.Web.SR.GetString("File_does_not_exist"));
            }
            if (vf is MapPathBasedVirtualFile)
            {
                return flag;
            }
            response.WriteVirtualFile(vf);
            response.ContentType = MimeMapping.GetMimeMapping(virtualPath);
            return true;
        }

        internal static void ProcessRequestInternal(HttpContext context, string overrideVirtualPath)
        {
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;
            if (!ProcessRequestForNonMapPathBasedVirtualFile(request, response, overrideVirtualPath))
            {
                string path;
                string physicalPath;
                if (overrideVirtualPath == null)
                {
                    path = request.Path;
                    physicalPath = request.PhysicalPath;
                }
                else
                {
                    path = overrideVirtualPath;
                    physicalPath = request.MapPath(overrideVirtualPath);
                }
                FileInfo info = GetFileInfo(path, physicalPath, response);
                DateTime lastModified = new DateTime(info.LastWriteTime.Year, info.LastWriteTime.Month, info.LastWriteTime.Day, info.LastWriteTime.Hour, info.LastWriteTime.Minute, info.LastWriteTime.Second, 0);
                DateTime now = DateTime.Now;
                if (lastModified > now)
                {
                    lastModified = new DateTime(now.Ticks - (now.Ticks % 0x989680L));
                }
                string etag = GenerateETag(context, lastModified, now);
                long length = info.Length;
                string str4 = request.Headers["Range"];
                if (!StringUtil.StringStartsWithIgnoreCase(str4, "bytes") || !ProcessRangeRequest(context, physicalPath, length, str4, etag, lastModified))
                {
                    SendFile(physicalPath, 0L, length, length, context);
                    response.ContentType = MimeMapping.GetMimeMapping(physicalPath);
                    response.AppendHeader("Accept-Ranges", "bytes");
                    response.AddFileDependency(physicalPath);
                    response.Cache.SetIgnoreRangeRequests();
                    response.Cache.SetExpires(DateTime.Now.AddDays(1.0));
                    response.Cache.SetLastModified(lastModified);
                    response.Cache.SetETag(etag);
                    response.Cache.SetCacheability(HttpCacheability.Public);
                }
            }
        }

        private static void SendBadRequest(HttpResponse response)
        {
            response.StatusCode = 400;
            response.Write("<html><body>Bad Request</body></html>");
        }

        private static void SendFile(string physicalPath, long offset, long length, long fileLength, HttpContext context)
        {
            try
            {
                context.Response.TransmitFile(physicalPath, offset, length);
            }
            catch (ExternalException exception)
            {
                if (IsSecurityError(exception.ErrorCode))
                {
                    throw new HttpException(0x191, System.Web.SR.GetString("Resource_access_forbidden"));
                }
                throw;
            }
        }

        private static void SendRangeNotSatisfiable(HttpResponse response, long fileLength)
        {
            response.StatusCode = 0x1a0;
            response.ContentType = null;
            response.AppendHeader("Content-Range", "bytes */" + fileLength.ToString(NumberFormatInfo.InvariantInfo));
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

