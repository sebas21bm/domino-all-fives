# Manejo de concurrencia

Domino All Fives debe permitir que diferentes clientes interactúen simultáneamente con el servidor sin corromper el estado compartido y sin bloquear innecesariamente la interfaz WPF.

La implementación divide la concurrencia en:

- **Servidor**: donde varias sesiones WCF pueden acceder al mismo estado compartido.
- **Cliente WPF**: donde las operaciones remotas no deben bloquear el hilo de interfaz (UI).


# Concurrencia en servicios WCF duplex

Los servicios duplex siguen la configuración utilizada por Lobby:

```csharp
[ServiceBehavior(
    InstanceContextMode = InstanceContextMode.PerSession,
    ConcurrencyMode = ConcurrencyMode.Reentrant,
    IncludeExceptionDetailInFaults = false)]
```

### `InstanceContextMode.PerSession`

WCF crea una instancia del servicio para cada sesión duplex.

En `LobbyService`, esto permite conservar información propia de la conexión:

```csharp
private Player _player;
private ILobbyCallback _callback;
private ICommunicationObject _callbackChannel;
```

Por lo tanto:

```text
Jugador A ──► LobbyService instancia A
Jugador B ──► LobbyService instancia B
Jugador C ──► LobbyService instancia C
```

El estado almacenado en estas variables pertenece a la conexión correspondiente.

> [!IMPORTANT]
> `PerSession` no protege el estado compartido entre jugadores. Las distintas instancias de `LobbyService` continúan utilizando el mismo `LobbyManager`, por lo que dicho estado necesita sincronización propia.

---

### `ConcurrencyMode.Reentrant`

Los servicios duplex utilizan:

```csharp
ConcurrencyMode.Reentrant
```

Esta configuración permite que una operación del servicio pueda realizar callbacks WCF sin mantener bloqueada permanentemente la instancia del servicio.

Es apropiada para el modelo utilizado por Lobby, donde después de realizar una operación se pueden enviar notificaciones a otros clientes:

```text
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
> `ConcurrencyMode.Reentrant` controla cómo WCF administra las llamadas sobre la instancia del servicio. No hace thread-safe a `LobbyManager`, `Dictionary`, listas u otros objetos compartidos.

La protección del estado compartido se realiza explícitamente mediante mecanismos de sincronización.

## Estado compartido del servidor

### `LobbyManager`

`LobbyManager` mantiene información utilizada por diferentes jugadores y sesiones:

```csharp
private readonly Dictionary<string, Lobby> _lobbies;
private readonly Dictionary<int, string> _playerLobbyCodes;
private readonly Dictionary<int, List<ChatMessage>> _lobbyMessages;

private int _nextLobbyId;
private int _nextMessageId;
```

Estos objetos representan estado compartido.

Por ejemplo, dos clientes podrían intentar entrar simultáneamente al mismo lobby.

Sin sincronización podría ocurrir:

```text
Jugador A                     Jugador B
    │                             │
    ▼                             ▼
lee Participants.Count = 3    lee Participants.Count = 3
    │                             │
    ▼                             ▼
agrega jugador                agrega jugador
```

Si el máximo fuera cuatro jugadores, ambas operaciones podrían considerar válido el estado anterior y producir un resultado inconsistente.


## Uso de `lock`

Para proteger este estado se utiliza un único objeto de sincronización:

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

```text
CreateLobby
JoinPlayer
LeavePlayer
GetLobby
StartLobby
ReleaseMatch
CloseLobby
AddChatMessage
GetChatMessages
GetChatRecipients
```

---

### ¿Qué garantiza el `lock`?

Mientras un hilo ejecuta:

```csharp
lock (_syncRoot)
{
    // sección crítica
}
```

otro hilo que necesite el mismo `_syncRoot` debe esperar.

Conceptualmente:

```text
Thread A
   │
   ▼
lock adquirido
   │
   │ modifica Lobby
   │
   ▼
lock liberado
             │
             ▼
         Thread B
             │
             ▼
       lock adquirido
