using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EmotionIsland
{
    public class SlashAttack : Bullet
    {
        public SlashAttack(World world, Vector2 spawnPosition, GameObject owner, Vector2 direction) 
            : base(world, spawnPosition, new Vector2(24, 24), TextureBin.Pixel, owner, 50, 0, direction)
        {
        
        }
    }
}
