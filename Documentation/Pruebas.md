# Pruebas

## Alcance y evidencia

Catálogo unitario actualizado el 2026-11-17 tras separar preparación y aplicación
de jugadas y cierres de ronda. Conserva PU-001 a PU-338, actualizando los casos
afectados por la nueva preparación, y añade PU-339 a PU-359. Cada ficha describe
un único caso ejecutable con sus parámetros concretos, numeración consecutiva
y sin duplicados.

Pasaron 351 casos unitarios del servidor y ocho de MatchProxyTests del cliente:
`DominoAllFives.Server/Tests/TestResults/prepared-transition-unit.trx` y
`DominoAllFives.Server/Tests/TestResults/prepared-transition-client-unit.trx`.
Son 359 casos Passed, cero fallidos y cero omitidos. El alcance del cliente sigue
limitado a su fixture de proxy; no se afirma catalogar otras funciones del cliente.

Los 21 casos nuevos comprueban copias independientes del tablero, reparto
preparado sin mutar entradas, independencia entre preparaciones, identidad de
jugadores y aplicación de jugadas y cierres con una sola versión. Los casos
existentes de mazos inválidos se adaptaron para comprobar las entradas reales
de RoundSetup y la conservación del resultado preparado anterior.

Se ejercitan objetos y agregados de dominio, validadores, generación, puntuación,
resolución, sesiones, cola y proyecciones en memoria. La serialización usa
MemoryStream y la inspección de contratos utiliza reflexión sin abrir canales.
Las guardas de controllers y repositorios se detienen antes de persistencia.
Las entradas, credenciales, claves y hashes son datos controlados de pruebas.
Las pruebas aleatorias existentes se identifican en sus fichas.

No se añadieron interfaces, generadores sustituibles, banderas de fallo, métodos
virtuales ni entradas de producción exclusivas para pruebas. No se presenta
como reproducida una excepción técnica de DeckGenerator durante un comando;
la cobertura ejecutada de errores de preparación usa mazos y datos inválidos.
Cada ficha incluye clase, caso completo, datos, origen y línea actuales,
precondiciones, postcondiciones, resultado esperado, observado y evidencia.

## Casos unitarios

### PU-001. Evaluar suspensión vencida o en el límite

