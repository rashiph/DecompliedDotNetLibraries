namespace System.Data
{
    using System;

    internal enum RBTreeError
    {
        AttachedNodeWithZerorbTreeNodeId = 0x12,
        CannotRotateInvalidsuccessorNodeinDelete = 11,
        CompareNodeInDataRowTree = 0x13,
        CompareSateliteTreeNodeInDataRowTree = 20,
        IndexOutOFRangeinGetNodeByIndex = 13,
        InvalidNextSizeInDelete = 7,
        InvalidNodeSizeinDelete = 9,
        InvalidPageSize = 1,
        InvalidStateinDelete = 8,
        InvalidStateinEndDelete = 10,
        InvalidStateinInsert = 5,
        NestedSatelliteTreeEnumerator = 0x15,
        NoFreeSlots = 4,
        PagePositionInSlotInUse = 3,
        RBDeleteFixup = 14,
        UnsupportedAccessMethod1 = 15,
        UnsupportedAccessMethod2 = 0x10,
        UnsupportedAccessMethodInNonNillRootSubtree = 0x11
    }
}

