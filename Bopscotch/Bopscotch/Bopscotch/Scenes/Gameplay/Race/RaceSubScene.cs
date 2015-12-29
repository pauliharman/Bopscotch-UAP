
using Microsoft.Xna.Framework;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Asset_Management;
using Leda.Core.Timing;
using Bopscotch.Data.Avatar;
using Bopscotch.Gameplay.Coordination;
using Bopscotch.Gameplay.Objects.Characters.Player;
using Bopscotch.Gameplay.Objects.Display.Race;
using Bopscotch.Gameplay.Objects.Environment;
using Bopscotch.Gameplay.Objects.Environment.Blocks;

namespace Bopscotch.Scenes.Gameplay.Race
{
    public class RaceSubScene : SinglePlayerSubScene
    {
        public delegate void RaceStartHandler();

        private CountdownPopup _countdownPopup;
        private RaceInfoPopup _positionStatusPopup;
        private RaceInfoPopup _raceEventPopup;
        private bool _raceStarted;
        private PowerUpButton _powerUpButton;
        private PowerUpTimer _powerUpDisplayTimer;
        private PowerUpHelper _powerUpHelper;
        private Blackout _blackout;
        private Timer _exitTimer;

        private RaceProgressCoordinator _progressCoordinator;
        private RacePowerUpCoordinator _powerUpCoordinator;

        public int PlayerSkinSlotIndex { get; set; }
        public Input.InputProcessorBase InputProcessor { private get; set; }
        public RaceStartHandler RaceStartCallback { private get; set; }
        public Communication.ICommunicator Communicator { get; set; }

        public bool ReadyToRace { get; private set; }
        public bool InputSourceLost { get; private set; }
        public bool AllLapsCompleted { get { return (_progressCoordinator.LapsCompleted >= LevelData.LapsToComplete); } }
        public int RaceTimeInMilliseconds { get { return _progressCoordinator.TotalRaceTimeElapsedInMilliseconds; } }

        private Data.RaceLevelData LevelData { get { return (Data.RaceLevelData)_levelData; } }
        private RaceDataDisplay StatusDisplay { get { return (RaceDataDisplay)_statusDisplay; } set { _statusDisplay = value; } }

        public RaceSubScene()
            : base(Definitions.IsWideScreen ? Wide_Buffer_Width : Standard_Buffer_Width, Buffer_Height)
        {
            StatusDisplay = new RaceDataDisplay();
            Communicator = null;

            _countdownPopup = new CountdownPopup();
            _powerUpButton = new PowerUpButton();
            _playerEventPopup.AnimationCompletionHandler = HandlePlayerEventAnimationComplete;

            _powerUpDisplayTimer = new PowerUpTimer();
            _powerUpDisplayTimer.TickCallback = _timerController.RegisterUpdateCallback;
            _powerUpHelper = new PowerUpHelper();
            _blackout = new Blackout();
            _blackout.TickCallback = _timerController.RegisterUpdateCallback;

            _positionStatusPopup = new RaceInfoPopup();
            _raceEventPopup = new RaceInfoPopup();

            _exitTimer = new Timer("", HandleExitTimerActionComplete);
            GlobalTimerController.GlobalTimer.RegisterUpdateCallback(_exitTimer.Tick);
        }

        public override void CreateBackBuffer()
        {
            CreateBackBuffer((int)_bufferDimensions.X, (int)_bufferDimensions.Y, false);
        }

        protected override void HandlePlayerEventAnimationComplete()
        {
            switch (_player.LastEvent)
            {
                case Player.PlayerEvent.Died: ResurrectPlayer(); break;
            }
        }

        private void HandleExitTimerActionComplete()
        {
            DeactivationHandler(typeof(RaceFinishScene));
        }

        public override void Initialize()
        {
            int bufferWidth = Definitions.IsWideScreen ? Wide_Buffer_Width : Standard_Buffer_Width;

            base.Initialize();

            _powerUpButton.Initialize();
            _powerUpButton.Center = new Vector2(bufferWidth - _powerUpButton.Radius, (_speedometer.Origin.X * 2) + _powerUpButton.Radius);

            _powerUpDisplayTimer.Initialize();
            _powerUpDisplayTimer.DisplayTopRight = new Vector2(bufferWidth, 0.0f);

            _countdownPopup.DisplayPosition = new Vector2(bufferWidth / 2.0f, Buffer_Height * 0.25f);
            _raceEventPopup.DisplayPosition = new Vector2(bufferWidth / 2.0f, Buffer_Height * 0.25f);
            _positionStatusPopup.DisplayPosition = new Vector2(bufferWidth / 2.0f, Buffer_Height - Position_Status_Popup_Bottom_Margin);
        }

        protected override void RegisterGameObject(IGameObject toRegister)
        {
            if (toRegister is RaceModePowerUpSmashBlock) 
            {
                ((RaceModePowerUpSmashBlock)toRegister).RegenerationParticleEffect = _additiveParticleEffectManager.LaunchCloudBurst; 
            }

            base.RegisterGameObject(toRegister);
        }

