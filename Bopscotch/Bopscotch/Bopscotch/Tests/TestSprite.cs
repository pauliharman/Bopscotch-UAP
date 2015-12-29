using System.Xml.Linq;

using Microsoft.Xna.Framework;

using Leda.Core.Game_Objects.Base_Classes;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Animation;
using Leda.Core.Asset_Management;
using Leda.Core.Serialization;

namespace Bopscotch.Tests
{
    public class TestSprite : StorableSimpleDrawableObject, IAnimated
    {
        private SpriteSheetAnimationEngine _animationEngine;

        public IAnimationEngine AnimationEngine { get { return _animationEngine; } }

        public TestSprite()
        {
            ID = "test-sprite";
            WorldPosition = new Vector2(400.0f, 200.0f);
            RenderLayer = 0;
            RenderDepth = 0.5f;
            Visible = true;

            _animationEngine = new SpriteSheetAnimationEngine(this);
        }

        public override void Initialize()
        {
            base.Initialize();

            TextureReference = "flag-goal";
            Texture = TextureManager.Textures["flag-goal"];
            Frame = new Rectangle(0, 0, 80, 60);
            Origin = Vector2.Zero;

            _animationEngine.Sequence = AnimationDataManager.Sequences["flag"];
        }

        protected override XElement Serialize(Serializer serializer)
        {
            base.Serialize(serializer);

            serializer.AddDataItem("animation-engine", _animationEngine);

            return serializer.SerializedData;
        }

        protected override Serializer Deserialize(Serializer serializer)
        {
            serializer.KnownSerializedObjects.Add(_animationEngine);

            base.Deserialize(serializer);

            _animationEngine = serializer.GetDataItem<SpriteSheetAnimationEngine>("animation-engine");

            return serializer;
        }
    }
}
