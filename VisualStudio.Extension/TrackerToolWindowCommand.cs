using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using System.Threading;
using System.Threading.Tasks;

namespace VisualStudio.Extension
{
    [VisualStudioContribution]
    public class TrackerToolWindowCommand : Command
    {
        /// <inheritdoc />
        public override CommandConfiguration CommandConfiguration => new(displayName: "Yarn Ops")
        {
            Placements = [CommandPlacement.KnownPlacements.ViewOtherWindowsMenu],
            Icon = new(ImageMoniker.Custom("YarnOps"), IconSettings.IconAndText),
        };

        /// <inheritdoc />
        public override async Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken)
        {
            await this.Extensibility.Shell().ShowToolWindowAsync<TrackerToolWindow>(activate: true, cancellationToken);
        }
    }
}
