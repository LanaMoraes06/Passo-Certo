# PassoCerto — Sistema de Gestão de Inspeções

Aplicação web desenvolvida em **C# com ASP.NET Core Razor Pages (.NET 8)** e **PostgreSQL** para registrar, acompanhar e consultar inspeções de lojas de uma rede varejista de calçados.

O projeto foi desenvolvido como prova técnica de **Desenvolvedor(a) .NET Júnior**, com foco em regras de negócio, autorização por perfil e abrangência regional, integridade histórica e consultas SQL escritas manualmente.

> **Escopo:** esta documentação descreve o funcionamento principal e a configuração local. Funcionalidades extras ou em desenvolvimento devem ser verificadas no código da versão clonada. As instruções de banco dependem dos scripts `.sql` efetivamente presentes em `database/`.

## 1. Funcionalidades

- Login com autenticação por **cookie** e validação de credenciais.
- Controle de acesso por perfil: **SUPERVISOR**, **DIRETORIA** e **ADMINISTRADOR** (funcionalidades administrativas de manutenção de cadastros são bônus, caso implementadas).
- Restrições por região para supervisores.
- Consulta de lojas disponíveis conforme o perfil.
- Criação de inspeções em **RASCUNHO**, com checklist de itens ativos no momento da criação.
- Respostas por item: **OK**, **PENDENTE** ou **NA** (não aplicável).
- Observação obrigatória para itens com resposta **PENDENTE**.
- Salvamento parcial e posterior conclusão de inspeções.
- Somente o autor pode modificar a própria inspeção, e apenas enquanto ela estiver em rascunho.
- Inspeções concluídas disponíveis para consulta, sem edição.
- Preservação de dados históricos da visita e dos itens do checklist (*snapshots*).
- Histórico de inspeções, com filtros e paginação.
- Dashboard com totais, percentuais, pendências e agrupamentos por região.
- Interface responsiva com Razor Pages e Bootstrap.

### Cálculo do percentual

O percentual de realização da inspeção concluída é:

```text
Percentual = OK / (OK + PENDENTE) × 100
```

Respostas `NA` não entram no cálculo. Se `OK + PENDENTE = 0`, o percentual é **não calculável** e deve ser apresentado como `—`.

## 2. Tecnologias

| Tecnologia | Utilização |
| --- | --- |
| C# / .NET 8 | Linguagem e plataforma |
| ASP.NET Core Razor Pages | Páginas e formulários web; sem Controllers MVC |
| PostgreSQL 14+ | Banco de dados relacional |
| Npgsql | Execução manual de SQL, parâmetros e leitura de resultados |
| Npgsql.DependencyInjection | Integração do Npgsql com injeção de dependência |
| BCrypt.Net-Next | Verificação de hashes de senha |
| Bootstrap | Componentes visuais e layout responsivo |
| Git | Controle de versão |

O arquivo `PassoCerto.Web.csproj` define `net8.0` e as dependências NuGet necessárias. A restauração delas é feita com `dotnet restore`; não é preciso instalar cada pacote manualmente.

**Importante:** o projeto não utiliza Entity Framework, NHibernate ou migrations de ORM. A modelagem e a carga inicial do banco são feitas por scripts SQL.

## 3. Pré-requisitos

Para executar localmente:

1. **Git**, para clonar o repositório.
2. **SDK do .NET 8**, e não apenas o Runtime.
3. **PostgreSQL 14 ou superior**, em execução e acessível.
4. Ferramenta para executar scripts SQL: `psql` ou DBeaver/pgAdmin.
5. Navegador web atualizado.
6. Acesso à internet na primeira execução para restaurar os pacotes NuGet.

> Instale o **SDK 8.x**. Mesmo que você já tenha versões mais recentes do SDK, confirme que o ambiente consegue executar projetos `net8.0` com `dotnet --list-sdks` e `dotnet --list-runtimes`.

## 4. Instalação do .NET 8 — passo a passo

### 4.1 Windows 10/11

**Opção A — instalador oficial (recomendada para iniciantes)**

