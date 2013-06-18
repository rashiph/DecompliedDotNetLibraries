namespace System.Web.Configuration
{
    using System;

    internal class CapabilitiesUse : CapabilitiesRule
    {
        internal string _as;
        internal string _var;

        internal CapabilitiesUse(string var, string asParam)
        {
            this._var = var;
            this._as = asParam;
        }

        internal override void Evaluate(CapabilitiesState state)
        {
            state.SetVariable(this._as, state.ResolveServerVariable(this._var));
            state.Exit = false;
        }
    }
}

