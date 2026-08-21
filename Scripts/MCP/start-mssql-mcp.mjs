/**
 * start-mssql-mcp.mjs
 *
 * Punto de entrada del servidor MCP de SQL Server para este proyecto. Hace dos
 * cosas antes de delegar:
 *   1. Levanta el puente TCP -> named pipe de LocalDB (ver localdb-tcp-bridge.mjs).
 *   2. Lanza `mssql-mcp` heredando stdin/stdout, de modo que el cliente MCP
 *      hable directamente con el servidor por JSON-RPC.
 *
 * Al vivir el puente dentro de este proceso, su ciclo de vida es el del MCP: no
 * queda nada colgado cuando el cliente cierra la sesion.
 *
 * La configuracion de conexion llega por variables de entorno desde .mcp.json.
 * MSSQL_LOCALDB_INSTANCE y MSSQL_BRIDGE_PORT son opcionales.
 */
import { spawn } from 'node:child_process';
import { iniciarPuente } from './localdb-tcp-bridge.mjs';

const instancia = process.env.MSSQL_LOCALDB_INSTANCE || 'MSSQLLocalDB';
const puerto = Number(process.env.MSSQL_BRIDGE_PORT || process.env.DB_PORT || 14330);

try {
  await iniciarPuente({ instancia, puerto });
} catch (e) {
  console.error('[start-mssql-mcp] no se pudo levantar el puente a LocalDB:', e.message);
  process.exit(1);
}

const hijo = spawn('npx', ['-y', 'mssql-mcp@latest'], {
  stdio: 'inherit',
  shell: true,
  env: { ...process.env, DB_PORT: String(puerto) },
});

hijo.on('exit', (code, signal) => process.exit(signal ? 1 : code ?? 0));
for (const s of ['SIGINT', 'SIGTERM']) process.on(s, () => hijo.kill(s));
