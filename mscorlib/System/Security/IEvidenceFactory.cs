namespace System.Security
{
    using System.Runtime.InteropServices;
    using System.Security.Policy;

    [ComVisible(true)]
    public interface IEvidenceFactory
    {
        System.Security.Policy.Evidence Evidence { get; }
    }
}

