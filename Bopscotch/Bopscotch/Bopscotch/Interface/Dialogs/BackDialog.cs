using Microsoft.Xna.Framework;

namespace Bopscotch.Interface.Dialogs
{
    public class BackDialog : ButtonDialog
    {
        public string ButtonCaption { set { _cancelButtonCaption = value; } }

        public BackDialog()
            : base()
        {
            Height = Dialog_Height;
            TopYWhenActive = (Definitions.Back_Buffer_Height * (1.0f - Data.Profile.Settings.DisplaySafeAreaFraction)) - (Dialog_Height + Bottom_Margin);

            AddButton("Back", new Vector2(Definitions.Left_Button_Column_X, Button_Y), Button.ButtonIcon.B, Color.Red, 0.6f);

            _cancelButtonCaption = "Back";
        }

        public override void Reset()
        {
            base.Reset();
            WorldPosition = new Vector2(0.0f, Definitions.Back_Buffer_Height);
            ActivateButton(_cancelButtonCaption);
        }

        private const int Dialog_Height = 150;
        private const float Bottom_Margin = 20.0f;

        private const float Button_Y = 75.0f;
        private const float Social_Button_Spacing = 125.0f;
    }
}
