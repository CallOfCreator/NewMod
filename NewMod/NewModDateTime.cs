using System;

namespace NewMod
{
    public static class NewModDateTime
    {
        public static DateTime NewModBirthday
        {
            get
            {
                var thisYear = new DateTime(DateTime.Now.Year, 8, 28, 16, 0, 0);
                return DateTime.Now <= thisYear ? thisYear : new DateTime(DateTime.Now.Year + 1, 8, 28);
            }
        }
        public static DateTime NewModBirthdayWeekEnd
        {
            get
            {
                return NewModBirthday.AddDays(7);
            }
        }

        public static bool IsNewModBirthdayWeek =>
            DateTime.Now >= NewModBirthday && DateTime.Now <= NewModBirthdayWeekEnd;
    }
}