1. Abra **https://dotnet.microsoft.com/download/dotnet/8.0**.
2. Localize **SDK 8.0** (não escolha somente ASP.NET Core Runtime).
3. Baixe o instalador correspondente à arquitetura do seu Windows, normalmente **x64**.
4. Execute o `.exe` e conclua a instalação.
5. Feche e reabra o PowerShell ou terminal do VS Code.
6. Confirme:

```powershell
dotnet --info
dotnet --list-sdks
dotnet --list-runtimes
```

A lista de SDKs deverá incluir uma entrada `8.0.xxx`; a de runtimes deverá incluir `Microsoft.AspNetCore.App 8.0.x`.

**Opção B — PowerShell com winget**

```powershell
winget install --id Microsoft.DotNet.SDK.8 --exact
```

Feche e abra o terminal e execute os comandos de verificação acima.

Documentação: https://learn.microsoft.com/dotnet/core/install/windows

### 4.2 Ubuntu

No Ubuntu com o pacote disponível nos repositórios configurados:

```bash
sudo apt update
sudo apt install -y dotnet-sdk-8.0

dotnet --info
dotnet --list-sdks
dotnet --list-runtimes
```

Se o pacote não for encontrado, consulte as instruções específicas da sua versão do Ubuntu: https://learn.microsoft.com/dotnet/core/install/linux-ubuntu-install. Não misture repositórios de diferentes versões do Ubuntu.

### 4.3 Arch Linux

Confirme a disponibilidade do pacote da versão 8 no repositório configurado:

```bash
sudo pacman -Syu
pacman -Ss dotnet-sdk
```

Se `dotnet-sdk-8.0` estiver disponível:

```bash
sudo pacman -S dotnet-sdk-8.0

dotnet --info
dotnet --list-sdks
```

Se a versão 8 não estiver disponível pelo gerenciador de pacotes, siga o método oficial de instalação manual do SDK 8 para Linux: https://learn.microsoft.com/dotnet/core/install/linux. Evite misturar instalações manuais e de pacotes sem revisar `PATH` e `DOTNET_ROOT`.

### 4.4 Verificação independente de sistema operacional

```bash
dotnet --version
dotnet --list-sdks
dotnet --list-runtimes
```

Um resultado como `8.0.xxx` confirma a presença da linha de SDK 8. Se o comando `dotnet` não for reconhecido, reinicie o terminal e verifique a instalação e o `PATH`.

## 5. Instalação e configuração do PostgreSQL

### 5.1 Windows

1. Acesse **https://www.postgresql.org/download/windows/**.
2. Instale uma versão compatível do PostgreSQL (**14 ou superior**).
3. Durante a instalação, defina uma senha para o superusuário `postgres` e anote a porta escolhida (geralmente `5432`).
4. Deixe o serviço PostgreSQL ativo.
5. Utilize o pgAdmin instalado com o PostgreSQL ou o `psql` para executar os comandos da seção seguinte.

Se usar `psql` no PowerShell e o comando não for encontrado, execute-o no **SQL Shell (psql)** instalado com o PostgreSQL ou adicione a pasta `bin` do PostgreSQL ao `PATH`.

### 5.2 Ubuntu

```bash
sudo apt update
sudo apt install -y postgresql postgresql-client
sudo systemctl enable --now postgresql
sudo systemctl status postgresql
```

Verifique a versão do servidor:

```bash
sudo -u postgres psql -c "SHOW server_version;"
```

> A versão instalada depende dos repositórios do sistema. Confirme que é PostgreSQL 14 ou superior.

### 5.3 Arch Linux

```bash
sudo pacman -S postgresql
```

Na primeira instalação, o serviço normalmente exige inicializar o cluster local (use o diretório de dados configurado pelo pacote):

```bash
sudo -iu postgres
initdb -D /var/lib/postgres/data --locale=C.UTF-8 --encoding=UTF8
exit
sudo systemctl enable --now postgresql
```

**Não execute `initdb` em um cluster que já contém dados.** Se houver um banco existente ou o diretório já estiver inicializado, apenas configure/inicie o serviço de forma apropriada.

