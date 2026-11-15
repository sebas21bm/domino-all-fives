# Manejo de excepciones

Domino All Fives distingue los errores según su naturaleza y la capa donde pueden ser correctamente interpretados.

La dos reglas generales son:

> [!WARNING]
> 1 - No se puede usar la clase Exception genérica para capturar las excepciones, jamás en ningún caso porque va en contra del estándar y del buen manejo de las excepciones.

> [!IMPORTANT]
> 2 - Una excepción debe **propagarse** cuando la capa actual no tiene suficiente contexto para resolverla, o debe **manejarse** cuando la capa puede tomar una decisión concreta. No debe capturarse únicamente para volver a lanzar la misma excepción sin aportar comportamiento.

Cuando una excepción se propaga directamente se utiliza:

```csharp
throw;
```

y no:

```csharp
throw ex;
```

porque `throw;` conserva el stack trace original.


# Clasificación de excepciones

La implementación distingue cuatro categorías principales:

```text
Excepciones
│
├── Argumentos inválidos
├── Estado inválido
├── Reglas de negocio
└── Infraestructura / comunicación
```

Cada categoría se implementa de manera distinta.

## Argumentos inválidos

Cuando un método recibe un argumento que viola directamente su contrato se utilizan excepciones estándar de .NET.

Ejemplos:

```csharp
throw new ArgumentNullException(nameof(player));
```

```csharp
throw new ArgumentException(null, nameof(lobbyCode));
```

```csharp
throw new ArgumentOutOfRangeException(nameof(playerId));
```

Se utilizan principalmente:

```text
ArgumentNullException
ArgumentException
ArgumentOutOfRangeException
```

No se deben crear excepciones personalizadas para representar errores que ya tienen una excepción estándar adecuada. 

Además, debe elegirse la excepción estándar que mejor describa la causa del error. No se debe utilizar ArgumentException de forma genérica cuando una excepción más específica permita identificar con mayor claridad qué ocurrió.

**Por ejemplo:**

* Cuando el argumento no puede ser null
```csharp
throw new ArgumentNullException(nameof(player));

```

* Cuando el argumento tiene un valor fuera de rango
```csharp
throw new ArgumentOutOfRangeException(nameof(playerId));
```

* Cuando el argumento existe, pero su contenido no cumple las condiciones requeridas
```csharp
throw new ArgumentException(
    "The lobby code cannot be empty.",
    nameof(lobbyCode));
```
>[!IMPORTANT]
>Se debe preferir la excepción estándar más específica aplicable. El objetivo no es únicamente lanzar una excepción, sino comunicar con precisión qué condición del contrato del método fue violada.


## Estado inválido

Cuando una operación no puede ejecutarse debido al estado actual del objeto se utilizan excepciones estándar.

Ejemplo en `LobbyClient`:

```csharp
if (_client != null)
{
    throw new InvalidOperationException(
        "A lobby connection is already open.");
}
```

También:

```csharp
throw new ObjectDisposedException(
    nameof(LobbyClient));
```

Las excepciones principales de esta categoría son:

```text
InvalidOperationException
ObjectDisposedException
```

Estas excepciones representan problemas internos de uso o ciclo de vida y normalmente no constituyen reglas de negocio.


## Reglas de negocio

Una regla de negocio representa un rechazo esperado de una operación válida desde el punto de vista técnico.

Ejemplos:

```text
credenciales inválidas
cuenta suspendida
lobby lleno
jugador ya pertenece a un lobby
lobby inexistente
jugador expulsado
```

En el servidor se utilizan excepciones internas asociadas a un código de problema, representado por un Enum (FeatureIssue).

Actualmente existen, entre otras:

```text
AccountRuleException
LobbyRuleException
ChatRuleException
```

Por ejemplo:

