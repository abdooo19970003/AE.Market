using AE.Market.Domain.Aggregates.Auth;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NetArchTest.Rules;
using System.Reflection;

namespace AE.Market.ArchitectureTests.API
{
    public class ApiTests:BaseTest
    {
        [Fact]
        public void Controllers_Should_BeSealed()
        {
            var results = Types
                .InAssembly(ApiAssembly)
                .That()
                .Inherit(typeof(ControllerBase))
                .Should()
                .BeSealed()
                .GetResult();

            var failingTypes = string.Join(",", results.FailingTypeNames ?? []); 
            results.IsSuccessful.Should().BeTrue("Controllers should be sealed."+$"Failing controllers: {failingTypes}");
        }

        [Fact]
        public void ControllersNames_Should_EndWith_ControllerSuffix()
        {
            var results = Types
               .InAssembly(ApiAssembly)
               .That()
               .Inherit(typeof(ControllerBase))
               .Should()
               .HaveNameEndingWith("Controller")
               .GetResult();

            var failingTypes = string.Join(",", results.FailingTypeNames ?? []);
            results.IsSuccessful.Should().BeTrue("Controllers name should end with Controller suffix." + $"Failing controllers: {failingTypes}");
        }

        [Fact]
        public void Controllers_Should_BeInControllersDirectory() {
            var results = Types
                  .InAssembly(ApiAssembly)
                  .That()
                  .Inherit(typeof(ControllerBase))
                  .Should()
                  .ResideInNamespace("AE.Market.Api.Controllers")
                  .GetResult();

            var failingTypes = string.Join(",", results.FailingTypeNames ?? []);
            results.IsSuccessful.Should().BeTrue("Controllers should be in controllers directory." + $"Failing controllers: {failingTypes}");
        }

        [Fact]
        public void Filters_Should_HaveNameSuffix_Filter()
        {
            var results = Types
                          .InAssembly(ApiAssembly)
                          .That()
                          .ImplementInterface(typeof(IFilterMetadata))
                          .Should()
                          .HaveNameEndingWith("Filter")
                          .GetResult();

            var failingTypes = string.Join(",", results.FailingTypeNames ?? []);
            results.IsSuccessful.Should().BeTrue("Filters name should end with Filter suffix." + $"Failing types: {failingTypes}");
        }

        [Fact]
        public void Filters_Should_BeSealed()
        {
            var results = Types
                          .InAssembly(ApiAssembly)
                          .That()
                          .ImplementInterface(typeof(IFilterMetadata))
                          .Should()
                          .BeSealed()
                          .GetResult();

            var failingTypes = string.Join(",", results.FailingTypeNames ?? []);
            results.IsSuccessful.Should().BeTrue("Filters should be sealed." + $"Failing types: {failingTypes}");
        }

        [Fact]
        public void Helpers_Should_BeStatic()
        {
            var results = Types
                          .InAssembly(ApiAssembly)
                          .That()
                          .ResideInNamespace("AE.Market.Api.Helpers")
                          .Should()
                          .BeStatic()
                          .GetResult();

            var failingTypes = string.Join(",", results.FailingTypeNames ?? []);
            results.IsSuccessful.Should().BeTrue("Helpers should be static classes." + $"Failing types: {failingTypes}");
        }


        [Fact]
        public void ExceptionHandlers_Should_BeSealed()
        {
            var results = Types
                          .InAssembly(ApiAssembly)
                          .That()
                          .ImplementInterface(typeof(IExceptionHandler))
                          .Should()
                          .BeSealed()
                          .GetResult();

            var failingTypes = string.Join(",", results.FailingTypeNames ?? []);
            results.IsSuccessful.Should().BeTrue("Exception handlers should be sealed." + $"Failing types: {failingTypes}");
        }


        [Fact]
        public void ExceptionHandlers_Should_BeInItsDirectory()
        {
            var results = Types
                          .InAssembly(ApiAssembly)
                          .That()
                          .ImplementInterface(typeof(IExceptionHandler))
                          .Should()
                          .ResideInNamespace("AE.Market.Api.Exceptions")
                          .GetResult();

            var failingTypes = string.Join(",", results.FailingTypeNames ?? []);
            results.IsSuccessful.Should().BeTrue("Exception handlers should be in Exceptions directory." + $"Failing types: {failingTypes}");
        }

        [Fact]
        public void Attributes_Should_BeSealed()
        {
            var results = Types
                          .InAssembly(ApiAssembly)
                          .That()
                          .Inherit(typeof(Attribute))
                          .Should()
                          .BeSealed()
                          .GetResult();

            var failingTypes = string.Join(",", results.FailingTypeNames ?? []);
            results.IsSuccessful.Should().BeTrue("Attributes should be sealed." + $"Failing types: {failingTypes}");
        }

        [Fact]
        public void Attributes_Should_HaveAttributeSuffix()
        {
            var results = Types
                          .InAssembly(ApiAssembly)
                          .That()
                          .Inherit(typeof(Attribute))
                          .Should()
                          .HaveNameEndingWith("Attribute")
                          .GetResult();

            var failingTypes = string.Join(",", results.FailingTypeNames ?? []);
            results.IsSuccessful.Should().BeTrue("Attributes should have name suffix attribute." + $"Failing types: {failingTypes}");
        }


