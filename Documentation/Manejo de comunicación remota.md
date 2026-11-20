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

### Responsabilidad sobre Entity Framework

En el servidor, la implementación WCF actúa como frontera remota: recibe la
solicitud, invoca el controller, traduce excepciones a faults y registra el fallo
en el nivel correspondiente. No crea repositorios ni administra el contexto de
Entity Framework.

El controller que ejecuta el caso de uso crea el contexto y los repositorios que
necesita la operación. El contexto debe limitarse a esa operación y liberarse con
`using` antes de devolver el resultado. Este patrón aplica tanto a Ranking como a
Login y a los siguientes contratos que accedan a persistencia.

```csharp
public PlayerAccount LoginWithAccount(Credentials credentials)
{
    ValidateCredentials(credentials);

    using (DominoAllFivesEntities context = new DominoAllFivesEntities())
    {
        IPlayerRepository playerRepository = new PlayerRepository(context);
        return Authenticate(credentials, playerRepository);
    }
}
```

No se agregan constructores, sobrecargas ni rutas alternativas para inyectar el
repositorio en estos controllers. Las pruebas que requieran persistencia deben
usar infraestructura aislada; la facilidad de sustitución en pruebas no cambia
la responsabilidad de composición del controller.

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
SessionClient (:ISessionClient)
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

`SessionClient` no contiene lógica de autenticación. Su responsabilidad consiste en adaptar la llamada del cliente al proxy generado:

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


### Ejemplo: Registro de cuenta

El registro utiliza el patrón request/response:
RegisterAccountViewModel → IRegistrationClient / RegistrationClient →
RegistrationServiceClient (proxy generado) → RegistrationEndpoint
(net.tcp, ruta Domino/registration) → DominoService : IRegistrationService →
RegistrationController → RegistrationValidator / repositorios / Entity Framework.

`Register(AccountRegistration)` devuelve un `PlayerAccount` con su `SessionKey`.
El controller conserva las reglas de negocio, el hash de contraseña y la creación
de Player y PlayerStats dentro del contexto de una operación. Devuelve directamente
PlayerAccount y comunica los rechazos con RegistrationRuleException, que contiene
RegistrationIssue. DominoService captura esa excepción y lanza directamente
new FaultException<RegistrationIssue>(ex.Issue), como Login. Los errores técnicos
se propagan hasta la frontera y se traducen a faults seguros.
Una solicitud nula produce InvalidRegistrationData. La frontera abre la sesión
con SessionManager después del alta, como en Login.

El ViewModel comprueba campos requeridos y coincidencia de contraseñas, captura
usuario/correo sin espacios externos y el idioma actual, y conserva la contraseña
sin transformarla. Ejecuta el registro con BackgroundJob y BackgroundWork;
deshabilita registrar y cancelar mientras espera. Clasifica rechazo, fault técnico
y pérdida de comunicación para seleccionar mensajes existentes de los recursos.
Después de validar la respuesta inicia PlayerSession con cuatro argumentos y
navega al menú principal al aceptar el mensaje de éxito.

Los metadatos WSDL/XSD se conservan junto a la referencia DominoAllFivesService.
RegenerateProxy.ps1 permite exportarlos desde Contracts y generar Reference.cs
con SvcUtil o con el generador de .NET Framework. No requiere iniciar el servidor.
Se ejecuta con Windows PowerShell, indicando el ensamblado Contracts compilado en
`-ContractsAssembly` y, opcionalmente, `-SvcUtilPath`.

WcfInvoker conserva el resultado recibido cuando Close falla por comunicación o
timeout: registra el cierre anómalo y aborta el canal. Un timeout durante la
operación sigue propagándose. No hay reintento automático de registro: un fallo
de transporte antes de recibir respuesta no permite asegurar si el alta ocurrió.

OldClient permanece como implementación histórica; el cliente actual no
referencia IAccountService local, BusinessLogic, repositorios ni Entity Framework.

