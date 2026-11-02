using Fiap.TechChallenge.OficinaMecanica.Application.Messaging;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using FluentAssertions;

namespace Fiap.TechChallenge.OficinaMecanica.Test.UnitTests.Units.Messaging;

public class MessagingDomainTests
{
    [Fact]
    public void InboxMessage_CriarDevePreservarMetadadosDoEvento()
    {
        var messageId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();
        var processadoEm = DateTimeOffset.UtcNow;

        var inbox = InboxMessage.Criar(messageId, correlationId, "DiagnosticoIniciado", processadoEm);

        inbox.MessageId.Should().Be(messageId);
        inbox.CorrelationId.Should().Be(correlationId);
        inbox.Tipo.Should().Be("DiagnosticoIniciado");
        inbox.ProcessadoEm.Should().Be(processadoEm);
    }

    [Fact]
    public void OutboxMessage_DeveRegistrarFalhaEPublicacao()
    {
        var criadoEm = DateTimeOffset.UtcNow;
        var outbox = OutboxMessage.Criar(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "IniciarDiagnostico",
            "ExecucaoIniciarDiagnostico",
            "{\"ordemServicoId\":3007}",
            criadoEm);

        outbox.CriadoEm.Should().Be(criadoEm);
        outbox.Tentativas.Should().Be(0);
        outbox.PublicadoEm.Should().BeNull();

        outbox.RegistrarFalha("falha temporaria");

        outbox.Tentativas.Should().Be(1);
        outbox.UltimoErro.Should().Be("falha temporaria");

        var publicadoEm = criadoEm.AddSeconds(1);
        outbox.MarcarPublicada(publicadoEm);

        outbox.PublicadoEm.Should().Be(publicadoEm);
        outbox.UltimoErro.Should().BeNull();
    }

    [Fact]
    public void OutboxMessage_DeveLimitarTamanhoDoUltimoErro()
    {
        var outbox = OutboxMessage.Criar(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "IniciarDiagnostico",
            "ExecucaoIniciarDiagnostico",
            "{}",
            DateTimeOffset.UtcNow);

        outbox.RegistrarFalha(new string('x', 2100));

        outbox.UltimoErro.Should().HaveLength(2000);
    }

    [Fact]
    public void SagaInicioDiagnostico_DevePercorrerEstadoPendenteAteConcluida()
    {
        var correlationId = Guid.NewGuid();
        var criadaEm = DateTimeOffset.UtcNow;
        var saga = SagaInicioDiagnostico.Criar(correlationId, 3007, criadaEm);

        saga.CorrelationId.Should().Be(correlationId);
        saga.OrdemServicoId.Should().Be(3007);
        saga.Status.Should().Be("Pendente");
        saga.DiagnosticoId.Should().BeNull();
        saga.CriadaEm.Should().Be(criadaEm);
        saga.AtualizadaEm.Should().Be(criadaEm);

        var concluidaEm = criadaEm.AddSeconds(2);
        saga.Concluir(1, concluidaEm);

        saga.Status.Should().Be("Concluida");
        saga.DiagnosticoId.Should().Be(1);
        saga.AtualizadaEm.Should().Be(concluidaEm);
    }

    [Fact]
    public void ContratosDeMensageria_DevemPreservarVersaoECorrelacao()
    {
        var messageId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();
        var ocorridoEm = DateTimeOffset.UtcNow;
        var iniciar = new IniciarDiagnosticoMessage(
            messageId,
            correlationId,
            ocorridoEm,
            3007,
            1004,
            "Ruido durante a frenagem",
            [1000],
            [1000]);
        var iniciado = new DiagnosticoIniciadoMessage(
            Guid.NewGuid(),
            correlationId,
            ocorridoEm,
            3007,
            1,
            "EmDiagnostico",
            ocorridoEm);

        iniciar.MessageId.Should().Be(messageId);
        iniciar.CorrelationId.Should().Be(correlationId);
        iniciar.Versao.Should().Be(1);
        iniciar.ServicoIdsSolicitados.Should().ContainSingle().Which.Should().Be(1000);
        iniciado.CorrelationId.Should().Be(correlationId);
        iniciado.Status.Should().Be("EmDiagnostico");
        iniciado.Versao.Should().Be(1);
    }
}
