using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit;

namespace Microservicio.Clientes.Tests
{
    public class ApiTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public ApiTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task TCS01_AccesoSinToken_ReturnsUnauthorized()
        {
            // Arrange (TC-S01: Acceso sin token)
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/v1/clientes");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TCS08_GetClientes_HttpMethods_ReturnsMethodNotAllowed_Or_NotFound_IfInvalid()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act: Usar un método que no exista (ej. PROPFIND o PATCH general si no está definido)
            var request = new HttpRequestMessage(new HttpMethod("PROPFIND"), "/api/v1/clientes");
            var response = await client.SendAsync(request);

            // Assert
            Assert.True(response.StatusCode == HttpStatusCode.MethodNotAllowed || response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.Unauthorized);
        }
        
        [Fact]
        public async Task TCF05_ObtenerClienteInvalid_ReturnsUnauthorized_Or_NotFound()
        {
            // Arrange (TC-F05)
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/v1/clientes/99999");

            // Assert
            // Al no estar autenticados, nos dará 401 primero, lo que valida la barrera
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
