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
- Eventos publicados: `adventurer.created`, `mission.accepted`, `mission.completed`
- `AuditConsumer` consome todos os eventos (`#`) e os registra em log para auditoria
- Fila de auditoria com Dead Letter Queue (DLQ) para mensagens com falha de processamento
- Falhas na publicação são isoladas e não interrompem as operações de negócio

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

### Guildas
- `GET /guilds` - Listar todas as guildas
- `POST /guilds` - Criar nova guilda

### Missões
- `GET /missions` - Listar todas as missões
- `POST /missions` - Criar nova missão
- `POST /missions/{id}/accept` - Aceitar uma missão
- `POST /missions/{id}/complete` - Completar uma missão

### Aventureiros
- `GET /adventurers` - Listar todos os aventureiros
- `POST /adventurers` - Criar novo aventureiro

### Lojas
- `GET /shops` - Listar todas as lojas
- `POST /shops` - Criar nova loja

### Comércio
- `POST /shops/{shopId}/buy/{itemId}` - Comprar item em loja
- `POST /shops/{shopId}/sell/{itemId}` - Vender item para loja

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

## 📦 Estrutura do Projeto

```
adventure-guild-api/
├── Controllers/          # Controladores da API
├── Model/               # Modelos de dados (Entidades)
├── Services/            # Lógica de negócio
├── Requests/            # DTOs de requisição
├── Enums/               # Enumerações do sistema
├── Messaging/
│   ├── Events/          # Eventos de domínio (DomainEvent, AdventurerCreatedEvent, etc.)
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