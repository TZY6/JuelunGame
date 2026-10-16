using BetterDesign.Core;
using UnityEngine;
namespace BetterDesign
{
    public interface IActorWorld
    {
        GameModel Model { get; }
        GameEvents Events { get; }
        ActorController Hero { get; }
        Vector2 Clamp(Vector2 position);
        void Melee(ActorController source,int damage);
        bool Fire(Vector2 position,Vector2 direction,int damage,bool arrow);
        void ShowHit(Vector2 position,Color color);
        void EnemyDefeated(ActorController actor,int id);
    }
    public readonly struct DamageContext
    {
        public readonly int Amount;
        public DamageContext(int amount) { Amount = amount; }
    }
    public static class DamageCalculator
    {
        public static int Calculate(DamageContext context,int defense = 0)
        {
            return context.Amount <= 0 ? 0 : Mathf.Max(1,context.Amount - Mathf.Max(0,defense));
        }
    }
}
