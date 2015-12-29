namespace Leda.Core.Game_Objects.Behaviours
{
    public interface ITransformationAnimatable : ISimpleRenderable
    {
        float Rotation { get; set; }
        float Scale { get; set; }
    }
}