### 5.4 Criar usuário e banco da aplicação

Conecte-se ao PostgreSQL como administrador:

```bash
# Ubuntu/Arch (autenticação local típica)
sudo -u postgres psql
```

No Windows, use o SQL Shell/pgAdmin como `postgres`.

Execute estes SQLs **substituindo a senha de exemplo por uma senha local forte e própria**:

```sql
CREATE ROLE passocerto_user WITH LOGIN PASSWORD 'ALTERE_ESTA_SENHA_LOCAL';
CREATE DATABASE passocerto OWNER passocerto_user;
```

Saia do `psql`:

```text
\q
```

**Observações:** se a role ou o banco já existirem, não recrie nem apague dados. Ajuste a configuração para utilizar o banco existente. `CREATE DATABASE` precisa ser executado fora de um bloco de transação explícito.

Teste a conexão:

```bash
psql -h localhost -p 5432 -U passocerto_user -d passocerto -c "SELECT current_database(), current_user;"
```

No Windows, se `psql` não estiver no `PATH`, use o SQL Shell ou o DBeaver.

## 6. Clonar o projeto

Instale o Git, caso ainda não tenha: https://git-scm.com/downloads.

No terminal:

```bash
git clone https://github.com/LanaMoraes06/Passo-Certo.git
cd Passo-Certo
```

Confirme que a pasta contém `PassoCerto.Web.csproj`, `Program.cs` e o diretório `database/`.

No Windows (PowerShell), os comandos `git clone` e `cd` são os mesmos.

## 7. Inicializar o banco com os scripts SQL

> **Etapa obrigatória antes de iniciar a aplicação.** O projeto usa scripts versionados em `database/`, não migrations automáticas.

**1. Descubra os arquivos `.sql` incluídos na versão clonada:**

Linux/macOS:

```bash
find database -type f -name '*.sql' | sort
```

Windows PowerShell:

```powershell
Get-ChildItem .\database -Recurse -Filter *.sql | Sort-Object FullName
```

**2. Abra os scripts e identifique a ordem correta**, normalmente:

1. Criação de tabelas, chaves e restrições (**schema**).
2. Dados iniciais obrigatórios, como regiões, lojas, usuários e checklist (**seed**).
3. Eventuais inspeções de exemplo ou scripts adicionais.

**3. Execute os scripts nesta ordem.** Exemplo de comando, substituindo o caminho pelo **nome real** do arquivo de schema encontrado no passo 1:

```bash
psql -v ON_ERROR_STOP=1 -h localhost -p 5432 -U passocerto_user -d passocerto -f database/ARQUIVO_REAL_DO_SCHEMA.sql
```

Repita para o arquivo real de dados iniciais e os scripts adicionais exigidos pelo projeto.

**Atenção:** `ARQUIVO_REAL_DO_SCHEMA.sql` é um marcador, **não é um arquivo que necessariamente existe no repositório**. Não execute todos os `.sql` em ordem alfabética sem verificar dependências e possíveis comandos destrutivos. Os nomes e a sequência exata devem ser conferidos na pasta `database/` da versão clonada.

No DBeaver: crie uma conexão para `localhost:5432`, banco `passocerto`, usuário `passocerto_user`; abra os arquivos SQL em ordem e execute cada script no editor SQL.

**4. Verifique as tabelas:**

```sql
SELECT table_name
FROM information_schema.tables
WHERE table_schema = 'public'
ORDER BY table_name;
```

As tabelas principais esperadas são `usuarios`, `regioes`, `usuarios_regioes`, `lojas`, `categorias_checklist`, `itens_checklist`, `inspecoes` e `respostas_inspecao`.

Confira os dados iniciais:

```sql
SELECT id, login, perfil, ativo FROM usuarios ORDER BY id;
SELECT id, codigo, nome, ativo FROM lojas ORDER BY id;
SELECT COUNT(*) FROM itens_checklist;
```

> A criação e a carga podem falhar se os arquivos SQL presumirem um proprietário, extensões ou credenciais diferentes. Leia a mensagem do PostgreSQL e ajuste o usuário/permissões ou o ambiente em vez de remover restrições do esquema.

