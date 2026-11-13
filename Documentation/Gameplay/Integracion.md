# Integración de jugabilidad

Verificada el 13 de noviembre de 2026.

Se integró `ImplementacionJugabilidad.zip` sobre los archivos locales, siguiendo
los nueve bloques de dominio, pruebas, estado activo, proyección, coordinación,
WCF, cliente, UI Process y presentación. El ZIP es suficiente como formato de
entrega; su carpeta `codigo` ya separa los tipos y conserva sus rutas.

## Comportamiento implementado

- Partidas de dos a cuatro jugadores, reparto 7/5, orden por dobles y sumas,
  apertura obligatoria y tablero de dos o cuatro extremos.
- Colocación, pozo, pases condicionados, puntuación All Fives, fin de ronda,
  revelado, redondeo y objetivos 100/150/200/250.
- Cinco fichas numéricas con poderes aleatorios por ronda: multiplicador rojo
  y bloqueo morado del extremo durante el siguiente turno.
- Cancelación sin ganador al abandonar explícitamente. Una desconexión técnica
  conserva el estado en memoria y permite reconectar con Actualizar.
- Autorización por sesión, pertenencia y versión en el servidor; una solicitud
  repetida sobre una versión anterior no vuelve a aplicar la jugada.
- Tablero público y manos privadas enviados por separado. El cliente aplica
  únicamente pares de snapshots con la misma versión e ignora datos ajenos.
- Navegación desde el lobby a Match, nombres, objetivo, mano, extremos,
  puntajes y acciones. El tablero gira las cadenas en sentido antihorario y
  reajusta el espacio virtual cuando hace falta.

El flujo de una jugada es `MatchViewModel → WcfMatchClient → MatchService →
MatchController → Match → Board`. El dominio aplica las reglas dentro de un
bloqueo por partida; el publicador entrega los callbacks fuera de ese bloqueo.
Las manos se proyectan en copias y se seleccionan por destinatario.

## Integración y ajustes

El [inventario de archivos](Archivos.md) detalla las rutas creadas o integradas
y sus hashes finales. Las áreas principales son Gameplay y sus carpetas Domain
y Enum, Controllers, Managers, Contracts/Gameplay, MatchService, la comunicación
del cliente, MatchViewModel y Match.xaml.

Se conservaron las responsabilidades, los namespaces y los atributos de
serialización de la entrega. Los proyectos clásicos incorporan cada nuevo
archivo mediante Compile; el cliente SDK los incluye automáticamente.
`DominoTileType` se trasladó a Models/Enum sin dejar una definición duplicada.

Se añadieron referencias directas a Contracts y System.Runtime.Serialization
en las pruebas del cliente: la referencia al proyecto clásico no resultaba
transitiva para ese proyecto. No se incorporaron paquetes adicionales.

La configuración añade MatchService y MatchEndpoint en el puerto 8737, sobre
el mismo host configurado para el lobby. No se sustituyó la configuración local
de SQL Server ni los endpoints anteriores. Se debe usar la dirección del host
que corresponda al despliegue real.

Se corrigieron respecto del paquete las lambdas con múltiples operaciones,
los nombres de excepciones, la exposición de la mano mutable mediante campo,
las constantes significativas, el orden de miembros, los saltos de línea,
las cadenas LINQ y la documentación XML de servicios y contratos. Las capturas
de excepciones realizan una recuperación, una traducción a fault, una aserción
de prueba o un registro; no quedaron capturas vacías.

La recepción de estado público ahora comprueba también MatchId antes de
aceptar su versión. Así, un snapshot de otra partida no impide aplicar los
snapshots válidos posteriores. Los pasos de red se extrajeron a métodos para
mantener simples las lambdas y conservar la intención capturada de la jugada.

## Estándar y fuentes

