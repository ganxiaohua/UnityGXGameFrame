
namespace GameFrame
{
    public interface IAssetReference
    {
        void RefInitialize();
        
        bool RefAsset(string asset);

        void LoadLater();

        void UnrefAsset(string asset);
        
        void UnrefAssets();

        bool IsLoadAll { get; }
    }
}