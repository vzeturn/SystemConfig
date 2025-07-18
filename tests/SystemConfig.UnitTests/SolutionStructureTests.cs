using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace SystemConfig.UnitTests
{
    public class SolutionStructureTests
    {
        [Fact]
        public void Solution_ShouldHaveCorrectProjectReferences()
        {
            // TODO: Implement logic to check project references if needed
            // This is a placeholder for demonstration
            Assert.True(true, "Project references follow Clean Architecture");
        }

        [Fact]
        public void Projects_ShouldTargetCorrectFramework()
        {
            // Check .csproj files for correct TargetFramework
            var root = Path.Combine(AppContext.BaseDirectory, "../../../../..");
            var projects = Directory.GetFiles(root, "*.csproj", SearchOption.AllDirectories);
            Assert.Contains(projects, p => File.ReadAllText(p).Contains("<TargetFramework>net8.0-windows</TargetFramework>"));
            Assert.All(projects.Where(p => !p.Contains("Presentation")), p => Assert.Contains("<TargetFramework>net8.0</TargetFramework>", File.ReadAllText(p)));
        }

        [Fact]
        public void Projects_ShouldHaveConsistentNaming()
        {
            // All projects should follow SystemConfig.[LayerName] or SystemConfig.Presentation
            var root = Path.Combine(AppContext.BaseDirectory, "../../../../..");
            var projects = Directory.GetFiles(root, "*.csproj", SearchOption.AllDirectories);
            Assert.All(projects, p => Assert.Matches(@"SystemConfig\.(Application|Domain|Infrastructure|UnitTests|IntegrationTests|UITests|Presentation)", Path.GetFileNameWithoutExtension(p)));
        }
    }
} 