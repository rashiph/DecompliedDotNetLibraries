namespace System.Web.Configuration
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Reflection;
    using System.Web;
    using System.Web.Util;

    [Serializable]
    public sealed class VirtualDirectoryMappingCollection : NameObjectCollectionBase
    {
        public VirtualDirectoryMappingCollection() : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public void Add(string virtualDirectory, VirtualDirectoryMapping mapping)
        {
            virtualDirectory = ValidateVirtualDirectoryParameter(virtualDirectory);
            if (mapping == null)
            {
                throw new ArgumentNullException("mapping");
            }
            if (this.Get(virtualDirectory) != null)
            {
                throw ExceptionUtil.ParameterInvalid("virtualDirectory");
            }
            mapping.SetVirtualDirectory(VirtualPath.CreateAbsoluteAllowNull(virtualDirectory));
            base.BaseAdd(virtualDirectory, mapping);
        }

        public void Clear()
        {
            base.BaseClear();
        }

        internal VirtualDirectoryMappingCollection Clone()
        {
            VirtualDirectoryMappingCollection mappings = new VirtualDirectoryMappingCollection();
            for (int i = 0; i < this.Count; i++)
            {
                VirtualDirectoryMapping mapping = this[i];
                mappings.Add(mapping.VirtualDirectory, mapping.Clone());
            }
            return mappings;
        }

        public void CopyTo(VirtualDirectoryMapping[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            int count = this.Count;
            if (array.Length < (count + index))
            {
                throw new ArgumentOutOfRangeException("index");
            }
            int num2 = 0;
            for (int i = index; num2 < count; i++)
            {
                array[i] = this.Get(num2);
                num2++;
            }
        }

        public VirtualDirectoryMapping Get(int index)
        {
            return (VirtualDirectoryMapping) base.BaseGet(index);
        }

        public VirtualDirectoryMapping Get(string virtualDirectory)
        {
            virtualDirectory = ValidateVirtualDirectoryParameter(virtualDirectory);
            return (VirtualDirectoryMapping) base.BaseGet(virtualDirectory);
        }

        public string GetKey(int index)
        {
            return base.BaseGetKey(index);
        }

        public void Remove(string virtualDirectory)
        {
            virtualDirectory = ValidateVirtualDirectoryParameter(virtualDirectory);
            base.BaseRemove(virtualDirectory);
        }

        public void RemoveAt(int index)
        {
            base.BaseRemoveAt(index);
        }

        private static string ValidateVirtualDirectoryParameter(string virtualDirectory)
        {
            return VirtualPath.GetVirtualPathString(VirtualPath.CreateAbsoluteAllowNull(virtualDirectory));
        }

        public ICollection AllKeys
        {
            get
            {
                return base.BaseGetAllKeys();
            }
        }

        public VirtualDirectoryMapping this[string virtualDirectory]
        {
            get
            {
                virtualDirectory = ValidateVirtualDirectoryParameter(virtualDirectory);
                return this.Get(virtualDirectory);
            }
        }

        public VirtualDirectoryMapping this[int index]
        {
            get
            {
                return this.Get(index);
            }
        }
    }
}

