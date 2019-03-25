namespace ChayaBot.Core.Games
{
    public class TicTacToeGrid
    {

        // Properties
        public byte Size { get; private set; }


        // Fields
        private CellTypes[,] cells;


        // Constructor
        public TicTacToeGrid(byte size)
        {
            Size = size;
            cells = new CellTypes[Size, Size];

            InitializeCells();
        }


        public void InitializeCells()
        {
            for (int x = 0; x < Size; x++)
            {
                for (int y = 0; y < Size; y++)
                {
                    cells[x, y] = CellTypes.NONE;
                }
            }
        }

        public CellTypes[,] GetCopy()
        {
            CellTypes[,] copy = new CellTypes[Size, Size];

            for (int x = 0; x < Size; x++)
            {
                for (int y = 0; y < Size; y++)
                {
                    copy[x, y] = cells[x, y];
                }
            }

            return copy;
        }

        public bool IsWon(int startX, int startY, CellTypes type, Directions direction)
        {
            if (direction == Directions.HORIZONTAL)
            {
                for (int y = 0; y < Size; y++)
                {
                    if (cells[startX, y] != type)
                        return false;
                }
            }
            else if (direction == Directions.VERTICAL)
            {
                for (int x = 0; x < Size; x++)
                {
                    if (cells[x, startY] != type)
                        return false;
                }
            }
            else // Diagonal
            {
                int count = 1;
                int x, y;

                // Up_Left
                x = startX - 1;
                y = startY - 1;

                while (x >= 0 && y >= 0)
                {
                    if (cells[x, y] != type)
                        break;

                    count++;
                    x--;
                    y--;
                }

                // Down_Right
                x = startX + 1;
                y = startY + 1;

                while (x < Size && y < Size)
                {
                    if (cells[x, y] != type)
                        break;

                    count++;
                    x++;
                    y++;
                }

                if (count == Size)
                    return true;

                count = 1;

                // Up_Right
                x = startX - 1;
                y = startY + 1;

                while (x >= 0 && y < Size)
                {
                    if (cells[x, y] != type)
                        break;

                    count++;
                    x--;
                    y++;
                }

                // Down_Left
                x = startX + 1;
                y = startY - 1;

                while (x < Size && y >= 0)
                {
                    if (cells[x, y] != type)
                        break;

                    count++;
                    x++;
                    y--;
                }

                return count == Size;
            }

            return true;
        }

        public bool IsFilled()
        {
            for (int x = 0; x < Size; x++)
            {
                for (int y = 0; y < Size; y++)
                {
                    if (cells[x, y] == CellTypes.NONE)
                        return false;
                }
            }

            return true;
        }

        public bool TryGetCell(int x, int y, out CellTypes? cellType)
        {
            cellType = null;

            if (x < 0 || x > Size || y < 0 || y > Size)
                return false;

            cellType = cells[x, y];
            return true;
        }

        public bool ChangeCellType(int x, int y, CellTypes type)
        {
            if (!TryGetCell(x, y, out CellTypes? cellType))
                return false;

            cells[x, y] = type;
            return true;
        }

    }
}
