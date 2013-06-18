namespace System.Runtime.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct ObjectReferenceStack
    {
        private int count;
        private object[] objectArray;
        private bool[] isReferenceArray;
        private Dictionary<object, object> objectDictionary;
        private const int MaximumArraySize = 0x10;
        private const int InitialArraySize = 4;
        internal void Push(object obj)
        {
            if (this.objectArray == null)
            {
                this.objectArray = new object[4];
                this.objectArray[this.count++] = obj;
            }
            else if (this.count < 0x10)
            {
                if (this.count == this.objectArray.Length)
                {
                    Array.Resize<object>(ref this.objectArray, this.objectArray.Length * 2);
                }
                this.objectArray[this.count++] = obj;
            }
            else
            {
                if (this.objectDictionary == null)
                {
                    this.objectDictionary = new Dictionary<object, object>();
                }
                this.objectDictionary.Add(obj, null);
                this.count++;
            }
        }

        internal void EnsureSetAsIsReference(object obj)
        {
            if (this.count != 0)
            {
                if (this.count > 0x10)
                {
                    Dictionary<object, object> objectDictionary = this.objectDictionary;
                    this.objectDictionary.Remove(obj);
                }
                else if ((this.objectArray != null) && (this.objectArray[this.count - 1] == obj))
                {
                    if (this.isReferenceArray == null)
                    {
                        this.isReferenceArray = new bool[4];
                    }
                    else if (this.count == this.isReferenceArray.Length)
                    {
                        Array.Resize<bool>(ref this.isReferenceArray, this.isReferenceArray.Length * 2);
                    }
                    this.isReferenceArray[this.count - 1] = true;
                }
            }
        }

        internal void Pop(object obj)
        {
            if (this.count > 0x10)
            {
                Dictionary<object, object> objectDictionary = this.objectDictionary;
                this.objectDictionary.Remove(obj);
            }
            this.count--;
        }

        internal bool Contains(object obj)
        {
            int count = this.count;
            if (count > 0x10)
            {
                if ((this.objectDictionary != null) && this.objectDictionary.ContainsKey(obj))
                {
                    return true;
                }
                count = 0x10;
            }
            for (int i = count - 1; i >= 0; i--)
            {
                if ((object.ReferenceEquals(obj, this.objectArray[i]) && (this.isReferenceArray != null)) && !this.isReferenceArray[i])
                {
                    return true;
                }
            }
            return false;
        }

        internal int Count
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.count;
            }
        }
    }
}

