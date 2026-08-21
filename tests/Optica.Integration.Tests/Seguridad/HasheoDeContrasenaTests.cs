using System.Text;
using Optica.Infrastructure.Seguridad;

namespace Optica.Integration.Tests.Seguridad;

/// <summary>
/// Ida y vuelta del hasheo de contraseñas: caracteres no latinos, espacios y longitud extrema.
///
/// Tarea T029. Cubre el caso borde de la especificación *"contraseña con caracteres no latinos,
/// espacios o longitud extrema: debe validarse y almacenarse sin corrupción ni truncamiento"* y la
/// clase de equivalencia V3 del análisis en `qa/001-auth-rbac-foundation/analisis/`.
///
/// NOTA DE UBICACIÓN. tasks.md sitúa esta prueba en
/// `tests/Optica.Domain.Tests/Seguridad/HasheoDeContrasenaTests.cs`, y ahí no puede vivir:
/// el hasheador se implementa en `Optica.Infrastructure` (tarea T034) y `Optica.Domain.Tests`
/// solo referencia `Optica.Domain`, como exige el principio II. Ponerla ahí obligaría a agregar
/// una referencia que viola la regla de dependencias y haría fallar la compuerta G1.
///
/// Se ubica aquí porque este es el único proyecto de prueba que referencia infraestructura. No es
/// una prueba de integración en sentido estricto —no toca base de datos ni host—, así que la
/// ubicación correcta sería un proyecto `Optica.Infrastructure.Tests` propio. Queda anotado como
/// corrección pendiente de tasks.md.
/// </summary>
public sealed class HasheoDeContrasenaTests
{
    private readonly HasheadorPbkdf2 hasheador = new();

    [Theory]
    [InlineData("Optica2026#Segura")]
    [InlineData("Ñandú Ópti¢a 2026 ✓")]
    [InlineData("contraseña con varios espacios")]
    [InlineData("  espacios al principio y al final  ")]
    [InlineData("Ω≈ç√∫˜µ≤≥÷")]
    [InlineData("密码测试2026")]
    [InlineData("emoji🔐enmedio")]
    [InlineData("salto\nde\nlinea")]
    [InlineData("tabulación\tinterna")]
    [InlineData("a")]
    public void Una_contrasena_derivada_coincide_consigo_misma(string contrasena)
    {
        var (hash, sal) = hasheador.Derivar(contrasena);

        Assert.True(hasheador.Coincide(contrasena, hash, sal));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(64)]
    [InlineData(255)]
    [InlineData(256)]
    public void Una_contrasena_en_los_bordes_de_longitud_coincide_sin_truncarse(int longitud)
    {
        var contrasena = new string('x', longitud - 1) + "Z";

        var (hash, sal) = hasheador.Derivar(contrasena);

        Assert.True(hasheador.Coincide(contrasena, hash, sal));
    }

    [Fact]
    public void Dos_contrasenas_que_solo_difieren_en_el_ultimo_caracter_no_coinciden()
    {
        var (hash, sal) = hasheador.Derivar(new string('x', 255) + "A");

        var coincide = hasheador.Coincide(new string('x', 255) + "B", hash, sal);

        Assert.False(coincide);
    }

    [Fact]
    public void Una_contrasena_distinta_no_coincide()
    {
        var (hash, sal) = hasheador.Derivar("Optica2026#Segura");

        Assert.False(hasheador.Coincide("ClaveEquivocada#1", hash, sal));
    }

    [Fact]
    public void La_comparacion_distingue_mayusculas_y_minusculas()
    {
        var (hash, sal) = hasheador.Derivar("Optica2026#Segura");

        Assert.False(hasheador.Coincide("optica2026#segura", hash, sal));
    }

    [Fact]
    public void Los_espacios_al_final_son_significativos_y_no_se_recortan()
    {
        var (hash, sal) = hasheador.Derivar("Optica2026#Segura ");

        Assert.False(hasheador.Coincide("Optica2026#Segura", hash, sal));
    }