## 8. Configuração da conexão do PassoCerto

No `Program.cs`, a aplicação lê a connection string chamada **`DefaultConnection`** a partir das configurações do ASP.NET Core.

**Atenção à versão publicada:** no `appsettings.json` consultado, `ConnectionStrings` aparece dentro de `Logging`, mas `GetConnectionString("DefaultConnection")` procura `ConnectionStrings` na **raiz**. A variável de ambiente e o User Secrets descritos abaixo definem o caminho correto; também é recomendável corrigir a estrutura do JSON no repositório, removendo a credencial versionada. Exemplo de formato correto (sem senha):

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Opção A — variável de ambiente (recomendada)

Configure a variável `ConnectionStrings__DefaultConnection` no terminal **antes de executar** o aplicativo.

Linux/macOS (bash/zsh):

```bash
export ConnectionStrings__DefaultConnection='Host=localhost;Port=5432;Database=passocerto;Username=passocerto_user;Password=SUA_SENHA_LOCAL'
```

PowerShell:

```powershell
$env:ConnectionStrings__DefaultConnection = 'Host=localhost;Port=5432;Database=passocerto;Username=passocerto_user;Password=SUA_SENHA_LOCAL'
```

Substitua `SUA_SENHA_LOCAL` pela senha cadastrada no PostgreSQL. O nome do parâmetro com **dois sublinhados `__`** corresponde à chave `ConnectionStrings:DefaultConnection` do sistema de configuração .NET.

A variável precisa estar definida no **mesmo terminal** em que você executar `dotnet run`.

### Opção B — User Secrets (desenvolvimento)

No diretório do projeto, execute:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=passocerto;Username=passocerto_user;Password=SUA_SENHA_LOCAL"
```

O projeto possui um `UserSecretsId` no `.csproj`; o mecanismo é destinado ao ambiente de desenvolvimento e evita precisar colocar a senha diretamente em um arquivo rastreado pelo Git.

**Importante:** arquivos de configuração versionados podem conter valores usados em outros ambientes. Não publique novas senhas, tokens ou credenciais reais no Git. Se credenciais válidas já foram publicadas, troque-as no banco e remova-as do código versionado.

## 9. Restaurar dependências e compilar

Na raiz do repositório, onde está `PassoCerto.Web.csproj`:

```bash
dotnet restore
dotnet build
```

A restauração baixa os pacotes NuGet declarados no projeto. Se a compilação falhar, leia o **primeiro erro** exibido no terminal.

Para verificar os pacotes declarados:

```bash
dotnet list package
```

## 10. Executar a aplicação

Com o PostgreSQL ativo, tabelas populadas e conexão configurada:

```bash
dotnet run
```

O terminal deve informar algo semelhante a:

```text
Now listening on: http://localhost:5209
Application started.
```

**A porta acima é apenas um exemplo.** Utilize o endereço exibido na sua execução ou confira `Properties/launchSettings.json`.

Abra a URL do terminal no navegador. Se houver perfil HTTPS, prefira a URL `https://localhost:PORTA` e, se necessário, configure o certificado local de desenvolvimento:

```bash
dotnet dev-certs https --trust
```

A disponibilidade do comando de confiança automática depende do sistema operacional. A aplicação configura cookies com política de segurança; portanto, confira o uso de HTTPS e o comportamento dos cookies se o login não persistir.

Para interromper o servidor, pressione **Ctrl + C**.

### Executar utilizando um perfil específico

Se o arquivo `Properties/launchSettings.json` tiver perfis nomeados, você pode selecioná-los com:

```bash
dotnet run --launch-profile NOME_REAL_DO_PERFIL
```

Consulte o arquivo antes de substituir `NOME_REAL_DO_PERFIL`.

## 11. Usuários de demonstração

O enunciado da prova prevê uma carga inicial de usuários de exemplo. **Use apenas em ambiente de avaliação, com dados fictícios**, e confirme que a versão clonada executou o seed correspondente.

