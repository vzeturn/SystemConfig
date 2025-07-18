using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;

namespace SystemConfig.UnitTests
{
    public class BuildIntegrationTests
    {
        [Fact]
        public void Solution_ShouldBuildSuccessfully()
        {
            // Run dotnet build and verify success
            var psi = new ProcessStartInfo("dotnet", "build ../../../../SystemConfig.sln --nologo --no-restore")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            var process = Process.Start(psi);
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            Assert.Equal(0, process.ExitCode);
            Assert.DoesNotContain("error", output + error, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void AllProjects_ShouldHaveConsistentVersioning()
        {
            // Check all .csproj files for consistent version
            var root = Path.Combine(AppContext.BaseDirectory, "../../../../..");
            var projects = Directory.GetFiles(root, "*.csproj", SearchOption.AllDirectories);
            var versions = projects.Select(p =>
            {
                var text = File.ReadAllText(p);
                var start = text.IndexOf("<Version>");
                if (start < 0) return null;
                var end = text.IndexOf("</Version>", start);
                return text.Substring(start + 9, end - start - 9);
            }).Where(v => v != null).Distinct().ToList();
            Assert.True(versions.Count <= 1, $"Found multiple versions: {string.Join(", ", versions)}");
        }
    }
} 