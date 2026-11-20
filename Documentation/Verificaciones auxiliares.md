# Verificaciones auxiliares

## Separación documental (2026-11-16)

Este archivo conserva verificaciones que no corresponden al catálogo de pruebas
unitarias. No cuenta como evidencia unitaria de la entrega académica.
Las pruebas de integración, funcionales e infraestructura pertenecen a AdditionalTest.

Se revisó la ejecución phase1-final.trx de Tests: contiene 212 resultados Passed.
De ellos, 199 corresponden al catálogo unitario y los 13 siguientes ejercitan
colaboraciones entre controllers y managers reales. Su ubicación actual en Tests
no cambia esa clasificación. La migración del código a AdditionalTest queda
pendiente: esta actualización modifica documentación, no archivos de pruebas.

| N.º | Caso | Origen | Clasificación y resultado |
| --- | --- | --- | --- |
| AUX-001 | `GameplayTests.GetHand_NonParticipant_IsRejected` | `DominoAllFives.Server/Tests/GameplayTests.cs` | Integración en memoria de controller y manager; Passed en phase1-final.trx. |
| AUX-002 | `GameplayTests.Play_ConcurrentSameVersion_AcceptsAtMostOneMutation` | `DominoAllFives.Server/Tests/GameplayTests.cs` | Integración en memoria de controller y manager; Passed en phase1-final.trx. |
| AUX-003 | `LobbyManagerTests.ConfirmMatchStarted_StartingLobby_ChangesStateToInProgress` | `DominoAllFives.Server/Tests/LobbyManagerTests.cs` | Integración en memoria de controller y manager; Passed en phase1-final.trx. |
| AUX-004 | `LobbyManagerTests.CreateLobby_PlayerAlreadyInLobby_ThrowsSpecificIssue` | `DominoAllFives.Server/Tests/LobbyManagerTests.cs` | Integración en memoria de controller y manager; Passed en phase1-final.trx. |
| AUX-005 | `LobbyManagerTests.CreateLobby_ValidSettings_ReturnsHostSnapshot` | `DominoAllFives.Server/Tests/LobbyManagerTests.cs` | Integración en memoria de controller y manager; Passed en phase1-final.trx. |
| AUX-006 | `LobbyManagerTests.GetLobby_ReturnedSnapshotChanges_PreservesStoredLobby` | `DominoAllFives.Server/Tests/LobbyManagerTests.cs` | Integración en memoria de controller y manager; Passed en phase1-final.trx. |
| AUX-007 | `LobbyManagerTests.JoinLobby_AvailableLobby_AddsPlayer` | `DominoAllFives.Server/Tests/LobbyManagerTests.cs` | Integración en memoria de controller y manager; Passed en phase1-final.trx. |
| AUX-008 | `LobbyManagerTests.JoinLobby_FullLobby_ThrowsSpecificIssue` | `DominoAllFives.Server/Tests/LobbyManagerTests.cs` | Integración en memoria de controller y manager; Passed en phase1-final.trx. |
| AUX-009 | `LobbyManagerTests.LeaveLobby_HostLeaves_ClosesLobby` | `DominoAllFives.Server/Tests/LobbyManagerTests.cs` | Integración en memoria de controller y manager; Passed en phase1-final.trx. |
| AUX-010 | `LobbyManagerTests.PrepareLobbyForMatch_InsufficientPlayers_ThrowsSpecificIssue` | `DominoAllFives.Server/Tests/LobbyManagerTests.cs` | Integración en memoria de controller y manager; Passed en phase1-final.trx. |
| AUX-011 | `LobbyManagerTests.PrepareLobbyForMatch_NonHost_ThrowsUnauthorizedAccessException` | `DominoAllFives.Server/Tests/LobbyManagerTests.cs` | Integración en memoria de controller y manager; Passed en phase1-final.trx. |
| AUX-012 | `LobbyManagerTests.PrepareLobbyForMatch_ValidLobby_ChangesStateToStarting` | `DominoAllFives.Server/Tests/LobbyManagerTests.cs` | Integración en memoria de controller y manager; Passed en phase1-final.trx. |
| AUX-013 | `LobbyManagerTests.RestoreLobbyAfterMatchStartFailure_StartingLobby_RestoresWaitingState` | `DominoAllFives.Server/Tests/LobbyManagerTests.cs` | Integración en memoria de controller y manager; Passed en phase1-final.trx. |

## Escenarios de infraestructura previamente diseñados

Se conserva el estado histórico registrado, sin afirmar una nueva ejecución.

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


### Casos diseñados que requieren infraestructura aislada

No ejecutados ni considerados cobertura unitaria. No se modifican datos reales.

| Caso a probar | Precondiciones | Postcondiciones | Resultado esperado | Resultado obtenido |
| --- | --- | --- | --- | --- |
| Login válido | SQL de pruebas con cuenta controlada | Sesión creada y luego limpiada | Cuenta con clave resoluble | No ejecutado |
| Rechazos persistidos | SQL con cuenta inexistente, eliminada, baneada o suspendida | Sin escrituras ajenas | Fault tipado correspondiente | No ejecutado |
| Fallos SQL y Entity Framework | Infraestructura que provoque cada excepción específica | Contexto liberado | LogError y fault seguro | No ejecutado |
| Transporte WCF real | Host y proxy de pruebas | Canal cerrado o abortado | Fault tipado serializado | No ejecutado |
| Login repetido con SQL | La misma cuenta inicia sesión dos veces | Segunda sesión limpiada | Primera clave deja de ser válida | No ejecutado |


## Verificaciones de frontera y transporte registradas anteriormente

- Ranking: dos casos auxiliares invocaban DominoService directamente con ID inválido,
  sin alojar WCF; reporte histórico DominoAllFives.Server/TestResults/ranking.trx.
- Sesión: AdditionalTest verificaba traducción de credenciales inválidas en el servicio.
  El registro histórico informó 68 casos conjuntos, sin un desglose auxiliar íntegro.
- Lobby: registro histórico de 95 casos en Tests y 8 auxiliares; 103 en conjunto.
- Partida: registro histórico de 159 casos en Tests y 11 auxiliares; 170 en conjunto.
  Los 159 anteriores eran el conteo del proyecto y no una clasificación revisada.
- AdditionalTest incluía host dúplex net.pipe, proxy generado, rechazo de valores de
  ficha falsificados, privacidad de manos, revocación de sesión y jugador ajeno.
  Esas comprobaciones dependen de host, canales o varias capas y no son unitarias.
- El orden entre confirmación de versiones y publicación quedó pendiente en esta
  actualización histórica; su comprobación se registra en la sección de fase 5.

Los resultados anteriores son antecedentes del documento original. No se
ejecutaron nuevamente estas pruebas al reorganizar la documentación.
## Fase 2 de Match: regresión auxiliar (2026-11-16)

Después de compilar la solución del servidor se ejecutaron, por separado de las
pruebas unitarias, los 13 casos de integración en memoria descritos arriba y los
11 casos existentes de AdditionalTest. Todos pasaron; cero fallidos y cero omitidos.
Los 13 primeros todavía están ubicados en Tests; no se migraron en esta fase.
Reportes locales: Tests/TestResults/phase2-memory-integration.trx y
Tests/TestResults/phase2-additional.trx. No se modificaron estas comprobaciones.
La ejecución utiliza estado controlado en memoria, invocaciones de frontera y
hosts locales net.pipe; no realiza operaciones sobre datos reales de SQL Server.

| N.º | Clase y caso auxiliar | Resultado obtenido |
| --- | --- | --- |
| AUX-014 | `LobbyIntegrationTests.MatchStart_CancelledTransition_PreservesLobbyAndMessages` | Passed en phase2-additional.trx. |
| AUX-015 | `LobbyIntegrationTests.MatchStart_CompletedTransition_ClosesLobbyChat` | Passed en phase2-additional.trx. |
| AUX-016 | `LobbyIntegrationTests.ReleaseMatch_FinishedMatch_AllowsParticipantsToCreateAnotherLobby` | Passed en phase2-additional.trx. |
| AUX-017 | `LobbyServiceWcfTests.OpenLobbySession_InvalidKey_ReturnsTypedAccessFaultOverNamedPipe` | Passed en phase2-additional.trx. |
| AUX-018 | `MatchServiceWcfTests.OpenMatchSession_NonParticipant_CannotReadMatch` | Passed en phase2-additional.trx. |
| AUX-019 | `MatchServiceWcfTests.PlayTile_GeneratedProxy_PreservesWireContractsAndPrivateCallbacks` | Passed en phase2-additional.trx. |
| AUX-020 | `MatchServiceWcfTests.PlayTile_NamedPipe_IgnoresForgedValuesAndPublishesOnlyOwnHand` | Passed en phase2-additional.trx. |
| AUX-021 | `RankingServiceTests.GetWinsRankingPositionById_InvalidId_ReturnsSanitizedFault(-1)` | Passed en phase2-additional.trx. |
| AUX-022 | `RankingServiceTests.GetWinsRankingPositionById_InvalidId_ReturnsSanitizedFault(0)` | Passed en phase2-additional.trx. |
| AUX-023 | `SessionServiceTests.LoginWithAccount_MissingPassword_ReturnsInvalidCredentialsFault` | Passed en phase2-additional.trx. |
| AUX-024 | `SessionServiceTests.LoginWithAccount_NullCredentials_ReturnsInvalidCredentialsFault` | Passed en phase2-additional.trx. |

