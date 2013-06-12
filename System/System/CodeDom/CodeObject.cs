namespace System.CodeDom
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class CodeObject
    {
        private IDictionary userData;

        public IDictionary UserData
        {
            get
            {
                if (this.userData == null)
                {
                    this.userData = new ListDictionary();
                }
                return this.userData;
            }
        }
    }
}

