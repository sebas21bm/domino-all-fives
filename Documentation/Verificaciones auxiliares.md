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
