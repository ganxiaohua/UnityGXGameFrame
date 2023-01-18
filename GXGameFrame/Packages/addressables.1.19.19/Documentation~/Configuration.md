---
uid: addressables-configuration
---

# Configuring Addressables

The following topics provide an overview of the configuration options for setting up the Addressables system in a project and links to more detailed information:

* [Initialization](#initialization)
* [System settings](#system-settings)
* [Group settings](#group-settings)
* [Profiles](#profiles)
* [Asset hosting service](#asset-hosting-service)
* [Preferences](#unity-preferences)
* [Additional topics](#additional-topics)

## Initialization
The Addressables system uses a set of [ScriptableObject] assets to store your configuration settings. The system stores these settings assets in the `Assets/AddressableAssetsData` folder of your Unity project. It creates this folder and default settings objects when you initialize Addressables from the Groups window. Open the [Groups window] \(menu:  __Window > Asset Management > Addressables > Groups__) after installing the Addressables package.

The first time you open the [Groups window], click __Create Addressables Settings__ to run the initialization command to create the settings folder and assets:

![](images/addr_gettingstarted_firstuse.png)<br/>*Before initializing the Addressables system in a Project*

Add the `AddressableAssetsData` folder and its contents to your source control system.

See [Getting started] for a quick guide to using the Addressable system and [Managing Addressables in the Editor] for information on ways to organize your Addressable assets.

## System settings
The AddressableAssetsSettings object contains the global, system settings for your Project. You can access these settings from the menu: __Window > Asset Management > Addressables > Settings__ or from the __Tools__ menu on the [Groups window].

See [Addressable system settings] for information about each setting. 

## Group settings
The Addressables system uses the [Groups] you define to determine how to package your Addressable assets into local and remote AsssetBundles. Each group has its own settings object that you can use to control that group's options. Addressables creates a new settings object whenever you create a group.

See [Groups] and [Group settings] for more information.

## Profiles

[Profiles] let you configure sets of build variables as appropriate for the purpose of build. For example, you could configure a profile to use for development builds of your project, one for test builds, and another for publishing release builds. You can create as many profiles as you need.

> [!TIP]
> When you host  

See [Profiles] for more information.

## Asset hosting service

The Addressables system provides a asset hosting service that runs within the Unity Editor. You can use this service to test your remote content via an HTTP connection.

See [Asset hosting service] for more information.

<a name="unity-preferences"></a>
## Unity Preferences
The Addressables package adds its own section to the Unity Editor [Preferences] window. The Addressables preferences include:

__Debug Build Layout__ 

When enabled, the build system produces the [Build layout report]. This option is disabled by default since it increases the time need to create a build. The build report contains a detailed description of each AssetBundle produced by the build.

See [Diagnostic tools] for a description of this and other analysis tools.

__Build Addressables on Player Build__ (Unity 2021.2+)

Determines whether Unity builds Addressables content as part of your Player build. 

Building Addressables content together with the Player can be convenient, but does increase build time, especially on large projects, since this rebuilds the Addressables content even when you haven't modified any assets. If you don't change your Addressables content between most builds, consider disabling this option.

The options include:
 
* __Build Addressables content on Player Build__: Always build Addressables content when building the Player.
* __Do not Build Addressables content on Player Build__: Never build Addressables content when building the Player. (If you modify Addressables content, you must rebuild it manually before building the Player.)
* __Use global Settings (stored in preferences)__: Use the value specified in the Unity Editor Preferences (under __Addressables__). This option allows every project contributor to set the option as they choose.

The first two options override the global Preference for the current Project and affect all contributors who build the Project. Otherwise, the global Preference applies to all Unity projects.  

See [Building Addressables content with Player builds](xref:addressables-builds#build-with-player) for more information.


## Additional topics

See the following topics on more involved setup options:

* [Continuous integration]
* [Build scripting]
* [Customizing Addressables runtime initialization]


[Groups window]: xref:addressables-groups#groups-window
[Addressable system settings]: xref:addressables-asset-settings
[Groups]: xref:addressables-groups
[Group settings]: xref:addressables-group-settings
[Profiles]: xref:addressables-profiles
[Asset hosting service]: xref:addressables-asset-hosting-services
[Continuous integration]: xref:addressables-ci
[Build scripting]: xref:addressables-api-build-player-content
[Customizing Addressables runtime initialization]: xref:addressables-api-initialize-async
[Build layout report]: xref:addressables-build-layout-report
[Diagnostic tools]: xref:addressables-diagnostic-tools
[Preferences]: xref:Preferences
[Getting started]: xref:addressables-getting-started
[Managing Addressables in the Editor]: xref:addressables-assets-development-cycle
[ScriptableObject]: xref:UnityEngine.ScriptableObject
