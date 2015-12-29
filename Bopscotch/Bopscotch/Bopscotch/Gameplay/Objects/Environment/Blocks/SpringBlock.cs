using System.Xml.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core.Game_Objects.Base_Classes;
using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Asset_Management;
using Leda.Core.Animation;
using Leda.Core.Serialization;

namespace Bopscotch.Gameplay.Objects.Environment.Blocks
{
    public class SpringBlock : Block, IAnimated, ISpriteSheetAnimatable
    {
        private SpriteSheetAnimationEngine _animationEngine;
        public IAnimationEngine AnimationEngine { get { return _animationEngine; } }

        public SpringBlock()
            : base()
        {
            TextureReference = Texture_Name;
            Texture = TextureManager.Textures[Texture_Name];
            Frame = new Rectangle(0, 0, Definitions.Grid_Cell_Pixel_Size, TextureManager.Textures[Texture_Name].Height);
            Origin = new Vector2(-Horizontal_Offset, -Vertical_Offset);

            _animationEngine = new SpriteSheetAnimationEngine(this);
        }

        public void TriggerLaunchAnimation()
        {
            _animationEngine.Sequence = AnimationDataManager.Sequences[Launch_Animation_Sequence_Name];
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

        private const string Texture_Name = "block-spring";
        private const string Launch_Animation_Sequence_Name = "spring-block-launch";

        public new const string Data_Node_Name = "spring-block";
        public const float Horizontal_Offset = -1.0f;
        public const float Vertical_Offset = -82.0f;
    }
}
