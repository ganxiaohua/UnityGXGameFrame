
namespace GameFrame
{
    public interface IAssetReference
    {
        void RefInitialize();
        
        bool RefAsset(string asset);

        void UnrefAsset(string asset);
        
        void UnrefAssets();
    }
}