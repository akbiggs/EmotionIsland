using Microsoft.Xna.Framework;

namespace EmotionIsland
{
    public interface IEmotionalAI
    {
        Vector2 NextPosition { get; set; }
        GameObject EmotionalTarget { get; set; }

        void UpdateAI(EmotionType emotion);

        bool InfectWithEmotion(EmotionType emotion, GameObject source);
        void Anger(GameObject source);
        void Terrify(GameObject source);
        void Excite(GameObject source);
        void Depress(GameObject source);

        void AngryUpdate();
        void VigilantUpdate();
        void HappyUpdate();
        void AdmirativeUpdate();
        void AmazedUpdate();
        void HatefulUpdate();
        void TerrifiedUpdate();
        void SadUpdate();
        void NeutralUpdate();

        void OnEmotionChanged(GameObject source);
    }
}