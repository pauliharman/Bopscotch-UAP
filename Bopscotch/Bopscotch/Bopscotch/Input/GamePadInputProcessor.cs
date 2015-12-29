using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using Bopscotch.Gameplay.Objects.Display.Race;

namespace Bopscotch.Input
{
    public class GamePadInputProcessor : InputProcessorBase
    {
        private bool _actionHeld;
        private bool _selectHeld;

        private bool _moveUpHeld;
        private bool _moveDownHeld;
        private bool _moveLeftHeld;
        private bool _moveRightHeld;

        private bool _aButtonHeld;
        private bool _bButtonHeld;
        private bool _xButtonHeld;
        private bool _yButtonHeld;
        private bool _startButtonHeld;

        private PlayerIndex _playerIndex;

        public override InputProcessorType ProcessorType { get { return InputProcessorType.Gamepad; } }

        public GamePadInputProcessor(PlayerIndex playerIndex)
            : base()
        {
            _playerIndex = playerIndex;
            MenuMode = true;
            AllowDirectionalRepeat = false;
        }

        public override void Update(int millsecondsSinceLastUpdate)
        {
            ResetPublicProperties();

            GamePadState padState = GamePad.GetState(_playerIndex);
            IsAvailable = padState.IsConnected;

            if (IsAvailable)
            {
                if (MenuMode) { UpdateButtonsForMenuMode(padState); }
                else { UpdateButtonsForGameplayMode(padState); }
            }
        }

        protected override void ResetPublicProperties()
        {
            base.ResetPublicProperties();

            AButtonPressed = false;
            BButtonPressed = false;
            XButtonPressed = false;
            YButtonPressed = false;
            StartButtonPressed = false;
        }

        private void UpdateButtonsForMenuMode(GamePadState padState)
        {
            if ((padState.ThumbSticks.Left.Y > Data.Profile.Settings.ControlSensitivity) || (padState.DPad.Up == ButtonState.Pressed))
            {
                if ((!_moveUpHeld) || (AllowDirectionalRepeat)) { MoveUp = true; }
                _moveUpHeld = true;
            }
            else if ((padState.ThumbSticks.Left.Y <= Data.Profile.Settings.ControlSensitivity) && (padState.DPad.Up != ButtonState.Pressed))
            {
                _moveUpHeld = false;
            }

            if ((padState.ThumbSticks.Left.Y < -Data.Profile.Settings.ControlSensitivity) || (padState.DPad.Down == ButtonState.Pressed))
            {
                if ((!_moveDownHeld) || (AllowDirectionalRepeat)) { MoveDown = true; }
                _moveDownHeld = true;
            }
            else if ((padState.ThumbSticks.Left.Y >= -Data.Profile.Settings.ControlSensitivity) && (padState.DPad.Down != ButtonState.Pressed))
            {
                _moveDownHeld = false;
            }

            if (MoveUp && MoveDown) { MoveUp = false; MoveDown = false; }

            if ((padState.ThumbSticks.Left.X < -Data.Profile.Settings.ControlSensitivity) || (padState.DPad.Left == ButtonState.Pressed) || (padState.IsButtonDown(Buttons.LeftShoulder)))
            {
                if ((!_moveLeftHeld) || (AllowDirectionalRepeat)) { MoveLeft = true; }
                _moveLeftHeld = true;
            }
            else if ((padState.ThumbSticks.Left.X >= -Data.Profile.Settings.ControlSensitivity) && (padState.DPad.Left != ButtonState.Pressed) && (!padState.IsButtonDown(Buttons.LeftShoulder)))
            {
                _moveLeftHeld = false;
            }

            if ((padState.ThumbSticks.Left.X > Data.Profile.Settings.ControlSensitivity) || (padState.DPad.Right == ButtonState.Pressed) || (padState.IsButtonDown(Buttons.RightShoulder)))
            {
                if ((!_moveRightHeld) || (AllowDirectionalRepeat)) { MoveRight = true; }
                _moveRightHeld = true;
            }
            else if ((padState.ThumbSticks.Left.X <= Data.Profile.Settings.ControlSensitivity) && (padState.DPad.Right != ButtonState.Pressed) && (!padState.IsButtonDown(Buttons.RightShoulder)))
            {
                _moveRightHeld = false;
            }

            if (MoveLeft && MoveRight) { MoveLeft = false; MoveRight = false; }

            if (padState.Buttons.A == ButtonState.Pressed)
            {
                if (!_aButtonHeld) { AButtonPressed = true; SelectionTriggered = true; }
                _aButtonHeld = true;
            }
            else if (padState.Buttons.A != ButtonState.Pressed)
            {
                _aButtonHeld = false;
            }

            if (padState.Buttons.B == ButtonState.Pressed)
            {
                if (!_bButtonHeld) { BButtonPressed = true; }
                _bButtonHeld = true;
            }
            else if (padState.Buttons.B != ButtonState.Pressed)
            {
                _bButtonHeld = false;
            }

            if (padState.Buttons.X == ButtonState.Pressed)
            {
                if (!_xButtonHeld) { XButtonPressed = true; }
                _xButtonHeld = true;
            }
            else if (padState.Buttons.X != ButtonState.Pressed)
            {
                _xButtonHeld = false;
            }

            if (padState.Buttons.Y == ButtonState.Pressed)
            {
                if (!_yButtonHeld) { YButtonPressed = true; }
                _yButtonHeld = true;
            }
            else if (padState.Buttons.Y != ButtonState.Pressed)
            {
                _yButtonHeld = false;
            }

            if (padState.Buttons.Start == ButtonState.Pressed)
            {
                if (!_startButtonHeld) { StartButtonPressed = true; SelectionTriggered = true; }
                _startButtonHeld = true;
            }
            else if (padState.Buttons.Start != ButtonState.Pressed)
            {
                _startButtonHeld = false;
            }
        }

