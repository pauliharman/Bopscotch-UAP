using Microsoft.Xna.Framework;

namespace Bopscotch.Interface.Dialogs.TitleScene
{
    public class MainMenuDialog : ButtonDialog
    {
        public MainMenuDialog()
            : base()
        {
            Height = Dialog_Height;
            TopYWhenActive = Top_Y_When_Active;

            AddButton("About", new Vector2(Definitions.Left_Button_Column_X, 85), Button.ButtonIcon.Help, Color.DodgerBlue, 0.7f);
            AddButton("Options", new Vector2(Definitions.Left_Button_Column_X, 215), Button.ButtonIcon.Options, Color.DodgerBlue, 0.7f);
            AddButton("Display", new Vector2(Definitions.Right_Button_Column_X, 85), Button.ButtonIcon.Display, Color.DodgerBlue, 0.7f);
            AddButton("Character", new Vector2(Definitions.Right_Button_Column_X, 215), Button.ButtonIcon.Character, Color.DodgerBlue, 0.7f);
            AddButton("Start!", new Vector2(Definitions.Back_Buffer_Center.X, 360), Button.ButtonIcon.Play, Color.LawnGreen);
            AddButton("Quit", new Vector2(Definitions.Back_Buffer_Center.X, 1500), Button.ButtonIcon.None, Color.Transparent);

            SetMovementLinksForButton("About", "", "Options", "", "Display");
            SetMovementLinksForButton("Options", "About", "Start!", "", "Character");
            SetMovementLinksForButton("Display", "", "Character", "About", "");
            SetMovementLinksForButton("Character", "Display", "Start!", "Options", "");
            SetMovementLinksForButton("Start!", "Character", "", "Options", "Character");

            _defaultButtonCaption = "Start!";
            _cancelButtonCaption = "Quit";
        }

        protected override void ActivateButton(string caption)
        {
            if ((caption == "Start!") && (_activeButtonCaption != null))
            {
                if (_activeButtonCaption == "Options") { SetMovementLinksForButton("Start!", "Options", "", "Options", "Character"); }
                else { SetMovementLinksForButton("Start!", "Character", "", "Options", "Character"); }
            }

            base.ActivateButton(caption);
        }

        private const int Dialog_Height = 480;
        private const float Top_Y_When_Active = 350.0f;
    }
}
