grammar MagicCardAbility;

import MagicCardAbility_Keyword, MagicCardAbility_Symbol;

ability
	: mana_ability EOF
	;

mana_ability
	: '(' TAP_SYMBOL ':' ADD MANA_SYMBOL '.)'
	;

WS 
	: ' '+ -> skip
	;