using MediatR;
using Optica.Domain.Comun;

namespace Optica.Application.Abstracciones;

/// <summary>
/// Caso de uso que **modifica** estado.
///
/// El principio III exige separar comandos de consultas, y esta interfaz es lo que hace la
/// separación aplicable en lugar de aspiracional: el comportamiento de transacción se registra solo
/// para comandos, así que una consulta no puede abrir una transacción de escritura ni por descuido.
///
/// Devuelve <see cref="Resultado"/> porque un fallo de negocio previsto no es una excepción, sino
/// una de las dos salidas normales de la operación (principio I).
/// </summary>
/// <typeparam name="TResultado">Lo que devuelve el comando al terminar bien.</typeparam>
public interface IComando<TResultado> : IRequest<TResultado>
    where TResultado : Resultado;

/// <summary>
/// Caso de uso que **lee** estado y no lo modifica.
///
/// Ninguna consulta debe producir escrituras. Si una necesita hacerlo, es un comando mal nombrado.
/// </summary>
/// <typeparam name="TResultado">Lo que devuelve la consulta.</typeparam>
public interface IConsulta<TResultado> : IRequest<TResultado>
    where TResultado : Resultado;
