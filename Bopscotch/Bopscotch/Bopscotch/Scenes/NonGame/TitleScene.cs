using System;

using Microsoft.Xna.Framework;
using Leda.Core.Asset_Management;
using Leda.Core.Game_Objects.Controllers;
using Leda.Core.Game_Objects.Behaviours;

using Bopscotch.Input;
using Bopscotch.Scenes.BaseClasses;
using Bopscotch.Interface;
using Bopscotch.Interface.Dialogs.TitleScene;
using Bopscotch.Interface.Content;
using Bopscotch.Effects.Popups;

namespace Bopscotch.Scenes.NonGame
{
    public class TitleScene : MenuDialogScene
    {
        private AnimationController _animationController;
        private string _firstDialog;
        private string _musicToStartOnDeactivation;
        private NewContentUnlockedDialog _unlockNotificationDialog;

        private PopupRequiringDismissal _titlePopup;
        //private BackgroundSnow _snowController;

        public TitleScene()
            : base()
        {
            _animationController = new AnimationController();

            _titlePopup = new PopupRequiringDismissal();
            _titlePopup.AnimationCompletionHandler = HandlePopupAnimationComplete;
            RegisterGameObject(_titlePopup);

            //_snowController = new BackgroundSnow();
            //RegisterGameObject(_snowController);

            _unlockNotificationDialog = new NewContentUnlockedDialog();

            _dialogs.Add("main", new MainMenuDialog());
            _dialogs.Add("start", new StartMenuDialog());
            _dialogs.Add("survival-levels", new SurvivalStartCarouselDialog(RegisterGameObject, UnregisterGameObject));
            _dialogs.Add("characters", new CharacterSelectionCarouselDialog(RegisterGameObject, UnregisterGameObject));
            _dialogs.Add("options", new OptionsDialog());
            _dialogs.Add("reset-areas", new ResetAreasConfirmDialog());
            _dialogs.Add("areas-reset", new ResetAreasCompleteDialog());
            _dialogs.Add("unlocks", _unlockNotificationDialog);

            _backgroundTextureName = Background_Texture_Name;

            RegisterGameObject(
                new TextContent(Translator.Translation("Leda Entertainment Presents"), new Vector2(Definitions.Back_Buffer_Center.X, 60.0f))
                {
                    RenderLayer = 2,
                    RenderDepth = 0.5f,
                    Scale = 0.65f
                });
        }

        private void HandlePopupAnimationComplete()
        {
            if (_titlePopup.AwaitingDismissal) { ActivateDialog(_firstDialog); }
            else { Deactivate(); }
        }

        public override void HandleAssetLoadCompletion(Type loaderSceneType)
        {
            _titlePopup.TextureReference = Title_Texture_Name;
            //_snowController.CreateSnowflakes();

            _dialogs["main"].ExitCallback = HandleMainDialogActionSelection;
            _dialogs["start"].ExitCallback = HandleStartDialogActionSelection;
            _dialogs["survival-levels"].ExitCallback = HandleLevelSelectDialogSelection;
            _dialogs["characters"].ExitCallback = HandleCharacterSelectDialogSelection;
            _dialogs["options"].ExitCallback = HandleOptionsDialogClose;
            _dialogs["reset-areas"].ExitCallback = HandleResetAreasConfirmDialogClose;
            _dialogs["areas-reset"].ExitCallback = HandleConfirmationDialogClose;
            _dialogs["unlocks"].ExitCallback = HandleConfirmationDialogClose;

            base.HandleAssetLoadCompletion(loaderSceneType);
        }

        private void HandleMainDialogActionSelection(string selectedOption)
        {
            switch (selectedOption)
            {
                case "Start!": ActivateDialog("start"); break;
                case "Character": ActivateDialog("characters"); break;
                case "About": NextSceneType = typeof(CreditsScene); Deactivate(); break;
                case "Options": ActivateDialog("options"); break;
                case "Display": NextSceneType = typeof(DisplayCalibrationScene); Deactivate(); break;
                case "Quit": ExitGame(); break;
            }
        }

        private void ExitGame()
        {
            DeactivationHandler = DeactivateForExit;
            NextSceneType = this.GetType();
            Deactivate();
        }

        private void DeactivateForExit(Type deactivationHandlerRequiredParameter)
        {
            MusicManager.StopMusic();
            Game.Exit();
        }

        protected override void CompleteDeactivation()
        {
            if (!string.IsNullOrEmpty(_musicToStartOnDeactivation)) { MusicManager.PlayLoopedMusic(_musicToStartOnDeactivation); }

            base.CompleteDeactivation();
        }

