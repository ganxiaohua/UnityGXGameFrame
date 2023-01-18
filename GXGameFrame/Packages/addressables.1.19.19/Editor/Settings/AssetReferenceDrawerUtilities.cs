using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor.AddressableAssets.GUI;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.U2D;

namespace UnityEditor.AddressableAssets.Settings
{
    using Object = UnityEngine.Object;

    /// <summary>
    /// Contains editor data for the AssetReference.
    /// </summary>
    internal static class AssetReferenceDrawerUtilities
    {
        internal const string noAssetString = "None (Addressable Asset)";
        internal const string noAssetTypeStringformat = "None (Addressable {0})";
        
        static internal bool ValidateAsset(AssetReference assetRefObject, List<AssetReferenceUIRestrictionSurrogate> restrictions, Object obj)
        {
            return assetRefObject != null
                   && assetRefObject.ValidateAsset(obj)
                   && restrictions != null
                   && restrictions.All(r => r.ValidateAsset(obj));
        }
        
        static internal bool ValidateAsset(AssetReference assetRefObject, List<AssetReferenceUIRestrictionSurrogate> restrictions, IReferenceEntryData entryData)
        {
            return assetRefObject != null
                   && assetRefObject.ValidateAsset(entryData?.AssetPath)
                   && restrictions != null
                   && restrictions.All(r => r.ValidateAsset(entryData));
        }
        
        static internal bool ValidateAsset(AssetReference assetRefObject, List<AssetReferenceUIRestrictionSurrogate> restrictions, string path)
        {
            if (assetRefObject != null && assetRefObject.ValidateAsset(path))
            {
                foreach (var restriction in restrictions)
                {
                    if (!restriction.ValidateAsset(path))
                        return false;
                }
                return true;
            }

            return false;
        }

        static internal bool SetObject(ref AssetReference assetRefObject, ref bool referencesSame, SerializedProperty property, Object target, FieldInfo fieldInfo, string label, out string guid)
        {
            guid = null;
            try
            {
                if (assetRefObject == null)
                    return false;
                Undo.RecordObject(property.serializedObject.targetObject, "Assign Asset Reference");
                if (target == null)
                {
                    guid = SetSingleAsset(ref assetRefObject, property, null, null);
                    if (property.serializedObject.targetObjects.Length > 1)
                        return SetMainAssets(ref referencesSame, property, null, null, fieldInfo, label);
                    return true;
                }

                Object subObject = null;
                if (target.GetType() == typeof(Sprite))
                {
                    var atlasEntries = new List<AddressableAssetEntry>();
                    AddressableAssetSettingsDefaultObject.Settings.GetAllAssets(atlasEntries, false, null,
                        e => AssetDatabase.GetMainAssetTypeAtPath(e.AssetPath) == typeof(SpriteAtlas));
                    var spriteName = FormatName(target.name);
                    foreach (var a in atlasEntries)
                    {
                        var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(a.AssetPath);
                        if (atlas == null)
                            continue;
                        var s = atlas.GetSprite(spriteName);
                        if (s == null)
                            continue;
                        subObject = target;
                        target = atlas;
                        break;
                    }
                }

                if (subObject == null && AssetDatabase.IsSubAsset(target))
                {
                    subObject = target;
                    target = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GetAssetPath(target));
                }

                guid = SetSingleAsset(ref assetRefObject, property, target, subObject);

                var success = true;
                if (property.serializedObject.targetObjects.Length > 1)
                {
                    success = SetMainAssets(ref referencesSame, property, target, subObject, fieldInfo, label);
                }

                return success;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return false;
        }
        
        static internal string SetSingleAsset(ref AssetReference assetReferenceObject, SerializedProperty property, Object asset, Object subObject)
        {
            string guid = null;
            bool success = false;
            if (asset == null)
            {
                assetReferenceObject.SetEditorAsset(null);
                SetDirty(property.serializedObject.targetObject);
                return guid;
            }

            success = assetReferenceObject.SetEditorAsset(asset);
            if (success)
            {
                if (subObject != null)
                    assetReferenceObject.SetEditorSubObject(subObject);
                else
                    assetReferenceObject.SubObjectName = null;
                guid = assetReferenceObject.AssetGUID;
                SetDirty(property.serializedObject.targetObject);
            }

            return guid;
        }
        
        static internal bool SetMainAssets(ref bool referencesSame, SerializedProperty property, Object asset, Object subObject, FieldInfo propertyField, string labelText)
        {
            var allsuccess = true;
            foreach (var targetObj in property.serializedObject.targetObjects)
            {
                var serializeObjectMulti = new SerializedObject(targetObj);
                SerializedProperty sp = serializeObjectMulti.FindProperty(property.propertyPath);
                var assetRefObject =
                    sp.GetActualObjectForSerializedProperty<AssetReference>(propertyField, ref labelText);
                if (assetRefObject != null)
                {
                    Undo.RecordObject(targetObj, "Assign Asset Reference");
                    var success = assetRefObject.SetEditorAsset(asset);
                    if (success)
                    {
                        if (subObject != null)
                            assetRefObject.SetEditorSubObject(subObject);
                        else
                            assetRefObject.SubObjectName = null;
                        SetDirty(targetObj);
                    }
                    else
                    {
                        allsuccess = false;
                    }
                }
            }

            referencesSame = allsuccess;
            return allsuccess;
        }
        
