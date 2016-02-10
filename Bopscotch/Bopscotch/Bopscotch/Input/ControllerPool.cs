using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace Bopscotch.Input
{
    public class ControllerPool : GameComponent
    {
        private static ControllerPool _instance;
        public static ControllerPool Controllers { get { return _instance; } }

        public static void CreateForGame(Game game) { _instance = new ControllerPool(game); }
        public static void SetPlayerOneController(InputProcessorBase controller) { _instance.SetControllerForPlayer(PlayerIndex.One, controller); }
        public static void SetPlayerTwoController(InputProcessorBase controller) { _instance.SetControllerForPlayer(PlayerIndex.Two, controller); }
        public static void SetControllersToMenuMode() { _instance.SetControllerMenuModeFlags(true); }
        public static void SetControllersToGameplayMode() { _instance.SetControllerMenuModeFlags(false); }

        private List<InputProcessorBase> _controllers;
        private int _playerOneController;
        private int _playerTwoController;

        private TimeSpan _lastUpdateTime;
        private int _lastUpdateDuration;

        public List<InputProcessorBase> All { get { return _controllers; } }
        public InputProcessorBase PlayerOne { get { return _playerOneController > -1 ? _controllers[_playerOneController] : null; } }
        public InputProcessorBase PlayerTwo { get { return _playerTwoController > -1 ? _controllers[_playerTwoController] : null; } }

        public List<InputProcessorBase> AllButPlayerOne
        {
            get
            {
                List<InputProcessorBase> allButPlayerOne = new List<InputProcessorBase>();
                for (int i = 0; i < _controllers.Count; i++)
                {
                    if (i != _playerOneController) { allButPlayerOne.Add(_controllers[i]); }
                }
                return allButPlayerOne;
            }
        }

        public ControllerPool(Game game)
            : base(game)
        {
            _playerOneController = -1;
            _playerTwoController = -1;

            CreatePlayerControllers();

            game.Components.Add(this);
        }

        private void CreatePlayerControllers()
        {
            _controllers = new List<InputProcessorBase>();
            foreach (PlayerController pc in Enum.GetValues(typeof(PlayerController))) { _controllers.Add(CreatePlayerController(pc)); }
        }

        private InputProcessorBase CreatePlayerController(PlayerController method)
        {
            switch (method)
            {
                case PlayerController.KeyboardOne: return KeyboardInputProcessor.CreateForPlayerOne(); break;
                case PlayerController.KeyboardTwo: return KeyboardInputProcessor.CreateForPlayerTwo(); break;
                case PlayerController.GamepadOne: return new GamePadInputProcessor(PlayerIndex.One); break;
                case PlayerController.GamepadTwo: return new GamePadInputProcessor(PlayerIndex.Two); break;
                case PlayerController.GamepadThree: return new GamePadInputProcessor(PlayerIndex.Three); break;
                case PlayerController.GamepadFour: return new GamePadInputProcessor(PlayerIndex.Four); break;
                case PlayerController.Touch: return new DragTapControls(); break;
            }

            return null;
        }

        public override void Update(GameTime gameTime)
        {
            if (_lastUpdateTime != TimeSpan.Zero)
            {
                TimeSpan difference = (gameTime.TotalGameTime - _lastUpdateTime);
                _lastUpdateDuration = difference.Milliseconds + (difference.Seconds / 1000);
            }

            for (int i = 0; i < _controllers.Count; i++) { _controllers[i].Update(_lastUpdateDuration); }

            base.Update(gameTime);

            _lastUpdateTime = gameTime.TotalGameTime;
        }

        private void SetControllerForPlayer(PlayerIndex player, InputProcessorBase controller)
        {
            int controllerIndex = -1;
            for (int i = 0; i < _controllers.Count; i++)
            {
                if (_controllers[i] == controller) { controllerIndex = i; break; }
            }

            switch (player)
            {
                case PlayerIndex.One: _playerOneController = controllerIndex; break;
                case PlayerIndex.Two: _playerTwoController = controllerIndex; break;
            }
        }

        private void SetControllerMenuModeFlags(bool menuModeOn)
        {
            for (int i = 0; i < _controllers.Count; i++)
            {
                if (_controllers[i] is GamePadInputProcessor) { ((GamePadInputProcessor)_controllers[i]).MenuMode = menuModeOn; }
            }
        }

        private enum PlayerController
        {
            KeyboardOne,
            KeyboardTwo,
            GamepadOne,
            GamepadTwo,
            GamepadThree,
            GamepadFour,
            Touch
        }
    }
}
