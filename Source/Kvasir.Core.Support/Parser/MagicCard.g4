grammar MagicCard;

import MagicCardShared, MagicCardKeyword;

ability
    : ability_Activated
    ;

ability_Activated
    : '(' cost ':' effect '.)'
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
