using StructureMap;

namespace ItineraryBuilder.IOC
{
    public class Bootstrapper : IBootstrapper
    {
        private static bool _hasStarted;

        public void BootstrapStructureMap()
        {
            ObjectFactory.Initialize(x =>
            {
                x.AddRegistry(new MainRegistry());
            });
        }

        /// <summary>
        /// Call this from tests setup
        /// </summary>
        public static void Restart()
        {
            if (_hasStarted)
            {
                ObjectFactory.ResetDefaults();
            }
            else
            {
                Bootstrap();
                _hasStarted = true;
            }
        }

        public static void Bootstrap()
        {
            new Bootstrapper().BootstrapStructureMap();
        }
    }
}
