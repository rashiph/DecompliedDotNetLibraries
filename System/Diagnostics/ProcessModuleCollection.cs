namespace System.Diagnostics
{
    using System;
    using System.Collections;
    using System.Reflection;

    public class ProcessModuleCollection : ReadOnlyCollectionBase
    {
        protected ProcessModuleCollection()
        {
        }

        public ProcessModuleCollection(ProcessModule[] processModules)
        {
            base.InnerList.AddRange(processModules);
        }

        public bool Contains(ProcessModule module)
        {
            return base.InnerList.Contains(module);
        }

        public void CopyTo(ProcessModule[] array, int index)
        {
            base.InnerList.CopyTo(array, index);
        }

        public int IndexOf(ProcessModule module)
        {
            return base.InnerList.IndexOf(module);
        }

        public ProcessModule this[int index]
        {
            get
            {
                return (ProcessModule) base.InnerList[index];
            }
        }
    }
}

