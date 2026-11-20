### Manejo de concurrencia

Domino All Fives debe permitir que diferentes clientes interactúen simultáneamente con el servidor sin corromper el estado compartido y sin bloquear innecesariamente la interfaz WPF.

La implementación divide la concurrencia en:
* **Servidor:** donde varias sesiones WCF pueden acceder al mismo estado compartido.
* **Cliente WPF:** donde las operaciones remotas no deben bloquear el hilo de interfaz (UI).

---

### Concurrencia en servicios WCF duplex

Los servicios duplex siguen la configuración utilizada por Lobby:

```csharp
[ServiceBehavior(
    InstanceContextMode = InstanceContextMode.PerSession,
    ConcurrencyMode = ConcurrencyMode.Reentrant,
    IncludeExceptionDetailInFaults = false)]
```

#### InstanceContextMode.PerSession
WCF crea una instancia del servicio para cada sesión duplex.

En LobbyService, esto permite conservar información propia de la conexión:

```csharp
private Player _player;
private ILobbyCallback _callback;
private ICommunicationObject _callbackChannel;
```

Por lo tanto:

```
Jugador A ──► LobbyService instancia A
Jugador B ──► LobbyService instancia B
Jugador C ──► LobbyService instancia C
```

El estado almacenado en estas variables pertenece a la conexión correspondiente.

> [!IMPORTANT]
> `PerSession` no protege el estado compartido entre jugadores. Las distintas instancias de `LobbyService` y `ChatService` continúan utilizando el mismo `LobbyManager.Instance`, por lo que dicho estado necesita sincronización propia.

#### ConcurrencyMode.Reentrant
Los servicios duplex utilizan:

```csharp
ConcurrencyMode.Reentrant
```

Esta configuración permite que una operación del servicio pueda realizar callbacks WCF sin mantener bloqueada permanentemente la instancia del servicio.

Es apropiada para el modelo utilizado por Lobby, donde después de realizar una operación se pueden enviar notificaciones a otros clientes:

```
Cliente
   │
   ▼
LobbyService
   │
   ▼
LobbyController
   │
   ▼
LobbyManager
   │
   ▼
LobbyEventPublisher
   │
   ▼
Callbacks
```

> [!IMPORTANT]
> `ConcurrencyMode.Reentrant` controla cómo WCF administra las llamadas sobre la instancia del servicio. No hace thread-safe a `LobbyManager`, `Dictionary`, listas u otros objetos compartidos. La protección del estado compartido se realiza explitamente mediante mecanismos de sincronización.

---

### Estado compartido del servidor

#### LobbyManager
`LobbyManager` mantiene información utilizada por diferentes jugadores y sesiones:

```csharp
private readonly Dictionary<string, Lobby> _lobbies;
private readonly Dictionary<int, string> _playerLobbyCodes;
private int _nextLobbyId;
```

Estos objetos representan estado compartido. Por ejemplo, dos clientes podrían intentar entrar simultáneamente al mismo lobby. Para proteger estas estructuras relacionadas se utiliza:

```csharp
private readonly object _syncRoot = new object();
```

Las operaciones que leen o modifican el estado compartido utilizan:

```csharp
lock (_syncRoot)
{
    // operación sobre el estado compartido
}
```

Ejemplos:

```
CreateLobby
JoinPlayer
LeavePlayer
GetLobby
PrepareLobbyForMatch
ConfirmMatchStarted
RestoreLobbyAfterMatchStartFailure
ReleaseMatch
CloseLobby
GetChatRecipientIds
```

#### ¿Qué garantiza el lock?
Mientras un hilo ejecuta:

```csharp
lock (_syncRoot)
{
    // sección crítica
}
```

otro hilo que necesite el mismo `_syncRoot` debe esperar.

Una sola operación puede modificar varias estructuras relacionadas. Por ejemplo, crear un lobby implica:

```
generar LobbyId
    │
    ▼
crear Lobby
    │
    ├── agregar a _lobbies
    ├── agregar a _playerLobbyCodes
    └── incrementar _nextLobbyId
```

Todas estas modificaciones deben representar una sola transición consistente. Aunque cada colección fuera individualmente thread-safe, podría ocurrir que otro hilo observara un estado intermedio. Por eso se utiliza `lock (_syncRoot)` para proteger la operación completa, no solamente una llamada a una colección.

#### Regla para utilizar lock
Se debe utilizar `lock` cuando:

- existe estado mutable compartido entre múltiples sesiones o hilos;
- una operación necesita leer y modificar varias estructuras relacionadas;
- varias instrucciones deben ejecutarse como una sola transición consistente;
- una validación y la modificación que depende de ella no deben separarse.

Ejemplo:

```csharp
lock (_syncRoot)
{
    ValidatePlayerCanEnterLobby(player.PlayerId);
    Lobby lobby = GetRegisteredLobby(lobbyCode);
    ValidateLobbyIsWaiting(lobby);

    if (lobby.Participants.Count >= lobby.Settings.MaxPlayers)
    {
        throw new LobbyRuleException(LobbyIssue.Full);
    }

    lobby.Participants.Add(CopyPlayer(player));
    _playerLobbyCodes.Add(player.PlayerId, lobby.LobbyCode);
}
```

La validación `lobby.Participants.Count` y la modificación `lobby.Participants.Add(...)` permanecen protegidas por el mismo `lock`.

#### Métodos internos y lock
Los métodos auxiliares de `LobbyManager`, como:

```
ValidatePlayerCanEnterLobby
GetRegisteredLobby
GetPlayerLobby
RemoveLobby
GenerateLobbyCode
```

no adquieren su propio `lock`. Se ejecutan dentro de una operación pública que ya posee `_syncRoot`.

La estructura buscada es:

```
Método público
   │
   ▼
lock (_syncRoot)
   │
   ├── validaciones internas
   ├── búsquedas internas
   ├── modificaciones
   └── creación del snapshot o proyección
```

Esto evita distribuir múltiples locks sobre una misma operación.

#### No exponer el estado mutable
Otro elemento importante de la estrategia es que `LobbyManager` no devuelve directamente las colecciones ni entidades mutables internas que mantiene. Según el contexto de la operación, aplica dos mecanismos:

1. **Copias defensivas (snapshots):** Cuando el consumidor necesita la entidad completa (como en operaciones de sala), construye una copia mediante `CreateSnapshot(lobby)`, copiando también:
   - `Player`
   - `LobbySettings`

   Conceptualmente:
   ```
   Estado interno
       │
       │ copia
       ▼
   Snapshot
       │
       ▼
   Controller / Service / Client
   ```

2. **Proyecciones de identificadores:** Cuando el consumidor únicamente necesita identificar a los participantes sin requerir sus datos completos ni imágenes de perfil (como en `GetChatRecipientIds`), retorna una lista nueva de tipos por valor (`List<int>`), evitando clonar objetos innecesariamente y manteniendo aislada la lista interna `lobby.Participants`.

> [!IMPORTANT]
> El `lock` protege el estado dentro de `LobbyManager`. Por ello no se deben devolver referencias mutables directas a las colecciones u objetos internos protegidos.

#### Duración del lock
La sección protegida debe limitarse al trabajo necesario para mantener consistente el estado compartido. La comunicación WCF con los clientes no debe realizarse dentro del lock de `LobbyManager`.

En Lobby el flujo es:

```
LobbyManager
    │
    │ lock
    ▼
modifica estado
    │
    ▼
crea snapshot
    │
    ▼
libera lock
    │
    ▼
LobbyService
    │
    ▼
LobbyEventPublisher
    │
    ▼
callbacks WCF
```

Esto es importante porque un callback puede:

- tardar;
- producir un timeout;
- encontrar un cliente desconectado;
- producir una `CommunicationException`.

El estado principal no debe permanecer bloqueado mientras se espera una operación de red.

