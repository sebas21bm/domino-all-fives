# Domino All Fives

**Domino All Fives** es un videojuego multijugador de escritorio basado en la variante de dominó *All Fives*. 

Este proyecto se desarrolla como parte de la experiencia educativa de **Tecnologías para la Construcción de Software** de la Licenciatura en Ingeniería de Software.

La aplicación utiliza una arquitectura cliente-servidor sobre tecnologías de Microsoft, con un cliente de escritorio desarrollado en **WPF**, comunicación remota mediante **Windows Communication Foundation (WCF)**, persistencia con **Entity Framework** y una base de datos **SQL Server**.

## Descripción

En Domino All Fives, los jugadores participan en partidas de dominó donde pueden obtener puntos cuando la suma de los extremos disponibles del tablero es un múltiplo de cinco.

El sistema contempla partidas multijugador y funcionalidades complementarias como administración de cuentas, perfiles, amigos, presencia, lobby, chat y rankings.

## Reglas principales de All Fives

El juego utiliza las reglas definidas específicamente para el proyecto.

- Una partida puede tener de 2 a 4 jugadores.
- Cada ficha colocada debe coincidir con alguno de los extremos disponibles.
- Si la suma de los extremos disponibles corresponde a un múltiplo de 5, el jugador obtiene esa cantidad de puntos.
- Si un jugador no dispone de una jugada válida, debe tomar fichas del pozo mientras existan fichas disponibles.
- Una ronda termina cuando un jugador utiliza todas sus fichas o cuando ya no existen jugadas posibles.
- Las partidas pueden utilizar objetivos de 100, 150, 200 o 250 puntos.
- Existen las fichas comodines: Puntos dobles (Rojas) y bloquear extremo (Moradas)

Las reglas completas del juego se mantienen en la documentación del proyecto.

## Funcionalidades

El alcance definido para Domino All Fives contempla:

- Registro e inicio de sesión de jugadores.
- Acceso como invitado.
- Administración y personalización del perfil.
- Recuperación y cambio de contraseña.
- Lista e invitación de amigos.
- Manejo de presencia de jugadores.
- Creación y administración de lobbies.
- Unión y expulsión de jugadores.
- Inicio y ejecución de partidas multijugador.
- Chat en lobby y partida.
- Ranking de jugadores.
- Soporte para español, portugués e inglés.


## Tecnologías

### Plataforma

- C#
- .NET Framework 4.8
- Visual Studio 2022

### Cliente

- Windows Presentation Foundation (WPF)
- XAML
- Patrón MVVM
- Recursos `.resx` para internacionalización
- Windows Communication Foundation (WCF) como consumidor de servicios remotos

### Servidor

- Windows Communication Foundation (WCF)
- Microsoft Entity Framework 6
- SQL Server
- `Microsoft.Extensions.Logging`
- Servicios de aplicación y lógica de negocio separados por proyectos

### Calidad

El proyecto incorpora prácticas relacionadas con:

- pruebas automatizadas;
- manejo estructurado de excepciones;
- logging;
- concurrencia;
- validación de datos;
- separación de responsabilidades;
- análisis estático mediante SonarQube/SonarCloud;
- control de versiones mediante Git y GitHub.

## Documentación

La documentación técnica asociada a las decisiones e implementación del proyecto se mantiene en:

```text
/Documentation
```

Entre los temas que se documentan se encuentran:

- comunicación remota;
- concurrencia;
- manejo de excepciones;
- logging;
- arquitectura;
- decisiones técnicas relevantes.

## Requisitos de desarrollo

Para trabajar con el proyecto se recomienda disponer de:

- Windows 10 u 11;
- Visual Studio 2022;
- .NET Framework 4.8 Developer Pack;
- SQL Server;
- acceso a una instancia de la base de datos de Domino All Fives;
- NuGet para restaurar las dependencias.

## Configuración inicial

### 1. Clonar el repositorio

```bash
git clone https://github.com/sebas21bm/domino-all-fives.git
cd domino-all-fives
```

La rama de integración del proyecto es:

```bash
git switch master
git pull
```

Para realizar cambios se recomienda crear una rama específica en lugar de trabajar directamente sobre `master`.

Por ejemplo:

```bash
git switch -c feature/example
```

### 2. Configurar la base de datos

El proyecto del servidor incluye el archivo:

```text
DominoAllFives.Server/
└── DominoAllFives.Server/
    └── Configuration/
        └── DatabaseConnection.example.config
```

Debe utilizarse como referencia para crear:

```text
DatabaseConnection.config
```

La configuración debe proporcionar la cadena de conexión `DominoAllFivesEntities`.

Las credenciales reales de base de datos no deben almacenarse en el repositorio.

### 3. Configurar WCF

Las direcciones de los servicios se encuentran definidas mediante los archivos `App.config` del cliente y del servidor.

Antes de ejecutar el sistema en otro equipo o red se deben comprobar las direcciones IP de los servicios incorporados.

Las direcciones configuradas deben corresponder con el equipo donde se ejecuta el servidor.

### 4. Restaurar dependencias

Desde Visual Studio se deben restaurar los paquetes NuGet de las soluciones antes de compilar.

También puede realizarse mediante las herramientas de restauración correspondientes al proyecto.

### 5. Compilar el servidor

Abrir:

```text
DominoAllFives.Server/DominoAllFives.Server.sln
```

y ejecutar:

```text
Build → Rebuild Solution
```

El proyecto ejecutable es:

```text
DominoAllFives.Server
```

### 6. Compilar el cliente

Abrir:

```text
DominoAllFives.Client/DominoAllFives.Client.sln
```

y ejecutar:

```text
Build → Rebuild Solution
```

El proyecto ejecutable es:

```text
DominoAllFives.Client.WPF
```

### 7. Ejecutar el sistema

El servidor debe iniciarse antes que el cliente para que los endpoints WCF se encuentren disponibles.

El flujo general es:

```text
1. SQL Server
        ↓
2. DominoAllFives.Server
        ↓
3. Servicios WCF disponibles
        ↓
4. DominoAllFives.Client.WPF
```


## Pruebas

El repositorio contiene proyectos de pruebas para cliente y servidor:

```text
DominoAllFives.Client.Tests
DominoAllFives.Server.Tests
```

Se ejecutan de la siguiente manera:

```text
Build → Rebuild Solution
Test → Run All Tests
```
