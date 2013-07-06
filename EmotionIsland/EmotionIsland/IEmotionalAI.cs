using Microsoft.Xna.Framework;

namespace EmotionIsland
{
    public interface IEmotionalAI
    {
        Vector2 TargetPosition { get; set; }

        void UpdateAI(EmotionType emotion);

        void AngryUpdate();
        void HappyUpdate();
        void TerrifiedUpdate();
        void SadUpdate();
        void NeutralUpdate();
    }
}