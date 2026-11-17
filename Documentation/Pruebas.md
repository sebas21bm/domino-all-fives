# Pruebas

## Bloque 1: IRankingService (2026-11-16)

### Alcance y decisiones

Flujo revisado: IRankingService → DominoService → RankingController →
IPlayerStatsRepository / PlayerStatsRepository → Entity Framework. La construcción
de entradas consulta ProfilePictureStorage y produce RankingEntry / ProfilePicture;
Ranking permite conservar un snapshot ordenado. Se conserva el contexto EF por
operación y su liberación mediante using. No se agrega estado compartido ni locks.

Se mantienen las firmas WCF, los metadatos de serialización, los diez primeros
jugadores, la exclusión de invitados y jugadores sin victorias en el top, y el
orden por victorias y fecha de última victoria. No se redefine la política de
empates ni la elegibilidad de la consulta individual.

Los errores SqlException, EntityException y DbUpdateException siguen propagándose
hasta DominoService: allí se registran una vez con contexto de la operación y se
traducen al fault seguro existente. La ausencia o fallo recuperable de una imagen
sigue resolviéndose en ProfilePictureStorage con contenido vacío y su diagnóstico.
No se crean fábricas, interfaces ni delegados para sustituir operaciones en pruebas.

Cambios: lectura más clara al construir las entradas, validación de contexto nulo
en PlayerStatsRepository, formato de consultas y mensajes de diagnóstico por
operación. Ranking sigue en DominoService.cs. La regla de organización se agregó
a Manejo de comunicación remota.md.

### Organización de pruebas

- Tests/Tests.csproj: pruebas unitarias de validación y snapshots sin SQL, red ni archivos.
- AdditionalTest/AdditionalTest.csproj: comprobación auxiliar de la frontera del servicio.
  Invoca DominoService directamente; no verifica transporte WCF ni abre SQL con IDs inválidos.
- DominoAllFives.Server.Tests conserva temporalmente las pruebas de los contratos
  pendientes. Su migración se hará con el bloque que corresponda.

Cada prueba tiene AAA y una sola aserción. Se mantienen NUnit 3.14 y su adaptador
existentes, sin añadir paquetes.

### Casos implementados

| Caso a probar | Precondiciones | Postcondiciones | Resultado esperado | Resultado obtenido |
| --- | --- | --- | --- | --- |
| Normal: FromRankedPlayer con valores válidos | Imagen vacía válida | Entrada creada | Conserva posición, nombre, victorias e imagen | Aprobado |
| Alternativo: ranking vacío | Lista vacía y posición cero | Snapshot vacío | Conserva posición cero | Aprobado |
| Alternativo: cambia la lista de origen | Snapshot creado con una entrada | Lista de origen vacía | El snapshot conserva la entrada | Aprobado |
| Alternativo/límite: identificador cero, -1 e int.MinValue | Controller con storage válido | No se abre SQL | ArgumentOutOfRangeException | Aprobado |
| Excepción: storage nulo | Dependencia nula | No se construye el controller | ArgumentNullException | Aprobado |
| Excepción: imagen nula | Resto de datos válidos | No se crea entrada | ArgumentNullException | Aprobado |
| Excepción: lista nula | Lista nula | No se crea snapshot | ArgumentNullException | Aprobado |
| Frontera: identificador cero o -1 | DominoService | No se abre SQL | FaultException con Invalid request. | Aprobado |

### Casos diseñados que requieren infraestructura aislada

No ejecutados ni considerados cobertura unitaria. No se modifican datos reales.

| Caso a probar | Precondiciones | Postcondiciones | Resultado esperado | Resultado obtenido |
| --- | --- | --- | --- | --- |
| Top con más de diez jugadores | SQL de pruebas con invitados y victorias distintas | Sin escrituras | Máximo diez elegibles, ordenados por victorias y fecha | No ejecutado |
| Posición individual normal | SQL de pruebas con jugador con victorias | Sin escrituras | Número de jugadores elegibles por delante + 1 | No ejecutado |
| Posición individual ausente/sin victorias | SQL de pruebas sin estadísticas o cero victorias | Sin escrituras | Cero | No ejecutado |
| Empates de victorias y fecha | SQL de pruebas controlado | Sin escrituras | Mantiene la política actual sin desempate adicional | No ejecutado |
| Fallo SQL, EF o actualización | Infraestructura de pruebas que provoque cada excepción | Contexto liberado | LogError en frontera y fault seguro sin detalles internos | No ejecutado |
| Imagen ausente, acceso denegado, ruta larga, directorio ausente o error IO | Almacenamiento de pruebas aislado | Sin alterar archivos reales | Contenido vacío; diagnóstico cuando corresponde | No ejecutado |

