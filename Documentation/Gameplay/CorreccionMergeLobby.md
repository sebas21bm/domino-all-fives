# Corrección de integración del lobby tras el merge

Los CS0246 de ChatMessage se originaban en archivos existentes que no estaban
incluidos mediante Compile en el proyecto clásico Contracts. Se incorporaron
ChatMessage, ChatContext, ChatScope y ChatIssue, y la excepción ChatRuleException
en BusinessLogic. El archivo Domain/ChatScope.cs se renombró ChatContext.cs para
coincidir con el tipo que ya declaraba; no cambió su namespace ni serialización.

La versión combinada de LobbyManager había perdido la inicialización opcional
de StartLobby y ReleaseMatch, todavía utilizados por la jugabilidad. Se
recuperaron conservando el almacenamiento y limpieza de mensajes. La creación
de partida se realiza antes de cambiar el estado y eliminar mensajes: si falla,
se conservan el lobby y su conversación. Las llamadas sin inicializador
mantienen el estado Starting anterior.

No se añadieron referencias del cliente a Contracts ni se regeneró su proxy:
el fallo estaba en la compilación interna del servidor. Esta corrección incluye
las dependencias de chat que usa LobbyManager; no activa ni integra las demás
clases del servicio de chat que estén fuera de los proyectos.

Validación: ambas soluciones Debug compiladas; 87 pruebas del servidor y
59 del cliente aprobadas. Tres pruebas nuevas verifican la inicialización,
la preservación de mensajes ante un fallo y la liberación de jugadores.
Análisis del estándar sobre 73 archivos C# relacionados: sin hallazgos.
Las 132 entradas Compile inspeccionadas no tienen duplicados ni rutas ausentes.
Los resultados de esta ejecución están en Validation/Merge.

No se realizaron operaciones de commit, push ni modificaciones del merge de Git.
