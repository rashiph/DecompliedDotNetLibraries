namespace System.Web.UI
{
    using System.Collections;

    internal interface IAssemblyDependencyParser
    {
        ICollection AssemblyDependencies { get; }
    }
}

