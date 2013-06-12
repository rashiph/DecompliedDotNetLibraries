namespace System.Text.RegularExpressions
{
    using System;
    using System.Collections;

    internal sealed class CachedCodeEntry
    {
        internal Hashtable _capnames;
        internal Hashtable _caps;
        internal int _capsize;
        internal string[] _capslist;
        internal RegexCode _code;
        internal RegexRunnerFactory _factory;
        internal string _key;
        internal SharedReference _replref;
        internal ExclusiveReference _runnerref;

        internal CachedCodeEntry(string key, Hashtable capnames, string[] capslist, RegexCode code, Hashtable caps, int capsize, ExclusiveReference runner, SharedReference repl)
        {
            this._key = key;
            this._capnames = capnames;
            this._capslist = capslist;
            this._code = code;
            this._caps = caps;
            this._capsize = capsize;
            this._runnerref = runner;
            this._replref = repl;
        }

        internal void AddCompiled(RegexRunnerFactory factory)
        {
            this._factory = factory;
            this._code = null;
        }
    }
}

