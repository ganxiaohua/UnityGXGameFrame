---
uid: addressables-analyze-tool
---

# Analyze tool

Analyze is a tool that gathers information on your Projects' Addressables layout. In some cases, Analyze may take appropriate actions to clean up the state of your Project. In others, Analyze is purely an informational tool that allows you to make more informed decisions about your Addressables layout.

## Using Analyze
In the Editor, open the **Addressables Analyze** window (**Window** > **Asset Management** > **Addressables** > **Analyze**), or open it via the **Addressables Groups** window by clicking  the **Tools** > **Window** > **Analyze** button.

The Analyze window displays a list of Analyze rules, along with the following operations: 

* Analyze Selected Rules
* Clear Selected Rules
* Fix Selected Rules

### The analyze operation
The analyze operation gathers the information needed by the rule. Run this action on a rule or set of rules to gather data about the build, dependency maps, and more. Each rule must gather any required data and report it back as a list of [AnalyzeResult] objects.

No action should be taken to modify any data or the state of the Project during the analyze step. Based on the data gathered in this step, the [fix] operation may be the appropriate course of action. Some rules, however, only contain an analyze step, as no reasonably appropriate and universal action can be taken based on the information gathered. [Check Scene to Addressable Duplicate Dependencies] and [Check Resources to Addressable Duplicate Dependencies] are examples of such rules.

Rules that are purely informational and contain no fix operation are categorized as **Unfixable Rules**. Those that do have a fix operation are categorized as **Fixable Rules**.

### The clear step
The clear operation removes any data gathered by the analysis and updates the `TreeView` accordingly.

### The fix operation
For **Fixable Rules**, you may choose to run the fix operation. The fix operation uses data gathered during the analyze step to perform any necessary modifications and resolve the issues.

The provided [Check Duplicate Bundle Dependencies] rule is an example of a fixable rule. Problems detected by this rule's analysis can be fixed because there is a reasonably appropriate action that can be taken to resolve them.

## Provided Analyze rules
### Fixable rules
#### Check Duplicate Bundle Dependencies
This rule checks for potentially duplicated assets, by scanning all groups with [BundledAssetGroupSchemas] and projecting the asset group layout. This essentially requires triggering a full build, so this check is time-consuming and performance-intensive.  

**Issues**: Duplicated assets result from assets in different groups sharing dependencies, for example two Prefabs that share a material existing in different Addressable groups. That material (and any of its dependencies) would be pulled into both groups containing the Prefabs. To prevent this, the material must be marked as Addressable, either with one of the Prefabs, or in its own space, thereby putting the material and its dependencies in a separate Addressable group.  

**Resolution**: If this check discovers any issues, run the fix operation on this rule to create a new Addressable group to which to move all dependent assets.

**Exceptions**: If you have an asset containing multiple objects, it is possible for different groups to only pull in portions of the asset, and not actually duplicate. An FBX with many meshes is an example of this. If one mesh is in "GroupA" and another is in "GroupB", this rule will think that the FBX is shared, and extract it into its own group if you run the fix operation. In this edge case, running the fix operation is actually harmful, as neither group would have the full FBX asset.

Also note that duplicate assets may not always be an issue. If assets will never be requested by the same set of users (for example, region-specific assets), then duplicate dependencies may be desired, or at least be inconsequential. Each Project is unique, so fixing duplicate asset dependencies should be evaluated on a case-by-case basis.

### Unfixable rules
#### Check Resources to Addressable Duplicate Dependencies
This rule detects if any assets or asset dependencies are duplicated between built Addressable data and assets residing in a `Resources` folder. 

**Issues**: These duplicates mean that data will be included in both the application build and the Addressables build.

**Resolution**: This rule is unfixable, because no appropriate action exists. It is purely informational, alerting you to the redundancy. You must decide how to proceed and what action to take, if any. One example of a possible manual fix is to move the offending asset(s) out of the `Resources` folder, and make them Addressable.

#### Check Scene to Addressable Duplicate Dependencies
This rule detects any assets or asset dependencies that are shared between the Scenes in the Editor Scene list and Addressables. 

**Issues**: These duplicates mean that data will be included in both the application build and the Addressables build.

**Resolution**: It is purely informational, alerting you to the redundancy. You must decide how to proceed and what action to take, if any. One example of a possible manual fix is to pull the built-in Scene(s) with duplicated references out of Build Settings and make it an Addressable Scene.

#### Bundle Layout Preview
This rule will show how assets explicitly marked as Addressable will be laid out in the Addressable build.  Given these explicit assets, we also show what assets are implicitly referenced by, and therefore will be pulled into, the build.

Data gathered by this rule does not indicate any particular issues.  It is purely informational. 

## Extending Analyze
Each unique Project may require additional Analyze rules beyond what comes pre-packaged. The Addressable Assets System allows you to create your own custom rule classes. 

See the [Custom analyze rule project] in the [Addressables-Sample] repository for an example.


### AnalyzeRule objects
Create a new child class of the [AnalyzeRule] class, overriding the following properties: 

* [CanFix] tells Analyze if the rule is deemed fixable or not.
* [ruleName] is the display name you'll see for this rule in the **Analyze window**.

You'll also need to override the following methods, which are detailed below: 