---

### Concurrencia en CallbackRegistry

Los callbacks representan otro estado compartido. Varias sesiones WCF pueden:

- registrar un callback;
- consultar un callback;
- eliminar un callback;

simultáneamente. Para este caso se utiliza:

```csharp
ConcurrentDictionary<int, ILobbyCallback>
```

dentro de `LobbyCallbackRegistry` (y su equivalente `ConcurrentDictionary<int, IChatCallback>` en `ChatCallbackRegistry`).

#### Por qué aquí se usa ConcurrentDictionary
Las operaciones del registry son simples e independientes:

```
PlayerId → Callback
```

Las operaciones principales son:

```csharp
TryAdd(...)
TryGetValue(...)
Remove(...)
```

No es necesario coordinar varias colecciones ni mantener una transición compleja entre diferentes estructuras. Por ello `ConcurrentDictionary` es suficiente para este caso.

La diferencia con `LobbyManager` es:

```
LobbyManager
    │
    └── varias estructuras + invariantes
            │
            ▼
      lock (_syncRoot)
```

```
LobbyCallbackRegistry
    │
    └── asociación independiente
        PlayerId → Callback
            │
            ▼
     ConcurrentDictionary
```

#### Regla para elegir entre lock y colecciones concurrentes

Utilizar `lock` cuando:
- varias instrucciones forman una operación atómica;
- existen varias colecciones relacionadas;
- una validación debe mantenerse válida hasta completar la modificación;
- se deben proteger invariantes del dominio.

Utilizar una colección concurrente cuando:
- la estructura es compartida entre hilos;
- las operaciones son principalmente independientes;
- las operaciones atómicas proporcionadas por la colección son suficientes.

No debe sustituirse automáticamente un `Dictionary` por `ConcurrentDictionary` suponiendo que eso hace thread-safe toda la lógica que lo utiliza.

---

### Concurrencia en el cliente WPF

WPF posee un hilo de interfaz encargado de:

- procesar entrada del usuario;
- actualizar controles;
- ejecutar bindings;
- renderizar la interfaz.

Si una llamada remota se ejecuta directamente sobre este hilo:

```
UI Thread
   │
   ▼
llamada WCF
   │
   │ espera servidor
   │
   ▼
respuesta
```

la interfaz permanece bloqueada mientras finaliza la operación. Para evitarlo, el proyecto utiliza:

```
Services/Threading/
├── IBackgroundJob
├── BackgroundJob
└── BackgroundWork
```

#### BackgroundWork
`BackgroundWork` envía el trabajo al `ThreadPool`:

```csharp
ThreadPool.QueueUserWorkItem(
    ExecuteJob,
    job);
```

Por lo tanto, la operación no se ejecuta sobre el hilo principal de WPF. El flujo es:

```
UI Thread
   │
   │ BackgroundWork.Start(job)
   ▼
ThreadPool
   │
   ▼
job.Execute()
   │
   │ operación WCF
   ▼
resultado / error
```

Cuando termina, la continuación debe regresar al hilo de UI. Para esto se utiliza:

```csharp
Application.Current.Dispatcher.BeginInvoke(
    new Action(job.Complete));
```

El flujo completo es:

```
UI Thread
   │
   │ Start()
   ▼
ThreadPool worker
   │
   │ Execute()
   ▼
operación remota
   │
   ▼
resultado
   │
   │ Dispatcher.BeginInvoke()
   ▼
UI Thread
   │
   ▼
Complete()
```

#### Ejemplo conceptual

```
ViewModel
   │
   │ crea BackgroundJob
   ▼
BackgroundWork.Start()
   │
   ▼
Worker Thread
   │
   │ ILobbyClient.JoinLobby(...)
   ▼
WCF
   │
   ▼
resultado
   │
   ▼
Dispatcher
   │
   ▼
OnLobbyReady(...)
   │
   ▼
ViewModel / UI
```

