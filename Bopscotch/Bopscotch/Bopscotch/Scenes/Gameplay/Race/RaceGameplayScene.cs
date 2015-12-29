using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.GamerServices;

using Leda.Core;
using Leda.Core.Gamestate_Management;
using Leda.Core.Asset_Management;
using Leda.Core.Game_Objects.Controllers;
using Leda.Core.Timing;

using Bopscotch.Input;

namespace Bopscotch.Scenes.Gameplay.Race
{
    public class RaceGameplayScene : Scene
    {
        private RaceSubScene _playerOneGameplayContainer;
        private RaceSubScene _playerTwoGameplayContainer;
        private RaceDialogContainerSubScene _dialogContainer;

        private int _startCoundown;
        private Timer _startSequenceTimer;

        public RaceGameplayScene()
            : base(Definitions.Back_Buffer_Width, Definitions.Back_Buffer_Height)
        {
            _playerOneGameplayContainer = CreateRaceContainer();
            _playerTwoGameplayContainer = CreateRaceContainer();

            _dialogContainer = new RaceDialogContainerSubScene();
            _dialogContainer.DialogCloseHandler = HandleDialogClose;
            RegisterGameObject(_dialogContainer);

            _startSequenceTimer = new Timer("", HandleStartCountdownStep);
            GlobalTimerController.GlobalTimer.RegisterUpdateCallback(_startSequenceTimer.Tick);
        }

        private RaceSubScene CreateRaceContainer()
        {
            RaceSubScene container = new RaceSubScene();
            container.RaceStartCallback = SubsceneReadyCallback;
            container.DeactivationHandler = SubsceneDeactivationHandler;
            RegisterGameObject(container);

            return container;
        }

        private void SubsceneReadyCallback()
        {
            if ((_playerOneGameplayContainer.ReadyToRace) && (_playerTwoGameplayContainer.ReadyToRace))
            {
                _playerOneGameplayContainer.StartCountdown();
                _playerTwoGameplayContainer.StartCountdown();

                _startCoundown = 3;
                _startSequenceTimer.NextActionDuration = 501;
            }
        }

        private void HandleStartCountdownStep()
        {
            if (_startCoundown-- > 0) { SoundEffectManager.PlayEffect("race-countdown"); _startSequenceTimer.NextActionDuration = 1001; }
            else { SoundEffectManager.PlayEffect("race-start"); MusicManager.PlayLoopedMusic("race-gameplay"); }
        }

        public void SubsceneDeactivationHandler(Type nextSceneType)
        {
            if (CurrentState == Status.Active)
            {
                NextSceneParameters.Set(RaceFinishScene.Outcome_Parameter_Name, RaceOutcome);
                NextSceneParameters.Set(Player_One_Avatar_Skin_Parameter, _playerOneGameplayContainer.PlayerSkinSlotIndex);
                NextSceneParameters.Set(Player_Two_Avatar_Skin_Parameter, _playerTwoGameplayContainer.PlayerSkinSlotIndex);
                NextSceneType = nextSceneType;
                Deactivate();
            }
        }

        protected override void CompleteDeactivation()
        {
            MusicManager.StopMusic();
            base.CompleteDeactivation();
            _startSequenceTimer.Reset();

            ControllerPool.SetControllersToMenuMode();
        }

        public void HandleDialogClose(string selectedOption)
        {
            switch (selectedOption)
            {
                case "Back": SetPauseState(false); break;
                case "Cancel": SetPauseState(false); break;
                case "Confirm": SubsceneDeactivationHandler(typeof(RaceFinishScene)); break;
            }
        }

        private void SetPauseState(bool isPaused)
        {
            _playerOneGameplayContainer.Paused = isPaused;
            _playerTwoGameplayContainer.Paused = isPaused;
            _startSequenceTimer.ActionSpeed = isPaused ? 0 : 1;
        }

        private Definitions.RaceOutcome RaceOutcome
        {
            get
            {
                if (_playerOneGameplayContainer.AllLapsCompleted && _playerTwoGameplayContainer.AllLapsCompleted)
                {
                    if (_playerOneGameplayContainer.RaceTimeInMilliseconds < _playerTwoGameplayContainer.RaceTimeInMilliseconds)
                    {
                        return Definitions.RaceOutcome.PlayerOneWin;
                    }
                    else if (_playerOneGameplayContainer.RaceTimeInMilliseconds > _playerTwoGameplayContainer.RaceTimeInMilliseconds)
                    {
                        return Definitions.RaceOutcome.PlayerTwoWin;
                    }
                    else
                    {
                        return Definitions.RaceOutcome.Draw;
                    }
                }
                else if (_playerOneGameplayContainer.AllLapsCompleted) { return Definitions.RaceOutcome.PlayerOneWin; }
                else if (_playerTwoGameplayContainer.AllLapsCompleted) { return Definitions.RaceOutcome.PlayerTwoWin; }

                return Definitions.RaceOutcome.Incomplete;
            }
        }

