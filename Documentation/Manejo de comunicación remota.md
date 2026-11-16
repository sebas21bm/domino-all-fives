# Manejo de comunicación remota

### Framework

Domino All Fives utiliza **Windows Communication Foundation (WCF)** para la comunicación entre el cliente WPF y el servidor.

La implementación distingue dos tipos de comunicación:

- **Request/response**, para operaciones en las que el cliente realiza una solicitud y espera directamente una respuesta.
- **Duplex**, para funcionalidades multijugador donde, además de responder solicitudes, el servidor debe enviar notificaciones a los clientes conectados.

El servidor mantiene el estado autoritativo. El cliente solicita operaciones y consume los resultados o notificaciones recibidas mediante WCF.

### Configuración

### Binding
Se utiliza `netTcpBinding` para la comunicación cliente-servidor.

Las operaciones que no requieren notificaciones posteriores utilizan llamadas request/response. Las funcionalidades que necesitan actualizaciones iniciadas por el servidor utilizan contratos duplex con callbacks.

## Organización de los servicios

Los contratos request/response se implementan en `DominoService`, como
`IRankingService`. No requieren un servicio independiente ni un archivo parcial
por contrato. Durante la refactorización se conserva esta organización; cualquier
separación debe tener una necesidad concreta y acordarse antes de aplicarla.
Los contratos duplex conservan sus servicios y el estado de su sesión según el
flujo documentado.

## Comunicación request/response
Para cada operación:

1. El cliente crea el proxy WCF.
2. Invoca la operación remota.
3. El servidor procesa la solicitud.
4. El resultado o fault regresa al cliente.
5. El proxy se cierra.
6. Si la operación falla antes del cierre normal, el proxy se aborta.

El flujo general es:

```text
ViewModel
    │
    ▼
Client service (INameService'Client -> NameServiceClient)
    │
    ▼
Generated WCF proxy
    │
    │ net.tcp
    ▼
WCF Service (Implementado en DominoService:IService)
    │
    ▼
Controller / Business Logic 
(maneja los Repository y el contexto de Entity Framework)
    │
    ▼
Result (según el método) or Fault
    │
    ▼
Client (al ViewModel)
```
---
### Manejo del proxy

Las llamadas request/response utilizan `WcfInvoker`.

```text
Services/Wcf/Proxies/WcfInvoker.cs
```

Su responsabilidad es ejecutar una operación sobre un proxy generado y garantizar su liberación.

El patrón utilizado es:

```csharp
TProxy proxy = createProxy();

try
{
    TResult result = operation(proxy);
    proxy.Close();
    return result;
}
finally
{
    if (!completed)
    {
        proxy.Abort();
    }
}
```

La regla para este tipo de comunicación es:
> [!IMPORTANT]
> Un proxy request/response se crea para la operación, se utiliza y posteriormente se libera. No se conserva como estado permanente del cliente.

Esto evita reutilizar canales WCF que hayan quedado en estado `Faulted`.


---
### Ejemplo: Login

El cliente utiliza:

```text
LoginClient (:ILoginClient)
    │
    ▼
SessionAccountClient
    │
    ▼
AuthenticationEndpoint
    │
    ▼
DominoService
```

`LoginClient` no contiene lógica de autenticación. Su responsabilidad consiste en adaptar la llamada del cliente al proxy generado:

```csharp
return WcfInvoker.Invoke<SessionAccountClient, PlayerAccount>(
    () => new SessionAccountClient("AuthenticationEndpoint"),
    proxy => proxy.LoginWithAccount(credentials));
```

El resultado de la operación es un `PlayerAccount` o un fault WCF cuando la operación no puede completarse.

---

### Ejemplo: Ranking

Ranking utiliza el mismo mecanismo.

```text
RankingClient
    │
    ▼
RankingRemoteServiceClient
    │
    ▼
RankingEndpoint
    │
    ▼
DominoService
```

Aquí se realizan dos operaciones independientes:

```csharp
GetTopWinsRanking()
GetWinsRankingPositionById(int playerId)
```

Cada operación crea y libera su propio proxy mediante `WcfInvoker`.


## Comunicación duplex

Lobby requiere que el servidor pueda enviar cambios a los jugadores sin esperar una nueva solicitud.

Para ello se utiliza WCF duplex:

```text
             request
Client ─────────────────► Server
   ▲                       │
   │                       │
   └───────────────────────┘
           callback
```

A diferencia de request/response, aquí el cliente mantiene abierto el canal durante el tiempo que permanece dentro del contexto.

