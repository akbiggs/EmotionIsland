using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace EmotionIsland
{
    public static class TextureBin
    {
        private static Dictionary<string, Texture2D> texDic = new Dictionary<string, Texture2D>();
        private static Dictionary<string, SoundEffect> soundDic = new Dictionary<string, SoundEffect>();
 
        public static Texture2D Pixel { get { return TextureBin.Get("Pixel"); } }

        public static void LoadContent(ContentManager cm)
        {
            foreach (string name in Directory.GetFiles("Content/Textures/", "*.xnb"))
            {
                string filename = name.Substring(name.LastIndexOf("/")+1, name.LastIndexOf(".") - (name.LastIndexOf("/")+1));
                texDic[filename] = cm.Load<Texture2D>("Textures/" + filename);
            }

            foreach (string name in Directory.GetFiles("Content/Sounds/", "*.xnb"))
            {
                string filename = name.Substring(name.LastIndexOf("/")+1, name.LastIndexOf(".") - (name.LastIndexOf("/")+1));
                soundDic[filename] = cm.Load<SoundEffect>("Sounds/" + filename);
            }
        }

        public static Texture2D Get(string name)
        {
            return texDic[name];
        }

        public static void PlaySound(string name)
        {
            soundDic[name].Play(0.25f, 0, 0);
        }
    }
}