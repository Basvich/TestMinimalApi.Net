using LiteBus.Queries.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspire9Test.Aplication.Cqs.Dummy.Read {

  public class GetRndNumsQuery:IQuery<double> {
    public double MaxValue { get; set; } = 10;
  }
  public class DummyNumsHandler : IQueryHandler<GetRndNumsQuery, double> {
    public Task<double> HandleAsync(GetRndNumsQuery message, CancellationToken cancellationToken = default) {
      //Devuelve un número aleatorio entre 0 y MaxValue  
      if (message.MaxValue <= 0) {
        throw new ArgumentOutOfRangeException(nameof(message), "MaxValue must be greater than 0.");
      }
      var random = new Random();
      double result = random.NextDouble() * message.MaxValue;
      return Task.FromResult(result);
    }
  }
}
