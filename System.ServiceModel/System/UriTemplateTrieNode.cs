namespace System
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    internal class UriTemplateTrieNode
    {
        private int depth;
        private UriTemplatePathPartiallyEquivalentSet endOfPath;
        private AscendingSortedCompoundSegmentsCollection<UriTemplatePathPartiallyEquivalentSet> finalCompoundSegment;
        private Dictionary<UriTemplateLiteralPathSegment, UriTemplatePathPartiallyEquivalentSet> finalLiteralSegment;
        private UriTemplatePathPartiallyEquivalentSet finalVariableSegment;
        private AscendingSortedCompoundSegmentsCollection<UriTemplateTrieLocation> nextCompoundSegment;
        private Dictionary<UriTemplateLiteralPathSegment, UriTemplateTrieLocation> nextLiteralSegment;
        private UriTemplateTrieLocation nextVariableSegment;
        private UriTemplateTrieLocation onFailure;
        private UriTemplatePathPartiallyEquivalentSet star;

        private UriTemplateTrieNode(int depth)
        {
            this.depth = depth;
            this.nextLiteralSegment = null;
            this.nextCompoundSegment = null;
            this.finalLiteralSegment = null;
            this.finalCompoundSegment = null;
            this.finalVariableSegment = new UriTemplatePathPartiallyEquivalentSet(depth + 1);
            this.star = new UriTemplatePathPartiallyEquivalentSet(depth);
            this.endOfPath = new UriTemplatePathPartiallyEquivalentSet(depth);
        }

        private static void Add(UriTemplateTrieNode root, KeyValuePair<UriTemplate, object> kvp)
        {
            UriTemplateTrieNode node = root;
            UriTemplate key = kvp.Key;
            bool flag = ((key.segments.Count == 0) || key.HasWildcard) || key.segments[key.segments.Count - 1].EndsWithSlash;
            for (int i = 0; i < key.segments.Count; i++)
            {
                if (i >= key.firstOptionalSegment)
                {
                    node.endOfPath.Items.Add(kvp);
                }
                UriTemplatePathSegment segment = key.segments[i];
                if (!segment.EndsWithSlash)
                {
                    switch (segment.Nature)
                    {
                        case UriTemplatePartType.Literal:
                            node.AddFinalLiteralSegment(segment as UriTemplateLiteralPathSegment, kvp);
                            break;

                        case UriTemplatePartType.Compound:
                            node.AddFinalCompoundSegment(segment as UriTemplateCompoundPathSegment, kvp);
                            break;

                        case UriTemplatePartType.Variable:
                            node.finalVariableSegment.Items.Add(kvp);
                            break;
                    }
                }
                else
                {
                    switch (segment.Nature)
                    {
                        case UriTemplatePartType.Literal:
                            node = node.AddNextLiteralSegment(segment as UriTemplateLiteralPathSegment);
                            break;

                        case UriTemplatePartType.Compound:
                            node = node.AddNextCompoundSegment(segment as UriTemplateCompoundPathSegment);
                            break;

                        case UriTemplatePartType.Variable:
                            node = node.AddNextVariableSegment();
                            break;
                    }
                }
            }
            if (flag)
            {
                if (key.HasWildcard)
                {
                    node.star.Items.Add(kvp);
                }
                else
                {
                    node.endOfPath.Items.Add(kvp);
                }
            }
        }

        private void AddFinalCompoundSegment(UriTemplateCompoundPathSegment cps, KeyValuePair<UriTemplate, object> kvp)
        {
            if (this.finalCompoundSegment == null)
            {
                this.finalCompoundSegment = new AscendingSortedCompoundSegmentsCollection<UriTemplatePathPartiallyEquivalentSet>();
            }
            UriTemplatePathPartiallyEquivalentSet set = this.finalCompoundSegment.Find(cps);
            if (set == null)
            {
                set = new UriTemplatePathPartiallyEquivalentSet(this.depth + 1);
                this.finalCompoundSegment.Add(cps, set);
            }
            set.Items.Add(kvp);
        }

        private void AddFinalLiteralSegment(UriTemplateLiteralPathSegment lps, KeyValuePair<UriTemplate, object> kvp)
        {
            if ((this.finalLiteralSegment != null) && this.finalLiteralSegment.ContainsKey(lps))
            {
                this.finalLiteralSegment[lps].Items.Add(kvp);
            }
            else
            {
                if (this.finalLiteralSegment == null)
                {
                    this.finalLiteralSegment = new Dictionary<UriTemplateLiteralPathSegment, UriTemplatePathPartiallyEquivalentSet>();
                }
                UriTemplatePathPartiallyEquivalentSet set = new UriTemplatePathPartiallyEquivalentSet(this.depth + 1);
                set.Items.Add(kvp);
                this.finalLiteralSegment.Add(lps, set);
            }
        }

        private UriTemplateTrieNode AddNextCompoundSegment(UriTemplateCompoundPathSegment cps)
        {
            if (this.nextCompoundSegment == null)
            {
                this.nextCompoundSegment = new AscendingSortedCompoundSegmentsCollection<UriTemplateTrieLocation>();
            }
            UriTemplateTrieLocation location = this.nextCompoundSegment.Find(cps);
            if (location == null)
            {
                UriTemplateTrieNode n = new UriTemplateTrieNode(this.depth + 1) {
                    onFailure = new UriTemplateTrieLocation(this, UriTemplateTrieIntraNodeLocation.AfterCompound)
                };
                location = new UriTemplateTrieLocation(n, UriTemplateTrieIntraNodeLocation.BeforeLiteral);
                this.nextCompoundSegment.Add(cps, location);
            }
            return location.node;
        }

        private UriTemplateTrieNode AddNextLiteralSegment(UriTemplateLiteralPathSegment lps)
        {
            if ((this.nextLiteralSegment != null) && this.nextLiteralSegment.ContainsKey(lps))
            {
                return this.nextLiteralSegment[lps].node;
            }
            if (this.nextLiteralSegment == null)
            {
                this.nextLiteralSegment = new Dictionary<UriTemplateLiteralPathSegment, UriTemplateTrieLocation>();
            }
            UriTemplateTrieNode n = new UriTemplateTrieNode(this.depth + 1) {
                onFailure = new UriTemplateTrieLocation(this, UriTemplateTrieIntraNodeLocation.AfterLiteral)
            };
            this.nextLiteralSegment.Add(lps, new UriTemplateTrieLocation(n, UriTemplateTrieIntraNodeLocation.BeforeLiteral));
            return n;
        }

        private UriTemplateTrieNode AddNextVariableSegment()
        {
            if (this.nextVariableSegment != null)
            {
                return this.nextVariableSegment.node;
            }
            UriTemplateTrieNode n = new UriTemplateTrieNode(this.depth + 1) {
                onFailure = new UriTemplateTrieLocation(this, UriTemplateTrieIntraNodeLocation.AfterVariable)
            };
            this.nextVariableSegment = new UriTemplateTrieLocation(n, UriTemplateTrieIntraNodeLocation.BeforeLiteral);
            return n;
        }

        private static bool CheckMultipleMatches(IList<IList<UriTemplateTrieLocation>> locationsSet, UriTemplateLiteralPathSegment[] wireData, ICollection<UriTemplateTableMatchCandidate> candidates)
        {
            bool flag = false;
            for (int i = 0; (i < locationsSet.Count) && !flag; i++)
            {
                for (int j = 0; j < locationsSet[i].Count; j++)
                {
                    if (GetMatch(locationsSet[i][j], wireData, candidates))
                    {
                        flag = true;
                    }
                }
            }
            return flag;
        }

        private static UriTemplate FindAnyUriTemplate(UriTemplateTrieNode node)
        {
            while (node != null)
            {
                if (node.endOfPath.Items.Count > 0)
                {
                    KeyValuePair<UriTemplate, object> pair = node.endOfPath.Items[0];
                    return pair.Key;
                }
                if (node.finalVariableSegment.Items.Count > 0)
                {
                    KeyValuePair<UriTemplate, object> pair2 = node.finalVariableSegment.Items[0];
                    return pair2.Key;
                }
                if (node.star.Items.Count > 0)
                {
                    KeyValuePair<UriTemplate, object> pair3 = node.star.Items[0];
                    return pair3.Key;
                }
                if (node.finalLiteralSegment != null)
                {
                    KeyValuePair<UriTemplate, object> pair4 = GetAnyDictionaryValue<UriTemplatePathPartiallyEquivalentSet>(node.finalLiteralSegment).Items[0];
                    return pair4.Key;
                }
                if (node.finalCompoundSegment != null)
                {
                    KeyValuePair<UriTemplate, object> pair5 = node.finalCompoundSegment.GetAnyValue().Items[0];
                    return pair5.Key;
                }
                if (node.nextLiteralSegment != null)
                {
                    node = GetAnyDictionaryValue<UriTemplateTrieLocation>(node.nextLiteralSegment).node;
                }
                else
                {
                    if (node.nextCompoundSegment != null)
                    {
                        node = node.nextCompoundSegment.GetAnyValue().node;
                        continue;
                    }
                    if (node.nextVariableSegment != null)
                    {
                        node = node.nextVariableSegment.node;
                        continue;
                    }
                    node = null;
                }
            }
            return null;
        }

        private static T GetAnyDictionaryValue<T>(IDictionary<UriTemplateLiteralPathSegment, T> dictionary)
        {
            using (IEnumerator<T> enumerator = dictionary.Values.GetEnumerator())
            {
                enumerator.MoveNext();
                return enumerator.Current;
            }
        }

        private static UriTemplateTrieLocation GetFailureLocationFromLocationsSet(IList<IList<UriTemplateTrieLocation>> locationsSet)
        {
            return locationsSet[0][0].node.onFailure;
        }

        private static bool GetMatch(UriTemplateTrieLocation location, UriTemplateLiteralPathSegment[] wireData, ICollection<UriTemplateTableMatchCandidate> candidates)
        {
            int depth = location.node.depth;
            do
            {
                SingleLocationOrLocationsSet set;
                UriTemplatePathPartiallyEquivalentSet set2;
                if (TryMatch(wireData, location, out set2, out set))
                {
                    if (set2 != null)
                    {
                        for (int i = 0; i < set2.Items.Count; i++)
                        {
                            KeyValuePair<UriTemplate, object> pair = set2.Items[i];
                            KeyValuePair<UriTemplate, object> pair2 = set2.Items[i];
                            candidates.Add(new UriTemplateTableMatchCandidate(pair.Key, set2.SegmentsCount, pair2.Value));
                        }
                    }
                    return true;
                }
                if (set.IsSingle)
                {
                    location = set.SingleLocation;
                }
                else
                {
                    if (CheckMultipleMatches(set.LocationsSet, wireData, candidates))
                    {
                        return true;
                    }
                    location = GetFailureLocationFromLocationsSet(set.LocationsSet);
                }
            }
            while ((location != null) && (location.node.depth >= depth));
            return false;
        }

        public static UriTemplateTrieNode Make(IEnumerable<KeyValuePair<UriTemplate, object>> keyValuePairs, bool allowDuplicateEquivalentUriTemplates)
        {
            UriTemplateTrieNode root = new UriTemplateTrieNode(0);
            foreach (KeyValuePair<UriTemplate, object> pair in keyValuePairs)
            {
                Add(root, pair);
            }
            Validate(root, allowDuplicateEquivalentUriTemplates);
            return root;
        }

        public bool Match(UriTemplateLiteralPathSegment[] wireData, ICollection<UriTemplateTableMatchCandidate> candidates)
        {
            UriTemplateTrieLocation location = new UriTemplateTrieLocation(this, UriTemplateTrieIntraNodeLocation.BeforeLiteral);
            return GetMatch(location, wireData, candidates);
        }

        private static bool TryMatch(UriTemplateLiteralPathSegment[] wireUriSegments, UriTemplateTrieLocation currentLocation, out UriTemplatePathPartiallyEquivalentSet success, out SingleLocationOrLocationsSet nextStep)
        {
            IList<IList<UriTemplatePathPartiallyEquivalentSet>> list2;
            success = null;
            nextStep = new SingleLocationOrLocationsSet();
            if (wireUriSegments.Length <= currentLocation.node.depth)
            {
                if (currentLocation.node.endOfPath.Items.Count != 0)
                {
                    success = currentLocation.node.endOfPath;
                    return true;
                }
                if (currentLocation.node.star.Items.Count != 0)
                {
                    success = currentLocation.node.star;
                    return true;
                }
                nextStep = new SingleLocationOrLocationsSet(currentLocation.node.onFailure);
                return false;
            }
            UriTemplateLiteralPathSegment key = wireUriSegments[currentLocation.node.depth];
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            bool flag4 = false;
            switch (currentLocation.locationWithin)
            {
                case UriTemplateTrieIntraNodeLocation.BeforeLiteral:
                    flag = true;
                    flag2 = true;
                    flag3 = true;
                    flag4 = true;
                    break;

                case UriTemplateTrieIntraNodeLocation.AfterLiteral:
                    flag = false;
                    flag2 = true;
                    flag3 = true;
                    flag4 = true;
                    break;

                case UriTemplateTrieIntraNodeLocation.AfterCompound:
                    flag = false;
                    flag2 = false;
                    flag3 = true;
                    flag4 = true;
                    break;

                case UriTemplateTrieIntraNodeLocation.AfterVariable:
                    flag = false;
                    flag2 = false;
                    flag3 = false;
                    flag4 = true;
                    break;
            }
            if (key.EndsWithSlash)
            {
                IList<IList<UriTemplateTrieLocation>> list;
                if ((flag && (currentLocation.node.nextLiteralSegment != null)) && currentLocation.node.nextLiteralSegment.ContainsKey(key))
                {
                    nextStep = new SingleLocationOrLocationsSet(currentLocation.node.nextLiteralSegment[key]);
                    return false;
                }
                if ((flag2 && (currentLocation.node.nextCompoundSegment != null)) && AscendingSortedCompoundSegmentsCollection<UriTemplateTrieLocation>.Lookup(currentLocation.node.nextCompoundSegment, key, out list))
                {
                    nextStep = new SingleLocationOrLocationsSet(list);
                    return false;
                }
                if ((flag3 && (currentLocation.node.nextVariableSegment != null)) && !key.IsNullOrEmpty())
                {
                    nextStep = new SingleLocationOrLocationsSet(currentLocation.node.nextVariableSegment);
                    return false;
                }
                if (flag4 && (currentLocation.node.star.Items.Count != 0))
                {
                    success = currentLocation.node.star;
                    return true;
                }
                nextStep = new SingleLocationOrLocationsSet(currentLocation.node.onFailure);
                return false;
            }
            if ((flag && (currentLocation.node.finalLiteralSegment != null)) && currentLocation.node.finalLiteralSegment.ContainsKey(key))
            {
                success = currentLocation.node.finalLiteralSegment[key];
                return true;
            }
            if ((flag2 && (currentLocation.node.finalCompoundSegment != null)) && AscendingSortedCompoundSegmentsCollection<UriTemplatePathPartiallyEquivalentSet>.Lookup(currentLocation.node.finalCompoundSegment, key, out list2))
            {
                if (list2[0].Count == 1)
                {
                    success = list2[0][0];
                }
                else
                {
                    success = new UriTemplatePathPartiallyEquivalentSet(currentLocation.node.depth + 1);
                    for (int i = 0; i < list2[0].Count; i++)
                    {
                        success.Items.AddRange(list2[0][i].Items);
                    }
                }
                return true;
            }
            if (flag3 && (currentLocation.node.finalVariableSegment.Items.Count != 0))
            {
                success = currentLocation.node.finalVariableSegment;
                return true;
            }
            if (flag4 && (currentLocation.node.star.Items.Count != 0))
            {
                success = currentLocation.node.star;
                return true;
            }
            nextStep = new SingleLocationOrLocationsSet(currentLocation.node.onFailure);
            return false;
        }

        private static void Validate(UriTemplatePathPartiallyEquivalentSet pes, bool allowDuplicateEquivalentUriTemplates)
        {
            if (pes.Items.Count >= 2)
            {
                for (int i = 0; i < (pes.Items.Count - 1); i++)
                {
                }
                UriTemplate[] array = new UriTemplate[pes.Items.Count];
                int b = 0;
                foreach (KeyValuePair<UriTemplate, object> pair in pes.Items)
                {
                    if (pes.SegmentsCount >= pair.Key.segments.Count)
                    {
                        array[b++] = pair.Key;
                    }
                }
                if (b > 0)
                {
                    UriTemplateHelpers.DisambiguateSamePath(array, 0, b, allowDuplicateEquivalentUriTemplates);
                }
            }
        }

        private static void Validate(UriTemplateTrieNode root, bool allowDuplicateEquivalentUriTemplates)
        {
            Queue<UriTemplateTrieNode> queue = new Queue<UriTemplateTrieNode>();
            UriTemplateTrieNode node = root;
            while (true)
            {
                Validate(node.endOfPath, allowDuplicateEquivalentUriTemplates);
                Validate(node.finalVariableSegment, allowDuplicateEquivalentUriTemplates);
                Validate(node.star, allowDuplicateEquivalentUriTemplates);
                if (node.finalLiteralSegment != null)
                {
                    foreach (KeyValuePair<UriTemplateLiteralPathSegment, UriTemplatePathPartiallyEquivalentSet> pair in node.finalLiteralSegment)
                    {
                        Validate(pair.Value, allowDuplicateEquivalentUriTemplates);
                    }
                }
                if (node.finalCompoundSegment != null)
                {
                    IList<IList<UriTemplatePathPartiallyEquivalentSet>> values = node.finalCompoundSegment.Values;
                    for (int i = 0; i < values.Count; i++)
                    {
                        if (!allowDuplicateEquivalentUriTemplates && (values[i].Count > 1))
                        {
                            object[] args = new object[2];
                            KeyValuePair<UriTemplate, object> pair3 = values[i][0].Items[0];
                            args[0] = pair3.Key.ToString();
                            KeyValuePair<UriTemplate, object> pair4 = values[i][1].Items[0];
                            args[1] = pair4.Key.ToString();
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UTTDuplicate", args)));
                        }
                        for (int j = 0; j < values[i].Count; j++)
                        {
                            Validate(values[i][j], allowDuplicateEquivalentUriTemplates);
                        }
                    }
                }
                if (node.nextLiteralSegment != null)
                {
                    foreach (KeyValuePair<UriTemplateLiteralPathSegment, UriTemplateTrieLocation> pair2 in node.nextLiteralSegment)
                    {
                        queue.Enqueue(pair2.Value.node);
                    }
                }
                if (node.nextCompoundSegment != null)
                {
                    IList<IList<UriTemplateTrieLocation>> list2 = node.nextCompoundSegment.Values;
                    for (int k = 0; k < list2.Count; k++)
                    {
                        if (!allowDuplicateEquivalentUriTemplates && (list2[k].Count > 1))
                        {
                            UriTemplate template = FindAnyUriTemplate(list2[k][0].node);
                            UriTemplate template2 = FindAnyUriTemplate(list2[k][1].node);
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UTTDuplicate", new object[] { template.ToString(), template2.ToString() })));
                        }
                        for (int m = 0; m < list2[k].Count; m++)
                        {
                            UriTemplateTrieLocation location = list2[k][m];
                            queue.Enqueue(location.node);
                        }
                    }
                }
                if (node.nextVariableSegment != null)
                {
                    queue.Enqueue(node.nextVariableSegment.node);
                }
                if (queue.Count == 0)
                {
                    return;
                }
                node = queue.Dequeue();
            }
        }

        private class AscendingSortedCompoundSegmentsCollection<T> where T: class
        {
            private SortedList<UriTemplateCompoundPathSegment, Collection<CollectionItem<T>>> items;

            public AscendingSortedCompoundSegmentsCollection()
            {
                this.items = new SortedList<UriTemplateCompoundPathSegment, Collection<CollectionItem<T>>>();
            }

            public void Add(UriTemplateCompoundPathSegment segment, T value)
            {
                int num = this.items.IndexOfKey(segment);
                if (num == -1)
                {
                    Collection<CollectionItem<T>> collection = new Collection<CollectionItem<T>> {
                        new CollectionItem<T>(segment, value)
                    };
                    this.items.Add(segment, collection);
                }
                else
                {
                    this.items.Values[num].Add(new CollectionItem<T>(segment, value));
                }
            }

            public T Find(UriTemplateCompoundPathSegment segment)
            {
                int num = this.items.IndexOfKey(segment);
                if (num != -1)
                {
                    Collection<CollectionItem<T>> collection = this.items.Values[num];
                    for (int i = 0; i < collection.Count; i++)
                    {
                        CollectionItem<T> item = collection[i];
                        if (item.Segment.IsEquivalentTo(segment, false))
                        {
                            CollectionItem<T> item2 = collection[i];
                            return item2.Value;
                        }
                    }
                }
                return default(T);
            }

            public IList<IList<T>> Find(UriTemplateLiteralPathSegment wireData)
            {
                IList<IList<T>> list = new List<IList<T>>();
                for (int i = 0; i < this.items.Values.Count; i++)
                {
                    List<T> list2 = null;
                    for (int j = 0; j < this.items.Values[i].Count; j++)
                    {
                        CollectionItem<T> item = this.items.Values[i][j];
                        if (item.Segment.IsMatch(wireData))
                        {
                            if (list2 == null)
                            {
                                list2 = new List<T>();
                            }
                            CollectionItem<T> item2 = this.items.Values[i][j];
                            list2.Add(item2.Value);
                        }
                    }
                    if (list2 != null)
                    {
                        list.Add(list2);
                    }
                }
                return list;
            }

            public T GetAnyValue()
            {
                if (this.items.Values.Count > 0)
                {
                    CollectionItem<T> item = this.items.Values[0][0];
                    return item.Value;
                }
                return default(T);
            }

            public static bool Lookup(UriTemplateTrieNode.AscendingSortedCompoundSegmentsCollection<T> collection, UriTemplateLiteralPathSegment wireData, out IList<IList<T>> results)
            {
                results = collection.Find(wireData);
                return ((results != null) && (results.Count > 0));
            }

            public IList<IList<T>> Values
            {
                get
                {
                    IList<IList<T>> list = new List<IList<T>>(this.items.Count);
                    for (int i = 0; i < this.items.Values.Count; i++)
                    {
                        list.Add(new List<T>(this.items.Values[i].Count));
                        for (int j = 0; j < this.items.Values[i].Count; j++)
                        {
                            CollectionItem<T> item = this.items.Values[i][j];
                            list[i].Add(item.Value);
                        }
                    }
                    return list;
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct CollectionItem
            {
                private UriTemplateCompoundPathSegment segment;
                private T value;
                public CollectionItem(UriTemplateCompoundPathSegment segment, T value)
                {
                    this.segment = segment;
                    this.value = value;
                }

                public UriTemplateCompoundPathSegment Segment
                {
                    get
                    {
                        return this.segment;
                    }
                }
                public T Value
                {
                    get
                    {
                        return this.value;
                    }
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SingleLocationOrLocationsSet
        {
            private readonly bool isSingle;
            private readonly IList<IList<UriTemplateTrieLocation>> locationsSet;
            private readonly UriTemplateTrieLocation singleLocation;
            public SingleLocationOrLocationsSet(UriTemplateTrieLocation singleLocation)
            {
                this.isSingle = true;
                this.singleLocation = singleLocation;
                this.locationsSet = null;
            }

            public SingleLocationOrLocationsSet(IList<IList<UriTemplateTrieLocation>> locationsSet)
            {
                this.isSingle = false;
                this.singleLocation = null;
                this.locationsSet = locationsSet;
            }

            public bool IsSingle
            {
                get
                {
                    return this.isSingle;
                }
            }
            public IList<IList<UriTemplateTrieLocation>> LocationsSet
            {
                get
                {
                    return this.locationsSet;
                }
            }
            public UriTemplateTrieLocation SingleLocation
            {
                get
                {
                    return this.singleLocation;
                }
            }
        }
    }
}