        public override void Activate()
        {
            SafeAreaOuterLimits = new Rectangle(0, 0, Definitions.IsWideScreen ? Wide_Buffer_Width : Standard_Buffer_Width, Buffer_Height);

            _raceStarted = false;
            _levelData = new Data.RaceLevelData();

            RaceAreaName = NextSceneParameters.Get<string>(Race.RaceGameplayScene.Course_Area_Parameter);

            base.Activate();

            ((PlayerMotionEngine)_player.MotionEngine).DifficultySpeedBoosterUnit = NextSceneParameters.Get<int>(Race.RaceGameplayScene.Course_Speed_Parameter);

            _player.ClearSkin();
            _player.SkinBones(AvatarComponentManager.SideFacingAvatarSkin(PlayerSkinSlotIndex));
            _player.CustomSkinSlotIndex = PlayerSkinSlotIndex;

            if (InputProcessor != null) 
            { 
                ((PlayerMotionEngine)_player.MotionEngine).InputProcessor = InputProcessor;
                InputProcessor.AddButtonArea(PowerUpButton.In_Game_Button_Name, _powerUpButton.Center, _powerUpButton.Radius, false);
            }

            SetCoordinatorsForRace();
            SetUpOpponentAttackEffects();

            ReadyToRace = false;
            Paused = false;
        }

        protected override void SetInterfaceDisplayObjectsForGame()
        {
            base.SetInterfaceDisplayObjectsForGame();

            _powerUpButton.Reset();
            RegisterGameObject(_powerUpButton);

            _powerUpDisplayTimer.Reset();
            RegisterGameObject(_powerUpDisplayTimer);

            _powerUpHelper.Reset();
            _powerUpHelper.MotionLine = BufferCenter.X;
            RegisterGameObject(_powerUpHelper);

            _countdownPopup.Reset();
            RegisterGameObject(_countdownPopup);

            _raceEventPopup.Reset();
            RegisterGameObject(_raceEventPopup);

            _positionStatusPopup.Reset();
            RegisterGameObject(_positionStatusPopup);
        }

        private void SetCoordinatorsForRace()
        {
            _powerUpCoordinator = new RacePowerUpCoordinator();
            _powerUpCoordinator.Player = _player;
            _powerUpCoordinator.DisplayTimer = _powerUpDisplayTimer;

            _progressCoordinator = new RaceProgressCoordinator();
            _progressCoordinator.Communicator = Communicator;
            _progressCoordinator.Player = _player;
            _progressCoordinator.StatusPopup = _positionStatusPopup;
            _progressCoordinator.StatusDisplay = StatusDisplay;
            _progressCoordinator.LapsToComplete = LevelData.LapsToComplete;
            _progressCoordinator.SetRestartPoint();

            _timerController.RegisterUpdateCallback(_progressCoordinator.SequenceTimerTick);
            RegisterGameObject(_progressCoordinator);

            Communicator.OwnPlayerData = _progressCoordinator;
        }

        private void SetUpOpponentAttackEffects()
        {
            _blackout.Reset();
            RegisterGameObject(_blackout);
        }

        protected override void HandlePlayerEvent()
        {
            switch (_player.LastEvent)
            {
                case Player.PlayerEvent.Died: 
                    ResetPowerUpDisplayFollowingPlayerDeath(); 
                    break;
                case Player.PlayerEvent.Restart_Point_Touched:
                    SoundEffectManager.PlayEffect("race-checkpoint");
                    _opaqueParticleEffectManager.LaunchFlagStars(_player.LastRaceRestartPointTouched);
                    _progressCoordinator.CheckAndUpdateRestartPoint();
                    break;
                case Player.PlayerEvent.Restart_Point_Changed_Direction:
                    _raceEventPopup.StartPopupForRaceInfo("popup-race-wrong-way");
                    break;
                case Player.PlayerEvent.Goal_Passed:
                    _progressCoordinator.CheckAndUpdateRestartPoint();
                    HandleLapCompleted();
                    break;
            }

            base.HandlePlayerEvent();
        }

        private void ResetPowerUpDisplayFollowingPlayerDeath()
        {
            if (_powerUpButton.Visible) { _powerUpButton.Deactivate(); }
            if (_powerUpDisplayTimer.Visible) { _powerUpDisplayTimer.Deactivate(); }
            if (_powerUpHelper.Visible) { _powerUpHelper.Dismiss(); }
        }

        private void HandleLapCompleted()
        {
            if (_progressCoordinator.LapCompleted())
            {
                SoundEffectManager.PlayEffect("generic-fanfare");
                if (AllLapsCompleted) { HandleRaceGoalAchieved(); }

                _opaqueParticleEffectManager.LaunchFlagStars(_player.LastRaceRestartPointTouched);
                if (_progressCoordinator.LapsCompleted == LevelData.LapsToComplete) { _raceEventPopup.StartPopupForRaceInfo("popup-race-goal"); }
                else if (_progressCoordinator.LapsCompleted + 1 == LevelData.LapsToComplete) { _raceEventPopup.StartPopupForRaceInfo("popup-race-last-lap"); }
            }
        }

