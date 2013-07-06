using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace EmotionIsland
{
    public class World
    {
        BufferedList<Player> players = new BufferedList<Player>();

        Texture2D tempTexture;

        int width;
        int height;
        int[] tiles;

        public World()
        {
            this.players.Add(new Player(this, new Vector2(40, 40), PlayerNumber.One));
            GenerateWorld();
        }

        public void GenerateWorld()
        {
            width = 400;
            height = 400;
            tiles = new int[width*height];
            Random rand = new Random();

            int borderWidth = rand.Next(3, 7);
            int waveFrequency = rand.Next(3, 40);
            int waveDepth = rand.Next(3, borderWidth);

            bool createRidge = false;
            int ridgeDepth = 0;
            int ridgeLength = 0;
            int ridgeCounter = 0;


            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    int ridgeOffset = 0;
                    if (createRidge)
                    {
                        ridgeOffset = Math.Abs((int)(ridgeDepth * Math.Sin((float)((float)ridgeCounter / (float)ridgeLength) * Math.PI)));

                        ridgeCounter++;
                        if (ridgeCounter == ridgeLength)
                            createRidge = false;
                    }

                    if (
                        r < (borderWidth + waveDepth * Math.Sin(((float)c / (float)width * waveFrequency) * Math.PI))
                        || c < (borderWidth + ridgeOffset + waveDepth * Math.Sin(((float)r / (float)width * waveFrequency) * Math.PI))
                        || c > (width - (borderWidth + waveDepth * Math.Sin(((float)r / (float)width * waveFrequency) * Math.PI)))
                        || r > (height - (borderWidth + waveDepth * Math.Sin(((float)c / (float)width * waveFrequency) * Math.PI)))
                    ){
                        tiles[r * width + c] = 1;

                        if (!createRidge)
                        {
                            createRidge = rand.Next(0, 100) == 0;
                            if (createRidge)
                            {
                                ridgeDepth = rand.Next(10, 40);
                                ridgeCounter = 0;
                                ridgeLength = rand.Next(width / 3, width / 2);
                            }
                        }
                    }

                    

                }
            }
        }

        public void Add(GameObject obj)
        {
            if (obj is Player)
            {
                this.players.BufferAdd((Player)obj); 
            }
        }

        public void Remove(GameObject obj)
        {
            if (obj is Player)
            {
                this.players.BufferRemove((Player)obj); 
            }
        }

        public void Update()
        {
            foreach (Player player in this.players)
            {
                player.Update();
            }
        }

        public void Draw(SpriteBatch spr)
        {
            foreach (Player player in players)
            {
                player.Draw(spr);
            }
            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    if (tiles[r * width + c] == 1)
                    {
                        spr.Draw(TextureBin.Pixel, new Vector2(c, r), null, Color.White, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
                    }
                }
            }
        }
    }
}