namespace System.Data.SqlClient
{
    using System;
    using System.Data.SqlTypes;

    internal sealed class SqlCollation
    {
        private const uint BinarySort = 0x1000000;
        private const uint IgnoreCase = 0x100000;
        private const uint IgnoreKanaType = 0x800000;
        private const uint IgnoreNonSpace = 0x200000;
        private const uint IgnoreWidth = 0x400000;
        internal uint info;
        private const int LcidVersionBitOffset = 0x1c;
        private const uint MaskCompareOpt = 0x1f00000;
        internal const uint MaskLcid = 0xfffff;
        private const uint MaskLcidVersion = 0xf0000000;
        internal byte sortId;

        private static int FirstSupportedCollationVersion(int lcid)
        {
            switch (lcid)
            {
                case 0x414:
                    return 2;

                case 0x417:
                    return 2;

                case 0x429:
                    return 2;

                case 0x42c:
                    return 2;

                case 0x42e:
                    return 2;

                case 0x42f:
                    return 1;

                case 0x420:
                    return 2;

                case 0x439:
                    return 1;

                case 0x43a:
                    return 2;

                case 0x43b:
                    return 2;

                case 0x43f:
                    return 1;

                case 0x442:
                    return 2;

                case 0x443:
                    return 1;

                case 0x444:
                    return 1;

                case 0x445:
                    return 2;

                case 0x44d:
                    return 2;

                case 0x451:
                    return 2;

                case 0x452:
                    return 2;

                case 0x453:
                    return 2;

                case 0x454:
                    return 2;

                case 0x45a:
                    return 1;

                case 0x461:
                    return 2;

                case 0x462:
                    return 2;

                case 0x463:
                    return 2;

                case 0x465:
                    return 1;

                case 0x46d:
                    return 2;

                case 0x47a:
                    return 2;

                case 0x47c:
                    return 2;

                case 0x47e:
                    return 2;

                case 0x480:
                    return 2;

                case 0x481:
                    return 2;

                case 0x483:
                    return 2;

                case 0x485:
                    return 2;

                case 0x82c:
                    return 2;

                case 0x83b:
                    return 2;

                case 0x85f:
                    return 2;

                case 0x48c:
                    return 2;

                case 0x81a:
                    return 2;

                case 0xc04:
                    return 1;

                case 0xc1a:
                    return 2;

                case 0x1404:
                    return 2;

                case 0x141a:
                    return 2;

                case 0x201a:
                    return 2;
            }
            return 0;
        }

        internal string TraceString()
        {
            return string.Format(null, "(LCID={0}, Opts={1})", new object[] { this.LCID, (int) this.SqlCompareOptions });
        }

        internal int LCID
        {
            get
            {
                return (((int) this.info) & 0xfffff);
            }
            set
            {
                int lcid = value & 0xfffff;
                int num2 = FirstSupportedCollationVersion(lcid) << 0x1c;
                this.info = (uint) (((this.info & 0x1f00000) | lcid) | num2);
            }
        }

        internal System.Data.SqlTypes.SqlCompareOptions SqlCompareOptions
        {
            get
            {
                System.Data.SqlTypes.SqlCompareOptions none = System.Data.SqlTypes.SqlCompareOptions.None;
                if ((this.info & 0x100000) != 0)
                {
                    none |= System.Data.SqlTypes.SqlCompareOptions.IgnoreCase;
                }
                if ((this.info & 0x200000) != 0)
                {
                    none |= System.Data.SqlTypes.SqlCompareOptions.IgnoreNonSpace;
                }
                if ((this.info & 0x400000) != 0)
                {
                    none |= System.Data.SqlTypes.SqlCompareOptions.IgnoreWidth;
                }
                if ((this.info & 0x800000) != 0)
                {
                    none |= System.Data.SqlTypes.SqlCompareOptions.IgnoreKanaType;
                }
                if ((this.info & 0x1000000) != 0)
                {
                    none |= System.Data.SqlTypes.SqlCompareOptions.BinarySort;
                }
                return none;
            }
            set
            {
                uint num = 0;
                if ((value & System.Data.SqlTypes.SqlCompareOptions.IgnoreCase) != System.Data.SqlTypes.SqlCompareOptions.None)
                {
                    num |= 0x100000;
                }
                if ((value & System.Data.SqlTypes.SqlCompareOptions.IgnoreNonSpace) != System.Data.SqlTypes.SqlCompareOptions.None)
                {
                    num |= 0x200000;
                }
                if ((value & System.Data.SqlTypes.SqlCompareOptions.IgnoreWidth) != System.Data.SqlTypes.SqlCompareOptions.None)
                {
                    num |= 0x400000;
                }
                if ((value & System.Data.SqlTypes.SqlCompareOptions.IgnoreKanaType) != System.Data.SqlTypes.SqlCompareOptions.None)
                {
                    num |= 0x800000;
                }
                if ((value & System.Data.SqlTypes.SqlCompareOptions.BinarySort) != System.Data.SqlTypes.SqlCompareOptions.None)
                {
                    num |= 0x1000000;
                }
                this.info = (this.info & 0xfffff) | num;
            }
        }
    }
}

