namespace Common
{
    public class StateMachine
    {
        private State currentState;
        
        public void stateTransition(State nextState)
        {
            currentState = nextState;
        }

        public void startMachine()
        {
            while(true)
            {
                currentState.getNextState();
            }
        }
    }
}