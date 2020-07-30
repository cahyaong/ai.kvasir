grammar MagicCard;

import MagicCardKeyword;

ability_Bootstrapper
    : ability EOF
    ;

cost_Bootstrapper
    : cost EOF
    ;

ability
    : ability_Activated
    ;

ability_Activated
    : OPEN_PAREN cost COLON effect DOT CLOSE_PAREN
    ;

cost
    : cost_Tapping
    | cost_PayingMana
    ;

cost_Tapping
    : SYMBOL_TAPPING
    ;

cost_PayingMana
    : SYMBOL_MANA_COLORLESS? SYMBOL_MANA_COLOR*
    ;

effect
    : effect_ProducingMana
    ;

effect_ProducingMana
    : ADD SYMBOL_MANA_COLOR
    ;