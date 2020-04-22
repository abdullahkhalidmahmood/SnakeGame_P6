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
            backgroundMusic.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "/SnakeBGM_1_Extended.wav";
            
            //Play the background music at the beginning
            backgroundMusic.Play();
        }

        public void SoundEffect()
        {
            SoundPlayer playerDie = new SoundPlayer();
            playerDie.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "/SnakeDie_1.wav";
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
        
        public int GameOverCheck(Queue<Position> snakeElements, Position snakeNewHead,int negativePoints, List<Position> obstacles)
        {
            if (snakeElements.Contains(snakeNewHead) || obstacles.Contains(snakeNewHead))
            {
                SoundEffect();
                Console.SetCursorPosition(0, 0);
                Console.ForegroundColor = ConsoleColor.Red;//Text color for game over
                Console.WriteLine("Game over!");//The text which user will view when game is over
                int userPoints = (snakeElements.Count - 4) * 100 - negativePoints;//points calculated for player
                                                                                  //if (userPoints < 0) userPoints = 0;
                userPoints = Math.Max(userPoints, 0);
                Console.WriteLine("Your points are: {0}", userPoints);//player total points shown once the game is over
                SavePointsToFile(userPoints);
                Console.ReadLine();//This line shows the output initially missing in the program thus terminal closes
                return 1;
            }
            return 0;
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
            int foodDissapearTime = 8000;
            int negativePoints = 0;
            Position[] directions = new Position[4];

            Program p = new Program();
            p.BackgroundMusic();

            // Define direction with characteristic of index of array
            p.Direction(directions);

            List<Position> obstacles = new List<Position>();
            p.InitialRandomObstacles(obstacles);


            //Do the initialization for sleepTime (Game's Speed), Snake's direction and food timing
            //Limit the number of rows of text accessible in the console window
            double sleepTime = 100;
            int direction = right;
            Random randomNumbersGenerator = new Random();
            Console.BufferHeight = Console.WindowHeight;
            lastFoodTime = Environment.TickCount;


            Console.WriteLine("Hello");
            Thread.Sleep(5000);
            //Show the obstacle in the windows with marking of "="
            foreach (Position obstacle in obstacles)
            {
                Console.SetCursorPosition(obstacle.col, obstacle.row);
                p.DrawObstacle();
            }

            //Initialise the snake position in top left corner of the windows
            //Havent draw the snake elements in the windows yet. Will be drawn in the code below
            Queue<Position> snakeElements = new Queue<Position>();
            for (int i = 0; i <= 3; i++) // Length of the snake was reduced to 3 units of *
            {
                snakeElements.Enqueue(new Position(0, i));
            }

            //To position food randomly when the program runs first time
            Position food = new Position();
            do
            {
                food = new Position(randomNumbersGenerator.Next(0, Console.WindowHeight), //Food generated within console height
                    randomNumbersGenerator.Next(0, Console.WindowWidth)); //Food generate within console width
            }
            //a loop is created - while the program contains food and the obstacle is not hit 
            //put food on different position which is "@"
            while (snakeElements.Contains(food) || obstacles.Contains(food));
            Console.SetCursorPosition(food.col, food.row);
            p.DrawFood();

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

                /////////////////
                int gameOver=p.GameOverCheck(snakeElements, snakeNewHead, negativePoints,obstacles);
                if (gameOver == 1)
                    return;
                //If snake head hits the obstacle the game is over and the player will start a new game
                
                ////////////////
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
                    do
                    {
                        food = new Position(randomNumbersGenerator.Next(0, Console.WindowHeight),
                            randomNumbersGenerator.Next(0, Console.WindowWidth));
                    }

                    //when the snake eat the food, the system tickcount will be set as lastFoodTime
                    //new food will be drawn, snake speed will increases
                    while (snakeElements.Contains(food) || obstacles.Contains(food));
                    lastFoodTime = Environment.TickCount;
                    Console.SetCursorPosition(food.col, food.row);
                    p.DrawFood();
                    sleepTime--;

                    //setting position of new obstacles randomly
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
                        (food.row != obstacle.row && food.col != obstacle.row));
                    obstacles.Add(obstacle);
                    Console.SetCursorPosition(obstacle.col, obstacle.row);
                    p.DrawObstacle();
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

                    do
                    {
                        food = new Position(randomNumbersGenerator.Next(0, Console.WindowHeight),
                            randomNumbersGenerator.Next(0, Console.WindowWidth));
                    }
                    while (snakeElements.Contains(food) || obstacles.Contains(food));
                    lastFoodTime = Environment.TickCount;
                }
                //draw food
                Console.SetCursorPosition(food.col, food.row);
                p.DrawFood();

                //snake moving speed increased 
                sleepTime -= 0.01;

                //pause the execution thread of snake moving speed
                Thread.Sleep((int)sleepTime);
            }
        }
    }
}