# Manejo de logging

Domino All Fives utiliza logging para conservar información útil sobre la ejecución del sistema, facilitar el diagnóstico de errores y mantener trazabilidad de operaciones relevantes.

El logging no sustituye el manejo de excepciones ni los mensajes mostrados al usuario.

Cada mecanismo tiene una responsabilidad diferente:

```text
Logger
    │
    └── diagnóstico técnico y trazabilidad

Exceptions / Faults
    │
    └── señalización y manejo de errores

.resx
    │
    └── mensajes mostrados al usuario
```

La regla general es:

> [!IMPORTANT]
> Se debe registrar únicamente información que aporte valor para comprender qué ocurrió en el sistema. Los logs no deben utilizarse como sustituto de validaciones, manejo de excepciones ni mensajes de interfaz.

# Framework de logging

El proyecto utiliza la abstracción:

```csharp
Microsoft.Extensions.Logging
```

mediante:

```csharp
ILogger<T>
```

Por ejemplo:

```csharp
private readonly ILogger<LoginViewModel> _logger;
```

El uso de `ILogger<T>` permite que las clases dependan de una abstracción de logging y no directamente del mecanismo que finalmente escribe el registro.

Conceptualmente:

```text
Clase
  │
  ▼
ILogger<T>
  │
  ▼
Proveedor de logging
  │
  ▼
Destino del log
```

El tipo `T` identifica el componente que produjo el mensaje.

Por ejemplo:

```csharp
ILogger<LoginViewModel>
ILogger<LobbyService>
ILogger<LobbyEventPublisher>
```

Esto permite identificar el origen de cada entrada sin escribir manualmente el nombre de la clase en todos los mensajes.

# Configuración general

Cliente y servidor utilizan la misma estrategia base de logging:

```text
ILogger<T>
    │
    ▼
LoggerFactoryProvider
    │
    ▼
Serilog
```

Ambos comparten:

```text
MinimumLevel.Debug
outputTemplate
structured logging
rotación diaria
rotación por tamaño
límite de 10 MB por archivo
retención de 14 archivos
convenciones de mensajes
protección de información sensible
```

Los destinos de salida y la ubicación de los archivos se adaptan al entorno de cada aplicación.

# Logging en el cliente

El cliente WPF centraliza la creación de loggers mediante:

```text
Services/LoggerFactoryProvider.cs
```

La clase mantiene una instancia compartida de:

```csharp
ILoggerFactory
```

y permite crear loggers tipados mediante:

```csharp
LoggerFactoryProvider.CreateLogger<T>();
```

Por ejemplo:

```csharp
private readonly ILogger<LobbyViewModel> _logger =
    LoggerFactoryProvider.CreateLogger<LobbyViewModel>();
```

## Serilog en el cliente

El cliente utiliza Serilog como proveedor de logging.

La configuración recomendada es:

```csharp
Serilog.Log.Logger =
    new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.File(
            path: logFilePath,
            rollingInterval: RollingInterval.Day,
            rollOnFileSizeLimit: true,
            fileSizeLimitBytes: 10 * 1024 * 1024,
            retainedFileCountLimit: 14,
            outputTemplate:
                "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} "
                + "[{Level:u3}] "
                + "[{SourceContext}] "
                + "{Message:lj}"
                + "{NewLine}"
                + "{Exception}")
        .CreateLogger();
```

Además del sink de archivo, el cliente utiliza:

```csharp
builder.AddDebug();
```

para disponer de la salida de diagnóstico durante el desarrollo.

Conceptualmente:

```text
Cliente
  │
  ▼
ILogger<T>
  │
  ▼
LoggerFactoryProvider
  │
  ├── Serilog
  │     └── File
  │
  └── Debug provider
        └── Visual Studio Output
```

## Ubicación de los logs del cliente

Los logs se almacenan en:

```text
LocalApplicationData
└── DominoAllFives
    └── Logs
        └── client_log-<fecha>.txt
```

La ruta se obtiene mediante:

```csharp
Environment.GetFolderPath(
    Environment.SpecialFolder.LocalApplicationData)
```

Esta ubicación es adecuada para el cliente porque corresponde a datos generados por una aplicación de usuario y normalmente proporciona permisos de escritura sin depender de la ubicación del ejecutable.

