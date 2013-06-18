namespace MS.Internal.Xaml.Context
{
    using MS.Internal.Xaml.Runtime;
    using System;
    using System.Runtime.CompilerServices;
    using System.Xaml;

    internal class FixupTarget : IAddLineInfo
    {
        XamlException IAddLineInfo.WithLineInfo(XamlException ex)
        {
            if (this.EndInstanceLineNumber > 0)
            {
                ex.SetLineInfo(this.EndInstanceLineNumber, this.EndInstanceLinePosition);
            }
            return ex;
        }

        public int EndInstanceLineNumber { get; set; }

        public int EndInstanceLinePosition { get; set; }

        public object Instance { get; set; }

        public bool InstanceIsOnTheStack { get; set; }

        public string InstanceName { get; set; }

        public XamlType InstanceType { get; set; }

        public bool InstanceWasGotten { get; set; }

        public FixupTargetKeyHolder KeyHolder { get; set; }

        public XamlMember Property { get; set; }

        public int TemporaryCollectionIndex { get; set; }
    }
}

