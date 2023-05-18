// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CardKind.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Tuesday, 25 December 2018 1:02:16 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

public enum CardKind
{
    Unknown = 0,

    Artifact,
    Creature,
    Enchantment,
    Instant,
    Land,
    Planeswalker,
    Sorcery,

    Stub
}

public enum CardSuperKind
{
    Unknown = 0,

    None,
    Basic,
    Legendary
}

public enum CardSubKind
{
    Unknown = 0,

    Equipment,

    Advisor,
    Angel,
    Ape,
    Archer,
    Assassin,
    Barbarian,
    Bear,
    Beast,
    Berserker,
    Bird,
    Cat,
    Cleric,
    Crocodile,
    Cyclops,
    Djinn,
    Dragon,
    Drake,
    Druid,
    Dryad,
    Elemental,
    Elf,
    Faerie,
    Fish,
    Frog,
    Giant,
    Goat,
    Goblin,
    Griffin,
    Hippo,
    Horror,
    Horse,
    Human,
    Kithkin,
    Knight,
    Illusion,
    Imp,
    Incarnation,
    Insect,
    Jellyfish,
    Leviathan,
    Lizard,
    Mercenary,
    Merfolk,
    Minotaur,
    Monk,
    Mutant,
    Nightstalker,
    Octopus,
    Pegasus,
    Pirate,
    Rat,
    Rhino,
    Rogue,
    Plant,
    Scout,
    Serpent,
    Shaman,
    Shapeshifter,
    Skeleton,
    Snake,
    Soldier,
    Spider,
    Spirit,
    Treefolk,
    Turtle,
    Unicorn,
    Vampire,
    Wall,
    Warrior,
    Wizard,
    Wraith,
    Wurm,
    Zombie,

    Aura,

    Plains,
    Island,
    Swamp,
    Mountain,
    Forest,

    Ajani,
    Chandra,
    Garruk,
    Jace,
    Liliana
}