### Verificación

- Compilación: MSBuild de Visual Studio 18, solución DominoAllFives.Server.sln,
  configuración Debug, target Build. Resultado: código de salida 0.
- Advertencias MSB3277 y MSB3836 por resolución de versiones y redirecciones de
  ensamblados en este entorno. No se modificaron versiones ni dependencias de
  producción para corregirlas dentro de este bloque.
- VSTest 18.10 con NUnit3TestAdapter 4.5: 11/11 aprobadas (9 unitarias y
  2 auxiliares), cero fallidas. Reporte local ignorado por Git:
  DominoAllFives.Server/TestResults/ranking.trx.
- git diff --check: sin errores.
- La validación de contexto nulo del repositorio fue revisada y compilada; no se
  añadió una prueba de SQL para justificar esta validación de constructor.
- Las pruebas auxiliares invocan el servicio directamente, sin verificar un host
  o transporte WCF real. Los casos con SQL y archivos permanecen no ejecutados.
- Los DataMember existentes de ranking conservan Order e IsRequired por
  compatibilidad. La guía general recomienda evitarlos cuando se valida en
  dominio; su eventual retirada requiere revisar el contrato y consumidores.
  No se declara conformidad total del contrato con esa recomendación.

## Bloque 2: ISessionAccountService (2026-11-16)

### Alcance y decisiones

Flujo revisado: ISessionAccountService → DominoService → AuthenticationController
→ IPlayerRepository / PlayerRepository → Entity Framework, seguido por la creación
de sesión en SessionManager.

AuthenticationController crea IPlayerRepository como colaborador con nombre y
administra el contexto de Entity Framework mediante `using` por operación.
DominoService conserva únicamente la responsabilidad de frontera WCF y creación
de la sesión posterior a la autenticación. Se eliminaron los delegados Func que
ocultaban el repositorio y sustituían el reloj únicamente durante pruebas. Se
preservan el recorte del identificador, los espacios de la contraseña, el orden
de validación, la comparación temporal existente y una sola sesión activa por
jugador.

Los rechazos esperados continúan como AccountRuleException dentro del servidor y
se transforman directamente en FaultException<AccountIssue> en la frontera. Los
errores SqlException, EntityException y DbUpdateException se registran una vez y
se convierten en el fault seguro existente. No se registran credenciales, hashes
ni claves de sesión.

No se retiraron Order ni IsRequired de los DataMember publicados. Ese cambio
afectaría los metadatos WCF y requiere una revisión específica de compatibilidad.
Tampoco se modificaron las firmas de ISessionAccountService.

### Organización de pruebas

- Tests: credenciales, contrato serializado, reglas de acceso, validación previa
  a persistencia, cuenta, hashing, repositorio y concurrencia de sesiones.
- AdditionalTest: traducción directa de credenciales inválidas en DominoService.
- La prueba de LobbyIssue se separó como LobbyContractTests y permanece en el
  proyecto heredado para el bloque de lobby.

Todas las pruebas nuevas siguen AAA y contienen una sola aserción. Las pruebas de
AuthenticationController cubren los rechazos que ocurren antes de crear el
contexto. No se agregó una ruta alternativa para sustituir el repositorio; los
escenarios que dependen de datos persistidos requieren una base de pruebas
aislada. SessionManager se prueba sin paralelizar su fixture y limpia las claves
creadas.

### Casos implementados

