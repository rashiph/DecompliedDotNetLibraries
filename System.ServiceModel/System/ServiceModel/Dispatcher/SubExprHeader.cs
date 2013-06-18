namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Xml.XPath;

    internal class SubExprHeader : SubExpr
    {
        private Dictionary<SubExpr, MyInt> indexLookup;
        private Dictionary<string, Dictionary<string, List<SubExpr>>> nameLookup;

        internal SubExprHeader(Opcode ops, int var) : base(null, ops, var)
        {
            this.nameLookup = new Dictionary<string, Dictionary<string, List<SubExpr>>>();
            this.indexLookup = new Dictionary<SubExpr, MyInt>();
            base.IncRef();
        }

        internal override void AddChild(SubExpr expr)
        {
            base.AddChild(expr);
            this.RebuildIndex();
            if (expr.useSpecial)
            {
                Dictionary<string, List<SubExpr>> dictionary;
                NodeQName qName = ((SelectOpcode) expr.FirstOp).Criteria.QName;
                string key = qName.Namespace;
                if (!this.nameLookup.TryGetValue(key, out dictionary))
                {
                    dictionary = new Dictionary<string, List<SubExpr>>();
                    this.nameLookup.Add(key, dictionary);
                }
                string name = qName.Name;
                List<SubExpr> list = new List<SubExpr>();
                if (!dictionary.TryGetValue(name, out list))
                {
                    list = new List<SubExpr>();
                    dictionary.Add(name, list);
                }
                list.Add(expr);
            }
        }

        internal override void EvalSpecial(ProcessingContext context)
        {
            int counterMarker = context.Processor.CounterMarker;
            if (!context.LoadVariable(base.var))
            {
                XPathMessageContext.HeaderFun.InvokeInternal(context, 0);
                context.SaveVariable(base.var, context.Processor.ElapsedCount(counterMarker));
            }
            NodeSequence[] sequenceArray = new NodeSequence[base.children.Count];
            NodeSequence sequence = context.Sequences[context.TopSequenceArg.basePtr].Sequence;
            for (int i = 0; i < base.children.Count; i++)
            {
                sequenceArray[i] = context.CreateSequence();
                sequenceArray[i].StartNodeset();
            }
            SeekableXPathNavigator node = sequence[0].GetNavigator();
            if (node.MoveToFirstChild())
            {
                do
                {
                    if (node.NodeType == XPathNodeType.Element)
                    {
                        List<SubExpr> list;
                        Dictionary<string, List<SubExpr>> dictionary;
                        string localName = node.LocalName;
                        string namespaceURI = node.NamespaceURI;
                        if (this.nameLookup.TryGetValue(namespaceURI, out dictionary))
                        {
                            if (dictionary.TryGetValue(localName, out list))
                            {
                                for (int k = 0; k < list.Count; k++)
                                {
                                    sequenceArray[this.indexLookup[list[k]].i].Add(node);
                                }
                            }
                            if (dictionary.TryGetValue(QueryDataModel.Wildcard, out list))
                            {
                                for (int m = 0; m < list.Count; m++)
                                {
                                    sequenceArray[this.indexLookup[list[m]].i].Add(node);
                                }
                            }
                        }
                        if (this.nameLookup.TryGetValue(QueryDataModel.Wildcard, out dictionary) && dictionary.TryGetValue(QueryDataModel.Wildcard, out list))
                        {
                            for (int n = 0; n < list.Count; n++)
                            {
                                sequenceArray[this.indexLookup[list[n]].i].Add(node);
                            }
                        }
                    }
                }
                while (node.MoveToNext());
            }
            int num6 = context.Processor.CounterMarker;
            for (int j = 0; j < base.children.Count; j++)
            {
                if (base.children[j].useSpecial)
                {
                    sequenceArray[j].StopNodeset();
                    context.Processor.CounterMarker = num6;
                    context.PushSequenceFrame();
                    context.PushSequence(sequenceArray[j]);
                    for (Opcode opcode = base.children[j].FirstOp.Next; opcode != null; opcode = opcode.Eval(context))
                    {
                    }
                    context.SaveVariable(base.children[j].var, context.Processor.ElapsedCount(counterMarker));
                    context.PopSequenceFrame();
                }
                else
                {
                    context.ReleaseSequence(sequenceArray[j]);
                }
            }
            context.Processor.CounterMarker = counterMarker;
        }

        internal void RebuildIndex()
        {
            this.indexLookup.Clear();
            for (int i = 0; i < base.children.Count; i++)
            {
                this.indexLookup.Add(base.children[i], new MyInt(i));
            }
        }

        internal override void RemoveChild(SubExpr expr)
        {
            base.RemoveChild(expr);
            this.RebuildIndex();
            if (expr.useSpecial)
            {
                Dictionary<string, List<SubExpr>> dictionary;
                NodeQName qName = ((SelectOpcode) expr.FirstOp).Criteria.QName;
                string key = qName.Namespace;
                if (this.nameLookup.TryGetValue(key, out dictionary))
                {
                    List<SubExpr> list;
                    string name = qName.Name;
                    if (dictionary.TryGetValue(name, out list))
                    {
                        list.Remove(expr);
                        if (list.Count == 0)
                        {
                            dictionary.Remove(name);
                        }
                    }
                    if (dictionary.Count == 0)
                    {
                        this.nameLookup.Remove(key);
                    }
                }
            }
        }

        internal class MyInt
        {
            internal int i;

            internal MyInt(int i)
            {
                this.i = i;
            }
        }
    }
}

