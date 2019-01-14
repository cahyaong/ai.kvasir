lexer grammar MagicCardAbility_Symbol;

import MagicCardAbility_Common;

TAP_SYMBOL
	: '{T}'
	;

MANA_SYMBOL
	: '{' ( [WUBRG] | DIGIT+ ) '}'
	;