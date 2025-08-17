using System;

namespace NewMod;
public static class NewModDateTime
{
    public static DateTime NewModBirthday
    {
        get
        {
            var thisYear = new DateTime(DateTime.Now.Year, 8, 28);
            return DateTime.Now <= thisYear ? thisYear : new DateTime(DateTime.Now.Year + 1, 8, 28);
        }
    }
}