Se consultaron mediante el conector de Google Drive las versiones disponibles
del [estándar 4.0](https://docs.google.com/document/d/1zefg_xCv_JrXeA7uPFBKmoZ_pp2VExomDO8AubRCOhM/edit)
y del [reglamento del Equipo 4](https://docs.google.com/document/d/19LjewYpM6IViaIr7zfXrdQzR5SqBTdJAArHc9R7byww/edit).
Las decisiones complementarias de apertura, poderes y abandono proceden de
LEEME.md del ZIP, cuya implementación solicitó el usuario.

El alcance revisado comprende los 59 archivos C# de la entrega, con la excepción
documentada del diseñador generado de recursos, más proyectos, configuración,
XAML y recursos afectados. La revisión sintáctica verificó nombres de campos y
parámetros, llaves, correspondencia entre tipo y archivo, líneas de hasta 100
caracteres, máximo de cuatro parámetros, complejidad ciclomática hasta diez,
lambdas simples, capturas de excepciones y nombres/fases AAA de las pruebas.
La revisión manual cubrió responsabilidades, constantes, documentación WCF,
localización y límites de confianza. No quedaron incumplimientos detectados
en ese alcance. Esto no constituye una certificación de todo el repositorio
ni garantiza la ausencia de defectos no detectados.

## Validación ejecutada

| Comprobación | Resultado |
|---|---|
| Solución Server, Debug, .NET Framework 4.8 | Compilación correcta |
| Solución Client, Debug, net48 | Compilación correcta |
| Suite Server | 79 aprobadas, 0 fallidas, 0 omitidas |
| Suite Client | 52 aprobadas, 0 fallidas, 0 omitidas |
| Total | 131 aprobadas |
| Análisis sintáctico del código integrado | Sin hallazgos pendientes |
| Entradas Compile explícitas inspeccionadas | 121; sin ausencias ni duplicados |
| Claves Match en tres idiomas | 22; traducciones y marcadores coherentes |
| Recursos anteriores de los tres idiomas | Conservados |
| Presentación WPF a 1600 × 900 | Render inspeccionado con datos de muestra |

Se conservaron las 116 pruebas de referencia y se añadieron quince casos:
identidades y valores de ficha inválidos (cinco), objetivos inválidos (cuatro),
extremos inexistentes (dos), pozo indebido, cancelación repetida, snapshot de
otra partida y callback encolado después de Dispose.

Las pruebas incluyen WCF NetTcp con dos clientes, privacidad de manos,
valores falsificados, faults tipados y revocación de sesión; las simulaciones
de partidas completas conservan las 28 fichas para dos, tres y cuatro jugadores.
El layout se prueba con cadenas largas y cien distribuciones mixtas.

La ejecución inicial restringida del servidor obtuvo 76/79 por el bloqueo
de sockets TCP 10013 del entorno. La ejecución con permiso de sockets locales
y la repetición final aprobaron 79/79 sin omitir ni debilitar esas pruebas.
Las sesiones de prueba son en memoria y no requieren modificar SQL Server.

Las evidencias finales están en [Validation](Validation/): client.trx,
server.trx, findings.json y structure.json. [Match.png](Match.png) es un render
de la vista real con datos de muestra; no representa una partida con SQL Server.
Los registros de compilación, el verificador Roslyn y los temporales permanecen
en `.gameplay-work`, junto al repositorio.

Comandos reproducibles desde la raíz del repositorio, con herramientas de
Visual Studio disponibles en PATH:

```powershell
MSBuild.exe DominoAllFives.Server/DominoAllFives.Server.sln /p:Configuration=Debug /p:UseSharedCompilation=false /v:minimal /nologo
MSBuild.exe DominoAllFives.Client/DominoAllFives.Client.sln /p:Configuration=Debug /p:UseSharedCompilation=false /v:minimal /nologo
vstest.console.exe DominoAllFives.Server/DominoAllFives.Server.Tests/bin/Debug/DominoAllFives.Server.Tests.dll /Logger:trx
vstest.console.exe DominoAllFives.Client/DominoAllFives.Client.Tests/bin/Debug/net48/DominoAllFives.Client.Tests.dll /Logger:trx
```

## Comprobación manual y límites

Para probar el recorrido completo, iniciar el servidor en el host configurado,
abrir dos clientes, autenticarlos, crear y unirse a la misma sala e iniciar la
partida como anfitrión. Seleccionar la ficha de apertura y el extremo; usar
Colocar, Pozo o Pasar según las acciones habilitadas. Actualizar permite volver
a consultar el estado. Regresar abandona y cancela para todos los participantes.

No se ejecutó aquí ese recorrido con cuentas reales y la base SQL del usuario.
El chat solo conserva su espacio visual. No se integraron historial/ranking de
resultados, persistencia de partidas ni recuperación tras reiniciar el proceso
del servidor. Las partidas terminadas se conservan treinta minutos en memoria.
Las 131 pruebas tampoco completan el requisito académico global de 300 pruebas.
No se realizaron commits, cambios de rama ni publicaciones.
