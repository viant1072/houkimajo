using Houkimajo.Systems;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Houkimajo.EditorTools
{
    /// <summary>Gắn ba ảnh đã duyệt vào dữ liệu màn và tạo một renderer nền dùng chung.</summary>
    public static class StageBackgroundBuilder
    {
        private const string ArtDirectory = "Assets/_Game/Art/Sprites/Backgrounds/";

        [MenuItem("Houkimajo/Apply Stage Backgrounds")]
        public static void ApplyToCurrentScene()
        {
            var director = Object.FindAnyObjectByType<StageDirector>();
            if (director == null)
            {
                Debug.LogWarning("[Background] Mở Stage01 có StageDirector trước khi gắn nền.");
                return;
            }

            string[] names = { "bg_stage01_night_forest_v1", "bg_stage02_pumpkin_marsh_v1", "bg_stage03_witch_tower_v1" };
            for (int i = 0; i < names.Length; i++)
            {
                string path = ArtDirectory + names[i] + ".png";
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) throw new System.InvalidOperationException("Thiếu background: " + path);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 100f;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = false;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = FilterMode.Bilinear;
                importer.maxTextureSize = 2048;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect;
                importer.SetTextureSettings(settings);
                importer.SaveAndReimport();
                var stage = AssetDatabase.LoadAssetAtPath<StageSO>($"Assets/_Game/Data/Stages/Stage_{i + 1:00}.asset");
                if (stage == null) throw new System.InvalidOperationException("Thiếu StageSO " + (i + 1));
                var data = new SerializedObject(stage);
                data.FindProperty("background").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                data.ApplyModifiedProperties();
            }

            var background = Object.FindAnyObjectByType<StageBackground>();
            if (background == null)
            {
                var go = new GameObject("StageBackground");
                Undo.RegisterCreatedObjectUndo(go, "Create stage background");
                background = Undo.AddComponent<StageBackground>(go);
            }
            var camera = Camera.main;
            var component = new SerializedObject(background);
            component.FindProperty("director").objectReferenceValue = director;
            component.FindProperty("targetCamera").objectReferenceValue = camera;
            component.ApplyModifiedProperties();
            var renderer = background.GetComponent<SpriteRenderer>();
            Undo.RecordObject(renderer, "Configure stage background");
            renderer.sortingLayerID = SortingLayer.NameToID("Default");
            renderer.sortingOrder = -1000;
            renderer.color = Color.white;
            // Unlit để art giữ màu dù scene có Global Light 2D hay không.
            const string materialPath = "Assets/_Game/Art/Sprites/Backgrounds/BackgroundUnlit.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                if (shader == null) throw new System.InvalidOperationException("Không tìm thấy URP 2D unlit shader.");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, materialPath);
            }
            renderer.sharedMaterial = material;
            background.Apply(AssetDatabase.LoadAssetAtPath<StageSO>("Assets/_Game/Data/Stages/Stage_01.asset"));
            EditorUtility.SetDirty(background);
            EditorSceneManager.MarkSceneDirty(director.gameObject.scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[Background] Đã gắn nền cho 3 màn. Ctrl+S để lưu scene.");
        }
    }
}
