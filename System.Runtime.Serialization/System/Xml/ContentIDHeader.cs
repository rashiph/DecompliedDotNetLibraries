namespace System.Xml
{
    using System;
    using System.Runtime;

    internal class ContentIDHeader : MimeHeader
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ContentIDHeader(string name, string value) : base(name, value)
        {
        }
    }
}

