Feature: Judge_CombatPhase

Scenario: Resolving combat damage for a single attacker without any blocker
	Given the attacker has power 3 and toughness 3
	When the combat phase is executed
	Then the active player should have 20 life
	And the nonactive player should have 17 life
	And all creatures should be in battlefield with 0 damage

Scenario: Resolving combat damage for a single attacker with a weaker blocker
	Given the attacker has power 3 and toughness 3
	And the blocker has power 1 and toughness 1
	When the combat phase is executed
	Then all players should have 20 life
	And the attacker should be in battlefield with 1 damage
	And the blocker should be in graveyard with 0 damage

Scenario: Resolving combat damage for a single attacker with a stronger blocker
	Given the attacker has power 1 and toughness 1
	And the blocker has power 3 and toughness 3
	When the combat phase is executed
	Then all players should have 20 life
	And the attacker should be in graveyard with 0 damage
	And the blocker should be in battlefield with 1 damage