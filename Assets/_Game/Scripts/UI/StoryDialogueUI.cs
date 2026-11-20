using System.Collections;
using Houkimajo.Systems;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Houkimajo.UI
{
    public class StoryDialogueUI : MonoBehaviour
    {
        private GameObject overlay;
        private Text speakerText;
        private Text lineText;
        private Font font;

        internal IEnumerator Read((string speaker, string line)[] lines)
        {
            while (GameManager.Instance.CurrentState == GameState.Shop || GameManager.Instance.CurrentState == GameState.Paused)
                yield return null;
            if (GameManager.Instance.CurrentState != GameState.Playing) yield break;
            if (overlay == null) Build();
            GameManager.Instance.SetState(GameState.Dialogue);
            overlay.SetActive(true);
            foreach (var entry in lines)
            {
                speakerText.text = entry.speaker;
                lineText.text = entry.line;
                // Mỗi câu cần một lần nhấn mới, kể cả ngay khi mở khung.
                do
                {
                    yield return null;
                    if (GameManager.Instance.CurrentState != GameState.Dialogue)
                    {
                        Close();
                        yield break;
                    }
                    if (Keyboard.current?.escapeKey.wasPressedThisFrame == true)
                    {
                        Close();
                        yield break;
                    }
                }
                while (Keyboard.current?.spaceKey.wasPressedThisFrame != true
                    && Keyboard.current?.enterKey.wasPressedThisFrame != true);
            }
            Close();
        }

        private void Close()
        {
            if (overlay != null) overlay.SetActive(false);
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Dialogue)
                GameManager.Instance.SetState(GameState.Playing);
        }

        private void OnDisable() => Close();

        private void Build()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            overlay = new GameObject("StoryDialogue", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            overlay.transform.SetParent(transform, false);
            var canvas = overlay.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = overlay.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;

            var shade = Region("Backdrop", overlay.transform, Vector2.zero, Vector2.one);
            shade.gameObject.AddComponent<Image>().color = new Color(0.04f, 0.02f, 0.09f, 0.55f);
            var panel = Region("Letter", shade, new Vector2(0.07f, 0.12f), new Vector2(0.93f, 0.47f));
            panel.gameObject.AddComponent<ArcanePanelGraphic>().Configure(ArcanePanelGraphic.PanelStyle.Window);
            speakerText = Label("Speaker", panel, 0.68f, 0.91f, 26, new Color(1f, 0.82f, 0.55f));
            lineText = Label("Line", panel, 0.22f, 0.68f, 27, Color.white);
            var hint = Label("Controls", panel, 0.04f, 0.21f, 17, new Color(0.83f, 0.78f, 0.9f));
            hint.alignment = TextAnchor.MiddleRight;
            hint.text = "Space / Enter · Tiếp      Esc · Bỏ qua đoạn";
        }

        private Text Label(string id, Transform parent, float bottom, float top, int size, Color color)
        {
            var rect = Region(id, parent, new Vector2(0.05f, bottom), new Vector2(0.95f, top));
            var label = rect.gameObject.AddComponent<Text>();
            label.font = font;
            label.fontSize = size;
            label.color = color;
            label.alignment = TextAnchor.MiddleLeft;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 12;
            label.resizeTextMaxSize = size;
            label.raycastTarget = false;
            return label;
        }

        private static RectTransform Region(string id, Transform parent, Vector2 min, Vector2 max)
        {
            var rect = (RectTransform)new GameObject(id, typeof(RectTransform)).transform;
            rect.SetParent(parent, false);
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return rect;
        }
    }
}
