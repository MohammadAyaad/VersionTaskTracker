using VersionTaskTracker.Model.Tracking;

namespace VersionTaskTracker.Model.Tracker
{
    public class VTTInstanceState
    {
        public Component Root;

        public VTTInstanceState(Component root)
        {
            this.Root = root;
        }
    }
}
