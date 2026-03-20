-- Schema initialization for oficina_mecanica
-- Compatible with PostgreSQL

SET search_path TO public;

-- CLIENTES
CREATE TABLE IF NOT EXISTS clientes (
    id uuid PRIMARY KEY,
    nome varchar(255) NOT NULL,
    cpfcnpj varchar(20) NOT NULL,
    telefone varchar(20) NOT NULL,
    email varchar(255) NOT NULL,
    datacadastro timestamp NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_clientes_cpfcnpj 
ON clientes (cpfcnpj);

CREATE INDEX IF NOT EXISTS ix_clientes_email 
ON clientes (email);


-- VEICULOS
CREATE TABLE IF NOT EXISTS veiculos (
    id uuid PRIMARY KEY,
    placa varchar(10) NOT NULL,
    marca varchar(100) NOT NULL,
    modelo varchar(100) NOT NULL,
    ano integer NOT NULL,
    clienteid uuid NOT NULL,
    
    CONSTRAINT fk_veiculos_cliente
        FOREIGN KEY (clienteid)
        REFERENCES clientes(id)
        ON DELETE RESTRICT
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_veiculos_placa 
ON veiculos (placa);


-- SERVICOS
CREATE TABLE IF NOT EXISTS servicos (
    id uuid PRIMARY KEY,
    nome varchar(255) NOT NULL,
    descricao text NOT NULL,
    preco numeric(18,2) NOT NULL,
    tempoestimado integer NOT NULL
);


-- PECAS
CREATE TABLE IF NOT EXISTS pecas (
    id uuid PRIMARY KEY,
    nome varchar(255) NOT NULL,
    preco numeric(18,2) NOT NULL,
    quantidadeestoque integer NOT NULL
);


-- ORDENS DE SERVICO
CREATE TABLE IF NOT EXISTS ordensdeservico (
    id uuid PRIMARY KEY,
    numero varchar(50) NOT NULL,
    clienteid uuid NOT NULL,
    veiculoid uuid NOT NULL,
    status integer NOT NULL,
    dataabertura timestamp NOT NULL,
    dataconclusao timestamp NULL,
    valortotal numeric(18,2) NOT NULL,

    CONSTRAINT fk_os_cliente
        FOREIGN KEY (clienteid)
        REFERENCES clientes(id)
        ON DELETE RESTRICT,

    CONSTRAINT fk_os_veiculo
        FOREIGN KEY (veiculoid)
        REFERENCES veiculos(id)
        ON DELETE RESTRICT
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_ordensdeservico_numero 
ON ordensdeservico (numero);

CREATE INDEX IF NOT EXISTS ix_ordensdeservico_status 
ON ordensdeservico (status);


-- RELACAO OS x SERVICOS
CREATE TABLE IF NOT EXISTS ordensdeservico_servicos (
    ordemdeid uuid NOT NULL,
    servicoid uuid NOT NULL,
    preco numeric(18,2) NOT NULL,
    tempoestimado integer NOT NULL,

    CONSTRAINT pk_ordem_servico 
        PRIMARY KEY (ordemdeid, servicoid),

    CONSTRAINT fk_ordem_servico_os
        FOREIGN KEY (ordemdeid)
        REFERENCES ordensdeservico(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_ordem_servico_servico
        FOREIGN KEY (servicoid)
        REFERENCES servicos(id)
        ON DELETE RESTRICT
);


-- RELACAO OS x PECAS
CREATE TABLE IF NOT EXISTS ordensdeservico_pecas (
    ordemdeid uuid NOT NULL,
    pecaid uuid NOT NULL,
    quantidade integer NOT NULL,
    preco numeric(18,2) NOT NULL,

    CONSTRAINT pk_ordem_peca
        PRIMARY KEY (ordemdeid, pecaid),

    CONSTRAINT fk_ordem_peca_os
        FOREIGN KEY (ordemdeid)
        REFERENCES ordensdeservico(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_ordem_peca_peca
        FOREIGN KEY (pecaid)
        REFERENCES pecas(id)
        ON DELETE RESTRICT
);


-- SEED DATA

INSERT INTO clientes (id, nome, cpfcnpj, telefone, email, datacadastro)
VALUES
('11111111-1111-1111-1111-111111111111','João Silva','12345678901','(11) 99999-0000','joao@exemplo.com',now())
ON CONFLICT (id) DO NOTHING;


INSERT INTO veiculos (id, placa, marca, modelo, ano, clienteid)
VALUES
('22222222-2222-2222-2222-222222222222','ABC1D23','Ford','Ka',2018,'11111111-1111-1111-1111-111111111111')
ON CONFLICT (id) DO NOTHING;


INSERT INTO servicos (id, nome, descricao, preco, tempoestimado)
VALUES
('33333333-3333-3333-3333-333333333333','Troca de óleo','Troca de óleo do motor',120.00,30),
('33333333-3333-3333-3333-333333333334','Alinhamento','Alinhamento e balanceamento',80.00,45)
ON CONFLICT (id) DO NOTHING;


INSERT INTO pecas (id, nome, preco, quantidadeestoque)
VALUES
('44444444-4444-4444-4444-444444444444','Filtro de óleo',25.00,10),
('44444444-4444-4444-4444-444444444445','Pastilha de freio',75.00,20)
ON CONFLICT (id) DO NOTHING;


INSERT INTO ordensdeservico (id, numero, clienteid, veiculoid, status, dataabertura, valortotal)
VALUES
('55555555-5555-5555-5555-555555555555','OS-0001','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222',2,now(),145.00)
ON CONFLICT (id) DO NOTHING;


INSERT INTO ordensdeservico_servicos (ordemdeid, servicoid, preco, tempoestimado)
VALUES
('55555555-5555-5555-5555-555555555555','33333333-3333-3333-3333-333333333333',120.00,30)
ON CONFLICT (ordemdeid, servicoid) DO NOTHING;


INSERT INTO ordensdeservico_pecas (ordemdeid, pecaid, quantidade, preco)
VALUES
('55555555-5555-5555-5555-555555555555','44444444-4444-4444-4444-444444444444',1,25.00)
ON CONFLICT (ordemdeid, pecaid) DO NOTHING;