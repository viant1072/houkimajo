using System;
using Houkimajo.Enemies;
using UnityEditor;
using UnityEngine;

namespace Houkimajo.EditorTools
{
    /// <summary>Đóng gói hình ảnh boss có vòng lặp idle; không gắn AI hay đổi stage hiện có.</summary>
    public static class BossAnimationBuilder
    {
        private const string OutputFolder = "Assets/_Game/Prefabs/Bosses/Visuals";
        private static readonly string[] Slugs = { "moth_queen", "pumpkin_regent", "astral_archwitch" };
        private static readonly string[] Names = { "MothQueen", "PumpkinRegent", "AstralArchwitch" };
        private static readonly float[] Rates = { 6f, 5f, 5f };

        [MenuItem("Houkimajo/Create Boss Animated Visuals")]
        public static void BuildVisualPrefabs()
        {
            EnsureFolder(OutputFolder);
            for (int i = 0; i < Slugs.Length; i++)
            {
                string sheet = "Assets/_Game/Art/Sprites/Bosses/boss_" + Slugs[i] + "_idle_v1.png";
                var frames = new Sprite[4];
                foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(sheet))
                    if (asset is Sprite sprite)
                        for (int frame = 0; frame < frames.Length; frame++)
                            if (sprite.name == "Boss_" + Names[i] + "_Idle_" + frame) frames[frame] = sprite;
                for (int frame = 0; frame < frames.Length; frame++)
                    if (frames[frame] == null) throw new InvalidOperationException("Thiếu frame " + frame + " trong " + sheet);

                string path = OutputFolder + "/Boss_" + Names[i] + "_AnimatedVisual.prefab";
                bool exists = AssetDatabase.LoadAssetAtPath<GameObject>(path) != null;
                var visual = exists ? PrefabUtility.LoadPrefabContents(path)
                    : new GameObject("Boss_" + Names[i] + "_AnimatedVisual");
                try
                {
                    var renderer = visual.GetComponent<SpriteRenderer>();
                    if (renderer == null) renderer = visual.AddComponent<SpriteRenderer>();
                    renderer.sprite = frames[0];
                    if (!exists) renderer.sortingOrder = 2;
                    // Bộ phát này chỉ đổi sprite nên dùng chung được cho quái và boss.
                    var animation = visual.GetComponent<EnemySpriteAnimation>();
                    if (animation == null) animation = visual.AddComponent<EnemySpriteAnimation>();
                    float rate = exists ? new SerializedObject(animation).FindProperty("framesPerSecond").floatValue : Rates[i];
                    animation.Configure(frames, rate);
                    PrefabUtility.SaveAsPrefabAsset(visual, path);
                }
                finally
                {
                    if (exists) PrefabUtility.UnloadPrefabContents(visual);
                    else UnityEngine.Object.DestroyImmediate(visual);
                }
                Debug.Log("[BossAnimation] " + Names[i] + ": 4 frames, " + Rates[i] + " fps; " + path);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("BOSS_ANIMATED_VISUALS_READY: 3 prefabs / 12 imported frames.");
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
