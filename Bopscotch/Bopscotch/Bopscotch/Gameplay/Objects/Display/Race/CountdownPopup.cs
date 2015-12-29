using Microsoft.Xna.Framework;

using Bopscotch.Effects.Popups;

namespace Bopscotch.Gameplay.Objects.Display.Race
{
    public class CountdownPopup : AutoDismissingPopup
    {
        private int _frameIndex;

        public bool Running { get; private set; }

        public CountdownPopup()
            : base()
        {
            base.AnimationCompletionHandler = null;
        }

        public override void Reset()
        {
            AnimationEngine = null;
            _frameIndex = 0;
            base.Reset();
            Running = false;
        }

        public override void Activate()
        {
            TextureReference = Texture_Name;
            SetFrameMetrics();
            Running = true;

            base.Activate();

            Scale = 0.0f;
            Tint = Color.White;
        }

        private void SetFrameMetrics()
        {
            Frame = new Rectangle(Number_Frame_Width * _frameIndex, 0, (_frameIndex < Frame_Count - 1 ? Number_Frame_Width : Go_Frame_Width), Frame.Height);
            Origin = new Vector2(Frame.Width, Frame.Height) / 2.0f;
        }

        protected override void HandleAnimationSequenceCompletion()
        {
            if (!_appearing) { HandleEndOfFrame(); }
            else { base.HandleAnimationSequenceCompletion(); }
        }

        private void HandleEndOfFrame()
        {
            if (++_frameIndex == Frame_Count) 
            {
                _frameIndex = 0;
                base.HandleAnimationSequenceCompletion();
            }
            else 
            {
                Activate(); 
            }
        }

        private const string Texture_Name = "popup-countdown";
        private const int Number_Frame_Width = 80;
        private const int Go_Frame_Width = 190;
        private const int Frame_Count = 4;
    }
}
