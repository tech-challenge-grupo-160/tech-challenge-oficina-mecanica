SET search_path TO public;

-- Cliente
CREATE TABLE IF NOT EXISTS "Cliente" (
    "Id" uuid PRIMARY KEY,
    "Nome" varchar(255) NOT NULL,
    "CpfCnpj" varchar(20) NOT NULL,
    "Telefone" varchar(20) NOT NULL,
    "Email" varchar(255) NOT NULL,
    "DataCadastro" timestamp NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS "UxClienteCpfCnpj" 
ON "Cliente" ("CpfCnpj");

CREATE INDEX IF NOT EXISTS "IxClienteEmail" 
ON "Cliente" ("Email");


-- Veiculo
CREATE TABLE IF NOT EXISTS "Veiculo" (
    "Id" uuid PRIMARY KEY,
    "Placa" varchar(10) NOT NULL,
    "Marca" varchar(100) NOT NULL,
    "Modelo" varchar(100) NOT NULL,
    "Ano" integer NOT NULL,
    "ClienteId" uuid NOT NULL,
    
    CONSTRAINT "FkVeiculoCliente"
        FOREIGN KEY ("ClienteId")
        REFERENCES "Cliente"("Id")
        ON DELETE RESTRICT
);

CREATE UNIQUE INDEX IF NOT EXISTS "UxVeiculoPlaca" 
ON "Veiculo" ("Placa");


-- Servico
CREATE TABLE IF NOT EXISTS "Servico" (
    "Id" uuid PRIMARY KEY,
    "Nome" varchar(255) NOT NULL,
    "Descricao" text NOT NULL,
    "Preco" numeric(18,2) NOT NULL,
    "TempoEstimadoMinutos" integer NOT NULL
);


-- Peca
CREATE TABLE IF NOT EXISTS "Peca" (
    "Id" uuid PRIMARY KEY,
    "Nome" varchar(255) NOT NULL,
    "Preco" numeric(18,2) NOT NULL,
    "QuantidadeEstoque" integer NOT NULL
);


-- OrdemServico
CREATE TABLE IF NOT EXISTS "OrdemServico" (
    "Id" uuid PRIMARY KEY,
    "Numero" varchar(50) NOT NULL,
    "ClienteId" uuid NOT NULL,
    "VeiculoId" uuid NOT NULL,
    "Status" integer NOT NULL,
    "DataAbertura" timestamp NOT NULL,
    "DataConclusao" timestamp NULL,
    "ValorTotal" numeric(18,2) NOT NULL,

    CONSTRAINT "FkOrdemServicoCliente"
        FOREIGN KEY ("ClienteId")
        REFERENCES "Cliente"("Id")
        ON DELETE RESTRICT,

    CONSTRAINT "FkOrdemServicoVeiculo"
        FOREIGN KEY ("VeiculoId")
        REFERENCES "Veiculo"("Id")
        ON DELETE RESTRICT
);

CREATE UNIQUE INDEX IF NOT EXISTS "UxOrdemServicoNumero" 
ON "OrdemServico" ("Numero");

CREATE INDEX IF NOT EXISTS "IxOrdemServicoStatus" 
ON "OrdemServico" ("Status");


-- OrdemServico x Servico
CREATE TABLE IF NOT EXISTS "OrdemServicoItemServico" (
    "OrdemServicoId" uuid NOT NULL,
    "ServicoId" uuid NOT NULL,
    "Preco" numeric(18,2) NOT NULL,
    "TempoEstimadoMinutos" integer NOT NULL,

    CONSTRAINT "PkOrdemServicoItemServico" 
        PRIMARY KEY ("OrdemServicoId", "ServicoId"),

    CONSTRAINT "FkItemServicoOrdemServico"
        FOREIGN KEY ("OrdemServicoId")
        REFERENCES "OrdemServico"("Id")
        ON DELETE CASCADE,

    CONSTRAINT "FkItemServicoServico"
        FOREIGN KEY ("ServicoId")
        REFERENCES "Servico"("Id")
        ON DELETE RESTRICT
);


-- OrdemServico x Peca
CREATE TABLE IF NOT EXISTS "OrdemServicoItemPeca" (
    "OrdemServicoId" uuid NOT NULL,
    "PecaId" uuid NOT NULL,
    "Quantidade" integer NOT NULL,
    "Preco" numeric(18,2) NOT NULL,

    CONSTRAINT "PkOrdemServicoItemPeca"
        PRIMARY KEY ("OrdemServicoId", "PecaId"),

    CONSTRAINT "FkItemPecaOrdemServico"
        FOREIGN KEY ("OrdemServicoId")
        REFERENCES "OrdemServico"("Id")
        ON DELETE CASCADE,

    CONSTRAINT "FkItemPecaPeca"
        FOREIGN KEY ("PecaId")
        REFERENCES "Peca"("Id")
        ON DELETE RESTRICT
);

-- SEED DATA

INSERT INTO "Cliente" ("Id", "Nome", "CpfCnpj", "Telefone", "Email", "DataCadastro")
VALUES
('11111111-1111-1111-1111-111111111111','João Silva','12345678901','(11) 99999-0000','joao@exemplo.com', now())
ON CONFLICT ("Id") DO NOTHING;


INSERT INTO "Veiculo" ("Id", "Placa", "Marca", "Modelo", "Ano", "ClienteId")
VALUES
('22222222-2222-2222-2222-222222222222','ABC1D23','Ford','Ka',2018,'11111111-1111-1111-1111-111111111111')
ON CONFLICT ("Id") DO NOTHING;


INSERT INTO "Servico" ("Id", "Nome", "Descricao", "Preco", "TempoEstimadoMinutos")
VALUES
('33333333-3333-3333-3333-333333333333','Troca de óleo','Troca de óleo do motor',120.00,30),
('33333333-3333-3333-3333-333333333334','Alinhamento','Alinhamento e balanceamento',80.00,45)
ON CONFLICT ("Id") DO NOTHING;


INSERT INTO "Peca" ("Id", "Nome", "Preco", "QuantidadeEstoque")
VALUES
('44444444-4444-4444-4444-444444444444','Filtro de óleo',25.00,10),
('44444444-4444-4444-4444-444444444445','Pastilha de freio',75.00,20)
ON CONFLICT ("Id") DO NOTHING;


INSERT INTO "OrdemServico" 
("Id", "Numero", "ClienteId", "VeiculoId", "Status", "DataAbertura", "ValorTotal")
VALUES
('55555555-5555-5555-5555-555555555555','OS-0001','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222',2, now(),145.00)
ON CONFLICT ("Id") DO NOTHING;


INSERT INTO "OrdemServicoItemServico" 
("OrdemServicoId", "ServicoId", "Preco", "TempoEstimadoMinutos")
VALUES
('55555555-5555-5555-5555-555555555555','33333333-3333-3333-3333-333333333333',120.00,30)
ON CONFLICT ("OrdemServicoId", "ServicoId") DO NOTHING;


INSERT INTO "OrdemServicoItemPeca" 
("OrdemServicoId", "PecaId", "Quantidade", "Preco")
VALUES
('55555555-5555-5555-5555-555555555555','44444444-4444-4444-4444-444444444444',1,25.00)
ON CONFLICT ("OrdemServicoId", "PecaId") DO NOTHING;
