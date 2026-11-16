# Pruebas

## Bloque 1: IRankingService (2026-11-16)

### Alcance y decisiones

Flujo revisado: IRankingService → DominoService → RankingController →
IPlayerStatsRepository / PlayerStatsRepository → Entity Framework. La construcción
de entradas consulta ProfilePictureStorage y produce RankingEntry / ProfilePicture;
Ranking permite conservar un snapshot ordenado. Se conserva el contexto EF por
operación y su liberación mediante using. No se agrega estado compartido ni locks.

Se mantienen las firmas WCF, los metadatos de serialización, los diez primeros
jugadores, la exclusión de invitados y jugadores sin victorias en el top, y el
orden por victorias y fecha de última victoria. No se redefine la política de
empates ni la elegibilidad de la consulta individual.

Los errores SqlException, EntityException y DbUpdateException siguen propagándose
hasta DominoService: allí se registran una vez con contexto de la operación y se
traducen al fault seguro existente. La ausencia o fallo recuperable de una imagen
sigue resolviéndose en ProfilePictureStorage con contenido vacío y su diagnóstico.
No se crean fábricas, interfaces ni delegados para sustituir operaciones en pruebas.

Cambios: lectura más clara al construir las entradas, validación de contexto nulo
en PlayerStatsRepository, formato de consultas y mensajes de diagnóstico por
operación. Ranking sigue en DominoService.cs. La regla de organización se agregó
a Manejo de comunicación remota.md.

### Organización de pruebas

- Tests/Tests.csproj: pruebas unitarias de validación y snapshots sin SQL, red ni archivos.
- AdditionalTest/AdditionalTest.csproj: comprobación auxiliar de la frontera del servicio.
  Invoca DominoService directamente; no verifica transporte WCF ni abre SQL con IDs inválidos.
- DominoAllFives.Server.Tests conserva temporalmente las pruebas de los contratos
  pendientes. Su migración se hará con el bloque que corresponda.

Cada prueba tiene AAA y una sola aserción. Se mantienen NUnit 3.14 y su adaptador
existentes, sin añadir paquetes.

### Casos implementados

| Caso a probar | Precondiciones | Postcondiciones | Resultado esperado | Resultado obtenido |
| --- | --- | --- | --- | --- |
| Normal: FromRankedPlayer con valores válidos | Imagen vacía válida | Entrada creada | Conserva posición, nombre, victorias e imagen | Aprobado |
| Alternativo: ranking vacío | Lista vacía y posición cero | Snapshot vacío | Conserva posición cero | Aprobado |
| Alternativo: cambia la lista de origen | Snapshot creado con una entrada | Lista de origen vacía | El snapshot conserva la entrada | Aprobado |
| Alternativo/límite: identificador cero, -1 e int.MinValue | Controller con storage válido | No se abre SQL | ArgumentOutOfRangeException | Aprobado |
| Excepción: storage nulo | Dependencia nula | No se construye el controller | ArgumentNullException | Aprobado |
| Excepción: imagen nula | Resto de datos válidos | No se crea entrada | ArgumentNullException | Aprobado |
| Excepción: lista nula | Lista nula | No se crea snapshot | ArgumentNullException | Aprobado |
| Frontera: identificador cero o -1 | DominoService | No se abre SQL | FaultException con Invalid request. | Aprobado |

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

### Verificación

- Compilación: MSBuild de Visual Studio 18, solución DominoAllFives.Server.sln,
  configuración Debug, target Build. Resultado: código de salida 0.
- Advertencias MSB3277 y MSB3836 por resolución de versiones y redirecciones de
  ensamblados en este entorno. No se modificaron versiones ni dependencias de
  producción para corregirlas dentro de este bloque.
- VSTest 18.10 con NUnit3TestAdapter 4.5: 11/11 aprobadas (9 unitarias y
  2 auxiliares), cero fallidas. Reporte local ignorado por Git:
  DominoAllFives.Server/TestResults/ranking.trx.
- git diff --check: sin errores.
- La validación de contexto nulo del repositorio fue revisada y compilada; no se
  añadió una prueba de SQL para justificar esta validación de constructor.
- Las pruebas auxiliares invocan el servicio directamente, sin verificar un host
  o transporte WCF real. Los casos con SQL y archivos permanecen no ejecutados.
- Los DataMember existentes de ranking conservan Order e IsRequired por
  compatibilidad. La guía general recomienda evitarlos cuando se valida en
  dominio; su eventual retirada requiere revisar el contrato y consumidores.
  No se declara conformidad total del contrato con esa recomendación.

## Continuidad de la refactorización

Se inventariaron los proyectos, contratos y clases de Server para delimitar los
flujos. Esta entrega revisa en detalle únicamente ranking; no constituye una
certificación de toda la solución. Los siguientes bloques pendientes son
ISessionAccountService, ILobbyService, IMatchService e IChatService, con sus
colaboradores y pruebas. Cada contrato se cierra con validación y pausa para commit.
Los componentes compartidos se revisarán en el contexto de cada flujo, sin cambios
ajenos al bloque. El archivo preexistente no rastreado Manejo de delegados.md no se
modificó y no pertenece a este bloque.
