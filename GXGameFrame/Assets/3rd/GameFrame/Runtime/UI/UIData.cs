namespace GameFrame
{
    public class UIObjectData : Entity
    {
        public object Data;
        
        public override void Dispose()
        {
            base.Dispose();
            Data = null;
        }
    }
}