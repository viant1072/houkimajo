using UnityEngine;
using UnityEngine.UI;
using Houkimajo.Systems;

namespace Houkimajo.UI
{
    /// <summary>
    /// Thông tin màn trên HUD: dòng "Màn 1/3 · Wave 2/2 · Vàng 120" ở góc, và chữ to giữa
    /// màn ("MÀN 1 — Rừng Đêm", "WAVE 2/3", "BOSS!") tự tắt sau vài giây. Chỉ dựng lại
    /// string khi có event, không làm mỗi frame.
    /// </summary>
    public class StageHudUI : MonoBehaviour
    {
        [SerializeField] private StageDirector director;
        [SerializeField] private Wallet wallet;
        [SerializeField] private Text infoText;
        [SerializeField] private Text bannerText;

        [SerializeField] private float stageBannerDuration = 2f;
        [SerializeField] private float shortBannerDuration = 1.5f;

        private float bannerHideTime = -1f;

        private void OnEnable()
        {
            if (director != null)
            {
                director.OnStageStarted += HandleStageStarted;
                director.OnWaveStarted += HandleWaveStarted;
                director.OnBossStarted += HandleBossStarted;
                director.OnStageCleared += HandleStageEnded;
                director.OnStageFailed += HandleStageFailed;
            }
            if (wallet != null) wallet.OnGoldChanged += HandleGoldChanged;
            if (bannerText != null) bannerText.gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            if (director != null)
            {
                director.OnStageStarted -= HandleStageStarted;
                director.OnWaveStarted -= HandleWaveStarted;
                director.OnBossStarted -= HandleBossStarted;
                director.OnStageCleared -= HandleStageEnded;
                director.OnStageFailed -= HandleStageFailed;
            }
            if (wallet != null) wallet.OnGoldChanged -= HandleGoldChanged;
        }

        private void Update()
        {
            if (bannerHideTime > 0f && Time.time >= bannerHideTime)
            {
                bannerHideTime = -1f;
                if (bannerText != null) bannerText.gameObject.SetActive(false);
            }
        }

        private void HandleStageStarted(StageSO stage, int index, int count)
        {
            ShowBanner($"MÀN {index + 1} — {stage.stageName}", stageBannerDuration);
            RefreshInfo();
        }

        private void HandleWaveStarted(int wave, int total)
        {
            ShowBanner($"WAVE {wave}/{total}", shortBannerDuration);
            RefreshInfo();
        }

        private void HandleBossStarted()
        {
            ShowBanner("BOSS!", shortBannerDuration);
            RefreshInfo();
        }

        private void HandleStageEnded(bool hasNextStage) { HideBanner(); }
        private void HandleStageFailed() { HideBanner(); }
        private void HandleGoldChanged(int gold) { RefreshInfo(); }

        private void ShowBanner(string message, float duration)
        {
            if (bannerText == null) return;
            bannerText.text = message;
            bannerText.gameObject.SetActive(true);
            bannerHideTime = Time.time + duration;
        }

        private void HideBanner()
        {
            bannerHideTime = -1f;
            if (bannerText != null) bannerText.gameObject.SetActive(false);
        }

        private void RefreshInfo()
        {
            if (infoText == null || director == null) return;

            string wavePart = director.IsBossPhase
                ? "Boss"
                : $"Wave {Mathf.Max(1, director.CurrentWave)}/{director.WaveCount}";
            int gold = wallet != null ? wallet.Gold : 0;
            infoText.text = $"Màn {director.StageIndex + 1}/{director.StageCount} · {wavePart} · Vàng {gold}";
        }
    }
}
