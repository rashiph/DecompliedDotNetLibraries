namespace System.Deployment.Application
{
    using Microsoft.Runtime.Hosting;
    using System;
    using System.Collections;
    using System.Deployment.Application.Manifest;
    using System.Deployment.Internal.Isolation.Manifest;
    using System.Globalization;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    internal class ComponentVerifier
    {
        protected static System.Deployment.Internal.Isolation.Manifest.CMS_HASH_DIGESTMETHOD[] _supportedDigestMethods = new System.Deployment.Internal.Isolation.Manifest.CMS_HASH_DIGESTMETHOD[] { System.Deployment.Internal.Isolation.Manifest.CMS_HASH_DIGESTMETHOD.CMS_HASH_DIGESTMETHOD_SHA1, System.Deployment.Internal.Isolation.Manifest.CMS_HASH_DIGESTMETHOD.CMS_HASH_DIGESTMETHOD_SHA256, System.Deployment.Internal.Isolation.Manifest.CMS_HASH_DIGESTMETHOD.CMS_HASH_DIGESTMETHOD_SHA384, System.Deployment.Internal.Isolation.Manifest.CMS_HASH_DIGESTMETHOD.CMS_HASH_DIGESTMETHOD_SHA512 };
        protected static System.Deployment.Internal.Isolation.Manifest.CMS_HASH_TRANSFORM[] _supportedTransforms = new System.Deployment.Internal.Isolation.Manifest.CMS_HASH_TRANSFORM[] { System.Deployment.Internal.Isolation.Manifest.CMS_HASH_TRANSFORM.CMS_HASH_TRANSFORM_MANIFESTINVARIANT, System.Deployment.Internal.Isolation.Manifest.CMS_HASH_TRANSFORM.CMS_HASH_TRANSFORM_IDENTITY };
        protected ArrayList _verificationComponents = new ArrayList();

        public void AddFileForVerification(string filePath, HashCollection verificationHashCollection)
        {
            FileComponent component = new FileComponent(filePath, verificationHashCollection);
            this._verificationComponents.Add(component);
        }

        public void AddSimplyNamedAssemblyForVerification(string filePath, AssemblyManifest assemblyManifest)
        {
            SimplyNamedAssemblyComponent component = new SimplyNamedAssemblyComponent(filePath, assemblyManifest);
            this._verificationComponents.Add(component);
        }

        public void AddStrongNameAssemblyForVerification(string filePath, AssemblyManifest assemblyManifest)
        {
            StrongNameAssemblyComponent component = new StrongNameAssemblyComponent(filePath, assemblyManifest);
            this._verificationComponents.Add(component);
        }

        public static byte[] GenerateDigestValue(string filePath, System.Deployment.Internal.Isolation.Manifest.CMS_HASH_DIGESTMETHOD digestMethod, System.Deployment.Internal.Isolation.Manifest.CMS_HASH_TRANSFORM transform)
        {
            Stream inputStream = null;
            byte[] buffer = null;
            try
            {
                HashAlgorithm hashAlgorithm = GetHashAlgorithm(digestMethod);
                inputStream = GetTransformedStream(filePath, transform);
                buffer = hashAlgorithm.ComputeHash(inputStream);
            }
            finally
            {
                if (inputStream != null)
                {
                    inputStream.Close();
                }
            }
            return buffer;
        }

        public static HashAlgorithm GetHashAlgorithm(System.Deployment.Internal.Isolation.Manifest.CMS_HASH_DIGESTMETHOD digestMethod)
        {
            if (digestMethod == System.Deployment.Internal.Isolation.Manifest.CMS_HASH_DIGESTMETHOD.CMS_HASH_DIGESTMETHOD_SHA1)
            {
                return new SHA1CryptoServiceProvider();
            }
            if (digestMethod == System.Deployment.Internal.Isolation.Manifest.CMS_HASH_DIGESTMETHOD.CMS_HASH_DIGESTMETHOD_SHA256)
            {
                if (PlatformSpecific.OnWindows2003 || PlatformSpecific.OnVistaOrAbove)
                {
                    return (CryptoConfig.CreateFromName("System.Security.Cryptography.SHA256CryptoServiceProvider") as HashAlgorithm);
                }
                return new SHA256Managed();
            }
            if (digestMethod == System.Deployment.Internal.Isolation.Manifest.CMS_HASH_DIGESTMETHOD.CMS_HASH_DIGESTMETHOD_SHA384)
            {
                if (PlatformSpecific.OnWindows2003 || PlatformSpecific.OnVistaOrAbove)
                {
                    return (CryptoConfig.CreateFromName("System.Security.Cryptography.SHA384CryptoServiceProvider") as HashAlgorithm);
                }
                return new SHA384Managed();
            }
            if (digestMethod != System.Deployment.Internal.Isolation.Manifest.CMS_HASH_DIGESTMETHOD.CMS_HASH_DIGESTMETHOD_SHA512)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_DigestMethodNotSupported"), new object[] { digestMethod.ToString() }));
            }
            if (!PlatformSpecific.OnWindows2003 && !PlatformSpecific.OnVistaOrAbove)
            {
                return new SHA512Managed();
            }
            return (CryptoConfig.CreateFromName("System.Security.Cryptography.SHA512CryptoServiceProvider") as HashAlgorithm);
        }

        public static Stream GetTransformedStream(string filePath, System.Deployment.Internal.Isolation.Manifest.CMS_HASH_TRANSFORM transform)
        {
            Stream stream = null;
            if (transform == System.Deployment.Internal.Isolation.Manifest.CMS_HASH_TRANSFORM.CMS_HASH_TRANSFORM_MANIFESTINVARIANT)
            {
                PEStream stream2 = null;
                try
                {
                    stream2 = new PEStream(filePath, true);
                    stream2.ZeroOutOptionalHeaderCheckSum();
                    stream2.ZeroOutDefaultId1ManifestResource();
                    return stream2;
                }
                finally
                {
                    if ((stream2 != stream) && (stream2 != null))
                    {
                        stream2.Close();
                    }
                }
            }
            if (transform != System.Deployment.Internal.Isolation.Manifest.CMS_HASH_TRANSFORM.CMS_HASH_TRANSFORM_IDENTITY)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_TransformAlgorithmNotSupported"), new object[] { transform.ToString() }));
            }
            return new FileStream(filePath, FileMode.Open, FileAccess.Read);
        }

        public static bool IsVerifiableHash(Hash hash)
        {
            return (((Array.IndexOf<System.Deployment.Internal.Isolation.Manifest.CMS_HASH_TRANSFORM>(VerifiableTransformTypes, hash.Transform) >= 0) && (Array.IndexOf<System.Deployment.Internal.Isolation.Manifest.CMS_HASH_DIGESTMETHOD>(VerifiableDigestMethods, hash.DigestMethod) >= 0)) && ((hash.DigestValue != null) && (hash.DigestValue.Length > 0)));
        }

        public static bool IsVerifiableHashCollection(HashCollection hashCollection)
        {
            using (HashCollection.HashEnumerator enumerator = hashCollection.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (!IsVerifiableHash(enumerator.Current))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void VerifyComponents()
        {
            foreach (VerificationComponent component in this._verificationComponents)
            {
                component.Verify();
            }
        }

        public static void VerifyFileHash(string filePath, Hash hash)
        {
            byte[] buffer;
            string fileName = Path.GetFileName(filePath);
            try
            {
                buffer = GenerateDigestValue(filePath, hash.DigestMethod, hash.Transform);
            }
            catch (IOException exception)
            {
                throw new InvalidDeploymentException(ExceptionTypes.HashValidation, Resources.GetString("Ex_HashValidationException"), exception);
            }
            byte[] digestValue = hash.DigestValue;
            bool flag = false;
            if (buffer.Length == digestValue.Length)
            {
                int index = 0;
                while (index < digestValue.Length)
                {
                    if (digestValue[index] != buffer[index])
                    {
                        break;
                    }
                    index++;
                }
                if (index >= digestValue.Length)
                {
                    flag = true;
                }
            }
            if (!flag)
            {
                Logger.AddInternalState("File," + fileName + ", has a different computed hash than specified in manifest. Computed hash is " + Encoding.UTF8.GetString(buffer) + ". Specified hash is " + Encoding.UTF8.GetString(digestValue));
                throw new InvalidDeploymentException(ExceptionTypes.HashValidation, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_DifferentHashes"), new object[] { fileName }));
            }
        }

        public static void VerifyFileHash(string filePath, HashCollection hashCollection)
        {
            string fileName = Path.GetFileName(filePath);
            if (hashCollection.Count == 0)
            {
                if (PolicyKeys.RequireHashInManifests())
                {
                    throw new InvalidDeploymentException(ExceptionTypes.HashValidation, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_HashNotSpecified"), new object[] { fileName }));
                }
                Logger.AddWarningInformation(string.Format(CultureInfo.CurrentUICulture, Resources.GetString("NoHashFile"), new object[] { fileName }));
            }
            foreach (Hash hash in hashCollection)
            {
                VerifyFileHash(filePath, hash);
            }
        }

        protected static void VerifyManifestComponentFiles(AssemblyManifest manifest, string componentPath, bool ignoreSelfReferentialFileHash)
        {
            string directoryName = Path.GetDirectoryName(componentPath);
            foreach (System.Deployment.Application.Manifest.File file in manifest.Files)
            {
                string strB = Path.Combine(directoryName, file.NameFS);
                if ((!ignoreSelfReferentialFileHash || (string.Compare(componentPath, strB, StringComparison.OrdinalIgnoreCase) != 0)) && System.IO.File.Exists(strB))
                {
                    VerifyFileHash(strB, file.HashCollection);
                }
            }
        }

        public static void VerifySimplyNamedAssembly(string filePath, AssemblyManifest assemblyManifest)
        {
            string fileName = Path.GetFileName(filePath);
            if (assemblyManifest.Identity.PublicKeyToken != null)
            {
                throw new InvalidDeploymentException(ExceptionTypes.Validation, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_SimplyNamedAsmWithPKT"), new object[] { fileName }));
            }
            if (((assemblyManifest.ManifestSourceFormat == ManifestSourceFormat.ID_1) && (assemblyManifest.ComplibIdentity != null)) && (assemblyManifest.ComplibIdentity.PublicKeyToken != null))
            {
                throw new InvalidDeploymentException(ExceptionTypes.IdentityMatchValidationForMixedModeAssembly, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_SimplyNamedAsmWithStrongNameComplib"), new object[] { fileName }));
            }
        }

        public static void VerifyStrongNameAssembly(string filePath, AssemblyManifest assemblyManifest)
        {
            string fileName = Path.GetFileName(filePath);
            if (assemblyManifest.Identity.PublicKeyToken == null)
            {
                throw new InvalidDeploymentException(ExceptionTypes.Validation, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_StrongNameAsmWithNoPKT"), new object[] { fileName }));
            }
            bool ignoreSelfReferentialFileHash = false;
            if (assemblyManifest.ManifestSourceFormat == ManifestSourceFormat.XmlFile)
            {
                assemblyManifest.ValidateSignature(null);
            }
            else if (assemblyManifest.ManifestSourceFormat == ManifestSourceFormat.ID_1)
            {
                bool flag2;
                if (assemblyManifest.ComplibIdentity == null)
                {
                    byte[] buffer = null;
                    PEStream stream = null;
                    MemoryStream s = null;
                    try
                    {
                        stream = new PEStream(filePath, true);
                        buffer = stream.GetDefaultId1ManifestResource();
                        if (buffer != null)
                        {
                            s = new MemoryStream(buffer);
                        }
                        if (s == null)
                        {
                            throw new InvalidDeploymentException(ExceptionTypes.StronglyNamedAssemblyVerification, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_StronglyNamedAssemblyNotVerifiable"), new object[] { fileName }));
                        }
                        assemblyManifest.ValidateSignature(s);
                        goto Label_01C3;
                    }
                    finally
                    {
                        if (stream != null)
                        {
                            stream.Close();
                        }
                        if (s != null)
                        {
                            s.Close();
                        }
                    }
                }
                if (!assemblyManifest.ComplibIdentity.Equals(assemblyManifest.Identity))
                {
                    throw new InvalidDeploymentException(ExceptionTypes.IdentityMatchValidationForMixedModeAssembly, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_IdentitiesDoNotMatchForMixedModeAssembly"), new object[] { fileName }));
                }
                if (!Microsoft.Runtime.Hosting.StrongNameHelpers.StrongNameSignatureVerificationEx(filePath, false, out flag2))
                {
                    throw new InvalidDeploymentException(ExceptionTypes.SignatureValidation, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_StrongNameSignatureInvalid"), new object[] { fileName }));
                }
                ignoreSelfReferentialFileHash = true;
            }
            else
            {
                bool flag3;
                if (assemblyManifest.ManifestSourceFormat != ManifestSourceFormat.CompLib)
                {
                    throw new InvalidDeploymentException(ExceptionTypes.StronglyNamedAssemblyVerification, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_StronglyNamedAssemblyNotVerifiable"), new object[] { fileName }));
                }
                if (!Microsoft.Runtime.Hosting.StrongNameHelpers.StrongNameSignatureVerificationEx(filePath, false, out flag3))
                {
                    throw new InvalidDeploymentException(ExceptionTypes.SignatureValidation, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_StrongNameSignatureInvalid"), new object[] { fileName }));
                }
                ignoreSelfReferentialFileHash = true;
            }
        Label_01C3:
            VerifyManifestComponentFiles(assemblyManifest, filePath, ignoreSelfReferentialFileHash);
        }

        public static System.Deployment.Internal.Isolation.Manifest.CMS_HASH_DIGESTMETHOD[] VerifiableDigestMethods
        {
            get
            {
                return _supportedDigestMethods;
            }
        }

        public static System.Deployment.Internal.Isolation.Manifest.CMS_HASH_TRANSFORM[] VerifiableTransformTypes
        {
            get
            {
                return _supportedTransforms;
            }
        }

        protected class FileComponent : ComponentVerifier.VerificationComponent
        {
            protected string _filePath;
            protected HashCollection _hashCollection;

            public FileComponent(string filePath, HashCollection hashCollection)
            {
                this._filePath = filePath;
                this._hashCollection = hashCollection;
            }

            public override void Verify()
            {
                ComponentVerifier.VerifyFileHash(this._filePath, this._hashCollection);
            }
        }

        protected class SimplyNamedAssemblyComponent : ComponentVerifier.VerificationComponent
        {
            protected AssemblyManifest _assemblyManifest;
            protected string _filePath;

            public SimplyNamedAssemblyComponent(string filePath, AssemblyManifest assemblyManifest)
            {
                this._filePath = filePath;
                this._assemblyManifest = assemblyManifest;
            }

            public override void Verify()
            {
                ComponentVerifier.VerifySimplyNamedAssembly(this._filePath, this._assemblyManifest);
            }
        }

        protected class StrongNameAssemblyComponent : ComponentVerifier.VerificationComponent
        {
            protected AssemblyManifest _assemblyManifest;
            protected string _filePath;

            public StrongNameAssemblyComponent(string filePath, AssemblyManifest assemblyManifest)
            {
                this._filePath = filePath;
                this._assemblyManifest = assemblyManifest;
            }

            public override void Verify()
            {
                ComponentVerifier.VerifyStrongNameAssembly(this._filePath, this._assemblyManifest);
            }
        }

        protected abstract class VerificationComponent
        {
            protected VerificationComponent()
            {
            }

            public abstract void Verify();
        }
    }
}

