// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayingNonLandHandlerTests.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Monday, July 15, 2024 2:11:05 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine.UnitTest;

using FluentAssertions;
using FluentAssertions.Execution;
using nGratis.AI.Kvasir.Contract;
using nGratis.AI.Kvasir.Framework;
using Xunit;

public class PlayingNonLandHandlerTests
{
    public class ResolveMethod
    {
        [Fact]
        public void WhenPlayingCreature_ShouldMarkWithSummoningSickness()
        {
            // Arrange.

            var playingNonLandHandler = new PlayingNonLandHandler();
            var tabletop = StubBuilder.CreateDefaultTabletop();

            tabletop
                .ActivePlayer.Hand
                .AddCreatureCard("ACTIVE", 0, 1);

            var playingCreatureAction = Action.PlayCard(tabletop.ActivePlayer.Hand.FindFromTop());
            playingCreatureAction.Target.Player = tabletop.ActivePlayer;

            // Act.

            playingNonLandHandler.Resolve(tabletop, playingCreatureAction);

            // Assert.

            tabletop
                .Battlefield
                .Must().HaveQuantity(1)
                .And.Contain("[_MOCK_CREATURE__ACTIVE_00_]");

            var permanent = tabletop.Battlefield.FindFromTop();

            using var _ = new AssertionScope();

            permanent
                .OwningPlayer
                .Should().Be(tabletop.ActivePlayer, "because permanent should be owned by active player");

            permanent
                .ControllingPlayer
                .Should().Be(tabletop.ActivePlayer, "because permanent should be controlled by active player");

            permanent
                .IsTapped
                .Should().BeFalse("because permanent should be untapped");

            permanent
                .Card
                .Should().NotBeNull("because permanent should have valid card");

            permanent
                .Card.Kind
                .Should().Be(CardKind.Creature, "because permanent should be creature");

            permanent
                .Card.Name
                .Should().Be("[_MOCK_CREATURE__ACTIVE_00_]", "because permanent should have correct card name");

            permanent
                .ToProxyCreature()
                .HasSummoningSickness
                .Should().BeTrue("because creature should have summoning sickness");
        }
    }
}