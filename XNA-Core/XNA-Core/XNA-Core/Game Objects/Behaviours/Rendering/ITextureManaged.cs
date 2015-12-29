using Microsoft.Xna.Framework.Graphics;

namespace Leda.Core.Game_Objects.Behaviours
{
    public interface ITextureManaged : ISerializable
    {
        string TextureReference { get; }
        Texture2D Texture { set; }
    }
}