| Login | Perfil | Abrangência |
| --- | --- | --- |
| `paulo.mendes` | SUPERVISOR | Litoral |
| `renata.alves` | SUPERVISOR | Metropolitana |
| `diego.costa` | SUPERVISOR | Interior |
| `helena.prado` | DIRETORIA | Todas as regiões |
| `carla.nunes` | ADMINISTRADOR | Todas as regiões |
| `sergio.lima` | SUPERVISOR | Interior; usuário inativo |

O enunciado informa uma **senha de demonstração para a carga inicial**. Consulte as instruções de avaliação/seed ao executar localmente; não reutilize essa senha em produção. Verifique as contas realmente disponíveis com a consulta SQL indicada na seção 7.

### Teste funcional rápido

1. Acesse a página de login.
2. Entre com um supervisor ativo existente no banco.
3. Abra **Lojas** e confira a abrangência regional.
4. Clique em **Nova inspeção**, selecione uma loja permitida e uma data.
5. Preencha alguns itens e **salve o rascunho**.
6. Reabra a inspeção e confira as respostas salvas.
7. Responda os demais itens e **conclua**.
8. Abra o **Histórico** e o **Dashboard**.
9. Faça login com a Diretoria e confira os dados de todas as regiões, sem permissão para criar inspeções.
10. Confirme que um usuário inativo não consegue se autenticar.

## 12. Estrutura do projeto

```text
Passo-Certo/
├── Exceptions/                  # Exceções de negócio e autorização
├── Extensions/                  # Métodos auxiliares, incluindo Claims
├── Models/
│   ├── Entity/                  # Entidades do domínio
│   ├── DTOs/                    # Objetos de entrada, saída e filtros
│   └── Enum/                    # Status, respostas e perfis
├── Pages/                       # Arquivos .cshtml e PageModels
│   ├── Auth/                    # Login e logout
│   ├── Lojas/                   # Consulta de lojas
│   ├── Inspecoes/               # Criação, histórico e detalhes
│   ├── Dashboard/               # Indicadores
│   └── Shared/                  # Layout comum
├── Repositories/
│   └── Interfaces/              # Contratos de persistência
├── Services/                    # Regras de negócio
├── database/                    # Scripts SQL versionados
├── wwwroot/                     # CSS, JS e arquivos estáticos
├── Properties/
│   └── launchSettings.json      # Perfis de inicialização
├── Program.cs                   # DI, autenticação e middlewares
├── PassoCerto.Web.csproj        # Framework e dependências
├── appsettings.json            # Configuração base
└── README.md
```

### Camadas e responsabilidades

```text
Navegador
   ↓
Razor Pages (.cshtml e .cshtml.cs)
   ↓
Services (validações e regras de negócio)
   ↓
Repositories (SQL parametrizado com Npgsql)
   ↓
PostgreSQL
```

- **Entity:** representa objetos do domínio, como loja, usuário, inspeção e resposta.
- **DTO:** representa os dados de uma operação ou consulta, como filtros do histórico ou dados de criação de uma inspeção.
- **Repository:** executa SQL, lê registros e converte linhas em objetos.
- **Service:** aplica regras de negócio e controla o fluxo das operações.
- **PageModel:** lida com requisições HTTP, model binding e respostas para o navegador.

## 13. Modelagem principal do banco

As entidades principais estão relacionadas da seguinte forma:

- `usuarios` ↔ `regioes`: associação muitos-para-muitos via `usuarios_regioes`.
- `regioes` → `lojas`: uma região contém várias lojas.
- `usuarios` → `inspecoes`: um usuário pode criar várias inspeções.
- `lojas` → `inspecoes`: uma loja pode ter várias visitas ao longo do tempo.
- `categorias_checklist` → `itens_checklist`: uma categoria possui vários itens.
- `inspecoes` → `respostas_inspecao`: uma inspeção possui várias respostas.

As respostas preservam informações históricas (*snapshots*) dos itens. Isso evita que a alteração de um item cadastrado modifique retroativamente uma inspeção já registrada.

## 14. Autorização e regras importantes

