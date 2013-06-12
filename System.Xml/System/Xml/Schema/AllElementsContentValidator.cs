namespace System.Xml.Schema
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal sealed class AllElementsContentValidator : ContentValidator
    {
        private int countRequired;
        private Hashtable elements;
        private BitSet isRequired;
        private object[] particles;

        public AllElementsContentValidator(XmlSchemaContentType contentType, int size, bool isEmptiable) : base(contentType, false, isEmptiable)
        {
            this.elements = new Hashtable(size);
            this.particles = new object[size];
            this.isRequired = new BitSet(size);
        }

        public bool AddElement(XmlQualifiedName name, object particle, bool isEmptiable)
        {
            if (this.elements[name] != null)
            {
                return false;
            }
            int count = this.elements.Count;
            this.elements.Add(name, count);
            this.particles[count] = particle;
            if (!isEmptiable)
            {
                this.isRequired.Set(count);
                this.countRequired++;
            }
            return true;
        }

        public override bool CompleteValidation(ValidationState context)
        {
            if ((context.CurrentState.AllElementsRequired != this.countRequired) && (!this.IsEmptiable || (context.CurrentState.AllElementsRequired != -1)))
            {
                return false;
            }
            return true;
        }

        public override ArrayList ExpectedElements(ValidationState context, bool isRequiredOnly)
        {
            ArrayList list = null;
            foreach (DictionaryEntry entry in this.elements)
            {
                if (!context.AllElementsSet[(int) entry.Value] && (!isRequiredOnly || this.isRequired[(int) entry.Value]))
                {
                    if (list == null)
                    {
                        list = new ArrayList();
                    }
                    list.Add(entry.Key);
                }
            }
            return list;
        }

        public override ArrayList ExpectedParticles(ValidationState context, bool isRequiredOnly, XmlSchemaSet schemaSet)
        {
            ArrayList particles = new ArrayList();
            foreach (DictionaryEntry entry in this.elements)
            {
                if (!context.AllElementsSet[(int) entry.Value] && (!isRequiredOnly || this.isRequired[(int) entry.Value]))
                {
                    ContentValidator.AddParticleToExpected(this.particles[(int) entry.Value] as XmlSchemaParticle, schemaSet, particles);
                }
            }
            return particles;
        }

        public override void InitValidation(ValidationState context)
        {
            context.AllElementsSet = new BitSet(this.elements.Count);
            context.CurrentState.AllElementsRequired = -1;
        }

        public override object ValidateElement(XmlQualifiedName name, ValidationState context, out int errorCode)
        {
            object obj2 = this.elements[name];
            errorCode = 0;
            if (obj2 == null)
            {
                context.NeedValidateChildren = false;
                return null;
            }
            int index = (int) obj2;
            if (context.AllElementsSet[index])
            {
                errorCode = -2;
                return null;
            }
            if (context.CurrentState.AllElementsRequired == -1)
            {
                context.CurrentState.AllElementsRequired = 0;
            }
            context.AllElementsSet.Set(index);
            if (this.isRequired[index])
            {
                context.CurrentState.AllElementsRequired++;
            }
            return this.particles[index];
        }

        public override bool IsEmptiable
        {
            get
            {
                if (!base.IsEmptiable)
                {
                    return (this.countRequired == 0);
                }
                return true;
            }
        }
    }
}

