namespace System.Runtime.Serialization
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public class SurrogateSelector : ISurrogateSelector
    {
        internal ISurrogateSelector m_nextSelector;
        internal SurrogateHashtable m_surrogates = new SurrogateHashtable(0x20);

        public virtual void AddSurrogate(Type type, StreamingContext context, ISerializationSurrogate surrogate)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (surrogate == null)
            {
                throw new ArgumentNullException("surrogate");
            }
            SurrogateKey key = new SurrogateKey(type, context);
            this.m_surrogates.Add(key, surrogate);
        }

        [SecurityCritical]
        public virtual void ChainSelector(ISurrogateSelector selector)
        {
            if (selector == null)
            {
                throw new ArgumentNullException("selector");
            }
            if (selector == this)
            {
                throw new SerializationException(Environment.GetResourceString("Serialization_DuplicateSelector"));
            }
            if (!HasCycle(selector))
            {
                throw new ArgumentException(Environment.GetResourceString("Serialization_SurrogateCycleInArgument"), "selector");
            }
            ISurrogateSelector nextSelector = selector.GetNextSelector();
            ISurrogateSelector selector5 = selector;
            while ((nextSelector != null) && (nextSelector != this))
            {
                selector5 = nextSelector;
                nextSelector = nextSelector.GetNextSelector();
            }
            if (nextSelector == this)
            {
                throw new ArgumentException(Environment.GetResourceString("Serialization_SurrogateCycle"), "selector");
            }
            nextSelector = selector;
            ISurrogateSelector selector4 = selector;
            while (nextSelector != null)
            {
                if (nextSelector == selector5)
                {
                    nextSelector = this.GetNextSelector();
                }
                else
                {
                    nextSelector = nextSelector.GetNextSelector();
                }
                if (nextSelector == null)
                {
                    break;
                }
                if (nextSelector == selector4)
                {
                    throw new ArgumentException(Environment.GetResourceString("Serialization_SurrogateCycle"), "selector");
                }
                if (nextSelector == selector5)
                {
                    nextSelector = this.GetNextSelector();
                }
                else
                {
                    nextSelector = nextSelector.GetNextSelector();
                }
                if (selector4 == selector5)
                {
                    selector4 = this.GetNextSelector();
                }
                else
                {
                    selector4 = selector4.GetNextSelector();
                }
                if (nextSelector == selector4)
                {
                    throw new ArgumentException(Environment.GetResourceString("Serialization_SurrogateCycle"), "selector");
                }
            }
            ISurrogateSelector selector2 = this.m_nextSelector;
            this.m_nextSelector = selector;
            if (selector2 != null)
            {
                selector5.ChainSelector(selector2);
            }
        }

        [SecurityCritical]
        public virtual ISurrogateSelector GetNextSelector()
        {
            return this.m_nextSelector;
        }

        [SecurityCritical]
        public virtual ISerializationSurrogate GetSurrogate(Type type, StreamingContext context, out ISurrogateSelector selector)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            selector = this;
            SurrogateKey key = new SurrogateKey(type, context);
            ISerializationSurrogate surrogate = (ISerializationSurrogate) this.m_surrogates[key];
            if (surrogate != null)
            {
                return surrogate;
            }
            if (this.m_nextSelector != null)
            {
                return this.m_nextSelector.GetSurrogate(type, context, out selector);
            }
            return null;
        }

        [SecurityCritical]
        private static bool HasCycle(ISurrogateSelector selector)
        {
            ISurrogateSelector nextSelector = selector;
            ISurrogateSelector selector3 = selector;
            while (nextSelector != null)
            {
                nextSelector = nextSelector.GetNextSelector();
                if (nextSelector == null)
                {
                    return true;
                }
                if (nextSelector == selector3)
                {
                    return false;
                }
                nextSelector = nextSelector.GetNextSelector();
                selector3 = selector3.GetNextSelector();
                if (nextSelector == selector3)
                {
                    return false;
                }
            }
            return true;
        }

        public virtual void RemoveSurrogate(Type type, StreamingContext context)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            SurrogateKey key = new SurrogateKey(type, context);
            this.m_surrogates.Remove(key);
        }
    }
}

