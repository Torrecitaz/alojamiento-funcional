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
    private readonly IColaboradoresDataService _colaboradorData;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUsuariosDataService usuarioData, 
        IClientesDataService clienteData, 
        IColaboradoresDataService colaboradorData,
        IConfiguration configuration)
    {
        _usuarioData = usuarioData;
        _clienteData = clienteData;
        _colaboradorData = colaboradorData;
        _configuration = configuration;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _usuarioData.GetByEmailAsync(request.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

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

        // Obtener ColaboradorId si el rol es Colaborador
        int? colaboradorId = null;
        if (user.Rol.Equals("Colaborador", StringComparison.OrdinalIgnoreCase))
        {
            var colaborador = await _colaboradorData.GetByUsuarioIdAsync(user.UsuarioId);
            if (colaborador != null)
            {
                colaboradorId = colaborador.ColaboradorId;
            }
        }

        // Generar JWT
        var tokenHandler = new JwtSecurityTokenHandler();
        var secretKey = _configuration["Jwt:Secret"];
        if (string.IsNullOrWhiteSpace(secretKey))
        {
            secretKey = "SuperSecretKeyOfAtLeast32CharactersLong!!!";
        }
        var key = Encoding.ASCII.GetBytes(secretKey);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UsuarioId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Rol)
        };

        if (clienteId.HasValue)
        {
            claims.Add(new Claim("clienteId", clienteId.Value.ToString()));
        }
        if (colaboradorId.HasValue)
        {
            claims.Add(new Claim("colaboradorId", colaboradorId.Value.ToString()));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return new LoginResponse(
            Token: tokenString,
            NombreCompleto: user.NombreCompleto,
            Email: user.Email,
            Roles: new[] { user.Rol },
            ClienteId: clienteId,
            ColaboradorId: colaboradorId
        );
    }
}
