using System.Diagnostics.CodeAnalysis;

namespace RomgleWebApi.Data.Models
{
    public struct DateAndTime
    {
        public DateTime Date { get; set; }

        public TimeSpan Time { get; set; }

        #region equals and hash code

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (!(obj is DateAndTime dateAndTime))
            {
                return false;
            }
            return ((DateTime)this).Equals(dateAndTime);
        }

        public override int GetHashCode()
        {
            return ((DateTime)this).GetHashCode();
        }

        #endregion

        #region DateAndTime operators

        public static bool operator ==(DateAndTime d1, DateAndTime d2)
        {
            return (DateTime)d1 == (DateTime)d2;
        }

        public static bool operator !=(DateAndTime d1, DateAndTime d2)
        {
            return (DateTime)d1 != (DateTime)d2;
        }

        public static bool operator <(DateAndTime d1, DateAndTime d2)
        {
            return (DateTime)d1 < (DateTime)d2;
        }

        public static bool operator >(DateAndTime d1, DateAndTime d2)
        {
            return (DateTime)d1 > (DateTime)d2;
        }

        public static bool operator <=(DateAndTime d1, DateAndTime d2)
        {
            return (DateTime)d1 <= (DateTime)d2;
        }

        public static bool operator >=(DateAndTime d1, DateAndTime d2)
        {
            return (DateTime)d1 >= (DateTime)d2;
        }

        #endregion DateAndTime operators

        #region DateTime operators

        #region left

        public static bool operator ==(DateAndTime d1, DateTime d2)
        {
            return (DateTime)d1 == d2;
        }

        public static bool operator !=(DateAndTime d1, DateTime d2)
        {
            return (DateTime)d1 != d2;
        }

        public static bool operator <(DateAndTime d1, DateTime d2)
        {
            return (DateTime)d1 < d2;
        }

        public static bool operator >(DateAndTime d1, DateTime d2)
        {
            return (DateTime)d1 > d2;
        }

        public static bool operator <=(DateAndTime d1, DateTime d2)
        {
            return (DateTime)d1 <= d2;
        }

        public static bool operator >=(DateAndTime d1, DateTime d2)
        {
            return (DateTime)d1 >= d2;
        }

        #endregion left

        #region right

        public static bool operator ==(DateTime d1, DateAndTime d2)
        {
            return d1 == (DateTime)d2;
        }

        public static bool operator !=(DateTime d1, DateAndTime d2)
        {
            return d1 != (DateTime)d2;
        }

        public static bool operator <(DateTime d1, DateAndTime d2)
        {
            return d1 < (DateTime)d2;
        }

        public static bool operator >(DateTime d1, DateAndTime d2)
        {
            return d1 > (DateTime)d2;
        }

        public static bool operator <=(DateTime d1, DateAndTime d2)
        {
            return d1 <= (DateTime)d2;
        }

        public static bool operator >=(DateTime d1, DateAndTime d2)
        {
            return d1 >= (DateTime)d2;
        }

        #endregion right

        #endregion DateTime operators

        #region implicit operators

        public static implicit operator DateTime(DateAndTime dateAndTime)
        {
            return dateAndTime.Date.Date.Add(dateAndTime.Time);
        }

        public static implicit operator DateAndTime(DateTime dateTime)
        {
            return new DateAndTime
            {
                Date = dateTime.Date,
                Time = dateTime.TimeOfDay
            };
        }

        #endregion implicit operators
    }
}