        public override void HandleAssetLoadCompletion(Type loaderSceneType)
        {
            base.HandleAssetLoadCompletion(loaderSceneType);
            InitializeGameObjects();
        }

        public override void Activate()
        {
            base.Activate();

            _startSequenceTimer.Reset();

            ControllerPool.SetControllersToGameplayMode();

            Communication.SubSceneCommunicator playerOneCommunicator = new Communication.SubSceneCommunicator();
            Communication.SubSceneCommunicator playerTwoCommunicator = new Communication.SubSceneCommunicator();

            playerOneCommunicator.OtherPlayerDataSource = playerTwoCommunicator;
            playerTwoCommunicator.OtherPlayerDataSource = playerOneCommunicator;

            _playerOneGameplayContainer.InputProcessor = ControllerPool.Controllers.PlayerOne;
            _playerOneGameplayContainer.Communicator = playerOneCommunicator;
            _playerOneGameplayContainer.PlayerSkinSlotIndex = NextSceneParameters.Get<int>(Player_One_Avatar_Skin_Parameter);
            _playerOneGameplayContainer.BufferArea = CreateDisplayArea(0);
            _playerOneGameplayContainer.Activate();

            _playerTwoGameplayContainer.InputProcessor = ControllerPool.Controllers.PlayerTwo;
            _playerTwoGameplayContainer.Communicator = playerTwoCommunicator;
            _playerTwoGameplayContainer.PlayerSkinSlotIndex = NextSceneParameters.Get<int>(Player_Two_Avatar_Skin_Parameter);
            _playerTwoGameplayContainer.BufferArea = CreateDisplayArea(1);
            _playerTwoGameplayContainer.Activate();

            _dialogContainer.BufferArea = ScaledBufferFrame;
            _dialogContainer.Activate();
        }

        private Rectangle CreateDisplayArea(float offset)
        {
            //float x = Definitions.Back_Buffer_Width * Data.Profile.Settings.DisplaySafeAreaFraction;
            //float y = Definitions.Back_Buffer_Height * Data.Profile.Settings.DisplaySafeAreaFraction;
            //float width = (Definitions.Back_Buffer_Width * (1.0f - Data.Profile.Settings.DisplaySafeAreaFraction)) - x;
            //float height = (Definitions.Back_Buffer_Height * (1.0f - (Data.Profile.Settings.DisplaySafeAreaFraction * 2.0f))) / 2.0f;

            //y += (height * offset);

            //return new Rectangle((int)(x + Data.Profile.Settings.DisplaySafeAreaTopLeft.X), (int)(y + Data.Profile.Settings.DisplaySafeAreaTopLeft.Y), (int)width, (int)height);

            return new Rectangle(ScaledBufferFrame.X, ScaledBufferFrame.Y + (( ScaledBufferFrame.Height / 2) * (int)offset), ScaledBufferFrame.Width, ScaledBufferFrame.Height / 2);
        }

        protected override void CompleteActivation()
        {
            base.CompleteActivation();
            _playerOneGameplayContainer.CompleteActivation();
            _playerTwoGameplayContainer.CompleteActivation();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _playerOneGameplayContainer.Update(MillisecondsSinceLastUpdate);
            _playerTwoGameplayContainer.Update(MillisecondsSinceLastUpdate);
            _dialogContainer.Update(MillisecondsSinceLastUpdate);

            if ((_playerOneGameplayContainer.InputSourceLost) || (_playerTwoGameplayContainer.InputSourceLost)) { HandleControllerUnplugged(); }
        }

        protected override void BeginRender()
        {
            _playerOneGameplayContainer.RenderContentToBackBuffer(SpriteBatch);
            _playerTwoGameplayContainer.RenderContentToBackBuffer(SpriteBatch);

            _dialogContainer.RenderContentToBackBuffer(SpriteBatch);

            base.BeginRender();
        }

        protected override void HandleBackButtonPress()
        {
            if (CurrentState == Status.Active)
            {
                if ((!_dialogContainer.DisplayingControllerDialog) && (!_playerOneGameplayContainer.Paused))
                {
                    _dialogContainer.HandleBackButtonPress(false);
                    SetPauseState(true);
                }
                else
                {
                    _dialogContainer.HandleBackButtonPress(true);
                }
            }
        }

        private void HandleControllerUnplugged()
        {
            if ((CurrentState == Status.Active) && (!_dialogContainer.DisplayingQuitRaceDialog) && (!_playerOneGameplayContainer.Paused))
            {
                SetPauseState(true);
                _dialogContainer.HandleControllerUnplugged();
            }
        }

        public const int Container_Split_Margin = 5;
        public const string Player_One_Avatar_Skin_Parameter = "player-one-avatar-skin";
        public const string Player_Two_Avatar_Skin_Parameter = "player-two-avatar-skin";
        public const string Course_Area_Parameter = "course-area-name";
        public const string Course_Speed_Parameter = "course-speed";
    }
}
