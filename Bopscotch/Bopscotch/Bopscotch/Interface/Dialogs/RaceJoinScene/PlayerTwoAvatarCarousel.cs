using Leda.Core.Gamestate_Management;

using Bopscotch.Input;

namespace Bopscotch.Interface.Dialogs.RaceJoinScene
{
    public class PlayerTwoAvatarCarousel : RaceCharacterSelectionCarouselDialog
    {
        public PlayerTwoAvatarCarousel(SelectionButtonHandler sceneButtonHandler,
            Scene.ObjectRegistrationHandler registrationHandler, Scene.ObjectUnregistrationHandler unregistrationHandler)
            : base(sceneButtonHandler, registrationHandler, unregistrationHandler)
        {
            _boxCaption = Translator.Translation("Player Two Select Character");

            TopYWhenActive = Definitions.Back_Buffer_Height - (Dialog_Height + Dialog_Margin);
            TopYWhenInactive = Definitions.Back_Buffer_Height;
        }

        public override void Activate()
        {
            InputSources[0] = ControllerPool.Controllers.PlayerTwo;
            base.Activate();
        }
    }
}
