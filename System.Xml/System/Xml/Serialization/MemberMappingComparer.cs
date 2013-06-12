namespace System.Xml.Serialization
{
    using System;
    using System.Collections;

    internal class MemberMappingComparer : IComparer
    {
        public int Compare(object o1, object o2)
        {
            MemberMapping mapping = (MemberMapping) o1;
            MemberMapping mapping2 = (MemberMapping) o2;
            if (mapping.IsText)
            {
                if (mapping2.IsText)
                {
                    return 0;
                }
                return 1;
            }
            if (mapping2.IsText)
            {
                return -1;
            }
            if ((mapping.SequenceId >= 0) || (mapping2.SequenceId >= 0))
            {
                if (mapping.SequenceId < 0)
                {
                    return 1;
                }
                if (mapping2.SequenceId < 0)
                {
                    return -1;
                }
                if (mapping.SequenceId < mapping2.SequenceId)
                {
                    return -1;
                }
                if (mapping.SequenceId > mapping2.SequenceId)
                {
                    return 1;
                }
            }
            return 0;
        }
    }
}

