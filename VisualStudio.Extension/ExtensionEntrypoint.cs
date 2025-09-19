using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Extensibility;

namespace VisualStudio.Extension
{
    [VisualStudioContribution]
    internal class ExtensionEntrypoint : Microsoft.VisualStudio.Extensibility.Extension
    {
        /// <inheritdoc/>
        public override ExtensionConfiguration ExtensionConfiguration => new()
        {
            Metadata = new(
                    id: "YarnOps.29e4c6f8-0b85-4fb1-aeb4-20ff57c65928",
                    version: this.ExtensionAssemblyVersion,
                    publisherName: "Jullyana Ramos",
                    displayName: "YarnOps",
                    description: "Stay focused during builds with a simple fiber project tracker.")
            {
                Icon = "Images\\YarnOps.png",
                InstallationTargetVersion = "[17.14,19.0)",
                MoreInfo = "https://github.com/jumattos/YarnOps#readme",
                PreviewImage = "Images\\PreviewImage.png",
                ReleaseNotes = "https://github.com/jumattos/YarnOps/releases",
                Tags = new[] { "productivity", "tracker", "build", "fiber", "yarn", "knitting", "knit", "crocheting", "crochet", "stitch", "counter" }
            }
        };

        /// <inheritdoc />
        protected override void InitializeServices(IServiceCollection serviceCollection)
        {
            base.InitializeServices(serviceCollection);
        }
    }
}
