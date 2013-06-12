namespace System.Linq.Expressions.Compiler
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic.Utils;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection.Emit;

    internal sealed class LabelInfo
    {
        private bool _acrossBlockJump;
        private readonly bool _canReturn;
        private readonly System.Linq.Expressions.Set<LabelScopeInfo> _definitions = new System.Linq.Expressions.Set<LabelScopeInfo>();
        private readonly ILGenerator _ilg;
        private System.Reflection.Emit.Label _label;
        private bool _labelDefined;
        private readonly LabelTarget _node;
        private OpCode _opCode = OpCodes.Leave;
        private readonly List<LabelScopeInfo> _references = new List<LabelScopeInfo>();
        private LocalBuilder _value;

        internal LabelInfo(ILGenerator il, LabelTarget node, bool canReturn)
        {
            this._ilg = il;
            this._node = node;
            this._canReturn = canReturn;
        }

        internal void Define(LabelScopeInfo block)
        {
            for (LabelScopeInfo info = block; info != null; info = info.Parent)
            {
                if (info.ContainsTarget(this._node))
                {
                    throw System.Linq.Expressions.Error.LabelTargetAlreadyDefined(this._node.Name);
                }
            }
            this._definitions.Add(block);
            block.AddLabelInfo(this._node, this);
            if (this._definitions.Count == 1)
            {
                foreach (LabelScopeInfo info2 in this._references)
                {
                    this.ValidateJump(info2);
                }
            }
            else
            {
                if (this._acrossBlockJump)
                {
                    throw System.Linq.Expressions.Error.AmbiguousJump(this._node.Name);
                }
                this._labelDefined = false;
            }
        }

        internal void EmitJump()
        {
            if (this._opCode == OpCodes.Ret)
            {
                this._ilg.Emit(OpCodes.Ret);
            }
            else
            {
                this.StoreValue();
                this._ilg.Emit(this._opCode, this.Label);
            }
        }

        private void EnsureLabelAndValue()
        {
            if (!this._labelDefined)
            {
                this._labelDefined = true;
                this._label = this._ilg.DefineLabel();
                if ((this._node != null) && (this._node.Type != typeof(void)))
                {
                    this._value = this._ilg.DeclareLocal(this._node.Type);
                }
            }
        }

        internal void Mark()
        {
            if (this._canReturn)
            {
                if (!this._labelDefined)
                {
                    return;
                }
                this._ilg.Emit(OpCodes.Ret);
            }
            else
            {
                this.StoreValue();
            }
            this.MarkWithEmptyStack();
        }

        internal void MarkWithEmptyStack()
        {
            this._ilg.MarkLabel(this.Label);
            if (this._value != null)
            {
                this._ilg.Emit(OpCodes.Ldloc, this._value);
            }
        }

        internal void Reference(LabelScopeInfo block)
        {
            this._references.Add(block);
            if (this._definitions.Count > 0)
            {
                this.ValidateJump(block);
            }
        }

        private void StoreValue()
        {
            this.EnsureLabelAndValue();
            if (this._value != null)
            {
                this._ilg.Emit(OpCodes.Stloc, this._value);
            }
        }

        internal void ValidateFinish()
        {
            if ((this._references.Count > 0) && (this._definitions.Count == 0))
            {
                throw System.Linq.Expressions.Error.LabelTargetUndefined(this._node.Name);
            }
        }

        private void ValidateJump(LabelScopeInfo reference)
        {
            this._opCode = this._canReturn ? OpCodes.Ret : OpCodes.Br;
            for (LabelScopeInfo info = reference; info != null; info = info.Parent)
            {
                if (this._definitions.Contains(info))
                {
                    return;
                }
                if ((info.Kind == LabelScopeKind.Finally) || (info.Kind == LabelScopeKind.Filter))
                {
                    break;
                }
                if ((info.Kind == LabelScopeKind.Try) || (info.Kind == LabelScopeKind.Catch))
                {
                    this._opCode = OpCodes.Leave;
                }
            }
            this._acrossBlockJump = true;
            if ((this._node != null) && (this._node.Type != typeof(void)))
            {
                throw System.Linq.Expressions.Error.NonLocalJumpWithValue(this._node.Name);
            }
            if (this._definitions.Count > 1)
            {
                throw System.Linq.Expressions.Error.AmbiguousJump(this._node.Name);
            }
            LabelScopeInfo first = this._definitions.First<LabelScopeInfo>();
            LabelScopeInfo info3 = Helpers.CommonNode<LabelScopeInfo>(first, reference, b => b.Parent);
            this._opCode = this._canReturn ? OpCodes.Ret : OpCodes.Br;
            for (LabelScopeInfo info4 = reference; info4 != info3; info4 = info4.Parent)
            {
                if (info4.Kind == LabelScopeKind.Finally)
                {
                    throw System.Linq.Expressions.Error.ControlCannotLeaveFinally();
                }
                if (info4.Kind == LabelScopeKind.Filter)
                {
                    throw System.Linq.Expressions.Error.ControlCannotLeaveFilterTest();
                }
                if ((info4.Kind == LabelScopeKind.Try) || (info4.Kind == LabelScopeKind.Catch))
                {
                    this._opCode = OpCodes.Leave;
                }
            }
            for (LabelScopeInfo info5 = first; info5 != info3; info5 = info5.Parent)
            {
                if (!info5.CanJumpInto)
                {
                    if (info5.Kind == LabelScopeKind.Expression)
                    {
                        throw System.Linq.Expressions.Error.ControlCannotEnterExpression();
                    }
                    throw System.Linq.Expressions.Error.ControlCannotEnterTry();
                }
            }
        }

        internal bool CanBranch
        {
            get
            {
                return (this._opCode != OpCodes.Leave);
            }
        }

        internal bool CanReturn
        {
            get
            {
                return this._canReturn;
            }
        }

        internal System.Reflection.Emit.Label Label
        {
            get
            {
                this.EnsureLabelAndValue();
                return this._label;
            }
        }
    }
}

