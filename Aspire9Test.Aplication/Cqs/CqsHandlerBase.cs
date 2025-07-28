using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspire9Test.Application.Cqs {
  public class CqsHandlerBase {
    protected IServiceProvider RequestServices;
    private ILogger<CqsHandlerBase>? _logger;
    public ILogger<CqsHandlerBase> Logger => _logger ??= RequestServices.GetRequiredService<ILogger<CqsHandlerBase>>();

    public CqsHandlerBase(IServiceProvider requestServices) {
      RequestServices = requestServices;
    }
  }
}