    /// <summary>
    /// Dos formas Unicode que se ven idénticas pero difieren en bytes. Si el hasheador normaliza,
    /// coinciden; si no, no coinciden. Cualquiera de las dos posturas es defendible, pero debe ser
    /// deliberada: esta prueba fija la decisión para que no cambie por accidente al tocar la
    /// codificación. La postura fijada es NO normalizar, que es el comportamiento por omisión.
    /// </summary>
    [Fact]
    public void La_derivacion_no_normaliza_formas_Unicode_equivalentes()
    {
        // La ñ como un único punto de código, frente a n seguida de tilde combinante.
        // Se escriben con secuencias de escape a propósito: en el archivo se verían idénticas.
        const string precompuesta = "cafe\u00F1";
        const string descompuesta = "cafen\u0303";

        Assert.Equal(
            precompuesta.Normalize(NormalizationForm.FormC),
            descompuesta.Normalize(NormalizationForm.FormC));
        Assert.NotEqual(
            Encoding.UTF8.GetBytes(precompuesta), Encoding.UTF8.GetBytes(descompuesta));

        var (hash, sal) = hasheador.Derivar(precompuesta);

        Assert.False(hasheador.Coincide(descompuesta, hash, sal));
    }

    // ---------------------------------------------------------------------
    // Decisión D-01: salt de 128 bits por usuario, clave derivada de 256 bits
    // ---------------------------------------------------------------------

    [Fact]
    public void El_salt_mide_128_bits()
    {
        var (_, sal) = hasheador.Derivar("Optica2026#Segura");

        Assert.Equal(16, sal.Length);
    }

    [Fact]
    public void El_hash_mide_256_bits()
    {
        var (hash, _) = hasheador.Derivar("Optica2026#Segura");

        Assert.Equal(32, hash.Length);
    }

    [Fact]
    public void Dos_derivaciones_de_la_misma_contrasena_producen_salt_distinto()
    {
        var (_, primeraSal) = hasheador.Derivar("Optica2026#Segura");
        var (_, segundaSal) = hasheador.Derivar("Optica2026#Segura");

        Assert.NotEqual(primeraSal, segundaSal);
    }

    [Fact]
    public void Dos_derivaciones_de_la_misma_contrasena_producen_hash_distinto()
    {
        var (primerHash, _) = hasheador.Derivar("Optica2026#Segura");
        var (segundoHash, _) = hasheador.Derivar("Optica2026#Segura");

        Assert.NotEqual(primerHash, segundoHash);
    }

    [Fact]
    public void El_hash_no_contiene_la_contrasena_en_claro()
    {
        const string contrasena = "Optica2026#Segura";
        var (hash, _) = hasheador.Derivar(contrasena);

        var comoTexto = Encoding.UTF8.GetString(hash);
        var comoBase64 = Convert.ToBase64String(hash);

        Assert.DoesNotContain(contrasena, comoTexto);
        Assert.DoesNotContain(contrasena, comoBase64);
    }

    // ---------------------------------------------------------------------
    // Decisión D-02: la derivación señuelo iguala el tiempo cuando la cuenta no existe
    // ---------------------------------------------------------------------

    [Fact]
    public void La_derivacion_senuelo_cuesta_lo_mismo_que_una_derivacion_real()
    {
        hasheador.Derivar("calentamiento");

        var real = Medir(() => hasheador.Derivar("Optica2026#Segura"));
        var senuelo = Medir(() => hasheador.DerivarSenuelo());

        var diferencia = Math.Abs(real.TotalMilliseconds - senuelo.TotalMilliseconds);
        var tolerancia = real.TotalMilliseconds * 0.5;

        Assert.True(
            diferencia <= tolerancia,
            $"El señuelo tardó {senuelo.TotalMilliseconds:F0} ms y la derivación real "
            + $"{real.TotalMilliseconds:F0} ms. La diferencia debe mantenerse dentro del "
            + "50 % del costo real para no delatar la ausencia de la cuenta.");
    }

    private static TimeSpan Medir(Action accion)
    {
        var inicio = TimeProvider.System.GetTimestamp();
        accion();
        return TimeProvider.System.GetElapsedTime(inicio);
    }
}
