namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Tasks.Deployment.ManifestUtilities;
    using Microsoft.Build.Utilities;
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Security.Cryptography;

    public sealed class SignFile : Task
    {
        private string certificateThumbprint;
        private ITaskItem sigingTarget;
        private string timestampUrl;

        public SignFile() : base(AssemblyResources.PrimaryResources, "MSBuild.")
        {
        }

        public override bool Execute()
        {
            try
            {
                SecurityUtilities.SignFile(this.CertificateThumbprint, (this.TimestampUrl == null) ? null : new Uri(this.TimestampUrl), this.SigningTarget.ItemSpec);
                return true;
            }
            catch (ArgumentException exception)
            {
                if (!exception.ParamName.Equals("certThumbprint"))
                {
                    throw;
                }
                base.Log.LogErrorWithCodeFromResources("SignFile.CertNotInStore", new object[0]);
                return false;
            }
            catch (FileNotFoundException exception2)
            {
                base.Log.LogErrorWithCodeFromResources("SignFile.TargetFileNotFound", new object[] { exception2.FileName });
                return false;
            }
            catch (ApplicationException exception3)
            {
                base.Log.LogErrorWithCodeFromResources("SignFile.SignToolError", new object[] { exception3.Message.Trim() });
                return false;
            }
            catch (WarningException exception4)
            {
                base.Log.LogWarningWithCodeFromResources("SignFile.SignToolWarning", new object[] { exception4.Message.Trim() });
                return true;
            }
            catch (CryptographicException exception5)
            {
                base.Log.LogErrorWithCodeFromResources("SignFile.SignToolError", new object[] { exception5.Message.Trim() });
                return false;
            }
            catch (Win32Exception exception6)
            {
                base.Log.LogErrorWithCodeFromResources("SignFile.SignToolError", new object[] { exception6.Message.Trim() });
                return false;
            }
            catch (UriFormatException exception7)
            {
                base.Log.LogErrorWithCodeFromResources("SignFile.SignToolError", new object[] { exception7.Message.Trim() });
                return false;
            }
        }

        [Required]
        public string CertificateThumbprint
        {
            get
            {
                return this.certificateThumbprint;
            }
            set
            {
                this.certificateThumbprint = value;
            }
        }

        [Required]
        public ITaskItem SigningTarget
        {
            get
            {
                return this.sigingTarget;
            }
            set
            {
                this.sigingTarget = value;
            }
        }

        public string TimestampUrl
        {
            get
            {
                return this.timestampUrl;
            }
            set
            {
                this.timestampUrl = value;
            }
        }
    }
}

