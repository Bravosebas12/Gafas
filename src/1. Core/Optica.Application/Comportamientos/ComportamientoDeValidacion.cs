using System.Reflection;
using FluentValidation;
using MediatR;
using Optica.Domain.Comun;

namespace Optica.Application.Comportamientos;

/// <summary>
/// Ejecuta los validadores de la petición **antes** del handler y corta el canal si alguno falla.
///
/// Corta devolviendo un <see cref="Resultado"/> fallido, no lanzando una excepción. La diferencia
/// no es de estilo: el principio I prohíbe usar excepciones como control de flujo, y una petición
/// mal formada es un resultado previsto de un endpoint anónimo, no una condición excepcional.
/// </summary>
/// <typeparam name="TPeticion">Comando o consulta.</typeparam>
/// <typeparam name="TRespuesta">Resultado que devuelve, siempre un <see cref="Resultado"/>.</typeparam>
public sealed class ComportamientoDeValidacion<TPeticion, TRespuesta>(
    IEnumerable<IValidator<TPeticion>> validadores)
    : IPipelineBehavior<TPeticion, TRespuesta>
    where TPeticion : notnull
    where TRespuesta : Resultado
{
    /// <summary>
    /// Construir un resultado fallido del tipo cerrado que el canal espera exige reflexión, porque
    /// <c>Resultado.Fallo</c> es genérico y aquí solo se conoce <typeparamref name="TRespuesta"/>.
    /// Se resuelve **una vez por tipo cerrado**, en la inicialización del campo estático, no en cada
    /// petición.
    /// </summary>
    private static readonly Func<ErrorDeDominio, TRespuesta> CrearFallo = ConstruirFabricaDeFallo();

    /// <inheritdoc />
    public async Task<TRespuesta> Handle(
        TPeticion peticion,
        RequestHandlerDelegate<TRespuesta> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        var fallos = await RecogerFallos(peticion, cancellationToken).ConfigureAwait(false);

        if (fallos.Count > 0)
        {
            return CrearFallo(ErroresDeValidacion.PeticionInvalida(fallos));
        }

        return await next(cancellationToken).ConfigureAwait(false);
    }

    private async Task<List<string>> RecogerFallos(TPeticion peticion, CancellationToken cancellationToken)
    {
        var fallos = new List<string>();
        var contexto = new ValidationContext<TPeticion>(peticion);

        foreach (var validador in validadores)
        {
            var resultado = await validador.ValidateAsync(contexto, cancellationToken).ConfigureAwait(false);

            foreach (var error in resultado.Errors)
            {
                fallos.Add(error.ErrorMessage);
            }
        }

        return fallos;
    }

    private static Func<ErrorDeDominio, TRespuesta> ConstruirFabricaDeFallo()
    {
        if (typeof(TRespuesta) == typeof(Resultado))
        {
            return error => (TRespuesta)Resultado.Fallo(error);
        }

        var tipoDelValor = typeof(TRespuesta).GetGenericArguments()[0];
        var falloGenerico = typeof(Resultado)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(metodo => metodo.Name == nameof(Resultado.Fallo) && metodo.IsGenericMethod)
            .MakeGenericMethod(tipoDelValor);

        return error => (TRespuesta)falloGenerico.Invoke(null, [error])!;
    }
}

/// <summary>Fallos que produce la validación previa al handler.</summary>
public static class ErroresDeValidacion
{
    /// <summary>Código estable del fallo de validación, que la presentación traduce a 400.</summary>
    public const string Codigo = "peticion-invalida";

    /// <summary>
    /// Construye el fallo agregando los mensajes de todos los validadores.
    /// </summary>
    /// <param name="fallos">Mensajes de los validadores, en el orden en que se produjeron.</param>
    /// <returns>El error con el código estable y los mensajes unidos.</returns>
    public static ErrorDeDominio PeticionInvalida(IEnumerable<string> fallos) =>
        new(Codigo, string.Join(" ", fallos));
}
