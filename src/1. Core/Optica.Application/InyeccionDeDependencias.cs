using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Optica.Application.Comportamientos;

namespace Optica.Application;

/// <summary>
/// Registro de la capa de aplicación en el contenedor.
/// </summary>
public static class InyeccionDeDependencias
{
    /// <summary>
    /// Registra MediatR, los validadores y los comportamientos de canal.
    /// </summary>
    /// <param name="servicios">Colección de servicios de la aplicación.</param>
    /// <returns>La misma colección, para poder encadenar.</returns>
    public static IServiceCollection AgregarCapaDeAplicacion(this IServiceCollection servicios)
    {
        ArgumentNullException.ThrowIfNull(servicios);

        var ensamblado = Assembly.GetExecutingAssembly();

        servicios.AddMediatR(configuracion => configuracion.RegisterServicesFromAssembly(ensamblado));
        servicios.AddValidatorsFromAssembly(ensamblado, includeInternalTypes: true);

        // El orden de registro **es** el orden de ejecución, y aquí importa.
        //
        // Log primero, para que un rechazo de validación quede registrado: si validación fuera
        // antes, cortaría el canal sin dejar traza de que la petición llegó.
        //
        // Validación antes de transacción, para no abrir una transacción de base de datos que se va
        // a descartar por una petición mal formada.
        servicios.AddTransient(typeof(IPipelineBehavior<,>), typeof(ComportamientoDeLog<,>));
        servicios.AddTransient(typeof(IPipelineBehavior<,>), typeof(ComportamientoDeValidacion<,>));
        servicios.AddTransient(typeof(IPipelineBehavior<,>), typeof(ComportamientoDeTransaccion<,>));

        return servicios;
    }
}
