namespace System.Runtime.InteropServices
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices.ComTypes;
    using System.Security;

    internal class ImporterCallback : ITypeLibImporterNotifySink
    {
        public void ReportEvent(ImporterEventKind EventKind, int EventCode, string EventMsg)
        {
        }

        [SecuritySafeCritical]
        public Assembly ResolveRef(object TypeLib)
        {
            try
            {
                ITypeLibConverter converter = new TypeLibConverter();
                return converter.ConvertTypeLibToAssembly(TypeLib, Marshal.GetTypeLibName((ITypeLib) TypeLib) + ".dll", TypeLibImporterFlags.None, new ImporterCallback(), null, null, null, null);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

