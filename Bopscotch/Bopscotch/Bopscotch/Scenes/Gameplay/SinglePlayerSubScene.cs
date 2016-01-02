using System;

using Microsoft.Xna.Framework;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Gamestate_Management;
using Leda.Core.Game_Objects.Controllers;
using Leda.Core.Game_Objects.Controllers.Collisions;
using Leda.Core.Timing;

using Bopscotch.Data;
using Bopscotch.Effects.Particles;
using Bopscotch.Effects.SmashBlockItems;
using Bopscotch.Gameplay;
using Bopscotch.Gameplay.Controllers;
using Bopscotch.Gameplay.Objects.Display;
using Bopscotch.Gameplay.Objects.Environment.Blocks;
using Bopscotch.Gameplay.Objects.Characters.Player;

namespace Bopscotch.Scenes.Gameplay
{
    public abstract class SinglePlayerSubScene : SubsceneBase
    {
        private MotionController _motionController;
        private AnimationController _animationController;
        private OneToManyCollisionController _playerCollisionController;
        private PauseController _pauseController;

        private LevelFactory _levelFactory;
        private SmashBlockItemFactory _smashBlockItemFactory;

        protected bool _active;
        protected TimerController _timerController;
        protected AdditiveLayerParticleEffectManager _additiveParticleEffectManager;
        protected OpaqueLayerParticleEffectManager _opaqueParticleEffectManager;
        protected PlayerTrackingCameraController _cameraController;
        protected Speedometer _speedometer;
        protected Player _player;
        protected LevelData _levelData;
        protected StatusDisplay _statusDisplay;
        protected PlayerEventPopup _playerEventPopup;

        protected string RaceAreaName { set { _levelFactory.RaceAreaName = value; } }

        public bool Paused { get { return _pauseController.Paused; } set { _pauseController.Paused = value; } }

        public Scene.DeactivationHandlerFunction DeactivationHandler { protected get; set; }

        public SinglePlayerSubScene(int backBufferWidth, int backBufferHeight)
            : base(backBufferWidth, backBufferHeight)
        {
            _motionController = new MotionController();
            _animationController = new AnimationController();
            _timerController = new TimerController();

            _pauseController = new PauseController();
            _pauseController.AddPausableObject(_timerController);
            _pauseController.AddPausableObject(_animationController);

            _cameraController = new Bopscotch.Gameplay.Controllers.PlayerTrackingCameraController();
            _cameraController.Viewport = new Rectangle(0, 0, backBufferWidth, backBufferHeight);
            _cameraController.ScrollBoundaryViewportFractions = new Vector2(Definitions.Horizontal_Scroll_Boundary_Fraction, Definitions.Vertical_Scroll_Boundary_Fraction);

            Renderer.ClipOffCameraRendering(_cameraController, Camera_Clipping_Margin);

            _playerCollisionController = new OneToManyCollisionController();

            _opaqueParticleEffectManager = new OpaqueLayerParticleEffectManager(_cameraController);
            _additiveParticleEffectManager = new AdditiveLayerParticleEffectManager(_cameraController);

            _levelFactory = new Bopscotch.Gameplay.LevelFactory(RegisterGameObject, _timerController.RegisterUpdateCallback);
            _levelFactory.BackgroundDimensions = new Point(backBufferWidth, backBufferHeight);

            _smashBlockItemFactory = new Effects.SmashBlockItems.SmashBlockItemFactory(RegisterGameObject, _timerController.RegisterUpdateCallback);

            _speedometer = new Bopscotch.Gameplay.Objects.Display.Speedometer();
            _speedometer.CenterPosition = new Vector2(backBufferWidth, 0.0f);
            _playerEventPopup = new PlayerEventPopup();
            _playerEventPopup.DisplayPosition = new Vector2(backBufferWidth / 2.0f, backBufferHeight / 4.0f);

            _active = false;
        }

        public override void Initialize()
        {
            base.Initialize();
            _speedometer.Initialize();
        }

        protected override void RegisterGameObject(IGameObject toRegister)
        {
            if (toRegister is IMobile) { _motionController.AddMobileObject((IMobile)toRegister); }
            if (toRegister is IAnimated) { _animationController.AddAnimatedObject((IAnimated)toRegister); }
            if (toRegister is ICameraLinked) { _cameraController.AddCameraLinkedObject((ICameraLinked)toRegister); }
            if (toRegister is ICollidable) { _playerCollisionController.AddCollidableObject((ICollidable)toRegister); }
            if (toRegister is IPausable) { _pauseController.AddPausableObject((IPausable)toRegister); }

            base.RegisterGameObject(toRegister);
        }

