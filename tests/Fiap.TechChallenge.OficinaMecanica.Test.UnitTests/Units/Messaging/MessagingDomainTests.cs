using Fiap.TechChallenge.OficinaMecanica.Application.Messaging;
using Fiap.TechChallenge.OficinaMecanica.Domain.Entities;
using Fiap.TechChallenge.OficinaMecanica.Infrastructure.Messaging;
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

        var orcamentoId = Guid.NewGuid();
        saga.ConfirmarOrcamento(orcamentoId, concluidaEm.AddSeconds(1));
        saga.OrcamentoId.Should().Be(orcamentoId);
        saga.Status.Should().Be("AguardandoAprovacao");
        saga.MarcarReservandoPecas(concluidaEm.AddSeconds(2));
        saga.Status.Should().Be("ReservandoPecas");
        saga.MarcarAguardandoEstoque(concluidaEm.AddSeconds(3));
        saga.Status.Should().Be("AguardandoEstoque");
        saga.MarcarEmExecucao(concluidaEm.AddSeconds(4));
        saga.Status.Should().Be("EmExecucao");
        saga.MarcarAguardandoPagamento(concluidaEm.AddSeconds(5));
        saga.Status.Should().Be("AguardandoPagamento");
        saga.MarcarAguardandoEntrega(concluidaEm.AddSeconds(6));
        saga.Status.Should().Be("AguardandoEntrega");
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

    [Fact]
    public void DiagnosticoConcluidoMessage_DevePreservarContratoCompleto()
    {
        var messageId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();
        var concluidoEm = DateTimeOffset.UtcNow;
        var servico = new ServicoDiagnosticoConcluidoMessage(
            1000,
            "Alinhamento",
            150m,
            30);
        var peca = new PecaDiagnosticoConcluidoMessage(
            1000,
            "Pastilha de Freio",
            2,
            45m);

        var evento = new DiagnosticoConcluidoMessage(
            messageId,
            correlationId,
            concluidoEm,
            3007,
            1,
            "Concluido",
            "Pastilhas desgastadas.",
            [servico],
            [peca],
            concluidoEm);

        evento.MessageId.Should().Be(messageId);
        evento.CorrelationId.Should().Be(correlationId);
        evento.OcorridoEm.Should().Be(concluidoEm);
        evento.OrdemServicoId.Should().Be(3007);
        evento.DiagnosticoId.Should().Be(1);
        evento.Status.Should().Be("Concluido");
        evento.ParecerTecnico.Should().Be("Pastilhas desgastadas.");
        evento.Servicos.Should().ContainSingle().Which.Should().Be(servico);
        evento.Pecas.Should().ContainSingle().Which.Should().Be(peca);
        evento.ConcluidoEm.Should().Be(concluidoEm);
        evento.Versao.Should().Be(1);
        servico.Should().BeEquivalentTo(new
        {
            ServicoId = 1000,
            Nome = "Alinhamento",
            Preco = 150m,
            TempoEstimado = 30
        });
        peca.Should().BeEquivalentTo(new
        {
            PecaId = 1000,
            Nome = "Pastilha de Freio",
            Quantidade = 2,
            Preco = 45m
        });
    }

    [Fact]
    public void MessagingOptions_DevePreservarFilaDeDiagnosticoConcluido()
    {
        var options = new MessagingOptions
        {
            DiagnosticoConcluidoQueueUrl = "http://localhost:4566/os-diagnostico-concluido"
        };

        options.DiagnosticoConcluidoQueueUrl.Should()
            .Be("http://localhost:4566/os-diagnostico-concluido");
    }

    [Fact]
    public void ContratosDeOrcamento_DevemPreservarComandoEEvento()
    {
        var correlationId = Guid.NewGuid();
        var agora = DateTimeOffset.UtcNow;
        var comando = new GerarOrcamentoMessage(
            Guid.NewGuid(), correlationId, agora, 3007, 1, "Parecer tecnico",
            [new ServicoGerarOrcamentoMessage(1000, "Alinhamento", 150m, 30)],
            [new PecaGerarOrcamentoMessage(1000, "Pastilha", 2, 45m)],
            agora);
        var evento = new OrcamentoGeradoMessage(
            Guid.NewGuid(), correlationId, agora, Guid.NewGuid(), 3007, 1,
            "Gerado", 150m, 90m, 240m);

        comando.CorrelationId.Should().Be(correlationId);
        comando.Servicos.Should().ContainSingle();
        comando.Pecas.Should().ContainSingle();
        comando.Versao.Should().Be(1);
        evento.CorrelationId.Should().Be(correlationId);
        evento.Status.Should().Be("Gerado");
        evento.ValorTotal.Should().Be(240m);
        evento.Versao.Should().Be(1);
    }

    [Fact]
    public void MessagingOptions_DevePreservarFilasDoBilling()
    {
        var options = new MessagingOptions
        {
            BillingCommandQueueUrl = "http://localhost:4566/billing-gerar-orcamento",
            OrcamentoGeradoQueueUrl = "http://localhost:4566/os-orcamento-gerado"
        };

        options.BillingCommandQueueUrl.Should().EndWith("billing-gerar-orcamento");
        options.OrcamentoGeradoQueueUrl.Should().EndWith("os-orcamento-gerado");
    }

    [Fact]
    public void ContratosDeAprovacaoEReserva_DevemPreservarTodosOsDados()
    {
        var correlationId = Guid.NewGuid();
        var orcamentoId = Guid.NewGuid();
        var ocorridoEm = DateTimeOffset.UtcNow;
        var itemSolicitado = new PecaReservarMessage(1002, 4);
        var itemResultado = new PecaReservaResultadoMessage(1002, 4, 0, 4);
        var aprovado = new OrcamentoAprovadoMessage(
            Guid.NewGuid(), correlationId, ocorridoEm, orcamentoId,
            3007, 1, "Aprovado", 339.60m);
        var recusado = new OrcamentoRecusadoMessage(
            Guid.NewGuid(), correlationId, ocorridoEm, orcamentoId,
            3007, 1, "Recusado", "Valor nao aprovado.");
        var reservar = new ReservarPecasMessage(
            Guid.NewGuid(), correlationId, ocorridoEm, 3007,
            orcamentoId, [itemSolicitado]);
        var reservadas = new PecasReservadasMessage(
            Guid.NewGuid(), correlationId, ocorridoEm, 3007,
            orcamentoId, [itemResultado]);
        var insuficiente = new EstoqueInsuficienteMessage(
            Guid.NewGuid(), correlationId, ocorridoEm, 3007,
            orcamentoId, [itemResultado]);

        aprovado.Should().BeEquivalentTo(new
        {
            CorrelationId = correlationId,
            OcorridoEm = ocorridoEm,
            OrcamentoId = orcamentoId,
            OrdemServicoId = 3007,
            DiagnosticoId = 1,
            Status = "Aprovado",
            ValorTotal = 339.60m,
            Versao = 1
        });
        recusado.Should().BeEquivalentTo(new
        {
            CorrelationId = correlationId,
            OcorridoEm = ocorridoEm,
            OrcamentoId = orcamentoId,
            OrdemServicoId = 3007,
            DiagnosticoId = 1,
            Status = "Recusado",
            Motivo = "Valor nao aprovado.",
            Versao = 1
        });
        reservar.Pecas.Should().ContainSingle().Which.Should().Be(itemSolicitado);
        reservar.OrdemServicoId.Should().Be(3007);
        reservar.OrcamentoId.Should().Be(orcamentoId);
        reservar.Versao.Should().Be(1);
        itemSolicitado.PecaId.Should().Be(1002);
        itemSolicitado.Quantidade.Should().Be(4);
        reservadas.Pecas.Should().ContainSingle().Which.Should().Be(itemResultado);
        reservadas.CorrelationId.Should().Be(correlationId);
        reservadas.OcorridoEm.Should().Be(ocorridoEm);
        reservadas.OrdemServicoId.Should().Be(3007);
        reservadas.OrcamentoId.Should().Be(orcamentoId);
        reservadas.Versao.Should().Be(1);
        insuficiente.Pecas.Should().ContainSingle().Which.Should().Be(itemResultado);
        insuficiente.CorrelationId.Should().Be(correlationId);
        insuficiente.OcorridoEm.Should().Be(ocorridoEm);
        insuficiente.OrdemServicoId.Should().Be(3007);
        insuficiente.OrcamentoId.Should().Be(orcamentoId);
        insuficiente.Versao.Should().Be(1);
        itemResultado.Should().BeEquivalentTo(new
        {
            PecaId = 1002,
            QuantidadeSolicitada = 4,
            QuantidadeReservada = 0,
            QuantidadeFaltante = 4
        });
    }

    [Fact]
    public void MessagingOptions_DevePreservarFilasDeAprovacaoEReserva()
    {
        var options = new MessagingOptions
        {
            OrcamentoAprovadoQueueUrl = "http://localhost:4566/os-orcamento-aprovado",
            OrcamentoRecusadoQueueUrl = "http://localhost:4566/os-orcamento-recusado",
            ReservaPecasCommandQueueUrl = "http://localhost:4566/execucao-reservar-pecas",
            PecasReservadasQueueUrl = "http://localhost:4566/os-pecas-reservadas",
            EstoqueInsuficienteQueueUrl = "http://localhost:4566/os-estoque-insuficiente"
        };

        options.OrcamentoAprovadoQueueUrl.Should().EndWith("os-orcamento-aprovado");
        options.OrcamentoRecusadoQueueUrl.Should().EndWith("os-orcamento-recusado");
        options.ReservaPecasCommandQueueUrl.Should().EndWith("execucao-reservar-pecas");
        options.PecasReservadasQueueUrl.Should().EndWith("os-pecas-reservadas");
        options.EstoqueInsuficienteQueueUrl.Should().EndWith("os-estoque-insuficiente");
    }

    [Fact]
    public void ContratosDeExecucao_DevemPreservarIdentidadeEEstado()
    {
        var correlationId = Guid.NewGuid();
        var orcamentoId = Guid.NewGuid();
        var agora = DateTimeOffset.UtcNow;
        var iniciar = new IniciarExecucaoMessage(
            Guid.NewGuid(), correlationId, agora, 3007, 1, orcamentoId);
        var finalizada = new ExecucaoFinalizadaMessage(
            Guid.NewGuid(), correlationId, agora, 3007, 1, orcamentoId,
            "Finalizada", agora);
        var options = new MessagingOptions
        {
            IniciarExecucaoCommandQueueUrl = "http://localhost:4566/execucao-iniciar-reparo",
            ExecucaoFinalizadaQueueUrl = "http://localhost:4566/os-execucao-finalizada"
        };

        iniciar.OrdemServicoId.Should().Be(3007);
        iniciar.DiagnosticoId.Should().Be(1);
        iniciar.OrcamentoId.Should().Be(orcamentoId);
        iniciar.Versao.Should().Be(1);
        finalizada.Status.Should().Be("Finalizada");
        finalizada.FinalizadaEm.Should().Be(agora);
        finalizada.Versao.Should().Be(1);
        options.IniciarExecucaoCommandQueueUrl.Should().EndWith("execucao-iniciar-reparo");
        options.ExecucaoFinalizadaQueueUrl.Should().EndWith("os-execucao-finalizada");
    }
}