En Lobby ese contexto comienza al crear o unirse a un lobby y termina al salir, cerrar el lobby, perder la conexión o liberar el cliente.



El contrato WCF declara un `CallbackContract` y requiere una sesión:

```csharp
[ServiceContract(
    CallbackContract = typeof(...),
    SessionMode = SessionMode.Required)]
```

El servidor utiliza:

```csharp
InstanceContextMode.PerSession
ConcurrencyMode.Reentrant
```

Por lo tanto, cada conexión duplex mantiene una instancia de servicio asociada a su sesión WCF.


### Componentes de una comunicación duplex

La implementación de Lobby se divide en las siguientes piezas:

```text
CLIENTE
│
├── LobbyClient
│
├── LobbyCallbackHandler
│
└── LobbyServiceClient
       │
       │ WCF duplex
       ▼
SERVIDOR
│
├── ILobbyService
├── ILobbyCallback
├── LobbyService
├── LobbyCallbackRegistry
└── LobbyEventPublisher
```


#### `ILobbyService`

Define las operaciones que el **cliente puede solicitar al servidor**.

Por ejemplo:

```text
OpenLobbySession
CreateLobby
JoinLobby
GetLobby
LeaveLobby
StartMatch
CloseLobby
```

Además, declara que el servicio es duplex:

```csharp
[ServiceContract(
    Namespace = "urn:domino-all-fives:services",
    CallbackContract = typeof(ILobbyCallback),
    SessionMode = SessionMode.Required)]
```

`CallbackContract` indica qué interfaz puede utilizar el servidor para comunicarse nuevamente con el cliente.

`SessionMode.Required` indica que las operaciones pertenecen a una sesión WCF.

---

#### `ILobbyCallback`

Define las operaciones que el **servidor puede invocar sobre el cliente**.

```csharp
OnLobbyChanged(Lobby lobby)
OnLobbyClosed()
OnMatchStarted(int matchId)
```

Estas operaciones están declaradas como:

```csharp
[OperationContract(IsOneWay = true)]
```

Por lo tanto, son notificaciones. El servidor las envía al cliente sin esperar un valor de retorno de la operación.

---

#### `LobbyClient`

Es el punto de entrada utilizado por el resto del cliente WPF.

Su responsabilidad es:

- crear el canal duplex;
- asociarlo al callback;
- abrir la sesión;
- conservar el proxy mientras el jugador permanece en el lobby;
- realizar las operaciones remotas;
- recibir los eventos producidos por los callbacks;
- cerrar o abortar correctamente la conexión.

El ViewModel no debe crear directamente un `LobbyClient`.

Debe consumir la abstracción correspondiente a `ILobbyClient`.

---

#### `LobbyCallbackHandler`

Es la implementación del callback en el cliente.

Implementa el contrato generado:

```csharp
ILobbyServiceCallback
```

Cuando WCF recibe una llamada del servidor, ejecuta uno de sus métodos:

```csharp
public void OnLobbyChanged(Lobby lobby)
{
    LobbyChanged?.Invoke(lobby);
}
```

El callback no contiene lógica de presentación.

Transforma la llamada WCF en eventos internos:

```csharp
LobbyChanged
LobbyClosed
MatchStarted
```

De esta manera, el resto del cliente no depende directamente del callback generado por WCF.

---

#### `LobbyService`

Es la implementación del servicio en el servidor:

```csharp
public sealed class LobbyService : ILobbyService
```

Su configuración actual es:

```csharp
[ServiceBehavior(
    InstanceContextMode = InstanceContextMode.PerSession,
    ConcurrencyMode = ConcurrencyMode.Reentrant,
    IncludeExceptionDetailInFaults = false)]
```

`InstanceContextMode.PerSession` hace que la instancia de `LobbyService` corresponda a una sesión WCF concreta.

Por eso la clase puede conservar información asociada a esa conexión, como:

```csharp
private Player _player;
private ILobbyCallback _callback;
private ICommunicationObject _callbackChannel;
```

> [!IMPORTANT]
> `ConcurrencyMode.Reentrant` permite que el servicio realice callbacks durante una operación WCF. No significa que los objetos compartidos sean automáticamente thread-safe. La sincronización del estado compartido se trata por separado en el documento de concurrencia.

---

#### `LobbyCallbackRegistry`

El servidor necesita saber qué callback corresponde a cada jugador conectado.

Para esto se utiliza:

```text
LobbyCallbackRegistry
```

Internamente mantiene:

```csharp
ConcurrentDictionary<int, ILobbyCallback>
```

Conceptualmente:

```text
PlayerId       Callback
────────       ───────────────
10       ───►  callback A
22       ───►  callback B
31       ───►  callback C
```

