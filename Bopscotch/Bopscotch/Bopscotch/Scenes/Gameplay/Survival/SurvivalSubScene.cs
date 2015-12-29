
using Microsoft.Xna.Framework;

using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Asset_Management;

using Bopscotch.Scenes.NonGame;
using Bopscotch.Data;
using Bopscotch.Input;
using Bopscotch.Gameplay.Objects.Display.Survival;
using Bopscotch.Gameplay.Objects.Characters.Player;
using Bopscotch.Gameplay.Objects.Environment.Blocks;
using Bopscotch.Gameplay.Objects.Environment.Collectables;
using Bopscotch.Effects.Popups;
using Bopscotch.Interface.Dialogs;
using Bopscotch.Interface.Dialogs.SurvivalGameplayScene;

namespace Bopscotch.Scenes.Gameplay.Survival
{
    public class SurvivalSubScene : SinglePlayerSubScene
    {
        private Input.InputProcessorBase _inputProcessor;
        private PopupRequiringDismissal _readyPopup;
        private PauseDialog _pauseDialog;
        private ControllerUnpluggedDialog _controllerDialog;
        private TutorialRunner _tutorialRunner;

        public Vector2 CameraOverspillMargin { set { _cameraController.Overspill = value; } }
        public bool SceneIsDeactivating { private get; set; }

        private SurvivalLevelData LevelData { get { return (SurvivalLevelData)_levelData; } }
        private SurvivalDataDisplay StatusDisplay { get { return (SurvivalDataDisplay)_statusDisplay; } set { _statusDisplay = value; } }

        public SurvivalSubScene()
            : base(Definitions.Back_Buffer_Width, Definitions.Back_Buffer_Height)
        {
            StatusDisplay = new SurvivalDataDisplay();

            _readyPopup = new PopupRequiringDismissal();
            _pauseDialog = new PauseDialog();
            _controllerDialog = new ControllerUnpluggedDialog();
            _tutorialRunner = new TutorialRunner();

            _pauseDialog.ExitCallback = HandleDialogClose;
            _controllerDialog.ExitCallback = HandleDialogClose;

            _playerEventPopup.AnimationCompletionHandler = HandlePlayerEventAnimationComplete;
        }

        public override void CreateBackBuffer()
        {
            CreateBackBuffer((int)_bufferDimensions.X, (int)_bufferDimensions.Y, false);
        }

        private void HandleDialogClose(string selectedOption)
        {
            switch (selectedOption)
            {
                case "Back": UnpauseIfNotDisplayingTutorialStep(); break;
                case "Continue": UnpauseIfNotDisplayingTutorialStep(); break;
                case "Skip Level": HandleLevelSkip(); break;
                case "Quit": DeactivationHandler(typeof(TitleScene)); break;
            }
        }

        private void UnpauseIfNotDisplayingTutorialStep()
        {
            if (!_tutorialRunner.DisplayingHelp) { Paused = false; _inputProcessor.MenuMode = false; }
        }

        private void HandleLevelSkip()
        {
            if ((_readyPopup.Visible) && ( !_readyPopup.BeingDismissed)) { _readyPopup.Dismiss(); }
            Profile.GoldenTickets--;
            _player.TriggerLevelSkip();
            Paused = false;
        }

        protected override void HandlePlayerEventAnimationComplete()
        {
            switch (_player.LastEvent)
            {
                case Player.PlayerEvent.Died: RefreshScene(); break;
                case Player.PlayerEvent.Goal_Passed: HandleLevelCleared(); break;
            }
        }

        private void RefreshScene()
        {
            DeactivationHandler(typeof(SurvivalGameplayScene));
        }

        private void HandleLevelCleared()
        {
            StatusDisplay.FreezeDisplayedScore = true;
            Profile.CurrentAreaData.UpdateCurrentLevelScore(LevelData.PointsScoredThisLevel);
            Profile.CurrentAreaData.StepToNextLevel();
            Profile.Save();

            if (Profile.CurrentAreaData.Completed) { CompleteArea(); }
            else { RefreshScene(); }
        }

        private void CompleteArea()
        {
            Profile.UnlockCurrentAreaContent();
            DeactivationHandler(typeof(SurvivalAreaCompleteScene));
        }

        protected override void RegisterGameObject(IGameObject toRegister)
        {
            base.RegisterGameObject(toRegister);

            if (toRegister is Collectable) { ((Collectable)toRegister).CollectionCallback = HandleCollectableCollection; }
        }

        private void HandleCollectableCollection(Collectable collectedItem)
        {
            LevelData.UpdateFromItemCollection(collectedItem);
        }

        public override void Activate()
        {
            _inputProcessor = ControllerPool.Controllers.PlayerOne;
            _pauseDialog.SetInputSourceToPlayerController(_inputProcessor);
            //_pauseDialog.SkipLevelButtonDisabled = false;
            _controllerDialog.SetInputSourcesToPlayerControllers();

            _levelData = new Data.SurvivalLevelData();

            StatusDisplay.CurrentLevelData = LevelData;
            StatusDisplay.FreezeDisplayedScore = false;
            SceneIsDeactivating = false;
            RaceAreaName = "";

            base.Activate();

            Paused = Profile.PauseOnSceneActivation;
            if (Paused) { ActivatePauseDialog(); }
            Profile.PauseOnSceneActivation = false;

            _player.InputProcessor = _inputProcessor;
            ((PlayerMotionEngine)_player.MotionEngine).DifficultySpeedBoosterUnit = Profile.CurrentAreaData.SpeedStep;
            _readyPopup.Activate();
        }

