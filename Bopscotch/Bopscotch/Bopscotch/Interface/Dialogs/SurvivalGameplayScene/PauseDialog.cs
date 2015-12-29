using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Bopscotch.Input;

namespace Bopscotch.Interface.Dialogs.SurvivalGameplayScene
{
    public class PauseDialog : ButtonDialog
    {
        public bool SkipLevelButtonDisabled { set { _buttons["Skip Level"].Disabled = value; } }

        private bool _inputSourceIsConnected;
        private InputProcessorBase _primaryInputSource;

        public PauseDialog()
            : base()
        {
            Height = Dialog_Height;
            TopYWhenActive = Definitions.Back_Buffer_Center.Y - (Dialog_Height / 2.0f);

            AddButton("Continue", new Vector2(Definitions.Left_Button_Column_X, 200), Button.ButtonIcon.Play, Color.LawnGreen);
            AddButton("Skip Level", new Vector2(Definitions.Right_Button_Column_X, 200), Button.ButtonIcon.Ticket, Color.DodgerBlue);
            AddButton("Quit", new Vector2(Definitions.Back_Buffer_Center.X, 320), Button.ButtonIcon.Quit, Color.Red, 0.7f);

            SetMovementLinksForButton("Continue", "", "Quit", "", "Skip Level");
            SetMovementLinksForButton("Skip Level", "", "Quit", "Continue", "");
            SetMovementLinksForButton("Quit", "", "", "Continue", "Skip Level");

            _cancelButtonCaption = "Continue";

            _boxCaption = "Paused";
        }

        public override void Reset()
        {
            base.Reset();
            WorldPosition = new Vector2(0.0f, -Height);
            ActivateButton("Continue");
        }

        public void SetInputSourceToPlayerController(InputProcessorBase inputSource)
        {
            _primaryInputSource = inputSource;

            InputSources = new List<InputProcessorBase>();
            InputSources.Add(inputSource);

            _inputSourceIsConnected = inputSource.IsAvailable;
        }

        protected override void ActivateButton(string caption)
        {
            if ((caption == "Quit") && (_activeButtonCaption != "Quit") && (_activeButtonCaption != null))
            {
                SetMovementLinksForButton("Quit", _activeButtonCaption, "", "Continue", "Skip Level");
            }

            base.ActivateButton(caption);
        }

        public override void Update(int millisecondsSinceLastUpdate)
        {
            base.Update(millisecondsSinceLastUpdate);

            if (_inputSourceIsConnected != _primaryInputSource.IsAvailable) 
            { 
                HandleConnectionStateChange(); 
                _inputSourceIsConnected = _primaryInputSource.IsAvailable; 
            }
        }

        private void HandleConnectionStateChange()
        {
            if (_primaryInputSource.IsAvailable) { SetInputSourceToPlayerController(_primaryInputSource); }
            else { InputSources = ControllerPool.Controllers.All; }
        }

        private const int Dialog_Height = 420;
    }
}
