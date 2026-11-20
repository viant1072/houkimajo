using System;
using Houkimajo.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Houkimajo.EditorTools
{
    /// <summary>Gắn pose giơ tay và cầu phép tạo sẵn vào Player, không đổi collider/profile đạn.</summary>
    public static class PlayerCastingBuilder
    {
        [MenuItem("Houkimajo/Apply Player Casting Animation")]
        public static void Apply()
        {
            var shooting = UnityEngine.Object.FindAnyObjectByType<PlayerShooting>();
            if (shooting == null) { Debug.LogWarning("Mở Stage01 có Player trước khi gắn casting."); return; }
            var animation = shooting.GetComponentInChildren<PlayerFlightAnimation>(true);
            if (animation == null) { Debug.LogWarning("Gắn animation bay trước khi gắn pose niệm phép."); return; }
            var frames = new Sprite[4];
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath("Assets/_Game/Art/Sprites/Player/player_witch_cast_v1.png"))
                if (asset is Sprite sprite)
                    for (int i = 0; i < frames.Length; i++) if (sprite.name == "Player_Cast_" + i) frames[i] = sprite;
            for (int i = 0; i < frames.Length; i++)
                if (frames[i] == null) throw new InvalidOperationException("Thiếu frame casting " + i);
            var animated = new SerializedObject(animation);
            var array = animated.FindProperty("castingFrames");
            array.arraySize = frames.Length;
            for (int i = 0; i < frames.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
            animated.ApplyModifiedProperties();

            var orbSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Game/Art/Sprites/Player/player_cast_orb_v1.png");
            if (orbSprite == null) throw new InvalidOperationException("Thiếu sprite cầu phép.");
            var child = shooting.transform.Find("CastOrb");
            if (child == null)
            {
                child = new GameObject("CastOrb").transform;
                child.SetParent(shooting.transform, false);
                Undo.RegisterCreatedObjectUndo(child.gameObject, "Create casting orb");
            }
            var renderer = child.GetComponent<SpriteRenderer>();
            if (renderer == null) renderer = child.gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = orbSprite;
            renderer.sortingOrder = 5;
            var light = child.GetComponent<Light2D>();
            if (light == null) light = child.gameObject.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Point;
            light.color = new Color(0.65f, 0.3f, 1f);
            light.pointLightInnerRadius = 0.06f;
            light.pointLightOuterRadius = 0.6f;
            light.intensity = 0f;
            light.falloffIntensity = 0.7f;
            child.gameObject.SetActive(false);

            var orb = shooting.GetComponent<PlayerCastingOrb>();
            if (orb == null) orb = Undo.AddComponent<PlayerCastingOrb>(shooting.gameObject);
            var serialized = new SerializedObject(orb);
            serialized.FindProperty("orbRenderer").objectReferenceValue = renderer;
            serialized.FindProperty("orbLight").objectReferenceValue = light;
            serialized.ApplyModifiedProperties();
            EditorSceneManager.MarkSceneDirty(shooting.gameObject.scene);
            Debug.Log("[PlayerCasting] Đã gắn 4 pose niệm phép và cầu bắn theo hướng ngắm. Ctrl+S để lưu.");
        }
    }
}