```csharp
public sealed class LobbyRuleException : Exception
{
    public LobbyIssue Issue { get; }

    public LobbyRuleException(LobbyIssue issue)
    {
        if (!Enum.IsDefined(typeof(LobbyIssue), issue))
        {
            throw new ArgumentOutOfRangeException(
                nameof(issue));
        }

        Issue = issue;
    }
}
```

Estas excepciones tienen una finalidad específica:

```text
Business Logic
     │
     │ detecta regla incumplida
     ▼
LobbyRuleException
     │
     │ contiene LobbyIssue
     ▼
Service boundary
```

> [!IMPORTANT]
> Las excepciones personalizadas no deben utilizarse para errores técnicos, argumentos inválidos o estados que ya tengan una excepción estándar de .NET.

Su justificación se limita a representar internamente un rechazo esperado de negocio acompañado de un `Issue` estructurado.


---
### Las excepciones de negocio no cruzan directamente WCF

Una excepción como:

```text
LobbyRuleException
```

pertenece al servidor.

El cliente no debe depender de ella.

Por ello, la frontera WCF la transforma en:

```csharp
FaultException<LobbyIssue>
```

Ejemplo:

```csharp
try
{
    Lobby lobby =
        controller.JoinLobby(player, lobbyCode);

    publisher.PublishLobbyChanged(lobby);

    return lobby;
}
catch (LobbyRuleException ex)
{
    throw LobbyFault(ex.Issue);
}
```

donde:

```csharp
private static FaultException<LobbyIssue>
    LobbyFault(LobbyIssue issue)
{
    return new FaultException<LobbyIssue>(issue);
}
```

El flujo es:

```text
LobbyManager
     │
     │ regla incumplida
     ▼
LobbyRuleException
     │
     ▼
LobbyService
     │
     │ transforma
     ▼
FaultException<LobbyIssue>
     │
     │ WCF
     ▼
Cliente
```

De esta forma el cliente conoce **qué ocurrió**, pero no depende de las excepciones internas del servidor.

---

### `FaultContract`

Los contratos WCF declaran explícitamente los faults que forman parte de la comunicación.

Por ejemplo:

```csharp
[OperationContract]
[FaultContract(typeof(LobbyIssue))]
[FaultContract(typeof(AccessIssue))]
Lobby JoinLobby(string lobbyCode);
```

Esto establece que determinados rechazos forman parte del contrato remoto.

Conceptualmente:

```text
Resultado normal
        │
        └── Lobby
```

o:

```text
Rechazo esperado
        │
        └── FaultException<LobbyIssue>
```

o:

```text
Problema de acceso
        │
        └── FaultException<AccessIssue>
```

---

## Errores de acceso

Los problemas relacionados con sesión o permisos utilizan:

```csharp
FaultException<AccessIssue>
```

Por ejemplo:

```csharp
if (_player == null)
{
    throw AccessFault(
        AccessIssue.SessionRequired);
}
```

Esto permite distinguir:

```text
SessionRequired
PermissionDenied
```

sin exponer detalles internos del servidor.

## Errores de infraestructura

Los errores técnicos no representan una regla de negocio.

Ejemplos:

```text
SqlException
EntityException
DbUpdateException
```

Estos errores pueden contener información interna sobre:

- SQL Server;
- Entity Framework;
- consultas;
- estructura interna;
- stack trace;
- infraestructura del servidor.

Por ello no deben enviarse directamente al cliente.

---

### Manejo en la frontera del servidor

Cuando el servicio puede identificar un error de infraestructura, lo registra y lo transforma en un fault seguro.

Ejemplo:

```csharp
catch (SqlException ex)
{
    logger.LogError(
        ex,
        "Lobby session could not resolve player.");

    throw new FaultException(
        "The service could not complete the operation.");
}
```

El flujo es:

```text
SqlException
     │
     ▼
Service
     │
     ├── LogError(ex)
     │
     └── transforma
             │
             ▼
      FaultException
             │
             ▼
          Cliente
```

El detalle técnico permanece disponible en el servidor mediante logging.

El cliente solo conoce que el servicio no pudo completar la operación.