## Generador concreto y PreparedRound: regresión auxiliar (2026-11-16)

Después de retirar la sustitución del generador se compilaron los proyectos de la
solución y se volvieron a ejecutar los 13 casos de integración en memoria de Tests
más los 11 casos de AdditionalTest. Resultado: 24 aprobados, cero fallidos y cero
omitidos. La lista de casos auxiliares no cambió. La comprobación incluyó inicio
por MatchManager, WCF local net.pipe, callbacks y privacidad de manos.

Reportes locales de esta ejecución:

- Tests/TestResults/direct-deck-memory.trx: 13 aprobados.
- Tests/TestResults/direct-deck-additional.trx: 11 aprobados.

Los 13 casos de integración siguen ubicados en Tests, documentados aquí por su
clasificación; no se migraron en esta entrega. Las comprobaciones utilizan estado
controlado y no modifican datos reales de SQL Server.

## Fase 3 de Match: puntuación de jugadas (2026-11-16)

Se ejecutaron los mismos 13 casos de integración en memoria y los 11 de
AdditionalTest después de compilar la solución del servidor con PlayScoring.
Todos pasaron, sin fallidos ni omitidos. Incluyen operaciones de MatchManager,
transporte WCF local net.pipe, proxy generado, callbacks, privacidad de manos y
rechazo de valores de ficha falsificados. Los casos no se cambiaron en esta fase.

- Tests/TestResults/phase3-memory.trx: 13 aprobados.
- Tests/TestResults/phase3-additional.trx: 11 aprobados.
- Tests/TestResults/phase3-build.log: compilación de solución en Debug aprobada;
  conserva advertencias MSB3277 y MSB3836 de ensamblados y redirecciones.

Los 13 casos de integración permanecen ubicados en Tests; su documentación y
resultado siguen separados del catálogo unitario. Estas comprobaciones no
modifican datos reales de SQL Server. No se introdujeron dobles de colaboradores
ni mecanismos de sustitución para ejecutarlas.

## Fase 4 de Match: resolución de rondas (2026-11-16)

Se repitieron los 13 casos de integración en memoria y los 11 de AdditionalTest
con RoundResolution y los resultados de ronda protegidos contra modificaciones.
Pasaron los 24 casos, sin fallidos ni omitidos. No se agregaron ni modificaron
pruebas auxiliares en esta fase; se conservan las verificaciones de managers,
WCF net.pipe, callbacks, proxy generado y privacidad de manos.

- Tests/TestResults/phase4-memory.trx: 13 aprobados.
- Tests/TestResults/phase4-additional.trx: 11 aprobados.
- Tests/TestResults/phase4-build.log: compilación Debug de solución aprobada con
  las advertencias MSB3277 y MSB3836 existentes.

Los 13 casos de integración permanecen en Tests y se documentan aquí según su
clasificación, sin contarlos como unitarios. Las comprobaciones no operan sobre
datos reales de SQL Server. La prueba nueva de proyección del resumen utiliza
el mapper en memoria, sin alojar servicios ni abrir canales.

## Fase 5 de Match: coordinación y publicación (2026-11-16)

Pasaron 27 verificaciones auxiliares, sin fallidos ni omitidos: los 13 casos
de integración en memoria que permanecen en Tests y 14 de AdditionalTest.
Se conservaron los 24 casos anteriores y se añadieron los tres casos siguientes.
Todos utilizan componentes concretos y datos controlados; no realizan operaciones
sobre información real de SQL Server ni se contabilizan como unitarios.

