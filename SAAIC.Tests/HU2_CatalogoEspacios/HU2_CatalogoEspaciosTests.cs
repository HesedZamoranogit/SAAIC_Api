using FluentAssertions;
using Moq;
using SAAIC.Application.Common.Models;
using SAAIC.Application.DTOs.Espacios;
using SAAIC.Application.Interfaces.Persistence;
using SAAIC.Application.Services;
using SAAIC.Domain.Entities;
using SAAIC.Domain.Enums;

namespace SAAIC.Tests.HU2_CatalogoEspacios;

/// <summary>
/// Pruebas unitarias para el Sprint 1 - Historia de Usuario 2 (HU2): Catálogo de espacios y aulas.
/// Casos de prueba: CP-HU2-01 a CP-HU2-03.
/// </summary>
public class HU2_CatalogoEspaciosTests
{
    private readonly Mock<IEspacioRepository> _espacioRepositoryMock;
    private readonly Mock<IRepository<Campus>> _campusRepositoryMock;
    private readonly Mock<IRepository<TipoEspacio>> _tipoEspacioRepositoryMock;
    private readonly EspacioService _espacioService;

    public HU2_CatalogoEspaciosTests()
    {
        _espacioRepositoryMock = new Mock<IEspacioRepository>();
        _campusRepositoryMock = new Mock<IRepository<Campus>>();
        _tipoEspacioRepositoryMock = new Mock<IRepository<TipoEspacio>>();

        _espacioService = new EspacioService(
            _espacioRepositoryMock.Object,
            _campusRepositoryMock.Object,
            _tipoEspacioRepositoryMock.Object);
    }

