using UnityEngine;

namespace Houkimajo.Systems
{
    /// <summary>Nền theo StageSO; giữ tỷ lệ ảnh và phủ kín camera khi đổi cửa sổ.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class StageBackground : MonoBehaviour
    {
        [SerializeField] private StageDirector director;
        [SerializeField] private Camera targetCamera;
        private SpriteRenderer backgroundRenderer;

        private void Awake()
        {
            backgroundRenderer = GetComponent<SpriteRenderer>();
            if (targetCamera == null) targetCamera = Camera.main;
        }

        private void OnEnable()
        {
            if (director != null) director.OnStageStarted += HandleStageStarted;
            if (director != null && director.CurrentStage != null) Apply(director.CurrentStage);
        }

        private void Start()
        {
            // Hỗ trợ cả thứ tự Start trước/sau director và component bật lại giữa màn.
            if (director != null && director.CurrentStage != null) Apply(director.CurrentStage);
        }

        private void OnDisable()
        {
            if (director != null) director.OnStageStarted -= HandleStageStarted;
        }

        private void HandleStageStarted(StageSO stage, int index, int count)
        {
            Apply(stage);
        }

        public void Apply(StageSO stage)
        {
            if (backgroundRenderer == null) backgroundRenderer = GetComponent<SpriteRenderer>();
            backgroundRenderer.sprite = stage != null ? stage.Background : null;
            backgroundRenderer.enabled = backgroundRenderer.sprite != null;
            FitToCamera();
        }

        private void LateUpdate()
        {
            FitToCamera();
        }

        public void FitToCamera()
        {
            if (targetCamera == null || !targetCamera.orthographic || backgroundRenderer == null
                || backgroundRenderer.sprite == null) return;
            Vector2 size = backgroundRenderer.sprite.bounds.size;
            if (size.x <= 0f || size.y <= 0f) return;
            float height = targetCamera.orthographicSize * 2f;
            // Cover thay vì kéo giãn để nét vẽ giữ tỷ lệ ở cả cửa sổ rộng/hẹp.
            float scale = Mathf.Max(height * targetCamera.aspect / size.x, height / size.y);
            transform.localScale = new Vector3(scale, scale, 1f);
            Vector3 cameraPosition = targetCamera.transform.position;
            transform.position = new Vector3(cameraPosition.x, cameraPosition.y, 5f);
        }
    }
}