Conceptualmente:

```text
C:\Users\<usuario>\
└── AppData
    └── Local
        └── DominoAllFives
            └── Logs
```

Los logs del cliente no deben almacenarse junto al `.exe`, ya que la aplicación podría ejecutarse desde una ubicación donde el usuario no tenga permisos de escritura.

# Logging en el servidor

El servidor debe utilizar la misma abstracción y estrategia general que el cliente:

```text
ILogger<T>
    │
    ▼
LoggerFactoryProvider
    │
    ▼
Serilog
```

El `ConsoleLogger<T>` implementado manualmente deja de ser necesario una vez realizada esta refactorización.

El servidor debe utilizar Serilog con dos sinks:

```text
Server
  │
  ▼
Serilog
  │
  ├── File
  └── Console
```

Para ello se requieren las dependencias:

```text
Microsoft.Extensions.Logging
Serilog.Extensions.Logging
Serilog.Sinks.File
Serilog.Sinks.Console
```

## Configuración de Serilog en el servidor

La configuración recomendada es:

```csharp
string outputTemplate =
    "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} "
    + "[{Level:u3}] "
    + "[{SourceContext}] "
    + "{Message:lj}"
    + "{NewLine}"
    + "{Exception}";

Serilog.Log.Logger =
    new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.Console(
            outputTemplate: outputTemplate)
        .WriteTo.File(
            path: logFilePath,
            rollingInterval: RollingInterval.Day,
            rollOnFileSizeLimit: true,
            fileSizeLimitBytes: 10 * 1024 * 1024,
            retainedFileCountLimit: 14,
            outputTemplate: outputTemplate)
        .CreateLogger();
```

La creación de loggers debe centralizarse mediante una clase equivalente a:

```text
Logging/
└── LoggerFactoryProvider.cs
```

que permita:

```csharp
LoggerFactoryProvider.CreateLogger<T>();
```

Por ejemplo:

```csharp
ILogger<DominoService> logger =
    LoggerFactoryProvider.CreateLogger<DominoService>();
```

en lugar de:

```csharp
new ConsoleLogger<DominoService>();
```

Lo mismo aplica para otros componentes:

```csharp
LoggerFactoryProvider.CreateLogger<LobbyEventPublisher>();
```

## Ubicación de los logs del servidor

Los logs del servidor se almacenan en una carpeta asociada al propio proceso servidor:

```text
DominoAllFives.Server
├── DominoAllFives.Server.exe
└── Logs
    └── server_log-<fecha>.txt
```

La ruta base puede obtenerse mediante:

```csharp
AppDomain.CurrentDomain.BaseDirectory
```

y combinarse con:

```csharp
Path.Combine(
    AppDomain.CurrentDomain.BaseDirectory,
    "Logs");
```

Esta ubicación es apropiada porque el servidor se ejecuta en un entorno controlado y sus logs forman parte de la operación del proceso servidor.

# Diferencia entre cliente y servidor

Cliente y servidor utilizan la misma política general, pero sus destinos se adaptan al tipo de aplicación.

```text
Cliente
├── ILogger<T>
├── LoggerFactoryProvider
├── Serilog
├── File
├── Debug
└── LocalApplicationData\DominoAllFives\Logs
```

```text
Servidor
├── ILogger<T>
├── LoggerFactoryProvider
├── Serilog
├── File
├── Console
└── <ServerBaseDirectory>\Logs
```

La diferencia de ubicación no representa una inconsistencia.

La consistencia se mantiene en:

```text
framework
niveles
formato
rotación
retención
structured logging
convenciones
manejo de excepciones
protección de datos
```

# Configuración de archivos de log

## Nivel mínimo

Durante desarrollo y pruebas, cliente y servidor utilizan:

```csharp
.MinimumLevel.Debug()
```

Esto permite registrar:

```text
Debug
Information
Warning
Error
Fatal
```

El nivel `Debug` facilita diagnosticar problemas relacionados con:

```text
WCF
callbacks
concurrencia
persistencia
sesiones
manejo de archivos
flujo de operaciones
```

> [!NOTE]
> En un despliegue productivo podría elevarse el nivel mínimo a `Information` para reducir el volumen de registros. Durante el desarrollo del proyecto se mantiene `Debug` para maximizar la información disponible para diagnóstico.

