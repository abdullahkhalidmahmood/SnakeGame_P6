using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading;
using System.Media;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Data;

namespace Snake
{
    //Define a structure for the position for every object in the game by row and column
    struct Position
    {
        public int row;
        public int col;
        public Position(int row, int col)
        {
            this.row = row;
            this.col = col;
        }
    }

    class Program
    {
        /// <summary>
        /// Functions start here
        /// </summary>
        public void BackgroundMusic()
        {
            //Create SoundPlayer objbect to control sound playback
            SoundPlayer backgroundMusic = new SoundPlayer();
            
            //Locate the SoundPlayer to the correct sound directory
            backgroundMusic.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "/SnakeBGM_1.wav";
            
            //Play the background music at the beginning
            backgroundMusic.PlayLooping();
        }

        /// <summary>
        /// Play the sound effect when player lose
        /// </summary>
        public void LoseSoundEffect()
        {
            SoundPlayer playerDie = new SoundPlayer();
            playerDie.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "/SnakeLose_1.wav";
            playerDie.Play(); //Play the die sound effect after player died
        }

        public void DrawFood()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("@");
        }
        
        public void DrawObstacle()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("=");
        }

        public void DrawSnakeBody()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("*");
        }
        
        public void Direction(Position[] directions)
        {
            
            directions[0] = new Position(0, 1);
            directions[1] = new Position(0, -1);
            directions[2] = new Position(1, 0);
            directions[3] = new Position(-1, 0);

        }
       public void InitialRandomObstacles(List<Position>obstacles)
        {
            //Create obstacles objects and initialise certain random position of obstacles at every game play
            //The randomise obstacles will not exist in the first row at the beginning.
            Random randomNumbersGenerator = new Random();
            obstacles.Add(new Position(randomNumbersGenerator.Next(1, Console.WindowHeight), randomNumbersGenerator.Next(0, Console.WindowWidth)));
            obstacles.Add(new Position(randomNumbersGenerator.Next(1, Console.WindowHeight), randomNumbersGenerator.Next(0, Console.WindowWidth)));
            obstacles.Add(new Position(randomNumbersGenerator.Next(1, Console.WindowHeight), randomNumbersGenerator.Next(0, Console.WindowWidth)));
            obstacles.Add(new Position(randomNumbersGenerator.Next(1, Console.WindowHeight), randomNumbersGenerator.Next(0, Console.WindowWidth)));
            obstacles.Add(new Position(randomNumbersGenerator.Next(1, Console.WindowHeight), randomNumbersGenerator.Next(0, Console.WindowWidth)));
            
            //Show the obstacle in the windows with marking of "="
            foreach (Position obstacle in obstacles)
            {
                Console.SetCursorPosition(obstacle.col, obstacle.row);
                DrawObstacle();
            }
        }

        public void CheckUserInput(ref int direction, byte right, byte left, byte down,byte up)
        {
            
            //User key pressed statement: depends on which direction the user want to go to get food or avoid obstacle
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo userInput = Console.ReadKey();
                if (userInput.Key == ConsoleKey.LeftArrow)
                {
                    if (direction != right) direction = left;
                }
                if (userInput.Key == ConsoleKey.RightArrow)
                {
                    if (direction != left) direction = right;
                }
                if (userInput.Key == ConsoleKey.UpArrow)
                {
                    if (direction != down) direction = up;
                }
                if (userInput.Key == ConsoleKey.DownArrow)
                {
                    if (direction != up) direction = down;
                }
            }
        }
        

        public int GameOverCheck(int currentTime, Queue<Position> snakeElements, Position snakeNewHead,int negativePoints, List<Position> obstacles)
        {
            if (snakeElements.Contains(snakeNewHead) || obstacles.Contains(snakeNewHead) || (Environment.TickCount-currentTime) > 30000)
            {
                LoseSoundEffect();
                Console.SetCursorPosition(0, 0);
                Console.ForegroundColor = ConsoleColor.Red;//Text color for game over
               
                int userPoints = (snakeElements.Count - 4) * 100 - negativePoints;//points calculated for player
                userPoints = Math.Max(userPoints, 0); //if (userPoints < 0) userPoints = 0;
                
                //Display all 
                PrintLinesInCenter("Game Over!", "Your points are:" + userPoints, "Press enter to exit the game!");
                
                SavePointsToFile(userPoints);//saving points to files

                //close only when enter key is pressed
                while (Console.ReadKey().Key != ConsoleKey.Enter) {}
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// Add winning requirement, snake eat 3 food to win the game
        /// </summary>
        /// <param name="snakeElements"></param>
        /// <param name="negativePoints"></param>
        /// <returns></returns>
        public int WinningCheck(Queue<Position> snakeElements, int negativePoints)
        {
            if (snakeElements.Count == 7)
            {
                Console.SetCursorPosition(0, 0);
                Console.ForegroundColor = ConsoleColor.Green;//Text color for game over

                int userPoints = (snakeElements.Count - 4) * 100 - negativePoints;//points calculated for player
                userPoints = Math.Max(userPoints, 0); //if (userPoints < 0) userPoints = 0;

                PrintLinesInCenter("You Win!", "Your points are:" + userPoints, "Press enter to exit the game!");
                SavePointsToFile(userPoints);//saving points to files

                //close only when enter key is pressed
                while (Console.ReadKey().Key != ConsoleKey.Enter) { }
                return 1;
            }
            return 0;
        }

        public void GenerateFood(ref Position food, Queue<Position> snakeElements, List<Position> obstacles)
        {
            Random randomNumbersGenerator = new Random();
            do
            {
                food = new Position(randomNumbersGenerator.Next(0, Console.WindowHeight), //Food generated within console height
                    randomNumbersGenerator.Next(0, Console.WindowWidth)); //Food generate within console width
            }
            //a loop is created - while the program contains food and the obstacle is not hit 
            //put food on different position which is "@"
            while (snakeElements.Contains(food) || obstacles.Contains(food));
            Console.SetCursorPosition(food.col, food.row);
            DrawFood();
        }

        public void GenerateNewObstacle(ref Position food, Queue<Position> snakeElements, List<Position> obstacles)
        {
            Random randomNumbersGenerator = new Random();

            Position obstacle = new Position();
            do
            {
                obstacle = new Position(randomNumbersGenerator.Next(0, Console.WindowHeight),
                    randomNumbersGenerator.Next(0, Console.WindowWidth));
            }
            //if snake or obstacles are already at certain position, new obstacle will not be drawn there
            //new obstacle will not be drawn at the same row & column of food
            while (snakeElements.Contains(obstacle) ||
                obstacles.Contains(obstacle) ||
                (food.row == obstacle.row && food.col == obstacle.col));
            obstacles.Add(obstacle);
            Console.SetCursorPosition(obstacle.col, obstacle.row);
            DrawObstacle();
        }

        public void SavePointsToFile(int userPoints)
        {

            
            String filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "userPoints.txt");
            try
            {
                if (!File.Exists(filePath))
                {
                    File.Create(filePath).Dispose();
                    File.WriteAllText(filePath, userPoints.ToString() + Environment.NewLine);
                }
                else
                {
                    File.AppendAllText(filePath, userPoints.ToString() + Environment.NewLine);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("{0} Exception caught.", exception);
            }
        }

        /// <summary>
        /// Read the user points from text file
        /// </summary>
        public string ReadPointsFromFile()
        {

            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "userPoints.txt");
            string[] scoreBoard = File.ReadAllLines(filePath);
            /*foreach (string point in scoreBoard)
            {
                Console.WriteLine(point);
            }*/
            var max = scoreBoard.Select(int.Parse).Max();
            string highestPoint = max.ToString();
            return highestPoint;
        }

        //Printing game output
        private static void PrintLinesInCenter(params string[] lines)
        {
            int verticalStart = (Console.WindowHeight - lines.Length) / 2; // work out where to start printing the lines
            int verticalPosition = verticalStart;
            foreach (var line in lines)
            {
                // work out where to start printing the line text horizontally
                int horizontalStart = (Console.WindowWidth - line.Length) / 2;
                // set the start position for this line of text
                Console.SetCursorPosition(horizontalStart, verticalPosition);
                // write the text
                Console.Write(line);
                // move to the next line
                ++verticalPosition;
            }
        }

        /// <summary>
        /// To display welcome message and score at start of game
        /// </summary>
        public void DisplayStartScreen()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            PrintLinesInCenter("WELCOME TO SNAKE GAME", "\n", "Highest Score", ReadPointsFromFile());
            Thread.Sleep(5000);
            Console.Clear();
        }
        /// <summary>
        /// Funstions end here
        /// </summary>



        /// <summary>
        /// Main starts here
        /// </summary>
        //Define direction by using index number
        //Set the time taken for the food to be dissappear
        //Initialise negative points
        static void Main(string[] args)
        {
            
            byte right = 0;
            byte left = 1;
            byte down = 2;
            byte up = 3;
            
            int lastFoodTime = 0;
            int foodDissapearTime = 10000; //food dissappears after 10 second 
            int negativePoints = 0;
            Position[] directions = new Position[4];

            

            Program p = new Program();
            //Play background music
            p.DisplayStartScreen();
            p.BackgroundMusic();
            int currentTime = Environment.TickCount;

            // Define direction with characteristic of index of array
            p.Direction(directions);

            // Initialised the obstacles location at the starting of the game
            List<Position> obstacles = new List<Position>();
            p.InitialRandomObstacles(obstacles);

            

            //Do the initialization for sleepTime (Game's Speed), Snake's direction and food timing
            //Limit the number of rows of text accessible in the console window
            double sleepTime = 100;
            int direction = right;
            Random randomNumbersGenerator = new Random();
            Console.BufferHeight = Console.WindowHeight;
            lastFoodTime = Environment.TickCount;


            
            

            //Initialise the snake position in top left corner of the windows
            //Havent draw the snake elements in the windows yet. Will be drawn in the code below
            Queue<Position> snakeElements = new Queue<Position>();
            for (int i = 0; i <= 3; i++) // Length of the snake was reduced to 3 units of *
            {
                snakeElements.Enqueue(new Position(0, i));
            }

            //To position food randomly when the program runs first time
            Position food = new Position();
            p.GenerateFood(ref food,snakeElements,obstacles);
            

            //while the game is running position snake on terminal with shape "*"
            foreach (Position position in snakeElements)
            {
                Console.SetCursorPosition(position.col, position.row);
                p.DrawSnakeBody();
            }

            while (true)
            {
                //negative points is initialized as 0 at the beginning of the game. As the player reaches out for food
                //negative points increment depending how far the food is
                negativePoints++;

                //Check the user input direction
                p.CheckUserInput(ref direction, right, left, down, up);
              
                //When the game starts the snake head is towards the end of his body with face direct to start from right.
                Position snakeHead = snakeElements.Last();
                Position nextDirection = directions[direction];

                //Snake position to go within the terminal window assigned.
                Position snakeNewHead = new Position(snakeHead.row + nextDirection.row,
                    snakeHead.col + nextDirection.col);

                if (snakeNewHead.col < 0) snakeNewHead.col = Console.WindowWidth - 1;
                if (snakeNewHead.row < 0) snakeNewHead.row = Console.WindowHeight - 1;
                if (snakeNewHead.row >= Console.WindowHeight) snakeNewHead.row = 0;
                if (snakeNewHead.col >= Console.WindowWidth) snakeNewHead.col = 0;

                //Check for GameOver Criteria
                int gameOver=p.GameOverCheck(currentTime, snakeElements, snakeNewHead, negativePoints,obstacles);
                if (gameOver == 1)
                    return;

                //Check for Winning Criteria
                int winning = p.WinningCheck(snakeElements, negativePoints);
                if (winning == 1) return;

                //The way snake head will change as the player changes his direction
                Console.SetCursorPosition(snakeHead.col, snakeHead.row);
                p.DrawSnakeBody();

                //Snake head shape when the user presses the key to change his direction
                snakeElements.Enqueue(snakeNewHead);
                Console.SetCursorPosition(snakeNewHead.col, snakeNewHead.row);
                Console.ForegroundColor = ConsoleColor.Gray;
                if (direction == right) Console.Write(">"); //Snake head when going right
                if (direction == left) Console.Write("<");//Snake head when going left
                if (direction == up) Console.Write("^");//Snake head when going up
                if (direction == down) Console.Write("v");//Snake head when going down


                // food will be positioned randomly until they are not at the same row & column as snake head
                if (snakeNewHead.col == food.col && snakeNewHead.row == food.row)
                {
                    Console.Beep();// Make a sound effect when food was eaten.
                    p.GenerateFood(ref food, snakeElements, obstacles);
                    

                    //when the snake eat the food, the system tickcount will be set as lastFoodTime
                    //new food will be drawn, snake speed will increases
                    lastFoodTime = Environment.TickCount;
                    sleepTime--;

                    //Generate new obstacle
                    p.GenerateNewObstacle(ref food,snakeElements,obstacles);
                    
                }
                else
                {
                    // snake is moving
                    Position last = snakeElements.Dequeue();
                    Console.SetCursorPosition(last.col, last.row);
                    Console.Write(" ");
                }

                //if snake did not eat the food before it disappears, 50 will be added to negative points
                //draw new food after the previous one disappeared
                if (Environment.TickCount - lastFoodTime >= foodDissapearTime)
                {
                    negativePoints = negativePoints + 50;
                    Console.SetCursorPosition(food.col, food.row);
                    Console.Write(" ");

                    //Generate the new food and record the system tick count
                    p.GenerateFood(ref food, snakeElements, obstacles);
                    lastFoodTime = Environment.TickCount;
                }
             
                //snake moving speed increased 
                sleepTime -= 0.01;

                //pause the execution thread of snake moving speed
                Thread.Sleep((int)sleepTime);
            }
        }
    }
}