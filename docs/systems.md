# Systems

> Khớp với `Assets/_Game/Scripts/Systems/` — namespace `Houkimajo.Systems`

## Goal

Điều phối luồng chơi từ lúc mở game tới khi phá đảo: menu chính → các màn chơi nối
tiếp nhau (mỗi màn: các wave quái → boss) → lưu tiến trình. Các module khác (Enemies,
Boss, Progression, UI) không phải tự biết thứ tự diễn ra.

## Requirements

### Scene & luồng tổng

```
MainMenu ──Chơi tiếp / Chơi mới──▶ Stage01.unity (chạy StageSO số N)
                                         │
     ┌───────────────────────────────────┤
     ▼                                   ▼
 Thắng màn N                          Hết mạng
 → lưu (màn đã qua, vàng)             → lưu vàng
 → "HOÀN THÀNH" 3s                    → GAME OVER: Enter = chơi lại màn N,
 → tự vào màn N+1                                 Esc = về menu
   (màn cuối: "PHÁ ĐẢO!" 4s → về menu)
```

- Chỉ có **2 scene**: `MainMenu` và `Stage01`. `Stage01` là scene gameplay dùng chung
  cho **mọi màn** — nội dung màn (wave, boss) lấy từ `StageSO`, không tạo 1 scene/màn.
  Thêm màn mới = tạo asset `StageSO` và thêm vào `StageCatalogSO`, không sửa code.
- `GameSession` (static) giữ chỉ số màn đang chơi giữa 2 lần load scene. Mở thẳng
  `Stage01` trong Editor rồi bấm Play thì chơi màn 1 (tiện test).
- `SceneFlow` (static): `LoadMainMenu()`, `LoadStage(index)`. Luôn trả
  `Time.timeScale = 1` trước khi load.

### Màn chơi

- `StageSO` (Create → Houkimajo → Stage): `id`, `stageName`, danh sách
  `StageWave { WaveSO wave, bool openShopAfter }`, cấu hình boss (`bossPhases`,
  `bossMaxHP`, `bossTint`), `clearGold` (thưởng khi qua màn).
- `StageCatalogSO` (Create → Houkimajo → Stage Catalog): danh sách màn theo thứ tự.
- 3 màn ban đầu, khó dần:

| Màn | Tên | Wave | Boss HP | Pattern boss |
|---|---|---|---|---|
| 1 | Rừng Đêm | 2 | 1000 | Radial → Spiral → Aimed (như hiện tại) |
| 2 | Đầm Lầy Bí Ngô | 3 | 1600 | Radial dày → Spiral kép nhanh → Aimed nhanh |
| 3 | Tháp Phù Thuỷ | 3 | 2400 | Spiral → Radial rất dày → Spiral điên cuồng |

- `StageDirector` chạy `StageSO`:
  1. Hiện "MÀN n — tên màn" 2s.
  2. Từng wave: hiện "WAVE i/n", gọi `EnemySpawner`, chờ `OnWaveCleared`. Hết wave:
     xoá hết đạn địch, pickup còn lại tự bay về player, nghỉ 1.5s.
  3. Hiện "BOSS!", bật boss với phase + HP của màn. Boss hạ → tắt boss, xoá đạn địch.
  4. Cộng `clearGold`, lưu, phát `OnStageCleared`, đợi rồi tự chuyển màn.
- Boss nằm sẵn trong scene nhưng bị ẩn tới lúc tới lượt.
- `openShopAfter` đã có trong dữ liệu nhưng **chưa dùng** — shop làm ở giai đoạn 5.

### Lưu game (bản tối thiểu)

- `SaveSystem` ghi JSON vào `Application.persistentDataPath/save.json`:
  `version`, `highestStageCleared` (-1 = chưa qua màn nào), `gold`.
  Giai đoạn 3 sẽ mở rộng thêm level, XP, skill, đồ (xem `progression.md`).
- **Chơi tiếp** = vào màn ngay sau màn cao nhất đã qua (đã phá đảo thì vào màn cuối).
- **Chơi mới** = xoá save, vào màn 1.
- Vàng: đầu màn nạp từ save vào `Wallet`; cuối màn (thắng hoặc thua) ghi lại.

### GameState

- Enum: `MainMenu`, `Playing`, `Shop`, `Paused`, `GameOver`, `Victory`.
- `Shop`, `Paused`, `GameOver`, `Victory` đều đặt `Time.timeScale = 0`.

### Score

- Giữ `ScoreManager` làm điểm phụ: điểm = sát thương gây ra + graze × 10.
  Vàng và XP là hệ thống riêng.

## Non-goals

- Không có màn chọn màn (chỉ Chơi tiếp / Chơi mới). Chơi lại màn cũ để cày: sau này.
- Không có cài đặt, nút thoát game ở menu.
- Không làm thoại / cutscene trước boss.
- Chưa có pause bằng Esc trong lúc chơi.

## Open questions

- Phá đảo màn 3 rồi thì sao: chỉ về menu, hay mở New Game+ (chơi lại từ màn 1 khó hơn)?
