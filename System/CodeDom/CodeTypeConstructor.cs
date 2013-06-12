namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class CodeTypeConstructor : CodeMemberMethod
    {
        public CodeTypeConstructor()
        {
            base.Name = ".cctor";
        }
    }
}

