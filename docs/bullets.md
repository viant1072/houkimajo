# Bullets

> Khớp với `Assets/_Game/Scripts/Bullets/` — namespace `Houkimajo.Bullets`

## Goal

Hệ thống đạn dùng chung cho player, quái, boss và skill: hiệu năng cao (hàng trăm viên
cùng lúc), pattern đạn cấu hình bằng data, và hỗ trợ các skill tác động lên đạn địch
(xoá, làm chậm, đóng băng).

## Requirements

### Pool

- **Mọi đạn/projectile đi qua pool**, không `Instantiate`/`Destroy` lúc runtime (rule cứng).
- `BulletPool` phải **theo dõi danh sách đạn đang hoạt động** để skill có thể:
  - `ClearInRadius(center, radius)` — trả về số viên đã xoá (Hoá Giải dùng để rớt xu)
  - `ClearAlongLine(...)` — Tia Sao Băng
  - `SpeedScale` (0..1) cho cả pool — Ngưng Đọng đặt về 0 trong vài giây
- Projectile skill có hành vi riêng (đuổi mục tiêu, nổ, xuyên thấu) → tạo pool riêng
  cho từng loại, dùng chung 1 lớp pool generic để không viết lại logic pool.

### Gây sát thương

- Thêm interface `IDamageable { void TakeDamage(int amount); }`, đặt trong namespace này.
  `BossHealth` và `EnemyHealth` implement nó.
- Đạn player gọi `IDamageable` thay vì gọi thẳng `BossHealth` như hiện tại — để đánh
  được cả quái lẫn boss mà `Bullets` không cần biết chi tiết từng loại.
- Sát thương truyền vào lúc `Launch(...)` (lấy từ `PlayerStats`), không cố định trên prefab.
- Phát event sát thương gây ra (để tích năng lượng ultimate) thay vì gọi thẳng
  `ScoreManager`.
- Hỗ trợ đạn **xuyên thấu** (không trả về pool khi trúng, trúng tối đa N mục tiêu).

### Hệ số tốc độ

- `BulletBase` có hệ số tốc độ cục bộ (cho vùng làm chậm của Đồng Hồ Cát) nhân với
  `SpeedScale` của pool.

### Vùng biên

- Đạn ra ngoài vùng camera + lề → trả về pool. Tính từ camera, không hardcode `9 x 6`
  như hiện tại.

### Pattern

- `BulletPatternSO` giữ nguyên các kiểu Radial / Spiral / Aimed.
- `Aimed` tìm player 1 lần (cache), không `FindGameObjectWithTag` mỗi lần bắn như hiện tại.

### Hình ảnh đạn player

- Sprite phép tím/lavender, lõi trắng và đuôi ngắn, PNG RGBA nền trong suốt.
- Import ở 2560 PPU, tăng kích thước hình 20% so với bản đầu (3072 PPU), không scale collider.
- Hướng gốc của sprite là +Y. `alignToDirection` trên prefab player cho phép xoay
  hình theo hướng bay ngay khi `Launch`, kể cả khi tái sử dụng từ pool.
- Collider tròn bán kính 0.08, sát thương, tốc độ và nhịp bắn giữ nguyên.
- Prefab `PlayerBullet` dùng sprite mới; Setup/Rebuild cập nhật prefab có sẵn.
- Đạn địch không bật tùy chọn xoay này.

## Non-goals

- Không làm đạn cong theo đường bezier / đạn có quỹ đạo phức tạp ở giai đoạn này.
- Không dùng Job System / ECS — chỉ tối ưu bằng pool là đủ cho quy mô hiện tại.

## Open questions

- Có cần giới hạn tổng số đạn địch trên màn (hard cap) để chống lag không?
