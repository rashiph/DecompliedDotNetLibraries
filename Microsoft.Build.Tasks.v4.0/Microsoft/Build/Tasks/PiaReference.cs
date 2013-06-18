namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Shared;
    using Microsoft.Build.Utilities;
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class PiaReference : ComReference
    {
        internal PiaReference(TaskLoggingHelper taskLoggingHelper, ComReferenceInfo referenceInfo, string itemName) : base(taskLoggingHelper, referenceInfo, itemName)
        {
        }

        internal override bool FindExistingWrapper(out ComReferenceWrapperInfo wrapperInfo, DateTime componentTimestamp)
        {
            string str;
            string str2;
            wrapperInfo = null;
            TypeLibConverter converter = new TypeLibConverter();
            if (!converter.GetPrimaryInteropAssembly(this.ReferenceInfo.attr.guid, this.ReferenceInfo.attr.wMajorVerNum, this.ReferenceInfo.attr.wMinorVerNum, this.ReferenceInfo.attr.lcid, out str, out str2))
            {
                return false;
            }
            try
            {
                if ((str2 != null) && (str2.Length > 0))
                {
                    Uri uri = new Uri(str2);
                    Assembly assembly = Assembly.UnsafeLoadFrom(uri.LocalPath);
                    wrapperInfo = new ComReferenceWrapperInfo();
                    wrapperInfo.path = uri.LocalPath;
                    wrapperInfo.assembly = assembly;
                    wrapperInfo.originalPiaName = new AssemblyNameExtension(AssemblyName.GetAssemblyName(uri.LocalPath));
                }
                else
                {
                    Assembly assembly2 = Assembly.Load(str);
                    wrapperInfo = new ComReferenceWrapperInfo();
                    wrapperInfo.path = assembly2.Location;
                    wrapperInfo.assembly = assembly2;
                    wrapperInfo.originalPiaName = new AssemblyNameExtension(str, true);
                }
            }
            catch (FileNotFoundException)
            {
            }
            catch (BadImageFormatException)
            {
                base.Log.LogWarningWithCodeFromResources("ResolveComReference.BadAssemblyImage", new object[] { str });
            }
            return (wrapperInfo != null);
        }
    }
}

