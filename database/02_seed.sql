CREATE EXTENSION IF NOT EXISTS pgcrypto;

BEGIN;


INSERT INTO regioes (nome)
VALUES
    ('Litoral'),
    ('Metropolitana'),
    ('Interior')
ON CONFLICT (nome) DO NOTHING;



INSERT INTO lojas
    (codigo, nome, gerente_nome, regiao_id)
VALUES

-- Região Litoral
(
    '101',
    'Centro',
    'Lucas Ferreira',
    (SELECT id FROM regioes WHERE nome = 'Litoral')
),
(
    '102',
    'Shopping Norte',
    'Bianca Rocha',
    (SELECT id FROM regioes WHERE nome = 'Litoral')
),
(
    '103',
    'Av. Brasil',
    'Tiago Moura',
    (SELECT id FROM regioes WHERE nome = 'Litoral')
),
(
    '104',
    'Orla',
    'Juliana Reis',
    (SELECT id FROM regioes WHERE nome = 'Litoral')
),

-- Região Metropolitana
(
    '201',
    'Galeria',
    'André Lopes',
    (SELECT id FROM regioes WHERE nome = 'Metropolitana')
),
(
    '202',
    'Terminal',
    'Patrícia Gomes',
    (SELECT id FROM regioes WHERE nome = 'Metropolitana')
),
(
    '203',
    'Shopping Sul',
    'Felipe Nunes',
    (SELECT id FROM regioes WHERE nome = 'Metropolitana')
),
(
    '204',
    'Jardins',
    'Camila Duarte',
    (SELECT id FROM regioes WHERE nome = 'Metropolitana')
),

-- Região Interior
(
    '301',
    'Rodoviária',
    'Marcos Vieira',
    (SELECT id FROM regioes WHERE nome = 'Interior')
),
(
    '302',
    'Centro Histórico',
    'Aline Martins',
    (SELECT id FROM regioes WHERE nome = 'Interior')
),
(
    '303',
    'Outlet',
    'Rafael Souza',
    (SELECT id FROM regioes WHERE nome = 'Interior')
)

ON CONFLICT (codigo) DO NOTHING;


INSERT INTO usuarios
    (login, senha_hash, perfil, ativo)
VALUES
(
    'paulo.mendes',
    crypt('Prova@123', gen_salt('bf', 12)),
    'SUPERVISOR',
    TRUE
),
(
    'renata.alves',
    crypt('Prova@123', gen_salt('bf', 12)),
    'SUPERVISOR',
    TRUE
),
(
    'diego.costa',
    crypt('Prova@123', gen_salt('bf', 12)),
    'SUPERVISOR',
    TRUE
),
(
    'helena.prado',
    crypt('Prova@123', gen_salt('bf', 12)),
    'DIRETORIA',
    TRUE
),
(
    'carla.nunes',
    crypt('Prova@123', gen_salt('bf', 12)),
    'ADMINISTRADOR',
    TRUE
),
(
    'sergio.lima',
    crypt('Prova@123', gen_salt('bf', 12)),
    'SUPERVISOR',
    FALSE
)

ON CONFLICT (login) DO NOTHING;


INSERT INTO usuarios_regioes
    (usuario_id, regiao_id)
VALUES

-- Paulo Mendes: Litoral
(
    (SELECT id FROM usuarios WHERE login = 'paulo.mendes'),
    (SELECT id FROM regioes WHERE nome = 'Litoral')
),

-- Renata Alves: Metropolitana
(
    (SELECT id FROM usuarios WHERE login = 'renata.alves'),
    (SELECT id FROM regioes WHERE nome = 'Metropolitana')
),

-- Diego Costa: Interior
(
    (SELECT id FROM usuarios WHERE login = 'diego.costa'),
    (SELECT id FROM regioes WHERE nome = 'Interior')
),

-- Sergio Lima: Interior (usuário inativo)
(
    (SELECT id FROM usuarios WHERE login = 'sergio.lima'),
    (SELECT id FROM regioes WHERE nome = 'Interior')
)

ON CONFLICT (usuario_id, regiao_id) DO NOTHING;



INSERT INTO categorias_checklist (nome, ordem)
VALUES
    ('Rotina e Gestão', 1),
    ('Apresentação e VM', 2),
    ('Estoque', 3),
    ('Precificação e Vendas', 4),
    ('Pessoas e Limpeza', 5)
ON CONFLICT (nome) DO NOTHING;



INSERT INTO itens_checklist
    (categoria_id, titulo, descricao, ordem)
VALUES

-- ROTINA E GESTÃO

(
    (SELECT id FROM categorias_checklist
     WHERE nome = 'Rotina e Gestão'),
    'Conferência do checklist da loja',
    'Checklist diário executado e assinado pelo gerente.',
    1
),
(
    (SELECT id FROM categorias_checklist
     WHERE nome = 'Rotina e Gestão'),
    'Reunião diária da equipe',
    'Reunião de abertura realizada e registrada.',
    2
),
(
    (SELECT id FROM categorias_checklist
     WHERE nome = 'Rotina e Gestão'),
    'Metas diárias e indicadores',
    'Metas divulgadas e indicadores do dia anterior tratados.',
    3
),

