using System.Xml.Linq;
using Xunit;

namespace Optica.Architecture.Tests;

/// <summary>
/// Verifica la tabla de referencias permitidas del principio II leyendo directamente los
/// archivos de proyecto.
///
/// Complementa a <see cref="ReglaDeDependenciasTests"/> en lugar de duplicarlo: NetArchTest
/// inspecciona dependencias de tipos compilados y compara espacios de nombres por prefijo, lo
/// que le impide distinguir <c>Optica.Web</c> de <c>Optica.Web.Client</c>. La referencia entre
/// proyectos, en cambio, es un hecho declarado en el archivo de proyecto y se puede comprobar
/// de forma exacta.
/// </summary>
public class ReferenciasDeProyectoTests
{
    /// <summary>
    /// Tabla del principio II. La clave es el proyecto; el valor, el conjunto exacto de
    /// proyectos que le está permitido referenciar.
    /// </summary>
    public static readonly TheoryData<string, string, string[]> TablaDeReferencias = new()
    {
        { "Optica.Domain", "src/1. Core/Optica.Domain", [] },
        { "Optica.Shared", "src/2. Infrastructure/Optica.Shared", [] },
        { "Optica.Application", "src/1. Core/Optica.Application", ["Optica.Domain", "Optica.Shared"] },
        {
            "Optica.Infrastructure", "src/2. Infrastructure/Optica.Infrastructure",
            ["Optica.Application", "Optica.Domain", "Optica.Shared"]
        },
        {
            // Infrastructure solo para registrar inyección de dependencias.
            // Optica.Web.Client es una exigencia de la plantilla de Blazor Web App: el proyecto
            // de servidor debe referenciar al de islas para poder servir su ensamblado.
            "Optica.Web", "src/3. Presentation/Optica.Web",
            ["Optica.Application", "Optica.Shared", "Optica.Infrastructure", "Optica.Web.Client"]
        },
        { "Optica.Web.Client", "src/3. Presentation/Optica.Web.Client", ["Optica.Shared"] },
    };

    [Theory]
    [MemberData(nameof(TablaDeReferencias))]
    public void El_proyecto_solo_referencia_lo_que_la_tabla_del_principio_II_permite(
        string proyecto,
        string rutaRelativa,
        string[] referenciasPermitidas)
    {
        var archivo = Path.Combine(RaizDelRepositorio(), rutaRelativa, $"{proyecto}.csproj");
        Assert.True(File.Exists(archivo), $"No se encontró el archivo de proyecto: {archivo}");

        var referenciasReales = XDocument.Load(archivo)
            .Descendants("ProjectReference")
            .Select(elemento => elemento.Attribute("Include")?.Value)
            .Where(valor => !string.IsNullOrWhiteSpace(valor))
            .Select(valor => Path.GetFileNameWithoutExtension(valor!))
            .OrderBy(nombre => nombre, StringComparer.Ordinal)
            .ToArray();

        var esperadas = referenciasPermitidas.OrderBy(nombre => nombre, StringComparer.Ordinal).ToArray();

        Assert.Equal(esperadas, referenciasReales);
    }

    /// <summary>
    /// Sube desde el directorio de salida hasta encontrar el archivo de solución.
    /// </summary>
    private static string RaizDelRepositorio()
    {
        var directorio = new DirectoryInfo(AppContext.BaseDirectory);

        while (directorio is not null && directorio.GetFiles("OpticaSolution.slnx").Length == 0)
        {
            directorio = directorio.Parent;
        }

        Assert.NotNull(directorio);
        return directorio!.FullName;
    }
}