Los tres nuevos casos pertenecen a
`DominoAllFives.Server.AdditionalTest.MatchPublicationWcfTests`, método
`LeaveMatch_DeferredPublication_DeliversOrderedFinishAndReleasesLobby`, en
[MatchPublicationWcfTests.cs](../DominoAllFives.Server/AdditionalTest/MatchPublicationWcfTests.cs#L27).
Cada caso aloja MatchService en un endpoint net.pipe local único, crea un lobby
y una partida con los managers reales, autentica dos sesiones y abre canales
dúplex. Confirma la jugada legal inicial mediante MatchManager, dejando la
versión 1 pendiente, y activa la publicación desde una llamada WCF LeaveMatch.
El callback real consulta LobbyManager durante OnMatchFinished para comprobar
que el lobby ya fue liberado. Se abortan canales, retiran sesiones y liberan los
datos del caso al finalizar.

### AUX-025. Activación posterior con dos destinatarios

- **Caso ejecutable:** `LeaveMatch_DeferredPublication_DeliversOrderedFinishAndReleasesLobby(0)`.
- **Precondiciones:** Ambos canales siguen abiertos; la jugada confirmó versión 1
  sin activar el publicador. La llamada WCF de salida confirma la versión 2.
- **Resultado esperado:** Ambos destinatarios reciben exactamente `view:1`,
  `hand:1`, `view:2`, `hand:2`, `finished:2`, cada uno con dos manos de su propio
  jugador; el lobby ya no existe al manejar finished y la consulta final es versión 2.
- **Resultado obtenido:** Passed, coincidente con el esperado.
- **Evidencia:** `phase5-additional.trx`, caso
  `372e3255-1ce0-a487-84b1-1e074fa9bd91`; duración `00:00:00.1830000`.

### AUX-026. Destinatario desconectado

- **Caso ejecutable:** `LeaveMatch_DeferredPublication_DeliversOrderedFinishAndReleasesLobby(1)`.
- **Precondiciones:** Se aborta el segundo canal y su fábrica después de abrir
  sesión y antes de confirmar las mutaciones. La salida WCF confirma versión 2
  y activa la jugada pendiente y el cierre.
- **Resultado esperado:** El primer destinatario recibe exactamente `view:1`,
  `hand:1`, `view:2`, `hand:2`, `finished:2`, conserva privacidad de sus dos manos
  y observa el lobby liberado. El segundo canal no impide finalizar; versión final 2.
- **Resultado obtenido:** Passed, coincidente con el esperado. El aborto real puede
  retirar el registro antes del envío; no se afirma forzar una excepción específica.
- **Evidencia:** `phase5-additional.trx`, caso
  `5c443967-ee02-822a-f3a8-a5df1c6f4d98`; duración `00:00:00.0890000`.

### AUX-027. Reintento de salida sin nueva mutación

- **Caso ejecutable:** `LeaveMatch_DeferredPublication_DeliversOrderedFinishAndReleasesLobby(2)`.
- **Precondiciones:** MatchManager confirma directamente jugada 1 y salida 2 sin
  activar el publicador. La llamada WCF repite la salida ya confirmada.
- **Resultado esperado:** El reintento activa la cola pendiente sin añadir otra
  actualización. Ambos destinatarios reciben exactamente `view:1`, `hand:1`,
  `view:2`, `hand:2`, `finished:2`, con manos propias y lobby liberado durante el
  callback final; el estado consultable conserva versión 2.
- **Resultado obtenido:** Passed, coincidente con el esperado.
- **Evidencia:** `phase5-additional.trx`, caso
  `1965c553-c639-a58e-e99d-0fb26109617e`; duración `00:00:00.0160000`.

Reportes locales de esta ejecución, bajo DominoAllFives.Server/Tests/TestResults:

- `phase5-memory.trx`: 13 aprobados.
- `phase5-additional.trx`: 14 aprobados.
- `phase5-build.log`: compilación Debug final de solución aprobada, con las
  advertencias MSB3836 de redirecciones existentes.

No se atribuye cobertura de recepción persistente, recuperación tras caída del
proceso ni de la carrera exacta entre registro de callback y cierre. El fallo de
generación de otra ronda sigue fuera de la cobertura reproducida. Las 298 pruebas
unitarias se registran individualmente y exclusivamente en Pruebas.md.

## Fase 6 de Match: contratos y cliente (2026-11-17)

Pasaron 72 verificaciones auxiliares, sin fallidos ni omitidos: 13 de managers
en memoria que permanecen en Tests, 33 de AdditionalTest y 26 de colaboración
de MatchViewModel y trabajo en segundo plano con Dispatcher WPF. Las 27 del
servidor de la fase anterior se repitieron; se añadieron 19 integraciones WCF
en AdditionalTest y se incluyeron las 26 comprobaciones existentes del cliente.
Ninguno de estos casos se cuenta como unitario ni se incluye en Pruebas.md.

Los nuevos casos de MatchServiceWcfTests usan el proxy generado real, un host
net.pipe aislado, sesiones auténticas de SessionManager y partidas creadas por
el manager concreto. Los rechazos comprueban el fault tipado, versión 0 y estado
InProgress sin mutación. Las versiones obsoletas envían -1; el turno ajeno usa
el otro participante; Draw y Pass se rechazan antes de la salida legal inicial;
el caso missing envía tile null. Los casos de acceso cambian el matchId o revocan
la clave real, según el parámetro revoked. No hay inyección de dobles o fallos
en producción. CommandSession solo organiza los componentes reales del fixture.

Las salidas usan LeaveMatch una vez o dos según repeat, esperan vista, mano y
resultado y consultan el estado final: Cancelled, versión 1 y exactamente una
vista, una mano y un resultado. El reintento no confirma otra transición.
Los canales se abortan y se retiran claves y partidas al finalizar cada caso.

Se adaptaron las dos integraciones preexistentes de PlayTile para consultar con
GetMatch tras el comando void. La del proxy también comprueba la versión del
callback público; ambas mantienen rechazo de datos falsificados y privacidad.
Las pruebas de publicación de la fase 5 vuelven a verificar orden y liberación
del lobby. No se realizan operaciones sobre datos reales de SQL Server.

### Casos WCF añadidos en esta fase

Origen: [MatchServiceWcfTests.cs](../DominoAllFives.Server/AdditionalTest/MatchServiceWcfTests.cs).
Clase: `DominoAllFives.Server.AdditionalTest.MatchServiceWcfTests`.
Evidencia: `DominoAllFives.Server/Tests/TestResults/phase6-additional.trx`.
Cada fila es un caso ejecutado distinto; el último parámetro de los rechazos
de reglas identifica el fault esperado.

| N.º | Caso ejecutable | Resultado y evidencia |
| --- | --- | --- |
| AUX-028 | `Command_GeneratedProxy_ReturnsTypedAccessFault("DrawTiles",False)` | Passed; `b51c16ec-ed1f-6114-e3a8-3680752210ef`; `00:00:00.0060000`. |
| AUX-029 | `Command_GeneratedProxy_ReturnsTypedAccessFault("DrawTiles",True)` | Passed; `1ceb9442-d6b6-481e-349d-9e0e998926b0`; `00:00:00.0090000`. |
| AUX-030 | `Command_GeneratedProxy_ReturnsTypedAccessFault("LeaveMatch",False)` | Passed; `22ebe7bc-80dd-263d-19f0-b24e86a3482d`; `00:00:00.0050000`. |
| AUX-031 | `Command_GeneratedProxy_ReturnsTypedAccessFault("LeaveMatch",True)` | Passed; `3ac1cb80-64ad-cd3f-f288-e83bdb4c82c8`; `00:00:00.0050000`. |
| AUX-032 | `Command_GeneratedProxy_ReturnsTypedAccessFault("PassTurn",False)` | Passed; `0cf65579-6bdc-45bd-49d8-e96f37c74079`; `00:00:00.0110000`. |
| AUX-033 | `Command_GeneratedProxy_ReturnsTypedAccessFault("PassTurn",True)` | Passed; `d1807312-6ecf-34de-1c87-9500a51f4337`; `00:00:00.0050000`. |
| AUX-034 | `Command_GeneratedProxy_ReturnsTypedAccessFault("PlayTile",False)` | Passed; `5d190a02-899b-d4c4-d430-7d2bd26c2dea`; `00:00:00.0310000`. |
| AUX-035 | `Command_GeneratedProxy_ReturnsTypedAccessFault("PlayTile",True)` | Passed; `5b6a445e-8e42-9480-a050-ba1b9913c75d`; `00:00:00.0050000`. |
| AUX-036 | `Command_GeneratedProxy_ReturnsTypedRuleFaultWithoutMutation("DrawTiles","legal",DrawNotAllowed)` | Passed; `4e75929a-ced4-18cf-2499-687d2cdd62ff`; `00:00:00.0050000`. |
| AUX-037 | `Command_GeneratedProxy_ReturnsTypedRuleFaultWithoutMutation("DrawTiles","stale",StaleVersion)` | Passed; `068a56b9-9e57-3b4c-6005-270e65c503b2`; `00:00:00.0060000`. |
| AUX-038 | `Command_GeneratedProxy_ReturnsTypedRuleFaultWithoutMutation("DrawTiles","turn",NotPlayersTurn)` | Passed; `51f1b215-4494-392e-6d22-14e02e1d1dbc`; `00:00:00.0080000`. |
| AUX-039 | `Command_GeneratedProxy_ReturnsTypedRuleFaultWithoutMutation("PassTurn","legal",PassNotAllowed)` | Passed; `2aadfdd8-707a-67d8-3738-dda8a3509043`; `00:00:00.0060000`. |
| AUX-040 | `Command_GeneratedProxy_ReturnsTypedRuleFaultWithoutMutation("PassTurn","stale",StaleVersion)` | Passed; `d9efd00d-b46f-6a05-f405-67f45bbaac9d`; `00:00:00.0060000`. |
| AUX-041 | `Command_GeneratedProxy_ReturnsTypedRuleFaultWithoutMutation("PassTurn","turn",NotPlayersTurn)` | Passed; `afa0776f-bb20-b74c-8fc3-d52f73b8b9bb`; `00:00:00.0050000`. |
| AUX-042 | `Command_GeneratedProxy_ReturnsTypedRuleFaultWithoutMutation("PlayTile","missing",TileNotOwned)` | Passed; `01375bd0-7ce7-2401-7790-8699ef6154a0`; `00:00:00.0140000`. |
| AUX-043 | `Command_GeneratedProxy_ReturnsTypedRuleFaultWithoutMutation("PlayTile","stale",StaleVersion)` | Passed; `6801ee65-88f9-af29-23db-dce046424bde`; `00:00:00.0090000`. |
| AUX-044 | `Command_GeneratedProxy_ReturnsTypedRuleFaultWithoutMutation("PlayTile","turn",NotPlayersTurn)` | Passed; `c6a8a5b4-3ad1-3c2d-b457-b0cad3544df5`; `00:00:00.0050000`. |
| AUX-045 | `LeaveMatch_GeneratedProxy_PublishesSingleTerminalSnapshot(False)` | Passed; `9282c468-0e30-5d6a-d3d9-a6885242eef6`; `00:00:00.0380000`. |
| AUX-046 | `LeaveMatch_GeneratedProxy_PublishesSingleTerminalSnapshot(True)` | Passed; `2247e81a-9995-c802-2072-be265eac8eb1`; `00:00:00.0310000`. |

### Colaboración del cliente comprobada

MatchBackgroundTests y MatchViewModelTests permanecen en el proyecto histórico
DominoAllFives.Client.Tests. Su uso de WpfTestHost, Dispatcher, BackgroundWork,
callbacks y navegación se clasifica aquí como verificación auxiliar. No se
añadieron casos ni nuevos puntos de inyección para esta ejecución; se adaptaron
las implementaciones de IMatchClient existentes a los comandos void y a MatchStatus.
Se conservan los dobles que esos fixtures ya utilizaban, sin presentarlos como
comprobaciones del transporte real. Comprueban captura de intención durante
callbacks, consultas de recuperación sin repetir jugadas, ausencia de operaciones
superpuestas, Draw/Pass en segundo plano, disposición y navegación tras salida.

Evidencia: `DominoAllFives.Server/Tests/TestResults/phase6-client-memory.trx`.

| Clase y caso ejecutable | Resultado y evidencia |
| --- | --- |
| `MatchViewModelTests.Callbacks_DifferentVersions_WaitsForCoherentSnapshot` | Passed; `e0704d27-1d63-b734-331a-de1e2545be4f`; `00:00:00.0040000`. |
| `MatchViewModelTests.Callbacks_DisposedModel_DiscardsQueuedSnapshot` | Passed; `b0eda227-e918-5ba1-bd6b-7a3c0262d7a6`; `00:00:00.0010000`. |
| `MatchViewModelTests.Callbacks_OlderRevision_DoesNotReplaceNewerHand` | Passed; `6cb9f298-0d54-2f26-37ea-d5fe69b3dba6`; `00:00:00.0000001`. |
| `MatchViewModelTests.Callbacks_OtherMatchView_DoesNotPoisonSnapshotVersions` | Passed; `f458a022-77b7-2ae7-b902-08976a4395ad`; `00:00:00.0000001`. |
| `MatchViewModelTests.Callbacks_OtherPlayersHand_IsIgnored` | Passed; `0de658bc-ccfe-fb86-94bb-a4326690fcbf`; `00:00:00.0000001`. |
| `MatchViewModelTests.Callbacks_PlayerSummaries_ShowNamesGoalAndOpponentCounts` | Passed; `2697217b-2c77-ec18-a410-d7b1d9180410`; `00:00:00.0010000`. |
| `MatchViewModelTests.Callbacks_PlayScore_ShowsOnlyAwardAndApplicableMultiplier(0,2,"","")` | Passed; `2f0dd19f-18d5-2a37-4a79-9ef7e13594bc`; `00:00:00.0000001`. |
| `MatchViewModelTests.Callbacks_PlayScore_ShowsOnlyAwardAndApplicableMultiplier(20,1,"+20","")` | Passed; `2f4ecca0-40bc-aa7f-8149-b2b52858da05`; `00:00:00.0010000`. |
| `MatchViewModelTests.Callbacks_PlayScore_ShowsOnlyAwardAndApplicableMultiplier(40,2,"+40","×2")` | Passed; `58d6328a-0b2a-3638-55ce-df2d2418b72d`; `00:00:00.0000001`. |
| `MatchBackgroundTests.Dispose_PendingSuccessOrFailure_ClosesOnceAfterWorkWithoutLeaving(False)` | Passed; `33b2922e-2b57-c0a0-26e2-e1662db757c1`; `00:00:00.1810000`. |
| `MatchBackgroundTests.Dispose_PendingSuccessOrFailure_ClosesOnceAfterWorkWithoutLeaving(True)` | Passed; `630c9862-0dcb-0c31-c341-a7085c40aaae`; `00:00:00.0990000`. |
| `MatchBackgroundTests.DrawOrPass_AllowedAction_UsesBackgroundOperation(False)` | Passed; `cb379a15-d55d-56b1-4557-71233500b1ed`; `00:00:00.0020000`. |
| `MatchBackgroundTests.DrawOrPass_AllowedAction_UsesBackgroundOperation(True)` | Passed; `4d468121-df45-bb4c-389f-38343f1858eb`; `00:00:00.0120000`. |
| `MatchBackgroundTests.Initialize_FailedCall_ReportsErrorAndAllowsRetry(System.InvalidOperationException)` | Passed; `5601f6b2-b81a-41d1-01e8-bd81e276ff11`; `00:00:00.0030000`. |
| `MatchBackgroundTests.Initialize_FailedCall_ReportsErrorAndAllowsRetry(System.ServiceModel.CommunicationException)` | Passed; `aeb6ea92-5273-29df-dbc8-093e7263dbde`; `00:00:00.0010000`. |
| `MatchBackgroundTests.Initialize_FailedCall_ReportsErrorAndAllowsRetry(System.TimeoutException)` | Passed; `53735ae1-54c1-da9e-b617-ac2229a868cf`; `00:00:00.0040000`. |
| `MatchBackgroundTests.Initialize_PendingCall_KeepsUiResponsiveAndPreventsOverlappingRequests` | Passed; `60c52b07-3e75-0498-baf8-e5349250098b`; `00:00:00.0010000`. |
| `MatchBackgroundTests.Leave_RemoteCompletion_NavigatesOnlyAfterSuccess(False)` | Passed; `7f7c5d11-031a-bfa8-9734-76df9d1b9a81`; `00:00:00.0020000`. |
| `MatchBackgroundTests.Leave_RemoteCompletion_NavigatesOnlyAfterSuccess(True)` | Passed; `ede0d5fa-43f4-c6cd-60d5-3113b452f033`; `00:00:00.0010000`. |
| `MatchBackgroundTests.Play_CallbackDuringPendingCall_PreservesCapturedTileEndAndVersion` | Passed; `6bb3a2f9-a8c0-7cb6-b0c1-96211ef8106d`; `00:00:00.0100000`. |
| `MatchBackgroundTests.Play_StaleVersion_QueriesWithoutReplayingOrRepeatingFailedRecovery(False)` | Passed; `02638b15-7bae-2c67-21ab-ab578a26676f`; `00:00:00.0210000`. |
| `MatchBackgroundTests.Play_StaleVersion_QueriesWithoutReplayingOrRepeatingFailedRecovery(True)` | Passed; `94ef75f5-aff5-6867-f342-1e98a5e8fb02`; `00:00:00.0010000`. |
| `MatchViewModelTests.PlayCommand_TurnAndPhase_EnforcesLocalAvailability(1,Cancelled,False)` | Passed; `5126142f-9f16-8a9c-614c-5f493f313f74`; `00:00:00.0000001`. |
| `MatchViewModelTests.PlayCommand_TurnAndPhase_EnforcesLocalAvailability(1,Completed,False)` | Passed; `3ef85ab9-6c9e-5806-e56e-c1f4b54ecd9a`; `00:00:00.0000001`. |
| `MatchViewModelTests.PlayCommand_TurnAndPhase_EnforcesLocalAvailability(1,InProgress,True)` | Passed; `a9b4c5bd-8048-9309-5479-2bc7ae4d918d`; `00:00:00.0010000`. |
| `MatchViewModelTests.PlayCommand_TurnAndPhase_EnforcesLocalAvailability(2,InProgress,False)` | Passed; `45c188f3-87bb-5db1-21db-aeed9b1911fd`; `00:00:00.0000001`. |

### Compilación y límites

- `phase6-build.log`: solución servidor Debug aprobada; advertencias MSB3836 existentes.
- `phase6-client-build.log`: solución cliente Debug aprobada; CS0168 existentes
  en las capturas de MatchClient y LanguageManager.
- `phase6-memory.trx`: 13 aprobadas.
- `phase6-additional.trx`: 33 aprobadas.
- `phase6-client-memory.trx`: 26 aprobadas.
- Proxy regenerado desde metadatos exportados del Contracts actual. Se verificó
  que regenerarlo dos veces produce el mismo hash de Reference.cs.

Los reportes y logs locales están bajo DominoAllFives.Server/Tests/TestResults.
No se ejecutó UI manual ni transporte net.tcp remoto, ni una partida completa por
WCF que fuerce Draw y Pass exitosos; sus rechazos y faults sí se ejecutaron por
transporte real y sus reglas de dominio conservan la regresión unitaria.
No se afirma cobertura de recuperación persistente, compatibilidad con binarios
anteriores ni del fallo de generación de otra ronda. Las 338 pruebas unitarias
ejecutadas están documentadas individualmente y por separado en Pruebas.md.

## Preparación previa de jugadas y cierres (2026-11-17)

Se repitieron las mismas 72 verificaciones auxiliares tras separar preparación
y aplicación del dominio. Pasaron todas, sin fallidos ni omitidos: 13 de managers
en memoria, 33 de AdditionalTest y 26 de colaboración WPF. No se añadieron casos
auxiliares ni se cambió su clasificación o ubicación histórica.

La regresión incluye comandos con proxy generado, faults tipados, privacidad de
manos, versiones ordenadas, reintentos de salida y liberación del lobby antes del
callback final. Los fixtures del cliente conservan los dobles existentes; no se
añadieron entradas de producción ni sustitutos para provocar errores del generador.
Los casos WCF utilizan host net.pipe, canales y componentes reales. No se operó
sobre información real de SQL Server ni se realizó un despliegue remoto.

Reportes de esta ejecución, bajo DominoAllFives.Server/Tests/TestResults:

- `prepared-transition-memory.trx`: 13 aprobadas.
- `prepared-transition-additional.trx`: 33 aprobadas.
- `prepared-transition-client-memory.trx`: 26 aprobadas.
- `prepared-transition-build.log`: solución servidor Debug aprobada, con MSB3836 existentes.
- `prepared-transition-test-build.log`: Tests recompilado tras actualizar expectativas y nombres.

Las pruebas ejecutadas de entradas inválidas pertenecen al catálogo unitario y
usan datos concretos. No se afirma haber reproducido un fallo técnico del
generador durante el comando real. Tampoco se afirma atomicidad ante fallos al
construir o registrar el snapshot después de aplicar el dominio, ni persistencia
o confirmación de recepción de callbacks. Las 359 fichas unitarias, cada una con
su propia numeración y evidencia, están exclusivamente en Pruebas.md.


## Migración del registro de cuenta (2026-11-18)

Se incorporaron doce verificaciones auxiliares: nueve de RegistrationServiceWcfTests
y tres de RegistrationPresentationTests. Todas pasaron. Las primeras usan
DominoService y el proxy generado real con un host net.pipe aislado; los datos
inválidos detienen el controller antes de abrir persistencia. El roundtrip usa
serializadores concretos del contrato servidor y del DTO generado. Las pruebas de
presentación usan PasswordBox, DialogService, MainWindowViewModel, PlayerSession
y RegistrationClient concretos; campos vacíos y contraseñas distintas no llegan a WCF.
La cancelación comprueba navegación local sin modificar la sesión.

No se añadieron interfaces ni rutas de producción para facilitar las pruebas.
IRegistrationClient es la dependencia remota de presentación solicitada en la
arquitectura. Los casos se conservan como auxiliares por su colaboración con
transporte, serialización y WPF; no amplían el catálogo unitario de Pruebas.md.

Los reportes están bajo `DominoAllFives.Server/Tests/TestResults`.
Cada fila identifica un caso nuevo ejecutado y su evidencia individual.
En los rechazos se espera exactamente el RegistrationIssue indicado en los
parámetros o InvalidRegistrationData para null. El roundtrip conserva los cuatro
campos, los espacios de la contraseña y pt-BR. La presentación conserva la sesión
sin autenticar; el caso de contraseñas distintas muestra el mensaje localizado.

| N.º | Caso ejecutado | Resultado y evidencia |
| --- | --- | --- |
| AUX-047 | `Register_InvalidInput_ReturnsTypedFaultBeforePersistence("player","player@example.com","StrongPass1!",InvalidEmail)` | Passed; `registration-wcf.trx`; `92d54abe-afcc-0ee0-00a9-9fc0d2647ae3`; `00:00:00.0050000`. |
| AUX-048 | `Serialize_RegistrationRequest_PreservesFieldsAcrossGeneratedProxy` | Passed; `registration-wcf.trx`; `8bb69cc1-2364-4837-539c-f200abc96f71`; `00:00:00.0170000`. |
| AUX-049 | `Register_InvalidInput_ReturnsTypedFaultBeforePersistence("   ","invalid-email","weak",InvalidUsername)` | Passed; `registration-wcf.trx`; `2a04cb2a-e658-db58-73ba-47b8d4d0a233`; `00:00:00.0090000`. |
| AUX-050 | `Register_InvalidInput_ReturnsTypedFaultBeforePersistence("player",null,"StrongPass1!",InvalidEmail)` | Passed; `registration-wcf.trx`; `68966240-5f7b-b7aa-11d2-feaf96c87d0d`; `00:00:00.0050000`. |
| AUX-051 | `Register_InvalidInput_ReturnsTypedFaultBeforePersistence("player","player@gmail.com","weak",InvalidPassword)` | Passed; `registration-wcf.trx`; `d84579c1-0d5d-8a8f-e185-67eefcd31b4e`; `00:00:00.0080000`. |
| AUX-052 | `Register_InvalidInput_ReturnsTypedFaultBeforePersistence("player","player@gmail.com",null,InvalidPassword)` | Passed; `registration-wcf.trx`; `50238fe4-098a-b1ff-d7c5-c5c95bed1650`; `00:00:00.0050000`. |
| AUX-053 | `Register_InvalidInput_ReturnsTypedFaultBeforePersistence(null,"player@gmail.com","StrongPass1!",InvalidUsername)` | Passed; `registration-wcf.trx`; `1b7072c2-d942-84ff-f036-140e77f0acc7`; `00:00:01.0030000`. |
| AUX-054 | `Register_NullRequest_ReturnsTypedInvalidDataFault` | Passed; `registration-wcf.trx`; `77a5ac35-4355-10c8-e60e-bf243948b3de`; `00:00:00.0060000`. |
| AUX-055 | `Register_InvalidInput_ReturnsTypedFaultBeforePersistence("player","invalid-email","weak",InvalidEmail)` | Passed; `registration-wcf.trx`; `23954b8b-dd85-82dd-6e6c-faacbd8f3550`; `00:00:00.0070000`. |
| AUX-056 | `Register_DifferentPasswords_ShowsLocalizedWarningWithoutStartingSession` | Passed; `registration-client.trx`; `9d2fb0b6-bcc0-811b-8111-8a3075dea7a2`; `00:00:00.2110000`. |
| AUX-057 | `Register_EmptyFields_ShowsRequiredIndicatorsWithoutStartingSession` | Passed; `registration-client.trx`; `4de4f2aa-d6b2-9759-e4ab-9dea4a494141`; `00:00:00.0000001`. |
| AUX-058 | `Cancel_IdleRegistration_InvokesNavigationWithoutChangingSession` | Passed; `registration-client.trx`; `0d04b246-f360-980d-5120-51a272e99291`; `00:00:00.0000001`. |

Fuentes:
[RegistrationServiceWcfTests.cs](../DominoAllFives.Server/AdditionalTest/RegistrationServiceWcfTests.cs)
y [RegistrationPresentationTests.cs](../DominoAllFives.Client/DominoAllFives.Client.Tests/RegistrationPresentationTests.cs).

La selección completa pasó 64 casos: 13 en registration-wcf.trx (incluye cuatro
regresiones de guardas de Login y Ranking), 30 en registration-client.trx
(incluye 27 regresiones existentes de Login, Credentials y Ranking) y 21 en
registration-server-regression.trx (PlayerAccount, SessionManager y contrato Login).
La prueba existente Factory_AuthenticatedSession_ReturnsInitializedPagesAndModal
también comprueba la apertura y cancelación del registro ya habilitado.
Se ejecutó inicialmente ese caso sobre un binario anterior mientras compilaba;
la ejecución final, tras finalizar la compilación, pasó los 30 casos del cliente.

Compilaron ambas soluciones Debug con MSBuild de Visual Studio. Evidencia:
registration-server-build.log y registration-client-build.log. Permanecen las
advertencias preexistentes MSB3836 del servidor y CS0168 de LanguageManager y
MatchClient. El intento inicial con dotnet build terminó sin diagnóstico útil;
se utilizó MSBuild de Visual Studio para la compilación verificada.

El proxy se regeneró dos veces desde Contracts con SvcUtil 4.8 y produjo el mismo
SHA-256: `f98b993b8339f8d110e4697bd18758af1406d3aa3c35dd107384dd8d8fd1dee3`.
El script y los metadatos quedan junto a Reference.cs.

Límites: no se usó SQL Server ni se creó una cuenta real. El alta exitosa con
Player+PlayerStats, duplicados, hash persistido y sesión devuelta requiere una BD
aislada configurada antes de su verificación integral. No se probaron UI manual,
net.tcp desplegado, fallos reales de persistencia ni el timeout exacto de Close.
No se afirma cobertura de fallos posteriores al commit ni idempotencia; una
respuesta perdida puede corresponder a una cuenta ya creada.


## Migración de perfil e imagen (2026-11-19)

Se añadieron 44 casos auxiliares: 34 del servidor con proxy generado/host net.pipe
y 10 del cliente con WPF y servicios concretos. Todos pasaron. Se ejecutaron
además 46 regresiones existentes: 13 en el reporte WCF y 33 del cliente.
Resultado final seleccionado: 90 Passed, cero fallidos y cero omitidos.

Los casos de sesión cubren las cinco operaciones con clave nula, vacía, espacios,
desconocida y revocada. Se espera AccessIssue.SessionRequired antes de acceder a
persistencia. Los casos de imagen con sesión auténtica usan un identificador sin
datos y se detienen por validación antes de crear el contexto EF: selección nula,
vacía, extensión inválida, precedencia extensión/tamaño, tamaño mayor a 1 MiB y
reemplazo conjunto nulo. Se espera el ProfileIssue indicado en cada caso.

Los tres roundtrips conservan perfil con y sin imagen, estadísticas, nombres y
contenido seleccionado independiente. Las diez comprobaciones de presentación
usan FrameNavigationService, MainWindowViewModel/DialogService, PlayerSession,
ProfileClient, ProfilePictureFileService y ProfilePictureImageService
concretos; ninguna abre el selector ni llama al servidor. Comprueban ausencia
de sesión, carga deshabilitada, subida sin selección, omisión/cancelación que
conservan la cuenta y fallback de imagen ausente, vacía o corrupta.

No se añadieron entradas de producción para pruebas ni dobles. El proyecto
auxiliar del cliente está en DominoAllFives.Client/AdditionalTest, separado del
proyecto histórico de tests y no incorporado a la solución. No se agregan estos
casos al catálogo unitario de Pruebas.md.

Evidencia individual bajo DominoAllFives.Server/Tests/TestResults:

| N.º | Caso ejecutado | Resultado y evidencia |
| --- | --- | --- |
| AUX-059 | `Operation_InvalidSession_ReturnsTypedAccessFaultBeforePersistence("GetProfile","null")` | Passed; `profile-wcf.trx`; `8c6d3256-3940-9bd7-6c22-2cf143bf646e`; `00:00:03.0890000`. |
| AUX-060 | `UpdateProfileWithPicture_NullSelection_ReturnsTypedFaultBeforePersistence` | Passed; `profile-wcf.trx`; `37f752c3-6428-968d-e1ef-dbe153a67949`; `00:00:00.0150000`. |
| AUX-061 | `Operation_InvalidSession_ReturnsTypedAccessFaultBeforePersistence("RemoveProfilePicture","whitespace")` | Passed; `profile-wcf.trx`; `3f546032-922b-188b-8991-ab5ee23ff6fa`; `00:00:00.0160000`. |
| AUX-062 | `Operation_InvalidSession_ReturnsTypedAccessFaultBeforePersistence("RemoveProfilePicture","null")` | Passed; `profile-wcf.trx`; `1775d322-3e37-19eb-e2c4-090fd38c6608`; `00:00:00.0110000`. |
| AUX-063 | `Serialize_SelectedPicture_PreservesOriginalNameAndCopiedContent` | Passed; `profile-wcf.trx`; `ca15e7fe-e1c9-3b66-d641-79d87e3c2d54`; `00:00:00.0010000`. |
| AUX-064 | `Serialize_Profile_PreservesIdentityStatisticsAndOptionalPicture(False)` | Passed; `profile-wcf.trx`; `aa5eb4c4-82b5-cdcb-7f31-8595e5b6759a`; `00:00:00.1840000`. |
| AUX-065 | `Operation_InvalidSession_ReturnsTypedAccessFaultBeforePersistence("UpdateProfile","whitespace")` | Passed; `profile-wcf.trx`; `4c301ddf-2bf2-9c2b-8a86-5bf7eb271473`; `00:00:00.0130000`. |
| AUX-066 | `Operation_InvalidSession_ReturnsTypedAccessFaultBeforePersistence("GetProfile","unknown")` | Passed; `profile-wcf.trx`; `3ceb3a94-09dd-349d-3907-a2abc86b0694`; `00:00:00.0240000`. |
| AUX-067 | `SetProfilePicture_InvalidSelection_ReturnsTypedFaultBeforePersistence("null",InvalidProfilePicture)` | Passed; `profile-wcf.trx`; `eb1b662e-6d3f-ad89-9abf-c758a74be0a3`; `00:00:00.0200000`. |
| AUX-068 | `Operation_InvalidSession_ReturnsTypedAccessFaultBeforePersistence("SetProfilePicture","whitespace")` | Passed; `profile-wcf.trx`; `069e77bf-4d37-b627-82a5-67241fa42cc0`; `00:00:00.0090000`. |
| AUX-069 | `Operation_InvalidSession_ReturnsTypedAccessFaultBeforePersistence("UpdateProfileWithPicture","null")` | Passed; `profile-wcf.trx`; `fb3a0b53-a9e4-60ec-b67e-588fcb1db72d`; `00:00:00.0270000`. |
| AUX-070 | `Operation_InvalidSession_ReturnsTypedAccessFaultBeforePersistence("UpdateProfile","null")` | Passed; `profile-wcf.trx`; `5bb1bc40-704b-ddd1-774d-1fd72181c0c9`; `00:00:00.0350000`. |
| AUX-071 | `SetProfilePicture_InvalidSelection_ReturnsTypedFaultBeforePersistence("format-before-size",InvalidProfilePictureFormat)` | Passed; `profile-wcf.trx`; `0d04c2f4-c42d-4627-083d-67c7a6814991`; `00:00:00.0250000`. |
| AUX-072 | `Operation_InvalidSession_ReturnsTypedAccessFaultBeforePersistence("SetProfilePicture","revoked")` | Passed; `profile-wcf.trx`; `588ef6b5-eac2-20b4-c4b2-c6594772a0db`; `00:00:00.0150000`. |
| AUX-073 | `Operation_InvalidSession_ReturnsTypedAccessFaultBeforePersistence("GetProfile","whitespace")` | Passed; `profile-wcf.trx`; `03b86169-c8ff-a66c-e0a8-0149be1d9d22`; `00:00:00.0200000`. |
| AUX-074 | `Operation_InvalidSession_ReturnsTypedAccessFaultBeforePersistence("UpdateProfileWithPicture","empty")` | Passed; `profile-wcf.trx`; `c079bcde-2904-9cb6-6ef9-f4eb9514f1a5`; `00:00:00.0110000`. |
| AUX-075 | `SetProfilePicture_InvalidSelection_ReturnsTypedFaultBeforePersistence("empty",InvalidProfilePicture)` | Passed; `profile-wcf.trx`; `77f0f3df-469e-5a47-829e-7b1b7c3cc41c`; `00:00:00.1290000`. |
| AUX-076 | `Operation_InvalidSession_ReturnsTypedAccessFaultBeforePersistence("UpdateProfileWithPicture","unknown")` | Passed; `profile-wcf.trx`; `e2864800-81b8-5fa9-dd76-0cb3d734c56f`; `00:00:00.0090000`. |
| AUX-077 | `Operation_InvalidSession_ReturnsTypedAccessFaultBeforePersistence("SetProfilePicture","unknown")` | Passed; `profile-wcf.trx`; `b388b902-e4db-9748-4068-58169d5487bb`; `00:00:00.0120000`. |
| AUX-078 | `Operation_InvalidSession_ReturnsTypedAccessFaultBeforePersistence("GetProfile","empty")` | Passed; `profile-wcf.trx`; `a55723f8-7d01-b184-813e-9280b0fa8dca`; `00:00:00.0210000`. |
| AUX-079 | `Operation_InvalidSession_ReturnsTypedAccessFaultBeforePersistence("SetProfilePicture","empty")` | Passed; `profile-wcf.trx`; `9c842590-160c-f1fe-7b50-f05f0d0a5309`; `00:00:00.0140000`. |
| AUX-080 | `Operation_InvalidSession_ReturnsTypedAccessFaultBeforePersistence("RemoveProfilePicture","empty")` | Passed; `profile-wcf.trx`; `54bb2a53-5d61-c5af-2de8-11e0a78d0920`; `00:00:00.0130000`. |
| AUX-081 | `Operation_InvalidSession_ReturnsTypedAccessFaultBeforePersistence("SetProfilePicture","null")` | Passed; `profile-wcf.trx`; `bf9885eb-9eaa-0969-e72c-b566349664da`; `00:00:00.0120000`. |
| AUX-082 | `Operation_InvalidSession_ReturnsTypedAccessFaultBeforePersistence("UpdateProfileWithPicture","whitespace")` | Passed; `profile-wcf.trx`; `49da57a4-403f-fca2-ff45-787aa440907d`; `00:00:00.0130000`. |
| AUX-083 | `Operation_InvalidSession_ReturnsTypedAccessFaultBeforePersistence("RemoveProfilePicture","unknown")` | Passed; `profile-wcf.trx`; `a0a857ec-a54e-7d2a-228a-592ed524f9d4`; `00:00:00.0140000`. |
| AUX-084 | `SetProfilePicture_InvalidSelection_ReturnsTypedFaultBeforePersistence("large",ProfilePictureTooLarge)` | Passed; `profile-wcf.trx`; `486d9c77-4bd2-80bf-3fab-84a74bc066e4`; `00:00:00.0220000`. |
| AUX-085 | `Operation_InvalidSession_ReturnsTypedAccessFaultBeforePersistence("RemoveProfilePicture","revoked")` | Passed; `profile-wcf.trx`; `ed78beb1-b4b4-defd-aa96-a3faaeeb2edb`; `00:00:00.0130000`. |
| AUX-086 | `SetProfilePicture_InvalidSelection_ReturnsTypedFaultBeforePersistence("format",InvalidProfilePictureFormat)` | Passed; `profile-wcf.trx`; `7fbd840d-43fd-67de-3c3f-da401f7f26af`; `00:00:00.0140000`. |
| AUX-087 | `Operation_InvalidSession_ReturnsTypedAccessFaultBeforePersistence("UpdateProfileWithPicture","revoked")` | Passed; `profile-wcf.trx`; `bd66562e-b34b-7391-08ab-64a023555bd6`; `00:00:00.0140000`. |
| AUX-088 | `Operation_InvalidSession_ReturnsTypedAccessFaultBeforePersistence("UpdateProfile","unknown")` | Passed; `profile-wcf.trx`; `34cc09b3-eb48-6370-dacc-82c0a6bf496f`; `00:00:00.0100000`. |
| AUX-089 | `Serialize_Profile_PreservesIdentityStatisticsAndOptionalPicture(True)` | Passed; `profile-wcf.trx`; `c84a6f4d-7d3f-f485-1c3f-1c3248fbf455`; `00:00:00.0140000`. |
| AUX-090 | `Operation_InvalidSession_ReturnsTypedAccessFaultBeforePersistence("GetProfile","revoked")` | Passed; `profile-wcf.trx`; `82f8b31a-4366-0411-18a6-7b3e89d8c3f8`; `00:00:00.0140000`. |
| AUX-091 | `Operation_InvalidSession_ReturnsTypedAccessFaultBeforePersistence("UpdateProfile","revoked")` | Passed; `profile-wcf.trx`; `178d0629-956e-6e38-8b51-886daa3985cf`; `00:00:00.0150000`. |
| AUX-092 | `Operation_InvalidSession_ReturnsTypedAccessFaultBeforePersistence("UpdateProfile","empty")` | Passed; `profile-wcf.trx`; `5c0b4134-81ab-15b7-a68c-b47c23ce55c0`; `00:00:00.0100000`. |
| AUX-093 | `SkipOrCancel_IdleUpload_InvokesCallbackAndPreservesCreatedAccount(True)` | Passed; `profile-client-additional.trx`; `8948b09f-73eb-f3fa-b94a-0bb7e949f829`; `00:00:00.0000001`. |
| AUX-094 | `GetProfilePicture_AbsentContent_ReturnsFrozenDefaultImage(False)` | Passed; `profile-client-additional.trx`; `acb8d60c-7c13-89d2-ce44-976b3a800d63`; `00:00:00.0910000`. |
| AUX-095 | `Profile_WithoutServerSession_ShowsNotFoundAndDisablesProfileActions(True)` | Passed; `profile-client-additional.trx`; `c349a129-17e9-db49-fad8-4df3be11c8ec`; `00:00:00.0000001`. |
| AUX-096 | `GetProfilePicture_CorruptedContent_ReturnsFrozenDefaultImage` | Passed; `profile-client-additional.trx`; `c9d9c523-6c83-56bd-d576-a1a308ecaeda`; `00:00:00.0160000`. |
| AUX-097 | `Profile_WithoutServerSession_ShowsNotFoundAndDisablesProfileActions(False)` | Passed; `profile-client-additional.trx`; `cbb3490a-e510-3217-f590-b83fb16a1a22`; `00:00:00.0010000`. |
| AUX-098 | `EditProfile_WithoutServerSession_ShowsNotFoundAndDisablesChanges(False)` | Passed; `profile-client-additional.trx`; `02d56817-f88c-f1fd-9b8c-b83bc2ffb2f2`; `00:00:00.0020000`. |
| AUX-099 | `SkipOrCancel_IdleUpload_InvokesCallbackAndPreservesCreatedAccount(False)` | Passed; `profile-client-additional.trx`; `929ef781-8d60-72e1-c007-c7f24d9f3f88`; `00:00:00.0000001`. |
| AUX-100 | `GetProfilePicture_AbsentContent_ReturnsFrozenDefaultImage(True)` | Passed; `profile-client-additional.trx`; `8c96e016-9dee-c324-7e0a-94b39c4a1205`; `00:00:00.0000001`. |
| AUX-101 | `EditProfile_WithoutServerSession_ShowsNotFoundAndDisablesChanges(True)` | Passed; `profile-client-additional.trx`; `79c9b559-1257-bc4f-46bf-d51c39f1b1b5`; `00:00:00.0000001`. |
| AUX-102 | `AddPhoto_WithoutSelection_ShowsInvalidImageAndPreservesSession` | Passed; `profile-client-additional.trx`; `9ab4ed87-edc8-2a4e-9c01-6aef67b98ce5`; `00:00:00.2540000`. |

Fuentes:
[ProfileServiceWcfTests.cs](../DominoAllFives.Server/AdditionalTest/ProfileServiceWcfTests.cs)
y [ProfilePresentationTests.cs](../DominoAllFives.Client/AdditionalTest/ProfilePresentationTests.cs).

Reportes finales: profile-wcf.trx (47 Passed), profile-client-additional.trx
(10 Passed) y profile-client-regression.trx (33 Passed). La regresión de fábrica
se adaptó al regreso de perfil que refresca MainMenuViewModel: siete regresos
simples y un refresco, en lugar de ocho regresos simples. La primera ejecución
detectó esa expectativa anterior; la final pasó íntegra.

Compilaciones Debug verificadas con MSBuild de Visual Studio: solución servidor,
solución cliente y proyecto auxiliar cliente independiente. Logs:
profile-server-solution-build.log, profile-client-build.log y
profile-client-additional-build.log. La primera compilación cliente detectó la
colisión entre el namespace local ProfilePicture y el DTO del proxy; se resolvió
con un alias explícito al DTO. Permanecen MSB3836 y CS0168 preexistentes. El restore
auxiliar emitió NU1900 al no poder consultar el índice de vulnerabilidades de
NuGet; los paquetes requeridos sí se restauraron y el proyecto compiló.

Se renderizó EditProfile a 1600×900 desde los recursos WPF compilados, sin abrir
una ventana ni ejecutar llamadas remotas. La inspección visual de
profile-edit-layout.png confirmó que el nuevo botón de eliminación cabe entre
la imagen y el nombre y que los botones inferiores permanecen visibles.
Se usaron datos de presentación de prueba; esto no sustituye una prueba funcional
manual de selección, guardado o eliminación.

La referencia se generó mediante SvcUtil 4.8 consultando metadatos MEX de un host
real de DominoService temporal y aislado. Los JSON conservan la URL
http://localhost:8734/Domino/ para Visual Studio. No se modificaron los endpoints
previos ni los bindings; únicamente se añadió el endpoint del nuevo contrato.

Límites: no se operó sobre SQL Server ni sobre imágenes reales almacenadas.
Consulta exitosa, username duplicado, guardado conjunto, eliminación persistida,
compensación de archivos y su interacción con una BD requieren infraestructura
aislada antes de verificarse integralmente. No se probó net.tcp desplegado ni
la selección manual de archivos. El reemplazo concurrente conserva el riesgo
previo de archivos huérfanos; no se afirma una transacción distribuida disco/BD,
idempotencia, reversión tras una respuesta perdida ni cancelación remota.


## Corrección de registro y perfil al patrón de excepciones (2026-11-19)

Los controllers ahora lanzan RegistrationRuleException o ProfileRuleException
con el Issue correspondiente. DominoService las captura, registra con Debug y
lanza directamente FaultException<TIssue>(ex.Issue). Se eliminaron los estados
retornados, los switches Status→Issue y los helpers de conversión; la consulta
de perfil ya no devuelve null para indicar un rechazo. Los argumentos nulos
siguen siendo ArgumentNullException dentro del servidor y la frontera los
traduce con capturas filtradas por ParamName para conservar el fault remoto.

Se preservaron el orden de validación, el contexto EF por operación, el guardado
conjunto y los finally de compensación. Se retiraron loggers y métodos sin uso;
Storage conserva el único diagnóstico de los fallos de limpieza que recupera.
Los catches técnicos se ajustaron a cada ruta en lugar de copiar la misma lista
para todas. La revisión se contrastó con AuthenticationController,
RankingController, sus operaciones WCF, Manejo de excepciones, Manejo de
comunicación remota y sección 4.7 del estándar.

Compiló la solución servidor Debug con MSBuild de Visual Studio. Evidencia:
rules-server-solution-build.log; permanecen advertencias MSB3836 preexistentes.
Pasaron 137 verificaciones seleccionadas: 54 casos unitarios nuevos de reglas,
36 regresiones unitarias existentes y 47 verificaciones auxiliares remotas.
Las fichas unitarias PU-360 a PU-413 se documentan exclusivamente en Pruebas.md.

La reejecución auxiliar no añade casos: rules-wcf.trx conserva los 34 de perfil,
nueve de registro y cuatro de guardas de Login/Ranking previamente catalogados.
Los 47 pasaron. Se usaron hosts net.pipe, proxies generados y componentes reales;
las solicitudes se detienen antes de persistencia. El reporte contiene el nombre,
los parámetros, el identificador y la duración individual de cada caso.
Las regresiones unitarias están en rules-server-regression.trx (36 Passed) y
las pruebas nuevas en rules-unit.trx (54 Passed).

Se verificó mediante SHA-256 que permanecieron idénticos al inicio de esta
corrección ambos App.config, los dos JSON de referencia, Reference.cs, WcfInvoker
y las interfaces IRegistrationService e IPlayerProfileService. No fue necesario
regenerar proxy ni cambiar contratos o configuración WCF.

La indicación del usuario quedó registrada fuera del repositorio en
agent-instructions/implementation.md: toda migración debe contrastarse con las
guías y el estándar y, ante dudas, con Login o Ranking. Trasladar código local no
se considera suficiente para completar una migración.

Límites: no se utilizó SQL Server ni almacenamiento persistido de imágenes.
No se afirma haber comprobado éxito persistido, duplicados en BD, cuenta borrada,
compensación tras fallo de SaveChanges ni el efecto de escrituras concurrentes.
La corrección conserva esos flujos; las comprobaciones ejecutadas se detienen
en reglas o guardas anteriores a persistencia y comprueban serialización/faults.

El alcance compilado es Server y sus pruebas; no se compiló ni adaptó OldClient.
Sus adaptadores locales todavía consumen las firmas históricas de los controllers;
no se añadieron rutas paralelas ni conversiones de estados para sostener ese flujo.
Los tipos de estado legados se conservan por sus consumidores históricos, pero
ya no intervienen en los controllers ni en la frontera WCF corregidos.


## Inicialización de los ViewModels de perfil (2026-11-19)

ProfileViewModel, EditProfileViewModel y UploadProfilePictureViewModel siguen la
separación de Login: constructor para sesión/acciones, estado y comandos;
inyección de servicios posterior mediante Initialize. Los constructores no
cargan datos ni muestran diálogos, incluso con una sesión activa.
ViewModelFactory construye, configura e inicializa antes de devolver cada objeto.

Los comandos permanecen deshabilitados hasta completar Initialize; las entradas
protegen el uso previo y las segundas inicializaciones se rechazan antes de
recargar. Para cumplir el máximo de cuatro argumentos por método, Profile
configura el logger previamente como Lobby; Edit configura previamente los
servicios locales de imagen y archivo. ConfigurePictures valida ambas
dependencias antes de asignarlas para no dejar una configuración parcial.
Initialize conserva el cliente remoto como última dependencia asignada.

Compilaron la solución Client y el proyecto auxiliar cliente. Logs:
profile-initialize-client-build.log y profile-initialize-additional-build.log.
Permanecen las advertencias CS0168 anteriores y NU1900 del proyecto auxiliar.
No se modificaron configuración WCF, referencias de servicio, WcfInvoker ni
DominoService; se comprobó su conservación mediante SHA-256.

Pasaron 54 verificaciones seleccionadas: 21 del fixture auxiliar de perfil
(profile-initialize-additional.trx) y 33 regresiones históricas del cliente
(profile-initialize-regression.trx). Los diez casos auxiliares anteriores se
adaptaron a la nueva inicialización y volvieron a pasar. Las once incorporaciones
se identifican individualmente abajo; no se agregan al catálogo unitario.

| N.º | Caso nuevo ejecutado | Resultado y evidencia |
| --- | --- | --- |
| AUX-103 | `Initialize_UploadTwice_RejectsSecondInitialization` | Passed; `profile-initialize-additional.trx`; `e0d50535-52b4-e3e8-398a-83f0091cc0d3`; `00:00:00.0010000`. |
| AUX-104 | `ConfigurePictures_MissingDependency_ThrowsArgumentNullException(False)` | Passed; `profile-initialize-additional.trx`; `a820572f-e7a0-88f4-acf7-4c694c590c11`; `00:00:00.0010000`. |
| AUX-105 | `Initialize_EditTwice_RejectsSecondInitialization` | Passed; `profile-initialize-additional.trx`; `dbae07c1-6311-c748-320f-b815961b500a`; `00:00:00.0010000`. |
| AUX-106 | `Constructor_EditWithActiveSession_DoesNotLoadAndDisablesCommands` | Passed; `profile-initialize-additional.trx`; `e2ac5385-5924-f4b2-8fae-8582274f2196`; `00:00:00.0010000`. |
| AUX-107 | `Initialize_ProfileWithoutLogger_ThrowsBeforeLoading` | Passed; `profile-initialize-additional.trx`; `07566f15-8c0d-6f3b-8590-1684c6ba5ece`; `00:00:00.0010000`. |
| AUX-108 | `ConfigurePictures_MissingDependency_ThrowsArgumentNullException(True)` | Passed; `profile-initialize-additional.trx`; `86c1d8eb-72e7-7bd4-1300-ae3650b09c4d`; `00:00:00.0000001`. |
| AUX-109 | `Constructor_UploadWithActiveSession_DoesNotLoadAndDisablesCommands` | Passed; `profile-initialize-additional.trx`; `52e21dd9-3cd3-1860-cd5d-87904ab09f04`; `00:00:00.0000001`. |
| AUX-110 | `Initialize_ProfileTwice_RejectsSecondInitialization` | Passed; `profile-initialize-additional.trx`; `f81554e4-248f-7854-406e-a1bf6a808bb0`; `00:00:00.0010000`. |
| AUX-111 | `Constructor_ProfileWithActiveSession_DoesNotLoadAndDisablesCommands` | Passed; `profile-initialize-additional.trx`; `b85ceb28-249a-e988-a624-1d4c7e9c902d`; `00:00:00.0000001`. |
| AUX-112 | `Initialize_EditWithoutPictures_ThrowsBeforeLoading` | Passed; `profile-initialize-additional.trx`; `386cb825-f50d-7682-1504-4c283686a05b`; `00:00:00.0010000`. |
| AUX-113 | `ConfigureLogger_NullLogger_ThrowsArgumentNullException` | Passed; `profile-initialize-additional.trx`; `7743c533-c240-8e6d-1584-9ada1fbf02b8`; `00:00:00.0100000`. |

Fuente: [ProfilePresentationTests.cs](../DominoAllFives.Client/AdditionalTest/ProfilePresentationTests.cs).

Los tres casos de constructor usan sesión activa sin inicializar servicios y
esperan ausencia de diálogos y comandos deshabilitados. Las dependencias nulas
producen ArgumentNullException. Inicializar sin configuración previa o repetir
Initialize produce InvalidOperationException. Las inicializaciones que cargan
usan una sesión sin token para detenerse antes de la red.

Las pruebas utilizan componentes concretos y recursos WPF sin ventanas visibles,
sin dobles, reflexión, selección manual, red ni BD. Se conservan los snapshots,
comparaciones de sesión, continuaciones con Dispatcher y navegación existentes.
No se afirma haber ejecutado una consulta o edición persistida ni una interacción
manual completa en esta corrección.


## Renombrado del contrato de perfil (2026-11-19)

Se alinearon los nombres del contrato y de sus consumidores: IProfileService,
ProfileIssue, ProfileRuleException, ProfileClient, GetMyProfile y
UpdateProfileName. UpdateProfileWithPicture conserva su nombre. La clase privada
que captura la sesión y la imagen durante la subida se denomina PictureUploaded
y continúa siendo sealed.

El proxy se regeneró desde los metadatos del servidor mediante un host temporal
net.pipe. ConnectedService.json y dotnet-svcutil.params.json conservaron la URL
http://localhost:8734/Domino/ y WcfInvoker permaneció idéntico. En los dos
App.config solo se actualizó la referencia contract= al nuevo nombre; no cambiaron
endpoint, dirección, binding ni puerto.

Compilaron Server, Client y el proyecto auxiliar del cliente. Pasaron 121
verificaciones seleccionadas: 33 reglas de perfil, 34 operaciones/serializaciones
WCF, 21 presentaciones de perfil y 33 regresiones de Login, registro, Ranking,
credenciales y BackgroundJob. Los reportes finales son
profile-renames-unit.trx, profile-renames-wcf-final.trx,
profile-renames-presentation.trx y profile-renames-client-regression.trx.

Permanecen advertencias previas de redirecciones de ensamblados en Server,
CS0168 en Client y NU1900 al consultar el índice de vulnerabilidades del proyecto
auxiliar. No se utilizó SQL Server ni almacenamiento persistido de imágenes.
