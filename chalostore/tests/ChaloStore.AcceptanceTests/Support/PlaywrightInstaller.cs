using System.Threading;

namespace ChaloStore.AcceptanceTests.Support;

internal static class PlaywrightInstaller
{
    private static readonly SemaphoreSlim Gate = new(1, 1);
    private static bool _installed;

    public static async Task EnsureInstalledAsync()
    {
        if (_installed)
        {
            return;
        }

        await Gate.WaitAsync();
        try
        {
            if (_installed)
            {
                return;
            }

            await Task.Run(() => Microsoft.Playwright.Program.Main(new[] { "install" }));
            _installed = true;
        }
        finally
        {
            Gate.Release();
        }
    }
}

