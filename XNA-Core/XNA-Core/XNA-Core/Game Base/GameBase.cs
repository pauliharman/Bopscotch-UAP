using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core.Timing;
using Leda.Core.Gamestate_Management;
using Leda.Core.Asset_Management;

#if WINDOWS_PHONE
using Microsoft.Phone.Shell;
#endif

namespace Leda.Core
{
    public class GameBase : Game
    {
        private static GameBase _instance = null;
        public static GameBase Instance { get { return _instance; } }

        public static Vector2 ScreenPosition(Vector2 worldSpacePosition) { return _instance.ConvertToScreenPosition(worldSpacePosition); }
        public static Vector2 ScreenPosition(float worldSpaceX, float worldSpaceY) { return _instance.ConvertToScreenPosition(worldSpaceX, worldSpaceY); }
        public static Vector2 WorldSpaceClipping { get { return _instance._resolutionOffset * _instance._resolutionScaling; } }
        public static float ScreenScale(float scale) { return _instance.ConvertToScreenScale(scale); }
        public static float ScreenScale() { return _instance.ConvertToScreenScale(1.0f); }
        public static Rectangle SafeDisplayArea { get { return _instance._safeDisplayArea; } }

		public static ScalingAxis DisplayControlAxis
		{
			set
			{
				_instance.SetResolutionMetrics(
					(int)_instance._unscaledBackBufferDimensions.X,
					(int)_instance._unscaledBackBufferDimensions.Y,
					value);
			}
		}

        private Dictionary<Type, Scene> _scenes;
        private Scene _currentScene;
        private string _tombstoneFileName;
        private string _sceneTransitionCrossFadeTextureName;

        public string TombstoneFileName { set { _tombstoneFileName = value; } }
        public int MillisecondsSinceLastUpdate { get { if (_currentScene != null) { return _currentScene.MillisecondsSinceLastUpdate; } else { return 0; } } }

        public bool EnsureAllContentIsVisible { get; set; }

        public Rectangle SceneBackBufferArea
        {
            set
            {
                foreach (KeyValuePair<Type, Scene> kvp in _scenes) { kvp.Value.ScaledBufferFrame = value; }
            }
        }

        public string SceneTransitionCrossFadeTextureName
        {
            set
            {
                _sceneTransitionCrossFadeTextureName = value;
                foreach (KeyValuePair<Type, Scene> kvp in _scenes) { kvp.Value.CrossFadeTextureName = _sceneTransitionCrossFadeTextureName; }
            }
        }
		protected Vector2 _unscaledBackBufferDimensions;
        protected Vector2 _resolutionOffset;
        protected float _resolutionScaling;
        protected Rectangle _safeDisplayArea;

		public GameBase(Orientation orientation)
            : base()
        {
            _instance = this;

            GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);

			if (orientation == Orientation.Portrait) 
			{
				graphics.SupportedOrientations = DisplayOrientation.Portrait;
#if IOS
				graphics.SupportedOrientations |= DisplayOrientation.PortraitDown;
#endif
			} 
            else 
            {
				graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
			}

			graphics.IsFullScreen = true;

            Content.RootDirectory = "Content";

			GlobalTimerController.ClearInstance();

            _scenes = new Dictionary<Type, Scene>();
            _currentScene = null;
            _tombstoneFileName = "";
        }

        private Vector2 ConvertToScreenPosition(Vector2 worldSpacePosition) { return (worldSpacePosition * _resolutionScaling) + _resolutionOffset; }
        private Vector2 ConvertToScreenPosition(float worldSpaceX, float worldSpaceY) { return (new Vector2(worldSpaceX, worldSpaceY) * _resolutionScaling) + _resolutionOffset; }
        private float ConvertToScreenScale(float scale) { return scale * _resolutionScaling; }

        protected override void Initialize()
        {
            MusicManager.Initialize();
            base.Initialize();
        }

        protected void AddScene(Scene toAdd)
        {
            if (toAdd.DeactivationHandler == null) { toAdd.DeactivationHandler = SceneTransitionHandler; }
            if (toAdd is AssetLoaderScene) { ((AssetLoaderScene)toAdd).LoadCompletionHandler = HandleAssetLoadCompletion; }

            if (!string.IsNullOrEmpty(_sceneTransitionCrossFadeTextureName)) { toAdd.CrossFadeTextureName = _sceneTransitionCrossFadeTextureName; }

            _scenes.Add(toAdd.GetType(), toAdd);
        }

        protected void SetResolutionMetrics(int optimumBackBufferWidth, int optimumBackBufferHeight, ScalingAxis scalingAxis)
        {
            SetResolutionMetrics(optimumBackBufferWidth, optimumBackBufferHeight, scalingAxis, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
        }

        protected void SetResolutionMetrics(int optimumBackBufferWidth, int optimumBackBufferHeight, ScalingAxis scalingAxis, int physicalScreenWidth, int physicalScreenHeight)
        {
            _unscaledBackBufferDimensions = new Vector2(optimumBackBufferWidth, optimumBackBufferHeight);

            if (scalingAxis == ScalingAxis.X)
            {
                _resolutionScaling = (float)physicalScreenWidth / (float)optimumBackBufferWidth;
                _resolutionOffset = new Vector2(0.0f, ((float)physicalScreenHeight - (optimumBackBufferHeight * _resolutionScaling)) / 2.0f);
            }
            else
            {
                _resolutionScaling = (float)physicalScreenHeight / (float)optimumBackBufferHeight;
                _resolutionOffset = new Vector2(((float)physicalScreenWidth - (optimumBackBufferWidth * _resolutionScaling)) / 2.0f, 0.0f);
            }

            //TouchProcessor.ResolutionScaling = _resolutionScaling;
            //TouchProcessor.ResolutionOffset = _resolutionOffset;

            _safeDisplayArea = new Rectangle(
                (int)Math.Max(-(_resolutionOffset.X / _resolutionScaling), 0),
                (int)Math.Max(-(_resolutionOffset.Y / _resolutionScaling), 0),
                (int)Math.Min(optimumBackBufferWidth + ((_resolutionOffset.X / _resolutionScaling) * 2.0f), optimumBackBufferWidth),
                (int)Math.Min(optimumBackBufferHeight + ((_resolutionOffset.Y / _resolutionScaling) * 2.0f), optimumBackBufferHeight));
        }

        protected void StartInitialScene(Type startingSceneType)
        {
            if ((_currentScene == null) && (_scenes.ContainsKey(startingSceneType)))
            {
                //if (FileManager.FileExists(_tombstoneFileName)) { startingSceneType = HandleTombstoneRecovery(startingSceneType); }
                SceneTransitionHandler(startingSceneType); 
            }
        }

        protected override void LoadContent()
        {
            base.LoadContent();
        }

        protected override bool BeginDraw()
        {
            if (_currentScene != null) { _currentScene.RenderContentToBackBuffer(); }

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            return base.BeginDraw();
        }

        private void SceneTransitionHandler(Type nextSceneType)
        {
            if (_scenes.ContainsKey(nextSceneType)) 
            {
                _currentScene = _scenes[nextSceneType];
                _scenes[nextSceneType].Activate(); 
            }
        }

        private void HandleAssetLoadCompletion(Type loaderSceneType)
        {
            foreach (KeyValuePair<Type, Scene> kvp in _scenes) { kvp.Value.HandleAssetLoadCompletion(loaderSceneType); }
        }
		public enum ScalingAxis
		{
			X,
			Y
		}
    }
}
