namespace Microsoft.Build.Tasks.Deployment.ManifestUtilities
{
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Resources;
    using System.Runtime.InteropServices;

    internal class ComImporter
    {
        private readonly ComClass[] comClasses;
        private static readonly string[] emptyArray = new string[0];
        private static readonly string[] knownImplementedCategories = new string[] { "{02496840-3AC4-11cf-87B9-00AA006C8166}", "{02496841-3AC4-11cf-87B9-00AA006C8166}", "{40FC6ED5-2438-11CF-A3DB-080036F12502}" };
        private static readonly string[] knownSubKeys = new string[] { "Control", "Programmable", "ToolboxBitmap32", "TypeLib", "Version", "VersionIndependentProgID" };
        private readonly string outputDisplayName;
        private readonly OutputMessageCollection outputMessages;
        private readonly ResourceManager resources = new ResourceManager("Microsoft.Build.Tasks.Deployment.ManifestUtilities.Strings", Assembly.GetExecutingAssembly());
        private bool success = true;
        private readonly Microsoft.Build.Tasks.Deployment.ManifestUtilities.TypeLib typeLib;

        public ComImporter(string path, OutputMessageCollection outputMessages, string outputDisplayName)
        {
            this.outputMessages = outputMessages;
            this.outputDisplayName = outputDisplayName;
            if (Microsoft.Build.Tasks.Deployment.ManifestUtilities.NativeMethods.SfcIsFileProtected(IntPtr.Zero, path) != 0)
            {
                outputMessages.AddWarningMessage("GenerateManifest.ComImport", new string[] { outputDisplayName, this.resources.GetString("ComImporter.ProtectedFile") });
            }
            object typeLib = null;
            try
            {
                Microsoft.Build.Tasks.Deployment.ManifestUtilities.NativeMethods.LoadTypeLibEx(path, Microsoft.Build.Tasks.Deployment.ManifestUtilities.NativeMethods.RegKind.RegKind_None, out typeLib);
            }
            catch (COMException)
            {
            }
            UCOMITypeLib lib = (UCOMITypeLib) typeLib;
            if (lib != null)
            {
                string str;
                string str2;
                string str3;
                int num;
                IntPtr zero = IntPtr.Zero;
                lib.GetLibAttr(out zero);
                TYPELIBATTR typelibattr = (TYPELIBATTR) Marshal.PtrToStructure(zero, typeof(TYPELIBATTR));
                lib.ReleaseTLibAttr(zero);
                Guid tlbId = typelibattr.guid;
                lib.GetDocumentation(-1, out str, out str2, out num, out str3);
                string helpDirectory = Microsoft.Build.Tasks.Deployment.ManifestUtilities.Util.FilterNonprintableChars(str3);
                this.typeLib = new Microsoft.Build.Tasks.Deployment.ManifestUtilities.TypeLib(tlbId, new Version(typelibattr.wMajorVerNum, typelibattr.wMinorVerNum), helpDirectory, typelibattr.lcid, Convert.ToInt32(typelibattr.wLibFlags, CultureInfo.InvariantCulture));
                List<ComClass> list = new List<ComClass>();
                int typeInfoCount = lib.GetTypeInfoCount();
                for (int i = 0; i < typeInfoCount; i++)
                {
                    TYPEKIND typekind;
                    lib.GetTypeInfoType(i, out typekind);
                    if (typekind == TYPEKIND.TKIND_COCLASS)
                    {
                        UCOMITypeInfo info;
                        lib.GetTypeInfo(i, out info);
                        IntPtr ppTypeAttr = IntPtr.Zero;
                        info.GetTypeAttr(out ppTypeAttr);
                        TYPEATTR typeattr = (TYPEATTR) Marshal.PtrToStructure(ppTypeAttr, typeof(TYPEATTR));
                        info.ReleaseTypeAttr(ppTypeAttr);
                        Guid guid = typeattr.guid;
                        guid.ToString("B");
                        lib.GetDocumentation(i, out str, out str2, out num, out str3);
                        string description = Microsoft.Build.Tasks.Deployment.ManifestUtilities.Util.FilterNonprintableChars(str2);
                        ClassInfo registeredClassInfo = this.GetRegisteredClassInfo(guid);
                        if (registeredClassInfo != null)
                        {
                            list.Add(new ComClass(tlbId, guid, registeredClassInfo.Progid, registeredClassInfo.ThreadingModel, description));
                        }
                    }
                }
                if (list.Count > 0)
                {
                    this.comClasses = list.ToArray();
                    this.success = true;
                }
                else
                {
                    outputMessages.AddErrorMessage("GenerateManifest.ComImport", new string[] { outputDisplayName, this.resources.GetString("ComImporter.NoRegisteredClasses") });
                    this.success = false;
                }
            }
            else
            {
                outputMessages.AddErrorMessage("GenerateManifest.ComImport", new string[] { outputDisplayName, this.resources.GetString("ComImporter.TypeLibraryLoadFailure") });
                this.success = false;
            }
        }

        private void CheckForUnknownSubKeys(RegistryKey key)
        {
            this.CheckForUnknownSubKeys(key, emptyArray);
        }

