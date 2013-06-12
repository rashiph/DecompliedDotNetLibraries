namespace System.Xml.Schema
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal sealed class ParticleContentValidator : ContentValidator
    {
        private SyntaxTreeNode contentNode;
        private bool enableUpaCheck;
        private bool isPartial;
        private int minMaxNodesCount;
        private Positions positions;
        private Stack stack;
        private SymbolsDictionary symbols;

        public ParticleContentValidator(XmlSchemaContentType contentType) : this(contentType, true)
        {
        }

        public ParticleContentValidator(XmlSchemaContentType contentType, bool enableUpaCheck) : base(contentType)
        {
            this.enableUpaCheck = enableUpaCheck;
        }

        public void AddChoice()
        {
            SyntaxTreeNode node = (SyntaxTreeNode) this.stack.Pop();
            InteriorNode node2 = new ChoiceNode {
                LeftChild = node
            };
            this.stack.Push(node2);
        }

        private void AddLeafNode(SyntaxTreeNode node)
        {
            if (this.stack.Count > 0)
            {
                InteriorNode node2 = (InteriorNode) this.stack.Pop();
                if (node2 != null)
                {
                    node2.RightChild = node;
                    node = node2;
                }
            }
            this.stack.Push(node);
            this.isPartial = true;
        }

        public void AddLeafRange(decimal min, decimal max)
        {
            LeafRangeNode particle = new LeafRangeNode(min, max);
            int num = this.positions.Add(-2, particle);
            particle.Pos = num;
            InteriorNode node = new SequenceNode {
                RightChild = particle
            };
            this.Closure(node);
            this.minMaxNodesCount++;
        }

        public void AddName(XmlQualifiedName name, object particle)
        {
            this.AddLeafNode(new LeafNode(this.positions.Add(this.symbols.AddName(name, particle), particle)));
        }

        public void AddNamespaceList(NamespaceList namespaceList, object particle)
        {
            this.symbols.AddNamespaceList(namespaceList, particle, false);
            this.AddLeafNode(new NamespaceListNode(namespaceList, particle));
        }

        public void AddPlus()
        {
            this.Closure(new PlusNode());
        }

        public void AddQMark()
        {
            this.Closure(new QmarkNode());
        }

        public void AddSequence()
        {
            SyntaxTreeNode node = (SyntaxTreeNode) this.stack.Pop();
            InteriorNode node2 = new SequenceNode {
                LeftChild = node
            };
            this.stack.Push(node2);
        }

        public void AddStar()
        {
            this.Closure(new StarNode());
        }

        private int[][] BuildTransitionTable(BitSet firstpos, BitSet[] followpos, int endMarkerPos)
        {
            int count = this.positions.Count;
            int num2 = 0x2000 / count;
            int index = this.symbols.Count;
            ArrayList list = new ArrayList();
            Hashtable hashtable = new Hashtable();
            hashtable.Add(new BitSet(count), -1);
            Queue queue = new Queue();
            int num4 = 0;
            queue.Enqueue(firstpos);
            hashtable.Add(firstpos, 0);
            list.Add(new int[index + 1]);
            while (queue.Count > 0)
            {
                BitSet set = (BitSet) queue.Dequeue();
                int[] numArray = (int[]) list[num4];
                if (set[endMarkerPos])
                {
                    numArray[index] = 1;
                }
                for (int i = 0; i < index; i++)
                {
                    BitSet set2 = new BitSet(count);
                    for (int j = set.NextSet(-1); j != -1; j = set.NextSet(j))
                    {
                        if (i == this.positions[j].symbol)
                        {
                            set2.Or(followpos[j]);
                        }
                    }
                    object obj2 = hashtable[set2];
                    if (obj2 != null)
                    {
                        numArray[i] = (int) obj2;
                    }
                    else
                    {
                        int num7 = hashtable.Count - 1;
                        if (num7 >= num2)
                        {
                            return null;
                        }
                        queue.Enqueue(set2);
                        hashtable.Add(set2, num7);
                        list.Add(new int[index + 1]);
                        numArray[i] = num7;
                    }
                }
                num4++;
            }
            return (int[][]) list.ToArray(typeof(int[]));
        }

        private BitSet[] CalculateTotalFollowposForRangeNodes(BitSet firstpos, BitSet[] followpos, out BitSet posWithRangeTerminals)
        {
            int count = this.positions.Count;
            posWithRangeTerminals = new BitSet(count);
            BitSet[] setArray = new BitSet[this.minMaxNodesCount];
            int index = 0;
            for (int i = count - 1; i >= 0; i--)
            {
                Position position = this.positions[i];
                if (position.symbol == -2)
                {
                    LeafRangeNode particle = position.particle as LeafRangeNode;
                    BitSet set = new BitSet(count);
                    set.Clear();
                    set.Or(followpos[i]);
                    if (particle.Min != particle.Max)
                    {
                        set.Or(particle.NextIteration);
                    }
                    for (int j = set.NextSet(-1); j != -1; j = set.NextSet(j))
                    {
                        if (j > i)
                        {
                            Position position2 = this.positions[j];
                            if (position2.symbol == -2)
                            {
                                LeafRangeNode node2 = position2.particle as LeafRangeNode;
                                set.Or(setArray[node2.Pos]);
                            }
                        }
                    }
                    setArray[index] = set;
                    particle.Pos = index++;
                    posWithRangeTerminals.Set(i);
                }
            }
            return setArray;
        }

        private void CheckCMUPAWithLeafRangeNodes(BitSet curpos)
        {
            object[] objArray = new object[this.symbols.Count];
            for (int i = curpos.NextSet(-1); i != -1; i = curpos.NextSet(i))
            {
                Position position = this.positions[i];
                int symbol = position.symbol;
                if (symbol >= 0)
                {
                    if (objArray[symbol] != null)
                    {
                        throw new UpaException(objArray[symbol], position.particle);
                    }
                    objArray[symbol] = position.particle;
                }
            }
        }

        private void CheckUniqueParticleAttribution(BitSet curpos)
        {
            object[] objArray = new object[this.symbols.Count];
            for (int i = curpos.NextSet(-1); i != -1; i = curpos.NextSet(i))
            {
                int symbol = this.positions[i].symbol;
                if (objArray[symbol] == null)
                {
                    objArray[symbol] = this.positions[i].particle;
                }
                else if (objArray[symbol] != this.positions[i].particle)
                {
                    throw new UpaException(objArray[symbol], this.positions[i].particle);
                }
            }
        }

        private void CheckUniqueParticleAttribution(BitSet firstpos, BitSet[] followpos)
        {
            this.CheckUniqueParticleAttribution(firstpos);
            for (int i = 0; i < this.positions.Count; i++)
            {
                this.CheckUniqueParticleAttribution(followpos[i]);
            }
        }

        public void CloseGroup()
        {
            SyntaxTreeNode node = (SyntaxTreeNode) this.stack.Pop();
            if (node != null)
            {
                if (this.stack.Count == 0)
                {
                    this.contentNode = node;
                    this.isPartial = false;
                }
                else
                {
                    InteriorNode node2 = (InteriorNode) this.stack.Pop();
                    if (node2 != null)
                    {
                        node2.RightChild = node;
                        node = node2;
                        this.isPartial = true;
                    }
                    else
                    {
                        this.isPartial = false;
                    }
                    this.stack.Push(node);
                }
            }
        }

        private void Closure(InteriorNode node)
        {
            if (this.stack.Count > 0)
            {
                SyntaxTreeNode node2 = (SyntaxTreeNode) this.stack.Pop();
                InteriorNode node3 = node2 as InteriorNode;
                if (this.isPartial && (node3 != null))
                {
                    node.LeftChild = node3.RightChild;
                    node3.RightChild = node;
                }
                else
                {
                    node.LeftChild = node2;
                    node2 = node;
                }
                this.stack.Push(node2);
            }
            else if (this.contentNode != null)
            {
                node.LeftChild = this.contentNode;
                this.contentNode = node;
            }
        }

        public override bool CompleteValidation(ValidationState context)
        {
            throw new InvalidOperationException();
        }

        public bool Exists(XmlQualifiedName name)
        {
            return this.symbols.Exists(name);
        }

        public ContentValidator Finish()
        {
            return this.Finish(true);
        }

        public ContentValidator Finish(bool useDFA)
        {
            if (this.contentNode == null)
            {
                if (base.ContentType != XmlSchemaContentType.Mixed)
                {
                    return ContentValidator.Empty;
                }
                bool isOpen = base.IsOpen;
                if (!base.IsOpen)
                {
                    return ContentValidator.TextOnly;
                }
                return ContentValidator.Any;
            }
            InteriorNode parent = new SequenceNode {
                LeftChild = this.contentNode
            };
            LeafNode node2 = new LeafNode(this.positions.Add(this.symbols.AddName(XmlQualifiedName.Empty, null), null));
            parent.RightChild = node2;
            this.contentNode.ExpandTree(parent, this.symbols, this.positions);
            int count = this.symbols.Count;
            int num = this.positions.Count;
            BitSet firstpos = new BitSet(num);
            BitSet lastpos = new BitSet(num);
            BitSet[] followpos = new BitSet[num];
            for (int i = 0; i < num; i++)
            {
                followpos[i] = new BitSet(num);
            }
            parent.ConstructPos(firstpos, lastpos, followpos);
            if (this.minMaxNodesCount > 0)
            {
                BitSet set3;
                BitSet[] minmaxFollowPos = this.CalculateTotalFollowposForRangeNodes(firstpos, followpos, out set3);
                if (this.enableUpaCheck)
                {
                    this.CheckCMUPAWithLeafRangeNodes(this.GetApplicableMinMaxFollowPos(firstpos, set3, minmaxFollowPos));
                    for (int j = 0; j < num; j++)
                    {
                        this.CheckCMUPAWithLeafRangeNodes(this.GetApplicableMinMaxFollowPos(followpos[j], set3, minmaxFollowPos));
                    }
                }
                return new RangeContentValidator(firstpos, followpos, this.symbols, this.positions, node2.Pos, base.ContentType, parent.LeftChild.IsNullable, set3, this.minMaxNodesCount);
            }
            int[][] transitionTable = null;
            if (!this.symbols.IsUpaEnforced)
            {
                if (this.enableUpaCheck)
                {
                    this.CheckUniqueParticleAttribution(firstpos, followpos);
                }
            }
            else if (useDFA)
            {
                transitionTable = this.BuildTransitionTable(firstpos, followpos, node2.Pos);
            }
            if (transitionTable != null)
            {
                return new DfaContentValidator(transitionTable, this.symbols, base.ContentType, base.IsOpen, parent.LeftChild.IsNullable);
            }
            return new NfaContentValidator(firstpos, followpos, this.symbols, this.positions, node2.Pos, base.ContentType, base.IsOpen, parent.LeftChild.IsNullable);
        }

        private BitSet GetApplicableMinMaxFollowPos(BitSet curpos, BitSet posWithRangeTerminals, BitSet[] minmaxFollowPos)
        {
            if (curpos.Intersects(posWithRangeTerminals))
            {
                BitSet set = new BitSet(this.positions.Count);
                set.Or(curpos);
                set.And(posWithRangeTerminals);
                curpos = curpos.Clone();
                for (int i = set.NextSet(-1); i != -1; i = set.NextSet(i))
                {
                    LeafRangeNode particle = this.positions[i].particle as LeafRangeNode;
                    curpos.Or(minmaxFollowPos[particle.Pos]);
                }
            }
            return curpos;
        }

        public override void InitValidation(ValidationState context)
        {
            throw new InvalidOperationException();
        }

        public void OpenGroup()
        {
            this.stack.Push(null);
        }

        public void Start()
        {
            this.symbols = new SymbolsDictionary();
            this.positions = new Positions();
            this.stack = new Stack();
        }

        public override object ValidateElement(XmlQualifiedName name, ValidationState context, out int errorCode)
        {
            throw new InvalidOperationException();
        }
    }
}

