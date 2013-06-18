namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using Microsoft.Build.Utilities;
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Text;

    public class CommandLineBuilderExtension : CommandLineBuilder
    {
        internal void AppendByChoiceSwitch(string switchName, Hashtable bag, string parameterName, string choice1, string choice2)
        {
            object obj2 = bag[parameterName];
            if (obj2 != null)
            {
                bool flag = (bool) obj2;
                base.AppendSwitchUnquotedIfNotNull(switchName, flag ? choice1 : choice2);
            }
        }

        internal void AppendNestedSwitch(string outerSwitchName, string innerSwitchName, string parameter)
        {
            base.AppendSwitchIfNotNull(outerSwitchName, innerSwitchName + this.GetQuotedText(parameter));
        }

        internal void AppendPlusOrMinusSwitch(string switchName, Hashtable bag, string parameterName)
        {
            object obj2 = bag[parameterName];
            if (obj2 != null)
            {
                bool flag = (bool) obj2;
                base.AppendSwitchUnquotedIfNotNull(switchName, flag ? "+" : "-");
            }
        }

        internal void AppendSwitchAliased(string switchName, string alias, string parameter)
        {
            base.AppendSwitchIfNotNull(switchName, alias + "=");
            base.AppendTextWithQuoting(parameter);
        }

        internal void AppendSwitchIfNotNull(string switchName, ITaskItem[] parameters, string[] attributes)
        {
            this.AppendSwitchIfNotNull(switchName, parameters, attributes, null);
        }

        internal void AppendSwitchIfNotNull(string switchName, ITaskItem[] parameters, string[] metadataNames, bool[] treatAsFlags)
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrow((treatAsFlags == null) || (metadataNames.Length == treatAsFlags.Length), "metadataNames and treatAsFlags should have the same length.");
            if (parameters != null)
            {
                foreach (ITaskItem item in parameters)
                {
                    base.AppendSwitchIfNotNull(switchName, item.ItemSpec);
                    if (metadataNames != null)
                    {
                        for (int i = 0; i < metadataNames.Length; i++)
                        {
                            string metadata = item.GetMetadata(metadataNames[i]);
                            if ((metadata != null) && (metadata.Length > 0))
                            {
                                if ((treatAsFlags == null) || !treatAsFlags[i])
                                {
                                    base.CommandLine.Append(',');
                                    base.AppendTextWithQuoting(metadata);
                                }
                                else if (MetadataConversionUtilities.TryConvertItemMetadataToBool(item, metadataNames[i]))
                                {
                                    base.CommandLine.Append(',');
                                    base.AppendTextWithQuoting(metadataNames[i]);
                                }
                            }
                            else if ((treatAsFlags == null) || !treatAsFlags[i])
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }

        internal void AppendSwitchWithInteger(string switchName, Hashtable bag, string parameterName)
        {
            object obj2 = bag[parameterName];
            if (obj2 != null)
            {
                base.AppendSwitchIfNotNull(switchName, ((int) obj2).ToString(CultureInfo.InvariantCulture));
            }
        }

        internal void AppendSwitchWithSplitting(string switchName, string parameter, string delimiter, params char[] splitOn)
        {
            if (parameter != null)
            {
                string[] strArray = parameter.Split(splitOn, StringSplitOptions.RemoveEmptyEntries);
                string[] parameters = new string[strArray.Length];
                for (int i = 0; i < strArray.Length; i++)
                {
                    parameters[i] = strArray[i].Trim();
                }
                base.AppendSwitchIfNotNull(switchName, parameters, delimiter);
            }
        }

        internal void AppendWhenTrue(string switchName, Hashtable bag, string parameterName)
        {
            object obj2 = bag[parameterName];
            if ((obj2 != null) && ((bool) obj2))
            {
                base.AppendSwitch(switchName);
            }
        }

        protected string GetQuotedText(string unquotedText)
        {
            StringBuilder buffer = new StringBuilder();
            base.AppendQuotedTextToBuffer(buffer, unquotedText);
            return buffer.ToString();
        }

        internal static bool IsParameterEmpty(string parameter, params char[] splitOn)
        {
            if (parameter != null)
            {
                string[] strArray = parameter.Split(splitOn, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < strArray.Length; i++)
                {
                    if (!string.IsNullOrEmpty(strArray[i].Trim()))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}

