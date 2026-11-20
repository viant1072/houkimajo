using System;
using Houkimajo.Boss;
using Houkimajo.Systems;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Houkimajo.EditorTools
{
    /// <summary>Gắn ánh sáng/flash và pool tia lóe, giữ gameplay và vật liệu renderer gốc.</summary>
    public static class CombatFeedbackBuilder
    {
        [MenuItem("Houkimajo/Apply Combat Lights + Hit Effects")]
        public static void Apply()
        {
            EnsureFolder("Assets/_Game/Art/Materials");
            var shader = Shader.Find("Houkimajo/CombatHitFlash");
            if (shader == null) throw new InvalidOperationException("Thiếu shader CombatHitFlash.");
            const string materialPath = "Assets/_Game/Art/Materials/CombatHitFlash.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, materialPath);
            }
            Sprite impact = null;
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath("Assets/_Game/Art/Sprites/Skills/Q/q_magic_missile_v1.png"))
                if (asset is Sprite sprite && sprite.name == "Q_MagicMissile_Impact") impact = sprite;
            if (impact == null) throw new InvalidOperationException("Thiếu sprite Q_MagicMissile_Impact.");

            const string playerPath = "Assets/_Game/Prefabs/Bullets/PlayerBullet.prefab";
            var playerBullet = PrefabUtility.LoadPrefabContents(playerPath);
            try
            {
                var child = playerBullet.transform.Find("ArcaneLight");
                if (child == null)
                {
                    child = new GameObject("ArcaneLight").transform;
                    child.SetParent(playerBullet.transform, false);
                }
                var light = child.GetComponent<Light2D>();
                if (light == null) light = child.gameObject.AddComponent<Light2D>();
                light.lightType = Light2D.LightType.Point;
                light.color = new Color(0.65f, 0.3f, 1f);
                light.intensity = 0.75f;
                light.pointLightInnerRadius = 0.06f;
                light.pointLightOuterRadius = 0.65f;
                light.falloffIntensity = 0.7f;
                PrefabUtility.SaveAsPrefabAsset(playerBullet, playerPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(playerBullet); }

            const string enemyPath = "Assets/_Game/Prefabs/Enemies/Enemy.prefab";
            var enemy = PrefabUtility.LoadPrefabContents(enemyPath);
            try { ApplyFlash(enemy, material); PrefabUtility.SaveAsPrefabAsset(enemy, enemyPath); }
            finally { PrefabUtility.UnloadPrefabContents(enemy); }

            var boss = UnityEngine.Object.FindAnyObjectByType<BossHealth>(FindObjectsInactive.Include);
            if (boss != null) ApplyFlash(boss.gameObject, material);
            var feedback = UnityEngine.Object.FindAnyObjectByType<CombatFeedback>(FindObjectsInactive.Include);
            if (feedback == null)
            {
                var go = new GameObject("CombatFeedback");
                Undo.RegisterCreatedObjectUndo(go, "Create combat feedback");
                feedback = Undo.AddComponent<CombatFeedback>(go);
            }
            var serialized = new SerializedObject(feedback);
            serialized.FindProperty("impactSprite").objectReferenceValue = impact;
            serialized.ApplyModifiedProperties();
            EditorSceneManager.MarkSceneDirty(feedback.gameObject.scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[CombatFeedback] Đã gắn đèn đạn, lóe đầu bắn, tia hit và flash quái/boss. Ctrl+S để lưu scene.");
        }

        private static void ApplyFlash(GameObject actor, Material material)
        {
            var flash = actor.GetComponent<HitFlash>();
            if (flash == null) flash = actor.AddComponent<HitFlash>();
            var serialized = new SerializedObject(flash);
            serialized.FindProperty("flashMaterial").objectReferenceValue = material;
            serialized.ApplyModifiedPropertiesWithoutUndo();
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
