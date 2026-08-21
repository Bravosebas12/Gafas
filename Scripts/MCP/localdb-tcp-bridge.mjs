/**
 * localdb-tcp-bridge.mjs
 *
 * LocalDB (SQL Server Express LocalDB) solo escucha en un named pipe: no abre
 * ningun puerto TCP. Los clientes Node (tedious / mssql, que es lo que usa el
 * servidor MCP) solo hablan TCP. Este puente acepta conexiones TCP en
 * 127.0.0.1:<puerto> y las reenvia byte a byte al named pipe de la instancia.
 * Los paquetes TDS son identicos en ambos transportes, asi que el relay es
 * transparente.
 *
 * El nombre del pipe cambia cada vez que LocalDB arranca, por eso se resuelve
 * en caliente con `sqllocaldb info` en lugar de fijarlo en la configuracion.
 *
 * Uso como CLI:
 *   node localdb-tcp-bridge.mjs [instancia] [puerto]
 *
 * Uso como modulo (lo hace start-mssql-mcp.mjs):
 *   import { iniciarPuente } from './localdb-tcp-bridge.mjs';
 *   await iniciarPuente({ instancia: 'MSSQLLocalDB', puerto: 14330 });
 *
 * Conectar despues como si fuera un SQL Server normal:
 *   sqlcmd -S tcp:127.0.0.1,14330 -U optica_app -P <clave> -d OpticaDB
 */
import net from 'node:net';
import { execFileSync } from 'node:child_process';
import { fileURLToPath } from 'node:url';

export function resolverPipe(instancia) {
  execFileSync('sqllocaldb', ['start', instancia], { stdio: 'ignore' });
  const info = execFileSync('sqllocaldb', ['info', instancia], { encoding: 'utf8' });
  const m = info.match(/np:(\\\\[^\r\n]+)/);
  if (!m) throw new Error(`No se pudo resolver el named pipe de ${instancia}:\n${info}`);
  return m[1].trim();
}

/**
 * Levanta el puente. Si el puerto ya esta ocupado asume que hay otro puente
 * corriendo (por ejemplo de otra sesion) y resuelve sin error.
 * @returns {Promise<{yaExistia: boolean}>}
 */
export function iniciarPuente({ instancia = 'MSSQLLocalDB', puerto = 14330 } = {}) {
  return new Promise((resolve, reject) => {
    const servidor = net.createServer((cliente) => {
      let pipe;
      try {
        pipe = net.connect(resolverPipe(instancia));
      } catch (e) {
        console.error('[bridge] fallo al resolver el pipe:', e.message);
        cliente.destroy();
        return;
      }
      pipe.on('connect', () => cliente.pipe(pipe).pipe(cliente));
      const cerrar = (etiqueta) => (e) => {
        if (e) console.error(`[bridge] ${etiqueta}:`, e.message);
        cliente.destroy();
        pipe.destroy();
      };
      cliente.on('error', cerrar('cliente'));
      pipe.on('error', cerrar('pipe'));
      cliente.on('close', cerrar('cierre cliente'));
      pipe.on('close', cerrar('cierre pipe'));
    });

    servidor.on('error', (e) => {
      if (e.code === 'EADDRINUSE') {
        console.error(`[bridge] 127.0.0.1:${puerto} ya esta en uso; se reutiliza el puente existente.`);
        resolve({ yaExistia: true });
      } else {
        reject(e);
      }
    });

    servidor.listen(puerto, '127.0.0.1', () => {
      servidor.unref();
      console.error(`[bridge] 127.0.0.1:${puerto} -> ${instancia} (${resolverPipe(instancia)})`);
      resolve({ yaExistia: false });
    });
  });
}

/* Ejecutado directamente: quedarse en primer plano. */
if (process.argv[1] === fileURLToPath(import.meta.url)) {
  await iniciarPuente({
    instancia: process.argv[2] || 'MSSQLLocalDB',
    puerto: Number(process.argv[3] || 14330),
  });
  setInterval(() => {}, 1 << 30); // mantener el proceso vivo
}
