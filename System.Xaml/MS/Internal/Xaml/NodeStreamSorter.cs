namespace MS.Internal.Xaml
{
    using MS.Internal.Xaml.Context;
    using MS.Internal.Xaml.Parser;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Xaml;

    internal class NodeStreamSorter : IEnumerator<XamlNode>, IDisposable, IEnumerator
    {
        private Queue<XamlNode> _buffer;
        private XamlParserContext _context;
        private XamlNode _current;
        private List<int> _moveList;
        private XamlNode[] _originalNodesInOrder;
        private List<SeenCtorDirectiveFlags> _seenStack = new List<SeenCtorDirectiveFlags>();
        private XamlXmlReaderSettings _settings;
        private ReorderInfo[] _sortingInfoArray;
        private IEnumerator<XamlNode> _source;
        private int _startObjectDepth;
        private Dictionary<string, string> _xmlnsDictionary;

        public NodeStreamSorter(XamlParserContext context, XamlPullParser parser, XamlXmlReaderSettings settings, Dictionary<string, string> xmlnsDictionary)
        {
            this._context = context;
            this._settings = settings;
            this._source = parser.Parse().GetEnumerator();
            this._xmlnsDictionary = xmlnsDictionary;
            this._buffer = new Queue<XamlNode>();
            this._sortingInfoArray = null;
            this.StartNewNodeStreamWithSettingsPreamble();
            this.ReadAheadAndSortCtorProperties();
        }

        private int AdvanceOverNoninstancingDirectives(int start, int depth)
        {
            int index = start;
            int end = index;
            int originalOrderIndex = this._sortingInfoArray[index].OriginalOrderIndex;
            for (XamlMember member = this._originalNodesInOrder[originalOrderIndex].Member; !this.IsInstancingMember(member); member = this._originalNodesInOrder[originalOrderIndex].Member)
            {
                if (!this.AdvanceTo(index, XamlNodeType.StartMember, depth, out end) && this.AdvanceTo(index, XamlNodeType.EndObject, depth, out end))
                {
                    return (end - start);
                }
                index = end;
                originalOrderIndex = this._sortingInfoArray[index].OriginalOrderIndex;
            }
            return (end - start);
        }

        private bool AdvanceTo(int start, XamlNodeType nodeType, int searchDepth, out int end)
        {
            for (int i = start + 1; i < this._sortingInfoArray.Length; i++)
            {
                XamlNodeType xamlNodeType = this._sortingInfoArray[i].XamlNodeType;
                int depth = this._sortingInfoArray[i].Depth;
                if (depth == searchDepth)
                {
                    if (xamlNodeType == nodeType)
                    {
                        end = i;
                        return true;
                    }
                }
                else if (depth < searchDepth)
                {
                    end = i;
                    return false;
                }
            }
            end = this._sortingInfoArray.Length;
            return false;
        }

        private bool AdvanceToNextCtorDirective(int current, int depth, out int end)
        {
            end = current;
            int originalOrderIndex = this._sortingInfoArray[current].OriginalOrderIndex;
            for (XamlMember member = this._originalNodesInOrder[originalOrderIndex].Member; !this.IsCtorDirective(member); member = this._originalNodesInOrder[originalOrderIndex].Member)
            {
                if (!this.AdvanceTo(current, XamlNodeType.StartMember, depth, out end))
                {
                    return false;
                }
                current = end;
                originalOrderIndex = this._sortingInfoArray[current].OriginalOrderIndex;
            }
            return true;
        }

        private bool AdvanceToNextInstancingMember(int current, int depth, out int end)
        {
            end = current;
            int originalOrderIndex = this._sortingInfoArray[current].OriginalOrderIndex;
            for (XamlMember member = this._originalNodesInOrder[originalOrderIndex].Member; !this.IsInstancingMember(member); member = this._originalNodesInOrder[originalOrderIndex].Member)
            {
                if (!this.AdvanceTo(current, XamlNodeType.StartMember, depth, out end))
                {
                    return false;
                }
                current = end;
                originalOrderIndex = this._sortingInfoArray[current].OriginalOrderIndex;
            }
            return true;
        }

        private bool BackupTo(int start, XamlNodeType nodeType, int searchDepth, out int end)
        {
            for (int i = start - 1; i >= 0; i--)
            {
                XamlNodeType xamlNodeType = this._sortingInfoArray[i].XamlNodeType;
                int depth = this._sortingInfoArray[i].Depth;
                if (depth == searchDepth)
                {
                    if (xamlNodeType == nodeType)
                    {
                        end = i;
                        return true;
                    }
                    if (depth < searchDepth)
                    {
                        end = i;
                        return false;
                    }
                }
            }
            end = 0;
            return false;
        }

        private void BuildSortingBuffer()
        {
            this._originalNodesInOrder = this._buffer.ToArray();
            this._buffer.Clear();
            this._sortingInfoArray = new ReorderInfo[this._originalNodesInOrder.Length];
            int num = 0;
            ReorderInfo info = new ReorderInfo();
            for (int i = 0; i < this._originalNodesInOrder.Length; i++)
            {
                info.Depth = num;
                info.OriginalOrderIndex = i;
                info.XamlNodeType = this._originalNodesInOrder[i].NodeType;
                switch (info.XamlNodeType)
                {
                    case XamlNodeType.StartObject:
                    case XamlNodeType.GetObject:
                        info.Depth = ++num;
                        break;

                    case XamlNodeType.EndObject:
                        info.Depth = num--;
                        break;
                }
                this._sortingInfoArray[i] = info;
            }
        }

        private bool CheckForOutOfOrderCtorDirectives(XamlNode node)
        {
            XamlMember member = node.Member;
            bool flag = false;
            if (this.IsCtorDirective(member))
            {
                if (this.HaveSeenInstancingProperty)
                {
                    this.HaveSeenOutOfOrderCtorDirective = true;
                    if (this._moveList == null)
                    {
                        this._moveList = new List<int>();
                    }
                    this._moveList.Add(this._buffer.Count);
                }
                return flag;
            }
            if (member.IsDirective && (member == XamlLanguage.Key))
            {
                return flag;
            }
            this.HaveSeenInstancingProperty = true;
            return true;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        private void EndObjectFrame()
        {
            this._startObjectDepth--;
        }

        private void EnqueueInitialExtraXmlNses()
        {
            if (this._xmlnsDictionary != null)
            {
                foreach (string str in this._xmlnsDictionary.Keys)
                {
                    if (this._context.FindNamespaceByPrefixInParseStack(str) == null)
                    {
                        string ns = this._xmlnsDictionary[str];
                        XamlNode item = new XamlNode(XamlNodeType.NamespaceDeclaration, new NamespaceDeclaration(ns, str));
                        this._buffer.Enqueue(item);
                    }
                }
            }
        }

        private void EnqueueInitialXmlState()
        {
            this._context.FindNamespaceByPrefix("xml");
            XamlSchemaContext schemaContext = this._context.SchemaContext;
            if (this._settings.XmlSpacePreserve)
            {
                this.EnqueueOneXmlDirectiveProperty(XamlLanguage.Space, "preserve");
            }
            if (!string.IsNullOrEmpty(this._settings.XmlLang))
            {
                this.EnqueueOneXmlDirectiveProperty(XamlLanguage.Lang, this._settings.XmlLang);
            }
            if (this._settings.BaseUri != null)
            {
                this.EnqueueOneXmlDirectiveProperty(XamlLanguage.Base, this._settings.BaseUri.ToString());
            }
        }

        private void EnqueueOneXmlDirectiveProperty(XamlMember xmlDirectiveProperty, string textValue)
        {
            XamlNode item = new XamlNode(XamlNodeType.StartMember, xmlDirectiveProperty);
            this._buffer.Enqueue(item);
            XamlNode node2 = new XamlNode(XamlNodeType.Value, textValue);
            this._buffer.Enqueue(node2);
            this._buffer.Enqueue(new XamlNode(XamlNodeType.EndMember));
        }

        private void InitializeObjectFrameStack()
        {
            if (this._seenStack.Count == 0)
            {
                this._seenStack.Add(new SeenCtorDirectiveFlags());
            }
            this._seenStack[0].SeenInstancingProperty = false;
            this._seenStack[0].SeenOutOfOrderCtorDirective = false;
        }

        private bool IsCtorDirective(XamlMember member)
        {
            if (!member.IsDirective)
            {
                return false;
            }
            if ((((member != XamlLanguage.Initialization) && (member != XamlLanguage.PositionalParameters)) && ((member != XamlLanguage.FactoryMethod) && (member != XamlLanguage.Arguments))) && ((member != XamlLanguage.TypeArguments) && (member != XamlLanguage.Base)))
            {
                return false;
            }
            return true;
        }

        private bool IsInstancingMember(XamlMember member)
        {
            if (this.IsCtorDirective(member))
            {
                return false;
            }
            if (member.IsDirective && (member == XamlLanguage.Key))
            {
                return false;
            }
            return true;
        }

        private void MoveList_Process()
        {
            int num;
            int num2;
            while (this.MoveList_RemoveStartMemberIndexWithGreatestDepth(out num2, out num))
            {
                int num3;
                int num4;
                if (this.BackupTo(num2, XamlNodeType.StartObject, num, out num3) && this.AdvanceTo(num3, XamlNodeType.StartMember, num, out num4))
                {
                    this.SortMembers(num4);
                }
            }
        }

        private bool MoveList_RemoveStartMemberIndexWithGreatestDepth(out int deepestCtorIdx, out int deepestDepth)
        {
            deepestDepth = -1;
            deepestCtorIdx = -1;
            int index = -1;
            if (this._moveList.Count == 0)
            {
                return false;
            }
            for (int i = 0; i < this._moveList.Count; i++)
            {
                int num3 = this._moveList[i];
                if (this._sortingInfoArray[num3].Depth > deepestDepth)
                {
                    deepestDepth = this._sortingInfoArray[num3].Depth;
                    deepestCtorIdx = num3;
                    index = i;
                }
            }
            this._moveList.RemoveAt(index);
            return true;
        }

        public bool MoveNext()
        {
            do
            {
                if (this._buffer.Count > 0)
                {
                    this._current = this._buffer.Dequeue();
                }
                else
                {
                    if (!this._source.MoveNext())
                    {
                        return false;
                    }
                    this._current = this._source.Current;
                    if (this._current.NodeType == XamlNodeType.StartObject)
                    {
                        this._buffer.Enqueue(this._current);
                        this.ReadAheadAndSortCtorProperties();
                        this._current = this._buffer.Dequeue();
                    }
                }
            }
            while (this._current.IsEndOfAttributes);
            return true;
        }

        private void ReadAheadAndSortCtorProperties()
        {
            this.InitializeObjectFrameStack();
            this._moveList = null;
            this.ReadAheadToEndObjectOrFirstPropertyElement();
            if (this._moveList != null)
            {
                this.SortContentsOfReadAheadBuffer();
            }
        }

        private void ReadAheadToEndObjectOrFirstPropertyElement()
        {
            this.ReadAheadToEndOfAttributes();
            this.ReadAheadToFirstInstancingProperty();
        }

        private void ReadAheadToEndOfAttributes()
        {
            int num = 0;
            bool flag = false;
            do
            {
                if (!this._source.MoveNext())
                {
                    throw new InvalidOperationException("premature end of stream before EoA");
                }
                XamlNode current = this._source.Current;
                switch (current.NodeType)
                {
                    case XamlNodeType.None:
                        if (current.IsEndOfAttributes && (num == 0))
                        {
                            flag = true;
                        }
                        break;

                    case XamlNodeType.StartObject:
                        this.StartObjectFrame();
                        break;

                    case XamlNodeType.EndObject:
                        this.EndObjectFrame();
                        if (num == 0)
                        {
                            flag = true;
                        }
                        break;

                    case XamlNodeType.StartMember:
                        num++;
                        if (!this.HaveSeenOutOfOrderCtorDirective)
                        {
                            this.CheckForOutOfOrderCtorDirectives(current);
                        }
                        break;

                    case XamlNodeType.EndMember:
                        num--;
                        break;
                }
                this._buffer.Enqueue(current);
            }
            while (!flag);
        }

        private void ReadAheadToFirstInstancingProperty()
        {
            int num = 0;
            bool flag = false;
            do
            {
                if (!this._source.MoveNext())
                {
                    throw new InvalidOperationException("premature end of stream after EoA");
                }
                XamlNode current = this._source.Current;
                switch (current.NodeType)
                {
                    case XamlNodeType.EndObject:
                        if (num == 0)
                        {
                            flag = true;
                        }
                        break;

                    case XamlNodeType.StartMember:
                        num++;
                        if (this.CheckForOutOfOrderCtorDirectives(current) && (num == 1))
                        {
                            flag = true;
                        }
                        break;

                    case XamlNodeType.EndMember:
                        num--;
                        break;
                }
                this._buffer.Enqueue(current);
            }
            while (!flag);
        }

        private void ReloadSortedBuffer()
        {
            for (int i = 0; i < this._sortingInfoArray.Length; i++)
            {
                int originalOrderIndex = this._sortingInfoArray[i].OriginalOrderIndex;
                this._buffer.Enqueue(this._originalNodesInOrder[originalOrderIndex]);
            }
            this._sortingInfoArray = null;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        private void SortContentsOfReadAheadBuffer()
        {
            this.BuildSortingBuffer();
            this.MoveList_Process();
            this.ReloadSortedBuffer();
        }

        private void SortMembers(int start)
        {
            int num4;
            int num5;
            int depth = this._sortingInfoArray[start].Depth;
            for (int i = start; (i < this._sortingInfoArray.Length) && (this._sortingInfoArray[i].XamlNodeType == XamlNodeType.StartMember); i = num4 + num5)
            {
                int num3;
                if (!this.AdvanceToNextInstancingMember(i, depth, out num3))
                {
                    return;
                }
                if (!this.AdvanceToNextCtorDirective(num3, depth, out num4))
                {
                    return;
                }
                num5 = this.AdvanceOverNoninstancingDirectives(num4, depth);
                this.SwapRanges(num3, num4, num4 + num5);
            }
        }

        private void StartNewNodeStreamWithSettingsPreamble()
        {
            bool flag = false;
            while (!flag)
            {
                this._source.MoveNext();
                XamlNode current = this._source.Current;
                XamlNodeType nodeType = current.NodeType;
                switch (nodeType)
                {
                    case XamlNodeType.None:
                        break;

                    case XamlNodeType.StartObject:
                    {
                        flag = true;
                        this.EnqueueInitialExtraXmlNses();
                        this._buffer.Enqueue(current);
                        this.EnqueueInitialXmlState();
                        continue;
                    }
                    default:
                    {
                        if (nodeType == XamlNodeType.NamespaceDeclaration)
                        {
                            this._buffer.Enqueue(current);
                        }
                        continue;
                    }
                }
                if (current.IsLineInfo)
                {
                    this._buffer.Enqueue(current);
                }
            }
        }

        private void StartObjectFrame()
        {
            this._startObjectDepth++;
            if (this._seenStack.Count <= this._startObjectDepth)
            {
                this._seenStack.Add(new SeenCtorDirectiveFlags());
            }
            this._seenStack[this._startObjectDepth].SeenInstancingProperty = false;
            this._seenStack[this._startObjectDepth].SeenOutOfOrderCtorDirective = false;
        }

        private void SwapRanges(int beginning, int middle, int end)
        {
            int length = middle - beginning;
            int num2 = end - middle;
            ReorderInfo[] destinationArray = new ReorderInfo[length];
            Array.Copy(this._sortingInfoArray, beginning, destinationArray, 0, length);
            Array.Copy(this._sortingInfoArray, middle, this._sortingInfoArray, beginning, num2);
            Array.Copy(destinationArray, 0, this._sortingInfoArray, beginning + num2, length);
        }

        public XamlNode Current
        {
            get
            {
                return this._current;
            }
        }

        private bool HaveSeenInstancingProperty
        {
            get
            {
                return this._seenStack[this._startObjectDepth].SeenInstancingProperty;
            }
            set
            {
                this._seenStack[this._startObjectDepth].SeenInstancingProperty = value;
            }
        }

        private bool HaveSeenOutOfOrderCtorDirective
        {
            get
            {
                return this._seenStack[this._startObjectDepth].SeenOutOfOrderCtorDirective;
            }
            set
            {
                this._seenStack[this._startObjectDepth].SeenOutOfOrderCtorDirective = value;
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return this._current;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ReorderInfo
        {
            public int Depth { get; set; }
            public int OriginalOrderIndex { get; set; }
            public System.Xaml.XamlNodeType XamlNodeType { get; set; }
        }

        private class SeenCtorDirectiveFlags
        {
            public bool SeenInstancingProperty;
            public bool SeenOutOfOrderCtorDirective;
        }
    }
}

