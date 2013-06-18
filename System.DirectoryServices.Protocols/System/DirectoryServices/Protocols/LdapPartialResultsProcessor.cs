namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Collections;
    using System.Threading;

    internal class LdapPartialResultsProcessor
    {
        private int currentIndex;
        private ArrayList resultList = new ArrayList();
        private ManualResetEvent workThreadWaitHandle;
        private bool workToDo;

        internal LdapPartialResultsProcessor(ManualResetEvent eventHandle)
        {
            this.workThreadWaitHandle = eventHandle;
        }

        public void Add(LdapPartialAsyncResult asyncResult)
        {
            lock (this)
            {
                this.resultList.Add(asyncResult);
                if (!this.workToDo)
                {
                    this.workThreadWaitHandle.Set();
                    this.workToDo = true;
                }
            }
        }

        private void AddResult(SearchResponse partialResults, SearchResponse newResult)
        {
            if (newResult != null)
            {
                if (newResult.Entries != null)
                {
                    for (int i = 0; i < newResult.Entries.Count; i++)
                    {
                        partialResults.Entries.Add(newResult.Entries[i]);
                    }
                }
                if (newResult.References != null)
                {
                    for (int j = 0; j < newResult.References.Count; j++)
                    {
                        partialResults.References.Add(newResult.References[j]);
                    }
                }
            }
        }

        public DirectoryResponse GetCompleteResult(LdapPartialAsyncResult asyncResult)
        {
            lock (this)
            {
                if (!this.resultList.Contains(asyncResult))
                {
                    throw new ArgumentException(Res.GetString("InvalidAsyncResult"));
                }
                this.resultList.Remove(asyncResult);
                if (asyncResult.exception != null)
                {
                    throw asyncResult.exception;
                }
                return asyncResult.response;
            }
        }

        public PartialResultsCollection GetPartialResults(LdapPartialAsyncResult asyncResult)
        {
            lock (this)
            {
                if (!this.resultList.Contains(asyncResult))
                {
                    throw new ArgumentException(Res.GetString("InvalidAsyncResult"));
                }
                if (asyncResult.exception != null)
                {
                    this.resultList.Remove(asyncResult);
                    throw asyncResult.exception;
                }
                PartialResultsCollection resultss = new PartialResultsCollection();
                if (asyncResult.response != null)
                {
                    if (asyncResult.response.Entries != null)
                    {
                        for (int i = 0; i < asyncResult.response.Entries.Count; i++)
                        {
                            resultss.Add(asyncResult.response.Entries[i]);
                        }
                        asyncResult.response.Entries.Clear();
                    }
                    if (asyncResult.response.References != null)
                    {
                        for (int j = 0; j < asyncResult.response.References.Count; j++)
                        {
                            resultss.Add(asyncResult.response.References[j]);
                        }
                        asyncResult.response.References.Clear();
                    }
                }
                return resultss;
            }
        }

        private void GetResultsHelper(LdapPartialAsyncResult asyncResult)
        {
            LdapConnection con = asyncResult.con;
            IntPtr ldapHandle = con.ldapHandle;
            ResultAll resultType = ResultAll.LDAP_MSG_RECEIVED;
            if (asyncResult.resultStatus == ResultsStatus.CompleteResult)
            {
                resultType = ResultAll.LDAP_MSG_POLLINGALL;
            }
            try
            {
                SearchResponse newResult = (SearchResponse) con.ConstructResponse(asyncResult.messageID, LdapOperation.LdapSearch, resultType, asyncResult.requestTimeout, false);
                if (newResult == null)
                {
                    if ((asyncResult.startTime.Ticks + asyncResult.requestTimeout.Ticks) <= DateTime.Now.Ticks)
                    {
                        throw new LdapException(0x55, LdapErrorMappings.MapResultCode(0x55));
                    }
                }
                else
                {
                    if (asyncResult.response != null)
                    {
                        this.AddResult(asyncResult.response, newResult);
                    }
                    else
                    {
                        asyncResult.response = newResult;
                    }
                    if (newResult.searchDone)
                    {
                        asyncResult.resultStatus = ResultsStatus.Done;
                    }
                }
            }
            catch (Exception exception)
            {
                if (exception is DirectoryOperationException)
                {
                    SearchResponse response = (SearchResponse) ((DirectoryOperationException) exception).Response;
                    if (asyncResult.response != null)
                    {
                        this.AddResult(asyncResult.response, response);
                    }
                    else
                    {
                        asyncResult.response = response;
                    }
                    ((DirectoryOperationException) exception).response = asyncResult.response;
                }
                else if (exception is LdapException)
                {
                    LdapException exception2 = (LdapException) exception;
                    int errorCode = exception2.ErrorCode;
                    if (asyncResult.response != null)
                    {
                        if (asyncResult.response.Entries != null)
                        {
                            for (int i = 0; i < asyncResult.response.Entries.Count; i++)
                            {
                                exception2.results.Add(asyncResult.response.Entries[i]);
                            }
                        }
                        if (asyncResult.response.References != null)
                        {
                            for (int j = 0; j < asyncResult.response.References.Count; j++)
                            {
                                exception2.results.Add(asyncResult.response.References[j]);
                            }
                        }
                    }
                }
                asyncResult.exception = exception;
                asyncResult.resultStatus = ResultsStatus.Done;
                Wldap32.ldap_abandon(con.ldapHandle, asyncResult.messageID);
            }
        }

        public void NeedCompleteResult(LdapPartialAsyncResult asyncResult)
        {
            lock (this)
            {
                if (!this.resultList.Contains(asyncResult))
                {
                    throw new ArgumentException(Res.GetString("InvalidAsyncResult"));
                }
                if (asyncResult.resultStatus == ResultsStatus.PartialResult)
                {
                    asyncResult.resultStatus = ResultsStatus.CompleteResult;
                }
            }
        }

        public void Remove(LdapPartialAsyncResult asyncResult)
        {
            lock (this)
            {
                if (!this.resultList.Contains(asyncResult))
                {
                    throw new ArgumentException(Res.GetString("InvalidAsyncResult"));
                }
                this.resultList.Remove(asyncResult);
            }
        }

        public void RetrievingSearchResults()
        {
            int count = 0;
            int num2 = 0;
            LdapPartialAsyncResult asyncResult = null;
            AsyncCallback callback = null;
            lock (this)
            {
                count = this.resultList.Count;
                if (count == 0)
                {
                    this.workThreadWaitHandle.Reset();
                    this.workToDo = false;
                    return;
                }
            Label_003D:
                if (this.currentIndex >= count)
                {
                    this.currentIndex = 0;
                }
                asyncResult = (LdapPartialAsyncResult) this.resultList[this.currentIndex];
                num2++;
                this.currentIndex++;
                if (asyncResult.resultStatus == ResultsStatus.Done)
                {
                    if (num2 >= count)
                    {
                        this.workToDo = false;
                        this.workThreadWaitHandle.Reset();
                        return;
                    }
                    goto Label_003D;
                }
                this.GetResultsHelper(asyncResult);
                if (asyncResult.resultStatus == ResultsStatus.Done)
                {
                    asyncResult.manualResetEvent.Set();
                    asyncResult.completed = true;
                    if (asyncResult.callback != null)
                    {
                        callback = asyncResult.callback;
                    }
                }
                else if ((((asyncResult.callback != null) && asyncResult.partialCallback) && (asyncResult.response != null)) && ((asyncResult.response.Entries.Count > 0) || (asyncResult.response.References.Count > 0)))
                {
                    callback = asyncResult.callback;
                }
            }
            if (callback != null)
            {
                callback(asyncResult);
            }
        }
    }
}

