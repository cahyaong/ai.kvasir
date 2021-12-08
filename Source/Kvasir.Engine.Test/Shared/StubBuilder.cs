﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StubBuilder.cs" company="nGratis">
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
// <creation_timestamp>Friday, November 26, 2021 10:47:20 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine.Test
{
    using nGratis.AI.Kvasir.Contract;

    public static class StubBuilder
    {
        public static Tabletop CreateDefaultTabletop()
        {
            var activePlayer = new Player
            {
                Name = "[_MOCK_ACTIVE_PLAYER_]",
                Kind = PlayerKind.Testing,
                Life = 20,
                Graveyard = new Zone(ZoneKind.Graveyard, Visibility.Public)
            };

            var nonactivePlayer = new Player
            {
                Name = "[_MOCK_NONACTIVE_PLAYER_]",
                Kind = PlayerKind.Testing,
                Life = 20,
                Graveyard = new Zone(ZoneKind.Graveyard, Visibility.Public)
            };

            activePlayer.Opponent = nonactivePlayer;
            nonactivePlayer.Opponent = activePlayer;

            return new()
            {
                ActivePlayer = activePlayer,
                NonactivePlayer = nonactivePlayer,
                Battlefield = new Zone(ZoneKind.Battlefield, Visibility.Public)
            };
        }
    }
}