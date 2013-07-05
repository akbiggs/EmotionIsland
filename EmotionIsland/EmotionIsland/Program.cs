using System;

namespace EmotionIsland
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (EmotionIsland game = new EmotionIsland())
            {
                game.Run();
            }
        }
    }
#endif
}

