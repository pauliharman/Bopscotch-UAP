using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Bopscotch.Input;

namespace Bopscotch.Interface.Dialogs.RaceGameplayScene
{
    public class QuitRaceDialog : ButtonDialog
    {
        private bool _inputSourcesAreConnected;

        public QuitRaceDialog()
            : base()
        {
            Height = Dialog_Height;
            TopYWhenActive = (Definitions.Back_Buffer_Height - Dialog_Height) / 2.0f;

            AddButton("Confirm", new Vector2(Definitions.Left_Button_Column_X, 200), Button.ButtonIcon.Tick, Color.Red);
            AddButton("Cancel", new Vector2(Definitions.Right_Button_Column_X, 200), Button.ButtonIcon.Play, Color.DodgerBlue);

            SetMovementLinksForButton("Confirm", "", "", "", "Cancel");
            SetMovementLinksForButton("Cancel", "", "", "Confirm", "");

            _defaultButtonCaption = "Cancel";
            _cancelButtonCaption = "Cancel";

            _boxCaption = "Quit Race?";
        }

        public override void Reset()
        {
            base.Reset();
            WorldPosition = new Vector2(0.0f, -Height);
        }

        public void SetInputSourcesToPlayerControllers()
        {
            InputSources = new List<InputProcessorBase>();
            InputSources.Add(ControllerPool.Controllers.PlayerOne);
            InputSources.Add(ControllerPool.Controllers.PlayerTwo);

            _inputSourcesAreConnected = (ControllerPool.Controllers.PlayerOne.IsAvailable || ControllerPool.Controllers.PlayerTwo.IsAvailable);
        }

        public override void Activate()
        {
            ControllerPool.SetControllersToMenuMode();

            base.Activate();
        }

        protected override void Dismiss()
        {
            ControllerPool.SetControllersToGameplayMode();

            base.Dismiss();
        }

        public override void Update(int millisecondsSinceLastUpdate)
        {
            base.Update(millisecondsSinceLastUpdate);

            if (_inputSourcesAreConnected != (ControllerPool.Controllers.PlayerOne.IsAvailable || ControllerPool.Controllers.PlayerTwo.IsAvailable))
            {
                HandleConnectionStateChange();
                _inputSourcesAreConnected = (ControllerPool.Controllers.PlayerOne.IsAvailable || ControllerPool.Controllers.PlayerTwo.IsAvailable);
            }
        }

        private void HandleConnectionStateChange()
        {
            if (ControllerPool.Controllers.PlayerOne.IsAvailable || ControllerPool.Controllers.PlayerTwo.IsAvailable) { SetInputSourcesToPlayerControllers(); }
            else { InputSources = ControllerPool.Controllers.All; }
        }

        private const int Dialog_Height = 350;
    }
}