-- APRESENTAÇÃO E VM

(
    (SELECT id FROM categorias_checklist
     WHERE nome = 'Apresentação e VM'),
    'Fachada e vitrine',
    'Vitrine limpa, alinhada e com preços visíveis.',
    4
),
(
    (SELECT id FROM categorias_checklist
     WHERE nome = 'Apresentação e VM'),
    'Salão de vendas',
    'Pilhas abastecidas, corredores livres e espelhos limpos.',
    5
),
(
    (SELECT id FROM categorias_checklist
     WHERE nome = 'Apresentação e VM'),
    'Expositores e paredes de calçados',
    'Sem buracos de reposição e com numeração organizada.',
    6
),
(
    (SELECT id FROM categorias_checklist
     WHERE nome = 'Apresentação e VM'),
    'Conferência dos pares expostos',
    'Pares do salão conferidos (pé direito e esquerdo).',
    7
),

-- ESTOQUE

(
    (SELECT id FROM categorias_checklist
     WHERE nome = 'Estoque'),
    'Organização do estoque',
    'Organizado por seção, marca e numeração.',
    8
),
(
    (SELECT id FROM categorias_checklist
     WHERE nome = 'Estoque'),
    'Remanejos entre lojas',
    'Remanejos enviados e recebidos no prazo.',
    9
),
(
    (SELECT id FROM categorias_checklist
     WHERE nome = 'Estoque'),
    'Material de almoxarifado',
    'Sacolas, etiquetas e embalagens armazenadas corretamente.',
    10
),

-- PRECIFICAÇÃO E VENDAS

(
    (SELECT id FROM categorias_checklist
     WHERE nome = 'Precificação e Vendas'),
    'Alterações de preço aplicadas',
    'Alterações da semana aplicadas nas etiquetas.',
    11
),
(
    (SELECT id FROM categorias_checklist
     WHERE nome = 'Precificação e Vendas'),
    'Acompanhamento do atendimento',
    'Atendimento observado e feedback dado.',
    12
),
(
    (SELECT id FROM categorias_checklist
     WHERE nome = 'Precificação e Vendas'),
    'Conferência de vendas e notas fiscais',
    'Vendas do dia anterior conferidas com as notas.',
    13
),

-- PESSOAS E LIMPEZA

(
    (SELECT id FROM categorias_checklist
     WHERE nome = 'Pessoas e Limpeza'),
    'Aparência visual da equipe',
    'Equipe uniformizada e com crachá.',
    14
),
(
    (SELECT id FROM categorias_checklist
     WHERE nome = 'Pessoas e Limpeza'),
    'Limpeza da loja, copa e banheiros',
    'Ambientes limpos, inclusive as áreas internas.',
    15
);


-- =========================================================
-- 9. INSPEÇÕES DEMONSTRATIVAS
-- =========================================================

-- Tabela temporária usada apenas durante a execução do seed.
CREATE TEMP TABLE seed_inspecoes_demo (
    codigo_loja TEXT,
    login_supervisor TEXT,
    data_visita DATE,
    status_final TEXT,
    padrao TEXT,
    marcador TEXT
) ON COMMIT DROP;

INSERT INTO seed_inspecoes_demo
    (codigo_loja, login_supervisor, data_visita,
     status_final, padrao, marcador)
VALUES
    -- Litoral
    ('101', 'paulo.mendes', '2026-06-12', 'CONCLUIDA', 'BAIXO', 'DEMO-001'),
    ('102', 'paulo.mendes', '2026-07-14', 'CONCLUIDA', 'MEDIO', 'DEMO-002'),
    ('103', 'paulo.mendes', '2026-08-18', 'CONCLUIDA', 'ALTO', 'DEMO-003'),
    ('104', 'paulo.mendes', '2026-09-16', 'CONCLUIDA', 'MEDIO', 'DEMO-004'),
    ('101', 'paulo.mendes', '2026-10-21', 'CONCLUIDA', 'ALTO', 'DEMO-005'),
    ('102', 'paulo.mendes', '2026-10-28', 'RASCUNHO', 'RASCUNHO', 'DEMO-006'),

    -- Metropolitana
    ('201', 'renata.alves', '2026-07-09', 'CONCLUIDA', 'BAIXO', 'DEMO-007'),
    ('202', 'renata.alves', '2026-08-11', 'CONCLUIDA', 'MEDIO', 'DEMO-008'),
    ('203', 'renata.alves', '2026-09-22', 'CONCLUIDA', 'ALTO', 'DEMO-009'),
    ('204', 'renata.alves', '2026-10-19', 'CONCLUIDA', 'ALTO', 'DEMO-010'),
    ('201', 'renata.alves', '2026-10-27', 'RASCUNHO', 'RASCUNHO', 'DEMO-011'),

    -- Interior
    ('301', 'diego.costa', '2026-06-23', 'CONCLUIDA', 'BAIXO', 'DEMO-012'),
    ('302', 'diego.costa', '2026-08-20', 'CONCLUIDA', 'MEDIO', 'DEMO-013'),
    ('303', 'diego.costa', '2026-10-15', 'CONCLUIDA', 'ALTO', 'DEMO-014'),
    ('301', 'diego.costa', '2026-10-29', 'RASCUNHO', 'RASCUNHO', 'DEMO-015');


