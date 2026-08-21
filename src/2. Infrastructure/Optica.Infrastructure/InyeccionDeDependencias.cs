using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Optica.Application.Abstracciones;
using Optica.Infrastructure.Persistencia;

namespace Optica.Infrastructure;

/// <summary>
/// Registro de la capa de infraestructura en el contenedor.
/// </summary>
public static class InyeccionDeDependencias
{
    /// <summary>
    /// Registra el contexto de persistencia, la unidad de trabajo y la fuente de tiempo.
    /// </summary>
    /// <param name="servicios">Colección de servicios de la aplicación.</param>
    /// <param name="cadenaDeConexion">
    /// Cadena de conexión a OpticaDB. Llega de configuración, nunca del código: el principio VI
    /// prohíbe secretos en el repositorio y una cadena con contraseña lo es.
    /// </param>
    /// <returns>La misma colección, para poder encadenar.</returns>
    public static IServiceCollection AgregarCapaDeInfraestructura(
        this IServiceCollection servicios,
        string cadenaDeConexion)
    {
        ArgumentNullException.ThrowIfNull(servicios);
        ArgumentException.ThrowIfNullOrWhiteSpace(cadenaDeConexion);

        servicios.AddDbContext<OpticaDbContext>(opciones =>
            opciones.UseSqlServer(cadenaDeConexion, sqlServer =>
                // Sin migraciones por el principio X: el esquema lo aplican los scripts numerados.
                // Este ensamblado se declara solo para que un `dotnet ef` accidental falle en un
                // sitio evidente en lugar de generar una migración contra el esquema congelado.
                sqlServer.MigrationsAssembly(typeof(OpticaDbContext).Assembly.FullName)));

        servicios.AddScoped<IUnidadDeTrabajo, UnidadDeTrabajo>();

        // Única fuente de tiempo de todo el sistema (decisión D-07). Registrarla aquí es lo que
        // permite sustituirla en las pruebas y prohibir `DateTime.Now` en el resto del código.
        // Se usa TryAdd para que una prueba que ya registró un reloj de prueba conserve el suyo.
        servicios.TryAddSingleton(TimeProvider.System);

        return servicios;
    }
}
