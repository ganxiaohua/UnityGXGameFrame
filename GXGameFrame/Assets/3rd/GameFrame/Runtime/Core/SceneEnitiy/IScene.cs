namespace GameFrame
{
    public interface IScene : IReference
    {
        public void Start(SceneEntity sceneEntity);
        public void Update(float elapseSeconds, float realElapseSeconds);
    }
}