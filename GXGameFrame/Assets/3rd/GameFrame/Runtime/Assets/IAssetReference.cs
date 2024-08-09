
namespace GameFrame
{
    public interface IAssetReference
    {
        void RefInitialize();
        
        bool RefAsset(string asset);

        void LoadLater();

        void UnrefAsset(string asset,bool immediately);
        
        void UnrefAssets(bool immediately);

        bool IsLoadAll { get; }
    }
}