Esto permite que posteriormente `LobbyEventPublisher` encuentre el canal correspondiente a cada participante.

---

#### `LobbyEventPublisher`

Es responsable de distribuir una notificación a los jugadores correspondientes.

Expone operaciones como:

```csharp
PublishLobbyChanged(...)
PublishLobbyClosed(...)
PublishMatchStarted(...)
```

El servicio no recorre directamente los callbacks.

En su lugar:

```text
LobbyService
      │
      ▼
LobbyEventPublisher
      │
      ▼
LobbyCallbackRegistry
      │
      ▼
Callbacks de los jugadores
```

Esto separa la ejecución del caso de uso de la distribución de eventos remotos.

---

### Apertura de una sesión duplex

La creación de una comunicación duplex en Lobby sigue varios pasos.

#### 1. Crear el callback en el cliente

`LobbyClient` crea:

```csharp
LobbyCallbackHandler callback = new LobbyCallbackHandler();
```

Esta instancia será el objeto sobre el cual el servidor podrá ejecutar:

```text
OnLobbyChanged
OnLobbyClosed
OnMatchStarted
```

---

#### 2. Crear el `InstanceContext`

El callback se entrega a WCF mediante:

```csharp
new InstanceContext(callback)
```

Luego se crea el proxy:

```csharp
new LobbyServiceClient(
    new InstanceContext(callback),
    "LobbyEndpoint")
```

Conceptualmente:

```text
LobbyClient
   │
   ├── crea LobbyCallbackHandler
   │          │
   │          ▼
   │    InstanceContext
   │          │
   │          ▼
   └── crea LobbyServiceClient
                  │
                  ▼
                 WCF
```

El `InstanceContext` indica a WCF qué objeto local debe recibir las llamadas provenientes del servidor.

---

#### 3. Conectar los callbacks con eventos del cliente

Antes de abrir la conexión, `LobbyClient` suscribe eventos al `LobbyCallbackHandler`.

Por ejemplo:

```text
LobbyCallbackHandler.LobbyChanged
             │
             ▼
LobbyClient.OnLobbyChanged(...)
             │
             ▼
LobbyClient.LobbyChanged
             │
             ▼
ViewModel
```

Esto mantiene la infraestructura WCF separada del código de presentación.

---

#### 4. Abrir el canal

Después se abre físicamente la conexión:

```csharp
_client.Open();
```

En este punto existe un canal WCF, pero todavía no está asociado a un jugador autenticado.

---

#### 5. Asociar el canal a la sesión del jugador

Inmediatamente después se ejecuta:

```csharp
_client.OpenLobbySession(
    _playerSession.SessionKey);
```

El servidor recibe esta operación en:

```csharp
LobbyService.OpenLobbySession(...)
```

El `sessionKey` se utiliza para recuperar al jugador mediante `SessionManager`.

Por lo tanto:

```text
sessionKey
    │
    ▼
SessionManager
    │
    ▼
Player
```

La sesión WCF queda asociada al jugador autenticado.

---

#### 6. Obtener el callback desde el servidor

Una vez autenticado el jugador, `LobbyService` obtiene el canal de retorno mediante:

```csharp
OperationContext.Current
    .GetCallbackChannel<ILobbyCallback>();
```

Este es un punto fundamental de la comunicación duplex.

El cliente no envía manualmente el objeto callback como parámetro.

WCF lo mantiene como parte del canal duplex y el servidor lo recupera mediante `OperationContext`.

---

#### 7. Registrar el callback

El callback se registra mediante:

```csharp
callbacks.RegisterCallback(
    _player.PlayerId,
    callback);
```

A partir de este momento:

```text
PlayerId
    │
    ▼
LobbyCallbackRegistry
    │
    ▼
ILobbyCallback
```

El servidor sabe qué canal debe utilizar para notificar a ese jugador.

---

### Flujo completo de apertura

El proceso completo queda de esta forma:

```text
CLIENTE
LobbyClient
    │
    │ crea
    ▼
LobbyCallbackHandler
    │
    │ se entrega a
    ▼
InstanceContext
    │
    │ crea
    ▼
LobbyServiceClient
    │
    │ Open()
    ▼
Canal WCF
    │
    │ OpenLobbySession(sessionKey)
    ▼
SERVIDOR
LobbyService
    │
    │ SessionManager.GetPlayer(sessionKey)
    ▼
Player autenticado
    │
    │ GetCallbackChannel<ILobbyCallback>()
    ▼
Callback del cliente
    │
    │ RegisterCallback(PlayerId, callback)
    ▼
LobbyCallbackRegistry
```

