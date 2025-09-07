using System;

namespace NewMod
{
    public static class NewModDateTime
    {
        private const int Month = 8;
        private const int Day = 28;
        private const int Hour = 16;
        public static readonly TimeSpan Window = TimeSpan.FromDays(8);
        public static DateTime BirthdayStartThisYear =>
            new(DateTime.Now.Year, Month, Day, Hour, 0, 0, DateTimeKind.Local);

        public static DateTime BirthdayStartNextYear =>
            new(DateTime.Now.Year + 1, Month, Day, Hour, 0, 0, DateTimeKind.Local);

        public static DateTime UpcomingBirthdayStart
        {
            get
            {
                var now = DateTime.Now;
                var startThis = BirthdayStartThisYear;
                var endThis = startThis + Window;

                if (now < startThis) return startThis;
                if (now < endThis) return startThis;

                return BirthdayStartNextYear;
            }
        }
        public static DateTime BirthdayWindowEndThisYear => BirthdayStartThisYear + Window;

        public static bool IsNewModBirthdayWeek
        {
            get
            {
                var now = DateTime.Now;
                var start = BirthdayStartThisYear;
                var end = start + Window;
                return now >= start && now < end;
            }
        }
        public static bool IsWraithCallerUnlocked => DateTime.Now >= BirthdayStartThisYear;
        private const int HalloweenMonth = 10;
        private const int HalloweenDay = 31;
        public static readonly TimeSpan HalloweenWindow = TimeSpan.FromDays(7);

        public static DateTime HalloweenStartThisYear =>
            new DateTime(DateTime.Now.Year, HalloweenMonth, HalloweenDay, 0, 0, 0, DateTimeKind.Local);

        public static DateTime HalloweenStartNextYear =>
            new DateTime(DateTime.Now.Year + 1, HalloweenMonth, HalloweenDay, 0, 0, 0, DateTimeKind.Local);
        public static bool IsNewModHalloween
        {
            get
            {
                var now = DateTime.Now;
                return now.Date == HalloweenStartNextYear.Date;
            }
        }
        public static bool IsHalloweenSeason
        {
            get
            {
                var now = DateTime.Now;
                var start = HalloweenStartThisYear;
                var seasonStart = start - HalloweenWindow;
                var seasonEnd = start + HalloweenWindow;
                return now >= seasonStart && now < seasonEnd;
            }
        }
        public static DateTime UpcomingHalloweenStart
        {
            get
            {
                var now = DateTime.Now;
                var startThis = HalloweenStartThisYear;
                var endThis = startThis + HalloweenWindow;
                if (now < startThis) return startThis;
                if (now < endThis) return startThis;
                return HalloweenStartNextYear;
            }
        }
    }
}
