
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace EmotionIsland
{
    public class World
    {

        public enum BaseTiles
        {
            Grass = 0,
            Water = 1,
            Sand = 2,
            Mountain = 3
        }

        int xPos;
        int yPos;

        int width;
        int height;
        int[] tiles;
        int[] tempTiles;
        bool drawTemp;

        long animationTimer;
        int animationCounter;

        private BufferedList<Player> players = new BufferedList<Player>();
        private BufferedList<Villager> villagers = new BufferedList<Villager>();

        private BufferedList<EmotionBeam> emotionBeams = new BufferedList<EmotionBeam>();
        private BufferedList<Bullet> bullets = new BufferedList<Bullet>();

        private Random rand;
        public World()
        {
            rand = new Random();
            this.players.Add(new Player(this, new Vector2(40, 40), PlayerNumber.One));

            drawTemp = false;
            GenerateWorld();

            this.villagers.Add(new Villager(this, new Vector2(200, 80), EmotionType.Angry));
            this.villagers.Add(new Villager(this, new Vector2(200, 120), EmotionType.Sad));
            this.villagers.Add(new Villager(this, new Vector2(200, 160), EmotionType.Happy));
            this.villagers.Add(new Villager(this, new Vector2(200, 200), EmotionType.Terrified));
            this.villagers.Add(new Villager(this, new Vector2(200, 240), EmotionType.Neutral));
            this.villagers.ForEach((villager) => villager.EmotionalTarget = players[0]);
        }

        /// <summary>
        /// Generate a random world.
        /// </summary>
        public void GenerateWorld()
        {
            //Create tile array
            width = 500;
            height = 500;
            tempTiles = new int[width*height];
            tiles = new int[width * height];

            //Create rivers
            int rivers = rand.Next(2, 5);
            for (int i = 0; i < rivers; i++)
            {
                Vector2 origin = new Vector2(rand.Next(0, width), rand.Next(0, height));
                Vector2 destination = new Vector2(rand.Next(0, width), rand.Next(0, height));
                createRiver(origin, destination);
            }

            int borderWidth = rand.Next(3, 7);
            int waveFrequency = rand.Next(3, 40);
            int waveDepth = rand.Next(3, borderWidth);

            //Create island by sorrounding map with water.
            bool createRidge = false;
            int ridgeDepth = 0;
            int ridgeLength = 0;
            int ridgeCounter = 0;

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
                        r < (borderWidth + ridgeOffset / 4 + waveDepth * Math.Sin(((float)c / (float)width * waveFrequency) * Math.PI))
                        || c < (borderWidth + ridgeOffset + waveDepth * Math.Sin(((float)r / (float)width * waveFrequency) * Math.PI))
                        || c > (width - (borderWidth + ridgeOffset + waveDepth * Math.Sin(((float)r / (float)width * waveFrequency) * Math.PI)))
                        || r > (height - (borderWidth + ridgeOffset + waveDepth * Math.Sin(((float)c / (float)width * waveFrequency) * Math.PI)))
                    )
                    {
                        tempTiles[r * width + c] = (int)BaseTiles.Water;

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

            //Create lakes
            int lakes = rand.Next(2, 5);
            for (int l = 0; l < lakes; l++)
            {
                Vector2 origin = new Vector2(0, 0);
                while (tempTiles[(int)origin.X + (int)origin.Y * width] != (int)BaseTiles.Grass)
                {
                    origin.X = rand.Next(0, width);
                    origin.Y = rand.Next(0, height);
                }
                recursiveGrowth((int)origin.X, (int)origin.Y, (int)BaseTiles.Water, (int)BaseTiles.Grass, rand.Next(20, 100));
            }

            //Create beaches
            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    //Create beach
                    if (checkTile(c, r, (int)BaseTiles.Grass))
                    {
                        for (int d = 1; d < 5; d++)
                        {
                            if (checkTile(c + d, r, (int)BaseTiles.Water) || checkTile(c - d, r, (int)BaseTiles.Water) ||
                            checkTile(c, r + d, (int)BaseTiles.Water) || checkTile(c, r - d, (int)BaseTiles.Water) ||
                            checkTile(c + d, r + d, (int)BaseTiles.Water) || checkTile(c - d, r + d, (int)BaseTiles.Water) ||
                            checkTile(c + d, r - d, (int)BaseTiles.Water) || checkTile(c - d, r - d, (int)BaseTiles.Water))
                                tempTiles[r * width + c] = (int)BaseTiles.Sand;
                        }
                    }
                }
            }

            //Create mountains
            int mountains = rand.Next(2, 5);
            for (int m = 0; m < mountains; m++)
            {
                Vector2 origin = new Vector2(0, 0);
                while (tempTiles[(int)origin.X + (int)origin.Y * width] != (int)BaseTiles.Grass)
                {
                    origin.X = rand.Next(0, width);
                    origin.Y = rand.Next(0, height);
                }
                recursiveGrowth((int)origin.X, (int)origin.Y, (int)BaseTiles.Mountain, (int)BaseTiles.Grass, rand.Next(20, 500));
            }

            //Normalize tiles
            bool rerun = true;
            int normalCounter = 0;
            while (rerun)
            {
                normalCounter++;
                if (normalCounter == 20)
                    break;
                rerun = false;
                for (int r = 0; r < height; r++)
                {
                    for (int c = 0; c < width; c++)
                    {
                        //Normalize beach edges vertically
                        if (checkTile(c, r, (int)BaseTiles.Sand) && checkTile(c, r - 1, (int)BaseTiles.Water) &&
                            checkTile(c, r + 1, (int)BaseTiles.Water))
                        {
                            tempTiles[r * width + c] = (int)BaseTiles.Water;
                            rerun = true;
                        }

                        //Normalize beach edges horizontally
                        if (checkTile(c, r, (int)BaseTiles.Sand) && checkTile(c - 1, r, (int)BaseTiles.Water) &&
                            checkTile(c + 1, r, (int)BaseTiles.Water))
                        {
                            tempTiles[r * width + c] = (int)BaseTiles.Water;
                            rerun = true;
                        }

                        //Get rid of vertical water ridges
                        if (checkTile(c, r, (int)BaseTiles.Water) && checkTile(c, r + 1, (int)BaseTiles.Sand) &&
                            checkTile(c, r - 1, (int)BaseTiles.Sand))
                        {
                            tempTiles[r * width + c] = (int)BaseTiles.Sand;
                            rerun = true;
                        }

                        //Get rid of horizontal water ridges
                        if (checkTile(c, r, (int)BaseTiles.Water) && checkTile(c + 1, r, (int)BaseTiles.Sand) &&
                            checkTile(c - 1, r, (int)BaseTiles.Sand))
                        {
                            tempTiles[r * width + c] = (int)BaseTiles.Sand;
                            rerun = true;
                        }

                        //Normalize grass edges vertically
                        if (checkTile(c, r, (int)BaseTiles.Grass) && checkTile(c, r - 1, (int)BaseTiles.Sand) &&
                            checkTile(c, r + 1, (int)BaseTiles.Sand))
                        {
                            tempTiles[r * width + c] = (int)BaseTiles.Sand;
                            rerun = true;
                        }

                        //Normalize grass edges horizontally
                        if (checkTile(c, r, (int)BaseTiles.Grass) && checkTile(c + 1, r, (int)BaseTiles.Sand) &&
                            checkTile(c - 1, r, (int)BaseTiles.Sand))
                        {
                            tempTiles[r * width + c] = (int)BaseTiles.Sand;
                            rerun = true;
                        }

                        //Get rid of vertical sand ridges
                        if (checkTile(c, r, (int)BaseTiles.Sand) && checkTile(c, r + 1, (int)BaseTiles.Grass) &&
                            checkTile(c, r - 1, (int)BaseTiles.Grass))
                        {
                            tempTiles[r * width + c] = (int)BaseTiles.Grass;
                            rerun = true;
                        }

                        //Get rid of horizontal sand ridges
                        if (checkTile(c, r, (int)BaseTiles.Sand) && checkTile(c + 1, r, (int)BaseTiles.Grass) &&
                            checkTile(c - 1, r, (int)BaseTiles.Grass))
                        {
                            tempTiles[r * width + c] = (int)BaseTiles.Grass;
                            rerun = true;
                        }

                        //Normalize mountain edges vertically
                        if (checkTile(c, r, (int)BaseTiles.Mountain) && checkTile(c, r - 1, (int)BaseTiles.Grass) &&
                            checkTile(c, r + 1, (int)BaseTiles.Grass))
                        {
                            tempTiles[r * width + c] = (int)BaseTiles.Grass;
                            rerun = true;
                        }

                        //Normalize mountain edges horizontally
                        if (checkTile(c, r, (int)BaseTiles.Mountain) && checkTile(c + 1, r, (int)BaseTiles.Grass) &&
                            checkTile(c - 1, r, (int)BaseTiles.Grass))
                        {
                            tempTiles[r * width + c] = (int)BaseTiles.Grass;
                            rerun = true;
                        }

                        //Get rid of vertical grass ridges
                        if (checkTile(c, r, (int)BaseTiles.Grass) && checkTile(c, r + 1, (int)BaseTiles.Mountain) &&
                            checkTile(c, r - 1, (int)BaseTiles.Mountain))
                        {
                            tempTiles[r * width + c] = (int)BaseTiles.Mountain;
                            rerun = true;
                        }

                        //Get rid of horizontal grass ridges
                        if (checkTile(c, r, (int)BaseTiles.Grass) && checkTile(c + 1, r, (int)BaseTiles.Mountain) &&
                            checkTile(c - 1, r, (int)BaseTiles.Mountain))
                        {
                            tempTiles[r * width + c] = (int)BaseTiles.Mountain;
                            rerun = true;
                        }

                    }
                }
            }


            //Apply tilesets
            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    int tile = c + r*width;

                    if (checkTile(c, r, (int)BaseTiles.Sand))
                    {
                        if (checkTile(c - 1, r, (int)BaseTiles.Grass) && checkTile(c, r + 1, (int)BaseTiles.Grass))
                            tiles[tile] = 134; //TOP BOTTOM LEFT
                        else if (checkTile(c + 1, r, (int)BaseTiles.Grass) && checkTile(c, r + 1, (int)BaseTiles.Grass))
                            tiles[tile] = 124; //TOP BOTTOM RIGHT
                        else if (checkTile(c + 1, r + 1, (int)BaseTiles.Grass) && checkTile(c + 1, r, (int)BaseTiles.Sand)
                            && checkTile(c, r + 1, (int)BaseTiles.Sand))
                            tiles[tile] = 6; //TOP LEFT
                        else if (checkTile(c - 1, r - 1, (int)BaseTiles.Grass) && checkTile(c, r - 1, (int)BaseTiles.Sand)
                            && checkTile(c - 1, r, (int)BaseTiles.Sand))
                            tiles[tile] = 28; //BOTTOM RIGHT
                        else if (checkTile(c - 1, r + 1, (int)BaseTiles.Grass) && checkTile(c, r + 1, (int)BaseTiles.Sand)
                        && checkTile(c - 1, r, (int)BaseTiles.Sand))
                            tiles[tile] = 8; //TOP RIGHT
                        else if (checkTile(c, r - 1, (int)BaseTiles.Sand) && checkTile(c + 1, r, (int)BaseTiles.Sand)
                            && checkTile(c + 1, r - 1, (int)BaseTiles.Grass))
                            tiles[tile] = 26; //BOTTOM LEFT
                        else if (checkTile(c + 1, r, (int)BaseTiles.Grass) &&
                            checkTile(c, r - 1, (int)BaseTiles.Grass))
                            tiles[tile] = 144; //BOTTOM with Grass on right
                        else if (checkTile(c - 1, r, (int)BaseTiles.Grass) &&
                            checkTile(c, r - 1, (int)BaseTiles.Grass))
                            tiles[tile] = 154; //BOTTOM with Grass on left
                        else if (checkTile(c, r + 1, (int)BaseTiles.Grass))
                            tiles[tile] = 7; //TOP
                        else if (checkTile(c, r - 1, (int)BaseTiles.Grass))
                            tiles[tile] = 10 + rand.Next(0, 3); //BOTTOM
                        else if (checkTile(c + 1, r, (int)BaseTiles.Grass))
                            tiles[tile] = 16; //LEFT SIDE
                        else if (checkTile(c - 1, r, (int)BaseTiles.Grass))
                            tiles[tile] = 18; //RIGHT SIDE
                        else
                            tiles[tile] = 3 + rand.Next(0, 3);//SAND
                        
                    }
                    else if (checkTile(c, r, (int)BaseTiles.Water))
                    {
                        if (checkTile(c - 1, r, (int)BaseTiles.Sand) && checkTile(c, r + 1, (int)BaseTiles.Sand))
                            tiles[tile] = 130; //TOP BOTTOM LEFT
                        else if (checkTile(c + 1, r, (int)BaseTiles.Sand) && checkTile(c, r + 1, (int)BaseTiles.Sand))
                            tiles[tile] = 120; //TOP BOTTOM RIGHT
                        else if (checkTile(c + 1, r + 1, (int)BaseTiles.Sand) && checkTile(c + 1, r, (int)BaseTiles.Water)
                            && checkTile(c, r + 1, (int)BaseTiles.Water))
                            tiles[tile] = 20; //TOP LEFT
                        else if (checkTile(c - 1, r - 1, (int)BaseTiles.Sand) && checkTile(c, r - 1, (int)BaseTiles.Water)
                            && checkTile(c - 1, r, (int)BaseTiles.Water))
                            tiles[tile] = 70; //BOTTOM RIGHT
                        else if (checkTile(c - 1, r + 1, (int)BaseTiles.Sand) && checkTile(c, r + 1, (int)BaseTiles.Water)
                        && checkTile(c - 1, r, (int)BaseTiles.Water))
                            tiles[tile] = 50; //TOP RIGHT
                        else if(checkTile(c, r-1, (int)BaseTiles.Water) && checkTile(c+1, r, (int)BaseTiles.Water)
                            && checkTile(c + 1, r-1, (int)BaseTiles.Sand))
                            tiles[tile] = 40; //BOTTOM LEFT
                        else if (checkTile(c + 1, r, (int)BaseTiles.Sand) &&
                            checkTile(c, r - 1, (int)BaseTiles.Sand))
                            tiles[tile] = 140; //BOTTOM with sand on right
                        else if (checkTile(c - 1, r, (int)BaseTiles.Sand) &&
                            checkTile(c, r - 1, (int)BaseTiles.Sand))
                            tiles[tile] = 150; //BOTTOM with sand on left
                        else if (checkTile(c, r + 1, (int)BaseTiles.Sand))
                            tiles[tile] = 80; //TOP
                        else if (checkTile(c, r - 1, (int)BaseTiles.Sand))
                            tiles[tile] = 90 + 10*rand.Next(0, 3); //BOTTOM
                        else if (checkTile(c + 1, r, (int)BaseTiles.Sand))
                            tiles[tile] = 30; //LEFT SIDE
                        else if (checkTile(c - 1, r, (int)BaseTiles.Sand))
                            tiles[tile] = 60; //RIGHT SIDE
                        else
                            tiles[tile] = 13; //WATER
                    }
                    else if (checkTile(c, r, (int)BaseTiles.Grass))
                    {
                        if (checkTile(c - 1, r, (int)BaseTiles.Mountain) && checkTile(c, r + 1, (int)BaseTiles.Mountain))
                            tiles[tile] = 135; //TOP BOTTOM LEFT
                        else if (checkTile(c + 1, r, (int)BaseTiles.Mountain) && checkTile(c, r + 1, (int)BaseTiles.Mountain))
                            tiles[tile] = 125; //TOP BOTTOM RIGHT
                        else if (checkTile(c + 1, r + 1, (int)BaseTiles.Mountain) && checkTile(c + 1, r, (int)BaseTiles.Grass)
                            && checkTile(c, r + 1, (int)BaseTiles.Grass))
                            tiles[tile] = 36; //TOP LEFT
                        else if (checkTile(c - 1, r - 1, (int)BaseTiles.Mountain) && checkTile(c, r - 1, (int)BaseTiles.Grass)
                            && checkTile(c - 1, r, (int)BaseTiles.Grass))
                            tiles[tile] = 58; //BOTTOM RIGHT
                        else if (checkTile(c - 1, r + 1, (int)BaseTiles.Mountain) && checkTile(c, r + 1, (int)BaseTiles.Grass)
                        && checkTile(c - 1, r, (int)BaseTiles.Grass))
                            tiles[tile] = 38; //TOP RIGHT
                        else if (checkTile(c, r - 1, (int)BaseTiles.Grass) && checkTile(c + 1, r, (int)BaseTiles.Grass)
                            && checkTile(c + 1, r - 1, (int)BaseTiles.Mountain))
                            tiles[tile] = 56; //BOTTOM LEFT
                        else if (checkTile(c + 1, r, (int)BaseTiles.Mountain) &&
                            checkTile(c, r - 1, (int)BaseTiles.Mountain))
                            tiles[tile] = 145; //BOTTOM with Mountain on right
                        else if (checkTile(c - 1, r, (int)BaseTiles.Mountain) &&
                            checkTile(c, r - 1, (int)BaseTiles.Mountain))
                            tiles[tile] = 155; //BOTTOM with Mountain on left
                        else if (checkTile(c, r + 1, (int)BaseTiles.Mountain))
                            tiles[tile] = 37; //TOP
                        else if (checkTile(c, r - 1, (int)BaseTiles.Mountain))
                            tiles[tile] = 57; //BOTTOM
                        else if (checkTile(c + 1, r, (int)BaseTiles.Mountain))
                            tiles[tile] = 46; //LEFT SIDE
                        else if (checkTile(c - 1, r, (int)BaseTiles.Mountain))
                            tiles[tile] = 48; //RIGHT SIDE
                        else
                            tiles[tile] = rand.Next(0, 3);//Grass
                    }
                    else if (checkTile(c, r, (int)BaseTiles.Mountain))
                    {
                        tiles[tile] = 47;
                    }
                }
            }

        }

        protected void recursiveGrowth(int x, int y, int type, int replaceType, int hops)
        {
            if (hops <= 0)
                return;

            if (x < 0 || x >= width || y < 0 || y >= height)
                return;

            int tile = x + y * width;

            if (!checkTile(x, y, replaceType) || (!checkTile(x + 1, y, replaceType) && !checkTile(x + 1, y, type)) ||
                (!checkTile(x - 1, y, replaceType) && !checkTile(x - 1, y, type)) ||
                (!checkTile(x, y + 1, replaceType) && !checkTile(x, y + 1, type)) ||
                (!checkTile(x, y - 1, replaceType) && !checkTile(x, y - 1, type)) ||
                (!checkTile(x - 1, y - 1, replaceType) && !checkTile(x - 1, y - 1, type)) ||
                (!checkTile(x + 1, y - 1, replaceType) && !checkTile(x + 1, y - 1, type)) ||
                (!checkTile(x - 1, y + 1, replaceType) && !checkTile(x - 1, y + 1, type)) ||
                (!checkTile(x + 1, y + 1, replaceType) && !checkTile(x + 1, y + 1, type)))
                return;

            tempTiles[tile] = type;

            recursiveGrowth(x + 1, y, type, replaceType, hops - rand.Next(1, 3));
            recursiveGrowth(x - 1, y, type, replaceType, hops - rand.Next(1, 3));
            recursiveGrowth(x, y + 1, type, replaceType, hops - rand.Next(1, 3));
            recursiveGrowth(x, y - 1, type, replaceType, hops - rand.Next(1, 3));
        }

        protected bool checkTile(int x, int y, int type){
            if (x < 0 || x >= width || y < 0 || y >= height)
                return false;

            return tempTiles[x + y*width] == type;
        }

        /// <summary>
        /// Generate a river
        /// </summary>
        /// <param name="origin">The origin.</param>
        /// <param name="destination">The destination.</param>
        protected void createRiver(Vector2 origin, Vector2 destination)
        {
            //Calculate and limit the slope
            float m = ((origin.Y - destination.Y)/ (origin.X - destination.X));
            if (Math.Abs(m) < 2)
                m = 2;

            if (Math.Abs(m) > 20)
                m = 20;

            //River random seeds
            int riverWidth = rand.Next(6, 12);
            int curve = rand.Next(50, 250);

            bool done = false;
            for (float x = 0; x < width && !done; x += 0.05f)
            {
                int curveOffset = (int)((float)curve * Math.Sin(x / 44f));

                //Draw river
                for (int w = 0; w < riverWidth; w++)
                {
                    if (w + curveOffset + (int)origin.Y * width + x + width * (int)((m * x)) < width * height &&
                        w + curveOffset + (int)origin.Y * width + x + width * (int)((m * x)) > 0)
                        tempTiles[(int)origin.Y * width + (int)x + width * (int)((m * x)) + w + curveOffset] = (int)BaseTiles.Water;
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
            else if (obj is Bullet)
            {
                this.bullets.BufferAdd((Bullet) obj);
            }
            else
            {
                throw new InvalidOperationException("Don't have a handler for adding this type of object");
            }
        }

        public void Add(EmotionBeam beam)
        {
            this.emotionBeams.BufferAdd(beam);
        }

        public void Remove(GameObject obj)
        {
            obj.IsAlive = false;
            if (obj is Player)
            {
                this.players.BufferRemove((Player)obj);
            }
            else if (obj is Villager)
            {
                this.villagers.BufferRemove((Villager)obj);
            }
            else if (obj is Bullet)
            {
                this.bullets.BufferRemove((Bullet) obj);
            }
            else
            {
                throw new InvalidOperationException("Don't have a handler for removing this type of object");
            }
        }

        public void Remove(EmotionBeam beam)
        {
            this.emotionBeams.BufferRemove(beam);
        }

        public void Update()
        {
            if(DateTime.Now.Ticks - animationTimer > 1151000){
                animationTimer = DateTime.Now.Ticks;
                animationCounter += 1;
                if (animationCounter > 3)
                    animationCounter = 0;
            }

            foreach (Player player in this.players)
            {
                player.Update();
                foreach (var bullet in bullets)
                {
                    player.HandleCollision(bullet);
                }
            }

            if (this.PartyWiped())
            {
                villagers.ForEach(villager => {villager.Emotion = new Emotion(EmotionType.Neutral); villager.OnEmotionChanged(null);});
            }

            foreach (var villager in villagers)
            {
                villager.Update();
                foreach (var bullet in bullets)
                {
                    villager.HandleCollision(bullet);
                }
            }

            foreach (var emotionBeam in this.emotionBeams)
            {
                emotionBeam.Update();
                foreach (var villager in this.villagers)
                {
                    foreach (var particle in emotionBeam.Particles)
                    {
                        villager.HandleCollision(particle);
                    }
                }
            }

            foreach (var bullet in bullets)
            {
                bullet.Update();
            }

            players.ApplyBuffers();
            villagers.ApplyBuffers();
            emotionBeams.ApplyBuffers();
            bullets.ApplyBuffers();
        }

        private bool PartyWiped()
        {
            return this.players.FindAll((player) => player.IsAlive).Count == 0;
        }

        public void Draw(SpriteBatch spr)
        {
            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    if (tiles[r * width + c] == 1)
                    {
                        spr.Draw(TextureBin.Pixel, new Vector2(c, r), null, Color.Blue, 0, Vector2.Zero, 
                            new Vector2(1, 1), SpriteEffects.None, 0);
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

            if (Input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.W) && yPos > 0)
                yPos -= 10;
            else if (Input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.S))
                yPos += 10;
            else if (Input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.A) && xPos > 0)
                xPos -= 10;
            else if (Input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D))
                xPos += 10;



            if (Input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.E))
                drawTemp = !drawTemp;
            
            if (drawTemp)
            {
                for (int r = 0; r < height; r++)
                {
                    for (int c = 0; c < width; c++)
                    {
                        if (drawTemp)
                        {
                            if (tempTiles[r * width + c] == (int)BaseTiles.Water)
                            {
                                spr.Draw(TextureBin.Pixel, new Vector2(c, r), null, Color.Blue, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
                            }
                            else if (tempTiles[r * width + c] == (int)BaseTiles.Grass)
                            {
                                spr.Draw(TextureBin.Pixel, new Vector2(c, r), null, Color.Green, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
                            }
                            else if (tempTiles[r * width + c] == (int)BaseTiles.Sand)
                            {
                                spr.Draw(TextureBin.Pixel, new Vector2(c, r), null, Color.Yellow, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
                            }
                            else if (tempTiles[r * width + c] == (int)BaseTiles.Mountain)
                            {
                                spr.Draw(TextureBin.Pixel, new Vector2(c, r), null, Color.DarkGray, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
                            }
                        }
                    }
                }
            }
            else
            {
                for (int r = yPos; r < yPos + 35; r++)
                {
                    for (int c = xPos; c < xPos + 40; c++)
                    {
                        if (tiles[r * width + c] >= 20 && tiles[r * width + c]%10 == 0)
                        {
                            spr.Draw(TextureBin.Get("tileset"), new Vector2(c * 32 - xPos * 32, r * 32 - yPos * 32),
                                new Rectangle((tiles[r * width + c] % 10) * 32 + animationCounter*32, (tiles[r * width + c] / 10) * 32, 32, 32),
                                Color.White, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
                        }
                        else
                        {
                            spr.Draw(TextureBin.Get("tileset"), new Vector2(c * 32 - xPos * 32, r * 32 - yPos * 32),
                                new Rectangle((tiles[r * width + c] % 10) * 32, (tiles[r * width + c] / 10) * 32, 32, 32),
                                Color.White, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
                        }
                    }
                }
            }

            foreach (Player player in players)
            {
                player.Draw(spr);
            }

            foreach (var villager in villagers)
            {
                villager.Draw(spr);
            }

            foreach (var emotionBeam in this.emotionBeams)
            {
                emotionBeam.Draw(spr);
            }

            foreach (var bullet in bullets)
            {
                bullet.Draw(spr);
            }
        }
    }
}