using Xunit;
using NetArchTest.Rules; // Головна бібліотека
using System.Reflection;  // Потрібно для завантаження збірки
using NetSdrClientApp.Messages; // Потрібно для посилання на збірку

namespace NetSdrClientAppTests
{
    public class ArchitectureTests
    {
        private static readonly Assembly AppAssembly = typeof(NetSdrMessageHelper).Assembly;

        [Fact]
        public void Messages_ShouldNot_DependOnNetworking()
        {
            // Arrange (Організація)
            // Визначаємо наше правило:
            var rule = Types
                .InAssembly(AppAssembly) // Беремо вашу головну збірку NetSdrClientApp
                .That()
                .ResideInNamespace("NetSdrClientApp.Messages") // Знаходимо всі класи в цьому неймспейсі
                .ShouldNot()
                .HaveDependencyOn("NetSdrClientApp.Networking"); // Перевіряємо, що вони не залежать від Networking

            // Act (Дія)
            var result = rule.GetResult();

            // Assert (Перевірка)
            // Якщо IsSuccessful == false, тест впаде і покаже, який клас порушив правило
            Assert.True(result.IsSuccessful, "Namespace 'Messages' should not depend on 'Networking'");
        }
    }
}
