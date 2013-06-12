namespace System.Text.RegularExpressions
{
    using System;
    using System.Collections;

    internal sealed class RegexTree
    {
        internal Hashtable _capnames;
        internal int[] _capnumlist;
        internal Hashtable _caps;
        internal string[] _capslist;
        internal int _captop;
        internal RegexOptions _options;
        internal RegexNode _root;

        internal RegexTree(RegexNode root, Hashtable caps, int[] capnumlist, int captop, Hashtable capnames, string[] capslist, RegexOptions opts)
        {
            this._root = root;
            this._caps = caps;
            this._capnumlist = capnumlist;
            this._capnames = capnames;
            this._capslist = capslist;
            this._captop = captop;
            this._options = opts;
        }
    }
}