| Caso a probar | Precondiciones | Postcondiciones | Resultado esperado | Resultado obtenido |
| --- | --- | --- | --- | --- |
| Identificador con espacios | Credenciales creadas para login | Credencial normalizada | Identificador recortado y contraseña intacta | Aprobado |
| Credenciales incompletas | Controller con null o password ausente | No se crea contexto ni sesión | AccountIssue.InvalidCredentials | Aprobado |
| Estado eliminado o baneado reconstruido | Banderas persistidas representadas en dominio | Estado evaluado sin SQL | InvalidCredentials, Banned o None | Aprobado |
| Suspensión futura, vencida o en el límite | Fecha controlada representada en dominio | Estado evaluado sin SQL | Suspended o None según el límite | Aprobado |
| Hash almacenado inválido | Formatos, algoritmo, iteraciones o Base64 inválidos | Sin excepción técnica expuesta | Verificación false | Aprobado |
| Sustitución de sesión | Dos sesiones para el mismo jugador | Solo la segunda permanece | Primera clave inválida | Aprobado |
| Sesiones concurrentes | Doce jugadores distintos | Mapeos limpiados al terminar | Todos los IDs se resuelven correctamente | Aprobado |
| Contexto o colaboradores nulos | Dependencia obligatoria nula | Objeto no construido | ArgumentNullException | Aprobado |
| Frontera con credenciales inválidas | DominoService con null o password ausente | Sin consulta SQL ni sesión | FaultException<AccountIssue> tipado | Aprobado |

### Casos diseñados que requieren infraestructura aislada

No ejecutados ni considerados cobertura unitaria. No se modifican datos reales.

| Caso a probar | Precondiciones | Postcondiciones | Resultado esperado | Resultado obtenido |
| --- | --- | --- | --- | --- |
| Login válido | SQL de pruebas con cuenta controlada | Sesión creada y luego limpiada | Cuenta con clave resoluble | No ejecutado |
| Rechazos persistidos | SQL con cuenta inexistente, eliminada, baneada o suspendida | Sin escrituras ajenas | Fault tipado correspondiente | No ejecutado |
| Fallos SQL y Entity Framework | Infraestructura que provoque cada excepción específica | Contexto liberado | LogError y fault seguro | No ejecutado |
| Transporte WCF real | Host y proxy de pruebas | Canal cerrado o abortado | Fault tipado serializado | No ejecutado |
| Login repetido con SQL | La misma cuenta inicia sesión dos veces | Segunda sesión limpiada | Primera clave deja de ser válida | No ejecutado |

### Verificación

- Compilación Debug de DominoAllFives.Server.sln con MSBuild: aprobada.
- Se conservaron las advertencias preexistentes MSB3277 y MSB3836 por versiones
  y redirecciones de ensamblados; no se cambiaron dependencias en este bloque.
- VSTest 18.10 con NUnit3TestAdapter 4.5: 68/68 aprobadas en Tests y
  AdditionalTest. De ellas, 57 casos corresponden al bloque de sesión y 11 son la
  regresión de ranking.
- Las consultas SQL y el transporte WCF alojado permanecen no ejecutados.

## Bloque 3: ILobbyService (2026-11-16)

El bloque de Lobby conservó el servicio WCF dúplex y movió CreateLobby y
JoinLobby a operaciones `void` cuyo resultado se entrega mediante callback. El
manager compartido protege sus colecciones con `_syncRoot`; la creación de la
partida ocurre fuera del lock mediante las transiciones Waiting → Starting →
InProgress y revierte a Waiting si falla. Las pruebas unitarias se trasladaron a
Tests y las pruebas WCF/integración a AdditionalTest. La verificación del bloque
aprobó 95 pruebas unitarias y 8 auxiliares, 103 en total.

## Bloque 4: IMatchService (2026-11-16)

### Alcance y decisiones

Flujo revisado: IMatchService → MatchService → MatchController → MatchManager →
dominio de jugabilidad, seguido por MatchEventPublisher, MatchCallbackRegistry e
IMatchCallback. `MatchService` compone estos colaboradores compartidos del mismo
modo que `LobbyService`, sin un contenedor `GameRuntime`. El alcance se limitó a
Server; no se modificó el proxy ni el cliente WPF.

