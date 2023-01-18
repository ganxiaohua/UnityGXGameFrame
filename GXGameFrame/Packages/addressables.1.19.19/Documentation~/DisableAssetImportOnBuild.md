---
uid: samples-disable-asset-import-on-build
---
# Disable Asset Import on Build
This sample provides a script that disables asset importing during a player build.  This improves build performance since `AssetBundles` are copied into StreamingAssets at build time.  This Sample is only relevant for Editor versions below 2021.2.  In 2021.2+, the Editor provides the ability to include folders outside of `Assets/` into `StreamingAssets`.

When the sample is imported into the project, a player build without asset importing can be triggered by the new menu item **Build/Disabled Importer Build**.  The build output is placed into **DisabledImporterBuildPath/{EditorUserBuildSettings.activeBuildTarget}/** by default.  The sample class `DisableAssetImportOnBuild` can be edited to alter the build path.