namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using Microsoft.Build.Utilities;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Text;

    internal static class PropertyParser
    {
        internal static bool GetTable(TaskLoggingHelper log, string parameterName, string[] propertyList, out Hashtable propertiesTable)
        {
            propertiesTable = null;
            if (propertyList != null)
            {
                propertiesTable = new Hashtable(StringComparer.OrdinalIgnoreCase);
                foreach (string str in propertyList)
                {
                    string str2 = string.Empty;
                    string str3 = string.Empty;
                    int index = str.IndexOf('=');
                    if (index != -1)
                    {
                        str2 = str.Substring(0, index).Trim();
                        str3 = str.Substring(index + 1).Trim();
                    }
                    if (str2.Length == 0)
                    {
                        if (log != null)
                        {
                            log.LogErrorWithCodeFromResources("General.InvalidPropertyError", new object[] { parameterName, str });
                        }
                        return false;
                    }
                    propertiesTable[str2] = str3;
                }
            }
            return true;
        }

        internal static bool GetTableWithEscaping(TaskLoggingHelper log, string parameterName, string syntaxName, string[] propertyNameValueStrings, out Hashtable finalPropertiesTable)
        {
            finalPropertiesTable = null;
            if (propertyNameValueStrings != null)
            {
                finalPropertiesTable = new Hashtable(StringComparer.OrdinalIgnoreCase);
                List<PropertyNameValuePair> list = new List<PropertyNameValuePair>();
                foreach (string str in propertyNameValueStrings)
                {
                    int index = str.IndexOf('=');
                    if (index != -1)
                    {
                        string propertyName = str.Substring(0, index).Trim();
                        string propertyValue = Microsoft.Build.Shared.EscapingUtilities.Escape(str.Substring(index + 1).Trim());
                        if (propertyName.Length == 0)
                        {
                            if (log != null)
                            {
                                log.LogErrorWithCodeFromResources("General.InvalidPropertyError", new object[] { syntaxName, str });
                            }
                            return false;
                        }
                        list.Add(new PropertyNameValuePair(propertyName, propertyValue));
                    }
                    else if (list.Count > 0)
                    {
                        string str4 = Microsoft.Build.Shared.EscapingUtilities.Escape(str.Trim());
                        list[list.Count - 1].Value.Append(';');
                        list[list.Count - 1].Value.Append(str4);
                    }
                    else
                    {
                        if (log != null)
                        {
                            log.LogErrorWithCodeFromResources("General.InvalidPropertyError", new object[] { syntaxName, str });
                        }
                        return false;
                    }
                }
                if (log != null)
                {
                    log.LogMessageFromText(parameterName, MessageImportance.Low);
                }
                foreach (PropertyNameValuePair pair in list)
                {
                    finalPropertiesTable[pair.Name] = pair.Value.ToString();
                    if (log != null)
                    {
                        log.LogMessageFromText(string.Format(CultureInfo.InvariantCulture, "  {0}={1}", new object[] { pair.Name, pair.Value.ToString() }), MessageImportance.Low);
                    }
                }
            }
            return true;
        }

        private class PropertyNameValuePair
        {
            private string name;
            private StringBuilder value;

            private PropertyNameValuePair()
            {
            }

            internal PropertyNameValuePair(string propertyName, string propertyValue)
            {
                this.Name = propertyName;
                this.Value = new StringBuilder();
                this.Value.Append(propertyValue);
            }

            internal string Name
            {
                get
                {
                    return this.name;
                }
                set
                {
                    this.name = value;
                }
            }

            internal StringBuilder Value
            {
                get
                {
                    return this.value;
                }
                set
                {
                    this.value = value;
                }
            }
        }
    }
}

