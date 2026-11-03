# 👟 PassoCerto — Sistema de Inspeções de Lojas

Aplicação web para registrar, acompanhar e administrar inspeções operacionais em lojas de varejo. Desenvolvida como **prova técnica de Desenvolvedor Júnior**, utilizando **C# / ASP.NET Core 8 (Razor Pages)**, **PostgreSQL 18**, **Npgsql** e **Docker Compose**.

O projeto possui três perfis de acesso: **Supervisor**, **Diretoria** e **Administrador**. O supervisor realiza inspeções nas lojas das regiões às quais está vinculado; a diretoria consulta indicadores; e o administrador gerencia os cadastros do sistema.

> **Execução rápida:** com Docker Engine, Docker Compose e Docker Buildx instalados, basta clonar, configurar o `.env` e executar `docker compose up --build -d`. As instruções completas estão abaixo.

## 🚀 Comece por aqui — Docker (recomendado)

### Pré-requisitos

- [Git](https://git-scm.com/)
- [Docker Engine](https://docs.docker.com/engine/install/) em execução
- [Docker Compose](https://docs.docker.com/compose/install/) (comando `docker compose`)
- Docker Buildx
- Acesso à internet para baixar imagens e dependências na primeira execução
- Porta **8080** disponível no computador

**Não é necessário instalar o SDK .NET nem PostgreSQL localmente** para esta forma de execução.

### 1. Clone o repositório

```bash
git clone https://github.com/LanaMoraes06/Passo-Certo.git
cd Passo-Certo
```

> O diretório criado pelo clone se chama `Passo-Certo`. O projeto C# dentro dele se chama `PassoCerto.Web`.

### 2. Configure o banco

```bash
cp .env.example .env
```

Edite o arquivo `.env` e **substitua** o valor de exemplo por uma senha de sua escolha:

```dotenv
DB_PASSWORD=escolha_uma_senha_forte_e_exclusiva
```

Essa senha é usada **apenas pelo PostgreSQL no Docker** e não é a senha de login dos usuários da aplicação. Não publique o `.env` nem cole seu conteúdo em issues ou logs.

### 3. Inicie a aplicação

```bash
docker compose config --quiet
docker compose up --build -d
```

O Compose inicia dois serviços:

| Serviço | Responsabilidade | Acesso |
|---|---|---|
| `db` | PostgreSQL 18 com scripts de inicialização | Apenas na rede interna do Docker |
| `web` | ASP.NET Core 8 / Razor Pages | `http://localhost:8080` |

O banco é iniciado primeiro, e a aplicação aguarda o healthcheck do PostgreSQL. A conexão com o banco dentro da rede Docker usa `Host=db`, configurado pelo `compose.yaml`.

### 4. Verifique a inicialização

```bash
docker compose ps
```

O esperado é `db` em estado saudável (*healthy*) e `web` em execução. Para consultar os logs:

```bash
docker compose logs --tail=100 db
docker compose logs --tail=100 web
```

**Acesse:** **http://localhost:8080** (página de login).

> O ambiente do `compose.yaml` é **local/de demonstração**: publica a aplicação apenas em `127.0.0.1:8080`, usa HTTP e define `ASPNETCORE_ENVIRONMENT=Development`. Não utilize essa configuração como implantação pública de produção.

### 5. Faça login com uma conta de demonstração

Os usuários são carregados por `database/02_seed.sql` durante a primeira inicialização do PostgreSQL.

| Perfil | Login | Senha de demonstração | Situação |
|---|---|---|---|
| Administrador | `carla.nunes` | `Prova@123` | Ativa |
| Diretoria | `helena.prado` | `Prova@123` | Ativa |
| Supervisor — Litoral | `paulo.mendes` | `Prova@123` | Ativa |
| Supervisor — Metropolitana | `renata.alves` | `Prova@123` | Ativa |
| Supervisor — Interior | `diego.costa` | `Prova@123` | Ativa |
| Supervisor — Interior | `sergio.lima` | `Prova@123` | **Inativa** (teste de bloqueio) |

**Atenção:** a senha `Prova@123` é pública e se destina exclusivamente à demonstração. Não a reutilize em sistemas reais.

### 6. Confirme os dados de demonstração

O seed inclui **3 regiões, 11 lojas, 6 usuários, 5 categorias, 15 itens de checklist e 15 inspeções demonstrativas** (12 concluídas e 3 rascunhos), distribuídas entre junho e outubro de 2026.

```bash
docker compose exec db psql -U passocerto_user -d passocerto \
  -c "SELECT status, COUNT(*) FROM inspecoes GROUP BY status ORDER BY status;"
```

Resultado esperado para um banco inicializado sem alterações posteriores:

```text
  status   | count
-----------+-------
 CONCLUIDA |    12
 RASCUNHO  |     3
```

As inspeções são **dados fictícios**, úteis para validar o histórico, a visualização de pendências e os indicadores do dashboard.

### 7. Pare ou reinicie os serviços

```bash
# Parar e remover containers sem excluir o banco
docker compose down

# Iniciar novamente
docker compose up -d
```

O banco permanece salvo no volume Docker `postgres_data` gerenciado pelo Compose.

> ⚠️ **Apaga os dados do banco Docker:** `docker compose down -v`. Use somente se quiser eliminar o volume desse projeto e começar uma base de demonstração do zero.

---

## 🧭 Roteiro sugerido para avaliação

Depois de iniciar o sistema:

1. **Administrador:** entre como `carla.nunes` e acesse os cadastros de regiões, lojas, usuários, vínculos regionais, categorias e itens do checklist.
2. **Supervisor:** entre como `paulo.mendes` e consulte as inspeções/lojas de sua região. Crie uma inspeção, preencha respostas e salve um rascunho.
3. **Conclusão:** responda aos itens obrigatórios, informe observação nos itens `PENDENTE` e conclua a inspeção para visualizar seu percentual.
4. **Diretoria:** entre como `helena.prado` e consulte os indicadores e a evolução histórica.
5. **Controle de acesso:** tente entrar com `sergio.lima` (usuário inativo) e verifique o bloqueio; observe também as restrições de acesso dos perfis.

Os dados demonstrativos do seed permitem explorar a aplicação mesmo antes de criar inspeções manualmente.

## ✨ Funcionalidades

### Autenticação e autorização

- Autenticação com cookies do ASP.NET Core.
- Senhas armazenadas como hash BCrypt.
- Perfis `SUPERVISOR`, `DIRETORIA` e `ADMINISTRADOR`.
- Acesso a páginas e operações de acordo com o perfil e os vínculos regionais.
- Bloqueio de contas inativas.

### Inspeções e checklist

- Seleção de loja e registro da data de visita.
- Checklist organizado por categorias e itens.
- Respostas `OK`, `PENDENTE` e `NA` (*não aplicável*).
- Observação obrigatória para respostas `PENDENTE`.
- Salvamento de inspeções como rascunho e conclusão posterior.
- Registro de snapshots do checklist na inspeção, preservando os dados históricos.
- Histórico de inspeções e filtros para consulta.
- Indicadores e acompanhamento da evolução mensal.

### Administração

- Cadastro, edição e desativação lógica de regiões e lojas.
- Gerenciamento de usuários e seus vínculos com regiões.
- Gerenciamento de categorias e itens do checklist.
- Restrições de integridade para preservar inspeções existentes.

### Cálculo do percentual de realização

O indicador considera apenas respostas `OK` e `PENDENTE`:

```text
Percentual = 100 × Quantidade de OK / (Quantidade de OK + Quantidade de PENDENTE)
```

Por exemplo, com 8 `OK`, 2 `PENDENTE` e 3 `NA`, o resultado é **80%**. As respostas `NA` não entram no denominador. Se não houver itens `OK` nem `PENDENTE`, o percentual é `NULL` (não calculável).

## 🏗️ Arquitetura e tecnologias

**Fluxo principal:**

```text
Navegador
   ↓
ASP.NET Core Razor Pages
   ↓
Services (regras de negócio)
   ↓
Repositories (SQL parametrizado / Npgsql)
   ↓
PostgreSQL 18
```

| Tecnologia | Uso |
|---|---|
| C# / .NET 8 | Linguagem e plataforma |
| ASP.NET Core Razor Pages | Interface e requisições web — sem controllers MVC |
| PostgreSQL 18 | Persistência relacional |
| Npgsql | Acesso direto ao banco com SQL parametrizado |
| BCrypt.Net-Next | Hash de senhas |
| Cookie Authentication | Sessão e autorização |
| Bootstrap | Interface responsiva |
| Docker / Docker Compose | Ambiente reproduzível com aplicação e banco |

**Organização principal do repositório:**

```text
Passo-Certo/
├── Exceptions/
├── Extensions/
├── Models/
├── Pages/
│   ├── Admin/
│   └── Auth/
├── Repositories/
├── Services/
├── wwwroot/
├── database/
│   ├── 01_schema.sql
│   └── 02_seed.sql
├── Dockerfile
├── compose.yaml
├── .dockerignore
├── .env.example
├── PassoCerto.Web.csproj
├── Program.cs
└── README.md
```

O projeto utiliza **SQL e Npgsql**, sem migrations do Entity Framework Core. Os arquivos SQL versionados definem a estrutura do banco e seus dados iniciais.

## 🗄️ Banco de dados

As principais tabelas são:

- `regioes`, `lojas` e `usuarios`;
- `usuarios_regioes` (vínculo de supervisores às regiões);
- `categorias_checklist` e `itens_checklist`;
- `inspecoes` e `respostas_inspecao`.

O arquivo `database/01_schema.sql` define tabelas, chaves, restrições e índices. O `database/02_seed.sql` insere os dados fictícios, incluindo as inspeções e respostas usadas para demonstrar o dashboard.

**Importante:** os arquivos SQL em `/docker-entrypoint-initdb.d` são executados **somente na primeira inicialização de um volume de banco vazio**. Alterar `02_seed.sql` e executar apenas `docker compose up` não reaplica os dados a um banco existente. Para preservar dados, aplique mudanças por scripts adequados; para reiniciar um ambiente descartável de teste, faça backup se necessário antes de remover volumes.

## 💻 Execução sem Docker (opcional)

Pré-requisitos: **SDK .NET 8** e **PostgreSQL** acessível.

1. Crie um banco `passocerto` e execute `database/01_schema.sql` e `database/02_seed.sql` nessa ordem, usando uma conta que possa criar as tabelas e a extensão `pgcrypto`.
2. Configure `ConnectionStrings:DefaultConnection` em configuração local não versionada (por exemplo, *User Secrets* ou variável de ambiente). Exemplo de valor, substituindo as credenciais:

   ```text
   Host=localhost;Port=5432;Database=passocerto;Username=passocerto_user;Password=SENHA_LOCAL
   ```

3. Na raiz do repositório:

   ```bash
   dotnet restore
   dotnet run
   ```

4. Abra a URL informada pelo `dotnet run`.

A conta local de PostgreSQL precisa ter permissões nas tabelas utilizadas pela aplicação. No Docker, o usuário do banco é configurado durante a própria inicialização.

## 🧯 Solução de problemas

**`docker: unknown command: docker compose`**  
Instale o plugin/pacote Docker Compose compatível com seu sistema. No Arch Linux, por exemplo: `sudo pacman -S docker-compose docker-buildx`.

**`permission denied ... /var/run/docker.sock`**  
Verifique se o Docker Engine está ativo e se sua conta pode utilizá-lo. O grupo `docker` concede privilégios equivalentes aos de administrador; consulte a [documentação oficial de pós-instalação](https://docs.docker.com/engine/install/linux-postinstall/) antes de conceder acesso.

**`DB_PASSWORD variable is not set`**  
Confirme que o `.env` está na raiz do repositório, ao lado de `compose.yaml`, e contém `DB_PASSWORD=` com um valor não vazio. Depois execute `docker compose config --quiet`.

**`failed to create ... veth ... operation not supported` (Arch Linux)**  
Pode ocorrer quando o kernel em execução não corresponde aos módulos instalados após uma atualização. Confira `uname -r`, `ls /usr/lib/modules/` e `sudo modprobe veth`; se o kernel foi atualizado, reiniciar o computador pode resolver. Em máquinas virtuais ou containers restritos, revise o suporte a interfaces virtuais.

**O serviço `db` inicia, mas as tabelas não aparecem**  
Confira `docker compose logs db`. Os scripts SQL só são executados automaticamente em um volume vazio, e falhas na inicialização aparecem nos logs.

**Os dados de demonstração não mudaram após editar o seed**  
Esse comportamento é esperado com volume persistente. Não apague o volume sem ter certeza de que os dados são descartáveis.

**O login não mantém a sessão no endereço HTTP local**  
Confira as configurações de cookie e HTTPS em `Program.cs`, o navegador usado e os logs da aplicação. O projeto configura cookies com `CookieSecurePolicy.Always`; para execução local, o comportamento em `localhost` deve ser validado no navegador. Para exposição pública, configure HTTPS adequadamente.

**A porta 8080 já está sendo utilizada**  
Pare o processo que ocupa a porta ou altere o mapeamento do serviço `web` no `compose.yaml` (por exemplo, `127.0.0.1:8081:8080`) e acesse a porta escolhida.

## 🔐 Segurança e limitações do ambiente de demonstração

- As contas de demonstração usam credenciais conhecidas publicamente: **não utilize este seed em produção**.
- O arquivo `.env` deve permanecer fora do Git; a senha do PostgreSQL é definida localmente.
- O `compose.yaml` foi preparado para **uso local**, com HTTP e ambiente de desenvolvimento.
- Uma implantação pública exige configuração adequada de HTTPS, cookies, proteção de segredos, backups e políticas de acesso.
- O PostgreSQL não publica a porta 5432 no host pelo Compose; o acesso da aplicação ocorre pela rede interna.

## ✅ Como avaliar se a instalação funcionou

```bash
# Serviços iniciados
docker compose ps

# Banco com inspeções de demonstração
docker compose exec db psql -U passocerto_user -d passocerto \
  -c "SELECT COUNT(*) AS total FROM inspecoes;"

# Logs da aplicação
docker compose logs --tail=50 web
```

A instalação estará pronta para avaliação quando a página de login carregar em **http://localhost:8080**, uma conta ativa conseguir entrar e as inspeções de demonstração estiverem disponíveis.

---

**Repositório:** https://github.com/LanaMoraes06/Passo-Certo  
**Finalidade:** projeto demonstrativo de prova técnica para Desenvolvedor Júnior.
