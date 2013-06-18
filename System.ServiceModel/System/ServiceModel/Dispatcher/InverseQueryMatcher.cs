namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Xml;

    internal class InverseQueryMatcher : QueryMatcher
    {
        private SubExprEliminator elim = new SubExprEliminator();
        private Dictionary<object, Opcode> lastLookup = new Dictionary<object, Opcode>();
        private bool match;

        internal InverseQueryMatcher(bool match)
        {
            this.match = match;
        }

        internal void Add(string expression, XmlNamespaceManager names, object item, bool forceExternal)
        {
            bool flag = false;
            OpcodeBlock newBlock = new OpcodeBlock();
            newBlock.Append(new NoOpOpcode(OpcodeID.QueryTree));
            if (!forceExternal)
            {
                try
                {
                    MultipleResultOpcode opcode;
                    ValueDataType none = ValueDataType.None;
                    newBlock.Append(QueryMatcher.CompileForInternalEngine(expression, names, QueryCompilerFlags.InverseQuery, out none));
                    if (!this.match)
                    {
                        opcode = new QueryMultipleResultOpcode();
                    }
                    else
                    {
                        opcode = new MatchMultipleResultOpcode();
                    }
                    opcode.AddItem(item);
                    newBlock.Append(opcode);
                    flag = true;
                    newBlock = new OpcodeBlock(this.elim.Add(item, newBlock.First));
                    base.subExprVars = this.elim.VariableCount;
                }
                catch (QueryCompileException)
                {
                }
            }
            if (!flag)
            {
                newBlock.Append(QueryMatcher.CompileForExternalEngine(expression, names, item, this.match));
            }
            QueryTreeBuilder builder = new QueryTreeBuilder();
            base.query = builder.Build(base.query, newBlock);
            this.lastLookup[item] = builder.LastOpcode;
        }

        internal void Clear()
        {
            foreach (object obj2 in this.lastLookup.Keys)
            {
                this.Remove(this.lastLookup[obj2], obj2);
                this.elim.Remove(obj2);
            }
            base.subExprVars = this.elim.VariableCount;
            this.lastLookup.Clear();
        }

        internal void Remove(object item)
        {
            this.Remove(this.lastLookup[item], item);
            this.lastLookup.Remove(item);
            this.elim.Remove(item);
            base.subExprVars = this.elim.VariableCount;
        }

        private void Remove(Opcode opcode, object item)
        {
            MultipleResultOpcode opcode2 = opcode as MultipleResultOpcode;
            if (opcode2 != null)
            {
                opcode2.RemoveItem(item);
            }
            else
            {
                opcode.Remove();
            }
        }

        internal override void Trim()
        {
            base.Trim();
            this.elim.Trim();
        }
    }
}

