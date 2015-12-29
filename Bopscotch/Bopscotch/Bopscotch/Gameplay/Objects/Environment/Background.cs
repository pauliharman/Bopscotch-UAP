using System.Xml.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leda.Core.Game_Objects.Behaviours;
using Leda.Core.Serialization;
using Leda.Core.Asset_Management;

namespace Bopscotch.Gameplay.Objects.Environment
{
    public class Background : ISimpleRenderable, ISerializable
    {
        protected Color _tint;
        protected float _renderDepth;

        public string ID { get; set; }
        public string TextureReference { get; set; }
        public bool Visible { get; set; }
        public int RenderLayer { get; set; }
        public Rectangle TargetArea { protected get; set; }

        public Background()
        {
            _tint = Color.White;
            _renderDepth = Render_Depth;

            ID = "background";
            RenderLayer = Render_Layer;
            Visible = true;

            TargetArea = new Rectangle(0, 0, Definitions.Back_Buffer_Width, Definitions.Back_Buffer_Height);
        }

        public void Initialize()
        {
        }

        public virtual void Reset()
        {
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(TextureManager.Textures[TextureReference], TargetArea, null, _tint, 0.0f, Vector2.Zero, SpriteEffects.None, _renderDepth);
        }

        public XElement Serialize()
        {
            Serializer serializer = new Serializer(this);

            serializer.AddDataItem("texture-name", TextureReference);
            serializer.AddDataItem("target-area", TargetArea);

            return serializer.SerializedData;
        }

        public void Deserialize(XElement serializedData)
        {
            Serializer serializer = new Serializer(serializedData);

            TextureReference = serializer.GetDataItem<string>("texture-name");
            TargetArea = serializer.GetDataItem<Rectangle>("target-area");
        }

        private const int Render_Layer = 0;
        private const float Render_Depth = 0.9f;
    }
}
