// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CardKind.cs" company="nGratis">
//  The MIT License (MIT)
//
//  Copyright (c) 2014 - 2021 Cahya Ong
//
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.
// </copyright>
// <author>Cahya Ong - cahya.ong@gmail.com</author>
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