# Manejo de delegados

Los delegados se utilizarán cuando el comportamiento que representan deba proporcionarse, almacenarse o ejecutarse como parte esencial de la responsabilidad de un componente.

Su uso deberá comunicar claramente qué operación se ejecutará y por qué dicha operación necesita proporcionarse de manera variable. No deberán introducirse únicamente para reducir código, evitar una llamada directa o facilitar la sustitución de métodos durante pruebas.

## Tipos de delegados proporcionados por .NET

Se podrán utilizar los siguientes tipos cuando su firma y propósito resulten claros:

* `Action` para operaciones que no reciben parámetros ni devuelven un resultado.
* `Action<T>` y sus variantes para operaciones que reciben parámetros y no devuelven un resultado.
* `Func<TResult>` y sus variantes para operaciones que devuelven un resultado. El último parámetro genérico representa el tipo del valor devuelto.
* `Predicate<T>` cuando una API requiera una operación que reciba un elemento y determine mediante un valor `bool` si cumple una condición.

Los parámetros, campos y propiedades de tipo delegado deberán utilizar nombres que comuniquen el propósito de la operación. Se evitarán nombres genéricos como `action`, `function`, `callback` o `handler` cuando no permitan identificar claramente la responsabilidad representada.

## Usos adecuados

Se permitirá almacenar o recibir delegados cuando el comportamiento intercambiable forme parte natural de la responsabilidad del componente, por ejemplo:

* Comandos que deban ejecutar una operación proporcionada por su consumidor.
* Trabajos en segundo plano que reciban el trabajo que se ejecutará y las operaciones de finalización.
* Callbacks y notificaciones que deban ejecutarse cuando ocurra un resultado determinado.
* Eventos con uno o varios suscriptores.
* Infraestructura genérica que ejecute diferentes operaciones dentro de un mismo ciclo de vida, como la creación, uso y cierre seguro de un proxy WCF.
* Estrategias sencillas cuyo comportamiento deba seleccionarse en tiempo de ejecución y cuya representación mediante una clase adicional no aporte mayor claridad.

## Sustitución incorrecta de colaboradores

Los delegados no deberán utilizarse como sustitutos genéricos de servicios, repositorios, fábricas, publicadores, colas u otros colaboradores estables de una clase.

Cuando una colaboración tenga una responsabilidad, identidad, estado, ciclo de vida o conjunto de operaciones reconocible, deberá representarse mediante una clase o interfaz con nombre.

La posibilidad de sustituir una operación durante una prueba unitaria no será, por sí sola, justificación suficiente para convertirla en un `Action`, `Func` o `Predicate`. La abstracción deberá tener también una finalidad clara dentro del diseño utilizado durante la ejecución normal de la aplicación.

Se deberá preferir una dependencia con nombre cuando:

* El delegado oculte qué componente realiza realmente la operación.
* Sea necesario revisar otro constructor o lugar de composición para descubrir qué método se ejecutará.
* La operación forme parte de una responsabilidad propia de un repositorio, servicio, fábrica, cola o publicador.
* El colaborador administre recursos o requiera un ciclo de vida determinado.
* Existan varias operaciones relacionadas que pertenezcan al mismo colaborador.
* El delegado haya sido introducido únicamente para proporcionar una implementación falsa durante las pruebas.

No deberán crearse interfaces o clases artificiales para cada operación individual. La decisión deberá basarse en si existe un colaborador o concepto real que resulte más claro mediante un tipo con nombre.

## Métodos compatibles

Cuando ya exista un método compatible con el delegado requerido, dicho método deberá proporcionarse directamente mediante su nombre.

No deberá envolverse innecesariamente en una expresión lambda ni en un método anónimo.

```csharp
_saveCommand = new RelayCommand(SavePlayer);
```

En lugar de:

```csharp
_saveCommand = new RelayCommand(
    parameter => SavePlayer(parameter));
```

Los bloques anónimos escritos mediante `delegate (...) { ... }` deberán evitarse cuando el comportamiento pueda expresarse mediante un método con nombre.