```

Esto permite que operaciones compuestas se ejecuten como una única sección crítica.

---

### Por qué no solo usar colecciones concurrentes

El problema de `LobbyManager` no consiste únicamente en realizar una operación segura sobre un `Dictionary`.

Una sola operación puede modificar varias estructuras relacionadas.

Por ejemplo, crear un lobby implica:

```text
generar LobbyId
    │
    ▼
crear Lobby
    │
    ├── agregar a _lobbies
    ├── agregar a _playerLobbyCodes
    ├── crear _lobbyMessages
    └── incrementar _nextLobbyId
```

Todas estas modificaciones deben representar una sola transición consistente.

Aunque cada colección fuera individualmente thread-safe, podría ocurrir que otro hilo observara un estado intermedio.

Por eso se utiliza:

```csharp
lock (_syncRoot)
```

para proteger la **operación completa**, no solamente una llamada a una colección.

---

### Regla para utilizar `lock`

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

    if (lobby.Participants.Count >= lobby.Settings.MaxPlayers)
    {
        throw new LobbyRuleException(LobbyIssue.Full);
    }

    lobby.Participants.Add(CopyPlayer(player));
    _playerLobbyCodes.Add(player.PlayerId, lobby.LobbyCode);
}
```

La validación:

```csharp
lobby.Participants.Count
```

y la modificación:

```csharp
lobby.Participants.Add(...)
```

deben permanecer protegidas por el mismo `lock`.

---

### Métodos internos y `lock`

Los métodos auxiliares de `LobbyManager`, como:

```text
GetRegisteredLobby
GetPlayerLobby
CloseLobby
ValidatePlayerCanEnterLobby
GenerateLobbyCode
```

no adquieren necesariamente su propio `lock`.

Se ejecutan dentro de una operación pública que ya posee `_syncRoot`.

La estructura buscada es:

```text
Método público
   │
   ▼
lock (_syncRoot)
   │
   ├── validaciones internas
   ├── búsquedas internas
   ├── modificaciones
   └── creación del snapshot
```

Esto evita distribuir múltiples locks sobre una misma operación.

---

### No exponer el estado mutable

Otro elemento importante de la estrategia es que `LobbyManager` no devuelve directamente los objetos internos que mantiene.

En su lugar crea snapshots:

```csharp
return CreateSnapshot(lobby);
```

También realiza copias de:

```text
Player
LobbySettings
ChatMessage
```

Conceptualmente:

```text
Estado interno
    │
    │ copia
    ▼
Snapshot
    │
    ▼
Controller / Service / Client
```

De esta forma, código externo no conserva una referencia que pueda modificar directamente el estado protegido después de salir del `lock`.

> [!IMPORTANT]
> El `lock` protege el estado dentro de `LobbyManager`. Por ello no se deben devolver referencias mutables directas a las colecciones u objetos internos protegidos.

---

### Duración del `lock`

La sección protegida debe limitarse al trabajo necesario para mantener consistente el estado compartido.

La comunicación WCF con los clientes no debe realizarse dentro del `lock` de `LobbyManager`.

En Lobby el flujo es:

```text
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


## Concurrencia en `CallbackRegistry`

Los callbacks representan otro estado compartido.

Varias sesiones WCF pueden:

- registrar un callback;
- consultar un callback;
- eliminar un callback;

simultáneamente.

Para este caso se utiliza:

```csharp
ConcurrentDictionary<int, ILobbyCallback>
```

dentro de:

```text
LobbyCallbackRegistry
```

---

### Por qué aquí se usa `ConcurrentDictionary`

Las operaciones del registry son simples e independientes:

```text
PlayerId → Callback
```

Las operaciones principales son:

```csharp
TryAdd(...)
TryGetValue(...)
Remove(...)
```

No es necesario coordinar varias colecciones ni mantener una transición compleja entre diferentes estructuras.

Por ello `ConcurrentDictionary` es suficiente para este caso.

La diferencia con `LobbyManager` es:

```text
LobbyManager
    │
    └── varias estructuras + invariantes
            │
            ▼
      lock (_syncRoot)
