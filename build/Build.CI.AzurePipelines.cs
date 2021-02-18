// Copyright 2019 Maintainers of NUKE.
// Distributed under the MIT License.
// https://github.com/nuke-build/nuke/blob/master/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI.AzurePipelines;
using Nuke.Common.CI.AzurePipelines.Configuration;
using Nuke.Common.Execution;
using Nuke.Common.Tooling;
using Nuke.Components;
using static Nuke.Enterprise.Notifications.IHazAzurePipelinesAccessToken;
using static Nuke.Enterprise.Notifications.IHazSlackCredentials;
#if ENTERPRISE
using Nuke.Enterprise.Notifications;
#endif

[AzurePipelines(
    suffix: null,
    AzurePipelinesImage.UbuntuLatest,
    AzurePipelinesImage.WindowsLatest,
    AzurePipelinesImage.MacOsLatest,
    ImportSecrets = new[]
                    {
                        nameof(EnterpriseAccessToken),
#if ENTERPRISE
                        Slack + nameof(IHazSlackCredentials.AppAccessToken),
                        Slack + nameof(IHazSlackCredentials.UserAccessToken),
#endif
                    },
#if ENTERPRISE
    ImportSystemAccessTokenAs = IHazAzurePipelinesAccessToken.AzurePipelines + nameof(IHazAzurePipelinesAccessToken.AccessToken),
#endif
    InvokedTargets = new[] { nameof(ITest.Test), nameof(IPack.Pack) },
    NonEntryTargets = new[] { nameof(IRestore.Restore), nameof(DownloadFonts), nameof(InstallFonts), nameof(ReleaseImage) },
    ExcludedTargets = new[] { nameof(Clean), nameof(IReportTestCoverage.ReportTestCoverage), nameof(SignPackages) })]
partial class Build
{
    public class AzurePipelinesAttribute : Nuke.Common.CI.AzurePipelines.AzurePipelinesAttribute
    {
        public AzurePipelinesAttribute(
            string suffix,
            AzurePipelinesImage image,
            params AzurePipelinesImage[] images)
            : base(suffix, image, images)
        {
        }

        protected override AzurePipelinesJob GetJob(
            ExecutableTarget executableTarget,
            LookupTable<ExecutableTarget, AzurePipelinesJob> jobs,
            IReadOnlyCollection<ExecutableTarget> relevantTargets)
        {
            var job = base.GetJob(executableTarget, jobs, relevantTargets);

            var dictionary = new Dictionary<string, string>
                             {
                                 { nameof(ICompile.Compile), "⚙️" },
                                 { nameof(ITest.Test), "🚦" },
                                 { nameof(IPack.Pack), "📦" },
                                 { nameof(IReportTestCoverage.ReportTestCoverage), "📊" },
                                 { nameof(IPublish.Publish), "🚚" },
                                 { nameof(Announce), "🗣" }
                             };
            var symbol = dictionary.GetValueOrDefault(job.Name).NotNull("symbol != null");
            job.DisplayName = job.PartitionName == null
                ? $"{symbol} {job.DisplayName}"
                : $"{symbol} {job.DisplayName} 🧩";
            return job;
        }
    }
}