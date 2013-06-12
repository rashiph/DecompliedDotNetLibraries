namespace System.Diagnostics
{
    using System;

    internal class ModuleInfo
    {
        public string baseName;
        public IntPtr baseOfDll;
        public IntPtr entryPoint;
        public string fileName;
        public int Id;
        public int sizeOfImage;
    }
}

