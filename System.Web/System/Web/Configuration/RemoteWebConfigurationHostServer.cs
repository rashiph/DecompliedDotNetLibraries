namespace System.Web.Configuration
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.AccessControl;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Text;
    using System.Web;
    using System.Xml;

    [ProgId("System.Web.Configuration.RemoteWebConfigurationHostServerV4_32"), ComVisible(true), ClassInterface(ClassInterfaceType.AutoDual), Guid("9FDB6D2C-90EA-4e42-99E6-38B96E28698E"), SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
    public class RemoteWebConfigurationHostServer : IRemoteWebConfigurationHostServer
    {
        internal const char FilePathsSeparatorChar = '<';
        internal static readonly char[] FilePathsSeparatorParams = new char[] { '<' };
        private const int MOVEFILE_COPY_ALLOWED = 2;
        private const int MOVEFILE_DELAY_UNTIL_REBOOT = 4;
        private const int MOVEFILE_REPLACE_EXISTING = 1;
        private const int MOVEFILE_WRITE_THROUGH = 8;

        public string DoEncryptOrDecrypt(bool doEncrypt, string xmlString, string protectionProviderName, string protectionProviderType, string[] paramKeys, string[] paramValues)
        {
            XmlNode node;
            Type c = Type.GetType(protectionProviderType, true);
            if (!typeof(ProtectedConfigurationProvider).IsAssignableFrom(c))
            {
                throw new Exception(System.Web.SR.GetString("WrongType_of_Protected_provider"));
            }
            ProtectedConfigurationProvider provider = (ProtectedConfigurationProvider) Activator.CreateInstance(c);
            NameValueCollection config = new NameValueCollection(paramKeys.Length);
            for (int i = 0; i < paramKeys.Length; i++)
            {
                config.Add(paramKeys[i], paramValues[i]);
            }
            provider.Initialize(protectionProviderName, config);
            XmlDocument document = new XmlDocument {
                PreserveWhitespace = true
            };
            document.LoadXml(xmlString);
            if (doEncrypt)
            {
                node = provider.Encrypt(document.DocumentElement);
            }
            else
            {
                node = provider.Decrypt(document.DocumentElement);
            }
            return node.OuterXml;
        }

        private void DuplicateFileAttributes(string oldFileName, string newFileName)
        {
            FileAttributes fileAttributes = File.GetAttributes(oldFileName);
            File.SetAttributes(newFileName, fileAttributes);
            DateTime creationTimeUtc = File.GetCreationTimeUtc(oldFileName);
            File.SetCreationTimeUtc(newFileName, creationTimeUtc);
            this.DuplicateTemplateAttributes(oldFileName, newFileName);
        }

        private void DuplicateTemplateAttributes(string oldFileName, string newFileName)
        {
            System.Security.AccessControl.FileSecurity accessControl;
            try
            {
                accessControl = File.GetAccessControl(oldFileName, AccessControlSections.Access | AccessControlSections.Audit);
                accessControl.SetAuditRuleProtection(accessControl.AreAuditRulesProtected, true);
            }
            catch (UnauthorizedAccessException)
            {
                accessControl = File.GetAccessControl(oldFileName, AccessControlSections.Access);
            }
            accessControl.SetAccessRuleProtection(accessControl.AreAccessRulesProtected, true);
            File.SetAccessControl(newFileName, accessControl);
        }

        public byte[] GetData(string fileName, bool getReadTimeOnly, out long readTime)
        {
            byte[] buffer;
            if (!fileName.ToLowerInvariant().EndsWith(".config", StringComparison.Ordinal))
            {
                throw new Exception(System.Web.SR.GetString("Can_not_access_files_other_than_config"));
            }
            if (File.Exists(fileName))
            {
                if (getReadTimeOnly)
                {
                    buffer = new byte[0];
                }
                else
                {
                    buffer = File.ReadAllBytes(fileName);
                }
                DateTime lastWriteTimeUtc = File.GetLastWriteTimeUtc(fileName);
                readTime = (DateTime.UtcNow > lastWriteTimeUtc) ? DateTime.UtcNow.Ticks : lastWriteTimeUtc.Ticks;
                return buffer;
            }
            buffer = new byte[0];
            readTime = DateTime.UtcNow.Ticks;
            return buffer;
        }

        public void GetFileDetails(string name, out bool exists, out long size, out long createDate, out long lastWriteDate)
        {
            System.Web.UnsafeNativeMethods.WIN32_FILE_ATTRIBUTE_DATA win_file_attribute_data;
            if (!name.ToLowerInvariant().EndsWith(".config", StringComparison.Ordinal))
            {
                throw new Exception(System.Web.SR.GetString("Can_not_access_files_other_than_config"));
            }
            if (System.Web.UnsafeNativeMethods.GetFileAttributesEx(name, 0, out win_file_attribute_data) && ((win_file_attribute_data.fileAttributes & 0x10) == 0))
            {
                exists = true;
                size = (win_file_attribute_data.fileSizeHigh << 0x20) | win_file_attribute_data.fileSizeLow;
                createDate = (win_file_attribute_data.ftCreationTimeHigh << 0x20) | win_file_attribute_data.ftCreationTimeLow;
                lastWriteDate = (win_file_attribute_data.ftLastWriteTimeHigh << 0x20) | win_file_attribute_data.ftLastWriteTimeLow;
            }
            else
            {
                exists = false;
                size = 0L;
                createDate = 0L;
                lastWriteDate = 0L;
            }
        }

        public string GetFilePaths(int webLevelAsInt, string path, string site, string locationSubPath)
        {
            string str;
            string str2;
            VirtualPath path3;
            string str3;
            string str4;
            string str5;
            VirtualPath path4;
            WebLevel webLevel = (WebLevel) webLevelAsInt;
            IConfigMapPath instance = IISMapPath.GetInstance();
            WebConfigurationHost.GetConfigPaths(instance, webLevel, VirtualPath.CreateNonRelativeAllowNull(path), site, locationSubPath, out path3, out str, out str2, out str3, out str4);
            ArrayList list = new ArrayList();
            list.Add(VirtualPath.GetVirtualPathString(path3));
            list.Add(str);
            list.Add(str2);
            list.Add(str3);
            list.Add(str4);
            WebConfigurationHost.GetSiteIDAndVPathFromConfigPath(str3, out str5, out path4);
            list.Add("machine");
            list.Add(HttpConfigurationSystem.MachineConfigurationFilePath);
            if (webLevel != WebLevel.Machine)
            {
                list.Add("machine/webroot");
                list.Add(HttpConfigurationSystem.RootWebConfigurationFilePath);
                for (VirtualPath path5 = path4; path5 != null; path5 = path5.Parent)
                {
                    string configPathFromSiteIDAndVPath = WebConfigurationHost.GetConfigPathFromSiteIDAndVPath(str2, path5);
                    string str7 = Path.Combine(instance.MapPath(str2, path5.VirtualPathString), "web.config");
                    list.Add(configPathFromSiteIDAndVPath);
                    list.Add(str7);
                }
            }
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < list.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append('<');
                }
                string str8 = (string) list[i];
                builder.Append(str8);
            }
            return builder.ToString();
        }

        private static string GetRandomFileExt()
        {
            byte[] data = new byte[2];
            new RNGCryptoServiceProvider().GetBytes(data);
            return (data[1].ToString("X", CultureInfo.InvariantCulture) + data[0].ToString("X", CultureInfo.InvariantCulture));
        }

        public void WriteData(string fileName, string templateFileName, byte[] data, ref long readTime)
        {
            if (!fileName.ToLowerInvariant().EndsWith(".config", StringComparison.Ordinal))
            {
                throw new Exception(System.Web.SR.GetString("Can_not_access_files_other_than_config"));
            }
            bool flag = File.Exists(fileName);
            FileInfo info = null;
            FileAttributes normal = FileAttributes.Normal;
            string path = null;
            Exception exception = null;
            FileStream stream = null;
            long ticks = 0L;
            long num2 = 0L;
            if (flag && (File.GetLastWriteTimeUtc(fileName).Ticks > readTime))
            {
                throw new Exception(System.Web.SR.GetString("File_changed_since_read", new object[] { fileName }));
            }
            if (flag)
            {
                try
                {
                    info = new FileInfo(fileName);
                    normal = info.Attributes;
                }
                catch
                {
                }
                if ((normal & (FileAttributes.Hidden | FileAttributes.ReadOnly)) != 0)
                {
                    throw new Exception(System.Web.SR.GetString("File_is_read_only", new object[] { fileName }));
                }
            }
            path = fileName + "." + GetRandomFileExt() + ".tmp";
            for (int i = 0; File.Exists(path); i++)
            {
                if (i > 100)
                {
                    throw new Exception(System.Web.SR.GetString("Unable_to_create_temp_file"));
                }
                path = fileName + "." + GetRandomFileExt() + ".tmp";
            }
            try
            {
                stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite, data.Length, FileOptions.None | FileOptions.WriteThrough);
                stream.Write(data, 0, data.Length);
            }
            catch (Exception exception2)
            {
                exception = exception2;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
            if (exception != null)
            {
                try
                {
                    File.Delete(path);
                }
                catch
                {
                }
                throw exception;
            }
            if (flag)
            {
                try
                {
                    this.DuplicateFileAttributes(fileName, path);
                    goto Label_0170;
                }
                catch
                {
                    goto Label_0170;
                }
            }
            if (templateFileName != null)
            {
                try
                {
                    this.DuplicateTemplateAttributes(fileName, templateFileName);
                }
                catch
                {
                }
            }
        Label_0170:
            if (!System.Web.UnsafeNativeMethods.MoveFileEx(path, fileName, 11))
            {
                try
                {
                    File.Delete(path);
                }
                catch
                {
                }
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
            if (flag)
            {
                info = new FileInfo(fileName) {
                    Attributes = normal
                };
            }
            ticks = File.GetLastWriteTimeUtc(fileName).Ticks;
            num2 = DateTime.UtcNow.Ticks;
            readTime = (num2 > ticks) ? num2 : ticks;
        }
    }
}

