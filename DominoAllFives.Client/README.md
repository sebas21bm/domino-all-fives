# Cliente remoto

Abrir `DominoAllFives.RemoteClient.sln` en Visual Studio. La solución contiene el cliente WPF sobre .NET Framework 4.8 y su proyecto de pruebas. La solución anterior se conserva en `../DominoAllFives.Client`.

Login y Ranking usan WCF. Las demás pantallas, estilos, imágenes y recursos de idioma se conservan como presentación para su migración posterior. Sus acciones de casos de uso están deshabilitadas; no acceden a la base de datos ni simulan operaciones exitosas. Cerrar sesión limpia el estado del cliente.

El cliente no referencia los proyectos Contracts, BusinessLogic ni DataAccess y no incluye Entity Framework ni configuración de SQL Server. Los nombres `DominoAllFives.Contracts.*` presentes en atributos del proxy son identificadores XML del protocolo, no referencias a esos ensamblados.

## Comunicación

La referencia `Connected Services/DominoAllFivesService` se generó desde el servidor en ejecución con la misma herramienta WCF Connected Service 8.0.0 y la misma dirección de descubrimiento que el cliente anterior:

- Descubrimiento: `http://localhost:8734/Domino/`
- Login: `net.tcp://localhost:8733/Domino/account`, contrato `ISessionAccount`.
- Ranking: `net.tcp://localhost:8733/Domino/ranking`, contrato `IRankingRemoteService`.

Para actualizarla, iniciar el servidor y actualizar la referencia de servicio en Visual Studio desde esa dirección, conservando el namespace `DominoAllFivesService`, métodos sincrónicos y reutilización de tipos deshabilitada. `ConnectedService.json` conserva esas opciones. `Reference.cs` es generado; no editarlo manualmente.

`Services/Wcf/Proxies/SessionAccountClient.cs` y `Services/Wcf/Proxies/RankingRemoteServiceClient.cs` conservan los constructores que toman los endpoints de `App.config`; `WcfInvoker.cs` conserva el cierre/aborto de cada canal. Las implementaciones están en `Services/Wcf/Services` y las interfaces en `Services/Wcf/Interfaces`.

Ranking expone dos operaciones desde el controller hasta el cliente:

```csharp
List<RankingEntry> GetTopWinsRanking();
int GetWinsRankingPositionById(int playerId);
```

La primera consulta devuelve únicamente la lista del top. La segunda obtiene únicamente la posición, con cero para un jugador ausente o sin victorias. El ViewModel compone ambos datos en `Models/Ranking` y publica la pantalla después de completar ambas llamadas. El orden y los desempates se conservan.

Login devuelve directamente `PlayerAccount` mediante `LoginWithAccount`. El servidor utiliza `AccountRuleException` con `AccountIssue` (`InvalidCredentials`, `Banned`, `Suspended`) y traduce los rechazos a `FaultException<AccountIssue>`. Los problemas técnicos de persistencia producen un fault general; el cliente distingue esos faults de los errores de conexión y utiliza los recursos de idioma existentes. El recorrido para la demostración está explicado en `../Documentation/LoginExceptionHandling.md`.

`LoginStatus` y `LoginResult` se conservan únicamente donde los necesita el código anterior. Las demás familias de enums todavía no se migran a excepciones.

El servidor existente también usa ahora `ISessionAccount`. El cliente anterior conserva su contrato anterior y requerirá actualizar su referencia si se quiere utilizar contra el servidor renombrado.

## Verificación realizada

- Compilación Debug del cliente remoto y del servidor correcta.
- 28 pruebas del servidor aprobadas: validaciones, restricciones de acceso, éxito y traducción de faults, además de las comprobaciones previas de Ranking.
- 21 pruebas del nuevo cliente aprobadas: 15 de Login y 6 de Ranking.
- Proxy regenerado consultando el descubrimiento del servidor local.
- Prueba WCF real: credenciales vacías y cuenta inexistente devuelven FaultException<AccountIssue> con InvalidCredentials; Ranking devuelve una lista y la posición se consulta por separado, con cero para un ID ausente.
- Inicio de sesión exitoso comprobado con datos en memoria; pendiente contra la base real porque no se proporcionaron credenciales de demostración.
- Compilación del cliente y pruebas con cero advertencias y cero errores utilizando paquetes ya restaurados.

Las pruebas remotas usaron el proyecto existente `../DominoAllFives.Client/DominoAllFives.WcfSmoke`, que enlaza el proxy de este cliente. No se modificaron datos reales ni se crearon cuentas.

Las pruebas del nuevo cliente se ejecutaron con VSTest de Visual Studio y NUnit3TestAdapter sobre el ensamblado compilado `DominoAllFives.RemoteClient.Tests.dll`. La compilación se realizó con `dotnet build --no-restore`; VSTest completó las 21 pruebas correctamente.