> [!NOTE]
> Los `FaultException` genéricos pueden incluir un mensaje técnico seguro y no sensible que facilite el diagnóstico y el logging del cliente. 



# `IncludeExceptionDetailInFaults`

Los servicios utilizan:

```csharp
IncludeExceptionDetailInFaults = false
```

Esto evita que WCF envíe automáticamente al cliente los detalles internos de excepciones no controladas.

No significa que se pierda la información técnica.

La separación es:

```text
Servidor
│
├── excepción completa
├── stack trace
├── InnerException
└── logger
```

```text
Cliente
│
└── Fault definido por el contrato
```

> [!IMPORTANT]
> Los detalles técnicos pertenecen al servidor. La información que debe conocer el cliente debe exponerse explícitamente mediante el contrato WCF.


# Propagar o manejar

La decisión principal ante una excepción es determinar si la capa actual puede hacer algo útil con ella.

## Propagar

Si una capa no puede resolver el problema:

```csharp
catch (SomeException)
{
    throw;
}
```

o, preferentemente, no se agrega un `catch` si no es necesario realizar ninguna acción.

```text
Capa actual
    │
    │ no puede resolver
    ▼
propaga
    │
    ▼
capa superior
```

No se debe utilizar:

```csharp
throw ex;
```

porque modifica el stack trace y dificulta encontrar el origen real del error.

## Manejar

Una excepción se considera manejada cuando la capa toma una decisión concreta.

Por ejemplo:

```csharp
catch (SqlException ex)
{
    logger.LogError(ex, "...");

    throw new FaultException(
        "The service could not complete the operation.");
}
```

Aquí la `SqlException` original ya no se propaga.

Fue:

```text
capturada
    │
    ├── registrada
    └── transformada
```

La excepción que continúa es un nuevo error perteneciente al contrato WCF.


# Evitar capturas sin propósito

No se debe hacer:

```csharp
try
{
    operation();
}
catch (Exception ex)
{
    throw ex;
}
```

Tampoco:

```csharp
try
{
    operation();
}
catch (Exception)
{
    throw;
}
```

si no existe ninguna acción adicional.

En ese caso es preferible:

```csharp
operation();
```

y permitir que la excepción continúe naturalmente.

---

# Logging de excepciones

Una excepción debe registrarse en la capa donde realmente es manejada o donde existe suficiente contexto para realizar un diagnóstico útil.

No se recomienda registrar la misma excepción repetidamente en todas las capas.

Ejemplo a evitar:

```text
Repository
   │
   └── LogError
        │
        ▼
Controller
   │
   └── LogError
        │
        ▼
Service
   │
   └── LogError
```

Esto puede producir tres registros del mismo problema.

El objetivo es:

```text
Excepción
    │
    ▼
sube hasta la capa que puede manejarla
    │
    ▼
Log
    │
    ▼
decisión
```

Los detalles específicos del sistema de logging se documentan por separado.

## Separación entre diagnóstico, fault y mensaje de interfaz

Cuando una excepción es manejada en el servidor, no toda la información asociada al error debe llegar al cliente ni mostrarse directamente al usuario.

La información se separa en tres niveles:

```text
Detalle real del error
    │
    └── log del servidor

Mensaje remoto seguro
    │
    └── FaultException / log del cliente

Mensaje para el usuario
    │
    └── recursos .resx
```

### Detalle real del error

El servidor conserva la información técnica necesaria para diagnóstico mediante el logger.

Por ejemplo:

```csharp
catch (SqlException ex)
{
    logger.LogError(
        ex,
        "Lobby session could not resolve player.");

    throw new FaultException(
        "The service could not complete the operation.");
}
```

En este caso, el log del servidor conserva la excepción original y su contexto técnico:

```text
SqlException
stack trace
InnerException
contexto de la operación
```

Esta información no debe enviarse directamente al cliente.

---

### Mensaje remoto seguro