[Verificaciones del registro](Verificaciones%20auxiliares.md#migración-del-registro-de-cuenta-2026-11-18).

---

### Perfil de jugador e imagen

El perfil usa IProfileService en DominoService, con el endpoint nuevo
PlayerProfileEndpoint en la ruta Domino/profile y los bindings existentes.
ProfileClient consume el proxy generado mediante WcfInvoker, con el mismo
patrón de lambdas utilizado por SessionClient y RankingClient.

| Operación | Resultado |
| --- | --- |
| GetMyProfile(sessionKey) | PlayerProfile propio con nombre, estadísticas y contenido de imagen opcional. |
| UpdateProfileName(sessionKey, username) | Actualiza el nombre y conserva la imagen. |
| UpdateProfileWithPicture(sessionKey, username, picture) | Guarda nombre e imagen en la operación conjunta del controller. |
| SetProfilePicture(sessionKey, picture) | Establece o reemplaza la imagen. |
| RemoveProfilePicture(sessionKey) | Retira la imagen asignada y aplica la limpieza existente. |

Todas las operaciones resuelven el propietario mediante SessionManager; no reciben
un identificador de jugador elegido por el cliente. Una clave ausente, desconocida
o revocada produce FaultException<AccessIssue>. Los controllers conservan EF,
repositorios, validadores, almacenamiento y limpieza de archivos. Las cuentas
borradas se consideran no encontradas. Los controllers devuelven datos o finalizan
sin resultado al tener éxito; los rechazos se comunican con ProfileRuleException.
DominoService captura esa excepción y lanza directamente
new FaultException<ProfileIssue>(ex.Issue), sin estados ni switches de conversión.
Los errores de persistencia o escritura se registran en la frontera y se convierten
en faults seguros. Los argumentos nulos conservan ArgumentNullException en el
controller y se traducen mediante capturas filtradas por su parámetro en WCF.
Los bloques finally de compensación y la recuperación de limpieza permanecen
en los componentes responsables, sin duplicar el logging de Storage.

PlayerProfile y PlayerStatistics tienen contratos de datos explícitos. Se utiliza
ProfilePicture, que ya transportaba nombre de selección y bytes, sin enviar rutas
de almacenamiento del servidor. Se conserva la validación actual de extensión
y tamaño máximo de 1 MiB; no se añade una política nueva de contenido del lado
servidor.

Los constructores de perfil reciben únicamente PlayerSession y, en la subida
opcional, las acciones de navegación. Inicializan estado y comandos sin acceder
a servicios ni cargar datos. ViewModelFactory inyecta los servicios después de
construirlos y llama a Initialize, con protección frente a uso previo o repetido.

Para respetar el límite de cuatro parámetros por método, Profile configura
primero el logger mediante ConfigureLogger, como Lobby. EditProfile configura
primero los dos servicios locales de imagen mediante ConfigurePictures. Después,
Initialize recibe los servicios restantes y comienza la carga. Upload recibe
sus cuatro servicios en Initialize y espera una acción del usuario. No se crean
contenedores adicionales de dependencias. Los comandos permanecen deshabilitados
hasta que la inicialización está completa.

ProfileViewModel y EditProfileViewModel cargan mediante BackgroundJob. Guardar,
subir y eliminar también usan BackgroundWork; las continuaciones actualizan la
vista en el Dispatcher. Se bloquean acciones incompatibles mientras hay una llamada
pendiente y se capturan clave, nombre e imagen antes de enviarla. La sesión se
actualiza con el nombre enviado que se confirmó, y se ignoran continuaciones cuya
clave ya no corresponde a PlayerSession.

EditProfile conserva la selección y previsualización local, el guardado conjunto,
los mensajes localizados y el regreso con refresco del perfil. El botón de
eliminación pide confirmación y explica que se guarda inmediatamente; descartar
las ediciones de nombre no revierte una eliminación ya confirmada. Al volver
desde perfil se refresca el menú para mostrar el nombre actual.

Después del registro vuelve a mostrarse UploadProfilePicture: subir una imagen
u omitir el paso lleva al menú; cancelar cierra el modal conservando la cuenta.
La previsualización decodifica bytes localmente y las imágenes dañadas recuperan
la imagen predeterminada sin dejar escapar errores de formato o lectura.

WcfInvoker dispone de una sobrecarga para operaciones void que reutiliza el
mismo cierre/aborto del invocador con resultado. El cliente no interpreta estados
de persistencia ni referencia IAccountService, BusinessLogic o Entity Framework.

Se mantienen las URL originales de ConnectedService.json y
dotnet-svcutil.params.json para actualizar desde el servidor en Visual Studio.
El proxy de esta entrega se generó con SvcUtil desde metadatos de DominoService
alojado temporalmente para desarrollo; no se cambiaron los bindings, direcciones
ni endpoints previos.

La evidencia y los límites están en
[Verificaciones auxiliares](Verificaciones%20auxiliares.md#migración-de-perfil-e-imagen-2026-11-19).

---

### Cambio de contraseña, idioma preferido y eliminación de cuenta

`IAccountService` se publica en `DominoService` mediante
`AccountSettingsEndpoint` en la ruta `Domino/account-settings`. El cliente
`AccountClient` utiliza el proxy generado y la sobrecarga `void` de `WcfInvoker`.

| Operación | Efecto confirmado por la respuesta |
| --- | --- |
| `ChangePassword(string sessionKey, string currentPassword, string newPassword)` | Guarda el hash de la nueva contraseña. |
| `DeleteAccount(string sessionKey, string password)` | Guarda el borrado lógico e invalida la sesión de la cuenta. |
| `ChangePreferredLanguage(string sessionKey, string preferredLanguageId)` | Guarda el idioma preferido de la cuenta. |

`DominoService` resuelve la identidad con `SessionManager.GetPlayerId`.
Una clave ausente, desconocida o revocada produce `FaultException<AccessIssue>`
con `SessionRequired`. `AccountController` conserva la comprobación de
credenciales, las reglas de negocio y la persistencia. Lanza
`AccountRuleException` al rechazar una regla. La frontera traduce ese rechazo a
`FaultException<AccountIssue>` y comunica los fallos de persistencia mediante
un fault genérico seguro.

Las vistas envían estas escrituras con `BackgroundWork` y actualizan la
presentación en el Dispatcher. Una respuesta perdida puede corresponder a un
cambio ya guardado, por lo que las escrituras no se reintentan automáticamente.

`AccountLanguageController` valida el idioma y usa `LanguagePreferenceStore`
para guardar la preferencia mediante Entity Framework. Un idioma no admitido
produce `AccountIssue.UnsupportedLanguage` y una cuenta no disponible produce
`AccountIssue.PlayerNotFound`. En Ajustes, la confirmación de una sesión vigente
actualiza `PlayerSession` y aplica el idioma con `LanguageManager`. Los invitados
guardan su preferencia en el dispositivo.

La referencia se regeneró con SvcUtil 4.8 desde el endpoint MEX de un host
temporal de `DominoService` sobre `net.pipe`. La integración se verificó con
SQL Server LocalDB aislado y cuentas sintéticas, incluyendo la persistencia
del hash, el borrado lógico, la revocación y los faults ante un fallo de
guardado. Se verificaron también los diálogos en los tres idiomas y el regreso
a la página de inicio al confirmar la eliminación.

---

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
