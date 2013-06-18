namespace MS.Internal.Xaml.Context
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Xaml;

    internal class XamlParserFrame : XamlCommonFrame
    {
        public override void Reset()
        {
            base.Reset();
            this.PreviousChildType = null;
            this.CtorArgCount = 0;
            this.ForcedToUseConstructor = false;
            this.InCollectionFromMember = false;
            this.InImplicitArray = false;
            this.InContainerDirective = false;
            this.TypeNamespace = null;
        }

        public int CtorArgCount { get; set; }

        public bool ForcedToUseConstructor { get; set; }

        public bool InCollectionFromMember { get; set; }

        public bool InContainerDirective { get; set; }

        public bool InImplicitArray { get; set; }

        public XamlType PreviousChildType { get; set; }

        public string TypeNamespace { get; set; }
    }
}

