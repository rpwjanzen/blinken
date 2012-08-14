
namespace Blinken
{
    public class Letter
    {
        public readonly bool[,] Data;

        public Letter(bool[,] data)
        {
            Data = data;
        }

        public static readonly Letter Empty = new Letter(new bool[0, 0]);
    }
}
