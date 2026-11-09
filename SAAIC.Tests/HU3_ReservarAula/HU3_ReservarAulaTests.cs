using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using SAAIC.Application.DTOs.Reservas;
using SAAIC.Application.Interfaces.Persistence;
using SAAIC.Application.Services;
using SAAIC.Domain.Entities;
using SAAIC.Domain.Enums;
using SAAIC.Domain.Exceptions;

namespace SAAIC.Tests.HU3_ReservarAula;

/// <summary>
/// Pruebas unitarias para el Sprint 1 - Historia de Usuario 3 (HU3): Reservar un aula.
/// Casos de prueba: CP-HU3-01 a CP-HU3-05.
/// </summary>
public class HU3_ReservarAulaTests
{
    private readonly Mock<IReservaRepository> _reservaRepositoryMock;
    private readonly Mock<IEspacioRepository> _espacioRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly ReservaService _reservaService;

    public HU3_ReservarAulaTests()
    {
        _reservaRepositoryMock = new Mock<IReservaRepository>();
        _espacioRepositoryMock = new Mock<IEspacioRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _reservaService = new ReservaService(
            _reservaRepositoryMock.Object,
            _espacioRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _userManagerMock.Object);
    }

    private static Espacio CrearAula101()
    {
        var campus = new Campus { Id = 1, Nombre = "Campus Náinari" };
        var tipoAula = new TipoEspacio { Id = 1, Nombre = "Aula" };

        return new Espacio
        {
            Id = 101,
            Nombre = "Aula 101",
            IdCampus = 1,
            Campus = campus,
            IdTipoEspacio = 1,
            TipoEspacio = tipoAula,
            CapacidadMaxima = 40,
            HoraApertura = new TimeOnly(7, 0),
            HoraCierre = new TimeOnly(21, 0),
            Estado = EstadoEspacio.Disponible,
            Active = true
        };
    }