Después de este proceso, el canal está listo para recibir operaciones y callbacks.

---

### Ejecución de una operación duplex

Una vez abierta la sesión, las operaciones siguen siendo llamadas normales cliente → servidor.

Por ejemplo, al entrar un jugador:

```csharp
_client.JoinLobby(lobbyCode);
```

El flujo es:

```text
Jugador A
    │
    │ JoinLobby()
    ▼
LobbyClient
    │
    ▼
LobbyServiceClient
    │
    │ net.tcp
    ▼
LobbyService.JoinLobby()
    │
    ▼
LobbyController
    │
    ▼
LobbyManager
```

Hasta este punto el comportamiento es similar a request/response.

La diferencia aparece después de modificar el estado.

`LobbyService` ejecuta:

```csharp
publisher.PublishLobbyChanged(lobby);
```

---

### Publicación hacia los demás clientes

`LobbyEventPublisher` recibe el lobby actualizado.

Después recorre:

```csharp
lobby.Participants
```

Para cada participante obtiene su callback mediante:

```csharp
_callbacks.GetCallback(player.PlayerId);
```

y ejecuta:

```csharp
callback.OnLobbyChanged(lobby);
```

El flujo completo queda:

```text
Jugador A
    │
    │ JoinLobby()
    ▼
LobbyService
    │
    ▼
LobbyController / LobbyManager
    │
    │ estado actualizado
    ▼
LobbyEventPublisher
    │
    ├── busca callback de A
    ├── busca callback de B
    └── busca callback de C
            │
            ▼
     ILobbyCallback
```

Entonces:

```text
                    SERVER
                      │
          LobbyEventPublisher
             ┌────────┼────────┐
             ▼        ▼        ▼
         callback A callback B callback C
             │        │        │
             ▼        ▼        ▼
          Client A Client B Client C
```

El jugador que realizó la operación también puede recibir el callback porque forma parte de los participantes del lobby.

---

### Recepción del callback en el cliente

Cuando el servidor ejecuta:

```csharp
callback.OnLobbyChanged(lobby);
```

WCF redirige la llamada al `LobbyCallbackHandler` asociado a ese cliente:

```csharp
public void OnLobbyChanged(Lobby lobby)
{
    LobbyChanged?.Invoke(lobby);
}
```

Ese evento llega posteriormente a `LobbyClient`.

`LobbyClient` actualiza:

```csharp
CurrentLobby = lobby;
```

y publica:

```csharp
LobbyChanged?.Invoke(lobby);
```

Finalmente el ViewModel puede reaccionar a dicho evento.

El recorrido completo de regreso es:

```text
LobbyEventPublisher
        │
        ▼
ILobbyCallback
        │
        │ WCF
        ▼
LobbyCallbackHandler
        │
        ▼
LobbyClient
        │
        ▼
LobbyChanged event
        │
        ▼
ViewModel
        │
        ▼
View
```

Por lo tanto, el ViewModel no conoce:

- `OperationContext`;
- `InstanceContext`;
- `ILobbyServiceCallback`;
- `LobbyCallbackRegistry`;
- `LobbyEventPublisher`.

Solo consume eventos expuestos por `LobbyClient`.

---

### Ciclo completo de una operación

Por ejemplo, si el jugador A entra al lobby del jugador B:

```text
CLIENT A
   │
   │ JoinLobby(code)
   ▼
LobbyClient
   │
   ▼
WCF Proxy
   │
   ▼
──────────────────────────────
            SERVER
──────────────────────────────
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
   │ modifica estado
   ▼
LobbyEventPublisher
   │
   ├───────────────┐
   ▼               ▼
Callback A     Callback B
   │               │
──────────────────────────────
            CLIENTS
──────────────────────────────
   │               │
   ▼               ▼
CallbackHandler CallbackHandler
   │               │
   ▼               ▼
LobbyClient     LobbyClient
   │               │
   ▼               ▼
ViewModel A     ViewModel B
```

Esta es la estructura que se debe seguir cuando una nueva funcionalidad requiera comunicación duplex.

---

### Eliminación de callbacks

El registro de callbacks no puede permanecer indefinidamente.

En `LobbyService`, el canal del callback se observa mediante:

```csharp
_callbackChannel.Closed += OnCallbackEnded;
_callbackChannel.Faulted += OnCallbackEnded;
```

Cuando el canal se cierra o entra en estado `Faulted`, se ejecuta:

```csharp
callbacks.RemoveCallback(
    _player.PlayerId,
    _callback);
```