| Regra | SUPERVISOR | DIRETORIA | ADMINISTRADOR |
| --- | --- | --- | --- |
| Consultar lojas e histórico | Apenas regiões vinculadas | Todas | Todas |
| Consultar dashboard | Apenas regiões vinculadas | Todas | Todas |
| Criar inspeção | Sim, em loja autorizada | Não | Não |
| Editar/concluir rascunho | Somente o próprio | Não | Não |
| Manutenção administrativa | Não | Não | Bônus, se implementado |

O controle de acesso não depende apenas de ocultar botões: deve ser verificado nos Services/Repositories e na autorização das páginas. Uma inspeção concluída é somente leitura, e itens/lojas inativos não devem participar de novas inspeções, mas continuam acessíveis no histórico permitido.

## 15. Problemas comuns e soluções

### `dotnet: command not found` / `dotnet não é reconhecido`

O SDK não foi instalado corretamente ou seu diretório não está no `PATH`. Reabra o terminal e execute `dotnet --info`. Confira a instalação da seção 4.

### Erro `NETSDK1045` ou SDK incompatível

Confirme o Target Framework `net8.0` e a presença de um SDK compatível com `dotnet --list-sdks`. Instale o SDK 8 quando necessário.

### `Unable to resolve service for type ...`

Uma dependência do Service não foi registrada no contêiner. Confira em `Program.cs` os `AddScoped<Interface, Implementacao>()` e os registros dos Services.

### `password authentication failed for user ...`

A senha da connection string não coincide com a do usuário PostgreSQL. Revise a seção 8 e teste o login com `psql` ou DBeaver.

### `connection refused` / não consegue conectar ao PostgreSQL

Confirme que o serviço está rodando, a porta `5432` está correta e o host é acessível. O PostgreSQL pode estar em outra porta ou servidor.

### `relation "usuarios" does not exist`

Os scripts SQL não foram aplicados no banco correto, ou foram executados em um esquema diferente. Revise a seção 7 e confira `SELECT current_database(), current_schema();`.

### Redirecionamento infinito para `/Auth/Login`

Confira as convenções `AllowAnonymousToPage("/Auth/Login")`, a ordem `UseAuthentication()` antes de `UseAuthorization()` e o cookie/sua URL HTTP ou HTTPS.

### Login funciona, mas a sessão não persiste

Confirme domínio, protocolo e política `Cookie.SecurePolicy`. Se o cookie exigir HTTPS, execute a aplicação pelo endereço HTTPS corretamente configurado.

### Erro `23514` / `ck_resposta_valor`

A resposta enviada não atende à restrição do banco. Os valores aceitos devem ser coerentes com os enums (`OK`, `PENDENTE`, `NA`) e respostas não preenchidas devem ser tratadas como `NULL` quando permitido.

### Erro em `Enum.Parse`

Confirme que os valores gravados em texto no banco correspondem aos membros definidos no enum. Quando apropriado, use conversão que ignore diferença de maiúsculas/minúsculas.

### Porta já utilizada

Consulte `Properties/launchSettings.json`, finalize a instância anterior ou escolha uma URL/porta disponível para iniciar a aplicação.

## 16. Comandos úteis

```bash
# Restaurar os pacotes
dotnet restore

# Compilar sem iniciar
dotnet build

# Executar
dotnet run

# Listar SDKs instalados
dotnet --list-sdks

# Listar runtimes instalados
dotnet --list-runtimes

# Listar dependências NuGet
dotnet list package

# Limpar saídas locais de build
dotnet clean

# Conferir alterações antes de enviar ao Git
git status
```

## 17. Referências

- Repositório: https://github.com/LanaMoraes06/Passo-Certo
- .NET 8: https://dotnet.microsoft.com/download/dotnet/8.0
- Instalação .NET no Windows: https://learn.microsoft.com/dotnet/core/install/windows
- Instalação .NET no Ubuntu: https://learn.microsoft.com/dotnet/core/install/linux-ubuntu-install
- ASP.NET Core Razor Pages: https://learn.microsoft.com/aspnet/core/razor-pages/
- Npgsql: https://www.npgsql.org/doc/
- PostgreSQL: https://www.postgresql.org/download/

---