MatchManager conserva la exclusión mutua para validar versión, aplicar una jugada
y construir snapshots consistentes. Ya no recibe un `Action<Match>` ni ejecuta
un `Action<MatchUpdate>` bajo el lock. Devuelve un MatchUpdate con la vista
pública, manos privadas y resultados; MatchService lo encola únicamente después
de liberar el bloqueo. MatchEventPublisher conserva una cola independiente por
partida y entrega los callbacks fuera del estado autoritativo.

El contrato WCF mantiene PerSession, Reentrant, SessionMode.Required, callbacks
one-way y las firmas request/response publicadas. PlayTile, DrawTiles y PassTurn
continúan devolviendo MatchView además de emitir callbacks porque cambiar esas
firmas exige regenerar y adaptar el cliente, expresamente fuera de este bloque.
La migración a operaciones void que solo difundan el resultado por callback queda
pendiente para un bloque coordinado Server + Client.

### Organización de pruebas

- Tests: dominio de fichas, tablero, puntuación, rondas, concurrencia por versión,
  conversión explícita mediante MatchContractMapper, contrato dúplex y
  MatchRuleException.
- AdditionalTest: host WCF dúplex real mediante named pipes, proxy generado,
  aislamiento de manos privadas, valores de ficha falsificados, sesión revocada
  y jugador ajeno a la partida.
- DominoAllFives.Server.Tests quedó sin pruebas y se retiró de la solución; sus
  archivos de proyecto permanecen en el repositorio para no eliminarlos dentro
  de este bloque.

Las pruebas unitarias migradas siguen AAA y utilizan una sola aserción por caso.
Las pruebas WCF permanecen en AdditionalTest porque dependen del host, canales y
callbacks reales.

### Casos implementados

| Caso a probar | Precondiciones | Postcondiciones | Resultado esperado | Resultado obtenido |
| --- | --- | --- | --- | --- |
| Apertura y jugada normal | Dos sesiones y partida activa | Versión incrementada y callbacks entregados | Vista pública y mano privada autoritativas | Aprobado |
| Valores de ficha falsificados | ID propio con valores modificados por cliente | Dominio usa valores almacenados | No se aceptan pips ni poder enviados | Aprobado |
| Misma versión concurrente | Dos jugadas simultáneas con versión cero | Una sola mutación | Una aceptada y otra StaleVersion | Aprobado |
| Robo con mano jugable | Stock disponible | Estado sin cambios | DrawNotAllowed | Aprobado |
| Pase con stock disponible | Partida activa | Estado sin cambios | PassNotAllowed | Aprobado |
| Apertura con ficha incorrecta | Mano válida | Tablero y versión intactos | OpeningTileRequired | Aprobado |
| Cancelación repetida | Partida ya cancelada | Sin segundo incremento | Resultado cancelado sin ganador | Aprobado |
| Sesión revocada | Canal abierto y clave eliminada | Operación rechazada | AccessIssue tipado | Aprobado |
| Jugador ajeno | Sesión válida sin participación | Canal sin acceso al estado | PlayerNotInMatch tipado | Aprobado |
| Contrato dúplex | Reflexión sobre contratos | Sin cambios | Sesión requerida y callbacks one-way | Aprobado |

### Verificación

- Compilación Debug de DominoAllFives.Server.sln con MSBuild de Visual Studio 18:
  aprobada.
- VSTest 18.10 con NUnit3TestAdapter 4.5: 159/159 pruebas unitarias y
  11/11 pruebas auxiliares aprobadas; total 170/170.
- Las pruebas WCF utilizan direcciones net.pipe únicas para evitar puertos TCP y
  no dependen de acceso de red.
- Se conservaron las advertencias preexistentes MSB3277 y MSB3836; no se
  cambiaron dependencias dentro de este bloque.

## Continuidad de la refactorización

Se inventariaron los proyectos, contratos y clases de Server para delimitar los
flujos. Los bloques terminados son IRankingService, ISessionAccountService,
ILobbyService e IMatchService; esto no constituye una certificación de toda la
solución. El siguiente bloque pendiente es IChatService, con sus colaboradores y
pruebas. Cada contrato se cierra con validación y pausa para commit. Los
componentes compartidos se revisan en el contexto de cada flujo, sin cambios
ajenos al bloque.
