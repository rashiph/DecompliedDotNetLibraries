namespace System.Xml.Schema
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal sealed class NfaContentValidator : ContentValidator
    {
        private int endMarkerPos;
        private BitSet firstpos;
        private BitSet[] followpos;
        private Positions positions;
        private SymbolsDictionary symbols;

        internal NfaContentValidator(BitSet firstpos, BitSet[] followpos, SymbolsDictionary symbols, Positions positions, int endMarkerPos, XmlSchemaContentType contentType, bool isOpen, bool isEmptiable) : base(contentType, isOpen, isEmptiable)
        {
            this.firstpos = firstpos;
            this.followpos = followpos;
            this.symbols = symbols;
            this.positions = positions;
            this.endMarkerPos = endMarkerPos;
        }

        public override bool CompleteValidation(ValidationState context)
        {
            if (!context.CurPos[context.CurrentState.CurPosIndex][this.endMarkerPos])
            {
                return false;
            }
            return true;
        }

        public override ArrayList ExpectedElements(ValidationState context, bool isRequiredOnly)
        {
            ArrayList list = null;
            BitSet set = context.CurPos[context.CurrentState.CurPosIndex];
            for (int i = set.NextSet(-1); i != -1; i = set.NextSet(i))
            {
                if (list == null)
                {
                    list = new ArrayList();
                }
                XmlSchemaParticle particle = (XmlSchemaParticle) this.positions[i].particle;
                if (particle == null)
                {
                    string str = this.symbols.NameOf(this.positions[i].symbol);
                    if (str.Length != 0)
                    {
                        list.Add(str);
                    }
                }
                else
                {
                    string nameString = particle.NameString;
                    if (!list.Contains(nameString))
                    {
                        list.Add(nameString);
                    }
                }
            }
            return list;
        }

        public override ArrayList ExpectedParticles(ValidationState context, bool isRequiredOnly, XmlSchemaSet schemaSet)
        {
            ArrayList particles = new ArrayList();
            BitSet set = context.CurPos[context.CurrentState.CurPosIndex];
            for (int i = set.NextSet(-1); i != -1; i = set.NextSet(i))
            {
                XmlSchemaParticle p = (XmlSchemaParticle) this.positions[i].particle;
                if (p != null)
                {
                    ContentValidator.AddParticleToExpected(p, schemaSet, particles);
                }
            }
            return particles;
        }

        public override void InitValidation(ValidationState context)
        {
            context.CurPos[0] = this.firstpos.Clone();
            context.CurPos[1] = new BitSet(this.firstpos.Count);
            context.CurrentState.CurPosIndex = 0;
        }

        public override object ValidateElement(XmlQualifiedName name, ValidationState context, out int errorCode)
        {
            BitSet set = context.CurPos[context.CurrentState.CurPosIndex];
            int index = (context.CurrentState.CurPosIndex + 1) % 2;
            BitSet set2 = context.CurPos[index];
            set2.Clear();
            int num2 = this.symbols[name];
            object particle = null;
            errorCode = 0;
            for (int i = set.NextSet(-1); i != -1; i = set.NextSet(i))
            {
                if (num2 == this.positions[i].symbol)
                {
                    set2.Or(this.followpos[i]);
                    particle = this.positions[i].particle;
                    break;
                }
            }
            if (!set2.IsEmpty)
            {
                context.CurrentState.CurPosIndex = index;
                return particle;
            }
            if (!base.IsOpen || !set[this.endMarkerPos])
            {
                context.NeedValidateChildren = false;
                errorCode = -1;
            }
            return null;
        }
    }
}

