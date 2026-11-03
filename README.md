# 👟 PassoCerto — Sistema de Inspeções de Lojas

Sistema web desenvolvido em **ASP.NET Core 8 com Razor Pages**, voltado ao gerenciamento de inspeções operacionais em lojas de varejo.

O projeto permite que supervisores realizem inspeções por meio de checklists, acompanhem pendências e consultem históricos. A diretoria pode visualizar indicadores de desempenho, enquanto administradores gerenciam usuários, regiões, lojas e itens do checklist.

A aplicação utiliza PostgreSQL para persistência de dados, Npgsql para acesso ao banco e Docker Compose para facilitar a execução do ambiente.

## 📋 Sobre o projeto

O PassoCerto foi desenvolvido como parte de uma prova técnica para Desenvolvedor Júnior.

Seu objetivo é oferecer uma solução para padronizar inspeções nas unidades de uma rede varejista, garantindo rastreabilidade, controle de acesso e acompanhamento dos resultados.

### Objetivos

- Digitalizar as inspeções operacionais das lojas.
- Organizar os itens de inspeção em categorias.
- Registrar conformidades e pendências.
- Restringir o acesso conforme o perfil do usuário.
- Preservar o histórico das inspeções concluídas.
- Disponibilizar indicadores gerenciais.
- Facilitar a implantação por meio do Docker.

## 🛠️ Tecnologias utilizadas

| Tecnologia | Finalidade |
|---|---|
| C# | Linguagem de programação |
| .NET 8 | Plataforma de desenvolvimento |
| ASP.NET Core Razor Pages | Interface web e processamento das requisições |
| PostgreSQL 18 | Banco de dados relacional |
| Npgsql | Acesso ao PostgreSQL |
| SQL | Consultas e operações de persistência |
| BCrypt | Hash de senhas |
| Cookie Authentication | Autenticação dos usuários |
| Bootstrap | Interface responsiva |
| Docker | Containerização da aplicação |
| Docker Compose | Orquestração dos serviços |
| xUnit | Estrutura para testes automatizados |

O acesso ao banco é realizado por meio de consultas SQL parametrizadas, sem dependência do Entity Framework Core para as operações de persistência.

## ✨ Funcionalidades

### Autenticação e autorização

- Login com usuário e senha.
- Senhas armazenadas por meio de hash.
- Autenticação baseada em cookies.
- Controle de acesso por perfil.
- Proteção das páginas administrativas.
- Restrição de acesso a usuários inativos.

### Gerenciamento de inspeções

- Criação de inspeções para lojas autorizadas.
- Registro da data da visita.
- Checklist dividido em categorias.
- Respostas `OK`, `PENDENTE` e `NA`.
- Registro de observações para pendências.
- Salvamento de inspeções em rascunho.
- Conclusão após preenchimento obrigatório.
- Cálculo automático do percentual de realização.
- Preservação dos dados históricos das inspeções concluídas.

### Histórico de inspeções

- Consulta de inspeções realizadas.
- Filtros para localização de registros.
- Visualização de detalhes e respostas.
- Identificação de pendências.
- Consulta de resultados anteriores.

### Dashboard

- Indicadores de inspeções.
- Percentuais de realização.
- Acompanhamento de pendências.
- Análise por loja e região.
- Visualização de informações conforme as permissões do usuário.
- Acompanhamento da evolução mensal dos resultados.

### Administração

O perfil de administrador possui funcionalidades de gerenciamento de:

- Regiões.
- Lojas.
- Usuários.
- Vínculos entre supervisores e regiões.
- Categorias do checklist.
- Itens do checklist.

Os cadastros utilizam desativação lógica para preservar a integridade referencial e os registros históricos.

## 👥 Perfis de acesso

O sistema possui três perfis principais.

| Perfil | Permissões |
|---|---|
| `SUPERVISOR` | Realizar e consultar inspeções das regiões autorizadas |
| `DIRETORIA` | Consultar indicadores e informações gerenciais |
| `ADMINISTRADOR` | Gerenciar cadastros e configurações administrativas |

Os supervisores são associados às regiões por meio da tabela `usuarios_regioes`.

Dessa forma, as operações sobre lojas e inspeções devem respeitar os vínculos regionais do usuário autenticado.

## 🧮 Cálculo do percentual de realização

Cada item do checklist pode receber uma das seguintes respostas:

| Resposta | Significado |
|---|---|
| `OK` | Item em conformidade |
| `PENDENTE` | Item que necessita de correção |
| `NA` | Item não aplicável à inspeção |

O percentual é calculado utilizando apenas os itens `OK` e `PENDENTE`.

**Fórmula:**

\[
\text{Realização (\%)} =
\frac{\text{Quantidade de OK}}
{\text{Quantidade de OK}+\text{Quantidade de PENDENTE}}
\times 100
\]

**Exemplo:**

Uma inspeção possui:

- 8 itens `OK`.
- 2 itens `PENDENTE`.
- 3 itens `NA`.

O percentual será:

\[
\frac{8}{8+2}\times100=80\%
\]

Itens `NA` são desconsiderados no cálculo.

Quando não existem itens `OK` ou `PENDENTE`, o percentual é representado como `NULL`, pois não há elementos aplicáveis suficientes para calcular o indicador.

## 🏗️ Arquitetura

A aplicação foi organizada em camadas para separar responsabilidades.

```text
Razor Pages
     │
     ▼
Services
     │
     ▼
Repositories / Npgsql
     │
     ▼
PostgreSQL
```

### Razor Pages

Responsáveis pela apresentação das páginas, recebimento dos dados dos formulários e tratamento das requisições HTTP.

### Services

Responsáveis pela implementação das regras de negócio, validações e coordenação das operações.

### Repositories

Responsáveis pelo acesso aos dados por meio do Npgsql e de comandos SQL parametrizados.

### Models e DTOs

Representam as entidades do sistema e os objetos utilizados para transferência e validação de dados.

### Autorização

O ASP.NET Core utiliza autenticação por cookies e políticas de autorização para restringir o acesso às funcionalidades.

## 📂 Organização do projeto

A estrutura principal está organizada da seguinte forma:

```text
PassoCerto.Web/
├── Extensions/
├── Models/
│   ├── Entity/
│   └── DTOs/
├── Pages/
│   ├── Admin/
│   │   ├── Regioes/
│   │   ├── Lojas/
│   │   ├── Usuarios/
│   │   ├── Vinculos/
│   │   └── Checklist/
│   ├── Auth/
│   └── ...
├── Repositories/
│   └── Interfaces/
├── Services/
│   └── Interfaces/
├── wwwroot/
├── database/
│   ├── 01_schema.sql
│   └── 02_seed.sql
├── Dockerfile
├── compose.yaml
├── .dockerignore
├── .env.example
├── appsettings.json
├── Program.cs
├── PassoCerto.Web.csproj
└── README.md
```

## 🐳 Executando com Docker (recomendado)

Esta é a maneira recomendada de executar o PassoCerto.

Com Docker Compose, a aplicação e o PostgreSQL são inicializados em containers separados.

### Pré-requisitos

- Docker Engine instalado e funcionando.
- Docker Compose disponível.
- Docker Buildx disponível.
- Git.

Não é necessário instalar .NET ou PostgreSQL diretamente no computador para executar a aplicação pelos containers.

### 1. Clonar o repositório

```bash
git clone <URL_DO_REPOSITORIO>
cd PassoCerto.Web
```

Substitua `<URL_DO_REPOSITORIO>` pelo endereço real do projeto no GitHub.

### 2. Configurar as variáveis de ambiente

Copie o arquivo de exemplo:

```bash
cp .env.example .env
```

Abra o arquivo `.env` e configure:

```dotenv
DB_PASSWORD=sua_senha_forte
```

O arquivo `.env` contém configurações locais e não deve ser enviado ao repositório.

### 3. Iniciar os containers

Execute:

```bash
docker compose up --build -d
```

O Docker Compose realizará as seguintes operações:

1. Construirá a imagem da aplicação ASP.NET Core.
2. Criará o container PostgreSQL.
3. Configurará a rede interna dos serviços.
4. Inicializará o banco de dados.
5. Executará os scripts SQL de criação e carga inicial.
6. Inicializará a aplicação web.

### 4. Verificar os containers

```bash
docker compose ps
```

Para consultar os logs da aplicação:

```bash
docker compose logs -f web
```

Para consultar os logs do PostgreSQL:

```bash
docker compose logs -f db
```

### 5. Acessar o sistema

Com os containers em execução, abra:

**http://localhost:8080**

O endereço é destinado ao acesso local no computador onde o Docker está rodando.

### 6. Parar os containers

```bash
docker compose down
```

Esse comando remove os containers e a rede, mas mantém o volume de dados do PostgreSQL.

Para iniciar novamente:

```bash
docker compose up -d
```

### Persistência dos dados

O PostgreSQL utiliza um volume Docker para armazenar os dados.

Isso permite que os registros sejam preservados mesmo após a remoção e recriação dos containers.

**Atenção:** o comando abaixo remove também o volume de dados:

```bash
docker compose down -v
```

Utilize-o apenas quando desejar apagar os dados desse ambiente e recriar o banco a partir dos scripts.

## 🗄️ Banco de dados

O banco de dados utilizado é o PostgreSQL.

O projeto contém scripts SQL para criação da estrutura e inserção dos dados de demonstração.

| Arquivo | Finalidade |
|---|---|
| `database/01_schema.sql` | Criação das tabelas, relacionamentos e restrições |
| `database/02_seed.sql` | Inserção dos dados iniciais de demonstração |

### Principais tabelas

