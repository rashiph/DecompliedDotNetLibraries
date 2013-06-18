namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.Text;

    internal class ComIntegrationManifestGenerator : MarshalByRefObject
    {
        private static void AsmCreateWin32ManifestFile(Stream s, Type[] aTypes)
        {
            string str = "</assembly>";
            WriteTypes(s, aTypes, 4);
            WriteUTFChars(s, str);
        }

        internal static void GenerateManifestCollectionFile(Guid[] manifests, string strAssemblyManifestFileName, string assemblyName)
        {
            string str = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>";
            string str2 = "<assembly xmlns=\"urn:schemas-microsoft-com:asm.v1\" manifestVersion=\"1.0\">";
            string str3 = "</assembly>";
            string directoryName = Path.GetDirectoryName(strAssemblyManifestFileName);
            if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.DirectoryNotFound(directoryName));
            }
            Stream s = null;
            try
            {
                s = File.Create(strAssemblyManifestFileName);
                WriteUTFChars(s, str + Environment.NewLine);
                WriteUTFChars(s, str2 + Environment.NewLine);
                WriteUTFChars(s, "<assemblyIdentity" + Environment.NewLine, 4);
                WriteUTFChars(s, "name=\"" + assemblyName + "\"" + Environment.NewLine, 8);
                WriteUTFChars(s, "version=\"1.0.0.0\"/>" + Environment.NewLine, 8);
                for (int i = 0; i < manifests.Length; i++)
                {
                    WriteUTFChars(s, "<dependency>" + Environment.NewLine, 4);
                    WriteUTFChars(s, "<dependentAssembly>" + Environment.NewLine, 8);
                    WriteUTFChars(s, "<assemblyIdentity" + Environment.NewLine, 12);
                    WriteUTFChars(s, "name=\"" + manifests[i].ToString() + "\"" + Environment.NewLine, 0x10);
                    WriteUTFChars(s, "version=\"1.0.0.0\"/>" + Environment.NewLine, 0x10);
                    WriteUTFChars(s, "</dependentAssembly>" + Environment.NewLine, 8);
                    WriteUTFChars(s, "</dependency>" + Environment.NewLine, 4);
                }
                WriteUTFChars(s, str3);
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
                s.Close();
                File.Delete(strAssemblyManifestFileName);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.ManifestCreationFailed(strAssemblyManifestFileName, exception.Message));
            }
            s.Close();
        }

        internal static void GenerateWin32ManifestFile(Type[] aTypes, string strAssemblyManifestFileName, string assemblyName)
        {
            string str = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>";
            string str2 = "<assembly xmlns=\"urn:schemas-microsoft-com:asm.v1\" manifestVersion=\"1.0\">";
            string directoryName = Path.GetDirectoryName(strAssemblyManifestFileName);
            if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.DirectoryNotFound(directoryName));
            }
            Stream s = null;
            try
            {
                s = File.Create(strAssemblyManifestFileName);
                WriteUTFChars(s, str + Environment.NewLine);
                WriteUTFChars(s, str2 + Environment.NewLine);
                WriteUTFChars(s, "<assemblyIdentity" + Environment.NewLine, 4);
                WriteUTFChars(s, "name=\"" + assemblyName + "\"" + Environment.NewLine, 8);
                WriteUTFChars(s, "version=\"1.0.0.0\"/>" + Environment.NewLine, 8);
                AsmCreateWin32ManifestFile(s, aTypes);
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
                s.Close();
                File.Delete(strAssemblyManifestFileName);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.ManifestCreationFailed(strAssemblyManifestFileName, exception.Message));
            }
            s.Close();
        }

        private static void WriteTypes(Stream s, Type[] aTypes, int offset)
        {
            RegistrationServices services = new RegistrationServices();
            string fullName = null;
            string imageRuntimeVersion = Assembly.GetExecutingAssembly().ImageRuntimeVersion;
            foreach (Type type in aTypes)
            {
                if (!services.TypeRequiresRegistration(type))
                {
                    throw Fx.AssertAndThrow("User defined types must be registrable");
                }
                string str3 = "{" + Marshal.GenerateGuidForType(type).ToString().ToUpperInvariant() + "}";
                fullName = type.FullName;
                if (services.TypeRepresentsComType(type) || type.IsValueType)
                {
                    WriteUTFChars(s, "<clrSurrogate" + Environment.NewLine, offset);
                    WriteUTFChars(s, "    clsid=\"" + str3 + "\"" + Environment.NewLine, offset);
                    WriteUTFChars(s, "    name=\"" + fullName + "\"" + Environment.NewLine, offset);
                    WriteUTFChars(s, "    runtimeVersion=\"" + imageRuntimeVersion + "\">" + Environment.NewLine, offset);
                    WriteUTFChars(s, "</clrSurrogate>" + Environment.NewLine, offset);
                }
            }
        }

        private static void WriteUTFChars(Stream s, string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            s.Write(bytes, 0, bytes.Length);
        }

        private static void WriteUTFChars(Stream s, string value, int offset)
        {
            for (int i = 0; i < offset; i++)
            {
                WriteUTFChars(s, " ");
            }
            WriteUTFChars(s, value);
        }
    }
}

