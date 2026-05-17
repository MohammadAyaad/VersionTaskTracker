using System.Diagnostics;
using System.Formats.Tar;
using System.IO.Compression;
using System.Runtime.InteropServices;
using Octokit;

namespace VersionTaskTracker.Cli.Services;

public static class UpgradeService
{
    public const string GITHUB_USERNAME = "MohammadAyaad";
    public const string APP_FULL_NAME = "VersionTaskTracker";
    public const string APP_CLI_NAME = "vtt";
    public const string GITHUB_REPOSITORY = "VersionTaskTracker";
    public const string GITHUB_URL_FULL =
        $"https://api.github.com/repos/{GITHUB_USERNAME}/{GITHUB_REPOSITORY}";
    public static string TEMP_DIR_NAME = "temp";
    public static string TEMP_DIR = Path.Combine(AppContext.BaseDirectory, TEMP_DIR_NAME)!;

    public static async Task<List<string>> GetAvailableVersions()
    {
        var github = new GitHubClient(new ProductHeaderValue("MohammadAyaad-VersionTaskTracker"));
        IReadOnlyList<Release> releases = await github.Repository.Release.GetAll(
            GITHUB_USERNAME,
            GITHUB_REPOSITORY
        );

        return releases.Select(r => r.Name).OrderByDescending(s => s).ToList();
    }

    public static async Task<string> GetVersionFileName(string version)
    {
        var github = new GitHubClient(new ProductHeaderValue("MohammadAyaad-VersionTaskTracker"));
        IReadOnlyList<Release> releases = await github.Repository.Release.GetAll(
            GITHUB_USERNAME,
            GITHUB_REPOSITORY
        );

        var rels = releases.Select(r => r.Name).OrderByDescending(s => s).ToList();
        if (!rels.Contains(version))
            throw new Exception($"Version '{version}' Unavailable");

        string assetName = null!;
        Release release = releases.First(r => r.Name.Equals(version));
        Architecture arch = RuntimeInformation.ProcessArchitecture;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            assetName =
                $"{APP_FULL_NAME}-{version}-Windows-{arch.ToString().ToLower()}-Compressed.zip";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            assetName =
                $"{APP_FULL_NAME}-{version}-Linux-{arch.ToString().ToLower()}-Compressed.tar.gz";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            assetName =
                $"{APP_FULL_NAME}-{version}-macOS-{arch.ToString().ToLower()}-Compressed.tar.gz";
        }
        else
        {
            throw new Exception("OS not supported.");
        }

