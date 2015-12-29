using Microsoft.Xna.Framework;

using Bopscotch.Interface.Dialogs;
using Bopscotch.Interface.Dialogs.Carousel;
using Bopscotch.Input;

namespace Bopscotch.Interface.Dialogs.RaceJoinScene
{
    public class PlayerTwoStartDialog : ButtonDialog
    {
        public PlayerTwoStartDialog()
            : base()
        {
            _boxCaption = Translator.Translation("Player Two Press Start Button");

            Height = Dialog_Height;
            TopYWhenActive = Definitions.Back_Buffer_Height - (Dialog_Height + Dialog_Margin);

            AddButton("Start", new Vector2(Definitions.Back_Buffer_Center.X, 325), Button.ButtonIcon.Play, Color.LawnGreen, 0.7f);
            AddButton("Back", new Vector2(Definitions.Back_Buffer_Center.X, 1325), Button.ButtonIcon.Play, Color.LawnGreen, 0.7f);

            _defaultButtonCaption = "Start";
            _cancelButtonCaption = "Back";
        }

        public override void Reset()
        {
            base.Reset();
            WorldPosition = new Vector2(WorldPosition.X, Definitions.Back_Buffer_Height);
        }

        private const int Dialog_Height = 400;
        private const float Dialog_Margin = 30.0f;
    }
}
