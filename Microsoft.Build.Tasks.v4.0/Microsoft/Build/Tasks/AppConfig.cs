namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Shared;
    using System;
    using System.Xml;

    internal sealed class AppConfig
    {
        private RuntimeSection runtime = new RuntimeSection();

        internal void Load(string appConfigFile)
        {
            XmlTextReader reader = null;
            try
            {
                reader = new XmlTextReader(appConfigFile);
                this.Read(reader);
            }
            catch (XmlException exception)
            {
                throw new AppConfigException(exception.Message, appConfigFile, (reader != null) ? reader.LineNumber : 0, (reader != null) ? reader.LinePosition : 0, exception);
            }
            catch (Exception exception2)
            {
                if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception2))
                {
                    throw;
                }
                throw new AppConfigException(exception2.Message, appConfigFile, (reader != null) ? reader.LineNumber : 0, (reader != null) ? reader.LinePosition : 0, exception2);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
        }

        internal void Read(XmlTextReader reader)
        {
            while (reader.Read())
            {
                if ((reader.NodeType == XmlNodeType.Element) && StringEquals(reader.Name, "runtime"))
                {
                    this.runtime.Read(reader);
                }
            }
        }

        internal static bool StringEquals(string a, string b)
        {
            return (string.Compare(a, b, StringComparison.OrdinalIgnoreCase) == 0);
        }

        internal RuntimeSection Runtime
        {
            get
            {
                return this.runtime;
            }
        }
    }
}

