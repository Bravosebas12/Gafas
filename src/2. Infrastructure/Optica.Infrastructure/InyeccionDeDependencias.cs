using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Optica.Application.Abstracciones;
using Optica.Infrastructure.Configuracion;
using Optica.Infrastructure.Persistencia;
using Optica.Infrastructure.Persistencia.Interceptores;

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
    /// <param name="configuracion">
    /// Configuración de la aplicación. De aquí sale la cadena de conexión y las opciones del token:
    /// el principio VI prohíbe secretos en el código, y una cadena con contraseña lo es.
    /// </param>
    /// <returns>La misma colección, para poder encadenar.</returns>
    public static IServiceCollection AgregarCapaDeInfraestructura(
        this IServiceCollection servicios,
        IConfiguration configuracion)
    {
        ArgumentNullException.ThrowIfNull(servicios);
        ArgumentNullException.ThrowIfNull(configuracion);

        // Primero de todo, antes de registrar nada: si la configuración declara un proveedor de
        // identidad externo, la aplicación no debe arrancar (FR-002, compuerta G5).
        ValidadorDeProveedoresExternos.Validar(configuracion);

        servicios
            .AddOptions<OpcionesDeToken>()
            .Bind(configuracion.GetSection(OpcionesDeToken.Seccion))
            .ValidateOnStart();

        servicios.AddSingleton<IValidateOptions<OpcionesDeToken>, ValidadorDeOpcionesDeToken>();

        var cadenaDeConexion = configuracion.GetConnectionString("OpticaDB")
            ?? throw new ConfiguracionProhibidaException(
                "Falta la cadena de conexión 'OpticaDB'. Llega de configuración, nunca del código.",
                new InvalidOperationException("ConnectionStrings:OpticaDB no está configurada."));

        // Las anotaciones de auditoría viven por petición y se exponen con dos caras: la capa de
        // aplicación las ve como IAuditor y solo puede anotar; el interceptor las ve completas y
        // puede materializarlas y limpiarlas. Es la misma instancia, con la superficie que cada uno
        // necesita y nada más.
        servicios.AddScoped<AnotacionesDeAuditoria>();
        servicios.AddScoped<IAuditor>(proveedor => proveedor.GetRequiredService<AnotacionesDeAuditoria>());

        servicios.AddScoped<AuditoriaInterceptor>();
        servicios.AddScoped<ColumnasDeAuditoriaInterceptor>();

        servicios.AddDbContext<OpticaDbContext>((proveedor, opciones) =>
        {
            opciones.UseSqlServer(cadenaDeConexion, sqlServer =>
                // Sin migraciones por el principio X: el esquema lo aplican los scripts numerados.
                // Este ensamblado se declara solo para que un `dotnet ef` accidental falle en un
                // sitio evidente en lugar de generar una migración contra el esquema congelado.
                sqlServer.MigrationsAssembly(typeof(OpticaDbContext).Assembly.FullName));

            // El orden importa. Las columnas de auditoría se marcan **antes** de materializar los
            // registros, para que el JSON del estado posterior incluya la fecha de actualización que
            // acaba de fijarse. Al revés, la auditoría registraría la marca anterior.
            opciones.AddInterceptors(
                proveedor.GetRequiredService<ColumnasDeAuditoriaInterceptor>(),
                proveedor.GetRequiredService<AuditoriaInterceptor>());
        });

        servicios.AddScoped<IUnidadDeTrabajo, UnidadDeTrabajo>();

        // Implementación por omisión, para los flujos sin sesión. La capa web registra la suya antes
        // de llamar a este método y entonces esta no se aplica.
        servicios.TryAddScoped<IUsuarioActual, UsuarioActualDeOperador>();

        // Única fuente de tiempo de todo el sistema (decisión D-07). Registrarla aquí es lo que
        // permite sustituirla en las pruebas y prohibir `DateTime.Now` en el resto del código.
        // Se usa TryAdd para que una prueba que ya registró un reloj de prueba conserve el suyo.
        servicios.TryAddSingleton(TimeProvider.System);

        return servicios;
    }
}
