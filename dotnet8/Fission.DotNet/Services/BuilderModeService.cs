namespace Fission.DotNet.Services
{
    public class BuilderModeService
    {
        public bool IsBuilderMode { get; }

        public BuilderModeService(bool isBuilderMode)
        {
            IsBuilderMode = isBuilderMode;
        }
    }
}