        private void UpdateButtonsForGameplayMode(GamePadState padState)
        {
            MoveUp = ((padState.ThumbSticks.Left.Y > Data.Profile.Settings.ControlSensitivity) || (padState.DPad.Up == ButtonState.Pressed));
            MoveDown = ((padState.ThumbSticks.Left.Y < -Data.Profile.Settings.ControlSensitivity) || (padState.DPad.Down == ButtonState.Pressed));
            if (MoveUp && MoveDown) { MoveUp = false; MoveDown = false; }

            MoveLeft = ((padState.ThumbSticks.Left.X < -Data.Profile.Settings.ControlSensitivity) || (padState.DPad.Left == ButtonState.Pressed));
            MoveRight = ((padState.ThumbSticks.Left.X > Data.Profile.Settings.ControlSensitivity) || (padState.DPad.Right == ButtonState.Pressed));
            if (MoveLeft && MoveRight) { MoveLeft = false; MoveRight = false; }

            if (((padState.Buttons.A == ButtonState.Pressed) || (padState.Buttons.B == ButtonState.Pressed)) && (!_actionHeld))
            {
                _actionHeld = true;
                ActionTriggered = true;
            }
            else if ((padState.Buttons.A == ButtonState.Released) && (padState.Buttons.B == ButtonState.Released))
            {
                _actionHeld = false;
            }

            if ((padState.Buttons.X == ButtonState.Pressed) || (padState.Buttons.Y == ButtonState.Pressed)) { HandlePowerUpControlTrigger(); }

            if (((padState.Buttons.A == ButtonState.Pressed) || (padState.Buttons.B == ButtonState.Pressed) || (padState.Buttons.Start == ButtonState.Pressed))
                && (!_selectHeld))
            {
                _selectHeld = true;
                SelectionTriggered = true;
            }
            else if ((padState.Buttons.A == ButtonState.Released) && (padState.Buttons.B == ButtonState.Released) && (padState.Buttons.Start == ButtonState.Released))
            {
                _selectHeld = false;
            }
        }

        private void HandlePowerUpControlTrigger()
        {
            if ((_inGameButtons.ContainsKey(PowerUpButton.In_Game_Button_Name)) && (_inGameButtons[PowerUpButton.In_Game_Button_Name].Active))
            {
                _inGameButtons[PowerUpButton.In_Game_Button_Name].Active = false;
                LastInGameButtonPressed = PowerUpButton.In_Game_Button_Name;
            }
        }
    }
}
