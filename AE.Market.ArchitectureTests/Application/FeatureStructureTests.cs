using FluentAssertions;
using NetArchTest.Rules;

namespace AE.Market.ArchitectureTests.Application
{
    public class FeatureStructureTests : BaseTest
    {
        private static List<string> GetFeatureNames()
        {
            return ApplicationAssembly
                .GetTypes()
                .Select(t => t.Namespace)
                .Where(ns => ns?.StartsWith("AE.Market.Application.Features.") == true)
                .Select(ns => ns!.Split('.')[4])
                .Distinct()
                .OrderBy(name => name)
                .ToList();
        }

        [Fact]
        public void EveryFeature_Should_HaveCommandsFolder()
        {
            List<string> features = GetFeatureNames();
            List<string> failingFeatures = [];

            foreach (string feature in features)
            {
                var ns = $"AE.Market.Application.Features.{feature}.Commands";
                if (
                    !Types
                        .InAssembly(ApplicationAssembly)
                        .That()
                        .ResideInNamespace(ns)
                        .GetTypes()
                        .Any()
                )
                {
                    failingFeatures.Add(feature);
                }
            }
            failingFeatures
                .Should()
                .BeEmpty(
                    "because every feature must have a Commands folder with at least one type. "
                        + $"Failing features: [{string.Join(", ", failingFeatures)}]"
                );
        }

        [Fact]
        public void EveryFeature_Should_HaveQueriesFolder()
        {
            List<string> features = GetFeatureNames();
            List<string> failingFeatures = [];

            foreach (string feature in features)
            {
                var ns = $"AE.Market.Application.Features.{feature}.Queries";
                if (
                    !Types
                        .InAssembly(ApplicationAssembly)
                        .That()
                        .ResideInNamespace(ns)
                        .GetTypes()
                        .Any()
                )
                {
                    failingFeatures.Add(feature);
                }
            }
            failingFeatures
                .Should()
                .BeEmpty(
                    "because every feature must have a Queries folder with at least one type. "
                        + $"Failing features: [{string.Join(", ", failingFeatures)}]"
                );
        }

        [Fact]
        public void EveryFeature_Should_HaveDTOsFolder()
        {
            List<string> features = GetFeatureNames();
            List<string> failingFeatures = [];

            foreach (string feature in features)
            {
                var ns = $"AE.Market.Application.Features.{feature}.DTOs";
                if (
                    !Types
                        .InAssembly(ApplicationAssembly)
                        .That()
                        .ResideInNamespace(ns)
                        .GetTypes()
                        .Any()
                )
                {
                    failingFeatures.Add(feature);
                }
            }
            failingFeatures
                .Should()
                .BeEmpty(
                    "because every feature must have a DTOs folder with at least one type. "
                        + $"Failing features: [{string.Join(", ", failingFeatures)}]"
                );
        }

        [Fact]
        public void EveryFeature_Should_HaveSpecsFolder()
        {
            List<string> features = GetFeatureNames();
            List<string> failingFeatures = [];

            foreach (string feature in features)
            {
                var ns = $"AE.Market.Application.Features.{feature}.Specs";
                if (
                    !Types
                        .InAssembly(ApplicationAssembly)
                        .That()
                        .ResideInNamespace(ns)
                        .GetTypes()
                        .Any()
                )
                {
                    failingFeatures.Add(feature);
                }
            }
            failingFeatures
                .Should()
                .BeEmpty(
                    "because every feature must have a Specs folder with at least one type. "
                        + $"Failing features: [{string.Join(", ", failingFeatures)}]"
                );
        }

        [Fact]
        public void EveryFeature_ShouldHave_CacheKeysStaticClass()
        {
            List<string> features = GetFeatureNames();
            List<string> failingFeatures = [];
            foreach (var feature in features)
            {
                var cacheKeysType = ApplicationAssembly.GetType(
                    $"AE.Market.Application.Features.{feature}.CacheKeys"
                );
                if (cacheKeysType is not { IsClass: true, IsAbstract: true, IsSealed: true })
                    failingFeatures.Add(feature);
            }

            failingFeatures
                .Should()
                .BeEmpty(
                    "because every feature must have a CacheKeys static class. "
                        + $"Failing features: [{string.Join(", ", failingFeatures)}]"
                );
        }
    }
}
