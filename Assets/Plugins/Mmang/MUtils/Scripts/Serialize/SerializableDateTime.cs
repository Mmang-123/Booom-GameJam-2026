

namespace Mmang.Util
{
    [System.Serializable]
    public struct SerializableDateTime
    {
        public int Year;
        public int Month;
        public int Day;
        public int Hour;
        public int Minute;
        public int Second;

        public SerializableDateTime(System.DateTime time)
        {
            Year = time.Year;
            Month = time.Month;
            Day = time.Day;
            Hour = time.Hour;
            Minute = time.Minute;
            Second = time.Second;
        }

        public override string ToString()
        {
            return $"{Year}/{Month}/{Day} {Hour}:{Minute}:{Second}";
        }
    }
}