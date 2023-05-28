namespace nGratis.AI.Kvasir.Engine;

using System;
using nGratis.AI.Kvasir.Contract;

public class ActivatingManaAbilityHandler : BaseActionHandler
{
    public override ActionKind ActionKind => ActionKind.ActivatingManaAbility;

    public override bool IsSpecialAction => true;

    protected override void ResolveCore(ITabletop tabletop, IAction action)
    {
        throw new NotImplementedException("WIP: Implement activating mana ability!");
    }
}