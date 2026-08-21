using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Optica.Infrastructure.Persistencia;

/// <summary>
/// Contexto de persistencia contra el esquema **existente** de OpticaDB.
///
/// Este contexto **no tiene migraciones y no debe tenerlas**. El principio X declara el script
/// `Scripts/SQL/001_modelo_datos_optica.sql` como fuente de verdad del modelo de datos, de modo que
/// el flujo normal de Entity Framework queda invertido: aquí el modelo de código se somete al
/// esquema, no lo genera. La decisión está en D-03, y la compuerta G10 la verifica.
///
/// Consecuencia práctica que conviene tener presente: si el modelo y el esquema divergen, el error
/// aparece en tiempo de ejecución al consultar, no al compilar. Por eso existen las pruebas de
/// integración contra SQL Server real de la decisión D-11, y no un juego de pruebas en memoria que
/// pasaría con un modelo equivocado.
/// </summary>
/// <param name="opciones">Opciones de configuración del contexto.</param>
public class OpticaDbContext(DbContextOptions<OpticaDbContext> opciones) : DbContext(opciones)
{
    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        // Cada entidad trae su propia configuración, en un archivo junto a ella. Se descubren por
        // ensamblado en lugar de enumerarse aquí: enumerarlas convierte este archivo en un punto de
        // conflicto que toca cada tarea que agrega una entidad.
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }
}
