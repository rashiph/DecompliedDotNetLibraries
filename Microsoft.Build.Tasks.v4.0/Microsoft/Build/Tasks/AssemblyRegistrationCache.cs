namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Shared;
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;

    [Serializable]
    internal sealed class AssemblyRegistrationCache : StateFileBase
    {
        private ArrayList assemblies = new ArrayList();
        private ArrayList typeLibraries = new ArrayList();

        internal AssemblyRegistrationCache()
        {
        }

        internal void AddEntry(string assemblyPath, string typeLibraryPath)
        {
            this.assemblies.Add(assemblyPath);
            this.typeLibraries.Add(typeLibraryPath);
        }

        internal void GetEntry(int index, out string assemblyPath, out string typeLibraryPath)
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrow((index >= 0) && (index < this.assemblies.Count), "Invalid index in the call to AssemblyRegistrationCache.GetEntry");
            assemblyPath = (string) this.assemblies[index];
            typeLibraryPath = (string) this.typeLibraries[index];
        }

        internal int Count
        {
            get
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(this.assemblies.Count == this.typeLibraries.Count, "Internal assembly and type library lists should have the same number of entries in AssemblyRegistrationCache");
                return this.assemblies.Count;
            }
        }
    }
}