## Rotación diaria

Los archivos utilizan:

```csharp
rollingInterval: RollingInterval.Day
```

Esto significa que Serilog crea un archivo diferente para cada día.

Por ejemplo:

```text
client_log-20261114.txt
client_log-20261115.txt
client_log-20261116.txt
```

o:

```text
server_log-20261114.txt
server_log-20261115.txt
server_log-20261116.txt
```

Al cambiar de día, Serilog comienza a escribir en un nuevo archivo.

El archivo anterior no se sobrescribe.

## Rotación por tamaño

Además de la rotación diaria se utiliza:

```csharp
rollOnFileSizeLimit: true
```

con:

```csharp
fileSizeLimitBytes: 10 * 1024 * 1024
```

Por lo tanto, cada archivo tiene un límite aproximado de:

```text
10 MB
```

Si el archivo alcanza ese tamaño antes de terminar el día, Serilog crea otro archivo y continúa escribiendo.

Conceptualmente:

```text
client_log-20261114.txt
        │
        │ alcanza 10 MB
        ▼
client_log-20261114_001.txt
        │
        │ alcanza 10 MB
        ▼
client_log-20261114_002.txt
```

Esto evita generar archivos excesivamente grandes.

## Retención

Se utiliza:

```csharp
retainedFileCountLimit: 14
```

Esto permite conservar hasta 14 archivos de log.

Cuando se supera el límite, Serilog elimina automáticamente los archivos más antiguos.

La combinación:

```text
rotación diaria
+
rotación por tamaño
+
retención de 14 archivos
```

evita un crecimiento indefinido del almacenamiento utilizado por los logs.

# Formato de salida

Cliente y servidor utilizan el mismo `outputTemplate`:

```csharp
"{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} "
+ "[{Level:u3}] "
+ "[{SourceContext}] "
+ "{Message:lj}"
+ "{NewLine}"
+ "{Exception}"
```

El formato contiene:

```text
Timestamp
Level
SourceContext
Message
Exception
```

Por ejemplo:

```text
2026-11-14 19:52:41.327 -06:00 [INF] [DominoAllFives.Client.WPF.ViewModels.LoginViewModel] Player 42 logged in successfully.
```

Con excepción:

```text
2026-11-14 19:53:12.804 -06:00 [ERR] [DominoAllFives.Client.WPF.ViewModels.LoginViewModel] Authentication service could not complete login.
System.ServiceModel.FaultException: The service could not complete the operation.
   at ...
   at ...
```

En el servidor:

```text
2026-11-14 19:53:12.601 -06:00 [ERR] [DominoAllFives.Server.DominoService] Login failed on server.
System.Data.SqlClient.SqlException: ...
   at ...
```

Esto permite relacionar temporalmente eventos entre cliente y servidor.

## `Timestamp`

```text
{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}
```

produce valores como:

```text
2026-11-14 19:52:41.327 -06:00
```

Incluye:

```text
fecha
hora
milisegundos
zona horaria
```

## `Level`

```text
{Level:u3}
```

produce abreviaciones como:

```text
DBG
INF
WRN
ERR
FTL
```

## `SourceContext`

```text
{SourceContext}
```

identifica automáticamente la clase asociada al `ILogger<T>`.

Por ejemplo:

```text
DominoAllFives.Client.WPF.ViewModels.LoginViewModel
```

o:

```text
DominoAllFives.Server.LobbyService
```

## `Message`

```text
{Message:lj}
```

representa el mensaje generado después de sustituir los placeholders.

Por ejemplo:

```csharp
_logger.LogInformation(
    "Player {PlayerId} joined lobby {LobbyId}.",
    playerId,
    lobbyId);
```

produce:

```text
Player 42 joined lobby 15.
```

## `Exception`

```text
{NewLine}
{Exception}
```

permite escribir debajo del mensaje la información completa de una excepción cuando existe.

No es necesario incluir manualmente:

```text
fecha
hora
nivel
nombre de clase
tipo de excepción
stack trace
```

dentro del mensaje porque el sistema de logging ya proporciona estos datos.

# Fallback del logger

