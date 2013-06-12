namespace System.Web.Hosting
{
    using System;

    public class AppDomainInfoEnum : IAppDomainInfoEnum
    {
        private AppDomainInfo[] _appDomainInfos;
        private int _curPos;

        internal AppDomainInfoEnum(AppDomainInfo[] appDomainInfos)
        {
            this._appDomainInfos = appDomainInfos;
            this._curPos = -1;
        }

        public int Count()
        {
            return this._appDomainInfos.Length;
        }

        public IAppDomainInfo GetData()
        {
            return this._appDomainInfos[this._curPos];
        }

        public bool MoveNext()
        {
            this._curPos++;
            if (this._curPos >= this._appDomainInfos.Length)
            {
                return false;
            }
            return true;
        }

        public void Reset()
        {
            this._curPos = -1;
        }
    }
}

