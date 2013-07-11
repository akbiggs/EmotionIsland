using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EmotionIsland
{
    public class Treasure : GameObject
    {
        public Treasure(World world, Vector2 position) : base(world, position, new Vector2(64, 64), TextureBin.Pixel)
        {
            this.Animations = new List<AnimationSet>
                {
                    new AnimationSet("main", TextureBin.Get("treasure"), 1, 64, 64, 1),
                    new AnimationSet("open", TextureBin.Get("treasure"), 5, 64, 64, FrameDuration, 5, false, 0)
                };
            this.ChangeAnimation("main");
        }

        public void Open()
        {
            this.ChangeAnimation("open");
        }

        public bool Opened { get { return this.CurAnimation.IsCalled("open") && this.CurAnimation.IsDonePlaying(); } }
    }
}
