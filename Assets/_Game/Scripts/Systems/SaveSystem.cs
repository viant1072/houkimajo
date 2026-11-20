using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace Houkimajo.Systems
{
    /// <summary>
    /// Dữ liệu lưu menu, chuyển màn và túi Bình HP prototype; giai đoạn 3 sẽ thêm
    /// level, XP, skill, đồ (xem docs/progression.md) — tăng <see cref="version"/> khi đổi cấu trúc.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public int version = 3;

        [Tooltip("Chỉ số màn cao nhất đã qua, -1 = chưa qua màn nào")]
        public int highestStageCleared = -1;

        public int gold;
        public int hpPotionCount;
        public float hpPotionCooldown;
        public bool shopTestGoldSeeded;
        public List<SkillUpgradeSave> skillUpgrades = new List<SkillUpgradeSave>();
    }

    /// <summary>
    /// Đọc/ghi SaveData dạng JSON trong Application.persistentDataPath. Static vì chỉ có
    /// 1 slot save và cần dùng được ở cả menu lẫn màn chơi.
    /// </summary>
    public static class SaveSystem
    {
        private const string FileName = "save.json";

        private static SaveData cached;

        private static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        public static bool HasSave => File.Exists(FilePath);

        /// <summary>Đọc save (cache trong bộ nhớ). Không có file / file hỏng thì trả save mới.</summary>
        public static SaveData Load()
        {
            if (cached != null) return cached;

            cached = new SaveData();
            if (!HasSave) return cached;

            try
            {
                var loaded = JsonUtility.FromJson<SaveData>(File.ReadAllText(FilePath));
                if (loaded != null) cached = loaded;
                if (cached.version < 2)
                {
                    cached.hpPotionCount = 0;
                    cached.hpPotionCooldown = 0f;
                    cached.shopTestGoldSeeded = false;
                    cached.version = 2;
                }
                if (cached.skillUpgrades == null) cached.skillUpgrades = new List<SkillUpgradeSave>();
                if (cached.version < 3) cached.version = 3;
            }
            catch (Exception exception)
            {
                // File hỏng thì chơi tiếp bằng save mới, không làm crash game.
                Debug.LogWarning($"[Save] Không đọc được save, dùng dữ liệu mới: {exception.Message}");
            }

            return cached;
        }

        public static bool Save(SaveData data)
        {
            cached = data;
            try
            {
                string temporaryPath = FilePath + ".tmp";
                Directory.CreateDirectory(Application.persistentDataPath);
                File.WriteAllText(temporaryPath, JsonUtility.ToJson(data, true));
                if (File.Exists(FilePath)) File.Replace(temporaryPath, FilePath, null);
                else File.Move(temporaryPath, FilePath);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[Save] Không ghi được save: {exception.Message}");
                return false;
            }
        }

        /// <summary>Xoá save (Chơi mới).</summary>
        public static void Delete()
        {
            cached = null;
            try
            {
                if (HasSave) File.Delete(FilePath);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[Save] Không xoá được save: {exception.Message}");
            }
        }

        // Tắt Domain Reload thì cache còn giữ dữ liệu lần Play trước -> đọc lại từ file.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCache()
        {
            cached = null;
        }
    }
}
