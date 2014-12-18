using System;

namespace A_Star_PathFinding
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
		[STAThread]
        static void Main(string[] args)
        {
            using (PathFinder game = new PathFinder())
            {
                game.Run();
            }
        }
    }
#endif
}

