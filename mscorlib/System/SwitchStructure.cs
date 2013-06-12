namespace System
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct SwitchStructure
    {
        internal string name;
        internal int value;
        internal SwitchStructure(string n, int v)
        {
            this.name = n;
            this.value = v;
        }
    }
}