El `FaultException` puede contener un mensaje técnico breve, seguro y no sensible:

```csharp
throw new FaultException(
    "The service could not complete the operation.");
```

Este mensaje permite identificar en el cliente que el servidor respondió con un error, sin exponer detalles internos.

El cliente puede registrarlo para diagnóstico:

```csharp
catch (FaultException ex)
{
    _logger.LogError(
        ex,
        "Lobby operation failed because the server returned a fault.");

    ShowGenericError();
}
```

El mensaje contenido en `FaultException` forma parte del contexto técnico de la comunicación y puede aparecer en el log del cliente.

> [!WARNING]
> El mensaje de un `FaultException` no debe utilizarse directamente como texto de interfaz.

---
### Mensaje para el usuario

La interfaz debe utilizar los recursos de internacionalización definidos en los archivos `.resx`.

Además, la selección de los mensajes mostrados al usuario debe centralizarse dentro del ViewModel correspondiente, evitando distribuir llamadas a `ShowDialog(...)` y decisiones de presentación por diferentes bloques `catch`.

Por ejemplo, `LoginViewModel` primero clasifica el error:

```csharp
catch (FaultException<AccountIssue> ex)
{
    _logger.LogWarning(
        ex,
        "Login rejected. Account issue: {AccountIssue}.",
        ex.Detail);

    ShowMessageFailure(ex.Detail);
    return;
}
catch (FaultException ex)
{
    _logger.LogError(
        ex,
        "Authentication service could not complete login.");

    ShowMessageFailure();
    return;
}
catch (TimeoutException ex)
{
    _logger.LogError(
        ex,
        "Authentication service timed out.");

    ShowMessageFailure(connectionFailure: true);
    return;
}
catch (CommunicationException ex)
{
    _logger.LogError(
        ex,
        "Authentication service unavailable.");

    ShowMessageFailure(connectionFailure: true);
    return;
}
```

Después, la decisión sobre qué mensaje presentar se concentra en un único método:

```csharp
private void ShowMessageFailure(
    AccountIssue? issue = null,
    bool connectionFailure = false)
{
    switch (issue)
    {
        case AccountIssue.Banned:
            _dialogService.ShowDialog(
                new DialogRequest
                {
                    Type = DialogType.Warning,
                    TitleKey =
                        "MessageAuthentication_msgBannedAccountTitle",
                    MessageKey =
                        "MessageAuthentication_msgBannedAccount"
                });
            break;

        case AccountIssue.Suspended:
            _dialogService.ShowDialog(
                new DialogRequest
                {
                    Type = DialogType.Warning,
                    TitleKey =
                        "MessageAuthentication_msgSuspendedAccountTitle",
                    MessageKey =
                        "MessageAuthentication_msgSuspendedAccount"
                });
            break;

        case AccountIssue.InvalidCredentials:
            _dialogService.ShowDialog(
                new DialogRequest
                {
                    Type = DialogType.Warning,
                    TitleKey =
                        "MessageAuthentication_msgInvalidCredentialsTitle",
                    MessageKey =
                        "MessageAuthentication_msgInvalidCredentials"
                });
            break;

        case null when connectionFailure:
            _dialogService.ShowDialog(
                new DialogRequest
                {
                    Type = DialogType.Error,
                    TitleKey = "Global_msgConnectionErrorTitle",
                    MessageKey =
                        "MessageAuthentication_msgConnectionError"
                });
            break;

        default:
            _dialogService.ShowDialog(
                new DialogRequest
                {
                    Type = DialogType.Error,
                    TitleKey = "Global_msgDefaultErrorTitle",
                    MessageKey = "Global_msgDefaultError"
                });
            break;
    }
}
```

La separación buscada es:

```text
Excepción recibida
        │
        ▼
catch correspondiente
        │
        ├── logging
        │
        └── clasifica el problema
                │
                ▼
        método centralizado
                │
                ▼
        selecciona claves .resx
                │
                ▼
           DialogService
                │
                ▼
              usuario
```

