using Microsoft.Xna.Framework;

using Leda.Core.Game_Objects.Base_Classes;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Asset_Management;
using Leda.Core.Animation;

namespace Bopscotch.Tests
{
    public class TestFlag : DisposableSimpleDrawableObject, ISpriteSheetAnimatable, IAnimated
    {
        private SpriteSheetAnimationEngine _animationEngine;
        public IAnimationEngine AnimationEngine { get { return _animationEngine; } }

        public TestFlag()
            : base()
        {
            RenderLayer = 0;
            Visible = true;

            _animationEngine = new SpriteSheetAnimationEngine(this);
        }

        public void Initialize()
        {
            Texture = TextureManager.Textures["flag-restart"];
            Frame = new Rectangle(0, 0, 80, 60);
            WorldPosition = new Vector2(100.0f, 100.0f);

            _animationEngine.Sequence = AnimationDataManager.Sequences["flag"];
        }
    }
}
