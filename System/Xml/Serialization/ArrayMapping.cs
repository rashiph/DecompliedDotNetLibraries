namespace System.Xml.Serialization
{
    using System;

    internal class ArrayMapping : TypeMapping
    {
        private ElementAccessor[] elements;
        private ArrayMapping next;
        private ElementAccessor[] sortedElements;
        private StructMapping topLevelMapping;

        internal ElementAccessor[] Elements
        {
            get
            {
                return this.elements;
            }
            set
            {
                this.elements = value;
                this.sortedElements = null;
            }
        }

        internal ElementAccessor[] ElementsSortedByDerivation
        {
            get
            {
                if (this.sortedElements == null)
                {
                    if (this.elements == null)
                    {
                        return null;
                    }
                    this.sortedElements = new ElementAccessor[this.elements.Length];
                    Array.Copy(this.elements, 0, this.sortedElements, 0, this.elements.Length);
                    AccessorMapping.SortMostToLeastDerived(this.sortedElements);
                }
                return this.sortedElements;
            }
        }

        internal ArrayMapping Next
        {
            get
            {
                return this.next;
            }
            set
            {
                this.next = value;
            }
        }

        internal StructMapping TopLevelMapping
        {
            get
            {
                return this.topLevelMapping;
            }
            set
            {
                this.topLevelMapping = value;
            }
        }
    }
}

