namespace System.Globalization
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    internal class EraInfo
    {
        [OptionalField(VersionAdded=4)]
        internal string abbrevEraName;
        [OptionalField(VersionAdded=4)]
        internal string englishEraName;
        internal int era;
        [OptionalField(VersionAdded=4)]
        internal string eraName;
        internal int maxEraYear;
        internal int minEraYear;
        internal long ticks;
        internal int yearOffset;

        internal EraInfo(int era, int startYear, int startMonth, int startDay, int yearOffset, int minEraYear, int maxEraYear)
        {
            this.era = era;
            this.yearOffset = yearOffset;
            this.minEraYear = minEraYear;
            this.maxEraYear = maxEraYear;
            DateTime time = new DateTime(startYear, startMonth, startDay);
            this.ticks = time.Ticks;
        }

        internal EraInfo(int era, int startYear, int startMonth, int startDay, int yearOffset, int minEraYear, int maxEraYear, string eraName, string abbrevEraName, string englishEraName)
        {
            this.era = era;
            this.yearOffset = yearOffset;
            this.minEraYear = minEraYear;
            this.maxEraYear = maxEraYear;
            DateTime time = new DateTime(startYear, startMonth, startDay);
            this.ticks = time.Ticks;
            this.eraName = eraName;
            this.abbrevEraName = abbrevEraName;
            this.englishEraName = englishEraName;
        }
    }
}

