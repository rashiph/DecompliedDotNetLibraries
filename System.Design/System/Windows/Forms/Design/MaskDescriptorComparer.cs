namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections.Generic;
    using System.Design;
    using System.Windows.Forms;

    internal class MaskDescriptorComparer : IComparer<MaskDescriptor>
    {
        private SortOrder sortOrder;
        private SortType sortType;

        public MaskDescriptorComparer(SortType sortType, SortOrder sortOrder)
        {
            this.sortType = sortType;
            this.sortOrder = sortOrder;
        }

        public int Compare(MaskDescriptor maskDescriptorA, MaskDescriptor maskDescriptorB)
        {
            string sample;
            string name;
            if ((maskDescriptorA == null) || (maskDescriptorB == null))
            {
                return 0;
            }
            switch (this.sortType)
            {
                case SortType.BySample:
                    sample = maskDescriptorA.Sample;
                    name = maskDescriptorB.Sample;
                    break;

                case SortType.ByValidatingTypeName:
                    sample = (maskDescriptorA.ValidatingType == null) ? System.Design.SR.GetString("MaskDescriptorValidatingTypeNone") : maskDescriptorA.ValidatingType.Name;
                    name = (maskDescriptorB.ValidatingType == null) ? System.Design.SR.GetString("MaskDescriptorValidatingTypeNone") : maskDescriptorB.ValidatingType.Name;
                    break;

                default:
                    sample = maskDescriptorA.Name;
                    name = maskDescriptorB.Name;
                    break;
            }
            int num = string.Compare(sample, name);
            if (this.sortOrder != SortOrder.Descending)
            {
                return num;
            }
            return -num;
        }

        public bool Equals(MaskDescriptor maskDescriptorA, MaskDescriptor maskDescriptorB)
        {
            if (MaskDescriptor.IsValidMaskDescriptor(maskDescriptorA) && MaskDescriptor.IsValidMaskDescriptor(maskDescriptorB))
            {
                return maskDescriptorA.Equals(maskDescriptorB);
            }
            return (maskDescriptorA == maskDescriptorB);
        }

        public int GetHashCode(MaskDescriptor maskDescriptor)
        {
            if (maskDescriptor != null)
            {
                return maskDescriptor.GetHashCode();
            }
            return 0;
        }

        public enum SortType
        {
            ByName,
            BySample,
            ByValidatingTypeName
        }
    }
}

