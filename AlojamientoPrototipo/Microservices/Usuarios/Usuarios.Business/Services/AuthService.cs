using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Usuarios.Business.DTOs.Auth;
using Usuarios.Business.Interfaces;
using Usuarios.DataManagement.Interfaces;

namespace Usuarios.Business.Services;

public class AuthService : IAuthService
{
    private readonly IUsuariosDataService _usuarioData;
    private readonly IClientesDataService _clienteData;
    private readonly IConfiguration _configuration;

    public AuthService(IUsuariosDataService usuarioData, IClientesDataService clienteData, IConfiguration configuration)
    {
        _usuarioData = usuarioData;
        _clienteData = clienteData;
        _configuration = configuration;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _usuarioData.GetByEmailAsync(request.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        // Generar JWT
        var tokenHandler = new JwtSecurityTokenHandler();
        var secretKey = _configuration["Jwt:Secret"] ?? "SuperSecretKeyOfAtLeast32CharactersLong!!!";
        var key = Encoding.ASCII.GetBytes(secretKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UsuarioId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Rol)
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        // Obtener ClienteId si el rol es Cliente
        int? clienteId = null;
        if (user.Rol.Equals("Cliente", StringComparison.OrdinalIgnoreCase))
        {
            var cliente = await _clienteData.GetByUsuarioIdAsync(user.UsuarioId);
            if (cliente != null)
            {
                clienteId = cliente.ClienteId;
            }
        }

        return new LoginResponse(
            Token: tokenString,
            NombreCompleto: user.NombreCompleto,
            Email: user.Email,
            Roles: new[] { user.Rol },
            ClienteId: clienteId
        );
    }
}
