using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.ToolWindows;
using Microsoft.VisualStudio.RpcContracts.RemoteUI;
using System.Threading;
using System.Threading.Tasks;

namespace VisualStudio.Extension
{
    /// <summary>
    /// A sample tool window.
    /// </summary>
    [VisualStudioContribution]
    public class TrackerToolWindow : ToolWindow
    {
        private readonly TrackerToolWindowContent content = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackerToolWindow" /> class.
        /// </summary>
        public TrackerToolWindow()
        {
            this.Title = "Yarn Ops";
        }

        /// <inheritdoc />
        public override ToolWindowConfiguration ToolWindowConfiguration => new()
        {
            Placement = ToolWindowPlacement.Floating,
        };

        /// <inheritdoc />
        public override Task InitializeAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public override Task<IRemoteUserControl> GetContentAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IRemoteUserControl>(content);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                content.Dispose();

            base.Dispose(disposing);
        }
    }
}
