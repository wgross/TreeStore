using System.Management.Automation;

namespace TreeStore.PsModule.Cmdlets
{
    [Cmdlet(VerbsCommon.New, "TreeStoreDrive", DefaultParameterSetName = "Path")]
    public sealed class NewTreeStoreDriveCmdlet : PSCmdlet
    {
        private string name;

        /// <summary>
        /// Gets or sets the name of the drive.
        /// </summary>
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public string Name
        {
            get => this.name;
            set => this.name = value; // ?? throw PSTraceSource.NewArgumentNullException(nameof(value));
        }

        [Parameter(Position = 1, ParameterSetName = "Path", ValueFromPipelineByPropertyName = true)]
        public string TreeStorePath { get; set; } = string.Empty;

        [Parameter(Position = 1, ParameterSetName = "LiteralPath", ValueFromPipeline = false, ValueFromPipelineByPropertyName = true)]
        [Alias("PSPath", "LP")]
        public string TreeStorePathLiteralPath
        {
            get => this.TreeStorePath;
            set => this.TreeStorePath = value;
        }

        [Parameter(ValueFromPipelineByPropertyName = true)]
        public string? Scope { get; set; } = null;

        protected override void ProcessRecord()
        {
            if (string.IsNullOrEmpty(this.TreeStorePath))
            {
                this.SessionState.Drive.New(new PSDriveInfo(
                    name: this.Name,
                    provider: this.SessionState.Provider.GetOne(TreeStoreCmdletProvider.Id),
                    root: string.Empty,
                    description: $"TreeStore in memory",
                    credential: null),
                    scope: this.Scope);
            }
            else
            {
                var providerPath = this.SessionState.Path.GetUnresolvedProviderPathFromPSPath(this.TreeStorePath, out var providerInfo, out var drive);

                this.SessionState.Drive.New(new PSDriveInfo(
                    name: this.Name,
                    provider: this.SessionState.Provider.GetOne(TreeStoreCmdletProvider.Id),
                    root: providerPath,
                    description: $"TreeStore created from database '{providerPath}'",
                    credential: null),
                    scope: this.Scope);
            }

            // Create function to switch to the drive
            if (!this.SessionState.InvokeProvider.Item.Exists($@"Function:\{this.NavigationFunctionName}:"))
                this.SessionState.InvokeProvider.Item.Set($@"Function:\{this.NavigationFunctionName}:", ScriptBlock.Create("Set-Location $MyInvocation.MyCommand.Name"));
        }

        /// <summary>
        /// The navigations function visibility should match the drives visibility.
        /// </summary>
        private string NavigationFunctionName => string.IsNullOrEmpty(this.Scope)
            ? this.Name
            : $"{this.Scope}:{this.Name}";
    }
}