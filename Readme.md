# Test de api rest, usando nuevo estilo Minimal AP con .net9
## 🚧 En plena construcción🚧

El objetivo de este repositorio, es simplemente comprobar como queda el funcionamiento y la estructura de un proyecto que usa una arquitectura de tipo Minimal API, unido al patron CQS, que siempre da un resultado muy limpio y sencillo de entender.

Anteriormente se usaba este patron usando la librería MediatR, pero debido a cambios en su licencia hubo que probar una alternativa, y en este caso es la librería [LiteBus](https://github.com/litenova/LiteBus).

Acaba siendo casi obligatorio necesitar usar clases dto, diferentes a las del dominio, por motivos variados. Con lo que es necesario usar cosas que nos faciliten esa especie de "burocracia obligatoria" para transformar de uinas a otras. 
Habitualmente se usa AutoMapper. Como tambíen modifico su licencia de uso, se buscó una alternativa, que en este caso se ha optado por la librería [Mapster](https://github.com/MapsterMapper/Mapster), (otras alternativas son igualmente válidas).

Para no escuchar de nuevo la importancia de *Separacion de responsabilidades*, se puede saltar directamente a la sección de [Mejoras y simplificaciones](#mejoras-y-simplificaciones) para ver como se puede simplificar el código de los endpoints, y como se puede usar la inyección de dependencias de una forma más limpia. 

## Separacion de responsabilidades

Se separan las responsabilidades en diferentes capas, como siempre, y hasta aqui nada especialmente nuevo. Siendo la capa de la aplicación realmente el nucleo de la aplicación en si, donde se implementa la mayor parte de la lógica de negocio. Me centraré principalmante en remarcar las diferencias con la mayor parte de las implementaciones que puede ver y en algunos casos trabajar.

### Aplicación

Aquí es donde se concentra la mayor parte de la lógica del negocio. El mayor cambio es que la mayor parte de la lógica de negocio se implementa en los comandos y consultas, que son manejados por LiteBus. Esto permite una separación clara entre la lógica de negocio y la infraestructura. A su vez, las clases que responden a los comandos y consultas se pueden  realizar para usar los típicos servicios externos, como persistencia, autorización, etc, principalmente a través de inyección de dependencias.

- Este proyecto, NO tiene que conocer absolutamente nada de la parte de la presentación (princpialmente web API), siendo lo más agnostico posible. Un posible ejemplo o idea que a veces tomo es que si esta bien realizada la aplicación, podría usar un interface tipo web api rest convencional, u otro tipo de protocolo, como gRPC, o incluso una aplicación de consola, etc. 
- Este proyecto puede contener su propio dominio de clases, para su lógica de negocio, aunque generalmente si hace falta algun tipo de persistencia, entonces esas mismas clases ya se encontrarían en un proyecto dominio separado, el cual es accesible desde aqui, y desde por ejemplo el servicio de persistencias.


> 💡 Como nueva utilidad que encaja perfectamente con esta arquitectura, es que si se quiere añadir un interface de tipo MCP (model Context Protocol) para la IA, se convierte en algo tremendamente trivial, ya que la lógica de negocio está completamente separada de la presentación.


### Presentación, Web Api, MCP, Consola, etc

La labor básica de este proyecto (o proyectos si ponemos varias presentaciones diferentes) es la de permitir la interacción con el usuario, que puede ir desde una clásica aplicación de consola, o un servicio web API rest, que a su vez proporciona servicio a una aplicación web, o incluso un servidor MCP (Model Context Protocol) Que sirve al usuario, pero alimentando por ejemplo un model LLM de IA.

Tenemos una serie de puntos claves:

- Normalmente, esta aplicación es la que acaba conociendo todas las demás, al menos en existencia, ya que es la encargada de declarar las clases que se instaciaran en las inyecciones de dependencias.
  - Facilita tambien una parte de testing, ya que en la parte de test, mas tipo integración, lo que hacemos es substituir el front, por simplemente llamadas a los mismos comandos y consultas que se harían desde la aplicación web, o consola, etc. No hace falta, o se puede incluso prescindir de test orientados a justo probar que una llamada a un endpoint (en si mismo) funciona.
- Generalmente suele conectar a nivel de funcionalidad directamente con la aplicación para hacer lo que tenga que hacer. Generalmente se hará usando el patron CQRS (Comandos y Consultas), que en este caso usamos la librería LiteBus (En vez de MediatR). El resultado final es que muchos de los endpoints, son simplemente lanzar un comando o query a la aplicación. En algunos casos en los que por ejemplo la aplicación web necesite algun tipo de datos o funcionalidad que no encaja perfectamente con el negocio, o es complejo y demasiado particular llevarlo a servicios, se puede implementar en esta parte.
- Pueden existir en esta capa, muchas de las clases *dto*, por ejemplo en su propio domain, ya que suelen ser cosas particulares del interface con el usuario, y el resto de la aplicación no necesita conocerlas.
- Tambien tendríamos en esta capa otros aspectos como la logíca de las respuestas, rest (o sea como normalizamos todo, indicamos errores, etc). Lo normal sería que la capa de la aplicación por ejemplo genera excepciones, y la capa de presentación las maneja, devolviendo el error correspondiente al usuario.
- De la capa de la aplicación, muchas veces tenemos clases que no son las que se usan para interactuar con el usuario, con lo que se acaban usando otras clases *dto*. Para convertir estas clases en las otras, se usan mapeadores, siendo el más clasico el auto-mapper. 
  - En muchas de las implementaciones de esta capa, se vé como se acaba repitiendo mucho código para añadir instrucciónes para el mapeo. Todo esto puede simplificarse enormemente con el uso de genericos, que se encargan de buscar y llamar los mapeos necesarios, sin necesidad de repetir código. En este proyecto se usa la librería  [Mapster](https://github.com/MapsterMapper/Mapster), que permite hacer esto de una forma muy sencilla y limpia.
- Comparado con implementaciones clasicas de esta capa, resulta en la implementación del endpoint, realmente simple, siendo muchas veces una única línea de código, no necesintando el uso de otros servicios propios de la aplicación en si.
- En esta capa, si que hay que añadir la lógica de seguridad, como la autenticación y autorización, el manejo del JWT, etc. 
  - 😕 Esto no quita que en la parte de la aplicación en si, no se vuelvan a comprobar de nuevo las politicas de seguridad. Es cierto que en la práctica, es probable que el mismo código de seguridad se acabe ejecutando varias veces.


### Servicios

Esto suele ser una aplicación o varias que agrupan distintos servicios, como por ejemplo la persistencia de los datos, la conexión a otros servicioes externos, etc. 


## Mejoras y simplificaciones

### Patron CQRS enlazado a los maps de endpoints.

Tenemos varias opciones para el agrupamiento de los endpoints, y jugar también con la inyección de dependencias. Una de ellas es usar un patron como:

```csharp
 public  class ProductEndpoints {

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

Básicamente uso la inyeccion de dependencias solo de las necesarias en el momento. Resulta en códig bastante más limpio y eficiente, que tener constructores con muchos parámetros de cosas que igual ni se usan en esas llamadas.
Sin embargo, puede empeorar un poco el test, ya que básicamente hay que moquear el requestServices (usar fictures o similar).

Para un endpoint por ejemplo que devuelve la lista de productos (o uno solo) usando el patron mediatos, quedaría:

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

Que es bastante limpio y sencillo. Pero a poco que nos fijemos, podemos ver que se repite mucho código, y que todavía podemos mejorar eso.


En la clase UserProductEndpoints, tenemos unos metodos de lectura, que podrían estar destinados a otros clientes, en los cuales devolvemos algo mucho mas habitual, que es una clase dto.

La simplificación que empieza a ser obvia es usar unas funciones que automáticamente llamen a los comandos, y realizen el mapeo si es necesario.

```csharp
   protected async Task<T> GetMediatorResult<T>(IQuery<T> request, CancellationToken cancellationToken) {...}

   protected async Task<TMapped> GetMappedMediatorResult<T, TMapped>(IQuery<T> request, CancellationToken cancellationToken) {...}
```

Estas funciones ya nos dan algo muy similar a lo que queremos, que es un *IResult* . A partir de aquí, se abren mas opciones, siendo una de ellas, usar una función que nos tome el resultado obtenido (incluidas excepciones) y lo ponga conforme la respuesta esperada.



Así por ejemplo, si usamos un mapeo:
La función original:

```csharp
private Task<IResult> GetAllProducts(CancellationToken ct = default) {
  private async Task<IResult> GetAllProducts(CancellationToken ct = default) {
  var products = await ReadMediator.QueryAsync(new GetAll(), ct);
  var productDtos = products.Adapt<List<ProductDto>>();
  return Results.Ok(productDtos);
}
```


se transforma en:
```csharp
private Task<IResult> GetAllProducts(CancellationToken ct = default) {
  return GetMappedMediatorIResult<List<Product>, List<ProductDto>>(new GetAll(),null, ct);     
}
```

y ya, todavía mas simple:
```csharp
 private Task<IResult> GetAllProducts(CancellationToken ct = default) => GetMappedMediatorIResult<List<Product>, List<ProductDto>>(new GetAll(), null, ct);
 ```

