using Microsoft.Xna.Framework;

namespace Leda.Core.Game_Objects.Behaviours
{
    public interface IColourAnimatable : ISimpleRenderable
    {
        Color Tint { get; set; }
    }
}