De esta forma:

```text
FaultException.Message
        │
        └── diagnóstico / logging
```

mientras que:

```text
Issue / tipo de error
        │
        └── determina qué mensaje corresponde
```

y finalmente:

```text
.resx
  │
  └── texto mostrado al usuario
```

Centralizar la selección de mensajes permite:

- evitar repetir lógica de presentación en distintos bloques `catch`;
- mantener en un solo lugar la relación entre cada tipo de error y su mensaje;
- mantener el ViewModel legible;
- facilitar cambios en los mensajes o en la clasificación de errores;
- conservar el soporte de internacionalización mediante `.resx`;
- evitar utilizar directamente `Exception.Message` como texto de interfaz.

> [!IMPORTANT]
> Los bloques `catch` deben encargarse de identificar y registrar el error. La selección del mensaje que verá el usuario debe centralizarse, cuando existan varios posibles resultados, en un método del ViewModel que traduzca el tipo de error o `Issue` a las claves `.resx` correspondientes.

La regla general es:

> El servidor registra el detalle técnico real, WCF transporta únicamente información segura, el cliente clasifica el error y la presentación al usuario se centraliza mediante claves definidas en los recursos `.resx`.



# Excepciones de comunicación en el cliente

Una vez fuera del servidor existen errores propios de la comunicación WCF.

Los más relevantes son:

```text
CommunicationException
TimeoutException
```

Estos no representan reglas de negocio.

Por ejemplo:

```text
FaultException<LobbyIssue>
        │
        └── servidor respondió
            y rechazó la operación
```

mientras que:

```text
CommunicationException
        │
        └── ocurrió un problema
            en la comunicación
```

y:

```text
TimeoutException
        │
        └── no se obtuvo respuesta
            dentro del tiempo permitido
```

El cliente debe distinguir estas situaciones.

---

# Manejo en el ViewModel

El ViewModel es la capa que posee contexto suficiente para decidir cómo presentar el error al usuario.

Por ejemplo, `JoinGameViewModel` distingue:

```csharp
FaultException<LobbyIssue>
```

de errores de comunicación.

Conceptualmente:

```text
Exception
   │
   ├── FaultException<LobbyIssue>
   │       │
   │       └── mensaje asociado a la
   │           regla de negocio
   │
   ├── CommunicationException
   │       │
   │       └── error de conexión
   │
   ├── TimeoutException
   │       │
   │       └── error de conexión
   │
   └── otro error
           │
           └── mensaje genérico
```

Los textos mostrados al usuario deben provenir de recursos de internacionalización y no del mensaje técnico de la excepción.

---

# Ejemplo completo: Lobby

Un ejemplo de rechazo esperado es intentar entrar a un lobby que no está disponible.

```text
CLIENTE
JoinGameViewModel
      │
      ▼
ILobbyClient.JoinLobby()
      │
      ▼
LobbyClient
      │
      │ WCF
      ▼
SERVIDOR
LobbyService.JoinLobby()
      │
      ▼
LobbyController
      │
      ▼
LobbyManager
      │
      │ detecta regla
      ▼
LobbyRuleException
      │
      ▼
LobbyService
      │
      │ transforma
      ▼
FaultException<LobbyIssue>
      │
      │ WCF
      ▼
CLIENTE
JoinGameViewModel
      │
      ▼
mensaje localizado
```

La excepción personalizada nunca cruza el límite del servidor.

---

# Ejemplo completo: error técnico

```text
CLIENTE
      │
      ▼
WCF
      │
      ▼
SERVIDOR
      │
      ▼
Entity Framework / SQL Server
      │
      ▼
SqlException
      │
      ▼
Service
      │
      ├── LogError(ex)
      │
      └── FaultException genérico
              │
              ▼
             WCF
              │
              ▼
           Cliente
              │
              ▼
       mensaje genérico
```

El usuario no recibe detalles de SQL Server.

