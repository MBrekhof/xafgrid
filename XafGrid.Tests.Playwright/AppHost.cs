using System.Diagnostics;

namespace XafGrid.Tests.Playwright;

/// <summary>
/// Starts the built XafGrid.Blazor.Server.exe once per test run on a fresh SQLite db and kills it afterwards.
/// Set XAFGRID_URL to point at an app you started yourself (headed debugging) — then nothing is launched.
/// </summary>
[SetUpFixture]
public class AppHost {
    public static string BaseUrl => Environment.GetEnvironmentVariable("XAFGRID_URL") ?? "http://localhost:5199";
    public static string RepoRoot { get; } = FindRepoRoot();

    static Process? _app;

    [OneTimeSetUp]
    public async Task StartApp() {
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
        if(await IsUpAsync(http)) return; // already running — reuse

        var serverDir = Path.Combine(RepoRoot, "XafGrid.Blazor.Server");
        var exe = Path.Combine(serverDir, "bin", "Debug", "net8.0", "XafGrid.Blazor.Server.exe");
        Assert.That(File.Exists(exe), Is.True, $"App not built: {exe}");

        foreach(var db in Directory.GetFiles(serverDir, "xafgrid.db*")) File.Delete(db);

        var logDir = Path.Combine(RepoRoot, "XafGrid.Tests.Playwright", "TestResults");
        Directory.CreateDirectory(logDir);
        var log = new StreamWriter(Path.Combine(logDir, "app.log"), append: false) { AutoFlush = true };

        _app = new Process {
            StartInfo = new ProcessStartInfo(exe, $"--urls {BaseUrl}") {
                WorkingDirectory = serverDir,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Environment = { ["ASPNETCORE_ENVIRONMENT"] = "Development" },
            },
        };
        _app.OutputDataReceived += (_, e) => { if(e.Data != null) log.WriteLine(e.Data); };
        _app.ErrorDataReceived += (_, e) => { if(e.Data != null) log.WriteLine("ERR " + e.Data); };
        _app.Start();
        _app.BeginOutputReadLine();
        _app.BeginErrorReadLine();

        var deadline = DateTime.UtcNow.AddSeconds(120); // first start creates + seeds the db
        while(DateTime.UtcNow < deadline) {
            if(_app.HasExited) Assert.Fail($"App exited with {_app.ExitCode}; see TestResults/app.log");
            if(await IsUpAsync(http)) return;
            await Task.Delay(1000);
        }
        Assert.Fail("App did not answer within 120 s; see TestResults/app.log");
    }

    [OneTimeTearDown]
    public void StopApp() {
        if(_app is { HasExited: false }) _app.Kill(entireProcessTree: true);
        _app?.Dispose();
    }

    static async Task<bool> IsUpAsync(HttpClient http) {
        try { return (await http.GetAsync(BaseUrl)).IsSuccessStatusCode; }
        catch { return false; }
    }

    static string FindRepoRoot() {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while(dir != null && !File.Exists(Path.Combine(dir.FullName, "XafGrid.sln"))) dir = dir.Parent;
        return dir?.FullName ?? throw new InvalidOperationException("XafGrid.sln not found above " + AppContext.BaseDirectory);
    }
}