El sistema de logging no debe impedir el funcionamiento de la aplicación si uno de sus destinos no puede inicializarse.

## Cliente

Si el cliente no puede crear el directorio o archivo debido a:

```text
IOException
UnauthorizedAccessException
```

se utiliza un logger alternativo mediante:

```csharp
AddDebug()
```

y se registra:

```csharp
"File logging is unavailable; using debug logging."
```

Esto permite continuar utilizando la salida de diagnóstico de Visual Studio.

## Servidor

Si el servidor no puede inicializar el sink de archivo, debe conservar la salida disponible mediante consola.

Conceptualmente:

```text
File sink falla
      │
      ▼
Console permanece disponible
      │
      ▼
servidor continúa
```

> [!CAUTION]
> El sistema no debe dejar de funcionar únicamente porque el archivo de logs no pudo ser creado.

# Logger tipado

Cada clase debe utilizar un logger asociado a su propio tipo.

Ejemplo:

```csharp
private readonly ILogger<LobbyEventPublisher> _logger;
```

y no:

```csharp
ILogger<object>
```

ni un logger perteneciente a una clase distinta.

Esto permite que `SourceContext` identifique automáticamente dónde ocurrió el evento:

```text
LobbyEventPublisher
LoginViewModel
LobbyService
ProfilePictureStorage
```

# Niveles de logging

El nivel debe representar la gravedad y el significado del evento, no simplemente el hecho de que se esté escribiendo un mensaje.

## `Debug`

Se utiliza para información técnica detallada útil principalmente durante desarrollo o diagnóstico.

Ejemplos posibles:

```text
Initializing lobby callback connection.
Loading cached profile picture.
```

No debe utilizarse para información necesaria para entender fallos importantes, porque esos eventos deben utilizar el nivel correspondiente.

Cliente y servidor admiten este nivel mediante:

```csharp
.MinimumLevel.Debug()
```

Durante desarrollo, este nivel proporciona información adicional para analizar el comportamiento interno del sistema.

## `Information`

Se utiliza para acontecimientos normales y relevantes del sistema.

Ejemplos:

```csharp
_logger.LogInformation(
    "Domino All Fives server is running.");
```

o:

```csharp
_logger.LogInformation(
    "Player {PlayerId} logged in successfully.",
    playerId);
```

Debe responder a la pregunta:

> ¿Este evento representa una transición o acontecimiento normal que podría ser útil para reconstruir el flujo del sistema?

Ejemplos apropiados:

```text
inicio del servidor
login exitoso
creación de una sesión relevante
inicio de una partida
finalización de una operación importante
```

No se debe registrar cada método, clic, cambio de propiedad o interacción menor como `Information`.

## `Warning`

Se utiliza cuando ocurre una situación anómala o un rechazo esperado, pero el sistema puede continuar funcionando.

Ejemplo de regla de negocio:

```csharp
_logger.LogWarning(
    ex,
    "Login rejected. Account issue: {AccountIssue}.",
    ex.Detail);
```

Ejemplo de recuperación:

```csharp
_logger.LogWarning(
    ex,
    "Lobby connection could not close normally. "
        + "The channel will be aborted.");
```

También se utiliza cuando una funcionalidad puede continuar de forma degradada:

```csharp
_logger.LogWarning(
    ex,
    "File logging is unavailable; using debug logging.");
```

Conceptualmente:

```text
Warning
   │
   ├── ocurrió algo fuera del flujo ideal
   ├── existe una respuesta conocida
   └── la aplicación puede continuar
```

## `Error`

Se utiliza cuando una operación no pudo completarse debido a un problema técnico o inesperado.

Ejemplo:

```csharp
logger.LogError(
    ex,
    "Lobby session could not resolve player.");
```

También:

```csharp
_logger.LogError(
    ex,
    "Authentication service could not complete login.");
```

Conceptualmente:

```text
Error
   │
   ├── una operación relevante falló
   ├── el resultado esperado no pudo producirse
   └── se requiere información para diagnóstico
```

Cuando existe una excepción, debe entregarse al logger:

```csharp
_logger.LogError(
    ex,
    "Ranking retrieval failed.");
```

y no escribirse manualmente:

```csharp
_logger.LogError(
    "Ranking retrieval failed: "
        + ex.Message);
```