        private void HandleRaceGoalAchieved()
        {
            if (_exitTimer.CurrentActionProgress == 1.0f) { _exitTimer.NextActionDuration = Exit_Sequence_Duration_In_Milliseconds; }
            _player.CanMoveHorizontally = false;
            _player.IsExitingLevel = true;
            _progressCoordinator.CompleteRace();
        }

        private void ResurrectPlayer()
        {
            _progressCoordinator.ResurrectPlayerAtLastRestartPoint();
            _cameraController.PositionForPlayStart();
        }

        public override void Update(int millisecondsSinceLastUpdate)
        {
            InputSourceLost = !InputProcessor.IsAvailable;
            if (!string.IsNullOrEmpty(InputProcessor.LastInGameButtonPressed)) { HandleInGameButtonPress(); }

            base.Update(millisecondsSinceLastUpdate);

            if ((!_raceStarted) && (_cameraController.LockedOntoStartPoint) && (!_countdownPopup.Running)) { ReadyToRace = true; RaceStartCallback(); }

            _progressCoordinator.Update(millisecondsSinceLastUpdate);
            if (_progressCoordinator.OpponentAttackPending) { SetUpOpponentAttack(); }

            HandleCommunications();
        }

        private void SetUpOpponentAttack()
        {
            SoundEffectManager.PlayEffect("opponent-attack");
            if (!_player.IsDead) { _additiveParticleEffectManager.LaunchEnemyAttack(_player); }

            switch (_progressCoordinator.NextOpponentAttackPowerUp)
            {
                case Definitions.PowerUp.Shades: _blackout.Activate(); break;
                case Definitions.PowerUp.Shell: _player.SetActivePowerUp(Definitions.PowerUp.Shell); break;
                case Definitions.PowerUp.Horn: _player.SetActivePowerUp(Definitions.PowerUp.Horn); break;
            }

            _progressCoordinator.SetLastOpponentAttackTime();
        }

        private void HandleCommunications()
        {
            Communicator.Update();

            if (!Communicator.ConnectionLost) { CheckAndHandleOpponentUpdates(); }
            else { HandleCommunicationLoss(); }
        }

        private void CheckAndHandleOpponentUpdates()
        {
            if ((_exitTimer.CurrentActionProgress == 1.0f) && (Communicator.OtherPlayerData.LapsCompleted >= LevelData.LapsToComplete))
            {
                _exitTimer.NextActionDuration = Exit_Sequence_Duration_In_Milliseconds -
                    (Communicator.OtherPlayerData.TotalRaceTimeElapsedInMilliseconds - Communicator.OtherPlayerData.LastCheckpointTimeInMilliseconds);
            }
        }

        private void HandleCommunicationLoss()
        {
        }

        private void HandleInGameButtonPress()
        {
            if ((InputProcessor.LastInGameButtonPressed == PowerUpButton.In_Game_Button_Name) && (_powerUpButton.Visible)) 
            {
                SoundEffectManager.PlayEffect("power-up");
                _opaqueParticleEffectManager.LaunchPowerUpStars(_player, _powerUpCoordinator.CurrentPowerUpAttacksOpponent);
                if (_powerUpCoordinator.CurrentPowerUpAttacksOpponent) { _progressCoordinator.StartAttackSequence(_powerUpCoordinator.AvailablePowerUp); }
                _powerUpCoordinator.ActivateAvailablePowerUp();
                _powerUpButton.Deactivate();
                _powerUpHelper.Dismiss();
            }
        }

        public void StartCountdown()
        {
             _progressCoordinator.StartTimerToBeginningOfRace();
            _countdownPopup.Activate();
        }

        protected override void HandleSmashBlockSmash(SmashBlock smashedBlock)
        {
            if (smashedBlock.Contents.Count > 0)
            {
                _powerUpButton.IconTexture = smashedBlock.Contents[0].TextureName;
                _powerUpCoordinator.SetAvailablePowerUpFromTexture(smashedBlock.Contents[0].TextureName);

                if (Data.Profile.Settings.ShowPowerUpHelpers)
                {
                    _powerUpHelper.SetHelpText(smashedBlock.Contents[0].TextureName);
                    _powerUpHelper.Activate();
                }

                InputProcessor.ActivateButton(PowerUpButton.In_Game_Button_Name);
            }

            base.HandleSmashBlockSmash(smashedBlock);
        }

        private const int Wide_Buffer_Width = 2048;
        private const int Standard_Buffer_Width = 1536;
        private const int Buffer_Height = 720;

        private const int Exit_Sequence_Duration_In_Milliseconds = 3500;
        private const float Position_Status_Popup_Bottom_Margin = 100.0f;
    }
}
