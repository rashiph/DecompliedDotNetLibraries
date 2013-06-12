namespace System.Drawing.Printing
{
    using System;
    using System.Drawing;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;

    [Serializable]
    public class InvalidPrinterException : SystemException
    {
        private PrinterSettings settings;

        public InvalidPrinterException(PrinterSettings settings) : base(GenerateMessage(settings))
        {
            this.settings = settings;
        }

        protected InvalidPrinterException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.settings = (PrinterSettings) info.GetValue("settings", typeof(PrinterSettings));
        }

        private static string GenerateMessage(PrinterSettings settings)
        {
            if (settings.IsDefaultPrinter)
            {
                return System.Drawing.SR.GetString("InvalidPrinterException_NoDefaultPrinter");
            }
            try
            {
                return System.Drawing.SR.GetString("InvalidPrinterException_InvalidPrinter", new object[] { settings.PrinterName });
            }
            catch (SecurityException)
            {
                return System.Drawing.SR.GetString("InvalidPrinterException_InvalidPrinter", new object[] { System.Drawing.SR.GetString("CantTellPrinterName") });
            }
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            IntSecurity.AllPrinting.Demand();
            info.AddValue("settings", this.settings);
            base.GetObjectData(info, context);
        }
    }
}

