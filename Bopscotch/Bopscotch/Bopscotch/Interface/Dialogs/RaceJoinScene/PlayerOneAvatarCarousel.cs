
using Leda.Core.Gamestate_Management;

using Bopscotch.Input;

namespace Bopscotch.Interface.Dialogs.RaceJoinScene
{
    public class PlayerOneAvatarCarousel : RaceCharacterSelectionCarouselDialog
    {
        public PlayerOneAvatarCarousel(SelectionButtonHandler sceneButtonHandler, 
            Scene.ObjectRegistrationHandler registrationHandler, Scene.ObjectUnregistrationHandler unregistrationHandler)
            : base(sceneButtonHandler, registrationHandler, unregistrationHandler)
        {
            _boxCaption = Translator.Translation("Player One Select Character");

            TopYWhenActive = Dialog_Margin;
            TopYWhenInactive = -Dialog_Height;
        }

        public override void Activate()
        {
            InputSources[0] = ControllerPool.Controllers.PlayerOne;
            base.Activate();
        }
    }
}
