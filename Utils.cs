namespace Amber
{
    class Utils
    {
        public static int TryCastInt(string s)
        {
            if (int.TryParse(s, out int x)) return x;
            return -1;
        }
    }
}
