namespace System.Web.DataAccess
{
    using System;
    using System.Web;

    internal sealed class SqlExpressConnectionErrorFormatter : DataConnectionErrorFormatter
    {
        internal SqlExpressConnectionErrorFormatter(DataConnectionErrorEnum error)
        {
            base._UserName = HttpRuntime.HasUnmanagedPermission() ? DataConnectionHelper.GetCurrentName() : string.Empty;
            base._Error = error;
        }

        internal SqlExpressConnectionErrorFormatter(string userName, DataConnectionErrorEnum error)
        {
            base._UserName = userName;
            base._Error = error;
        }

        protected override string Description
        {
            get
            {
                string name = null;
                string str2 = null;
                string str3;
                switch (base._Error)
                {
                    case DataConnectionErrorEnum.CanNotCreateDataDir:
                        name = "DataAccessError_CanNotCreateDataDir_Description";
                        str2 = "DataAccessError_CanNotCreateDataDir_Description_2";
                        break;

                    case DataConnectionErrorEnum.CanNotWriteToDataDir:
                        name = "SqlExpressError_CanNotWriteToDataDir_Description";
                        str2 = "SqlExpressError_CanNotWriteToDataDir_Description_2";
                        break;

                    case DataConnectionErrorEnum.CanNotWriteToDBFile:
                        name = "SqlExpressError_CanNotWriteToDbfFile_Description";
                        str2 = "SqlExpressError_CanNotWriteToDbfFile_Description_2";
                        break;
                }
                if (!string.IsNullOrEmpty(base._UserName))
                {
                    str3 = System.Web.SR.GetString(name, new object[] { base._UserName });
                }
                else
                {
                    str3 = System.Web.SR.GetString(str2);
                }
                return (str3 + " " + System.Web.SR.GetString("SqlExpressError_Description_1"));
            }
        }

        protected override string ErrorTitle
        {
            get
            {
                string name = null;
                switch (base._Error)
                {
                    case DataConnectionErrorEnum.CanNotCreateDataDir:
                        name = "DataAccessError_CanNotCreateDataDir_Title";
                        break;

                    case DataConnectionErrorEnum.CanNotWriteToDataDir:
                        name = "SqlExpressError_CanNotWriteToDataDir_Title";
                        break;

                    case DataConnectionErrorEnum.CanNotWriteToDBFile:
                        name = "SqlExpressError_CanNotWriteToDbfFile_Title";
                        break;
                }
                return System.Web.SR.GetString(name);
            }
        }
    }
}

