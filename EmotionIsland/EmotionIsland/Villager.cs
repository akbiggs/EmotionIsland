using Microsoft.Xna.Framework;

namespace EmotionIsland
{
    public class Villager : EmotionalObject
    {
        public override Color Color { get { return this.Emotion.ToColor(); } }

        public Villager(World world, Vector2 pos, EmotionType emotion) 
            : base(world, pos, new Vector2(32, 32), TextureBin.Pixel, 3, emotion)
        {
        }

        public override void AngryUpdate()
        {

            base.AngryUpdate();
        }
    }
}