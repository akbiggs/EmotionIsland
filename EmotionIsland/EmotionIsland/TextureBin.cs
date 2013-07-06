using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace EmotionIsland
{
    public static class TextureBin
    {
        private static Dictionary<string, Texture2D> texDic = new Dictionary<string, Texture2D>();

        public static Texture2D Pixel { get { return TextureBin.Get("Pixel"); } }

        public static void LoadContent(ContentManager cm)
        {
            foreach (string name in Directory.GetFiles("Content/Textures/", "*.xnb"))
            {
                string filename = name.Substring(name.LastIndexOf("/")+1, name.LastIndexOf(".") - (name.LastIndexOf("/")+1));
                texDic[filename] = cm.Load<Texture2D>("Textures/" + filename);
            }
        }

        public static Texture2D Get(string name)
        {
            return texDic[name];
        }
    }
}