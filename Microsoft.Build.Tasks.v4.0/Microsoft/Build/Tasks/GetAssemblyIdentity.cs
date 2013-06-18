namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using Microsoft.Build.Utilities;
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Reflection;
    using System.Text;

    public class GetAssemblyIdentity : TaskExtension
    {
        private ITaskItem[] assemblies;
        private ITaskItem[] assemblyFiles;

        private static string ByteArrayToHex(byte[] a)
        {
            if (a == null)
            {
                return null;
            }
            StringBuilder builder = new StringBuilder(a.Length);
            foreach (byte num in a)
            {
                builder.Append(num.ToString("X02", CultureInfo.InvariantCulture));
            }
            return builder.ToString();
        }

        public override bool Execute()
        {
            ArrayList list = new ArrayList();
            foreach (ITaskItem item in this.AssemblyFiles)
            {
                AssemblyName assemblyName;
                try
                {
                    assemblyName = AssemblyName.GetAssemblyName(item.ItemSpec);
                }
                catch (BadImageFormatException exception)
                {
                    base.Log.LogErrorWithCodeFromResources("GetAssemblyIdentity.CouldNotGetAssemblyName", new object[] { item.ItemSpec, exception.Message });
                    continue;
                }
                catch (Exception exception2)
                {
                    if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception2))
                    {
                        throw;
                    }
                    base.Log.LogErrorWithCodeFromResources("GetAssemblyIdentity.CouldNotGetAssemblyName", new object[] { item.ItemSpec, exception2.Message });
                    continue;
                }
                ITaskItem destinationItem = new TaskItem(assemblyName.FullName);
                destinationItem.SetMetadata("Name", assemblyName.Name);
                if (assemblyName.Version != null)
                {
                    destinationItem.SetMetadata("Version", assemblyName.Version.ToString());
                }
                if (assemblyName.GetPublicKeyToken() != null)
                {
                    destinationItem.SetMetadata("PublicKeyToken", ByteArrayToHex(assemblyName.GetPublicKeyToken()));
                }
                if (assemblyName.CultureInfo != null)
                {
                    destinationItem.SetMetadata("Culture", assemblyName.CultureInfo.ToString());
                }
                item.CopyMetadataTo(destinationItem);
                list.Add(destinationItem);
            }
            this.Assemblies = (ITaskItem[]) list.ToArray(typeof(ITaskItem));
            return !base.Log.HasLoggedErrors;
        }

        [Output]
        public ITaskItem[] Assemblies
        {
            get
            {
                return this.assemblies;
            }
            set
            {
                this.assemblies = value;
            }
        }

        [Required]
        public ITaskItem[] AssemblyFiles
        {
            get
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(this.assemblyFiles, "assemblyFiles");
                return this.assemblyFiles;
            }
            set
            {
                this.assemblyFiles = value;
            }
        }
    }
}

