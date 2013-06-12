namespace System.Xml.Schema
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal sealed class RangeContentValidator : ContentValidator
    {
        private int endMarkerPos;
        private BitSet firstpos;
        private BitSet[] followpos;
        private int minMaxNodesCount;
        private Positions positions;
        private BitSet positionsWithRangeTerminals;
        private SymbolsDictionary symbols;

        internal RangeContentValidator(BitSet firstpos, BitSet[] followpos, SymbolsDictionary symbols, Positions positions, int endMarkerPos, XmlSchemaContentType contentType, bool isEmptiable, BitSet positionsWithRangeTerminals, int minmaxNodesCount) : base(contentType, false, isEmptiable)
        {
            this.firstpos = firstpos;
            this.followpos = followpos;
            this.symbols = symbols;
            this.positions = positions;
            this.positionsWithRangeTerminals = positionsWithRangeTerminals;
            this.minMaxNodesCount = minmaxNodesCount;
            this.endMarkerPos = endMarkerPos;
        }

        public override bool CompleteValidation(ValidationState context)
        {
            return context.HasMatched;
        }

        public override ArrayList ExpectedElements(ValidationState context, bool isRequiredOnly)
        {
            ArrayList list = null;
            if (context.RunningPositions != null)
            {
                List<RangePositionInfo> runningPositions = context.RunningPositions;
                BitSet set = new BitSet(this.positions.Count);
                for (int i = context.CurrentState.NumberOfRunningPos - 1; i >= 0; i--)
                {
                    set.Or(runningPositions[i].curpos);
                }
                for (int j = set.NextSet(-1); j != -1; j = set.NextSet(j))
                {
                    if (list == null)
                    {
                        list = new ArrayList();
                    }
                    if (this.positions[j].symbol >= 0)
                    {
                        XmlSchemaParticle particle = this.positions[j].particle as XmlSchemaParticle;
                        if (particle == null)
                        {
                            string str = this.symbols.NameOf(this.positions[j].symbol);
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
            if (context.RunningPositions != null)
            {
                List<RangePositionInfo> runningPositions = context.RunningPositions;
                BitSet set = new BitSet(this.positions.Count);
                for (int i = context.CurrentState.NumberOfRunningPos - 1; i >= 0; i--)
                {
                    set.Or(runningPositions[i].curpos);
                }
                for (int j = set.NextSet(-1); j != -1; j = set.NextSet(j))
                {
                    if (this.positions[j].symbol >= 0)
                    {
                        XmlSchemaParticle p = this.positions[j].particle as XmlSchemaParticle;
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
            int count = this.positions.Count;
            List<RangePositionInfo> runningPositions = context.RunningPositions;
            if (runningPositions != null)
            {
                runningPositions.Clear();
            }
            else
            {
                runningPositions = new List<RangePositionInfo>();
                context.RunningPositions = runningPositions;
            }
            RangePositionInfo item = new RangePositionInfo {
                curpos = this.firstpos.Clone(),
                rangeCounters = new decimal[this.minMaxNodesCount]
            };
            runningPositions.Add(item);
            context.CurrentState.NumberOfRunningPos = 1;
            context.HasMatched = item.curpos.Get(this.endMarkerPos);
        }

        public override object ValidateElement(XmlQualifiedName name, ValidationState context, out int errorCode)
        {
            RangePositionInfo info;
            errorCode = 0;
            int num = this.symbols[name];
            bool flag = false;
            List<RangePositionInfo> runningPositions = context.RunningPositions;
            int numberOfRunningPos = context.CurrentState.NumberOfRunningPos;
            int count = 0;
            int index = -1;
            int num5 = -1;
            bool flag2 = false;
            while (count < numberOfRunningPos)
            {
                info = runningPositions[count];
                BitSet curpos = info.curpos;
                for (int i = curpos.NextSet(-1); i != -1; i = curpos.NextSet(i))
                {
                    if (num == this.positions[i].symbol)
                    {
                        index = i;
                        if (num5 == -1)
                        {
                            num5 = count;
                        }
                        flag2 = true;
                        break;
                    }
                }
                if (flag2 && (this.positions[index].particle is XmlSchemaElement))
                {
                    break;
                }
                count++;
            }
            if ((count == numberOfRunningPos) && (index != -1))
            {
                count = num5;
            }
            if (count < numberOfRunningPos)
            {
                if (count != 0)
                {
                    runningPositions.RemoveRange(0, count);
                }
                numberOfRunningPos -= count;
                count = 0;
                while (count < numberOfRunningPos)
                {
                    info = runningPositions[count];
                    if (info.curpos.Get(index))
                    {
                        info.curpos = this.followpos[index];
                        runningPositions[count] = info;
                        count++;
                    }
                    else
                    {
                        numberOfRunningPos--;
                        if (numberOfRunningPos > 0)
                        {
                            RangePositionInfo info2 = runningPositions[numberOfRunningPos];
                            runningPositions[numberOfRunningPos] = runningPositions[count];
                            runningPositions[count] = info2;
                        }
                    }
                }
            }
            else
            {
                numberOfRunningPos = 0;
            }
            if (numberOfRunningPos > 0)
            {
                if (numberOfRunningPos >= 0x2710)
                {
                    context.TooComplex = true;
                    numberOfRunningPos /= 2;
                }
                for (count = numberOfRunningPos - 1; count >= 0; count--)
                {
                    int num7 = count;
                    BitSet set2 = runningPositions[count].curpos;
                    flag = flag || set2.Get(this.endMarkerPos);
                    while ((numberOfRunningPos < 0x2710) && set2.Intersects(this.positionsWithRangeTerminals))
                    {
                        BitSet set3 = set2.Clone();
                        set3.And(this.positionsWithRangeTerminals);
                        int num8 = set3.NextSet(-1);
                        LeafRangeNode particle = this.positions[num8].particle as LeafRangeNode;
                        info = runningPositions[num7];
                        if ((numberOfRunningPos + 2) >= runningPositions.Count)
                        {
                            RangePositionInfo item = new RangePositionInfo();
                            runningPositions.Add(item);
                            RangePositionInfo info5 = new RangePositionInfo();
                            runningPositions.Add(info5);
                        }
                        RangePositionInfo info3 = runningPositions[numberOfRunningPos];
                        if (info3.rangeCounters == null)
                        {
                            info3.rangeCounters = new decimal[this.minMaxNodesCount];
                        }
                        Array.Copy(info.rangeCounters, 0, info3.rangeCounters, 0, info.rangeCounters.Length);
                        decimal num9 = info3.rangeCounters[particle.Pos] = decimal.op_Increment(info3.rangeCounters[particle.Pos]);
                        if (num9 == particle.Max)
                        {
                            info3.curpos = this.followpos[num8];
                            info3.rangeCounters[particle.Pos] = 0M;
                            runningPositions[numberOfRunningPos] = info3;
                            num7 = numberOfRunningPos++;
                        }
                        else
                        {
                            if (num9 < particle.Min)
                            {
                                info3.curpos = particle.NextIteration;
                                runningPositions[numberOfRunningPos] = info3;
                                numberOfRunningPos++;
                                break;
                            }
                            info3.curpos = particle.NextIteration;
                            runningPositions[numberOfRunningPos] = info3;
                            num7 = numberOfRunningPos + 1;
                            info3 = runningPositions[num7];
                            if (info3.rangeCounters == null)
                            {
                                info3.rangeCounters = new decimal[this.minMaxNodesCount];
                            }
                            Array.Copy(info.rangeCounters, 0, info3.rangeCounters, 0, info.rangeCounters.Length);
                            info3.curpos = this.followpos[num8];
                            info3.rangeCounters[particle.Pos] = 0M;
                            runningPositions[num7] = info3;
                            numberOfRunningPos += 2;
                        }
                        set2 = runningPositions[num7].curpos;
                        flag = flag || set2.Get(this.endMarkerPos);
                    }
                }
                context.HasMatched = flag;
                context.CurrentState.NumberOfRunningPos = numberOfRunningPos;
                return this.positions[index].particle;
            }
            errorCode = -1;
            context.NeedValidateChildren = false;
            return null;
        }
    }
}