El logger ya puede conservar:

```text
tipo de excepción
Message
StackTrace
InnerException
```

## `Critical`

Se reserva para fallos que impiden que una parte esencial de la aplicación continúe funcionando.

Por ejemplo, podría utilizarse si el servidor no puede iniciar ninguno de sus servicios esenciales y debe terminar completamente.

No debe utilizarse como sustituto de `Error`.

Actualmente no constituye un nivel de uso habitual en el proyecto.

## Regla para elegir el nivel

La decisión puede representarse así:

```text
¿Es ejecución normal relevante?
        │
       Sí
        ▼
 Information

¿Es una anomalía o rechazo
del que el sistema puede recuperarse?
        │
       Sí
        ▼
 Warning

¿La operación no pudo completarse
debido a un fallo?
        │
       Sí
        ▼
 Error

¿El sistema no puede continuar?
        │
       Sí
        ▼
 Critical
```

`Debug` queda reservado para información diagnóstica adicional que no sea necesaria en el flujo normal.

# Estructura de los mensajes

Los mensajes escritos en código deben estar en inglés y describir el evento de forma breve y específica.

La estructura general recomendada es:

```text
<Subject> <action/outcome>. <Relevant context if necessary>.
```

Por ejemplo:

```text
Lobby session could not resolve player.
```

```text
Lobby callback failed for player {PlayerId}.
```

```text
Login rejected. Account issue: {AccountIssue}.
```

```text
Ranking retrieval failed.
```

El mensaje debe permitir comprender qué operación estaba ocurriendo sin tener que interpretar únicamente el tipo de excepción.

# Mensaje orientado al evento

El log debe describir **qué ocurrió**, no el nombre del método que se estaba ejecutando.

Preferir:

```csharp
_logger.LogError(
    ex,
    "Lobby could not be created.");
```

en lugar de:

```csharp
_logger.LogError(
    ex,
    "Error in CreateLobby().");
```

También preferir:

```csharp
_logger.LogWarning(
    ex,
    "Lobby callback failed for player {PlayerId}.",
    playerId);
```

en lugar de:

```csharp
_logger.LogWarning(
    ex,
    "CommunicationException in Publish().");
```

La excepción ya indica su tipo. El mensaje debe agregar **contexto de negocio o de operación**.

# Structured logging

Los valores variables no deben concatenarse directamente dentro del mensaje.

No utilizar:

```csharp
_logger.LogInformation(
    "Player " + playerId + " logged in.");
```

ni:

```csharp
_logger.LogInformation(
    $"Player {playerId} logged in.");
```

Se debe utilizar structured logging:

```csharp
_logger.LogInformation(
    "Player {PlayerId} logged in successfully.",
    playerId);
```

El template permanece constante:

```text
Player {PlayerId} logged in successfully.
```

y el valor:

```text
PlayerId = 25
```

se conserva como dato asociado al evento.

Esto facilita búsqueda, filtrado y análisis de los registros.

## Nombres de placeholders

Los placeholders deben utilizar, siempre que sea posible, el mismo nombre del atributo, propiedad o dato que representan.

Por ejemplo:

```text
PlayerId
    └── {PlayerId}

LobbyId
    └── {LobbyId}

MatchId
    └── {MatchId}

AccountIssue
    └── {AccountIssue}

ProfilePictureFileName
    └── {ProfilePictureFileName}
```

Ejemplo:

```csharp
_logger.LogWarning(
    "Lobby callback failed for player {PlayerId}.",
    player.PlayerId);
```

Aquí:

```text
player.PlayerId
        │
        ▼
   {PlayerId}
```

De esta forma, el nombre utilizado en el log corresponde directamente con el dato utilizado en el código.

Cuando se registra directamente una propiedad, debe mantenerse su nombre:

```csharp
_logger.LogInformation(
    "Player {PlayerId} joined lobby {LobbyId}.",
    player.PlayerId,
    lobby.LobbyId);
```

No utilizar:

```csharp
_logger.LogInformation(
    "Player {Id} joined lobby {Room}.",
    player.PlayerId,
    lobby.LobbyId);
```

porque:

```text
{Id}
{Room}
```

no corresponden con los nombres reales de los datos.

