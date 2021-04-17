using Semver;

namespace GitHub.ReleaseDownloader
{
    public interface IReleaseDownloader
    {
        bool IsLatestRelease(string version);
        string GetLatestReleaseVersion();
        bool DownloadLatestRelease();
        void DeInit();
    }
}