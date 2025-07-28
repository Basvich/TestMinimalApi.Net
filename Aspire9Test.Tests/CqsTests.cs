using Aspire9Test.Aplication.Cqs.Dummy.Read;
using FluentAssertions;
using System.Threading.Tasks;
using Xunit;

namespace Aspire9Test.Tests {
  public class CqsTests : BaseTests {
    public CqsTests() : base() {
      // Constructor que usa el IServiceProvider configurado en BaseTests
    }

    [Fact]
    public async Task GetRandomNum() {
      // Ahora puedes usar QueryMediator para ejecutar tus consultas
      var result = await QueryMediator.QueryAsync(new GetRndNumsQuery());
      result.Should().BeInRange(0, 10, "El número aleatorio debe estar entre 0 y 10 por defecto.");
    }
  }
}
