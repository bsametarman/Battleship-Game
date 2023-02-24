using System.Diagnostics;

namespace BattleshipGame
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Random rnd = new Random();

        //Change here for different ship sizes
        int[,] opCoords = new int[14, 2];

        private void button8_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

            Debug.WriteLine("IMPORTANT -----> " + (opCoords.GetLength(0)));
            
            // Array for opponent positions, changes x coord to 15
            for (int i = 0; i < opCoords.GetLength(0); i++)
            {
                opCoords[i, 0] = 15;
            }
            Debug.WriteLine(opCoords[13, 0]);

            Button[,] opMap = new Button[7, 11];
            
            int top = 20;
            int left = 20;

            // Draw Buttons
            for (int i = 0; i < opMap.GetUpperBound(0); i++)
            {
                for (int x = 0; x < opMap.GetUpperBound(1); x++)
                {
                    opMap[i, x] = new Button();
                    opMap[i, x].Width = 60;
                    opMap[i, x].Height = 60;
                    Point p = new Point(left, top);
                    opMap[i,x].Location = p;
                    opMap[i, x].BackColor = Color.DeepSkyBlue;
                    opMap[i, x].Text = $"[{i},{x}]";
                    left += 60;
                    this.Controls.Add(opMap[i, x]);
                }
                top += 60;
                left = 20;
            }

            // Placing Ships
            for (int i = 5; i >= 2; i--)
            {
                PlaceShips(ref opMap, i);
            }

            // Op coords
            for (int i = 0; i < opCoords.GetLength(0); i++)
            {
                Debug.WriteLine($"Coord --> [{opCoords[i, 0]},{opCoords[i, 1]}]");
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            
        }

        private int PickRandomCoord(out int yCoord, out bool isVertical)
        {
            isVertical = rnd.Next(0, 2) == 0 ? true : false;

            int xCoord = rnd.Next(0, 6);
            yCoord = rnd.Next(0, 10);

            return xCoord;
        }

        private void PlaceShips(ref Button[,] map, int shipSize)
        {
            bool isVertical;
            int yCoord;
            int xCoord = PickRandomCoord(out yCoord, out isVertical);

            Debug.WriteLine(xCoord + " " + yCoord + " " + isVertical);

            
            if (isVertical)
            {
                if(xCoord + (shipSize - 1) < map.GetLength(0) - 2)
                {
                    if (IsMatching(xCoord, yCoord, isVertical, shipSize) == false)
                    {
                        for (int x = 0; x < shipSize; x++)
                        {
                            map[xCoord + x, yCoord].BackColor = Color.Black;
                            Add(ref opCoords, xCoord + x, yCoord);
                        }
                    }
                    else
                        PlaceShips(ref map, shipSize);
                }
                else
                    PlaceShips(ref map, shipSize);
            }
            else
            {
                if(yCoord + (shipSize - 1) < map.GetLength(1) - 2)
                {
                    if (IsMatching(xCoord, yCoord, isVertical, shipSize) == false)
                    {
                        for (int x = 0; x < shipSize; x++)
                        {
                            map[xCoord, yCoord + x].BackColor = Color.Black;
                            Add(ref opCoords, xCoord, yCoord + x);
                        }
                    }
                    else
                        PlaceShips(ref map, shipSize);
                }
                else
                    PlaceShips(ref map, shipSize);
            }
            
        }

        // Add Coordinates to Array
        private void Add(ref int[,] coords, int x, int y)
        {
            for (int i = 0; i < coords.GetLength(0); i++)
            {
                if (coords[i,0] == 15)
                {
                    coords[i, 0] = x;
                    coords[i, 1] = y;
                    break;
                }
            }
        }

        // Check if ships overlap
        private bool IsMatching(int x, int y, bool isVertical, int shipSize)
        {
            if (isVertical)
            {
                for (int i = 0; i < shipSize; i++)
                {
                    for (int j = 0; j < opCoords.GetLength(0); j++)
                    {
                        if (opCoords[j, 0] == x && opCoords[j, 1] == y)
                        {
                            return true;
                        }
                    }
                    x++;
                }
            }
            else
            {
                for (int i = 0; i < shipSize; i++)
                {
                    for (int j = 0; j < opCoords.GetLength(0); j++)
                    {
                        if (opCoords[i, 0] == x && opCoords[i, 1] == y)
                        {
                            return true;
                        }
                    }
                    y++;
                }
            }
            return false;
        }
    }
}