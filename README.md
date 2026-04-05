# Adventure Guild API

## 📋 Descrição

API RESTful desenvolvida em .NET 9.0 para gerenciamento de um sistema de guildas de aventureiros. Este projeto simula um mundo de RPG onde aventureiros podem aceitar missões, completá-las para receber recompensas em ouro, e utilizar esse ouro para comprar itens em diferentes tipos de lojas dentro de uma cidade medieval.

## 🏗️ Arquitetura e Tecnologia

- **Framework**: .NET 9.0
- **Banco de Dados**: MongoDB
- **Mensageria**: RabbitMQ (via `RabbitMQ.Client`)
- **Linguagem**: C#
- **Padrão Arquitetural**: MVC com Services
- **Serialização**: System.Text.Json com suporte a enums
- **Configuração**: Environment Variables (.env)
- **Autenticação**: JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- **Rate Limiting**: `Microsoft.AspNetCore.RateLimiting` (nativo .NET 9)

## 🎮 Funcionalidades Implementadas

### Guildas
- Cadastro e gerenciamento de guildas
- Cada guilda pode oferecer múltiplas missões
- Localização opcional para guildas

### Missões
- Criação de missões com título, descrição e recompensa
- Sistema de status: `Available`, `InProgress`, `Completed`
- Aventureiros só podem aceitar uma missão por vez
- Sistema automático de recompensa ao completar missões

### Aventureiros
- Cadastro de aventureiros com nome e saldo de ouro
- Controle de missão ativa
- Sistema de inventário para itens adquiridos

### Mensageria (RabbitMQ)
- Publicação de eventos de domínio de forma assíncrona via exchange `adventure-guild` (tipo `topic`)
- Eventos de negócio publicados: `adventurer.created`, `mission.accepted`, `mission.completed`
- Eventos de infraestrutura publicados pelos middlewares: `http.request.audit` (toda requisição) e `http.error.audit` (erros 500)
- `AuditConsumer` consome todos os eventos (`#`) e os registra em log para auditoria
- Fila de auditoria com Dead Letter Queue (DLQ) para mensagens com falha de processamento
- Falhas na publicação são isoladas e não interrompem as operações de negócio

### Middlewares
- **ErrorHandlingMiddleware** — captura exceções não tratadas e retorna `ErrorResponse` padronizado; publica evento `http.error.audit` em erros 500
- **RateLimitingMiddleware** — limita requisições por IP (Fixed Window); retorna 429 com header `Retry-After`
- **RequestLoggingMiddleware** — loga metadados de entrada/saída com Correlation ID; publica evento `http.request.audit` após cada requisição
- **AuthMiddleware** — valida JWT Bearer; retorna `ErrorResponse` 401/403 padronizado

Pipeline de execução: `ErrorHandling → RateLimiting → RequestLogging → Authentication → Authorization → Controllers`

### Autenticação e Autorização
- Autenticação via JWT Bearer com validação de assinatura, emissor, audiência e expiração
- Dois papéis: `Guild_Master` (gerencia guildas e missões) e `Adventurer` (aceita e completa missões)
- Endpoint `POST /auth/token` para geração de tokens de teste (apenas para fins de portfólio)

### Lojas e Comércio
- Três tipos de lojas: `Blacksmith`, `MagicArtifacts`, `Tavern`
- Sistema de validação de tipos de itens por loja
- Transações de compra e venda com verificação de saldo
- Cada loja possui seu próprio saldo de ouro

### Itens
- Cinco categorias: `Weapon`, `Armor`, `Food`, `MagicArtifact`, `Potion`
- Sistema de valores e restrições de venda por tipo de loja
- Validação automática de compatibilidade item-loja

## 🎯 Endpoints da API

### Auth
- `POST /auth/token` - Gerar token JWT para testes (body: `{ "role": "Guild_Master" }` ou `{ "role": "Adventurer" }`)

### Guildas
- `GET /guilds` - Listar todas as guildas _(público)_
- `GET /guilds/{id}` - Buscar guilda por ID _(público)_
- `POST /guilds` - Criar nova guilda _(requer Guild_Master)_
- `PUT /guilds/{id}` - Atualizar guilda _(requer Guild_Master, apenas o criador)_
- `DELETE /guilds/{id}` - Remover guilda _(requer Guild_Master, apenas o criador)_

### Missões
- `GET /missions` - Listar todas as missões _(público)_
- `GET /missions/{id}` - Buscar missão por ID _(público)_
- `POST /missions` - Criar nova missão _(requer Guild_Master)_
- `PUT /missions/{id}` - Atualizar missão _(requer Guild_Master)_
- `PATCH /missions/{id}` - Atualizar parcialmente _(requer Guild_Master)_
- `DELETE /missions/{id}` - Remover missão _(requer Guild_Master)_
- `POST /missions/{id}/accept` - Aceitar uma missão _(requer Adventurer)_
- `POST /missions/{id}/complete` - Completar uma missão _(requer Adventurer)_

### Aventureiros
- `POST /adventurous` - Criar novo aventureiro _(público)_

