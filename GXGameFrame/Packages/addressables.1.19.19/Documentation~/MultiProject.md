---
uid: addressables-multiple-projects
---

# Loading from Multiple Projects

Should your situation require a multi-project workflow, such as a large project broken up across multiple Unity projects that have the Addressables package installed, we have [`Addressables.LoadContentCatalogAsync`](LoadContentCatalogAsync.md) to link together code and content across the various projects.  Studios with teams that works on many facets of an application simultaneously may find benefit with this workflow.

## Setting up multiple projects
The main items to note for a multi-project setup is to make sure:
1. Each project uses the same version of the Unity Editor
2. Each project uses the same version of the Addressables package

From there projects can contain whatever you see fit for your given situation.  One of your projects must be your "main project" or "source project".  This is the project that you'll actually build and deploy your game binaries from.  Typically, this source project is primarily comprised of code and very little to no content.  The main piece of content that you would want in the primary project would be a bootstrap scene at minimum.  It may also be desirable to include any scenes that need to be local for performance purposes before any AssetBundles have had a chance to be downloaded and cached.

Secondary projects are, in most cases, the exact opposite.  Mostly content and little to no code.  **These projects need to have all remote Addressable Groups and Build Remote Catalog turned on.**  Any local data built into these projects cannot be loaded in your source project's application. Non-critical scenes can live in these projects and be downloaded by the primary project when requested.

#### The Typical Workflow
Once you have your projects setup, the workflow generally is as follows:
1. Build remote content for all secondary projects
2. Build Addressables content for source project
3. Start source project Play Mode or build source project binaries
4. In source project, use [`Addressables.LoadContentCatalogAsync`](LoadContentCatalogAsync.md) to load the remote catalogs of your other various projects
5. Proceed with game runtime as normal.  Now that the catalogs are loaded Addressables is able to load assets from any of these locations.

In regards to the source project, it should be noted that it may be worth having some minimal amount of content built locally with that project.  Each project is unique, and so has unique needs, but having a small set of content needed to run your game in the event of internet connection issues or other various problems may be advisable. 

#### Handling Shaders
Addressables builds a Unity built in shader bundle for each set of Addressables player data that gets built.  This means that when multiple AssetBundles are loaded that were built in secondary projects, there could be multiple built in shader bundles loaded at the same time.

Depending on your specific situation, you may need to utilize the Shader Bundle Naming Prefix on the `AddressableAssetSettings` object.  Each built in shader bundle needs to be named different from others built in your other projects.  If they're not named differently you'll get `The AssetBundle [bundle] can't be loaded because another AssetBundle with the same files is already loaded.` errors.