- **Clase:** `DominoAllFives.Server.Tests.AccountAccessStatusTests`.
- **Caso ejecutable:** `EnsureCanLogin_ExpiredOrBoundarySuspension_ReturnsNone(-1)`.
- **Origen:** [AccountAccessStatusTests.cs](../DominoAllFives.Server/Tests/AccountAccessStatusTests.cs#L47).
- **Datos:** `minuteOffset=-1`
- **Precondiciones:** Fecha UTC fija 2026-11-16 12:00; suspensión desplazada -1 minutos.
- **Postcondiciones:** Estado evaluado en memoria.
- **Resultado esperado:** LoginStatus.None.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `6d8eb0b0-8584-1a80-3000-a8b48340618f`; duración `00:00:00.0990000`.

### PU-002. Evaluar suspensión vencida o en el límite

- **Clase:** `DominoAllFives.Server.Tests.AccountAccessStatusTests`.
- **Caso ejecutable:** `EnsureCanLogin_ExpiredOrBoundarySuspension_ReturnsNone(0)`.
- **Origen:** [AccountAccessStatusTests.cs](../DominoAllFives.Server/Tests/AccountAccessStatusTests.cs#L47).
- **Datos:** `minuteOffset=0`
- **Precondiciones:** Fecha UTC fija 2026-11-16 12:00; suspensión desplazada 0 minutos.
- **Postcondiciones:** Estado evaluado en memoria.
- **Resultado esperado:** LoginStatus.None.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `07b255f7-f071-721b-e73a-227da6fde409`; duración `00:00:00.0000001`.

### PU-003. Rechazar suspensión vigente

- **Clase:** `DominoAllFives.Server.Tests.AccountAccessStatusTests`.
- **Caso ejecutable:** `EnsureCanLogin_FutureSuspension_ReturnsSuspended`.
- **Origen:** [AccountAccessStatusTests.cs](../DominoAllFives.Server/Tests/AccountAccessStatusTests.cs#L31).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Cuenta activa; suspensión un minuto después de la fecha UTC fija.
- **Postcondiciones:** Acceso rechazado.
- **Resultado esperado:** LoginStatus.Suspended.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `84979553-e5a3-1a45-3685-440b178d0c07`; duración `00:00:00.0010000`.

### PU-004. Evaluar banderas persistidas de acceso

- **Clase:** `DominoAllFives.Server.Tests.AccountAccessStatusTests`.
- **Caso ejecutable:** `EnsureCanLogin_StoredFlags_ReturnsExpectedStatus(False,False,None)`.
- **Origen:** [AccountAccessStatusTests.cs](../DominoAllFives.Server/Tests/AccountAccessStatusTests.cs#L14).
- **Datos:** `isDeleted=False; isBanned=False; expectedStatus=None`
- **Precondiciones:** isDeleted=False; isBanned=False; sin suspensión.
- **Postcondiciones:** Estado de acceso calculado.
- **Resultado esperado:** LoginStatus.None.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `0592f7fc-a12e-17ea-e78c-87a2086191f5`; duración `00:00:00.0000001`.

### PU-005. Evaluar banderas persistidas de acceso

- **Clase:** `DominoAllFives.Server.Tests.AccountAccessStatusTests`.
- **Caso ejecutable:** `EnsureCanLogin_StoredFlags_ReturnsExpectedStatus(False,True,Banned)`.
- **Origen:** [AccountAccessStatusTests.cs](../DominoAllFives.Server/Tests/AccountAccessStatusTests.cs#L14).
- **Datos:** `isDeleted=False; isBanned=True; expectedStatus=Banned`
- **Precondiciones:** isDeleted=False; isBanned=True; sin suspensión.
- **Postcondiciones:** Estado de acceso calculado.
- **Resultado esperado:** LoginStatus.Banned.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `9764ca15-9ae1-a62d-946d-730d3f97c28f`; duración `00:00:00.0000001`.

### PU-006. Evaluar banderas persistidas de acceso

- **Clase:** `DominoAllFives.Server.Tests.AccountAccessStatusTests`.
- **Caso ejecutable:** `EnsureCanLogin_StoredFlags_ReturnsExpectedStatus(True,True,InvalidCredentials)`.
- **Origen:** [AccountAccessStatusTests.cs](../DominoAllFives.Server/Tests/AccountAccessStatusTests.cs#L14).
- **Datos:** `isDeleted=True; isBanned=True; expectedStatus=InvalidCredentials`
- **Precondiciones:** isDeleted=True; isBanned=True; sin suspensión.
- **Postcondiciones:** Estado de acceso calculado.
- **Resultado esperado:** LoginStatus.InvalidCredentials.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `a3ffc70e-ed4c-c360-93e2-69953062af4c`; duración `00:00:00.0000001`.

### PU-007. Conservar código de rechazo de negocio

- **Clase:** `DominoAllFives.Server.Tests.AccountRuleExceptionTests`.
- **Caso ejecutable:** `Constructor_DefinedIssue_PreservesIssue(Banned)`.
- **Origen:** [AccountRuleExceptionTests.cs](../DominoAllFives.Server/Tests/AccountRuleExceptionTests.cs#L14).
- **Datos:** `issue=Banned`
- **Precondiciones:** Valor definido del enum de la excepción: Banned.
- **Postcondiciones:** Excepción construida con Issue inmutable.
- **Resultado esperado:** Issue igual a Banned.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `d22481b6-53db-61b1-0f04-5c4b876bb677`; duración `00:00:00.0000001`.

### PU-008. Conservar código de rechazo de negocio

- **Clase:** `DominoAllFives.Server.Tests.AccountRuleExceptionTests`.
- **Caso ejecutable:** `Constructor_DefinedIssue_PreservesIssue(InvalidCredentials)`.
- **Origen:** [AccountRuleExceptionTests.cs](../DominoAllFives.Server/Tests/AccountRuleExceptionTests.cs#L14).
- **Datos:** `issue=InvalidCredentials`
- **Precondiciones:** Valor definido del enum de la excepción: InvalidCredentials.
- **Postcondiciones:** Excepción construida con Issue inmutable.
- **Resultado esperado:** Issue igual a InvalidCredentials.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `eb804825-53e8-b574-a627-29a0a047ca07`; duración `00:00:00.0020000`.

### PU-009. Conservar código de rechazo de negocio

- **Clase:** `DominoAllFives.Server.Tests.AccountRuleExceptionTests`.
- **Caso ejecutable:** `Constructor_DefinedIssue_PreservesIssue(Suspended)`.
- **Origen:** [AccountRuleExceptionTests.cs](../DominoAllFives.Server/Tests/AccountRuleExceptionTests.cs#L14).
- **Datos:** `issue=Suspended`
- **Precondiciones:** Valor definido del enum de la excepción: Suspended.
- **Postcondiciones:** Excepción construida con Issue inmutable.
- **Resultado esperado:** Issue igual a Suspended.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `fed1156e-ade6-373b-4cda-04d37bde1792`; duración `00:00:00.0000001`.

### PU-010. Rechazar código de negocio indefinido

- **Clase:** `DominoAllFives.Server.Tests.AccountRuleExceptionTests`.
- **Caso ejecutable:** `Constructor_UndefinedIssue_ThrowsArgumentOutOfRangeException`.
- **Origen:** [AccountRuleExceptionTests.cs](../DominoAllFives.Server/Tests/AccountRuleExceptionTests.cs#L25).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Enum convertido desde int.MaxValue.
- **Postcondiciones:** No se construye una excepción de negocio válida.
- **Resultado esperado:** ArgumentOutOfRangeException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `0f6ce035-0e7e-2b93-1bb9-55a41d92e343`; duración `00:00:00.0120000`.

### PU-011. Rechazar credenciales sin contraseña

- **Clase:** `DominoAllFives.Server.Tests.AuthenticationControllerTests`.
- **Caso ejecutable:** `LoginWithAccount_MissingPassword_ThrowsInvalidCredentials`.
- **Origen:** [AuthenticationControllerTests.cs](../DominoAllFives.Server/Tests/AuthenticationControllerTests.cs#L27).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** AuthenticationController; identificador de prueba válido y contraseña null.
- **Postcondiciones:** No se inicia autenticación persistida.
- **Resultado esperado:** AccountRuleException con Issue=InvalidCredentials.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `871030ee-3627-6860-bf02-7dc85bcf0cdc`; duración `00:00:00.0580000`.

### PU-012. Rechazar credenciales nulas

- **Clase:** `DominoAllFives.Server.Tests.AuthenticationControllerTests`.
- **Caso ejecutable:** `LoginWithAccount_NullCredentials_ThrowsInvalidCredentials`.
- **Origen:** [AuthenticationControllerTests.cs](../DominoAllFives.Server/Tests/AuthenticationControllerTests.cs#L13).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** AuthenticationController; argumento Credentials null.
- **Postcondiciones:** No se inicia autenticación persistida.
- **Resultado esperado:** AccountRuleException con Issue=InvalidCredentials.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `44d2ed0b-49bb-a1da-8b25-43f2bd0af27c`; duración `00:00:00.0030000`.

### PU-013. Normalizar identificador de login

- **Clase:** `DominoAllFives.Server.Tests.CredentialsTests`.
- **Caso ejecutable:** `ForLogin_PaddedIdentifier_TrimsIdentifier`.
- **Origen:** [CredentialsTests.cs](../DominoAllFives.Server/Tests/CredentialsTests.cs#L11).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Identificador de prueba con dos espacios a cada lado.
- **Postcondiciones:** Credencial creada.
- **Resultado esperado:** EmailOrUsername igual a player.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `dc39804a-f4f4-43c4-4407-a65d08c7eec2`; duración `00:00:00.0000001`.

### PU-014. Conservar espacios de contraseña

- **Clase:** `DominoAllFives.Server.Tests.CredentialsTests`.
- **Caso ejecutable:** `ForLogin_PaddedPassword_PreservesPassword`.
- **Origen:** [CredentialsTests.cs](../DominoAllFives.Server/Tests/CredentialsTests.cs#L22).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Contraseña ficticia con dos espacios a cada lado.
- **Postcondiciones:** Credencial creada sin recortar contraseña.
- **Resultado esperado:** Password idéntico al argumento de entrada.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `a0a523a5-25b8-5734-da93-0c4b3cd24580`; duración `00:00:00.0000001`.

### PU-015. Validar credencial completa

- **Clase:** `DominoAllFives.Server.Tests.CredentialsTests`.
- **Caso ejecutable:** `Validate_CompleteCredentials_ReturnsNone`.
- **Origen:** [CredentialsTests.cs](../DominoAllFives.Server/Tests/CredentialsTests.cs#L33).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Identificador y contraseña ficticios no vacíos.
- **Postcondiciones:** Credencial evaluada.
- **Resultado esperado:** LoginStatus.None.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `4ec0ee55-94b6-6bf8-0a84-ddbc815db55d`; duración `00:00:00.0000001`.

### PU-016. Rechazar credencial incompleta

- **Clase:** `DominoAllFives.Server.Tests.CredentialsTests`.
- **Caso ejecutable:** `Validate_IncompleteCredentials_ReturnsInvalidCredentials("   ","secret")`.
- **Origen:** [CredentialsTests.cs](../DominoAllFives.Server/Tests/CredentialsTests.cs#L49).
- **Datos:** `identifier="   "; password="secret"`
- **Precondiciones:** identifier="   "; password="secret"; valores exclusivamente de prueba.
- **Postcondiciones:** Credencial evaluada sin autenticar.
- **Resultado esperado:** LoginStatus.InvalidCredentials.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `bea75ace-0d75-5065-d66d-994f8053ee45`; duración `00:00:00.0000001`.

### PU-017. Rechazar credencial incompleta

- **Clase:** `DominoAllFives.Server.Tests.CredentialsTests`.
- **Caso ejecutable:** `Validate_IncompleteCredentials_ReturnsInvalidCredentials("","secret")`.
- **Origen:** [CredentialsTests.cs](../DominoAllFives.Server/Tests/CredentialsTests.cs#L49).
- **Datos:** `identifier=""; password="secret"`
- **Precondiciones:** identifier=""; password="secret"; valores exclusivamente de prueba.
- **Postcondiciones:** Credencial evaluada sin autenticar.
- **Resultado esperado:** LoginStatus.InvalidCredentials.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `7e94825a-3e4d-0979-5fdf-dfaa39e5f142`; duración `00:00:00.0000001`.

### PU-018. Rechazar credencial incompleta

- **Clase:** `DominoAllFives.Server.Tests.CredentialsTests`.
- **Caso ejecutable:** `Validate_IncompleteCredentials_ReturnsInvalidCredentials("player","   ")`.
- **Origen:** [CredentialsTests.cs](../DominoAllFives.Server/Tests/CredentialsTests.cs#L49).
- **Datos:** `identifier="player"; password="   "`
- **Precondiciones:** identifier="player"; password="   "; valores exclusivamente de prueba.
- **Postcondiciones:** Credencial evaluada sin autenticar.
- **Resultado esperado:** LoginStatus.InvalidCredentials.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `01ab9428-c214-bacc-e3f4-189efa69bc42`; duración `00:00:00.0000001`.

### PU-019. Rechazar credencial incompleta

- **Clase:** `DominoAllFives.Server.Tests.CredentialsTests`.
- **Caso ejecutable:** `Validate_IncompleteCredentials_ReturnsInvalidCredentials("player","")`.
- **Origen:** [CredentialsTests.cs](../DominoAllFives.Server/Tests/CredentialsTests.cs#L49).
- **Datos:** `identifier="player"; password=""`
- **Precondiciones:** identifier="player"; password=""; valores exclusivamente de prueba.
- **Postcondiciones:** Credencial evaluada sin autenticar.
- **Resultado esperado:** LoginStatus.InvalidCredentials.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `4bca3e4a-d345-ce0c-0b2a-50f58b839509`; duración `00:00:00.0000001`.

### PU-020. Rechazar credencial incompleta

- **Clase:** `DominoAllFives.Server.Tests.CredentialsTests`.
- **Caso ejecutable:** `Validate_IncompleteCredentials_ReturnsInvalidCredentials("player",null)`.
- **Origen:** [CredentialsTests.cs](../DominoAllFives.Server/Tests/CredentialsTests.cs#L49).
- **Datos:** `identifier="player"; password=null`
- **Precondiciones:** identifier="player"; password=null; valores exclusivamente de prueba.
- **Postcondiciones:** Credencial evaluada sin autenticar.
- **Resultado esperado:** LoginStatus.InvalidCredentials.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `a2693381-398b-57bb-fc50-604b31c20f28`; duración `00:00:00.0000001`.

### PU-021. Rechazar credencial incompleta

- **Clase:** `DominoAllFives.Server.Tests.CredentialsTests`.
- **Caso ejecutable:** `Validate_IncompleteCredentials_ReturnsInvalidCredentials(null,"secret")`.
- **Origen:** [CredentialsTests.cs](../DominoAllFives.Server/Tests/CredentialsTests.cs#L49).
- **Datos:** `identifier=null; password="secret"`
- **Precondiciones:** identifier=null; password="secret"; valores exclusivamente de prueba.
- **Postcondiciones:** Credencial evaluada sin autenticar.
- **Resultado esperado:** LoginStatus.InvalidCredentials.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `a70349e0-797a-4285-7c09-5e5ee2679c38`; duración `00:00:00.0000001`.

### PU-022. Rechazar primer valor negativo de ficha

- **Clase:** `DominoAllFives.Server.Tests.Gameplay.TileTests`.
- **Caso ejecutable:** `Constructor_NegativeFirstValue_ThrowsArgumentOutOfRangeException`.
- **Origen:** [TileTests.cs](../DominoAllFives.Server/Tests/TileTests.cs#L197).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** ID 1; valores -1 y 5.
- **Postcondiciones:** Ficha no construida.
- **Resultado esperado:** ArgumentOutOfRangeException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `d7eee735-75f2-d9ed-fa7f-785f97c8c8e7`; duración `00:00:00.0010000`.

### PU-023. Rechazar segundo valor mayor a seis

- **Clase:** `DominoAllFives.Server.Tests.Gameplay.TileTests`.
- **Caso ejecutable:** `Constructor_SecondValueAboveSix_ThrowsArgumentOutOfRangeException`.
- **Origen:** [TileTests.cs](../DominoAllFives.Server/Tests/TileTests.cs#L218).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** ID 1; valores 2 y 7.
- **Postcondiciones:** Ficha no construida.
- **Resultado esperado:** ArgumentOutOfRangeException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `87f3ae20-245f-5f92-97db-ceb6d6fa07c6`; duración `00:00:00.0010000`.

### PU-024. Rechazar poder indefinido

- **Clase:** `DominoAllFives.Server.Tests.Gameplay.TileTests`.
- **Caso ejecutable:** `Constructor_UndefinedSpecialPower_ThrowsArgumentOutOfRangeException`.
- **Origen:** [TileTests.cs](../DominoAllFives.Server/Tests/TileTests.cs#L239).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** ID 1; valores 2 y 5; poder convertido desde 99.
- **Postcondiciones:** Ficha no construida.
- **Resultado esperado:** ArgumentOutOfRangeException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `e6cfa0d2-3907-59f0-87f7-8c046e2facd4`; duración `00:00:00.0000001`.

### PU-025. Rechazar ID cero de ficha

- **Clase:** `DominoAllFives.Server.Tests.Gameplay.TileTests`.
- **Caso ejecutable:** `Constructor_ZeroIdentifier_ThrowsArgumentOutOfRangeException`.
- **Origen:** [TileTests.cs](../DominoAllFives.Server/Tests/TileTests.cs#L176).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** ID 0; valores 2 y 5.
- **Postcondiciones:** Ficha no construida.
- **Resultado esperado:** ArgumentOutOfRangeException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `f6c23e84-8d58-fc49-22a5-6432c7ec108c`; duración `00:00:00.0000001`.

### PU-026. Consultar valor opuesto al primero

- **Clase:** `DominoAllFives.Server.Tests.Gameplay.TileTests`.
- **Caso ejecutable:** `GetOtherValue_FirstValue_ReturnsSecondValue`.
- **Origen:** [TileTests.cs](../DominoAllFives.Server/Tests/TileTests.cs#L129).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Ficha 2-5; consulta 2.
- **Postcondiciones:** Ficha conservada.
- **Resultado esperado:** 5.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `a2273596-45db-b533-9648-a691f4ef21aa`; duración `00:00:00.0000001`.

### PU-027. Consultar valor opuesto al segundo

- **Clase:** `DominoAllFives.Server.Tests.Gameplay.TileTests`.
- **Caso ejecutable:** `GetOtherValue_SecondValue_ReturnsFirstValue`.
- **Origen:** [TileTests.cs](../DominoAllFives.Server/Tests/TileTests.cs#L142).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Ficha 2-5; consulta 5.
- **Postcondiciones:** Ficha conservada.
- **Resultado esperado:** 2.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `748b64eb-48c3-b6e7-8202-396d3886cb79`; duración `00:00:00.0020000`.

### PU-028. Rechazar valor ausente en ficha

- **Clase:** `DominoAllFives.Server.Tests.Gameplay.TileTests`.
- **Caso ejecutable:** `GetOtherValue_ValueNotOnTile_ThrowsMatchRuleException`.
- **Origen:** [TileTests.cs](../DominoAllFives.Server/Tests/TileTests.cs#L155).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Ficha 2-5; consulta 4.
- **Postcondiciones:** Ficha conservada.
- **Resultado esperado:** MatchRuleException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `d1fc1a87-09d1-01af-5626-1382f4c55871`; duración `00:00:00.0010000`.

### PU-029. Reconocer doble blanco

- **Clase:** `DominoAllFives.Server.Tests.Gameplay.TileTests`.
- **Caso ejecutable:** `IsDouble_BlankDouble_ReturnsTrue`.
- **Origen:** [TileTests.cs](../DominoAllFives.Server/Tests/TileTests.cs#L38).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Ficha 0-0.
- **Postcondiciones:** Ficha conservada.
- **Resultado esperado:** true.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `0deddfbd-e9d8-92bf-e5bd-d39146312dea`; duración `00:00:00.0030000`.

### PU-030. Descartar doble con valores distintos

- **Clase:** `DominoAllFives.Server.Tests.Gameplay.TileTests`.
- **Caso ejecutable:** `IsDouble_DifferentValues_ReturnsFalse`.
- **Origen:** [TileTests.cs](../DominoAllFives.Server/Tests/TileTests.cs#L25).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Ficha 2-5.
- **Postcondiciones:** Ficha conservada.
- **Resultado esperado:** false.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `ddc17133-a0fe-5ad8-62bc-dcceb2e74db2`; duración `00:00:00.0010000`.

### PU-031. Reconocer doble

- **Clase:** `DominoAllFives.Server.Tests.Gameplay.TileTests`.
- **Caso ejecutable:** `IsDouble_EqualValues_ReturnsTrue`.
- **Origen:** [TileTests.cs](../DominoAllFives.Server/Tests/TileTests.cs#L12).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Ficha 3-3.
- **Postcondiciones:** Ficha conservada.
- **Resultado esperado:** true.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `4842eded-dc37-5d9d-d8c3-dbb4aa1e8fe7`; duración `00:00:00.0000001`.

### PU-032. Reconocer coincidencia con primer valor

- **Clase:** `DominoAllFives.Server.Tests.Gameplay.TileTests`.
- **Caso ejecutable:** `Matches_FirstValue_ReturnsTrue`.
- **Origen:** [TileTests.cs](../DominoAllFives.Server/Tests/TileTests.cs#L90).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Ficha 2-5; consulta 2.
- **Postcondiciones:** Ficha conservada.
- **Resultado esperado:** true.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `2d299053-f0cd-6307-111a-3abe310fe05c`; duración `00:00:00.0000001`.

### PU-033. Reconocer coincidencia con segundo valor

- **Clase:** `DominoAllFives.Server.Tests.Gameplay.TileTests`.
- **Caso ejecutable:** `Matches_SecondValue_ReturnsTrue`.
- **Origen:** [TileTests.cs](../DominoAllFives.Server/Tests/TileTests.cs#L103).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Ficha 2-5; consulta 5.
- **Postcondiciones:** Ficha conservada.
- **Resultado esperado:** true.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `82e56026-1a0c-e389-8ddd-2651efb954c0`; duración `00:00:00.0000001`.

### PU-034. Descartar coincidencia ausente

- **Clase:** `DominoAllFives.Server.Tests.Gameplay.TileTests`.
- **Caso ejecutable:** `Matches_ValueNotOnTile_ReturnsFalse`.
- **Origen:** [TileTests.cs](../DominoAllFives.Server/Tests/TileTests.cs#L116).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Ficha 2-5; consulta 4.
- **Postcondiciones:** Ficha conservada.
- **Resultado esperado:** false.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `0276a3df-a0cf-b67a-9410-b0a8216eed9f`; duración `00:00:00.0000001`.

### PU-035. Sumar doble blanco

- **Clase:** `DominoAllFives.Server.Tests.Gameplay.TileTests`.
- **Caso ejecutable:** `TotalPips_BlankDouble_ReturnsZero`.
- **Origen:** [TileTests.cs](../DominoAllFives.Server/Tests/TileTests.cs#L64).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Ficha 0-0.
- **Postcondiciones:** Ficha conservada.
- **Resultado esperado:** 0.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `e55b36f0-63a8-2228-286c-431849867f3f`; duración `00:00:00.0010000`.

### PU-036. Sumar valores de ficha

- **Clase:** `DominoAllFives.Server.Tests.Gameplay.TileTests`.
- **Caso ejecutable:** `TotalPips_DifferentValues_ReturnsTheirSum`.
- **Origen:** [TileTests.cs](../DominoAllFives.Server/Tests/TileTests.cs#L51).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Ficha 2-5.
- **Postcondiciones:** Ficha conservada.
- **Resultado esperado:** 7.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `0aae8f5e-687d-fd71-ec0b-85dac7992803`; duración `00:00:00.0000001`.

### PU-037. Sumar doble máximo

- **Clase:** `DominoAllFives.Server.Tests.Gameplay.TileTests`.
- **Caso ejecutable:** `TotalPips_HighestDouble_ReturnsTwelve`.
- **Origen:** [TileTests.cs](../DominoAllFives.Server/Tests/TileTests.cs#L77).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Ficha 6-6.
- **Postcondiciones:** Ficha conservada.
- **Resultado esperado:** 12.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `1d0a18ad-f4d7-f0fb-61d3-661d467ef979`; duración `00:00:00.0010000`.

### PU-038. Excluir extremo morado durante bloqueo

- **Clase:** `DominoAllFives.Server.Tests.GameplayTests`.
- **Caso ejecutable:** `Evaluate_PurpleEnd_IsExcludedDuringNextTurn`.
- **Origen:** [GameplayTests.cs](../DominoAllFives.Server/Tests/GameplayTests.cs#L303).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Tablero 5-3 seguido de 3-5 morada en turno 1.
- **Postcondiciones:** PlayScoring evalúa las bases de puntuación de los turnos 2 y 3 sin modificar el tablero.
- **Resultado esperado:** 5 puntos en turno 2; 10 en turno 3.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `0495a48f-0a34-b2e0-141a-82f501e9a4d8`; duración `00:00:00.0020000`.

### PU-039. Cancelar partida activa

- **Clase:** `DominoAllFives.Server.Tests.GameplayTests`.
- **Caso ejecutable:** `Cancel_ActiveMatch_HasNoWinnerAndPreventsFurtherMoves`.
- **Origen:** [GameplayTests.cs](../DominoAllFives.Server/Tests/GameplayTests.cs#L493).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Match con dos jugadores; luego se intenta Draw.
- **Postcondiciones:** Cancelación conserva ausencia de ganador y rechaza nuevas acciones.
- **Resultado esperado:** Phase=Cancelled; cero ganadores; MatchNotInProgress al robar.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `8d401c8f-1079-560e-3bc4-2a5fe7e8f751`; duración `00:00:00.0290000`.

### PU-040. Evitar segunda transición al cancelar

- **Clase:** `DominoAllFives.Server.Tests.GameplayTests`.
- **Caso ejecutable:** `Cancel_RepeatedRequest_ProducesOnlyOneVersionChange`.
- **Origen:** [GameplayTests.cs](../DominoAllFives.Server/Tests/GameplayTests.cs#L112).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Partida ya cancelada; versión anterior capturada.
- **Postcondiciones:** Sin segundo incremento de versión.
- **Resultado esperado:** Versión conservada; cero ganadores.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `7a7b96e3-cbc9-0ffc-099c-6e244cd66121`; duración `00:00:00.0020000`.

### PU-041. Generar composición del mazo

- **Clase:** `DominoAllFives.Server.Tests.GameplayTests`.
- **Caso ejecutable:** `Create_FullDeck_HasUniqueTilesAndFiveSpecialPowers`.
- **Origen:** [GameplayTests.cs](../DominoAllFives.Server/Tests/GameplayTests.cs#L204).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** DeckGenerator.Generate sin servicios externos.
- **Postcondiciones:** Mazo generado.
- **Resultado esperado:** 28 identificadores distintos; exactamente 5 poderes.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `87621bec-01f9-5d11-8b33-fc4870ad0a7e`; duración `00:00:00.0160000`.

### PU-042. Proteger ficha del dominio frente a modificación del contrato

- **Clase:** `DominoAllFives.Server.Tests.GameplayTests`.
- **Caso ejecutable:** `CreateView_ModifiedNetworkTile_DoesNotChangeDomain`.
- **Origen:** [GameplayTests.cs](../DominoAllFives.Server/Tests/GameplayTests.cs#L478).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Match y contrato de mano creado en memoria.
- **Postcondiciones:** Modificar FirstValue del contrato a 999 no altera la ficha original.
- **Resultado esperado:** FirstValue del dominio conserva el valor previo.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `4fa21550-6b37-86a0-4d51-0aec15ce36bd`; duración `00:00:00.0020000`.

### PU-043. Robar hasta obtener jugada

- **Clase:** `DominoAllFives.Server.Tests.GameplayTests`.
- **Caso ejecutable:** `Draw_NoPlayableTile_DrawsUntilPlayableWithoutAdvancingTurn`.
- **Origen:** [GameplayTests.cs](../DominoAllFives.Server/Tests/GameplayTests.cs#L383).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** 6-6 abierta; mano del jugador 2 sin jugada; pozo controlado.
- **Postcondiciones:** Jugador 2 conserva turno y recibe una ficha.
- **Resultado esperado:** Mano 8; pozo 13; jugador actual 2; versión 2.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `9e208386-c9ef-d168-8aed-f825e710971f`; duración `00:00:00.0090000`.

### PU-044. Rechazar robo con mano jugable

- **Clase:** `DominoAllFives.Server.Tests.GameplayTests`.
- **Caso ejecutable:** `Draw_PlayableHand_RejectsWithoutConsumingStock`.
- **Origen:** [GameplayTests.cs](../DominoAllFives.Server/Tests/GameplayTests.cs#L89).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Partida recién creada con mano jugable.
- **Postcondiciones:** Pozo y versión intactos.
- **Resultado esperado:** DrawNotAllowed; mismo StockCount; versión 0.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `e116b9ad-90a8-fd8f-6d0c-f72a2f756fec`; duración `00:00:00.0010000`.

### PU-045. Completar partida mediante acciones legales

- **Clase:** `DominoAllFives.Server.Tests.GameplayTests`.
- **Caso ejecutable:** `Match_LegalAutomatedPlay_CompletesAndConservesTiles(2)`.
- **Origen:** [GameplayTests.cs](../DominoAllFives.Server/Tests/GameplayTests.cs#L528).
- **Datos:** `playerCount=2`
- **Precondiciones:** 2 jugadores; objetivo 100; máximo 5000 acciones; barajado variable.
- **Postcondiciones:** Partida completa conservando fichas en cada transición.
- **Resultado esperado:** Phase=Completed; máximo de puntos al menos 100; total de fichas siempre 28.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `c8f2754a-f410-fdef-e6b4-c20b34de97af`; duración `00:00:00.0150000`.

### PU-046. Completar partida mediante acciones legales

- **Clase:** `DominoAllFives.Server.Tests.GameplayTests`.
- **Caso ejecutable:** `Match_LegalAutomatedPlay_CompletesAndConservesTiles(3)`.
- **Origen:** [GameplayTests.cs](../DominoAllFives.Server/Tests/GameplayTests.cs#L528).
- **Datos:** `playerCount=3`
- **Precondiciones:** 3 jugadores; objetivo 100; máximo 5000 acciones; barajado variable.
- **Postcondiciones:** Partida completa conservando fichas en cada transición.
- **Resultado esperado:** Phase=Completed; máximo de puntos al menos 100; total de fichas siempre 28.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `5bb9104f-8d84-05a6-c837-665c539abbef`; duración `00:00:00.0020000`.

### PU-047. Completar partida mediante acciones legales

- **Clase:** `DominoAllFives.Server.Tests.GameplayTests`.
- **Caso ejecutable:** `Match_LegalAutomatedPlay_CompletesAndConservesTiles(4)`.
- **Origen:** [GameplayTests.cs](../DominoAllFives.Server/Tests/GameplayTests.cs#L528).
- **Datos:** `playerCount=4`
- **Precondiciones:** 4 jugadores; objetivo 100; máximo 5000 acciones; barajado variable.
- **Postcondiciones:** Partida completa conservando fichas en cada transición.
- **Resultado esperado:** Phase=Completed; máximo de puntos al menos 100; total de fichas siempre 28.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `b9baab94-decf-309e-5691-17d70ca9c911`; duración `00:00:00.0020000`.

### PU-048. Verificar cantidades del reparto inicial

- **Clase:** `DominoAllFives.Server.Tests.GameplayTests`.
- **Caso ejecutable:** `Match_NewRound_DealsDocumentedHandSize(2,7,14)`.
- **Origen:** [GameplayTests.cs](../DominoAllFives.Server/Tests/GameplayTests.cs#L186).
- **Datos:** `count=2; size=7; stock=14`
- **Precondiciones:** 2 jugadores; mazo generado completo.
- **Postcondiciones:** Manos y pozo repartidos.
- **Resultado esperado:** Cada mano tiene 7 fichas; pozo tiene 14.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `90a53364-10a8-91eb-943d-bc9cfdfd267f`; duración `00:00:00.0050000`.

### PU-049. Verificar cantidades del reparto inicial

- **Clase:** `DominoAllFives.Server.Tests.GameplayTests`.
- **Caso ejecutable:** `Match_NewRound_DealsDocumentedHandSize(3,5,13)`.
- **Origen:** [GameplayTests.cs](../DominoAllFives.Server/Tests/GameplayTests.cs#L186).
- **Datos:** `count=3; size=5; stock=13`
- **Precondiciones:** 3 jugadores; mazo generado completo.
- **Postcondiciones:** Manos y pozo repartidos.
- **Resultado esperado:** Cada mano tiene 5 fichas; pozo tiene 13.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `997108a1-abec-1a9c-64d6-4403cacea453`; duración `00:00:00.0000001`.

### PU-050. Verificar cantidades del reparto inicial

- **Clase:** `DominoAllFives.Server.Tests.GameplayTests`.
- **Caso ejecutable:** `Match_NewRound_DealsDocumentedHandSize(4,5,8)`.
- **Origen:** [GameplayTests.cs](../DominoAllFives.Server/Tests/GameplayTests.cs#L186).
- **Datos:** `count=4; size=5; stock=8`
- **Precondiciones:** 4 jugadores; mazo generado completo.
- **Postcondiciones:** Manos y pozo repartidos.
- **Resultado esperado:** Cada mano tiene 5 fichas; pozo tiene 8.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `f9e5317d-c62a-bb35-caa4-077d99479d4b`; duración `00:00:00.0000001`.

### PU-051. Cerrar ronda y conservar sus manos

- **Clase:** `DominoAllFives.Server.Tests.GameplayTests`.
- **Caso ejecutable:** `Match_RoundFinishes_RevealsPreviousHandsAndDealsNewRound`.
- **Origen:** [GameplayTests.cs](../DominoAllFives.Server/Tests/GameplayTests.cs#L156).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Dos jugadores; objetivo 250; mazo inicial fijo; rondas siguientes generadas con DeckGenerator; máximo 1000 acciones.
- **Postcondiciones:** Resumen anterior conservado y segunda ronda repartida.
- **Resultado esperado:** LastRound de ronda 1 con dos manos; ronda actual 2; manos de siete; el jugador actual posee la ficha de salida requerida.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `8120fba7-6663-0997-3ba6-f5b6970db174`; duración `00:00:00.0040000`.

### PU-052. Rechazar objetivo no admitido

- **Clase:** `DominoAllFives.Server.Tests.GameplayTests`.
- **Caso ejecutable:** `Match_UnsupportedTarget_RejectsInput(0)`.
- **Origen:** [GameplayTests.cs](../DominoAllFives.Server/Tests/GameplayTests.cs#L42).
- **Datos:** `targetPoints=0`
- **Precondiciones:** targetPoints=0; dos jugadores distintos.
- **Postcondiciones:** No se construye partida válida.
- **Resultado esperado:** ArgumentOutOfRangeException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `ac7bcfa0-ff51-e12b-0c68-f9290e100057`; duración `00:00:00.0010000`.

### PU-053. Rechazar objetivo no admitido

- **Clase:** `DominoAllFives.Server.Tests.GameplayTests`.
- **Caso ejecutable:** `Match_UnsupportedTarget_RejectsInput(125)`.
- **Origen:** [GameplayTests.cs](../DominoAllFives.Server/Tests/GameplayTests.cs#L42).
- **Datos:** `targetPoints=125`
- **Precondiciones:** targetPoints=125; dos jugadores distintos.
- **Postcondiciones:** No se construye partida válida.
- **Resultado esperado:** ArgumentOutOfRangeException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `b04941da-4808-59b1-8087-867bb942b797`; duración `00:00:00.0000001`.

### PU-054. Rechazar objetivo no admitido

- **Clase:** `DominoAllFives.Server.Tests.GameplayTests`.
- **Caso ejecutable:** `Match_UnsupportedTarget_RejectsInput(300)`.
- **Origen:** [GameplayTests.cs](../DominoAllFives.Server/Tests/GameplayTests.cs#L42).
- **Datos:** `targetPoints=300`
- **Precondiciones:** targetPoints=300; dos jugadores distintos.
- **Postcondiciones:** No se construye partida válida.
- **Resultado esperado:** ArgumentOutOfRangeException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `256963e6-399d-db6b-9e38-a84d946dc4e4`; duración `00:00:00.0000001`.

### PU-055. Rechazar objetivo no admitido

- **Clase:** `DominoAllFives.Server.Tests.GameplayTests`.
- **Caso ejecutable:** `Match_UnsupportedTarget_RejectsInput(99)`.
- **Origen:** [GameplayTests.cs](../DominoAllFives.Server/Tests/GameplayTests.cs#L42).
- **Datos:** `targetPoints=99`
- **Precondiciones:** targetPoints=99; dos jugadores distintos.
- **Postcondiciones:** No se construye partida válida.
- **Resultado esperado:** ArgumentOutOfRangeException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `0e582a29-aa45-6a9b-899f-21f21acb50b8`; duración `00:00:00.0000001`.

### PU-056. Rechazar pase con pozo disponible

- **Clase:** `DominoAllFives.Server.Tests.GameplayTests`.
- **Caso ejecutable:** `Pass_StockNotEmpty_IsRejected`.
- **Origen:** [GameplayTests.cs](../DominoAllFives.Server/Tests/GameplayTests.cs#L406).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Match recién creado.
- **Postcondiciones:** Pase rechazado.
- **Resultado esperado:** MatchRuleException con Issue=PassNotAllowed.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `1940dfde-bc50-cbe7-5c08-e2f69364f6c0`; duración `00:00:00.0020000`.

### PU-057. Rechazar ficha incompatible

- **Clase:** `DominoAllFives.Server.Tests.GameplayTests`.
- **Caso ejecutable:** `Place_MismatchedTile_DoesNotMutateBoard`.
- **Origen:** [GameplayTests.cs](../DominoAllFives.Server/Tests/GameplayTests.cs#L247).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Tablero abierto con 2-3; intento 4-6 al extremo derecho.
- **Postcondiciones:** Tablero conserva su única ficha.
- **Resultado esperado:** InvalidPlacement; Tiles.Count=1.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `6c6f436d-7260-1bca-be31-6807f4f2ff8d`; duración `00:00:00.0010000`.

### PU-058. Abrir tablero con doble

- **Clase:** `DominoAllFives.Server.Tests.GameplayTests`.
- **Caso ejecutable:** `Place_OpeningDouble_EnablesFourEndsAndCountsItOnce`.
- **Origen:** [GameplayTests.cs](../DominoAllFives.Server/Tests/GameplayTests.cs#L221).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Board vacío; ficha 5-5.
- **Postcondiciones:** Tablero abierto.
- **Resultado esperado:** 4 extremos; puntuación 10.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `bc32596a-283e-9e44-b479-f2f30c9ddc6e`; duración `00:00:00.0010000`.

### PU-059. Abrir tablero sin doble

- **Clase:** `DominoAllFives.Server.Tests.GameplayTests`.
- **Caso ejecutable:** `Place_OpeningNonDouble_EnablesTwoEnds`.
- **Origen:** [GameplayTests.cs](../DominoAllFives.Server/Tests/GameplayTests.cs#L234).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Board vacío; ficha 2-3.
- **Postcondiciones:** Tablero abierto.
- **Resultado esperado:** 2 extremos; puntuación 5.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `7cdf4325-82e0-acfa-bae6-93cdc2c5e159`; duración `00:00:00.0000001`.

### PU-060. Limitar bloqueo al turno siguiente

- **Clase:** `DominoAllFives.Server.Tests.GameplayTests`.
- **Caso ejecutable:** `Place_Purple_BlocksOnlyNextTurnAndExcludesItsPoints`.
- **Origen:** [GameplayTests.cs](../DominoAllFives.Server/Tests/GameplayTests.cs#L281).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** 2-3 abierta y 3-5 morada derecha en turno 1.
- **Postcondiciones:** Legalidad evaluada para 5-6 en turnos sucesivos.
- **Resultado esperado:** Bloqueado en turno 2; liberado en 3; BlockedTurn=2.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `9b39b3ba-eb60-f2b6-136c-a819e57eaa81`; duración `00:00:00.0050000`.

### PU-061. Puntuar doble terminal

- **Clase:** `DominoAllFives.Server.Tests.GameplayTests`.
- **Caso ejecutable:** `Place_TerminalDouble_CountsBothHalves`.
- **Origen:** [GameplayTests.cs](../DominoAllFives.Server/Tests/GameplayTests.cs#L269).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** 5-0 abierta; colocar 5-5 a la izquierda.
- **Postcondiciones:** Ambas mitades del doble aportan puntos.
- **Resultado esperado:** Puntuación de tablero 10.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `004a45c0-6b47-6524-7671-135bc36bee26`; duración `00:00:00.0010000`.

### PU-062. Rechazar extremo indefinido

- **Clase:** `DominoAllFives.Server.Tests.GameplayTests`.
- **Caso ejecutable:** `Place_UndefinedEnd_PreservesBoard(-1)`.
- **Origen:** [GameplayTests.cs](../DominoAllFives.Server/Tests/GameplayTests.cs#L67).
- **Datos:** `invalidEnd=-1`
- **Precondiciones:** Board vacío; ficha 6-6; extremo=-1.
- **Postcondiciones:** Sin fichas ni extremos agregados.
- **Resultado esperado:** InvalidPlacement; Tiles.Count=0; Ends.Count=0.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `afcfc58c-fd58-b0c6-9ce2-fdb97c70f212`; duración `00:00:00.0010000`.

### PU-063. Rechazar extremo indefinido

- **Clase:** `DominoAllFives.Server.Tests.GameplayTests`.
- **Caso ejecutable:** `Place_UndefinedEnd_PreservesBoard(4)`.
- **Origen:** [GameplayTests.cs](../DominoAllFives.Server/Tests/GameplayTests.cs#L67).
- **Datos:** `invalidEnd=4`
- **Precondiciones:** Board vacío; ficha 6-6; extremo=4.
- **Postcondiciones:** Sin fichas ni extremos agregados.
- **Resultado esperado:** InvalidPlacement; Tiles.Count=0; Ends.Count=0.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `b8dfdb2a-c140-1d77-4761-57d7d1f81ab8`; duración `00:00:00.0000001`.

### PU-064. Exigir ficha de salida seleccionada

- **Clase:** `DominoAllFives.Server.Tests.GameplayTests`.
- **Caso ejecutable:** `Play_DifferentOpeningTile_IsRejectedWithoutMutation`.
- **Origen:** [GameplayTests.cs](../DominoAllFives.Server/Tests/GameplayTests.cs#L132).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Jugador de salida intenta otra ficha propia.
- **Postcondiciones:** Tablero y versión intactos.
- **Resultado esperado:** OpeningTileRequired; versión 0; tablero vacío.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `a676bfae-59e5-fcde-6542-a290580ac5d3`; duración `00:00:00.0030000`.

### PU-065. Duplicar premio de ficha roja

- **Clase:** `DominoAllFives.Server.Tests.GameplayTests`.
- **Caso ejecutable:** `Play_RedOpening_DoublesOnlyMovePoints`.
- **Origen:** [GameplayTests.cs](../DominoAllFives.Server/Tests/GameplayTests.cs#L364).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Jugador 1 posee 5-5 roja obligatoria para salir.
- **Postcondiciones:** Premio acreditado y versión incrementada.
- **Resultado esperado:** 20 puntos para jugador 1; versión 1.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `ee09cf0d-cd97-626e-6017-0ecda4ef3c9e`; duración `00:00:00.0010000`.

### PU-066. Rechazar jugada con versión obsoleta

- **Clase:** `DominoAllFives.Server.Tests.GameplayTests`.
- **Caso ejecutable:** `Play_StaleVersion_PreservesState`.
- **Origen:** [GameplayTests.cs](../DominoAllFives.Server/Tests/GameplayTests.cs#L318).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Partida en versión 0; se solicita versión -1.
- **Postcondiciones:** Sin colocación ni avance de versión.
- **Resultado esperado:** StaleVersion; versión 0; tablero vacío.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `0a0c1a9b-40cb-f18c-daf5-5032c6cda5bf`; duración `00:00:00.0010000`.

### PU-067. Rechazar ficha no poseída

- **Clase:** `DominoAllFives.Server.Tests.GameplayTests`.
- **Caso ejecutable:** `Play_UnownedTile_IsRejected`.
- **Origen:** [GameplayTests.cs](../DominoAllFives.Server/Tests/GameplayTests.cs#L341).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Jugador actual intenta TileId=999.
- **Postcondiciones:** Jugada rechazada.
- **Resultado esperado:** MatchRuleException con Issue=TileNotOwned.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `609bb95f-5f97-c2ce-d387-65ab8283766b`; duración `00:00:00.0010000`.

### PU-068. Rechazar identidad o valores de ficha inválidos

- **Clase:** `DominoAllFives.Server.Tests.GameplayTests`.
- **Caso ejecutable:** `Tile_InvalidIdentityOrPips_RejectsInput(0,1,2)`.
- **Origen:** [GameplayTests.cs](../DominoAllFives.Server/Tests/GameplayTests.cs#L20).
- **Datos:** `tileId=0; first=1; second=2`
- **Precondiciones:** tileId=0; first=1; second=2; sin poder.
- **Postcondiciones:** No se crea ficha válida.
- **Resultado esperado:** ArgumentOutOfRangeException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `792db082-afa7-fe4f-62cb-6b8878442e3a`; duración `00:00:00.0000001`.

### PU-069. Rechazar identidad o valores de ficha inválidos

- **Clase:** `DominoAllFives.Server.Tests.GameplayTests`.
- **Caso ejecutable:** `Tile_InvalidIdentityOrPips_RejectsInput(1,-1,2)`.
- **Origen:** [GameplayTests.cs](../DominoAllFives.Server/Tests/GameplayTests.cs#L20).
- **Datos:** `tileId=1; first=-1; second=2`
- **Precondiciones:** tileId=1; first=-1; second=2; sin poder.
- **Postcondiciones:** No se crea ficha válida.
- **Resultado esperado:** ArgumentOutOfRangeException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `f8a16d40-8618-598f-434b-a5430305592f`; duración `00:00:00.0000001`.

### PU-070. Rechazar identidad o valores de ficha inválidos

- **Clase:** `DominoAllFives.Server.Tests.GameplayTests`.
- **Caso ejecutable:** `Tile_InvalidIdentityOrPips_RejectsInput(1,1,-1)`.
- **Origen:** [GameplayTests.cs](../DominoAllFives.Server/Tests/GameplayTests.cs#L20).
- **Datos:** `tileId=1; first=1; second=-1`
- **Precondiciones:** tileId=1; first=1; second=-1; sin poder.
- **Postcondiciones:** No se crea ficha válida.
- **Resultado esperado:** ArgumentOutOfRangeException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `0c3f1ceb-8d35-fea6-ea78-4710f361e0f8`; duración `00:00:00.0000001`.

### PU-071. Rechazar identidad o valores de ficha inválidos

- **Clase:** `DominoAllFives.Server.Tests.GameplayTests`.
- **Caso ejecutable:** `Tile_InvalidIdentityOrPips_RejectsInput(1,1,7)`.
- **Origen:** [GameplayTests.cs](../DominoAllFives.Server/Tests/GameplayTests.cs#L20).
- **Datos:** `tileId=1; first=1; second=7`
- **Precondiciones:** tileId=1; first=1; second=7; sin poder.
- **Postcondiciones:** No se crea ficha válida.
- **Resultado esperado:** ArgumentOutOfRangeException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `2904bcfc-b8e7-d506-a6b5-478f4be2ccc9`; duración `00:00:00.0000001`.

### PU-072. Rechazar identidad o valores de ficha inválidos

- **Clase:** `DominoAllFives.Server.Tests.GameplayTests`.
- **Caso ejecutable:** `Tile_InvalidIdentityOrPips_RejectsInput(1,7,2)`.
- **Origen:** [GameplayTests.cs](../DominoAllFives.Server/Tests/GameplayTests.cs#L20).
- **Datos:** `tileId=1; first=7; second=2`
- **Precondiciones:** tileId=1; first=7; second=2; sin poder.
- **Postcondiciones:** No se crea ficha válida.
- **Resultado esperado:** ArgumentOutOfRangeException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `2c0c3ed4-f174-bb06-e352-3913a09072db`; duración `00:00:00.0000001`.

### PU-073. Verificar metadatos de callbacks

- **Clase:** `DominoAllFives.Server.Tests.LobbyContractTests`.
- **Caso ejecutable:** `CallbackContract_AllOperations_AreOneWay`.
- **Origen:** [LobbyContractTests.cs](../DominoAllFives.Server/Tests/LobbyContractTests.cs#L72).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Reflexión sobre interfaz de callback; sin canales.
- **Postcondiciones:** Contrato inspeccionado en memoria.
- **Resultado esperado:** Todos los OperationContract tienen IsOneWay=true.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `c80b36cb-e6bf-20ec-593e-b6e565bcd2fb`; duración `00:00:00.0220000`.

### PU-074. Conservar valor serializado de LobbyIssue

- **Clase:** `DominoAllFives.Server.Tests.LobbyContractTests`.
- **Caso ejecutable:** `Serialize_RenamedLobbyIssue_PreservesPublishedValue(Full,"LobbyFull")`.
- **Origen:** [LobbyContractTests.cs](../DominoAllFives.Server/Tests/LobbyContractTests.cs#L19).
- **Datos:** `issue=Full; expectedWireValue="LobbyFull"`
- **Precondiciones:** issue=Full; DataContractSerializer y MemoryStream.
- **Postcondiciones:** Valor escrito y reconstruido en memoria.
- **Resultado esperado:** Texto XML="LobbyFull"; enum reconstruido igual a Full.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `bbdaee66-3452-7867-f0cb-c2d576cb0db7`; duración `00:00:00.0000001`.

### PU-075. Conservar valor serializado de LobbyIssue

- **Clase:** `DominoAllFives.Server.Tests.LobbyContractTests`.
- **Caso ejecutable:** `Serialize_RenamedLobbyIssue_PreservesPublishedValue(NotFound,"LobbyNotFound")`.
- **Origen:** [LobbyContractTests.cs](../DominoAllFives.Server/Tests/LobbyContractTests.cs#L19).
- **Datos:** `issue=NotFound; expectedWireValue="LobbyNotFound"`
- **Precondiciones:** issue=NotFound; DataContractSerializer y MemoryStream.
- **Postcondiciones:** Valor escrito y reconstruido en memoria.
- **Resultado esperado:** Texto XML="LobbyNotFound"; enum reconstruido igual a NotFound.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `6a58a1b6-193d-ec69-b22e-3dcd5438e8d7`; duración `00:00:00.5080000`.

### PU-076. Conservar valor serializado de LobbyIssue

- **Clase:** `DominoAllFives.Server.Tests.LobbyContractTests`.
- **Caso ejecutable:** `Serialize_RenamedLobbyIssue_PreservesPublishedValue(Unavailable,"LobbyUnavailable")`.
- **Origen:** [LobbyContractTests.cs](../DominoAllFives.Server/Tests/LobbyContractTests.cs#L19).
- **Datos:** `issue=Unavailable; expectedWireValue="LobbyUnavailable"`
- **Precondiciones:** issue=Unavailable; DataContractSerializer y MemoryStream.
- **Postcondiciones:** Valor escrito y reconstruido en memoria.
- **Resultado esperado:** Texto XML="LobbyUnavailable"; enum reconstruido igual a Unavailable.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `38ab0818-f748-6b6d-b739-1e9cc18ac39e`; duración `00:00:00.0010000`.

### PU-077. Verificar firmas de creación y unión

- **Clase:** `DominoAllFives.Server.Tests.LobbyContractTests`.
- **Caso ejecutable:** `ServiceContract_CreateAndJoin_ReturnVoid`.
- **Origen:** [LobbyContractTests.cs](../DominoAllFives.Server/Tests/LobbyContractTests.cs#L60).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Reflexión sobre ILobbyService.
- **Postcondiciones:** Firmas inspeccionadas sin ejecutar servicio.
- **Resultado esperado:** CreateLobby y JoinLobby retornan void.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `af7ae70f-c11a-a181-9d17-513b19d1fd66`; duración `00:00:00.0010000`.

### PU-078. Verificar metadatos dúplex de lobby

- **Clase:** `DominoAllFives.Server.Tests.LobbyContractTests`.
- **Caso ejecutable:** `ServiceContract_Lobby_PreservesDuplexSessionConfiguration`.
- **Origen:** [LobbyContractTests.cs](../DominoAllFives.Server/Tests/LobbyContractTests.cs#L42).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Reflexión sobre ILobbyService.
- **Postcondiciones:** Atributos inspeccionados sin alojar servicio.
- **Resultado esperado:** CallbackContract=ILobbyCallback; SessionMode.Required.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `2364d6b2-e1fc-37f9-f44f-db78450c0015`; duración `00:00:00.0140000`.

### PU-079. Rechazar dependencia manager nula

- **Clase:** `DominoAllFives.Server.Tests.LobbyControllerTests`.
- **Caso ejecutable:** `Constructor_NullManager_ThrowsArgumentNullException`.
- **Origen:** [LobbyControllerTests.cs](../DominoAllFives.Server/Tests/LobbyControllerTests.cs#L15).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** LobbyManager null.
- **Postcondiciones:** Controller no construido.
- **Resultado esperado:** ArgumentNullException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `1a54c164-51f0-2be7-5434-bf3a8a9e7c71`; duración `00:00:00.0010000`.

### PU-080. Rechazar nombre vacío

- **Clase:** `DominoAllFives.Server.Tests.LobbyControllerTests`.
- **Caso ejecutable:** `CreateLobby_BlankUsername_ThrowsArgumentException`.
- **Origen:** [LobbyControllerTests.cs](../DominoAllFives.Server/Tests/LobbyControllerTests.cs#L82).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Jugador con Username de espacios; configuración válida.
- **Postcondiciones:** Validación del controller rechaza antes de registrar lobby.
- **Resultado esperado:** ArgumentException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `f772936c-a6cb-a37e-1a7e-881cdde5c473`; duración `00:00:00.0040000`.

### PU-081. Rechazar límite de jugadores

- **Clase:** `DominoAllFives.Server.Tests.LobbyControllerTests`.
- **Caso ejecutable:** `CreateLobby_InvalidPlayerLimit_ThrowsSpecificIssue(1)`.
- **Origen:** [LobbyControllerTests.cs](../DominoAllFives.Server/Tests/LobbyControllerTests.cs#L27).
- **Datos:** `maximumPlayers=1`
- **Precondiciones:** MaxPlayers=1; objetivo 100; jugador válido.
- **Postcondiciones:** No se registra lobby.
- **Resultado esperado:** LobbyRuleException con Issue=InvalidPlayerLimit.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `d198ed43-8197-8bb2-c7cd-6b276e26d928`; duración `00:00:00.0040000`.

### PU-082. Rechazar límite de jugadores

- **Clase:** `DominoAllFives.Server.Tests.LobbyControllerTests`.
- **Caso ejecutable:** `CreateLobby_InvalidPlayerLimit_ThrowsSpecificIssue(5)`.
- **Origen:** [LobbyControllerTests.cs](../DominoAllFives.Server/Tests/LobbyControllerTests.cs#L27).
- **Datos:** `maximumPlayers=5`
- **Precondiciones:** MaxPlayers=5; objetivo 100; jugador válido.
- **Postcondiciones:** No se registra lobby.
- **Resultado esperado:** LobbyRuleException con Issue=InvalidPlayerLimit.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `c23928dd-80b8-ff61-7bec-63926306146b`; duración `00:00:00.0010000`.

### PU-083. Rechazar objetivo de lobby no admitido

- **Clase:** `DominoAllFives.Server.Tests.LobbyControllerTests`.
- **Caso ejecutable:** `CreateLobby_InvalidTargetPoints_ThrowsSpecificIssue`.
- **Origen:** [LobbyControllerTests.cs](../DominoAllFives.Server/Tests/LobbyControllerTests.cs#L42).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** MaxPlayers=4; TargetPoints=300; jugador válido.
- **Postcondiciones:** No se registra lobby.
- **Resultado esperado:** LobbyRuleException con Issue=InvalidTargetPoints.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `e2229534-c902-60f7-44e6-73db19ed21c2`; duración `00:00:00.0020000`.

### PU-084. Rechazar código de lobby inválido

- **Clase:** `DominoAllFives.Server.Tests.LobbyControllerTests`.
- **Caso ejecutable:** `JoinLobby_InvalidCode_ThrowsArgumentException("   ")`.
- **Origen:** [LobbyControllerTests.cs](../DominoAllFives.Server/Tests/LobbyControllerTests.cs#L59).
- **Datos:** `lobbyCode="   "`
- **Precondiciones:** lobbyCode="   "; jugador válido.
- **Postcondiciones:** No se consulta ni se incorpora participante.
- **Resultado esperado:** ArgumentException o subtipo ArgumentNullException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `4f175aa4-a556-c1b7-4a94-dc3c0201563e`; duración `00:00:00.0010000`.

### PU-085. Rechazar código de lobby inválido

- **Clase:** `DominoAllFives.Server.Tests.LobbyControllerTests`.
- **Caso ejecutable:** `JoinLobby_InvalidCode_ThrowsArgumentException("")`.
- **Origen:** [LobbyControllerTests.cs](../DominoAllFives.Server/Tests/LobbyControllerTests.cs#L59).
- **Datos:** `lobbyCode=""`
- **Precondiciones:** lobbyCode=""; jugador válido.
- **Postcondiciones:** No se consulta ni se incorpora participante.
- **Resultado esperado:** ArgumentException o subtipo ArgumentNullException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `0fbf4cab-a1ad-3b8b-073e-256a2a1c7444`; duración `00:00:00.0020000`.

### PU-086. Rechazar código de lobby inválido

- **Clase:** `DominoAllFives.Server.Tests.LobbyControllerTests`.
- **Caso ejecutable:** `JoinLobby_InvalidCode_ThrowsArgumentException(null)`.
- **Origen:** [LobbyControllerTests.cs](../DominoAllFives.Server/Tests/LobbyControllerTests.cs#L59).
- **Datos:** `lobbyCode=null`
- **Precondiciones:** lobbyCode=null; jugador válido.
- **Postcondiciones:** No se consulta ni se incorpora participante.
- **Resultado esperado:** ArgumentException o subtipo ArgumentNullException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `b65975b8-f907-7dde-7b55-6a5b76c583d7`; duración `00:00:00.0030000`.

### PU-087. Rechazar ID al abandonar lobby

- **Clase:** `DominoAllFives.Server.Tests.LobbyControllerTests`.
- **Caso ejecutable:** `LeaveLobby_InvalidPlayerId_ThrowsArgumentOutOfRangeException(-1)`.
- **Origen:** [LobbyControllerTests.cs](../DominoAllFives.Server/Tests/LobbyControllerTests.cs#L71).
- **Datos:** `playerId=-1`
- **Precondiciones:** playerId=-1.
- **Postcondiciones:** No se solicita salida al manager.
- **Resultado esperado:** ArgumentOutOfRangeException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `9e1ce1ac-982b-bc74-822f-6ff19aeea07c`; duración `00:00:00.0000001`.

### PU-088. Rechazar ID al abandonar lobby

- **Clase:** `DominoAllFives.Server.Tests.LobbyControllerTests`.
- **Caso ejecutable:** `LeaveLobby_InvalidPlayerId_ThrowsArgumentOutOfRangeException(0)`.
- **Origen:** [LobbyControllerTests.cs](../DominoAllFives.Server/Tests/LobbyControllerTests.cs#L71).
- **Datos:** `playerId=0`
- **Precondiciones:** playerId=0.
- **Postcondiciones:** No se solicita salida al manager.
- **Resultado esperado:** ArgumentOutOfRangeException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `172755d0-3029-11b8-2c4b-a96af3ac214e`; duración `00:00:00.0010000`.

### PU-089. Conservar código de rechazo de negocio

- **Clase:** `DominoAllFives.Server.Tests.LobbyRuleExceptionTests`.
- **Caso ejecutable:** `Constructor_DefinedIssue_PreservesIssue(Full)`.
- **Origen:** [LobbyRuleExceptionTests.cs](../DominoAllFives.Server/Tests/LobbyRuleExceptionTests.cs#L14).
- **Datos:** `issue=Full`
- **Precondiciones:** Valor definido del enum de la excepción: Full.
- **Postcondiciones:** Excepción construida con Issue inmutable.
- **Resultado esperado:** Issue igual a Full.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `8ff9d919-4dad-2e54-9e5b-f137fa058dcb`; duración `00:00:00.0000001`.

### PU-090. Conservar código de rechazo de negocio

- **Clase:** `DominoAllFives.Server.Tests.LobbyRuleExceptionTests`.
- **Caso ejecutable:** `Constructor_DefinedIssue_PreservesIssue(InsufficientPlayers)`.
- **Origen:** [LobbyRuleExceptionTests.cs](../DominoAllFives.Server/Tests/LobbyRuleExceptionTests.cs#L14).
- **Datos:** `issue=InsufficientPlayers`
- **Precondiciones:** Valor definido del enum de la excepción: InsufficientPlayers.
- **Postcondiciones:** Excepción construida con Issue inmutable.
- **Resultado esperado:** Issue igual a InsufficientPlayers.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `9459e39b-3291-1528-d84d-35aff8639a03`; duración `00:00:00.0000001`.

### PU-091. Conservar código de rechazo de negocio

- **Clase:** `DominoAllFives.Server.Tests.LobbyRuleExceptionTests`.
- **Caso ejecutable:** `Constructor_DefinedIssue_PreservesIssue(NotFound)`.
- **Origen:** [LobbyRuleExceptionTests.cs](../DominoAllFives.Server/Tests/LobbyRuleExceptionTests.cs#L14).
- **Datos:** `issue=NotFound`
- **Precondiciones:** Valor definido del enum de la excepción: NotFound.
- **Postcondiciones:** Excepción construida con Issue inmutable.
- **Resultado esperado:** Issue igual a NotFound.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `c45b7913-99ae-7ae5-2ca4-03a4e5313e63`; duración `00:00:00.0060000`.

### PU-092. Rechazar código de negocio indefinido

- **Clase:** `DominoAllFives.Server.Tests.LobbyRuleExceptionTests`.
- **Caso ejecutable:** `Constructor_UndefinedIssue_ThrowsArgumentOutOfRangeException`.
- **Origen:** [LobbyRuleExceptionTests.cs](../DominoAllFives.Server/Tests/LobbyRuleExceptionTests.cs#L24).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Enum convertido desde int.MaxValue.
- **Postcondiciones:** No se construye una excepción de negocio válida.
- **Resultado esperado:** ArgumentOutOfRangeException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `e99b7bf3-396f-f2e3-e493-0f62eb5f03e5`; duración `00:00:00.0010000`.

### PU-093. Verificar metadatos de callbacks

- **Clase:** `DominoAllFives.Server.Tests.MatchContractTests`.
- **Caso ejecutable:** `CallbackContract_AllOperations_AreOneWay`.
- **Origen:** [MatchContractTests.cs](../DominoAllFives.Server/Tests/MatchContractTests.cs#L178).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Reflexión sobre interfaz de callback; sin canales.
- **Postcondiciones:** Contrato inspeccionado en memoria.
- **Resultado esperado:** Todos los OperationContract tienen IsOneWay=true.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `6228d5f9-6668-6407-953e-f5f4f0774cab`; duración `00:00:00.0010000`.

### PU-094. Verificar metadatos dúplex de partida

- **Clase:** `DominoAllFives.Server.Tests.MatchContractTests`.
- **Caso ejecutable:** `ServiceContract_Match_PreservesDuplexSessionConfiguration`.
- **Origen:** [MatchContractTests.cs](../DominoAllFives.Server/Tests/MatchContractTests.cs#L138).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Reflexión sobre IMatchService.
- **Postcondiciones:** Atributos inspeccionados sin alojar servicio.
- **Resultado esperado:** CallbackContract=IMatchCallback; SessionMode.Required.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `8f8d1af7-a80b-2e25-ddff-6cb0db5e4514`; duración `00:00:00.0010000`.

### PU-095. Verificar apertura de sesión de partida

- **Clase:** `DominoAllFives.Server.Tests.MatchContractTests`.
- **Caso ejecutable:** `ServiceContract_OpenSession_IsOnlyInitiatingOperation`.
- **Origen:** [MatchContractTests.cs](../DominoAllFives.Server/Tests/MatchContractTests.cs#L156).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Métodos de IMatchService inspeccionados por reflexión.
- **Postcondiciones:** Ciclo de sesión publicado conservado.
- **Resultado esperado:** Solo OpenMatchSession tiene IsInitiating=true.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `6f8db78f-e4ee-932e-7fbc-cfbcaa942308`; duración `00:00:00.0010000`.

### PU-096. Conservar objetivo admitido de partida

- **Clase:** `DominoAllFives.Server.Tests.MatchPreparationTests`.
- **Caso ejecutable:** `Constructor_AllowedTarget_PreservesTarget(100)`.
- **Origen:** [MatchPreparationTests.cs](../DominoAllFives.Server/Tests/MatchPreparationTests.cs#L92).
- **Datos:** `target=100`
- **Precondiciones:** target=100; mazo fijo y dos participantes.
- **Postcondiciones:** Partida construida.
- **Resultado esperado:** TargetPoints=100.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `0c8885ad-4b3c-4522-4e2c-57e8c4a0eb63`; duración `00:00:00.0010000`.

### PU-097. Conservar objetivo admitido de partida

- **Clase:** `DominoAllFives.Server.Tests.MatchPreparationTests`.
- **Caso ejecutable:** `Constructor_AllowedTarget_PreservesTarget(150)`.
- **Origen:** [MatchPreparationTests.cs](../DominoAllFives.Server/Tests/MatchPreparationTests.cs#L92).
- **Datos:** `target=150`
- **Precondiciones:** target=150; mazo fijo y dos participantes.
- **Postcondiciones:** Partida construida.
- **Resultado esperado:** TargetPoints=150.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `e88d44df-4a1e-056e-e609-b757d68b2e37`; duración `00:00:00.0000001`.

### PU-098. Conservar objetivo admitido de partida

- **Clase:** `DominoAllFives.Server.Tests.MatchPreparationTests`.
- **Caso ejecutable:** `Constructor_AllowedTarget_PreservesTarget(200)`.
- **Origen:** [MatchPreparationTests.cs](../DominoAllFives.Server/Tests/MatchPreparationTests.cs#L92).
- **Datos:** `target=200`
- **Precondiciones:** target=200; mazo fijo y dos participantes.
- **Postcondiciones:** Partida construida.
- **Resultado esperado:** TargetPoints=200.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `e893b914-5d09-d318-28b7-4717c5c06583`; duración `00:00:00.0000001`.

### PU-099. Conservar objetivo admitido de partida

- **Clase:** `DominoAllFives.Server.Tests.MatchPreparationTests`.
- **Caso ejecutable:** `Constructor_AllowedTarget_PreservesTarget(250)`.
- **Origen:** [MatchPreparationTests.cs](../DominoAllFives.Server/Tests/MatchPreparationTests.cs#L92).
- **Datos:** `target=250`
- **Precondiciones:** target=250; mazo fijo y dos participantes.
- **Postcondiciones:** Partida construida.
- **Resultado esperado:** TargetPoints=250.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `95a3cea9-593d-a88c-405c-b431384606ca`; duración `00:00:00.0000001`.

### PU-100. Rechazar composición de mazo inválida

- **Clase:** `DominoAllFives.Server.Tests.MatchPreparationTests`.
- **Caso ejecutable:** `Constructor_InvalidDeck_RejectsBeforeDealing(0)`.
- **Origen:** [MatchPreparationTests.cs](../DominoAllFives.Server/Tests/MatchPreparationTests.cs#L123).
- **Datos:** `scenario=0`
- **Precondiciones:** Mazo null.
- **Postcondiciones:** Constructor rechazado antes de repartir.
- **Resultado esperado:** ArgumentException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `de6a1cb2-9103-e9ce-3d35-b69bc7f43272`; duración `00:00:00.0010000`.

### PU-101. Rechazar composición de mazo inválida

- **Clase:** `DominoAllFives.Server.Tests.MatchPreparationTests`.
- **Caso ejecutable:** `Constructor_InvalidDeck_RejectsBeforeDealing(1)`.
- **Origen:** [MatchPreparationTests.cs](../DominoAllFives.Server/Tests/MatchPreparationTests.cs#L123).
- **Datos:** `scenario=1`
- **Precondiciones:** Mazo de 27 fichas.
- **Postcondiciones:** Constructor rechazado antes de repartir.
- **Resultado esperado:** ArgumentException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `1927d059-2f8a-75c6-bdd2-fbaf05d8b903`; duración `00:00:00.0000001`.

### PU-102. Rechazar composición de mazo inválida

- **Clase:** `DominoAllFives.Server.Tests.MatchPreparationTests`.
- **Caso ejecutable:** `Constructor_InvalidDeck_RejectsBeforeDealing(2)`.
- **Origen:** [MatchPreparationTests.cs](../DominoAllFives.Server/Tests/MatchPreparationTests.cs#L123).
- **Datos:** `scenario=2`
- **Precondiciones:** Una ficha null.
- **Postcondiciones:** Constructor rechazado antes de repartir.
- **Resultado esperado:** ArgumentException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `b4f6b5ba-2d1a-910d-c0d8-07875051af20`; duración `00:00:00.0000001`.

### PU-103. Rechazar composición de mazo inválida

- **Clase:** `DominoAllFives.Server.Tests.MatchPreparationTests`.
- **Caso ejecutable:** `Constructor_InvalidDeck_RejectsBeforeDealing(3)`.
- **Origen:** [MatchPreparationTests.cs](../DominoAllFives.Server/Tests/MatchPreparationTests.cs#L123).
- **Datos:** `scenario=3`
- **Precondiciones:** Identificador repetido entre fichas de valores distintos.
- **Postcondiciones:** Constructor rechazado antes de repartir.
- **Resultado esperado:** ArgumentException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `506807b7-3734-e74b-36cf-2be39ec73e74`; duración `00:00:00.0000001`.

### PU-104. Rechazar composición de mazo inválida

- **Clase:** `DominoAllFives.Server.Tests.MatchPreparationTests`.
- **Caso ejecutable:** `Constructor_InvalidDeck_RejectsBeforeDealing(4)`.
- **Origen:** [MatchPreparationTests.cs](../DominoAllFives.Server/Tests/MatchPreparationTests.cs#L123).
- **Datos:** `scenario=4`
- **Precondiciones:** Valores 0-1 y 1-0 repetidos con IDs distintos.
- **Postcondiciones:** Constructor rechazado antes de repartir.
- **Resultado esperado:** ArgumentException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `d74f07d5-5660-5aa5-4ad0-223a51a46ab7`; duración `00:00:00.0000001`.

### PU-105. Rechazar composición de mazo inválida

- **Clase:** `DominoAllFives.Server.Tests.MatchPreparationTests`.
- **Caso ejecutable:** `Constructor_InvalidDeck_RejectsBeforeDealing(5)`.
- **Origen:** [MatchPreparationTests.cs](../DominoAllFives.Server/Tests/MatchPreparationTests.cs#L123).
- **Datos:** `scenario=5`
- **Precondiciones:** Seis fichas con poder Block.
- **Postcondiciones:** Constructor rechazado antes de repartir.
- **Resultado esperado:** ArgumentException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `7783496d-2f90-6b50-9222-7755505b2fd4`; duración `00:00:00.0000001`.

### PU-106. Rechazar identidad o dependencia de Match

- **Clase:** `DominoAllFives.Server.Tests.MatchPreparationTests`.
- **Caso ejecutable:** `Constructor_InvalidIdentity_ReportsArgument(0,"matchId",System.ArgumentOutOfRangeException)`.
- **Origen:** [MatchPreparationTests.cs](../DominoAllFives.Server/Tests/MatchPreparationTests.cs#L198).
- **Datos:** `scenario=0; parameterName="matchId"; exceptionType=System.ArgumentOutOfRangeException`
- **Precondiciones:** matchId=0.
- **Postcondiciones:** La construcción de Match se rechaza antes del reparto.
- **Resultado esperado:** System.ArgumentOutOfRangeException; ParamName="matchId".
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `97f59db0-6f73-0b43-4373-ba32f93c99fd`; duración `00:00:00.0010000`.

### PU-107. Rechazar identidad o dependencia de Match

- **Clase:** `DominoAllFives.Server.Tests.MatchPreparationTests`.
- **Caso ejecutable:** `Constructor_InvalidIdentity_ReportsArgument(1,"matchId",System.ArgumentOutOfRangeException)`.
- **Origen:** [MatchPreparationTests.cs](../DominoAllFives.Server/Tests/MatchPreparationTests.cs#L198).
- **Datos:** `scenario=1; parameterName="matchId"; exceptionType=System.ArgumentOutOfRangeException`
- **Precondiciones:** matchId=-1.
- **Postcondiciones:** La construcción de Match se rechaza antes del reparto.
- **Resultado esperado:** System.ArgumentOutOfRangeException; ParamName="matchId".
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `48cf5e60-366a-8a8c-04ee-49c8d8ee2c2f`; duración `00:00:00.0000001`.

### PU-108. Rechazar identidad o dependencia de Match

- **Clase:** `DominoAllFives.Server.Tests.MatchPreparationTests`.
- **Caso ejecutable:** `Constructor_InvalidIdentity_ReportsArgument(2,"playerIds",System.ArgumentNullException)`.
- **Origen:** [MatchPreparationTests.cs](../DominoAllFives.Server/Tests/MatchPreparationTests.cs#L198).
- **Datos:** `scenario=2; parameterName="playerIds"; exceptionType=System.ArgumentNullException`
- **Precondiciones:** playerIds=null.
- **Postcondiciones:** La construcción de Match se rechaza antes del reparto.
- **Resultado esperado:** System.ArgumentNullException; ParamName="playerIds".
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `2426e99d-12e5-b867-3dfb-0f794c6b6e3d`; duración `00:00:00.0000001`.

### PU-109. Rechazar participantes inválidos

- **Clase:** `DominoAllFives.Server.Tests.MatchPreparationTests`.
- **Caso ejecutable:** `Constructor_InvalidPlayers_ReportsPlayerIds([-1, 2])`.
- **Origen:** [MatchPreparationTests.cs](../DominoAllFives.Server/Tests/MatchPreparationTests.cs#L175).
- **Datos:** `ids=[-1, 2]`
- **Precondiciones:** ids=[-1, 2]; lista de fichas disponible.
- **Postcondiciones:** La construcción de Match se rechaza antes del reparto.
- **Resultado esperado:** ArgumentException; ParamName=playerIds.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `78038e6f-5426-616e-c0ba-6cee7cb7eb1c`; duración `00:00:00.0000001`.

### PU-110. Rechazar participantes inválidos

- **Clase:** `DominoAllFives.Server.Tests.MatchPreparationTests`.
- **Caso ejecutable:** `Constructor_InvalidPlayers_ReportsPlayerIds([0, 2])`.
- **Origen:** [MatchPreparationTests.cs](../DominoAllFives.Server/Tests/MatchPreparationTests.cs#L175).
- **Datos:** `ids=[0, 2]`
- **Precondiciones:** ids=[0, 2]; lista de fichas disponible.
- **Postcondiciones:** La construcción de Match se rechaza antes del reparto.
- **Resultado esperado:** ArgumentException; ParamName=playerIds.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `3d71c352-f556-5c84-d3d9-ae73b4d740c3`; duración `00:00:00.0000001`.

### PU-111. Rechazar participantes inválidos

- **Clase:** `DominoAllFives.Server.Tests.MatchPreparationTests`.
- **Caso ejecutable:** `Constructor_InvalidPlayers_ReportsPlayerIds([1, 1])`.
- **Origen:** [MatchPreparationTests.cs](../DominoAllFives.Server/Tests/MatchPreparationTests.cs#L175).
- **Datos:** `ids=[1, 1]`
- **Precondiciones:** ids=[1, 1]; lista de fichas disponible.
- **Postcondiciones:** La construcción de Match se rechaza antes del reparto.
- **Resultado esperado:** ArgumentException; ParamName=playerIds.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `9d0dda7b-535e-4426-fe3f-268e093cf02b`; duración `00:00:00.0000001`.

### PU-112. Rechazar participantes inválidos

- **Clase:** `DominoAllFives.Server.Tests.MatchPreparationTests`.
- **Caso ejecutable:** `Constructor_InvalidPlayers_ReportsPlayerIds([1, 2, 3, 4, 5])`.
- **Origen:** [MatchPreparationTests.cs](../DominoAllFives.Server/Tests/MatchPreparationTests.cs#L175).
- **Datos:** `ids=[1, 2, 3, 4, 5]`
- **Precondiciones:** ids=[1, 2, 3, 4, 5]; lista de fichas disponible.
- **Postcondiciones:** La construcción de Match se rechaza antes del reparto.
- **Resultado esperado:** ArgumentException; ParamName=playerIds.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `cd4d6368-fed9-8c1f-7680-9375549f33cc`; duración `00:00:00.0000001`.

### PU-113. Rechazar participantes inválidos

- **Clase:** `DominoAllFives.Server.Tests.MatchPreparationTests`.
- **Caso ejecutable:** `Constructor_InvalidPlayers_ReportsPlayerIds([1])`.
- **Origen:** [MatchPreparationTests.cs](../DominoAllFives.Server/Tests/MatchPreparationTests.cs#L175).
- **Datos:** `ids=[1]`
- **Precondiciones:** ids=[1]; lista de fichas disponible.
- **Postcondiciones:** La construcción de Match se rechaza antes del reparto.
- **Resultado esperado:** ArgumentException; ParamName=playerIds.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `057cb13a-073e-54f8-fd1e-0882282246e7`; duración `00:00:00.0010000`.

### PU-114. Elegir salida sin dobles repartidos

- **Clase:** `DominoAllFives.Server.Tests.MatchPreparationTests`.
- **Caso ejecutable:** `Constructor_NoDealtDoubles_SelectsHighestSumThenHighestValue`.
- **Origen:** [MatchPreparationTests.cs](../DominoAllFives.Server/Tests/MatchPreparationTests.cs#L103).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** 4-5 y 3-6 son fichas máximas de las dos manos.
- **Postcondiciones:** Partida lista para salir.
- **Resultado esperado:** CurrentPlayerId=2; OpeningTileId corresponde a 3-6.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `bf5a57e6-ce9c-eb6a-360b-f35d816a9aa5`; duración `00:00:00.0010000`.

### PU-115. Verificar reparto exacto y estado inicial

- **Clase:** `DominoAllFives.Server.Tests.MatchPreparationTests`.
- **Caso ejecutable:** `Constructor_OrderedDeck_PreservesExactHandsAndInitialState(2,7,14)`.
- **Origen:** [MatchPreparationTests.cs](../DominoAllFives.Server/Tests/MatchPreparationTests.cs#L45).
- **Datos:** `playerCount=2; handSize=7; stockCount=14`
- **Precondiciones:** 2 jugadores; 6-6 y 5-5 primero; mazo fijo.
- **Postcondiciones:** Manos corresponden al segmento de 7 fichas por jugador.
- **Resultado esperado:** Pozo=14; ronda 1; turno 0; versión 0; sale jugador 1 con 6-6; tablero vacío; InProgress.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `898619df-ddad-55ee-0d8b-e791feff0bda`; duración `00:00:00.0010000`.

### PU-116. Verificar reparto exacto y estado inicial

- **Clase:** `DominoAllFives.Server.Tests.MatchPreparationTests`.
- **Caso ejecutable:** `Constructor_OrderedDeck_PreservesExactHandsAndInitialState(3,5,13)`.
- **Origen:** [MatchPreparationTests.cs](../DominoAllFives.Server/Tests/MatchPreparationTests.cs#L45).
- **Datos:** `playerCount=3; handSize=5; stockCount=13`
- **Precondiciones:** 3 jugadores; 6-6 y 5-5 primero; mazo fijo.
- **Postcondiciones:** Manos corresponden al segmento de 5 fichas por jugador.
- **Resultado esperado:** Pozo=13; ronda 1; turno 0; versión 0; sale jugador 1 con 6-6; tablero vacío; InProgress.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `d1aeccfc-c62b-22e7-f4fa-33b45a6bd890`; duración `00:00:00.0000001`.

### PU-117. Verificar reparto exacto y estado inicial

- **Clase:** `DominoAllFives.Server.Tests.MatchPreparationTests`.
- **Caso ejecutable:** `Constructor_OrderedDeck_PreservesExactHandsAndInitialState(4,5,8)`.
- **Origen:** [MatchPreparationTests.cs](../DominoAllFives.Server/Tests/MatchPreparationTests.cs#L45).
- **Datos:** `playerCount=4; handSize=5; stockCount=8`
- **Precondiciones:** 4 jugadores; 6-6 y 5-5 primero; mazo fijo.
- **Postcondiciones:** Manos corresponden al segmento de 5 fichas por jugador.
- **Resultado esperado:** Pozo=8; ronda 1; turno 0; versión 0; sale jugador 1 con 6-6; tablero vacío; InProgress.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `4ffadd15-853c-e1fb-bbb0-3a1c47916d0a`; duración `00:00:00.0000001`.

### PU-118. Rechazar acción incompatible con pozo vacío

- **Clase:** `DominoAllFives.Server.Tests.MatchRoundTests`.
- **Caso ejecutable:** `Operation_EmptyStockWithIncompatibleAction_PreservesTurn("Draw",21,DrawNotAllowed)`.
- **Origen:** [MatchRoundTests.cs](../DominoAllFives.Server/Tests/MatchRoundTests.cs#L243).
- **Datos:** `operation="Draw"; precedingActions=21; issue=DrawNotAllowed`
- **Precondiciones:** Escenario 109 tras 21 acciones; operación="Draw".
- **Postcondiciones:** Turno, versión y tamaño de mano conservados.
- **Resultado esperado:** Issue=DrawNotAllowed; StockCount=0.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `8465e27e-8aef-7a4b-688d-547f6d55b26d`; duración `00:00:00.0040000`.

### PU-119. Rechazar acción incompatible con pozo vacío

- **Clase:** `DominoAllFives.Server.Tests.MatchRoundTests`.
- **Caso ejecutable:** `Operation_EmptyStockWithIncompatibleAction_PreservesTurn("Pass",22,PassNotAllowed)`.
- **Origen:** [MatchRoundTests.cs](../DominoAllFives.Server/Tests/MatchRoundTests.cs#L243).
- **Datos:** `operation="Pass"; precedingActions=22; issue=PassNotAllowed`
- **Precondiciones:** Escenario 109 tras 22 acciones; operación="Pass".
- **Postcondiciones:** Turno, versión y tamaño de mano conservados.
- **Resultado esperado:** Issue=PassNotAllowed; StockCount=0.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `54724c92-77a5-d0ab-ad4f-8d08f511e6a1`; duración `00:00:00.0020000`.

### PU-120. Resolver cierre por bloqueo

- **Clase:** `DominoAllFives.Server.Tests.MatchRoundTests`.
- **Caso ejecutable:** `Pass_BlockedRound_ResolvesWinnerOrTie(109,2,20)`.
- **Origen:** [MatchRoundTests.cs](../DominoAllFives.Server/Tests/MatchRoundTests.cs#L179).
- **Datos:** `scenario=109; winnerId=2; awardedPoints=20`
- **Precondiciones:** Escenario 109: jugador 1 conserva 6-6 (12 puntos), jugador 2 conserva 3-3 (6 puntos); pozo vacío; un pase final pendiente.
- **Postcondiciones:** Se prepara la ronda 2 con un mazo generado y se incrementa una vez la versión.
- **Resultado esperado:** WinnerId=2; premio=20 por suma 18; marcadores de jugadores 1 y 2: 30 y 50; ambas manos no vacías.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `80a7548b-e0ca-f03e-4676-c61da4c80675`; duración `00:00:00.0060000`.

### PU-121. Resolver cierre por bloqueo

- **Clase:** `DominoAllFives.Server.Tests.MatchRoundTests`.
- **Caso ejecutable:** `Pass_BlockedRound_ResolvesWinnerOrTie(3951,null,0)`.
- **Origen:** [MatchRoundTests.cs](../DominoAllFives.Server/Tests/MatchRoundTests.cs#L179).
- **Datos:** `scenario=3951; winnerId=null; awardedPoints=0`
- **Precondiciones:** Escenario 3951: cada jugador conserva 7 puntos en mano; pozo vacío; un pase final pendiente.
- **Postcondiciones:** Se prepara la ronda 2 con un mazo generado y se incrementa una vez la versión.
- **Resultado esperado:** WinnerId=null; premio=0; marcadores de jugadores 1 y 2: 30 y 15; ambas manos no vacías.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `493e390d-561f-3a12-a0c9-c23671cb15ad`; duración `00:00:00.0020000`.

### PU-122. Resolver última ficha y redondear premio

- **Clase:** `DominoAllFives.Server.Tests.MatchRoundTests`.
- **Caso ejecutable:** `Play_LastTile_RoundsRemainingPipsAndStartsNextRound(0,12,1,0)`.
- **Origen:** [MatchRoundTests.cs](../DominoAllFives.Server/Tests/MatchRoundTests.cs#L138).
- **Datos:** `scenario=0; precedingActions=12; remainingPips=1; awardedPoints=0`
- **Precondiciones:** Escenario 0; 12 acciones previas; una jugada final pendiente.
- **Postcondiciones:** Mano ganadora vacía en resumen; ronda 2; turno 0; versión +1; tablero nuevo; pozo 14.
- **Resultado esperado:** Pips restantes=1; premio=0; marcador anterior más premio; LastPlay de ronda 1 con RoundPoints=0.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `44b78783-d40d-5321-6719-e3b9c1eb2c08`; duración `00:00:00.0080000`.

### PU-123. Resolver última ficha y redondear premio

- **Clase:** `DominoAllFives.Server.Tests.MatchRoundTests`.
- **Caso ejecutable:** `Play_LastTile_RoundsRemainingPipsAndStartsNextRound(1,19,20,20)`.
- **Origen:** [MatchRoundTests.cs](../DominoAllFives.Server/Tests/MatchRoundTests.cs#L138).
- **Datos:** `scenario=1; precedingActions=19; remainingPips=20; awardedPoints=20`
- **Precondiciones:** Escenario 1; 19 acciones previas; una jugada final pendiente.
- **Postcondiciones:** Mano ganadora vacía en resumen; ronda 2; turno 0; versión +1; tablero nuevo; pozo 14.
- **Resultado esperado:** Pips restantes=20; premio=20; marcador anterior más premio; LastPlay de ronda 1 con RoundPoints=20.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `eef8a4c5-1605-d6d1-4fde-18f1d802ea46`; duración `00:00:00.0010000`.

### PU-124. Resolver última ficha y redondear premio

- **Clase:** `DominoAllFives.Server.Tests.MatchRoundTests`.
- **Caso ejecutable:** `Play_LastTile_RoundsRemainingPipsAndStartsNextRound(10,18,4,5)`.
- **Origen:** [MatchRoundTests.cs](../DominoAllFives.Server/Tests/MatchRoundTests.cs#L138).
- **Datos:** `scenario=10; precedingActions=18; remainingPips=4; awardedPoints=5`
- **Precondiciones:** Escenario 10; 18 acciones previas; una jugada final pendiente.
- **Postcondiciones:** Mano ganadora vacía en resumen; ronda 2; turno 0; versión +1; tablero nuevo; pozo 14.
- **Resultado esperado:** Pips restantes=4; premio=5; marcador anterior más premio; LastPlay de ronda 1 con RoundPoints=5.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `7c78c061-4132-9fb9-a240-33fa667e0f32`; duración `00:00:00.0000001`.

### PU-125. Resolver última ficha y redondear premio

- **Clase:** `DominoAllFives.Server.Tests.MatchRoundTests`.
- **Caso ejecutable:** `Play_LastTile_RoundsRemainingPipsAndStartsNextRound(2,13,53,55)`.
- **Origen:** [MatchRoundTests.cs](../DominoAllFives.Server/Tests/MatchRoundTests.cs#L138).
- **Datos:** `scenario=2; precedingActions=13; remainingPips=53; awardedPoints=55`
- **Precondiciones:** Escenario 2; 13 acciones previas; una jugada final pendiente.
- **Postcondiciones:** Mano ganadora vacía en resumen; ronda 2; turno 0; versión +1; tablero nuevo; pozo 14.
- **Resultado esperado:** Pips restantes=53; premio=55; marcador anterior más premio; LastPlay de ronda 1 con RoundPoints=55.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `739a9255-03f9-2a18-2587-c28fa16b6552`; duración `00:00:00.0010000`.

### PU-126. Resolver última ficha y redondear premio

- **Clase:** `DominoAllFives.Server.Tests.MatchRoundTests`.
- **Caso ejecutable:** `Play_LastTile_RoundsRemainingPipsAndStartsNextRound(6,12,7,5)`.
- **Origen:** [MatchRoundTests.cs](../DominoAllFives.Server/Tests/MatchRoundTests.cs#L138).
- **Datos:** `scenario=6; precedingActions=12; remainingPips=7; awardedPoints=5`
- **Precondiciones:** Escenario 6; 12 acciones previas; una jugada final pendiente.
- **Postcondiciones:** Mano ganadora vacía en resumen; ronda 2; turno 0; versión +1; tablero nuevo; pozo 14.
- **Resultado esperado:** Pips restantes=7; premio=5; marcador anterior más premio; LastPlay de ronda 1 con RoundPoints=5.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `4946d83e-437d-63d2-24b5-b97c30038cbc`; duración `00:00:00.0010000`.

### PU-127. Terminar por premio de ronda

- **Clase:** `DominoAllFives.Server.Tests.MatchRoundTests`.
- **Caso ejecutable:** `Play_RoundAwardReachesTarget_CompletesWithoutDealingAgain`.
- **Origen:** [MatchRoundTests.cs](../DominoAllFives.Server/Tests/MatchRoundTests.cs#L218).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Escenario 2 tras 13 acciones; objetivo 100; jugador 1 con 45 puntos.
- **Postcondiciones:** Completed; jugador actual null; permanece ronda 1; no se reparte otra ronda.
- **Resultado esperado:** Puntos=100; premio y RoundPoints=55; versión 14; permanece ronda 1 con la mano ganadora vacía.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `7d599889-bb2c-0a9d-bdef-8e2073f330a5`; duración `00:00:00.0020000`.

### PU-128. Caracterizar objetivo y última ficha simultáneos

- **Clase:** `DominoAllFives.Server.Tests.MatchRoundTests`.
- **Caso ejecutable:** `Play_TargetReachedOnLastTile_CompletesBeforeRoundResolution`.
- **Origen:** [MatchRoundTests.cs](../DominoAllFives.Server/Tests/MatchRoundTests.cs#L286).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Escenario 9 tras 14 acciones; puntos iniciales ajustados a 85; objetivo 100.
- **Postcondiciones:** Completed; mano vacía; ronda 1; sin nuevo reparto.
- **Resultado esperado:** Puntos=100; premio de jugada=15; RoundPoints=0; LastRound=null; versión 15. Precedencia actual pendiente de contraste con reglamento.
- **Resultado obtenido:** Aprobada como caracterización de la precedencia actual; no certifica conformidad con el reglamento externo.
- **Evidencia:** `prepared-transition-unit.trx`, caso `05376eaf-0fbd-d870-3d86-aaf981894f8a`; duración `00:00:00.0010000`.

### PU-129. Conservar código de rechazo de negocio

- **Clase:** `DominoAllFives.Server.Tests.MatchRuleExceptionTests`.
- **Caso ejecutable:** `Constructor_DefinedIssue_PreservesIssue(InvalidPlacement)`.
- **Origen:** [MatchRuleExceptionTests.cs](../DominoAllFives.Server/Tests/MatchRuleExceptionTests.cs#L13).
- **Datos:** `issue=InvalidPlacement`
- **Precondiciones:** Valor definido del enum de la excepción: InvalidPlacement.
- **Postcondiciones:** Excepción construida con Issue inmutable.
- **Resultado esperado:** Issue igual a InvalidPlacement.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `e3c06431-be10-c164-b99c-bf5a26261a66`; duración `00:00:00.0000001`.

### PU-130. Conservar código de rechazo de negocio

- **Clase:** `DominoAllFives.Server.Tests.MatchRuleExceptionTests`.
- **Caso ejecutable:** `Constructor_DefinedIssue_PreservesIssue(MatchNotFound)`.
- **Origen:** [MatchRuleExceptionTests.cs](../DominoAllFives.Server/Tests/MatchRuleExceptionTests.cs#L13).
- **Datos:** `issue=MatchNotFound`
- **Precondiciones:** Valor definido del enum de la excepción: MatchNotFound.
- **Postcondiciones:** Excepción construida con Issue inmutable.
- **Resultado esperado:** Issue igual a MatchNotFound.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `8e9ba50b-784b-5bd8-5c0e-5e65a986e253`; duración `00:00:00.0030000`.

### PU-131. Conservar código de rechazo de negocio

- **Clase:** `DominoAllFives.Server.Tests.MatchRuleExceptionTests`.
- **Caso ejecutable:** `Constructor_DefinedIssue_PreservesIssue(StaleVersion)`.
- **Origen:** [MatchRuleExceptionTests.cs](../DominoAllFives.Server/Tests/MatchRuleExceptionTests.cs#L13).
- **Datos:** `issue=StaleVersion`
- **Precondiciones:** Valor definido del enum de la excepción: StaleVersion.
- **Postcondiciones:** Excepción construida con Issue inmutable.
- **Resultado esperado:** Issue igual a StaleVersion.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `a50e3591-ef9e-e959-2e50-dc644010e3e7`; duración `00:00:00.0000001`.

### PU-132. Rechazar código de negocio indefinido

- **Clase:** `DominoAllFives.Server.Tests.MatchRuleExceptionTests`.
- **Caso ejecutable:** `Constructor_UndefinedIssue_ThrowsArgumentOutOfRangeException`.
- **Origen:** [MatchRuleExceptionTests.cs](../DominoAllFives.Server/Tests/MatchRuleExceptionTests.cs#L23).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Enum convertido desde int.MaxValue.
- **Postcondiciones:** No se construye una excepción de negocio válida.
- **Resultado esperado:** ArgumentOutOfRangeException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `14454109-ee36-c16a-faef-b0425e62dbd2`; duración `00:00:00.0000001`.

### PU-133. Rechazar operación inválida de partida

- **Clase:** `DominoAllFives.Server.Tests.MatchTurnTests`.
- **Caso ejecutable:** `Operation_InvalidContext_PreservesState("Cancel",PlayerNotInMatch)`.
- **Origen:** [MatchTurnTests.cs](../DominoAllFives.Server/Tests/MatchTurnTests.cs#L28).
- **Datos:** `operation="Cancel"; issue=PlayerNotInMatch`
- **Precondiciones:** playerId=99, ajeno a la partida; operación="Cancel"; mazo fijo.
- **Postcondiciones:** Se conservan versión, fase, turno, ronda, pozo, tablero, resultados, fichas y puntos.
- **Resultado esperado:** MatchRuleException con Issue=PlayerNotInMatch.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `2a9e26de-70d9-f3dc-cd03-eea28fd3da6a`; duración `00:00:00.0000001`.

### PU-134. Rechazar operación inválida de partida

- **Clase:** `DominoAllFives.Server.Tests.MatchTurnTests`.
- **Caso ejecutable:** `Operation_InvalidContext_PreservesState("Draw",DrawNotAllowed)`.
- **Origen:** [MatchTurnTests.cs](../DominoAllFives.Server/Tests/MatchTurnTests.cs#L28).
- **Datos:** `operation="Draw"; issue=DrawNotAllowed`
- **Precondiciones:** Mano con jugada disponible; operación="Draw"; mazo fijo.
- **Postcondiciones:** Se conservan versión, fase, turno, ronda, pozo, tablero, resultados, fichas y puntos.
- **Resultado esperado:** MatchRuleException con Issue=DrawNotAllowed.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `67d9ee8a-a297-4cb0-8880-f64842ac81e3`; duración `00:00:00.0000001`.

### PU-135. Rechazar operación inválida de partida

- **Clase:** `DominoAllFives.Server.Tests.MatchTurnTests`.
- **Caso ejecutable:** `Operation_InvalidContext_PreservesState("Draw",MatchNotInProgress)`.
- **Origen:** [MatchTurnTests.cs](../DominoAllFives.Server/Tests/MatchTurnTests.cs#L28).
- **Datos:** `operation="Draw"; issue=MatchNotInProgress`
- **Precondiciones:** Partida cancelada previamente; operación="Draw"; mazo fijo.
- **Postcondiciones:** Se conservan versión, fase, turno, ronda, pozo, tablero, resultados, fichas y puntos.
- **Resultado esperado:** MatchRuleException con Issue=MatchNotInProgress.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `6a1473fb-6857-5432-5e10-661bb1d42754`; duración `00:00:00.0000001`.

### PU-136. Rechazar operación inválida de partida

- **Clase:** `DominoAllFives.Server.Tests.MatchTurnTests`.
- **Caso ejecutable:** `Operation_InvalidContext_PreservesState("Draw",NotPlayersTurn)`.
- **Origen:** [MatchTurnTests.cs](../DominoAllFives.Server/Tests/MatchTurnTests.cs#L28).
- **Datos:** `operation="Draw"; issue=NotPlayersTurn`
- **Precondiciones:** playerId=2 cuando corresponde al jugador 1; operación="Draw"; mazo fijo.
- **Postcondiciones:** Se conservan versión, fase, turno, ronda, pozo, tablero, resultados, fichas y puntos.
- **Resultado esperado:** MatchRuleException con Issue=NotPlayersTurn.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `b47f5997-5994-b3c0-bad5-e88fa97b026b`; duración `00:00:00.0000001`.

### PU-137. Rechazar operación inválida de partida

- **Clase:** `DominoAllFives.Server.Tests.MatchTurnTests`.
- **Caso ejecutable:** `Operation_InvalidContext_PreservesState("Draw",PlayerNotInMatch)`.
- **Origen:** [MatchTurnTests.cs](../DominoAllFives.Server/Tests/MatchTurnTests.cs#L28).
- **Datos:** `operation="Draw"; issue=PlayerNotInMatch`
- **Precondiciones:** playerId=99, ajeno a la partida; operación="Draw"; mazo fijo.
- **Postcondiciones:** Se conservan versión, fase, turno, ronda, pozo, tablero, resultados, fichas y puntos.
- **Resultado esperado:** MatchRuleException con Issue=PlayerNotInMatch.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `538faea2-55f5-2081-5272-89140daf31da`; duración `00:00:00.0000001`.

### PU-138. Rechazar operación inválida de partida

- **Clase:** `DominoAllFives.Server.Tests.MatchTurnTests`.
- **Caso ejecutable:** `Operation_InvalidContext_PreservesState("Draw",StaleVersion)`.
- **Origen:** [MatchTurnTests.cs](../DominoAllFives.Server/Tests/MatchTurnTests.cs#L28).
- **Datos:** `operation="Draw"; issue=StaleVersion`
- **Precondiciones:** Versión solicitada anterior a la versión actual; operación="Draw"; mazo fijo.
- **Postcondiciones:** Se conservan versión, fase, turno, ronda, pozo, tablero, resultados, fichas y puntos.
- **Resultado esperado:** MatchRuleException con Issue=StaleVersion.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `623364a4-dfe3-6956-3b13-30a6bfaadf38`; duración `00:00:00.0000001`.

### PU-139. Rechazar operación inválida de partida

- **Clase:** `DominoAllFives.Server.Tests.MatchTurnTests`.
- **Caso ejecutable:** `Operation_InvalidContext_PreservesState("Pass",MatchNotInProgress)`.
- **Origen:** [MatchTurnTests.cs](../DominoAllFives.Server/Tests/MatchTurnTests.cs#L28).
- **Datos:** `operation="Pass"; issue=MatchNotInProgress`
- **Precondiciones:** Partida cancelada previamente; operación="Pass"; mazo fijo.
- **Postcondiciones:** Se conservan versión, fase, turno, ronda, pozo, tablero, resultados, fichas y puntos.
- **Resultado esperado:** MatchRuleException con Issue=MatchNotInProgress.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `3f847e70-1359-5af4-67dd-0dff6a777941`; duración `00:00:00.0000001`.

### PU-140. Rechazar operación inválida de partida

- **Clase:** `DominoAllFives.Server.Tests.MatchTurnTests`.
- **Caso ejecutable:** `Operation_InvalidContext_PreservesState("Pass",NotPlayersTurn)`.
- **Origen:** [MatchTurnTests.cs](../DominoAllFives.Server/Tests/MatchTurnTests.cs#L28).
- **Datos:** `operation="Pass"; issue=NotPlayersTurn`
- **Precondiciones:** playerId=2 cuando corresponde al jugador 1; operación="Pass"; mazo fijo.
- **Postcondiciones:** Se conservan versión, fase, turno, ronda, pozo, tablero, resultados, fichas y puntos.
- **Resultado esperado:** MatchRuleException con Issue=NotPlayersTurn.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `9154d326-7b8f-2c5f-64f9-4a5cef963bd2`; duración `00:00:00.0000001`.

### PU-141. Rechazar operación inválida de partida

- **Clase:** `DominoAllFives.Server.Tests.MatchTurnTests`.
- **Caso ejecutable:** `Operation_InvalidContext_PreservesState("Pass",PassNotAllowed)`.
- **Origen:** [MatchTurnTests.cs](../DominoAllFives.Server/Tests/MatchTurnTests.cs#L28).
- **Datos:** `operation="Pass"; issue=PassNotAllowed`
- **Precondiciones:** Pozo todavía disponible; operación="Pass"; mazo fijo.
- **Postcondiciones:** Se conservan versión, fase, turno, ronda, pozo, tablero, resultados, fichas y puntos.
- **Resultado esperado:** MatchRuleException con Issue=PassNotAllowed.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `48f3385b-e8cb-b33c-4840-d5cf585ba391`; duración `00:00:00.0000001`.

### PU-142. Rechazar operación inválida de partida

- **Clase:** `DominoAllFives.Server.Tests.MatchTurnTests`.
- **Caso ejecutable:** `Operation_InvalidContext_PreservesState("Pass",PlayerNotInMatch)`.
- **Origen:** [MatchTurnTests.cs](../DominoAllFives.Server/Tests/MatchTurnTests.cs#L28).
- **Datos:** `operation="Pass"; issue=PlayerNotInMatch`
- **Precondiciones:** playerId=99, ajeno a la partida; operación="Pass"; mazo fijo.
- **Postcondiciones:** Se conservan versión, fase, turno, ronda, pozo, tablero, resultados, fichas y puntos.
- **Resultado esperado:** MatchRuleException con Issue=PlayerNotInMatch.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `cd42f781-c2b5-7900-58a0-12d45c11ee29`; duración `00:00:00.0000001`.

### PU-143. Rechazar operación inválida de partida

- **Clase:** `DominoAllFives.Server.Tests.MatchTurnTests`.
- **Caso ejecutable:** `Operation_InvalidContext_PreservesState("Pass",StaleVersion)`.
- **Origen:** [MatchTurnTests.cs](../DominoAllFives.Server/Tests/MatchTurnTests.cs#L28).
- **Datos:** `operation="Pass"; issue=StaleVersion`
- **Precondiciones:** Versión solicitada anterior a la versión actual; operación="Pass"; mazo fijo.
- **Postcondiciones:** Se conservan versión, fase, turno, ronda, pozo, tablero, resultados, fichas y puntos.
- **Resultado esperado:** MatchRuleException con Issue=StaleVersion.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `1cfbd051-d731-2652-6c5d-fc07203de0b4`; duración `00:00:00.0000001`.

### PU-144. Rechazar operación inválida de partida

- **Clase:** `DominoAllFives.Server.Tests.MatchTurnTests`.
- **Caso ejecutable:** `Operation_InvalidContext_PreservesState("Play",InvalidPlacement)`.
- **Origen:** [MatchTurnTests.cs](../DominoAllFives.Server/Tests/MatchTurnTests.cs#L28).
- **Datos:** `operation="Play"; issue=InvalidPlacement`
- **Precondiciones:** Extremo convertido desde 99; operación="Play"; mazo fijo.
- **Postcondiciones:** Se conservan versión, fase, turno, ronda, pozo, tablero, resultados, fichas y puntos.
- **Resultado esperado:** MatchRuleException con Issue=InvalidPlacement.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `880975fd-b616-6819-45e3-45f2095a2e91`; duración `00:00:00.0000001`.

### PU-145. Rechazar operación inválida de partida

- **Clase:** `DominoAllFives.Server.Tests.MatchTurnTests`.
- **Caso ejecutable:** `Operation_InvalidContext_PreservesState("Play",MatchNotInProgress)`.
- **Origen:** [MatchTurnTests.cs](../DominoAllFives.Server/Tests/MatchTurnTests.cs#L28).
- **Datos:** `operation="Play"; issue=MatchNotInProgress`
- **Precondiciones:** Partida cancelada previamente; operación="Play"; mazo fijo.
- **Postcondiciones:** Se conservan versión, fase, turno, ronda, pozo, tablero, resultados, fichas y puntos.
- **Resultado esperado:** MatchRuleException con Issue=MatchNotInProgress.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `4585c29a-661a-f26c-9dc8-54a757397509`; duración `00:00:00.0000001`.

### PU-146. Rechazar operación inválida de partida

- **Clase:** `DominoAllFives.Server.Tests.MatchTurnTests`.
- **Caso ejecutable:** `Operation_InvalidContext_PreservesState("Play",NotPlayersTurn)`.
- **Origen:** [MatchTurnTests.cs](../DominoAllFives.Server/Tests/MatchTurnTests.cs#L28).
- **Datos:** `operation="Play"; issue=NotPlayersTurn`
- **Precondiciones:** playerId=2 cuando corresponde al jugador 1; operación="Play"; mazo fijo.
- **Postcondiciones:** Se conservan versión, fase, turno, ronda, pozo, tablero, resultados, fichas y puntos.
- **Resultado esperado:** MatchRuleException con Issue=NotPlayersTurn.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `8bece964-210d-09bc-6022-a15c235538c1`; duración `00:00:00.0000001`.

### PU-147. Rechazar operación inválida de partida

- **Clase:** `DominoAllFives.Server.Tests.MatchTurnTests`.
- **Caso ejecutable:** `Operation_InvalidContext_PreservesState("Play",OpeningTileRequired)`.
- **Origen:** [MatchTurnTests.cs](../DominoAllFives.Server/Tests/MatchTurnTests.cs#L28).
- **Datos:** `operation="Play"; issue=OpeningTileRequired`
- **Precondiciones:** Se solicita la segunda ficha de la mano en lugar de la salida obligatoria; operación="Play"; mazo fijo.
- **Postcondiciones:** Se conservan versión, fase, turno, ronda, pozo, tablero, resultados, fichas y puntos.
- **Resultado esperado:** MatchRuleException con Issue=OpeningTileRequired.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `6ee15662-73ec-052d-f30f-bd88483443e6`; duración `00:00:00.0000001`.

### PU-148. Rechazar operación inválida de partida

- **Clase:** `DominoAllFives.Server.Tests.MatchTurnTests`.
- **Caso ejecutable:** `Operation_InvalidContext_PreservesState("Play",PlayerNotInMatch)`.
- **Origen:** [MatchTurnTests.cs](../DominoAllFives.Server/Tests/MatchTurnTests.cs#L28).
- **Datos:** `operation="Play"; issue=PlayerNotInMatch`
- **Precondiciones:** playerId=99, ajeno a la partida; operación="Play"; mazo fijo.
- **Postcondiciones:** Se conservan versión, fase, turno, ronda, pozo, tablero, resultados, fichas y puntos.
- **Resultado esperado:** MatchRuleException con Issue=PlayerNotInMatch.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `5d31feab-4f4d-a00c-f7d9-041fbc3c8551`; duración `00:00:00.0030000`.

### PU-149. Rechazar operación inválida de partida

- **Clase:** `DominoAllFives.Server.Tests.MatchTurnTests`.
- **Caso ejecutable:** `Operation_InvalidContext_PreservesState("Play",StaleVersion)`.
- **Origen:** [MatchTurnTests.cs](../DominoAllFives.Server/Tests/MatchTurnTests.cs#L28).
- **Datos:** `operation="Play"; issue=StaleVersion`
- **Precondiciones:** Versión solicitada anterior a la versión actual; operación="Play"; mazo fijo.
- **Postcondiciones:** Se conservan versión, fase, turno, ronda, pozo, tablero, resultados, fichas y puntos.
- **Resultado esperado:** MatchRuleException con Issue=StaleVersion.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `fe5e1087-7b0d-44e1-f08e-c4dd89d6858a`; duración `00:00:00.0000001`.

### PU-150. Rechazar operación inválida de partida

- **Clase:** `DominoAllFives.Server.Tests.MatchTurnTests`.
- **Caso ejecutable:** `Operation_InvalidContext_PreservesState("Play",TileNotOwned)`.
- **Origen:** [MatchTurnTests.cs](../DominoAllFives.Server/Tests/MatchTurnTests.cs#L28).
- **Datos:** `operation="Play"; issue=TileNotOwned`
- **Precondiciones:** TileId=99, ausente de la mano; operación="Play"; mazo fijo.
- **Postcondiciones:** Se conservan versión, fase, turno, ronda, pozo, tablero, resultados, fichas y puntos.
- **Resultado esperado:** MatchRuleException con Issue=TileNotOwned.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `90d4a170-d806-08b6-6945-ef020d494812`; duración `00:00:00.0000001`.

### PU-151. Rechazar contraseña vacía para hashing

- **Clase:** `DominoAllFives.Server.Tests.PasswordHasherTests`.
- **Caso ejecutable:** `HashPassword_BlankPassword_ThrowsArgumentException("   ")`.
- **Origen:** [PasswordHasherTests.cs](../DominoAllFives.Server/Tests/PasswordHasherTests.cs#L62).
- **Datos:** `password="   "`
- **Precondiciones:** password="   "; dato ficticio.
- **Postcondiciones:** No se genera hash válido.
- **Resultado esperado:** ArgumentException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `eab44813-4fb7-46e0-9c95-5e39c4de22d5`; duración `00:00:00.0000001`.

### PU-152. Rechazar contraseña vacía para hashing

- **Clase:** `DominoAllFives.Server.Tests.PasswordHasherTests`.
- **Caso ejecutable:** `HashPassword_BlankPassword_ThrowsArgumentException("")`.
- **Origen:** [PasswordHasherTests.cs](../DominoAllFives.Server/Tests/PasswordHasherTests.cs#L62).
- **Datos:** `password=""`
- **Precondiciones:** password=""; dato ficticio.
- **Postcondiciones:** No se genera hash válido.
- **Resultado esperado:** ArgumentException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `0ee8279c-dac4-4cfb-f400-63ac9a1e7976`; duración `00:00:00.0020000`.

### PU-153. Rechazar contraseña null para hashing

- **Clase:** `DominoAllFives.Server.Tests.PasswordHasherTests`.
- **Caso ejecutable:** `HashPassword_NullPassword_ThrowsArgumentNullException`.
- **Origen:** [PasswordHasherTests.cs](../DominoAllFives.Server/Tests/PasswordHasherTests.cs#L50).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Contraseña null.
- **Postcondiciones:** No se genera hash válido.
- **Resultado esperado:** ArgumentNullException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `014f01fe-29b6-da32-c683-b56725efffd3`; duración `00:00:00.0000001`.

### PU-154. Rechazar contraseña distinta

- **Clase:** `DominoAllFives.Server.Tests.PasswordHasherTests`.
- **Caso ejecutable:** `VerifyPassword_DifferentPassword_ReturnsFalse`.
- **Origen:** [PasswordHasherTests.cs](../DominoAllFives.Server/Tests/PasswordHasherTests.cs#L22).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Hash de contraseña ficticia; verificación con otra contraseña ficticia.
- **Postcondiciones:** Hash evaluado en memoria.
- **Resultado esperado:** false.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `5dbcb826-7c64-ddbf-d6d1-e6b45cfa8b70`; duración `00:00:01.1530000`.

### PU-155. Rechazar formato de hash inválido

- **Clase:** `DominoAllFives.Server.Tests.PasswordHasherTests`.
- **Caso ejecutable:** `VerifyPassword_InvalidStoredHash_ReturnsFalse("   ")`.
- **Origen:** [PasswordHasherTests.cs](../DominoAllFives.Server/Tests/PasswordHasherTests.cs#L39).
- **Datos:** `storedHash="   "`
- **Precondiciones:** storedHash="   "; datos sintéticos.
- **Postcondiciones:** Verificación rechazada sin excepción técnica.
- **Resultado esperado:** false.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `b79355e9-e4e7-6c65-feec-bcb743aa7ee2`; duración `00:00:00.0000001`.

### PU-156. Rechazar formato de hash inválido

- **Clase:** `DominoAllFives.Server.Tests.PasswordHasherTests`.
- **Caso ejecutable:** `VerifyPassword_InvalidStoredHash_ReturnsFalse("")`.
- **Origen:** [PasswordHasherTests.cs](../DominoAllFives.Server/Tests/PasswordHasherTests.cs#L39).
- **Datos:** `storedHash=""`
- **Precondiciones:** storedHash=""; datos sintéticos.
- **Postcondiciones:** Verificación rechazada sin excepción técnica.
- **Resultado esperado:** false.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `45b436b5-be54-2f4f-5873-46ba7a70db33`; duración `00:00:00.0000001`.

### PU-157. Rechazar formato de hash inválido

- **Clase:** `DominoAllFives.Server.Tests.PasswordHasherTests`.
- **Caso ejecutable:** `VerifyPassword_InvalidStoredHash_ReturnsFalse("invalid")`.
- **Origen:** [PasswordHasherTests.cs](../DominoAllFives.Server/Tests/PasswordHasherTests.cs#L39).
- **Datos:** `storedHash="invalid"`
- **Precondiciones:** storedHash="invalid"; datos sintéticos.
- **Postcondiciones:** Verificación rechazada sin excepción técnica.
- **Resultado esperado:** false.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `ef1039ce-3a07-bf20-b983-c90e662ee643`; duración `00:00:00.0000001`.

### PU-158. Rechazar formato de hash inválido

- **Clase:** `DominoAllFives.Server.Tests.PasswordHasherTests`.
- **Caso ejecutable:** `VerifyPassword_InvalidStoredHash_ReturnsFalse("OTHER$100000$c2FsdA==$aGFzaA==")`.
- **Origen:** [PasswordHasherTests.cs](../DominoAllFives.Server/Tests/PasswordHasherTests.cs#L39).
- **Datos:** `storedHash="OTHER$100000$c2FsdA==$aGFzaA=="`
- **Precondiciones:** storedHash="OTHER$100000$c2FsdA==$aGFzaA=="; datos sintéticos.
- **Postcondiciones:** Verificación rechazada sin excepción técnica.
- **Resultado esperado:** false.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `67d84f05-2efe-62e6-f9da-9f8c107bc53d`; duración `00:00:00.0000001`.

### PU-159. Rechazar formato de hash inválido

- **Clase:** `DominoAllFives.Server.Tests.PasswordHasherTests`.
- **Caso ejecutable:** `VerifyPassword_InvalidStoredHash_ReturnsFalse("PBKDF2-SHA256$100000$invalid$aGFzaA==")`.
- **Origen:** [PasswordHasherTests.cs](../DominoAllFives.Server/Tests/PasswordHasherTests.cs#L39).
- **Datos:** `storedHash="PBKDF2-SHA256$100000$invalid$aGFzaA=="`
- **Precondiciones:** storedHash="PBKDF2-SHA256$100000$invalid$aGFzaA=="; datos sintéticos.
- **Postcondiciones:** Verificación rechazada sin excepción técnica.
- **Resultado esperado:** false.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `e0aa1ee8-fcad-3272-9909-9fdaa967b319`; duración `00:00:00.0000001`.

### PU-160. Rechazar formato de hash inválido

- **Clase:** `DominoAllFives.Server.Tests.PasswordHasherTests`.
- **Caso ejecutable:** `VerifyPassword_InvalidStoredHash_ReturnsFalse("PBKDF2-SHA256$invalid$c2FsdA==$aGFzaA==")`.
- **Origen:** [PasswordHasherTests.cs](../DominoAllFives.Server/Tests/PasswordHasherTests.cs#L39).
- **Datos:** `storedHash="PBKDF2-SHA256$invalid$c2FsdA==$aGFzaA=="`
- **Precondiciones:** storedHash="PBKDF2-SHA256$invalid$c2FsdA==$aGFzaA=="; datos sintéticos.
- **Postcondiciones:** Verificación rechazada sin excepción técnica.
- **Resultado esperado:** false.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `ba5daa4c-071b-f666-3a44-41e7066b13c3`; duración `00:00:00.0000001`.

### PU-161. Rechazar formato de hash inválido

- **Clase:** `DominoAllFives.Server.Tests.PasswordHasherTests`.
- **Caso ejecutable:** `VerifyPassword_InvalidStoredHash_ReturnsFalse(null)`.
- **Origen:** [PasswordHasherTests.cs](../DominoAllFives.Server/Tests/PasswordHasherTests.cs#L39).
- **Datos:** `storedHash=null`
- **Precondiciones:** storedHash=null; datos sintéticos.
- **Postcondiciones:** Verificación rechazada sin excepción técnica.
- **Resultado esperado:** false.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `62cc0256-7978-570d-dbb7-08482716444c`; duración `00:00:00.0000001`.

### PU-162. Validar contraseña coincidente

- **Clase:** `DominoAllFives.Server.Tests.PasswordHasherTests`.
- **Caso ejecutable:** `VerifyPassword_MatchingPassword_ReturnsTrue`.
- **Origen:** [PasswordHasherTests.cs](../DominoAllFives.Server/Tests/PasswordHasherTests.cs#L11).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Hash recién generado de contraseña ficticia.
- **Postcondiciones:** Hash evaluado en memoria.
- **Resultado esperado:** true.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `0f54d8f0-8065-5954-7d80-4a1803e4a0f5`; duración `00:00:01.1180000`.

### PU-163. Rechazar estado de acceso null

- **Clase:** `DominoAllFives.Server.Tests.PlayerAccountTests`.
- **Caso ejecutable:** `FromStoredAccount_NullAccessStatus_ThrowsArgumentNullException`.
- **Origen:** [PlayerAccountTests.cs](../DominoAllFives.Server/Tests/PlayerAccountTests.cs#L38).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Identidad válida; accessStatus null.
- **Postcondiciones:** Cuenta no construida.
- **Resultado esperado:** ArgumentNullException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `15304f95-db1a-a547-4657-6ad31a11b6ef`; duración `00:00:00.0010000`.

### PU-164. Construir identidad de cuenta

- **Clase:** `DominoAllFives.Server.Tests.PlayerAccountTests`.
- **Caso ejecutable:** `FromStoredAccount_ValidValues_PreservesAccountIdentity`.
- **Origen:** [PlayerAccountTests.cs](../DominoAllFives.Server/Tests/PlayerAccountTests.cs#L12).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** ID 7; nombre player; idioma es-MX; estado de acceso válido.
- **Postcondiciones:** Cuenta creada.
- **Resultado esperado:** Conserva ID, nombre, idioma y referencia al estado.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `161a0e70-d7f7-ad72-aec5-efd1766edbb9`; duración `00:00:00.0010000`.

### PU-165. Rechazar idioma demasiado largo

- **Clase:** `DominoAllFives.Server.Tests.PlayerAccountTests`.
- **Caso ejecutable:** `SetPreferredLanguage_ExcessiveLength_ThrowsArgumentException`.
- **Origen:** [PlayerAccountTests.cs](../DominoAllFives.Server/Tests/PlayerAccountTests.cs#L64).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Cuenta válida; valor language-id.
- **Postcondiciones:** Asignación rechazada.
- **Resultado esperado:** ArgumentException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `c802ae3e-3fd6-501d-5d4b-28822e646c7a`; duración `00:00:00.0010000`.

### PU-166. Rechazar idioma null

- **Clase:** `DominoAllFives.Server.Tests.PlayerAccountTests`.
- **Caso ejecutable:** `SetPreferredLanguage_NullLanguage_ThrowsArgumentNullException`.
- **Origen:** [PlayerAccountTests.cs](../DominoAllFives.Server/Tests/PlayerAccountTests.cs#L53).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Cuenta válida.
- **Postcondiciones:** Asignación rechazada.
- **Resultado esperado:** ArgumentNullException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `be8a15fc-a5cd-faa1-46fa-26635cbe1f59`; duración `00:00:00.0000001`.

### PU-167. Rechazar clave de sesión vacía

- **Clase:** `DominoAllFives.Server.Tests.PlayerAccountTests`.
- **Caso ejecutable:** `SetSessionKey_BlankKey_ThrowsArgumentException("   ")`.
- **Origen:** [PlayerAccountTests.cs](../DominoAllFives.Server/Tests/PlayerAccountTests.cs#L87).
- **Datos:** `sessionKey="   "`
- **Precondiciones:** Cuenta válida; sessionKey="   "; dato ficticio.
- **Postcondiciones:** Asignación rechazada.
- **Resultado esperado:** ArgumentException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `98ae046a-60a8-1341-2cd4-de9e406535fa`; duración `00:00:00.0000001`.

### PU-168. Rechazar clave de sesión vacía

- **Clase:** `DominoAllFives.Server.Tests.PlayerAccountTests`.
- **Caso ejecutable:** `SetSessionKey_BlankKey_ThrowsArgumentException("")`.
- **Origen:** [PlayerAccountTests.cs](../DominoAllFives.Server/Tests/PlayerAccountTests.cs#L87).
- **Datos:** `sessionKey=""`
- **Precondiciones:** Cuenta válida; sessionKey=""; dato ficticio.
- **Postcondiciones:** Asignación rechazada.
- **Resultado esperado:** ArgumentException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `3dcc85a0-6b8e-33d1-7f2d-6e8e96cc8f9a`; duración `00:00:00.0010000`.

### PU-169. Rechazar clave de sesión null

- **Clase:** `DominoAllFives.Server.Tests.PlayerAccountTests`.
- **Caso ejecutable:** `SetSessionKey_NullKey_ThrowsArgumentNullException`.
- **Origen:** [PlayerAccountTests.cs](../DominoAllFives.Server/Tests/PlayerAccountTests.cs#L75).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Cuenta válida.
- **Postcondiciones:** Asignación rechazada.
- **Resultado esperado:** ArgumentNullException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `6f6b0d17-7e3b-bdf8-610c-9c7f807d7bdc`; duración `00:00:00.0000001`.

### PU-170. Conservar clave asignada

- **Clase:** `DominoAllFives.Server.Tests.PlayerAccountTests`.
- **Caso ejecutable:** `SetSessionKey_ValidKey_PreservesKey`.
- **Origen:** [PlayerAccountTests.cs](../DominoAllFives.Server/Tests/PlayerAccountTests.cs#L98).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Cuenta válida; clave ficticia session-key.
- **Postcondiciones:** Clave asignada.
- **Resultado esperado:** SessionKey igual al valor de prueba.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `7799566e-8ca1-262f-a797-979486a3dfa1`; duración `00:00:00.0000001`.

### PU-171. Rechazar contexto del repositorio null

- **Clase:** `DominoAllFives.Server.Tests.PlayerRepositoryTests`.
- **Caso ejecutable:** `Constructor_NullContext_ThrowsArgumentNullException`.
- **Origen:** [PlayerRepositoryTests.cs](../DominoAllFives.Server/Tests/PlayerRepositoryTests.cs#L12).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** PlayerRepository recibe contexto null; no se abre conexión.
- **Postcondiciones:** Repositorio no construido.
- **Resultado esperado:** ArgumentNullException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `7ef33c71-1ada-f0b5-0ddd-21bef6db0212`; duración `00:00:00.0010000`.

### PU-172. Proteger explicación de puntuación

- **Clase:** `DominoAllFives.Server.Tests.PlayScoreTests`.
- **Caso ejecutable:** `CreateView_ChangedBreakdown_DoesNotMutateAuthoritativeValues`.
- **Origen:** [PlayScoreTests.cs](../DominoAllFives.Server/Tests/PlayScoreTests.cs#L149).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Jugada realizada; vista creada; se vacían EndValues y se cambia FirstValue en copia.
- **Postcondiciones:** Estado autorizado conservado.
- **Resultado esperado:** EndValues del dominio contiene 12; ficha conserva 6; puntos de jugada 0.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `2af3cc3d-54b2-3a49-6659-df74afbfb1f6`; duración `00:00:00.0090000`.

### PU-173. Conservar explicación durante robo

- **Clase:** `DominoAllFives.Server.Tests.PlayScoreTests`.
- **Caso ejecutable:** `Draw_AfterPlacement_PreservesLastPlayExplanation`.
- **Origen:** [PlayScoreTests.cs](../DominoAllFives.Server/Tests/PlayScoreTests.cs#L170).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** 6-6 colocada; siguiente jugador sin jugada; LastPlay capturado.
- **Postcondiciones:** Robo realizado.
- **Resultado esperado:** LastPlay conserva la misma referencia.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `45d3c3a9-2a2c-06c3-e84e-9eb0898b6018`; duración `00:00:00.0030000`.

### PU-174. Explicar puntuación de jugada morada

- **Clase:** `DominoAllFives.Server.Tests.PlayScoreTests`.
- **Caso ejecutable:** `Play_PurpleBlock_RecordsScoringTurnInsteadOfNextTurn`.
- **Origen:** [PlayScoreTests.cs](../DominoAllFives.Server/Tests/PlayScoreTests.cs#L116).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Secuencia fija con 6-6, 0-6 y 1-6; jugar 0-3 morada.
- **Postcondiciones:** Explicación conserva el turno de la jugada; vista representa el turno siguiente.
- **Resultado esperado:** Extremo bloqueado explicado=Right; valores 3 y 12; premio 15; Right ya no bloqueado en vista siguiente.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `57699c82-ab2b-c03e-5bb7-3e8635f7c3af`; duración `00:00:00.0140000`.

### PU-175. Conservar premio rojo en proyección

- **Clase:** `DominoAllFives.Server.Tests.PlayScoreTests`.
- **Caso ejecutable:** `Play_RedWithOpeningDouble_ReportsExactAwardAndSurvivesContractMapping`.
- **Origen:** [PlayScoreTests.cs](../DominoAllFives.Server/Tests/PlayScoreTests.cs#L83).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Secuencia fija; 6-6 inicial aún aporta; 4-5 roja pendiente.
- **Postcondiciones:** Premio de dominio y explicación contractual coinciden.
- **Resultado esperado:** Valores 3,5,12; total 20; doble inicial incluido; multiplicador 2; premio y aumento 40; cero extremos bloqueados.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `f032396b-e356-7507-0452-b9bd7319772e`; duración `00:00:00.0020000`.

### PU-176. Rechazar almacenamiento null

- **Clase:** `DominoAllFives.Server.Tests.RankingControllerTests`.
- **Caso ejecutable:** `Constructor_NullStorage_ThrowsArgumentNullException`.
- **Origen:** [RankingControllerTests.cs](../DominoAllFives.Server/Tests/RankingControllerTests.cs#L31).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** RankingController recibe storage null.
- **Postcondiciones:** Controller no construido.
- **Resultado esperado:** ArgumentNullException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `b63ab3f6-0a46-2ec4-3ca8-4dcec6af979b`; duración `00:00:00.0010000`.

### PU-177. Construir ranking vacío

- **Clase:** `DominoAllFives.Server.Tests.RankingControllerTests`.
- **Caso ejecutable:** `FromOrderedEntries_EmptyRanking_PreservesAbsentPosition`.
- **Origen:** [RankingControllerTests.cs](../DominoAllFives.Server/Tests/RankingControllerTests.cs#L65).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Lista vacía y posición cero.
- **Postcondiciones:** Snapshot construido.
- **Resultado esperado:** Cero entradas y posición actual 0.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `2b50e8aa-bd3c-346b-06bd-b6b219a524f5`; duración `00:00:00.0010000`.

### PU-178. Rechazar colección de ranking null

- **Clase:** `DominoAllFives.Server.Tests.RankingControllerTests`.
- **Caso ejecutable:** `FromOrderedEntries_NullEntries_ThrowsArgumentNullException`.
- **Origen:** [RankingControllerTests.cs](../DominoAllFives.Server/Tests/RankingControllerTests.cs#L91).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** entries null; posición 0.
- **Postcondiciones:** Ranking no construido.
- **Resultado esperado:** ArgumentNullException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `3fb2b1fe-5ff3-b215-5286-46cf97f2cfe5`; duración `00:00:00.0000001`.

### PU-179. Conservar snapshot de ranking

- **Clase:** `DominoAllFives.Server.Tests.RankingControllerTests`.
- **Caso ejecutable:** `FromOrderedEntries_SourceChanges_PreservesSnapshot`.
- **Origen:** [RankingControllerTests.cs](../DominoAllFives.Server/Tests/RankingControllerTests.cs#L77).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Ranking con una entrada; se vacía lista de origen.
- **Postcondiciones:** Snapshot conserva su entrada.
- **Resultado esperado:** TopPlayers contiene la entrada original.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `903800f9-5f6c-c31b-5e59-b2e20ebd77b2`; duración `00:00:00.0010000`.

### PU-180. Rechazar imagen null de ranking

- **Clase:** `DominoAllFives.Server.Tests.RankingControllerTests`.
- **Caso ejecutable:** `FromRankedPlayer_NullPicture_ThrowsArgumentNullException`.
- **Origen:** [RankingControllerTests.cs](../DominoAllFives.Server/Tests/RankingControllerTests.cs#L54).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Rango 1; nombre y victorias válidos; imagen null.
- **Postcondiciones:** Entrada no construida.
- **Resultado esperado:** ArgumentNullException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `4618a915-a60d-8fa0-88ed-8fb9f45b5a42`; duración `00:00:00.0000001`.

### PU-181. Construir entrada de ranking

- **Clase:** `DominoAllFives.Server.Tests.RankingControllerTests`.
- **Caso ejecutable:** `FromRankedPlayer_ValidEntry_PreservesServerValues`.
- **Origen:** [RankingControllerTests.cs](../DominoAllFives.Server/Tests/RankingControllerTests.cs#L42).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Rango 2; nombre Player; 5 victorias; imagen vacía válida.
- **Postcondiciones:** Entrada creada.
- **Resultado esperado:** Conserva rango, nombre, victorias y referencia de imagen.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `8968c8d0-372b-0e81-c0fb-243ba9b700a0`; duración `00:00:00.0010000`.

### PU-182. Rechazar ID de ranking inválido

- **Clase:** `DominoAllFives.Server.Tests.RankingControllerTests`.
- **Caso ejecutable:** `GetWinsRankingPositionById_InvalidId_RejectsWithoutOpeningDatabase(-1)`.
- **Origen:** [RankingControllerTests.cs](../DominoAllFives.Server/Tests/RankingControllerTests.cs#L17).
- **Datos:** `playerId=-1`
- **Precondiciones:** playerId=-1; storage con NullLogger.
- **Postcondiciones:** Rechazo previo a consulta persistida.
- **Resultado esperado:** ArgumentOutOfRangeException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `98daa180-a3e6-7179-a87b-ab9541fdf159`; duración `00:00:00.0000001`.

### PU-183. Rechazar ID de ranking inválido

- **Clase:** `DominoAllFives.Server.Tests.RankingControllerTests`.
- **Caso ejecutable:** `GetWinsRankingPositionById_InvalidId_RejectsWithoutOpeningDatabase(0)`.
- **Origen:** [RankingControllerTests.cs](../DominoAllFives.Server/Tests/RankingControllerTests.cs#L17).
- **Datos:** `playerId=0`
- **Precondiciones:** playerId=0; storage con NullLogger.
- **Postcondiciones:** Rechazo previo a consulta persistida.
- **Resultado esperado:** ArgumentOutOfRangeException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `4860b7e0-9b68-5cdc-6875-6ad1adeb7477`; duración `00:00:00.0270000`.

### PU-184. Rechazar ID de ranking inválido

- **Clase:** `DominoAllFives.Server.Tests.RankingControllerTests`.
- **Caso ejecutable:** `GetWinsRankingPositionById_InvalidId_RejectsWithoutOpeningDatabase(int.MinValue)`.
- **Origen:** [RankingControllerTests.cs](../DominoAllFives.Server/Tests/RankingControllerTests.cs#L17).
- **Datos:** `playerId=int.MinValue`
- **Precondiciones:** playerId=int.MinValue; storage con NullLogger.
- **Postcondiciones:** Rechazo previo a consulta persistida.
- **Resultado esperado:** ArgumentOutOfRangeException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `b4253e32-0ca1-c0df-bd50-0c608926d02d`; duración `00:00:00.0000001`.

### PU-185. Verificar serialización de credenciales

- **Clase:** `DominoAllFives.Server.Tests.SessionContractTests`.
- **Caso ejecutable:** `Serialize_Credentials_PreservesPublishedContract`.
- **Origen:** [SessionContractTests.cs](../DominoAllFives.Server/Tests/SessionContractTests.cs#L29).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Identificador y contraseña ficticios con espacios; MemoryStream.
- **Postcondiciones:** Contrato escrito y reconstruido en memoria.
- **Resultado esperado:** Nombre Credentials; namespace de contratos Domain; identificador player; contraseña con espacios conservados.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `807edd6a-5539-eeea-36e5-2ce649f2ecb4`; duración `00:00:00.2020000`.

### PU-186. Verificar identidad publicada de contrato

- **Clase:** `DominoAllFives.Server.Tests.SessionContractTests`.
- **Caso ejecutable:** `ServiceContract_SessionAccount_PreservesPublishedIdentity`.
- **Origen:** [SessionContractTests.cs](../DominoAllFives.Server/Tests/SessionContractTests.cs#L15).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Reflexión sobre ISessionService.
- **Postcondiciones:** Atributo leído en memoria.
- **Resultado esperado:** Name=ISessionAccount; Namespace=urn:domino-all-fives:services.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `62a8465b-b4d9-4cc5-e208-9d5802b1974c`; duración `00:00:00.0020000`.

### PU-187. Conservar sesiones concurrentes

- **Clase:** `DominoAllFives.Server.Tests.SessionManagerTests`.
- **Caso ejecutable:** `CreateSession_ConcurrentPlayers_PreservesEveryMapping`.
- **Origen:** [SessionManagerTests.cs](../DominoAllFives.Server/Tests/SessionManagerTests.cs#L126).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** 12 IDs ficticios desde 91100; fixture no paralelizable.
- **Postcondiciones:** Todas las sesiones creadas se limpian al finalizar.
- **Resultado esperado:** Cada clave resuelve su ID original.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `0d2a35b3-d3e3-f5e2-cd3b-95c84c9db4b4`; duración `00:00:00.0090000`.

### PU-188. Distinguir sesiones de jugadores

- **Clase:** `DominoAllFives.Server.Tests.SessionManagerTests`.
- **Caso ejecutable:** `CreateSession_DifferentPlayers_ProducesDifferentKeys`.
- **Origen:** [SessionManagerTests.cs](../DominoAllFives.Server/Tests/SessionManagerTests.cs#L71).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** IDs ficticios 91003 y 91004.
- **Postcondiciones:** Claves generadas y limpiadas al finalizar.
- **Resultado esperado:** Las dos claves son diferentes.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `ab5f9e60-244d-16e3-6c9b-94eed2b4a468`; duración `00:00:00.0010000`.

### PU-189. Rechazar ID de sesión inválido

- **Clase:** `DominoAllFives.Server.Tests.SessionManagerTests`.
- **Caso ejecutable:** `CreateSession_InvalidPlayerId_ThrowsArgumentOutOfRangeException(-1)`.
- **Origen:** [SessionManagerTests.cs](../DominoAllFives.Server/Tests/SessionManagerTests.cs#L38).
- **Datos:** `playerId=-1`
- **Precondiciones:** playerId=-1.
- **Postcondiciones:** No se crea sesión válida.
- **Resultado esperado:** ArgumentOutOfRangeException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `7a813c60-dc7a-dd5b-8240-959f82e349bf`; duración `00:00:00.0000001`.

### PU-190. Rechazar ID de sesión inválido

- **Clase:** `DominoAllFives.Server.Tests.SessionManagerTests`.
- **Caso ejecutable:** `CreateSession_InvalidPlayerId_ThrowsArgumentOutOfRangeException(0)`.
- **Origen:** [SessionManagerTests.cs](../DominoAllFives.Server/Tests/SessionManagerTests.cs#L38).
- **Datos:** `playerId=0`
- **Precondiciones:** playerId=0.
- **Postcondiciones:** No se crea sesión válida.
- **Resultado esperado:** ArgumentOutOfRangeException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `ff10c001-5a9c-7413-ca4e-1e8a179c6317`; duración `00:00:00.0000001`.

### PU-191. Rechazar ID de sesión inválido

- **Clase:** `DominoAllFives.Server.Tests.SessionManagerTests`.
- **Caso ejecutable:** `CreateSession_InvalidPlayerId_ThrowsArgumentOutOfRangeException(int.MinValue)`.
- **Origen:** [SessionManagerTests.cs](../DominoAllFives.Server/Tests/SessionManagerTests.cs#L38).
- **Datos:** `playerId=int.MinValue`
- **Precondiciones:** playerId=int.MinValue.
- **Postcondiciones:** No se crea sesión válida.
- **Resultado esperado:** ArgumentOutOfRangeException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `65089ea4-c85d-2643-9ad7-eec8a554f7b9`; duración `00:00:00.0000001`.

### PU-192. Sustituir sesión del mismo jugador

- **Clase:** `DominoAllFives.Server.Tests.SessionManagerTests`.
- **Caso ejecutable:** `CreateSession_SamePlayerTwice_InvalidatesPreviousKey`.
- **Origen:** [SessionManagerTests.cs](../DominoAllFives.Server/Tests/SessionManagerTests.cs#L49).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** ID ficticio 91002 con una sesión previa.
- **Postcondiciones:** Nueva clave sustituye anterior; ambas limpiadas al finalizar.
- **Resultado esperado:** Resolver primera clave lanza KeyNotFoundException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `53397e48-e9a4-df52-f815-536655349ad9`; duración `00:00:00.0010000`.

### PU-193. Crear sesión y resolver identidad

- **Clase:** `DominoAllFives.Server.Tests.SessionManagerTests`.
- **Caso ejecutable:** `CreateSession_ValidPlayer_ResolvesPlayerId`.
- **Origen:** [SessionManagerTests.cs](../DominoAllFives.Server/Tests/SessionManagerTests.cs#L16).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** ID ficticio 91001.
- **Postcondiciones:** Clave creada y limpiada al finalizar.
- **Resultado esperado:** GetPlayerId devuelve 91001.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `c4c4d8ab-168b-9ea2-0438-28936d25f875`; duración `00:00:00.0000001`.

### PU-194. Rechazar clave de consulta vacía

- **Clase:** `DominoAllFives.Server.Tests.SessionManagerTests`.
- **Caso ejecutable:** `GetPlayerId_BlankKey_ThrowsArgumentException("   ")`.
- **Origen:** [SessionManagerTests.cs](../DominoAllFives.Server/Tests/SessionManagerTests.cs#L115).
- **Datos:** `sessionKey="   "`
- **Precondiciones:** sessionKey="   ".
- **Postcondiciones:** Consulta rechazada.
- **Resultado esperado:** ArgumentException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `7d9ec7be-827b-0bce-74fc-ba5967252531`; duración `00:00:00.0000001`.

### PU-195. Rechazar clave de consulta vacía

- **Clase:** `DominoAllFives.Server.Tests.SessionManagerTests`.
- **Caso ejecutable:** `GetPlayerId_BlankKey_ThrowsArgumentException("")`.
- **Origen:** [SessionManagerTests.cs](../DominoAllFives.Server/Tests/SessionManagerTests.cs#L115).
- **Datos:** `sessionKey=""`
- **Precondiciones:** sessionKey="".
- **Postcondiciones:** Consulta rechazada.
- **Resultado esperado:** ArgumentException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `36cd22ed-67ed-afd3-b4ae-9acf4d8133a7`; duración `00:00:00.0010000`.

### PU-196. Rechazar clave de consulta null

- **Clase:** `DominoAllFives.Server.Tests.SessionManagerTests`.
- **Caso ejecutable:** `GetPlayerId_NullKey_ThrowsArgumentNullException`.
- **Origen:** [SessionManagerTests.cs](../DominoAllFives.Server/Tests/SessionManagerTests.cs#L103).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** sessionKey null.
- **Postcondiciones:** Consulta rechazada.
- **Resultado esperado:** ArgumentNullException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `5087b919-818d-fa32-2f54-502ce74f94b1`; duración `00:00:00.0000001`.

### PU-197. Eliminar sesión existente

- **Clase:** `DominoAllFives.Server.Tests.SessionManagerTests`.
- **Caso ejecutable:** `RemoveSession_ExistingSession_ReturnsTrue`.
- **Origen:** [SessionManagerTests.cs](../DominoAllFives.Server/Tests/SessionManagerTests.cs#L92).
- **Datos:** Sin parámetros; valores de preparación descritos abajo.
- **Precondiciones:** Sesión creada para ID ficticio 91005.
- **Postcondiciones:** Sesión eliminada.
- **Resultado esperado:** true.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `eb31e97a-aa64-5a50-8ea4-7e566cd7c936`; duración `00:00:00.0000001`.

### PU-198. Generar cada combinación doble seis exactamente una vez

- **Clase:** `DominoAllFives.Server.Tests.DeckGeneratorTests`.
- **Caso ejecutable:** `Generate_DoubleSixDeck_ContainsEveryValuePairOnce`.
- **Origen:** [DeckGeneratorTests.cs](../DominoAllFives.Server/Tests/DeckGeneratorTests.cs#L11).
- **Datos:** Sin parámetros; preparación descrita abajo.
- **Precondiciones:** DeckGenerator sin estado previo; conjunto esperado de las 28 parejas canónicas 0-0 a 6-6.
- **Postcondiciones:** Mazo generado.
- **Resultado esperado:** 28 fichas; 28 IDs distintos; conjunto de valores igual al doble seis completo.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `1476fc7f-c28c-08dd-cbc5-c938ef6f6010`; duración `00:00:00.0160000`.

### PU-199. Generar mazos independientes en llamadas sucesivas

- **Clase:** `DominoAllFives.Server.Tests.DeckGeneratorTests`.
- **Caso ejecutable:** `Generate_RepeatedCall_ReturnsIndependentDeck`.
- **Origen:** [DeckGeneratorTests.cs](../DominoAllFives.Server/Tests/DeckGeneratorTests.cs#L68).
- **Datos:** Sin parámetros; preparación descrita abajo.
- **Precondiciones:** Un mazo ya generado; mismo DeckGenerator.
- **Postcondiciones:** Se genera segundo mazo y se vacía el primero.
- **Resultado esperado:** El segundo mazo conserva 28 fichas.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `de93ea2e-513d-83dd-966d-e41af77c02e7`; duración `00:00:00.0030000`.

### PU-200. Asignar cinco poderes admitidos

- **Clase:** `DominoAllFives.Server.Tests.DeckGeneratorTests`.
- **Caso ejecutable:** `Generate_SpecialTiles_AssignsExactlyFiveSupportedPowers`.
- **Origen:** [DeckGeneratorTests.cs](../DominoAllFives.Server/Tests/DeckGeneratorTests.cs#L40).
- **Datos:** Sin parámetros; preparación descrita abajo.
- **Precondiciones:** DeckGenerator sin estado previo.
- **Postcondiciones:** Mazo generado con poderes.
- **Resultado esperado:** 23 fichas None y 5 con Block o DoublePoints, sin exigir presencia de ambos poderes.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `9fc1f204-f379-9e56-9d80-9ea1be68136d`; duración `00:00:00.0010000`.

### PU-201. Conservar pozo y manos al modificar la lista de origen

- **Clase:** `DominoAllFives.Server.Tests.RoundSetupTests`.
- **Caso ejecutable:** `Prepare_SourceDeckChanges_PreservesPreparedStockAndHands`.
- **Origen:** [RoundSetupTests.cs](../DominoAllFives.Server/Tests/RoundSetupTests.cs#L86).
- **Datos:** Sin parámetros; preparación descrita abajo.
- **Precondiciones:** Dos jugadores y mazo completo; preparación correcta capturada en PreparedRound.
- **Postcondiciones:** Vaciar la lista original no cambia las manos, el orden ni el pozo del resultado preparado.
- **Resultado esperado:** Manos, identificadores y puntuaciones de los participantes preparados, y cola, idénticos a los capturados antes del vaciado.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `1c9a34eb-7b87-a7d1-3d9a-6e5fc231b208`; duración `00:00:00.0010000`.

### PU-202. Preparar manos sin modificar jugadores originales

- **Clase:** `DominoAllFives.Server.Tests.RoundSetupTests`.
- **Caso ejecutable:** `Prepare_ExistingHands_ReturnsNewHandsWithoutChangingPlayers(2,7,14)`.
- **Origen:** [RoundSetupTests.cs](../DominoAllFives.Server/Tests/RoundSetupTests.cs#L152).
- **Datos:** `playerCount=2; handSize=7; stockCount=14`
- **Precondiciones:** 2 jugadores con mano anterior y puntos PlayerId por 10; mazo fijo con 6-6 y 5-5 primero.
- **Postcondiciones:** El resultado contiene manos nuevas; los jugadores auxiliares conservan su identidad, mano y puntuación previas.
- **Resultado esperado:** Reparto exacto por PlayerId y tamaños indicados; cola con el segmento restante y ficha de salida; estado original de jugadores sin cambios.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `a1d68299-66fa-3618-f202-034023490c81`; duración `00:00:00.0040000`.

### PU-203. Preparar manos sin modificar jugadores originales

- **Clase:** `DominoAllFives.Server.Tests.RoundSetupTests`.
- **Caso ejecutable:** `Prepare_ExistingHands_ReturnsNewHandsWithoutChangingPlayers(3,5,13)`.
- **Origen:** [RoundSetupTests.cs](../DominoAllFives.Server/Tests/RoundSetupTests.cs#L152).
- **Datos:** `playerCount=3; handSize=5; stockCount=13`
- **Precondiciones:** 3 jugadores con mano anterior y puntos PlayerId por 10; mazo fijo con 6-6 y 5-5 primero.
- **Postcondiciones:** El resultado contiene manos nuevas; los jugadores auxiliares conservan su identidad, mano y puntuación previas.
- **Resultado esperado:** Reparto exacto por PlayerId y tamaños indicados; cola con el segmento restante y ficha de salida; estado original de jugadores sin cambios.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `0d9c9e54-20ab-2b81-6c8d-b65a5eb7bfc4`; duración `00:00:00.0000001`.

### PU-204. Preparar manos sin modificar jugadores originales

- **Clase:** `DominoAllFives.Server.Tests.RoundSetupTests`.
- **Caso ejecutable:** `Prepare_ExistingHands_ReturnsNewHandsWithoutChangingPlayers(4,5,8)`.
- **Origen:** [RoundSetupTests.cs](../DominoAllFives.Server/Tests/RoundSetupTests.cs#L152).
- **Datos:** `playerCount=4; handSize=5; stockCount=8`
- **Precondiciones:** 4 jugadores con mano anterior y puntos PlayerId por 10; mazo fijo con 6-6 y 5-5 primero.
- **Postcondiciones:** El resultado contiene manos nuevas; los jugadores auxiliares conservan su identidad, mano y puntuación previas.
- **Resultado esperado:** Reparto exacto por PlayerId y tamaños indicados; cola con el segmento restante y ficha de salida; estado original de jugadores sin cambios.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `685849f5-f920-ae46-33d1-7e5a0b0793bb`; duración `00:00:00.0020000`.

### PU-205. Conservar preparación anterior ante siguiente mazo inválido

- **Clase:** `DominoAllFives.Server.Tests.RoundSetupTests`.
- **Caso ejecutable:** `Prepare_InvalidNextDeck_PreservesPreviousPreparation`.
- **Origen:** [RoundSetupTests.cs](../DominoAllFives.Server/Tests/RoundSetupTests.cs#L271).
- **Datos:** Sin parámetros; preparación descrita abajo.
- **Precondiciones:** Primera preparación correcta con mazo completo; segundo mazo de 27 fichas suministrado directamente.
- **Postcondiciones:** El resultado previo conserva sus manos, orden y pozo después de rechazar el mazo incompleto.
- **Resultado esperado:** ArgumentException; snapshot de jugadores y cola anterior idénticos a los previos al intento.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `8b59569c-8629-3078-54f5-6a0588b53584`; duración `00:00:00.0030000`.

### PU-206. Rechazar mazo inválido sin alterar jugadores

- **Clase:** `DominoAllFives.Server.Tests.RoundSetupTests`.
- **Caso ejecutable:** `Prepare_InvalidDeck_PreservesInputs(0)`.
- **Origen:** [RoundSetupTests.cs](../DominoAllFives.Server/Tests/RoundSetupTests.cs#L239).
- **Datos:** `scenario=0`
- **Precondiciones:** Identificadores 1 y 2; mazo inválido según el escenario individual indicado en Datos.
- **Postcondiciones:** Se rechaza la preparación sin modificar la lista de identificadores ni el mazo recibido.
- **Resultado esperado:** ArgumentException; identificadores y contenido del mazo idénticos a sus valores previos.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `fb9c4cdf-6904-b616-e7a0-5344cb200288`; duración `00:00:00.0020000`.

### PU-207. Rechazar mazo inválido sin alterar jugadores

- **Clase:** `DominoAllFives.Server.Tests.RoundSetupTests`.
- **Caso ejecutable:** `Prepare_InvalidDeck_PreservesInputs(1)`.
- **Origen:** [RoundSetupTests.cs](../DominoAllFives.Server/Tests/RoundSetupTests.cs#L239).
- **Datos:** `scenario=1`
- **Precondiciones:** Identificadores 1 y 2; mazo inválido según el escenario individual indicado en Datos.
- **Postcondiciones:** Se rechaza la preparación sin modificar la lista de identificadores ni el mazo recibido.
- **Resultado esperado:** ArgumentException; identificadores y contenido del mazo idénticos a sus valores previos.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `4f9b7a6d-baf4-c623-8b9f-1eb8b3faf9d1`; duración `00:00:00.0000001`.

### PU-208. Rechazar mazo inválido sin alterar jugadores

- **Clase:** `DominoAllFives.Server.Tests.RoundSetupTests`.
- **Caso ejecutable:** `Prepare_InvalidDeck_PreservesInputs(2)`.
- **Origen:** [RoundSetupTests.cs](../DominoAllFives.Server/Tests/RoundSetupTests.cs#L239).
- **Datos:** `scenario=2`
- **Precondiciones:** Identificadores 1 y 2; mazo inválido según el escenario individual indicado en Datos.
- **Postcondiciones:** Se rechaza la preparación sin modificar la lista de identificadores ni el mazo recibido.
- **Resultado esperado:** ArgumentException; identificadores y contenido del mazo idénticos a sus valores previos.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `b16dd95a-43b1-3016-e053-6c6bdf7b379e`; duración `00:00:00.0000001`.

### PU-209. Rechazar mazo inválido sin alterar jugadores

- **Clase:** `DominoAllFives.Server.Tests.RoundSetupTests`.
- **Caso ejecutable:** `Prepare_InvalidDeck_PreservesInputs(3)`.
- **Origen:** [RoundSetupTests.cs](../DominoAllFives.Server/Tests/RoundSetupTests.cs#L239).
- **Datos:** `scenario=3`
- **Precondiciones:** Identificadores 1 y 2; mazo inválido según el escenario individual indicado en Datos.
- **Postcondiciones:** Se rechaza la preparación sin modificar la lista de identificadores ni el mazo recibido.
- **Resultado esperado:** ArgumentException; identificadores y contenido del mazo idénticos a sus valores previos.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `84f31c5b-7697-f26f-2393-001368facad7`; duración `00:00:00.0000001`.

### PU-210. Rechazar mazo inválido sin alterar jugadores

- **Clase:** `DominoAllFives.Server.Tests.RoundSetupTests`.
- **Caso ejecutable:** `Prepare_InvalidDeck_PreservesInputs(4)`.
- **Origen:** [RoundSetupTests.cs](../DominoAllFives.Server/Tests/RoundSetupTests.cs#L239).
- **Datos:** `scenario=4`
- **Precondiciones:** Identificadores 1 y 2; mazo inválido según el escenario individual indicado en Datos.
- **Postcondiciones:** Se rechaza la preparación sin modificar la lista de identificadores ni el mazo recibido.
- **Resultado esperado:** ArgumentException; identificadores y contenido del mazo idénticos a sus valores previos.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `490e2590-bb70-8326-fe04-1468e2de8294`; duración `00:00:00.0000001`.

### PU-211. Rechazar mazo inválido sin alterar jugadores

- **Clase:** `DominoAllFives.Server.Tests.RoundSetupTests`.
- **Caso ejecutable:** `Prepare_InvalidDeck_PreservesInputs(5)`.
- **Origen:** [RoundSetupTests.cs](../DominoAllFives.Server/Tests/RoundSetupTests.cs#L239).
- **Datos:** `scenario=5`
- **Precondiciones:** Identificadores 1 y 2; mazo inválido según el escenario individual indicado en Datos.
- **Postcondiciones:** Se rechaza la preparación sin modificar la lista de identificadores ni el mazo recibido.
- **Resultado esperado:** ArgumentException; identificadores y contenido del mazo idénticos a sus valores previos.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `e6389e5f-6641-f6a7-8ac2-6710468e6132`; duración `00:00:00.0000001`.

### PU-212. Rechazar cantidad de jugadores de ronda

- **Clase:** `DominoAllFives.Server.Tests.RoundSetupTests`.
- **Caso ejecutable:** `Prepare_InvalidPlayerCount_ReportsPlayerIdsArgument(0)`.
- **Origen:** [RoundSetupTests.cs](../DominoAllFives.Server/Tests/RoundSetupTests.cs#L129).
- **Datos:** `playerCount=0`
- **Precondiciones:** 0 jugadores con identidad, puntos y mano previa.
- **Postcondiciones:** La preparación se rechaza antes del reparto.
- **Resultado esperado:** ArgumentException con ParamName=playerIds.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `d9894aa0-6384-8de8-4d62-27b961981fca`; duración `00:00:00.0010000`.

### PU-213. Rechazar cantidad de jugadores de ronda

- **Clase:** `DominoAllFives.Server.Tests.RoundSetupTests`.
- **Caso ejecutable:** `Prepare_InvalidPlayerCount_ReportsPlayerIdsArgument(1)`.
- **Origen:** [RoundSetupTests.cs](../DominoAllFives.Server/Tests/RoundSetupTests.cs#L129).
- **Datos:** `playerCount=1`
- **Precondiciones:** 1 jugadores con identidad, puntos y mano previa.
- **Postcondiciones:** La preparación se rechaza antes del reparto.
- **Resultado esperado:** ArgumentException con ParamName=playerIds.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `106a7ce9-251b-34b1-6eff-c38e41592097`; duración `00:00:00.0000001`.

### PU-214. Rechazar cantidad de jugadores de ronda

- **Clase:** `DominoAllFives.Server.Tests.RoundSetupTests`.
- **Caso ejecutable:** `Prepare_InvalidPlayerCount_ReportsPlayerIdsArgument(5)`.
- **Origen:** [RoundSetupTests.cs](../DominoAllFives.Server/Tests/RoundSetupTests.cs#L129).
- **Datos:** `playerCount=5`
- **Precondiciones:** 5 jugadores con identidad, puntos y mano previa.
- **Postcondiciones:** La preparación se rechaza antes del reparto.
- **Resultado esperado:** ArgumentException con ParamName=playerIds.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `6649c1c0-0f2a-4ac2-dd31-9c5d026509c6`; duración `00:00:00.0000001`.

### PU-215. Rechazar lista de jugadores nula

- **Clase:** `DominoAllFives.Server.Tests.RoundSetupTests`.
- **Caso ejecutable:** `Prepare_NullPlayerIds_ReportsPlayerIdsArgument`.
- **Origen:** [RoundSetupTests.cs](../DominoAllFives.Server/Tests/RoundSetupTests.cs#L107).
- **Datos:** Sin parámetros; preparación descrita abajo.
- **Precondiciones:** RoundSetup concreto; lista de identificadores null y mazo completo.
- **Postcondiciones:** La preparación se rechaza antes del reparto.
- **Resultado esperado:** ArgumentNullException con ParamName=playerIds.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `c9eb7283-a16f-d65d-bded-9231b40d611f`; duración `00:00:00.0010000`.

### PU-216. Ordenar participantes según prioridad de salida

- **Clase:** `DominoAllFives.Server.Tests.RoundSetupTests`.
- **Caso ejecutable:** `Prepare_OpeningPriority_OrdersPlayersAndSelectsRequiredTile(0,1,0)`.
- **Origen:** [RoundSetupTests.cs](../DominoAllFives.Server/Tests/RoundSetupTests.cs#L204).
- **Datos:** `scenario=0; openingPlayerId=1; openingValue=0`
- **Precondiciones:** Jugador 1 con 0-0 y jugador 2 con 5-6, sin otros dobles repartidos; dos jugadores con mazo fijo.
- **Postcondiciones:** PreparedRound ordena sus jugadores por la salida; los identificadores de entrada siguen en orden 1, 2.
- **Resultado esperado:** Primer jugador=1; valor de ficha obligatoria=0.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `2131de22-0b36-208b-fa0b-57281a96915f`; duración `00:00:00.0020000`.

### PU-217. Ordenar participantes según prioridad de salida

- **Clase:** `DominoAllFives.Server.Tests.RoundSetupTests`.
- **Caso ejecutable:** `Prepare_OpeningPriority_OrdersPlayersAndSelectsRequiredTile(1,2,66)`.
- **Origen:** [RoundSetupTests.cs](../DominoAllFives.Server/Tests/RoundSetupTests.cs#L204).
- **Datos:** `scenario=1; openingPlayerId=2; openingValue=66`
- **Precondiciones:** Jugador 1 con 5-5 y jugador 2 con 6-6; dos jugadores con mazo fijo.
- **Postcondiciones:** PreparedRound ordena sus jugadores por la salida; los identificadores de entrada siguen en orden 1, 2.
- **Resultado esperado:** Primer jugador=2; valor de ficha obligatoria=66.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `1648c976-931b-655f-f138-0fd58f9f18a1`; duración `00:00:00.0000001`.

### PU-218. Ordenar participantes según prioridad de salida

- **Clase:** `DominoAllFives.Server.Tests.RoundSetupTests`.
- **Caso ejecutable:** `Prepare_OpeningPriority_OrdersPlayersAndSelectsRequiredTile(2,2,36)`.
- **Origen:** [RoundSetupTests.cs](../DominoAllFives.Server/Tests/RoundSetupTests.cs#L204).
- **Datos:** `scenario=2; openingPlayerId=2; openingValue=36`
- **Precondiciones:** Sin dobles repartidos; máximas 4-5 y 3-6, con suma empatada; dos jugadores con mazo fijo.
- **Postcondiciones:** PreparedRound ordena sus jugadores por la salida; los identificadores de entrada siguen en orden 1, 2.
- **Resultado esperado:** Primer jugador=2; valor de ficha obligatoria=36.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `b1b36ef3-6b32-34e2-b9da-53e038ae5668`; duración `00:00:00.0000001`.

### PU-219. Acreditar exactamente el premio explicado de apertura

- **Clase:** `DominoAllFives.Server.Tests.PlayScoreTests`.
- **Caso ejecutable:** `Play_OpeningScore_AccreditsExactlyExplainedPoints(Block,1,10)`.
- **Origen:** [PlayScoreTests.cs](../DominoAllFives.Server/Tests/PlayScoreTests.cs#L15).
- **Datos:** `power=Block; multiplier=1; awardedPoints=10`
- **Precondiciones:** Mazo inicial fijo con 5-5 de poder Block; objetivo 250; sin puntos previos.
- **Postcondiciones:** Match acredita la jugada y conserva cero puntos para el oponente; LastPlay identifica jugador, ronda 1, ficha y extremo izquierdo.
- **Resultado esperado:** Base=10; multiplicador=1; premio explicado y puntos acreditados=10; RoundPoints=0.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `8a49f150-4ec2-c669-0ef1-ba9bd5806bb5`; duración `00:00:00.0000001`.

### PU-220. Acreditar exactamente el premio explicado de apertura

- **Clase:** `DominoAllFives.Server.Tests.PlayScoreTests`.
- **Caso ejecutable:** `Play_OpeningScore_AccreditsExactlyExplainedPoints(DoublePoints,2,20)`.
- **Origen:** [PlayScoreTests.cs](../DominoAllFives.Server/Tests/PlayScoreTests.cs#L15).
- **Datos:** `power=DoublePoints; multiplier=2; awardedPoints=20`
- **Precondiciones:** Mazo inicial fijo con 5-5 de poder DoublePoints; objetivo 250; sin puntos previos.
- **Postcondiciones:** Match acredita la jugada y conserva cero puntos para el oponente; LastPlay identifica jugador, ronda 1, ficha y extremo izquierdo.
- **Resultado esperado:** Base=10; multiplicador=2; premio explicado y puntos acreditados=20; RoundPoints=0.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `c25fe182-d0b4-f20c-f68e-e79561fc2ec7`; duración `00:00:00.0000001`.

### PU-221. Acreditar exactamente el premio explicado de apertura

- **Clase:** `DominoAllFives.Server.Tests.PlayScoreTests`.
- **Caso ejecutable:** `Play_OpeningScore_AccreditsExactlyExplainedPoints(None,1,10)`.
- **Origen:** [PlayScoreTests.cs](../DominoAllFives.Server/Tests/PlayScoreTests.cs#L15).
- **Datos:** `power=None; multiplier=1; awardedPoints=10`
- **Precondiciones:** Mazo inicial fijo con 5-5 de poder None; objetivo 250; sin puntos previos.
- **Postcondiciones:** Match acredita la jugada y conserva cero puntos para el oponente; LastPlay identifica jugador, ronda 1, ficha y extremo izquierdo.
- **Resultado esperado:** Base=10; multiplicador=1; premio explicado y puntos acreditados=10; RoundPoints=0.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `70d58df7-3ee5-3ded-a760-8285a3318206`; duración `00:00:00.0020000`.

### PU-222. Duplicar jugada roja sin duplicar premio de cierre

- **Clase:** `DominoAllFives.Server.Tests.PlayScoreTests`.
- **Caso ejecutable:** `Play_RedLastTile_DoublesPlayWithoutDoublingRoundAward`.
- **Origen:** [PlayScoreTests.cs](../DominoAllFives.Server/Tests/PlayScoreTests.cs#L51).
- **Datos:** Sin parámetros; preparación descrita abajo.
- **Precondiciones:** Mazo fijo; 14 acciones legales; jugador 1 con 15 puntos; última ficha 5-6 roja; objetivo 250.
- **Postcondiciones:** Se cierra la ronda y se prepara la ronda 2 con el marcador correcto.
- **Resultado esperado:** Base=15; multiplicador=2; jugada=30; RoundPoints y premio de ronda=5; marcador del resumen y del jugador=50.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `7d53bd8e-bd5a-dfcc-7716-b4cdbdbeee6b`; duración `00:00:00.0040000`.

### PU-223. Evaluar doble blanco sin premio

- **Clase:** `DominoAllFives.Server.Tests.PlayScoringTests`.
- **Caso ejecutable:** `Evaluate_BlankDouble_ReportsZeroAward(DoublePoints,2)`.
- **Origen:** [PlayScoringTests.cs](../DominoAllFives.Server/Tests/PlayScoringTests.cs#L64).
- **Datos:** `power=DoublePoints; multiplier=2`
- **Precondiciones:** Board real con 0-0 de poder DoublePoints colocado en turno 0.
- **Postcondiciones:** PlayScore contiene la explicación del doble inicial.
- **Resultado esperado:** Total=0; base=0; multiplicador=2; premio=0; EndValues=[0]; IncludesOpeningDouble=true.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `dc8bea5d-fed7-a41a-45a3-fc0ac3d1be27`; duración `00:00:00.0000001`.

### PU-224. Evaluar doble blanco sin premio

- **Clase:** `DominoAllFives.Server.Tests.PlayScoringTests`.
- **Caso ejecutable:** `Evaluate_BlankDouble_ReportsZeroAward(None,1)`.
- **Origen:** [PlayScoringTests.cs](../DominoAllFives.Server/Tests/PlayScoringTests.cs#L64).
- **Datos:** `power=None; multiplier=1`
- **Precondiciones:** Board real con 0-0 de poder None colocado en turno 0.
- **Postcondiciones:** PlayScore contiene la explicación del doble inicial.
- **Resultado esperado:** Total=0; base=0; multiplicador=1; premio=0; EndValues=[0]; IncludesOpeningDouble=true.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `271e98eb-c0d7-a89d-0605-44ec51b639d9`; duración `00:00:00.0010000`.

### PU-225. Excluir y explicar extremo bloqueado

- **Clase:** `DominoAllFives.Server.Tests.PlayScoringTests`.
- **Caso ejecutable:** `Evaluate_BlockedEnd_ExcludesItsValueAndExplainsBlock`.
- **Origen:** [PlayScoringTests.cs](../DominoAllFives.Server/Tests/PlayScoringTests.cs#L135).
- **Datos:** Sin parámetros; preparación descrita abajo.
- **Precondiciones:** 5-2 inicial; 2-4 morada a la derecha en turno 1; 0-5 a la izquierda en turno 2.
- **Postcondiciones:** Se evalúa la jugada del turno 2 con bloqueo vigente.
- **Resultado esperado:** EndValues=[0]; BlockedEnds=[Right]; total, base y premio=0.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `bed41adb-a109-dea4-0129-07a6df67c144`; duración `00:00:00.0020000`.

### PU-226. Conservar explicación al cambiar el tablero

- **Clase:** `DominoAllFives.Server.Tests.PlayScoringTests`.
- **Caso ejecutable:** `Evaluate_BoardChanges_PreservesPreviousExplanation`.
- **Origen:** [PlayScoringTests.cs](../DominoAllFives.Server/Tests/PlayScoringTests.cs#L178).
- **Datos:** Sin parámetros; preparación descrita abajo.
- **Precondiciones:** Tablero con Right bloqueado en turno 2 y primera evaluación capturada; después se coloca 4-5 a la derecha en turno 3.
- **Postcondiciones:** La segunda evaluación no modifica el resultado anterior.
- **Resultado esperado:** Resultado anterior: EndValues=[0], BlockedEnds=[Right], premio=0; resultado nuevo: EndValues=[0,5], sin bloqueos y premio=5.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `c1ca245a-f4d4-d810-67b0-b911b014d02e`; duración `00:00:00.0010000`.

### PU-227. Puntuar extremo liberado tras bloqueo

- **Clase:** `DominoAllFives.Server.Tests.PlayScoringTests`.
- **Caso ejecutable:** `Evaluate_ExpiredBlock_IncludesReleasedEnd`.
- **Origen:** [PlayScoringTests.cs](../DominoAllFives.Server/Tests/PlayScoringTests.cs#L158).
- **Datos:** Sin parámetros; preparación descrita abajo.
- **Precondiciones:** Tablero con bloqueo en turno 2; 4-5 colocada a la derecha en turno 3.
- **Postcondiciones:** Se incluye el extremo liberado en la explicación.
- **Resultado esperado:** EndValues=[0,5]; total=5; premio=5; cero extremos bloqueados.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `25df8c83-a16e-7f85-7724-3072d95802f1`; duración `00:00:00.0010000`.

### PU-228. Rechazar contexto de evaluación inválido

- **Clase:** `DominoAllFives.Server.Tests.PlayScoringTests`.
- **Caso ejecutable:** `Evaluate_InvalidBoardOrTurn_ReportsFailure(0,System.ArgumentNullException,"board")`.
- **Origen:** [PlayScoringTests.cs](../DominoAllFives.Server/Tests/PlayScoringTests.cs#L206).
- **Datos:** `scenario=0; exceptionType=System.ArgumentNullException; parameterName="board"`
- **Precondiciones:** board=null; turno 0.
- **Postcondiciones:** No se devuelve una evaluación válida.
- **Resultado esperado:** System.ArgumentNullException; ParamName="board".
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `85b2510e-23d7-203c-79de-7709fe2b2a2c`; duración `00:00:00.0010000`.

### PU-229. Rechazar contexto de evaluación inválido

- **Clase:** `DominoAllFives.Server.Tests.PlayScoringTests`.
- **Caso ejecutable:** `Evaluate_InvalidBoardOrTurn_ReportsFailure(1,System.ArgumentOutOfRangeException,"turnNumber")`.
- **Origen:** [PlayScoringTests.cs](../DominoAllFives.Server/Tests/PlayScoringTests.cs#L206).
- **Datos:** `scenario=1; exceptionType=System.ArgumentOutOfRangeException; parameterName="turnNumber"`
- **Precondiciones:** Board con 0-0 colocada; turnNumber=-1.
- **Postcondiciones:** No se devuelve una evaluación válida.
- **Resultado esperado:** System.ArgumentOutOfRangeException; ParamName="turnNumber".
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `ab497deb-3708-7c0b-9093-d2a5ec39dfa7`; duración `00:00:00.0000001`.

### PU-230. Rechazar contexto de evaluación inválido

- **Clase:** `DominoAllFives.Server.Tests.PlayScoringTests`.
- **Caso ejecutable:** `Evaluate_InvalidBoardOrTurn_ReportsFailure(2,System.InvalidOperationException,null)`.
- **Origen:** [PlayScoringTests.cs](../DominoAllFives.Server/Tests/PlayScoringTests.cs#L206).
- **Datos:** `scenario=2; exceptionType=System.InvalidOperationException; parameterName=null`
- **Precondiciones:** Board vacío; turno 0.
- **Postcondiciones:** No se devuelve una evaluación válida.
- **Resultado esperado:** System.InvalidOperationException; ParamName=null.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `c229c399-229c-75b8-68a1-d5d4ba24b163`; duración `00:00:00.0000001`.

### PU-231. Conservar suma sin conceder puntos fuera de múltiplo de cinco

- **Clase:** `DominoAllFives.Server.Tests.PlayScoringTests`.
- **Caso ejecutable:** `Evaluate_NonScoringTotal_PreservesTotalWithoutAward(DoublePoints,2)`.
- **Origen:** [PlayScoringTests.cs](../DominoAllFives.Server/Tests/PlayScoringTests.cs#L47).
- **Datos:** `power=DoublePoints; multiplier=2`
- **Precondiciones:** Board real con 3-3 de poder DoublePoints colocado en turno 0.
- **Postcondiciones:** PlayScore explica el total aunque no exista premio.
- **Resultado esperado:** Total=6; base=0; multiplicador=2; premio=0.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `2ef98566-0af1-695f-1656-0723509d66b1`; duración `00:00:00.0000001`.

### PU-232. Conservar suma sin conceder puntos fuera de múltiplo de cinco

- **Clase:** `DominoAllFives.Server.Tests.PlayScoringTests`.
- **Caso ejecutable:** `Evaluate_NonScoringTotal_PreservesTotalWithoutAward(None,1)`.
- **Origen:** [PlayScoringTests.cs](../DominoAllFives.Server/Tests/PlayScoringTests.cs#L47).
- **Datos:** `power=None; multiplier=1`
- **Precondiciones:** Board real con 3-3 de poder None colocado en turno 0.
- **Postcondiciones:** PlayScore explica el total aunque no exista premio.
- **Resultado esperado:** Total=6; base=0; multiplicador=1; premio=0.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `4d90dd9e-5dd5-6f73-c99c-e49e4577c7a3`; duración `00:00:00.0010000`.

### PU-233. Evaluar premio y explicación del doble inicial

- **Clase:** `DominoAllFives.Server.Tests.PlayScoringTests`.
- **Caso ejecutable:** `Evaluate_OpeningDouble_ReportsExactAward(Block,1,10)`.
- **Origen:** [PlayScoringTests.cs](../DominoAllFives.Server/Tests/PlayScoringTests.cs#L13).
- **Datos:** `power=Block; multiplier=1; awardedPoints=10`
- **Precondiciones:** Board real con 5-5 de poder Block colocado a la izquierda en turno 0.
- **Postcondiciones:** El resultado identifica la ficha y el extremo reales de la colocación.
- **Resultado esperado:** Total y base=10; multiplicador=1; premio=10; EndValues=[10]; doble inicial incluido; sin bloqueos del turno actual.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `151c9de1-2a92-d8f4-57c7-043f50f8ca95`; duración `00:00:00.0000001`.

### PU-234. Evaluar premio y explicación del doble inicial

- **Clase:** `DominoAllFives.Server.Tests.PlayScoringTests`.
- **Caso ejecutable:** `Evaluate_OpeningDouble_ReportsExactAward(DoublePoints,2,20)`.
- **Origen:** [PlayScoringTests.cs](../DominoAllFives.Server/Tests/PlayScoringTests.cs#L13).
- **Datos:** `power=DoublePoints; multiplier=2; awardedPoints=20`
- **Precondiciones:** Board real con 5-5 de poder DoublePoints colocado a la izquierda en turno 0.
- **Postcondiciones:** El resultado identifica la ficha y el extremo reales de la colocación.
- **Resultado esperado:** Total y base=10; multiplicador=2; premio=20; EndValues=[10]; doble inicial incluido; sin bloqueos del turno actual.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `3e02ee63-6344-a0c0-08f2-9911046dd4f4`; duración `00:00:00.0000001`.

### PU-235. Evaluar premio y explicación del doble inicial

- **Clase:** `DominoAllFives.Server.Tests.PlayScoringTests`.
- **Caso ejecutable:** `Evaluate_OpeningDouble_ReportsExactAward(None,1,10)`.
- **Origen:** [PlayScoringTests.cs](../DominoAllFives.Server/Tests/PlayScoringTests.cs#L13).
- **Datos:** `power=None; multiplier=1; awardedPoints=10`
- **Precondiciones:** Board real con 5-5 de poder None colocado a la izquierda en turno 0.
- **Postcondiciones:** El resultado identifica la ficha y el extremo reales de la colocación.
- **Resultado esperado:** Total y base=10; multiplicador=1; premio=10; EndValues=[10]; doble inicial incluido; sin bloqueos del turno actual.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `c840acbe-7933-13a7-11c2-04e1cfa383e7`; duración `00:00:00.0010000`.

### PU-236. Contar una sola vez el doble inicial con extremos libres

- **Clase:** `DominoAllFives.Server.Tests.PlayScoringTests`.
- **Caso ejecutable:** `Evaluate_OpeningDoubleWithUnusedEnds_CountsOpeningOnlyOnce`.
- **Origen:** [PlayScoringTests.cs](../DominoAllFives.Server/Tests/PlayScoringTests.cs#L120).
- **Datos:** Sin parámetros; preparación descrita abajo.
- **Precondiciones:** 5-5 inicial seguido de 0-5 a la izquierda en turno 1; quedan tres extremos iniciales libres.
- **Postcondiciones:** Se evalúa el aporte del doble sin triplicarlo.
- **Resultado esperado:** EndValues=[0,10]; total=10; IncludesOpeningDouble=true.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `39084bf4-df17-9749-68ca-9806592f1e04`; duración `00:00:00.0010000`.

### PU-237. Puntuar ambas mitades del doble terminal

- **Clase:** `DominoAllFives.Server.Tests.PlayScoringTests`.
- **Caso ejecutable:** `Evaluate_TerminalDouble_CountsBothHalves(DoublePoints,2,20)`.
- **Origen:** [PlayScoringTests.cs](../DominoAllFives.Server/Tests/PlayScoringTests.cs#L87).
- **Datos:** `power=DoublePoints; multiplier=2; awardedPoints=20`
- **Precondiciones:** 0-5 inicial; 5-5 de poder DoublePoints colocado a la derecha en turno 1.
- **Postcondiciones:** El resultado conserva ficha y extremo derecho; no identifica un doble inicial.
- **Resultado esperado:** EndValues=[0,10]; total y base=10; multiplicador=2; premio=20; IncludesOpeningDouble=false.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `1e860a35-a398-d856-fdda-8568cc99d825`; duración `00:00:00.0000001`.

### PU-238. Puntuar ambas mitades del doble terminal

- **Clase:** `DominoAllFives.Server.Tests.PlayScoringTests`.
- **Caso ejecutable:** `Evaluate_TerminalDouble_CountsBothHalves(None,1,10)`.
- **Origen:** [PlayScoringTests.cs](../DominoAllFives.Server/Tests/PlayScoringTests.cs#L87).
- **Datos:** `power=None; multiplier=1; awardedPoints=10`
- **Precondiciones:** 0-5 inicial; 5-5 de poder None colocado a la derecha en turno 1.
- **Postcondiciones:** El resultado conserva ficha y extremo derecho; no identifica un doble inicial.
- **Resultado esperado:** EndValues=[0,10]; total y base=10; multiplicador=1; premio=10; IncludesOpeningDouble=false.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `9acb6007-d4e0-06d9-c10e-7a856635aa93`; duración `00:00:00.0040000`.

### PU-239. Proteger resumen autorizado frente a cambios del contrato

- **Clase:** `DominoAllFives.Server.Tests.MatchRoundTests`.
- **Caso ejecutable:** `CreateView_ChangedRoundContract_PreservesAuthoritativeRound`.
- **Origen:** [MatchRoundTests.cs](../DominoAllFives.Server/Tests/MatchRoundTests.cs#L98).
- **Datos:** Sin parámetros; preparación descrita abajo.
- **Precondiciones:** Partida con mazo fijo del escenario 2, objetivo 250 y 14 acciones legales; resumen de primera ronda convertido a MatchView.
- **Postcondiciones:** Se modifica una ficha revelada del jugador 2 y se vacían su mano contractual y los marcadores del contrato; el dominio permanece intacto.
- **Resultado esperado:** Resumen autorizado conserva dos marcadores, 100 puntos del jugador 1 y la cantidad y primer valor previos de la mano del jugador 2.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `ad67d1e8-de57-8956-27b4-4d9f561b01e8`; duración `00:00:00.0130000`.

### PU-240. Rechazar argumentos inválidos al resolver ronda

- **Clase:** `DominoAllFives.Server.Tests.RoundResolutionTests`.
- **Caso ejecutable:** `Resolve_InvalidRound_ReportsArgument("Blocked",0,System.ArgumentNullException,"players")`.
- **Origen:** [RoundResolutionTests.cs](../DominoAllFives.Server/Tests/RoundResolutionTests.cs#L195).
- **Datos:** `operation="Blocked"; scenario=0; exceptionType=System.ArgumentNullException; parameterName="players"`
- **Precondiciones:** Operación "Blocked"; players=null.
- **Postcondiciones:** La evaluación no produce un resumen válido.
- **Resultado esperado:** System.ArgumentNullException; ParamName="players".
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `e854ebde-2a35-329e-e8dc-5ffe6cd69150`; duración `00:00:00.0000001`.

### PU-241. Rechazar argumentos inválidos al resolver ronda

- **Clase:** `DominoAllFives.Server.Tests.RoundResolutionTests`.
- **Caso ejecutable:** `Resolve_InvalidRound_ReportsArgument("Blocked",1,System.ArgumentException,"players")`.
- **Origen:** [RoundResolutionTests.cs](../DominoAllFives.Server/Tests/RoundResolutionTests.cs#L195).
- **Datos:** `operation="Blocked"; scenario=1; exceptionType=System.ArgumentException; parameterName="players"`
- **Precondiciones:** Operación "Blocked"; lista de cero jugadores.
- **Postcondiciones:** La evaluación no produce un resumen válido.
- **Resultado esperado:** System.ArgumentException; ParamName="players".
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `d5c3b1c4-f397-da58-e0fe-15940f5d5e60`; duración `00:00:00.0000001`.

### PU-242. Rechazar argumentos inválidos al resolver ronda

- **Clase:** `DominoAllFives.Server.Tests.RoundResolutionTests`.
- **Caso ejecutable:** `Resolve_InvalidRound_ReportsArgument("Blocked",2,System.ArgumentException,"players")`.
- **Origen:** [RoundResolutionTests.cs](../DominoAllFives.Server/Tests/RoundResolutionTests.cs#L195).
- **Datos:** `operation="Blocked"; scenario=2; exceptionType=System.ArgumentException; parameterName="players"`
- **Precondiciones:** Operación "Blocked"; lista de un jugador.
- **Postcondiciones:** La evaluación no produce un resumen válido.
- **Resultado esperado:** System.ArgumentException; ParamName="players".
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `f38711b6-098d-c3d3-75ae-edf838ec014c`; duración `00:00:00.0000001`.

### PU-243. Rechazar argumentos inválidos al resolver ronda

- **Clase:** `DominoAllFives.Server.Tests.RoundResolutionTests`.
- **Caso ejecutable:** `Resolve_InvalidRound_ReportsArgument("Blocked",3,System.ArgumentException,"players")`.
- **Origen:** [RoundResolutionTests.cs](../DominoAllFives.Server/Tests/RoundResolutionTests.cs#L195).
- **Datos:** `operation="Blocked"; scenario=3; exceptionType=System.ArgumentException; parameterName="players"`
- **Precondiciones:** Operación "Blocked"; lista de cinco jugadores.
- **Postcondiciones:** La evaluación no produce un resumen válido.
- **Resultado esperado:** System.ArgumentException; ParamName="players".
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `327a0a9d-dd4e-5e0e-1093-eff4ed7638fe`; duración `00:00:00.0000001`.

### PU-244. Rechazar argumentos inválidos al resolver ronda

- **Clase:** `DominoAllFives.Server.Tests.RoundResolutionTests`.
- **Caso ejecutable:** `Resolve_InvalidRound_ReportsArgument("Blocked",4,System.ArgumentOutOfRangeException,"roundNumber")`.
- **Origen:** [RoundResolutionTests.cs](../DominoAllFives.Server/Tests/RoundResolutionTests.cs#L195).
- **Datos:** `operation="Blocked"; scenario=4; exceptionType=System.ArgumentOutOfRangeException; parameterName="roundNumber"`
- **Precondiciones:** Operación "Blocked"; roundNumber=0 con jugadores válidos.
- **Postcondiciones:** La evaluación no produce un resumen válido.
- **Resultado esperado:** System.ArgumentOutOfRangeException; ParamName="roundNumber".
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `b1df5178-70a6-b75c-cbdb-4bf591010c82`; duración `00:00:00.0000001`.

### PU-245. Rechazar argumentos inválidos al resolver ronda

- **Clase:** `DominoAllFives.Server.Tests.RoundResolutionTests`.
- **Caso ejecutable:** `Resolve_InvalidRound_ReportsArgument("Blocked",5,System.ArgumentOutOfRangeException,"roundNumber")`.
- **Origen:** [RoundResolutionTests.cs](../DominoAllFives.Server/Tests/RoundResolutionTests.cs#L195).
- **Datos:** `operation="Blocked"; scenario=5; exceptionType=System.ArgumentOutOfRangeException; parameterName="roundNumber"`
- **Precondiciones:** Operación "Blocked"; roundNumber=-1 con jugadores válidos.
- **Postcondiciones:** La evaluación no produce un resumen válido.
- **Resultado esperado:** System.ArgumentOutOfRangeException; ParamName="roundNumber".
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `113f6a37-a987-1381-249c-f4e3ed6af7c8`; duración `00:00:00.0000001`.

### PU-246. Rechazar argumentos inválidos al resolver ronda

- **Clase:** `DominoAllFives.Server.Tests.RoundResolutionTests`.
- **Caso ejecutable:** `Resolve_InvalidRound_ReportsArgument("PlayedOut",0,System.ArgumentNullException,"players")`.
- **Origen:** [RoundResolutionTests.cs](../DominoAllFives.Server/Tests/RoundResolutionTests.cs#L195).
- **Datos:** `operation="PlayedOut"; scenario=0; exceptionType=System.ArgumentNullException; parameterName="players"`
- **Precondiciones:** Operación "PlayedOut"; players=null.
- **Postcondiciones:** La evaluación no produce un resumen válido.
- **Resultado esperado:** System.ArgumentNullException; ParamName="players".
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `c4ac2d5c-0878-7936-c5f5-d06576b3601b`; duración `00:00:00.0020000`.

### PU-247. Rechazar argumentos inválidos al resolver ronda

- **Clase:** `DominoAllFives.Server.Tests.RoundResolutionTests`.
- **Caso ejecutable:** `Resolve_InvalidRound_ReportsArgument("PlayedOut",1,System.ArgumentException,"players")`.
- **Origen:** [RoundResolutionTests.cs](../DominoAllFives.Server/Tests/RoundResolutionTests.cs#L195).
- **Datos:** `operation="PlayedOut"; scenario=1; exceptionType=System.ArgumentException; parameterName="players"`
- **Precondiciones:** Operación "PlayedOut"; lista de cero jugadores.
- **Postcondiciones:** La evaluación no produce un resumen válido.
- **Resultado esperado:** System.ArgumentException; ParamName="players".
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `ef85d3d3-f58e-6351-3dc2-8989bac2691f`; duración `00:00:00.0000001`.

### PU-248. Rechazar argumentos inválidos al resolver ronda

- **Clase:** `DominoAllFives.Server.Tests.RoundResolutionTests`.
- **Caso ejecutable:** `Resolve_InvalidRound_ReportsArgument("PlayedOut",2,System.ArgumentException,"players")`.
- **Origen:** [RoundResolutionTests.cs](../DominoAllFives.Server/Tests/RoundResolutionTests.cs#L195).
- **Datos:** `operation="PlayedOut"; scenario=2; exceptionType=System.ArgumentException; parameterName="players"`
- **Precondiciones:** Operación "PlayedOut"; lista de un jugador.
- **Postcondiciones:** La evaluación no produce un resumen válido.
- **Resultado esperado:** System.ArgumentException; ParamName="players".
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `af1e60ef-3da0-01fc-38f1-5a9652195a95`; duración `00:00:00.0000001`.

### PU-249. Rechazar argumentos inválidos al resolver ronda

- **Clase:** `DominoAllFives.Server.Tests.RoundResolutionTests`.
- **Caso ejecutable:** `Resolve_InvalidRound_ReportsArgument("PlayedOut",3,System.ArgumentException,"players")`.
- **Origen:** [RoundResolutionTests.cs](../DominoAllFives.Server/Tests/RoundResolutionTests.cs#L195).
- **Datos:** `operation="PlayedOut"; scenario=3; exceptionType=System.ArgumentException; parameterName="players"`
- **Precondiciones:** Operación "PlayedOut"; lista de cinco jugadores.
- **Postcondiciones:** La evaluación no produce un resumen válido.
- **Resultado esperado:** System.ArgumentException; ParamName="players".
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `9a00fdf4-6c25-8e23-4000-10af6508aafd`; duración `00:00:00.0000001`.

### PU-250. Rechazar argumentos inválidos al resolver ronda

- **Clase:** `DominoAllFives.Server.Tests.RoundResolutionTests`.
- **Caso ejecutable:** `Resolve_InvalidRound_ReportsArgument("PlayedOut",4,System.ArgumentOutOfRangeException,"roundNumber")`.
- **Origen:** [RoundResolutionTests.cs](../DominoAllFives.Server/Tests/RoundResolutionTests.cs#L195).
- **Datos:** `operation="PlayedOut"; scenario=4; exceptionType=System.ArgumentOutOfRangeException; parameterName="roundNumber"`
- **Precondiciones:** Operación "PlayedOut"; roundNumber=0 con jugadores válidos.
- **Postcondiciones:** La evaluación no produce un resumen válido.
- **Resultado esperado:** System.ArgumentOutOfRangeException; ParamName="roundNumber".
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `d0931dd4-ff6f-1141-9bb0-99ae6d297692`; duración `00:00:00.0000001`.

### PU-251. Rechazar argumentos inválidos al resolver ronda

- **Clase:** `DominoAllFives.Server.Tests.RoundResolutionTests`.
- **Caso ejecutable:** `Resolve_InvalidRound_ReportsArgument("PlayedOut",5,System.ArgumentOutOfRangeException,"roundNumber")`.
- **Origen:** [RoundResolutionTests.cs](../DominoAllFives.Server/Tests/RoundResolutionTests.cs#L195).
- **Datos:** `operation="PlayedOut"; scenario=5; exceptionType=System.ArgumentOutOfRangeException; parameterName="roundNumber"`
- **Precondiciones:** Operación "PlayedOut"; roundNumber=-1 con jugadores válidos.
- **Postcondiciones:** La evaluación no produce un resumen válido.
- **Resultado esperado:** System.ArgumentOutOfRangeException; ParamName="roundNumber".
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `d9892bab-b577-84df-0a17-eca69f40d176`; duración `00:00:00.0000001`.

### PU-252. Resolver mínimo de manos y empate por bloqueo

- **Clase:** `DominoAllFives.Server.Tests.RoundResolutionTests`.
- **Caso ejecutable:** `ResolveBlocked_RemainingHands_SelectsWinnerOrTieWithoutMutation(0,2,20)`.
- **Origen:** [RoundResolutionTests.cs](../DominoAllFives.Server/Tests/RoundResolutionTests.cs#L58).
- **Datos:** `scenario=0; winnerId=2; awardedPoints=20`
- **Precondiciones:** Ronda 4; manos restantes con puntos [12,6]; marcadores iniciales PlayerId por 10.
- **Postcondiciones:** Los participantes conservan orden, identidad, puntos y fichas; solo se proyecta el resultado.
- **Resultado esperado:** WinnerId=2; premio=20; marcadores proyectados=[10,40]; RoundNumber=4.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `828ab4f3-1d6b-c741-a7ed-70a07322b4bc`; duración `00:00:00.0060000`.

### PU-253. Resolver mínimo de manos y empate por bloqueo

- **Clase:** `DominoAllFives.Server.Tests.RoundResolutionTests`.
- **Caso ejecutable:** `ResolveBlocked_RemainingHands_SelectsWinnerOrTieWithoutMutation(1,3,15)`.
- **Origen:** [RoundResolutionTests.cs](../DominoAllFives.Server/Tests/RoundResolutionTests.cs#L58).
- **Datos:** `scenario=1; winnerId=3; awardedPoints=15`
- **Precondiciones:** Ronda 4; manos restantes con puntos [6,6,2]: un mínimo menor aparece después del empate; marcadores iniciales PlayerId por 10.
- **Postcondiciones:** Los participantes conservan orden, identidad, puntos y fichas; solo se proyecta el resultado.
- **Resultado esperado:** WinnerId=3; premio=15; marcadores proyectados=[10,20,45]; RoundNumber=4.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `8887d9c1-b36f-5e19-befb-bba7e2b3cbc9`; duración `00:00:00.0000001`.

### PU-254. Resolver mínimo de manos y empate por bloqueo

- **Clase:** `DominoAllFives.Server.Tests.RoundResolutionTests`.
- **Caso ejecutable:** `ResolveBlocked_RemainingHands_SelectsWinnerOrTieWithoutMutation(2,null,0)`.
- **Origen:** [RoundResolutionTests.cs](../DominoAllFives.Server/Tests/RoundResolutionTests.cs#L58).
- **Datos:** `scenario=2; winnerId=null; awardedPoints=0`
- **Precondiciones:** Ronda 4; manos restantes con puntos [6,6,8]: dos mínimos empatados; marcadores iniciales PlayerId por 10.
- **Postcondiciones:** Los participantes conservan orden, identidad, puntos y fichas; solo se proyecta el resultado.
- **Resultado esperado:** WinnerId=null; premio=0; marcadores proyectados=[10,20,30]; RoundNumber=4.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `790a53d5-be56-cdfa-31fb-12fecebfe959`; duración `00:00:00.0000001`.

### PU-255. Resolver mínimo de manos y empate por bloqueo

- **Clase:** `DominoAllFives.Server.Tests.RoundResolutionTests`.
- **Caso ejecutable:** `ResolveBlocked_RemainingHands_SelectsWinnerOrTieWithoutMutation(3,null,0)`.
- **Origen:** [RoundResolutionTests.cs](../DominoAllFives.Server/Tests/RoundResolutionTests.cs#L58).
- **Datos:** `scenario=3; winnerId=null; awardedPoints=0`
- **Precondiciones:** Ronda 4; manos restantes con puntos [4,6,4,8]: empate no contiguo; marcadores iniciales PlayerId por 10.
- **Postcondiciones:** Los participantes conservan orden, identidad, puntos y fichas; solo se proyecta el resultado.
- **Resultado esperado:** WinnerId=null; premio=0; marcadores proyectados=[10,20,30,40]; RoundNumber=4.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `4919823b-6b3f-f06a-889f-908dc2a9f446`; duración `00:00:00.0000001`.

### PU-256. Resolver mínimo de manos y empate por bloqueo

- **Clase:** `DominoAllFives.Server.Tests.RoundResolutionTests`.
- **Caso ejecutable:** `ResolveBlocked_RemainingHands_SelectsWinnerOrTieWithoutMutation(4,3,20)`.
- **Origen:** [RoundResolutionTests.cs](../DominoAllFives.Server/Tests/RoundResolutionTests.cs#L58).
- **Datos:** `scenario=4; winnerId=3; awardedPoints=20`
- **Precondiciones:** Ronda 4; manos restantes con puntos [6,6,3,4]: mínimo único posterior; marcadores iniciales PlayerId por 10.
- **Postcondiciones:** Los participantes conservan orden, identidad, puntos y fichas; solo se proyecta el resultado.
- **Resultado esperado:** WinnerId=3; premio=20; marcadores proyectados=[10,20,50,40]; RoundNumber=4.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `7c2774b8-5c95-e053-deda-fd406647d442`; duración `00:00:00.0000001`.

### PU-257. Resolver mínimo de manos y empate por bloqueo

- **Clase:** `DominoAllFives.Server.Tests.RoundResolutionTests`.
- **Caso ejecutable:** `ResolveBlocked_RemainingHands_SelectsWinnerOrTieWithoutMutation(5,null,0)`.
- **Origen:** [RoundResolutionTests.cs](../DominoAllFives.Server/Tests/RoundResolutionTests.cs#L58).
- **Datos:** `scenario=5; winnerId=null; awardedPoints=0`
- **Precondiciones:** Ronda 4; manos restantes con puntos [6,6,6,6]: empate de todos; marcadores iniciales PlayerId por 10.
- **Postcondiciones:** Los participantes conservan orden, identidad, puntos y fichas; solo se proyecta el resultado.
- **Resultado esperado:** WinnerId=null; premio=0; marcadores proyectados=[10,20,30,40]; RoundNumber=4.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `64e38347-7cc6-bf9c-0369-0ee2637e419e`; duración `00:00:00.0000001`.

### PU-258. Ignorar poder especial al contar puntos restantes

- **Clase:** `DominoAllFives.Server.Tests.RoundResolutionTests`.
- **Caso ejecutable:** `ResolveBlocked_SpecialPower_DoesNotChangeRemainingPips(Block)`.
- **Origen:** [RoundResolutionTests.cs](../DominoAllFives.Server/Tests/RoundResolutionTests.cs#L91).
- **Datos:** `power=Block`
- **Precondiciones:** Jugador 1 conserva 0-6 con poder Block y 10 puntos; jugador 2 conserva 0-5 ordinaria y 20 puntos; ronda 1.
- **Postcondiciones:** Se conserva el poder en la mano capturada y no se modifican los jugadores.
- **Resultado esperado:** Ganador=2; suma restante 11 concede premio 10; marcadores proyectados 10 y 30; puntos originales siguen 10 y 20.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `825f3ffc-f478-2bd5-7317-e8a9eddb9532`; duración `00:00:00.0000001`.

### PU-259. Ignorar poder especial al contar puntos restantes

- **Clase:** `DominoAllFives.Server.Tests.RoundResolutionTests`.
- **Caso ejecutable:** `ResolveBlocked_SpecialPower_DoesNotChangeRemainingPips(DoublePoints)`.
- **Origen:** [RoundResolutionTests.cs](../DominoAllFives.Server/Tests/RoundResolutionTests.cs#L91).
- **Datos:** `power=DoublePoints`
- **Precondiciones:** Jugador 1 conserva 0-6 con poder DoublePoints y 10 puntos; jugador 2 conserva 0-5 ordinaria y 20 puntos; ronda 1.
- **Postcondiciones:** Se conserva el poder en la mano capturada y no se modifican los jugadores.
- **Resultado esperado:** Ganador=2; suma restante 11 concede premio 10; marcadores proyectados 10 y 30; puntos originales siguen 10 y 20.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `1f5aff14-d4a1-fe07-41fc-16cb69f06a8b`; duración `00:00:00.0000001`.

### PU-260. Ignorar poder especial al contar puntos restantes

- **Clase:** `DominoAllFives.Server.Tests.RoundResolutionTests`.
- **Caso ejecutable:** `ResolveBlocked_SpecialPower_DoesNotChangeRemainingPips(None)`.
- **Origen:** [RoundResolutionTests.cs](../DominoAllFives.Server/Tests/RoundResolutionTests.cs#L91).
- **Datos:** `power=None`
- **Precondiciones:** Jugador 1 conserva 0-6 con poder None y 10 puntos; jugador 2 conserva 0-5 ordinaria y 20 puntos; ronda 1.
- **Postcondiciones:** Se conserva el poder en la mano capturada y no se modifican los jugadores.
- **Resultado esperado:** Ganador=2; suma restante 11 concede premio 10; marcadores proyectados 10 y 30; puntos originales siguen 10 y 20.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `4cfa88b6-4004-d27b-b6ed-b37f3f20c6a6`; duración `00:00:00.0010000`.

### PU-261. Rechazar modificación de colección del resumen

- **Clase:** `DominoAllFives.Server.Tests.RoundResolutionTests`.
- **Caso ejecutable:** `ResolvePlayedOut_CollectionMutation_IsRejected(0)`.
- **Origen:** [RoundResolutionTests.cs](../DominoAllFives.Server/Tests/RoundResolutionTests.cs#L145).
- **Datos:** `collection=0`
- **Precondiciones:** Cierre válido con suma restante 7; intento de vaciar Scores mediante IDictionary.
- **Postcondiciones:** Las colecciones del resumen mantienen su contenido.
- **Resultado esperado:** NotSupportedException; dos marcadores, dos manos registradas y una ficha en la mano del jugador 2.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `0257abe2-5003-787f-996d-d45dad9709c5`; duración `00:00:00.0020000`.

### PU-262. Rechazar modificación de colección del resumen

- **Clase:** `DominoAllFives.Server.Tests.RoundResolutionTests`.
- **Caso ejecutable:** `ResolvePlayedOut_CollectionMutation_IsRejected(1)`.
- **Origen:** [RoundResolutionTests.cs](../DominoAllFives.Server/Tests/RoundResolutionTests.cs#L145).
- **Datos:** `collection=1`
- **Precondiciones:** Cierre válido con suma restante 7; intento de vaciar RemainingHands mediante IDictionary.
- **Postcondiciones:** Las colecciones del resumen mantienen su contenido.
- **Resultado esperado:** NotSupportedException; dos marcadores, dos manos registradas y una ficha en la mano del jugador 2.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `d0ad6dc2-65f2-f06c-5bba-f2b21e68facb`; duración `00:00:00.0000001`.

### PU-263. Rechazar modificación de colección del resumen

- **Clase:** `DominoAllFives.Server.Tests.RoundResolutionTests`.
- **Caso ejecutable:** `ResolvePlayedOut_CollectionMutation_IsRejected(2)`.
- **Origen:** [RoundResolutionTests.cs](../DominoAllFives.Server/Tests/RoundResolutionTests.cs#L145).
- **Datos:** `collection=2`
- **Precondiciones:** Cierre válido con suma restante 7; intento de vaciar la lista de fichas del jugador 2 mediante IList.
- **Postcondiciones:** Las colecciones del resumen mantienen su contenido.
- **Resultado esperado:** NotSupportedException; dos marcadores, dos manos registradas y una ficha en la mano del jugador 2.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `8ef075b5-af65-8550-33ba-669e478d15e3`; duración `00:00:00.0000001`.

### PU-264. Redondear premio por última ficha sin aplicar puntos

- **Clase:** `DominoAllFives.Server.Tests.RoundResolutionTests`.
- **Caso ejecutable:** `ResolvePlayedOut_RemainingPips_ProjectsRoundedAwardWithoutMutation(0,0)`.
- **Origen:** [RoundResolutionTests.cs](../DominoAllFives.Server/Tests/RoundResolutionTests.cs#L23).
- **Datos:** `remainingPips=0; awardedPoints=0`
- **Precondiciones:** Ronda 3; ganador 1 sin fichas y con 20 puntos; jugador 2 con 40 puntos y suma restante 0.
- **Postcondiciones:** Los jugadores conservan sus datos originales; el resumen captura mano vacía del ganador y copia de la mano del rival.
- **Resultado esperado:** WinnerId=1; premio=0; marcadores proyectados 20 y 40; RoundNumber=3.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `532bfea8-d8d7-483f-5ad6-94fbe6bb9ac7`; duración `00:00:00.0010000`.

### PU-265. Redondear premio por última ficha sin aplicar puntos

- **Clase:** `DominoAllFives.Server.Tests.RoundResolutionTests`.
- **Caso ejecutable:** `ResolvePlayedOut_RemainingPips_ProjectsRoundedAwardWithoutMutation(1,0)`.
- **Origen:** [RoundResolutionTests.cs](../DominoAllFives.Server/Tests/RoundResolutionTests.cs#L23).
- **Datos:** `remainingPips=1; awardedPoints=0`
- **Precondiciones:** Ronda 3; ganador 1 sin fichas y con 20 puntos; jugador 2 con 40 puntos y suma restante 1.
- **Postcondiciones:** Los jugadores conservan sus datos originales; el resumen captura mano vacía del ganador y copia de la mano del rival.
- **Resultado esperado:** WinnerId=1; premio=0; marcadores proyectados 20 y 40; RoundNumber=3.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `b2fa80d6-1f15-e53e-6093-9e355c2c71c7`; duración `00:00:00.0000001`.

### PU-266. Redondear premio por última ficha sin aplicar puntos

- **Clase:** `DominoAllFives.Server.Tests.RoundResolutionTests`.
- **Caso ejecutable:** `ResolvePlayedOut_RemainingPips_ProjectsRoundedAwardWithoutMutation(10,10)`.
- **Origen:** [RoundResolutionTests.cs](../DominoAllFives.Server/Tests/RoundResolutionTests.cs#L23).
- **Datos:** `remainingPips=10; awardedPoints=10`
- **Precondiciones:** Ronda 3; ganador 1 sin fichas y con 20 puntos; jugador 2 con 40 puntos y suma restante 10.
- **Postcondiciones:** Los jugadores conservan sus datos originales; el resumen captura mano vacía del ganador y copia de la mano del rival.
- **Resultado esperado:** WinnerId=1; premio=10; marcadores proyectados 30 y 40; RoundNumber=3.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `74a72161-d712-215c-f504-ecbd82473a29`; duración `00:00:00.0000001`.

### PU-267. Redondear premio por última ficha sin aplicar puntos

- **Clase:** `DominoAllFives.Server.Tests.RoundResolutionTests`.
- **Caso ejecutable:** `ResolvePlayedOut_RemainingPips_ProjectsRoundedAwardWithoutMutation(2,0)`.
- **Origen:** [RoundResolutionTests.cs](../DominoAllFives.Server/Tests/RoundResolutionTests.cs#L23).
- **Datos:** `remainingPips=2; awardedPoints=0`
- **Precondiciones:** Ronda 3; ganador 1 sin fichas y con 20 puntos; jugador 2 con 40 puntos y suma restante 2.
- **Postcondiciones:** Los jugadores conservan sus datos originales; el resumen captura mano vacía del ganador y copia de la mano del rival.
- **Resultado esperado:** WinnerId=1; premio=0; marcadores proyectados 20 y 40; RoundNumber=3.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `59d581ee-99ed-2bc1-7726-8784cc716256`; duración `00:00:00.0000001`.

### PU-268. Redondear premio por última ficha sin aplicar puntos

- **Clase:** `DominoAllFives.Server.Tests.RoundResolutionTests`.
- **Caso ejecutable:** `ResolvePlayedOut_RemainingPips_ProjectsRoundedAwardWithoutMutation(3,5)`.
- **Origen:** [RoundResolutionTests.cs](../DominoAllFives.Server/Tests/RoundResolutionTests.cs#L23).
- **Datos:** `remainingPips=3; awardedPoints=5`
- **Precondiciones:** Ronda 3; ganador 1 sin fichas y con 20 puntos; jugador 2 con 40 puntos y suma restante 3.
- **Postcondiciones:** Los jugadores conservan sus datos originales; el resumen captura mano vacía del ganador y copia de la mano del rival.
- **Resultado esperado:** WinnerId=1; premio=5; marcadores proyectados 25 y 40; RoundNumber=3.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `5da58251-4902-e081-22b4-0a1e774708a9`; duración `00:00:00.0000001`.

### PU-269. Redondear premio por última ficha sin aplicar puntos

- **Clase:** `DominoAllFives.Server.Tests.RoundResolutionTests`.
- **Caso ejecutable:** `ResolvePlayedOut_RemainingPips_ProjectsRoundedAwardWithoutMutation(4,5)`.
- **Origen:** [RoundResolutionTests.cs](../DominoAllFives.Server/Tests/RoundResolutionTests.cs#L23).
- **Datos:** `remainingPips=4; awardedPoints=5`
- **Precondiciones:** Ronda 3; ganador 1 sin fichas y con 20 puntos; jugador 2 con 40 puntos y suma restante 4.
- **Postcondiciones:** Los jugadores conservan sus datos originales; el resumen captura mano vacía del ganador y copia de la mano del rival.
- **Resultado esperado:** WinnerId=1; premio=5; marcadores proyectados 25 y 40; RoundNumber=3.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `d3eb685e-e8a8-1663-759a-fbc23224d80b`; duración `00:00:00.0000001`.

### PU-270. Redondear premio por última ficha sin aplicar puntos

- **Clase:** `DominoAllFives.Server.Tests.RoundResolutionTests`.
- **Caso ejecutable:** `ResolvePlayedOut_RemainingPips_ProjectsRoundedAwardWithoutMutation(5,5)`.
- **Origen:** [RoundResolutionTests.cs](../DominoAllFives.Server/Tests/RoundResolutionTests.cs#L23).
- **Datos:** `remainingPips=5; awardedPoints=5`
- **Precondiciones:** Ronda 3; ganador 1 sin fichas y con 20 puntos; jugador 2 con 40 puntos y suma restante 5.
- **Postcondiciones:** Los jugadores conservan sus datos originales; el resumen captura mano vacía del ganador y copia de la mano del rival.
- **Resultado esperado:** WinnerId=1; premio=5; marcadores proyectados 25 y 40; RoundNumber=3.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `336ea7b0-74b0-3ca2-2896-2d44d6d99640`; duración `00:00:00.0000001`.

### PU-271. Redondear premio por última ficha sin aplicar puntos

- **Clase:** `DominoAllFives.Server.Tests.RoundResolutionTests`.
- **Caso ejecutable:** `ResolvePlayedOut_RemainingPips_ProjectsRoundedAwardWithoutMutation(53,55)`.
- **Origen:** [RoundResolutionTests.cs](../DominoAllFives.Server/Tests/RoundResolutionTests.cs#L23).
- **Datos:** `remainingPips=53; awardedPoints=55`
- **Precondiciones:** Ronda 3; ganador 1 sin fichas y con 20 puntos; jugador 2 con 40 puntos y suma restante 53.
- **Postcondiciones:** Los jugadores conservan sus datos originales; el resumen captura mano vacía del ganador y copia de la mano del rival.
- **Resultado esperado:** WinnerId=1; premio=55; marcadores proyectados 75 y 40; RoundNumber=3.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `bf612d42-9873-6269-d68e-cb6487a0a248`; duración `00:00:00.0000001`.

### PU-272. Redondear premio por última ficha sin aplicar puntos

- **Clase:** `DominoAllFives.Server.Tests.RoundResolutionTests`.
- **Caso ejecutable:** `ResolvePlayedOut_RemainingPips_ProjectsRoundedAwardWithoutMutation(6,5)`.
- **Origen:** [RoundResolutionTests.cs](../DominoAllFives.Server/Tests/RoundResolutionTests.cs#L23).
- **Datos:** `remainingPips=6; awardedPoints=5`
- **Precondiciones:** Ronda 3; ganador 1 sin fichas y con 20 puntos; jugador 2 con 40 puntos y suma restante 6.
- **Postcondiciones:** Los jugadores conservan sus datos originales; el resumen captura mano vacía del ganador y copia de la mano del rival.
- **Resultado esperado:** WinnerId=1; premio=5; marcadores proyectados 25 y 40; RoundNumber=3.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `fa4ae18b-3496-10a7-280c-26e0d472d52d`; duración `00:00:00.0000001`.

### PU-273. Redondear premio por última ficha sin aplicar puntos

- **Clase:** `DominoAllFives.Server.Tests.RoundResolutionTests`.
- **Caso ejecutable:** `ResolvePlayedOut_RemainingPips_ProjectsRoundedAwardWithoutMutation(7,5)`.
- **Origen:** [RoundResolutionTests.cs](../DominoAllFives.Server/Tests/RoundResolutionTests.cs#L23).
- **Datos:** `remainingPips=7; awardedPoints=5`
- **Precondiciones:** Ronda 3; ganador 1 sin fichas y con 20 puntos; jugador 2 con 40 puntos y suma restante 7.
- **Postcondiciones:** Los jugadores conservan sus datos originales; el resumen captura mano vacía del ganador y copia de la mano del rival.
- **Resultado esperado:** WinnerId=1; premio=5; marcadores proyectados 25 y 40; RoundNumber=3.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `f38b3ca3-5c03-03c5-fc53-a7278c4478c7`; duración `00:00:00.0000001`.

### PU-274. Redondear premio por última ficha sin aplicar puntos

- **Clase:** `DominoAllFives.Server.Tests.RoundResolutionTests`.
- **Caso ejecutable:** `ResolvePlayedOut_RemainingPips_ProjectsRoundedAwardWithoutMutation(8,10)`.
- **Origen:** [RoundResolutionTests.cs](../DominoAllFives.Server/Tests/RoundResolutionTests.cs#L23).
- **Datos:** `remainingPips=8; awardedPoints=10`
- **Precondiciones:** Ronda 3; ganador 1 sin fichas y con 20 puntos; jugador 2 con 40 puntos y suma restante 8.
- **Postcondiciones:** Los jugadores conservan sus datos originales; el resumen captura mano vacía del ganador y copia de la mano del rival.
- **Resultado esperado:** WinnerId=1; premio=10; marcadores proyectados 30 y 40; RoundNumber=3.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `900772e7-f441-8091-7eda-29523d6aefd0`; duración `00:00:00.0000001`.

### PU-275. Redondear premio por última ficha sin aplicar puntos

- **Clase:** `DominoAllFives.Server.Tests.RoundResolutionTests`.
- **Caso ejecutable:** `ResolvePlayedOut_RemainingPips_ProjectsRoundedAwardWithoutMutation(9,10)`.
- **Origen:** [RoundResolutionTests.cs](../DominoAllFives.Server/Tests/RoundResolutionTests.cs#L23).
- **Datos:** `remainingPips=9; awardedPoints=10`
- **Precondiciones:** Ronda 3; ganador 1 sin fichas y con 20 puntos; jugador 2 con 40 puntos y suma restante 9.
- **Postcondiciones:** Los jugadores conservan sus datos originales; el resumen captura mano vacía del ganador y copia de la mano del rival.
- **Resultado esperado:** WinnerId=1; premio=10; marcadores proyectados 30 y 40; RoundNumber=3.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `947c3bb6-fbaa-a285-0e72-0be940847925`; duración `00:00:00.0000001`.

### PU-276. Conservar resumen después de modificar fuentes

- **Clase:** `DominoAllFives.Server.Tests.RoundResolutionTests`.
- **Caso ejecutable:** `ResolvePlayedOut_SourceChanges_PreservesRoundSnapshot`.
- **Origen:** [RoundResolutionTests.cs](../DominoAllFives.Server/Tests/RoundResolutionTests.cs#L117).
- **Datos:** Sin parámetros; preparación descrita abajo.
- **Precondiciones:** Resumen de ronda 5 con ganador 1, suma restante 7 y marcadores previos 20 y 40.
- **Postcondiciones:** Se cambian puntos a 500 y 800, se vacía mano del rival y lista de jugadores; el resumen no cambia.
- **Resultado esperado:** Ronda=5; ganador=1; premio=5; marcadores 25 y 40; rival conserva una ficha de siete puntos en el snapshot.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `5886df89-f11d-288b-b491-85536840803e`; duración `00:00:00.0010000`.

### PU-277. Rechazar ganador ajeno a la ronda

- **Clase:** `DominoAllFives.Server.Tests.RoundResolutionTests`.
- **Caso ejecutable:** `ResolvePlayedOut_UnknownWinner_RejectsWithoutMutation(-1)`.
- **Origen:** [RoundResolutionTests.cs](../DominoAllFives.Server/Tests/RoundResolutionTests.cs#L252).
- **Datos:** `winnerId=-1`
- **Precondiciones:** Dos participantes válidos con IDs 1 y 2; winnerId=-1; jugador 1 sin fichas.
- **Postcondiciones:** No cambia el estado de los jugadores.
- **Resultado esperado:** MatchRuleException con Issue=PlayerNotInMatch.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `bc492b93-0c2e-c42f-fc3a-04f628cbb459`; duración `00:00:00.0000001`.

### PU-278. Rechazar ganador ajeno a la ronda

- **Clase:** `DominoAllFives.Server.Tests.RoundResolutionTests`.
- **Caso ejecutable:** `ResolvePlayedOut_UnknownWinner_RejectsWithoutMutation(0)`.
- **Origen:** [RoundResolutionTests.cs](../DominoAllFives.Server/Tests/RoundResolutionTests.cs#L252).
- **Datos:** `winnerId=0`
- **Precondiciones:** Dos participantes válidos con IDs 1 y 2; winnerId=0; jugador 1 sin fichas.
- **Postcondiciones:** No cambia el estado de los jugadores.
- **Resultado esperado:** MatchRuleException con Issue=PlayerNotInMatch.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `47a4d0f8-c776-7c92-0ede-d3d08ee68eec`; duración `00:00:00.0010000`.

### PU-279. Rechazar ganador ajeno a la ronda

- **Clase:** `DominoAllFives.Server.Tests.RoundResolutionTests`.
- **Caso ejecutable:** `ResolvePlayedOut_UnknownWinner_RejectsWithoutMutation(99)`.
- **Origen:** [RoundResolutionTests.cs](../DominoAllFives.Server/Tests/RoundResolutionTests.cs#L252).
- **Datos:** `winnerId=99`
- **Precondiciones:** Dos participantes válidos con IDs 1 y 2; winnerId=99; jugador 1 sin fichas.
- **Postcondiciones:** No cambia el estado de los jugadores.
- **Resultado esperado:** MatchRuleException con Issue=PlayerNotInMatch.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `f6d19309-7b5a-1e3b-227c-9021f983b16f`; duración `00:00:00.0000001`.

### PU-280. Rechazar cierre por salida con mano no vacía

- **Clase:** `DominoAllFives.Server.Tests.RoundResolutionTests`.
- **Caso ejecutable:** `ResolvePlayedOut_WinnerStillHasTiles_RejectsWithoutMutation`.
- **Origen:** [RoundResolutionTests.cs](../DominoAllFives.Server/Tests/RoundResolutionTests.cs#L274).
- **Datos:** Sin parámetros; preparación descrita abajo.
- **Precondiciones:** Dos jugadores conservan fichas; se solicita ganador 1 por última ficha.
- **Postcondiciones:** No cambia el estado de los jugadores.
- **Resultado esperado:** InvalidOperationException.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `999e8838-b92d-5701-e903-9e42ef60d499`; duración `00:00:00.0010000`.

### PU-281. Rechazar identificador inválido de cola

- **Clase:** `DominoAllFives.Server.Tests.MatchUpdateQueueTests`.
- **Caso ejecutable:** `Constructor_InvalidMatchId_ReportsArgument(-1)`.
- **Origen:** [MatchUpdateQueueTests.cs](../DominoAllFives.Server/Tests/MatchUpdateQueueTests.cs#L15).
- **Datos:** Argumentos de esta ejecución: `(-1)`.
- **Precondiciones:** Se intenta crear una cola con el identificador indicado.
- **Postcondiciones:** No se crea una cola utilizable.
- **Resultado esperado:** ArgumentOutOfRangeException con ParamName matchId.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `08bef597-6093-2649-1fec-f318369e1d04`; duración `00:00:00.0000001`.

### PU-282. Rechazar identificador inválido de cola

- **Clase:** `DominoAllFives.Server.Tests.MatchUpdateQueueTests`.
- **Caso ejecutable:** `Constructor_InvalidMatchId_ReportsArgument(0)`.
- **Origen:** [MatchUpdateQueueTests.cs](../DominoAllFives.Server/Tests/MatchUpdateQueueTests.cs#L15).
- **Datos:** Argumentos de esta ejecución: `(0)`.
- **Precondiciones:** Se intenta crear una cola con el identificador indicado.
- **Postcondiciones:** No se crea una cola utilizable.
- **Resultado esperado:** ArgumentOutOfRangeException con ParamName matchId.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `6522b01d-9a25-c41d-6b99-4e58ea79afcd`; duración `00:00:00.0010000`.

### PU-283. Cerrar el registro después del resultado terminal

- **Clase:** `DominoAllFives.Server.Tests.MatchUpdateQueueTests`.
- **Caso ejecutable:** `Enqueue_AfterTerminalUpdate_RejectsFurtherChanges`.
- **Origen:** [MatchUpdateQueueTests.cs](../DominoAllFives.Server/Tests/MatchUpdateQueueTests.cs#L195).
- **Datos:** Sin parámetros; preparación descrita abajo.
- **Precondiciones:** Cola de partida 1 con snapshot terminal de versión 1; se intenta agregar versión 2.
- **Postcondiciones:** Solo se consume el terminal original; la cola vacía no vuelve a activarse.
- **Resultado esperado:** InvalidOperationException; terminal original, sin más elementos ni nueva activación.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `417705e7-9e1b-cda9-ff5a-2a09ef771f6d`; duración `00:00:00.0040000`.

### PU-284. Rechazar versión duplicada

- **Clase:** `DominoAllFives.Server.Tests.MatchUpdateQueueTests`.
- **Caso ejecutable:** `Enqueue_DuplicateVersion_KeepsOnlyOriginalUpdate`.
- **Origen:** [MatchUpdateQueueTests.cs](../DominoAllFives.Server/Tests/MatchUpdateQueueTests.cs#L166).
- **Datos:** Sin parámetros; preparación descrita abajo.
- **Precondiciones:** Cola de partida 1 con versión 1 registrada; se intenta registrar otra versión 1.
- **Postcondiciones:** Se conserva exactamente el snapshot original.
- **Resultado esperado:** InvalidOperationException, identidad original y cola vacía después de consumirla.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `50d4951d-5892-7399-eef5-87ec6ceee2fc`; duración `00:00:00.0010000`.

### PU-285. Rechazar snapshot inválido

- **Clase:** `DominoAllFives.Server.Tests.MatchUpdateQueueTests`.
- **Caso ejecutable:** `Enqueue_InvalidSnapshot_RejectsWithoutScheduling(0,System.ArgumentNullException)`.
- **Origen:** [MatchUpdateQueueTests.cs](../DominoAllFives.Server/Tests/MatchUpdateQueueTests.cs#L97).
- **Datos:** Argumentos de esta ejecución: `(0,System.ArgumentNullException)`.
- **Precondiciones:** Cola vacía de partida 1. Actualización null.
- **Postcondiciones:** No se registra trabajo ni se adquiere consumidor.
- **Resultado esperado:** ArgumentNullException y TryStartPublishing false.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `573a3505-83bd-4c08-8913-ebea861f35ae`; duración `00:00:00.0010000`.

### PU-286. Rechazar snapshot inválido

- **Clase:** `DominoAllFives.Server.Tests.MatchUpdateQueueTests`.
- **Caso ejecutable:** `Enqueue_InvalidSnapshot_RejectsWithoutScheduling(1,System.ArgumentException)`.
- **Origen:** [MatchUpdateQueueTests.cs](../DominoAllFives.Server/Tests/MatchUpdateQueueTests.cs#L97).
- **Datos:** Argumentos de esta ejecución: `(1,System.ArgumentException)`.
- **Precondiciones:** Cola vacía de partida 1. Actualización con View null.
- **Postcondiciones:** No se registra trabajo ni se adquiere consumidor.
- **Resultado esperado:** ArgumentException y TryStartPublishing false.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `be034c76-952c-7daa-a3f7-191ca3053d75`; duración `00:00:00.0000001`.

### PU-287. Rechazar snapshot inválido

- **Clase:** `DominoAllFives.Server.Tests.MatchUpdateQueueTests`.
- **Caso ejecutable:** `Enqueue_InvalidSnapshot_RejectsWithoutScheduling(2,System.ArgumentException)`.
- **Origen:** [MatchUpdateQueueTests.cs](../DominoAllFives.Server/Tests/MatchUpdateQueueTests.cs#L97).
- **Datos:** Argumentos de esta ejecución: `(2,System.ArgumentException)`.
- **Precondiciones:** Cola vacía de partida 1. Actualización de versión 1 con MatchId 2.
- **Postcondiciones:** No se registra trabajo ni se adquiere consumidor.
- **Resultado esperado:** ArgumentException y TryStartPublishing false.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `2bf5ef1d-c1d1-2a37-e35a-7b4aabc7db6d`; duración `00:00:00.0000001`.

### PU-288. Rechazar snapshot inválido

- **Clase:** `DominoAllFives.Server.Tests.MatchUpdateQueueTests`.
- **Caso ejecutable:** `Enqueue_InvalidSnapshot_RejectsWithoutScheduling(3,System.ArgumentException)`.
- **Origen:** [MatchUpdateQueueTests.cs](../DominoAllFives.Server/Tests/MatchUpdateQueueTests.cs#L97).
- **Datos:** Argumentos de esta ejecución: `(3,System.ArgumentException)`.
- **Precondiciones:** Cola vacía de partida 1. Actualización de partida 1, versión 1, con HasStateChange false.
- **Postcondiciones:** No se registra trabajo ni se adquiere consumidor.
- **Resultado esperado:** ArgumentException y TryStartPublishing false.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `c372afec-76c0-2607-ef8e-43e6c6f71ab6`; duración `00:00:00.0000001`.

### PU-289. Rechazar salto de versión sin avanzar la secuencia

- **Clase:** `DominoAllFives.Server.Tests.MatchUpdateQueueTests`.
- **Caso ejecutable:** `Enqueue_NonConsecutiveVersion_PreservesExpectedVersion(-1L)`.
- **Origen:** [MatchUpdateQueueTests.cs](../DominoAllFives.Server/Tests/MatchUpdateQueueTests.cs#L139).
- **Datos:** Argumentos de esta ejecución: `(-1L)`.
- **Precondiciones:** Cola nueva de partida 1; se intenta registrar la versión indicada y después la versión válida 1.
- **Postcondiciones:** La versión inválida no altera la secuencia; la versión 1 se consume con su identidad original.
- **Resultado esperado:** InvalidOperationException para la versión indicada; después se acepta y consume el snapshot 1.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `4839f12b-6648-d65f-ab32-45777ec2f50d`; duración `00:00:00.0010000`.

### PU-290. Rechazar salto de versión sin avanzar la secuencia

- **Clase:** `DominoAllFives.Server.Tests.MatchUpdateQueueTests`.
- **Caso ejecutable:** `Enqueue_NonConsecutiveVersion_PreservesExpectedVersion(0L)`.
- **Origen:** [MatchUpdateQueueTests.cs](../DominoAllFives.Server/Tests/MatchUpdateQueueTests.cs#L139).
- **Datos:** Argumentos de esta ejecución: `(0L)`.
- **Precondiciones:** Cola nueva de partida 1; se intenta registrar la versión indicada y después la versión válida 1.
- **Postcondiciones:** La versión inválida no altera la secuencia; la versión 1 se consume con su identidad original.
- **Resultado esperado:** InvalidOperationException para la versión indicada; después se acepta y consume el snapshot 1.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `0704f103-9f40-7416-5020-9c0593f98b30`; duración `00:00:00.0000001`.

### PU-291. Rechazar salto de versión sin avanzar la secuencia

- **Clase:** `DominoAllFives.Server.Tests.MatchUpdateQueueTests`.
- **Caso ejecutable:** `Enqueue_NonConsecutiveVersion_PreservesExpectedVersion(2L)`.
- **Origen:** [MatchUpdateQueueTests.cs](../DominoAllFives.Server/Tests/MatchUpdateQueueTests.cs#L139).
- **Datos:** Argumentos de esta ejecución: `(2L)`.
- **Precondiciones:** Cola nueva de partida 1; se intenta registrar la versión indicada y después la versión válida 1.
- **Postcondiciones:** La versión inválida no altera la secuencia; la versión 1 se consume con su identidad original.
- **Resultado esperado:** InvalidOperationException para la versión indicada; después se acepta y consume el snapshot 1.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `43c02b4d-3ceb-4250-3d94-4a29709a95da`; duración `00:00:00.0000001`.

### PU-292. Mantener colas independientes por partida

- **Clase:** `DominoAllFives.Server.Tests.MatchUpdateQueueTests`.
- **Caso ejecutable:** `TryDequeue_DifferentMatches_PreservesIndependentQueues`.
- **Origen:** [MatchUpdateQueueTests.cs](../DominoAllFives.Server/Tests/MatchUpdateQueueTests.cs#L291).
- **Datos:** Sin parámetros; preparación descrita abajo.
- **Precondiciones:** Dos colas reales de partidas 1 y 2 con snapshot versión 1 de su propia partida y consumidor adquirido.
- **Postcondiciones:** Cada cola entrega exclusivamente su propio snapshot.
- **Resultado esperado:** Identidad original de ambos snapshots, sin cruce entre partidas.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `d1446e0e-7b57-c8be-0bf0-fec41b90fa3d`; duración `00:00:00.0010000`.

### PU-293. Conservar datos sin consumidor adquirido

- **Clase:** `DominoAllFives.Server.Tests.MatchUpdateQueueTests`.
- **Caso ejecutable:** `TryDequeue_WithoutConsumer_KeepsPendingUpdate`.
- **Origen:** [MatchUpdateQueueTests.cs](../DominoAllFives.Server/Tests/MatchUpdateQueueTests.cs#L48).
- **Datos:** Sin parámetros; preparación descrita abajo.
- **Precondiciones:** Cola de partida 1 con snapshot 1; todavía no se adquirió consumidor.
- **Postcondiciones:** El intento inicial no extrae datos; después de adquirir consumidor se entrega el snapshot original.
- **Resultado esperado:** False y null antes de la adquisición; adquisición true y consumo true con el snapshot original.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `10be9ea3-b8ce-c58a-4975-aebd62c6f4b9`; duración `00:00:00.0010000`.

### PU-294. Incorporar trabajo al consumidor activo

- **Clase:** `DominoAllFives.Server.Tests.MatchUpdateQueueTests`.
- **Caso ejecutable:** `TryStartPublishing_ActiveConsumer_DrainsNewWorkWithoutAnotherConsumer`.
- **Origen:** [MatchUpdateQueueTests.cs](../DominoAllFives.Server/Tests/MatchUpdateQueueTests.cs#L248).
- **Datos:** Sin parámetros; preparación descrita abajo.
- **Precondiciones:** Consumidor adquirido tras versión 1; se consume esa versión sin consultar aún el final de la cola.
- **Postcondiciones:** La versión 2 agregada se entrega por el mismo consumidor.
- **Resultado esperado:** Nueva adquisición false; consumo true con identidad del snapshot 2.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `2056addf-c143-b373-2b01-1e448bea6346`; duración `00:00:00.0010000`.

### PU-295. Reactivar una cola drenada

- **Clase:** `DominoAllFives.Server.Tests.MatchUpdateQueueTests`.
- **Caso ejecutable:** `TryStartPublishing_AfterDrain_AcquiresNewWork`.
- **Origen:** [MatchUpdateQueueTests.cs](../DominoAllFives.Server/Tests/MatchUpdateQueueTests.cs#L227).
- **Datos:** Sin parámetros; preparación descrita abajo.
- **Precondiciones:** Versión 1 consumida y fin de cola consultado; se prepara snapshot versión 2.
- **Postcondiciones:** El nuevo trabajo permite adquirir otro consumidor y se entrega una vez.
- **Resultado esperado:** Adquisición true, consumo true y snapshot original 2.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `cdf0e01a-2f50-6fbe-d06e-321307281d17`; duración `00:00:00.0010000`.

### PU-296. Adquirir un solo consumidor ante solicitudes concurrentes

- **Clase:** `DominoAllFives.Server.Tests.MatchUpdateQueueTests`.
- **Caso ejecutable:** `TryStartPublishing_ConcurrentRequests_AcquiresExactlyOneConsumer`.
- **Origen:** [MatchUpdateQueueTests.cs](../DominoAllFives.Server/Tests/MatchUpdateQueueTests.cs#L268).
- **Datos:** Sin parámetros; preparación descrita abajo.
- **Precondiciones:** Cola con versión 1; dos solicitudes concretas compiten mediante Parallel.For.
- **Postcondiciones:** Solo una solicitud adquiere la publicación.
- **Resultado esperado:** Exactamente un resultado true entre las dos solicitudes.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `41e21793-7b08-0eb6-17b0-1b3460b9c0ce`; duración `00:00:00.0030000`.

### PU-297. No activar una cola vacía

- **Clase:** `DominoAllFives.Server.Tests.MatchUpdateQueueTests`.
- **Caso ejecutable:** `TryStartPublishing_EmptyQueue_DoesNotAcquireConsumer`.
- **Origen:** [MatchUpdateQueueTests.cs](../DominoAllFives.Server/Tests/MatchUpdateQueueTests.cs#L33).
- **Datos:** Sin parámetros; preparación descrita abajo.
- **Precondiciones:** Cola nueva de partida 1 sin snapshots.
- **Postcondiciones:** No se adquiere consumidor ni se extraen datos.
- **Resultado esperado:** Adquisición false, extracción false y actualización null.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `d389d72f-5bbe-25a7-d07c-8c26e1787cf6`; duración `00:00:00.0010000`.

### PU-298. Drenar en orden aunque active una actualización posterior

- **Clase:** `DominoAllFives.Server.Tests.MatchUpdateQueueTests`.
- **Caso ejecutable:** `TryStartPublishing_LaterUpdateActivatesQueue_PreservesCommittedOrder`.
- **Origen:** [MatchUpdateQueueTests.cs](../DominoAllFives.Server/Tests/MatchUpdateQueueTests.cs#L67).
- **Datos:** Sin parámetros; preparación descrita abajo.
- **Precondiciones:** Se registran snapshot 1 y terminal 2 antes de activar mediante la referencia de publicación del terminal.
- **Postcondiciones:** Se entregan 1 y 2 en ese orden; un intento tardío desde 1 no recrea trabajo.
- **Resultado esperado:** Adquisición inicial true; ambos snapshots con identidad y orden originales; fin de cola false/null y activación tardía false.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `e3779728-2b8e-a119-f62a-8fc7c3a6e20c`; duración `00:00:00.0010000`.

### PU-299. Conservar callback sin respuesta

- **Clase:** `DominoAllFives.Server.Tests.MatchContractTests`.
- **Caso ejecutable:** `CallbackContract_Operation_ReturnsVoidOneWay("OnMatchChanged")`.
- **Origen:** [MatchContractTests.cs](../DominoAllFives.Server/Tests/MatchContractTests.cs#L125).
- **Datos:** Argumentos exactos de esta ejecución: `("OnMatchChanged")`.
- **Precondiciones:** Se obtiene el método indicado de IMatchCallback mediante reflexión.
- **Postcondiciones:** El contrato se conserva intacto.
- **Resultado esperado:** Retorno void e IsOneWay true.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `dc4055d9-325a-c25e-2a34-23d12b127d58`; duración `00:00:00.0010000`.

### PU-300. Conservar callback sin respuesta

- **Clase:** `DominoAllFives.Server.Tests.MatchContractTests`.
- **Caso ejecutable:** `CallbackContract_Operation_ReturnsVoidOneWay("OnMatchFinished")`.
- **Origen:** [MatchContractTests.cs](../DominoAllFives.Server/Tests/MatchContractTests.cs#L125).
- **Datos:** Argumentos exactos de esta ejecución: `("OnMatchFinished")`.
- **Precondiciones:** Se obtiene el método indicado de IMatchCallback mediante reflexión.
- **Postcondiciones:** El contrato se conserva intacto.
- **Resultado esperado:** Retorno void e IsOneWay true.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `82277b8e-6bde-77c0-8501-6054123205fa`; duración `00:00:00.0000001`.

### PU-301. Conservar callback sin respuesta

- **Clase:** `DominoAllFives.Server.Tests.MatchContractTests`.
- **Caso ejecutable:** `CallbackContract_Operation_ReturnsVoidOneWay("OnPrivateHandChanged")`.
- **Origen:** [MatchContractTests.cs](../DominoAllFives.Server/Tests/MatchContractTests.cs#L125).
- **Datos:** Argumentos exactos de esta ejecución: `("OnPrivateHandChanged")`.
- **Precondiciones:** Se obtiene el método indicado de IMatchCallback mediante reflexión.
- **Postcondiciones:** El contrato se conserva intacto.
- **Resultado esperado:** Retorno void e IsOneWay true.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `496fedeb-2221-cf0c-cedd-27e2dc3589b4`; duración `00:00:00.0010000`.

### PU-302. Conservar callback sin respuesta

- **Clase:** `DominoAllFives.Server.Tests.MatchContractTests`.
- **Caso ejecutable:** `CallbackContract_Operation_ReturnsVoidOneWay("OnRoundFinished")`.
- **Origen:** [MatchContractTests.cs](../DominoAllFives.Server/Tests/MatchContractTests.cs#L125).
- **Datos:** Argumentos exactos de esta ejecución: `("OnRoundFinished")`.
- **Precondiciones:** Se obtiene el método indicado de IMatchCallback mediante reflexión.
- **Postcondiciones:** El contrato se conserva intacto.
- **Resultado esperado:** Retorno void e IsOneWay true.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `657e67c3-4b5b-deaa-61d0-a1ab32bdca84`; duración `00:00:00.0000001`.

### PU-303. Declarar todos los valores de enumeración

- **Clase:** `DominoAllFives.Server.Tests.MatchContractTests`.
- **Caso ejecutable:** `DataContract_Enum_MarksEveryValue(DominoAllFives.Contracts.Gameplay.BoardEnd)`.
- **Origen:** [MatchContractTests.cs](../DominoAllFives.Server/Tests/MatchContractTests.cs#L49).
- **Datos:** Argumentos exactos de esta ejecución: `(DominoAllFives.Contracts.Gameplay.BoardEnd)`.
- **Precondiciones:** Se inspecciona la enumeración concreta indicada en los datos mediante reflexión.
- **Postcondiciones:** No se modifica el contrato ni sus valores.
- **Resultado esperado:** DataContract presente y EnumMember en cada valor declarado.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `d3641f71-46f8-a2b5-9208-7321d1336a05`; duración `00:00:00.0010000`.

### PU-304. Declarar todos los valores de enumeración

- **Clase:** `DominoAllFives.Server.Tests.MatchContractTests`.
- **Caso ejecutable:** `DataContract_Enum_MarksEveryValue(DominoAllFives.Contracts.Gameplay.MatchIssue)`.
- **Origen:** [MatchContractTests.cs](../DominoAllFives.Server/Tests/MatchContractTests.cs#L49).
- **Datos:** Argumentos exactos de esta ejecución: `(DominoAllFives.Contracts.Gameplay.MatchIssue)`.
- **Precondiciones:** Se inspecciona la enumeración concreta indicada en los datos mediante reflexión.
- **Postcondiciones:** No se modifica el contrato ni sus valores.
- **Resultado esperado:** DataContract presente y EnumMember en cada valor declarado.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `80fcebc0-2f33-8950-76cd-d8000bb26b2f`; duración `00:00:00.0000001`.

### PU-305. Declarar todos los valores de enumeración

- **Clase:** `DominoAllFives.Server.Tests.MatchContractTests`.
- **Caso ejecutable:** `DataContract_Enum_MarksEveryValue(DominoAllFives.Contracts.Gameplay.MatchStatus)`.
- **Origen:** [MatchContractTests.cs](../DominoAllFives.Server/Tests/MatchContractTests.cs#L49).
- **Datos:** Argumentos exactos de esta ejecución: `(DominoAllFives.Contracts.Gameplay.MatchStatus)`.
- **Precondiciones:** Se inspecciona la enumeración concreta indicada en los datos mediante reflexión.
- **Postcondiciones:** No se modifica el contrato ni sus valores.
- **Resultado esperado:** DataContract presente y EnumMember en cada valor declarado.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `5c53a82a-7b1c-1ae8-ed50-41c5896be7f1`; duración `00:00:00.0000001`.

### PU-306. Declarar todos los valores de enumeración

- **Clase:** `DominoAllFives.Server.Tests.MatchContractTests`.
- **Caso ejecutable:** `DataContract_Enum_MarksEveryValue(DominoAllFives.Contracts.Gameplay.SpecialPower)`.
- **Origen:** [MatchContractTests.cs](../DominoAllFives.Server/Tests/MatchContractTests.cs#L49).
- **Datos:** Argumentos exactos de esta ejecución: `(DominoAllFives.Contracts.Gameplay.SpecialPower)`.
- **Precondiciones:** Se inspecciona la enumeración concreta indicada en los datos mediante reflexión.
- **Postcondiciones:** No se modifica el contrato ni sus valores.
- **Resultado esperado:** DataContract presente y EnumMember en cada valor declarado.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `57c63981-da36-d958-a720-195126a04b9e`; duración `00:00:00.0000001`.

### PU-307. Declarar modelo con miembros simples

- **Clase:** `DominoAllFives.Server.Tests.MatchContractTests`.
- **Caso ejecutable:** `DataContract_Model_DeclaresSimpleMembers(DominoAllFives.Contracts.Gameplay.DominoTile)`.
- **Origen:** [MatchContractTests.cs](../DominoAllFives.Server/Tests/MatchContractTests.cs#L26).
- **Datos:** Argumentos exactos de esta ejecución: `(DominoAllFives.Contracts.Gameplay.DominoTile)`.
- **Precondiciones:** Se inspecciona el tipo de modelo concreto indicado en los datos mediante reflexión.
- **Postcondiciones:** Los metadatos del modelo permanecen intactos.
- **Resultado esperado:** DataContract presente; cada propiedad pública tiene DataMember, Order -1 e IsRequired false.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `724a3a2d-d670-802d-6107-fe7072de169b`; duración `00:00:00.0010000`.

### PU-308. Declarar modelo con miembros simples

- **Clase:** `DominoAllFives.Server.Tests.MatchContractTests`.
- **Caso ejecutable:** `DataContract_Model_DeclaresSimpleMembers(DominoAllFives.Contracts.Gameplay.MatchResult)`.
- **Origen:** [MatchContractTests.cs](../DominoAllFives.Server/Tests/MatchContractTests.cs#L26).
- **Datos:** Argumentos exactos de esta ejecución: `(DominoAllFives.Contracts.Gameplay.MatchResult)`.
- **Precondiciones:** Se inspecciona el tipo de modelo concreto indicado en los datos mediante reflexión.
- **Postcondiciones:** Los metadatos del modelo permanecen intactos.
- **Resultado esperado:** DataContract presente; cada propiedad pública tiene DataMember, Order -1 e IsRequired false.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `a1ba377e-a855-0e2e-7e47-9b808f876ec4`; duración `00:00:00.0000001`.

### PU-309. Declarar modelo con miembros simples

- **Clase:** `DominoAllFives.Server.Tests.MatchContractTests`.
- **Caso ejecutable:** `DataContract_Model_DeclaresSimpleMembers(DominoAllFives.Contracts.Gameplay.MatchView)`.
- **Origen:** [MatchContractTests.cs](../DominoAllFives.Server/Tests/MatchContractTests.cs#L26).
- **Datos:** Argumentos exactos de esta ejecución: `(DominoAllFives.Contracts.Gameplay.MatchView)`.
- **Precondiciones:** Se inspecciona el tipo de modelo concreto indicado en los datos mediante reflexión.
- **Postcondiciones:** Los metadatos del modelo permanecen intactos.
- **Resultado esperado:** DataContract presente; cada propiedad pública tiene DataMember, Order -1 e IsRequired false.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `376e4c23-14e6-5022-b57c-d9545cb8bac9`; duración `00:00:00.0000001`.

### PU-310. Declarar modelo con miembros simples

- **Clase:** `DominoAllFives.Server.Tests.MatchContractTests`.
- **Caso ejecutable:** `DataContract_Model_DeclaresSimpleMembers(DominoAllFives.Contracts.Gameplay.PlacedTile)`.
- **Origen:** [MatchContractTests.cs](../DominoAllFives.Server/Tests/MatchContractTests.cs#L26).
- **Datos:** Argumentos exactos de esta ejecución: `(DominoAllFives.Contracts.Gameplay.PlacedTile)`.
- **Precondiciones:** Se inspecciona el tipo de modelo concreto indicado en los datos mediante reflexión.
- **Postcondiciones:** Los metadatos del modelo permanecen intactos.
- **Resultado esperado:** DataContract presente; cada propiedad pública tiene DataMember, Order -1 e IsRequired false.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `c21f0297-0e0c-0bb6-ce58-1292cf00ca70`; duración `00:00:00.0000001`.

### PU-311. Declarar modelo con miembros simples

- **Clase:** `DominoAllFives.Server.Tests.MatchContractTests`.
- **Caso ejecutable:** `DataContract_Model_DeclaresSimpleMembers(DominoAllFives.Contracts.Gameplay.PlayerHand)`.
- **Origen:** [MatchContractTests.cs](../DominoAllFives.Server/Tests/MatchContractTests.cs#L26).
- **Datos:** Argumentos exactos de esta ejecución: `(DominoAllFives.Contracts.Gameplay.PlayerHand)`.
- **Precondiciones:** Se inspecciona el tipo de modelo concreto indicado en los datos mediante reflexión.
- **Postcondiciones:** Los metadatos del modelo permanecen intactos.
- **Resultado esperado:** DataContract presente; cada propiedad pública tiene DataMember, Order -1 e IsRequired false.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `f744c6d9-6f5d-a492-1e19-381022dc699b`; duración `00:00:00.0000001`.

### PU-312. Declarar modelo con miembros simples

- **Clase:** `DominoAllFives.Server.Tests.MatchContractTests`.
- **Caso ejecutable:** `DataContract_Model_DeclaresSimpleMembers(DominoAllFives.Contracts.Gameplay.PlayerScore)`.
- **Origen:** [MatchContractTests.cs](../DominoAllFives.Server/Tests/MatchContractTests.cs#L26).
- **Datos:** Argumentos exactos de esta ejecución: `(DominoAllFives.Contracts.Gameplay.PlayerScore)`.
- **Precondiciones:** Se inspecciona el tipo de modelo concreto indicado en los datos mediante reflexión.
- **Postcondiciones:** Los metadatos del modelo permanecen intactos.
- **Resultado esperado:** DataContract presente; cada propiedad pública tiene DataMember, Order -1 e IsRequired false.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `ff337003-edd1-0552-8cad-dc36cb62cfe7`; duración `00:00:00.0000001`.

### PU-313. Declarar modelo con miembros simples

- **Clase:** `DominoAllFives.Server.Tests.MatchContractTests`.
- **Caso ejecutable:** `DataContract_Model_DeclaresSimpleMembers(DominoAllFives.Contracts.Gameplay.PlayScore)`.
- **Origen:** [MatchContractTests.cs](../DominoAllFives.Server/Tests/MatchContractTests.cs#L26).
- **Datos:** Argumentos exactos de esta ejecución: `(DominoAllFives.Contracts.Gameplay.PlayScore)`.
- **Precondiciones:** Se inspecciona el tipo de modelo concreto indicado en los datos mediante reflexión.
- **Postcondiciones:** Los metadatos del modelo permanecen intactos.
- **Resultado esperado:** DataContract presente; cada propiedad pública tiene DataMember, Order -1 e IsRequired false.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `47f1eb36-8be5-d6b4-b794-ca11349acd8e`; duración `00:00:00.0010000`.

### PU-314. Declarar modelo con miembros simples

- **Clase:** `DominoAllFives.Server.Tests.MatchContractTests`.
- **Caso ejecutable:** `DataContract_Model_DeclaresSimpleMembers(DominoAllFives.Contracts.Gameplay.PublicBoard)`.
- **Origen:** [MatchContractTests.cs](../DominoAllFives.Server/Tests/MatchContractTests.cs#L26).
- **Datos:** Argumentos exactos de esta ejecución: `(DominoAllFives.Contracts.Gameplay.PublicBoard)`.
- **Precondiciones:** Se inspecciona el tipo de modelo concreto indicado en los datos mediante reflexión.
- **Postcondiciones:** Los metadatos del modelo permanecen intactos.
- **Resultado esperado:** DataContract presente; cada propiedad pública tiene DataMember, Order -1 e IsRequired false.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `01e78d68-fb82-1b8d-fb87-bd007b93863c`; duración `00:00:00.0000001`.

### PU-315. Declarar modelo con miembros simples

- **Clase:** `DominoAllFives.Server.Tests.MatchContractTests`.
- **Caso ejecutable:** `DataContract_Model_DeclaresSimpleMembers(DominoAllFives.Contracts.Gameplay.PublicEnd)`.
- **Origen:** [MatchContractTests.cs](../DominoAllFives.Server/Tests/MatchContractTests.cs#L26).
- **Datos:** Argumentos exactos de esta ejecución: `(DominoAllFives.Contracts.Gameplay.PublicEnd)`.
- **Precondiciones:** Se inspecciona el tipo de modelo concreto indicado en los datos mediante reflexión.
- **Postcondiciones:** Los metadatos del modelo permanecen intactos.
- **Resultado esperado:** DataContract presente; cada propiedad pública tiene DataMember, Order -1 e IsRequired false.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `7aa5afba-9510-874d-b672-8e4dd99be7fa`; duración `00:00:00.0000001`.

### PU-316. Declarar modelo con miembros simples

- **Clase:** `DominoAllFives.Server.Tests.MatchContractTests`.
- **Caso ejecutable:** `DataContract_Model_DeclaresSimpleMembers(DominoAllFives.Contracts.Gameplay.RoundResult)`.
- **Origen:** [MatchContractTests.cs](../DominoAllFives.Server/Tests/MatchContractTests.cs#L26).
- **Datos:** Argumentos exactos de esta ejecución: `(DominoAllFives.Contracts.Gameplay.RoundResult)`.
- **Precondiciones:** Se inspecciona el tipo de modelo concreto indicado en los datos mediante reflexión.
- **Postcondiciones:** Los metadatos del modelo permanecen intactos.
- **Resultado esperado:** DataContract presente; cada propiedad pública tiene DataMember, Order -1 e IsRequired false.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `6723e7e8-20ca-0e9d-e489-700c773c1a70`; duración `00:00:00.0000001`.

### PU-317. Conservar los faults de una operación

- **Clase:** `DominoAllFives.Server.Tests.MatchContractTests`.
- **Caso ejecutable:** `ServiceContract_Operation_DeclaresAccessAndMatchFaults("DrawTiles")`.
- **Origen:** [MatchContractTests.cs](../DominoAllFives.Server/Tests/MatchContractTests.cs#L107).
- **Datos:** Argumentos exactos de esta ejecución: `("DrawTiles")`.
- **Precondiciones:** Se obtiene el método indicado de IMatchService del servidor mediante reflexión.
- **Postcondiciones:** Se inspeccionan únicamente los atributos declarados.
- **Resultado esperado:** Exactamente los tipos AccessIssue y MatchIssue, sin depender de su orden.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `ed95f3ef-5782-d5b4-929d-6eae6190393c`; duración `00:00:00.0000001`.

### PU-318. Conservar los faults de una operación

- **Clase:** `DominoAllFives.Server.Tests.MatchContractTests`.
- **Caso ejecutable:** `ServiceContract_Operation_DeclaresAccessAndMatchFaults("GetHand")`.
- **Origen:** [MatchContractTests.cs](../DominoAllFives.Server/Tests/MatchContractTests.cs#L107).
- **Datos:** Argumentos exactos de esta ejecución: `("GetHand")`.
- **Precondiciones:** Se obtiene el método indicado de IMatchService del servidor mediante reflexión.
- **Postcondiciones:** Se inspeccionan únicamente los atributos declarados.
- **Resultado esperado:** Exactamente los tipos AccessIssue y MatchIssue, sin depender de su orden.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `398c784e-ba3d-f64e-b8a7-071a8ce84a79`; duración `00:00:00.0000001`.

### PU-319. Conservar los faults de una operación

- **Clase:** `DominoAllFives.Server.Tests.MatchContractTests`.
- **Caso ejecutable:** `ServiceContract_Operation_DeclaresAccessAndMatchFaults("GetMatch")`.
- **Origen:** [MatchContractTests.cs](../DominoAllFives.Server/Tests/MatchContractTests.cs#L107).
- **Datos:** Argumentos exactos de esta ejecución: `("GetMatch")`.
- **Precondiciones:** Se obtiene el método indicado de IMatchService del servidor mediante reflexión.
- **Postcondiciones:** Se inspeccionan únicamente los atributos declarados.
- **Resultado esperado:** Exactamente los tipos AccessIssue y MatchIssue, sin depender de su orden.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `e014c159-5192-f4a9-ed35-1dbf8e296eca`; duración `00:00:00.0000001`.

### PU-320. Conservar los faults de una operación

- **Clase:** `DominoAllFives.Server.Tests.MatchContractTests`.
- **Caso ejecutable:** `ServiceContract_Operation_DeclaresAccessAndMatchFaults("LeaveMatch")`.
- **Origen:** [MatchContractTests.cs](../DominoAllFives.Server/Tests/MatchContractTests.cs#L107).
- **Datos:** Argumentos exactos de esta ejecución: `("LeaveMatch")`.
- **Precondiciones:** Se obtiene el método indicado de IMatchService del servidor mediante reflexión.
- **Postcondiciones:** Se inspeccionan únicamente los atributos declarados.
- **Resultado esperado:** Exactamente los tipos AccessIssue y MatchIssue, sin depender de su orden.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `65423300-193d-048d-6d5b-a2a156a9bbd7`; duración `00:00:00.0000001`.

### PU-321. Conservar los faults de una operación

- **Clase:** `DominoAllFives.Server.Tests.MatchContractTests`.
- **Caso ejecutable:** `ServiceContract_Operation_DeclaresAccessAndMatchFaults("OpenMatchSession")`.
- **Origen:** [MatchContractTests.cs](../DominoAllFives.Server/Tests/MatchContractTests.cs#L107).
- **Datos:** Argumentos exactos de esta ejecución: `("OpenMatchSession")`.
- **Precondiciones:** Se obtiene el método indicado de IMatchService del servidor mediante reflexión.
- **Postcondiciones:** Se inspeccionan únicamente los atributos declarados.
- **Resultado esperado:** Exactamente los tipos AccessIssue y MatchIssue, sin depender de su orden.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `ffaf99c5-b3c5-3577-4e17-5a049ce41767`; duración `00:00:00.0230000`.

### PU-322. Conservar los faults de una operación

- **Clase:** `DominoAllFives.Server.Tests.MatchContractTests`.
- **Caso ejecutable:** `ServiceContract_Operation_DeclaresAccessAndMatchFaults("PassTurn")`.
- **Origen:** [MatchContractTests.cs](../DominoAllFives.Server/Tests/MatchContractTests.cs#L107).
- **Datos:** Argumentos exactos de esta ejecución: `("PassTurn")`.
- **Precondiciones:** Se obtiene el método indicado de IMatchService del servidor mediante reflexión.
- **Postcondiciones:** Se inspeccionan únicamente los atributos declarados.
- **Resultado esperado:** Exactamente los tipos AccessIssue y MatchIssue, sin depender de su orden.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `280c4da2-8ddb-970b-440e-61cb347e0065`; duración `00:00:00.0000001`.

### PU-323. Conservar los faults de una operación

- **Clase:** `DominoAllFives.Server.Tests.MatchContractTests`.
- **Caso ejecutable:** `ServiceContract_Operation_DeclaresAccessAndMatchFaults("PlayTile")`.
- **Origen:** [MatchContractTests.cs](../DominoAllFives.Server/Tests/MatchContractTests.cs#L107).
- **Datos:** Argumentos exactos de esta ejecución: `("PlayTile")`.
- **Precondiciones:** Se obtiene el método indicado de IMatchService del servidor mediante reflexión.
- **Postcondiciones:** Se inspeccionan únicamente los atributos declarados.
- **Resultado esperado:** Exactamente los tipos AccessIssue y MatchIssue, sin depender de su orden.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `f49f4f35-90c5-5f29-05ad-8c6ed407651d`; duración `00:00:00.0000001`.

### PU-324. Conservar respuesta sin snapshot en comando publicado

- **Clase:** `DominoAllFives.Server.Tests.MatchContractTests`.
- **Caso ejecutable:** `ServiceContract_PublishedOperation_ReturnsVoidWithFaultReply("DrawTiles")`.
- **Origen:** [MatchContractTests.cs](../DominoAllFives.Server/Tests/MatchContractTests.cs#L72).
- **Datos:** Argumentos exactos de esta ejecución: `("DrawTiles")`.
- **Precondiciones:** Se obtiene el método indicado de IMatchService del servidor mediante reflexión.
- **Postcondiciones:** La consulta de metadatos no modifica el contrato.
- **Resultado esperado:** Retorno void, IsOneWay false e IsInitiating false.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `7db073fa-13ad-e52c-f617-3c8599cc8889`; duración `00:00:00.0000001`.

### PU-325. Conservar respuesta sin snapshot en comando publicado

- **Clase:** `DominoAllFives.Server.Tests.MatchContractTests`.
- **Caso ejecutable:** `ServiceContract_PublishedOperation_ReturnsVoidWithFaultReply("LeaveMatch")`.
- **Origen:** [MatchContractTests.cs](../DominoAllFives.Server/Tests/MatchContractTests.cs#L72).
- **Datos:** Argumentos exactos de esta ejecución: `("LeaveMatch")`.
- **Precondiciones:** Se obtiene el método indicado de IMatchService del servidor mediante reflexión.
- **Postcondiciones:** La consulta de metadatos no modifica el contrato.
- **Resultado esperado:** Retorno void, IsOneWay false e IsInitiating false.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `caf9eadd-d8a7-f5f8-8478-f202f0e1c389`; duración `00:00:00.0000001`.

### PU-326. Conservar respuesta sin snapshot en comando publicado

- **Clase:** `DominoAllFives.Server.Tests.MatchContractTests`.
- **Caso ejecutable:** `ServiceContract_PublishedOperation_ReturnsVoidWithFaultReply("PassTurn")`.
- **Origen:** [MatchContractTests.cs](../DominoAllFives.Server/Tests/MatchContractTests.cs#L72).
- **Datos:** Argumentos exactos de esta ejecución: `("PassTurn")`.
- **Precondiciones:** Se obtiene el método indicado de IMatchService del servidor mediante reflexión.
- **Postcondiciones:** La consulta de metadatos no modifica el contrato.
- **Resultado esperado:** Retorno void, IsOneWay false e IsInitiating false.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `254eee62-32f2-098b-6483-4300ef68cae7`; duración `00:00:00.0000001`.

### PU-327. Conservar respuesta sin snapshot en comando publicado

- **Clase:** `DominoAllFives.Server.Tests.MatchContractTests`.
- **Caso ejecutable:** `ServiceContract_PublishedOperation_ReturnsVoidWithFaultReply("PlayTile")`.
- **Origen:** [MatchContractTests.cs](../DominoAllFives.Server/Tests/MatchContractTests.cs#L72).
- **Datos:** Argumentos exactos de esta ejecución: `("PlayTile")`.
- **Precondiciones:** Se obtiene el método indicado de IMatchService del servidor mediante reflexión.
- **Postcondiciones:** La consulta de metadatos no modifica el contrato.
- **Resultado esperado:** Retorno void, IsOneWay false e IsInitiating false.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `e5d3a5cc-5c0f-a954-4e89-9cc8e96ea919`; duración `00:00:00.0010000`.

### PU-328. Conservar el resultado de una consulta

- **Clase:** `DominoAllFives.Server.Tests.MatchContractTests`.
- **Caso ejecutable:** `ServiceContract_QueryOperation_ReturnsSnapshot("GetHand",DominoAllFives.Contracts.Gameplay.PlayerHand)`.
- **Origen:** [MatchContractTests.cs](../DominoAllFives.Server/Tests/MatchContractTests.cs#L88).
- **Datos:** Argumentos exactos de esta ejecución: `("GetHand",DominoAllFives.Contracts.Gameplay.PlayerHand)`.
- **Precondiciones:** Se obtiene el método indicado de IMatchService del servidor mediante reflexión.
- **Postcondiciones:** No se altera el estado de contrato.
- **Resultado esperado:** Retorno exactamente del tipo indicado en los datos.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `66f66509-3058-6198-842d-ad0a9bebbf34`; duración `00:00:00.0000001`.

### PU-329. Conservar el resultado de una consulta

- **Clase:** `DominoAllFives.Server.Tests.MatchContractTests`.
- **Caso ejecutable:** `ServiceContract_QueryOperation_ReturnsSnapshot("GetMatch",DominoAllFives.Contracts.Gameplay.MatchView)`.
- **Origen:** [MatchContractTests.cs](../DominoAllFives.Server/Tests/MatchContractTests.cs#L88).
- **Datos:** Argumentos exactos de esta ejecución: `("GetMatch",DominoAllFives.Contracts.Gameplay.MatchView)`.
- **Precondiciones:** Se obtiene el método indicado de IMatchService del servidor mediante reflexión.
- **Postcondiciones:** No se altera el estado de contrato.
- **Resultado esperado:** Retorno exactamente del tipo indicado en los datos.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `8f970fa4-18b9-158f-ed5a-254cca6e4ac0`; duración `00:00:00.0000001`.

### PU-330. Conservar el resultado de una consulta

- **Clase:** `DominoAllFives.Server.Tests.MatchContractTests`.
- **Caso ejecutable:** `ServiceContract_QueryOperation_ReturnsSnapshot("OpenMatchSession",DominoAllFives.Contracts.Gameplay.MatchView)`.
- **Origen:** [MatchContractTests.cs](../DominoAllFives.Server/Tests/MatchContractTests.cs#L88).
- **Datos:** Argumentos exactos de esta ejecución: `("OpenMatchSession",DominoAllFives.Contracts.Gameplay.MatchView)`.
- **Precondiciones:** Se obtiene el método indicado de IMatchService del servidor mediante reflexión.
- **Postcondiciones:** No se altera el estado de contrato.
- **Resultado esperado:** Retorno exactamente del tipo indicado en los datos.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `e9dcf9d5-cfce-56f8-3556-7f801485cdb4`; duración `00:00:00.0000001`.

### PU-331. Mantener independencia de los contratos del servidor

- **Clase:** `DominoAllFives.Client.Tests.MatchProxyTests`.
- **Caso ejecutable:** `ClientAssemblies_GeneratedProxy_DoNotReferenceServerContracts`.
- **Origen:** [MatchProxyTests.cs](../DominoAllFives.Client/DominoAllFives.Client.Tests/MatchProxyTests.cs#L52).
- **Datos:** Sin parámetros; ensamblados del cliente compilados en Debug.
- **Precondiciones:** Se inspeccionan ensamblados de aplicación, pruebas, proxy y DominoTile del cliente.
- **Postcondiciones:** No se ejecutan servicios ni se modifica el estado de la aplicación.
- **Resultado esperado:** Proxy y DominoTile pertenecen al ensamblado de aplicación; aplicación y pruebas no referencian DominoAllFives.Contracts.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-client-unit.trx`, caso `703b04d2-91cd-09cf-a788-86364945c8c1`; duración `00:00:00.1260000`.

### PU-332. Alinear la firma de comando del cliente

- **Clase:** `DominoAllFives.Client.Tests.MatchProxyTests`.
- **Caso ejecutable:** `ClientCommand_PublicSignature_ExposesNoSnapshot("Draw")`.
- **Origen:** [MatchProxyTests.cs](../DominoAllFives.Client/DominoAllFives.Client.Tests/MatchProxyTests.cs#L40).
- **Datos:** Argumentos exactos de esta ejecución: `("Draw")`.
- **Precondiciones:** Se obtienen los métodos indicados de IMatchClient y MatchClient mediante reflexión.
- **Postcondiciones:** No se crea cliente ni canal remoto.
- **Resultado esperado:** Ambos retornos son void.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-client-unit.trx`, caso `d2f5ce8e-8ee3-fd58-4ce7-42f656eedb7d`; duración `00:00:00.0000001`.

### PU-333. Alinear la firma de comando del cliente

- **Clase:** `DominoAllFives.Client.Tests.MatchProxyTests`.
- **Caso ejecutable:** `ClientCommand_PublicSignature_ExposesNoSnapshot("Pass")`.
- **Origen:** [MatchProxyTests.cs](../DominoAllFives.Client/DominoAllFives.Client.Tests/MatchProxyTests.cs#L40).
- **Datos:** Argumentos exactos de esta ejecución: `("Pass")`.
- **Precondiciones:** Se obtienen los métodos indicados de IMatchClient y MatchClient mediante reflexión.
- **Postcondiciones:** No se crea cliente ni canal remoto.
- **Resultado esperado:** Ambos retornos son void.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-client-unit.trx`, caso `299ef9cc-9c2d-a4a4-9f97-f039206a1b70`; duración `00:00:00.0000001`.

### PU-334. Alinear la firma de comando del cliente

- **Clase:** `DominoAllFives.Client.Tests.MatchProxyTests`.
- **Caso ejecutable:** `ClientCommand_PublicSignature_ExposesNoSnapshot("Play")`.
- **Origen:** [MatchProxyTests.cs](../DominoAllFives.Client/DominoAllFives.Client.Tests/MatchProxyTests.cs#L40).
- **Datos:** Argumentos exactos de esta ejecución: `("Play")`.
- **Precondiciones:** Se obtienen los métodos indicados de IMatchClient y MatchClient mediante reflexión.
- **Postcondiciones:** No se crea cliente ni canal remoto.
- **Resultado esperado:** Ambos retornos son void.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-client-unit.trx`, caso `6e9312d4-021c-0101-36ca-eceaf68e037d`; duración `00:00:00.0010000`.

### PU-335. Conservar comando y faults del proxy generado

- **Clase:** `DominoAllFives.Client.Tests.MatchProxyTests`.
- **Caso ejecutable:** `GeneratedContract_PublishedOperation_ReturnsVoidWithFaultReply("DrawTiles")`.
- **Origen:** [MatchProxyTests.cs](../DominoAllFives.Client/DominoAllFives.Client.Tests/MatchProxyTests.cs#L19).
- **Datos:** Argumentos exactos de esta ejecución: `("DrawTiles")`.
- **Precondiciones:** Se obtiene el método indicado de IMatchService generado en el cliente.
- **Postcondiciones:** No se crea canal ni se ejecuta comunicación.
- **Resultado esperado:** Retorno void, IsOneWay false y faults AccessIssue y MatchIssue del propio proxy.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-client-unit.trx`, caso `bff95221-8596-24a0-5516-3d410dc397a3`; duración `00:00:00.0000001`.

### PU-336. Conservar comando y faults del proxy generado

- **Clase:** `DominoAllFives.Client.Tests.MatchProxyTests`.
- **Caso ejecutable:** `GeneratedContract_PublishedOperation_ReturnsVoidWithFaultReply("LeaveMatch")`.
- **Origen:** [MatchProxyTests.cs](../DominoAllFives.Client/DominoAllFives.Client.Tests/MatchProxyTests.cs#L19).
- **Datos:** Argumentos exactos de esta ejecución: `("LeaveMatch")`.
- **Precondiciones:** Se obtiene el método indicado de IMatchService generado en el cliente.
- **Postcondiciones:** No se crea canal ni se ejecuta comunicación.
- **Resultado esperado:** Retorno void, IsOneWay false y faults AccessIssue y MatchIssue del propio proxy.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-client-unit.trx`, caso `088b2643-e682-42ae-9eb4-1a15625b47e6`; duración `00:00:00.0000001`.

### PU-337. Conservar comando y faults del proxy generado

- **Clase:** `DominoAllFives.Client.Tests.MatchProxyTests`.
- **Caso ejecutable:** `GeneratedContract_PublishedOperation_ReturnsVoidWithFaultReply("PassTurn")`.
- **Origen:** [MatchProxyTests.cs](../DominoAllFives.Client/DominoAllFives.Client.Tests/MatchProxyTests.cs#L19).
- **Datos:** Argumentos exactos de esta ejecución: `("PassTurn")`.
- **Precondiciones:** Se obtiene el método indicado de IMatchService generado en el cliente.
- **Postcondiciones:** No se crea canal ni se ejecuta comunicación.
- **Resultado esperado:** Retorno void, IsOneWay false y faults AccessIssue y MatchIssue del propio proxy.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-client-unit.trx`, caso `bd82d036-badf-0437-4688-b639048f52cc`; duración `00:00:00.0000001`.

### PU-338. Conservar comando y faults del proxy generado

- **Clase:** `DominoAllFives.Client.Tests.MatchProxyTests`.
- **Caso ejecutable:** `GeneratedContract_PublishedOperation_ReturnsVoidWithFaultReply("PlayTile")`.
- **Origen:** [MatchProxyTests.cs](../DominoAllFives.Client/DominoAllFives.Client.Tests/MatchProxyTests.cs#L19).
- **Datos:** Argumentos exactos de esta ejecución: `("PlayTile")`.
- **Precondiciones:** Se obtiene el método indicado de IMatchService generado en el cliente.
- **Postcondiciones:** No se crea canal ni se ejecuta comunicación.
- **Resultado esperado:** Retorno void, IsOneWay false y faults AccessIssue y MatchIssue del propio proxy.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-client-unit.trx`, caso `8aa6c024-952c-3c40-1f52-9be0f9251f8f`; duración `00:00:00.0840000`.

### PU-339. Separar registros mutables de la copia

- **Clase:** `DominoAllFives.Server.Tests.BoardCopyTests`.
- **Caso ejecutable:** `Copy_ChangedMutableRecord_PreservesOriginalRecord(False)`.
- **Origen:** [BoardCopyTests.cs](../DominoAllFives.Server/Tests/BoardCopyTests.cs#L70).
- **Datos:** Argumentos exactos de esta ejecución: `(False)`.
- **Precondiciones:** Tablero con doble 6-6 de bloqueo y su copia independiente. Se cambian ExposedValue y ConnectingValue de la primera colocación de la copia a 99.
- **Postcondiciones:** Solo los valores de la copia se sustituyen por 99.
- **Resultado esperado:** Captura completa del original idéntica a la anterior.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `82b581f4-dfe7-70c6-a021-befef4a7cc54`; duración `00:00:00.0100000`.

### PU-340. Separar registros mutables de la copia

- **Clase:** `DominoAllFives.Server.Tests.BoardCopyTests`.
- **Caso ejecutable:** `Copy_ChangedMutableRecord_PreservesOriginalRecord(True)`.
- **Origen:** [BoardCopyTests.cs](../DominoAllFives.Server/Tests/BoardCopyTests.cs#L70).
- **Datos:** Argumentos exactos de esta ejecución: `(True)`.
- **Precondiciones:** Tablero con doble 6-6 de bloqueo y su copia independiente. Se cambian Value y BlockedTurn del primer extremo de la copia a 99.
- **Postcondiciones:** Solo los valores de la copia se sustituyen por 99.
- **Resultado esperado:** Captura completa del original idéntica a la anterior.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `a236b33f-3c4a-ea10-566b-d2870694294c`; duración `00:00:00.0000001`.

### PU-341. Copiar tablero vacío

- **Clase:** `DominoAllFives.Server.Tests.BoardCopyTests`.
- **Caso ejecutable:** `Copy_EmptyBoard_ReturnsIndependentEmptyBoard`.
- **Origen:** [BoardCopyTests.cs](../DominoAllFives.Server/Tests/BoardCopyTests.cs#L11).
- **Datos:** Sin parámetros; datos concretos descritos en Precondiciones.
- **Precondiciones:** Tablero nuevo sin fichas ni extremos.
- **Postcondiciones:** Se obtiene otra instancia sin alterar el original.
- **Resultado esperado:** Referencia distinta, cero fichas y cero extremos.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `3390bba8-77f3-770a-1f57-c02cc829da38`; duración `00:00:00.0010000`.

### PU-342. Copiar campos del tablero y sus bloqueos

- **Clase:** `DominoAllFives.Server.Tests.BoardCopyTests`.
- **Caso ejecutable:** `Copy_ExistingBoard_PreservesEveryField(Block)`.
- **Origen:** [BoardCopyTests.cs](../DominoAllFives.Server/Tests/BoardCopyTests.cs#L29).
- **Datos:** Argumentos exactos de esta ejecución: `(Block)`.
- **Precondiciones:** Tablero abierto con doble 6-6 y el poder indicado en los datos.
- **Postcondiciones:** La copia conserva colocaciones, extremos y puntuación por turno.
- **Resultado esperado:** Mismos IDs, orientación, valores conectores y expuestos, valores puntuables, turnos de bloqueo y marcas de apertura; puntuación idéntica en turnos 0 y 1.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `f2a46df4-686a-e9f5-659e-d6c68f5582d6`; duración `00:00:00.0000001`.

### PU-343. Copiar campos del tablero y sus bloqueos

- **Clase:** `DominoAllFives.Server.Tests.BoardCopyTests`.
- **Caso ejecutable:** `Copy_ExistingBoard_PreservesEveryField(DoublePoints)`.
- **Origen:** [BoardCopyTests.cs](../DominoAllFives.Server/Tests/BoardCopyTests.cs#L29).
- **Datos:** Argumentos exactos de esta ejecución: `(DoublePoints)`.
- **Precondiciones:** Tablero abierto con doble 6-6 y el poder indicado en los datos.
- **Postcondiciones:** La copia conserva colocaciones, extremos y puntuación por turno.
- **Resultado esperado:** Mismos IDs, orientación, valores conectores y expuestos, valores puntuables, turnos de bloqueo y marcas de apertura; puntuación idéntica en turnos 0 y 1.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `2dfdf32c-2015-732e-21a5-bad1140a1d22`; duración `00:00:00.0000001`.

### PU-344. Copiar campos del tablero y sus bloqueos

- **Clase:** `DominoAllFives.Server.Tests.BoardCopyTests`.
- **Caso ejecutable:** `Copy_ExistingBoard_PreservesEveryField(None)`.
- **Origen:** [BoardCopyTests.cs](../DominoAllFives.Server/Tests/BoardCopyTests.cs#L29).
- **Datos:** Argumentos exactos de esta ejecución: `(None)`.
- **Precondiciones:** Tablero abierto con doble 6-6 y el poder indicado en los datos.
- **Postcondiciones:** La copia conserva colocaciones, extremos y puntuación por turno.
- **Resultado esperado:** Mismos IDs, orientación, valores conectores y expuestos, valores puntuables, turnos de bloqueo y marcas de apertura; puntuación idéntica en turnos 0 y 1.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `9ed5b7c3-075f-9e36-469c-ceb6b2102567`; duración `00:00:00.0010000`.

### PU-345. Colocar sobre copia sin modificar origen

- **Clase:** `DominoAllFives.Server.Tests.BoardCopyTests`.
- **Caso ejecutable:** `Copy_PlacementOnCopy_PreservesOriginalBoard(Block)`.
- **Origen:** [BoardCopyTests.cs](../DominoAllFives.Server/Tests/BoardCopyTests.cs#L50).
- **Datos:** Argumentos exactos de esta ejecución: `(Block)`.
- **Precondiciones:** Tablero con doble 6-6 y poder indicado; copia independiente y ficha 5-6 con bloqueo.
- **Postcondiciones:** La colocación derecha en turno 1 afecta solo la copia.
- **Resultado esperado:** Captura completa del original intacta y dos fichas en la copia.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `a1063578-95c8-d42d-da1b-8bf247a9e477`; duración `00:00:00.0000001`.

### PU-346. Colocar sobre copia sin modificar origen

- **Clase:** `DominoAllFives.Server.Tests.BoardCopyTests`.
- **Caso ejecutable:** `Copy_PlacementOnCopy_PreservesOriginalBoard(DoublePoints)`.
- **Origen:** [BoardCopyTests.cs](../DominoAllFives.Server/Tests/BoardCopyTests.cs#L50).
- **Datos:** Argumentos exactos de esta ejecución: `(DoublePoints)`.
- **Precondiciones:** Tablero con doble 6-6 y poder indicado; copia independiente y ficha 5-6 con bloqueo.
- **Postcondiciones:** La colocación derecha en turno 1 afecta solo la copia.
- **Resultado esperado:** Captura completa del original intacta y dos fichas en la copia.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `3c467580-b851-cba7-3f9a-52b93dd3a4b6`; duración `00:00:00.0000001`.

### PU-347. Colocar sobre copia sin modificar origen

- **Clase:** `DominoAllFives.Server.Tests.BoardCopyTests`.
- **Caso ejecutable:** `Copy_PlacementOnCopy_PreservesOriginalBoard(None)`.
- **Origen:** [BoardCopyTests.cs](../DominoAllFives.Server/Tests/BoardCopyTests.cs#L50).
- **Datos:** Argumentos exactos de esta ejecución: `(None)`.
- **Precondiciones:** Tablero con doble 6-6 y poder indicado; copia independiente y ficha 5-6 con bloqueo.
- **Postcondiciones:** La colocación derecha en turno 1 afecta solo la copia.
- **Resultado esperado:** Captura completa del original intacta y dos fichas en la copia.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `bb66e6e3-ddfe-e08a-e6de-2fd5e1fe3107`; duración `00:00:00.0010000`.

### PU-348. Aplicar jugada preparada conservando identidad

- **Clase:** `DominoAllFives.Server.Tests.MatchPreparationTests`.
- **Caso ejecutable:** `Play_PreparedPlacement_PreservesPlayerIdentityAndPreviousSnapshots(2)`.
- **Origen:** [MatchPreparationTests.cs](../DominoAllFives.Server/Tests/MatchPreparationTests.cs#L14).
- **Datos:** Argumentos exactos de esta ejecución: `(2)`.
- **Precondiciones:** Partida nueva con la cantidad de jugadores indicada y mazo fijo; referencias al jugador, tablero y vista de mano anteriores.
- **Postcondiciones:** Se coloca la ficha de salida sin alterar los objetos anteriores del tablero y la mano.
- **Resultado esperado:** Misma instancia de jugador; tablero previo vacío y vista previa con tamaño original; mano actual pierde una ficha, tablero tiene una, versión 1, turno 1 y pozo intacto.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `3ceb8b35-5703-bf95-197a-bd553c119cdf`; duración `00:00:00.0010000`.

### PU-349. Aplicar jugada preparada conservando identidad

- **Clase:** `DominoAllFives.Server.Tests.MatchPreparationTests`.
- **Caso ejecutable:** `Play_PreparedPlacement_PreservesPlayerIdentityAndPreviousSnapshots(3)`.
- **Origen:** [MatchPreparationTests.cs](../DominoAllFives.Server/Tests/MatchPreparationTests.cs#L14).
- **Datos:** Argumentos exactos de esta ejecución: `(3)`.
- **Precondiciones:** Partida nueva con la cantidad de jugadores indicada y mazo fijo; referencias al jugador, tablero y vista de mano anteriores.
- **Postcondiciones:** Se coloca la ficha de salida sin alterar los objetos anteriores del tablero y la mano.
- **Resultado esperado:** Misma instancia de jugador; tablero previo vacío y vista previa con tamaño original; mano actual pierde una ficha, tablero tiene una, versión 1, turno 1 y pozo intacto.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `8aa86321-e08f-7208-ec48-f054332b3795`; duración `00:00:00.0000001`.

### PU-350. Aplicar jugada preparada conservando identidad

- **Clase:** `DominoAllFives.Server.Tests.MatchPreparationTests`.
- **Caso ejecutable:** `Play_PreparedPlacement_PreservesPlayerIdentityAndPreviousSnapshots(4)`.
- **Origen:** [MatchPreparationTests.cs](../DominoAllFives.Server/Tests/MatchPreparationTests.cs#L14).
- **Datos:** Argumentos exactos de esta ejecución: `(4)`.
- **Precondiciones:** Partida nueva con la cantidad de jugadores indicada y mazo fijo; referencias al jugador, tablero y vista de mano anteriores.
- **Postcondiciones:** Se coloca la ficha de salida sin alterar los objetos anteriores del tablero y la mano.
- **Resultado esperado:** Misma instancia de jugador; tablero previo vacío y vista previa con tamaño original; mano actual pierde una ficha, tablero tiene una, versión 1, turno 1 y pozo intacto.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `69fb4956-0390-b0bd-08d6-8c67d17f0832`; duración `00:00:00.0000001`.

### PU-351. Aplicar cierre preparado por bloqueo

- **Clase:** `DominoAllFives.Server.Tests.MatchRoundTests`.
- **Caso ejecutable:** `Pass_NextRound_PreservesPlayersAndLastPlay(109)`.
- **Origen:** [MatchRoundTests.cs](../DominoAllFives.Server/Tests/MatchRoundTests.cs#L41).
- **Datos:** Argumentos exactos de esta ejecución: `(109)`.
- **Precondiciones:** Partida de dos jugadores, objetivo 250, escenario indicado con pozo vacío y sin movimientos; primer pase confirmado.
- **Postcondiciones:** El siguiente pase aplica la ronda nueva preservando las instancias y explicación de la última jugada.
- **Resultado esperado:** Mismos jugadores y LastPlay; siete fichas por jugador, ronda 2, turno 0 y versión incrementada una vez.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `cffc30c0-8a5a-1a95-a50b-9b8a824581b4`; duración `00:00:00.0140000`.

### PU-352. Aplicar cierre preparado por bloqueo

- **Clase:** `DominoAllFives.Server.Tests.MatchRoundTests`.
- **Caso ejecutable:** `Pass_NextRound_PreservesPlayersAndLastPlay(3951)`.
- **Origen:** [MatchRoundTests.cs](../DominoAllFives.Server/Tests/MatchRoundTests.cs#L41).
- **Datos:** Argumentos exactos de esta ejecución: `(3951)`.
- **Precondiciones:** Partida de dos jugadores, objetivo 250, escenario indicado con pozo vacío y sin movimientos; primer pase confirmado.
- **Postcondiciones:** El siguiente pase aplica la ronda nueva preservando las instancias y explicación de la última jugada.
- **Resultado esperado:** Mismos jugadores y LastPlay; siete fichas por jugador, ronda 2, turno 0 y versión incrementada una vez.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `8783f72f-5297-5581-0e1f-7034090bc821`; duración `00:00:00.0020000`.

### PU-353. Completar partida por premio de ronda bloqueada

- **Clase:** `DominoAllFives.Server.Tests.MatchRoundTests`.
- **Caso ejecutable:** `Pass_RoundAwardReachesTarget_CompletesWithoutReplacingHands`.
- **Origen:** [MatchRoundTests.cs](../DominoAllFives.Server/Tests/MatchRoundTests.cs#L70).
- **Datos:** Sin parámetros; datos concretos descritos en Precondiciones.
- **Precondiciones:** Escenario 109 con dos jugadores, objetivo 100; primer pase confirmado y ganador 2 con 80 puntos.
- **Postcondiciones:** El siguiente pase concede 20 puntos sin comenzar otro reparto.
- **Resultado esperado:** Jugador 2 con 100, fase Completed, ronda 1, pozo vacío, sin jugador actual, mismo tablero y contenido de mano; versión incrementada una vez.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `2304b65b-854b-a958-86f9-ad8505af7e33`; duración `00:00:00.0040000`.

### PU-354. Aplicar cierre preparado por última ficha

- **Clase:** `DominoAllFives.Server.Tests.MatchRoundTests`.
- **Caso ejecutable:** `Play_NextRound_PreservesPlayersAndPreviousBoard`.
- **Origen:** [MatchRoundTests.cs](../DominoAllFives.Server/Tests/MatchRoundTests.cs#L12).
- **Datos:** Sin parámetros; datos concretos descritos en Precondiciones.
- **Precondiciones:** Partida de dos jugadores, objetivo 250, mazo fijo escenario 2 tras 13 acciones; se guardan jugadores, tablero y versión.
- **Postcondiciones:** La siguiente jugada cierra la ronda y aplica el nuevo reparto.
- **Resultado esperado:** Mismas instancias de ambos jugadores; tablero anterior intacto; siete fichas por jugador, ronda 2, turno 0 y versión incrementada una vez.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `32746fa0-7070-b7bf-ca6d-6b5021889774`; duración `00:00:00.0030000`.

### PU-355. Mantener preparaciones independientes

- **Clase:** `DominoAllFives.Server.Tests.RoundSetupTests`.
- **Caso ejecutable:** `Prepare_FirstResultChanges_PreservesSecondResult`.
- **Origen:** [RoundSetupTests.cs](../DominoAllFives.Server/Tests/RoundSetupTests.cs#L61).
- **Datos:** Sin parámetros; datos concretos descritos en Precondiciones.
- **Precondiciones:** Dos preparaciones concretas con IDs 1 y 2 y el mismo mazo completo.
- **Postcondiciones:** Se vacía una mano, extrae una ficha y abre tablero del primer resultado.
- **Resultado esperado:** Manos, orden y pozo del segundo permanecen idénticos; su tablero sigue vacío.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `a3863fac-10ee-0911-3b73-eda6a69678ba`; duración `00:00:00.0010000`.

### PU-356. Rechazar identificador inválido sin cambiar entradas

- **Clase:** `DominoAllFives.Server.Tests.RoundSetupTests`.
- **Caso ejecutable:** `Prepare_InvalidPlayerId_RejectsWithoutChangingInputs(-1)`.
- **Origen:** [RoundSetupTests.cs](../DominoAllFives.Server/Tests/RoundSetupTests.cs#L14).
- **Datos:** Argumentos exactos de esta ejecución: `(-1)`.
- **Precondiciones:** Lista de identificadores 1, -1 y mazo completo de 28 fichas. El segundo identificador no es positivo.
- **Postcondiciones:** No se obtiene preparación ni se altera la lista o el mazo.
- **Resultado esperado:** ArgumentException con ParamName playerIds; identificadores y mazo conservan contenido exacto.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `1ee15463-ea06-9a95-c7f1-6e1c16eec0ea`; duración `00:00:00.0000001`.

### PU-357. Rechazar identificador inválido sin cambiar entradas

- **Clase:** `DominoAllFives.Server.Tests.RoundSetupTests`.
- **Caso ejecutable:** `Prepare_InvalidPlayerId_RejectsWithoutChangingInputs(0)`.
- **Origen:** [RoundSetupTests.cs](../DominoAllFives.Server/Tests/RoundSetupTests.cs#L14).
- **Datos:** Argumentos exactos de esta ejecución: `(0)`.
- **Precondiciones:** Lista de identificadores 1, 0 y mazo completo de 28 fichas. El segundo identificador no es positivo.
- **Postcondiciones:** No se obtiene preparación ni se altera la lista o el mazo.
- **Resultado esperado:** ArgumentException con ParamName playerIds; identificadores y mazo conservan contenido exacto.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `56679222-e260-dbe2-0896-f03a4f2e4e80`; duración `00:00:00.0010000`.

### PU-358. Rechazar identificador inválido sin cambiar entradas

- **Clase:** `DominoAllFives.Server.Tests.RoundSetupTests`.
- **Caso ejecutable:** `Prepare_InvalidPlayerId_RejectsWithoutChangingInputs(1)`.
- **Origen:** [RoundSetupTests.cs](../DominoAllFives.Server/Tests/RoundSetupTests.cs#L14).
- **Datos:** Argumentos exactos de esta ejecución: `(1)`.
- **Precondiciones:** Lista de identificadores 1, 1 y mazo completo de 28 fichas. El identificador 1 está duplicado.
- **Postcondiciones:** No se obtiene preparación ni se altera la lista o el mazo.
- **Resultado esperado:** ArgumentException con ParamName playerIds; identificadores y mazo conservan contenido exacto.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `9a250898-4832-e086-7bd4-bfe378137422`; duración `00:00:00.0000001`.

### PU-359. Separar preparación de identificadores de entrada

- **Clase:** `DominoAllFives.Server.Tests.RoundSetupTests`.
- **Caso ejecutable:** `Prepare_SourceIdsChange_PreservesPreparedPlayersAndBoard`.
- **Origen:** [RoundSetupTests.cs](../DominoAllFives.Server/Tests/RoundSetupTests.cs#L41).
- **Datos:** Sin parámetros; datos concretos descritos en Precondiciones.
- **Precondiciones:** IDs 1 y 2, mazo con 6-6 primero y 5-5 después; se obtiene PreparedRound.
- **Postcondiciones:** Se vacía la lista de IDs externa; permanece el resultado preparado.
- **Resultado esperado:** Dos jugadores en orden 1 y 2, mano inicial de siete, pozo de 14 y tablero sin fichas ni extremos.
- **Resultado obtenido:** Aprobada (Passed); las aserciones del resultado esperado se cumplieron.
- **Evidencia:** `prepared-transition-unit.trx`, caso `fc5d0d39-afbb-eb73-f167-2c5dfa783966`; duración `00:00:00.0010000`.

## Verificación del catálogo

- 359 fichas únicas y consecutivas, PU-001 a PU-359.
- Correspondencia exacta con los 351 casos Passed de prepared-transition-unit.trx
  y los ocho de prepared-transition-client-unit.trx; cero fallidos y omitidos.
- Se conservaron los 338 casos del catálogo previo, con actualización de nombres,
  origen y expectativas de preparación; se añadieron 21 casos individuales.
- Los métodos unitarios nuevos y los modificados de RoundSetup conservan
  Arrange, Act, Assert y una sola aserción por caso ejecutado.
- VSTest 18.10, NUnit3TestAdapter 4.5 y compilación Debug del servidor aprobada.
- La preparación y copia se utilizan en producción; no se agregaron sustitutos
  ni mecanismos para provocar artificialmente errores del generador.