        return assetName;
    }

    public static async Task<string> DownloadVersion(string version, string fileName)
    {
        var github = new GitHubClient(new ProductHeaderValue("MohammadAyaad-VersionTaskTracker"));
        IReadOnlyList<Release> releases = await github.Repository.Release.GetAll(
            GITHUB_USERNAME,
            GITHUB_REPOSITORY
        );

        var rels = releases.Select(r => r.Name).OrderByDescending(s => s).ToList();
        if (!rels.Contains(version))
            throw new Exception($"Version '{version}' Unavailable");

        Release release = releases.First(r => r.Name.Equals(version));
        ReleaseAsset asset =
            release.Assets.FirstOrDefault(a => a.Name.Equals(fileName))
            ?? throw new Exception($"Installation file for '{fileName}' not found.");

        string outputFile = Path.Combine(TEMP_DIR, fileName)!;

        await using var responseStream = await new HttpClient().GetStreamAsync(
            asset.BrowserDownloadUrl
        );
        await using var fileStream = new FileStream(outputFile, System.IO.FileMode.CreateNew);

        await responseStream.CopyToAsync(fileStream);
        return outputFile;
    }

    public static async Task CopySelfToTemp()
    {
        string executableName = Path.GetFileName(Environment.ProcessPath!);
        string src = Path.Combine(AppContext.BaseDirectory, executableName);
        string dst = Path.Combine(TEMP_DIR, executableName);
        File.Copy(src, dst, true);
    }

    public static async Task RunTempVersion()
    {
        string executableName = Path.GetFileName(Environment.ProcessPath!);
        string target = Path.Combine(TEMP_DIR, executableName!);
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = target,
            Arguments = "upgrade continue",
            UseShellExecute = false,
        };
        Process.Start(startInfo);
    }

    private static bool TryDeleteFileWithRetry(
        string filePath,
        int maxRetries,
        int delayMilliseconds
    )
    {
        if (!File.Exists(filePath))
        {
            return true; // File is already gone
        }

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                // Attempt to open the file exclusively.
                // If this succeeds, the file is not locked by any other process.
                using (
                    FileStream fs = new FileStream(
                        filePath,
                        System.IO.FileMode.Open,
                        FileAccess.ReadWrite,
                        FileShare.None
                    )
                )
                {
                    // File is free! We can safely close the stream and delete it.
                }

                File.Delete(filePath);
                return true;
            }
            catch (IOException)
            {
                // The file is still locked. Wait and try again.
                Thread.Sleep(delayMilliseconds);
            }
            catch (UnauthorizedAccessException)
            {
                // Sometimes a closing process temporarily throws an UnauthorizedAccessException
                // instead of an IOException. Treat it as a temporary lock.
                Thread.Sleep(delayMilliseconds);
            }
            catch (Exception ex)
            {
                // Handle or log other unexpected errors (e.g., permissions issues)
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return false;
            }
        }

        return false; // Reached max retries without success
    }

    public static async Task RemoveSelfOldFromTemp()
    {
        //running from within the temp folder
        if (
            Path.GetFileName(AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar))
            != TEMP_DIR_NAME
        )
            throw new Exception("Invalid Directory of execution.");
        string target = Path.Combine(
            Directory.GetParent(AppContext.BaseDirectory)!.ToString(),
            Path.GetFileName(Environment.ProcessPath!)
        );
        if (!TryDeleteFileWithRetry(target, 20, 500))
        {
            throw new Exception(
                "Could not continue due to the file lock unclearing after 20 tries over 10 seconds."
            );
        }
    }

    public static async Task CopyNewCompressedVersion()
    {
        //running from within the temp folder
        if (
            Path.GetFileName(AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar))
            != TEMP_DIR_NAME
        )
            throw new Exception("Invalid Directory of execution.");
        string srcDir = AppContext.BaseDirectory; //this is now the temp folder anyway
        string src = Directory
            .GetFiles(srcDir)
            .First(f =>
                Path.GetFileName(f).StartsWith(APP_FULL_NAME)
                && (f.EndsWith(".tar.gz") || f.EndsWith(".zip"))
            );
        string dst = Path.Combine(
            Directory.GetParent(AppContext.BaseDirectory)!.ToString(),
            Path.GetFileName(src)
        );
        File.Copy(src, dst, true);
    }

    public static async Task ExtractNewVersion()
    {
        //running from within the temp folder
        if (
            Path.GetFileName(AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar))
            != TEMP_DIR_NAME
        )
            throw new Exception("Invalid Directory of execution.");

        string target = Directory.GetParent(AppContext.BaseDirectory)!.ToString();
        string compressed = Directory
            .GetFiles(target)
            .First(f =>
                Path.GetFileName(f).StartsWith(APP_FULL_NAME)
                && (f.EndsWith(".tar.gz") || f.EndsWith(".zip"))
            );
        if (compressed.EndsWith(".zip"))
        {
            ZipFile.ExtractToDirectory(compressed, target);
        }
        else if (compressed.EndsWith(".tar.gz"))
        {
            FileStream fs = File.OpenRead(compressed);

            GZipStream gzipStream = new GZipStream(fs, CompressionMode.Decompress);

            TarFile.ExtractToDirectory(gzipStream, target, overwriteFiles: true);
        }
        else
            throw new Exception("Failed to identify the compressed file's compression.");
    }

    public static async Task RunNewVersion()
    {
        string target = Path.Combine(
            Directory.GetParent(AppContext.BaseDirectory)!.ToString(),
            Path.GetFileName(Environment.ProcessPath!)!
        );
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = target,
            Arguments = "upgrade clean",
            UseShellExecute = false,
        };
        Process.Start(startInfo);
    }

    public static async Task CleanOldVersion()
    {
        string target = Path.Combine(TEMP_DIR, Path.GetFileName(Environment.ProcessPath!)!);
        if (!TryDeleteFileWithRetry(target, 20, 500))
            throw new Exception(
                "Could not continue due to the file lock unclearing after 20 tries over 10 seconds."
            );
        Directory.Delete(TEMP_DIR, recursive: true);
    }
}
