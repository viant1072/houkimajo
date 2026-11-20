# Boss

> Khớp với `Assets/_Game/Scripts/Boss/` — namespace `Houkimajo.Boss`

## Goal

Boss là thử thách cuối mỗi stage, xuất hiện sau khi hết các wave quái. Nhiều pha, mỗi
pha một kiểu bắn khác nhau, khó dần theo HP. Hạ boss nhận phần thưởng lớn vàng + XP.

## Requirements

- Giữ kiến trúc hiện có: `BossStateMachine` + list `BossPhaseSO`, chuyển pha theo % HP
  hoặc hết giờ pha. Thêm pha = thêm asset, không sửa code.
- `BossHealth` implement `IDamageable` (xem [`bullets.md`](bullets.md)).
- Boss có `RewardData { gold, xp }` cấu hình trong Inspector — Stage01 đề xuất 300 vàng / 200 XP.
- Boss **không** có sẵn trong scene ngay từ đầu: `StageDirector` kích hoạt boss sau
  shop trước-boss (xem [`systems.md`](systems.md)).
- Boss xuất hiện ở giữa phía trên arena. Vì player giờ có thể bay khắp màn, boss nên
  di chuyển chậm (lượn qua lại) để không đứng yên làm bia — kiểu di chuyển đặt trong
  `BossPhaseSO` (`Static` / `Hover` / `FollowPlayerX`).
- Va chạm thân boss = mất 1 mạng như trúng đạn.

### Hình ảnh gameplay

- Ba mẫu đã duyệt: Nữ Hoàng Bướm Đêm (Rừng Đêm), Bá Tước Bí Ngô (Đầm Lầy Bí Ngô),
  Phù Thuỷ Tinh Tú (Tháp Phù Thuỷ).
- Bộ PNG nền trong suốt nằm trong `Assets/_Game/Art/Sprites/Bosses/`, tên
  `boss_moth_queen_gameplay_v1`, `boss_pumpkin_regent_gameplay_v1`,
  `boss_astral_archwitch_gameplay_v1`. Mỗi mẫu một pose idle, PPU512, pivot giữa.
- Vòng lặp idle bốn frame riêng cho từng boss nằm ở `boss_*_idle_v1.png`: Bướm Đêm
  vỗ cánh (6fps), Bí Ngô chuyển động dây leo/áo choàng (5fps), Phù Thuỷ đung đưa tóc/áo (5fps).
- Prefab xem thử trong `Assets/_Game/Prefabs/Bosses/Visuals`, tạo/cập nhật bằng menu
  **Houkimajo > Create Boss Animated Visuals**. Dùng chung bộ phát frame
  `EnemySpriteAnimation`, không thêm trạng thái AI/phase. Pause dừng animation.
- Mỗi `StageSO` giữ sprite, bốn frame idle, tốc độ và bán kính collider thân boss.
  Màn1/2/3 lần lượt dùng Bướm Đêm/Bí Ngô/Phù Thuỷ Tinh Tú, tốc độ6/5/5fps.
- `StageDirector` gắn art trước khi bật boss, đặt màu trắng để giữ palette gốc;
  stage chưa có art tiếp tục dùng hình scene và bossTint. Animation dừng theo pause
  (bao gồm thoại/shop), reset frame0 khi boss được kích hoạt lại.
- Collider thân có bán kính0.42/0.52/0.42 world units ở scale1, không lấy toàn bộ
  cánh, tay dây leo hoặc áo choàng làm hitbox. Không đổi HP, phase, đạn hay phần thưởng.
- Menu **Houkimajo > Apply Boss Art To Gameplay** gắn/cập nhật art cho cả ba stage
  và boss trong scene đang mở. Setup/Rebuild cũng gọi bước này. Chạy lại giữ tốc độ
  animation và collider đã chỉnh trên data, không tạo thêm boss/renderer/animation.

## Non-goals

- Không làm nhiều boss trong 1 stage.
- Không làm "spell card" có tên/hiệu ứng riêng mỗi pha (có thể làm sau).

## Open questions

- Boss có nên triệu hồi thêm quái nhỏ ở một số pha không?