    /// <summary>
    /// CP-HU3-01: Reserva válida.
    /// Precondición: Aula 101 sin reservas en la fecha F.
    /// Pasos: 1) Abrir Reservar aula. 2) Elegir Aula 101, fecha F, 10:00 a 12:00.
    ///        3) Escribir responsable R1 y motivo "Ensayo de teatro". 4) Guardar.
    /// Resultado esperado: Se registra y muestra confirmación con el resumen. La reserva aparece al consultar el Aula 101 en la fecha F.
    /// </summary>
    [Fact]
    public async Task CP_HU3_01_ReservaValida_DebeRegistrarYConfirmarConResumen()
    {
        // Arrange
        const int idAula101 = 101;
        const string usuarioId = "user-123";
        const string responsableR1 = "Prof. Roberto Gómez";
        const string motivoEnsayo = "Ensayo de teatro";
        var fechaF = DateOnly.FromDateTime(DateTime.Today.AddDays(7));
        var horaInicio = new TimeOnly(10, 0);
        var horaFin = new TimeOnly(12, 0);

        var aula101 = CrearAula101();

        _espacioRepositoryMock
            .Setup(r => r.ObtenerConBloqueoPesimistaAsync(idAula101, It.IsAny<CancellationToken>()))
            .ReturnsAsync(aula101);

        // Sin colisiones de horario en la fecha F
        _reservaRepositoryMock
            .Setup(r => r.ExisteConflictoHorarioAsync(
                idAula101, fechaF, horaInicio, horaFin, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        Reserva? reservaGuardada = null;
        _reservaRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Reserva>(), It.IsAny<CancellationToken>()))
            .Callback<Reserva, CancellationToken>((r, _) =>
            {
                r.Id = 1;
                reservaGuardada = r;
            })
            .ReturnsAsync((Reserva r, CancellationToken _) => r);

        _reservaRepositoryMock
            .Setup(r => r.ObtenerConDetallesPorIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => reservaGuardada);

        var request = new CrearReservaPorPeriodoRequestDto(
            IdEspacio: idAula101,
            ResponsableEvento: responsableR1,
            Descripcion: motivoEnsayo,
            MaximoAsistentes: 30,
            Fecha: fechaF,
            HoraInicio: horaInicio,
            HoraFin: horaFin);

        // Act
        var result = await _reservaService.CrearReservaPorPeriodoAsync(request, usuarioId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Reservación por periodo asignada exitosamente.");

        result.Data.Should().NotBeNull();
        var resumen = result.Data!;
        resumen.IdEspacio.Should().Be(idAula101);
        resumen.ResponsableEvento.Should().Be(responsableR1);
        resumen.Descripcion.Should().Be(motivoEnsayo);
        resumen.FechaInicio.Should().Be(fechaF);
        resumen.HoraInicio.Should().Be(horaInicio);
        resumen.HoraFin.Should().Be(horaFin);
        resumen.Estado.Should().Be(EstadoReserva.Confirmada);

        // Verificar que se persistió y se confirmó la transacción
        _reservaRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Reserva>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// CP-HU3-02: Campos del formulario.
    /// Precondición: Ninguna.
    /// Pasos: 1) Abrir Reservar aula.
    /// Resultado esperado: El formulario pide aula, fecha, horario de inicio y de fin, responsable y motivo.
    /// </summary>
    [Fact]
    public void CP_HU3_02_CamposDelFormulario_DebeContenerAulaFechaHorarioResponsableYMotivo()
    {
        // Arrange & Act
        var requestType = typeof(CrearReservaPorPeriodoRequestDto);
        var propiedades = requestType.GetProperties().Select(p => p.Name).ToList();

        // Assert - Verificar que el contrato DTO del formulario contiene todos los campos exigidos
        propiedades.Should().Contain(nameof(CrearReservaPorPeriodoRequestDto.IdEspacio), "debe pedir aula");
        propiedades.Should().Contain(nameof(CrearReservaPorPeriodoRequestDto.Fecha), "debe pedir fecha");
        propiedades.Should().Contain(nameof(CrearReservaPorPeriodoRequestDto.HoraInicio), "debe pedir horario de inicio");
        propiedades.Should().Contain(nameof(CrearReservaPorPeriodoRequestDto.HoraFin), "debe pedir horario de fin");
        propiedades.Should().Contain(nameof(CrearReservaPorPeriodoRequestDto.ResponsableEvento), "debe pedir responsable");
        propiedades.Should().Contain(nameof(CrearReservaPorPeriodoRequestDto.Descripcion), "debe pedir motivo/descripción");
    }

    /// <summary>
    /// CP-HU3-03: La capacidad del aula es visible.
    /// Precondición: Aula 101 con aforo 40.
    /// Pasos: 1) Elegir Aula 101 en el formulario.
    /// Resultado esperado: El formulario muestra el aforo de 40 personas, tomado del catálogo.
    ///                    Además, valida que no se permitan reservas que excedan dicha capacidad.
    /// </summary>
    [Fact]
    public void CP_HU3_03_CapacidadDelAula_VisibleYValidadaSegunCatalogo()
    {
        // Arrange
        var aula101 = CrearAula101();
        aula101.CapacidadMaxima.Should().Be(40, "el aforo del Aula 101 registrado en catálogo debe ser 40");

        var fechaF = DateOnly.FromDateTime(DateTime.Today.AddDays(3));

        // Act & Assert 1: Reserva dentro de la capacidad (<= 40) debe ser válida
        var reservaValida = Reserva.CrearPorPeriodo(
            aula101,
            "user-1",
            "Profesor",
            "Clase",
            maximoAsistentes: 40,
            fecha: fechaF,
            horaInicio: new TimeOnly(10, 0),
            horaFin: new TimeOnly(12, 0));

        reservaValida.MaximoAsistentes.Should().Be(40);

        // Act & Assert 2: Reserva que exceda la capacidad máxima (> 40) debe ser rechazada
        var actExcedeCapacidad = () => Reserva.CrearPorPeriodo(
            aula101,
            "user-1",
            "Profesor",
            "Evento Masivo",
            maximoAsistentes: 45,
            fecha: fechaF,
            horaInicio: new TimeOnly(10, 0),
            horaFin: new TimeOnly(12, 0));

        actExcedeCapacidad.Should().Throw<DomainException>()
            .WithMessage("*supera la capacidad máxima permitida para el espacio 'Aula 101' (40 personas)*");
    }

    /// <summary>
    /// CP-HU3-04: Campos obligatorios vacíos.
    /// Precondición: Ninguna.
    /// Pasos: 1) Dejar vacío un campo obligatorio a la vez, empezando por aula y siguiendo con fecha, horario, responsable y motivo.
    ///        2) Guardar.
    /// Resultado esperado: No guarda. Marca el campo que falta junto a él y conserva lo ya escrito.
    /// </summary>
    [Fact]
    public async Task CP_HU3_04_CamposObligatoriosVacios_AulaVacia_DebeRechazarEIndicarCampoFaltante()
    {
        // Arrange - Aula vacía / no seleccionada (IdEspacio = 0)
        var request = new CrearReservaPorPeriodoRequestDto(
            IdEspacio: 0,
            ResponsableEvento: "Prof. Roberto Gómez",
            Descripcion: "Ensayo de teatro",
            MaximoAsistentes: 20,
            Fecha: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            HoraInicio: new TimeOnly(10, 0),
            HoraFin: new TimeOnly(12, 0));

        // Act & Assert
        var act = async () => await _reservaService.CrearReservaPorPeriodoAsync(request, "user-1");

        var exception = await act.Should().ThrowAsync<DomainException>();
        exception.WithMessage("El aula o espacio a reservar es obligatorio.");
    }

    [Theory]
    [InlineData("", "Ensayo de teatro", "El nombre del responsable del evento es obligatorio.")]
    [InlineData("   ", "Ensayo de teatro", "El nombre del responsable del evento es obligatorio.")]
    [InlineData("Prof. Roberto", "", "La descripción del evento es obligatoria.")]
    [InlineData("Prof. Roberto", "   ", "La descripción del evento es obligatoria.")]
    public void CP_HU3_04_CamposObligatoriosVacios_ResponsableOMotivoVacio_DebeRechazarEIndicarCampo(
        string responsable,
        string motivo,
        string mensajeEsperado)
    {
        // Arrange
        var aula101 = CrearAula101();
        var fecha = DateOnly.FromDateTime(DateTime.Today.AddDays(2));

        // Act & Assert
        var act = () => Reserva.CrearPorPeriodo(
            aula101,
            "user-1",
            responsable,
            motivo,
            maximoAsistentes: 20,
            fecha: fecha,
            horaInicio: new TimeOnly(10, 0),
            horaFin: new TimeOnly(12, 0));

        act.Should().Throw<DomainException>()
            .WithMessage(mensajeEsperado);
    }

    /// <summary>
    /// CP-HU3-05: Fecha pasada u horario inválido.
    /// Precondición: Fecha de ayer; después fecha F con fin igual al inicio (10:00 a 10:00) y con fin anterior al inicio (12:00 a 10:00).
    /// Pasos: 1) Capturar cada combinación. 2) Guardar.
    /// Resultado esperado: Rechaza las tres con un mensaje claro y no registra ninguna reserva.
    /// </summary>
    [Fact]
    public void CP_HU3_05_Combinacion1_FechaPasada_DebeRechazarConMensajeClaro()
    {
        // Arrange - Fecha de ayer (fecha pasada)
        var aula101 = CrearAula101();
        var fechaAyer = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));

        // Act & Assert
        var act = () => Reserva.CrearPorPeriodo(
            aula101,
            "user-1",
            "Prof. Roberto",
            "Ensayo de teatro",
            maximoAsistentes: 20,
            fecha: fechaAyer,
            horaInicio: new TimeOnly(10, 0),
            horaFin: new TimeOnly(12, 0));

        act.Should().Throw<DomainException>()
            .WithMessage("No se pueden realizar reservaciones en fechas pasadas.");
    }

