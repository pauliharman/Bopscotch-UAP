using Leda.Core.Effects.Particles;

namespace Leda.Core.Game_Objects.Behaviours
{
    public interface IHasParticleEffects
    {
        ParticleController.ParticleRegistrationHandler ParticleRegistrationCallback { set; }
    }
}
