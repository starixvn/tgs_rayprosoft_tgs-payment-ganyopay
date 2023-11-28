using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Valhalla.MessageQueue;

namespace Tgs.Payment.GanyoPay.MessageQueue;
internal class DepositTimeoutCheckHandler : IMessageHandler
{
    public DepositTimeoutCheckHandler()
    {
    }

    public ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default) => throw new NotImplementedException();
}