        static internal bool SetSubAssets(SerializedProperty property, Object subAsset, FieldInfo propertyField, string labelText)
        {
            bool valueChanged = false;
            string spriteName = null;
            if (subAsset != null && subAsset.GetType() == typeof(Sprite))
            {
                spriteName = FormatName(subAsset.name);
            }

            foreach (var t in property.serializedObject.targetObjects)
            {
                var serializeObjectMulti = new SerializedObject(t);
                var sp = serializeObjectMulti.FindProperty(property.propertyPath);
                var assetRefObject =
                    sp.GetActualObjectForSerializedProperty<AssetReference>(propertyField, ref labelText);
                if (assetRefObject != null && (assetRefObject.SubObjectName == null || assetRefObject.SubObjectName != spriteName))
                {
                    Undo.RecordObject(t, "Assign Asset Reference Sub Object");
                    var success = assetRefObject.SetEditorSubObject(subAsset);
                    if (success)
                    {
                        valueChanged = true;
                        SetDirty(t);
                    }
                    else
                    {
                        valueChanged = false;
                    }
                }
            }
            return valueChanged;
        }
        
        static internal bool CheckTargetObjectsSubassetsAreDifferent(SerializedProperty property, string objName, FieldInfo propertyField, string labelText)
        {
            foreach (var targetObject in property.serializedObject.targetObjects)
            {
                var serializeObjectMulti = new SerializedObject(targetObject);
                var sp = serializeObjectMulti.FindProperty(property.propertyPath);
                var assetRefObject = sp.GetActualObjectForSerializedProperty<AssetReference>(propertyField, ref labelText);
                if (assetRefObject != null && assetRefObject.SubObjectName != null)
                {
                    if (assetRefObject.SubObjectName != objName)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        static void SetDirty(Object obj)
        {
            UnityEngine.GUI.changed = true; // To support EditorGUI.BeginChangeCheck() / EditorGUI.EndChangeCheck()
            
            EditorUtility.SetDirty(obj);
            AddressableAssetUtility.OpenAssetIfUsingVCIntegration(obj);
            var comp = obj as Component;
            if (comp != null && comp.gameObject != null && comp.gameObject.activeInHierarchy)
                EditorSceneManager.MarkSceneDirty(comp.gameObject.scene);
        }

        static internal List<AssetReferenceUIRestrictionSurrogate> GatherFilters(SerializedProperty property)
        {
            List<AssetReferenceUIRestrictionSurrogate> restrictions = new List<AssetReferenceUIRestrictionSurrogate>();
            var o = property.serializedObject.targetObject;
            if (o != null)
            {
                var t = o.GetType();
                FieldInfo info = null;
                
                // We need to look into sub types, if any.
                string[] pathParts = property.propertyPath.Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < pathParts.Length; i++)
                {
                    FieldInfo f = t.GetField(pathParts[i],
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (f != null)
                    {
                        t = f.FieldType;
                        info = f;
                    }
                }
                
                if (info != null)
                {
                    var a = info.GetCustomAttributes(false);
                    foreach (var attr in a)
                    {
                        var uiRestriction = attr as AssetReferenceUIRestriction;
                        if (uiRestriction != null)
                        {
                            var surrogate = AssetReferenceUtility.GetSurrogate(uiRestriction.GetType());

                            if (surrogate != null)
                            {
                                var surrogateInstance =
                                    Activator.CreateInstance(surrogate) as AssetReferenceUIRestrictionSurrogate;
                                if (surrogateInstance != null)
                                {
                                    surrogateInstance.Init(uiRestriction);
                                    restrictions.Add(surrogateInstance);
                                }
                            }
                            else
                            {
                                AssetReferenceUIRestrictionSurrogate restriction =
                                    new AssetReferenceUIRestrictionSurrogate();
                                restriction.Init(uiRestriction);
                                restrictions.Add(restriction);
                            }
                        }
                    }
                }
            }

            return restrictions;
        }
        
        static internal List<Object> GetSubAssetsList(AssetReference assetReferenceObject)
        {
            var subAssets = new List<Object>();
            subAssets.Add(null);
            var assetPath = AssetDatabase.GUIDToAssetPath(assetReferenceObject.AssetGUID);

            var repr = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);
            if (repr.Any())
            {
                var subtype = assetReferenceObject.SubOjbectType ?? GetGenericTypeFromAssetReference(assetReferenceObject);
                if (subtype != null)
                    repr = repr.Where(o => subtype.IsInstanceOfType(o)).OrderBy(s => s.name).ToArray();
            }

            subAssets.AddRange(repr);

            var mainType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
            if (mainType == typeof(SpriteAtlas))
            {
                var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(assetPath);
                var sprites = new Sprite[atlas.spriteCount];
                atlas.GetSprites(sprites);
                subAssets.AddRange(sprites.OrderBy(s => s.name));
            }

            return subAssets;
        }
        
        static Type GetGenericTypeFromAssetReference(AssetReference assetReferenceObject)
        {
            var type = assetReferenceObject?.GetType();
            while (type != null)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(AssetReferenceT<>))
                    return type.GenericTypeArguments[0];
                type = type.BaseType;
            }

            return null;
        }
        