```

```text
LobbyCallbackRegistry
    │
    └── asociación independiente
        PlayerId → Callback
            │
            ▼
     ConcurrentDictionary
```

---

## Regla para elegir entre `lock` y colecciones concurrentes

### Utilizar `lock` cuando:

- varias instrucciones forman una operación atómica;
- existen varias colecciones relacionadas;
- una validación debe mantenerse válida hasta completar la modificación;
- se debe proteger una invariantes del dominio.

### Utilizar una colección concurrente cuando:

- la estructura es compartida entre hilos;
- las operaciones son principalmente independientes;
- las operaciones atómicas proporcionadas por la colección son suficientes.

No debe sustituirse automáticamente un `Dictionary` por `ConcurrentDictionary` suponiendo que eso hace thread-safe toda la lógica que lo utiliza.



# Concurrencia en el cliente WPF

WPF posee un hilo de interfaz encargado de:

- procesar entrada del usuario;
- actualizar controles;
- ejecutar bindings;
- renderizar la interfaz.

Si una llamada remota se ejecuta directamente sobre este hilo:

```text
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

la interfaz permanece bloqueada mientras finaliza la operación.

Para evitarlo, el proyecto utiliza:

```text
Services/Threading/
├── IBackgroundJob
├── BackgroundJob
└── BackgroundWork
```

---

## `BackgroundWork`

`BackgroundWork` envía el trabajo al `ThreadPool`:

```csharp
ThreadPool.QueueUserWorkItem(
    ExecuteJob,
    job);
```

Por lo tanto, la operación no se ejecuta sobre el hilo principal de WPF.

El flujo es:

```text
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

Cuando termina, la continuación debe regresar al hilo de UI.

Para esto se utiliza:

```csharp
Application.Current.Dispatcher.BeginInvoke(
    new Action(job.Complete));
```

El flujo completo es:

```text
UI Thread
   |
   | Start()
   ▼
ThreadPool worker
   |
   | Execute()
   ▼
operación remota
   |
   ▼
resultado
   |
   | Dispatcher.BeginInvoke()
   ▼
UI Thread
   |
   ▼
Complete()
```

---

## `BackgroundJob`

`BackgroundJob<TState, TResult>` encapsula:

```text
Work
State
OnCompleted
OnFailed
OnCompletedAfterCancel
Result
Error
```

Esto permite separar claramente:

```text
Trabajo en segundo plano
        |
        ▼
Work
```

de:

```text
Actualización de interfaz
        |
        ▼
OnCompleted / OnFailed
```

Los callbacks de finalización se ejecutan después de regresar al `Dispatcher`.

Por eso es seguro que estos callbacks actualicen propiedades utilizadas por la interfaz WPF.

---

### Ejemplo conceptual

```text
ViewModel
   |
   | crea BackgroundJob
   ▼
BackgroundWork.Start()
   |
   ▼
Worker Thread
   |
   | ILobbyClient.JoinLobby(...)
   ▼
WCF
   |
   ▼
resultado
   |
   ▼
Dispatcher
   |
   ▼
OnLobbyReady(...)
   |
   ▼
ViewModel / UI
```

Este mecanismo se utiliza actualmente en funcionalidades como creación, unión, salida e inicio de Lobby.


### Cancelación

`BackgroundJob` contiene:

```csharp
Cancel()
```

y protege su estado mediante:

```csharp
lock (_stateLock)
```

Este `lock` protege exclusivamente:

```csharp
_isCancelled
```

La cancelación no detiene forzosamente una operación WCF que ya comenzó.

Su función es indicar que el resultado normal ya no debe procesarse como una operación activa de la interfaz.

Por ello existen dos posibles caminos:

```text
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


## Uso de `Task`, `Task.Run`, `async` y `await`

La implementación actual no utiliza `Task.Run` ni un flujo basado en `async`/`await` para ejecutar las operaciones remotas de la aplicación.

Esto es una decisión de implementación, no una limitación del lenguaje o de .NET Framework 4.8.

