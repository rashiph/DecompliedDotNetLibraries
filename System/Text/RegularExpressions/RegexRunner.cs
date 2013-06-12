namespace System.Text.RegularExpressions
{
    using System;
    using System.ComponentModel;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class RegexRunner
    {
        protected internal int[] runcrawl;
        protected internal int runcrawlpos;
        protected internal Match runmatch;
        protected internal Regex runregex;
        protected internal int[] runstack;
        protected internal int runstackpos;
        protected internal string runtext;
        protected internal int runtextbeg;
        protected internal int runtextend;
        protected internal int runtextpos;
        protected internal int runtextstart;
        protected internal int[] runtrack;
        protected internal int runtrackcount;
        protected internal int runtrackpos;

        protected internal RegexRunner()
        {
        }

        protected void Capture(int capnum, int start, int end)
        {
            if (end < start)
            {
                int num = end;
                end = start;
                start = num;
            }
            this.Crawl(capnum);
            this.runmatch.AddMatch(capnum, start, end - start);
        }

        protected static bool CharInClass(char ch, string charClass)
        {
            return RegexCharClass.CharInClass(ch, charClass);
        }

        protected static bool CharInSet(char ch, string set, string category)
        {
            string str = RegexCharClass.ConvertOldStringsToClass(set, category);
            return RegexCharClass.CharInClass(ch, str);
        }

        protected void Crawl(int i)
        {
            if (this.runcrawlpos == 0)
            {
                this.DoubleCrawl();
            }
            this.runcrawl[--this.runcrawlpos] = i;
        }

        protected int Crawlpos()
        {
            return (this.runcrawl.Length - this.runcrawlpos);
        }

        protected void DoubleCrawl()
        {
            int[] destinationArray = new int[this.runcrawl.Length * 2];
            Array.Copy(this.runcrawl, 0, destinationArray, this.runcrawl.Length, this.runcrawl.Length);
            this.runcrawlpos += this.runcrawl.Length;
            this.runcrawl = destinationArray;
        }

        protected void DoubleStack()
        {
            int[] destinationArray = new int[this.runstack.Length * 2];
            Array.Copy(this.runstack, 0, destinationArray, this.runstack.Length, this.runstack.Length);
            this.runstackpos += this.runstack.Length;
            this.runstack = destinationArray;
        }

        protected void DoubleTrack()
        {
            int[] destinationArray = new int[this.runtrack.Length * 2];
            Array.Copy(this.runtrack, 0, destinationArray, this.runtrack.Length, this.runtrack.Length);
            this.runtrackpos += this.runtrack.Length;
            this.runtrack = destinationArray;
        }

        protected void EnsureStorage()
        {
            if (this.runstackpos < (this.runtrackcount * 4))
            {
                this.DoubleStack();
            }
            if (this.runtrackpos < (this.runtrackcount * 4))
            {
                this.DoubleTrack();
            }
        }

        protected abstract bool FindFirstChar();
        protected abstract void Go();
        private void InitMatch()
        {
            if (this.runmatch == null)
            {
                if (this.runregex.caps != null)
                {
                    this.runmatch = new MatchSparse(this.runregex, this.runregex.caps, this.runregex.capsize, this.runtext, this.runtextbeg, this.runtextend - this.runtextbeg, this.runtextstart);
                }
                else
                {
                    this.runmatch = new Match(this.runregex, this.runregex.capsize, this.runtext, this.runtextbeg, this.runtextend - this.runtextbeg, this.runtextstart);
                }
            }
            else
            {
                this.runmatch.Reset(this.runregex, this.runtext, this.runtextbeg, this.runtextend, this.runtextstart);
            }
            if (this.runcrawl != null)
            {
                this.runtrackpos = this.runtrack.Length;
                this.runstackpos = this.runstack.Length;
                this.runcrawlpos = this.runcrawl.Length;
            }
            else
            {
                this.InitTrackCount();
                int num = this.runtrackcount * 8;
                int num2 = this.runtrackcount * 8;
                if (num < 0x20)
                {
                    num = 0x20;
                }
                if (num2 < 0x10)
                {
                    num2 = 0x10;
                }
                this.runtrack = new int[num];
                this.runtrackpos = num;
                this.runstack = new int[num2];
                this.runstackpos = num2;
                this.runcrawl = new int[0x20];
                this.runcrawlpos = 0x20;
            }
        }

        protected abstract void InitTrackCount();
        protected bool IsBoundary(int index, int startpos, int endpos)
        {
            return (((index > startpos) && RegexCharClass.IsWordChar(this.runtext[index - 1])) != ((index < endpos) && RegexCharClass.IsWordChar(this.runtext[index])));
        }

        protected bool IsECMABoundary(int index, int startpos, int endpos)
        {
            return (((index > startpos) && RegexCharClass.IsECMAWordChar(this.runtext[index - 1])) != ((index < endpos) && RegexCharClass.IsECMAWordChar(this.runtext[index])));
        }

        protected bool IsMatched(int cap)
        {
            return this.runmatch.IsMatched(cap);
        }

        protected int MatchIndex(int cap)
        {
            return this.runmatch.MatchIndex(cap);
        }

        protected int MatchLength(int cap)
        {
            return this.runmatch.MatchLength(cap);
        }

        protected int Popcrawl()
        {
            return this.runcrawl[this.runcrawlpos++];
        }

        protected internal Match Scan(Regex regex, string text, int textbeg, int textend, int textstart, int prevlen, bool quick)
        {
            bool flag = false;
            this.runregex = regex;
            this.runtext = text;
            this.runtextbeg = textbeg;
            this.runtextend = textend;
            this.runtextstart = textstart;
            int num = this.runregex.RightToLeft ? -1 : 1;
            int num2 = this.runregex.RightToLeft ? this.runtextbeg : this.runtextend;
            this.runtextpos = textstart;
            if (prevlen == 0)
            {
                if (this.runtextpos == num2)
                {
                    return Match.Empty;
                }
                this.runtextpos += num;
            }
            while (true)
            {
                if (this.FindFirstChar())
                {
                    if (!flag)
                    {
                        this.InitMatch();
                        flag = true;
                    }
                    this.Go();
                    if (this.runmatch._matchcount[0] > 0)
                    {
                        return this.TidyMatch(quick);
                    }
                    this.runtrackpos = this.runtrack.Length;
                    this.runstackpos = this.runstack.Length;
                    this.runcrawlpos = this.runcrawl.Length;
                }
                if (this.runtextpos == num2)
                {
                    this.TidyMatch(true);
                    return Match.Empty;
                }
                this.runtextpos += num;
            }
        }

        private Match TidyMatch(bool quick)
        {
            if (!quick)
            {
                Match runmatch = this.runmatch;
                this.runmatch = null;
                runmatch.Tidy(this.runtextpos);
                return runmatch;
            }
            return null;
        }

        protected void TransferCapture(int capnum, int uncapnum, int start, int end)
        {
            if (end < start)
            {
                int num3 = end;
                end = start;
                start = num3;
            }
            int num = this.MatchIndex(uncapnum);
            int num2 = num + this.MatchLength(uncapnum);
            if (start >= num2)
            {
                end = start;
                start = num2;
            }
            else if (end <= num)
            {
                start = num;
            }
            else
            {
                if (end > num2)
                {
                    end = num2;
                }
                if (num > start)
                {
                    start = num;
                }
            }
            this.Crawl(uncapnum);
            this.runmatch.BalanceMatch(uncapnum);
            if (capnum != -1)
            {
                this.Crawl(capnum);
                this.runmatch.AddMatch(capnum, start, end - start);
            }
        }

        protected void Uncapture()
        {
            int cap = this.Popcrawl();
            this.runmatch.RemoveMatch(cap);
        }
    }
}

