namespace System.Web.Configuration
{
    using System;

    internal class CapabilitiesAssignment : CapabilitiesRule
    {
        internal CapabilitiesPattern _pat;
        internal string _var;

        internal CapabilitiesAssignment(string var, CapabilitiesPattern pat)
        {
            base._type = 1;
            this._var = var;
            this._pat = pat;
        }

        internal override void Evaluate(CapabilitiesState state)
        {
            state.SetVariable(this._var, this._pat.Expand(state));
            state.Exit = false;
        }
    }
}

