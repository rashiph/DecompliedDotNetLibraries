namespace System.Web.Mail
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Hosting;

    [Obsolete("The recommended alternative is System.Net.Mail.SmtpClient. http://go.microsoft.com/fwlink/?linkid=14202")]
    public class SmtpMail
    {
        private static object _lockObject = new object();
        private static string _server;

        private SmtpMail()
        {
        }

        [AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.Medium), SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
        public static void Send(MailMessage message)
        {
            lock (_lockObject)
            {
                if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                {
                    throw new PlatformNotSupportedException(System.Web.SR.GetString("RequiresNT"));
                }
                if (!CdoSysHelper.OsSupportsCdoSys())
                {
                    throw new PlatformNotSupportedException(System.Web.SR.GetString("SmtpMail_not_supported_on_Win7_and_higher"));
                }
                if (Environment.OSVersion.Version.Major <= 4)
                {
                    CdoNtsHelper.Send(message);
                }
                else
                {
                    CdoSysHelper.Send(message);
                }
            }
        }

        [SecurityPermission(SecurityAction.Assert, UnmanagedCode=true), AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.Medium)]
        public static void Send(string from, string to, string subject, string messageText)
        {
            lock (_lockObject)
            {
                if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                {
                    throw new PlatformNotSupportedException(System.Web.SR.GetString("RequiresNT"));
                }
                if (!CdoSysHelper.OsSupportsCdoSys())
                {
                    throw new PlatformNotSupportedException(System.Web.SR.GetString("SmtpMail_not_supported_on_Win7_and_higher"));
                }
                if (Environment.OSVersion.Version.Major <= 4)
                {
                    CdoNtsHelper.Send(from, to, subject, messageText);
                }
                else
                {
                    CdoSysHelper.Send(from, to, subject, messageText);
                }
            }
        }

        public static string SmtpServer
        {
            get
            {
                string str = _server;
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                _server = value;
            }
        }

        internal class CdoNtsHelper
        {
            private static SmtpMail.LateBoundAccessHelper _helper = new SmtpMail.LateBoundAccessHelper("CDONTS.NewMail");

            private CdoNtsHelper()
            {
            }

            internal static void Send(MailMessage message)
            {
                object obj2 = _helper.CreateInstance();
                if (message.From != null)
                {
                    _helper.SetProp(obj2, "From", message.From);
                }
                if (message.To != null)
                {
                    _helper.SetProp(obj2, "To", message.To);
                }
                if (message.Cc != null)
                {
                    _helper.SetProp(obj2, "Cc", message.Cc);
                }
                if (message.Bcc != null)
                {
                    _helper.SetProp(obj2, "Bcc", message.Bcc);
                }
                if (message.Subject != null)
                {
                    _helper.SetProp(obj2, "Subject", message.Subject);
                }
                if (message.Priority != MailPriority.Normal)
                {
                    int propValue = 0;
                    switch (message.Priority)
                    {
                        case MailPriority.Normal:
                            propValue = 1;
                            break;

                        case MailPriority.Low:
                            propValue = 0;
                            break;

                        case MailPriority.High:
                            propValue = 2;
                            break;
                    }
                    _helper.SetProp(obj2, "Importance", propValue);
                }
                if (message.BodyEncoding != null)
                {
                    _helper.CallMethod(obj2, "SetLocaleIDs", new object[] { message.BodyEncoding.CodePage });
                }
                if (message.UrlContentBase != null)
                {
                    _helper.SetProp(obj2, "ContentBase", message.UrlContentBase);
                }
                if (message.UrlContentLocation != null)
                {
                    _helper.SetProp(obj2, "ContentLocation", message.UrlContentLocation);
                }
                if (message.Headers.Count > 0)
                {
                    IDictionaryEnumerator enumerator = message.Headers.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        string key = (string) enumerator.Key;
                        string str2 = (string) enumerator.Value;
                        _helper.SetProp(obj2, "Value", key, str2);
                    }
                }
                if (message.BodyFormat == MailFormat.Html)
                {
                    _helper.SetProp(obj2, "BodyFormat", 0);
                    _helper.SetProp(obj2, "MailFormat", 0);
                }
                _helper.SetProp(obj2, "Body", (message.Body != null) ? message.Body : string.Empty);
                IEnumerator enumerator2 = message.Attachments.GetEnumerator();
                while (enumerator2.MoveNext())
                {
                    MailAttachment current = (MailAttachment) enumerator2.Current;
                    int num3 = 0;
                    switch (current.Encoding)
                    {
                        case MailEncoding.UUEncode:
                            num3 = 0;
                            break;

                        case MailEncoding.Base64:
                            num3 = 1;
                            break;
                    }
                    object[] args = new object[3];
                    args[0] = current.Filename;
                    args[2] = num3;
                    _helper.CallMethod(obj2, "AttachFile", args);
                }
                _helper.CallMethod(obj2, "Send", new object[5]);
                Marshal.ReleaseComObject(obj2);
            }

            internal static void Send(string from, string to, string subject, string messageText)
            {
                MailMessage message = new MailMessage {
                    From = from,
                    To = to,
                    Subject = subject,
                    Body = messageText
                };
                Send(message);
            }
        }

        internal class CdoSysHelper
        {
            private static SmtpMail.LateBoundAccessHelper _helper = new SmtpMail.LateBoundAccessHelper("CDO.Message");
            private static CdoSysLibraryStatus cdoSysLibraryInfo = CdoSysLibraryStatus.NotChecked;

            private CdoSysHelper()
            {
            }

            private static bool CdoSysExists()
            {
                if (cdoSysLibraryInfo != CdoSysLibraryStatus.NotChecked)
                {
                    return (cdoSysLibraryInfo == CdoSysLibraryStatus.Exists);
                }
                IntPtr hModule = System.Web.UnsafeNativeMethods.LoadLibrary("cdosys.dll");
                if (hModule != IntPtr.Zero)
                {
                    System.Web.UnsafeNativeMethods.FreeLibrary(hModule);
                    cdoSysLibraryInfo = CdoSysLibraryStatus.Exists;
                    return true;
                }
                cdoSysLibraryInfo = CdoSysLibraryStatus.DoesntExist;
                return false;
            }

            internal static bool OsSupportsCdoSys()
            {
                Version version = Environment.OSVersion.Version;
                return (((version.Major < 7) && ((version.Major != 6) || (version.Minor < 1))) || CdoSysExists());
            }

            internal static void Send(MailMessage message)
            {
                object obj2 = _helper.CreateInstance();
                if (message.From != null)
                {
                    _helper.SetProp(obj2, "From", message.From);
                }
                if (message.To != null)
                {
                    _helper.SetProp(obj2, "To", message.To);
                }
                if (message.Cc != null)
                {
                    _helper.SetProp(obj2, "Cc", message.Cc);
                }
                if (message.Bcc != null)
                {
                    _helper.SetProp(obj2, "Bcc", message.Bcc);
                }
                if (message.Subject != null)
                {
                    _helper.SetProp(obj2, "Subject", message.Subject);
                }
                if (message.Priority != MailPriority.Normal)
                {
                    string str = null;
                    switch (message.Priority)
                    {
                        case MailPriority.Normal:
                            str = "normal";
                            break;

                        case MailPriority.Low:
                            str = "low";
                            break;

                        case MailPriority.High:
                            str = "high";
                            break;
                    }
                    if (str != null)
                    {
                        SetField(obj2, "importance", str);
                    }
                }
                if (message.BodyEncoding != null)
                {
                    object prop = _helper.GetProp(obj2, "BodyPart");
                    SmtpMail.LateBoundAccessHelper.SetPropStatic(prop, "Charset", message.BodyEncoding.BodyName);
                    Marshal.ReleaseComObject(prop);
                }
                if (message.UrlContentBase != null)
                {
                    SetField(obj2, "content-base", message.UrlContentBase);
                }
                if (message.UrlContentLocation != null)
                {
                    SetField(obj2, "content-location", message.UrlContentLocation);
                }
                if (message.Headers.Count > 0)
                {
                    IDictionaryEnumerator enumerator = message.Headers.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        SetField(obj2, (string) enumerator.Key, (string) enumerator.Value);
                    }
                }
                if (message.Body != null)
                {
                    if (message.BodyFormat == MailFormat.Html)
                    {
                        _helper.SetProp(obj2, "HtmlBody", message.Body);
                    }
                    else
                    {
                        _helper.SetProp(obj2, "TextBody", message.Body);
                    }
                }
                else
                {
                    _helper.SetProp(obj2, "TextBody", string.Empty);
                }
                IEnumerator enumerator2 = message.Attachments.GetEnumerator();
                while (enumerator2.MoveNext())
                {
                    MailAttachment current = (MailAttachment) enumerator2.Current;
                    object[] args = new object[3];
                    args[0] = current.Filename;
                    object o = _helper.CallMethod(obj2, "AddAttachment", args);
                    if (current.Encoding == MailEncoding.UUEncode)
                    {
                        _helper.SetProp(obj2, "MimeFormatted", false);
                    }
                    if (o != null)
                    {
                        Marshal.ReleaseComObject(o);
                    }
                }
                string smtpServer = SmtpMail.SmtpServer;
                if (!string.IsNullOrEmpty(smtpServer) || (message.Fields.Count > 0))
                {
                    object propStatic = SmtpMail.LateBoundAccessHelper.GetPropStatic(obj2, "Configuration");
                    if (propStatic != null)
                    {
                        SmtpMail.LateBoundAccessHelper.SetPropStatic(propStatic, "Fields", "http://schemas.microsoft.com/cdo/configuration/sendusing", 2);
                        SmtpMail.LateBoundAccessHelper.SetPropStatic(propStatic, "Fields", "http://schemas.microsoft.com/cdo/configuration/smtpserverport", 0x19);
                        if (!string.IsNullOrEmpty(smtpServer))
                        {
                            SmtpMail.LateBoundAccessHelper.SetPropStatic(propStatic, "Fields", "http://schemas.microsoft.com/cdo/configuration/smtpserver", smtpServer);
                        }
                        foreach (DictionaryEntry entry in message.Fields)
                        {
                            SmtpMail.LateBoundAccessHelper.SetPropStatic(propStatic, "Fields", (string) entry.Key, entry.Value);
                        }
                        object obj6 = SmtpMail.LateBoundAccessHelper.GetPropStatic(propStatic, "Fields");
                        SmtpMail.LateBoundAccessHelper.CallMethodStatic(obj6, "Update", new object[0]);
                        Marshal.ReleaseComObject(obj6);
                        Marshal.ReleaseComObject(propStatic);
                    }
                }
                if (HostingEnvironment.IsHosted)
                {
                    using (new ProcessImpersonationContext())
                    {
                        _helper.CallMethod(obj2, "Send", new object[0]);
                        goto Label_03C0;
                    }
                }
                _helper.CallMethod(obj2, "Send", new object[0]);
            Label_03C0:
                Marshal.ReleaseComObject(obj2);
            }

            internal static void Send(string from, string to, string subject, string messageText)
            {
                MailMessage message = new MailMessage {
                    From = from,
                    To = to,
                    Subject = subject,
                    Body = messageText
                };
                Send(message);
            }

            private static void SetField(object m, string name, string value)
            {
                _helper.SetProp(m, "Fields", "urn:schemas:mailheader:" + name, value);
                object prop = _helper.GetProp(m, "Fields");
                SmtpMail.LateBoundAccessHelper.CallMethodStatic(prop, "Update", new object[0]);
                Marshal.ReleaseComObject(prop);
            }

            private enum CdoSysLibraryStatus
            {
                NotChecked,
                Exists,
                DoesntExist
            }
        }

        internal class LateBoundAccessHelper
        {
            private string _progId;
            private Type _type;

            internal LateBoundAccessHelper(string progId)
            {
                this._progId = progId;
            }

            internal object CallMethod(object obj, string methodName, object[] args)
            {
                object obj2;
                try
                {
                    obj2 = CallMethod(this.LateBoundType, obj, methodName, args);
                }
                catch (Exception exception)
                {
                    throw new HttpException(GetInnerMostException(exception).Message, exception);
                }
                return obj2;
            }

            private static object CallMethod(Type type, object obj, string methodName, object[] args)
            {
                return type.InvokeMember(methodName, BindingFlags.InvokeMethod, null, obj, args, CultureInfo.InvariantCulture);
            }

            internal static object CallMethodStatic(object obj, string methodName, object[] args)
            {
                return CallMethod(obj.GetType(), obj, methodName, args);
            }

            internal object CreateInstance()
            {
                return Activator.CreateInstance(this.LateBoundType);
            }

            private static Exception GetInnerMostException(Exception e)
            {
                if (e.InnerException == null)
                {
                    return e;
                }
                return GetInnerMostException(e.InnerException);
            }

            internal object GetProp(object obj, string propName)
            {
                object obj2;
                try
                {
                    obj2 = GetProp(this.LateBoundType, obj, propName);
                }
                catch (Exception exception)
                {
                    throw new HttpException(GetInnerMostException(exception).Message, exception);
                }
                return obj2;
            }

            private static object GetProp(Type type, object obj, string propName)
            {
                return type.InvokeMember(propName, BindingFlags.GetProperty, null, obj, new object[0], CultureInfo.InvariantCulture);
            }

            internal static object GetPropStatic(object obj, string propName)
            {
                return GetProp(obj.GetType(), obj, propName);
            }

            internal void SetProp(object obj, string propName, object propValue)
            {
                try
                {
                    SetProp(this.LateBoundType, obj, propName, propValue);
                }
                catch (Exception exception)
                {
                    throw new HttpException(GetInnerMostException(exception).Message, exception);
                }
            }

            internal void SetProp(object obj, string propName, object propKey, object propValue)
            {
                try
                {
                    SetProp(this.LateBoundType, obj, propName, propKey, propValue);
                }
                catch (Exception exception)
                {
                    throw new HttpException(GetInnerMostException(exception).Message, exception);
                }
            }

            private static void SetProp(Type type, object obj, string propName, object propValue)
            {
                if (((propValue != null) && (propValue is string)) && (((string) propValue).IndexOf('\0') >= 0))
                {
                    throw new ArgumentException();
                }
                type.InvokeMember(propName, BindingFlags.SetProperty, null, obj, new object[] { propValue }, CultureInfo.InvariantCulture);
            }

            private static void SetProp(Type type, object obj, string propName, object propKey, object propValue)
            {
                if (((propValue != null) && (propValue is string)) && (((string) propValue).IndexOf('\0') >= 0))
                {
                    throw new ArgumentException();
                }
                type.InvokeMember(propName, BindingFlags.SetProperty, null, obj, new object[] { propKey, propValue }, CultureInfo.InvariantCulture);
            }

            internal static void SetPropStatic(object obj, string propName, object propValue)
            {
                SetProp(obj.GetType(), obj, propName, propValue);
            }

            internal static void SetPropStatic(object obj, string propName, object propKey, object propValue)
            {
                SetProp(obj.GetType(), obj, propName, propKey, propValue);
            }

            private Type LateBoundType
            {
                get
                {
                    if (this._type == null)
                    {
                        try
                        {
                            this._type = Type.GetTypeFromProgID(this._progId);
                        }
                        catch
                        {
                        }
                        if (this._type == null)
                        {
                            throw new HttpException(System.Web.SR.GetString("SMTP_TypeCreationError", new object[] { this._progId }));
                        }
                    }
                    return this._type;
                }
            }
        }
    }
}

