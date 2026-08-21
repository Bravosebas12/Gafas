using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using Optica.Domain.Comun;

namespace Optica.Application.Comportamientos;

/// <summary>
/// Registra el inicio y el desenlace de cada caso de uso, dentro de un ámbito que lleva el
/// identificador de correlación.
///
/// El identificador se toma del <see cref="Activity"/> en curso, que ASP.NET Core crea por petición
/// y propaga: es la misma traza que ya viaja en las cabeceras, así que un caso de uso invocado desde
/// una petición HTTP comparte identificador con todo lo demás que esa petición produzca. Inventar
/// una abstracción propia habría creado un segundo identificador que nadie correlaciona con el
/// primero.
///
/// **Nunca registra la petición completa.** Registra su nombre. La diferencia importa: un comando de
/// ingreso lleva la contraseña dentro, y volcar el objeto es la forma más común de filtrarla
/// (compuerta G8).
/// </summary>
/// <typeparam name="TPeticion">Comando o consulta.</typeparam>
/// <typeparam name="TRespuesta">Resultado que devuelve.</typeparam>
public sealed class ComportamientoDeLog<TPeticion, TRespuesta>(
    ILogger<ComportamientoDeLog<TPeticion, TRespuesta>> registro)
    : IPipelineBehavior<TPeticion, TRespuesta>
    where TPeticion : notnull
    where TRespuesta : Resultado
{
    /// <inheritdoc />
    public async Task<TRespuesta> Handle(
        TPeticion peticion,
        RequestHandlerDelegate<TRespuesta> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        var operacion = typeof(TPeticion).Name;
        var correlacion = Activity.Current?.TraceId.ToString() ?? "sin-correlacion";

        using var ambito = registro.BeginScope(new Dictionary<string, object>
        {
            ["Operacion"] = operacion,
            ["Correlacion"] = correlacion,
        });

        var cronometro = Stopwatch.StartNew();

        try
        {
            var respuesta = await next(cancellationToken).ConfigureAwait(false);
            cronometro.Stop();

            if (respuesta.EsCorrecto)
            {
                registro.OperacionCorrecta(operacion, cronometro.ElapsedMilliseconds);
            }
            else
            {
                // El código del error, nunca su mensaje ni la petición: el código es estable y no
                // transporta datos de quien la envió.
                registro.OperacionRechazada(
                    operacion, respuesta.Error.Codigo, cronometro.ElapsedMilliseconds);
            }

            return respuesta;
        }
        catch (Exception excepcion)
        {
            cronometro.Stop();
            registro.OperacionConFallo(operacion, cronometro.ElapsedMilliseconds, excepcion);
            throw;
        }
    }
}

/// <summary>
/// Mensajes de registro declarados con generación en tiempo de compilación.
///
/// Se usa el generador en lugar de llamadas directas para que el mensaje quede fijado, con su
/// identificador y su nivel, y no se degrade en una interpolación que meta datos por accidente.
/// </summary>
internal static partial class MensajesDeLog
{
    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Information,
        Message = "Operación {Operacion} completada en {Milisegundos} ms")]
    public static partial void OperacionCorrecta(
        this ILogger registro, string operacion, long milisegundos);

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Operación {Operacion} rechazada con código {CodigoDeError} en {Milisegundos} ms")]
    public static partial void OperacionRechazada(
        this ILogger registro, string operacion, string codigoDeError, long milisegundos);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Error,
        Message = "Operación {Operacion} falló con excepción tras {Milisegundos} ms")]
    public static partial void OperacionConFallo(
        this ILogger registro, string operacion, long milisegundos, Exception excepcion);
}