Este mecanismo se utiliza actualmente en funcionalidades como creación, unión, salida e inicio de Lobby.

#### Cancelación
`BackgroundJob` contiene `Cancel()` y protege su estado mediante:

```csharp
lock (_stateLock)
```

Este `lock` protege exclusivamente `_isCancelled`. La cancelación no detiene forzosamente una operación WCF que ya comenzó; su función es indicar que el resultado normal ya no debe procesarse como una operación activa de la interfaz. Por ello existen dos posibles caminos:

```
operación termina
     │
     ├── no cancelada
     │      ▼
     │   OnCompleted
     │
     └── cancelada
            ▼
     OnCompletedAfterCancel
```

#### Uso de Task, Task.Run, async y await
La implementación actual no utiliza `Task.Run` ni un flujo basado en `async`/`await` para ejecutar las operaciones remotas de la aplicación. Esto es una decisión de implementación, no una limitación del lenguaje o de .NET Framework 4.8.

> [!IMPORTANT]
> `async`/`await` no son incorrectos. Simplemente no forman parte del modelo de concurrencia adoptado actualmente por el proyecto.

##### Por qué no Task.Run sobre las llamadas WCF
Los proxies utilizados por el proyecto exponen las operaciones que estamos consumiendo de forma síncrona, por ejemplo `_client.JoinLobby(lobbyCode);`. Colocar esta operación dentro de `Task.Run(...)` no convierte la operación de red verdaderamente asíncrona; únicamente ejecutaría la llamada síncrona en otro hilo del `ThreadPool`, que es precisamente lo que el proyecto ya centraliza mediante `BackgroundWork` y `ThreadPool.QueueUserWorkItem`.

##### Por qué no async/await
Para adoptar `async`/`await` como modelo general sería necesario definir de forma consistente contratos o proxies asíncronos, interfaces del cliente con métodos `Task<T>`, ViewModels asíncronos, propagación de excepciones mediante `Task`, cancelación, ciclo de vida de los canales WCF, tratamiento de callbacks duplex y pruebas correspondientes. La implementación actual ya posee una solución común para ejecutar operaciones sin bloquear WPF (`BackgroundJob` + `BackgroundWork` + `ThreadPool` + `Dispatcher`).

#### Regla para operaciones desde ViewModel
Una operación que pueda bloquear por comunicación, persistencia o trabajo relevante no debe ejecutarse directamente sobre el hilo de UI. El patrón es:

```csharp
BackgroundJob<TState, TResult> job =
    new BackgroundJob<TState, TResult>(
        WorkOnWorker,
        state,
        OnCompleted,
        OnFailed);

BackgroundWork.Start(job);
```

En el cliente:

```
¿La operación puede bloquear el UI?
          │
          ▼
         Sí
          │
          ▼
BackgroundJob
          │
          ▼
BackgroundWork
          │
          ▼
ThreadPool
          │
          ▼
Dispatcher
          │
          ▼
actualización del ViewModel
```

---

### Resumen
La estrategia de concurrencia adoptada se divide en cuatro mecanismos:

```
WCF duplex
    │
    ├── InstanceContextMode.PerSession
    └── ConcurrencyMode.Reentrant
```

```
Estado de dominio compartido
    │
    └── lock (_syncRoot)
        Ejemplo: LobbyManager
```

```
Registro concurrente simple
    │
    └── ConcurrentDictionary
        Ejemplo: LobbyCallbackRegistry
```

```
Cliente WPF
    │
    └── BackgroundWork
        ├── ThreadPool
        ├── BackgroundJob
        └── Dispatcher
```

La regla principal es que cada mecanismo tiene una responsabilidad diferente:

- WCF controla la concurrencia de las instancias del servicio.
- `lock` protege invariantes y estado mutable compartido.
- `ConcurrentDictionary` protege asociaciones concurrentes simples.
- `BackgroundWork` evita bloquear el hilo de interfaz.

Ninguno de estos mecanismos sustituye automáticamente a los demás.