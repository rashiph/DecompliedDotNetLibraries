namespace System.Xml.Schema
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal sealed class DfaContentValidator : ContentValidator
    {
        private SymbolsDictionary symbols;
        private int[][] transitionTable;

        internal DfaContentValidator(int[][] transitionTable, SymbolsDictionary symbols, XmlSchemaContentType contentType, bool isOpen, bool isEmptiable) : base(contentType, isOpen, isEmptiable)
        {
            this.transitionTable = transitionTable;
            this.symbols = symbols;
        }

        public override bool CompleteValidation(ValidationState context)
        {
            if (!context.HasMatched)
            {
                return false;
            }
            return true;
        }

        public override ArrayList ExpectedElements(ValidationState context, bool isRequiredOnly)
        {
            ArrayList list = null;
            int[] numArray = this.transitionTable[context.CurrentState.State];
            if (numArray != null)
            {
                for (int i = 0; i < (numArray.Length - 1); i++)
                {
                    if (numArray[i] != -1)
                    {
                        if (list == null)
                        {
                            list = new ArrayList();
                        }
                        XmlSchemaParticle particle = (XmlSchemaParticle) this.symbols.GetParticle(i);
                        if (particle == null)
                        {
                            string str = this.symbols.NameOf(i);
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
                }
            }
            return list;
        }

        public override ArrayList ExpectedParticles(ValidationState context, bool isRequiredOnly, XmlSchemaSet schemaSet)
        {
            ArrayList particles = new ArrayList();
            int[] numArray = this.transitionTable[context.CurrentState.State];
            if (numArray != null)
            {
                for (int i = 0; i < (numArray.Length - 1); i++)
                {
                    if (numArray[i] != -1)
                    {
                        XmlSchemaParticle p = (XmlSchemaParticle) this.symbols.GetParticle(i);
                        if (p != null)
                        {
                            ContentValidator.AddParticleToExpected(p, schemaSet, particles);
                        }
                    }
                }
            }
            return particles;
        }

        public override void InitValidation(ValidationState context)
        {
            context.CurrentState.State = 0;
            context.HasMatched = this.transitionTable[0][this.symbols.Count] > 0;
        }

        public override object ValidateElement(XmlQualifiedName name, ValidationState context, out int errorCode)
        {
            int index = this.symbols[name];
            int num2 = this.transitionTable[context.CurrentState.State][index];
            errorCode = 0;
            if (num2 != -1)
            {
                context.CurrentState.State = num2;
                context.HasMatched = this.transitionTable[context.CurrentState.State][this.symbols.Count] > 0;
                return this.symbols.GetParticle(index);
            }
            if (!base.IsOpen || !context.HasMatched)
            {
                context.NeedValidateChildren = false;
                errorCode = -1;
            }
            return null;
        }
    }
}

