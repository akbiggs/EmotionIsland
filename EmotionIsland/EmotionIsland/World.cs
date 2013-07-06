using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace EmotionIsland
{
    public class World
    {

        Texture2D tempTexture;

        int width;
        int height;
        int[] tiles;

        private BufferedList<Player> players = new BufferedList<Player>();
        private BufferedList<Villager> villagers = new BufferedList<Villager>();

        Random rand;

        public World()
        {
            rand = new Random();
            this.players.Add(new Player(this, new Vector2(40, 40), PlayerNumber.One));
            GenerateWorld();
        }

        public void GenerateWorld()
        {
            width = 500;
            height = 500;
            tiles = new int[width*height];

            int borderWidth = rand.Next(3, 7);
            int waveFrequency = rand.Next(3, 40);
            int waveDepth = rand.Next(3, borderWidth);

            bool createRidge = false;
            int ridgeDepth = 0;
            int ridgeLength = 0;
            int ridgeCounter = 0;

            int rivers = rand.Next(2, 5);
            for (int i = 0; i < rivers; i++)
            {
                Vector2 origin = new Vector2(rand.Next(0, width), rand.Next(0, height));
                Vector2 destination = new Vector2(rand.Next(0, width), rand.Next(0, height));
                createRiver(origin, destination);
            }

            //Generate island layout.
            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    int ridgeOffset = 0;
                        if (createRidge)
                        {
                            ridgeOffset = Math.Abs((int)(ridgeDepth * Math.Sin((float)((float)ridgeCounter / (float)ridgeLength) * Math.PI)));

                            ridgeCounter++;
                            if (ridgeCounter >= ridgeLength)
                                createRidge = false;
                        }
                    

                    if (
                        r < (borderWidth + ridgeOffset/4 + waveDepth * Math.Sin(((float)c / (float)width * waveFrequency) * Math.PI))
                        || c < (borderWidth + ridgeOffset + waveDepth * Math.Sin(((float)r / (float)width * waveFrequency) * Math.PI))
                        || c > (width - (borderWidth + ridgeOffset + waveDepth * Math.Sin(((float)r / (float)width * waveFrequency) * Math.PI)))
                        || r > (height - (borderWidth + ridgeOffset + waveDepth * Math.Sin(((float)c / (float)width * waveFrequency) * Math.PI)))
                    ){
                        tiles[r * width + c] = 1;

                            if (!createRidge)
                            {
                                createRidge = rand.Next(0, 100) == 0;
                                if (createRidge)
                                {
                                    ridgeDepth = rand.Next(10, 40);
                                    ridgeCounter = 0;
                                    ridgeLength = 160 * rand.Next(width / 3, width / 2);
                                }
                            }
                    }

                }
            }

            //Draw ground sprites
            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    if (checkTile(c, r, 0))
                    {
                        for (int d = 1; d < 5; d++)
                        {
                            if (checkTile(c + d, r, 1) || checkTile(c - d, r, 1) ||
                            checkTile(c, r + d, 1) || checkTile(c, r - d, 1) ||
                            checkTile(c + d, r + d, 1) || checkTile(c - d, r + d, 1) ||
                            checkTile(c + d, r - d, 1) || checkTile(c - d, r - d, 1))
                                tiles[r * width + c] = 2;
                        }
                        
                            
                    }
                }
            }


            this.villagers.Add(new Villager(this, new Vector2(40, 80), EmotionType.Angry));
            this.villagers.Add(new Villager(this, new Vector2(40, 120), EmotionType.Sad));
            this.villagers.Add(new Villager(this, new Vector2(40, 160), EmotionType.Happy));
            this.villagers.Add(new Villager(this, new Vector2(40, 200), EmotionType.Terrified));
            this.villagers.Add(new Villager(this, new Vector2(40, 240), EmotionType.Neutral));
        }

        protected bool checkTile(int x, int y, int type){
            if (x < 0 || x >= width || y < 0 || y >= height)
                return false;

            return tiles[x + y*width] == type;
        }

        protected void createRiver(Vector2 origin, Vector2 destination)
        {
            float m = ((origin.Y - destination.Y)/ (origin.X - destination.X));

            if (Math.Abs(m) < 2)
            {
                m = 2;
            }
            if (Math.Abs(m) > 20)
                m = 20;

            int riverWidth = rand.Next(5, 9);
            int curve = rand.Next(50, 250);

            bool done = false;
            for (float x = 0; x < width; x += 0.05f)
            {
                if(done)
                    break;

                int curveOffset = (int)((float)curve * Math.Sin(x / 44f));

                for (int w = 0; w < riverWidth; w++)
                {
                    if (w + curveOffset + (int)origin.Y + x + width * (int)((m * x)) < width * height &&
                        w + curveOffset + (int)origin.Y + x + width * (int)((m * x)) > 0)
                    {
                        tiles[(int)origin.Y + (int)x + width * (int)((m * x)) + w + curveOffset] = 1;
                    }
                    else
                        done = true;
                }

            }
        }

        public void Add(GameObject obj)
        {
            if (obj is Player)
            {
                this.players.BufferAdd((Player)obj);
            }
            else if (obj is Villager)
            {
                this.villagers.BufferAdd((Villager)obj);
            }
        }

        public void Remove(GameObject obj)
        {
            if (obj is Player)
            {
                this.players.BufferRemove((Player)obj);
            }
            else if (obj is Villager)
            {
                this.villagers.BufferRemove((Villager)obj);
            }
        }

        public void Update()
        {
            foreach (Player player in this.players)
            {
                player.Update();
            }

            foreach (var villager in villagers)
            {
                villager.Update();
            }

            players.ApplyBuffers();
            villagers.ApplyBuffers();
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
                        spr.Draw(TextureBin.Pixel, new Vector2(c, r), null, Color.Blue, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
                    }
                    else if (tiles[r * width + c] == 0)
                    {
                        spr.Draw(TextureBin.Pixel, new Vector2(c, r), null, Color.Green, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
                    }
                    else if (tiles[r * width + c] == 2)
                    {
                        spr.Draw(TextureBin.Pixel, new Vector2(c, r), null, Color.Yellow, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
                    }
                }
            }

            foreach (var villager in villagers)
            {
                villager.Draw(spr);
            }

        }
    }
}