lexer grammar MagicCardKeyword;

import MagicShared;

ADD
    : A D D
    ;

PLAINS 
    : P L A I N S
    ;

ISLAND
    : I S L A N D
    ;

SWAMP
    : S W A M P
    ;

MOUNTAIN
    : M O U N T A I N
    ;

FOREST
    : F O R E S T
    ;

SYMBOL_TAPPING
    : OPEN_BRACE T CLOSE_BRACE
    ;

SYMBOL_MANA_COLORLESS
    : OPEN_BRACE DIGIT+ CLOSE_BRACE
    ;

SYMBOL_MANA_COLOR
    : OPEN_BRACE [WUBRG] CLOSE_BRACE
    ;