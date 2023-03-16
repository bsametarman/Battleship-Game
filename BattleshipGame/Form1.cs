using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace BattleshipGame
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.Size = new Size(740, 880);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        Random rnd = new Random();

        Button[,] opMap = new Button[7, 11];
        Button[,] playerMap = new Button[7, 11];

        //Change here for different ship sizes
        int[,] opCoords = new int[15, 2];

        //Change here for different ship sizes
        int[,] playerCoords = new int[15, 2];

        int opShipPartCount = 14;
        int playerShipPartCount = 14;

        int playerShipSizeToPlace = 5;

        int playerCurrentCoordX = 0;
        int playerCurrentCoordY = 0;

        bool PlayerMapIsVertical = false;
        bool gameStarted = false;
        bool isPlayerTurn = true;
        bool isAnyShipPartCountZero = false;

        // For opponent coord choosing to shot
        int x = -1;
        int y = -1;

        int xCoordHolder = -1;
        int yCoordHolder = -1;

        bool opCoordChooseIsVertical = true;
        bool hitRed = false;
        bool hitException = false;
        int greenCount = 0;


        private void Form1_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;

            // Array for opponent positions, changes x coord to 15
            for (int i = 0; i < opCoords.GetLength(0); i++)
            {
                opCoords[i, 0] = 15;
            }

            // Array for player positions, changes x coord to 15
            for (int i = 0; i < opCoords.GetLength(0); i++)
            {
                playerCoords[i, 0] = 15;
            }

            // Drawing opponent map
            DrawOpMap(20, 20);

            // Placing opponent ships
            for (int i = 5; i > 1; i--)
            {
                PlaceOpShips(i);
            }

            // Changing button state to false before player set the ships
            ChangeButtonEnableToFalse(ref opMap, false);

            // Drawing player map
            PlayerDrawMap(440, 20);

            Button playButton = new Button();
            playButton.Height = 80;
            playButton.Width = 80;
            playButton.Text = "Start Game";
            playButton.Location = new Point(630, 285);
            playButton.Click += new EventHandler(PlayGame_Event);
            this.Controls.Add(playButton);

            Button restartButton = new Button();
            restartButton.Height = 80;
            restartButton.Width = 80;
            restartButton.Text = "Restart";
            restartButton.Location = new Point(630, 370);
            restartButton.Click += new EventHandler(RestartGame_Event);
            this.Controls.Add(restartButton);

            Button exitButton = new Button();
            exitButton.Height = 80;
            exitButton.Width = 80;
            exitButton.Text = "Exit";
            exitButton.Location = new Point(630, 455);
            exitButton.Click += new EventHandler(ExitGame_Event);
            this.Controls.Add(exitButton);

            Button helpButton = new Button();
            helpButton.Height = 80;
            helpButton.Width = 80;
            helpButton.Text = "Help";
            helpButton.Location = new Point(630, 540);
            helpButton.Click += new EventHandler(Help_Event);
            this.Controls.Add(helpButton);
        }

        private void PlayGame_Event(object sender, EventArgs e)
        {
            if (sender != null)
                ((Control)sender).Enabled = false;

            if (playerShipSizeToPlace == 1 && gameStarted == false)
                gameStarted = true;


            var taskController = new CancellationTokenSource();
            var token = taskController.Token;
            Task playGameTask = new Task(new Action(PlayGame), token);

            if (!isAnyShipPartCountZero)
            {
                playGameTask.Start();
            }
            else
            {
                taskController.Cancel();
                taskController.Dispose();
            }

            if (playerShipPartCount == 0)
            {
                MessageBox.Show("Game Over!\nOpponent Won!!!");
                Application.Restart();
            }

            if (opShipPartCount == 0)
            {
                MessageBox.Show("Game Over!\nYou Won!!!");
                Application.Restart();
            }
        }

        private void RestartGame_Event(object sender, EventArgs e)
        {
            Application.Restart();
        }

        private void ExitGame_Event(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Help_Event(object sender, EventArgs e)
        {
            MessageBox.Show("Welcome to the BattleShips game !!!\n\n" +
                            "Place your ships and fight with your opponent.\n\n" +
                            "Left Click: Places ship and shots.\n" +
                            "Right Click: Changes position of ship. (Vertical or horizontal)\n\n" +
                            "Author: bsametarman\n" +
                            "Github: https://github.com/bsametarman");
        }

        private void playerButtonClick_Event(object sender, EventArgs e)
        {
            PlacePlayerShips(playerCurrentCoordX, playerCurrentCoordY, playerShipSizeToPlace, ref playerCoords);
        }

        private void mouseRightClick_Event(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                PlayerMapIsVertical = PlayerMapIsVertical ? false : true;

                // Clear unchanged ship parts
                if (!PlayerMapIsVertical)
                {
                    if (playerCurrentCoordX + playerShipSizeToPlace <= playerMap.GetUpperBound(0))
                    {
                        for (int i = 0; i < playerShipSizeToPlace; i++)
                        {
                            if (!IsMatching(playerCurrentCoordX + i, playerCurrentCoordY, playerShipSizeToPlace, ref playerCoords, false, PlayerMapIsVertical))
                                playerMap[playerCurrentCoordX + i, playerCurrentCoordY].BackColor = Color.DeepSkyBlue;
                        }
                    }
                }
                else
                {
                    if (playerCurrentCoordY + playerShipSizeToPlace <= playerMap.GetUpperBound(1))
                    {
                        for (int i = 0; i < playerShipSizeToPlace; i++)
                        {
                            if (!IsMatching(playerCurrentCoordX, playerCurrentCoordY + i, playerShipSizeToPlace, ref playerCoords, false, PlayerMapIsVertical))
                                playerMap[playerCurrentCoordX, playerCurrentCoordY + i].BackColor = Color.DeepSkyBlue;
                        }
                    }
                }
            }
        }

        private void btnHover_Event(object sender, EventArgs e)
        {
            playerCurrentCoordX = Convert.ToInt16(((Control)sender).Name[0].ToString());
            playerCurrentCoordY = Convert.ToInt16(((Control)sender).Name[1].ToString());

            PreviewPlayerShips(playerCurrentCoordX, playerCurrentCoordY, Color.Black, playerShipSizeToPlace);
        }

        private void btnHoverLeave_Event(object sender, EventArgs e)
        {
            if (!IsMatching(playerCurrentCoordX, playerCurrentCoordY, playerShipSizeToPlace, ref playerCoords, true, PlayerMapIsVertical))
                PreviewPlayerShips(playerCurrentCoordX, playerCurrentCoordY, Color.DeepSkyBlue, playerShipSizeToPlace);
        }

        private void opButtonClick_Event(object sender, EventArgs e)
        {
            if(((Control)sender).BackColor != Color.Red && ((Control)sender).BackColor != Color.Green)
            {
                if (((Control)sender).Name == "x")
                {
                    ((Control)sender).BackColor = Color.Red;
                    ((Control)sender).Enabled = false;

                    opShipPartCount--;
                    isPlayerTurn = false;
                }
                else
                {
                    ((Control)sender).BackColor = Color.Green;
                    ((Control)sender).Enabled = false;
                    isPlayerTurn = false;
                }
            }
        }

        private void PlayGame()
        {
            if (gameStarted)
            {
                while (opShipPartCount != 0 && playerShipPartCount != 0)
                {
                    if (!isPlayerTurn)
                    {
                        if (xCoordHolder == -1 && yCoordHolder == -1)
                        {
                            x = rnd.Next(0, 6);
                            y = rnd.Next(0, 10);
                            xCoordHolder = x;
                            yCoordHolder = y;
                        }
                        else
                        {
                            if (opCoordChooseIsVertical)
                            {
                                UpAndDownCoordChanging();
                            }
                            else
                            {
                                RightAndLeftCoordChanging();
                            }
                        }

                        if (playerMap[x, y].BackColor != Color.Red && playerMap[x, y].BackColor != Color.Green)
                        {
                            OpponentFire();
                        }
                        else if (playerMap[x, y].BackColor == Color.Red)
                        {
                            if (y + 1 < playerMap.GetUpperBound(1) && playerMap[x, y + 1].BackColor != Color.Red && playerMap[x, y + 1].BackColor != Color.Green)
                                y++;
                            else if (y + 1 >= playerMap.GetUpperBound(1) && playerMap[x, y - 1].BackColor != Color.Red && playerMap[x, y - 1].BackColor != Color.Green)
                                y--;
                            else
                            {
                                xCoordHolder = -1;
                                yCoordHolder = -1;
                                PlayGame();
                            }

                            OpponentFire();
                        }
                        else
                        {
                            xCoordHolder = -1;
                            yCoordHolder = -1;
                            PlayGame();
                        }
                    }
                    else
                    {
                        ChangeButtonEnableToFalse(ref opMap, true);
                    }
                }
                gameStarted = false;
                isAnyShipPartCountZero = true;
            }

            PlayGame_Event(null, null);
        }

        private void OpponentFire()
        {
            if (IsMatching(x, y, 0, ref playerCoords, false, false))
            {
                playerShipPartCount--;

                //Debug.WriteLine("Player left ship parts: " + playerShipPartCount);
                //Debug.WriteLine($"Coord ----> [{x},{y}]");

                playerMap[x, y].BackColor = Color.Red;
                hitRed = true;
                isPlayerTurn = true;
                hitException = false;

                if (greenCount == 3)
                {
                    xCoordHolder = x;
                    yCoordHolder = y;
                }
            }
            else
            {
                //Debug.WriteLine("Player left ship parts: " + playerShipPartCount);
                //Debug.WriteLine($"Coord ----> [{x},{y}]");

                playerMap[x, y].BackColor = Color.Green;
                isPlayerTurn = true;

                if ((hitRed && hitException == false) || greenCount == 1)
                {
                    greenCount++;
                }
                if (!hitRed)
                {
                    xCoordHolder = -1;
                    yCoordHolder = -1;
                }

                if (greenCount == 2)
                {
                    opCoordChooseIsVertical = opCoordChooseIsVertical ? false : true;
                    x = xCoordHolder;
                    y = yCoordHolder;
                }

                hitException = false;

                if (greenCount == 3 && hitException == true)
                {
                    opCoordChooseIsVertical = opCoordChooseIsVertical ? false : true;
                    hitRed = false;
                    xCoordHolder = -1;
                    yCoordHolder = -1;
                    PlayGame();
                }
            }
        }

        private void UpAndDownCoordChanging()
        {
            if (x + 1 < playerMap.GetUpperBound(0) && greenCount % 2 == 0 && playerMap[x + 1, y].BackColor != Color.Green && playerMap[x + 1, y].BackColor != Color.Red)
            {
                x++;
            }
            else
            {
                if (x + 1 >= playerMap.GetUpperBound(0) && hitRed && playerMap[x, y].BackColor != Color.Green)
                {
                    greenCount++;
                    hitException = true;
                }

                if (x + 1 < playerMap.GetUpperBound(0) && playerMap[x + 1, y].BackColor == Color.Green && playerMap[x + 1, y].BackColor != Color.DeepSkyBlue && hitRed)
                {
                    greenCount++;
                    hitException = true;
                }

                if (xCoordHolder - 1 >= 0 && playerMap[xCoordHolder - 1, y].BackColor != Color.Red && playerMap[xCoordHolder - 1, y].BackColor != Color.Green)
                {
                    if (x > xCoordHolder)
                    {
                        x = xCoordHolder - 1;
                        if(IsMatching(x, y, 0, ref playerCoords, false, false))
                            xCoordHolder = x;
                    }
                    else if (x > 0)
                    {
                        x--;
                        if (IsMatching(x, y, 0, ref playerCoords, false, false))
                            xCoordHolder = x;
                    }
                }
                else
                {
                    opCoordChooseIsVertical = opCoordChooseIsVertical ? false : true;
                    x = xCoordHolder;

                    greenCount++;

                    hitException = false;
                    PlayGame();
                }
            }
        }

        private void RightAndLeftCoordChanging()
        {
            if (y + 1 < playerMap.GetUpperBound(1) && greenCount % 2 == 0 && playerMap[x, y + 1].BackColor != Color.Green && playerMap[x, y + 1].BackColor != Color.Red)
            {
                y++;
            }
            else
            {
                if (y + 1 >= playerMap.GetUpperBound(1) && hitRed && playerMap[x, y].BackColor != Color.Green)
                {
                    greenCount++;
                    hitException = true;
                }

                if (y + 1 < playerMap.GetUpperBound(1) && playerMap[x, y + 1].BackColor == Color.Green && playerMap[x, y + 1].BackColor != Color.DeepSkyBlue && hitRed)
                {
                    greenCount++;
                    hitException = true;
                }

                if (yCoordHolder - 1 >= 0 && playerMap[x, yCoordHolder - 1].BackColor != Color.Green && playerMap[x, yCoordHolder - 1].BackColor != Color.Red)
                {
                    if (y > yCoordHolder)
                    {
                        y = yCoordHolder - 1;
                    }
                    else if (y > 0)
                        y--;
                }
                else
                {
                    opCoordChooseIsVertical = opCoordChooseIsVertical ? false : true;
                    greenCount = 0;
                    hitRed = false;
                    xCoordHolder = -1;
                    yCoordHolder = -1;
                    PlayGame();
                }
            }
        }


        private void DrawOpMap(int top, int left)
        {
            // Draw Buttons (ships)
            for (int i = 0; i < opMap.GetUpperBound(0); i++)
            {
                for (int x = 0; x < opMap.GetUpperBound(1); x++)
                {
                    opMap[i, x] = new Button();
                    opMap[i, x].Width = 60;
                    opMap[i, x].Height = 60;

                    Point p = new Point(left, top);
                    opMap[i, x].Location = p;

                    opMap[i, x].BackColor = Color.DeepSkyBlue;
                    //opMap[i, x].Text = $"[{i},{x}]";

                    opMap[i, x].Click += new EventHandler(opButtonClick_Event);
                    left += 60;
                    this.Controls.Add(opMap[i, x]);
                }
                top += 60;
                left = 20;
            }
        }

        private int PickRandomCoord(out int yCoord, out bool isVertical)
        {
            isVertical = rnd.Next(0, 2) == 0 ? true : false;

            int xCoord = rnd.Next(0, 6);
            yCoord = rnd.Next(0, 10);

            return xCoord;
        }

        private void PlaceOpShips(int shipSize)
        {
            bool isVertical;
            int yCoord;
            int xCoord = PickRandomCoord(out yCoord, out isVertical);

            //Debug.WriteLine(xCoord + " " + yCoord + " " + isVertical);


            if (isVertical)
            {
                // Checks if ship goes through border
                if (xCoord + shipSize <= opMap.GetUpperBound(0))
                {
                    if (!IsMatching(xCoord, yCoord, shipSize, ref opCoords, true, isVertical))
                    {
                        for (int x = 0; x < shipSize; x++)
                        {
                            //map[xCoord + x, yCoord].BackColor = Color.Black;      //(uncomment if want to see op ships)
                            opMap[xCoord + x, yCoord].Name = "x";
                            Add(ref opCoords, xCoord + x, yCoord);

                            // To see op ship coords
                            //Debug.WriteLine($"[{xCoord + x}, {yCoord}]");
                        }
                    }
                    else
                        PlaceOpShips(shipSize);
                }
                else
                    PlaceOpShips(shipSize);
            }
            else
            {
                // Checks if ship goes through border
                if (yCoord + shipSize <= opMap.GetUpperBound(1))
                {
                    if (!IsMatching(xCoord, yCoord, shipSize, ref opCoords, true, isVertical))
                    {
                        for (int x = 0; x < shipSize; x++)
                        {
                            //map[xCoord, yCoord + x].BackColor = Color.Black;      //(uncomment if want to see op ships)
                            opMap[xCoord, yCoord + x].Name = "x";
                            Add(ref opCoords, xCoord, yCoord + x);

                            // To see op ship coords
                            //Debug.WriteLine($"[{xCoord}, {yCoord + x}]");
                        }
                    }
                    else
                        PlaceOpShips(shipSize);
                }
                else
                    PlaceOpShips(shipSize);
            }
        }


        private void PlayerDrawMap(int top, int left)
        {
            // Draw Buttons (ships)
            for (int i = 0; i < playerMap.GetUpperBound(0); i++)
            {
                for (int x = 0; x < playerMap.GetUpperBound(1); x++)
                {
                    playerMap[i, x] = new Button();
                    playerMap[i, x].Width = 60;
                    playerMap[i, x].Height = 60;

                    Point p = new Point(left, top);
                    playerMap[i, x].Location = p;

                    playerMap[i, x].BackColor = Color.DeepSkyBlue;
                    //playerMap[i, x].Text = $"[{i},{x}]";
                    playerMap[i, x].Name = $"{i}{x}";

                    playerMap[i, x].Click += new EventHandler(playerButtonClick_Event);
                    playerMap[i, x].MouseUp += new MouseEventHandler(mouseRightClick_Event);
                    playerMap[i, x].MouseHover += new EventHandler(btnHover_Event);
                    playerMap[i, x].MouseLeave += new EventHandler(btnHoverLeave_Event);

                    left += 60;
                    this.Controls.Add(playerMap[i, x]);
                }
                top += 60;
                left = 20;
            }
        }

        private void PreviewPlayerShips(int x, int y, Color color, int shipSize)
        {
            if (PlayerMapIsVertical)
            {
                if (x + shipSize <= playerMap.GetUpperBound(0))
                {
                    if(!IsMatching(x, y, shipSize, ref playerCoords, true, PlayerMapIsVertical))
                    {
                        for (int i = 0; i < shipSize; i++)
                        {
                            playerMap[x + i, y].BackColor = color;
                        }
                    } 
                }
            }
            else
            {
                if (y + shipSize <= playerMap.GetUpperBound(1))
                {
                    if (!IsMatching(x, y, shipSize, ref playerCoords, true, PlayerMapIsVertical))
                    {
                        for (int i = 0; i < shipSize; i++)
                        {
                            playerMap[x, y + i].BackColor = color;
                        }
                    }
                }
            }
        }

        private void PlacePlayerShips(int x, int y, int shipSize, ref int[,] playerCoords)
        {
            if (PlayerMapIsVertical)
            {
                if (x + shipSize <= playerMap.GetUpperBound(0))
                {
                    if (!IsMatching(x, y, shipSize, ref playerCoords, true, PlayerMapIsVertical))
                    {
                        for (int i = 0; i < shipSize; i++)
                        {
                            playerMap[x + i, y].BackColor = Color.Black;
                            playerMap[x + i, y].Enabled = false;
                            Add(ref playerCoords, x + i, y);
                        }
                        playerShipSizeToPlace--;
                        if (playerShipSizeToPlace == 1)
                            ChangeButtonEnableToFalse(ref playerMap, false);
                    }
                }
            }
            else
            {
                if (y + shipSize <= playerMap.GetUpperBound(1))
                {
                    if (!IsMatching(x, y, shipSize, ref playerCoords, true, PlayerMapIsVertical))
                    {
                        for (int i = 0; i < shipSize; i++)
                        {
                            playerMap[x, y + i].BackColor = Color.Black;
                            playerMap[x, y + i].Enabled = false;
                            Add(ref playerCoords, x, y + i);
                        }
                        playerShipSizeToPlace--;
                        if (playerShipSizeToPlace == 1)
                            ChangeButtonEnableToFalse(ref playerMap, false);
                    }
                }
            }
        }


        private void ChangeButtonEnableToFalse(ref Button[,] map, bool isEnabled)
        {
            for (int i = 0; i < map.GetUpperBound(0); i++)
            {
                for (int x = 0; x < map.GetUpperBound(1); x++)
                {
                    map[i, x].Enabled = isEnabled;
                } 
            }
        }

        // Add Coordinates to Array
        private void Add(ref int[,] coords, int x, int y)
        {
            for (int i = 0; i <= coords.GetUpperBound(0); i++)
            {
                if (coords[i,0] == 15)
                {
                    coords[i, 0] = x;
                    coords[i, 1] = y;
                    break;
                }
            }
        }

        /// <summary>
        /// This method checks if ships are overlap or not.
        /// </summary>
        /// <param name ="x"> Represents the coordinate X </param>
        /// <param name="y"> Represents the coordinate Y </param>
        /// <param name="shipSize"> The length of ship that will be checked in the array. </param>
        /// <param name="shipCoords"> The array to search coordinates. </param>
        /// <param name="isDetailed"> For the searching array. (True if every part of the ship will be seached in array. False if just one coordinate will be searched in array) </param>
        /// <param name="isVertical"> For the direction of the ships that placed. (True if ships were placed vertical) </param>
        /// <returns>Returns True if coordinate is found in given array.</returns>
        private bool IsMatching(int x, int y, int shipSize, ref int[,] shipCoords, bool isDetailed, bool isVertical)
        {
            if (isDetailed)
            {
                if (isVertical)
                {
                    for (int i = 0; i < shipSize; i++)
                    {
                        for (int j = 0; j < shipCoords.GetUpperBound(0); j++)
                        {
                            if (shipCoords[j, 0] == x && shipCoords[j, 1] == y)
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
                        for (int j = 0; j < shipCoords.GetLength(0); j++)
                        {
                            if (shipCoords[j, 0] == x && shipCoords[j, 1] == y)
                            {
                                return true;
                            }
                        }
                        y++;
                    }
                }

                return false;
            }
            else
            {
                for (int i = 0; i < shipCoords.GetUpperBound(0); i++)
                {
                    if (shipCoords[i, 0] == x && shipCoords[i, 1] == y)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}