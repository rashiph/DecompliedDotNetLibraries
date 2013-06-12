namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [Serializable, ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class CodeStatement : CodeObject
    {
        [OptionalField]
        private CodeDirectiveCollection endDirectives;
        private CodeLinePragma linePragma;
        [OptionalField]
        private CodeDirectiveCollection startDirectives;

        public CodeDirectiveCollection EndDirectives
        {
            get
            {
                if (this.endDirectives == null)
                {
                    this.endDirectives = new CodeDirectiveCollection();
                }
                return this.endDirectives;
            }
        }

        public CodeLinePragma LinePragma
        {
            get
            {
                return this.linePragma;
            }
            set
            {
                this.linePragma = value;
            }
        }

        public CodeDirectiveCollection StartDirectives
        {
            get
            {
                if (this.startDirectives == null)
                {
                    this.startDirectives = new CodeDirectiveCollection();
                }
                return this.startDirectives;
            }
        }
    }
}

