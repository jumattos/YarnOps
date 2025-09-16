using Microsoft.VisualStudio.Extensibility.UI;

namespace VisualStudio.Extension
{
    internal class TrackerToolWindowContent : RemoteUserControl
    {
        public TrackerToolWindowContent()
            : base(dataContext: new TrackerToolWindowData())
        {
        }
    }
}
