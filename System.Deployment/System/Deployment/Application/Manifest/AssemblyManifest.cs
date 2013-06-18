namespace System.Deployment.Application.Manifest
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Deployment.Application;
    using System.Deployment.Application.Win32InterOp;
    using System.Deployment.Internal.CodeSigning;
    using System.Deployment.Internal.Isolation;
    using System.Deployment.Internal.Isolation.Manifest;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Xml;

    internal class AssemblyManifest
    {
        private DependentAssembly _clrDependentAssembly;
        private bool _clrDependentAssemblyChecked;
        private System.Deployment.Internal.Isolation.Manifest.ICMS _cms;
        private object _compatibleFrameworks;
        private System.Deployment.Application.DefinitionIdentity _complibIdentity;
        private object _dependentAssemblies;
        private object _dependentOS;
        private object _deployment;
        private object _description;
        private object _entryPoints;
        private object _fileAssociations;
        private object _files;
        private System.Deployment.Application.DefinitionIdentity _id1Identity;
        private bool _id1ManifestPresent;
        private string _id1RequestedExecutionLevel;
        private object _identity;
        private object _manifestFlags;
        private System.Deployment.Application.Manifest.ManifestSourceFormat _manifestSourceFormat;
        private byte[] _rawXmlBytes;
        private string _rawXmlFilePath;
        private object _requestedExecutionLevel;
        private object _requestedExecutionLevelUIAccess;
        private bool _signed;
        private ulong _sizeInBytes;
        private bool _unhashedDependencyPresent;
        private bool _unhashedFilePresent;
        private static char[] SpecificInvalidIdentityChars = new char[] { '#', '&' };

        public AssemblyManifest(System.Deployment.Internal.Isolation.Manifest.ICMS cms)
        {
            this._manifestSourceFormat = System.Deployment.Application.Manifest.ManifestSourceFormat.Unknown;
            if (cms == null)
            {
                throw new ArgumentNullException("cms");
            }
            this._cms = cms;
        }

        public AssemblyManifest(FileStream fileStream)
        {
            this._manifestSourceFormat = System.Deployment.Application.Manifest.ManifestSourceFormat.Unknown;
            this.LoadCMSFromStream(fileStream);
            this._rawXmlFilePath = fileStream.Name;
            this._manifestSourceFormat = System.Deployment.Application.Manifest.ManifestSourceFormat.XmlFile;
            this._sizeInBytes = (ulong) fileStream.Length;
        }

        public AssemblyManifest(Stream stream)
        {
            this._manifestSourceFormat = System.Deployment.Application.Manifest.ManifestSourceFormat.Unknown;
            this.LoadCMSFromStream(stream);
            this._manifestSourceFormat = System.Deployment.Application.Manifest.ManifestSourceFormat.Stream;
            this._sizeInBytes = (ulong) stream.Length;
        }

        public AssemblyManifest(string filePath)
        {
            this._manifestSourceFormat = System.Deployment.Application.Manifest.ManifestSourceFormat.Unknown;
            string extension = Path.GetExtension(filePath);
            StringComparison invariantCultureIgnoreCase = StringComparison.InvariantCultureIgnoreCase;
            if (extension.Equals(".application", invariantCultureIgnoreCase) || extension.Equals(".manifest", invariantCultureIgnoreCase))
            {
                this.LoadFromRawXmlFile(filePath);
            }
            else if (extension.Equals(".dll", invariantCultureIgnoreCase) || extension.Equals(".exe", invariantCultureIgnoreCase))
            {
                this.LoadFromInternalManifestFile(filePath);
            }
            else
            {
                this.LoadFromUnknownFormatFile(filePath);
            }
        }

        internal static CertificateStatus AnalyzeManifestCertificate(string manifestPath)
        {
            Logger.AddMethodCall("AnalyzeManifestCertificate called.");
            CertificateStatus unknownCertificateStatus = CertificateStatus.UnknownCertificateStatus;
            System.Deployment.Internal.CodeSigning.SignedCmiManifest manifest = null;
            try
            {
                XmlDocument manifestDom = new XmlDocument {
                    PreserveWhitespace = true
                };
                manifestDom.Load(manifestPath);
                manifest = new System.Deployment.Internal.CodeSigning.SignedCmiManifest(manifestDom);
                manifest.Verify(System.Deployment.Internal.CodeSigning.CmiManifestVerifyFlags.None);
                if ((manifest == null) || (manifest.AuthenticodeSignerInfo == null))
                {
                    unknownCertificateStatus = CertificateStatus.NoCertificate;
                }
                else
                {
                    unknownCertificateStatus = CertificateStatus.TrustedPublisher;
                }
            }
            catch (Exception exception)
            {
                if (ExceptionUtility.IsHardException(exception))
                {
                    throw;
                }
                if ((exception is CryptographicException) && (manifest.AuthenticodeSignerInfo != null))
                {
                    switch (manifest.AuthenticodeSignerInfo.ErrorCode)
                    {
                        case -2146762479:
                            unknownCertificateStatus = CertificateStatus.DistrustedPublisher;
                            goto Label_0094;

                        case -2146885616:
                            unknownCertificateStatus = CertificateStatus.RevokedCertificate;
                            goto Label_0094;

                        case -2146762748:
                            unknownCertificateStatus = CertificateStatus.AuthenticodedNotInTrustedList;
                            goto Label_0094;
                    }
                    unknownCertificateStatus = CertificateStatus.NoCertificate;
                }
            Label_0094:
                Logger.AddInternalState("Exception thrown : " + exception.GetType().ToString() + ":" + exception.Message);
            }
            Logger.AddInternalState("Certificate Status=" + unknownCertificateStatus.ToString());
            return unknownCertificateStatus;
        }

        public ulong CalculateDependenciesSize()
        {
            ulong num = 0L;
            foreach (System.Deployment.Application.Manifest.File file in this.GetFilesInGroup(null, true))
            {
                num += file.Size;
            }
            foreach (DependentAssembly assembly in this.GetPrivateAssembliesInGroup(null, true))
            {
                num += assembly.Size;
            }
            return num;
        }

        private static System.Deployment.Application.DefinitionIdentity ExtractIdentityFromCompLibAssembly(string filePath)
        {
            System.Deployment.Application.DefinitionIdentity definitionIdentityFromManagedAssembly;
            Logger.AddMethodCall("AssemblyManifest.ExtractIdentityFromCompLibAssembly(" + filePath + ") called.");
            try
            {
                using (AssemblyMetaDataImport import = new AssemblyMetaDataImport(filePath))
                {
                    AssemblyName name = import.Name;
                    definitionIdentityFromManagedAssembly = SystemUtils.GetDefinitionIdentityFromManagedAssembly(filePath);
                }
            }
            catch (BadImageFormatException)
            {
                definitionIdentityFromManagedAssembly = null;
            }
            catch (COMException)
            {
                definitionIdentityFromManagedAssembly = null;
            }
            catch (SEHException)
            {
                definitionIdentityFromManagedAssembly = null;
            }
            return definitionIdentityFromManagedAssembly;
        }

        public DependentAssembly GetDependentAssemblyByIdentity(System.Deployment.Internal.Isolation.IReferenceIdentity refid)
        {
            object ppUnknown = null;
            try
            {
                ((System.Deployment.Internal.Isolation.ISectionWithReferenceIdentityKey) this._cms.AssemblyReferenceSection).Lookup(refid, out ppUnknown);
            }
            catch (ArgumentException)
            {
                return null;
            }
            System.Deployment.Internal.Isolation.Manifest.IAssemblyReferenceEntry entry = (System.Deployment.Internal.Isolation.Manifest.IAssemblyReferenceEntry) ppUnknown;
            return new DependentAssembly(entry.AllData);
        }

        public System.Deployment.Application.Manifest.File GetFileFromName(string fileName)
        {
            object ppUnknown = null;
            try
            {
                ((System.Deployment.Internal.Isolation.ISectionWithStringKey) this._cms.FileSection).Lookup(fileName, out ppUnknown);
            }
            catch (ArgumentException)
            {
                return null;
            }
            System.Deployment.Internal.Isolation.Manifest.IFileEntry entry = (System.Deployment.Internal.Isolation.Manifest.IFileEntry) ppUnknown;
            return new System.Deployment.Application.Manifest.File(entry.AllData);
        }

        public System.Deployment.Application.Manifest.File[] GetFilesInGroup(string group, bool optionalOnly)
        {
            StringComparison invariantCultureIgnoreCase = StringComparison.InvariantCultureIgnoreCase;
            ArrayList list = new ArrayList();
            foreach (System.Deployment.Application.Manifest.File file in this.Files)
            {
                if (((group == null) && !file.IsOptional) || (((group != null) && group.Equals(file.Group, invariantCultureIgnoreCase)) && (file.IsOptional || !optionalOnly)))
                {
                    list.Add(file);
                }
            }
            return (System.Deployment.Application.Manifest.File[]) list.ToArray(typeof(System.Deployment.Application.Manifest.File));
        }

        public DependentAssembly[] GetPrivateAssembliesInGroup(string group, bool optionalOnly)
        {
            StringComparison invariantCultureIgnoreCase = StringComparison.InvariantCultureIgnoreCase;
            Hashtable hashtable = new Hashtable();
            foreach (DependentAssembly assembly in this.DependentAssemblies)
            {
                if (!assembly.IsPreRequisite && (((group == null) && !assembly.IsOptional) || (((group != null) && group.Equals(assembly.Group, invariantCultureIgnoreCase)) && (assembly.IsOptional || !optionalOnly))))
                {
                    DependentAssembly assembly2 = null;
                    if (IsResourceReference(assembly))
                    {
                        throw new InvalidDeploymentException(ExceptionTypes.ManifestSemanticValidation, Resources.GetString("Ex_SatelliteResourcesNotSupported"));
                    }
                    assembly2 = assembly;
                    if ((assembly2 != null) && !hashtable.Contains(assembly2.Identity))
                    {
                        hashtable.Add(assembly2.Identity, assembly2);
                    }
                }
            }
            DependentAssembly[] array = new DependentAssembly[hashtable.Count];
            hashtable.Values.CopyTo(array, 0);
            return array;
        }

        private static bool IsInvalidHash(HashCollection hashCollection)
        {
            return !ComponentVerifier.IsVerifiableHashCollection(hashCollection);
        }

        private static bool IsResourceReference(DependentAssembly dependentAssembly)
        {
            return (((dependentAssembly.ResourceFallbackCulture != null) && (dependentAssembly.Identity != null)) && (dependentAssembly.Identity.Culture == null));
        }

        private void LoadCMSFromStream(Stream stream)
        {
            System.Deployment.Internal.Isolation.Manifest.ICMS icms = null;
            int length;
            ManifestParseErrors callback = new ManifestParseErrors();
            try
            {
                length = (int) stream.Length;
                this._rawXmlBytes = new byte[length];
                if (stream.CanSeek)
                {
                    stream.Seek(0L, SeekOrigin.Begin);
                }
                stream.Read(this._rawXmlBytes, 0, length);
            }
            catch (IOException exception)
            {
                throw new InvalidDeploymentException(ExceptionTypes.ManifestParse, Resources.GetString("Ex_ManifestReadException"), exception);
            }
            try
            {
                icms = (System.Deployment.Internal.Isolation.Manifest.ICMS) System.Deployment.Internal.Isolation.IsolationInterop.CreateCMSFromXml(this._rawXmlBytes, (uint) length, callback, ref System.Deployment.Internal.Isolation.IsolationInterop.IID_ICMS);
            }
            catch (COMException exception2)
            {
                StringBuilder builder = new StringBuilder();
                foreach (ManifestParseErrors.ManifestParseError error in callback)
                {
                    builder.AppendFormat(CultureInfo.CurrentUICulture, Resources.GetString("Ex_ManifestParseCMSErrorMessage"), new object[] { error.hr, error.StartLine, error.nStartColumn, error.ErrorStatusHostFile });
                }
                throw new InvalidDeploymentException(ExceptionTypes.ManifestParse, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_ManifestCMSParsingException"), new object[] { builder.ToString() }), exception2);
            }
            catch (SEHException exception3)
            {
                StringBuilder builder2 = new StringBuilder();
                foreach (ManifestParseErrors.ManifestParseError error2 in callback)
                {
                    builder2.AppendFormat(CultureInfo.CurrentUICulture, Resources.GetString("Ex_ManifestParseCMSErrorMessage"), new object[] { error2.hr, error2.StartLine, error2.nStartColumn, error2.ErrorStatusHostFile });
                }
                throw new InvalidDeploymentException(ExceptionTypes.ManifestParse, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_ManifestCMSParsingException"), new object[] { builder2.ToString() }), exception3);
            }
            catch (ArgumentException exception4)
            {
                StringBuilder builder3 = new StringBuilder();
                foreach (ManifestParseErrors.ManifestParseError error3 in callback)
                {
                    builder3.AppendFormat(CultureInfo.CurrentUICulture, Resources.GetString("Ex_ManifestParseCMSErrorMessage"), new object[] { error3.hr, error3.StartLine, error3.nStartColumn, error3.ErrorStatusHostFile });
                }
                throw new InvalidDeploymentException(ExceptionTypes.ManifestParse, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_ManifestCMSParsingException"), new object[] { builder3.ToString() }), exception4);
            }
            if (icms == null)
            {
                throw new InvalidDeploymentException(ExceptionTypes.ManifestParse, Resources.GetString("Ex_IsoNullCmsCreated"));
            }
            this._cms = icms;
        }

        private bool LoadFromCompLibAssembly(string filePath)
        {
            bool flag;
            try
            {
                using (AssemblyMetaDataImport import = new AssemblyMetaDataImport(filePath))
                {
                    AssemblyName name = import.Name;
                    this._identity = SystemUtils.GetDefinitionIdentityFromManagedAssembly(filePath);
                    this._complibIdentity = (System.Deployment.Application.DefinitionIdentity) this.Identity.Clone();
                    AssemblyModule[] files = import.Files;
                    AssemblyReference[] references = import.References;
                    System.Deployment.Application.Manifest.File[] fileArray = new System.Deployment.Application.Manifest.File[files.Length + 1];
                    fileArray[0] = new System.Deployment.Application.Manifest.File(Path.GetFileName(filePath), 0L);
                    for (int i = 0; i < files.Length; i++)
                    {
                        fileArray[i + 1] = new System.Deployment.Application.Manifest.File(files[i].Name, files[i].Hash, 0L);
                    }
                    this._files = fileArray;
                    DependentAssembly[] assemblyArray = new DependentAssembly[references.Length];
                    for (int j = 0; j < references.Length; j++)
                    {
                        assemblyArray[j] = new DependentAssembly(new System.Deployment.Application.ReferenceIdentity(references[j].Name.ToString()));
                    }
                    this._dependentAssemblies = assemblyArray;
                    this._manifestSourceFormat = System.Deployment.Application.Manifest.ManifestSourceFormat.CompLib;
                    flag = true;
                }
            }
            catch (BadImageFormatException)
            {
                flag = false;
            }
            catch (COMException)
            {
                flag = false;
            }
            catch (SEHException)
            {
                flag = false;
            }
            catch (IOException)
            {
                flag = false;
            }
            return flag;
        }

        private void LoadFromInternalManifestFile(string filePath)
        {
            byte[] buffer = null;
            PEStream stream = null;
            MemoryStream stream2 = null;
            AssemblyManifest manifest = null;
            bool isImageFileDll = true;
            try
            {
                stream = new PEStream(filePath, true);
                buffer = stream.GetDefaultId1ManifestResource();
                if (buffer != null)
                {
                    stream2 = new MemoryStream(buffer);
                    manifest = new AssemblyManifest(stream2);
                    Logger.AddInternalState("id1Manifest is parsed successfully.");
                    this._id1ManifestPresent = true;
                }
                isImageFileDll = stream.IsImageFileDll;
            }
            catch (IOException exception)
            {
                ManifestLoadExceptionHelper(exception, filePath);
            }
            catch (Win32Exception exception2)
            {
                ManifestLoadExceptionHelper(exception2, filePath);
            }
            catch (InvalidDeploymentException exception3)
            {
                ManifestLoadExceptionHelper(exception3, filePath);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
                if (stream2 != null)
                {
                    stream2.Close();
                }
            }
            if (manifest != null)
            {
                if (!manifest.Identity.IsEmpty)
                {
                    if (!this.LoadFromPEResources(filePath))
                    {
                        ManifestLoadExceptionHelper(new DeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_CannotLoadInternalManifest")), filePath);
                    }
                    this._complibIdentity = ExtractIdentityFromCompLibAssembly(filePath);
                    Logger.AddInternalState("_complibIdentity =" + ((this._complibIdentity == null) ? "null" : this._complibIdentity.ToString()));
                }
                else if (!isImageFileDll)
                {
                    if (!this.LoadFromCompLibAssembly(filePath))
                    {
                        ManifestLoadExceptionHelper(new DeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_CannotLoadInternalManifest")), filePath);
                    }
                    this._id1Identity = manifest.Identity;
                    this._id1RequestedExecutionLevel = manifest.RequestedExecutionLevel;
                }
                else
                {
                    ManifestLoadExceptionHelper(new DeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_EmptyIdentityInternalManifest")), filePath);
                }
            }
            else if (!this.LoadFromCompLibAssembly(filePath))
            {
                ManifestLoadExceptionHelper(new DeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_CannotLoadInternalManifest")), filePath);
            }
        }

        private bool LoadFromPEResources(string filePath)
        {
            byte[] manifestFromPEResources = null;
            try
            {
                manifestFromPEResources = SystemUtils.GetManifestFromPEResources(filePath);
            }
            catch (Win32Exception exception)
            {
                ManifestLoadExceptionHelper(exception, filePath);
            }
            if (manifestFromPEResources != null)
            {
                using (MemoryStream stream = new MemoryStream(manifestFromPEResources))
                {
                    this.LoadCMSFromStream(stream);
                }
                this._id1Identity = (System.Deployment.Application.DefinitionIdentity) this.Identity.Clone();
                this._id1RequestedExecutionLevel = this.RequestedExecutionLevel;
                Logger.AddInternalState("_id1Identity = " + ((this._id1Identity == null) ? "null" : this._id1Identity.ToString()));
                this._manifestSourceFormat = System.Deployment.Application.Manifest.ManifestSourceFormat.ID_1;
                return true;
            }
            Logger.AddInternalState("File does not contain ID_1 manifest.");
            return false;
        }

        private void LoadFromRawXmlFile(string filePath)
        {
            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                this.LoadCMSFromStream(stream);
                this._rawXmlFilePath = filePath;
                this._manifestSourceFormat = System.Deployment.Application.Manifest.ManifestSourceFormat.XmlFile;
                this._sizeInBytes = (ulong) stream.Length;
            }
        }

        private void LoadFromUnknownFormatFile(string filePath)
        {
            try
            {
                this.LoadFromRawXmlFile(filePath);
            }
            catch (InvalidDeploymentException exception)
            {
                if ((exception.SubType != ExceptionTypes.ManifestParse) && (exception.SubType != ExceptionTypes.ManifestSemanticValidation))
                {
                    throw;
                }
                this.LoadFromInternalManifestFile(filePath);
            }
        }

        private static void ManifestLoadExceptionHelper(Exception exception, string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            string message = string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_ManifestLoadFromFile"), new object[] { fileName });
            throw new InvalidDeploymentException(ExceptionTypes.ManifestLoad, message, exception);
        }

        internal static void ReValidateManifestSignatures(AssemblyManifest depManifest, AssemblyManifest appManifest)
        {
            if (depManifest.Signed && !appManifest.Signed)
            {
                throw new InvalidDeploymentException(ExceptionTypes.SignatureValidation, Resources.GetString("Ex_DepSignedAppUnsigned"));
            }
            if (!depManifest.Signed && appManifest.Signed)
            {
                throw new InvalidDeploymentException(ExceptionTypes.SignatureValidation, Resources.GetString("Ex_AppSignedDepUnsigned"));
            }
        }

        internal static Uri UriFromMetadataEntry(string uriString, string exResourceStr)
        {
            Uri uri;
            try
            {
                uri = (uriString != null) ? new Uri(uriString) : null;
            }
            catch (UriFormatException exception)
            {
                throw new InvalidDeploymentException(ExceptionTypes.Manifest, string.Format(CultureInfo.CurrentUICulture, Resources.GetString(exResourceStr), new object[] { uriString }), exception);
            }
            return uri;
        }

        private void ValidateApplicationDependency(DependentAssembly da)
        {
            ValidateAssemblyIdentity(da.Identity);
            if (da.Identity.PublicKeyToken == null)
            {
                throw new InvalidDeploymentException(ExceptionTypes.ManifestComponentSemanticValidation, Resources.GetString("Ex_DepAppRefNotStrongNamed"));
            }
            if (IsInvalidHash(da.HashCollection))
            {
                throw new InvalidDeploymentException(ExceptionTypes.ManifestComponentSemanticValidation, Resources.GetString("Ex_DepAppRefHashInvalid"));
            }
            if (string.Compare(this.Identity.ProcessorArchitecture, da.Identity.ProcessorArchitecture, StringComparison.OrdinalIgnoreCase) != 0)
            {
                throw new InvalidDeploymentException(ExceptionTypes.ManifestComponentSemanticValidation, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_DepAppRefProcArchMismatched"), new object[] { da.Identity.ProcessorArchitecture, this.Identity.ProcessorArchitecture }));
            }
            if (((da.ResourceFallbackCulture != null) || da.IsPreRequisite) || da.IsOptional)
            {
                throw new InvalidDeploymentException(ExceptionTypes.ManifestComponentSemanticValidation, Resources.GetString("Ex_DepAppRefPrereqOrOptionalOrResourceFallback"));
            }
            Uri uri = null;
            try
            {
                uri = new Uri(da.Codebase, UriKind.RelativeOrAbsolute);
            }
            catch (UriFormatException exception)
            {
                throw new InvalidDeploymentException(ExceptionTypes.ManifestComponentSemanticValidation, Resources.GetString("Ex_DepAppRefInvalidCodebaseUri"), exception);
            }
            if (uri.IsAbsoluteUri && !UriHelper.IsSupportedScheme(uri))
            {
                throw new InvalidDeploymentException(ExceptionTypes.ManifestComponentSemanticValidation, Resources.GetString("Ex_DepAppRefInvalidCodebaseUri"));
            }
            if (!UriHelper.IsValidRelativeFilePath(da.Identity.Name) || UriHelper.PathContainDirectorySeparators(da.Identity.Name))
            {
                throw new InvalidDeploymentException(ExceptionTypes.ManifestComponentSemanticValidation, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_DepAppRefInvalidIdentityName"), new object[] { da.Identity.Name }));
            }
        }

        private static void ValidateAssemblyIdentity(System.Deployment.Application.DefinitionIdentity identity)
        {
            if ((identity.Name != null) && (((identity.Name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) || (identity.Name.IndexOfAny(Path.GetInvalidPathChars()) >= 0)) || (identity.Name.IndexOfAny(SpecificInvalidIdentityChars) >= 0)))
            {
                Logger.AddInternalState(identity.Name + " contains an invalid character. InvalidIdentityChars=[" + (new string(Path.GetInvalidFileNameChars()) + " " + new string(Path.GetInvalidPathChars()) + " " + new string(SpecificInvalidIdentityChars)) + "].");
                throw new InvalidDeploymentException(ExceptionTypes.ManifestComponentSemanticValidation, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_IdentityWithInvalidChars"), new object[] { identity.Name }));
            }
            try
            {
                if (identity.ToString().Length > 0x800)
                {
                    throw new InvalidDeploymentException(ExceptionTypes.ManifestComponentSemanticValidation, Resources.GetString("Ex_IdentityTooLong"));
                }
            }
            catch (COMException exception)
            {
                throw new InvalidDeploymentException(ExceptionTypes.ManifestComponentSemanticValidation, Resources.GetString("Ex_IdentityIsNotValid"), exception);
            }
            catch (SEHException exception2)
            {
                throw new InvalidDeploymentException(ExceptionTypes.ManifestComponentSemanticValidation, Resources.GetString("Ex_IdentityIsNotValid"), exception2);
            }
        }

        private static void ValidateAssemblyIdentity(System.Deployment.Application.ReferenceIdentity identity)
        {
            if ((identity.Name != null) && (((identity.Name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) || (identity.Name.IndexOfAny(Path.GetInvalidPathChars()) >= 0)) || (identity.Name.IndexOfAny(SpecificInvalidIdentityChars) >= 0)))
            {
                Logger.AddInternalState(identity.Name + " contains an invalid character. InvalidIdentityChars= [" + (new string(Path.GetInvalidFileNameChars()) + " " + new string(Path.GetInvalidPathChars()) + " " + new string(SpecificInvalidIdentityChars)) + "].");
                throw new InvalidDeploymentException(ExceptionTypes.ManifestComponentSemanticValidation, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_IdentityWithInvalidChars"), new object[] { identity.Name }));
            }
            try
            {
                if (identity.ToString().Length > 0x800)
                {
                    throw new InvalidDeploymentException(ExceptionTypes.ManifestComponentSemanticValidation, Resources.GetString("Ex_IdentityTooLong"));
                }
            }
            catch (COMException exception)
            {
                throw new InvalidDeploymentException(ExceptionTypes.ManifestComponentSemanticValidation, Resources.GetString("Ex_IdentityIsNotValid"), exception);
            }
            catch (SEHException exception2)
            {
                throw new InvalidDeploymentException(ExceptionTypes.ManifestComponentSemanticValidation, Resources.GetString("Ex_IdentityIsNotValid"), exception2);
            }
        }

        private static void ValidateComponentDependency(DependentAssembly da)
        {
            ValidateAssemblyIdentity(da.Identity);
            if (!da.IsPreRequisite)
            {
                if (da.ResourceFallbackCulture != null)
                {
                    if (da.Identity.Culture != null)
                    {
                        if (IsInvalidHash(da.HashCollection))
                        {
                            throw new InvalidDeploymentException(ExceptionTypes.ManifestComponentSemanticValidation, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_DependencyInvalidHash"), new object[] { da.Identity.ToString() }));
                        }
                        if (da.Codebase == null)
                        {
                            throw new InvalidDeploymentException(ExceptionTypes.ManifestComponentSemanticValidation, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_DependencyNoCodebase"), new object[] { da.Identity.ToString() }));
                        }
                        if (!UriHelper.IsValidRelativeFilePath(da.Codebase))
                        {
                            throw new InvalidDeploymentException(ExceptionTypes.ManifestComponentSemanticValidation, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_DependencyNotRelativePath"), new object[] { da.Identity.ToString() }));
                        }
                        if (da.ResourceFallbackCulture != null)
                        {
                            throw new InvalidDeploymentException(ExceptionTypes.ManifestComponentSemanticValidation, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_DependencyResourceWithFallback"), new object[] { da.Identity.ToString() }));
                        }
                    }
                    else
                    {
                        if (da.Codebase != null)
                        {
                            throw new InvalidDeploymentException(ExceptionTypes.ManifestComponentSemanticValidation, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_DependencyResourceWithCodebase"), new object[] { da.Identity.ToString() }));
                        }
                        if (da.HashCollection.Count > 0)
                        {
                            throw new InvalidDeploymentException(ExceptionTypes.ManifestComponentSemanticValidation, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_DependencyResourceWithHash"), new object[] { da.Identity.ToString() }));
                        }
                    }
                }
                else
                {
                    if (IsInvalidHash(da.HashCollection))
                    {
                        throw new InvalidDeploymentException(ExceptionTypes.ManifestComponentSemanticValidation, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_DependencyInvalidHash"), new object[] { da.Identity.ToString() }));
                    }
                    if (da.Codebase == null)
                    {
                        throw new InvalidDeploymentException(ExceptionTypes.ManifestComponentSemanticValidation, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_DependencyNoCodebase"), new object[] { da.Identity.ToString() }));
                    }
                    if (!UriHelper.IsValidRelativeFilePath(da.Codebase))
                    {
                        throw new InvalidDeploymentException(ExceptionTypes.ManifestComponentSemanticValidation, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_DependencyNotRelativePath"), new object[] { da.Identity.ToString() }));
                    }
                    if (da.IsOptional && (da.Group == null))
                    {
                        throw new InvalidDeploymentException(ExceptionTypes.ManifestComponentSemanticValidation, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_DependencyOptionalButNoGroup"), new object[] { da.Identity.ToString() }));
                    }
                }
            }
            else if (!PlatformDetector.IsCLRDependencyText(da.Identity.Name) && (da.Identity.PublicKeyToken == null))
            {
                throw new InvalidDeploymentException(ExceptionTypes.ManifestComponentSemanticValidation, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_DependencyGACNoPKT"), new object[] { da.Identity.ToString() }));
            }
            if (da.SupportUrl != null)
            {
                if (!da.SupportUrl.IsAbsoluteUri)
                {
                    throw new InvalidDeploymentException(ExceptionTypes.ManifestComponentSemanticValidation, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_DependencySupportUrlNoAbsolute"), new object[] { da.Identity.ToString() }));
                }
                if (!UriHelper.IsSupportedScheme(da.SupportUrl))
                {
                    throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_DependencySupportUrlNotSupportedUriScheme"), new object[] { da.Identity.ToString() }));
                }
                if (da.SupportUrl.AbsoluteUri.Length > 0x4000)
                {
                    throw new InvalidDeploymentException(ExceptionTypes.ManifestComponentSemanticValidation, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_DependencySupportUrlTooLong"), new object[] { da.Identity.ToString() }));
                }
            }
        }

        private static void ValidateFile(System.Deployment.Application.Manifest.File f)
        {
            if (IsInvalidHash(f.HashCollection))
            {
                throw new InvalidDeploymentException(ExceptionTypes.ManifestComponentSemanticValidation, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_InvalidFileHash"), new object[] { f.Name }));
            }
            if (!UriHelper.IsValidRelativeFilePath(f.Name))
            {
                throw new InvalidDeploymentException(ExceptionTypes.ManifestComponentSemanticValidation, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_FilePathNotRelative"), new object[] { f.Name }));
            }
            if (f.IsOptional && (f.Group == null))
            {
                throw new InvalidDeploymentException(ExceptionTypes.ManifestComponentSemanticValidation, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_FileOptionalButNoGroup"), new object[] { f.Name }));
            }
            if (f.IsOptional && f.IsData)
            {
                throw new InvalidDeploymentException(ExceptionTypes.ManifestComponentSemanticValidation, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_FileOptionalAndData"), new object[] { f.Name }));
            }
        }

        public void ValidateSemantics(ManifestType manifestType)
        {
            switch (manifestType)
            {
                case ManifestType.Application:
                    this.ValidateSemanticsForApplicationRole();
                    return;

                case ManifestType.Deployment:
                    this.ValidateSemanticsForDeploymentRole();
                    return;
            }
        }

        internal void ValidateSemanticsForApplicationRole()
        {
            try
            {
                ValidateAssemblyIdentity(this.Identity);
                if (this.EntryPoints.Length != 1)
                {
                    throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_AppOneEntryPoint"));
                }
                EntryPoint point = this.EntryPoints[0];
                if (!point.CustomHostSpecified && ((((point.Assembly == null) || point.Assembly.IsOptional) || (point.Assembly.IsPreRequisite || (point.Assembly.Codebase == null))) || (((!UriHelper.IsValidRelativeFilePath(point.Assembly.Codebase) || UriHelper.PathContainDirectorySeparators(point.Assembly.Codebase)) || (!UriHelper.IsValidRelativeFilePath(point.CommandFile) || UriHelper.PathContainDirectorySeparators(point.CommandFile))) || (!point.CommandFile.Equals(point.Assembly.Codebase, StringComparison.OrdinalIgnoreCase) || (string.Compare(this.Identity.ProcessorArchitecture, point.Assembly.Identity.ProcessorArchitecture, StringComparison.OrdinalIgnoreCase) != 0)))))
                {
                    throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_AppInvalidEntryPoint"));
                }
                if (this.Application && (point.CommandParameters != null))
                {
                    throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_AppInvalidEntryPointParameters"));
                }
                if ((this.DependentAssemblies == null) || (this.DependentAssemblies.Length == 0))
                {
                    throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_AppAtLeastOneDependency"));
                }
                if (this.Deployment != null)
                {
                    throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_AppNoDeploymentAllowed"));
                }
                if (this.CompatibleFrameworks != null)
                {
                    throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_AppNoCompatibleFrameworksAllowed"));
                }
                if (this.UseManifestForTrust)
                {
                    if ((this.Description == null) || ((this.Description != null) && ((this.Description.Publisher == null) || (this.Description.Product == null))))
                    {
                        throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_AppNoOverridePublisherProduct"));
                    }
                }
                else if ((this.Description != null) && ((this.Description.Publisher != null) || (this.Description.Product != null)))
                {
                    throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_AppNoPublisherProductAllowed"));
                }
                if (((this.Description != null) && (this.Description.IconFile != null)) && !UriHelper.IsValidRelativeFilePath(this.Description.IconFile))
                {
                    throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_AppInvalidIconFile"));
                }
                if ((this.Description != null) && (this.Description.SupportUri != null))
                {
                    if (!this.Description.SupportUri.IsAbsoluteUri)
                    {
                        throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_DescriptionSupportUrlNotAbsolute"));
                    }
                    if (!UriHelper.IsSupportedScheme(this.Description.SupportUri))
                    {
                        throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_DescriptionSupportUrlNotSupportedUriScheme"));
                    }
                    if (this.Description.SupportUri.AbsoluteUri.Length > 0x4000)
                    {
                        throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_DescriptionSupportUrlTooLong"));
                    }
                }
                if (this.Files.Length > 0x6000)
                {
                    throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_TooManyFilesInManifest"));
                }
                Hashtable hashtable = new Hashtable();
                foreach (System.Deployment.Application.Manifest.File file in this.Files)
                {
                    ValidateFile(file);
                    if (!file.IsOptional && !hashtable.Contains(file.Name))
                    {
                        hashtable.Add(file.Name.ToLower(), file);
                    }
                    if (file.HashCollection.Count == 0)
                    {
                        this._unhashedFilePresent = true;
                    }
                }
                if ((this.FileAssociations.Length > 0) && point.HostInBrowser)
                {
                    throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_FileAssociationNotSupportedForHostInBrowser"));
                }
                if ((this.FileAssociations.Length > 0) && point.CustomHostSpecified)
                {
                    throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_FileAssociationNotSupportedForCustomHost"));
                }
                if (this.FileAssociations.Length > 8)
                {
                    throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_TooManyFileAssociationsInManifest"), new object[] { 8 }));
                }
                Hashtable hashtable2 = new Hashtable();
                foreach (FileAssociation association in this.FileAssociations)
                {
                    if ((string.IsNullOrEmpty(association.Extension) || string.IsNullOrEmpty(association.Description)) || (string.IsNullOrEmpty(association.ProgID) || string.IsNullOrEmpty(association.DefaultIcon)))
                    {
                        throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_FileExtensionInfoMissing"));
                    }
                    if (association.Extension.Length > 0)
                    {
                        char ch = association.Extension[0];
                        if (ch != '.')
                        {
                            throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_FileAssociationExtensionNoDot"), new object[] { association.Extension }));
                        }
                    }
                    if (!UriHelper.IsValidRelativeFilePath("file" + association.Extension))
                    {
                        throw new InvalidDeploymentException(ExceptionTypes.ManifestComponentSemanticValidation, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_FileAssociationInvalid"), new object[] { association.Extension }));
                    }
                    if (association.Extension.Length > 0x18)
                    {
                        throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_FileExtensionTooLong"), new object[] { association.Extension }));
                    }
                    if (!hashtable.Contains(association.DefaultIcon.ToLower()))
                    {
                        throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_FileAssociationIconFileNotFound"), new object[] { association.DefaultIcon }));
                    }
                    if (hashtable2.Contains(association.Extension))
                    {
                        throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_MultipleInstanceFileExtension"), new object[] { association.Extension }));
                    }
                    hashtable2.Add(association.Extension, association);
                }
                if (this.DependentAssemblies.Length > 0x6000)
                {
                    throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_TooManyAssembliesInManifest"));
                }
                bool flag = false;
                foreach (DependentAssembly assembly in this.DependentAssemblies)
                {
                    ValidateComponentDependency(assembly);
                    if (assembly.IsPreRequisite && PlatformDetector.IsCLRDependencyText(assembly.Identity.Name))
                    {
                        flag = true;
                        this._clrDependentAssembly = assembly;
                    }
                    if (!assembly.IsPreRequisite && (assembly.HashCollection.Count == 0))
                    {
                        this._unhashedDependencyPresent = true;
                    }
                }
                this._clrDependentAssemblyChecked = true;
                if ((this.DependentOS != null) && (this.DependentOS.SupportUrl != null))
                {
                    if (!this.DependentOS.SupportUrl.IsAbsoluteUri)
                    {
                        throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_DepenedentOSSupportUrlNotAbsolute"));
                    }
                    if (!UriHelper.IsSupportedScheme(this.DependentOS.SupportUrl))
                    {
                        throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_DepenedentOSSupportUrlNotSupportedUriScheme"));
                    }
                    if (this.DependentOS.SupportUrl.AbsoluteUri.Length > 0x4000)
                    {
                        throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_DepenedentOSSupportUrlTooLong"));
                    }
                }
                if (!flag)
                {
                    throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_AppNoCLRDependency"), new object[0]));
                }
            }
            catch (InvalidDeploymentException exception)
            {
                throw new InvalidDeploymentException(ExceptionTypes.ManifestSemanticValidation, Resources.GetString("Ex_SemanticallyInvalidApplicationManifest"), exception);
            }
        }

        internal void ValidateSemanticsForDeploymentRole()
        {
            try
            {
                ValidateAssemblyIdentity(this.Identity);
                if (this.Identity.PublicKeyToken == null)
                {
                    throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_DepNotStronglyNamed"));
                }
                if (!PlatformDetector.IsSupportedProcessorArchitecture(this.Identity.ProcessorArchitecture))
                {
                    throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_DepProcArchNotSupported"));
                }
                if (this.Deployment == null)
                {
                    throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_DepMissingDeploymentSection"));
                }
                if (this.UseManifestForTrust)
                {
                    throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_DepWithUseManifestForTrust"));
                }
                if (((this.Description == null) || string.IsNullOrEmpty(this.Description.FilteredPublisher)) || string.IsNullOrEmpty(this.Description.FilteredProduct))
                {
                    throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_DepPublisherProductRequired"));
                }
                if ((this.Description.FilteredPublisher.Length + this.Description.FilteredProduct.Length) > 260)
                {
                    throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_PublisherProductNameTooLong"));
                }
                if (this.EntryPoints.Length != 0)
                {
                    throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_DepEntryPointNotAllowed"));
                }
                if (this.Files.Length != 0)
                {
                    throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_DepFileNotAllowed"));
                }
                if (this.FileAssociations.Length > 0)
                {
                    throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_DepFileAssocNotAllowed"));
                }
                if (this.Description.IconFile != null)
                {
                    throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_DepIconFileNotAllowed"));
                }
                if (this.Deployment.DisallowUrlActivation && !this.Deployment.Install)
                {
                    throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_DepOnlineOnlyAndDisallowUrlActivation"));
                }
                if (this.Deployment.DisallowUrlActivation && this.Deployment.TrustURLParameters)
                {
                    throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_DepTrustUrlAndDisallowUrlActivation"));
                }
                if (this.CompatibleFrameworks != null)
                {
                    if (this.CompatibleFrameworks.SupportUrl != null)
                    {
                        if (!this.CompatibleFrameworks.SupportUrl.IsAbsoluteUri)
                        {
                            throw new InvalidDeploymentException(ExceptionTypes.ManifestComponentSemanticValidation, Resources.GetString("Ex_CompatibleFrameworksSupportUrlNoAbsolute"));
                        }
                        if (!UriHelper.IsSupportedScheme(this.CompatibleFrameworks.SupportUrl))
                        {
                            throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_CompatibleFrameworksSupportUrlNotSupportedUriScheme"));
                        }
                        if (this.CompatibleFrameworks.SupportUrl.AbsoluteUri.Length > 0x4000)
                        {
                            throw new InvalidDeploymentException(ExceptionTypes.ManifestComponentSemanticValidation, Resources.GetString("Ex_CompatibleFrameworksSupportUrlTooLong"));
                        }
                    }
                    if (this.CompatibleFrameworks.Frameworks.Count < 1)
                    {
                        throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_DepAtLeastOneFramework"));
                    }
                    for (int i = 0; i < this.CompatibleFrameworks.Frameworks.Count; i++)
                    {
                        CompatibleFramework framework = this.CompatibleFrameworks.Frameworks[i];
                        Version version = null;
                        try
                        {
                            version = new Version(framework.TargetVersion);
                        }
                        catch (SystemException exception)
                        {
                            if (ExceptionUtility.IsHardException(exception))
                            {
                                throw;
                            }
                            throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_DepInvalidTargetVersion"), new object[] { framework.TargetVersion }), exception);
                        }
                        try
                        {
                            new Version(framework.SupportedRuntime);
                        }
                        catch (SystemException exception2)
                        {
                            if (ExceptionUtility.IsHardException(exception2))
                            {
                                throw;
                            }
                            throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_DepInvalidSupportedRuntime"), new object[] { framework.SupportedRuntime }), exception2);
                        }
                        switch (version.Major)
                        {
                            case 2:
                            {
                                if (!string.IsNullOrEmpty(framework.Profile))
                                {
                                    throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_DepUnsupportedFrameworkProfile"), new object[] { framework.Profile, framework.TargetVersion }));
                                }
                                continue;
                            }
                            case 3:
                                if ((version.Minor >= 1) && (version.Minor < 5))
                                {
                                    throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_DepUnsupportedFrameworkTargetVersion"), new object[] { framework.TargetVersion }));
                                }
                                break;

                            default:
                                goto Label_0444;
                        }
                        if (string.IsNullOrEmpty(framework.Profile) || ((version.Minor >= 5) && "Client".Equals(framework.Profile, StringComparison.OrdinalIgnoreCase)))
                        {
                            continue;
                        }
                        throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_DepUnsupportedFrameworkProfile"), new object[] { framework.Profile, framework.TargetVersion }));
                    Label_0444:
                        if (version.Major <= 1)
                        {
                            throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_DepUnsupportedFrameworkTargetVersion"), new object[] { framework.TargetVersion }));
                        }
                        if (string.IsNullOrEmpty(framework.Profile))
                        {
                            throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_DepFrameworkProfileRequired"), new object[] { framework.TargetVersion }));
                        }
                    }
                }
                if (this.Deployment.Install)
                {
                    if (this.Deployment.ProviderCodebaseUri != null)
                    {
                        if (!this.Deployment.ProviderCodebaseUri.IsAbsoluteUri)
                        {
                            throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_DepProviderNotAbsolute"));
                        }
                        if (!UriHelper.IsSupportedScheme(this.Deployment.ProviderCodebaseUri))
                        {
                            throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_DepProviderNotSupportedUriScheme"));
                        }
                        if (this.Deployment.ProviderCodebaseUri.AbsoluteUri.Length > 0x4000)
                        {
                            throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_DepProviderTooLong"));
                        }
                    }
                    if ((this.Deployment.MinimumRequiredVersion != null) && (this.Deployment.MinimumRequiredVersion > this.Identity.Version))
                    {
                        throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_MinimumRequiredVersionExceedDeployment"));
                    }
                }
                else if (this.Deployment.MinimumRequiredVersion != null)
                {
                    throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_DepNoMinVerForOnlineApps"));
                }
                if (this.DependentAssemblies.Length != 1)
                {
                    throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_DepApplicationDependencyRequired"));
                }
                this.ValidateApplicationDependency(this.DependentAssemblies[0]);
                if (this.DependentAssemblies[0].HashCollection.Count == 0)
                {
                    this._unhashedDependencyPresent = true;
                }
                if (this.Deployment.DeploymentUpdate.BeforeApplicationStartup && this.Deployment.DeploymentUpdate.MaximumAgeSpecified)
                {
                    throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_DepBeforeStartupMaxAgeBothPresent"));
                }
                if (this.Deployment.DeploymentUpdate.MaximumAgeSpecified && (this.Deployment.DeploymentUpdate.MaximumAgeAllowed > TimeSpan.FromDays(365.0)))
                {
                    throw new InvalidDeploymentException(ExceptionTypes.InvalidManifest, Resources.GetString("Ex_MaxAgeTooLarge"));
                }
            }
            catch (InvalidDeploymentException exception3)
            {
                throw new InvalidDeploymentException(ExceptionTypes.ManifestSemanticValidation, Resources.GetString("Ex_SemanticallyInvalidDeploymentManifest"), exception3);
            }
        }

        internal void ValidateSignature(Stream s)
        {
            if (string.Equals(this.Identity.PublicKeyToken, "0000000000000000", StringComparison.Ordinal) && !PolicyKeys.RequireSignedManifests())
            {
                Logger.AddWarningInformation(string.Format(CultureInfo.CurrentUICulture, Resources.GetString("UnsignedManifest"), new object[0]));
                Logger.AddInternalState("Manifest is unsigned.");
                this._signed = false;
            }
            else
            {
                XmlDocument manifestDom = new XmlDocument {
                    PreserveWhitespace = true
                };
                if (s != null)
                {
                    manifestDom.Load(s);
                }
                else
                {
                    manifestDom.Load(this._rawXmlFilePath);
                }
                try
                {
                    new System.Deployment.Internal.CodeSigning.SignedCmiManifest(manifestDom).Verify(System.Deployment.Internal.CodeSigning.CmiManifestVerifyFlags.StrongNameOnly);
                }
                catch (CryptographicException exception)
                {
                    throw new InvalidDeploymentException(ExceptionTypes.SignatureValidation, Resources.GetString("Ex_InvalidXmlSignature"), exception);
                }
                if (this.RequiredHashMissing)
                {
                    throw new InvalidDeploymentException(ExceptionTypes.SignatureValidation, Resources.GetString("Ex_SignedManifestUnhashedComponent"));
                }
                this._signed = true;
            }
        }

        public bool Application
        {
            get
            {
                return ((this.ManifestFlags & 4) != 0);
            }
        }

        public DependentAssembly CLRDependentAssembly
        {
            get
            {
                if (!this._clrDependentAssemblyChecked)
                {
                    foreach (DependentAssembly assembly in this.DependentAssemblies)
                    {
                        if (assembly.IsPreRequisite && PlatformDetector.IsCLRDependencyText(assembly.Identity.Name))
                        {
                            this._clrDependentAssembly = assembly;
                        }
                    }
                    this._clrDependentAssemblyChecked = true;
                }
                return this._clrDependentAssembly;
            }
        }

        public System.Deployment.Application.CompatibleFrameworks CompatibleFrameworks
        {
            get
            {
                if ((this._compatibleFrameworks == null) && (this._cms != null))
                {
                    System.Deployment.Internal.Isolation.Manifest.IMetadataSectionEntry metadataSectionEntry = (System.Deployment.Internal.Isolation.Manifest.IMetadataSectionEntry) this._cms.MetadataSectionEntry;
                    System.Deployment.Internal.Isolation.Manifest.ICompatibleFrameworksMetadataEntry compatibleFrameworksData = metadataSectionEntry.CompatibleFrameworksData;
                    if (compatibleFrameworksData != null)
                    {
                        System.Deployment.Internal.Isolation.ISection section = (this._cms != null) ? this._cms.CompatibleFrameworksSection : null;
                        uint celt = (section != null) ? section.Count : 0;
                        CompatibleFramework[] frameworkArray = new CompatibleFramework[celt];
                        if (celt > 0)
                        {
                            uint celtFetched = 0;
                            System.Deployment.Internal.Isolation.Manifest.ICompatibleFrameworkEntry[] rgelt = new System.Deployment.Internal.Isolation.Manifest.ICompatibleFrameworkEntry[celt];
                            System.Deployment.Internal.Isolation.IEnumUnknown unknown = (System.Deployment.Internal.Isolation.IEnumUnknown) section._NewEnum;
                            Marshal.ThrowExceptionForHR(unknown.Next(celt, rgelt, ref celtFetched));
                            if (celtFetched != celt)
                            {
                                throw new InvalidDeploymentException(ExceptionTypes.Manifest, Resources.GetString("Ex_IsoEnumFetchNotEqualToCount"));
                            }
                            for (uint i = 0; i < celt; i++)
                            {
                                frameworkArray[i] = new CompatibleFramework(rgelt[i].AllData);
                            }
                        }
                        System.Deployment.Application.CompatibleFrameworks frameworks = new System.Deployment.Application.CompatibleFrameworks(compatibleFrameworksData.AllData, frameworkArray);
                        Interlocked.CompareExchange(ref this._compatibleFrameworks, frameworks, null);
                    }
                }
                return (System.Deployment.Application.CompatibleFrameworks) this._compatibleFrameworks;
            }
        }

        public System.Deployment.Application.DefinitionIdentity ComplibIdentity
        {
            get
            {
                return this._complibIdentity;
            }
        }

        public DependentAssembly[] DependentAssemblies
        {
            get
            {
                if (this._dependentAssemblies == null)
                {
                    System.Deployment.Internal.Isolation.ISection section = (this._cms != null) ? this._cms.AssemblyReferenceSection : null;
                    uint celt = (section != null) ? section.Count : 0;
                    DependentAssembly[] assemblyArray = new DependentAssembly[celt];
                    if (celt > 0)
                    {
                        uint celtFetched = 0;
                        System.Deployment.Internal.Isolation.Manifest.IAssemblyReferenceEntry[] rgelt = new System.Deployment.Internal.Isolation.Manifest.IAssemblyReferenceEntry[celt];
                        System.Deployment.Internal.Isolation.IEnumUnknown unknown = (System.Deployment.Internal.Isolation.IEnumUnknown) section._NewEnum;
                        Marshal.ThrowExceptionForHR(unknown.Next(celt, rgelt, ref celtFetched));
                        if (celtFetched != celt)
                        {
                            throw new InvalidDeploymentException(ExceptionTypes.Manifest, Resources.GetString("Ex_IsoEnumFetchNotEqualToCount"));
                        }
                        for (uint i = 0; i < celt; i++)
                        {
                            assemblyArray[i] = new DependentAssembly(rgelt[i].AllData);
                        }
                    }
                    Interlocked.CompareExchange(ref this._dependentAssemblies, assemblyArray, null);
                }
                return (DependentAssembly[]) this._dependentAssemblies;
            }
        }

        public System.Deployment.Application.Manifest.DependentOS DependentOS
        {
            get
            {
                if ((this._dependentOS == null) && (this._cms != null))
                {
                    System.Deployment.Internal.Isolation.Manifest.IMetadataSectionEntry metadataSectionEntry = (System.Deployment.Internal.Isolation.Manifest.IMetadataSectionEntry) this._cms.MetadataSectionEntry;
                    System.Deployment.Internal.Isolation.Manifest.IDependentOSMetadataEntry dependentOSData = metadataSectionEntry.DependentOSData;
                    if (dependentOSData != null)
                    {
                        System.Deployment.Application.Manifest.DependentOS tos = new System.Deployment.Application.Manifest.DependentOS(dependentOSData.AllData);
                        Interlocked.CompareExchange(ref this._dependentOS, tos, null);
                    }
                }
                return (System.Deployment.Application.Manifest.DependentOS) this._dependentOS;
            }
        }

        public System.Deployment.Application.Manifest.Deployment Deployment
        {
            get
            {
                if ((this._deployment == null) && (this._cms != null))
                {
                    System.Deployment.Internal.Isolation.Manifest.IMetadataSectionEntry metadataSectionEntry = (System.Deployment.Internal.Isolation.Manifest.IMetadataSectionEntry) this._cms.MetadataSectionEntry;
                    System.Deployment.Internal.Isolation.Manifest.IDeploymentMetadataEntry deploymentData = metadataSectionEntry.DeploymentData;
                    if (deploymentData != null)
                    {
                        System.Deployment.Application.Manifest.Deployment deployment = new System.Deployment.Application.Manifest.Deployment(deploymentData.AllData);
                        Interlocked.CompareExchange(ref this._deployment, deployment, null);
                    }
                }
                return (System.Deployment.Application.Manifest.Deployment) this._deployment;
            }
        }

        public System.Deployment.Application.Manifest.Description Description
        {
            get
            {
                if ((this._description == null) && (this._cms != null))
                {
                    System.Deployment.Internal.Isolation.Manifest.IMetadataSectionEntry metadataSectionEntry = (System.Deployment.Internal.Isolation.Manifest.IMetadataSectionEntry) this._cms.MetadataSectionEntry;
                    System.Deployment.Internal.Isolation.Manifest.IDescriptionMetadataEntry descriptionData = metadataSectionEntry.DescriptionData;
                    if (descriptionData != null)
                    {
                        System.Deployment.Application.Manifest.Description description = new System.Deployment.Application.Manifest.Description(descriptionData.AllData);
                        Interlocked.CompareExchange(ref this._description, description, null);
                    }
                }
                return (System.Deployment.Application.Manifest.Description) this._description;
            }
        }

        public EntryPoint[] EntryPoints
        {
            get
            {
                if (this._entryPoints == null)
                {
                    System.Deployment.Internal.Isolation.ISection section = (this._cms != null) ? this._cms.EntryPointSection : null;
                    uint celt = (section != null) ? section.Count : 0;
                    EntryPoint[] pointArray = new EntryPoint[celt];
                    if (celt > 0)
                    {
                        uint celtFetched = 0;
                        System.Deployment.Internal.Isolation.Manifest.IEntryPointEntry[] rgelt = new System.Deployment.Internal.Isolation.Manifest.IEntryPointEntry[celt];
                        System.Deployment.Internal.Isolation.IEnumUnknown unknown = (System.Deployment.Internal.Isolation.IEnumUnknown) section._NewEnum;
                        Marshal.ThrowExceptionForHR(unknown.Next(celt, rgelt, ref celtFetched));
                        if (celtFetched != celt)
                        {
                            throw new InvalidDeploymentException(ExceptionTypes.Manifest, Resources.GetString("Ex_IsoEnumFetchNotEqualToCount"));
                        }
                        for (uint i = 0; i < celt; i++)
                        {
                            pointArray[i] = new EntryPoint(rgelt[i].AllData, this);
                        }
                    }
                    Interlocked.CompareExchange(ref this._entryPoints, pointArray, null);
                }
                return (EntryPoint[]) this._entryPoints;
            }
        }

        public FileAssociation[] FileAssociations
        {
            get
            {
                if (this._fileAssociations == null)
                {
                    System.Deployment.Internal.Isolation.ISection section = (this._cms != null) ? this._cms.FileAssociationSection : null;
                    uint celt = (section != null) ? section.Count : 0;
                    FileAssociation[] associationArray = new FileAssociation[celt];
                    if (celt > 0)
                    {
                        uint celtFetched = 0;
                        System.Deployment.Internal.Isolation.Manifest.IFileAssociationEntry[] rgelt = new System.Deployment.Internal.Isolation.Manifest.IFileAssociationEntry[celt];
                        System.Deployment.Internal.Isolation.IEnumUnknown unknown = (System.Deployment.Internal.Isolation.IEnumUnknown) section._NewEnum;
                        Marshal.ThrowExceptionForHR(unknown.Next(celt, rgelt, ref celtFetched));
                        if (celtFetched != celt)
                        {
                            throw new InvalidDeploymentException(ExceptionTypes.Manifest, Resources.GetString("Ex_IsoEnumFetchNotEqualToCount"));
                        }
                        for (uint i = 0; i < celt; i++)
                        {
                            associationArray[i] = new FileAssociation(rgelt[i].AllData);
                        }
                    }
                    Interlocked.CompareExchange(ref this._fileAssociations, associationArray, null);
                }
                return (FileAssociation[]) this._fileAssociations;
            }
        }

        public System.Deployment.Application.Manifest.File[] Files
        {
            get
            {
                if (this._files == null)
                {
                    System.Deployment.Internal.Isolation.ISection section = (this._cms != null) ? this._cms.FileSection : null;
                    uint celt = (section != null) ? section.Count : 0;
                    System.Deployment.Application.Manifest.File[] fileArray = new System.Deployment.Application.Manifest.File[celt];
                    if (celt > 0)
                    {
                        uint celtFetched = 0;
                        System.Deployment.Internal.Isolation.Manifest.IFileEntry[] rgelt = new System.Deployment.Internal.Isolation.Manifest.IFileEntry[celt];
                        System.Deployment.Internal.Isolation.IEnumUnknown unknown = (System.Deployment.Internal.Isolation.IEnumUnknown) section._NewEnum;
                        Marshal.ThrowExceptionForHR(unknown.Next(celt, rgelt, ref celtFetched));
                        if (celtFetched != celt)
                        {
                            throw new InvalidDeploymentException(ExceptionTypes.Manifest, Resources.GetString("Ex_IsoEnumFetchNotEqualToCount"));
                        }
                        for (uint i = 0; i < celt; i++)
                        {
                            fileArray[i] = new System.Deployment.Application.Manifest.File(rgelt[i].AllData);
                        }
                    }
                    Interlocked.CompareExchange(ref this._files, fileArray, null);
                }
                return (System.Deployment.Application.Manifest.File[]) this._files;
            }
        }

        public System.Deployment.Application.DefinitionIdentity Id1Identity
        {
            get
            {
                return this._id1Identity;
            }
        }

        public bool Id1ManifestPresent
        {
            get
            {
                return this._id1ManifestPresent;
            }
        }

        public string Id1RequestedExecutionLevel
        {
            get
            {
                return this._id1RequestedExecutionLevel;
            }
        }

        public System.Deployment.Application.DefinitionIdentity Identity
        {
            get
            {
                if ((this._identity == null) && (this._cms != null))
                {
                    System.Deployment.Application.DefinitionIdentity identity = null;
                    if (this._cms.Identity == null)
                    {
                        identity = new System.Deployment.Application.DefinitionIdentity();
                    }
                    else
                    {
                        identity = new System.Deployment.Application.DefinitionIdentity(this._cms.Identity);
                    }
                    Interlocked.CompareExchange(ref this._identity, identity, null);
                }
                return (System.Deployment.Application.DefinitionIdentity) this._identity;
            }
        }

        public DependentAssembly MainDependentAssembly
        {
            get
            {
                return this.DependentAssemblies[0];
            }
        }

        public uint ManifestFlags
        {
            get
            {
                if ((this._manifestFlags == null) && (this._cms != null))
                {
                    System.Deployment.Internal.Isolation.Manifest.IMetadataSectionEntry metadataSectionEntry = (System.Deployment.Internal.Isolation.Manifest.IMetadataSectionEntry) this._cms.MetadataSectionEntry;
                    uint manifestFlags = 0;
                    manifestFlags = metadataSectionEntry.ManifestFlags;
                    Interlocked.CompareExchange(ref this._manifestFlags, manifestFlags, null);
                }
                return (uint) this._manifestFlags;
            }
        }

        public System.Deployment.Application.Manifest.ManifestSourceFormat ManifestSourceFormat
        {
            get
            {
                return this._manifestSourceFormat;
            }
        }

        public byte[] RawXmlBytes
        {
            get
            {
                return this._rawXmlBytes;
            }
        }

        public string RawXmlFilePath
        {
            get
            {
                return this._rawXmlFilePath;
            }
        }

        public string RequestedExecutionLevel
        {
            get
            {
                if ((this._requestedExecutionLevel == null) && (this._cms != null))
                {
                    System.Deployment.Internal.Isolation.Manifest.IMetadataSectionEntry metadataSectionEntry = (System.Deployment.Internal.Isolation.Manifest.IMetadataSectionEntry) this._cms.MetadataSectionEntry;
                    string requestedExecutionLevel = metadataSectionEntry.RequestedExecutionLevel;
                    Interlocked.CompareExchange(ref this._requestedExecutionLevel, requestedExecutionLevel, null);
                }
                return (string) this._requestedExecutionLevel;
            }
        }

        public bool RequestedExecutionLevelUIAccess
        {
            get
            {
                if ((this._requestedExecutionLevelUIAccess == null) && (this._cms != null))
                {
                    System.Deployment.Internal.Isolation.Manifest.IMetadataSectionEntry metadataSectionEntry = (System.Deployment.Internal.Isolation.Manifest.IMetadataSectionEntry) this._cms.MetadataSectionEntry;
                    bool requestedExecutionLevelUIAccess = false;
                    requestedExecutionLevelUIAccess = metadataSectionEntry.RequestedExecutionLevelUIAccess;
                    Interlocked.CompareExchange(ref this._requestedExecutionLevelUIAccess, requestedExecutionLevelUIAccess, null);
                }
                return (bool) this._requestedExecutionLevelUIAccess;
            }
        }

        public bool RequiredHashMissing
        {
            get
            {
                if (!this._unhashedDependencyPresent)
                {
                    return this._unhashedFilePresent;
                }
                return true;
            }
        }

        public bool Signed
        {
            get
            {
                return this._signed;
            }
        }

        public ulong SizeInBytes
        {
            get
            {
                return this._sizeInBytes;
            }
        }

        public bool UseManifestForTrust
        {
            get
            {
                return ((this.ManifestFlags & 8) != 0);
            }
        }

        internal enum CertificateStatus
        {
            TrustedPublisher,
            AuthenticodedNotInTrustedList,
            NoCertificate,
            DistrustedPublisher,
            RevokedCertificate,
            UnknownCertificateStatus
        }

        protected class ManifestParseErrors : System.Deployment.Internal.Isolation.IManifestParseErrorCallback, IEnumerable
        {
            protected ArrayList _parsingErrors = new ArrayList();

            public ParseErrorEnumerator GetEnumerator()
            {
                return new ParseErrorEnumerator(this);
            }

            public void OnError(uint StartLine, uint nStartColumn, uint cCharacterCount, int hr, string ErrorStatusHostFile, uint ParameterCount, string[] Parameters)
            {
                ManifestParseError error = new ManifestParseError {
                    StartLine = StartLine,
                    nStartColumn = nStartColumn,
                    cCharacterCount = cCharacterCount,
                    hr = hr,
                    ErrorStatusHostFile = ErrorStatusHostFile,
                    ParameterCount = ParameterCount,
                    Parameters = Parameters
                };
                this._parsingErrors.Add(error);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            public class ManifestParseError
            {
                public uint cCharacterCount;
                public string ErrorStatusHostFile;
                public int hr;
                public uint nStartColumn;
                public uint ParameterCount;
                public string[] Parameters;
                public uint StartLine;
            }

            public class ParseErrorEnumerator : IEnumerator
            {
                private int _index;
                private AssemblyManifest.ManifestParseErrors _manifestParseErrors;

                public ParseErrorEnumerator(AssemblyManifest.ManifestParseErrors manifestParseErrors)
                {
                    this._manifestParseErrors = manifestParseErrors;
                    this._index = -1;
                }

                public bool MoveNext()
                {
                    this._index++;
                    return (this._index < this._manifestParseErrors._parsingErrors.Count);
                }

                public void Reset()
                {
                    this._index = -1;
                }

                public AssemblyManifest.ManifestParseErrors.ManifestParseError Current
                {
                    get
                    {
                        return (AssemblyManifest.ManifestParseErrors.ManifestParseError) this._manifestParseErrors._parsingErrors[this._index];
                    }
                }

                object IEnumerator.Current
                {
                    get
                    {
                        return this._manifestParseErrors._parsingErrors[this._index];
                    }
                }
            }
        }

        internal enum ManifestType
        {
            Application,
            Deployment
        }
    }
}