        protected override void HandlePlayerEvent()
        {
            switch (_player.LastEvent)
            {
                case Player.PlayerEvent.Goal_Passed:
                    //_pauseDialog.SkipLevelButtonDisabled = true;
                    _playerEventPopup.StartPopupForEvent(Player.PlayerEvent.Goal_Passed);
                    SoundEffectManager.PlayEffect("level-clear");
                    break;
            }

            base.HandlePlayerEvent();
        }

        protected override void SetInterfaceDisplayObjectsForGame()
        {
            base.SetInterfaceDisplayObjectsForGame();

            SetStatusAndTutorialDisplays();

            _readyPopup.TextureReference = Ready_Popup_Texture;
            RegisterGameObject(_readyPopup);

            RegisterGameObject(_pauseDialog);
            RegisterGameObject(_controllerDialog);
        }

        private void SetStatusAndTutorialDisplays()
        {
            if (Profile.CurrentAreaData.Name == "Tutorial")
            {
                _tutorialRunner.Initialize();
                _tutorialRunner.DisplayArea = new Rectangle(0, 0, Definitions.Back_Buffer_Width, Definitions.Back_Buffer_Height);
                _tutorialRunner.PauseTrigger = HoldForTutorialStep;
                _tutorialRunner.Visible = true;
                RegisterGameObject(_tutorialRunner);

                _player.TutorialStepTrigger = _tutorialRunner.CheckForStepTrigger;
                _statusDisplay.Visible = false;
            }
            else
            {
                _statusDisplay.Position = Vector2.Zero;
                _statusDisplay.Visible = true;
            }
        }

        public override void Update(int millisecondsSinceLastUpdate)
        {
            base.Update(millisecondsSinceLastUpdate);

            if (_inputProcessor.ActionTriggered) { HandleActionTrigger(); }

            UpdateScore(millisecondsSinceLastUpdate);

            if (!_inputProcessor.IsAvailable) { HandleControllerUnplugged(millisecondsSinceLastUpdate); }
        }

        private void HandleActionTrigger()
        {
            if (!Paused && _readyPopup.AwaitingDismissal) { BeginPlay(); }

            if (Paused && !_pauseDialog.Visible && !_controllerDialog.Visible && _tutorialRunner.DisplayingHelp && _tutorialRunner.StepCanBeDismissed)
            { 
                Paused = false; 
                _tutorialRunner.ClearCurrentStep(); 
            }
        }

        private void BeginPlay()
        {
            _readyPopup.Dismiss();
            _player.SetForMovement();
            _levelData.CurrentPlayState = Data.LevelData.PlayState.InPlay;
        }

        private void UpdateScore(int millisecondsSinceLastUpdate)
        {
            if ((!Paused) && (_player.CanMoveHorizontally))
            {
                LevelData.UpdateScoreForMovement(millisecondsSinceLastUpdate, ((PlayerMotionEngine)_player.MotionEngine).Speed);
            }
        }

        protected override void HandleSmashBlockSmash(SmashBlock smashedBlock)
        {
            LevelData.UpdateFromSmashBlockContents(smashedBlock);
            base.HandleSmashBlockSmash(smashedBlock);
        }

        public void HandlebackButtonPress()
        {
            if (!_controllerDialog.Visible)
            {
                if (!Paused || (_tutorialRunner.DisplayingHelp && !_pauseDialog.Active))
                {
                    if (SceneIsDeactivating) { Profile.PauseOnSceneActivation = true; }
                    else { EnablePause(); }
                }
                else if (_pauseDialog.Active)
                {
                    _pauseDialog.Cancel();
                }
            }
            else if (_inputProcessor.IsAvailable)
            {
                UnpauseIfNotDisplayingTutorialStep();
            }
        }

        private void EnablePause()
        {
            Paused = true;
            ActivatePauseDialog();
        }

        private void ActivatePauseDialog()
        {
            _pauseDialog.SkipLevelButtonDisabled = ((Profile.GoldenTickets < 1) || (Profile.CurrentAreaData.Name == "Tutorial"));
            _pauseDialog.Activate();
        }

        private void HoldForTutorialStep()
        {
            Paused = true;
        }

        private void HandleControllerUnplugged(int millisecondsSinceLastUpdate)
        {
            if (!_pauseDialog.Visible)
            {
                if (!_controllerDialog.Visible)
                {
                    Paused = true;
                    _controllerDialog.Activate();
                    _inputProcessor.MenuMode = true;
                }
                else
                {
                    _controllerDialog.Update(millisecondsSinceLastUpdate);
                }
            }
        }

        private const string Ready_Popup_Texture = "popup-get-ready";
    }
}