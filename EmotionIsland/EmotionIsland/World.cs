
using System.Diagnostics;
using EmotionIsland.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace EmotionIsland
{
    public class World
    {

        public enum BaseTiles
        {
            Grass = 0,
            Water = 1,
            Sand = 2,
            Mountain = 3,
            Doodad = 255
        }

        public enum BlockTiles
        {
            Free = 0,
            All = 15,
            Up = 1,
            Down = 2,
            Left = 4,
            Right = 8
        }

        int xPos;
        int yPos;

        public int width;
        public int height;
        int[] tiles;
        public int[] collisionMap;
        int[] tempTiles;
        bool drawTemp;

        public Camera2D Camera;
        public Vector2 LastCameraPos { get; set; }

        long animationTimer;
        int animationCounter;

        private BufferedList<Player> players = new BufferedList<Player>();
        public BufferedList<Player> Players { get { return players; } }
        private BufferedList<Villager> villagers = new BufferedList<Villager>();
        public BufferedList<Villager> Villagers { get { return villagers; } }

        private BufferedList<EmotionBeam> emotionBeams = new BufferedList<EmotionBeam>();
        private BufferedList<Bullet> bullets = new BufferedList<Bullet>();

        private Random rand;
        private int spawnTime = 500;

        List<Vector2> villages;

        public World()
        {
            rand = new Random();
            this.players.Add(new Player(this, new Vector2(40*32, 40*32), PlayerNumber.One));
            //this.players.Add(new Player(this, new Vector2(45*32, 45*32), PlayerNumber.Two));
            //this.players.Add(new Player(this, new Vector2(50*32, 50*32), PlayerNumber.Three));
            //this.players.Add(new Player(this, new Vector2(55*32, 55*32), PlayerNumber.Four));

            drawTemp = false;
            GenerateWorld();


            //this.villagers.Add(new Villager(this, new Vector2(200, 80), EmotionType.Angry));
            this.villagers.Add(new Villager(this, new Vector2(200, 120), EmotionType.Sad));
            //this.villagers.Add(new Villager(this, new Vector2(200, 160), EmotionType.Happy));
            this.villagers.Add(new Villager(this, new Vector2(200, 200), EmotionType.Terrified));
            //this.villagers.Add(new Villager(this, new Vector2(200, 240), EmotionType.Neutral));
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
            collisionMap = new int[width * height];

            //Create rivers
            int rivers = rand.Next(3, 6);
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
            int lakes = rand.Next(8, 23);
            for (int l = 0; l < lakes; l++)
            {
                Vector2 origin = new Vector2(0, 0);
                while (tempTiles[(int)origin.X + (int)origin.Y * width] != (int)BaseTiles.Grass)
                {
                    origin.X = rand.Next(0, width);
                    origin.Y = rand.Next(0, height);
                }
                recursiveGrowth((int)origin.X, (int)origin.Y, (int)BaseTiles.Water, (int)BaseTiles.Grass, rand.Next(40, 130));
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
            int mountains = rand.Next(4, 9);
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
                        collisionMap[tile] = (int)BlockTiles.Free;
                        if (checkTile(c - 1, r, (int)BaseTiles.Grass) && checkTile(c, r + 1, (int)BaseTiles.Grass))
                        {
                            tiles[tile] = 134; //TOP BOTTOM LEFT
                            collisionMap[tile] = (int)BlockTiles.All;
                        }
                        else if (checkTile(c + 1, r, (int)BaseTiles.Grass) && checkTile(c, r + 1, (int)BaseTiles.Grass))
                        {
                            tiles[tile] = 124; //TOP BOTTOM RIGHT
                            collisionMap[tile] = (int)BlockTiles.All;
                        }
                        else if (checkTile(c + 1, r + 1, (int)BaseTiles.Grass) && checkTile(c + 1, r, (int)BaseTiles.Sand)
                            && checkTile(c, r + 1, (int)BaseTiles.Sand))
                        {
                            tiles[tile] = 6; //TOP LEFT
                            collisionMap[tile + width] = (int)BlockTiles.Down;
                        }
                        else if (checkTile(c - 1, r - 1, (int)BaseTiles.Grass) && checkTile(c, r - 1, (int)BaseTiles.Sand)
                            && checkTile(c - 1, r, (int)BaseTiles.Sand))
                        {
                            tiles[tile] = 28; //BOTTOM RIGHT
                            collisionMap[tile] = (int)BlockTiles.Right;
                        }
                        else if (checkTile(c - 1, r + 1, (int)BaseTiles.Grass) && checkTile(c, r + 1, (int)BaseTiles.Sand)
                        && checkTile(c - 1, r, (int)BaseTiles.Sand))
                        {
                            tiles[tile] = 8; //TOP RIGHT
                            collisionMap[tile + width] = (int)BlockTiles.Down;
                        }
                        else if (checkTile(c, r - 1, (int)BaseTiles.Sand) && checkTile(c + 1, r, (int)BaseTiles.Sand)
                            && checkTile(c + 1, r - 1, (int)BaseTiles.Grass))
                        {
                            tiles[tile] = 26; //BOTTOM LEFT
                            collisionMap[tile] = (int)BlockTiles.Left;
                        }
                        else if (checkTile(c + 1, r, (int)BaseTiles.Grass) &&
                            checkTile(c, r - 1, (int)BaseTiles.Grass))
                        {
                            tiles[tile] = 144; //BOTTOM with Grass on right
                            collisionMap[tile] = (int)BlockTiles.All;
                            collisionMap[tile + width] = (int)BlockTiles.Down;
                        }
                        else if (checkTile(c - 1, r, (int)BaseTiles.Grass) &&
                            checkTile(c, r - 1, (int)BaseTiles.Grass))
                        {
                            tiles[tile] = 154; //BOTTOM with Grass on left
                            collisionMap[tile] = (int)BlockTiles.All;
                            collisionMap[tile + width] = (int)BlockTiles.Down;
                        }
                        else if (checkTile(c, r + 1, (int)BaseTiles.Grass))
                        {
                            tiles[tile] = 7; //TOP
                            collisionMap[tile] = (int)BlockTiles.Up;
                            collisionMap[tile + width] = (int)BlockTiles.Down;
                        }
                        else if (checkTile(c, r - 1, (int)BaseTiles.Grass))
                        {
                            tiles[tile] = 10 + rand.Next(0, 3); //BOTTOM
                            collisionMap[tile] = (int)BlockTiles.All;
                        }
                        else if (checkTile(c + 1, r, (int)BaseTiles.Grass))
                        {
                            tiles[tile] = 16; //LEFT SIDE
                            collisionMap[tile] = (int)BlockTiles.Left;
                            collisionMap[tile + 1] = (int)BlockTiles.Right;
                        }
                        else if (checkTile(c - 1, r, (int)BaseTiles.Grass))
                        {
                            tiles[tile] = 18; //RIGHT SIDE
                            collisionMap[tile] = (int)BlockTiles.Right;
                            collisionMap[tile - 1] = (int)BlockTiles.Left;
                        }
                        else
                        {
                            int randSand = rand.Next(0, 12);
                            if (randSand < 10)
                                tiles[tile] = 3 + rand.Next(0,3);//SAND
                            else if (randSand == 10)
                                tiles[tile] = 29;
                            else if(randSand == 11)
                                tiles[tile] = 39;
                        }
                        
                    }
                    else if (checkTile(c, r, (int)BaseTiles.Water))
                    {
                        collisionMap[tile] = (int)BlockTiles.All;
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
                        {
                            if (rand.Next(0, 15) == 0)
                                tiles[tile] = 160;
                            else
                                tiles[tile] = 13; //WATER
                        }
                    }
                    else if (checkTile(c, r, (int)BaseTiles.Grass))
                    {
                        if (checkTile(c - 1, r, (int)BaseTiles.Mountain) && checkTile(c, r + 1, (int)BaseTiles.Mountain))
                        {
                            tiles[tile] = 135; //TOP BOTTOM LEFT
                            collisionMap[tile] = (int)BlockTiles.All;
                        }
                        else if (checkTile(c + 1, r, (int)BaseTiles.Mountain) && checkTile(c, r + 1, (int)BaseTiles.Mountain))
                        {
                            tiles[tile] = 125; //TOP BOTTOM RIGHT
                            collisionMap[tile] = (int)BlockTiles.All;
                        }
                        else if (checkTile(c + 1, r + 1, (int)BaseTiles.Mountain) && checkTile(c + 1, r, (int)BaseTiles.Grass)
                            && checkTile(c, r + 1, (int)BaseTiles.Grass))
                        {
                            tiles[tile] = 36; //TOP LEFT
                            collisionMap[tile + width] = (int)BlockTiles.Down;
                        }
                        else if (checkTile(c - 1, r - 1, (int)BaseTiles.Mountain) && checkTile(c, r - 1, (int)BaseTiles.Grass)
                            && checkTile(c - 1, r, (int)BaseTiles.Grass))
                        {
                            tiles[tile] = 58; //BOTTOM RIGHT
                            collisionMap[tile] = (int)BlockTiles.Right;
                        }
                        else if (checkTile(c - 1, r + 1, (int)BaseTiles.Mountain) && checkTile(c, r + 1, (int)BaseTiles.Grass)
                        && checkTile(c - 1, r, (int)BaseTiles.Grass))
                        {
                            tiles[tile] = 38; //TOP RIGHT
                            collisionMap[tile + width] = (int)BlockTiles.Down;
                        }
                        else if (checkTile(c, r - 1, (int)BaseTiles.Grass) && checkTile(c + 1, r, (int)BaseTiles.Grass)
                            && checkTile(c + 1, r - 1, (int)BaseTiles.Mountain))
                        {
                            tiles[tile] = 56; //BOTTOM LEFT
                            collisionMap[tile] = (int)BlockTiles.Left;
                        }
                        else if (checkTile(c + 1, r, (int)BaseTiles.Mountain) &&
                            checkTile(c, r - 1, (int)BaseTiles.Mountain))
                        {
                            tiles[tile] = 145; //BOTTOM with Mountain on right
                            collisionMap[tile] = (int)BlockTiles.All;
                            collisionMap[tile + width] = (int)BlockTiles.Down;
                        }
                        else if (checkTile(c - 1, r, (int)BaseTiles.Mountain) &&
                            checkTile(c, r - 1, (int)BaseTiles.Mountain))
                        {
                            tiles[tile] = 155; //BOTTOM with Mountain on left
                            collisionMap[tile] = (int)BlockTiles.All;
                            collisionMap[tile + width] = (int)BlockTiles.Down;
                        }
                        else if (checkTile(c, r + 1, (int)BaseTiles.Mountain))
                        {
                            tiles[tile] = 37; //TOP
                            collisionMap[tile] = (int)BlockTiles.Up;
                            collisionMap[tile + width] = (int)BlockTiles.Down;
                        }
                        else if (checkTile(c, r - 1, (int)BaseTiles.Mountain))
                        {
                            tiles[tile] = 57; //BOTTOM
                            collisionMap[tile] = (int)BlockTiles.All;
                        }
                        else if (checkTile(c + 1, r, (int)BaseTiles.Mountain))
                        {
                            tiles[tile] = 46; //LEFT SIDE
                            collisionMap[tile] = (int)BlockTiles.Left;
                            collisionMap[tile + 1] = (int)BlockTiles.Right;
                        }
                        else if (checkTile(c - 1, r, (int)BaseTiles.Mountain))
                        {
                            tiles[tile] = 48; //RIGHT SIDE
                            collisionMap[tile] = (int)BlockTiles.Right;
                            collisionMap[tile - 1] = (int)BlockTiles.Left;
                        }
                        else
                        {
                            tiles[tile] = rand.Next(0, 3);//Grass
                        }
                    }
                    else if (checkTile(c, r, (int)BaseTiles.Mountain))
                    {
                        int floorTile = rand.Next(0, 3);
                        if (floorTile == 0)
                        {
                            tiles[tile] = 47;
                        }
                        else if (floorTile == 1)
                        {
                            tiles[tile] = 9;
                        }
                        else if (floorTile == 2)
                        {
                            tiles[tile] = 19;
                        }
                    }
                }
            }

            //Create villages
            villages = new List<Vector2>();
            int numVillages = rand.Next(2, 5);
            for (int village = 0; village < numVillages; village++)
            {
                Vector2 origin = new Vector2(0, 0);
                while (!checkTile((int)origin.X, (int)origin.Y, (int)BaseTiles.Grass)
                    && !checkTile((int)origin.X, (int)origin.Y+5, (int)BaseTiles.Grass)
                    && !checkTile((int)origin.X + 5, (int)origin.Y, (int)BaseTiles.Grass)
                    && !checkTile((int)origin.X + 5, (int)origin.Y + 5, (int)BaseTiles.Grass))
                {
                    origin.X = rand.Next(0, width);
                    origin.Y = rand.Next(0, height);
                }
                villages.Add(origin);

                int numHouses = rand.Next(3, 5);

                int posx = (int)origin.X;
                int posy = (int)origin.Y;
                for (int house = 0; house < numHouses; house++)
                {
                    if (checkTile(posx, posy, (int)BaseTiles.Grass) &&
                            checkTile(posx, posy + 1, (int)BaseTiles.Grass) &&
                            checkTile(posx, posy - 1, (int)BaseTiles.Grass) &&
                            checkTile(posx + 1, posy, (int)BaseTiles.Grass) &&
                            checkTile(posx - 1, posy, (int)BaseTiles.Grass) &&
                            checkTile(posx+ 1, posy+ 1, (int)BaseTiles.Grass) &&
                            checkTile(posx- 1, posy- 1, (int)BaseTiles.Grass))
                    {
                        players[0].Position = new Vector2(villages[0].X*32, villages[0].Y*32);
                        int tile = posx + posy * width;

                        int houseType = rand.Next(0, 3);
                        tiles[tile] = 74 + houseType * 2;
                        tiles[tile + width] = 84 + houseType * 2;
                        tiles[tile + 1] = 75 + houseType * 2;
                        tiles[tile + width + 1] = 85 + houseType*2;

                        tempTiles[tile] = (int)BaseTiles.Doodad;
                        tempTiles[tile + width] = (int)BaseTiles.Doodad;
                        tempTiles[tile + 1] = (int)BaseTiles.Doodad;
                        tempTiles[tile + width + 1] = (int)BaseTiles.Doodad;
                        collisionMap[tile] = (int)BlockTiles.All;
                        collisionMap[tile + 1] = (int)BlockTiles.All;
                        collisionMap[tile + width] = (int)BlockTiles.All;
                        collisionMap[tile + 1 + width] = (int)BlockTiles.All;

                        posx += 3 + rand.Next(0, 2);
                        posy += rand.Next(-3, 3);

                    }
                }

            }

            //Populate doodads
            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    int tile = r * width + c;

                    //Check for left grass ramps
                    if(checkTile(c, r, (int)BaseTiles.Grass) && 
                        checkTile(c, r - 1, (int)BaseTiles.Grass) &&
                        checkTile(c, r + 2, (int)BaseTiles.Grass) &&
                        checkTile(c, r+1, (int)BaseTiles.Grass) &&
                        checkTile(c - 1, r, (int)BaseTiles.Sand) &&
                        checkTile(c - 1, r - 1, (int)BaseTiles.Sand) &&
                        checkTile(c - 1, r + 2, (int)BaseTiles.Sand) && 
                        checkTile(c - 1, r+1, (int)BaseTiles.Sand)){
                            if (rand.Next(0, 3) == 0)
                            {
                                tiles[tile - 1] = 117;
                                tiles[tile + width - 1] = 127;
                                collisionMap[tile - 1] = (int)BlockTiles.Free;
                                collisionMap[tile + width - 1] = (int)BlockTiles.Free;
                                collisionMap[tile] = (int)BlockTiles.Free;
                                collisionMap[tile + width] = (int)BlockTiles.Free;
                                tempTiles[tile] = (int)BaseTiles.Doodad;
                                tempTiles[tile + width] = (int)BaseTiles.Doodad;
                                tempTiles[tile - 1] = (int)BaseTiles.Doodad;
                                tempTiles[tile + width - 1] = (int)BaseTiles.Doodad;
                                continue;
                            }

                    }

                    //Check for right grass ramps
                    if (checkTile(c, r, (int)BaseTiles.Grass) &&
                        checkTile(c, r - 1, (int)BaseTiles.Grass) &&
                        checkTile(c, r + 2, (int)BaseTiles.Grass) &&
                        checkTile(c, r + 1, (int)BaseTiles.Grass) &&
                        checkTile(c + 1, r, (int)BaseTiles.Sand) &&
                        checkTile(c + 1, r - 1, (int)BaseTiles.Sand) &&
                        checkTile(c + 1, r + 2, (int)BaseTiles.Sand) &&
                        checkTile(c + 1, r + 1, (int)BaseTiles.Sand))
                    {
                        if (rand.Next(0, 3) == 0)
                        {
                            tiles[tile + 1] = 119;
                            tiles[tile + width + 1] = 129;
                            collisionMap[tile + 1] = (int)BlockTiles.Free;
                            collisionMap[tile + width + 1] = (int)BlockTiles.Free;
                            collisionMap[tile] = (int)BlockTiles.Free;
                            collisionMap[tile + width] = (int)BlockTiles.Free;
                            tempTiles[tile] = (int)BaseTiles.Doodad;
                            tempTiles[tile + width] = (int)BaseTiles.Doodad;
                            tempTiles[tile + 1] = (int)BaseTiles.Doodad;
                            tempTiles[tile + width + 1] = (int)BaseTiles.Doodad;
                            continue;
                        }

                    }

                    //Check for top grass ramps
                    if (checkTile(c, r, (int)BaseTiles.Grass) &&
                        checkTile(c + 1, r, (int)BaseTiles.Grass) &&
                        checkTile(c - 1, r, (int)BaseTiles.Grass) &&
                        checkTile(c, r - 1, (int)BaseTiles.Sand) &&
                        checkTile(c - 1, r - 1, (int)BaseTiles.Sand) &&
                        checkTile(c + 1, r - 1, (int)BaseTiles.Sand) &&
                        checkTile(c, r + 2, (int)BaseTiles.Grass))
                    {
                        if (rand.Next(0, 3) == 0)
                        {
                            tiles[tile - width] = 108;
                            collisionMap[tile] = (int)BlockTiles.Free;
                            collisionMap[tile - width] = (int)BlockTiles.Free;
                            tempTiles[tile] = (int)BaseTiles.Doodad;
                            tempTiles[tile - width] = (int)BaseTiles.Doodad;
                            continue;
                        }

                    }

                    //Check for bottom grass ramps
                    if (checkTile(c, r, (int)BaseTiles.Grass) &&
                        checkTile(c + 1, r, (int)BaseTiles.Grass) &&
                        checkTile(c - 1, r, (int)BaseTiles.Grass) &&
                        checkTile(c, r + 1, (int)BaseTiles.Sand) &&
                        checkTile(c-1, r + 1, (int)BaseTiles.Sand) &&
                        checkTile(c + 1, r + 1, (int)BaseTiles.Sand) &&
                        checkTile(c, r + 2, (int)BaseTiles.Sand) &&
                        checkTile(c, r - 1, (int)BaseTiles.Grass))
                    {
                        if (rand.Next(0, 3) == 0)
                        {
                            tiles[tile + width] = 128;
                            collisionMap[tile] = (int)BlockTiles.Free;
                            collisionMap[tile + width] = (int)BlockTiles.Free;
                            tempTiles[tile] = (int)BaseTiles.Doodad;
                            tempTiles[tile + width] = (int)BaseTiles.Doodad;
                            continue;
                        }

                    }

                    //Check for left Mountain ramps
                    if (checkTile(c, r, (int)BaseTiles.Mountain) &&
                        checkTile(c, r - 1, (int)BaseTiles.Mountain) &&
                        checkTile(c, r + 2, (int)BaseTiles.Mountain) &&
                        checkTile(c, r + 1, (int)BaseTiles.Mountain) &&
                        checkTile(c - 1, r, (int)BaseTiles.Grass) &&
                        checkTile(c - 1, r - 1, (int)BaseTiles.Grass) &&
                        checkTile(c - 1, r + 2, (int)BaseTiles.Grass) &&
                        checkTile(c - 1, r + 1, (int)BaseTiles.Grass))
                    {
                        if (rand.Next(0, 3) == 0)
                        {
                            tiles[tile - 1] = 147;
                            tiles[tile + width - 1] = 157;
                            collisionMap[tile - 1] = (int)BlockTiles.Free;
                            collisionMap[tile + width - 1] = (int)BlockTiles.Free;
                            collisionMap[tile] = (int)BlockTiles.Free;
                            collisionMap[tile + width] = (int)BlockTiles.Free;
                            tempTiles[tile] = (int)BaseTiles.Doodad;
                            tempTiles[tile + width] = (int)BaseTiles.Doodad;
                            tempTiles[tile - 1] = (int)BaseTiles.Doodad;
                            tempTiles[tile + width - 1] = (int)BaseTiles.Doodad;
                            continue;
                        }

                    }

                    //Check for right Mountain ramps
                    if (checkTile(c, r, (int)BaseTiles.Mountain) &&
                        checkTile(c, r - 1, (int)BaseTiles.Mountain) &&
                        checkTile(c, r + 2, (int)BaseTiles.Mountain) &&
                        checkTile(c, r + 1, (int)BaseTiles.Mountain) &&
                        checkTile(c + 1, r, (int)BaseTiles.Grass) &&
                        checkTile(c + 1, r - 1, (int)BaseTiles.Grass) &&
                        checkTile(c + 1, r + 2, (int)BaseTiles.Grass) &&
                        checkTile(c + 1, r + 1, (int)BaseTiles.Grass))
                    {
                        if (rand.Next(0, 3) == 0)
                        {
                            tiles[tile + 1] = 149;
                            tiles[tile + width + 1] = 159;
                            collisionMap[tile + 1] = (int)BlockTiles.Free;
                            collisionMap[tile + width + 1] = (int)BlockTiles.Free;
                            collisionMap[tile] = (int)BlockTiles.Free;
                            collisionMap[tile + width] = (int)BlockTiles.Free;
                            tempTiles[tile] = (int)BaseTiles.Doodad;
                            tempTiles[tile + width] = (int)BaseTiles.Doodad;
                            tempTiles[tile + 1] = (int)BaseTiles.Doodad;
                            tempTiles[tile + width + 1] = (int)BaseTiles.Doodad;
                            continue;
                        }

                    }

                    //Check for top Mountain ramps
                    if (checkTile(c, r, (int)BaseTiles.Mountain) &&
                        checkTile(c + 1, r, (int)BaseTiles.Mountain) &&
                        checkTile(c - 1, r, (int)BaseTiles.Mountain) &&
                        checkTile(c, r - 1, (int)BaseTiles.Grass) &&
                        checkTile(c - 1, r - 1, (int)BaseTiles.Grass) &&
                        checkTile(c + 1, r - 1, (int)BaseTiles.Grass) &&
                        checkTile(c, r + 2, (int)BaseTiles.Mountain))
                    {
                        if (rand.Next(0, 3) == 0)
                        {
                            tiles[tile - width] = 138;
                            collisionMap[tile] = (int)BlockTiles.Free;
                            collisionMap[tile - width] = (int)BlockTiles.Free;
                            tempTiles[tile] = (int)BaseTiles.Doodad;
                            tempTiles[tile - width] = (int)BaseTiles.Doodad;
                            continue;
                        }

                    }

                    //Check for bottom Mountain ramps
                    if (checkTile(c, r, (int)BaseTiles.Mountain) &&
                        checkTile(c + 1, r, (int)BaseTiles.Mountain) &&
                        checkTile(c - 1, r, (int)BaseTiles.Mountain) &&
                        checkTile(c, r + 1, (int)BaseTiles.Grass) &&
                        checkTile(c - 1, r + 1, (int)BaseTiles.Grass) &&
                        checkTile(c + 1, r + 1, (int)BaseTiles.Grass) &&
                        checkTile(c, r + 2, (int)BaseTiles.Grass) &&
                        checkTile(c, r - 1, (int)BaseTiles.Mountain))
                    {
                        if (rand.Next(0, 3) == 0)
                        {
                            tiles[tile + width] = 158;
                            collisionMap[tile] = (int)BlockTiles.Free;
                            collisionMap[tile + width] = (int)BlockTiles.Free;
                            tempTiles[tile] = (int)BaseTiles.Doodad;
                            tempTiles[tile + width] = (int)BaseTiles.Doodad;
                            continue;
                        }

                    }

                    //Horizontal Bridges
                    if (checkTile(c, r, (int)BaseTiles.Sand) && 
                        checkTile(c, r+1, (int)BaseTiles.Sand) &&
                        checkTile(c + 1, r, (int)BaseTiles.Water) && 
                        checkTile(c + 1, r+1, (int)BaseTiles.Water)){
                            bool canBeBridged = true;
                            int i = 1;
                            for (i = 1; i < 15; i++)
                            {
                                if (checkTile(c + i, r, (int)BaseTiles.Sand) &&
                                checkTile(c + i, r + 1, (int)BaseTiles.Sand))
                                {
                                    canBeBridged = true;
                                    break;
                                }
                                else if (!checkTile(c + i, r, (int)BaseTiles.Water) ||
                               !checkTile(c + i, r + 1, (int)BaseTiles.Water))
                                {
                                    canBeBridged = false;
                                    break;
                                }
                            }

                            if (canBeBridged)
                            {
                                if (!checkTile(c + i, r, (int)BaseTiles.Sand) ||
                                !checkTile(c + i, r + 1, (int)BaseTiles.Sand))
                                    canBeBridged = false;
                            }

                            if (canBeBridged && rand.Next(0,12) == 0)
                            {
                                collisionMap[tile + width] = (int)BlockTiles.Free;
                                collisionMap[tile] = (int)BlockTiles.Free;
                                tempTiles[tile] = (int)BaseTiles.Doodad;
                                tempTiles[tile + width] = (int)BaseTiles.Doodad;

                                for (int j = 1; j < i; j++)
                                {
                                    tiles[tile + j] = 94;
                                    tiles[tile + j + width] = 104;

                                    tempTiles[tile + j] = (int)BaseTiles.Doodad;
                                    tempTiles[tile + width + j] = (int)BaseTiles.Doodad;
                                    tempTiles[tile - width + j] = (int)BaseTiles.Doodad;

                                    collisionMap[tile + j + width] = (int)BlockTiles.Free;
                                    collisionMap[tile + j] = (int)BlockTiles.Free;
                                }

                            }
                    }

                    //Vertical Bridges
                    if (checkTile(c, r, (int)BaseTiles.Sand) &&
                        checkTile(c+1, r, (int)BaseTiles.Sand) &&
                        checkTile(c, r + 1, (int)BaseTiles.Water) &&
                        checkTile(c + 1, r + 1, (int)BaseTiles.Water))
                    {
                        bool canBeBridged = true;
                        int i = 1;
                        for (i = 1; i < 15; i++)
                        {
                            if (checkTile(c, r+i, (int)BaseTiles.Sand) &&
                            checkTile(c + 1, r + i, (int)BaseTiles.Sand))
                            {
                                canBeBridged = true;
                                break;
                            }
                            else if (!checkTile(c, r+i, (int)BaseTiles.Water) ||
                           !checkTile(c + 1, r + i, (int)BaseTiles.Water))
                            {
                                canBeBridged = false;
                                break;
                            }
                        }

                        if (canBeBridged)
                        {
                            if (!checkTile(c, r + i, (int)BaseTiles.Sand) ||
                            !checkTile(c + 1, r + i, (int)BaseTiles.Sand))
                                canBeBridged = false;
                        }

                        if (canBeBridged && rand.Next(0, 12) == 0)
                        {
                            collisionMap[tile + 1] = (int)BlockTiles.Free;
                            collisionMap[tile] = (int)BlockTiles.Free;
                            tempTiles[tile] = (int)BaseTiles.Doodad;
                            tempTiles[tile + 1] = (int)BaseTiles.Doodad;

                            for (int j = 1; j < i; j++)
                            {
                                tiles[tile + j*width] = 95;
                                tiles[tile + 1 + j*width] = 96;

                                tempTiles[tile + j * width] = (int)BaseTiles.Doodad;
                                tempTiles[tile + 1 + j * width] = (int)BaseTiles.Doodad;
                                tempTiles[tile - 1 + j * width] = (int)BaseTiles.Doodad;

                                collisionMap[tile + j * width] = (int)BlockTiles.Free;
                                collisionMap[tile + j * width + 1] = (int)BlockTiles.Free;
                            }

                        }
                    }


                    //Do grass doodads
                    if (checkTile(c, r, (int)BaseTiles.Grass) && 
                        checkTile(c, r+1, (int)BaseTiles.Grass) && 
                        checkTile(c, r-1, (int)BaseTiles.Grass) && 
                        checkTile(c+1, r, (int)BaseTiles.Grass) && 
                        checkTile(c-1, r, (int)BaseTiles.Grass) && 
                        checkTile(c+1, r+1, (int)BaseTiles.Grass) && 
                        checkTile(c-1, r-1, (int)BaseTiles.Grass) && rand.Next(0, 4) == 0)
                    {
                        int doodadType = rand.Next(0, 9);
                        if (doodadType == 0)
                        {
                            tiles[tile] = 15;
                            collisionMap[tile] = (int)BlockTiles.All;
                        }
                        else if (doodadType == 1)
                        {
                            tiles[tile] = 25;
                            collisionMap[tile] = (int)BlockTiles.All;
                        }
                        else if (doodadType == 2)
                        {
                            tiles[tile] = 45;
                        }
                        else if (doodadType == 3)
                        {
                            tiles[tile] = 44;
                        }
                        else if (doodadType == 4)
                        {
                            tiles[tile] = 34;
                            tiles[tile + 1] = 35;
                            tempTiles[tile + 1] = (int)BaseTiles.Doodad;
                        }
                        else if (doodadType >= 5)
                        {
                            tiles[tile] = 14;
                            tiles[tile + width] = 24;

                            collisionMap[tile] = (int)BlockTiles.All;
                            collisionMap[tile + width] = (int)BlockTiles.All;
                            tempTiles[tile + width] = (int)BaseTiles.Doodad;
                        }
                        
                        tempTiles[tile] = (int)BaseTiles.Doodad;
                        
                    }

                    if (checkTile(c, r, (int)BaseTiles.Sand) &&
                        checkTile(c, r + 1, (int)BaseTiles.Sand) &&
                        checkTile(c, r - 1, (int)BaseTiles.Sand) &&
                        checkTile(c + 1, r, (int)BaseTiles.Sand) &&
                        checkTile(c - 1, r, (int)BaseTiles.Sand) &&
                        checkTile(c + 1, r + 1, (int)BaseTiles.Sand) &&
                        checkTile(c - 1, r - 1, (int)BaseTiles.Sand) && rand.Next(0, 4) == 0)
                    {
                        if (rand.Next(0, 5) == 0)
                        {
                            tiles[tile] = 54;
                            tiles[tile + width] = 64;
                            tiles[tile + 1] = 55;
                            tiles[tile + width + 1] = 65;

                            tempTiles[tile] = (int)BaseTiles.Doodad;
                            tempTiles[tile + width] = (int)BaseTiles.Doodad;
                            tempTiles[tile + 1] = (int)BaseTiles.Doodad;
                            tempTiles[tile + width + 1] = (int)BaseTiles.Doodad;
                            collisionMap[tile] = (int)BlockTiles.All;
                            collisionMap[tile+1] = (int)BlockTiles.All;
                            collisionMap[tile+width] = (int)BlockTiles.All;
                            collisionMap[tile+1+width] = (int)BlockTiles.All;
                        }
                    }

                    if (checkTile(c, r, (int)BaseTiles.Mountain) &&
                        checkTile(c, r + 1, (int)BaseTiles.Mountain) &&
                        checkTile(c, r - 1, (int)BaseTiles.Mountain) &&
                        checkTile(c + 1, r, (int)BaseTiles.Mountain) &&
                        checkTile(c - 1, r, (int)BaseTiles.Mountain) &&
                        checkTile(c + 1, r + 1, (int)BaseTiles.Mountain) &&
                        checkTile(c - 1, r - 1, (int)BaseTiles.Mountain) && rand.Next(0, 12) == 0)
                    {
                        tiles[tile] = 105;
                        tiles[tile + width] = 115;
                        tiles[tile + 1] = 106;
                        tiles[tile + width + 1] = 116;

                        tempTiles[tile] = (int)BaseTiles.Doodad;
                        tempTiles[tile + width] = (int)BaseTiles.Doodad;
                        tempTiles[tile + 1] = (int)BaseTiles.Doodad;
                        tempTiles[tile + width + 1] = (int)BaseTiles.Doodad;
                        collisionMap[tile] = (int)BlockTiles.All;
                        collisionMap[tile + 1] = (int)BlockTiles.All;
                        collisionMap[tile + width] = (int)BlockTiles.All;
                        collisionMap[tile + 1 + width] = (int)BlockTiles.All;
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
            if (Math.Abs(m) < 5)
                m = 5;

            if (Math.Abs(m) > 20)
                m = 20;

            //River random seeds
            int riverWidth = rand.Next(6, 12);
            int curve = rand.Next(50, 250);

            bool done = false;

            int riverCurveType = rand.Next(0, 3);
            float curveDivider = rand.Next(10, 30);

            for (float x = 0; x < width && !done; x += 0.05f)
            {
                int curveOffset = 0;
                if(riverCurveType == 0)
                    curveOffset = (int)((float)curve * Math.Sin(x / curveDivider));
                else if (riverCurveType == 0)
                    curveOffset = (int)((float)curve * Math.Sin(x / curveDivider) * Math.Cos(x / curveDivider));
                else if (riverCurveType == 0)
                    curveOffset = (int)((float)curve * Math.Cos(x / curveDivider));

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
                Player player = (Player) obj;
                player.Beam.Stopped = true;
                this.players.BufferRemove(player);
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

        public void Update()
        {
            if (Camera != null)
            {
                Camera.Zoom = GetCameraZoom();

                if (!this.PartyWiped)
                {
                    Camera.Update(GetPlayerCenter());
                    this.LastCameraPos = GetPlayerCenter();
                }
            }
            for (int i = 2; i <= 4; i++)
            {
                PlayerIndex index;
                if (!PartyWiped && PlayerIndex.TryParse(value: i.ToString(), result: out index))
                {
                    if (Input.gps[(int)index-1].IsConnected && Input.gps[(int)index-1].Buttons.Start == ButtonState.Pressed && NoPlayerWith(index))
                        this.Add(new Player(this, players[0].Position + new Vector2(50, 50), Player.Nums[(int)index]));
                }
            }
            TimeSpan span = new TimeSpan(DateTime.Now.Ticks);
            if (span.TotalMilliseconds - animationTimer > 200)
            {
                animationTimer = (long)span.TotalMilliseconds;
                animationCounter += 1;
                if (animationCounter > 3)
                    animationCounter = 0;
            }

            if (++this.Timer-spawnTime == 0 && !this.PartyWiped)
            {
                SpawnWaveOfVillagers();
                this.spawnTime = MathExtra.RandomInt(200, 500);
                this.Timer = 0;
            }

            foreach (Player player in this.players)
            {
                player.Update();
                foreach (var bullet in bullets)
                {
                    player.HandleCollision(bullet);
                }
                foreach (var villager in Villagers)
                {
                    player.HandleCollision(villager);
                }
            }

            if (this.PartyWiped)
            {
                villagers.ForEach(villager => {villager.Emotion = new Emotion(EmotionType.Neutral);
                    //villager.OnEmotionChanged(null);
                });
            }

            foreach (var villager in villagers)
            {
                villager.Update();
                foreach (var bullet in bullets)
                {
                    villager.HandleCollision(bullet);
                }
                foreach (var other in Villagers)
                {
                    if (other != villager)
                    {
                        villager.HandleCollision(other);
                    }
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

        private bool NoPlayerWith(PlayerIndex index)
        {
            foreach (Player player in players)
            {
                if (player.PlayerIndex == index) return false;
            }
            return true;
        }

        private float GetCameraZoom()
        {
            float totalPlayerDistance = 0;

            foreach (Player player in Players)
            {
                float distance = Vector2.DistanceSquared(this.Camera.Center, player.Position);
                totalPlayerDistance += distance;
            }

            return  MathHelper.Lerp(1, 0.5f, Math.Min(1, totalPlayerDistance/500000));
        }

        private Vector2 GetPlayerCenter()
        {
            Vector2 center = Vector2.Zero;
            foreach (Player player in Players)
            {
                center += player.Center;
            }

            center = center/Players.Count;
            return center;
        }

        private void SpawnWaveOfVillagers()
        {
            Player randomPlayer = players[MathExtra.RandomInt(players.Count)];
            Vector2 spawnOffset = randomPlayer.GetLastMovementDirection()*new Vector2(600);

            EmotionType randomEmotion = Emotion.RandomEmotion();

            for (int i = 0; i < MathExtra.RandomInt(5, 15); i++)
            {
                Villager villager = new Villager(this, randomPlayer.Position + spawnOffset + new Vector2(MathExtra.RandomInt(0, 200), MathExtra.RandomInt(0, 200)), randomEmotion);
                villager.EmotionalTarget = randomPlayer;
                this.Add(villager);
            }
        }

        protected int Timer
        {
            get; set;
        }

        public bool PartyWiped
        {
            get { return this.players.FindAll((player) => player.IsAlive).Count == 0; }
        }

        public void Draw(SpriteBatch spr)
        {
            if (Camera == null)
            {
                this.Camera = new Camera2D(spr.GraphicsDevice.PresentationParameters.BackBufferWidth, spr.GraphicsDevice.PresentationParameters.BackBufferHeight);
            }

            if (!drawTemp)
            {
                spr.Begin(SpriteSortMode.Deferred,
                                  BlendState.AlphaBlend,
                                  SamplerState.PointClamp,
                                  null,
                                  null,
                                  null,
                                  Camera.GetTransformation());
            }
            else
            {
                spr.Begin(SpriteSortMode.Deferred,
                                  BlendState.AlphaBlend);
            }

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
                yPos = (int)Camera.TopLeft.Y/32;
                xPos = (int)Camera.TopLeft.X/32;
                if (yPos < 0)
                    yPos = 0;
                if (xPos < 0)
                    xPos = 0;
                for (int r = yPos; r < yPos + 70*(1.5f) && r < height; r++)
                {
                    for (int c = xPos; c < xPos + 80 * (1.5f) && c < width; c++)
                    {
                        if (tiles[r * width + c] >= 20 && tiles[r * width + c]%10 == 0)
                        {
                            spr.Draw(TextureBin.Get("tileset"), new Vector2(c * 32, r * 32),
                                new Rectangle((tiles[r * width + c] % 10) * 32 + animationCounter*32, (tiles[r * width + c] / 10) * 32, 32, 32),
                                Color.White, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
                        }
                        else
                        {
                            spr.Draw(TextureBin.Get("tileset"), new Vector2(c * 32, r * 32),
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

            spr.End();
        }
    }
}