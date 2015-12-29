using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Game_Objects.Controllers.Rendering;
using Leda.Core.Gamestate_Management;

using Bopscotch.Gameplay.Objects.Behaviours;

namespace Bopscotch.Scenes.Gameplay
{
    public abstract class SubsceneBase : ISimpleRenderable
    {
        private RenderController _renderController;

        private RenderTarget2D _backBuffer;
        protected Vector2 _bufferDimensions;
        private Rectangle _bufferSourceArea;

        private List<IGameObject> _gameObjects;
        private List<ITemporary> _temporaryObjects;
        private List<ICanHaveGlowEffect> _objectWithGlowEffect;

        public bool Visible { get { return true; } set { } }
        public virtual int RenderLayer { get { return Render_Layer; } set { } }
        public Rectangle BufferArea { protected get; set; }

        protected RenderController Renderer { get { return _renderController; } }
        protected SceneParameters NextSceneParameters { get { return SceneParameters.Instance; } }
        protected Vector2 BufferCenter { get { return _bufferDimensions / 2.0f; } }

        protected Rectangle _safeDisplayArea;
        public Rectangle SafeAreaOuterLimits
        {
            set
            {
                _safeDisplayArea.X = value.X + (int)(value.Width * Data.Profile.Settings.DisplaySafeAreaFraction);
                _safeDisplayArea.Y = value.Y + (int)(value.Height * Data.Profile.Settings.DisplaySafeAreaFraction);
                _safeDisplayArea.Width = value.Width - (int)(value.Width * (Data.Profile.Settings.DisplaySafeAreaFraction * 2));
                _safeDisplayArea.Height = value.Height - (int)(value.Height * (Data.Profile.Settings.DisplaySafeAreaFraction * 2));
            }
        }

        public SubsceneBase(int bufferWidth, int bufferHeight)
        {
            _renderController = new RenderController();

            _backBuffer = null;
            _bufferDimensions = new Vector2(bufferWidth, bufferHeight);

            _gameObjects = new List<IGameObject>();
            _temporaryObjects = new List<ITemporary>();
            _objectWithGlowEffect = new List<ICanHaveGlowEffect>();

            Visible = false;
            BufferArea = new Rectangle(0, 0, bufferWidth, bufferHeight);
            _safeDisplayArea = new Rectangle(0, 0, bufferWidth, bufferHeight);
        }

        public abstract void CreateBackBuffer();

        protected void CreateBackBuffer(int width, int height, bool useMipMaps)
        {
            _bufferSourceArea = new Rectangle(0, 0, width, height);

            _backBuffer = new RenderTarget2D(
                GameBase.Instance.GraphicsDevice,
                width,
                useMipMaps ? width : height,
                useMipMaps,
                SurfaceFormat.Color,
                DepthFormat.None,
                4,
                RenderTargetUsage.DiscardContents);
        }

        protected virtual void RegisterGameObject(IGameObject toRegister)
        {
            if (!_gameObjects.Contains(toRegister)) { _gameObjects.Add(toRegister); }
            if (toRegister is ITemporary) { _temporaryObjects.Add((ITemporary)toRegister); }
            if (toRegister is ISimpleRenderable) { _renderController.AddRenderableObject((ISimpleRenderable)toRegister); }
            if (toRegister is ICanHaveGlowEffect) { _objectWithGlowEffect.Add((ICanHaveGlowEffect)toRegister); }
        }

        protected virtual void UnregisterGameObject(IGameObject toUnregister)
        {
            if (_gameObjects.Contains(toUnregister)) { _gameObjects.Remove(toUnregister); }
            if (toUnregister is ITemporary) { _temporaryObjects.Remove((ITemporary)toUnregister); }
            if (toUnregister is ISimpleRenderable) { _renderController.RemoveRenderableObject((ISimpleRenderable)toUnregister); }
            if (toUnregister is ICanHaveGlowEffect) { _objectWithGlowEffect.Remove((ICanHaveGlowEffect)toUnregister); }
        }

        protected void FlushGameObjects()
        {
            for (int i = _gameObjects.Count - 1; i >= 0; i--) { UnregisterGameObject(_gameObjects[i]); }
        }

        public List<IGameObject> GameObjects(Type toGet)
        {
            List<IGameObject> objects = new List<IGameObject>();
            for (int i = 0; i < _gameObjects.Count; i++)
            {
                if (_gameObjects[i].GetType() == toGet) { objects.Add(_gameObjects[i]); }
            }
            return objects;
        }

        public virtual void Initialize()
        {
            for (int i = 0; i < _gameObjects.Count; i++) { _gameObjects[i].Initialize(); }
        }

        public virtual void Update(int millisecondsSinceLastUpdate)
        {
            for (int i = 0; i < _objectWithGlowEffect.Count; i++) { _objectWithGlowEffect[i].UpdateGlow(millisecondsSinceLastUpdate); }

            RemoveDisposedObjects();
        }

        public void RenderContentToBackBuffer(SpriteBatch spriteBatch)
        {
            if (_backBuffer == null) { CreateBackBuffer(); }

            GameBase.Instance.GraphicsDevice.SetRenderTarget(_backBuffer);
            GameBase.Instance.GraphicsDevice.BlendState = BlendState.Opaque;
            GameBase.Instance.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GameBase.Instance.GraphicsDevice.Clear(Color.Transparent);

            Render(spriteBatch);
        }

        protected virtual void Render(SpriteBatch spriteBatch)
        {
            _renderController.RenderObjects(spriteBatch);
        }

        private void RemoveDisposedObjects()
        {
            for (int i = _temporaryObjects.Count - 1; i >= 0; i--)
            {
                if (_temporaryObjects[i].ReadyForDisposal)
                {
                    _temporaryObjects[i].PrepareForDisposal();
                    UnregisterGameObject((IGameObject)_temporaryObjects[i]); 
                }
            }
        }

        public void Reset()
        {
            for (int i = 0; i < _gameObjects.Count; i++) { _gameObjects[i].Reset(); }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_backBuffer != null) { spriteBatch.Draw(_backBuffer, BufferArea, _bufferSourceArea, Color.White); }
        }

        private const int Render_Layer = 0;
    }
}
