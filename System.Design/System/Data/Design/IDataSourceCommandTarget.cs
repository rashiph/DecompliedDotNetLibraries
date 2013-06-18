namespace System.Data.Design
{
    using System;
    using System.Collections;

    internal interface IDataSourceCommandTarget
    {
        void AddChild(object child, bool fixName);
        bool CanAddChildOfType(Type childType);
        bool CanInsertChildOfType(Type childType, object refChild);
        bool CanRemoveChildren(ICollection children);
        object GetObject(int index, bool getSiblingIfOutOfRange);
        int IndexOf(object child);
        void InsertChild(object child, object refChild);
        void RemoveChildren(ICollection children);
    }
}

