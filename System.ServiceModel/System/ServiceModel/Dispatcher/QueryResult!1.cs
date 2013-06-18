namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;

    internal class QueryResult<TResult> : IEnumerable<KeyValuePair<MessageQuery, TResult>>, IEnumerable
    {
        private bool evalBody;
        private QueryMatcher matcher;
        private Message message;

        internal QueryResult(QueryMatcher matcher, Message message, bool evalBody)
        {
            this.matcher = matcher;
            this.message = message;
            this.evalBody = evalBody;
        }

        public IEnumerator<KeyValuePair<MessageQuery, TResult>> GetEnumerator()
        {
            IEnumerator<KeyValuePair<MessageQuery, TResult>> enumerator;
            QueryProcessor processor = this.matcher.CreateProcessor();
            Collection<KeyValuePair<MessageQuery, XPathResult>> collection = new Collection<KeyValuePair<MessageQuery, XPathResult>>();
            processor.ResultSet = collection;
            try
            {
                processor.Eval(this.matcher.RootOpcode, this.message, this.evalBody);
                if (typeof(TResult) == typeof(XPathResult))
                {
                    return (IEnumerator<KeyValuePair<MessageQuery, TResult>>) collection.GetEnumerator();
                }
                if ((!(typeof(TResult) == typeof(string)) && !(typeof(TResult) == typeof(bool))) && !(typeof(TResult) == typeof(object)))
                {
                    throw Fx.AssertAndThrowFatal("unsupported type");
                }
                Collection<KeyValuePair<MessageQuery, TResult>> collection2 = new Collection<KeyValuePair<MessageQuery, TResult>>();
                foreach (KeyValuePair<MessageQuery, XPathResult> pair in collection)
                {
                    if (typeof(TResult) == typeof(string))
                    {
                        collection2.Add(new KeyValuePair<MessageQuery, TResult>(pair.Key, (TResult) pair.Value.GetResultAsString()));
                    }
                    else if (typeof(TResult) == typeof(bool))
                    {
                        collection2.Add(new KeyValuePair<MessageQuery, TResult>(pair.Key, (TResult) pair.Value.GetResultAsBoolean()));
                    }
                    else
                    {
                        collection2.Add(new KeyValuePair<MessageQuery, TResult>(pair.Key, (TResult) pair.Value));
                    }
                }
                enumerator = collection2.GetEnumerator();
            }
            catch (XPathNavigatorException exception)
            {
                throw TraceUtility.ThrowHelperError(exception.Process(this.matcher.RootOpcode), this.message);
            }
            catch (NavigatorInvalidBodyAccessException exception2)
            {
                throw TraceUtility.ThrowHelperError(exception2.Process(this.matcher.RootOpcode), this.message);
            }
            finally
            {
                if (this.evalBody)
                {
                    this.message.Close();
                }
                this.matcher.ReleaseProcessor(processor);
            }
            return enumerator;
        }

        public TResult GetSingleResult()
        {
            XPathResult queryResult;
            QueryProcessor processor = this.matcher.CreateProcessor();
            try
            {
                processor.Eval(this.matcher.RootOpcode, this.message, this.evalBody);
            }
            catch (XPathNavigatorException exception)
            {
                throw TraceUtility.ThrowHelperError(exception.Process(this.matcher.RootOpcode), this.message);
            }
            catch (NavigatorInvalidBodyAccessException exception2)
            {
                throw TraceUtility.ThrowHelperError(exception2.Process(this.matcher.RootOpcode), this.message);
            }
            finally
            {
                if (this.evalBody)
                {
                    this.message.Close();
                }
                queryResult = processor.QueryResult;
                this.matcher.ReleaseProcessor(processor);
            }
            if ((typeof(TResult) == typeof(XPathResult)) || (typeof(TResult) == typeof(object)))
            {
                return (TResult) queryResult;
            }
            if (typeof(TResult) == typeof(string))
            {
                return (TResult) queryResult.GetResultAsString();
            }
            if (typeof(TResult) != typeof(bool))
            {
                throw Fx.AssertAndThrowFatal("unsupported type");
            }
            return (TResult) queryResult.GetResultAsBoolean();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}

