namespace System.Web.DataAccess
{
    using System;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Web;
    using System.Web.Handlers;
    using System.Web.UI;

    internal class DataConnectionErrorFormatter : ErrorFormatter
    {
        protected DataConnectionErrorEnum _Error;
        protected string _UserName;
        protected static NameValueCollection s_errMessages = new NameValueCollection();
        protected static object s_Lock = new object();

        private string GetResourceStringAndSetAdaptiveNumberedText(ref int currentNumber, string resourceId)
        {
            string resourceString = System.Web.SR.GetString(resourceId);
            this.SetAdaptiveNumberedText(ref currentNumber, resourceString);
            return resourceString;
        }

        private string GetResourceStringAndSetAdaptiveNumberedText(ref int currentNumber, string resourceId, string parameter1)
        {
            string resourceString = System.Web.SR.GetString(resourceId, new object[] { parameter1 });
            this.SetAdaptiveNumberedText(ref currentNumber, resourceString);
            return resourceString;
        }

        private void SetAdaptiveNumberedText(ref int currentNumber, string resourceString)
        {
            string str = ((int) currentNumber).ToString(CultureInfo.InvariantCulture) + " " + resourceString;
            this.AdaptiveMiscContent.Add(str);
            currentNumber++;
        }

        protected override string ColoredSquareContent
        {
            get
            {
                return null;
            }
        }

        protected override string ColoredSquareTitle
        {
            get
            {
                return null;
            }
        }

        protected override string Description
        {
            get
            {
                return null;
            }
        }

        protected override string ErrorTitle
        {
            get
            {
                return null;
            }
        }

        protected override string MiscSectionContent
        {
            get
            {
                string str4;
                int currentNumber = 1;
                string resourceStringAndSetAdaptiveNumberedText = this.GetResourceStringAndSetAdaptiveNumberedText(ref currentNumber, "DataAccessError_MiscSection_1");
                string str3 = "<ol>\n<li>" + resourceStringAndSetAdaptiveNumberedText + "</li>\n";
                switch (this._Error)
                {
                    case DataConnectionErrorEnum.CanNotCreateDataDir:
                        resourceStringAndSetAdaptiveNumberedText = this.GetResourceStringAndSetAdaptiveNumberedText(ref currentNumber, "DataAccessError_MiscSection_2_CanNotCreateDataDir");
                        str3 = str3 + "<li>" + resourceStringAndSetAdaptiveNumberedText + "</li>\n";
                        resourceStringAndSetAdaptiveNumberedText = this.GetResourceStringAndSetAdaptiveNumberedText(ref currentNumber, "DataAccessError_MiscSection_2");
                        str3 = str3 + "<li>" + resourceStringAndSetAdaptiveNumberedText + "</li>\n";
                        break;

                    case DataConnectionErrorEnum.CanNotWriteToDataDir:
                        resourceStringAndSetAdaptiveNumberedText = this.GetResourceStringAndSetAdaptiveNumberedText(ref currentNumber, "DataAccessError_MiscSection_2");
                        str3 = str3 + "<li>" + resourceStringAndSetAdaptiveNumberedText + "</li>\n";
                        break;

                    case DataConnectionErrorEnum.CanNotWriteToDBFile:
                        resourceStringAndSetAdaptiveNumberedText = this.GetResourceStringAndSetAdaptiveNumberedText(ref currentNumber, "DataAccessError_MiscSection_2_CanNotWriteToDBFile_a");
                        str3 = str3 + "<li>" + resourceStringAndSetAdaptiveNumberedText + "</li>\n";
                        resourceStringAndSetAdaptiveNumberedText = this.GetResourceStringAndSetAdaptiveNumberedText(ref currentNumber, "DataAccessError_MiscSection_2_CanNotWriteToDBFile_b");
                        str3 = str3 + "<li>" + resourceStringAndSetAdaptiveNumberedText + "</li>\n";
                        break;
                }
                resourceStringAndSetAdaptiveNumberedText = this.GetResourceStringAndSetAdaptiveNumberedText(ref currentNumber, "DataAccessError_MiscSection_3");
                str3 = str3 + "<li>" + resourceStringAndSetAdaptiveNumberedText + "<br></li>\n";
                string str = AssemblyResourceLoader.GetWebResourceUrl(typeof(Page), "properties_security_tab.gif", true);
                str3 = str3 + "<br><br><IMG SRC=\"" + str + "\"><br><br><br>";
                resourceStringAndSetAdaptiveNumberedText = this.GetResourceStringAndSetAdaptiveNumberedText(ref currentNumber, "DataAccessError_MiscSection_ClickAdd");
                str3 = str3 + "<li>" + resourceStringAndSetAdaptiveNumberedText + "</li>\n";
                str = AssemblyResourceLoader.GetWebResourceUrl(typeof(Page), "add_permissions_for_users.gif", true);
                str3 = str3 + "<br><br><IMG SRC=\"" + str + "\"><br><br>";
                if (!string.IsNullOrEmpty(this._UserName))
                {
                    str4 = this.GetResourceStringAndSetAdaptiveNumberedText(ref currentNumber, "DataAccessError_MiscSection_4", this._UserName);
                }
                else
                {
                    str4 = this.GetResourceStringAndSetAdaptiveNumberedText(ref currentNumber, "DataAccessError_MiscSection_4_2");
                }
                str3 = str3 + "<li>" + str4 + "</li>\n";
                resourceStringAndSetAdaptiveNumberedText = this.GetResourceStringAndSetAdaptiveNumberedText(ref currentNumber, "DataAccessError_MiscSection_ClickOK");
                str3 = str3 + "<li>" + resourceStringAndSetAdaptiveNumberedText + "</li>\n";
                resourceStringAndSetAdaptiveNumberedText = this.GetResourceStringAndSetAdaptiveNumberedText(ref currentNumber, "DataAccessError_MiscSection_5");
                str3 = str3 + "<li>" + resourceStringAndSetAdaptiveNumberedText + "</li>\n";
                str = AssemblyResourceLoader.GetWebResourceUrl(typeof(Page), "properties_security_tab_w_user.gif", true);
                str3 = str3 + "<br><br><IMG SRC=\"" + str + "\"><br><br>";
                resourceStringAndSetAdaptiveNumberedText = this.GetResourceStringAndSetAdaptiveNumberedText(ref currentNumber, "DataAccessError_MiscSection_ClickOK");
                return (str3 + "<li>" + resourceStringAndSetAdaptiveNumberedText + "</li>\n");
            }
        }

        protected override string MiscSectionTitle
        {
            get
            {
                return System.Web.SR.GetString("DataAccessError_MiscSectionTitle");
            }
        }

        protected override bool ShowSourceFileInfo
        {
            get
            {
                return false;
            }
        }
    }
}

