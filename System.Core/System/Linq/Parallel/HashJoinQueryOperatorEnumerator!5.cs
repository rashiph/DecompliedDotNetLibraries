namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal class HashJoinQueryOperatorEnumerator<TLeftInput, TLeftKey, TRightInput, THashKey, TOutput> : QueryOperatorEnumerator<TOutput, TLeftKey>
    {
        private readonly CancellationToken m_cancellationToken;
        private readonly Func<TLeftInput, IEnumerable<TRightInput>, TOutput> m_groupResultSelector;
        private readonly IEqualityComparer<THashKey> m_keyComparer;
        private readonly QueryOperatorEnumerator<Pair<TLeftInput, THashKey>, TLeftKey> m_leftSource;
        private Mutables<TLeftInput, TLeftKey, TRightInput, THashKey, TOutput> m_mutables;
        private readonly QueryOperatorEnumerator<Pair<TRightInput, THashKey>, int> m_rightSource;
        private readonly Func<TLeftInput, TRightInput, TOutput> m_singleResultSelector;

        internal HashJoinQueryOperatorEnumerator(QueryOperatorEnumerator<Pair<TLeftInput, THashKey>, TLeftKey> leftSource, QueryOperatorEnumerator<Pair<TRightInput, THashKey>, int> rightSource, Func<TLeftInput, TRightInput, TOutput> singleResultSelector, Func<TLeftInput, IEnumerable<TRightInput>, TOutput> groupResultSelector, IEqualityComparer<THashKey> keyComparer, CancellationToken cancellationToken)
        {
            this.m_leftSource = leftSource;
            this.m_rightSource = rightSource;
            this.m_singleResultSelector = singleResultSelector;
            this.m_groupResultSelector = groupResultSelector;
            this.m_keyComparer = keyComparer;
            this.m_cancellationToken = cancellationToken;
        }

        protected override void Dispose(bool disposing)
        {
            this.m_leftSource.Dispose();
            this.m_rightSource.Dispose();
        }

        internal override bool MoveNext(ref TOutput currentElement, ref TLeftKey currentKey)
        {
            Mutables<TLeftInput, TLeftKey, TRightInput, THashKey, TOutput> mutables = this.m_mutables;
            if (mutables == null)
            {
                mutables = this.m_mutables = new Mutables<TLeftInput, TLeftKey, TRightInput, THashKey, TOutput>();
                mutables.m_rightHashLookup = new HashLookup<THashKey, Pair<TRightInput, ListChunk<TRightInput>>>(this.m_keyComparer);
                Pair<TRightInput, THashKey> pair = new Pair<TRightInput, THashKey>();
                int num = 0;
                int num2 = 0;
                while (this.m_rightSource.MoveNext(ref pair, ref num))
                {
                    if ((num2++ & 0x3f) == 0)
                    {
                        CancellationState.ThrowIfCanceled(this.m_cancellationToken);
                    }
                    TRightInput first = pair.First;
                    THashKey second = pair.Second;
                    if (second != null)
                    {
                        Pair<TRightInput, ListChunk<TRightInput>> pair2 = new Pair<TRightInput, ListChunk<TRightInput>>();
                        if (!mutables.m_rightHashLookup.TryGetValue(second, ref pair2))
                        {
                            pair2 = new Pair<TRightInput, ListChunk<TRightInput>>(first, null);
                            if (this.m_groupResultSelector != null)
                            {
                                pair2.Second = new ListChunk<TRightInput>(2);
                                pair2.Second.Add(first);
                            }
                            mutables.m_rightHashLookup.Add(second, pair2);
                        }
                        else
                        {
                            if (pair2.Second == null)
                            {
                                pair2.Second = new ListChunk<TRightInput>(2);
                                mutables.m_rightHashLookup[second] = pair2;
                            }
                            pair2.Second.Add(first);
                        }
                    }
                }
            }
            ListChunk<TRightInput> currentRightMatches = mutables.m_currentRightMatches;
            if ((currentRightMatches != null) && (mutables.m_currentRightMatchesIndex == currentRightMatches.Count))
            {
                currentRightMatches = mutables.m_currentRightMatches = currentRightMatches.Next;
                mutables.m_currentRightMatchesIndex = 0;
            }
            if (mutables.m_currentRightMatches == null)
            {
                Pair<TLeftInput, THashKey> pair3 = new Pair<TLeftInput, THashKey>();
                TLeftKey local3 = default(TLeftKey);
                while (this.m_leftSource.MoveNext(ref pair3, ref local3))
                {
                    if ((mutables.m_outputLoopCount++ & 0x3f) == 0)
                    {
                        CancellationState.ThrowIfCanceled(this.m_cancellationToken);
                    }
                    Pair<TRightInput, ListChunk<TRightInput>> pair4 = new Pair<TRightInput, ListChunk<TRightInput>>();
                    TLeftInput local4 = pair3.First;
                    THashKey key = pair3.Second;
                    if (((key != null) && mutables.m_rightHashLookup.TryGetValue(key, ref pair4)) && (this.m_singleResultSelector != null))
                    {
                        mutables.m_currentRightMatches = pair4.Second;
                        mutables.m_currentRightMatchesIndex = 0;
                        currentElement = this.m_singleResultSelector(local4, pair4.First);
                        currentKey = local3;
                        if (pair4.Second != null)
                        {
                            mutables.m_currentLeft = local4;
                            mutables.m_currentLeftKey = local3;
                        }
                        return true;
                    }
                    if (this.m_groupResultSelector != null)
                    {
                        IEnumerable<TRightInput> enumerable = pair4.Second;
                        if (enumerable == null)
                        {
                            enumerable = (IEnumerable<TRightInput>) ParallelEnumerable.Empty<TRightInput>();
                        }
                        currentElement = this.m_groupResultSelector(local4, enumerable);
                        currentKey = local3;
                        return true;
                    }
                }
                return false;
            }
            currentElement = this.m_singleResultSelector(mutables.m_currentLeft, mutables.m_currentRightMatches.m_chunk[mutables.m_currentRightMatchesIndex]);
            currentKey = mutables.m_currentLeftKey;
            mutables.m_currentRightMatchesIndex++;
            return true;
        }

        private class Mutables
        {
            internal TLeftInput m_currentLeft;
            internal TLeftKey m_currentLeftKey;
            internal ListChunk<TRightInput> m_currentRightMatches;
            internal int m_currentRightMatchesIndex;
            internal int m_outputLoopCount;
            internal HashLookup<THashKey, Pair<TRightInput, ListChunk<TRightInput>>> m_rightHashLookup;
        }
    }
}

