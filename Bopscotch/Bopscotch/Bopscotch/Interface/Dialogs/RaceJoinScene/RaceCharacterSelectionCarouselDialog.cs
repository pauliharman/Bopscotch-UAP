using Microsoft.Xna.Framework;

using Leda.Core.Gamestate_Management;
using Leda.Core.Asset_Management;

using Bopscotch.Input;
using Bopscotch.Interface.Dialogs.Carousel;

namespace Bopscotch.Interface.Dialogs.RaceJoinScene
{
    public class RaceCharacterSelectionCarouselDialog : AvatarSelectionCarouselDialog
    {
        public delegate void SelectionButtonHandler(string buttonCaption, AvatarSelectionCarouselDialog sender);

        private SelectionButtonHandler _sendButtonActionToScene;

        public RaceCharacterSelectionCarouselDialog(SelectionButtonHandler sceneButtonHandler,
            Scene.ObjectRegistrationHandler registrationHandler, Scene.ObjectUnregistrationHandler unregistrationHandler)
            : base(registrationHandler, unregistrationHandler)
        {
            _sendButtonActionToScene = sceneButtonHandler;

            AddButton("Back", new Vector2(Definitions.Left_Button_Column_X, 325), Button.ButtonIcon.B, Color.Red, 0.7f);
            AddButton("Select", new Vector2(Definitions.Right_Button_Column_X, 325), Button.ButtonIcon.A, Color.LawnGreen, 0.7f);

            Height = Race_Character_Selection_Dialog_Height;

            InputSources.Add(null);
        }

        public override void Activate()
        {
            base.Activate();
            EnableForSelection();
        }

        private void EnableForSelection()
        {
            _buttons["Select"].Caption = "Select";
            AButtonPressedValue = "Select";
            CarouselDisabled = false;
        }

        protected override void SelectionHandler(string buttonCaption)
        {
            base.SelectionHandler(buttonCaption);

            if (buttonCaption == "Select")
            {
                _buttons["Select"].Caption = "Change";
                AButtonPressedValue = "Change";
                CarouselDisabled = true;
                foreach (ICarouselDialogItem item in _items)
                {
                    if (item.SelectionValue == Selection)
                    {
                        ((CarouselAvatar)item).AnimationEngine.Sequence = AnimationDataManager.Sequences["player-front-win"];
                        item.Active = true;
                    }
                }
            }
            else if (buttonCaption == "Change")
            {
                EnableForSelection();
                AButtonPressedValue = "Select";
                foreach (ICarouselDialogItem item in _items)
                {
                    if (item.SelectionValue == Selection) 
                    {
                        ((CarouselAvatar)item).AnimationEngine.Sequence = AnimationDataManager.Sequences["player-front-resting"];
                    }
                }
            }

            _sendButtonActionToScene(buttonCaption, this);
        }

        private const int Race_Character_Selection_Dialog_Height = 395;
    }
}