## 🔄 Fluxos Principais do Sistema

### 1. Ciclo de Missões
1. **Criação**: Guilda cria missão com status `Available`
2. **Aceitação**: Aventureiro aceita missão (verifica se não tem missão ativa)
3. **Execução**: Status muda para `InProgress`
4. **Conclusão**: Missão completada, status `Completed`, recompensa adicionada ao ouro

### 2. Sistema de Comércio
1. **Verificação**: Sistema valida saldo do aventureiro e compatibilidade item-loja
2. **Transação**: Oro transferido entre aventureiro e loja
3. **Inventário**: Item adicionado ao inventário do aventureiro

## 🏛️ Estrutura de Dados

### Relacionamentos
- **Guild** → **Missions** (1:N)
- **Mission** → **Adventurer** (0..1)
- **Adventurer** → **InventoryItems** (1:N)
- **Shop** → **Items** (1:N)

### Tipos de Loja e Itens Permitidos
| Tipo de Loja | Itens Permitidos |
|-------------|------------------|
| Blacksmith | Weapon, Armor |
| MagicArtifacts | MagicArtifact |
| Tavern | Food |

## 🚀 Como Executar

### Pré-requisitos
- .NET 9.0 SDK
- Docker

### Configuração

#### 1. Configurar Variáveis de Ambiente
Crie o arquivo `.env` na raiz do projeto:
```
MONGO_CONNECTION=mongodb://localhost:27017
MONGO_DATABASE=adventure_guild

RABBITMQ_HOST=localhost
RABBITMQ_PORT=5672
RABBITMQ_USER=guild_user
RABBITMQ_PASS=Password123

JWT_SECRET=sua-chave-secreta-com-minimo-32-caracteres
JWT_ISSUER=adventure-guild-api
JWT_AUDIENCE=adventure-guild-clients

RATE_LIMIT_MAX_REQUESTS=100
RATE_LIMIT_WINDOW_SECONDS=60
```

#### 2. Subir os containers
```bash
docker compose up -d
```

#### 3. Criar o usuário do RabbitMQ
O `RABBITMQ_DEFAULT_USER/PASS` pode não ser aplicado automaticamente dependendo da versão da imagem. Após os containers subirem, crie o usuário manualmente:

```bash
docker exec rabbitmq-adventure-guild rabbitmqctl add_user guild_user Password123
docker exec rabbitmq-adventure-guild rabbitmqctl set_user_tags guild_user administrator
docker exec rabbitmq-adventure-guild rabbitmqctl set_permissions -p / guild_user ".*" ".*" ".*"
```

> Isso só precisa ser feito uma vez. O volume persiste o estado do RabbitMQ entre reinicializações.

#### 4. Executar o Projeto
```bash
dotnet run
```

### Documentação da API
- Acesse `http://localhost:5017/scalar/v1` para documentação interativa (Scalar UI)
- Os endpoints estão disponíveis em `http://localhost:5017`

### Testando com autenticação
1. Gere um token: `POST /auth/token` com body `{ "role": "Guild_Master" }`
2. Copie o valor do campo `token` da resposta
3. Nas requisições protegidas, adicione o header: `Authorization: Bearer <token>`

> O token expira em 8 horas. Gere um novo quando necessário.

## 📦 Estrutura do Projeto

```
adventure-guild-api/
├── Controllers/          # Controladores da API
├── Model/               # Modelos de dados (Entidades)
├── Services/            # Lógica de negócio
├── Requests/            # DTOs de requisição
│   ├── Auth/            # GenerateTokenRequest
│   ├── Adventurous/
│   ├── Guilds/
│   └── Missions/
├── Middlewares/
│   ├── ErrorHandlingMiddleware.cs
│   ├── RequestLoggingMiddleware.cs
│   ├── IAppMiddleware.cs
│   └── Extensions/      # Métodos de extensão para registro no pipeline
├── Enums/               # Enumerações do sistema
├── Messaging/
│   ├── Events/          # DomainEvent base + eventos de negócio e infraestrutura
│   ├── Publisher/       # IEventBus e RabbitMqEventBus
│   ├── Consumer/        # AuditConsumer (IHostedService)
│   └── Connection/      # RabbitMqConnection
├── Program.cs           # Configuração da aplicação
└── README.md            # Documentação
```

## 🎮 Possíveis Expansões Futuras

- Sistema de reputação nas guildas
- Raridade de itens (Comum, Raro, Épico, Lendário)
- Sistema de durabilidade de equipamentos
- Rank e nível de aventureiros
- Missões em grupo (múltiplos aventureiros)
- Sistema de habilidades e classes
- Mercado global entre jogadores
- Eventos especiais e missões temporárias

---

## 📄 Licença

Este projeto está licenciado sob os termos da licença MIT. Consulte o arquivo LICENSE para mais detalhes.

## 🤝 Contribuição

Contribuições são bem-vindas! Sinta-se à vontade para abrir issues para reportar bugs ou sugerir novas funcionalidades.