        [Fact]
        public void Middlewares_Should_BeSealed()
        {
            var results = Types
                    .InAssembly(ApiAssembly)
                    .That()
                    .HaveNameEndingWith("Middleware")
                    .Should()
                    .BeSealed()
                    .GetResult();

            var failingTypes = string.Join(",", results.FailingTypeNames ?? []);
            results.IsSuccessful.Should().BeTrue("Middlewares should be sealed." + $"Failing types: {failingTypes}");
        }
        [Fact]
        public void Middlewares_Should_BeInMiddlewaresDirectory()
        {
            var results = Types
                    .InAssembly(ApiAssembly)
                    .That()
                    .HaveNameEndingWith("Middleware")
                    .Should()
                    .ResideInNamespace("AE.Market.Api.Middlewares")
                    .GetResult();

            var failingTypes = string.Join(",", results.FailingTypeNames ?? []);
            results.IsSuccessful.Should().BeTrue("Middlewares should be in middlewares directory." + $"Failing types: {failingTypes}");
        }

        [Fact]
        public void Controllers_Should_HavePrimaryConstructorWithMediator()
        {
            var controllers = Types
                .InAssembly(ApiAssembly)
                .That()
                .Inherit(typeof(ControllerBase))
                .GetTypes();

            var failingTypes = new List<string>();
            foreach (var controller in controllers)
            {
                var constructors = controller.GetConstructors();
                if (constructors.Length != 1)
                {
                    failingTypes.Add($"{controller.Name} (expected 1 constructor, found {constructors.Length})");
                    continue;
                }

                var ctor = constructors[0];
                var parameters = ctor.GetParameters();
                if (parameters.Length != 1 || parameters[0].ParameterType != typeof(IMediator))
                {
                    failingTypes.Add($"{controller.Name} (expected single IMediator parameter)");
                }
            }

            failingTypes.Should().BeEmpty(
                $"because all controllers should use a primary constructor injecting IMediator. " +
                $"Failing: [{string.Join(", ", failingTypes)}]");
        }

        [Fact]
        public void AdminEndpoints_Should_HaveMutateUsersPermission()
        {
            var methods = ApiAssembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(ControllerBase)))
                .SelectMany(t => t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
                .Where(m => m.CustomAttributes.Any(a =>
                    a.AttributeType.Name is "HttpPutAttribute" or "HttpDeleteAttribute" or "HttpPostAttribute"))
                .ToList();

            var adminRoutes = new[] { "users" };
            var failingMethods = new List<string>();

            foreach (var method in methods)
            {
                var routeAttr = method.CustomAttributes
                    .FirstOrDefault(a => a.AttributeType.Name is "HttpPutAttribute" or "HttpDeleteAttribute" or "HttpPostAttribute" or "HttpGetAttribute");
                if (routeAttr == null) continue;

                var routeTemplate = routeAttr.ConstructorArguments.FirstOrDefault().Value?.ToString() ?? "";
                bool isAdminRoute = adminRoutes.Any(r => routeTemplate.Contains(r, StringComparison.OrdinalIgnoreCase))
                    && !routeTemplate.Contains("login", StringComparison.OrdinalIgnoreCase)
                    && !routeTemplate.Contains("register", StringComparison.OrdinalIgnoreCase)
                    && !routeTemplate.Contains("profile", StringComparison.OrdinalIgnoreCase);

                if (isAdminRoute)
                {
                    bool hasMutateUsers = method.CustomAttributes
                        .Any(a => a.AttributeType.Name == "HasPermissionAttribute"
                            && a.ConstructorArguments.Any(c =>
                            {
                                if (c.Value is int intVal)
                                    return intVal == (int)Permission.MutateUsers;
                                return c.Value?.ToString() == "MutateUsers";
                            }));
                    bool hasAuthorize = method.CustomAttributes
                        .Any(a => a.AttributeType.Name == "AuthorizeAttribute");

                    if (!hasMutateUsers || !hasAuthorize)
                    {
                        failingMethods.Add($"{method.DeclaringType?.Name}.{method.Name} (route: {routeTemplate})");
                    }
                }
            }

            failingMethods.Should().BeEmpty(
                $"because all admin endpoints must have [Authorize] and [HasPermission(Permission.MutateUsers)]. " +
                $"Failing: [{string.Join(", ", failingMethods)}]");
        }

        [Fact]
        public void Endpoints_Should_ReturnIActionResult()
        {
            var methods = ApiAssembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(ControllerBase)))
                .SelectMany(t => t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
                .Where(m => m.CustomAttributes.Any(a =>
                    a.AttributeType.Name is "HttpGetAttribute" or "HttpPostAttribute"
                        or "HttpPutAttribute" or "HttpDeleteAttribute" or "HttpPatchAttribute"))
                .ToList();

            var failingMethods = methods
                .Where(m => m.ReturnType != typeof(IActionResult) && m.ReturnType != typeof(Task<IActionResult>))
                .Select(m => $"{m.DeclaringType?.Name}.{m.Name} (returns {m.ReturnType.Name})")
                .ToList();

            failingMethods.Should().BeEmpty(
                $"because all controller endpoints should return IActionResult. " +
                $"Failing: [{string.Join(", ", failingMethods)}]");
        }
    }
}
