namespace System.Runtime.Serialization.Formatters.Binary
{
    using System;

    internal sealed class NameInfo
    {
        internal InternalArrayTypeE NIarrayEnum;
        internal long NIassemId;
        internal string NIFullName;
        internal bool NIisArray;
        internal bool NIisArrayItem;
        internal bool NIisParentTypeOnObject;
        internal bool NIisSealed;
        internal long NIobjectId;
        internal InternalPrimitiveTypeE NIprimitiveTypeEnum;
        private bool NIsealedStatusChecked;
        internal bool NItransmitTypeOnMember;
        internal bool NItransmitTypeOnObject;
        internal Type NItype;

        internal NameInfo()
        {
        }

        internal void Init()
        {
            this.NIFullName = null;
            this.NIobjectId = 0L;
            this.NIassemId = 0L;
            this.NIprimitiveTypeEnum = InternalPrimitiveTypeE.Invalid;
            this.NItype = null;
            this.NIisSealed = false;
            this.NItransmitTypeOnObject = false;
            this.NItransmitTypeOnMember = false;
            this.NIisParentTypeOnObject = false;
            this.NIisArray = false;
            this.NIisArrayItem = false;
            this.NIarrayEnum = InternalArrayTypeE.Empty;
            this.NIsealedStatusChecked = false;
        }

        public bool IsSealed
        {
            get
            {
                if (!this.NIsealedStatusChecked)
                {
                    this.NIisSealed = this.NItype.IsSealed;
                    this.NIsealedStatusChecked = true;
                }
                return this.NIisSealed;
            }
        }

        public string NIname
        {
            get
            {
                if (this.NIFullName == null)
                {
                    this.NIFullName = this.NItype.FullName;
                }
                return this.NIFullName;
            }
            set
            {
                this.NIFullName = value;
            }
        }
    }
}