También deben evitarse placeholders genéricos como:

```text
{Id}
{Value}
{Data}
{Object}
{Item}
```

cuando existe un nombre específico para el dato.

Por ejemplo:

```text
{PlayerId}
```

es preferible a:

```text
{Id}
```

y:

```text
{LobbyIssue}
```

es preferible a:

```text
{Error}
```

Cuando el valor no corresponde directamente a una propiedad, debe utilizarse un nombre específico que describa correctamente el dato.

Por ejemplo:

```csharp
int participantCount =
    lobby.Participants.Count;

_logger.LogInformation(
    "Lobby has {ParticipantCount} participants.",
    participantCount);
```

> [!IMPORTANT]
> El placeholder debe conservar el mismo nombre del atributo, propiedad o concepto que representa siempre que sea posible. Esto mantiene consistencia entre código y logs y facilita la búsqueda de eventos relacionados.

# No interpolar excepciones manualmente

Cuando existe una excepción:

```csharp
catch (SqlException ex)
```

debe proporcionarse como argumento del logger:

```csharp
_logger.LogError(
    ex,
    "Login failed on server.");
```

No utilizar:

```csharp
_logger.LogError(
    "Login failed: {Exception}",
    ex);
```

ni:

```csharp
_logger.LogError(
    "Login failed: " + ex);
```

El overload que recibe `Exception` permite que el proveedor registre correctamente su información completa.

# Logging y manejo de excepciones

El logging debe respetar la política definida en el documento de **Manejo de excepciones**.

Una excepción no debe registrarse automáticamente en cada capa por la que pasa.

Ejemplo incorrecto:

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

Esto genera múltiples entradas para el mismo problema.

La regla es:

> [!IMPORTANT]
> Una excepción debe registrarse donde exista suficiente contexto para diagnosticarla o donde sea realmente manejada. No debe registrarse nuevamente en cada capa únicamente porque atraviesa dicha capa.

Si una capa únicamente permite que la excepción continúe:

```csharp
throw;
```

no necesita necesariamente generar otro log.

## Excepción manejada y recuperada

Cuando una excepción es capturada y el sistema aplica una recuperación conocida, el log debe describir tanto el problema como el contexto de recuperación cuando sea relevante.

Por ejemplo:

```csharp
catch (CommunicationException ex)
{
    _logger.LogWarning(
        ex,
        "Lobby connection could not close normally. "
            + "The channel will be aborted.");

    _client.Abort();
}
```

Aquí la excepción no se propaga porque fue manejada.

El log conserva evidencia de que el cierre normal falló.

# Rechazos esperados de negocio

Los rechazos de negocio no deben registrarse automáticamente como `Error`.

Por ejemplo:

```text
credenciales inválidas
cuenta suspendida
lobby lleno
jugador ya se encuentra en un lobby
```

son resultados conocidos del dominio.

Cuando resulte útil conservar trazabilidad se utiliza normalmente:

```csharp
LogWarning
```

Ejemplo:

```csharp
_logger.LogWarning(
    ex,
    "Login rejected. Account issue: {AccountIssue}.",
    ex.Detail);
```

El hecho de utilizar una excepción o `FaultException<TIssue>` para representar el rechazo no significa automáticamente que el evento sea un error técnico del sistema.

# Logging del servidor y del cliente

Un mismo problema remoto puede generar información en ambos extremos, pero cada log debe aportar contexto diferente.

Por ejemplo:

```text
SERVIDOR
SqlException
    │
    ▼
LogError
"Lobby session could not resolve player."
    │
    ▼
FaultException seguro
    │
    │ WCF
    ▼
CLIENTE
FaultException
    │
    ▼
LogError
"Lobby operation failed because
the server returned a fault."
```

Esto no se considera necesariamente una duplicación inútil porque representan dos perspectivas distintas:

```text
servidor
    └── causa técnica original

cliente
    └── efecto observable de la operación remota
```

Sin embargo, el cliente no debe intentar reconstruir ni duplicar el detalle técnico interno del servidor.

# Logs y mensajes de interfaz

Los mensajes del logger y los mensajes mostrados al usuario tienen propósitos diferentes.

```text
Logger
    │
    └── desarrollador / diagnóstico

.resx
    │
    └── usuario
```

