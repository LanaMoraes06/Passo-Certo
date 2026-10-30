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

COMMIT;
