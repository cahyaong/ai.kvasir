grammar MagicCardAbility;

import MagicCardAbility_Keyword, MagicCardAbility_Symbol;

ability
	: producingManaAbility EOF
	;

producingManaAbility
	: '(' TAPPING_SYMBOL ':' ADD MANA_SYMBOL '.)'
	;