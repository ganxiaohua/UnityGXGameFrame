namespace GameFrame
{
    public interface Parameter : IReference
    {
    }

    public interface Parameter<P1> : Parameter
    {
    }

    public interface Parameter<P1, P2> : Parameter
    {
    }

    public interface Parameter<P1, P2, P3> : Parameter
    {
    }

    public class ParameterP1<P1> : Parameter<P1>
    {
        public P1 Param1;

        public void Set(P1 p1)
        {
            Param1 = p1;
        }

        public void Clear()
        {
            Param1 = default(P1);
        }
    }

    public class ParameterP2<P1, P2> : Parameter<P1, P2>
    {
        public P1 Param1;

        public P2 Param2;

        public void Set(P1 p1, P2 p2)
        {
            Param1 = p1;
            Param2 = p2;
        }

        public void Clear()
        {
            Param1 = default(P1);
            Param2 = default(P2);
        }
    }

    public class ParameterP3<P1, P2, P3> : Parameter<P1, P2, P3>
    {
        public P1 Param1;

        public P2 Param2;

        public P3 Param3;

        public void Set(P1 p1, P2 p2, P3 p3)
        {
            Param1 = p1;
            Param2 = p2;
            Param3 = p3;
        }

        public void Clear()
        {
            Param1 = default(P1);
            Param2 = default(P2);
            Param3 = default(P3);
        }
    }
}