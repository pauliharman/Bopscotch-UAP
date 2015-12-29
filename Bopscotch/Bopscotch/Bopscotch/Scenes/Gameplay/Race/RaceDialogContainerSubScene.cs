using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core.Game_Objects.Controllers;

using Bopscotch.Interface.Dialogs;
using Bopscotch.Interface.Dialogs.RaceGameplayScene;

namespace Bopscotch.Scenes.Gameplay.Race
{
    public class RaceDialogContainerSubScene : SubsceneBase
    {
        private MotionController _motionController;
        private QuitRaceDialog _quitRaceDialog;
        private ControllerUnpluggedDialog _controllerDialog;

        public override int RenderLayer { get { return Render_Layer; } set { } }
        public bool DisplayingQuitRaceDialog { get { return _quitRaceDialog.Visible; } }
        public bool DisplayingControllerDialog { get { return _controllerDialog.Visible; } }

        public ButtonDialog.ButtonSelectionHandler DialogCloseHandler
        {
            set
            {
                _quitRaceDialog.SelectionCallback = value;
                _controllerDialog.SelectionCallback = value;
            }
        }


        public RaceDialogContainerSubScene()
            : base(Definitions.Back_Buffer_Width, Definitions.Back_Buffer_Height)
        {
            _quitRaceDialog = new QuitRaceDialog();
            RegisterGameObject(_quitRaceDialog);

            _controllerDialog = new ControllerUnpluggedDialog();
            RegisterGameObject(_controllerDialog);

            _motionController = new MotionController();
            _motionController.AddMobileObject(_quitRaceDialog);
            _motionController.AddMobileObject(_controllerDialog);
        }

        public override void CreateBackBuffer()
        {
            CreateBackBuffer((int)_bufferDimensions.X, (int)_bufferDimensions.Y, false);
        }

        public void Activate()
        {
            //float x = Definitions.Back_Buffer_Width * Data.Profile.Settings.DisplaySafeAreaFraction;
            //float y = Definitions.Back_Buffer_Height * Data.Profile.Settings.DisplaySafeAreaFraction;
            //float width = (Definitions.Back_Buffer_Width * (1.0f - Data.Profile.Settings.DisplaySafeAreaFraction)) - x;
            //float height = (Definitions.Back_Buffer_Height * (1.0f - Data.Profile.Settings.DisplaySafeAreaFraction)) - y;

            //BufferArea = new Rectangle((int)(x + Data.Profile.Settings.DisplaySafeAreaTopLeft.X), (int)(y + Data.Profile.Settings.DisplaySafeAreaTopLeft.Y),
            //    (int)width, (int)height);



            _quitRaceDialog.SetInputSourcesToPlayerControllers();
        }

        public override void Update(int millisecondsSinceLastUpdate)
        {
            base.Update(millisecondsSinceLastUpdate);

            _motionController.Update(millisecondsSinceLastUpdate);
        }

        public void HandleBackButtonPress(bool raceIsPaused)
        {
            if (_controllerDialog.Active)
            {
                _controllerDialog.DismissWithReturnValue("Back");
            }
            else if (!_controllerDialog.Visible)
            {
                if (!raceIsPaused) { _quitRaceDialog.Activate(); }
                else if (_quitRaceDialog.Active) { _quitRaceDialog.DismissWithReturnValue("Cancel"); }
            }
        }

        public void HandleControllerUnplugged()
        {
            _controllerDialog.Activate();
        }

        private const int Render_Layer = 4;
    }
}
