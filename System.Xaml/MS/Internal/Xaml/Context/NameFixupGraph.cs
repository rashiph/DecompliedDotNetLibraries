namespace MS.Internal.Xaml.Context
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Xaml;
    using System.Xaml.Context;
    using System.Xaml.MS.Impl;
    using System.Xaml.Schema;

    internal class NameFixupGraph
    {
        private NameFixupToken _deferredRootProvideValue;
        private Dictionary<object, NameFixupToken> _dependenciesByChildObject;
        private Dictionary<string, FrugalObjectList<NameFixupToken>> _dependenciesByName;
        private Dictionary<object, FrugalObjectList<NameFixupToken>> _dependenciesByParentObject;
        private Queue<NameFixupToken> _resolvedTokensPendingProcessing;
        private System.Xaml.Context.HashSet<object> _uninitializedObjectsAtParseEnd;

        public NameFixupGraph()
        {
            ReferenceEqualityComparer<object> singleton = ReferenceEqualityComparer<object>.Singleton;
            this._dependenciesByChildObject = new Dictionary<object, NameFixupToken>(singleton);
            this._dependenciesByName = new Dictionary<string, FrugalObjectList<NameFixupToken>>(StringComparer.Ordinal);
            this._dependenciesByParentObject = new Dictionary<object, FrugalObjectList<NameFixupToken>>(singleton);
            this._resolvedTokensPendingProcessing = new Queue<NameFixupToken>();
            this._uninitializedObjectsAtParseEnd = new System.Xaml.Context.HashSet<object>(singleton);
        }

        public void AddDependency(NameFixupToken fixupToken)
        {
            if (fixupToken.Target.Property == null)
            {
                this._deferredRootProvideValue = fixupToken;
            }
            else
            {
                object instance = fixupToken.Target.Instance;
                AddToMultiDict<object>(this._dependenciesByParentObject, instance, fixupToken);
                if (fixupToken.ReferencedObject != null)
                {
                    this._dependenciesByChildObject.Add(fixupToken.ReferencedObject, fixupToken);
                }
                else
                {
                    foreach (string str in fixupToken.NeededNames)
                    {
                        AddToMultiDict<string>(this._dependenciesByName, str, fixupToken);
                    }
                }
            }
        }

        public void AddEndOfParseDependency(object childThatHasUnresolvedChildren, FixupTarget parentObject)
        {
            NameFixupToken token = new NameFixupToken {
                Target = parentObject,
                FixupType = FixupType.UnresolvedChildren,
                ReferencedObject = childThatHasUnresolvedChildren
            };
            AddToMultiDict<object>(this._dependenciesByParentObject, parentObject.Instance, token);
        }

        private static void AddToMultiDict<TKey>(Dictionary<TKey, FrugalObjectList<NameFixupToken>> dict, TKey key, NameFixupToken value)
        {
            FrugalObjectList<NameFixupToken> list;
            if (!dict.TryGetValue(key, out list))
            {
                list = new FrugalObjectList<NameFixupToken>(1);
                dict.Add(key, list);
            }
            list.Add(value);
        }

        private bool FindDependencies(NameFixupToken inEdge, List<NameFixupToken> alreadyTraversed)
        {
            if (!alreadyTraversed.Contains(inEdge))
            {
                FrugalObjectList<NameFixupToken> list;
                alreadyTraversed.Add(inEdge);
                if ((inEdge.ReferencedObject == null) || !this._dependenciesByParentObject.TryGetValue(inEdge.ReferencedObject, out list))
                {
                    return true;
                }
                for (int i = 0; i < list.Count; i++)
                {
                    NameFixupToken token = list[i];
                    if (token.FixupType == FixupType.MarkupExtensionFirstRun)
                    {
                        return false;
                    }
                    if (!this.FindDependencies(token, alreadyTraversed))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void GetDependentNames(object instance, List<string> result)
        {
            FrugalObjectList<NameFixupToken> list;
            if (this._dependenciesByParentObject.TryGetValue(instance, out list))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    NameFixupToken token = list[i];
                    if ((token.FixupType == FixupType.MarkupExtensionFirstRun) || (token.FixupType == FixupType.UnresolvedChildren))
                    {
                        this.GetDependentNames(token.ReferencedObject, result);
                    }
                    else if (token.NeededNames != null)
                    {
                        foreach (string str in token.NeededNames)
                        {
                            if (!result.Contains(str))
                            {
                                result.Add(str);
                            }
                        }
                    }
                }
            }
        }

        public NameFixupToken GetNextResolvedTokenPendingProcessing()
        {
            return this._resolvedTokensPendingProcessing.Dequeue();
        }

        public IEnumerable<NameFixupToken> GetRemainingObjectDependencies()
        {
            List<NameFixupToken> markupExtensionTokens = new List<NameFixupToken>();
            foreach (NameFixupToken token in this._dependenciesByChildObject.Values)
            {
                if (token.FixupType == FixupType.MarkupExtensionFirstRun)
                {
                    markupExtensionTokens.Add(token);
                }
            }
            while (markupExtensionTokens.Count > 0)
            {
                bool iteratorVariable1 = false;
                int index = 0;
                while (index < markupExtensionTokens.Count)
                {
                    NameFixupToken inEdge = markupExtensionTokens[index];
                    List<NameFixupToken> alreadyTraversed = new List<NameFixupToken>();
                    if (!this.FindDependencies(inEdge, alreadyTraversed))
                    {
                        index++;
                    }
                    else
                    {
                        for (int i = alreadyTraversed.Count - 1; i >= 0; i--)
                        {
                            NameFixupToken iteratorVariable6 = alreadyTraversed[i];
                            this.RemoveTokenByParent(iteratorVariable6);
                            yield return iteratorVariable6;
                        }
                        iteratorVariable1 = true;
                        markupExtensionTokens.RemoveAt(index);
                    }
                }
                if (!iteratorVariable1)
                {
                    ThrowProvideValueCycle(markupExtensionTokens);
                }
            }
            while (this._dependenciesByParentObject.Count > 0)
            {
                FrugalObjectList<NameFixupToken> iteratorVariable7 = null;
                foreach (FrugalObjectList<NameFixupToken> list in this._dependenciesByParentObject.Values)
                {
                    iteratorVariable7 = list;
                    break;
                }
                for (int j = 0; j < iteratorVariable7.Count; j++)
                {
                    List<NameFixupToken> iteratorVariable9 = new List<NameFixupToken>();
                    this.FindDependencies(iteratorVariable7[j], iteratorVariable9);
                    for (int k = iteratorVariable9.Count - 1; k >= 0; k--)
                    {
                        NameFixupToken iteratorVariable11 = iteratorVariable9[k];
                        this.RemoveTokenByParent(iteratorVariable11);
                        yield return iteratorVariable11;
                    }
                }
            }
            if (this._deferredRootProvideValue != null)
            {
                yield return this._deferredRootProvideValue;
            }
            else
            {
                yield break;
            }
        }

        public IEnumerable<NameFixupToken> GetRemainingReparses()
        {
            List<object> iteratorVariable0 = new List<object>(this._dependenciesByParentObject.Keys);
            foreach (object iteratorVariable1 in iteratorVariable0)
            {
                FrugalObjectList<NameFixupToken> iteratorVariable2 = this._dependenciesByParentObject[iteratorVariable1];
                int index = 0;
                while (index < iteratorVariable2.Count)
                {
                    NameFixupToken iteratorVariable4 = iteratorVariable2[index];
                    if ((iteratorVariable4.FixupType == FixupType.MarkupExtensionFirstRun) || (iteratorVariable4.FixupType == FixupType.UnresolvedChildren))
                    {
                        index++;
                        continue;
                    }
                    iteratorVariable2.RemoveAt(index);
                    if (iteratorVariable2.Count == 0)
                    {
                        this._dependenciesByParentObject.Remove(iteratorVariable1);
                    }
                    foreach (string str in iteratorVariable4.NeededNames)
                    {
                        FrugalObjectList<NameFixupToken> list = this._dependenciesByName[str];
                        if (list.Count == 1)
                        {
                            list.Remove(iteratorVariable4);
                        }
                        else
                        {
                            this._dependenciesByName.Remove(str);
                        }
                    }
                    yield return iteratorVariable4;
                }
            }
        }

        public IEnumerable<NameFixupToken> GetRemainingSimpleFixups()
        {
            foreach (object obj2 in this._dependenciesByParentObject.Keys)
            {
                this._uninitializedObjectsAtParseEnd.Add(obj2);
            }
            List<string> iteratorVariable0 = new List<string>(this._dependenciesByName.Keys);
            foreach (string iteratorVariable1 in iteratorVariable0)
            {
                FrugalObjectList<NameFixupToken> iteratorVariable2 = this._dependenciesByName[iteratorVariable1];
                int index = 0;
                while (index < iteratorVariable2.Count)
                {
                    NameFixupToken token = iteratorVariable2[index];
                    if (!token.CanAssignDirectly)
                    {
                        index++;
                        continue;
                    }
                    iteratorVariable2.RemoveAt(index);
                    if (iteratorVariable2.Count == 0)
                    {
                        this._dependenciesByName.Remove(iteratorVariable1);
                    }
                    this.RemoveTokenByParent(token);
                    yield return token;
                }
            }
        }

        public bool HasUnresolvedChildren(object parent)
        {
            if (parent == null)
            {
                return false;
            }
            return this._dependenciesByParentObject.ContainsKey(parent);
        }

        public bool HasUnresolvedOrPendingChildren(object instance)
        {
            if (this.HasUnresolvedChildren(instance))
            {
                return true;
            }
            foreach (NameFixupToken token in this._resolvedTokensPendingProcessing)
            {
                if (token.Target.Instance == instance)
                {
                    return true;
                }
            }
            return false;
        }

        public void IsOffTheStack(object instance, string name, int lineNumber, int linePosition)
        {
            FrugalObjectList<NameFixupToken> list;
            if (this._dependenciesByParentObject.TryGetValue(instance, out list))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].Target.InstanceIsOnTheStack = false;
                    list[i].Target.InstanceName = name;
                    list[i].Target.EndInstanceLineNumber = lineNumber;
                    list[i].Target.EndInstanceLinePosition = linePosition;
                }
            }
        }

        private void RemoveTokenByParent(NameFixupToken token)
        {
            object instance = token.Target.Instance;
            FrugalObjectList<NameFixupToken> list = this._dependenciesByParentObject[instance];
            if (list.Count == 1)
            {
                this._dependenciesByParentObject.Remove(instance);
            }
            else
            {
                list.Remove(token);
            }
        }

        public void ResolveDependenciesTo(object instance, string name)
        {
            NameFixupToken token = null;
            FrugalObjectList<NameFixupToken> list;
            if ((instance != null) && this._dependenciesByChildObject.TryGetValue(instance, out token))
            {
                this._dependenciesByChildObject.Remove(instance);
                this.RemoveTokenByParent(token);
                this._resolvedTokensPendingProcessing.Enqueue(token);
            }
            if ((name != null) && this._dependenciesByName.TryGetValue(name, out list))
            {
                int index = 0;
                while (index < list.Count)
                {
                    token = list[index];
                    object obj2 = token.ResolveName(name);
                    if (instance != obj2)
                    {
                        index++;
                    }
                    else
                    {
                        if (token.CanAssignDirectly)
                        {
                            token.ReferencedObject = instance;
                        }
                        token.NeededNames.Remove(name);
                        list.RemoveAt(index);
                        if (list.Count == 0)
                        {
                            this._dependenciesByName.Remove(name);
                        }
                        if (token.NeededNames.Count == 0)
                        {
                            this.RemoveTokenByParent(token);
                            this._resolvedTokensPendingProcessing.Enqueue(token);
                        }
                    }
                }
            }
        }

        private static void ThrowProvideValueCycle(IEnumerable<NameFixupToken> markupExtensionTokens)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(System.Xaml.SR.Get("ProvideValueCycle"));
            foreach (NameFixupToken token in markupExtensionTokens)
            {
                builder.AppendLine();
                string str = token.ReferencedObject.ToString();
                if (token.LineNumber != 0)
                {
                    if (token.LinePosition != 0)
                    {
                        builder.Append(System.Xaml.SR.Get("LineNumberAndPosition", new object[] { str, token.LineNumber, token.LinePosition }));
                    }
                    else
                    {
                        builder.Append(System.Xaml.SR.Get("LineNumberOnly", new object[] { str, token.LineNumber }));
                    }
                }
                else
                {
                    builder.Append(str);
                }
            }
            throw new XamlObjectWriterException(builder.ToString());
        }

        public bool WasUninitializedAtEndOfParse(object instance)
        {
            return this._uninitializedObjectsAtParseEnd.ContainsKey(instance);
        }

        public bool HasResolvedTokensPendingProcessing
        {
            get
            {
                return (this._resolvedTokensPendingProcessing.Count > 0);
            }
        }



    }
}

