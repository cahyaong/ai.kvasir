lexer grammar MagicCardAbility_Symbol;

import MagicCardAbility_Common;

TAPPING_SYMBOL
	: '{T}'
	;

MANA_SYMBOL
	: '{' ( [WUBRG] | DIGIT+ ) '}'
	;