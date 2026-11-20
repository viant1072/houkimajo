using System;
using Houkimajo.Boss;
using Houkimajo.Enemies;
using Houkimajo.Systems;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Houkimajo.EditorTools
{
    /// <summary>Gắn art boss theo stage, giữ chỉ số chiến đấu và chỉ tạo một bộ phát trên boss.</summary>
    public static class BossGameplayBuilder
    {
        private static readonly string[] Slugs = { "moth_queen", "pumpkin_regent", "astral_archwitch" };
        private static readonly string[] Names = { "MothQueen", "PumpkinRegent", "AstralArchwitch" };
        private static readonly float[] Rates = { 6f, 5f, 5f };
        private static readonly float[] Radii = { 0.42f, 0.52f, 0.42f };

        [MenuItem("Houkimajo/Apply Boss Art To Gameplay")]
        public static void Apply()
        {
            StageSO firstStage = null;
            for (int i = 0; i < Slugs.Length; i++)
            {
                string stagePath = "Assets/_Game/Data/Stages/Stage_0" + (i + 1) + ".asset";
                var stage = AssetDatabase.LoadAssetAtPath<StageSO>(stagePath);
                if (stage == null) throw new InvalidOperationException("Thiếu stage: " + stagePath);
                var frames = new Sprite[4];
                string sheet = "Assets/_Game/Art/Sprites/Bosses/boss_" + Slugs[i] + "_idle_v1.png";
                foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(sheet))
                    if (asset is Sprite sprite)
                        for (int frame = 0; frame < frames.Length; frame++)
                            if (sprite.name == "Boss_" + Names[i] + "_Idle_" + frame) frames[frame] = sprite;
                for (int frame = 0; frame < frames.Length; frame++)
                    if (frames[frame] == null) throw new InvalidOperationException("Thiếu frame " + frame + " của " + Names[i]);

                bool hasAnimation = stage.bossAnimationFrames != null && stage.bossAnimationFrames.Length > 0;
                stage.bossSprite = frames[0];
                stage.bossAnimationFrames = frames;
                if (!hasAnimation) stage.bossAnimationFramesPerSecond = Rates[i];
                if (stage.bossColliderRadius <= 0f) stage.bossColliderRadius = Radii[i];
                EditorUtility.SetDirty(stage);
                if (i == 0) firstStage = stage;
            }
            AssetDatabase.SaveAssets();

            var health = UnityEngine.Object.FindAnyObjectByType<BossHealth>(FindObjectsInactive.Include);
            if (health == null)
            {
                Debug.Log("[BossArt] Đã gắn art cho 3 stage. Mở Stage01 rồi chạy lại để cập nhật boss trong scene.");
                return;
            }
            var renderer = health.GetComponent<SpriteRenderer>();
            if (renderer == null) throw new InvalidOperationException("Boss thiếu SpriteRenderer.");
            var animation = health.GetComponent<EnemySpriteAnimation>();
            if (animation == null) animation = Undo.AddComponent<EnemySpriteAnimation>(health.gameObject);
            Undo.RecordObjects(new UnityEngine.Object[] { renderer, animation }, "Apply boss gameplay art");
            renderer.sprite = firstStage.bossSprite;
            renderer.color = Color.white;
            if (renderer.sortingOrder == 0) renderer.sortingOrder = 2;
            animation.Configure(firstStage.bossAnimationFrames, firstStage.bossAnimationFramesPerSecond);
            EditorUtility.SetDirty(animation);
            EditorSceneManager.MarkSceneDirty(health.gameObject.scene);
            Debug.Log("[BossArt] Đã gắn Bướm Đêm/Bí Ngô/Phù Thuỷ vào 3 stage; Ctrl+S để lưu scene.");
        }
    }
}
