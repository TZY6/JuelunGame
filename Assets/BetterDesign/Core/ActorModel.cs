using System;

namespace BetterDesign.Core
{
    // Authoritative actor state. Unity views never own gameplay position or health.
    public sealed class ActorModel
    {
        public int Health { get; private set; }
        public float X { get; private set; }
        public float Y { get; private set; }
        public float FacingX { get; private set; } = 1;
        public float FacingY { get; private set; }
        public readonly ActorStateMachine Machine = new ActorStateMachine();
        public void Reset(int health, float x, float y)
        {
            Health = Math.Max(0,health); X = x; Y = y; FacingX = 1; FacingY = 0;
            Machine.Reset(); if (Health == 0) Machine.TryEnter(ActorState.Dead);
        }
        public void MoveTo(float x,float y) { X = x; Y = y; }
        public void Face(float x,float y) { FacingX = x; FacingY = y; }
        public void Damage(int amount) { Health = Math.Max(0,Health - Math.Max(0,amount)); if (Health == 0) Machine.TryEnter(ActorState.Dead); }
        public void Heal(int amount,int maximum) { Health = Math.Min(maximum,Health + Math.Max(0,amount)); }
    }
}
