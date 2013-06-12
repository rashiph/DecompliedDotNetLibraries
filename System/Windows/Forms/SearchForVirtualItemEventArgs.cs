namespace System.Windows.Forms
{
    using System;
    using System.Drawing;

    public class SearchForVirtualItemEventArgs : EventArgs
    {
        private SearchDirectionHint direction;
        private bool includeSubItemsInSearch;
        private int index = -1;
        private bool isPrefixSearch;
        private bool isTextSearch;
        private int startIndex;
        private Point startingPoint;
        private string text;

        public SearchForVirtualItemEventArgs(bool isTextSearch, bool isPrefixSearch, bool includeSubItemsInSearch, string text, Point startingPoint, SearchDirectionHint direction, int startIndex)
        {
            this.isTextSearch = isTextSearch;
            this.isPrefixSearch = isPrefixSearch;
            this.includeSubItemsInSearch = includeSubItemsInSearch;
            this.text = text;
            this.startingPoint = startingPoint;
            this.direction = direction;
            this.startIndex = startIndex;
        }

        public SearchDirectionHint Direction
        {
            get
            {
                return this.direction;
            }
        }

        public bool IncludeSubItemsInSearch
        {
            get
            {
                return this.includeSubItemsInSearch;
            }
        }

        public int Index
        {
            get
            {
                return this.index;
            }
            set
            {
                this.index = value;
            }
        }

        public bool IsPrefixSearch
        {
            get
            {
                return this.isPrefixSearch;
            }
        }

        public bool IsTextSearch
        {
            get
            {
                return this.isTextSearch;
            }
        }

        public int StartIndex
        {
            get
            {
                return this.startIndex;
            }
        }

        public Point StartingPoint
        {
            get
            {
                return this.startingPoint;
            }
        }

        public string Text
        {
            get
            {
                return this.text;
            }
        }
    }
}

