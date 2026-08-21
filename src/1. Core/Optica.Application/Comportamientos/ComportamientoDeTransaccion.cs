using MediatR;
using Optica.Application.Abstracciones;
using Optica.Domain.Comun;

namespace Optica.Application.Comportamientos;

/// <summary>
/// Envuelve cada **comando** en una transacción explícita. Las consultas no pasan por aquí.
///
/// Es el mecanismo que sostiene la compuerta G7: el registro de auditoría lo escribe un interceptor
/// de persistencia durante el guardado, así que queda dentro de esta misma transacción sin que el
/// handler tenga que coordinarlo. Si la operación falla, la auditoría se revierte con ella y no
/// queda una fila afirmando un cambio que nunca ocurrió.
///
/// Un resultado **fallido no revierte**: un fallo de negocio previsto —credenciales incorrectas,
/// último administrador— es una salida normal, y en varios casos debe persistir lo que ya escribió,
/// como el intento fallido que FR-006 obliga a registrar. Solo una excepción revierte.
/// </summary>
/// <typeparam name="TComando">Comando en curso.</typeparam>
/// <typeparam name="TRespuesta">Resultado que devuelve.</typeparam>
public sealed class ComportamientoDeTransaccion<TComando, TRespuesta>(IUnidadDeTrabajo unidadDeTrabajo)
    : IPipelineBehavior<TComando, TRespuesta>
    where TComando : IComando<TRespuesta>
    where TRespuesta : Resultado
{
    /// <inheritdoc />
    public Task<TRespuesta> Handle(
        TComando comando,
        RequestHandlerDelegate<TRespuesta> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        return unidadDeTrabajo.EjecutarEnTransaccion(
            async token =>
            {
                var respuesta = await next(token).ConfigureAwait(false);
                await unidadDeTrabajo.GuardarCambios(token).ConfigureAwait(false);
                return respuesta;
            },
            cancellationToken);
    }
}