        protected virtual void HandleSmashBlockSmash(SmashBlock smashedBlock)
        {
            _opaqueParticleEffectManager.LaunchCrateSmash(smashedBlock);
            _smashBlockItemFactory.CreateItemsForSmashBlock(smashedBlock);
        }

        protected override void UnregisterGameObject(IGameObject toUnregister)
        {
            if (toUnregister is IMobile) { _motionController.RemoveMobileObject((IMobile)toUnregister); }
            if (toUnregister is IAnimated) { _animationController.RemoveAnimatedObject((IAnimated)toUnregister); }
            if (toUnregister is ICameraLinked) { _cameraController.RemoveCameraLinkedObject((ICameraLinked)toUnregister); }
            if (toUnregister is ICollidable) { _playerCollisionController.RemoveCollidableObject((ICollidable)toUnregister); }
            if (toUnregister is IPausable) { _pauseController.RemovePausableObject((IPausable)toUnregister); }

            base.UnregisterGameObject(toUnregister);
        }

        public virtual void Activate()
        {
            FlushGameObjects();
            _animationController.FlushAnimatedObjects();
            GC.Collect();

            RegisterGameObject(_levelFactory);
            RegisterGameObject(_opaqueParticleEffectManager);
            RegisterGameObject(_additiveParticleEffectManager);

            _pauseController.Paused = false;

            SetForNewLevelStart();

            _player = _levelFactory.Player;
            _player.PlayerEventCallback = HandlePlayerEvent;
            _player.CollisionController = _playerCollisionController;
            _player.DeathByFallingThreshold = _levelFactory.Map.MapWorldDimensions.Y;

            _cameraController.WorldDimensions = _levelFactory.Map.MapWorldDimensions;
            _cameraController.PlayerToTrack = _player;
            _cameraController.PositionForPlayStart();

            SetInterfaceDisplayObjectsForGame();
        }

        private void SetForNewLevelStart()
        {
            _levelFactory.AnimationController = _animationController;
            _levelFactory.SmashBlockCallback = HandleSmashBlockSmash;
            _levelFactory.SmashBlockRegenerationCallback = _additiveParticleEffectManager.LaunchCloudBurst;
            _levelFactory.BombBlockDetonationCallback = _additiveParticleEffectManager.LaunchFireball;
            _levelFactory.LoadAndInitializeLevel();

            if (Data.Profile.PlayingRaceMode) { ((Data.RaceLevelData)_levelData).LapsToComplete = _levelFactory.RaceLapCount; }
        }

        protected virtual void HandlePlayerEvent()
        {
            switch (_player.LastEvent)
            {
                case Player.PlayerEvent.Died:
                    _additiveParticleEffectManager.LaunchCloudBurst(_player);
                    _playerEventPopup.StartPopupForEvent(_player.LastEvent);
                    break;
            }
        }

        protected virtual void HandlePlayerEventAnimationComplete()
        {
        }

        protected virtual void SetInterfaceDisplayObjectsForGame()
        {
            _speedometer.PlayerMotionEngine = (PlayerMotionEngine)_player.MotionEngine;
            _speedometer.Reset();
            RegisterGameObject(_speedometer);

            _statusDisplay.Position = Vector2.Zero;
            RegisterGameObject(_statusDisplay);

            RegisterGameObject(_playerEventPopup);
        }

        public virtual void CompleteActivation()
        {
            _active = true;
        }

        public override void Update(int millisecondsSinceLastUpdate)
        {
            base.Update(millisecondsSinceLastUpdate);

            _motionController.Update(millisecondsSinceLastUpdate);
            _cameraController.Update(millisecondsSinceLastUpdate);
            _timerController.Update(millisecondsSinceLastUpdate);

            if (!_pauseController.Paused)
            {
                _opaqueParticleEffectManager.Update(millisecondsSinceLastUpdate);
                _additiveParticleEffectManager.Update(millisecondsSinceLastUpdate);
                _animationController.Update(millisecondsSinceLastUpdate);
                _playerCollisionController.CheckForCollisions();
            }
        }

        private const int Camera_Clipping_Margin = 160;
    }
}
