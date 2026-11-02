using Fiap.TechChallenge.OficinaMecanica.Shared.Helpers;
using FluentAssertions;

namespace Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Units.Helpers;

public class DateTimeHelperTests
{
    [Fact]
    public void BrazilToUtc_DeveConverterHorarioSemFusoParaUtc()
    {
        var horarioBrasil = new DateTime(2026, 11, 1, 21, 33, 54, DateTimeKind.Unspecified);

        var resultado = DateTimeHelper.BrazilToUtc(horarioBrasil);

        resultado.Should().Be(new DateTimeOffset(2026, 11, 2, 0, 33, 54, TimeSpan.Zero));
        resultado.Offset.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void BrazilToUtc_DevePreservarHorarioQueJaEstaEmUtc()
    {
        var horarioUtc = new DateTime(2026, 11, 2, 0, 33, 54, DateTimeKind.Utc);

        var resultado = DateTimeHelper.BrazilToUtc(horarioUtc);

        resultado.Should().Be(new DateTimeOffset(horarioUtc));
        resultado.Offset.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void BrazilToUtc_DeveConverterHorarioComKindLocal()
    {
        var horarioLocal = new DateTime(2026, 11, 1, 21, 33, 54, DateTimeKind.Local);

        var resultado = DateTimeHelper.BrazilToUtc(horarioLocal);

        resultado.Should().Be(new DateTimeOffset(horarioLocal.ToUniversalTime()));
        resultado.Offset.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void UTCToBrazil_DeveAplicarOffsetDeMenosTresHoras()
    {
        var horarioUtc = new DateTime(2026, 11, 2, 0, 33, 54, DateTimeKind.Utc);

        var resultado = DateTimeHelper.UTCToBrazil(horarioUtc);

        resultado.Should().Be(new DateTime(2026, 11, 1, 21, 33, 54, DateTimeKind.Unspecified));
        resultado.Kind.Should().Be(DateTimeKind.Unspecified);
    }
}
