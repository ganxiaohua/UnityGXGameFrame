---
uid: addressables-assets-development-cycle
---

# Managing Addressables in the Editor

While it's impossible to comprehensively catalog all the different ways you can organize the assets in your project, [Organizing Addressable assets] outlines several considerations to take into account when you plan your organizational strategy.

You should also understand [How Addressables interact with your Project assets] while you consider how to manage your assets.

Addressable [Groups] are the primary unit of organization with which you manage Addressable assets. An important consideration when using Addressables are your options for [Packing groups into AssetBundles].

In addition to your group settings, you can use the following to control how Addressables work in a project:

* [Addressable Asset Settings]\: the Project-level settings
* [Profiles]: defines collections of build path settings that you can switch between depending on the purpose of a build. (Primarily of interest if you plan to distribute content remotely.)
* [Labels]: edit the Addressable asset labels used in your project.  
* [Play Mode Scripts]: choose how the Addressables system loads assets when you enter Play mode in the Editor.

[AssetReferences] provide a UI-friendly way to use Addressable assets. You can include AssetReference fields in your MonoBehaviour and ScriptableObject classes and then assign assets to them in the Editor using drag-and-drop or the object picker dialog.

The Addressables system provides the following additional tools to aid development:

* [Analyze tool]\: provides various analysis rules that you can run to verify that you have organized your assets the way you want, including a report on how Addressables will package your assets into bundles.
* [Event viewer]: provides a profile view that shows when your assets are loaded and released.  Use the Event viewer to verify that you are releasing assets and to monitor peak memory use.
* [Hosting Service]: provides a simple asset server that you can use to host remote assets for local development.
* [Build layout report]: provides a description of the AssetBundles produced by a build.
* [Build profile log]: provides a log profiling the build process itself so that you can see which parts take the longest.

## Organizing Addressable Assets

There’s no single best way to organize your assets; it depends on the specific requirements of each project. Aspects to consider when planning how to manage your assets in a project include:

* Logical organization: keeping assets in logical categories can make it easier to understand your organization and spot items that are out of place.
* Runtime performance: performance bottlenecks can occur if your bundles become very large, or alternatively if you have a very large number of bundles.
* Runtime memory management: keeping assets together that you use together can help lower peak memory requirements.
* Scale: some ways of organizing assets might work well in small games, but not large ones, and vice versa.
* Platform characteristics: the characteristics and requirements of a platform can be a large consideration in how to organize your assets. Some examples:
  * Platforms that provide abundant virtual memory can handle large bundle sizes better than those with limited virtual memory. 
  * Some platforms don't support downloading content, ruling out remote distribution of assets entirely. 
  * Some platforms don't support AssetBundle caching, so putting assets in local bundles, when possible, is more efficient.
* Distribution: whether you distribute your content remotely or not means, at the very least, that you must separate your remote content from your local content.  
* How often assets are updated: keep assets that you expect to update frequently separate from those that you plan to rarely update.
* Version control: the more people who work on the same assets and asset groups, the greater the chance for version control conflicts to occur in a project.

## Common strategies

Typical strategies include:

* Concurrent usage: group assets that you load at the same time together, such as all the assets for a given level. This strategy is often the most effective in the long term and can help reduce peak memory use in a project.
* Logical entity: group assets belonging to the same logical entity together. For example, UI layout assets, textures, sound effects. Or character models and animations.
* Type: group assets of the same type together. For example, music files, textures.

Depending on the needs of your project, one of these strategies might make more sense than the others. For example, in a game with many levels, organizing according to concurrent usage might be the most efficient both from a project management and from a runtime memory performance standpoint. At the same time, you might use different strategies for different types of assets. For example, your UI assets for menu screens might all be grouped together in a level-based game that otherwise groups its level data separately. You might also pack a group that contains the assets for a level into bundles that contain a particular type of asset.

See [Preparing Assets for AssetBundles] for additional information.

[Addressable Asset Settings]: xref:addressables-asset-settings
[Analyze tool]: xref:addressables-analyze-tool
[AssetReferences]: xref:addressables-asset-references
[Event viewer]: xref:addressables-event-viewer
[Groups]: xref:addressables-groups
[Hosting Service]: xref:addressables-asset-hosting-services
[How Addressables interact with your Project assets]: xref:addressables-managing-assets
[Labels]: xref:addressables-labels
[Organizing Addressable assets]: #organizing-addressable-assets
[Play Mode Scripts]: xref:addressables-groups#play-mode-scripts
[Profiles]: xref:addressables-profiles
[Build layout report]: xref:addressables-build-layout-report
[Build profile log]: xref:addressables-build-profile-log
[Packing groups into AssetBundles]: xref:addressables-packing-groups
[Preparing Assets for AssetBundles]: xref:AssetBundles-Preparing