- `usuarios`
- `regioes`
- `lojas`
- `usuarios_regioes`
- `categorias_checklist`
- `itens_checklist`
- `inspecoes`
- `respostas_inspecao`

### Inicialização do banco no Docker

Na primeira execução, quando o volume do PostgreSQL está vazio, os scripts da pasta `database/` são executados automaticamente em ordem.

Os scripts não são executados novamente quando o volume já contém um banco inicializado.

O projeto utiliza scripts SQL para a criação e preparação do banco, sem depender de migrations do Entity Framework Core.

## 🔑 Usuários de demonstração

O script de seed disponibiliza contas para testar os diferentes níveis de acesso.

| Login | Perfil | Situação |
|---|---|---|
| `paulo.mendes` | SUPERVISOR | Ativo |
| `renata.alves` | SUPERVISOR | Ativo |
| `diego.costa` | SUPERVISOR | Ativo |
| `helena.prado` | DIRETORIA | Ativo |
| `carla.nunes` | ADMINISTRADOR | Ativo |
| `sergio.lima` | SUPERVISOR | Inativo |

**Senha das contas de demonstração:**

```text
Prova@123
```

Essas credenciais são exclusivamente para o ambiente de demonstração e não devem ser reutilizadas em produção.

### Vínculos regionais de demonstração

| Supervisor | Região |
|---|---|
| Paulo Mendes | Litoral |
| Renata Alves | Metropolitana |
| Diego Costa | Interior |
| Sergio Lima | Interior (conta inativa) |

Os perfis podem ser utilizados para verificar se o sistema aplica corretamente as restrições de acesso.

## 🧪 Dados de teste

O banco possui dados fictícios para demonstração.

Entre eles:

- Três regiões.
- Onze lojas.
- Usuários com diferentes perfis.
- Cinco categorias de checklist.
- Quinze itens de verificação.
- Inspeções demonstrativas com diferentes situações e resultados, quando incluídas no seed.

As inspeções de demonstração permitem testar históricos, percentuais de realização, pendências e evolução dos indicadores ao longo do tempo.

## 💻 Executando sem Docker

Para executar a aplicação diretamente no ambiente de desenvolvimento, são necessários:

- SDK .NET 8.
- PostgreSQL.
- Banco `passocerto` configurado.
- Connection string válida.

### 1. Restaurar dependências

```bash
dotnet restore
```

### 2. Configurar a conexão

Configure `ConnectionStrings:DefaultConnection` usando o `appsettings.Development.json` local ou uma variável de ambiente.

Exemplo ilustrativo:

```text
Host=localhost;Port=5432;Database=passocerto;Username=passocerto_user;Password=SENHA_LOCAL
```

Não armazene senhas reais em arquivos versionados.

### 3. Executar a aplicação

```bash
dotnet run
```

O endereço e a porta serão informados no terminal, conforme o perfil de inicialização configurado.

## ✅ Regras de negócio

Entre as regras implementadas ou previstas para validação do sistema, destacam-se:

1. Somente usuários ativos podem acessar o sistema.
2. Supervisores devem acessar apenas lojas das regiões autorizadas.
3. A data de visita de uma inspeção não pode ser futura.
4. Inspeções podem permanecer em rascunho.
5. Para concluir uma inspeção, todos os itens exigidos devem possuir resposta.
6. Uma resposta `PENDENTE` exige observação.
7. Respostas `NA` não participam do cálculo percentual.
8. Inspeções concluídas não podem ser alteradas.
9. As informações históricas do checklist devem ser preservadas por snapshots.
10. Cadastros administrativos devem respeitar as restrições de integridade do banco.
11. Registros associados a históricos devem ser desativados logicamente, quando aplicável.
12. O acesso às funcionalidades administrativas é restrito ao perfil `ADMINISTRADOR`.

## 🔐 Segurança

O projeto utiliza mecanismos de segurança como:

- Hash de senhas utilizando BCrypt.
- Autenticação baseada em cookies.
- Autorização por perfil e políticas.
- Validação de dados de entrada.
- Comandos SQL parametrizados.
- Restrição regional de acesso.
- Credenciais do banco configuráveis por variáveis de ambiente.

O ambiente Docker apresentado é destinado a desenvolvimento e avaliação local.

Para uso em produção, seriam necessárias configurações adicionais, como HTTPS, gerenciamento seguro de segredos, configuração de cookies e políticas de infraestrutura.



## 📄 Contexto acadêmico e técnico

O PassoCerto foi desenvolvido como projeto de avaliação técnica, com foco na aplicação prática de conceitos de desenvolvimento web, programação orientada a objetos, persistência de dados, segurança, regras de negócio e containerização.

O código foi estruturado com o objetivo de facilitar sua compreensão, manutenção e evolução.

---

**Desenvolvido com C#, ASP.NET Core Razor Pages, PostgreSQL e Docker.**
