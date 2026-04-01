// ============================================================
// Adventure Guild — Seed inspirado em Tormenta 20
// ============================================================

const db = connect("mongodb://localhost:27017/adventure_guild");

// ── Guilds ───────────────────────────────────────────────────
const guildArcanosId   = "a1b2c3d4-0001-0001-0001-000000000001";
const guildFerroId     = "a1b2c3d4-0002-0002-0002-000000000002";
const guildAventureirosId    = "a1b2c3d4-0003-0003-0003-000000000003";

db.guilds.insertMany([
  {
    Id: guildArcanosId,
    Name: "Guilda dos Arcanistas de Vectora",
    CreatedAt: new Date("2024-01-01")
  },
  {
    Id: guildFerroId,
    Name: "Ordem do Ferro Eterno",
    CreatedAt: new Date("2024-01-02")
  },
  {
    Id: guildSombraId,
    Name: "Guilda dos Aventureiros de Valkaria",
    CreatedAt: new Date("2024-01-03")
  }
]);

// ── Adventurers ──────────────────────────────────────────────
const advArantesId  = "b1b2c3d4-0001-0001-0001-000000000001";
const advLirielId   = "b1b2c3d4-0002-0002-0002-000000000002";
const advKordakId   = "b1b2c3d4-0003-0003-0003-000000000003";
const advNyxaraId   = "b1b2c3d4-0004-0004-0004-000000000004";
const advBrumakId   = "b1b2c3d4-0005-0005-0005-000000000005";

db.adventurous.insertMany([
  {
    Id: advArantesId,
    Name: "Arantes, o Paladino de Khalmyr",
    Level: 8,
    Class: "Paladino",
    Money: 320.0,
    CreatedAt: new Date("2024-02-01")
  },
  {
    Id: advLirielId,
    Name: "Liriel Ventoveloz",
    Level: 6,
    Class: "Arcanista",
    Money: 510.0,
    CreatedAt: new Date("2024-02-05")
  },
  {
    Id: advKordakId,
    Name: "Kordak Rompecrânios",
    Level: 7,
    Class: "Guerreiro",
    Money: 180.0,
    CreatedAt: new Date("2024-02-10")
  },
  {
    Id: advNyxaraId,
    Name: "Nyxara das Sombras",
    Level: 5,
    Class: "Ladino",
    Money: 740.0,
    CreatedAt: new Date("2024-02-15")
  },
  {
    Id: advBrumakId,
    Name: "Brumak Peledeaço",
    Level: 9,
    Class: "Bárbaro",
    Money: 95.0,
    CreatedAt: new Date("2024-02-20")
  }
]);

// ── Missions ─────────────────────────────────────────────────
const missionSlimeId    = "c1b2c3d4-0001-0001-0001-000000000001";
const missionDragonId   = "c1b2c3d4-0002-0002-0002-000000000002";
const missionNecroId    = "c1b2c3d4-0003-0003-0003-000000000003";
const missionCultId     = "c1b2c3d4-0004-0004-0004-000000000004";
const missionArtifactId = "c1b2c3d4-0005-0005-0005-000000000005";

db.missions.insertMany([
  {
    Id: missionSlimeId,
    Name: "Praga de Slimes em Valkaria",
    Task: "Eliminar a colônia de slimes que infesta os esgotos de Valkaria antes que atinja o mercado central.",
    Reward: 150.0,
    GuildId: guildFerroId,
    WinnerAdventurous: null,
    Status: "Available",
    CreatedAt: new Date("2024-03-01")
  },
  {
    Id: missionDragonId,
    Name: "O Dragão de Lena",
    Task: "Um jovem dragão negro está aterrorizando aldeias próximas a Lena. Neutralizá-lo ou afugentá-lo permanentemente.",
    Reward: 900.0,
    GuildId: guildFerroId,
    WinnerAdventurous: advBrumakId,
    Status: "Completed",
    CreatedAt: new Date("2024-03-05")
  },
  {
    Id: missionNecroId,
    Name: "Necromante nas Ruínas de Tamu-ra",
    Task: "Investigar e eliminar o necromante que está reanimando os mortos nas ruínas de Tamu-ra.",
    Reward: 600.0,
    GuildId: guildArcanosId,
    WinnerAdventurous: advLirielId,
    Status: "Completed",
    CreatedAt: new Date("2024-03-10")
  },
  {
    Id: missionCultId,
    Name: "Culto de Tenebra em Deheon",
    Task: "Infiltrar e desmantelar a célula do Culto de Tenebra operando nas docas de Deheon.",
    Reward: 450.0,
    GuildId: guildSombraId,
    WinnerAdventurous: null,
    Status: "Available",
    CreatedAt: new Date("2024-03-15")
  },
  {
    Id: missionArtifactId,
    Name: "Artefato Perdido de Wynna",
    Task: "Recuperar o grimório sagrado de Wynna roubado por ladrões que fugiram para as Montanhas Uivantes.",
    Reward: 350.0,
    GuildId: guildArcanosId,
    WinnerAdventurous: null,
    Status: "Available",
    CreatedAt: new Date("2024-03-20")
  }
]);

// ── Accepted Missions ────────────────────────────────────────
db.acceptedmissions.insertMany([
  {
    Id: "d1b2c3d4-0001-0001-0001-000000000001",
    MissionId: missionDragonId,
    AdventurousId: advBrumakId,
    Status: "Completed",
    CreatedAt: new Date("2024-03-06")
  },
  {
    Id: "d1b2c3d4-0002-0002-0002-000000000002",
    MissionId: missionNecroId,
    AdventurousId: advLirielId,
    Status: "Completed",
    CreatedAt: new Date("2024-03-11")
  },
  {
    Id: "d1b2c3d4-0003-0003-0003-000000000003",
    MissionId: missionCultId,
    AdventurousId: advNyxaraId,
    Status: "InProgress",
    CreatedAt: new Date("2024-03-16")
  }
]);

print("Seed aplicado com sucesso!");
