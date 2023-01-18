---
uid: addressables-packing-groups
---

# Packing groups into AssetBundles

You have a few options when choosing how the assets in a group are packed into AssetBundles:

* You can pack all Addressables in a group together in a single bundle. 
* You can pack each Addressable in a group separately in its own bundle.
* You can pack all Addressables sharing the same set of labels into their own bundles.

Scene assets are always packed separately from other Addressable assets in the group. Thus a group containing a mix of Scene and non-Scene assets always produces at least two bundles when built, one for scenes and one for everything else.

Assets in folders that are marked as Addressable and compound assets like Sprite Sheets are treated specially when you choose to pack each Addressable separately:

* All the assets in a folder that is marked as Addressable are packed together in the same folder (except for assets in the folder that are individually marked as Addressable themselves). 
* Sprites in an Addressable Sprite Atlas are included in the same bundle.

See [Content Packing & Loading settings] for more information.

> [!NOTE]
> Keeping many assets in the same group can increase the chance of version control conflicts when many people work on the same project.

The choice whether to pack your content into a few large bundles or into many smaller bundles, can have consequences at either extreme:

Dangers of too many bundles:

* Each bundle has [memory overhead]. This is tied to a number of factors, outlined on that page, but the short version is that this overhead can be significant. If you anticipate 100's or even 1000's of bundles loaded in memory at once, this could mean a noticeable amount of memory eaten up.
* There are concurrency limits for downloading bundles. If you have 1000's of bundles you need all at once, they cannot not all be downloaded at the same time. Some number will be downloaded, and as they finish, more will trigger. In practice this is a fairly minor concern, so minor that you'll often be gated by the total size of your download, rather than how many bundles it's broken into.
* Bundle information can bloat the catalog. To be able to download or load catalogs, we store string-based information about your bundles. 1000's of bundles worth of data can greatly increase the size of the catalog.
* Greater likelihood of duplicated assets. Say two materials are marked as Addressable and each depend on the same texture. If they are in the same bundle, then the texture is pulled in once, and referenced by both. If they are in separate bundles, and the texture is not itself Addressable, then it will be duplicated. You then either need to mark the texture as Addressable, accept the duplication, or put the materials in the same bundle.

Dangers of too few bundles:

* The UnityWebRequest (which we use to download) does not resume failed downloads. So if a large bundle is downloading and your user loses connection, the download is started over once they regain connection.
* Items can be loaded individually from bundles, but cannot be unloaded individually. For example, if you have 10 materials in a bundle, load all 10, then tell Addressables to release 9 of them, all 10 will likely be in memory. See [Memory management] for more information. 

## Scale implications as your project grows larger

As your project grows larger, keep an eye on the following aspects of your assets and bundles:

* __Total bundle size__: Historically Unity has not supported files larger than 4GB. This has been fixed in some recent editor versions, but there can still be issues. It is recommended to keep the content of a given bundle under this limit for best compatibility across all platforms.
* __Bundle layout at scale__: The memory and performance trade-offs between the number of AssetBundles produced by your content build and the size of those bundles can change as your project grows larger.
* __Sub assets affecting UI performance__: There is no hard limit here, but if you have many assets, and those assets have many subassets, it may be best to turn off sub-asset display. This option only affects how the data is displayed in the Groups window, and does not affect what you can and cannot load at runtime. The option is available in the groups window under __Tools__ > __Show Sprite and Subobject Addresses__. Disabling this will make the UI more responsive.
* __Group hierarchy display__: Another UI-only option to help with scale is __Group Hierarchy with Dashes__. This is available within the inspector of the top level settings. With this enabled, groups that contain dashes '-' in their names will display as if the dashes represented folder hierarchy. This does not affect the actual group name, or the way things are built. For example, two groups called "x-y-z" and "x-y-w" would display as if inside a folder called "x", there was a folder called "y". Inside that folder were two groups, called "x-y-z" and "x-y-w". This will not really affect UI responsiveness, but simply makes it easier to browse a large collection of groups.


[Content Packing & Loading settings]: xref:addressables-group-settings#content-packing-loading-settings
[Groups to AssetBundles]: xref:addressables-groups#groups-window
[memory overhead]: xref:addressables-memory-management#assetbundle-memory-overhead
[Memory management]: xref:addressables-memory-management