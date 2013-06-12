namespace System.Security.Policy
{
    using System;
    using System.Security;

    internal class CodeGroupPositionMarker
    {
        internal SecurityElement element;
        internal int elementIndex;
        internal int groupIndex;

        internal CodeGroupPositionMarker(int elementIndex, int groupIndex, SecurityElement element)
        {
            this.elementIndex = elementIndex;
            this.groupIndex = groupIndex;
            this.element = element;
        }
    }
}

