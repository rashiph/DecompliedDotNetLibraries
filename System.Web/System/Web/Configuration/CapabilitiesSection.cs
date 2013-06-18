namespace System.Web.Configuration
{
    using System;
    using System.Collections;
    using System.Text.RegularExpressions;

    internal class CapabilitiesSection : CapabilitiesRule
    {
        internal CapabilitiesPattern _expr;
        internal DelayedRegex _regex;
        internal CapabilitiesRule[] _rules;

        internal CapabilitiesSection(int type, DelayedRegex regex, CapabilitiesPattern expr, ArrayList rulelist)
        {
            base._type = type;
            this._regex = regex;
            this._expr = expr;
            this._rules = (CapabilitiesRule[]) rulelist.ToArray(typeof(CapabilitiesRule));
        }

        internal override void Evaluate(CapabilitiesState state)
        {
            state.Exit = false;
            if (this._regex != null)
            {
                Match match = this._regex.Match(this._expr.Expand(state));
                if (!match.Success)
                {
                    return;
                }
                state.AddMatch(this._regex, match);
            }
            for (int i = 0; i < this._rules.Length; i++)
            {
                this._rules[i].Evaluate(state);
                if (state.Exit)
                {
                    break;
                }
            }
            if (this._regex != null)
            {
                state.PopMatch();
            }
            state.Exit = this.Type == 3;
        }
    }
}

