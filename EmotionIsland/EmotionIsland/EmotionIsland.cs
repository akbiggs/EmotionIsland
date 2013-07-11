using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using EmotionIsland.Helpers;

namespace EmotionIsland
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class EmotionIsland : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        public static SpriteFont font;
        public static SpriteFont fontTiny;

        private World world;
        public Color fadeColor = Color.Black;
        public static Color nextFade;
        private bool fadingTitle;

        List<String> Creators;

        public EmotionIsland()
        {
            Creators = new List<string>();
            List<String> creators = new List<string> { "Artur Kink: Programmer", "Alexander Biggs: Programmer", "Andrew Wong: Artist" };
            //Randomly order creators
            while (creators.Count > 0)
            {
                int newIndex = MathExtra.RandomInt(creators.Count);
                Creators.Add(creators[newIndex]);
                creators.RemoveAt(newIndex);
            }

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Input.init();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            IsMouseVisible = false;
            graphics.IsFullScreen = true;
            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 780;

            graphics.ApplyChanges();
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            TextureBin.LoadContent(Content);
            font = Content.Load<SpriteFont>("Tahoma");
            fontTiny = Content.Load<SpriteFont>("TahomaTiny");
            
            this.world = new World();
            FadeTo(Color.Transparent);
            this.ShouldDrawTitle = true;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Input.IsKeyDown(Keys.I))
                this.Exit();

            if (fadingTitle && this.fadeColor == Color.Black)
            {
                this.ShouldDrawTitle = false;
                FadeTo(Color.Transparent);
            }
#if !ARCADE
            if (Input.KeyPressed(Keys.Q))
                world.GenerateWorld();
            if(Input.KeyPressed(Keys.R)){
                world.Players[0].Position = world.villages[MathExtra.RandomInt(world.villages.Count)];
                world.Players[0].Position = new Vector2(world.Players[0].Position.X * 32, world.Players[0].Position.Y * 32);
            }
#endif
            if (nextFade != this.fadeColor)
            {
                this.fadeColor = this.fadeColor.PushTowards(nextFade, 2);
            }
            Input.Update();

            if (this.fadingTitle)
                this.world.Update();

#if KEYBOARD
            if (this.world != null && this.world.PartyWiped &&
                (Input.gps[(int)PlayerIndex.One].Buttons.Start == ButtonState.Pressed ||
                Input.gps[(int)PlayerIndex.Two].Buttons.Start == ButtonState.Pressed))
            {
                this.world = new World();
            }
            if (this.ShouldDrawTitle &&
                (Input.gps[(int)PlayerIndex.One].Buttons.Start == ButtonState.Pressed ||
                Input.gps[(int)PlayerIndex.Two].Buttons.Start == ButtonState.Pressed))
            {
                this.fadingTitle = true;
                FadeTo(Color.Black);
            }
#else
            if (this.world != null && this.world.PartyWiped && 
                (Input.gps[(int)PlayerIndex.One].Buttons.Start == ButtonState.Pressed ||
                Input.gps[(int)PlayerIndex.Two].Buttons.Start == ButtonState.Pressed ||
                Input.gps[(int)PlayerIndex.Three].Buttons.Start == ButtonState.Pressed ||
                Input.gps[(int)PlayerIndex.Four].Buttons.Start == ButtonState.Pressed))
            {
                this.world = new World();
            }
            if (this.ShouldDrawTitle && 
                (Input.gps[(int)PlayerIndex.One].Buttons.Start == ButtonState.Pressed ||
                Input.gps[(int)PlayerIndex.Two].Buttons.Start == ButtonState.Pressed ||
                Input.gps[(int)PlayerIndex.Three].Buttons.Start == ButtonState.Pressed ||
                Input.gps[(int)PlayerIndex.Four].Buttons.Start == ButtonState.Pressed))
            {
                this.fadingTitle = true;
                FadeTo(Color.Black);
            }
#endif
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            if (!this.ShouldDrawTitle)
            {
                world.Draw(spriteBatch);
                spriteBatch.Begin();

                int completeCounter = 0;
                foreach (Treasure treasure in world.treasures)
                {
                    if (treasure.Opened)
                        completeCounter++;
                }
                
                spriteBatch.DrawString(fontTiny, "Treasures Collected: " + completeCounter + "/" + world.treasures.Count, new Vector2(5, 5), Color.White);
                spriteBatch.End();
                if (completeCounter == world.treasures.Count)
                {
                    world.GenerateWorld();
                }
            }
            else
            {
                spriteBatch.Begin();
                spriteBatch.Draw(TextureBin.Get("title"), new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), Color.White);
                spriteBatch.DrawString(font, "A game by", new Vector2(20, 480), Color.Black);

                for (int i = 0; i < 3; i++)
                {
                    spriteBatch.DrawString(font, Creators[i], new Vector2(20, 512 + i * 32), Color.Black);
                }
                
                spriteBatch.End();
            }

            spriteBatch.Begin();
            spriteBatch.Draw(TextureBin.Pixel, new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), this.fadeColor);
            spriteBatch.End();
            base.Draw(gameTime);
        }

        protected bool ShouldDrawTitle { get; set; }

        public static void FadeTo(Color newColor)
        {
            EmotionIsland.nextFade = newColor;
        }

    }
}
