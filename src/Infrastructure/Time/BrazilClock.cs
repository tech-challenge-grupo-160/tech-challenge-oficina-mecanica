using Fiap.TechChallenge.OficinaMecanica.Application.Common;
using Fiap.TechChallenge.OficinaMecanica.Shared.Helpers;

namespace Fiap.TechChallenge.OficinaMecanica.Infrastructure.Time;

public sealed class BrazilClock : IClock
{
    public DateTime Now => DateTimeHelper.UTCBrazilNow();
}
