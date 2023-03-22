namespace GameFrame
{
    public interface Parameter
    {
    }
    public interface Parameter<P1> :Parameter
    {
    }
    
    public interface Parameter<P1,P2> :Parameter
    {
    }
    
    public interface Parameter<P1,P2,P3> :Parameter
    {
    }
    
    public interface Parameter<P1,P2,P3,P4> :Parameter
    {
        
    }

    public class ParameterP1<P1>:Parameter<P1>
    {
        public P1 p1;

        public void Set(P1 p1)
        {
            
        }
    }
}