* [List\<AnalyzeResult\> RefreshAnalysis(AddressableAssetSettings settings)]
* [void FixIssues(AddressableAssetSettings settings)]
* [void ClearAnalysis()]

> [!TIP]
> If your rule is designated unfixable, you don't have to override the `FixIssues` method.

#### RefreshAnalysis
This is your analyze operation. In this method, perform any calculations you'd like and cache any data you might need for a potential fix. The return value is a `List<AnalyzeResult>` list. After you'd gathered your data, create a new [AnalyzeResult] for each entry in your analysis, containing the data as a string for the first parameter and a [MessageType] for the second (to optionally designate the message type as a warning or error). Return the list of objects you create.

If you need to make child elements in the `TreeView` for a particular [AnalyzeResult] object, you can delineate the parent item and any children with [kDelimiter]. Include the delimiter between the parent and child items.

#### FixIssues
This is your fix operation. If there is an appropriate action to take in response to the analyze step, execute it here.

#### ClearAnalysis
This is your clear operation. Any data you cached in the analyze step can be cleaned or removed in this function. The `TreeView` will update to reflect the lack of data.

### Adding custom rules to the GUI
A custom rule must register itself with the GUI class using `AnalyzeSystem.RegisterNewRule<RuleType>()`, in order to show up in the **Analyze** window. For example:

[!code-cs[sample](../Tests/Editor/DocExampleCode/MyRule.cs#doc_CustomRule)]

<!--
```
class MyRule : AnalyzeRule {}
[InitializeOnLoad]
class RegisterMyRule
{
    static RegisterMyRule()
    {
        AnalyzeSystem.RegisterNewRule<MyRule>();
    }
}
```
-->

#### AnalyzeRule classes
In order to make it faster to setup custom rules, Addressables includes the following classes, which inherit from [AnalyzeRule]:

* [BundleRuleBase] is a base class for handling [AnalyzeRule] tasks. It includes some basic methods to retrieve information about bundle and resource dependencies.
* __Check bundle duplicates__ base classes help check for bundle dependency duplicates. Override the [FixIssues] method  implementation to perform some custom action.
  * [CheckBundleDupeDependencies] inherits from [BundleRuleBase] and includes further methods for [AnalyzeRule] to check bundle dependencies for duplicates and a method to attempt to resolve these duplicates.
  * [CheckResourcesDupeDependencies] is the same, but resource dependencies specific.
  * [CheckSceneDupeDependencies] is the same, but for scene dependencies specific.

[AnalyzeResult]: xref:UnityEditor.AddressableAssets.Build.AnalyzeRules.AnalyzeRule.AnalyzeResult
[AnalyzeRule]: xref:UnityEditor.AddressableAssets.Build.AnalyzeRules.AnalyzeRule
[BundledAssetGroupSchemas]: xref:UnityEditor.AddressableAssets.Settings.GroupSchemas.BundledAssetGroupSchema
[CanFix]: xref:UnityEditor.AddressableAssets.Build.AnalyzeRules.AnalyzeRule.CanFix
[Check Duplicate Bundle Dependencies]: #check-duplicate-bundle-dependencies
[Check Resources to Addressable Duplicate Dependencies]: #check-resources-to-addressable-duplicate-dependencies
[Check Scene to Addressable Duplicate Dependencies]: #check-scene-to-addressable-duplicate-dependencies
[fix]: #the-fix-operation
[kDelimiter]: xref:UnityEditor.AddressableAssets.Build.AnalyzeRules.AnalyzeRule.kDelimiter
[List\<AnalyzeResult\> RefreshAnalysis(AddressableAssetSettings settings)]: xref:UnityEditor.AddressableAssets.Build.AnalyzeRules.AnalyzeRule.RefreshAnalysis*
[MessageType]: xref:UnityEditor.AddressableAssets.Build.AnalyzeRules.AnalyzeRule.AnalyzeResult.severity
[ruleName]: xref:UnityEditor.AddressableAssets.Build.AnalyzeRules.AnalyzeRule.ruleName
[void ClearAnalysis()]: xref:UnityEditor.AddressableAssets.Build.AnalyzeRules.AnalyzeRule.ClearAnalysis
[void FixIssues(AddressableAssetSettings settings)]: xref:UnityEditor.AddressableAssets.Build.AnalyzeRules.AnalyzeRule.FixIssues*
[Addressables-Sample]: https://github.com/Unity-Technologies/Addressables-Sample
[Custom analyze rule project]: https://github.com/Unity-Technologies/Addressables-Sample/tree/master/Advanced/CustomAnalyzeRule
[BundleRuleBase]: xref:UnityEditor.AddressableAssets.Build.AnalyzeRules.BundleRuleBase
[FixIssues]: xref:UnityEditor.AddressableAssets.Build.AnalyzeRules.CheckBundleDupeDependencies.FixIssues*
[CheckBundleDupeDependencies]: xref:UnityEditor.AddressableAssets.Build.AnalyzeRules.CheckBundleDupeDependencies
[CheckResourcesDupeDependencies]: xref:UnityEditor.AddressableAssets.Build.AnalyzeRules.CheckResourcesDupeDependencies
[CheckSceneDupeDependencies]: xref:UnityEditor.AddressableAssets.Build.AnalyzeRules.CheckSceneDupeDependencies
