namespace System.Runtime
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.DurableInstancing;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Xml.Linq;

    internal class PersistencePipeline
    {
        private Stage expectedStage;
        private readonly IEnumerable<IPersistencePipelineModule> modules;
        private ReadOnlyDictionary<XName, InstanceValue> readOnlyView;
        private ValueDictionaryView readWriteView;
        private IDictionary<XName, InstanceValue> values;
        private ValueDictionaryView writeOnlyView;

        public PersistencePipeline(IEnumerable<IPersistencePipelineModule> modules)
        {
            this.expectedStage = Stage.Load;
            this.modules = modules;
        }

        public PersistencePipeline(IEnumerable<IPersistencePipelineModule> modules, Dictionary<XName, InstanceValue> initialValues)
        {
            this.expectedStage = Stage.Collect;
            this.modules = modules;
            this.values = initialValues;
            this.readOnlyView = new ReadOnlyDictionary<XName, InstanceValue>(this.values, false);
            this.readWriteView = new ValueDictionaryView(this.values, false);
            this.writeOnlyView = new ValueDictionaryView(this.values, true);
        }

        public void Abort()
        {
            foreach (IPersistencePipelineModule module in this.modules)
            {
                try
                {
                    module.Abort();
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    throw Fx.Exception.AsError(new CallbackException(SRCore.PersistencePipelineAbortThrew(module.GetType().Name), exception));
                }
            }
        }

        public IAsyncResult BeginLoad(TimeSpan timeout, AsyncCallback callback, object state)
        {
            Fx.AssertAndThrow(this.expectedStage == Stage.Load, "Load called at the wrong time.");
            this.expectedStage = Stage.None;
            return new IOAsyncResult(this, true, timeout, callback, state);
        }

        public IAsyncResult BeginSave(TimeSpan timeout, AsyncCallback callback, object state)
        {
            Fx.AssertAndThrow(this.expectedStage == Stage.Save, "Save called at the wrong time.");
            this.expectedStage = Stage.None;
            return new IOAsyncResult(this, false, timeout, callback, state);
        }

        public void Collect()
        {
            Fx.AssertAndThrow(this.expectedStage == Stage.Collect, "Collect called at the wrong time.");
            this.expectedStage = Stage.None;
            foreach (IPersistencePipelineModule module in this.modules)
            {
                IDictionary<XName, object> dictionary;
                IDictionary<XName, object> dictionary2;
                module.CollectValues(out dictionary, out dictionary2);
                if (dictionary != null)
                {
                    foreach (KeyValuePair<XName, object> pair in dictionary)
                    {
                        try
                        {
                            this.values.Add(pair.Key, new InstanceValue(pair.Value));
                        }
                        catch (ArgumentException exception)
                        {
                            throw Fx.Exception.AsError(new InvalidOperationException(SRCore.NameCollisionOnCollect(pair.Key, module.GetType().Name), exception));
                        }
                    }
                }
                if (dictionary2 != null)
                {
                    foreach (KeyValuePair<XName, object> pair2 in dictionary2)
                    {
                        try
                        {
                            this.values.Add(pair2.Key, new InstanceValue(pair2.Value, InstanceValueOptions.WriteOnly | InstanceValueOptions.Optional));
                        }
                        catch (ArgumentException exception2)
                        {
                            throw Fx.Exception.AsError(new InvalidOperationException(SRCore.NameCollisionOnCollect(pair2.Key, module.GetType().Name), exception2));
                        }
                    }
                }
            }
            this.expectedStage = Stage.Map;
        }

        public void EndLoad(IAsyncResult result)
        {
            IOAsyncResult.End(result);
            this.expectedStage = Stage.Publish;
        }

        public void EndSave(IAsyncResult result)
        {
            IOAsyncResult.End(result);
        }

        public void Map()
        {
            Fx.AssertAndThrow(this.expectedStage == Stage.Map, "Map called at the wrong time.");
            this.expectedStage = Stage.None;
            List<Tuple<IPersistencePipelineModule, IDictionary<XName, object>>> list = null;
            foreach (IPersistencePipelineModule module in this.modules)
            {
                IDictionary<XName, object> dictionary = module.MapValues(this.readWriteView, this.writeOnlyView);
                if (dictionary != null)
                {
                    if (list == null)
                    {
                        list = new List<Tuple<IPersistencePipelineModule, IDictionary<XName, object>>>();
                    }
                    list.Add(new Tuple<IPersistencePipelineModule, IDictionary<XName, object>>(module, dictionary));
                }
            }
            if (list != null)
            {
                foreach (Tuple<IPersistencePipelineModule, IDictionary<XName, object>> tuple in list)
                {
                    foreach (KeyValuePair<XName, object> pair in tuple.Item2)
                    {
                        try
                        {
                            this.values.Add(pair.Key, new InstanceValue(pair.Value, InstanceValueOptions.WriteOnly | InstanceValueOptions.Optional));
                        }
                        catch (ArgumentException exception)
                        {
                            throw Fx.Exception.AsError(new InvalidOperationException(SRCore.NameCollisionOnMap(pair.Key, tuple.Item1.GetType().Name), exception));
                        }
                    }
                }
                this.writeOnlyView.ResetCaches();
            }
            this.expectedStage = Stage.Save;
        }

        public void Publish()
        {
            Fx.AssertAndThrow((this.expectedStage == Stage.Publish) || (this.expectedStage == Stage.Load), "Publish called at the wrong time.");
            this.expectedStage = Stage.None;
            foreach (IPersistencePipelineModule module in this.modules)
            {
                module.PublishValues(this.readWriteView);
            }
        }

        public void SetLoadedValues(IDictionary<XName, InstanceValue> values)
        {
            Fx.AssertAndThrow(this.expectedStage == Stage.Load, "SetLoadedValues called at the wrong time.");
            this.values = values;
            this.readOnlyView = (values as ReadOnlyDictionary<XName, InstanceValue>) ?? new ReadOnlyDictionary<XName, InstanceValue>(values, false);
            this.readWriteView = new ValueDictionaryView(this.values, false);
        }

        public bool IsLoadTransactionRequired
        {
            get
            {
                return (this.modules.FirstOrDefault<IPersistencePipelineModule>(value => value.IsLoadTransactionRequired) != null);
            }
        }

        public bool IsSaveTransactionRequired
        {
            get
            {
                return (this.modules.FirstOrDefault<IPersistencePipelineModule>(value => value.IsSaveTransactionRequired) != null);
            }
        }

        public ReadOnlyDictionary<XName, InstanceValue> Values
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.readOnlyView;
            }
        }

        private class IOAsyncResult : AsyncResult
        {
            private Exception exception;
            private bool isLoad;
            private IPersistencePipelineModule[] pendingModules;
            private PersistencePipeline pipeline;
            private int remainingModules;

            public IOAsyncResult(PersistencePipeline pipeline, bool isLoad, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.pipeline = pipeline;
                this.isLoad = isLoad;
                this.pendingModules = (from value in this.pipeline.modules
                    where value.IsIOParticipant
                    select value).ToArray<IPersistencePipelineModule>();
                this.remainingModules = this.pendingModules.Length;
                bool flag = false;
                if (this.pendingModules.Length == 0)
                {
                    flag = true;
                }
                else
                {
                    for (int i = 0; i < this.pendingModules.Length; i++)
                    {
                        IPersistencePipelineModule module = this.pendingModules[i];
                        IAsyncResult result = null;
                        try
                        {
                            if (this.isLoad)
                            {
                                result = module.BeginOnLoad(this.pipeline.readWriteView, timeout, Fx.ThunkCallback(new AsyncCallback(this.OnIOComplete)), i);
                            }
                            else
                            {
                                result = module.BeginOnSave(this.pipeline.readWriteView, this.pipeline.writeOnlyView, timeout, Fx.ThunkCallback(new AsyncCallback(this.OnIOComplete)), i);
                            }
                        }
                        catch (Exception exception)
                        {
                            if (Fx.IsFatal(exception))
                            {
                                throw;
                            }
                            this.pendingModules[i] = null;
                            this.ProcessException(exception);
                        }
                        if (result == null)
                        {
                            if (this.CompleteOne())
                            {
                                flag = true;
                            }
                        }
                        else if (result.CompletedSynchronously)
                        {
                            this.pendingModules[i] = null;
                            if (this.IOComplete(result, module))
                            {
                                flag = true;
                            }
                        }
                    }
                }
                if (flag)
                {
                    base.Complete(true, this.exception);
                }
            }

            private void Abort()
            {
                for (int i = 0; i < this.pendingModules.Length; i++)
                {
                    IPersistencePipelineModule module = this.pendingModules[i];
                    if (module != null)
                    {
                        try
                        {
                            module.Abort();
                        }
                        catch (Exception exception)
                        {
                            if (Fx.IsFatal(exception))
                            {
                                throw;
                            }
                            throw Fx.Exception.AsError(new CallbackException(SRCore.PersistencePipelineAbortThrew(module.GetType().Name), exception));
                        }
                    }
                }
            }

            private bool CompleteOne()
            {
                return (Interlocked.Decrement(ref this.remainingModules) == 0);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<PersistencePipeline.IOAsyncResult>(result);
            }

            private bool IOComplete(IAsyncResult result, IPersistencePipelineModule module)
            {
                try
                {
                    if (this.isLoad)
                    {
                        module.EndOnLoad(result);
                    }
                    else
                    {
                        module.EndOnSave(result);
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    this.ProcessException(exception);
                }
                return this.CompleteOne();
            }

            private void OnIOComplete(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    int asyncState = (int) result.AsyncState;
                    IPersistencePipelineModule module = this.pendingModules[asyncState];
                    this.pendingModules[asyncState] = null;
                    if (this.IOComplete(result, module))
                    {
                        base.Complete(false, this.exception);
                    }
                }
            }

            private void ProcessException(Exception exception)
            {
                if (exception != null)
                {
                    bool flag = false;
                    lock (this.pendingModules)
                    {
                        if (this.exception == null)
                        {
                            this.exception = exception;
                            flag = true;
                        }
                    }
                    if (flag)
                    {
                        this.Abort();
                    }
                }
            }
        }

        private enum Stage
        {
            None,
            Collect,
            Map,
            Save,
            Load,
            Publish
        }

        private class ValueDictionaryView : IDictionary<XName, object>, ICollection<KeyValuePair<XName, object>>, IEnumerable<KeyValuePair<XName, object>>, IEnumerable
        {
            private IDictionary<XName, InstanceValue> basis;
            private List<XName> keys;
            private List<object> values;
            private bool writeOnly;

            public ValueDictionaryView(IDictionary<XName, InstanceValue> basis, bool writeOnly)
            {
                this.basis = basis;
                this.writeOnly = writeOnly;
            }

            public void Add(KeyValuePair<XName, object> item)
            {
                throw Fx.Exception.AsError(this.CreateReadOnlyException());
            }

            public void Add(XName key, object value)
            {
                throw Fx.Exception.AsError(this.CreateReadOnlyException());
            }

            public void Clear()
            {
                throw Fx.Exception.AsError(this.CreateReadOnlyException());
            }

            public bool Contains(KeyValuePair<XName, object> item)
            {
                object obj2;
                if (!this.TryGetValue(item.Key, out obj2))
                {
                    return false;
                }
                return EqualityComparer<object>.Default.Equals(obj2, item.Value);
            }

            public bool ContainsKey(XName key)
            {
                object obj2;
                return this.TryGetValue(key, out obj2);
            }

            public void CopyTo(KeyValuePair<XName, object>[] array, int arrayIndex)
            {
                foreach (KeyValuePair<XName, object> pair in this)
                {
                    array[arrayIndex++] = pair;
                }
            }

            private Exception CreateReadOnlyException()
            {
                return new InvalidOperationException(SRCore.DictionaryIsReadOnly);
            }

            public IEnumerator<KeyValuePair<XName, object>> GetEnumerator()
            {
                return (from value in this.basis
                    where value.Value.IsWriteOnly() == this.writeOnly
                    select new KeyValuePair<XName, object>(value.Key, value.Value.Value)).GetEnumerator();
            }

            public bool Remove(KeyValuePair<XName, object> item)
            {
                throw Fx.Exception.AsError(this.CreateReadOnlyException());
            }

            public bool Remove(XName key)
            {
                throw Fx.Exception.AsError(this.CreateReadOnlyException());
            }

            internal void ResetCaches()
            {
                this.keys = null;
                this.values = null;
            }

            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            public bool TryGetValue(XName key, out object value)
            {
                InstanceValue value2;
                if (!this.basis.TryGetValue(key, out value2) || (value2.IsWriteOnly() != this.writeOnly))
                {
                    value = null;
                    return false;
                }
                value = value2.Value;
                return true;
            }

            public int Count
            {
                get
                {
                    return this.Keys.Count;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            public object this[XName key]
            {
                get
                {
                    object obj2;
                    if (!this.TryGetValue(key, out obj2))
                    {
                        throw Fx.Exception.AsError(new KeyNotFoundException());
                    }
                    return obj2;
                }
                set
                {
                    throw Fx.Exception.AsError(this.CreateReadOnlyException());
                }
            }

            public ICollection<XName> Keys
            {
                get
                {
                    Func<KeyValuePair<XName, InstanceValue>, bool> predicate = null;
                    if (this.keys == null)
                    {
                        if (predicate == null)
                        {
                            predicate = value => value.Value.IsWriteOnly() == this.writeOnly;
                        }
                        this.keys = new List<XName>(from value in this.basis.Where<KeyValuePair<XName, InstanceValue>>(predicate) select value.Key);
                    }
                    return this.keys;
                }
            }

            public ICollection<object> Values
            {
                get
                {
                    Func<KeyValuePair<XName, InstanceValue>, bool> predicate = null;
                    if (this.values == null)
                    {
                        if (predicate == null)
                        {
                            predicate = value => value.Value.IsWriteOnly() == this.writeOnly;
                        }
                        this.values = new List<object>(from value in this.basis.Where<KeyValuePair<XName, InstanceValue>>(predicate) select value.Value.Value);
                    }
                    return this.values;
                }
            }
        }
    }
}

