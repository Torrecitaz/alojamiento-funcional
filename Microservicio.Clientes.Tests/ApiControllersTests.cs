using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit;

namespace Microservicio.Clientes.Tests
{
    public class ApiControllersTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public ApiControllersTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData("/api/v1/calificaciones")]
        [InlineData("/api/v1/colaboradores")]
        [InlineData("/api/v1/habitaciones")]
        [InlineData("/api/v1/maestros/ciudades")]
        [InlineData("/api/v1/pagos")]
        [InlineData("/api/v1/propiedades")]
        [InlineData("/api/v1/reservas")]
        public async Task TestProtectedEndpoints_WithoutToken_ReturnsUnauthorized(string endpoint)
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync(endpoint);

            // Assert
            // Esperamos que TODAS las nuevas APIs tengan [Authorize] activo (401 Unauthorized)
            // A diferencia de ClientesController que devolvía 200/404 erróneamente sin token.
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
        
        [Fact]
        public async Task Test_Reservas_POST_WithInvalidPayload_ReturnsBadRequest_Or_Unauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();
            var content = new StringContent("{ \"invalid\": \"data\" }", System.Text.Encoding.UTF8, "application/json");
            
            // Act
            var response = await client.PostAsync("/api/v1/reservas", content);
            
            // Assert
            // Si está protegido devolverá 401. Si de casualidad estuviera público, fallaría en el DTO Binding con 400.
            Assert.True(response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.BadRequest);
        }
    }
}