        private void HandleStartDialogActionSelection(string selectedOption)
        {
            switch (selectedOption)
            {
                case "Adventure":
                    Data.Profile.PlayingRaceMode = false;
                    ControllerPool.SetPlayerOneController(_dialogs["start"].ActuatingController);
                    ActivateDialog("survival-levels");
                    break;
                case "Race":
                    Data.Profile.PlayingRaceMode = true;
                    ControllerPool.SetPlayerOneController(_dialogs["start"].ActuatingController);
                    ControllerPool.SetPlayerTwoController(null);
                    NextSceneType = typeof(Gameplay.Race.RaceStartScene);
                    _titlePopup.Dismiss();
                    break;
                case "Back":
                    ActivateDialog("main");
                    break;
            }
        }

        private void HandleLevelSelectDialogSelection(string selectedOption)
        {
            switch (selectedOption)
            {
                case "Start!":
                    NextSceneType = typeof(Gameplay.Survival.SurvivalGameplayScene);
                    _musicToStartOnDeactivation = "survival-gameplay";
                    _titlePopup.Dismiss();
                    break;
                case "Back":
                    ActivateDialog("start");
                    break;
            }
        }

        private void HandleCharacterSelectDialogSelection(string selectedOption)
        {
            switch (selectedOption)
            {
                case "Back":
                    ActivateDialog("main");
                    break;
                case "Select":
                    UpdateSelectedCharacter();
                    ActivateDialog("main");
                    break;
                case "Edit":
                    UpdateSelectedCharacter();
                    NextSceneType = typeof(AvatarCustomisationScene);
                    _musicToStartOnDeactivation = "avatar-build";
                    Deactivate();
                    break;
            }
        }

        private void HandleContentDialogClose(string selectedOption)
        {
            _titlePopup.Activate();
        }

        private void HandleOptionsDialogClose(string selectedOption)
        {
            if (selectedOption == "Reset Game") { ActivateDialog("reset-areas"); }
            else { ActivateDialog("main"); }
        }

        private void HandleResetAreasConfirmDialogClose(string selectedOption)
        {
            if (selectedOption == "Confirm") { Data.Profile.ResetAreas(); ActivateDialog("areas-reset"); }
            else { ActivateDialog("options"); }
        }

        private void HandleConfirmationDialogClose(string selectedOption)
        {
            ActivateDialog("main");
        }

        private void UpdateSelectedCharacter()
        {
            Data.Profile.Settings.SelectedAvatarSlot = ((CharacterSelectionCarouselDialog)_dialogs["characters"]).SelectedAvatarSkinSlot;
            Data.Profile.Save();
        }

        protected override void RegisterGameObject(IGameObject toRegister)
        {
            if (toRegister is IAnimated) { _animationController.AddAnimatedObject((IAnimated)toRegister); }
            base.RegisterGameObject(toRegister);
        }

        public override void Activate()
        {
            ControllerPool.SetControllersToMenuMode();

            if (!NextSceneParameters.Get<bool>("music-already-running")) { MusicManager.PlayLoopedMusic("title"); }

            _musicToStartOnDeactivation = "";

            base.Activate();
        }

        protected override void CompleteActivation()
        {
            _firstDialog = NextSceneParameters.Get<string>(First_Dialog_Parameter_Name);
            if (string.IsNullOrEmpty(_firstDialog)) { _firstDialog = Default_First_Dialog; }

            if (!Definitions.Simulate_Trial_Mode) { UnlockFullVersionContent(); }

            _titlePopup.Activate();

            base.CompleteActivation();
        }

        private void UnlockFullVersionContent()
        {
            _unlockNotificationDialog.PrepareForActivation();

            if ((Data.Profile.AreaIsLocked("Waterfall")) && (Data.Profile.AreaHasBeenCompleted("Hilltops")))
            {
                Data.Profile.UnlockNamedArea("Waterfall");
                _unlockNotificationDialog.AddItem("New Levels - Waterfall Area");
            }

            if (!Data.Profile.AvatarCostumeUnlocked("Angel"))
            {
                Data.Profile.UnlockCostume("Angel");
                Data.Profile.UnlockCostume("Wizard");
                Data.Profile.UnlockCostume("Mummy");
                _unlockNotificationDialog.AddItem("New Costumes - Angel, Wizard, Mummy");
            }

            if (_unlockNotificationDialog.HasContent) { _firstDialog = "unlocks"; }
        }

        public override void Update(GameTime gameTime)
        {
            _animationController.Update(MillisecondsSinceLastUpdate);
            //_snowController.Update(MillisecondsSinceLastUpdate);

            base.Update(gameTime);
        }

        protected override void HandleBackButtonPress()
        {
            if ((!_titlePopup.AwaitingDismissal) && (CurrentState != Status.Deactivating)) { ExitGame(); }

            base.HandleBackButtonPress();
        }

        protected override void Render()
        {
            base.Render();
            SpriteBatch.Begin();

            SpriteBatch.End();
        }

        private const string Background_Texture_Name = "background-1";
        private const string Title_Texture_Name = "popup-title";
        private const string Default_First_Dialog = "main";

        public const string First_Dialog_Parameter_Name = "first-dialog";
    }
}
