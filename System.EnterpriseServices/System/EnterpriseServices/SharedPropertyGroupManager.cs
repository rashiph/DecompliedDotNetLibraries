namespace System.EnterpriseServices
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;

    [ComVisible(false)]
    public sealed class SharedPropertyGroupManager : IEnumerable
    {
        private ISharedPropertyGroupManager _ex = ((ISharedPropertyGroupManager) new xSharedPropertyGroupManager());

        public SharedPropertyGroup CreatePropertyGroup(string name, ref PropertyLockMode dwIsoMode, ref PropertyReleaseMode dwRelMode, out bool fExist)
        {
            return new SharedPropertyGroup(this._ex.CreatePropertyGroup(name, ref dwIsoMode, ref dwRelMode, out fExist));
        }

        public IEnumerator GetEnumerator()
        {
            IEnumerator pEnum = null;
            this._ex.GetEnumerator(out pEnum);
            return pEnum;
        }

        public SharedPropertyGroup Group(string name)
        {
            return new SharedPropertyGroup(this._ex.Group(name));
        }
    }
}

