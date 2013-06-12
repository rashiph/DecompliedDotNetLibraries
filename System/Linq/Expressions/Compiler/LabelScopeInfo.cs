namespace System.Linq.Expressions.Compiler
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Runtime.InteropServices;

    internal sealed class LabelScopeInfo
    {
        internal readonly LabelScopeKind Kind;
        private Dictionary<LabelTarget, LabelInfo> Labels;
        internal readonly LabelScopeInfo Parent;

        internal LabelScopeInfo(LabelScopeInfo parent, LabelScopeKind kind)
        {
            this.Parent = parent;
            this.Kind = kind;
        }

        internal void AddLabelInfo(LabelTarget target, LabelInfo info)
        {
            if (this.Labels == null)
            {
                this.Labels = new Dictionary<LabelTarget, LabelInfo>();
            }
            this.Labels.Add(target, info);
        }

        internal bool ContainsTarget(LabelTarget target)
        {
            if (this.Labels == null)
            {
                return false;
            }
            return this.Labels.ContainsKey(target);
        }

        internal bool TryGetLabelInfo(LabelTarget target, out LabelInfo info)
        {
            if (this.Labels == null)
            {
                info = null;
                return false;
            }
            return this.Labels.TryGetValue(target, out info);
        }

        internal bool CanJumpInto
        {
            get
            {
                switch (this.Kind)
                {
                    case LabelScopeKind.Statement:
                    case LabelScopeKind.Block:
                    case LabelScopeKind.Switch:
                    case LabelScopeKind.Lambda:
                        return true;
                }
                return false;
            }
        }
    }
}

