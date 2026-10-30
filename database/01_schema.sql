BEGIN;

-- 1. REGIÕES

CREATE TABLE regioes (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    nome VARCHAR(100) NOT NULL UNIQUE,
    ativo BOOLEAN NOT NULL DEFAULT TRUE,
    criado_em TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    atualizado_em TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 2. LOJAS

CREATE TABLE lojas (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    codigo VARCHAR(10) NOT NULL UNIQUE,
    nome VARCHAR(150) NOT NULL,
    gerente_nome VARCHAR(150) NOT NULL,
    regiao_id BIGINT NOT NULL,
    ativo BOOLEAN NOT NULL DEFAULT TRUE,
    criado_em TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    atualizado_em TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT fk_lojas_regiao
        FOREIGN KEY (regiao_id)
        REFERENCES regioes(id)
        ON DELETE RESTRICT
);

-- 3. USUÁRIOS

CREATE TABLE usuarios (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    login VARCHAR(100) NOT NULL UNIQUE,
    senha_hash TEXT NOT NULL,
    perfil VARCHAR(20) NOT NULL,
    ativo BOOLEAN NOT NULL DEFAULT TRUE,
    criado_em TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    atualizado_em TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT ck_usuarios_perfil
        CHECK (perfil IN (
            'SUPERVISOR',
            'DIRETORIA',
            'ADMINISTRADOR'
        ))
);

-- 4. RELACIONAMENTO USUÁRIOS / REGIÕES

CREATE TABLE usuarios_regioes (
    usuario_id BIGINT NOT NULL,
    regiao_id BIGINT NOT NULL,

    PRIMARY KEY (usuario_id, regiao_id),

    CONSTRAINT fk_ur_usuario
        FOREIGN KEY (usuario_id)
        REFERENCES usuarios(id)
        ON DELETE RESTRICT,

    CONSTRAINT fk_ur_regiao
        FOREIGN KEY (regiao_id)
        REFERENCES regioes(id)
        ON DELETE RESTRICT
);

-- 5. CATEGORIAS DO CHECKLIST

CREATE TABLE categorias_checklist (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    nome VARCHAR(150) NOT NULL UNIQUE,
    ordem INTEGER NOT NULL,
    ativo BOOLEAN NOT NULL DEFAULT TRUE,
    criado_em TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    atualizado_em TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT ck_categoria_ordem
        CHECK (ordem > 0)
);

-- 6. ITENS DO CHECKLIST

CREATE TABLE itens_checklist (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    categoria_id BIGINT NOT NULL,
    titulo VARCHAR(200) NOT NULL,
    descricao TEXT NOT NULL,
    ordem INTEGER NOT NULL,
    ativo BOOLEAN NOT NULL DEFAULT TRUE,
    criado_em TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    atualizado_em TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT fk_item_categoria
        FOREIGN KEY (categoria_id)
        REFERENCES categorias_checklist(id)
        ON DELETE RESTRICT,

    CONSTRAINT ck_item_ordem
        CHECK (ordem > 0)
);

-- 7. INSPEÇÕES

CREATE TABLE inspecoes (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    loja_id BIGINT NOT NULL,
    regiao_id BIGINT NOT NULL,
    gerente_nome VARCHAR(150) NOT NULL,
    autor_id BIGINT NOT NULL,
    data_visita DATE NOT NULL,
    observacao_geral TEXT,
    status VARCHAR(20) NOT NULL DEFAULT 'RASCUNHO',
    percentual_realizacao NUMERIC(5,2),
    concluida_em TIMESTAMPTZ,
    criado_em TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    atualizado_em TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    criado_por BIGINT NOT NULL,
    atualizado_por BIGINT NOT NULL,

    CONSTRAINT fk_inspecao_loja
        FOREIGN KEY (loja_id)
        REFERENCES lojas(id)
        ON DELETE RESTRICT,

    CONSTRAINT fk_inspecao_regiao
        FOREIGN KEY (regiao_id)
        REFERENCES regioes(id)
        ON DELETE RESTRICT,

    CONSTRAINT fk_inspecao_autor
        FOREIGN KEY (autor_id)
        REFERENCES usuarios(id)
        ON DELETE RESTRICT,

    CONSTRAINT fk_inspecao_criado_por
        FOREIGN KEY (criado_por)
        REFERENCES usuarios(id)
        ON DELETE RESTRICT,

    CONSTRAINT fk_inspecao_atualizado_por
        FOREIGN KEY (atualizado_por)
        REFERENCES usuarios(id)
        ON DELETE RESTRICT,

    CONSTRAINT ck_inspecao_status
        CHECK (status IN ('RASCUNHO', 'CONCLUIDA')),

    CONSTRAINT ck_inspecao_percentual
        CHECK (
            percentual_realizacao IS NULL
            OR percentual_realizacao BETWEEN 0 AND 100
        ),

    CONSTRAINT ck_inspecao_conclusao
        CHECK (
            (status = 'RASCUNHO' AND concluida_em IS NULL)
            OR
            (status = 'CONCLUIDA' AND concluida_em IS NOT NULL)
        )
);

-- 8. RESPOSTAS DA INSPEÇÃO

CREATE TABLE respostas_inspecao (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    inspecao_id BIGINT NOT NULL,
    item_id BIGINT NOT NULL,

    -- Snapshot do checklist
    categoria_nome VARCHAR(150) NOT NULL,
    categoria_ordem INTEGER NOT NULL,
    item_titulo VARCHAR(200) NOT NULL,
    item_descricao TEXT NOT NULL,
    item_ordem INTEGER NOT NULL,

    resposta VARCHAR(10),
    observacao TEXT,

    criado_em TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    atualizado_em TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    criado_por BIGINT NOT NULL,
    atualizado_por BIGINT NOT NULL,

    CONSTRAINT fk_resposta_inspecao
        FOREIGN KEY (inspecao_id)
        REFERENCES inspecoes(id)
        ON DELETE RESTRICT,

    CONSTRAINT fk_resposta_item
        FOREIGN KEY (item_id)
        REFERENCES itens_checklist(id)
        ON DELETE RESTRICT,

    CONSTRAINT fk_resposta_criado_por
        FOREIGN KEY (criado_por)
        REFERENCES usuarios(id)
        ON DELETE RESTRICT,

    CONSTRAINT fk_resposta_atualizado_por
        FOREIGN KEY (atualizado_por)
        REFERENCES usuarios(id)
        ON DELETE RESTRICT,

    CONSTRAINT uq_resposta_inspecao_item
        UNIQUE (inspecao_id, item_id),

    CONSTRAINT ck_resposta_valor
        CHECK (
            resposta IS NULL
            OR resposta IN ('OK', 'PENDENTE', 'NA')
        ),

    CONSTRAINT ck_resposta_observacao
        CHECK (
            resposta IS DISTINCT FROM 'PENDENTE'
            OR NULLIF(BTRIM(observacao), '') IS NOT NULL
        )
);

-- Índices úteis para filtros e relacionamentos

CREATE INDEX idx_lojas_regiao
    ON lojas(regiao_id);

CREATE INDEX idx_usuarios_regioes_regiao
    ON usuarios_regioes(regiao_id);

CREATE INDEX idx_itens_categoria
    ON itens_checklist(categoria_id);

CREATE INDEX idx_inspecoes_data
    ON inspecoes(data_visita);

CREATE INDEX idx_inspecoes_loja_data
    ON inspecoes(loja_id, data_visita);

CREATE INDEX idx_inspecoes_regiao_data
    ON inspecoes(regiao_id, data_visita);

CREATE INDEX idx_inspecoes_autor
    ON inspecoes(autor_id);

CREATE INDEX idx_respostas_item
    ON respostas_inspecao(item_id);

COMMIT;
