namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using Microsoft.Build.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;

    internal class ComReferenceInfo
    {
        internal System.Runtime.InteropServices.ComTypes.TYPELIBATTR attr;
        internal List<string> dependentWrapperPaths;
        internal ComReferenceInfo primaryOfAxImpRef;
        internal ITaskItem referencePathItem;
        internal ComReferenceWrapperInfo resolvedWrapper;
        internal ITaskItem taskItem;
        internal string typeLibName;
        internal string typeLibPath;
        internal ITypeLib typeLibPointer;

        internal ComReferenceInfo()
        {
            this.dependentWrapperPaths = new List<string>();
        }

        internal ComReferenceInfo(ComReferenceInfo copyFrom)
        {
            this.attr = copyFrom.attr;
            this.typeLibName = copyFrom.typeLibName;
            this.typeLibPath = copyFrom.typeLibPath;
            this.typeLibPointer = copyFrom.typeLibPointer;
            this.primaryOfAxImpRef = copyFrom.primaryOfAxImpRef;
            this.resolvedWrapper = copyFrom.resolvedWrapper;
            this.taskItem = new TaskItem(copyFrom.taskItem);
            this.dependentWrapperPaths = copyFrom.dependentWrapperPaths;
            this.referencePathItem = copyFrom.referencePathItem;
        }

        private string GetTypeLibId(TaskLoggingHelper log)
        {
            if (this.taskItem != null)
            {
                return this.taskItem.ItemSpec;
            }
            return log.FormatResourceString("ResolveComReference.TypeLibAttrId", new object[] { this.attr.guid, this.attr.wMajorVerNum, this.attr.wMinorVerNum });
        }

        internal bool InitializeWithPath(TaskLoggingHelper log, string path, ITaskItem originalTaskItem, string targetProcessorArchitecture)
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(path, "path");
            this.taskItem = originalTaskItem;
            this.typeLibPath = ComReference.StripTypeLibNumberFromPath(path, new Microsoft.Build.Shared.FileExists(File.Exists));
            string str = targetProcessorArchitecture;
            if (str != null)
            {
                if (!(str == "AMD64") && !(str == "IA64"))
                {
                    if (str == "x86")
                    {
                        this.typeLibPointer = (ITypeLib) Microsoft.Build.Tasks.NativeMethods.LoadTypeLibEx(path, 0x20);
                        goto Label_00A2;
                    }
                    if (str == "MSIL")
                    {
                    }
                }
                else
                {
                    this.typeLibPointer = (ITypeLib) Microsoft.Build.Tasks.NativeMethods.LoadTypeLibEx(path, 0x40);
                    goto Label_00A2;
                }
            }
            this.typeLibPointer = (ITypeLib) Microsoft.Build.Tasks.NativeMethods.LoadTypeLibEx(path, 2);
        Label_00A2:
            try
            {
                ComReference.GetTypeLibAttrForTypeLib(ref this.typeLibPointer, out this.attr);
                if (!ComReference.GetTypeLibNameForITypeLib(log, this.typeLibPointer, this.GetTypeLibId(log), out this.typeLibName))
                {
                    this.ReleaseTypeLibPtr();
                    return false;
                }
            }
            catch (COMException)
            {
                this.ReleaseTypeLibPtr();
                throw;
            }
            return true;
        }

        internal bool InitializeWithTypeLibAttrs(TaskLoggingHelper log, System.Runtime.InteropServices.ComTypes.TYPELIBATTR tlbAttr, ITaskItem originalTaskItem, string targetProcessorArchitecture)
        {
            System.Runtime.InteropServices.ComTypes.TYPELIBATTR typeLibAttr = tlbAttr;
            ComReference.RemapAdoTypeLib(log, ref typeLibAttr);
            if (!ComReference.GetPathOfTypeLib(log, ref typeLibAttr, out this.typeLibPath))
            {
                return false;
            }
            return this.InitializeWithPath(log, this.typeLibPath, originalTaskItem, targetProcessorArchitecture);
        }

        internal void ReleaseTypeLibPtr()
        {
            if (this.typeLibPointer != null)
            {
                Marshal.ReleaseComObject(this.typeLibPointer);
                this.typeLibPointer = null;
            }
        }

        internal string SourceItemSpec
        {
            get
            {
                if (this.taskItem == null)
                {
                    return null;
                }
                return this.taskItem.ItemSpec;
            }
        }
    }
}

