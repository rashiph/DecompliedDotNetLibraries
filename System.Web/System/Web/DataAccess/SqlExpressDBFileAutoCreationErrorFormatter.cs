namespace System.Web.DataAccess
{
    using System;
    using System.Web;

    internal sealed class SqlExpressDBFileAutoCreationErrorFormatter : UnhandledErrorFormatter
    {
        private static string s_errMessage = null;
        private static object s_Lock = new object();

        internal SqlExpressDBFileAutoCreationErrorFormatter(Exception exception) : base(exception)
        {
        }

        internal static string CustomErrorMessage
        {
            get
            {
                if (s_errMessage == null)
                {
                    lock (s_Lock)
                    {
                        if (s_errMessage == null)
                        {
                            string str = System.Web.SR.GetString("SqlExpress_MDF_File_Auto_Creation");
                            s_errMessage = s_errMessage + "<br><br><p>" + str + "<br></p>\n";
                            s_errMessage = s_errMessage + "<ol>\n";
                            str = System.Web.SR.GetString("SqlExpress_MDF_File_Auto_Creation_1");
                            s_errMessage = s_errMessage + "<li>" + str + "</li>\n";
                            str = System.Web.SR.GetString("SqlExpress_MDF_File_Auto_Creation_2");
                            s_errMessage = s_errMessage + "<li>" + str + "</li>\n";
                            str = System.Web.SR.GetString("SqlExpress_MDF_File_Auto_Creation_3");
                            s_errMessage = s_errMessage + "<li>" + str + "</li>\n";
                            str = System.Web.SR.GetString("SqlExpress_MDF_File_Auto_Creation_4");
                            s_errMessage = s_errMessage + "<li>" + str + "</li>\n";
                            s_errMessage = s_errMessage + "</ol>\n";
                        }
                    }
                }
                return s_errMessage;
            }
        }

        protected override string MiscSectionContent
        {
            get
            {
                return CustomErrorMessage;
            }
        }

        protected override string MiscSectionTitle
        {
            get
            {
                return System.Web.SR.GetString("SqlExpress_MDF_File_Auto_Creation_MiscSectionTitle");
            }
        }
    }
}