El servidor conserva la información necesaria para diagnóstico.


# Limpieza de canales WCF

El manejo de errores también debe considerar el estado del canal.

Cuando `Close()` produce una excepción y la conexión se recupera mediante `Abort()`, la excepción se considera manejada.  Un canal `Faulted` no debe reutilizarse.

En estos casos, si el fallo aporta información útil para diagnóstico, debe registrarse antes de completar la recuperación:

```csharp
catch (CommunicationException ex)
{
    _logger.LogWarning(
        ex,
        "Lobby connection could not close normally. The channel will be aborted.");

    _client.Abort();
}
catch (TimeoutException ex)
{
    _logger.LogWarning(
        ex,
        "Lobby connection close timed out. The channel will be aborted.");

    _client.Abort();
}
```

El objetivo no es propagar una excepción que ya fue resuelta mediante `Abort()`, sino conservar trazabilidad del cierre anómalo.


La elección concreta del nivel de logging y las reglas para evitar registros innecesarios se definen en el documento de **Manejo de logging**.


# Excepciones personalizadas

La regla para crear nuevas excepciones personalizadas es restrictiva.

Antes de crear una excepción se debe comprobar si existe una excepción estándar adecuada.

```text
Argumento nulo
    └── ArgumentNullException

Argumento inválido
    └── ArgumentException /
        ArgumentOutOfRangeException

Estado inválido
    └── InvalidOperationException

Objeto destruido
    └── ObjectDisposedException

Acceso no autorizado interno
    └── UnauthorizedAccessException
```

Solo se debe considerar una excepción personalizada cuando exista una semántica de negocio que las excepciones estándar no representen adecuadamente.

En el modelo actual:

```text
AccountRuleException
LobbyRuleException
ChatRuleException
```

sirven como señales internas del servidor para transportar un `Issue` hasta la frontera WCF.

No deben utilizarse como sustitutos genéricos de excepciones estándar.


# Regla para implementar un nuevo caso de uso

Ante una condición de error:

```text
¿Qué ocurrió?
      │
      ├── argumento inválido
      │       └── excepción .NET estándar
      │
      ├── estado inválido
      │       └── excepción .NET estándar
      │
      ├── regla de negocio
      │       └── RuleException + Issue
      │               │
      │               ▼
      │         service boundary
      │               │
      │               ▼
      │       FaultException<Issue>
      │
      └── infraestructura
              │
              ├── si esta capa no puede manejarla
              │       └── propagar
              │
              └── si esta capa es la frontera
                      ├── LogError
                      └── FaultException seguro
```

En el cliente:

```text
FaultException<Issue>
      │
      └── rechazo esperado
```

```text
CommunicationException
TimeoutException
      │
      └── problema de conexión
```

```text
FaultException genérico
      │
      └── error técnico del servidor
```


# Resumen

La política de excepciones del proyecto se basa en las siguientes reglas:

```text
1. Preferir excepciones estándar de .NET.
```

```text
2. Crear excepciones personalizadas únicamente cuando exista una justificación de negocio.
```

```text
3. Si una capa no puede manejar una excepción, permitir que se propague.
```

```text
4. Al volver a lanzar la misma excepción utilizar throw; nunca throw ex;
```

```text
5. Manejar una excepción únicamente cuando la capa pueda tomar una decisión concreta.
```

```text
6. Registrar el detalle técnico en la capa donde el error es manejado.
```

```text
7. No exponer excepciones internas del servidor directamente mediante WCF.
```

```text
8. Traducir reglas de negocio a FaultException<TIssue>.
```

```text
9. Traducir errores técnicos del servidor a faults seguros.
```

```text
10. Mantener IncludeExceptionDetailInFaults = false.
```

```text
11. El cliente debe distinguir entre rechazo de negocio, fallo remoto y problema de comunicación.
```

El objetivo es mantener suficiente información para diagnosticar los errores sin romper la separación entre capas ni exponer detalles internos del servidor al cliente.