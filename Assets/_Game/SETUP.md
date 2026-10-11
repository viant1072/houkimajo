# Houkimajo — Dựng scene chơi thử

## Cách nhanh (khuyên dùng) — 1 click

Đã viết sẵn Editor script tự động dựng toàn bộ scene test (Player, đạn, pool, boss,
1 pattern đạn mẫu) bằng sprite hình khối màu tạm — chưa cần art thật.

1. Mở project trong Unity, đợi compile xong (thanh progress ở góc dưới phải biến mất)
2. Mở scene `Assets/_Game/Scenes/Stage01.unity`
3. Menu bar → **Houkimajo → Setup Playtest Scene (Placeholder Art)**
4. Xem Console: nếu không có lỗi đỏ, bấm **Play**

Điều khiển: WASD/mũi tên di chuyển, giữ **Shift** = focus mode (bay chậm, né chính
xác hơn), giữ **Z** = bắn liên tục.

Script tự tạo:
- `Player` (tag `Player`, có `PlayerController`/`PlayerHealth`/`PlayerShooting`)
- `Boss` (có `BossHealth`/`BulletEmitter`/`BossStateMachine`)
- 2 prefab đạn (`Assets/_Game/Prefabs/Bullets/`) + 2 `BulletPool`
- **3 pha boss** (`Assets/_Game/Data/BossPhases/Test_Phase1-3.asset`), độ khó tăng
  dần theo HP giảm: Radial (100%→66%) → Spiral (66%→33%) → Aimed bắn thẳng vào
  player, nhanh hơn (33%→0%)
- `GameManager` + `ScoreManager` (singleton quản lý state/điểm)
- `HUDCanvas` hiện Score / Lives / Boss HP, cộng chữ to **GAME OVER** / **VICTORY**
  giữa màn hình khi Player hết mạng hoặc Boss hết HP (game tự dừng, `Time.timeScale = 0`)
- Sprite placeholder (`Assets/_Game/Art/Sprites/_Placeholder/`) — **thay bằng art
  thật sau khi gameplay đã "đã" tay**, xem lý do trong `docs/`

Chạy lại script khi đã có Player/Boss trong scene sẽ bị chặn (tránh tạo trùng) —
xoá `Player`/`Boss` thủ công trong Hierarchy trước nếu muốn dựng lại từ đầu (ví dụ
sau khi cập nhật script có thêm HUD mà scene cũ chưa có).

## Nếu gặp lỗi compile

Mở **Window → General → Console**, copy nguyên văn lỗi đỏ gửi lại để debug.

## Sau khi chạy thử được

- Thêm pattern đạn mới: `Assets/_Game/Data/BulletPatterns` → chuột phải →
  **Create → Houkimajo → Bullet Pattern**
- Thêm pha boss mới: tương tự với **Create → Houkimajo → Boss Phase**, rồi kéo vào
  list `Phases` của `BossStateMachine` trên `Boss`
- Viết spec vào `docs/*.md` trước khi mở rộng thêm cơ chế lớn
- Art/animation/âm thanh thật: làm sau khi đã chốt cảm giác gameplay (di chuyển,
  nhịp bắn, độ khó né đạn) — xem lý do ở phần trao đổi trong `docs/README.md`

## Cách làm tay (nếu muốn tự kiểm soát từng bước thay vì dùng script)

<details>
<summary>Mở rộng nếu cần</summary>

1. **Edit → Project Settings → Tags and Layers** → thêm tag `Player`
2. Tạo Player: Sprite + `CircleCollider2D` (Is Trigger) + `Rigidbody2D` (Kinematic) +
   `PlayerController`/`PlayerHealth`/`PlayerShooting`, con `FirePoint`
3. Tạo Bullet prefab (player + enemy): Sprite + `Rigidbody2D` (Kinematic) +
   `CircleCollider2D` (Is Trigger) + `BulletBase`, kéo vào `Prefabs/Bullets/`
4. Tạo `BulletPool` (2 cái), gán prefab tương ứng
5. Tạo Boss: Sprite + Collider + `BossHealth`/`BulletEmitter`/`BossStateMachine`
6. Tạo `BulletPatternSO` và `BossPhaseSO`, gán vào `BossStateMachine`
7. Bấm Play

</details>
