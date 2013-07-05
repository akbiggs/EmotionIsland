using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace EmotionIsland
{
    public static class TextureBin
    {
        private static List<string> names = new List<string> {"Pixel"};
        private static Dictionary<string, Texture2D> texDic = new Dictionary<string, Texture2D>();

        public static Texture2D Pixel { get { return TextureBin.Get("Pixel"); } }

        public static void LoadContent(ContentManager cm)
        {
            foreach (var name in names)
            {
                texDic[name] = cm.Load<Texture2D>("Textures/" + name);
            }
        }

        public static Texture2D Get(string name)
        {
            return texDic[name];
        }
    }
}