namespace SAAIC.Domain.Enums;

/// <summary>
/// Días de la semana combinables mediante banderas (Flags) para configurar reservaciones recurrentes semestrales.
/// Ejemplo: DiasSemana.Lunes | DiasSemana.Miercoles | DiasSemana.Viernes
/// </summary>
[Flags]
public enum DiasSemana
{
    Ninguno = 0,
    Lunes = 1 << 0,      // 1
    Martes = 1 << 1,     // 2
    Miercoles = 1 << 2,  // 4
    Jueves = 1 << 3,     // 8
    Viernes = 1 << 4,    // 16
    Sabado = 1 << 5,     // 32
    Domingo = 1 << 6     // 64
}

public static class DiasSemanaExtensions
{
    /// <summary>
    /// Convierte el DayOfWeek nativo de .NET a su bandera correspondiente en DiasSemana.
    /// </summary>
    public static DiasSemana ToDiasSemanaFlag(this DayOfWeek dayOfWeek) => dayOfWeek switch
    {
        DayOfWeek.Monday => DiasSemana.Lunes,
        DayOfWeek.Tuesday => DiasSemana.Martes,
        DayOfWeek.Wednesday => DiasSemana.Miercoles,
        DayOfWeek.Thursday => DiasSemana.Jueves,
        DayOfWeek.Friday => DiasSemana.Viernes,
        DayOfWeek.Saturday => DiasSemana.Sabado,
        DayOfWeek.Sunday => DiasSemana.Domingo,
        _ => DiasSemana.Ninguno
    };

    /// <summary>
    /// Verifica si una fecha específica coincide con alguno de los días seleccionados en la máscara.
    /// </summary>
    public static bool IncluyeFecha(this DiasSemana diasSeleccionados, DateOnly fecha)
    {
        var flagDia = fecha.DayOfWeek.ToDiasSemanaFlag();
        return flagDia != DiasSemana.Ninguno && diasSeleccionados.HasFlag(flagDia);
    }
}
