using System.Reflection;
using NetArchTest.Rules;
using Xunit;

namespace Optica.Architecture.Tests;

/// <summary>
/// Compuerta G1 de la constitución. El principio II exige que "una prueba de arquitectura
/// automatizada DEBE fallar la compilación ante cualquier referencia que viole la tabla".
/// Hay un caso por fila de esa tabla.
/// </summary>
public class ReglaDeDependenciasTests
{
    private const string Domain = "Optica.Domain";
    private const string Application = "Optica.Application";
    private const string Infrastructure = "Optica.Infrastructure";
    private const string Shared = "Optica.Shared";
    private const string Web = "Optica.Web";
    private const string WebClient = "Optica.Web.Client";

    private static Assembly Ensamblado(string nombre) => Assembly.Load(nombre);

    /// <summary>
    /// Excepción documentada: la plantilla de Blazor Web App obliga a que el proyecto de
    /// servidor referencie al de islas WebAssembly para poder servir su ensamblado. No es
    /// una decisión de diseño y no aparece en la tabla del principio II.
    /// </summary>
    private static readonly string[] ExcepcionesDePlantilla = [WebClient];

    [Fact]
    public void Domain_no_referencia_ningun_otro_proyecto_de_la_solucion()
    {
        var resultado = Types.InAssembly(Ensamblado(Domain))
            .Should()
            .NotHaveDependencyOnAny(Application, Infrastructure, Shared, Web, WebClient)
            .GetResult();

        Assert.True(resultado.IsSuccessful, Mensaje(Domain, resultado));
    }

    [Fact]
    public void Shared_no_referencia_ningun_otro_proyecto_de_la_solucion()
    {
        var resultado = Types.InAssembly(Ensamblado(Shared))
            .Should()
            .NotHaveDependencyOnAny(Domain, Application, Infrastructure, Web, WebClient)
            .GetResult();

        Assert.True(resultado.IsSuccessful, Mensaje(Shared, resultado));
    }

    [Fact]
    public void Application_solo_referencia_Domain_y_Shared()
    {
        var resultado = Types.InAssembly(Ensamblado(Application))
            .Should()
            .NotHaveDependencyOnAny(Infrastructure, Web, WebClient)
            .GetResult();

        Assert.True(resultado.IsSuccessful, Mensaje(Application, resultado));
    }

    [Fact]
    public void Infrastructure_no_referencia_la_capa_de_presentacion()
    {
        var resultado = Types.InAssembly(Ensamblado(Infrastructure))
            .Should()
            .NotHaveDependencyOnAny(Web, WebClient)
            .GetResult();

        Assert.True(resultado.IsSuccessful, Mensaje(Infrastructure, resultado));
    }

    [Fact]
    public void WebClient_solo_referencia_Shared()
    {
        // No se incluye Optica.Web en la lista de prohibidos: NetArchTest compara por prefijo
        // de espacio de nombres, y "Optica.Web" coincide con "Optica.Web.Client", que es el
        // propio ensamblado bajo prueba. Incluirlo produce un falso positivo permanente.
        // La ausencia de referencia de proyecto de Web.Client hacia Web la garantiza el
        // archivo de proyecto, verificado en ReferenciasDeProyectoTests.
        var resultado = Types.InAssembly(Ensamblado(WebClient))
            .Should()
            .NotHaveDependencyOnAny(Domain, Application, Infrastructure)
            .GetResult();

        Assert.True(resultado.IsSuccessful, Mensaje(WebClient, resultado));
    }

    [Fact]
    public void Web_es_el_punto_de_entrada_y_nadie_lo_referencia()
    {
        // La fila de Optica.Web en la tabla del principio II le permite referenciar
        // Application, Shared e Infrastructure, más Web.Client por exigencia de la plantilla
        // (ver ExcepcionesDePlantilla). No queda ninguna referencia prohibida que comprobar
        // desde Web hacia fuera, así que la regla que sí aporta valor es la inversa: ninguna
        // otra capa puede depender del punto de entrada.
        // Optica.Web.Client queda fuera de esta comprobación por la colisión de prefijos:
        // su propio espacio de nombres empieza por "Optica.Web", de modo que NetArchTest lo
        // reportaría siempre como infractor de sí mismo.
        var dependenOfensivamenteDeWeb = new[] { Domain, Shared, Application, Infrastructure }
            .Where(proyecto => !Types.InAssembly(Ensamblado(proyecto))
                .Should()
                .NotHaveDependencyOn(Web)
                .GetResult()
                .IsSuccessful)
            .ToArray();

        Assert.Empty(dependenOfensivamenteDeWeb);
    }

    private static string Mensaje(string proyecto, TestResult resultado)
    {
        var tipos = resultado.FailingTypeNames is null
            ? "(sin detalle)"
            : string.Join(", ", resultado.FailingTypeNames);

        return $"{proyecto} viola la regla de dependencias del principio II. Tipos infractores: {tipos}";
    }
}
