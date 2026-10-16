using UnityEngine;

namespace BetterDesign
{
    public interface IPlayerInput
    {
        Vector2 Move { get; }
        bool Attack { get; }
        bool Dash { get; }
        bool Spell { get; }
    }
}