## Delegados personalizados

Se permitirá declarar un tipo de delegado personalizado cuando su nombre represente un concepto relevante del dominio o cuando una misma firma forme parte de un contrato reutilizado por diferentes componentes.

El nombre deberá utilizar PascalCase y comunicar el comportamiento representado. No deberá crearse un delegado personalizado cuando `Action`, `Func` o `Predicate` expresen la operación con suficiente claridad.

## Campos y parámetros

Los campos de tipo delegado que no deban cambiar después de construir el objeto deberán declararse `readonly`. Su accesibilidad deberá limitarse al nivel mínimo necesario.

Los delegados recibidos como callbacks, estrategias o parámetros funcionales cuentan para el límite establecido en la sección 4.12. No se considerarán automáticamente dependencias colaboradoras exentas del máximo de parámetros.

Estas reglas no deberán utilizarse para modificar firmas exigidas por .NET, WPF, WCF, Entity Framework u otras bibliotecas utilizadas por el proyecto.


## Ejemplos
### Buen ejemplo

`RelayCommand` recibe una operación porque su responsabilidad consiste en ejecutarla posteriormente:

```csharp
public sealed class RelayCommand : ICommand
{
    private readonly Action<object> _execute;

    public RelayCommand(Action<object> execute)
    {
        _execute = execute
            ?? throw new ArgumentNullException(nameof(execute));
    }

    public void Execute(object parameter)
    {
        _execute(parameter);
    }
}
```

El consumidor proporciona directamente un método compatible:

```csharp
SaveCommand = new RelayCommand(SavePlayer);
```

```csharp
private void SavePlayer(object parameter)
{
    _playerService.Save();
}
```

Un trabajo en segundo plano recibe operaciones porque estas forman parte del comportamiento que coordina:

```csharp
BackgroundJob<string, PlayerAccount> job =
    new BackgroundJob<string, PlayerAccount>(
        Login,
        username,
        OnLoginCompleted,
        OnLoginFailed);
```

Un controller del servidor crea el repositorio con nombre dentro de la operación
en vez de ocultar su comportamiento detrás de un delegado:

```csharp
public PlayerAccount GetAccount(int playerId)
{
    using (DominoAllFivesEntities context = new DominoAllFivesEntities())
    {
        IPlayerRepository playerRepository = new PlayerRepository(context);
        Player player = playerRepository.GetById(playerId);
        return CreatePlayerAccount(player);
    }
}
```

En Domino All Fives, los controllers crean y liberan el contexto de Entity
Framework y componen sus repositorios por operación. No reciben el repositorio
desde la frontera WCF ni agregan una ruta de inyección únicamente para pruebas.
Esta responsabilidad se especifica en `Manejo de comunicación remota.md`.

### Mal ejemplo

Ocultar un repositorio detrás de una función genérica:

```csharp
private readonly Func<string, Player> _findPlayer;

public AuthenticationController(Func<string, Player> findPlayer)
{
    _findPlayer = findPlayer
        ?? throw new ArgumentNullException(nameof(findPlayer));
}
```

```csharp
Player player = _findPlayer(identifier);
```

Ocultar una cola o publicador detrás de un `Action`:

```csharp
private readonly Action<MatchUpdate> _enqueue;

public MatchController(Action<MatchUpdate> enqueue)
{
    _enqueue = enqueue
        ?? throw new ArgumentNullException(nameof(enqueue));
}
```

Utilizar un delegado únicamente para sustituir la hora durante las pruebas, sin que exista una abstracción temporal en el diseño de producción:

```csharp
private readonly Func<DateTime> _utcNow;
```

Utilizar nombres que no comuniquen la responsabilidad:

```csharp
private readonly Action<Player> _action;
private readonly Func<Player, bool> _function;
```

Envolver innecesariamente un método compatible:

```csharp
SaveCommand = new RelayCommand(
    parameter => SavePlayer(parameter));
```
