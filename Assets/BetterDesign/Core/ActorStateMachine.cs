using System;
namespace BetterDesign.Core
{
    public enum ActorState { Idle, Move, Attack, Dash, Hurt, Dead, Skill, Interact }
    public enum LocomotionState { Idle, Move }
    public sealed class ActorStateMachine
    {
        public ActorState State { get; private set; }
        public LocomotionState Locomotion { get; private set; }
        public float Elapsed { get; private set; }
        public int Transitions { get; private set; }
        public int ActionVersion { get; private set; }
        public event Action<ActorState> Entered;
        public event Action<ActorState> Exited;
        public bool Locked { get { return State != ActorState.Idle && State != ActorState.Move; } }
        bool actionAllowsMovement;
        public bool AllowsMovement { get { return !Locked || actionAllowsMovement; } }
        public void Tick(float dt) { if (dt > 0 && !float.IsInfinity(dt) && !float.IsNaN(dt)) Elapsed += dt; }
        public void SetLocomotion(bool moving)
        {
            Locomotion = moving ? LocomotionState.Move : LocomotionState.Idle;
            if (!Locked) TryEnter(moving ? ActorState.Move : ActorState.Idle);
        }
        public bool CanTransition(ActorState next)
        {
            if (next == State || State == ActorState.Dead) return false;
            if (next == ActorState.Dead) return true;
            if (next == ActorState.Hurt) return State != ActorState.Dash;
            return !Locked;
        }
        public bool TryEnter(ActorState next,bool allowMovement = false)
        {
            if (!CanTransition(next)) return false;
            actionAllowsMovement = allowMovement || next == ActorState.Skill;
            Enter(next); return true;
        }
        public void Complete()
        {
            if (State != ActorState.Dead) Enter(Locomotion == LocomotionState.Move ? ActorState.Move : ActorState.Idle);
        }
        public bool IsCurrent(int version) { return ActionVersion == version && State != ActorState.Dead; }
        public void Reset()
        {
            if (Exited != null) Exited(State);
            actionAllowsMovement = false;
            State = ActorState.Idle; Locomotion = LocomotionState.Idle;
            Elapsed = 0; Transitions = 0; ActionVersion++;
        }
        void Enter(ActorState next)
        {
            if (Exited != null) Exited(State);
            State = next; Elapsed = 0; Transitions++; ActionVersion++;
            if (Entered != null) Entered(next);
        }
    }
}
