namespace BetterDesign.Core
{
    public enum ActorState { Idle, Move, Attack, Dash, Hurt, Dead }
    // One clock per actor. Attacks/dashes cannot be interrupted by movement.
    public sealed class ActorStateMachine
    {
        public ActorState State { get; private set; }
        public float Elapsed { get; private set; }
        public int Transitions { get; private set; }
        public bool Locked { get { return State == ActorState.Attack || State == ActorState.Hurt || State == ActorState.Dash || State == ActorState.Dead; } }
        public void Tick(float dt) { if (dt > 0) Elapsed += dt; }
        public bool TryEnter(ActorState next)
        {
            if (next == State || State == ActorState.Dead) return false;
            if (Locked && next != ActorState.Hurt && next != ActorState.Dead) return false;
            Enter(next); return true;
        }
        public void Complete()
        {
            if (State != ActorState.Dead) Enter(ActorState.Idle);
        }
        public void Reset() { State = ActorState.Idle; Elapsed = 0; Transitions = 0; }
        void Enter(ActorState next) { State = next; Elapsed = 0; Transitions++; }
    }
}