        static internal string GetNameForAsset(ref bool referencesSame, SerializedProperty property, bool isNotAddressable, FieldInfo propertyField, string labelText)
        {
            var currentRef =
                property.GetActualObjectForSerializedProperty<AssetReference>(propertyField, ref labelText);

            string nameToUse = currentRef.editorAsset != null ? 
                currentRef.editorAsset.name :
                ConstructNoAssetLabel(propertyField.FieldType);
            
            if (property.serializedObject.targetObjects.Length > 1)
            {
                foreach (var t in property.serializedObject.targetObjects)
                {
                    var serializeObjectMulti = new SerializedObject(t);
                    var sp = serializeObjectMulti.FindProperty(property.propertyPath);
                    var assetRefObject =
                        sp.GetActualObjectForSerializedProperty<AssetReference>(propertyField, ref labelText);
                    if (assetRefObject.AssetGUID != currentRef.AssetGUID)
                    {
                        referencesSame = false;
                        return "--";
                    }
                }
            }

            if (isNotAddressable)
            {
                nameToUse = "Not Addressable - " + nameToUse;
            }

            return nameToUse;
        }

        internal static string ConstructNoAssetLabel(Type t)
        {
            if (t == null || t == typeof(AssetReference))
                return FormatNoAssetString(string.Empty);
            t = GetGenericType(t);
            if (t == null || t == typeof(AssetReference))
                return FormatNoAssetString(string.Empty);
            return FormatNoAssetString(t.Name);
        }

        static string FormatNoAssetString(string n) => string.IsNullOrEmpty(n) ? noAssetString : string.Format(noAssetTypeStringformat, n);

        private static Type GetGenericType(Type t)
        {
            if (t == null)
                return null;
            while (t.GenericTypeArguments.Length > 0)
                t = t.GenericTypeArguments[0];
            if (t.BaseType != null && t.BaseType.GenericTypeArguments.Length == 1)
                t = GetGenericType(t.BaseType);
            if (t.HasElementType)
                t = GetGenericType(t.GetElementType());
            return t;
        }

        static internal string FormatName(string name)
        {
            var formatted = string.IsNullOrEmpty(name) ? "<none>" : name;
            if (formatted.EndsWith("(Clone)"))
                formatted = formatted.Replace("(Clone)", "");
            return formatted;
        }

        static internal bool ValidateDrag(AssetReference assetReferenceObject, List<AssetReferenceUIRestrictionSurrogate> restrictions, List<AssetEntryTreeViewItem> aaEntries, Object[] dropObjReferences, string[] dropPaths)
        {
            if (aaEntries != null)
            {
                if (aaEntries.Count != 1)
                     return true;

                if (aaEntries[0] == null || aaEntries[0].entry == null || aaEntries[0].entry.IsInResources || !ValidateAsset(assetReferenceObject, restrictions, aaEntries[0].entry.AssetPath))
                    return true;
            }
            else if (dropObjReferences != null && dropObjReferences.Length == 1 && AssetDatabase.IsSubAsset(dropObjReferences[0]))
            {
                if (!ValidateAsset(assetReferenceObject, restrictions, dropObjReferences[0]))
                    return true;
            }
            else
            {
                if (dropPaths.Length != 1)
                {
                    return true;
                }
                if (!ValidateAsset(assetReferenceObject, restrictions, DragAndDrop.paths[0]))
                    return true;
            }
            
            return false;
        }

        static internal bool CheckForNewEntry(ref string assetName, AddressableAssetSettings aaSettings, string guid, string checkToForceAddressable)
        {
            var entry = aaSettings.FindAssetEntry(guid);
            if (entry != null)
            {
                assetName = entry.address;
            }
            else
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(path))
                {
                    if (!aaSettings.IsAssetPathInAddressableDirectory(path, out assetName))
                    {
                        assetName = path;
                        if (!string.IsNullOrEmpty(checkToForceAddressable))
                        {
                            var newEntry = aaSettings.CreateOrMoveEntry(guid, aaSettings.DefaultGroup);
                            Addressables.LogFormat("Created AddressableAsset {0} in group {1}.", newEntry.address, aaSettings.DefaultGroup.Name);
                        }
                        else
                        {
                            if (!File.Exists(path))
                            {
                                assetName = "Missing File!";
                            }
                            else
                                return true;
                        }
                    }
                }
                else
                {
                    assetName = "Missing File!";
                }
            }

            return false;
        }
    }
}