namespace System.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security;
    using System.Security.Permissions;
    using System.Xml;

    internal class UriSectionReader
    {
        private string configFilePath;
        private XmlReader reader;
        private const string rootElementName = "configuration";
        private UriSectionData sectionData;

        private UriSectionReader(string configFilePath, UriSectionData parentData)
        {
            this.configFilePath = configFilePath;
            this.sectionData = new UriSectionData();
            if (parentData != null)
            {
                this.sectionData.IriParsing = parentData.IriParsing;
                this.sectionData.IdnScope = parentData.IdnScope;
                foreach (KeyValuePair<string, SchemeSettingInternal> pair in parentData.SchemeSettings)
                {
                    this.sectionData.SchemeSettings.Add(pair.Key, pair.Value);
                }
            }
        }

        private static bool AreEqual(string value1, string value2)
        {
            return (string.Compare(value1, value2, StringComparison.OrdinalIgnoreCase) == 0);
        }

        private void ClearSchemeSetting()
        {
            this.sectionData.SchemeSettings.Clear();
        }

        private UriSectionData GetSectionData()
        {
            new FileIOPermission(FileIOPermissionAccess.Read, this.configFilePath).Assert();
            try
            {
                if (File.Exists(this.configFilePath))
                {
                    using (FileStream stream = new FileStream(this.configFilePath, FileMode.Open, FileAccess.Read))
                    {
                        XmlReaderSettings settings = new XmlReaderSettings {
                            IgnoreComments = true,
                            IgnoreWhitespace = true,
                            IgnoreProcessingInstructions = true
                        };
                        using (this.reader = XmlReader.Create(stream, settings))
                        {
                            if (this.ReadConfiguration())
                            {
                                return this.sectionData;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            return null;
        }

        private bool IsEndElement(string elementName)
        {
            return ((this.reader.NodeType == XmlNodeType.EndElement) && (string.Compare(this.reader.Name, elementName, StringComparison.OrdinalIgnoreCase) == 0));
        }

        public static UriSectionData Read(string configFilePath)
        {
            return Read(configFilePath, null);
        }

        public static UriSectionData Read(string configFilePath, UriSectionData parentData)
        {
            UriSectionReader reader = new UriSectionReader(configFilePath, parentData);
            return reader.GetSectionData();
        }

        private bool ReadAddSchemeSetting()
        {
            string attribute = this.reader.GetAttribute("name");
            string str2 = this.reader.GetAttribute("genericUriParserOptions");
            if (string.IsNullOrEmpty(attribute) || string.IsNullOrEmpty(str2))
            {
                return false;
            }
            try
            {
                GenericUriParserOptions options = (GenericUriParserOptions) Enum.Parse(typeof(GenericUriParserOptions), str2);
                SchemeSettingInternal internal2 = new SchemeSettingInternal(attribute, options);
                this.sectionData.SchemeSettings[internal2.Name] = internal2;
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        private bool ReadConfiguration()
        {
            if (this.ReadToUriSection())
            {
            Label_007A:
                if (!this.reader.Read())
                {
                    return false;
                }
                if (this.IsEndElement("uri"))
                {
                    return true;
                }
                if (this.reader.NodeType != XmlNodeType.Element)
                {
                    return false;
                }
                string name = this.reader.Name;
                if (!AreEqual(name, "iriParsing"))
                {
                    if (AreEqual(name, "idn"))
                    {
                        if (!this.ReadIdnScope())
                        {
                            goto Label_0078;
                        }
                        goto Label_007A;
                    }
                    if (AreEqual(name, "schemeSettings") && this.ReadSchemeSettings())
                    {
                        goto Label_007A;
                    }
                }
                else if (this.ReadIriParsing())
                {
                    goto Label_007A;
                }
            }
            else
            {
                return false;
            }
        Label_0078:
            return false;
        }

        private bool ReadIdnScope()
        {
            string attribute = this.reader.GetAttribute("enabled");
            try
            {
                this.sectionData.IdnScope = new UriIdnScope?((UriIdnScope) Enum.Parse(typeof(UriIdnScope), attribute, true));
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        private bool ReadIriParsing()
        {
            bool flag;
            if (bool.TryParse(this.reader.GetAttribute("enabled"), out flag))
            {
                this.sectionData.IriParsing = new bool?(flag);
                return true;
            }
            return false;
        }

        private bool ReadRemoveSchemeSetting()
        {
            string attribute = this.reader.GetAttribute("name");
            if (string.IsNullOrEmpty(attribute))
            {
                return false;
            }
            this.sectionData.SchemeSettings.Remove(attribute);
            return true;
        }

        private bool ReadSchemeSettings()
        {
            while (this.reader.Read())
            {
                if (this.IsEndElement("schemeSettings"))
                {
                    return true;
                }
                if (this.reader.NodeType != XmlNodeType.Element)
                {
                    return false;
                }
                string name = this.reader.Name;
                if (AreEqual(name, "add"))
                {
                    if (!this.ReadAddSchemeSetting())
                    {
                        goto Label_0070;
                    }
                    continue;
                }
                if (AreEqual(name, "remove"))
                {
                    if (!this.ReadRemoveSchemeSetting())
                    {
                        goto Label_0070;
                    }
                    continue;
                }
                if (AreEqual(name, "clear"))
                {
                    this.ClearSchemeSetting();
                    continue;
                }
            Label_0070:
                return false;
            }
            return false;
        }

        private bool ReadToUriSection()
        {
            if (!this.reader.ReadToFollowing("configuration"))
            {
                return false;
            }
            if (this.reader.Depth != 0)
            {
                return false;
            }
            do
            {
                if (!this.reader.ReadToFollowing("uri"))
                {
                    return false;
                }
            }
            while (this.reader.Depth != 1);
            return true;
        }
    }
}

