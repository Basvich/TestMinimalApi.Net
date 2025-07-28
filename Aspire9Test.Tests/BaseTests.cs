using LiteBus.Queries.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Aspire9Test.Tests {
  public class BaseTests {
    protected IServiceProvider RequestServices { get; }
    private IQueryMediator? _queryMediator;
    protected IQueryMediator QueryMediator => _queryMediator ??= RequestServices.GetRequiredService<IQueryMediator>();

    public BaseTests() {
      RequestServices = TestSetup.ConfigureTestServices();
    }

    public BaseTests(IServiceProvider requestServices) {
      RequestServices = requestServices;
    }
  }
}