Por ello:

```csharp
_logger.LogError(
    ex,
    "Authentication service could not complete login.");
```

puede coexistir con:

```csharp
ShowMessageFailure();
```

que finalmente utiliza:

```text
Global_msgDefaultErrorTitle
Global_msgDefaultError
```

No se debe utilizar:

```csharp
_dialogService.ShowDialog(ex.Message);
```

ni reutilizar el texto del log como mensaje de interfaz.

# Información sensible

Los logs pueden permanecer almacenados después de que una operación haya terminado.

Por ello no deben contener secretos ni información cuya exposición pueda comprometer una cuenta o sesión.

> [!WARNING]
> Nunca se deben registrar contraseñas, `sessionKey`, códigos de autenticación o verificación, tokens, credenciales completas ni datos binarios de las fotografías.

También debe evitarse registrar innecesariamente información personal.

Cuando exista un identificador interno suficiente, se debe preferir:

```text
{PlayerId}
```

sobre datos como:

```text
correo electrónico
nombre completo
```

Por ejemplo, después de una autenticación exitosa es preferible:

```csharp
_logger.LogInformation(
    "Player {PlayerId} logged in successfully.",
    authenticatedAccount.PlayerId);
```

en lugar de conservar innecesariamente el correo utilizado para iniciar sesión.

## Datos de chat

El contenido de los mensajes de chat no debe registrarse como parte del logging normal.

Se puede registrar información contextual cuando resulte necesaria:

```text
PlayerId
LobbyId
MessageId
ChatIssue
```

pero no:

```text
contenido completo del mensaje
```

salvo que exista posteriormente un requisito explícito de auditoría o moderación que defina su tratamiento, almacenamiento y privacidad.

Logging y almacenamiento funcional del chat son responsabilidades distintas.

## Fotografías

No deben registrarse:

```text
byte[]
contenido codificado de la imagen
Base64
```

Cuando sea necesario diagnosticar almacenamiento de fotografías puede utilizarse información limitada, por ejemplo:

```csharp
_logger.LogWarning(
    ex,
    "Unable to read profile picture file "
        + "{ProfilePictureFileName}.",
    profilePictureFileName);
```

# No registrar objetos completos indiscriminadamente

No se deben enviar objetos de dominio completos al logger únicamente por comodidad.

Evitar patrones como:

```csharp
_logger.LogInformation(
    "Player data: {Player}",
    player);
```

si el objeto puede contener propiedades que no se necesitan para diagnóstico.

Se deben seleccionar explícitamente los identificadores relevantes:

```csharp
_logger.LogInformation(
    "Player {PlayerId} joined lobby {LobbyId}.",
    player.PlayerId,
    lobby.LobbyId);
```

Esto mantiene los logs pequeños, previsibles y reduce el riesgo de registrar información innecesaria.

# Qué operaciones registrar

No todos los métodos necesitan un log.

Se debe registrar un evento cuando aporta información útil para reconstruir el comportamiento del sistema, diagnosticar un fallo o conocer una transición significativa.

Ejemplos apropiados incluyen:

```text
inicio o cierre de infraestructura importante
autenticación exitosa o rechazada
creación o cierre de contexto multijugador
inicio o fin de partida
fallos de persistencia
fallos de comunicación
callbacks eliminados por pérdida de conexión
fallbacks de infraestructura
operaciones importantes que no pudieron completarse
```

No es necesario registrar:

```text
cada getter
cada setter
cada binding
cada clic
cada navegación trivial
cada ejecución de un método auxiliar
```

El objetivo no es producir la mayor cantidad posible de logs.

El objetivo es producir logs que sean útiles.

## Inicio y resultado de operaciones

No es obligatorio registrar tanto el inicio como el final de todas las operaciones.

Por ejemplo:

```csharp
LogInformation("Attempting login...");
```

seguido de:

```csharp
LogInformation("Login succeeded...");
```

puede ser útil en determinadas operaciones, pero no debe convertirse en un patrón obligatorio para cada método.

Cuando el resultado final proporciona suficiente información, un único evento puede ser preferible.

Esto reduce ruido y facilita analizar los archivos.

# Redacción de mensajes

Los mensajes deben:

- escribirse en inglés;
- ser breves y específicos;
- terminar con punto;
- describir el evento o resultado;
- proporcionar contexto mediante placeholders;
- evitar repetir información proporcionada automáticamente por el logger;
- evitar datos sensibles;
- evitar textos destinados al usuario.

Ejemplo recomendado:

```csharp
_logger.LogWarning(
    ex,
    "Lobby callback failed for player {PlayerId}.",
    player.PlayerId);
```

El mensaje indica:

```text
componente
    → SourceContext automático

gravedad
    → LogLevel automático

qué ocurrió
    → callback failed

contexto
    → PlayerId

detalle técnico
    → Exception
```

# Ejemplos de mensajes

### Operación exitosa

```csharp
_logger.LogInformation(
    "Player {PlayerId} logged in successfully.",
    playerId);
```

### Rechazo esperado

```csharp
_logger.LogWarning(
    ex,
    "Login rejected. Account issue: {AccountIssue}.",
    ex.Detail);
```

### Error técnico

```csharp
_logger.LogError(
    ex,
    "Lobby session could not resolve player.");
```

### Fallo de callback recuperable

```csharp
_logger.LogWarning(
    ex,
    "Lobby callback failed for player {PlayerId}.",
    playerId);
```

### Fallback

```csharp
_logger.LogWarning(
    ex,
    "File logging is unavailable; using debug logging.");
```

# Regla para agregar un nuevo log

Antes de agregar un registro se debe responder:

```text
¿Qué ocurrió?
    │
    ▼
¿Es útil conocerlo posteriormente?
    │
    ├── No ───► no registrar
    │
    └── Sí
         │
         ▼
¿Es ejecución normal?
    │
    └── Information
         │
¿Es una anomalía recuperable
o rechazo esperado?
    │
    └── Warning
         │
¿La operación falló?
    │
    └── Error
         │
¿El sistema no puede continuar?
    │
    └── Critical
```

Después se debe comprobar:

```text
¿Existe una excepción?
    │
    └── pasarla al overload del logger

¿Existe contexto útil?
    │
    └── agregar placeholders estructurados

¿Contiene información sensible?
    │
    └── eliminarla o sustituirla
        por un identificador seguro
```

# Regla general de implementación

La estructura recomendada para un evento con excepción es:

```csharp
_logger.LogWarning(
    ex,
    "Feature operation failed for entity {EntityId}.",
    entityId);
```

Para un evento sin excepción:

```csharp
_logger.LogInformation(
    "Player {PlayerId} joined lobby {LobbyId}.",
    playerId,
    lobbyId);
```

No se deben construir mensajes mediante concatenación o interpolación cuando contienen valores variables.

# Resumen

La estrategia de logging de Domino All Fives se basa en:

```text
ILogger<T>
    │
    ├── Cliente
    │    └── LoggerFactoryProvider
    │         └── Serilog
    │              ├── File
    │              └── Debug
    │
    └── Servidor
         └── LoggerFactoryProvider
              └── Serilog
                   ├── File
                   └── Console
```

Ambos utilizan:

```text
MinimumLevel.Debug
RollingInterval.Day
rollOnFileSizeLimit = true
fileSizeLimitBytes = 10 MB
retainedFileCountLimit = 14
mismo outputTemplate
structured logging
misma convención de mensajes
```

La ubicación de los archivos se adapta a cada aplicación:

```text
Cliente
    └── LocalApplicationData
        └── DominoAllFives
            └── Logs
```

```text
Servidor
    └── AppDomain.CurrentDomain.BaseDirectory
        └── Logs
```

Los niveles se interpretan como:

```text
Debug
    └── diagnóstico detallado

Information
    └── ejecución normal relevante

Warning
    └── anomalía recuperable
        o rechazo esperado

Error
    └── operación fallida

Critical
    └── sistema o componente
        esencial no puede continuar
```

Los mensajes siguen:

```text
evento claro
+
structured logging
+
contexto necesario
+
Exception cuando exista
-
información sensible
```

La regla final es:

> Registrar la información mínima necesaria para comprender qué ocurrió, dónde ocurrió y sobre qué entidad ocurrió, utilizando el nivel apropiado y sin exponer datos sensibles ni convertir el log en un mensaje de interfaz.