    [Fact]
    public void CP_HU3_05_Combinacion2_FinIgualAlInicio_DebeRechazarConMensajeClaro()
    {
        // Arrange - Fin igual al inicio (10:00 a 10:00)
        var aula101 = CrearAula101();
        var fechaF = DateOnly.FromDateTime(DateTime.Today.AddDays(5));
        var horaMisma = new TimeOnly(10, 0);

        // Act & Assert
        var act = () => Reserva.CrearPorPeriodo(
            aula101,
            "user-1",
            "Prof. Roberto",
            "Ensayo de teatro",
            maximoAsistentes: 20,
            fecha: fechaF,
            horaInicio: horaMisma,
            horaFin: horaMisma);

        act.Should().Throw<DomainException>()
            .WithMessage("La hora de inicio debe ser estrictamente menor a la hora de finalización.");
    }

    [Fact]
    public void CP_HU3_05_Combinacion3_FinAnteriorAlInicio_DebeRechazarConMensajeClaro()
    {
        // Arrange - Fin anterior al inicio (12:00 a 10:00)
        var aula101 = CrearAula101();
        var fechaF = DateOnly.FromDateTime(DateTime.Today.AddDays(5));
        var horaInicio = new TimeOnly(12, 0);
        var horaFin = new TimeOnly(10, 0);

        // Act & Assert
        var act = () => Reserva.CrearPorPeriodo(
            aula101,
            "user-1",
            "Prof. Roberto",
            "Ensayo de teatro",
            maximoAsistentes: 20,
            fecha: fechaF,
            horaInicio: horaInicio,
            horaFin: horaFin);

        act.Should().Throw<DomainException>()
            .WithMessage("La hora de inicio debe ser estrictamente menor a la hora de finalización.");
    }
}
