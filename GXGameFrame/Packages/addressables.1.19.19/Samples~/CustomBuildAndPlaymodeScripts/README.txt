Custom Play Mode and Build Scripts
A custom play mode script and build script have been provided in this sample.  This custom build script creates a build that only includes the currently open scene.  A bootstrap scene is automatically created and a script is added that loads the built scene on startup.  The custom play mode script works similarly to the Use Existing Build (requires built groups) play mode script already included.  The methods added to accomplish this are CreateCurrentSceneOnlyBuildSetup and RevertCurrentSceneSetup on the CustomBuildScript.

For this examples, the build and load paths used by default are [UnityEngine.AddressableAssets.Addressables.BuildPath]/[BuildTarget] and {UnityEngine.AddressableAssets.Addressables.RuntimePath}/[BuildTarget] respectively.

Custom play mode scripts inherit from BuildScriptBase.  There are several overridable methods, such as ClearCachedData, IsDataBuilt, and CanBuildData<T>.  However, the most noteable method to override is BuildDataImplementation<TResult>.  This is the method that is used to setup or build content.

The CanBuildData<T> determines if the customs script shows up in the Build/New Build/ menu or the Play Mode Scripts menu.  If the data type being built is AddressablesPlayModeBuildResult, the script shows up in the Play Mode Scripts menu.  If the type is AddressablesPlayerBuildResult, the script shows up in the Build/New Build/ menu.  

The ScriptableObject of the class has already been created, but the Create menu can be used to make another ScriptableObject if you desire.  For this CustomPlayModeScript the create menu path is Addressables/Content Builders/Use CustomPlayMode Script.  By default, this creates a CustomPlayMode.asset ScriptableObject.  The same goes for the CustomBuildScript.

When creating custom scripts, you need to specify the CreateAssetMenu tag on your class in order to create the ScriptableObject.

Once the ScriptableObject is created, add it to the list of ScriptableObjects called Build and Play Mode Scripts on the AddressableAssetSettings object.