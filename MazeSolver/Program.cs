using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeSolver
{
    class Program
    {
        // Maze Solver v1 (Written Aug. 23rd, 2017)
        // Uses Brute Force methods

        // TODO : WRITE MAZE CREATOR / EDITER

        MazeLoader loader = new MazeLoader();
        Bitmap img, solution;

        List<string> visited = new List<string>();
        List<string> intersections = new List<string>();
        List<string> visitedIntersections = new List<string>();
        int[] startPos = new int[2];
        int[] goalPos = new int[2];
        int[] curPos = new int[2];
        int maxWidth = 0, maxHeight = 0;
        int failCount = 0;
        bool ReachedGoal = false, solveAll = false, debug = true;
        string lastIntersection;
        int mazeToLoad = 0;

        static void Main(string[] args)
            => new Program().Start();

        public void Start()
        {
            Console.Title = "Maze Solver";
            Console.WindowWidth = 52;

            int wallCount = 0, pathCount = 0;

            if (!solveAll)
            {
                string dirPath = String.Format("C:\\Users\\{0}\\Documents\\MazeSolver Mazes\\", loader.GetUsername());
                int mazeCount = 0;
                int fileCount = Directory.GetFiles(dirPath).Length;

                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);

                if (fileCount == 0)
                {
                    ColourSay("No Mazes to load.", ConsoleColor.Red);
                    Pause();
                }

                Console.WriteLine("====== Select a Maze ======");

                foreach (string name in Directory.GetFiles(dirPath))
                {
                    if (name.ToLower().Contains("maze_") && name.ToLower().EndsWith(".png"))
                        mazeCount += 1;
                }

                for (int i = 0; i < mazeCount; i += 1)
                    Console.WriteLine(String.Format("[{0}] MAZE #{0}", i));

                Console.Write("\nSolve which maze? (type \"all\" for all): ");

                string mazeToSolve = Console.ReadLine().ToLower();

                for (int i = 0; i < mazeCount; i += 1)
                {
                    if (mazeToSolve.Equals(i.ToString()))
                    {
                        solveAll = false;
                        mazeToLoad = i;
                    }
                    else if (mazeToSolve.Equals("all"))
                        solveAll = true;
                }
            }
            
            img = loader.GetMaze(mazeToLoad);
            maxWidth = img.Width;
            maxHeight = img.Height;

            for (int w = 0; w < maxWidth; w += 1)
            {
                for (int h = 0; h < maxHeight; h += 1)
                {
                    Color px = img.GetPixel(w, h);

                    if (loader.IsPath(px))
                    {
                        Color above = img.GetPixel(w, h - 1);
                        Color left = img.GetPixel(w - 1, h);
                        Color bottom = img.GetPixel(w, h + 1);
                        Color right = img.GetPixel(w + 1, h);
                        
                        if (loader.IsPath(above) && loader.IsPath(bottom) && (loader.IsPath(left) || loader.IsPath(right)))
                            intersections.Add(String.Format("{0},{1}", w, h));

                        pathCount += 1;
                    }
                    else
                    {
                        if (loader.IsStart(px))
                        {
                            startPos[0] = w;
                            startPos[1] = h;
                        }
                        else if (loader.IsGoal(px))
                        {
                            goalPos[0] = w;
                            goalPos[1] = h;
                        }
                        else wallCount += 1;
                    }
                }
            }

            curPos[0] = startPos[0];
            curPos[1] = startPos[1];

            bool validMaze = (startPos.Length > 0 && goalPos.Length > 0);

            if (debug)
            {
                Console.WriteLine(String.Format("======= MAZE #{0} =======", mazeToLoad));
                Console.WriteLine("Has Start: " + (startPos.Length > 0));
                Console.WriteLine("Has Goal: " + (goalPos.Length > 0));
                Console.WriteLine("Total Walls: " + wallCount);
                Console.WriteLine("Total Paths: " + pathCount);
                Console.WriteLine("Total Intersections: " + intersections.Count);
                Console.WriteLine("Valid Maze: " + validMaze);

                if (intersections.Count > 0)
                    Console.WriteLine();

                for (int i = 0; i < intersections.Count; i += 1)
                    Console.WriteLine(String.Format("Found Intersection @ ({0})", intersections[i]));

                Console.WriteLine();
            }

            if (!validMaze)
            {
                ColourSay("[ERROR] INVALID MAZE", ConsoleColor.Red);
                Pause();
            }

            while (!ReachedGoal && validMaze)
            {
                Move("up");
                Move("left");
                Move("down");
                Move("right");
            }

            Pause();
        }

        private void DebugKeys()
        {
            string key = Console.ReadKey().Key.ToString().ToLower();

            key = key.Replace("arrow", "");

            if (key.Contains("escape"))
                Environment.Exit(0);
            
            Move(key);
            Console.WriteLine("Already Visited: " + AlreadyVisited(curPos[0], curPos[1]));

            for (int i = 0; i < visited.Count; i += 1)
                Console.WriteLine("Visited: " + visited[i]);

            DebugKeys();
        }

        public bool AlreadyVisited(int x, int y)
        {
            string check = String.Format("{0},{1}", x, y);
            return (visited.Contains(check));
        }

        public bool VerifyMove(string direction)
        {
            switch (direction.ToLower())
            {
                case "left":
                    if ((curPos[0] - 1) >= 0)
                        return (!AlreadyVisited(curPos[0] - 1, curPos[1]) && loader.IsPath(img.GetPixel(curPos[0] - 1, curPos[1]))
                            || loader.IsStart(img.GetPixel(curPos[0] - 1, curPos[1])) || loader.IsGoal(img.GetPixel(curPos[0] - 1, curPos[1])));
                    else return false;
                case "right":
                    if ((curPos[0] + 1) <= maxWidth)
                        return (!AlreadyVisited(curPos[0] + 1, curPos[1]) && loader.IsPath(img.GetPixel(curPos[0] + 1, curPos[1]))
                            || loader.IsStart(img.GetPixel(curPos[0] + 1, curPos[1])) || loader.IsGoal(img.GetPixel(curPos[0] + 1, curPos[1])));
                    else return false;
                case "up":
                    if ((curPos[1] - 1) >= 0)
                        return (!AlreadyVisited(curPos[0], curPos[1] - 1) && loader.IsPath(img.GetPixel(curPos[0], curPos[1] - 1))
                            || loader.IsStart(img.GetPixel(curPos[0], curPos[1] - 1)) || loader.IsGoal(img.GetPixel(curPos[0], curPos[1] - 1)));
                    else return false;
                case "down":
                    if ((curPos[1] + 1) <= maxHeight)
                        return (!AlreadyVisited(curPos[0], curPos[1] + 1) && loader.IsPath(img.GetPixel(curPos[0], curPos[1] + 1))
                            || loader.IsStart(img.GetPixel(curPos[0], curPos[1] + 1)) || loader.IsGoal(img.GetPixel(curPos[0], curPos[1] + 1)));
                    else return false;
                default:
                    return false;
            }
        }

        public void Move(string direction)
        {
            if (ReachedGoal) return;

            string visitDir = String.Format("{0},{1}", curPos[0], curPos[1]);

            if (!visited.Contains(visitDir))
                visited.Add(visitDir);

            if (failCount >= 4)
            {
                if (debug)
                    ColourSay(String.Format("STUCK! (LAST INTERSECTION: {0})", lastIntersection), ConsoleColor.Magenta);

                if (lastIntersection == null || lastIntersection.Length < 1)
                {
                    ColourSay("Cannot be solved.", ConsoleColor.Red);

                    string dirPath = String.Format("C:\\Users\\{0}\\Documents\\MazeSolver Mazes\\", loader.GetUsername());
                    int mazeCount = 0;
                    int fileCount = Directory.GetFiles(dirPath).Length;

                    foreach (string name in Directory.GetFiles(dirPath))
                    {
                        if (name.ToLower().Contains("maze_") && name.ToLower().EndsWith(".png"))
                            mazeCount += 1;
                    }

                    mazeCount -= 1;

                    if (solveAll && mazeToLoad != mazeCount)
                    {
                        mazeToLoad += 1;
                        ReachedGoal = false;
                        visited.Clear();
                        intersections.Clear();
                        visitedIntersections.Clear();
                        lastIntersection = "";

                        Start();
                    }
                    else Pause();

                }

                curPos[0] = int.Parse(lastIntersection.Substring(0, lastIntersection.IndexOf(",")));
                curPos[1] = int.Parse(lastIntersection.Substring(lastIntersection.IndexOf(",") + 1));

                failCount = 0;

                // even after moving to last Intersection, it cannot solve it, move to the next intersection
                if (!VerifyMove("up") && !VerifyMove("left") && !VerifyMove("down") && !VerifyMove("right") && visitedIntersections.Contains(String.Format("{0},{1}", curPos[0], curPos[1])))
                {
                    if (debug)
                        ColourSay("All Directions Failed!", ConsoleColor.Magenta);

                    string check;

                    for (int i = 0; i < visitedIntersections.Count; i += 1)
                    {
                        check = String.Format("{0},{1}", curPos[0], curPos[1]);
                        
                        visitedIntersections.Remove(check);

                        if(debug)
                            Console.WriteLine(String.Format("Visited Intersections: ({0})", visitedIntersections[i]));
                    }

                    curPos[0] = int.Parse(visitedIntersections[0].Substring(0, visitedIntersections[0].IndexOf(",")));
                    curPos[1] = int.Parse(visitedIntersections[0].Substring(visitedIntersections[0].IndexOf(",") + 1));

                    lastIntersection = String.Format("{0},{1}", curPos[0], curPos[1]);
                }
            }

            if (VerifyMove(direction))
            {
                failCount = 0;

                switch (direction)
                {
                    case "left":
                        if (AlreadyVisited(curPos[0] - 1, curPos[1]))
                            break;
                        else
                            curPos[0] -= 1;
                        break;
                    case "right":
                        if (AlreadyVisited(curPos[0] + 1, curPos[1]))
                            break;
                        else
                            curPos[0] += 1;
                        break;
                    case "up":
                        if (AlreadyVisited(curPos[0], curPos[1] - 1))
                            break;
                        else
                            curPos[1] -= 1;
                        break;
                    case "down":
                        if (AlreadyVisited(curPos[0], curPos[1] + 1))
                            break;
                        else
                            curPos[1] += 1;
                        break;
                }

                if (curPos[0] == goalPos[0] && curPos[1] == goalPos[1])
                {
                    if (debug)
                        ColourSay("GOAL WAS REACHED!", ConsoleColor.Yellow);

                    ReachedGoal = true;

                    visited.Add(String.Format("{0},{1}", goalPos[0], goalPos[1]));

                    string listVisited = "";
                    string dirPath = String.Format("C:\\Users\\{0}\\Documents\\MazeSolver Mazes\\", loader.GetUsername());
                    string path = String.Format("{0}Solutions\\Maze_{1}.png", dirPath, mazeToLoad);
                    Color[] pixels = new Color[visited.Count];
                    int x, y;

                    for (int i = 0; i < visited.Count; i += 1)
                    {
                        listVisited += String.Format("\n({0}), ", visited[i]);

                        x = int.Parse(visited[i].Substring(0, visited[i].IndexOf(",")));
                        y = int.Parse(visited[i].Substring(visited[i].IndexOf(",") + 1));

                        pixels[i] = img.GetPixel(x, y);
                    }

                    listVisited = listVisited.Substring(0, listVisited.Length - 2);
                    solution = new Bitmap(img.Width, img.Height);

                    // draw maze
                    for (int i = 0; i < (img.Width * img.Height); i += 1)
                    {
                        for (int w = 0; w < img.Width; w += 1)
                        {
                            for (int h = 0; h < img.Height; h += 1)
                            {
                                solution.SetPixel(w, h, img.GetPixel(w, h));
                            }
                        }
                    }

                    visited.Remove(visited[0]);
                    visited.Remove(visited[visited.Count - 1]);

                    // draw path the ai took
                    for (int i = 0; i < visited.Count; i += 1)
                    {
                        x = int.Parse(visited[i].Substring(0, visited[i].IndexOf(",")));
                        y = int.Parse(visited[i].Substring(visited[i].IndexOf(",") + 1));

                        solution.SetPixel(x, y, Color.DeepSkyBlue);
                    }

                    if (!Directory.Exists(dirPath + "\\Solutions"))
                        Directory.CreateDirectory(dirPath + "\\Solutions");

                    solution.Save(path);

                    if (debug)
                        Console.WriteLine("Visited: " + listVisited);

                    int mazeCount = 0;
                    int fileCount = Directory.GetFiles(dirPath).Length;

                    foreach (string name in Directory.GetFiles(dirPath))
                    {
                        if (name.ToLower().Contains("maze_") && name.ToLower().EndsWith(".png"))
                            mazeCount += 1;
                    }

                    mazeCount -= 1;

                    if (!solveAll) mazeCount = 1;

                    if (mazeToLoad != mazeCount && solveAll)
                    {
                        mazeToLoad += 1;
                        ReachedGoal = false;
                        visited.Clear();
                        intersections.Clear();
                        visitedIntersections.Clear();
                        lastIntersection = "";

                        Start();
                    }
                    else
                    {
                        ColourSay(String.Format("\nFINISHED SOLVING {0} MAZE{1}", mazeCount, (mazeCount > 1) ? "S" : ""), ConsoleColor.Green);
                        ColourSay("Solutions can be found in the Solutions folder.", ConsoleColor.Green);
                    }
                }

                string check;
                int interCnt = 0;

                for (int i = 0; i < intersections.Count; i += 1)
                {
                    check = String.Format("{0},{1}", curPos[0], curPos[1]);

                    if (intersections.Contains(check))
                    {
                        if (interCnt != 1)
                        {
                            if (debug)
                                ColourSay(String.Format("@ AN INTERSECTION ({0})", check), ConsoleColor.Cyan);

                            interCnt = 1;
                        }

                        lastIntersection = check;

                        if (!visitedIntersections.Contains(check))
                            visitedIntersections.Add(check);
                    }
                }

                if (!ReachedGoal && debug)
                    ColourSay("SUCCESS", ConsoleColor.Green);
            }
            else
            {
                if (debug)
                    ColourSay("FAILED", ConsoleColor.Red);

                failCount += 1;
            }

            // Debug reasons only
            if (!(curPos[0] == goalPos[0] && curPos[1] == goalPos[1]) && debug)
                Console.WriteLine(String.Format("({0}) Current Position: ({1},{2})", direction, curPos[0], curPos[1]));
        }

        public void ColourSay(string msg, ConsoleColor col)
        {
            ConsoleColor org = Console.ForegroundColor;

            Console.ForegroundColor = col;
            Console.WriteLine(msg);
            Console.ForegroundColor = org;
        }

        private void Pause()
        {
            ColourSay("Press any key to exit...", ConsoleColor.White);

            Console.ReadKey();
            Environment.Exit(0);
        }
    }
}
