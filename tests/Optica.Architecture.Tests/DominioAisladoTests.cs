using System.Reflection;
using NetArchTest.Rules;
using Xunit;

namespace Optica.Architecture.Tests;

/// <summary>
/// El principio II prohíbe de forma explícita que <c>Optica.Domain</c> conozca el ORM, la
/// configuración, el contexto HTTP o el reloj del sistema. El tiempo entra por una abstracción
/// y siempre en UTC (decisión D-07).
/// </summary>
public class DominioAisladoTests
{
    private static Assembly Dominio => Assembly.Load("Optica.Domain");

    [Fact]
    public void Domain_no_conoce_el_ORM()
    {
        var resultado = Types.InAssembly(Dominio)
            .Should()
            .NotHaveDependencyOnAny(
                "Microsoft.EntityFrameworkCore",
                "System.Data.SqlClient",
                "Microsoft.Data.SqlClient",
                "Dapper")
            .GetResult();

        Assert.True(resultado.IsSuccessful, Mensaje("acceso a datos", resultado));
    }

    [Fact]
    public void Domain_no_conoce_la_configuracion_ni_el_contexto_HTTP()
    {
        var resultado = Types.InAssembly(Dominio)
            .Should()
            .NotHaveDependencyOnAny(
                "Microsoft.Extensions.Configuration",
                "Microsoft.AspNetCore")
            .GetResult();

        Assert.True(resultado.IsSuccessful, Mensaje("configuración o HTTP", resultado));
    }

    [Fact]
    public void Domain_no_accede_al_reloj_del_sistema()
    {
        // DateTime.Now y DateTime.UtcNow están prohibidos: el tiempo entra por TimeProvider.
        // NetArchTest no inspecciona llamadas a miembros estáticos, así que esta prueba cubre
        // el caso de un tipo del dominio que dependa de DateTime como tipo declarado y sirve
        // de red de seguridad junto a la revisión de código.
        var tiposQueUsanDateTime = Types.InAssembly(Dominio)
            .That()
            .HaveDependencyOn("System.DateTime")
            .GetTypes();

        Assert.Empty(tiposQueUsanDateTime);
    }

    private static string Mensaje(string capacidad, TestResult resultado)
    {
        var tipos = resultado.FailingTypeNames is null
            ? "(sin detalle)"
            : string.Join(", ", resultado.FailingTypeNames);

        return $"Optica.Domain no debe depender de {capacidad}. Tipos infractores: {tipos}";
    }
}
