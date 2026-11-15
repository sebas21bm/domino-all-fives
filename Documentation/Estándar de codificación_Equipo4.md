**Estándar de codificación para C\# y .NET**  
**Tecnologías para la Construcción de Software**  
*Facultad de Estadística e Informática \- Universidad Veracruzana*

Docente: Juan Carlos Pérez Arriaga 

Equipo 4:  
Sebastián Barrera Mora  
Yarazareth Zacnite Ortiz Olmos

Versión: 5.0

Fecha de entrega: 15 de Noviembre de 2026

**Contenido**

[**1\. Introducción	4**](#introducción)

[1.1. Problemática	4](#1.1.-problemática)

[1.2. Propósito	4](#1.2.-propósito)

[1.3. Tecnologías utilizadas	4](#1.3.-tecnologías-utilizadas)

[1.4. Idioma	4](#1.4.-idioma)

[1.4.1 Idioma del código	4](#1.4.1-idioma-del-código)

[1.4.2 Idioma de la base de datos	5](#1.4.2-idioma-de-la-base-de-datos)

[**2\. Reglas de nombrado	5**](#reglas-de-nombrado)

[2.1. Variables locales	5](#2.1.-variables-locales)

[2.1.1 Variables de incremento	5](#2.1.1-variables-de-incremento)

[2.2. Campos	6](#2.2.-campos)

[2.3. Propiedades	7](#2.3.-propiedades)

[2.4. Constantes	8](#2.4.-constantes)

[2.5. Métodos	8](#2.5.-métodos)

[2.6. Clases	9](#2.6.-clases)

[2.7. Estructuras	10](#2.7.-estructuras)

[2.8. Interfaces	10](#2.8.-interfaces)

[2.9. Enumeraciones	10](#2.9.-enumeraciones)

[2.10. Namespaces	11](#2.10.-namespaces)

[2.11. Parámetros	12](#2.11.-parámetros)

[2.12. Eventos	13](#2.12.-eventos)

[2.13 Componentes gráficos de WPF	14](#2.13-componentes-gráficos-de-wpf)

[2.14 Vistas y archivos XAML	17](#2.14-vistas-y-archivos-xaml)

[2.15 Recursos para el proyecto	17](#2.15-recursos-para-el-proyecto)

[2.16 Recursos de internacionalización y claves	17](#2.16-recursos-de-internacionalización-y-claves)

[2.17 Base de datos, tablas y columnas	18](#2.17-base-de-datos,-tablas-y-columnas)

[2.18 Proyectos y soluciones	18](#2.18-proyectos-y-soluciones)

[2.19 Contratos y elementos de WCF	18](#2.19-contratos-y-elementos-de-wcf)

[2.20 Nombrado de excepciones personalizadas de reglas de negocio	20](#2.20-nombrado-de-excepciones-personalizadas-de-reglas-de-negocio)

[**3\. Estilo de código	20**](#estilo-de-código)

[3.1. Formato y estructura general	20](#3.1.-formato-y-estructura-general)

[3.1.1. Indentación	20](#3.1.1.-indentación)

[3.1.2. Longitud de línea	22](#3.1.2.-longitud-de-línea)

[3.1.3. Espacios en blanco	24](#3.1.3.-espacios-en-blanco)

[3.1.4. Llaves	27](#3.1.4.-llaves)

[3.2. Organización del código en el archivo	28](#3.2.-organización-del-código-en-el-archivo)

[3.3. Orden de miembros	29](#3.3.-orden-de-miembros)

[**4\. Prácticas de programación	32**](#prácticas-de-programación)

[4.1. Modificadores de acceso	32](#4.1.-modificadores-de-acceso)

[4.2. Encapsulamiento	33](#4.2.-encapsulamiento)

[4.3. Herencia	34](#4.3.-herencia)

[4.4. Interfaces	37](#4.4.-interfaces)

[4.5. Sobrescritura de métodos	39](#4.5.-sobrescritura-de-métodos)

[4.6. Manejo de valores nulos	41](#4.6.-manejo-de-valores-nulos)

[4.7. Manejo de excepciones	42](#4.7.-manejo-de-excepciones)

[4.8. Expresiones lambda y cuerpos de expresión	44](#4.8.-expresiones-lambda-y-cuerpos-de-expresión)

[4.9. Números y cadenas mágicas	45](#4.9.-números-y-cadenas-mágicas)

[4.10. Complejidad de métodos	46](#4.10.-complejidad-de-métodos)

[4.11. Complejidad ciclomática	47](#4.11.-complejidad-ciclomática)

[4.12. Número de parámetros	49](#4.12.-número-de-parámetros)

[4.13. Expresiones lógicas	50](#4.13.-expresiones-lógicas)

[4.14. Registro de eventos y errores (Logs)	51](#4.14.-registro-de-eventos-y-errores-\(logs\))

[4.14.1 Categorización de errores	52](#4.14.1-categorización-de-errores)

[4.15 Inicialización y asignación	54](#4.15-inicialización-y-asignación)

[4.16 Composición de textos e Internacionalización	55](#4.16-composición-de-textos-e-internacionalización)

[4.17 Uso de las variables de tipo local	56](#4.17-uso-de-las-variables-de-tipo-local)

[4.18 Consultas de datos con LINQ	56](#4.18-consultas-de-datos-con-linq)

[4.19 Uso de enumeraciones	57](#4.19-uso-de-enumeraciones)

[4.20 Enfoque orientado a objetos	57](#4.20-enfoque-orientado-a-objetos)

[4.21 Manejo de concurrencia y sincronización	57](#4.21-manejo-de-concurrencia-y-sincronización)

[4.22 Comunicación remota con WCF	59](#4.22-comunicación-remota-con-wcf)

[**5\. Comentarios y documentación	61**](#comentarios-y-documentación)

[5.1. Comentarios de implementación	61](#5.1.-comentarios-de-implementación)

[5.2. Comentarios de una sola línea	62](#5.2.-comentarios-de-una-sola-línea)

[5.3. Comentarios en bloque	63](#5.3.-comentarios-en-bloque)

[5.4. Comentarios de documentación XML	63](#5.4.-comentarios-de-documentación-xml)

[5.5. Comentarios TODO	65](#5.5.-comentarios-todo)

[5.6. Comentarios FIXME	65](#5.6.-comentarios-fixme)

[5.7 Comentarios en archivos XAML	66](#5.7-comentarios-en-archivos-xaml)

[5.8 Excepciones en archivos autogenerados	67](#5.8-excepciones-en-archivos-autogenerados)

[**6\. Estructuras de control	68**](#estructuras-de-control)

[6.1. Sentencias condicionales	68](#6.1.-sentencias-condicionales)

[6.2. Bucles	69](#6.2.-bucles)

[6.3. Sentencias switch	71](#6.3.-sentencias-switch)

[6.4. Operador ternario	72](#6.4.-operador-ternario)

[6.5. Operador de coalescencia nula	73](#6.5.-operador-de-coalescencia-nula)

[6.6. Control de flujo	73](#6.6.-control-de-flujo)

[**7\. Pruebas	76**](#pruebas)

[7.1. Pruebas unitarias	76](#7.1.-pruebas-unitarias)

[7.2. Ubicación de las pruebas	77](#7.2.-ubicación-de-las-pruebas)

[7.3. Nomenclatura de las pruebas	77](#7.3.-nomenclatura-de-las-pruebas)

[7.4. Estructura de las pruebas	78](#7.4.-estructura-de-las-pruebas)

[**8\. Declaración de uso de IA	80**](#declaración-de-uso-de-ia)

[**9\. Referencias	81**](#referencias)

1. # Introducción {#introducción}

En la industria del desarrollo de software, se considera una buena práctica el uso de los estándares de codificación para el desarrollo de proyectos. Un estándar de codificación es un conjunto de reglas y prácticas que se deben seguir durante la elaboración de un proyecto, esto con el fin de garantizar coherencia y una mejor calidad del código. Aunque existen muchos estándares creados por organizaciones reconocidas, los equipos de desarrollo de software suelen crear su propio estándar que se adapte al proyecto a desarrollar, llegando a un acuerdo entre los miembros para definir el estilo a seguir en el resto del proyecto.

## 1.1. Problemática {#1.1.-problemática}

Con fin de formarnos como ingenieros de software, en la experiencia educativa de Tecnologías para la Construcción de Software se elaborará un proyecto de desarrollo de un videojuego. El videojuego será Domino All Fives, y será una versión multijugador del juego que deberá permitir a jugadores conectarse y jugar al juego.

## 1.2. Propósito {#1.2.-propósito}

Este estándar de codificación busca establecer las directrices y buenas prácticas que el equipo deberá seguir durante el desarrollo del proyecto de la experiencia educativa. 

## 1.3. Tecnologías utilizadas {#1.3.-tecnologías-utilizadas}

Las tecnologías que se utilizarán para este proyecto son: 

* C\#  
* .NET Framework 4.8  
* SQL Server  
* Microsoft Entity Framework   
* Windows Communication Foundation 

## 1.4. Idioma {#1.4.-idioma}

### 1.4.1 Idioma del código {#1.4.1-idioma-del-código}

El idioma que se utilizará en la elaboración del proyecto será el idioma inglés, tanto para el código como otros aspectos como la base de datos. Esto se hace debido a que se busca adaptarse a las herramientas internacionales que se usan en la industria. Este estándar utilizará el idioma inglés en lo referente al código, por lo tanto los ejemplos presentados serán únicamente en este idioma.

### 1.4.2 Idioma de la base de datos {#1.4.2-idioma-de-la-base-de-datos}

Al igual que el código, la base de datos y las tablas deben estar en el idioma inglés. Esto se hace para mantener una coherencia entre el idioma del código y el idioma de la fuente de datos, en este caso la base de datos. 

2. # Reglas de nombrado {#reglas-de-nombrado}

Los nombres deberán ser **significativos, descriptivos** y expresar el propósito o significado del dato almacenado, priorizando la claridad sobre la brevedad (Microsoft, 2026a).

##  2.1. Variables locales {#2.1.-variables-locales}

Los nombres de las variables locales deberán escribirse utilizando **camelCase** (Microsoft, 2026a).

Se deberán utilizar palabras completas cuando sea posible y evitar abreviaturas ambiguas o nombres que no comuniquen claramente la finalidad de la variable. El nombre deberá describir el **significado del dato y no su tipo**, debido a que el tipo ya se encuentra expresado o puede inferirse mediante C\# (Microsoft, 2026b).

Para valores booleanos se utilizarán nombres que expresan el estado o condición representada, procurando que puedan interpretarse como verdadero o falso.

| Con estándar | Sin estándar |
| ----- | ----- |
| `int playerScore = 1500;` `string playerName = "Yara";` `bool isGamePaused = false;` `float movementSpeed = 5.5f;` `Player currentPlayer = new Player();` | `int PlayerScore = 1500;` `string player_name = "Sebastián";` `bool flag = false;` `float spd = 5.5f;` `Player PLAYER = new Player();` |

### 2.1.1 Variables de incremento  {#2.1.1-variables-de-incremento}

Para las variables de incremento, no se hará uso de `i`, `j` o `k`. Se escribirán en **camelCase**. 

| Con estándar |
| ----- |
| `for (int enemyIndex = 0; enemyIndex < activeEnemies.Count; enemyIndex++)` |
| **Sin estándar** |
| `for (int i= 0; enemyIndex < activeEnemies.Count; i++)` |

## 

## 2.2. Campos {#2.2.-campos}

Los campos de instancia `private` o `internal` que no sean constantes deberán escribirse utilizando **camelCase** precedido por un guión bajo (`_`). Si además el campo es `static`, este usará **camelCase** y no se colocará nigún guión bajo..

Cuando exista un campo público, su nombre deberá utilizar **PascalCase**.

| Con estándar |
| ----- |
| `public class Player {     private int _currentHealth;     private bool _isInvulnerable;     private List<string> _unlockedAbilities;     private static int activePlayerCount;     public int MaximumPlayers { get; set; } }` |
| **Sin estándar** |
| `public class Player {     private int CurrentHealth;     private bool inv;     private List<string> abilitiesList;     private static int ActivePlayerCount;     public int maximum_players; }` |

## 2.3. Propiedades {#2.3.-propiedades}

Los nombres de las propiedades deberán utilizar **PascalCase**. Usar sustantivos, frases nominales o adjetivos para nombrar propiedades (Microsoft, 2023a).

Las propiedades booleanas deberán utilizar nombres que expresen una condición de manera afirmativa. Cuando aporte claridad, se podrán utilizar los prefijos `Is`, `Can` o `Has`. (Microsoft, 2023a).

Las propiedades que representen colecciones deberán utilizar un nombre en plural que describa los elementos contenidos. No se deberá agregar innecesariamente `List` o `Collection` al final del nombre cuando el plural ya comunique que se trata de una colección (Microsoft, 2023a).

| Con estándar |
| ----- |
| `public class Player` `{`     `private float _movementSpeed;`     `public string PlayerName { get; set; }`     `public int CurrentHealth { get; private set; }`     `public int MaximumHealth { get; }`     `public bool IsAlive { get; private set; }`     `public bool CanAttack { get; set; }`     `public bool HasShield { get; private set; }`     `public IReadOnlyList<string> UnlockedAbilities { get; }`     `public float MovementSpeed`     `{`         `get`         `{`             `return _movementSpeed;`         `}`         `set`         `{`             `_movementSpeed = Math.Max(0f, value);`         `}`     `}` `}` |
| **Sin estándar** |
| `public class Player` `{`     `public string playerName { get; set; }`     `public int current_health { get; private set; }`     `public int maxHealth { get; }`     `public bool alive { get; private set; }`     `public bool can_attack { get; set; }`     `public bool shield { get; private set; }`     `public List<string> abilityList { get; }`     `public float movement_speed`     `{`         `get => _movementSpeed;`         `set => _movementSpeed = Math.Max(0f, value);`     `}`     `private float _movementSpeed;` `}` |

## 2.4. Constantes {#2.4.-constantes}

Las constantes deberán utilizar **PascalCase**, independientemente de su nivel de acceso. Esta convención aplica tanto a constantes declaradas como campos como a constantes locales (Microsoft, 2026a).

| Con estándar |
| ----- |
| `public class Player {     public const int MaximumHealth = 100;     public const float CriticalHealthThreshold = 0.2f;     public const string DefaultPlayerName = "Player"; }` |
| **Sin estándar** |
| `public class Player {     public const int MAX_HEALTH = 100;     public const float crit_hp = 0.2f;     public const string PLAYER = "Player"; }` |

## 2.5. Métodos {#2.5.-métodos}

El nombre deberá comunicar **qué acción realiza el método** y utilizar **PascalCase** (Microsoft, 2026a). 

No deberán utilizarse nombres que describan únicamente el tipo de dato que devuelve el método ni prefijos genéricos como `Do`, `Method` o `Function` cuando no aporten información sobre la operación. 

| Con estándar |
| ----- |
| `public class PlayerController {     public void MovePlayer(Vector3 targetPosition, float movementSpeed)     {     }     public void TakeDamage(int damageAmount)     {     } }` |
| **Sin estándar** |
| `public class PlayerController {     public void move_player(Vector3 TargetPosition, float MovementSpeed)     {     }     public void Do(int x)     {     } }` |

## 2.6. Clases {#2.6.-clases}

El nombre deberá representar con  un **sustantivo o frase nominal** la entidad, componente o responsabilidad principal de la clase y usar **PascalCase.** (Microsoft, 2026).

| Con estándar | Sin estándar |
| ----- | ----- |
| `public class PlayerController { }` | `public class player_ctrl { }` |

## 2.7. Estructuras {#2.7.-estructuras}

El nombre de una estructura deberá utilizar **PascalCase**, al igual que los demás tipos definidos en C\# (Microsoft, 2026).

| Con estándar | Sin estándar |
| ----- | ----- |
| `public struct DamageRange { }` | `public struct player_Data { }` |

## 2.8. Interfaces {#2.8.-interfaces}

Los nombres de las interfaces deberán utilizar **PascalCase** y comenzar con la letra mayúscula **`I`**. El uso de este prefijo sí está permitido. (Microsoft, 2026b).

| Con estándar | Sin estándar |
| ----- | ----- |
| `public interface IDamageable { }` | `public interface Damageable { }` |

## 2.9. Enumeraciones {#2.9.-enumeraciones}

Los nombres de las enumeraciones y también de sus miembros deberán utilizar **PascalCase**. (Microsoft, 2026a).

Los miembros de una enumeración deberán utilizar nombres que identifiquen directamente el estado, categoría u opción que representan. No deberán incluir prefijos o sufijos que repitan innecesariamente el nombre de la enumeración. (Microsoft, 2023).

| Con estándar | Sin estándar |
| ----- | ----- |
| `public enum GameDifficulty {     Easy,     Normal,     Hard }` | `public enum game_difficulty {     GAME_DIFFICULTY_EASY,     GAME_DIFFICULTY_NORMAL,     GAME_DIFFICULTY_HARD }` |

## 2.10. Namespaces {#2.10.-namespaces}

Los namespaces deberán utilizar **PascalCase** y organizarse de manera que reflejen la estructura lógica y pertenencia dentro del proyecto (Microsoft, 2026a). Los namespaces deberán utilizar una **estructura jerárquica**.

Namespace raíz debe corresponder al nombre del proyecto y los namespaces posteriores deben representar agrupaciones lógicas del código, siguiendo la forma: `namespace ProjectName.Feature`.

| Con estándar | Sin estándar |
| ----- | ----- |
| `namespace GameProject.Gameplay namespace GameProject.Gameplay.Enemies` | `namespace gameproject.gameplay namespace GP.GameplayStuff` |

## 

## 2.11. Parámetros {#2.11.-parámetros}

Los parámetros deberán utilizar **camelCase** (Microsoft, 2026a; Microsoft, 2023a).

Los parámetros de los métodos no deberán utilizar los mismos nombres de los campos de las clases, ya que puede generar confusiones al leer el código.

| Con estándar |
| ----- |
| `public class Player {     private int _currentHealth;     public void TakeDamage(int damageAmount)     {         _currentHealth -= damageAmount;      }     public bool HasEnoughMana(int requiredMana)     {         return true;     } }` |
| **Sin estándar** |
| `public class Player {     private int currentHealth;     public void TakeDamage(int currentHealth)     {         this.currentHealth -= currentHealth;       }     public bool HasEnoughMana(int m)     {         return true;     } }` |

## 

## 2.12. Eventos {#2.12.-eventos}

Los eventos deberán utilizar **PascalCase** para sus nombres y deberán representar una acción, cambio de estado o situación relevante que otros componentes puedan detectar (Microsoft, 2026a).

Los nombres de los eventos deberán utilizar preferentemente verbos o frases verbales que describan aquello que ha ocurrido. Cuando corresponda a una acción que está a punto de producirse y pueda cancelarse, deberá utilizarse el prefijo `On` de acuerdo con las convenciones de diseño de eventos de .NET (Microsoft, 2023).

| Con estándar |
| ----- |
| `public class Player` `{`     `public event EventHandler Died;`     `public event EventHandler<CancelEventArgs> OnGameEnding;` `}` |
| **Sin estándar** |
| `public class Player` `{`     `public event EventHandler player_died;`     `public event EventHandler<CancelEventArgs> game_ending_event;` `}` |

## 

## 2.13 Componentes gráficos de WPF {#2.13-componentes-gráficos-de-wpf}

Los identificadores (x:Name) de los controles de interfaz gráfica (XAML) deberán utilizar **camelCase**. Cada identificador debe comenzar obligatoriamente con un prefijo de tres letras (en minúsculas) que indique el tipo de control visual, seguido de un nombre descriptivo de su función o contenido (Microsoft, 2026g).

Los controles que no se mencionan explícitamente en esta sección pero que en dado caso fuera necesario su uso, se deben nombrar con las primeras 3 letras de su nombre.

* **Excepción:** Cuando un elemento visual representa una entidad fundamental del dominio del juego o represente un elemento visual importante para la interfaz gráfica, como el contenedor principal, se permite el uso de nombres semánticos directos en **camelCase** sin requerir el prefijo técnico. Esto con tal de hacer más fácil entender su propósito cuando el prefijo lo puede complicar.

| Button | Con estándar |
| :---: | ----- |
|  | `<Button x:Name="btnLogin"/>`  |
| btn | **Sin estándar** |
|  | `<Button x:Name="botonEnviar"/>`  |

| TextBlock | Con estándar |
| :---: | ----- |
|  | `<TextBlock x:Name="txbRoomCode"/>`  |
| txb | **Sin estándar** |
|  | `<TextBlock x:Name="codigo_sala"/>`  |

| Label | Con estándar |
| :---: | ----- |
|  | `<Label x:Name="lblRoomCode"/>` |
| lbl | **Sin estándar** |
|  | `<Label x:Name="codigo_sala"/>`  |

| TextBox | Con estándar |
| :---: | ----- |
|  | `<TextBox x:Name="txtUsername"/>` |
| txt | **Sin estándar** |
|  | `<TextBox x:Name="textUsername"/>` |

| PasswordBox | Con estándar |
| :---: | ----- |
|  | `<PasswordBox x:Name="pwdPassword"/>` |
| pwd | **Sin estándar** |
|  | `<PasswordBox x:Name="password"/>` |

| Image | Con estándar |
| :---: | ----- |
|  | `<Image x:Name="imgProfileAvatar"/>`  |
| img | **Sin estándar** |
|  | `<Image x:Name="avatar_perfil"/>`  |

| ListBox / ListView | Con estándar |
| ----- | ----- |
|  | `<ListView x:Name="lstOnlineFriends"/>`  |
| lst | **Sin estándar** |
|  | `<ListView x:Name="ListaAmigos"/>`  |

| ComboBox | Con estándar |
| ----- | ----- |
|  | `<ComboBox x:Name="cbxLanguageSelector"/>`  |
| cbx | **Sin estándar** |
|  | `<ComboBox x:Name="ComboBoxLanguage"/>`   |

| CheckBox | Con estándar |
| ----- | ----- |
|  | `<CheckBox x:Name="chkMuteAudio"/>` |
| chk | **Sin estándar** |
|  | `<CheckBox x:Name="mute-audio"/>`  |

| RadioButton | Con estándar |
| ----- | ----- |
|  | `<RadioButton x:Name="rdoPublicRoom"/>`   |
| rdo | **Sin estándar** |
|  | `<RadioButton x:Name="RoomP"/>`  |

| Run | Con estándar |
| :---: | ----- |
|  | `<Run x:Name="runTimerDisplay”/>`   |
| run | **Sin estándar** |
|  | `<Run x:Name="timer/>` |

| ItemsControl | Con estándar |
| :---: | ----- |
|  | `<ItemsControl x:Name="itmChatMessages"/>`   |
| itm | **Sin estándar** |
|  | `<ItemsControl x:Name="messages"/>` |

| Grid | Con estándar |
| :---: | ----- |
|  | `<Grid x:Name="grdTileContent">` |
| grd | **Sin estándar** |
|  | `<Grid x:Name="tileContent">` |

## 

## 2.14 Vistas y archivos XAML  {#2.14-vistas-y-archivos-xaml}

Los archivos `.xaml` que representan la interfaz gráfica deberán escribirse estrictamente en **inglés** utilizando **PascalCase**. 

| Con estándar | Sin estándar |
| ----- | ----- |
| `MainMenu.xaml Settings.xaml CreateLobby.xaml` | `menu_principal.xaml CONFIGURACION.xaml crearlobby.xaml` |

## 2.15 Recursos para el proyecto {#2.15-recursos-para-el-proyecto}

Los archivos que sean recursos visuales, como imagenes, se nombran utilizando **camelCase** y mantener extensiones en minúsculas. 

| Con estándar | Sin estándar |
| ----- | ----- |
| `defaultAvatar.png woodBackground.jpg iconSettings.svg` | `DEFAULT_AVATAR.png fondo-madera.jpg icono.svg` |

## 2.16 Recursos de internacionalización y claves  {#2.16-recursos-de-internacionalización-y-claves}

Los archivos físicos de diccionarios de recursos deberán escribirse en **PascalCase**. 

Las claves deberán utilizar la estructura ViewName\_ControlName (o Global\_ElementName para textos transversales). Está estrictamente prohibido el uso de puntos (.) en las claves para evitar errores en el generador de código de Visual Studio (Microsoft, 2024). 

Para las reglas de nombrado de ViewName ver la [sección 2.13](#2.13-componentes-gráficos-de-wpf).

| Con estándar | Sin estándar |
| ----- | ----- |
| `Login_lblUsername Global_btnBack` | `Login.lblUsername BotonRegresar` |

## 2.17 Base de datos, tablas y columnas {#2.17-base-de-datos,-tablas-y-columnas}

Tanto la base de datos como las tablas y columnas deberán escribirse en **PascalCase.** 

| Con estándar | Sin estándar |
| ----- | ----- |
| `DominoAllFives Base de datos FriendRequest Tabla IdInvitation Columna` | `domino_bd Base de datos friend_request Tabla invitation Columna` |

## 2.18 Proyectos y soluciones {#2.18-proyectos-y-soluciones}

Para el nombrado de los proyectos, deberán utilizar **PascalCase**, las cuales deberán seguir una estructura jerárquica en **PascalCase**, reflejando claramente su propósito arquitectónico. La regla de nombrado aplica también para las soluciones, indicando el dominio principal y el ámbito o capa correspondiente.

| Con estándar | Sin estándar |
| ----- | ----- |
| `DominoAllFives.Client  DominoAllFives.Services` | `services domino.wpf` |

## 2.19 Contratos y elementos de WCF {#2.19-contratos-y-elementos-de-wcf}

Los elementos que conforman la frontera de comunicación mediante *Windows Communication Foundation* (WCF) deberán respetar convenciones de nombrado que reflejen su propósito como interfaces públicas del sistema y que sean consistentes con los conceptos del dominio. 

* **Contratos de Servicio (ServiceContract):** Deberán ser interfaces, comenzar con la letra **`I`** mayúscula y terminar obligatoriamente con el sufijo **`Service`** (o **`Callback`** para contratos dúplex). 

| Con estándar | Sin estándar |
| ----- | ----- |
| `IRegistrationService  IMatchCallback`  | `RegistrationContract  MatchClientInterface`  |

* **Contratos de Datos (DataContract):** Deberán utilizar **PascalCase** y representar conceptos explícitos del modelo o transferencia 

| Con estándar | Sin estándar |
| ----- | ----- |
| `PlayerProfile`  | `GetProfileResponse`  |

* **Fallos y Errores de Negocio (FaultContract y Enums):** Las enumeraciones que tipifican los errores de WCF deberán utilizar el sufijo **`Issue`** para diferenciarlas de las excepciones nativas de C\# 

| Con estándar | Sin estándar |
| ----- | ----- |
| `AccountIssue`  | `AccountExceptionCode`  |

* **Operaciones de Servicio (OperationContract):** Los métodos expuestos en los contratos deberán utilizar **PascalCase** y expresar claramente una acción de negocio.

| Con estándar | Sin estándar |
| ----- | ----- |
| `[OperationContract] Lobby JoinLobby(string lobbyCode);` | `[OperationContract] Lobby RequestLobbyJoin(string lobbyCode);` |

* **Operaciones de Notificación (CallbackContract):** Los métodos que el servidor utiliza para notificar al cliente (marcados con `IsOneWay = true`) deberán utilizar obligatoriamente el prefijo **`On`** seguido de la acción en pasado o del evento ocurrido, para distinguir claramente que son eventos de respuesta y no invocaciones directas del cliente. 

| Con estándar | Sin estándar |
| ----- | ----- |
| `[OperationContract(IsOneWay = true)] void OnMatchStarted(int matchId);` | `[OperationContract(IsOneWay = true)] void MatchStartNotification(int matchId);` |


* **Implementaciones de Servicio:** Las clases que implementan un contrato de servicio WCF deberán conservar el mismo nombre de la interfaz sin el prefijo **I**, finalizando con el sufijo **Service**.

| Con estándar | Sin estándar |
| ----- | ----- |
| `LobbyService` `ChatService` | `LobbyServiceImpl` `WcfChatManager` |

## 2.20 Nombrado de excepciones personalizadas de reglas de negocio {#2.20-nombrado-de-excepciones-personalizadas-de-reglas-de-negocio}

Las clases de excepción que representen rechazos esperados de reglas de negocio dentro del servidor deberán utilizar **PascalCase** con el nombre del dominio seguido obligatoriamente del sufijo **RuleException**.

| Con estándar | Sin estándar |
| ----- | ----- |
| `AccountRuleException` `LobbyRuleException` `ChatRuleException` | `AccountBusinessException` `LobbyErrorException` `ChatValidationException` |

3. # Estilo de código {#estilo-de-código}

## 3.1. Formato y estructura general {#3.1.-formato-y-estructura-general}

### 3.1.1. Indentación {#3.1.1.-indentación}

El código deberá utilizar **cuatro espacios por cada nivel de indentación** y no se utilizarán caracteres de tabulación. (Microsoft, 2025; .NET Foundation & Microsoft, 2026).

Las llaves no se desplazan respecto a la estructura a la que pertenecen (Microsoft, 2025). El contenido de un bloque debe estar un nivel más indentado que el elemento que lo contiene. 

En switch, los case se indentarán un nivel y sus instrucciones otro nivel adicional.

En cuanto a las líneas de continuación (líneas divididas) mantener una indentación consistente con la de la instrucción anterior, que evidencie que juntas son la instrucción completa.

| Con estándar |
| ----- |
| `public class DominoGameController {     public void ProcessPlayerTurn(Tile selectedTile)     {         if (isTurnValid)         {             PlaceTileOnBoard(selectedTile);         }         switch (currentGameState)         {             case GameState.EvaluatingScore:                 CalculateAllFivesScore();                 break;             default:                 WaitForNextPlayer();                 break;         }     } }` |
| **Sin estándar** |
| `public class DominoGameController { public void ProcessPlayerTurn(Tile selectedTile) {     if (isTurnValid)     {     PlaceTileOnBoard(selectedTile);     } switch (currentGameState) { case GameState.EvaluatingScore: CalculateAllFivesScore(); break; case GameState.RoundEnded: StartNextRound(); break; } } }` |

### 3.1.2. Longitud de línea {#3.1.2.-longitud-de-línea}

Las instrucciones demasiado extensas deberán dividirse de manera que su estructura continúe siendo clara y fácil de seguir.

Las líneas de código deberán mantenerse en un máximo de **100 caracteres siempre que sea razonablemente posible**. (Google, s. f.).

* Cuando una **declaración, llamada a método, expresión o inicialización** supere este límite, deberá dividirse en varias líneas en puntos que conserven claramente la relación entre sus elementos.   
* En **llamadas o declaraciones de métodos** con varios argumentos, estos podrán distribuirse en líneas adicionales manteniendo una indentación consistente.(Google, s. f.).  
* Para expresiones largas, los saltos deberán realizarse de forma que **no dificulten la comprensión de la operación**.   
* Cuando sea necesario dividir una expresión que contiene **operadores binarios**, el salto de línea puede realizarse **antes del operador**.(Microsoft, 2025).  
* Las instrucciones, argumentos o expresiones cortas como `nameof(parametro)` que no superen el límite de 100 caracteres deberán mantenerse en una sola línea, evitando saltos de línea innecesarios.

No se deberán **acortar nombres descriptivos** de clases, métodos o variables únicamente para cumplir con el límite de longitud.

* **Excepciones:** Podrán exceder el límite aquellas expresiones **cuya división perjudique más la comprensión que mantenerlas completas**, como ciertos identificadores externos, rutas o cadenas cuyo contenido deba conservarse íntegro. También, se exentan del límite de la longitud de línea los archivos de código fuente C\# generados de manera automática por herramientas del entorno (por ejemplo, diseñadores de datos `.edmx`, plantillas T4, referencias de servicio WCF o Entity Framework) que incluyan el encabezado `<auto-generated>`.

| Con estándar |
| ----- |
| `public void SpawnEnemy(     EnemyType enemyType,     Vector3 spawnPosition,     int difficultyLevel) {     bool canSpawnEnemy =         activeEnemyCount < maximumEnemyCount         && currentGameState == GameState.Playing;     if (canSpawnEnemy)     {         Enemy enemy = enemyFactory.CreateEnemy(             enemyType,             spawnPosition,             difficultyLevel);         activeEnemies.Add(enemy);     } }` |
| **Sin estándar** |
| `public void SpawnEnemy(EnemyType enemyType, Vector3 spawnPosition, int difficultyLevel, int maximumEnemyCount, GameState currentGameState) {     bool canSpawnEnemy = activeEnemyCount < maximumEnemyCount && currentGameState == GameState.Playing && spawnPosition != Vector3.Zero;     if (canSpawnEnemy)     {         Enemy enemy = enemyFactory.CreateEnemy(enemyType, spawnPosition, difficultyLevel, maximumEnemyCount, currentGameState);         activeEnemies.Add(enemy);     } }` |

### 3.1.3. Espacios en blanco {#3.1.3.-espacios-en-blanco}

| Estructuras de control | Colocar un espacio entre la palabra clave y el paréntesis de apertura.  No deberán colocarse espacios inmediatamente después del paréntesis de apertura ni antes del paréntesis de cierre. (Microsoft, 2025a). |
| :---: | :---- |

| Con estándar | Sin estándar |
| ----- | ----- |
| `if (canAttack) {     AttackPlayer(); }` | `if( canAttack ) {     AttackPlayer(); }` |

| Operadores lógicos y aritméticos | Como `=`, `+`, `-`, `*`, `/`, `==`, `!=`, `<`, `>`, `&&` y `||`, deberán escribirse con un espacio antes y después. |
| :---: | :---- |

| Con estándar |
| ----- |
| `int totalPoints = baseDamage + bonusPoints;`  |
| **Sin estándar** |
| `int totalPoints=baseDamage+bonusPoints;`  |

| Declaraciones y llamadas a métodos | No se colocará un espacio entre el nombre del método y el paréntesis de apertura, y tampoco se agregarán espacios inmediatamente dentro de los paréntesis.  |
| :---: | :---- |

| Con estándar |
| ----- |
| `public void UpdatePlayerState(Vector3 targetPosition) {     MoveTo(targetPosition); }` |
| **Sin estándar** |
| `public void UpdatePlayerState ( Vector3 targetPosition ) {     MoveTo (targetPosition); }` |

| Declaraciones y llamadas a métodos | Deberá utilizarse una coma y no se colocará espacio antes de ella. Esta regla se aplicará a argumentos, parámetros, inicializadores y demás construcciones que utilicen comas. |
| :---: | :---- |

| Con estándar |
| ----- |
| `SpawnTile(tileIndex, position, isFaceUp);`  |
| **Sin estándar** |
| `SpawnTile(tileIndex ,position , isFaceUp);`  |

| Operador de acceso a miembros | No deberán colocarse espacios antes ni después del punto operador de acceso (.) (Microsoft, 2025a). |
| :---: | :---- |

| Con estándar | Sin estándar |
| ----- | ----- |
| `if (canAttack) {     AttackPlayer(); }` | `if( canAttack ) {     AttackPlayer(); }` |

| Instrucción *for* | Se deberá utilizar un espacio después de cada punto y coma y ninguno antes de él. (Microsoft, 2025a). |
| :---: | :---- |

| Con estándar |
| ----- |
| `for (int tileIndex = 0; tileIndex < activeTiles.Count; tileIndex++)`  |
| **Sin estándar** |
| `for (int tileIndex = 0 ;tileIndex < activeTiles.Count ;tileIndex++)`  |

| Espacio vertical entre los miembros. | Deberá existir al menos una línea en blanco entre definiciones de métodos y propiedades cuando resulte necesario para distinguir sus responsabilidades. (.NET Foundation & Microsoft, 2026; Microsoft, 2025b). |
| :---: | :---- |

| Con estándar |
| ----- |
| `public class DominoPlayer {     public int Score { get; private set; }     public void AddPoints(int points)     {         Score += points;     } }` |
| **Sin estándar** |
| `public class DominoPlayer {     public int Score { get; private set; }     public void AddPoints(int points)     {         Score += points;     } }`  |

| Espacio entre declaraciones de un archivo | Se deberá separar con una linea en blanco los diferentes niveles de organización que tendrán los archivos. Estos niveles se observan en la sección [3.2 Organización del código del archivo](#3.2.-organización-del-código-en-el-archivo). |
| :---: | :---- |

| Con estándar |
| ----- |
| `using System;  namespace Starfall.DominoGame.Gameplay {    public class TileBoard    {    } }`  |
| **Sin estándar** |
| `using System; namespace Starfall.DominoGame.Gameplay {    public class TileBoard    {    } }` |

### 3.1.4. Llaves {#3.1.4.-llaves}

Se utilizará el **estilo Allman**, colocando tanto la llave de apertura como la llave de cierre en una línea independiente y alineadas con la declaración que contiene el bloque. (Microsoft, 2024; .NET Foundation & Microsoft, 2026).

Las llaves deberán utilizarse en las estructuras de control **aunque el bloque contenga una sola instrucción**. Esta regla se aplica a: `if`, `else`, `for`, `foreach`, `while`, `do` y demás bloques de control que admitan llaves. 

Cuando aparezcan cláusulas relacionadas como `else`, `catch` o `finally`, la llave de cierre del bloque anterior permanecerá en su propia línea y la palabra clave siguiente comenzará en una nueva línea. (NET Foundation & Microsoft, 2026; Microsoft, 2025).

| Con estándar |
| ----- |
| `public class EnemyController {     public void UpdateEnemy()     {         if (playerIsVisible)         {             AttackPlayer();         }         else         {             PatrolArea();         } }` |
| **Sin estándar** |
| `public class EnemyController {     public void UpdateEnemy() {         if (playerIsVisible)             AttackPlayer();         else {             PatrolArea();         } }` |

## 3.2. Organización del código en el archivo {#3.2.-organización-del-código-en-el-archivo}

Los archivos C\# del proyecto deberán organizarse, de forma general, en el siguiente orden, además de separar cada nivel con una linea en blanco (salto de linea):

|  | Elemento | Descripción |
| ----- | ----- | ----- |
| **1** | **Directivas `using`** | Se colocarán al inicio del archivo y fuera del `namespace`. Se ordenarán alfabéticamente, colocando primero las directivas de `System`. |
| **2** | **Declaración `namespace`** | Los namespaces deberán declararse utilizando **bloques de llaves tradicionales**. |
| **3** | **Declaración del tipo principal** | Después del `namespace` se declarará la clase, estructura, interfaz, enumeración o delegado principal del archivo. |

| Con estándar |
| ----- |
| `using System; using System.Collections.Generic; using Starfall.Audio; using Starfall.Gameplay.Entities;`  `namespace Starfall.Gameplay.Enemies {     public class EnemySpawner     {     } }`  |
| **Sin estándar** |
| `namespace Starfall.Gameplay.Enemies {     public class EnemySpawner     {     }     using Starfall.Gameplay.Entities;     using System.Collections.Generic;     using Starfall.Audio;     using System; } public class EnemyAudioManager { }` |

## 3.3. Orden de miembros {#3.3.-orden-de-miembros}

Dentro de cada clase, los miembros deberán declararse en el siguiente orden:

Cuando una clase no requiera alguno de los grupos anteriores, este simplemente se omitirá y se continuará con el siguiente. No deberán añadirse miembros vacíos o artificiales únicamente para conservar todas las categorías.

Los miembros que pertenezcan a la misma categoría deberán mantenerse juntos. La **consistencia del orden tiene prioridad** sobre intentar organizar los miembros según el momento en que fueron agregados al proyecto.

|  | Elemento |
| ----- | :---: |
| **1** | **Constantes** |
| **2** | **Campos estáticos** |
| **3** | **Campos de instancia** |
| **4** | **Propiedades** |
| **5** | **Constructores** |
| **6** | **Eventos** |
| **7** | **Métodos públicos** |
| **8** | **Métodos protegidos** |
| **9** | **Métodos privados** |

| Con estándar |
| ----- |
| `namespace Starfall.Gameplay.Enemies` `{    public class EnemyController    {        private const int MaximumHealth = 100;        private static int activeEnemyCount;        private int _currentHealth;        public int CurrentHealth { get; set; }        public EnemyController(EnemyWeapon weapon)        {        }        public event EventHandler Defeated;`         `public int GenerateAttackDamage()        {        }        protected virtual void OnCriticalHealth()        {        }        private void ApplyDamage(int damageAmount)        {        }    } }` |
| **Sin estándar** |
| `namespace Starfall.Gameplay.Enemies {     public class EnemyController     {         private void ApplyDamage(int damageAmount)         {         }              public int CurrentHealth => _currentHealth;                  private int _currentHealth;         public void TakeDamage(int damageAmount)         {         }         private const int MaximumHealth = 100;         protected virtual void OnCriticalHealth()         {         }         public EnemyController(EnemyWeapon weapon)         {         }         private static int activeEnemyCount;         public event EventHandler? Defeated;     } }` |

4. # Prácticas de programación {#prácticas-de-programación}

## 4.1. Modificadores de acceso {#4.1.-modificadores-de-acceso}

Aplicar el principio de **menor accesibilidad necesaria**: un tipo o miembro solo deberá ser accesible desde las partes del programa que realmente necesiten utilizarlo. (Microsoft, 2026a). 

Se deberá **escribir explícitamente el modificador de acceso** en tipos y miembros siempre que la sintaxis lo permita. Se exceptúan miembros cuya accesibilidad está determinada por el lenguaje.

| Con estándar |
| ----- |
| `namespace Starfall.Gameplay.Enemies {     public class EnemyController     {         private int _currentHealth;         public void TakeDamage(int damageAmount)         {         }         protected virtual void OnDefeated()         {         }         private void ApplyDamage(int damageAmount)         {         }         private void DropLoot()         {         }     } }` |
| **Sin estándar** |
| `namespace Starfall.Gameplay.Enemies {     public class EnemyController     {         public int _currentHealth;         public void ApplyDamage(int damageAmount)         {         }         public void DropLoot()         {         }         protected int CalculateInternalState()         {         }     } }` |

## 4.2. Encapsulamiento {#4.2.-encapsulamiento}

Los campos que representen el **estado interno** **de una clase** deberán mantenerse **privados (**`private`**)** siempre que sea posible. 

Cuando otra clase únicamente necesite **consultar** ese dato, se expondrá mediante una propiedad de sólo lectura, se utilizará `get` sin un `set` público. 

Si el valor puede modificarse pero únicamente desde la propia clase, se utilizará una propiedad con `private set` o se modificará mediante un método que represente la operación correspondiente.

Las propiedades deberán utilizar la **menor capacidad de modificación necesaria**. 

| Con estándar |
| ----- |
| `namespace Starfall.Gameplay.Player {     public class Player     {         private const int MinimumHealth = 0;         private int _currentHealth;         public int MaximumHealth { get; }     } }` |
| **Sin estándar** |
| `namespace Starfall.Gameplay.Player {     public class Player     {         public int CurrentHealth;         public int MaximumHealth;     } }` |

## 4.3. Herencia {#4.3.-herencia}

La herencia deberá utilizarse únicamente cuando exista una relación conceptual clara de tipo **“es un”** entre la clase derivada y la clase base.

Los métodos que deban permitir una implementación diferente en las clases derivadas deberán declararse `virtual` o `abstract` en la clase base y utilizar `override` en las clases derivadas. Un miembro no deberá marcarse `virtual` únicamente como medida preventiva.

**Abstract vs Virtual**

* Se utilizará `abstract` cuando todas las clases derivadas concretas deban proporcionar obligatoriamente su propia implementación.   
* Se utilizará `virtual` cuando la clase base proporcione un comportamiento válido por defecto, pero permita que una clase derivada lo sustituya cuando lo necesite (Microsoft, 2026a).  
  * Cuando una clase derivada sobrescriba un miembro virtual, deberá utilizar explícitamente `override`. 

| Con estándar |
| ----- |
| `namespace Starfall.Gameplay.Enemies {     public abstract class Enemy     {         public int CurrentHealth { get; private set; }         protected Enemy(int maximumHealth)         {         }              public virtual void UpdateBehavior()         {             Patrol();         }         public abstract void Attack(Player target);              protected void Patrol()         {         }     }     public sealed class RangedEnemy : Enemy     {         private readonly Weapon _weapon;         public RangedEnemy(int maximumHealth, Weapon weapon)             : base(maximumHealth)         {             _weapon = weapon;         }         public override void UpdateBehavior()         {             MaintainDistance();         }         public override void Attack(Player target)         {            _weapon.FireAt(target);         }         private void MaintainDistance()         {         }     } }` |
| **Sin estándar** |
| `namespace Starfall.Gameplay.Enemies {     public class Enemy     {         public int CurrentHealth;         public void Attack(Player target)         {         }         protected void SaveGame()         {         }         public virtual void CalculateScore()         {         }     }     public class RangedEnemy : Enemy     {         public void Attack(Player target)         {         }         public new void CalculateScore()         {         }         public void SaveGameData()         {         }     }     public class BossEnemy : RangedEnemy     {     }     public class FinalBossEnemy : BossEnemy     {     } }` |

## 4.4. Interfaces {#4.4.-interfaces}

Las interfaces se utilizarán cuando **varios tipos diferentes necesiten compartir una misma capacidad o comportamiento observable**, especialmente cuando dichos tipos no pertenezcan necesariamente a la misma jerarquía de herencia. (Microsoft, 2023a).

Cada interfaz deberá tener una **responsabilidad concreta** y agrupar únicamente miembros relacionados con esa capacidad.

No se utilizarán **interfaces vacías únicamente como marcadores**, para indicar que un objeto posee cierta característica (Microsoft, 2023a).

Se deberá procurar que toda interfaz creada para el proyecto tenga **al menos una implementación real y un lugar donde sea consumida**, por ejemplo, como parámetro, propiedad o dependencia.

| Con estándar |
| ----- |
| `namespace Starfall.Gameplay.Combat {     public interface IDamageable     {          bool IsAlive { get; }         void TakeDamage(int damageAmount);     }     public sealed class Player : IDamageable     {         public int CurrentHealth { get; private set; }         public bool IsAlive`         `{`             `get`             `{`                 `return CurrentHealth > 0;`             `}`         `}`          `public Player(int maximumHealth)         {             CurrentHealth = maximumHealth;         }         public void TakeDamage(int damageAmount)         {             if (damageAmount <= 0)             {                 return;             }             CurrentHealth = Math.Max(0, CurrentHealth - damageAmount);         }     } }` |
| **Sin estándar** |
| `namespace Starfall.Gameplay.Combat {     public interface Damageable     {     }     public interface IGameObject     {         void TakeDamage(int damageAmount);         void SaveGame();         void PlayMusic();         void OpenInventory();         void LoadLevel();     }     public sealed class DamageSystem     {          public void ApplyDamage(Player player, int damageAmount)          {             player.TakeDamage(damageAmount);          }     } }` |

## 4.5. Sobrescritura de métodos {#4.5.-sobrescritura-de-métodos}

Se utilizará `virtual` cuando la clase base proporcione una implementación válida por defecto, pero permita que las clases derivadas la sustituyan. Se utilizará `abstract` cuando la clase base no pueda proporcionar una implementación adecuada y cada clase derivada concreta deba definir obligatoriamente su propio comportamiento (Microsoft, 2026a).

El método declarado con `override` deberá mantener la **misma firma** que el miembro que sobrescribe. (Microsoft, 2026a).

Los métodos no deberán declararse `virtual` solamente porque en el futuro podrían llegar a necesitar una sobrescritura. 

| Con estándar |
| ----- |
| `namespace Starfall.Gameplay.Enemies {     public abstract class Enemy     {         public virtual void Move(Player target)         {             FollowTarget(target);         }             public abstract void Attack(Player target);         protected void FollowTarget(Player target)         {        }     }     public class MeleeEnemy : Enemy     {         public override void Attack(Player target)         {             target.TakeDamage(25);         }     }     public class FlyingEnemy : Enemy     {         public override void Move(Player target)         {             base.Move(target);             MaintainFlightHeight();         }         public override void Attack(Player target)         {            target.TakeDamage(15);         }         private void MaintainFlightHeight()         {         }     } }` |
| **Sin estándar** |
| `namespace Starfall.Gameplay.Enemies {     public class Enemy     {          public void Move(Player target)         {         }         public void Attack(Player target)         {         }     }     public class FlyingEnemy : Enemy     {         public new void Move(Player target)         {         }         public void Attack(Player enemyTarget)         {         }         public virtual void InternalFlightCalculation()         {         }     } }` |

## 4.6. Manejo de valores nulos {#4.6.-manejo-de-valores-nulos}

No se permite utilizar `return null;` para comunicar fallos de ejecución. Los errores técnicos no deberán ocultarse mediante retornos nulos; cuando una condición inválida impida completar la operación, deberá lanzarse una excepción específica.

Las consultas de colecciones sin resultados deberán devolver colecciones vacías. Los valores predeterminados únicamente podrán devolverse cuando tengan un significado válido y definido para la operación.

**Excepción para consultas de Entity Framework:** Se permite devolver `null` en métodos de acceso a datos que consulten una entidad individual mediante operaciones como `FirstOrDefault` o `SingleOrDefault`, exclusivamente para indicar que no existe una coincidencia. El consumidor deberá comprobar el resultado antes de utilizarlo. No se permite capturar errores de conexión, consulta o persistencia y convertirlos en un retorno nulo.

| Con estándar |
| :---- |
| `public Player GetById(int playerId) {     return _context.Player.FirstOrDefault(         player => player.IdPlayer == playerId); }` |
| **Sin estándar** |
| `public Player GetById(int playerId) {     try     {         return _context.Player.FirstOrDefault(             player => player.IdPlayer == playerId);     }     catch (Exception)     {         return null;     } }` |

## 4.7. Manejo de excepciones {#4.7.-manejo-de-excepciones}

Los errores deberán clasificarse y manejarse según su naturaleza y la capa donde exista contexto suficiente para resolverlos o transformarlos:

* **Prohibición de captura genérica y de enmascaramiento:** Queda estrictamente prohibido capturar la clase base `Exception` en bloques `catch`, así como dejar bloques `catch` vacíos o capturar excepciones únicamente para volver a lanzarlas sin realizar una acción concreta o registro en bitácora. Cuando se capture una excepción específica, la variable deberá nombrarse `ex`.  
* **Propagación:** Si una capa no cuenta con contexto para resolver una excepción, deberá permitir que se propague sin capturarla, o bien utilizar `throw;` (nunca `throw ex;`) para preservar el *stack trace* original.  
* **Excepciones estándar de .NET:** Para violaciones de contrato en argumentos se deberá utilizar la excepción estándar más específica aplicable:  
  * `ArgumentNullException(nameof(parametro))` cuando el valor sea nulo.  
  * `ArgumentOutOfRangeException(nameof(parametro))` cuando esté fuera del rango permitido.  
  * `ArgumentException` cuando el contenido sea inválido.   
  * Para estados inválidos del ciclo de vida del objeto se utilizarán `InvalidOperationException`, `ObjectDisposedException` o `UnauthorizedAccessException`.  
* **Excepciones de reglas de negocio (RuleException) y frontera WCF:** Las excepciones personalizadas se limitan a representar rechazos esperados de reglas de negocio en `BusinessLogic`. Deberán declararse como clases `sealed` que hereden de `Exception`, exponer únicamente una propiedad inmutable `Issue` del tipo enumerado `{Dominio}Issue` correspondiente y comprobar su validez en el constructor mediante `Enum.IsDefined`. Estas excepciones no deberán cruzar hacia el cliente; el servicio WCF deberá capturarlas y lanzar directamente `new FaultException<{Dominio}Issue>(ex.Issue)` sin métodos envoltorio intermedios.  
* **Errores de infraestructura y presentación en el cliente:** Los fallos técnicos de persistencia (`SqlException`, `EntityException`, `DbUpdateException`) deberán capturarse en la frontera del servicio WCF, registrarse con `LogError` y transformarse en un `FaultException` genérico con un mensaje técnico seguro (`"The service could not complete the operation."`), manteniendo `IncludeExceptionDetailInFaults = false`. En el cliente, nunca se deberá mostrar `Exception.Message` ni `FaultException.Message` al usuario; el `ViewModel` deberá clasificar el error y centralizar en un método la traducción hacia claves de recursos `.resx`.

| Con estándar |
| :---- |
| `public sealed class ChatRuleException : Exception` `{`     `public ChatIssue Issue { get; }`     `public ChatRuleException(ChatIssue issue)`     `{`         `if (!Enum.IsDefined(typeof(ChatIssue), issue))`         `{`             `throw new ArgumentOutOfRangeException(nameof(issue));`         `}`         `Issue = issue;`     `}` `}` `public Lobby JoinLobby(string lobbyCode)` `{`     `try`     `{`         `Player player = GetAuthenticatedPlayer();`         `Lobby lobby = controller.JoinLobby(player, lobbyCode);`         `publisher.PublishLobbyChanged(lobby);`         `return lobby;`     `}`     `catch (LobbyRuleException ex)`     `{`         `throw new FaultException<LobbyIssue>(ex.Issue);`     `}`     `catch (SqlException ex)`     `{`         `logger.LogError(ex, "Lobby operation could not access persistence.");`         `throw new FaultException("The service could not complete the operation.");`     `}` `}` |
| **Sin estándar** |
| `public class ChatRuleException : Exception` `{`     `public ChatIssue Issue { get; set; }` `}` `public Lobby JoinLobby(string lobbyCode)` `{`     `try`     `{`         `Player player = GetAuthenticatedPlayer();`         `return controller.JoinLobby(player, lobbyCode);`     `}`     `catch (LobbyRuleException ex)`     `{`         `throw LobbyFault(ex.Issue);`     `}`     `catch (Exception ex)`     `{`         `throw ex;`     `}` `}` `private static FaultException<LobbyIssue> LobbyFault(LobbyIssue issue)` `{`     `return new FaultException<LobbyIssue>(issue);` `}` |

## 4.8. Expresiones lambda y cuerpos de expresión {#4.8.-expresiones-lambda-y-cuerpos-de-expresión}

Se distingue entre miembros con cuerpo de expresión y expresiones lambda utilizadas como delegados:

* **Prohibición de miembros con cuerpo de expresión (=\>):** Queda prohibido utilizar el operador `=>` para definir el cuerpo de métodos, constructores, propiedades calculadas o descriptores de acceso (`get`, `set`, `add`, `remove`). Todo miembro deberá implementar su cuerpo mediante bloques explícitos con llaves `{ }` siguiendo el estilo Allman (Sección 3.1.4).  
* **Uso acotado de expresiones lambda:** Las expresiones lambda (`parametro => expresion`) deberán mantenerse simples y enfocadas en una sola expresión sin bloque de llaves, limitando su uso a argumentos de métodos que requieran delegados (`Action`, `Func`, `Predicate`), consultas de Entity Framework o inicialización de comandos y pruebas. Los parámetros deberán utilizar nombres descriptivos. Cuando la lógica requiera múltiples instrucciones, deberá extraerse a un método privado con nombre descriptivo o sustituirse por un bucle `foreach` explícito.

| Con estándar |
| ----- |
| `public bool HasPoints` `{`     `get`     `{`         `return CurrentRoundPoints > MinimumPointsThreshold;`     `}` `}` `List<Player> activePlayers = players`     `.Where(player => player.HasPoints)`     `.ToList();` |
| **Sin estándar** |
| `public bool HasPoints => CurrentRoundPoints > MinimumPointsThreshold;` `List<Enemy> activeEnemies = enemies`     `.Where(enemy =>`     `{`         `bool isAlive = enemy.Health > 0;`         `bool isEnabled = enemy.IsActive;`         `return isAlive && isEnabled;`     `})`     `.ToList();` |

## 4.9. Números y cadenas mágicas {#4.9.-números-y-cadenas-mágicas}

Se evitará el uso de “números mágicos”, que son números que solemos incluir en el código que por sí solos no nos dicen nada sobre qué representa dicho valor, necesitando de comentarios que lo tengan que explicar. En su lugar, se definirán como variables, campos o constantes con un nombre descriptivo siguiendo las normas de nombrado que le correspondan. 

| Con estándar |
| ----- |
| `private const int MaxInventorySlots = 20; private const float CriticalHitMultiplier = 1.5f; private const string DefaultPlayerName = "Player"; public bool CanAddItem(int currentItemCount) {     return currentItemCount < MaxInventorySlots; } public int CalculateCriticalDamage(int baseDamage) {     return (int)(baseDamage * CriticalHitMultiplier); }` |
| **Sin estándar** |
| `public bool CanAddItem(int currentItemCount) {     return currentItemCount < 20; }` |

## 4.10. Complejidad de métodos {#4.10.-complejidad-de-métodos}

Los métodos deberán mantenerse **enfocados en una responsabilidad concreta**. Un método que acumule demasiadas instrucciones, operaciones o responsabilidades deberá considerarse candidato a refactorización, especialmente cuando su tamaño dificulte comprender, probar o modificar su comportamiento. 

| Con estándar |
| ----- |
| `public void InitializeLevel(LevelData levelData) {     LoadEnvironment(levelData);     SpawnPlayer(levelData.PlayerSpawn);     SpawnEnemies(levelData.EnemySpawns);     ConfigureObjectives(levelData.Objectives); }` |
| **Sin estándar** |
| `public void InitializeLevel(LevelData levelData) {     LoadEnvironment(levelData);     SpawnPlayer(levelData.PlayerSpawn);     SpawnEnemies(levelData.EnemySpawns);     ConfigureObjectives(levelData.Objectives);     LoadMusic(levelData.Music);     ConfigureLighting(levelData.Lighting);     InitializeDoors(levelData.Doors);     InitializeTriggers(levelData.Triggers);     InitializeCollectibles(levelData.Collectibles);     ConfigureCheckpoints(levelData.Checkpoints);     LoadDialogue(levelData.Dialogue);     InitializeEnemies(levelData.Enemies);     ConfigureBoss(levelData.Boss);     InitializeParticles(levelData.Particles);     ConfigureWeather(levelData.Weather);     InitializeNavigation(levelData.Navigation);     LoadCutscenes(levelData.Cutscenes);     ConfigureCamera(levelData.Camera);     InitializeUI(levelData.Interface);     LoadTutorial(levelData.Tutorial);     InitializeSavePoint(levelData.SavePoint);     ConfigureObjectives(levelData.Objectives);     ValidateLevel(levelData);     StartLevelTimer();     ResetPlayerState();     ResetEnemyState();     ResetObjectives();     UpdateLoadingScreen(); }` |

## 4.11. Complejidad ciclomática {#4.11.-complejidad-ciclomática}

El nivel de complejidad ciclomática permitida en los métodos es de máximo 10\.

| Con estándar |
| ----- |
| `public void UpdateEnemy(Enemy enemy) {     if (!enemy.IsActive)     {         return;     }     if (enemy.Health <= 0)     {         enemy.Die();         return;     }     if (enemy.DistanceToPlayer < 10)     {         enemy.Attack();     }     else if (enemy.DistanceToPlayer < 30)     {         enemy.ChasePlayer();     }     else     {         enemy.Patrol();     }     if (enemy.IsStunned)     {         enemy.StopMovement();     }     if (enemy.HasTarget)     {         enemy.UpdateTarget();     } }` |
| **Sin estándar** |
| `public void UpdateEnemy(Enemy enemy) {     if (!enemy.IsActive)     {         return;     }     if (enemy.Health <= 0)     {         enemy.Die();         return;     }     if (enemy.IsStunned)     {         enemy.StopMovement();     }     else if (enemy.IsFrozen)     {         enemy.StopMovement();     }     else if (enemy.IsSlowed)     {         enemy.ReduceMovementSpeed();     }     if (enemy.DistanceToPlayer < 10)     {         if (enemy.HasWeapon)         {             if (enemy.CanAttack)             {                 enemy.Attack();             }             else             {                 enemy.Wait();             }         }         else         {             enemy.ChasePlayer();         }     }     else if (enemy.DistanceToPlayer < 30)     {         if (enemy.HasTarget)         {             enemy.ChasePlayer();         }         else         {             enemy.SearchForTarget();         }     }     else     {         if (enemy.HasPatrolRoute)         {             enemy.Patrol();         }         else         {             enemy.Idle();         }     } }` |

## 4.12. Número de parámetros {#4.12.-número-de-parámetros}

El número máximo de parámetros permitidos para un método es de 4\. 

* Esta regla no aplica para métodos propios del framework de Microsoft, debido a que su firma no se puede alterar, sólo únicamente a métodos creados por los miembros del equipo.  
* También, se exentan del límite de parámetros los constructores destinados a la inyección de dependencias de servicios del sistema o controladores de UI.

| Con estándar |
| ----- |
| `public void SpawnEnemy(Vector3 position, EnemyType enemyType, int level) { }` |
| **Sin estándar** |
| `public void SpawnEnemy(int xPosition, int yPosition, EnemyType enemyType, int level, bool isActive, string name)` `{`  `}` |

## 4.13. Expresiones lógicas {#4.13.-expresiones-lógicas}

Las expresiones lógicas deberán escribirse de manera que su resultado pueda comprenderse fácilmente, evitando condiciones innecesariamente complejas o difíciles de interpretar. Las expresiones deberán utilizar operadores lógicos de forma consistente y, cuando una condición sea demasiado extensa, deberá dividirse en expresiones más pequeñas y descriptivas.

Cuando se hagan expresiones con valores booleanos se permite que no se incluya la comparación con True o False, siempre que el tipo de dato y la sintaxis del lenguaje lo permitan, ya que puede ser redundante por el tipo de dato que es.

| Con estándar |
| ----- |
| `if (player.IsAlive && player.HasWeapon) {     player.Attack(); }` |
| **Sin estándar** |
| `if (player.IsAlive == true && player.HasWeapon == true) {     player.Attack(); }` |

## 4.14. Registro de eventos y errores (Logs) {#4.14.-registro-de-eventos-y-errores-(logs)}

El proyecto deberá utilizar la abstracción `ILogger<T>` de `Microsoft.Extensions.Logging` (donde `T` corresponde al tipo de la clase que registra el evento), obtenida de manera centralizada mediante `LoggerFactoryProvider.CreateLogger<T>()` sobre Serilog.

* **Formato del mensaje y orientación al evento:** Los mensajes deberán escribirse en **inglés**, iniciar con letra mayúscula, finalizar con punto (**.**) y describir el evento o resultado ocurrido en el sistema, evitando escribir únicamente el nombre del método ejecutado.  
* **Structured logging y nombrado de placeholders:** Queda prohibido concatenar (`+`) o interpolar (`$""`) valores variables dentro del mensaje. Se deberán utilizar plantillas constantes con marcadores de posición en **PascalCase** que conserven exactamente el mismo nombre de la propiedad, atributo o concepto representado (por ejemplo, `{PlayerId}`, `{LobbyId}`, `{AccountIssue}`, `{ChatIssue}`), evitando marcadores genéricos como `{Id}`, `{Issue}`, `{Data}` o `{Error}`.  
* **Registro de excepciones y no duplicación:** Cuando exista una excepción capturada, la instancia `ex` deberá pasarse como primer argumento del método del logger (`_logger.LogWarning(ex, ...)` o `_logger.LogError(ex, ...)`). Cada excepción deberá registrarse únicamente en la capa donde sea manejada o transformada, evitando registrarla repetidamente en cada capa intermedia.  
* **Protección de información sensible:** Nunca deberán registrarse contraseñas, `sessionKey`, códigos de verificación, contenido de mensajes de chat, datos binarios de imágenes (`byte[]`), correos electrónicos personales ni objetos de dominio completos.

| Con estándar |
| ----- |
| `private readonly ILogger<LobbyEventPublisher> _logger =`     `LoggerFactoryProvider.CreateLogger<LobbyEventPublisher>();` `_logger.LogInformation(`     `"Player {PlayerId} joined lobby {LobbyId}.",`     `player.PlayerId,`     `lobby.LobbyId);` |
| **Sin estándar** |
| `private readonly ILogger<object> _logger = new ConsoleLogger<object>();` `_logger.LogInformation(`     `$"Player {player.PlayerId} joined lobby {lobby.LobbyId}");` |

### 4.14.1 Categorización de errores {#4.14.1-categorización-de-errores}

Cada registro deberá utilizar uno de los niveles de severidad establecidos por `LogLevel`: **Trace, Debug, Information, Warning, Error o Critical** (reflejado como `Fatal` / `FTL` en la salida de Serilog).

| Trace | Información extremadamente detallada sobre la ejecución interna. Se utilizará únicamente cuando sea necesario investigar un comportamiento específico y no deberá mantenerse habilitado normalmente. |
| :---: | :---- |

| Con estándar |
| ----- |
| `_logger.LogTrace(     "Updating enemy {EnemyId} at position {Position}.",`     `enemy.Id,     enemy.Position);` |
| **Sin estándar** |
| `_logger.LogTrace(     "Level {LevelName} started successfully",     levelName);` |

| Debug | Situaciones anómalas recuperables (como fallos de callback o cierre anómalo de un canal WCF) o rechazos esperados por reglas de negocio (`FaultException<TIssue>`) que no impiden que el sistema continúe funcionando. |
| :---: | :---- |

| Con estándar |
| ----- |
| `_logger.LogDebug(     "Player {PlayerId} selected tile [{LeftPip}|{RightPip}] from hand.",`     `player.Id,      selectedTile.LeftPip,      selectedTile.RightPip);` |
| **Sin estándar** |
| `_logger.LogDebug(     "Could not open save file for player {PlayerId}",      player.Id);` |

| Information | Eventos normales y relevantes del flujo de ejecución que puedan ser útiles posteriormente para conocer qué ocurrió durante una sesión. |
| :---- | :---- |

| Con estándar |
| ----- |
| `_logger.LogInformation(     "Round {RoundNumber} started for room {RoomId} with {PlayerCount} players.",`     `roundNumber,      roomId,      players.Count);` |
| **Sin estándar** |
| `Console.WriteLine("Round started");` |

| Warning | Situaciones anómalas o inesperadas que no impiden que el videojuego continúe funcionando correctamente. |
| :---- | :---- |

| Con estándar |
| ----- |
| `_logger.LogWarning(     "Turn timeout reached for player {PlayerId}. Auto-passing turn.",      player.Id);` |
| **Sin estándar** |
| `_logger.LogError(     "Player {PlayerId} took too long to play, passing turn automatically",      player.Id);` |

| Error | Fallos que impiden completar correctamente una operación o que provocan una excepción que no pudo ser manejada adecuadamente. |
| :---- | :---- |

| Con estándar |
| ----- |
| `_logger.LogError(     ex,      "Failed to send turn state to player {PlayerId} in room {RoomId}.",`     `player.Id,      roomId);` |
| **Sin estándar** |
| `_logger.LogError(     "Failed to send turn state for player " + player.Id);` |

| Critical | Fallos graves que impiden que un componente esencial o la aplicación completa continúe en ejecución. |
| :---- | :---- |

| Con estándar |
| ----- |
| `_logger.LogCritical(`     `ex,`     `"Server host could not start essential WCF services.");` |
| **Sin estándar** |
| `_logger.LogCritical(`     `ex,`     `"Player {PlayerId} could not join the lobby.",`     `playerId);` |

## 4.15 Inicialización y asignación {#4.15-inicialización-y-asignación}

Se permite inicializar campos en su declaración únicamente para valores predeterminados estáticos, constantes, o instancias simples que no dependen de ningún parámetro externo ni de lógica de cómputo. 

Fuera de esas excepciones, la inicialización se **debe** realizar en el constructor siempre que el valor inicial del campo dependa de parámetros pasados al instanciar la clase, requiera validaciones de entrada, o dependa de la inyección de dependencias.

| Con estándar |
| ----- |
| `public class DominoPlayer` `{`     `private readonly List<Tile> _hand = new List<Tile>();`     `private readonly ILogger<DominoPlayer> _logger;`     `private int _currentRoundScore;`     `public string PlayerName { get; }`     `public DominoPlayer(string playerName, ILogger<DominoPlayer> logger)`     `{`         `if (string.IsNullOrWhiteSpace(playerName))`         `{`             `throw new ArgumentException("The player name cannot be empty.", nameof(playerName));`         `}`         `PlayerName = playerName;`         `_logger = logger ?? throw new ArgumentNullException(nameof(logger));`         `_currentRoundScore = 0;`     `}` `}` |
| **Sin estándar** |
| `public class DominoPlayer` `{`     `private List<Tile> _hand;`     `private string _playerName = "DefaultPlayer";`     `public DominoPlayer(string playerName)`     `{`     `}`     `public void StartHand()`     `{`         `_hand = new List<Tile>();`     `}` `}` |

## 

## 4.16 Composición de textos e Internacionalización  {#4.16-composición-de-textos-e-internacionalización}

Se debe evitar formar oraciones o mensajes en la interfaz concatenando fragmentos de texto fijo con variables dinámicas. Las traducciones deben ser oraciones completas almacenadas en el diccionario de recursos que incluyan **marcadores de posición numéricos** para inyectar las variables. Esto garantiza que la traducción mantenga la gramática, conjugación y el orden de palabras correcto del idioma destino (Microsoft, 2022).

| Con estándar |
| ----- |
| `Welcome, {0}! You have {1} points.  ¡Bienvenido, {0}! Tienes {1} puntos.`  |
| **Sin estándar** |
| `lblWelcome.Text = "Welcome " + playerName + "! You have " + points + " points.";`  |

## 4.17 Uso de las variables de tipo local {#4.17-uso-de-las-variables-de-tipo-local}

Las variables locales deberán declararse especificando siempre su **tipo de dato explícito**, prohibiéndose el uso de la palabra clave `var`. Declarar el tipo explícitamente permite identificar de forma inmediata el contrato de cada variable al leer o revisar el código sin depender de las ayudas contextuales del entorno de desarrollo.

| Con estándar |
| ----- |
| `Player player = new Player();` `int totalScore = CalculateAllFivesBonus(board);` |
| **Sin estándar** |
| `var player = new Player();` `var totalScore = CalculateAllFivesBonus(board);` |

## 4.18 Consultas de datos con LINQ {#4.18-consultas-de-datos-con-linq}

Las consultas LINQ deberán escribirse utilizando la **sintaxis de métodos (Fluent syntax)** mediante expresiones lambda simples (Microsoft, 2026a). 

Cuando una consulta contenga múltiples operaciones encadenadas o su extensión dificulte la lectura, cada operación deberá colocarse en una línea independiente. (Microsoft, 2026a; SonarSource, s. f.). Las expresiones lambda cuya lógica sea compleja deberán extraerse a métodos con nombres descriptivos para reducir la complejidad cognitiva (SonarSource, s. f.). 

| Con estándar |
| ----- |
| `List<Player> ranking = players`     `.Where(player => player.GamesWon > 0)     .OrderByDescending(player => player.GamesWon)     .Take(10)     .ToList();` |
| **Sin estándar** |
| `var ranking = (from p in players                where p.GamesWon > 0                orderby p.GamesWon descending select p).Take(10).ToList();` |

## 4.19 Uso de enumeraciones {#4.19-uso-de-enumeraciones}

Las enumeraciones deberán utilizarse exclusivamente para representar conjuntos cerrados de estados de dominio, opciones tipificadas o diccionarios de errores de negocio.

| Con estándar | Sin estándar |
| ----- | ----- |
| `public enum AccountIssue {     InvalidadCredentials,     IsBanned,     IsSuspended }` | `public enum Player {     Name,     Email,     ProfilePicture }` |

## 

## 4.20 Enfoque orientado a objetos {#4.20-enfoque-orientado-a-objetos}

Cada clase deberá representar una responsabilidad concreta y cohesiva. Su nombre deberá comunicar su propósito dentro del sistema, ya sea como entidad, objeto de valor, servicio, controlador, modelo de presentación, resultado de operación u otro componente justificado. No deberán agruparse responsabilidades ajenas entre sí en una misma clase. 

| Con estándar | Sin estándar |
| ----- | ----- |
| `public class PlayerAccount { }` | `public class AccountResult { }` |

## 4.21 Manejo de concurrencia y sincronización {#4.21-manejo-de-concurrencia-y-sincronización}

El control de concurrencia deberá separar las responsabilidades del servidor y del cliente WPF sin mezclar mecanismos de sincronización:

* **Concurrencia en servicios WCF dúplex:** Las clases de servicio dúplex deberán configurar `[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant, IncludeExceptionDetailInFaults = false)]`. Queda prohibido utilizar la instrucción `lock` dentro de clases de servicio WCF, controladores (`Controllers`), repositorios (`Repositories`) y `ViewModels`.  
* **Estado compartido en memoria (lock):** Cuando múltiples sesiones accedan a colecciones o estructuras relacionadas en un `Manager`, las operaciones compuestas de validación y modificación deberán protegerse mediante un único objeto `private readonly object _syncRoot = new object();` y bloques `lock (_syncRoot)`. Nunca deberán invocarse callbacks ni operaciones de red WCF mientras se mantenga adquirido el bloqueo. Para evitar modificaciones externas concurrentes sobre el estado interno sin degradar el desempeño, se deberá evaluar el retorno de vistas de solo lectura (`IReadOnlyList<T>`), proyecciones o copias defensivas (*snapshots*) según el tamaño de los datos y el contexto de la operación.  
* **Colecciones concurrentes:** Cuando el estado compartido consista únicamente en asociaciones independientes por clave (como `*CallbackRegistry`), se utilizará `ConcurrentDictionary<TKey, TValue>` sin bloques `lock` adicionales.  
* **Ejecución en segundo plano en el cliente WPF:** No se utilizarán `async`, `await` ni `Task.Run` para invocar operaciones remotas. Toda operación susceptible de bloquear el hilo de interfaz gráfica desde un `ViewModel` deberá ejecutarse mediante `BackgroundJob<TState, TResult>` y `BackgroundWork.Start(job)`, actualizando las propiedades de la vista únicamente en los delegados de finalización despachados a través del `Dispatcher`.

| Con estándar |
| ----- |
| `public Lobby LeavePlayer(int playerId)` `{`     `lock (_syncRoot)`     `{`         `Lobby lobby = GetPlayerLobbyInternal(playerId);`         `lobby.Participants.Remove(FindPlayer(lobby.Participants, playerId));`         `_playerLobbyCodes.Remove(playerId);`         `return CreateSnapshot(lobby);`     `}` `}` `BackgroundJob<string, Lobby> job = new BackgroundJob<string, Lobby>(`     `code => _lobbyService.JoinLobby(code),`     `roomCode,`     `OnLobbyReady,`     `OnLobbyFailed);` `BackgroundWork.Start(job);` |
| **Sin estándar** |
| `public void LeavePlayerAndNotify(int playerId)` `{`     `lock (_syncRoot)`     `{`         `Lobby lobby = _lobbies[playerId];`         `lobby.Participants.RemoveAll(player => player.PlayerId == playerId);`         `_publisher.PublishLobbyChanged(lobby);`     `}` `}` `public async Task JoinRoomAsync(string roomCode)` `{`     `Lobby lobby = await Task.Run(() => _lobbyService.JoinLobby(roomCode));`     `ApplyLobby(lobby);` `}` |

## 4.22 Comunicación remota con WCF {#4.22-comunicación-remota-con-wcf}

La definición de contratos y el manejo de canales WCF deberán cumplir las siguientes reglas:

* **Serialización explícita:** Toda clase de modelo transferida por WCF y toda enumeración de contrato deberá decorarse con `[DataContract]`, marcando las propiedades transferibles con `[DataMember]` simple (sin acoplamiento posicional `Order` ni `IsRequired` cuando la validación se realice en dominio) y cada miembro de enumeración con `[EnumMember]`.  
* **Contratos dúplex y callbacks:** Todo `[ServiceContract]` que declare un `CallbackContract` deberá configurar `SessionMode = SessionMode.Required`. Las operaciones del `CallbackContract` deberán declararse con `[OperationContract(IsOneWay = true)]` y retornar `void`. Cuando una operación del servicio difunda su resultado a los participantes mediante un `EventPublisher`, dicha operación deberá retornar `void` para evitar duplicidad con la notificación del callback.  
* **Ciclo de vida de proxies y separación en el cliente:** En operaciones *request/response*, el proxy deberá crearse, utilizarse y cerrarse por operación mediante `WcfInvoker`, sin conservarse como estado permanente. En comunicación dúplex deberán mantenerse separadas las responsabilidades de `*Client`, `*CallbackHandler`, `*CallbackRegistry` y `*EventPublisher`.  
* **Cierre seguro de canales:** Al cerrar un canal WCF, si su estado es `CommunicationState.Faulted`, deberá invocarse `Abort()`. Si al invocar `Close()` se produce `CommunicationException` o `TimeoutException`, deberá capturarse la excepción específica, registrarse con `LogWarning` cuando aplique y abortarse el canal mediante `Abort()`.

| Con estándar |
| ----- |
| `[DataContract]` `public sealed class ChatMessage` `{`     `[DataMember]`     `public int ContextId { get; set; }`     `[DataMember]`     `public ChatScope Scope { get; set; }`     `[DataMember]`     `public int SenderId { get; set; }`     `[DataMember]`     `public string SenderUsername { get; set; }`     `[DataMember]`     `public string Content { get; set; }` `}` `[ServiceContract(`     `CallbackContract = typeof(IChatCallback),`     `SessionMode = SessionMode.Required)]` `public interface IChatService` `{`     `[OperationContract]`     `[FaultContract(typeof(AccessIssue))]`     `void OpenChatSession(string sessionKey);`     `[OperationContract]`     `[FaultContract(typeof(AccessIssue))]`     `[FaultContract(typeof(ChatIssue))]`     `void SendMessage(ChatMessage message);` `}` |
| **Sin estándar** |
| `[DataContract]` `public class ChatMessage` `{`     `[DataMember(IsRequired = true, Order = 1)]`     `public int ContextId { get; set; }`     `public string Content { get; set; }` `}` `[ServiceContract(CallbackContract = typeof(IChatCallback))]` `public interface IChatService` `{`     `[OperationContract]`     `ChatMessage SendMessage(ChatMessage message);` `}` |

5. # Comentarios y documentación {#comentarios-y-documentación}

## 5.1. Comentarios de implementación {#5.1.-comentarios-de-implementación}

Los comentarios de implementación deberán utilizarse para explicar **decisiones, comportamientos o condiciones del código que no sean evidentes a partir de su implementación**. No deberán utilizarse para describir literalmente lo que el código ya expresa de manera clara. (Microsoft, 2025).

Los comentarios deberán mantenerse **breves y claros**. 

Los comentarios deberán colocarse en una **línea independiente del código** y deberán utilizar el delimitador `//` para comentarios de una sola línea o bloques `/* */` para comentarios de más de una línea.  No se permite escribir comentarios al final de una línea de código.

| Con estándar |
| :---- |
| `public void UpdateEnemyPosition(float deltaTime) {     // Preserve the previous position to calculate the movement delta.     Vector3 previousPosition = _position;     _position += _velocity * deltaTime;     _movementDelta = _position - previousPosition; }` |
| **Sin estándar** |
| `public void UpdateEnemyPosition(float deltaTime) {     // Create a variable called previousPosition.     Vector3 previousPosition = _position;     _position += _velocity * deltaTime;   _movementDeltaa = _position - previousPosition; //Calculate the movement delta. }` |

## 5.2. Comentarios de una sola línea {#5.2.-comentarios-de-una-sola-línea}

Los comentarios de una sola línea deberán utilizar la sintaxis `//` y se emplearán para explicaciones breves. 

El texto del comentario deberá comenzar después de un espacio respecto al delimitador `//` y deberá colocarse en una línea independiente del código. (Microsoft, 2025). 

| Con estándar |
| :---- |
| `// Restore the player to the latest checkpoint.` |
| **Sin estándar** |
| `/*restore the player to the latest checkpoint*/` |

## 5.3. Comentarios en bloque {#5.3.-comentarios-en-bloque}

Los comentarios en bloque deberán utilizar los delimitadores **/\*** y **\*/**. (Microsoft, 2025).

Cuando sea necesario utilizar un comentario en bloque, el delimitador de apertura deberá colocarse en una línea independiente y el contenido deberá mantener una indentación consistente con el código al que pertenece. El delimitador de cierre deberá colocarse en una línea independiente. 

| Con estándar |
| :---- |
| `/*  * The following calculation uses the previous frame  * to preserve consistent movement during the transition.  */ _movementDelta = _currentPosition - _previousPosition;` |
| **Sin estándar** |
| `/* _movementDelta = _currentPosition - _previousPosition; _previousPosition = _currentPosition; UpdateAnimation(); */` |

## 5.4. Comentarios de documentación XML {#5.4.-comentarios-de-documentación-xml}

Los comentarios de documentación XML deberán utilizarse para documentar **clases, interfaces y métodos públicos de la capa de servicios de WCF,** ya que estos se pueden representar como la parte pública del proyecto**,** pero no se limita a estos y en caso de necesitarlo puede utilizarse para partes relevantes del código. Deberán escribirse utilizando el delimitador `///` y colocarse inmediatamente antes del elemento que documentan (Microsoft, 2026a).

No será necesario incluir comentarios de documentación en los siguientes archivos:

* Archivos code-behind de XAML, ya que son archivos vacíos sin miembros públicos adicionales.   
* Archivos autogenerados, además de que no representan la interfaz pública del proyecto desarrollada por el equipo, al ser autogenerados no es conveniente que se añadan los comentarios por cuestiones de tiempo, además que se perderían.

Los comentarios XML deberán contener **inglés**, al igual que el código fuente del proyecto. (Microsoft, 2026a). Las etiquetas que se pueden utilizar para los comentarios de documentación y una breve explicación de su uso son:

| ETIQUETAS |  |
| :---: | :---- |
| **\<summary\>** | Usar para describir de manera breve y concisa la finalidad, función o propósito del tipo o miembro. Evitar repetir simplemente su nombre o la información que ya resulta evidente por su declaración (Microsoft, 2026b). |
| **\<param\> y \<returns\>** | Utilizarse para métodos con parámetros o valor de retorno relevantes. La documentación de los parámetros deberá utilizar el **mismo nombre definido en la declaración del método** (Microsoft, 2026a) |
| **\<remarks\>** | Utilizarse **únicamente** cuando sea necesario proporcionar **información adicional** que no pueda incluirse adecuadamente en `<summary>`, como: restricciones de uso, comportamiento particular o consideraciones importantes para los integrantes del equipo. |
| **\<exception\>** | Utilizarse en los métodos públicos para documentar las excepciones que el método puede lanzar durante su ejecución, especificando el tipo de excepción y la condición que la provoca (Microsoft, 2026).  |

| Con estándar |
| :---- |
| `/// <summary> /// Represents the player character and its current state. /// </summary> public class Player {     /// <summary>     /// Applies damage to the player.     /// </summary>     /// <param name="damageAmount">The amount of health to remove.</param>     public void TakeDamage(int damageAmount)     {     } }` |
| **Sin estándar** |
| `// This is the player class. public class Player {     // Takes damage.     public void TakeDamage(int damageAmount)     {     } }` |

## 5.5. Comentarios TODO {#5.5.-comentarios-todo}

Los comentarios `TODO` podrán utilizarse para **identificar trabajo pendiente que deba realizarse posteriormente**. 

El identificador `TODO` deberá escribirse en **mayúsculas**, seguido de dos puntos y una descripción breve pero puntual acerca de la característica que faltante.

El comentario debe seguir lo establecido en la sección [5.2. Comentarios de una sola línea](#5.2.-comentarios-de-una-sola-línea)

Los comentarios `TODO` deberán eliminarse una vez que la tarea correspondiente haya sido completada. 

| Con estándar |
| :---- |
| `// TODO: Add support for loading player settings from the save file.` |
| **Sin estándar** |
| `// todo maybe improve this later.` |

## 5.6. Comentarios FIXME {#5.6.-comentarios-fixme}

Los comentarios `FIXME` podrán utilizarse para **identificar código que presenta un problema conocido y que requiere corrección**. 

El identificador `FIXME` deberá escribirse en **mayúsculas**, seguido de dos puntos y una descripción breve y concreta que indique **qué problema debe corregirse** . 

El comentario debe seguir lo establecido en la sección [5.2. Comentarios de una sola línea](#5.2.-comentarios-de-una-sola-línea).

Los comentarios `FIXME` deberán eliminarse una vez corregido el problema que identifican.

| Con estándar |
| :---- |
| `// FIXME: This calculation produces incorrect damage values for critical hits.` |
| **Sin estándar** |
| `// FIXME: This is bad.` |

## 5.7 Comentarios en archivos XAML {#5.7-comentarios-en-archivos-xaml}

Se evitará el uso de comentarios en archivos XAML, ya que estos serán utilizados para las pantallas de interfaz gráfica de WPF. Cuando sí se permite el uso de comentarios en archivos de XAML es cuando los archivos sean de **estilos globales, recursos compartidos o plantillas,** y se permite para indicar su finalidad o hacer más fácil su lectura. 

| Con estándar |
| :---- |
| `<Grid.ColumnDefinitions>     <ColumnDefinition Width="100"/>     <ColumnDefinition Width="*"/>     <ColumnDefinition Width="310"/> </Grid.ColumnDefinitions>` |
| **Sin estándar** |
| `<Grid.ColumnDefinitions> <!-- This has 3 columns --!>     <ColumnDefinition Width="100"/>     <ColumnDefinition Width="*"/>     <ColumnDefinition Width="310"/> </Grid.ColumnDefinitions>` |

## 5.8 Excepciones en archivos autogenerados {#5.8-excepciones-en-archivos-autogenerados}

Los archivos de código fuente C\# generados de manera automática por herramientas del entorno (por ejemplo, diseñadores de datos `.edmx`, plantillas T4, referencias de servicio WCF o Entity Framework) que incluyan el encabezado `<auto-generated>` quedan exentos de la regla del idioma en los comentarios y su revisión, debido a que seria esfuerzo gastado ya que estos archivos pueden sobreescribirse y volver a generar los comentarios.

6. # Estructuras de control {#estructuras-de-control}

## 6.1. Sentencias condicionales {#6.1.-sentencias-condicionales}

Cuando varias condiciones representen alternativas mutuamente excluyentes de una misma decisión, se utilizará una cadena `if`–`else if`–`else`, evitando varios `if` independientes. No será obligatorio incluir un `else` final cuando no exista una acción alternativa. Esta regla no aplica a validaciones independientes ni a cláusulas de guarda que finalicen el flujo mediante `return` o `throw`. 

Cuando las condiciones comiencen a producir **numerosos niveles de anidamiento**, se deberá considerar simplificar la lógica mediante retornos tempranos, métodos auxiliares o una estructura `switch`.

Las condiciones deberán expresar claramente la decisión que se está tomando y se evitará repetir comparaciones innecesarias.

Los bloques deberán utilizar siempre llaves conforme a la sección 3.1.4

| Con estándar |
| :---- |
| `if (!player.IsAlive) {     EndGame(); } else if (player.Health <= criticalHealthThreshold) {     ActivateLowHealthEffect(); } else {     ContinueGame(); }` |
| **Sin estándar** |
| `if (player.IsAlive == false)     EndGame(); if (player.Health <= criticalHealthThreshold)     ActivateLowHealthEffect(); if (player.IsAlive == true && player.Health > criticalHealthThreshold)     ContinueGame();` |

## 6.2. Bucles {#6.2.-bucles}

Para los bucles su uso se delimita de la siguiente manera (Microsoft, 2026c).:

| foreach | Cuando únicamente sea necesario recorrer los elementos de una colección |
| :---: | :---- |

| Con estándar |
| ----- |
| `foreach (Enemy enemy in activeEnemies) {     enemy.UpdateBehavior(); }` |
| **Sin estándar** |
| `for (int enemyIndex = 0; enemyIndex < activeEnemies.Count; enemyIndex++)     activeEnemies[enemyIndex].UpdateBehavior();` |

| for | Cuando sea necesario trabajar explícitamente con la posición o índice de los elementos.  |
| :---: | :---- |

| Con estándar |
| ----- |
| `for (int enemyIndex = 0; enemyIndex < activeEnemies.Count; enemyIndex++) {     activeEnemies[enemyIndex].SetSpawnOrder(enemyIndex); }` |
| **Sin estándar** |
| `for(int i=0;i<activeTiles.Count;i++) {     activeTiles[i].UpdateState(); }` |

| while | Cuando la repetición dependa principalmente de una condición y sea posible que el bloque no tenga que ejecutarse ninguna vez. |
| :---: | :---- |

| Con estándar |
| ----- |
| `while (remainingEnemies > 0) {     DefeatNextEnemy();     remainingEnemies--; }` |
| **Sin estándar** |
| `while (remainingEnemies > 0) {     DefeatNextEnemy(); }` |

| do-while | Cuando el bloque necesite ejecutarse al menos una vez. |
| :---: | :---- |

| Con estándar |
| ----- |
| `do {     selectedDifficulty = ReadDifficulty(); } while (!IsValidDifficulty(selectedDifficulty));` |
| **Sin estándar** |
| `do     selectedDifficulty = ReadDifficulty(); while (!IsValidDifficulty(selectedDifficulty));` |

## 

## 6.3. Sentencias switch {#6.3.-sentencias-switch}

Se preferirá `switch` frente a cadenas extensas de `if`–`else if` (Microsoft, 2026a).

**Sí** es posible agrupar varias etiquetas `case` consecutivas para que compartan el mismo comportamiento (Microsoft, 2025).

Se utilizará `default` cuando exista la posibilidad de recibir un valor no contemplado por los demás casos y el programa necesite definir cómo responder ante él. No se considerará obligatorio agregar un `default` artificial cuando todos los valores posibles estén controlados deliberadamente.

| Con estándar |
| :---- |
| `switch (enemyState)` `{`     `case EnemyState.Idle:`         `StayIdle();`         `break;`     `case EnemyState.Patrol:`         `PatrolArea();`         `break;`     `case EnemyState.Attack:`         `AttackPlayer();`         `break;`     `default:`         `ReturnToSpawn();`         `break;` `}` |
| **Sin estándar** |
| `if (enemyState == EnemyState.Idle)` `{`     `StayIdle();` `}` `if (enemyState == EnemyState.Patrol)` `{`     `PatrolArea();` `}` `if (enemyState == EnemyState.Attack)` `{`     `AttackPlayer();` `}` |

## 6.4. Operador ternario {#6.4.-operador-ternario}

Podrá utilizarse para **seleccionar uno de dos valores mediante una condición**, siempre que **ambas alternativas sean simples y la expresión resulte fácil de comprender.** Esto incluye la selección de objetos, implementaciones de servicios y delegados durante la construcción o configuración de dependencias..

No deberá utilizarse cuando las alternativas requieran varias instrucciones, produzcan efectos secundarios difíciles de reconocer o contengan lógica extensa. Se evitarán los ternarios anidados cuando dificulten la lectura; en esos casos se utilizará una estructura condicional o un método auxiliar.

Las expresiones podrán distribuirse en varias líneas para mejorar su legibilidad.

| Con estándar |
| :---- |
| `string playerStatus = player.IsAlive ? "Alive" : "Defeated"; string interactionText = hasKey ? "Open Door" : "Key Required"; _canExecute = canExecute != null     ? new Predicate<object>(parameter => canExecute())     : null;` |
| **Sin estándar** |
| `string playerStatus = player.IsAlive     ? player.Health > criticalHealth         ? "Healthy"         : "Critical"     : "Defeated"; int movementSpeed = isRunning     ? hasSpeedBoost         ? boostedRunningSpeed         : runningSpeed     : walkingSpeed;` |

## 6.5. Operador de coalescencia nula {#6.5.-operador-de-coalescencia-nula}

El operador `??` podrá utilizarse para proporcionar un valor alternativo cuando una expresión sea `null`, siempre que dicho valor sea válido para la operación y no oculte errores.

En constructores y métodos que reciban dependencias obligatorias, podrá combinarse con una expresión `throw` para validar y asignar la dependencia. Se utilizará `ArgumentNullException` con `nameof` para identificar el parámetro inválido.

No deberá utilizarse para sustituir silenciosamente una dependencia obligatoria ni para asignar valores predeterminados que oculten datos inválidos.

| Con estándar |
| :---- |
| `_accountRepository = accountRepository          ?? throw new ArgumentNullException(nameof(accountRepository));`  |
| **Sin estándar** |
| `_accountRepository = accountRepository          ?? new AccountRepository();` |

## 6.6. Control de flujo {#6.6.-control-de-flujo}

Deberán utilizarse de forma que resulte evidente **qué parte del programa se abandona o continúa**. (Microsoft, 2026f)

| break | Se utilizará para terminar inmediatamente el ciclo o `switch` más cercano cuando ya no sea necesario continuar su ejecución.  |
| :---: | :---- |

| Con estándar |
| ----- |
| `public void UpdateEnemies() {     foreach (Enemy enemy in activeEnemies)     {         if (enemy.HasReachedGoal)         {             EndWave();             break;         }         enemy.UpdateBehavior();     } }` |
| **Sin estándar** |
| `foreach (Tile tile in playerHand) {     if (board.IsValidMove(tile))     {         hasPlayableTile = true;         goto EndSearch;     } } EndSearch: ;` |

| continue | Se utilizará dentro de un ciclo cuando la iteración actual ya no necesite ejecutar las instrucciones restantes y desee comenzar la siguiente iteración. Si numerosas condiciones lo requieren, evaluar la posibilidad de simplificar el ciclo. |
| :---: | :---- |

| Con estándar |
| ----- |
| `foreach (Tile tile in boardTiles) {     if (!tile.IsDouble)     {         continue;     }     totalScore += tile.TotalPips; }` |
| **Sin estándar** |
| `foreach (Tile tile in boardTiles) {     if (tile.IsDouble)     {         totalScore += tile.TotalPips;     } }` |

| return | Se permiten retornos anticipados cuando una validación o condición determine que no corresponde continuar la ejecución del método. También se permite devolver resultados al completar una operación o resolver una alternativa, siempre que el flujo sea claro. |
| :---: | :---- |

| Con estándar |
| ----- |
| `public void SendChatMessage(string message) {     if (string.IsNullOrWhiteSpace(message))     {         return;     }     _chatService.SendMessage(message); }`  |
| **Sin estándar** |
| `public Player GetById(int playerId) {     try     {         return _context.Player.FirstOrDefault(             player => player.IdPlayer == playerId);     }     catch (Exception)     {         return null;     } }` |

7. # Pruebas {#pruebas}

## 7.1. Pruebas unitarias {#7.1.-pruebas-unitarias}

Las pruebas unitarias son pruebas destinadas a verificar el comportamiento de **unidades individuales de software**, como métodos o componentes, de manera aislada.

Las pruebas unitarias deben: 

* Centrarse en el código bajo control del desarrollador   
* Evitar depender de elementos de infraestructura como bases de datos, sistemas de archivos o recursos de red. 

## 7.2. Ubicación de las pruebas {#7.2.-ubicación-de-las-pruebas}

Dentro de la estructura del proyecto, se deberá crear una **sección aparte del código principal** dedicada a las pruebas unitarias. 

## 7.3. Nomenclatura de las pruebas {#7.3.-nomenclatura-de-las-pruebas}

Los métodos de prueba deberán utilizar un nombre que permita identificar:

* **qué método se está probando**  
* **bajo qué escenario**   
* **cuál es el comportamiento esperado**.(Microsoft, 2025).

Para este estándar se utilizará la siguiente estructura:

**`Metodo_Scenario_ComportamientoEsperado`**

Los nombres deberán utilizar **PascalCase** en cada una de las partes, separadas por guión bajo. Deberán ser descriptivos y evitar nombres genéricos.

Nota: Para el ejemplo se utilizó el framework de pruebas **NUnit**, sin embargo, no significa que el framework de pruebas a utilizar en el proyecto será este, únicamente se usó para cuestiones de demostración.

| Con estándar |
| ----- |
| `using NUnit.Framework;` `[TestFixture]` `public class PlayerTests` `{`     `[Test]`     `public void TakeDamage_ValidAmount_ReducesHealth()`     `{`     `}`     `[Test]`     `public void TakeDamage_ZeroAmount_PreservesHealth()`     `{`     `}` `}` |
| **Sin estándar** |
| `using NUnit.Framework;` `[TestFixture]` `public class PlayerTest` `{`     `[Test]`     `public void Test1()`     `{`     `}`     `[Test]`     `public void TestTakeDamage()`     `{`     `}` `}` |

## 7.4. Estructura de las pruebas {#7.4.-estructura-de-las-pruebas}

Cada prueba unitaria deberá seguir la estructura **Arrange, Act, Assert (AAA)**, separando claramente la preparación de los datos, la ejecución del comportamiento que se desea probar y la verificación del resultado. Deberá agregarse como comentario de una sola línea ([5.2. Comentarios de una sola línea](https://docs.google.com/document/d/1zefg_xCv_JrXeA7uPFBKmoZ_pp2VExomDO8AubRCOhM/edit?tab=t.0#heading=h.y123eugb916g)) la parte de la prueba a la que se refiere, esto con el objetivo de facilitar la legibilidad de las mismas para los integrantes del equipo.(Microsoft, 2025).

| Arrange | Deberá contener la creación y configuración de los objetos necesarios, así como los valores que serán utilizados por el método bajo prueba |
| :---: | :---- |
| **Act** | Deberá contener la ejecución del comportamiento que se desea comprobar. Cada prueba deberá procurar tener **una sola operación principal.** |
| **Assert** | Deberá verificar que el resultado obtenido corresponde con el comportamiento esperado. Una prueba deberá contener **al menos una aserción**  |

Nota: Para el ejemplo se utilizó el framework de pruebas **NUnit**, sin embargo, no significa que el framework de pruebas a utilizar en el proyecto será este, únicamente se usó para cuestiones de demostración.

| Con estándar |
| ----- |
| `using NUnit.Framework;` `[TestFixture]` `public class PlayerTests` `{`     `[Test]`     `public void TakeDamage_ValidAmount_ReducesHealth()`     `{`         `// Arrange`         `Player player = new Player(100);`         `int damageAmount = 25;`         `// Act`         `player.TakeDamage(damageAmount);`         `// Assert`         `Assert.That(player.Health, Is.EqualTo(75));`     `}` `}` |
| **Sin estándar** |
| `using NUnit.Framework;` `[TestFixture]` `public class PlayerTests` `{`     `[Test]`     `public void TakeDamage_ValidAmount_ReducesHealth()`     `{`         `var player = new Player(100);`         `int damageAmount = 25;`         `player.TakeDamage(damageAmount);`         `Assert.That(player.Health, Is.EqualTo(75));`         `player.TakeDamage(10);`         `Assert.That(player.Health, Is.EqualTo(65));`     `}` `}` |

8. # Declaración de uso de IA {#declaración-de-uso-de-ia}

Para la elaboración de este estándar se hizo uso de la herramienta ChatGPT para proponer una estructura del estándar. Posteriormente, fue utilizada para investigar de fuentes confiables información de los distintos apartados, la cual fue revisada revisada a partir de la cadena de razonamiento solicitada en cada promt y refinada por los miembros del equipo para incluirla en el estándar.

Además, se hizo uso de Gemini para darle una revisión al estándar y verificar que no hubiera inconsistencias entre los ejemplos, así como encontrar puntos que mejorar y aclarar ciertas reglas.

Como parte de la revisión final, se hizo uso de ChatGPT y Gemini para identificar puntos carentes del estándar e incoherencias para la entrega.

9. # Referencias {#referencias}

.NET Foundation, & Microsoft. (s. f.-a). *.editorconfig*. En *dotnet/runtime*. GitHub. Recuperado el 21 de agosto de 2026, de [https\://github.com/dotnet/runtime/blob/main/.editorconfig](https://github.com/dotnet/runtime/blob/main/.editorconfig)

.NET Foundation, & Microsoft. (s. f.-b). *C\# coding style*. En *dotnet/runtime*. GitHub. Recuperado el 21 de agosto de 2026, de [https\://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md)

Google. (s. f.). *C\# at Google Style Guide*. Google Style Guides. Recuperado el 30 de septiembre de 2026, de [https\://google.github.io/styleguide/csharp-style.html](https://google.github.io/styleguide/csharp-style.html)

Microsoft. (2023a). *Capitalization conventions*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/capitalization-conventions](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/capitalization-conventions)

Microsoft. (2023b). *Choosing between class and struct*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/choosing-between-class-and-struct](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/choosing-between-class-and-struct)

Microsoft. (2023c). *Code-style naming rules—.NET*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/naming-rules](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/naming-rules)

Microsoft. (2023d). *Enum design*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/enum](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/enum)

Microsoft. (2023e). *Event design*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/event](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/event)

Microsoft. (2023f). *EventArgs design*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/eventargs](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/eventargs)

Microsoft. (2023g). *Exception handling—C\#*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/exceptions/exception-handling](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/exceptions/exception-handling)

Microsoft. (2023h). *Field design—Framework Design Guidelines*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/field](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/field)

Microsoft. (2023i). *IDE0011: Add braces*. Microsoft Learn.

Microsoft. (2023j). *Implicitly typed local variables—C\#*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/implicitly-typed-local-variables](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/implicitly-typed-local-variables)

Microsoft. (2023k). *Interface design*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/interface](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/interface)

Microsoft. (2023l). *Names of namespaces*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-namespaces](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-namespaces)

Microsoft. (2023m). *Names of type members*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-type-members](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-type-members)

Microsoft. (2023n). *Naming parameters*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/naming-parameters](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/naming-parameters)

Microsoft. (2023o). *Protected members*. Microsoft Learn.

Microsoft. (2023p). *Struct design*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/struct](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/struct)

Microsoft. (2023q). *Using standard exception types—Framework Design Guidelines*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/using-standard-exception-types](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/using-standard-exception-types)

Microsoft. (2024). *.NET logging and tracing*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/core/diagnostics/logging-tracing](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/logging-tracing)

Microsoft. (2025a). *Access modifiers—C\# programming guide*. Microsoft Learn.

Microsoft. (2025b). *Best practices for exceptions—.NET*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/standard/exceptions/best-practices-for-exceptions](https://learn.microsoft.com/en-us/dotnet/standard/exceptions/best-practices-for-exceptions)

Microsoft. (2025c). *Best practices for writing unit tests—.NET*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)

Microsoft. (2025d). *C\# formatting options*. Microsoft Learn.

Microsoft. (2025e). *Classes, structs, and records—C\#*. Microsoft Learn.

Microsoft. (2025f). *Code-style rule options*. Microsoft Learn.

Microsoft. (2025g). *Expressions—C\# language specification*. Microsoft Learn.

Microsoft. (2025h). *Names of classes, structs, and interfaces*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-classes-structs-and-interfaces](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-classes-structs-and-interfaces)

Microsoft. (2025i). *Statements—C\# language specification*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/statements](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/statements)

Microsoft. (2025j). *Use the Task List*. Microsoft Learn. [https\://learn.microsoft.com/en-us/visualstudio/ide/using-task-list?view=vs-2022](https://learn.microsoft.com/en-us/visualstudio/ide/using-task-list?view=vs-2022)

Microsoft. (2026a). *.NET coding conventions—C\#*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)

Microsoft. (2026b). *Access modifiers—C\# reference*. Microsoft Learn.

Microsoft. (2026c). *Accessibility levels—C\# reference*. Microsoft Learn.

Microsoft. (2026d). *Boolean logical operators—C\# reference*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/boolean-logical-operators](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/boolean-logical-operators)

Microsoft. (2026e). *CA1031: Do not catch general exception types*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1031](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1031)

Microsoft. (2026f). *CA2201: Do not raise reserved exception types*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca2201](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca2201)

Microsoft. (2026g). *CA2254: Template should be a static expression*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca2254](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca2254)

Microsoft. (2026h). *Code comments: // and /* */*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/csharp/language-reference/tokens/comments](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/tokens/comments)

Microsoft. (2026i). *Create resource files*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/core/extensions/create-resource-files](https://learn.microsoft.com/en-us/dotnet/core/extensions/create-resource-files)

Microsoft. (2026j). *Documentation comments—C\# language specification*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments)

Microsoft. (2026k). *Exception handling statements—C\# reference*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/csharp/language-reference/statements/exception-handling-statements](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/statements/exception-handling-statements)

Microsoft. (2026l). *Generate XML API documentation comments—C\# reference*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/)

Microsoft. (2026m). *Handle and raise events—.NET*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/standard/events/](https://learn.microsoft.com/en-us/dotnet/standard/events/)

Microsoft. (2026n). *Identifier names: Rules and conventions—C\#*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/identifier-names](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/identifier-names)

Microsoft. (2026o). *Interfaces—C\# language specification*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/interfaces](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/interfaces)

Microsoft. (2026p). *Iteration statements in C\#*. Microsoft Learn.

Microsoft. (2026q). *Iteration statements—for, foreach, do, and while*. Microsoft Learn.

Microsoft. (2026r). *Jump statements—break, continue, return, and goto*. Microsoft Learn.

Microsoft. (2026s). *Lexical structure—C\# language specification*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/lexical-structure](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/lexical-structure)

Microsoft. (2026t). *Localization in .NET*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/core/extensions/localization](https://learn.microsoft.com/en-us/dotnet/core/extensions/localization)

Microsoft. (2026u). *Logging in C\#—.NET*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/core/extensions/logging](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging)

Microsoft. (2026v). *LogLevel Enum*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loglevel](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loglevel)

Microsoft. (2026w). *MSTest assertions—.NET*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-writing-tests-assertions](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-writing-tests-assertions)

Microsoft. (2026x). *Namespaces and using directives—C\#*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/program-structure/namespaces](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/program-structure/namespaces)

Microsoft. (2026y). *Namespaces—C\# language reference*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/namespace](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/namespace)

Microsoft. (2026z). *Namespaces—C\# language specification*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/namespaces](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/namespaces)

Microsoft. (2026aa). *Operators—C\# reference*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/)

Microsoft. (2026ab). *Order unit tests—.NET*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/core/testing/order-unit-tests](https://learn.microsoft.com/en-us/dotnet/core/testing/order-unit-tests)

Microsoft. (2026ac). *Properties—C\#*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/properties](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/properties)

Microsoft. (2026ad). *Recommended XML tags for C\# documentation comments*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags)

Microsoft. (2026ae). *Selection statements in C\#*. Microsoft Learn.

Microsoft. (2026af). *Selection statements—if, if-else, and switch*. Microsoft Learn.

Microsoft. (2026ag). *Structure types—C\# reference*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/struct](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/struct)

Microsoft. (2026ah). *Testing in .NET*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/core/testing/](https://learn.microsoft.com/en-us/dotnet/core/testing/)

Microsoft. (2026ai). *The const keyword—C\# reference*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/const](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/const)

Microsoft. (2026aj). *The using directive: Import types from a namespace—C\# reference*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/using-directive](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/using-directive)

Microsoft. (2026ak). *Tutorial: Create XML documentation—C\#*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/tutorials/xml-documentation](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/tutorials/xml-documentation)

Microsoft. (2026al). *Windows*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/desktop/wpf/windows/](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/windows/)

Microsoft. (2026am). *Write tests with MSTest*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-writing-tests](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-writing-tests)

Microsoft. (2026an). *x:Name directive*. Microsoft Learn. [https\://learn.microsoft.com/en-us/dotnet/desktop/xaml-services/xname-directive](https://learn.microsoft.com/en-us/dotnet/desktop/xaml-services/xname-directive)

Microsoft. (2026ao). *?: operator—the ternary conditional operator*. Microsoft Learn.

SonarSource. (s. f.). *Coding style*. En *sonar-dotnet*. GitHub. Recuperado el 30 de septiembre de 2026, de [https\://github.com/SonarSource/sonar-dotnet/blob/master/docs/coding-style.md](https://github.com/SonarSource/sonar-dotnet/blob/master/docs/coding-style.md)  
