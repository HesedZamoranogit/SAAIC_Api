using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using SAAIC.Application.DTOs.Auth;
using SAAIC.Application.Interfaces.Services;
using SAAIC.Application.Services;
using SAAIC.Domain.Common;
using SAAIC.Domain.Entities;
using SAAIC.Domain.Exceptions;
using SAAIC.Infrastructure.Persistence.Context;
using SAAIC.Tests.Helpers;

namespace SAAIC.Tests.HU1_IniciarSesion;

/// <summary>
/// Pruebas unitarias para el Sprint 1 - Historia de Usuario 1 (HU1): Iniciar sesión.
/// Casos de prueba: CP-HU1-01 a CP-HU1-05.
/// </summary>
public class HU1_IniciarSesionTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly AuthService _authService;

    public HU1_IniciarSesionTests()
    {
        _dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        _userManagerMock = MockUserManagerFactory.Create(_dbContext);
        _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        _emailServiceMock = new Mock<IEmailService>();

        _authService = new AuthService(
            _userManagerMock.Object,
            _jwtTokenGeneratorMock.Object,
            _emailServiceMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    /// <summary>
    /// CP-HU1-01: Inicio de sesión con credenciales válidas.
    /// Precondición: Red ITSON. Usuario A con su contraseña correcta.
    /// Resultado esperado: Entra a la plataforma y ve la pantalla principal con su nombre o número de control.
    /// </summary>
    [Fact]
    public async Task CP_HU1_01_InicioDeSesion_ConCredencialesValidas_DebeEntrarYRetornarDatosDeUsuario()
    {
        // Arrange
        const string idInstitucional = "00000123456";
        const string passwordCorrecto = "Password123!";
        var usuarioA = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = idInstitucional,
            IdInstitucional = idInstitucional,
            Email = "profesor.a@itson.edu.mx",
            NombreCompleto = "Prof. Juan Pérez",
            Active = true
        };

        _dbContext.Users.Add(usuarioA);
        await _dbContext.SaveChangesAsync();

        _userManagerMock
            .Setup(m => m.CheckPasswordAsync(usuarioA, passwordCorrecto))
            .ReturnsAsync(true);

        _userManagerMock
            .Setup(m => m.GetRolesAsync(usuarioA))
            .ReturnsAsync(new List<string> { RolesSistema.Profesor });

        _jwtTokenGeneratorMock
            .Setup(g => g.GenerateToken(usuarioA, It.IsAny<IEnumerable<string>>()))
            .Returns(("token-jwt-valido", DateTime.UtcNow.AddHours(2)));

        var request = new LoginRequestDto(idInstitucional, passwordCorrecto);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Inicio de sesión exitoso.");
        result.Data.Should().NotBeNull();
        result.Data!.Token.Should().Be("token-jwt-valido");
        result.Data.NombreCompleto.Should().Be("Prof. Juan Pérez");
        result.Data.IdInstitucional.Should().Be(idInstitucional);
        result.Data.Roles.Should().Contain(RolesSistema.Profesor);
    }

    /// <summary>
    /// CP-HU1-02: Contraseña incorrecta.
    /// Precondición: Red ITSON. Usuario A con una contraseña errónea.
    /// Resultado esperado: No entra. Mensaje de credenciales incorrectas.
    /// </summary>
    [Fact]
    public async Task CP_HU1_02_InicioDeSesion_ConPasswordIncorrecto_DebeRechazarConMensajeCredencialesInvalidas()
    {
        // Arrange
        const string idInstitucional = "00000123456";
        const string passwordErroneo = "ContrasenaEquivocada!";
        var usuarioA = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = idInstitucional,
            IdInstitucional = idInstitucional,
            NombreCompleto = "Prof. Juan Pérez",
            Active = true
        };

        _dbContext.Users.Add(usuarioA);
        await _dbContext.SaveChangesAsync();

        _userManagerMock
            .Setup(m => m.CheckPasswordAsync(usuarioA, passwordErroneo))
            .ReturnsAsync(false);

        var request = new LoginRequestDto(idInstitucional, passwordErroneo);

        // Act & Assert
        var act = async () => await _authService.LoginAsync(request);

        var exception = await act.Should().ThrowAsync<DomainException>();
        exception.WithMessage("Credenciales inválidas.");
    }

    /// <summary>
    /// CP-HU1-03: Número de control no registrado.
    /// Precondición: Red ITSON. Usuario C con cualquier contraseña.
    /// Resultado esperado: No entra. El mensaje es el mismo que en CP-HU1-02 y no revela si el usuario existe.
    /// </summary>
    [Fact]
    public async Task CP_HU1_03_InicioDeSesion_ConNumeroDeControlNoRegistrado_DebeRechazarConMismoMensajeSinRevelarExistencia()
    {
        // Arrange
        const string idNoRegistrado = "00000999999";
        const string cualquierPassword = "CualquierPassword123!";

        var request = new LoginRequestDto(idNoRegistrado, cualquierPassword);

        // Act & Assert
        var act = async () => await _authService.LoginAsync(request);

        var exception = await act.Should().ThrowAsync<DomainException>();
        // El mensaje debe ser idéntico al de contraseña incorrecta (CP-HU1-02)
        exception.WithMessage("Credenciales inválidas.");
    }

    /// <summary>
    /// CP-HU1-04: Campos obligatorios vacíos.
    /// Precondición: Red ITSON. Probar tres combinaciones: ambos vacíos, solo número de control vacío, solo contraseña vacía.
    /// Resultado esperado: No envía el formulario / rechaza la petición. Indica qué campo falta.
    /// </summary>
    [Theory]
    [InlineData("", "", "Debe ingresar el ID Institucional y la contraseña.")]
    [InlineData("   ", "   ", "Debe ingresar el ID Institucional y la contraseña.")]
    [InlineData("", "Password123!", "El ID Institucional es obligatorio.")]
    [InlineData("   ", "Password123!", "El ID Institucional es obligatorio.")]
    [InlineData("00000123456", "", "La contraseña es obligatoria.")]
    [InlineData("00000123456", "   ", "La contraseña es obligatoria.")]
    public async Task CP_HU1_04_InicioDeSesion_ConCamposVacios_DebeIndicarCampoFaltante(
        string idInstitucional,
        string password,
        string mensajeEsperado)
    {
        // Arrange
        var request = new LoginRequestDto(idInstitucional, password);

        // Act & Assert
        var act = async () => await _authService.LoginAsync(request);

        var exception = await act.Should().ThrowAsync<DomainException>();
        exception.WithMessage(mensajeEsperado);
    }

    /// <summary>
    /// CP-HU1-05: La contraseña distingue mayúsculas y minúsculas.
    /// Precondición: Red ITSON. Usuario A. Contraseña correcta con una letra cambiada de mayúscula a minúscula.
    /// Resultado esperado: No entra, igual que en CP-HU1-02.
    /// </summary>
    [Fact]
    public async Task CP_HU1_05_InicioDeSesion_PasswordDistingueMayusculasYMinusculas_DebeRechazarComoInvalida()
    {
        // Arrange
        const string idInstitucional = "00000123456";
        const string passwordCorrecto = "Password123!";
        const string passwordConLetraCambiadoCase = "password123!"; // 'P' cambiada a 'p'

        var usuarioA = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = idInstitucional,
            IdInstitucional = idInstitucional,
            NombreCompleto = "Prof. Juan Pérez",
            Active = true
        };

        _dbContext.Users.Add(usuarioA);
        await _dbContext.SaveChangesAsync();

        // El PasswordHasher y CheckPasswordAsync devuelven true solo para el password exacto
        _userManagerMock
            .Setup(m => m.CheckPasswordAsync(usuarioA, passwordCorrecto))
            .ReturnsAsync(true);

        _userManagerMock
            .Setup(m => m.CheckPasswordAsync(usuarioA, passwordConLetraCambiadoCase))
            .ReturnsAsync(false);

        var request = new LoginRequestDto(idInstitucional, passwordConLetraCambiadoCase);

        // Act & Assert
        var act = async () => await _authService.LoginAsync(request);

        var exception = await act.Should().ThrowAsync<DomainException>();
        exception.WithMessage("Credenciales inválidas.");
    }
}
