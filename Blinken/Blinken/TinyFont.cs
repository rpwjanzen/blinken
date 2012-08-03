
namespace Blinken
{
    public class Letter
    {
        public readonly byte [,] Data;

        public Letter(byte [,] data)
        {
            Data = data;
        }

        public static readonly Letter Empty = new Letter(new byte[0, 0]);
    }
}
