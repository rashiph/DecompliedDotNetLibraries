namespace System.Xml.Schema
{
    using System;
    using System.Collections;

    internal class SelectorActiveAxis : ActiveAxis
    {
        private ConstraintStruct cs;
        private int KSpointer;
        private ArrayList KSs;

        public SelectorActiveAxis(Asttree axisTree, ConstraintStruct cs) : base(axisTree)
        {
            this.KSs = new ArrayList();
            this.cs = cs;
        }

        public override bool EndElement(string localname, string URN)
        {
            base.EndElement(localname, URN);
            return ((this.KSpointer > 0) && (base.CurrentDepth == this.lastDepth));
        }

        public KeySequence PopKS()
        {
            return ((KSStruct) this.KSs[--this.KSpointer]).ks;
        }

        public int PushKS(int errline, int errcol)
        {
            KSStruct struct2;
            KeySequence ks = new KeySequence(this.cs.TableDim, errline, errcol);
            if (this.KSpointer < this.KSs.Count)
            {
                struct2 = (KSStruct) this.KSs[this.KSpointer];
                struct2.ks = ks;
                for (int i = 0; i < this.cs.TableDim; i++)
                {
                    struct2.fields[i].Reactivate(ks);
                }
            }
            else
            {
                struct2 = new KSStruct(ks, this.cs.TableDim);
                for (int j = 0; j < this.cs.TableDim; j++)
                {
                    struct2.fields[j] = new LocatedActiveAxis(this.cs.constraint.Fields[j], ks, j);
                    this.cs.axisFields.Add(struct2.fields[j]);
                }
                this.KSs.Add(struct2);
            }
            struct2.depth = base.CurrentDepth - 1;
            return this.KSpointer++;
        }

        public bool EmptyStack
        {
            get
            {
                return (this.KSpointer == 0);
            }
        }

        public int lastDepth
        {
            get
            {
                if (this.KSpointer != 0)
                {
                    return ((KSStruct) this.KSs[this.KSpointer - 1]).depth;
                }
                return -1;
            }
        }
    }
}

