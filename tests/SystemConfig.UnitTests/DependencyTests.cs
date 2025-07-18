using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace SystemConfig.UnitTests
{
    public class DependencyTests
    {
        [Fact]
        public void Domain_ShouldHaveNoDependencies()
        {
            // Domain layer should not reference any non-System assemblies
            var domainAssembly = Assembly.Load("SystemConfig.Domain");
            var dependencies = domainAssembly.GetReferencedAssemblies();
            Assert.DoesNotContain(dependencies, d => !d.Name.StartsWith("System"));
        }

        [Fact]
        public void Application_ShouldOnlyDependOnDomain()
        {
            // Application layer should only reference Domain (besides System)
            var appAssembly = Assembly.Load("SystemConfig.Application");
            var dependencies = appAssembly.GetReferencedAssemblies();
            Assert.Contains(dependencies, d => d.Name == "SystemConfig.Domain");
            Assert.DoesNotContain(dependencies, d => d.Name == "SystemConfig.Infrastructure");
        }
    }
} 