    /// <summary>
    /// CP-HU2-01: Ver el catálogo completo.
    /// Precondición: Sesión iniciada. Aula 101, Foro 1, Taller 1 y Bodega 1 registrados.
    /// Pasos: Entrar a la opción Catálogo de espacios.
    /// Resultado esperado: Lista los cuatro espacios, uno de cada tipo, sin filtros aplicados.
    /// </summary>
    [Fact]
    public async Task CP_HU2_01_CatalogoCompleto_SinFiltros_DebeListarLosCuatroEspaciosUnoDeCadaTipo()
    {
        // Arrange
        var campusNainari = new Campus { Id = 1, Nombre = "Campus Náinari" };

        var tipoAula = new TipoEspacio { Id = 1, Nombre = "Aula" };
        var tipoForo = new TipoEspacio { Id = 2, Nombre = "Foro" };
        var tipoTaller = new TipoEspacio { Id = 3, Nombre = "Taller" };
        var tipoBodega = new TipoEspacio { Id = 4, Nombre = "Bodega" };

        var espaciosRegistrados = new List<Espacio>
        {
            new()
            {
                Id = 101,
                Nombre = "Aula 101",
                IdTipoEspacio = tipoAula.Id,
                TipoEspacio = tipoAula,
                IdCampus = campusNainari.Id,
                Campus = campusNainari,
                CapacidadMaxima = 40,
                HoraApertura = new TimeOnly(7, 0),
                HoraCierre = new TimeOnly(21, 0),
                Estado = EstadoEspacio.Disponible,
                Active = true
            },
            new()
            {
                Id = 201,
                Nombre = "Foro 1",
                IdTipoEspacio = tipoForo.Id,
                TipoEspacio = tipoForo,
                IdCampus = campusNainari.Id,
                Campus = campusNainari,
                CapacidadMaxima = 120,
                HoraApertura = new TimeOnly(8, 0),
                HoraCierre = new TimeOnly(20, 0),
                Estado = EstadoEspacio.Disponible,
                Active = true
            },
            new()
            {
                Id = 301,
                Nombre = "Taller 1",
                IdTipoEspacio = tipoTaller.Id,
                TipoEspacio = tipoTaller,
                IdCampus = campusNainari.Id,
                Campus = campusNainari,
                CapacidadMaxima = 25,
                HoraApertura = new TimeOnly(7, 0),
                HoraCierre = new TimeOnly(19, 0),
                Estado = EstadoEspacio.Disponible,
                Active = true
            },
            new()
            {
                Id = 401,
                Nombre = "Bodega 1",
                IdTipoEspacio = tipoBodega.Id,
                TipoEspacio = tipoBodega,
                IdCampus = campusNainari.Id,
                Campus = campusNainari,
                CapacidadMaxima = 10,
                HoraApertura = new TimeOnly(8, 0),
                HoraCierre = new TimeOnly(18, 0),
                Estado = EstadoEspacio.Disponible,
                Active = true
            }
        };

        var pagedResult = new PagedResult<Espacio>
        {
            Items = espaciosRegistrados,
            PageNumber = 1,
            PageSize = 10,
            TotalRecords = 4,
            TotalPages = 1
        };

        _espacioRepositoryMock
            .Setup(r => r.ObtenerConFiltrosPaginadoAsync(
                null, null, null, true, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act - Consultar catálogo sin filtros
        var result = await _espacioService.ObtenerConFiltrosAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(4);

        // Verificar que lista los 4 espacios con sus respectivos nombres y tipos
        var nombres = result.Data.Items.Select(e => e.Nombre).ToList();
        nombres.Should().Contain(new[] { "Aula 101", "Foro 1", "Taller 1", "Bodega 1" });

        var tipos = result.Data.Items.Select(e => e.NombreTipoEspacio).Distinct().ToList();
        tipos.Should().HaveCount(4);
        tipos.Should().Contain(new[] { "Aula", "Foro", "Taller", "Bodega" });
    }

    /// <summary>
    /// CP-HU2-02: Datos que muestra cada espacio.
    /// Precondición: Cuatro espacios registrados (Aula 101, Foro 1, Taller 1, Bodega 1).
    /// Pasos: Revisar cada fila o tarjeta del listado.
    /// Resultado esperado: Cada espacio muestra nombre o número, campus y aforo máximo, y ningún dato aparece vacío.
    /// </summary>
    [Fact]
    public async Task CP_HU2_02_DatosQueMuestraCadaEspacio_CadaFilaDebeMostrarNombreCampusYAforoSinDatosVacios()
    {
        // Arrange
        var campusNainari = new Campus { Id = 1, Nombre = "Campus Náinari" };

        var espacios = new List<Espacio>
        {
            new()
            {
                Id = 1,
                Nombre = "Aula 101",
                Campus = campusNainari,
                IdCampus = 1,
                TipoEspacio = new TipoEspacio { Id = 1, Nombre = "Aula" },
                IdTipoEspacio = 1,
                CapacidadMaxima = 40,
                HoraApertura = new TimeOnly(7, 0),
                HoraCierre = new TimeOnly(21, 0),
                Estado = EstadoEspacio.Disponible,
                Active = true
            },
            new()
            {
                Id = 2,
                Nombre = "Foro 1",
                Campus = campusNainari,
                IdCampus = 1,
                TipoEspacio = new TipoEspacio { Id = 2, Nombre = "Foro" },
                IdTipoEspacio = 2,
                CapacidadMaxima = 120,
                HoraApertura = new TimeOnly(8, 0),
                HoraCierre = new TimeOnly(20, 0),
                Estado = EstadoEspacio.Disponible,
                Active = true
            },
            new()
            {
                Id = 3,
                Nombre = "Taller 1",
                Campus = campusNainari,
                IdCampus = 1,
                TipoEspacio = new TipoEspacio { Id = 3, Nombre = "Taller" },
                IdTipoEspacio = 3,
                CapacidadMaxima = 25,
                HoraApertura = new TimeOnly(7, 0),
                HoraCierre = new TimeOnly(19, 0),
                Estado = EstadoEspacio.Disponible,
                Active = true
            },
            new()
            {
                Id = 4,
                Nombre = "Bodega 1",
                Campus = campusNainari,
                IdCampus = 1,
                TipoEspacio = new TipoEspacio { Id = 4, Nombre = "Bodega" },
                IdTipoEspacio = 4,
                CapacidadMaxima = 10,
                HoraApertura = new TimeOnly(8, 0),
                HoraCierre = new TimeOnly(18, 0),
                Estado = EstadoEspacio.Disponible,
                Active = true
            }
        };

        var pagedResult = new PagedResult<Espacio>
        {
            Items = espacios,
            PageNumber = 1,
            PageSize = 10,
            TotalRecords = 4,
            TotalPages = 1
        };

        _espacioRepositoryMock
            .Setup(r => r.ObtenerConFiltrosPaginadoAsync(
                null, null, null, true, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _espacioService.ObtenerConFiltrosAsync();

        // Assert
        result.Data!.Items.Should().NotBeEmpty();

        foreach (var espacio in result.Data.Items)
        {
            // Cada espacio muestra nombre o número
            espacio.Nombre.Should().NotBeNullOrWhiteSpace();

            // Campus presente
            espacio.IdCampus.Should().BeGreaterThan(0);
            espacio.NombreCampus.Should().NotBeNullOrWhiteSpace();

            // Aforo máximo mayor a cero
            espacio.CapacidadMaxima.Should().BeGreaterThan(0);

            // Tipo de espacio y estado presentes
            espacio.NombreTipoEspacio.Should().NotBeNullOrWhiteSpace();
            espacio.EstadoDescripcion.Should().NotBeNullOrWhiteSpace();
        }
    }

    /// <summary>
    /// CP-HU2-03: Los datos coinciden con el registro.
    /// Precondición: Aula 101 registrada con campus 1 y aforo 40.
    /// Pasos: 1) Buscar el Aula 101. 2) Comparar sus datos con el registro de origen.
    /// Resultado esperado: Campus 1 y aforo 40, sin diferencias.
    /// </summary>
    [Fact]
    public async Task CP_HU2_03_DatosCoincidenConElRegistro_Aula101DebeTenerCampus1YAforo40()
    {
        // Arrange
        const int idAula101 = 101;
        const int campusEsperado = 1;
        const int aforoEsperado = 40;

        var campus = new Campus { Id = campusEsperado, Nombre = "Campus Náinari", Codigo = "COB-NAI" };
        var tipoAula = new TipoEspacio { Id = 1, Nombre = "Aula" };

        var aula101 = new Espacio
        {
            Id = idAula101,
            Nombre = "Aula 101",
            IdCampus = campusEsperado,
            Campus = campus,
            IdTipoEspacio = tipoAula.Id,
            TipoEspacio = tipoAula,
            CapacidadMaxima = aforoEsperado,
            HoraApertura = new TimeOnly(7, 0),
            HoraCierre = new TimeOnly(21, 0),
            Estado = EstadoEspacio.Disponible,
            Active = true
        };

        _espacioRepositoryMock
            .Setup(r => r.ObtenerConDetallesPorIdAsync(idAula101, It.IsAny<CancellationToken>()))
            .ReturnsAsync(aula101);

        // Act - Buscar Aula 101
        var result = await _espacioService.ObtenerPorIdAsync(idAula101);

        // Assert - Comparar con el registro de origen
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();

        var espacioDto = result.Data!;
        espacioDto.Nombre.Should().Be("Aula 101");
        espacioDto.IdCampus.Should().Be(campusEsperado, "el campus debe ser 1");
        espacioDto.CapacidadMaxima.Should().Be(aforoEsperado, "el aforo debe ser exactamente 40");
        espacioDto.NombreCampus.Should().Be("Campus Náinari");
        espacioDto.NombreTipoEspacio.Should().Be("Aula");
    }
}
