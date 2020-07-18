using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeSolver
{
    class MazeLoader
    {
        string username;

        public MazeLoader()
        {
            this.username = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            this.username = this.username.Replace(this.username.Substring(0, this.username.IndexOf("\\") + 1), "");
        }

        public Bitmap GetMaze(int mazeToLoad)
        {
            string mazeName = String.Format("Maze_{0}.png", mazeToLoad);
            string path = String.Format("C:\\Users\\{0}\\Documents\\MazeSolver Mazes\\{1}", username, mazeName);

            return new Bitmap(path);
        }

        public bool IsPath(Color px)
        {
            int c = px.R + px.G + px.B;
            return (c == 765);
        }

        public bool IsWall(Color px)
        {
            int c = px.R + px.G + px.B;
            return (c == 0);
        }

        public bool IsStart(Color px)
        {
            return (px.R == 255 && (px.G + px.B) == 0);
        }

        public bool IsGoal(Color px)
        {
            return (px.R == 255 && px.G == 216 && px.B == 0);
        }

        public string GetUsername()
        {
            return username;
        }
    }
}
