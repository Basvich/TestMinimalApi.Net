# Test de API REST usando el nuevo estilo Minimal API con .NET 9

## 🚧 En plena construcción 🚧

- [Test de API REST usando el nuevo estilo Minimal API con .NET 9](#test-de-api-rest-usando-el-nuevo-estilo-minimal-api-con-net-9)
  - [🚧 En plena construcción 🚧](#-en-plena-construcción-)
  - [Separación de responsabilidades](#separación-de-responsabilidades)
    - [Aplicación](#aplicación)
    - [Presentación: Web API, MCP, Consola, etc.](#presentación-web-api-mcp-consola-etc)
    - [Servicios](#servicios)
  - [Mejoras y simplificaciones](#mejoras-y-simplificaciones)
    - [Patrón CQRS enlazado a los mapas de endpoints](#patrón-cqrs-enlazado-a-los-mapas-de-endpoints)
  - [Siguientes pasos](#siguientes-pasos)
    - [Extracción a una clase separada](#extracción-a-una-clase-separada)
    - [Extracción a una librería](#extracción-a-una-librería)

---

El objetivo de este repositorio es comprobar cómo queda el funcionamiento y la estructura de un proyecto que usa una arquitectura de tipo Minimal API, unido al patrón CQS, que siempre da un resultado muy limpio y sencillo de entender.

Anteriormente se usaba este patrón con la librería MediatR, pero debido a cambios en su licencia se buscó una alternativa, y en este caso es la librería [LiteBus](https://github.com/litenova/LiteBus).

Es casi obligatorio usar clases DTO diferentes a las del dominio, por motivos variados. Por ello, es necesario emplear herramientas que faciliten esa "burocracia obligatoria" para transformar unas en otras. Habitualmente se usa AutoMapper. Como también modificó su licencia de uso, se buscó una alternativa, y en este caso se ha optado por la librería [Mapster](https://github.com/MapsterMapper/Mapster) (otras alternativas son igualmente válidas).

Para no insistir de nuevo en la importancia de la *separación de responsabilidades*, puedes saltar directamente a la sección de [Mejoras y simplificaciones](#mejoras-y-simplificaciones) para ver cómo se puede simplificar el código de los endpoints y cómo se puede usar la inyección de dependencias de una forma más limpia.

## Separación de responsabilidades

Se separan las responsabilidades en diferentes capas, como siempre, y hasta aquí nada especialmente nuevo. La capa de la aplicación es realmente el núcleo, donde se implementa la mayor parte de la lógica de negocio. Me centraré principalmente en remarcar las diferencias con la mayoría de implementaciones que he visto y en algunos casos trabajado.

### Aplicación

Aquí es donde se concentra la mayor parte de la lógica de negocio. El mayor cambio es que la lógica de negocio se implementa en los comandos y consultas, que son manejados por LiteBus. Esto permite una separación clara entre la lógica de negocio y la infraestructura. A su vez, las clases que responden a los comandos y consultas pueden usar los típicos servicios externos, como persistencia, autorización, etc., principalmente a través de inyección de dependencias.

- Este proyecto NO debe conocer absolutamente nada de la parte de presentación (principalmente Web API), siendo lo más agnóstico posible. Un ejemplo: si la aplicación está bien realizada, podría usar una interfaz tipo Web API REST convencional, otro protocolo como gRPC, o incluso una aplicación de consola, etc.
- Este proyecto puede contener su propio dominio de clases para su lógica de negocio, aunque generalmente, si hace falta algún tipo de persistencia, entonces esas mismas clases ya se encontrarían en un proyecto de dominio separado, el cual es accesible desde aquí y desde, por ejemplo, el servicio de persistencia.

> 💡 Como nueva utilidad que encaja perfectamente con esta arquitectura, si se quiere añadir una interfaz de tipo MCP (Model Context Protocol) para IA, se convierte en algo trivial, ya que la lógica de negocio está completamente separada de la presentación.

### Presentación: Web API, MCP, Consola, etc.

La labor básica de este proyecto (o proyectos, si hay varias presentaciones diferentes) es permitir la interacción con el usuario, que puede ir desde una clásica aplicación de consola, un servicio Web API REST que a su vez proporciona servicio a una aplicación web, o incluso un servidor MCP (Model Context Protocol) que sirve al usuario, pero alimentando, por ejemplo, un modelo LLM de IA.

Puntos clave:

- Normalmente, esta aplicación es la que acaba conociendo todas las demás, al menos en existencia, ya que es la encargada de declarar las clases que se instanciarán en la inyección de dependencias.
  - Facilita también la parte de testing, ya que en los tests de integración lo que hacemos es sustituir el front por llamadas a los mismos comandos y consultas que se harían desde la aplicación web o consola. No hace falta, o se puede incluso prescindir, de tests orientados a probar que una llamada a un endpoint (en sí mismo) funciona.
- Generalmente suele conectar a nivel de funcionalidad directamente con la aplicación para hacer lo que tenga que hacer. Normalmente se hará usando el patrón CQRS (Comandos y Consultas), que en este caso usa la librería LiteBus (en vez de MediatR). El resultado final es que muchos de los endpoints simplemente lanzan un comando o consulta a la aplicación. En algunos casos, cuando la aplicación web necesita algún tipo de dato o funcionalidad que no encaja perfectamente con el negocio, o es demasiado particular para llevarlo a servicios, se puede implementar en esta parte.
- Pueden existir en esta capa muchas de las clases *DTO*, por ejemplo en su propio dominio, ya que suelen ser cosas particulares de la interfaz con el usuario, y el resto de la aplicación no necesita conocerlas.
- También tendríamos en esta capa otros aspectos como la lógica de las respuestas REST (cómo normalizamos todo, indicamos errores, etc.). Lo normal sería que la capa de la aplicación, por ejemplo, genere excepciones, y la capa de presentación las maneje, devolviendo el error correspondiente al usuario.
- De la capa de la aplicación, muchas veces tenemos clases que no son las que se usan para interactuar con el usuario, por lo que se acaban usando otras clases *DTO*. Para convertir estas clases en las otras, se usan mapeadores, siendo el más clásico AutoMapper.
  - En muchas implementaciones de esta capa, se ve cómo se acaba repitiendo mucho código para añadir instrucciones de mapeo. Todo esto puede simplificarse enormemente con el uso de genéricos, que se encargan de buscar y llamar los mapeos necesarios, sin necesidad de repetir código. En este proyecto se usa la librería [Mapster](https://github.com/MapsterMapper/Mapster), que permite hacer esto de una forma muy sencilla y limpia.
- Comparado con implementaciones clásicas de esta capa, la implementación del endpoint resulta realmente simple, siendo muchas veces una única línea de código, sin necesitar el uso de otros servicios propios de la aplicación.
- En esta capa sí hay que añadir la lógica de seguridad, como la autenticación y autorización, el manejo del JWT, etc.
  - 😕 Esto no quita que en la parte de la aplicación no se vuelvan a comprobar de nuevo las políticas de seguridad. Es cierto que en la práctica, es probable que el mismo código de seguridad se acabe ejecutando varias veces.

### Servicios

Esto suele ser una aplicación o varias que agrupan distintos servicios, como por ejemplo la persistencia de los datos, la conexión a otros servicios externos, etc.

## Mejoras y simplificaciones

### Patrón CQRS enlazado a los mapas de endpoints

Tenemos varias opciones para el agrupamiento de los endpoints y para jugar también con la inyección de dependencias. Una de ellas es usar un patrón como:

```csharp
public class ProductEndpoints {
    private readonly IServiceProvider requestServices;
    private IQueryMediator? _readMediator;
    protected IQueryMediator ReadMediator => _readMediator ??= requestServices.GetRequiredService<IQueryMediator>();
    private ICommandMediator? _commandMediator;
    protected ICommandMediator CommandMediator => _commandMediator ??= requestServices.GetRequiredService<ICommandMediator>();
    public ProductEndpoints(IServiceProvider requestServices) {
      this.requestServices = requestServices;
    }
}
```

Básicamente, uso la inyección de dependencias solo de las necesarias en el momento. Resulta en código bastante más limpio y eficiente que tener constructores con muchos parámetros de cosas que igual ni se usan en esas llamadas.
Sin embargo, puede complicar un poco los tests, ya que básicamente hay que simular el requestServices (usando fixtures o similar).

Para un endpoint, por ejemplo, que devuelve la lista de productos (o uno solo) usando el patrón mediador, quedaría:

```csharp
private async Task<IResult> GetAllProducts() {
  var products = await ReadMediator.QueryAsync(new GetAll());
  return Results.Ok(products);
}

private async Task<IResult> GetProductById(int id) {
  var product = await ReadMediator.QueryAsync(new GetById { Id = id });
  return product != null ? Results.Ok(product) : Results.NotFound();
}
```

Que es bastante limpio y sencillo. Pero si nos fijamos, se repite mucho código, y todavía podemos mejorar eso.

En la clase UserProductEndpoints, tenemos unos métodos de lectura, que podrían estar destinados a otros clientes, en los cuales devolvemos algo mucho más habitual, que es una clase DTO.

La simplificación que empieza a ser obvia es usar unas funciones que automáticamente llamen a los comandos y realicen el mapeo si es necesario.

```csharp
protected async Task<T> GetMediatorResult<T>(IQuery<T> request, CancellationToken cancellationToken) {...}

protected async Task<TMapped> GetMappedMediatorResult<T, TMapped>(IQuery<T> request, CancellationToken cancellationToken) {...}
```

Estas funciones ya nos dan algo muy similar a lo que queremos, que es un *IResult*. A partir de aquí, se abren más opciones, siendo una de ellas usar una función que tome el resultado obtenido (incluidas excepciones) y lo adapte a la respuesta esperada.

Así, por ejemplo, si usamos un mapeo:
La función original:

```csharp
private async Task<IResult> GetAllProducts(CancellationToken ct = default) {
  var products = await ReadMediator.QueryAsync(new GetAll(), ct);
  var productDtos = products.Adapt<List<ProductDto>>();
  return Results.Ok(productDtos);
}
```

se transforma en:

```csharp
private Task<IResult> GetAllProducts(CancellationToken ct = default) {
  return GetMappedMediatorIResult<List<Product>, List<ProductDto>>(new GetAll(), null, ct);
}
```

y ya, todavía más simple:

```csharp
private Task<IResult> GetAllProducts(CancellationToken ct = default) => GetMappedMediatorIResult<List<Product>, List<ProductDto>>(new GetAll(), null, ct);
```

Si las funciones helper de llamadas a CQRS como `GetMappedMediatorResult()` se implementan en una clase base, ya tenemos esa funcionalidad para todo el proyecto.

> [Ejemplo de clase base](https://github.com/Basvich/TestMinimalApi.Net/blob/master/Aspire9Test.ApiService/Endpoints/BaseEnpoints.cs)

## Siguientes pasos

### Extracción a una clase separada

Tenemos la funcionalidad como parte de una clase base, y se usa en clases derivadas. Se puede extraer a una clase aparte y usar de forma muy similar.

### Extracción a una librería

Teniendo el código en una clase separada, lo obvio es extraerlo a una librería, la cual se podría reutilizar en cualquier otro proyecto, ya que básicamente depende de Minimal API, de Mapper y de LiteBus.

En cuanto se intente, el primer problema es que no se puede llevar a una librería aparte, ya que depende de IResult (Results), y no es posible usar eso fuera del proyecto.
En teoría, parece que el principal motivo es para no violar el principio de inversión de dependencias. Lo cual no sería estrictamente cierto, ya que sigue siendo independiente de la aplicación.

Si pensamos, por ejemplo, en cómo podemos plantear esa misma solución para añadir un servidor MCP, que sería la nueva capa de presentación, entonces vemos que básicamente lo que hay que hacer es separar la funcionalidad de mapeo y de CQRS de la parte de construir la respuesta HTTP. Así, separaríamos totalmente esa parte de mapeo HTTP, y podríamos crear otra similar para MCP, quedando bien separadas las responsabilidades.

Eso si, la parte justo de la funcionalidad que adaptaría por ejemplo a minimal Api http, tendría que estar en el proyecto .ApiServices .