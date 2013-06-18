namespace System.Data.OracleClient
{
    using System;

    internal sealed class OciErrorHandle : OciHandle
    {
        private bool _connectionIsBroken;

        internal OciErrorHandle(OciHandle parent) : base(parent, OCI.HTYPE.OCI_HTYPE_ERROR)
        {
        }

        internal bool ConnectionIsBroken
        {
            get
            {
                return this._connectionIsBroken;
            }
            set
            {
                this._connectionIsBroken = value;
            }
        }
    }
}

