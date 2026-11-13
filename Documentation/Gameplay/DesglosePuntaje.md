# Desglose de puntos por colocación

La vista Match muestra un panel encima del tablero con la explicación de la
última colocación aceptada. Se actualiza para todos los participantes junto con
el snapshot coherente del tablero y la mano. El mensaje incluye jugador, ficha,
ronda, sumandos, total, puntos base, efecto de color y puntos de esa colocación.

Por ejemplo, al colocar una roja con extremos 12, 5 y 3:

```text
Extremos: 12 + 5 + 3 = 20. Puntos base: 20.
Roja: duplica solo esta jugada. 20 × 2 = 40 puntos.
Puntos ganados por esta colocación: +40.
```

Cuando corresponde, se explica que el doble inicial completo se cuenta una
sola vez mientras tenga conexiones libres. Los extremos excluidos por un
bloqueo morado se identifican por su dirección. Al jugar una morada se distingue
ese efecto futuro: bloqueará el extremo durante el siguiente turno. Si la suma
no otorga puntos también se explica. El premio por terminar una ronda con la
última ficha se muestra separado de los puntos de colocación.

## Datos y responsabilidades

Board.GetScoringValues proporciona los mismos sumandos que utiliza
CalculatePoints. Match captura LastPlay antes de avanzar el turno o repartir
la siguiente ronda. Así, el mensaje conserva los bloqueos del momento del
cálculo; no se reconstruye a partir de los extremos del turno siguiente.

PlayScore del dominio conserva valores de solo lectura y la ficha inmutable.
MatchProjection crea copias independientes en el DTO PlayScore. MatchView añade
LastPlay como DataMember 11, sin alterar los miembros existentes ni las firmas
de las operaciones. Los datos proceden exclusivamente de fichas ya públicas.

Se regeneró el proxy desde los metadatos WCF actualizados. El cliente continúa
sin referencias al proyecto Contracts. PlayScoreFormatter compone textos con
recursos completos y marcadores de posición en es-MX, en-US y pt-BR; no replica
las reglas de puntuación. El panel conserva la última colocación al comer,
pasar o reconectar. No es un historial de todas las jugadas ni una predicción
de la ficha seleccionada. Al cambiar de ronda, el encabezado identifica la
ronda a la que pertenece la última colocación.

## Verificación

- Compilaciones Debug de cliente y servidor en sus carpetas habituales.
- Suite final: 59 pruebas del cliente y 84 del servidor; 143 aprobadas.
- Reproducción de la secuencia de las capturas: suma 20, roja ×2, premio 40.
- Exclusión morada conservada en LastPlay aunque el siguiente turno ya muestre
  ese extremo liberado.
- Copias del desglose que no permiten mutar el dominio; conservación al comer.
- Desglose recibido por proxy WCF y publicado por el ViewModel.
- Formato en los tres idiomas y separación visual de los premios.
- Análisis del estándar sobre 67 archivos C# relacionados, con las excepciones
  de código generado: sin hallazgos pendientes. Referencias Compile y recursos
  de idioma comprobados; render WPF inspeccionado a 1600 × 900.

Durante la primera compilación, los ejecutables abiertos bloquearon sus archivos.
Se verificó inicialmente con salidas separadas. Después de cerrar el usuario
cliente y servidor, las compilaciones y pruebas finales se hicieron en las
carpetas habituales. Las pruebas WCF utilizaron localhost y sesiones en memoria.

Para probarlo, reiniciar servidor y clientes con las versiones compiladas y
colocar una ficha. El panel cambia después de que el servidor acepte la jugada.
Ambos componentes deben actualizarse para recibir el nuevo desglose. La
generación del proxy ya está incluida; no hay que añadir Contracts al cliente.

Las reglas y los puntajes no se modificaron. Este cambio explica el cálculo
existente y no implica una nueva validación funcional de cuentas SQL reales.