> [!IMPORTANT]
> `async`/`await` no son incorrectos. Simplemente no forman parte del modelo de concurrencia adoptado actualmente por el proyecto.

---

### Por qué no `Task.Run` sobre las llamadas WCF

Los proxies utilizados por el proyecto exponen las operaciones que estamos consumiendo de forma síncrona.

Por ejemplo:

```csharp
_client.JoinLobby(lobbyCode);
```

Colocar esta operación dentro de:

```csharp
Task.Run(...)
```

no convierte la operación WCF en una operación de red verdaderamente asíncrona.

Únicamente ejecutaría la llamada síncrona en otro hilo del `ThreadPool`.

Eso es precisamente lo que el proyecto ya centraliza mediante:

```text
BackgroundWork
    │
    ▼
ThreadPool.QueueUserWorkItem
```

Introducir `Task.Run` adicionalmente produciría dos formas diferentes de resolver el mismo problema.

---

### Por qué no `async`/`await`

Para adoptar `async`/`await` como modelo general sería necesario definir de forma consistente:

- contratos o proxies asíncronos;
- interfaces del cliente con métodos `Task<T>`;
- ViewModels asíncronos;
- propagación de excepciones mediante `Task`;
- cancelación;
- ciclo de vida de los canales WCF;
- tratamiento de callbacks duplex;
- pruebas correspondientes.

La implementación actual ya posee una solución común para ejecutar operaciones sin bloquear WPF:

```text
BackgroundJob
+
BackgroundWork
+
ThreadPool
+
Dispatcher
```

Por consistencia, las nuevas operaciones deben utilizar este mecanismo.

---

### Uso interno de `Task` en `BackgroundJob`

`BackgroundJob` sí utiliza internamente:

```csharp
Task<TResult>
```

mediante:

```csharp
Task<TResult> operation =
    new Task<TResult>(() => Work(State));

operation.RunSynchronously();
```

Esto no representa un modelo `async`/`await`.

El `Task` se ejecuta sincrónicamente sobre el worker (hilo de trabajo del ThreadPool) que `BackgroundWork` ya obtuvo del `ThreadPool`.

Por lo tanto:

```text
BackgroundWork
    │
    │ obtiene worker
    ▼
BackgroundJob.Execute()
    │
    │ Task.RunSynchronously()
    ▼
Work(State)
```

No se crea una segunda operación concurrente mediante `Task.Run`.


## Regla para operaciones desde ViewModel

Una operación que pueda bloquear por comunicación, persistencia o trabajo relevante no debe ejecutarse directamente sobre el hilo de UI.

El patrón es:

```csharp
BackgroundJob<TState, TResult> job =
    new BackgroundJob<TState, TResult>(
        WorkOnWorker,
        state,
        OnCompleted,
        OnFailed);

BackgroundWork.Start(job);
```

La separación debe mantenerse:

```text
WorkOnWorker
    │
    └── comunicación / procesamiento
```

```text
OnCompleted
OnFailed
    │
    └── actualización del estado del ViewModel
```



# Regla general de implementación

Al implementar una nueva funcionalidad se debe identificar primero qué tipo de estado concurrente existe.

```text
¿Existe estado mutable compartido?
          |
     ┌────┴────┐
     |         |
    Sí         No
     |
     ▼
¿la operación necesita
varios pasos atómicos?
     |
 ┌───┴────┐
 |        |
Sí        No
 |        |
 ▼        ▼
lock   colección concurrente
       si sus operaciones
       son suficientes
```

En el cliente:

```text
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


# Resumen

La estrategia de concurrencia adoptada se divide en cuatro mecanismos.

```text
WCF duplex
    │
    ├── InstanceContextMode.PerSession
    └── ConcurrencyMode.Reentrant
```

```text
Estado de dominio compartido
    │
    └── lock (_syncRoot)
        Ejemplo: LobbyManager
```

```text
Registro concurrente simple
    │
    └── ConcurrentDictionary
        Ejemplo: LobbyCallbackRegistry
```

```text
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
