public sealed class BuildParametersListeResultats
{
    public string CurrentPath { get; }
    public string SiteCode { get; }
    public string Origin { get; }
    public BuildParameters(string origin, string currentPath, string siteCode)
    {
        CurrentPath = currentPath;
        SiteCode = siteCode;
        Origin = origin;
    }
}