Así se evita conservar callbacks correspondientes a conexiones que ya no existen.

---

### Fallo durante una publicación

También es posible que el canal deje de funcionar antes de que el servidor detecte su cierre.

Por ello `LobbyEventPublisher` protege cada callback.

Si durante una notificación ocurre:

```text
CommunicationException
TimeoutException
ObjectDisposedException
```

el callback se elimina del registry y se registra el problema mediante el logger.

Conceptualmente:

```text
Publish callback
      │
      ├── éxito ─────► continuar
      │
      └── error
            │
            ▼
      RemoveCallback()
            │
            ▼
         LogWarning
```

El fallo de comunicación con un jugador no debe impedir que se intente notificar a los demás participantes.

---

### Cierre del canal en el cliente

Los canales WCF deben cerrarse explícitamente.

En comunicación duplex, `LobbyClient` conserva el proxy mientras el contexto sigue activo.

Al salir del lobby se ejecuta el proceso de limpieza:

```text
Reset()
   │
   ├── UnsubscribeCallbacks()
   │
   ├── CloseConnection()
   │
   └── limpiar estado local
```

Si el canal es utilizable:

```text
Channel
   │
   ▼
Close()
```

Si se encuentra en estado `Faulted`:

```text
Faulted
   │
   ▼
Abort()
```

También se utiliza `Abort()` si `Close()` produce:

```text
CommunicationException
TimeoutException
```

> [!IMPORTANT]
> Un canal duplex se conserva mientras dura el contexto que necesita recibir notificaciones. Una vez cerrado o faulted, no debe reutilizarse.

---

### Regla para implementar otro servicio duplex

Si se necesita implementar una nueva funcionalidad siguiendo el modelo de Lobby, deben existir como mínimo las siguientes responsabilidades:

```text
Contracts/Services
├── IFeatureService
└── IFeatureCallback

Client/Services/Wcf
├── Interfaces/
│   └── IFeatureClient
├── Services/
│   └── FeatureClient
└── Callbacks/
    └── FeatureCallbackHandler

Server
├── FeatureService
└── Infrastructure/
    └── Feature/
        ├── FeatureCallbackRegistry
        └── FeatureEventPublisher
```

El proceso debe seguir este orden:

```text
1. Definir ServiceContract + CallbackContract
2. Definir operaciones cliente → servidor
3. Definir callbacks servidor → cliente
4. Implementar CallbackHandler en el cliente
5. Crear InstanceContext(callback)
6. Crear y abrir el proxy duplex
7. Abrir/autenticar la sesión
8. Obtener GetCallbackChannel<T>() en servidor
9. Registrar callback por jugador
10. Ejecutar operación de negocio
11. Publicar cambio mediante EventPublisher
12. Resolver destinatarios desde CallbackRegistry
13. Ejecutar callbacks
14. Transformarlos a eventos del cliente
15. Eliminar callbacks al cerrar/fallar el canal
```

No se debe colocar lógica de negocio dentro del callback, del registry ni del publisher.

Sus responsabilidades son exclusivamente de comunicación e infraestructura.


## Regla de implementación

Para nuevas funcionalidades remotas se debe decidir primero si requieren únicamente una respuesta directa o si el servidor necesita iniciar notificaciones.

### Utilizar request/response cuando:

- la operación comienza siempre por una solicitud del cliente;
- el resultado puede regresarse directamente;
- no es necesario mantener un canal abierto;
- no existen actualizaciones que el servidor deba enviar posteriormente.

### Utilizar duplex cuando:

- varios clientes observan un mismo estado;
- una acción de un jugador debe actualizar a otros jugadores;
- el servidor necesita enviar eventos sin una nueva solicitud;
- el contexto requiere mantener una sesión de comunicación.

No se debe introducir comunicación duplex cuando una operación request/response sea suficiente.


## Resumen

La comunicación remota de Domino All Fives sigue dos mecanismos definidos.

```text
Request/response
Login
Ranking
    │
    └── proxy por operación
        ├── crear
        ├── invoke
        ├── Close()
        └── Abort() si falla
```

```text
Duplex
Lobby
    │
    └── canal mantenido durante el contexto
        ├── CallbackHandler
        ├── InstanceContext
        ├── proxy duplex
        ├── sesión autenticada
        ├── CallbackRegistry
        ├── EventPublisher
        ├── callbacks servidor → cliente
        └── liberación al abandonar el contexto
```

Estas reglas constituyen la referencia para implementar nuevos casos de uso remotos sin introducir mecanismos de comunicación distintos de los ya adoptados por el proyecto.