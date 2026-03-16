# adventure-guild-api

1. Objetivo

Sistema backend para gerenciamento de guildas, aventureiros, missões, lojas e itens dentro de uma cidade. O sistema permite que aventureiros aceitem missões em guildas, recebam recompensas e utilizem o dinheiro para comprar itens em lojas especializadas.

2. Entidades do Sistema
2.1 Guild

Representa uma guilda de aventureiros que oferece missões.

Atributos

Id

Name

Location (opcional)

CreatedAt

Regras

Uma cidade pode possuir várias guildas.

Uma guilda pode possuir muitas missões.

2.2 Mission

Representa uma missão disponível para aventureiros.

Atributos

Id

Title

Description

Reward (quantidade de dinheiro)

Status

Available

InProgress

Completed

GuildId

Regras

Cada missão pertence a uma guilda.

Uma missão pode ser aceita por apenas um aventureiro por vez.

Quando concluída, o aventureiro recebe a recompensa.

2.3 Adventurer

Representa um aventureiro que pode realizar missões e comprar itens.

Atributos

Id

Name

Gold

CurrentMissionId (nullable)

Regras

Um aventureiro pode ter apenas uma missão ativa por vez.

Um aventureiro pode comprar itens em lojas.

Ao completar uma missão, o valor da recompensa é adicionado ao seu Gold.

2.4 Shop

Representa uma loja dentro da cidade.

Atributos

Id

Name

ShopType

Gold

Tipos de Loja

Blacksmith (ferreiro)

MagicArtifacts

Tavern

Regras

Cada loja possui itens disponíveis para compra e venda.

Lojas podem comprar itens de aventureiros (dependendo do tipo).

Algumas lojas possuem restrições de tipo de item.

2.5 Item

Representa um item que pode ser comprado ou vendido.

Atributos

Id

Name

Value

ItemType

Tipos de Item

Weapon

Armor

Food

MagicArtifact

Potion

Regras

Nem todas as lojas podem vender todos os tipos de itens.

Exemplo: poções mágicas não podem ser vendidas em lojas.

3. Regras de Negócio
Missões

Aventureiros só podem aceitar uma missão por vez.

Uma missão só pode estar em um dos três estados:

Available

InProgress

Completed

Ao completar a missão:

Status → Completed

Recompensa → adicionada ao ouro do aventureiro.

Comércio

Aventureiros podem comprar itens em lojas se tiverem ouro suficiente.

Lojas podem comprar itens de aventureiros.

Cada loja pode vender apenas certos tipos de itens, dependendo do ShopType.

Exemplo:

Tipo de Loja	Itens Permitidos
Blacksmith	Weapon, Armor
MagicArtifacts	MagicArtifact
Tavern	Food
Inventário (Opcional / Expansão)

Aventureiros podem possuir um inventário de itens.

AdventurerInventory

AdventurerId

ItemId

Quantity

4. Relacionamentos
Guild
 └── Missions (1:N)

Mission
 └── Adventurer (0..1)

Adventurer
 └── InventoryItems (1:N)

Shop
 └── Items (1:N)
5. Fluxos Principais
Criar missão

Guild cria uma missão.

Missão fica com status Available.

Aceitar missão

Aventureiro seleciona missão.

Sistema verifica se ele já possui missão ativa.

Missão muda para InProgress.

Completar missão

Missão é finalizada.

Recompensa é adicionada ao ouro do aventureiro.

Missão muda para Completed.

Comprar item

Aventureiro seleciona item na loja.

Sistema verifica:

ouro suficiente

item permitido na loja

Item é adicionado ao inventário.

Ouro é transferido.

6. Possíveis Endpoints (API)
Guilds
GET /guilds
POST /guilds
Missions
GET /missions
POST /missions
POST /missions/{id}/accept
POST /missions/{id}/complete
Adventurers
GET /adventurers
POST /adventurers
Shops
GET /shops
POST /shops
Commerce
POST /shops/{shopId}/buy/{itemId}
POST /shops/{shopId}/sell/{itemId}
7. Possíveis Expansões

Sistema de reputação nas guildas

Raridade de itens

Durabilidade de equipamentos

Rank de aventureiros

Missões com grupo de aventureiros