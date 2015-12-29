using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

using Leda.Core.Gamestate_Management;
using Leda.Core.Asset_Management;
using Leda.Core.Timing;

using Bopscotch.Data.Avatar;

namespace Bopscotch.Interface.Dialogs.RaceFinishScene
{
    public class ResultsDialog : ButtonDialog
    {
        private Scene.ObjectRegistrationHandler _registerObject;

        private ResultsAvatar _playerOne;
        private ResultsAvatar _playerTwo;
        private Timer _resultsAnnouncementTimer;
        private ResultsPopup _outcomePopup;
        private Effects.GlowBurst _glowBurst;
        private SoundEffectInstance _resultsSoundInstance;

        public Definitions.RaceOutcome Outcome { private get; set; }
        public int PlayerOneSkinSlotIndex { private get; set; }
        public int PlayerTwoSkinSlotIndex { private get; set; }

        public ResultsDialog(Scene.ObjectRegistrationHandler objectRegistrationHandler)
            : base()
        {
            _registerObject = objectRegistrationHandler;

            _boxCaption = Translator.Translation("Race Results");

            Height = Dialog_Height;
            TopYWhenActive = Top_Y_When_Active;

            AddButton("Exit", new Vector2(Definitions.Left_Button_Column_X, 625), Button.ButtonIcon.B, Color.Red, 0.7f);

            _defaultButtonCaption = "Exit";
            _cancelButtonCaption = "Exit";

            _resultsAnnouncementTimer = new Timer("", AnnounceResults);
            GlobalTimerController.GlobalTimer.RegisterUpdateCallback(_resultsAnnouncementTimer.Tick);
        }

        private void AnnounceResults()
        {
            if (Active)
            {
                switch (Outcome)
                {
                    case Definitions.RaceOutcome.PlayerOneWin:
                        _playerOne.AnimationEngine.Sequence = AnimationDataManager.Sequences["player-front-win"];
                        _playerTwo.AnimationEngine.Sequence = AnimationDataManager.Sequences["player-front-lose"];
                        _glowBurst.Position = _playerOne.WorldPosition;
                        _glowBurst.Visible = true;
                        SoundEffectManager.PlayEffect("race-winner");
                        break;
                    case Definitions.RaceOutcome.PlayerTwoWin:
                        _playerTwo.AnimationEngine.Sequence = AnimationDataManager.Sequences["player-front-win"];
                        _playerOne.AnimationEngine.Sequence = AnimationDataManager.Sequences["player-front-lose"];
                        _glowBurst.Position = _playerTwo.WorldPosition;
                        _glowBurst.Visible = true;
                        SoundEffectManager.PlayEffect("race-winner");
                        break;
                    case Definitions.RaceOutcome.Incomplete:
                        _playerOne.AnimationEngine.Sequence = AnimationDataManager.Sequences["player-front-lose"];
                        _playerTwo.AnimationEngine.Sequence = AnimationDataManager.Sequences["player-front-lose"];
                        SoundEffectManager.PlayEffect("race-loser");
                        break;
                }

                _outcomePopup.Activate();
            }
        }

        public void InitializeComponents()
        {
            _playerOne = CreatePlayerSkeleton(-Player_Offset_From_Center);
            _playerTwo = CreatePlayerSkeleton(Player_Offset_From_Center);

            _outcomePopup = new ResultsPopup();
            _registerObject(_outcomePopup);

            _glowBurst = new Effects.GlowBurst(3, 1.0f, 0.1f);
            _glowBurst.RenderDepth = 0.095f;
        }

        private ResultsAvatar CreatePlayerSkeleton(float offsetFromCenter)
        {
            ResultsAvatar avatar = new ResultsAvatar();
            avatar.OffsetFromCenter = offsetFromCenter;
            avatar.CreateBonesFromDataManager(Definitions.Avatar_Skeleton_Front);

            _registerObject(avatar);

            return avatar;
        }

        public override void Activate()
        {
            base.Activate();

            _playerOne.SkinBones(AvatarComponentManager.FrontFacingAvatarSkin(PlayerOneSkinSlotIndex));
            _playerOne.StartRestingAnimationSequence();

            _playerTwo.SkinBones(AvatarComponentManager.FrontFacingAvatarSkin(PlayerTwoSkinSlotIndex));
            _playerTwo.StartRestingAnimationSequence();

            _outcomePopup.PlayerOneSkinSlotIndex = PlayerOneSkinSlotIndex;
            _outcomePopup.PlayerTwoSkinSlotIndex = PlayerTwoSkinSlotIndex;
            _outcomePopup.Outcome = Outcome;
            _outcomePopup.Reset();

            _glowBurst.Visible = false;

            SynchroniseComponentsWithDialog();

            _resultsAnnouncementTimer.NextActionDuration = Delay_Before_Result_Announcement_In_Milliseconds;
            _resultsSoundInstance = SoundEffectManager.PlayEffect("race-results");
        }

        private void SynchroniseComponentsWithDialog()
        {
            _playerOne.ParentDialogY = WorldPosition.Y;
            _playerTwo.ParentDialogY = WorldPosition.Y;

            _outcomePopup.ParentDialogY = WorldPosition.Y;

            if (_glowBurst.Visible) { _glowBurst.Position = new Vector2(_glowBurst.Position.X, _playerOne.WorldPosition.Y); }
        }

        public override void Update(int millisecondsSinceLastUpdate)
        {
            base.Update(millisecondsSinceLastUpdate);
            SynchroniseComponentsWithDialog();

            _glowBurst.Update(millisecondsSinceLastUpdate);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            _playerOne.Draw(spriteBatch);
            _playerTwo.Draw(spriteBatch);

            _glowBurst.Draw(spriteBatch);
        }

        protected override void Dismiss()
        {
            if (_resultsSoundInstance != null) { _resultsSoundInstance.Stop(); }
            base.Dismiss();
        }

        private const float Top_Y_When_Active = 100.0f;
        private const int Dialog_Height = 700;
        private const float Player_Offset_From_Center = 300.0f;
        private const int Delay_Before_Result_Announcement_In_Milliseconds = 3000;
    }
}
