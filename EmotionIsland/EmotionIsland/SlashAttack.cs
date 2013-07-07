using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using EmotionIsland.Helpers;

namespace EmotionIsland
{
    public class SlashAttack : Bullet
    {
        public override float Rotation
        {
            get { return this.Direction.ToAngle(); }
        }

        public SlashAttack(World world, Vector2 spawnPosition, GameObject owner, Vector2 direction) 
            : base(world, spawnPosition, new Vector2(0.25f), TextureBin.Pixel, owner, 50, 0, direction)
        {
            this.Animations = new List<AnimationSet>
                {
                    new AnimationSet("main", TextureBin.Get("swipe"), 6, 64, 64, 3, -1, false)
                };
            this.ChangeAnimation("main");
        }

        public override void Update()
        {
            if (this.CurAnimation.IsDonePlaying())
                this.World.Remove(this);

            base.Update();
        }
    }
}
