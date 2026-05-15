namespace Plejecenter.Shared.Enums;

[Flags]
public enum ShiftMask
{
    None = 0,
    Morgen = 1,
    Eftermiddag = 2,
    Nat = 4,
    All = Morgen | Eftermiddag | Nat
}
