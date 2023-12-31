using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Tests.Models;
using Tests.Services.Factories;
using Tests.Services.Generators;
using Tests.Services.Repositories;

namespace Siva.Extensions.DependencyInjection.Implementations.Tests
{
    public class ImplementationInjectionExtensionsTests
    {
        [Theory]
        [InlineData(typeof(IRepository<>), typeof(IBookRepository))]
        [InlineData(typeof(Factory<>), typeof(Factory<Train>))]
        public void AddImplementationsOfGenericInterfacesFromAssembly(Type targetAssignableType, Type expectedServiceType)
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();

            // Act
            services.AddImplementations(targetAssignableType, typeof(ImplementationInjectionExtensionsTests).Assembly);

            // Assert
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            object service = serviceProvider.GetRequiredService(expectedServiceType);

            ServiceDescriptor? serviceDescriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == expectedServiceType);

            serviceDescriptor.Should().NotBeNull();
            service.Should().BeAssignableTo(expectedServiceType);
        }

        [Fact]
        public async Task AddImplementationsWithDependenciesFromAssemblyAsync()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();
            IEnumerable<Type> types = typeof(ImplementationInjectionExtensionsTests).Assembly.GetExportedTypes();

            // Act
            services.AddImplementations(typeof(ITokenGenerator<>), types);
            services.AddImplementations(typeof(IRepository<>), types);

            // Assert
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            IUserRepository userRepository = serviceProvider.GetRequiredService<IUserRepository>();

            ServiceDescriptor? userRepositoryServiceDescriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IUserRepository));

            User? user = await userRepository.GetByNameAsync("Eva");
            user.Should().NotBeNull();
            userRepository.GenerateToken(user!).Should().NotBeNull();

            userRepositoryServiceDescriptor.Should().NotBeNull();
        }

        [Fact]
        public void AddImplementationsFromAssemblies()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();

            IEnumerable<Assembly> assemblies = new Assembly[] {
                typeof(ImplementationInjectionExtensions).Assembly,
                typeof(ImplementationInjectionExtensionsTests).Assembly
            };

            // Act
            services.AddImplementations(typeof(Factory<>), assemblies);

            // Assert
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            Factory<Shoe> shoeFactory = serviceProvider.GetRequiredService<Factory<Shoe>>();

            ServiceDescriptor? shoeFactoryServiceDescriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(Factory<Shoe>));

            Shoe? shoe = shoeFactory.Create();
            shoe.Should().NotBeNull();
            shoeFactoryServiceDescriptor.Should().NotBeNull();
        }

        [Fact]
        public void AddImplementationsOfSealedTypes_ThrowsArgumentException()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();
            Assembly assembly = typeof(ImplementationInjectionExtensionsTests).Assembly;

            // Assert
            Assert.Throws<ArgumentException>(() =>
            {
                // Act
                services.AddImplementations(typeof(string), assembly);
            });
        }
    }
}