-- =========================================================
-- 10. CRIAÇÃO DAS INSPEÇÕES
-- =========================================================

INSERT INTO inspecoes (
    loja_id,
    regiao_id,
    gerente_nome,
    autor_id,
    data_visita,
    observacao_geral,
    status,
    criado_por,
    atualizado_por
)
SELECT
    l.id,
    l.regiao_id,
    l.gerente_nome,
    u.id,
    s.data_visita,
    'Inspeção demonstrativa - ' || s.marcador,
    'RASCUNHO',
    u.id,
    u.id
FROM seed_inspecoes_demo s
INNER JOIN lojas l
    ON l.codigo = s.codigo_loja
INNER JOIN usuarios u
    ON u.login = s.login_supervisor
WHERE NOT EXISTS (
    SELECT 1
    FROM inspecoes i
    WHERE i.observacao_geral =
        'Inspeção demonstrativa - ' || s.marcador
);


-- =========================================================
-- 11. RESPOSTAS E SNAPSHOTS DOS ITENS
-- =========================================================

WITH itens_ordenados AS (
    SELECT
        i.id,
        i.titulo,
        i.descricao,
        i.ordem,
        i.categoria_id,
        c.nome AS categoria_nome,
        c.ordem AS categoria_ordem,
        ROW_NUMBER() OVER (
            ORDER BY c.ordem, i.ordem, i.id
        ) AS numero
    FROM itens_checklist i
    INNER JOIN categorias_checklist c
        ON c.id = i.categoria_id
    WHERE i.ativo = TRUE
      AND c.ativo = TRUE
),
respostas_geradas AS (
    SELECT
        ins.id AS inspecao_id,
        ins.autor_id,
        it.id AS item_id,
        it.categoria_nome,
        it.categoria_ordem,
        it.titulo,
        it.descricao,
        it.ordem,

        CASE
            WHEN s.status_final = 'RASCUNHO'
                 AND it.numero % 3 = 0
                THEN NULL

            WHEN it.numero % 11 = 0
                THEN 'NA'

            WHEN s.padrao = 'BAIXO'
                 AND it.numero % 2 = 0
                THEN 'PENDENTE'

            WHEN s.padrao = 'MEDIO'
                 AND it.numero % 4 = 0
                THEN 'PENDENTE'

            WHEN s.padrao = 'ALTO'
                 AND it.numero % 9 = 0
                THEN 'PENDENTE'

            WHEN s.padrao = 'RASCUNHO'
                 AND it.numero % 5 = 0
                THEN 'PENDENTE'

            ELSE 'OK'
        END AS resposta
    FROM seed_inspecoes_demo s
    INNER JOIN inspecoes ins
        ON ins.observacao_geral =
            'Inspeção demonstrativa - ' || s.marcador
    CROSS JOIN itens_ordenados it
)
INSERT INTO respostas_inspecao (
    inspecao_id,
    item_id,
    categoria_nome,
    categoria_ordem,
    item_titulo,
    item_descricao,
    item_ordem,
    resposta,
    observacao,
    criado_por,
    atualizado_por
)
SELECT
    r.inspecao_id,
    r.item_id,
    r.categoria_nome,
    r.categoria_ordem,
    r.titulo,
    r.descricao,
    r.ordem,
    r.resposta,

    CASE
        WHEN r.resposta = 'PENDENTE'
            THEN 'Ajuste necessário. Responsável deverá corrigir a pendência.'
        ELSE NULL
    END,

    r.autor_id,
    r.autor_id
FROM respostas_geradas r
ON CONFLICT (inspecao_id, item_id) DO NOTHING;


-- =========================================================
-- 12. CONCLUSÃO E CÁLCULO DOS PERCENTUAIS
-- =========================================================

UPDATE inspecoes ins
SET
    status = 'CONCLUIDA',

    percentual_realizacao = (
        SELECT ROUND(
            100.0 * COUNT(*) FILTER (
                WHERE r.resposta = 'OK'
            ) / NULLIF(
                COUNT(*) FILTER (
                    WHERE r.resposta IN ('OK', 'PENDENTE')
                ),
                0
            ),
            1
        )
        FROM respostas_inspecao r
        WHERE r.inspecao_id = ins.id
    ),

    concluida_em =
        s.data_visita::timestamp + INTERVAL '18 hours',

    atualizado_em = NOW(),
    atualizado_por = ins.autor_id

FROM seed_inspecoes_demo s
WHERE ins.observacao_geral =
    'Inspeção demonstrativa - ' || s.marcador
  AND s.status_final = 'CONCLUIDA'
  AND ins.status = 'RASCUNHO';


COMMIT;