        private void CheckForUnknownSubKeys(RegistryKey key, string[] knownNames)
        {
            if (key.SubKeyCount > 0)
            {
                foreach (string str in key.GetSubKeyNames())
                {
                    if (Array.BinarySearch<string>(knownNames, str, StringComparer.OrdinalIgnoreCase) < 0)
                    {
                        this.outputMessages.AddWarningMessage("GenerateManifest.ComImport", new string[] { this.outputDisplayName, string.Format(CultureInfo.CurrentCulture, this.resources.GetString("ComImporter.SubKeyNotImported"), new object[] { key.Name + @"\" + str }) });
                    }
                }
            }
        }

        private void CheckForUnknownValues(RegistryKey key)
        {
            this.CheckForUnknownValues(key, emptyArray);
        }

        private void CheckForUnknownValues(RegistryKey key, string[] knownNames)
        {
            if (key.ValueCount > 0)
            {
                foreach (string str in key.GetValueNames())
                {
                    if (!string.IsNullOrEmpty(str) && (Array.BinarySearch<string>(knownNames, str, StringComparer.OrdinalIgnoreCase) < 0))
                    {
                        this.outputMessages.AddWarningMessage("GenerateManifest.ComImport", new string[] { this.outputDisplayName, string.Format(CultureInfo.CurrentCulture, this.resources.GetString("ComImporter.ValueNotImported"), new object[] { key.Name + @"\@" + str }) });
                    }
                }
            }
        }

        private ClassInfo GetRegisteredClassInfo(Guid clsid)
        {
            ClassInfo info = null;
            RegistryKey rootKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\CLASSES\CLSID");
            if (this.GetRegisteredClassInfo(rootKey, clsid, ref info))
            {
                return info;
            }
            RegistryKey key2 = Registry.ClassesRoot.OpenSubKey("CLSID");
            if (this.GetRegisteredClassInfo(key2, clsid, ref info))
            {
                return info;
            }
            return null;
        }

        private bool GetRegisteredClassInfo(RegistryKey rootKey, Guid clsid, ref ClassInfo info)
        {
            if (rootKey == null)
            {
                return false;
            }
            string name = clsid.ToString("B");
            RegistryKey key = rootKey.OpenSubKey(name);
            if (key == null)
            {
                return false;
            }
            bool flag = true;
            string str2 = null;
            string threadingModel = null;
            string progid = null;
            foreach (string str5 in key.GetSubKeyNames())
            {
                RegistryKey key2 = key.OpenSubKey(str5);
                if (string.Equals(str5, "InProcServer32", StringComparison.OrdinalIgnoreCase))
                {
                    str2 = (string) key2.GetValue(null);
                    threadingModel = (string) key2.GetValue("ThreadingModel");
                    this.CheckForUnknownSubKeys(key2);
                    this.CheckForUnknownValues(key2, new string[] { "ThreadingModel" });
                }
                else if (string.Equals(str5, "ProgID", StringComparison.OrdinalIgnoreCase))
                {
                    key.OpenSubKey(str5);
                    progid = (string) key2.GetValue(null);
                    this.CheckForUnknownSubKeys(key2);
                    this.CheckForUnknownValues(key2);
                }
                else if (string.Equals(str5, "LocalServer32", StringComparison.OrdinalIgnoreCase))
                {
                    this.outputMessages.AddWarningMessage("GenerateManifest.ComImport", new string[] { this.outputDisplayName, string.Format(CultureInfo.CurrentCulture, this.resources.GetString("ComImporter.LocalServerNotSupported"), new object[] { key.Name + @"\LocalServer32" }) });
                }
                else if (string.Equals(str5, "Implemented Categories", StringComparison.OrdinalIgnoreCase))
                {
                    this.CheckForUnknownSubKeys(key2, knownImplementedCategories);
                    this.CheckForUnknownValues(key2);
                }
                else if (Array.BinarySearch<string>(knownSubKeys, str5, StringComparer.OrdinalIgnoreCase) < 0)
                {
                    this.outputMessages.AddWarningMessage("GenerateManifest.ComImport", new string[] { this.outputDisplayName, string.Format(CultureInfo.CurrentCulture, this.resources.GetString("ComImporter.SubKeyNotImported"), new object[] { key.Name + @"\" + str5 }) });
                }
            }
            if (string.IsNullOrEmpty(str2))
            {
                this.outputMessages.AddErrorMessage("GenerateManifest.ComImport", new string[] { this.outputDisplayName, string.Format(CultureInfo.CurrentCulture, this.resources.GetString("ComImporter.MissingValue"), new object[] { key.Name + @"\InProcServer32", "(Default)" }) });
                flag = false;
            }
            info = new ClassInfo(progid, str2, threadingModel);
            return flag;
        }

        public ComClass[] ComClasses
        {
            get
            {
                return this.comClasses;
            }
        }

        public bool Success
        {
            get
            {
                return this.success;
            }
        }

        public Microsoft.Build.Tasks.Deployment.ManifestUtilities.TypeLib TypeLib
        {
            get
            {
                return this.typeLib;
            }
        }

        private class ClassInfo
        {
            internal readonly string Progid;
            internal readonly string RegisteredPath;
            internal readonly string ThreadingModel;

            internal ClassInfo(string progid, string registeredPath, string threadingModel)
            {
                this.Progid = progid;
                this.RegisteredPath = registeredPath;
                this.ThreadingModel = threadingModel;
            }
        }
    }
}

