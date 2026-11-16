using System;
using Houkimajo.Enemies;
using UnityEditor;
using UnityEngine;

namespace Houkimajo.EditorTools
{
    /// <summary>Tạo prefab chỉ chứa hình ảnh để xem thử; không thay quái/wave đang có.</summary>
    public static class EnemyAnimationBuilder
    {
        private const string OutputFolder = "Assets/_Game/Prefabs/Enemies/Visuals";
        private static readonly string[] Slugs =
        {
            "shadow_bat", "ember_firefly", "flying_pumpkin", "wisp_ghost", "cursed_grimoire", "moon_jelly"
        };
        private static readonly string[] Names =
        {
            "ShadowBat", "EmberFirefly", "FlyingPumpkin", "WispGhost", "CursedGrimoire", "MoonJelly"
        };
        private static readonly float[] Rates = { 6f, 10f, 5f, 5f, 6f, 4f };

        [MenuItem("Houkimajo/Create Enemy Animated Visuals")]
        public static void BuildVisualPrefabs()
        {
            EnsureFolder(OutputFolder);
            for (int i = 0; i < Slugs.Length; i++)
            {
                string sheetPath = "Assets/_Game/Art/Sprites/Enemies/enemy_" + Slugs[i] + "_flight_v1.png";
                var frames = new Sprite[4];
                foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(sheetPath))
                {
                    if (!(asset is Sprite sprite)) continue;
                    for (int frame = 0; frame < frames.Length; frame++)
                        if (sprite.name == "Enemy_" + Names[i] + "_Flight_" + frame) frames[frame] = sprite;
                }
                for (int frame = 0; frame < frames.Length; frame++)
                    if (frames[frame] == null) throw new InvalidOperationException("Thiếu frame: " + sheetPath + " / " + frame);

                string prefabPath = OutputFolder + "/Enemy_" + Names[i] + "_AnimatedVisual.prefab";
                bool exists = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null;
                var visual = exists ? PrefabUtility.LoadPrefabContents(prefabPath)
                    : new GameObject("Enemy_" + Names[i] + "_AnimatedVisual");
                try
                {
                    var renderer = visual.GetComponent<SpriteRenderer>();
                    if (renderer == null) renderer = visual.AddComponent<SpriteRenderer>();
                    renderer.sprite = frames[0];
                    if (!exists) renderer.sortingOrder = 1;
                    var animation = visual.GetComponent<EnemySpriteAnimation>();
                    if (animation == null) animation = visual.AddComponent<EnemySpriteAnimation>();
                    var serialized = new SerializedObject(animation);
                    var array = serialized.FindProperty("frames");
                    array.arraySize = frames.Length;
                    for (int frame = 0; frame < frames.Length; frame++)
                        array.GetArrayElementAtIndex(frame).objectReferenceValue = frames[frame];
                    if (!exists) serialized.FindProperty("framesPerSecond").floatValue = Rates[i];
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                    PrefabUtility.SaveAsPrefabAsset(visual, prefabPath);
                }
                finally
                {
                    if (exists) PrefabUtility.UnloadPrefabContents(visual);
                    else UnityEngine.Object.DestroyImmediate(visual);
                }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[Houkimajo] Đã tạo/cập nhật 6 prefab animation quái trong " + OutputFolder);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int slash = path.LastIndexOf('/');
            string parent = path.Substring(0, slash);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, path.Substring(slash + 1));
        }
    }
}
