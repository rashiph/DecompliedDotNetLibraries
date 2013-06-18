namespace System.EnterpriseServices
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;

    internal class RegistrationExporterNotifySink : ITypeLibExporterNotifySink
    {
        private Report _report;
        private string _tlb;

        internal RegistrationExporterNotifySink(string tlb, Report report)
        {
            this._tlb = tlb;
            this._report = report;
        }

        public void ReportEvent(ExporterEventKind EventKind, int EventCode, string EventMsg)
        {
            if ((EventKind != ExporterEventKind.NOTIF_TYPECONVERTED) && (this._report != null))
            {
                this._report(EventMsg);
            }
        }

        public object ResolveRef(Assembly asm)
        {
            string str2 = Path.Combine(Path.GetDirectoryName(asm.Location), asm.GetName().Name) + ".tlb";
            if (this._report != null)
            {
                this._report(Resource.FormatString("Reg_AutoExportMsg", asm.FullName, str2));
            }
            return (ITypeLib) RegistrationDriver.GenerateTypeLibrary(asm, str2, this._report);
        }
    }
}

