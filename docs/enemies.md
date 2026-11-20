# Enemies

> Khớp với `Assets/_Game/Scripts/Enemies/` — namespace `Houkimajo.Enemies`

## Goal

Thêm quái thường xuất hiện theo từng đợt (wave) từ **mọi phía** màn hình, để player
có mục tiêu tiêu diệt trước boss, đồng thời là nguồn chính của **vàng** và **XP**
cho hệ thống RPG (xem [`progression.md`](progression.md)).

## Requirements

### Dữ liệu (data-driven)

- `EnemyDataSO` (Create → Houkimajo → Enemy Data) định nghĩa 1 loại quái:
  - `maxHP`, `moveSpeed`, `contactDamage` (đụng người player = mất 1 mạng như trúng đạn)
  - `movement`: kiểu di chuyển — `Chase` (đuổi theo player), `Orbit` (bay vòng quanh
    player ở bán kính cố định), `Strafe` (bay ngang qua màn hình rồi đi mất)
  - `bulletPattern` (`BulletPatternSO`, có thể để trống = quái không bắn)
  - `reward`: `RewardData { gold, xp }`
  - `sprite`, `colliderRadius`
- `WaveSO` (Create → Houkimajo → Wave) = 1 đợt quái, gồm danh sách
  `WaveEntry { enemyData, count, spawnInterval }` + `startDelay`.
- Thêm loại quái mới = tạo asset `EnemyDataSO` mới, **không sửa code** (trừ khi cần
  kiểu di chuyển mới).

### Spawn & vòng đời

- `EnemySpawner` chạy 1 `WaveSO`: spawn quái tại **vị trí ngẫu nhiên ngay ngoài mép
  màn hình** (cả 4 cạnh), phát event `OnWaveCleared` khi toàn bộ quái của wave đã chết
  (quái `Strafe` bay ra khỏi màn hình cũng tính là xong).
- Quái lấy từ `EnemyPool` — **không `Instantiate`/`Destroy` lúc runtime** (rule cứng,
  giống đạn). 1 prefab quái chung, cấu hình hình dạng/chỉ số bằng `EnemyDataSO` lúc
  lấy ra khỏi pool.
- `EnemyController` (1 component gộp di chuyển + máu + bắn) implement `IDamageable`
  (xem [`bullets.md`](bullets.md)), `ISkillStatusTarget` (chậm, choáng) và
  `IKnockbackTarget` (đẩy lùi) để các skill tác động được lên quái.
- Khi chết: phát event `OnEnemyKilled(EnemyDataSO, position)`, rớt **pickup vàng**
  tại chỗ (pickup cũng đi qua pool), trả quái về pool. XP chỉ khai báo trong data,
  chưa rớt cho tới giai đoạn 3 (chưa có hệ level).
- Tối đa **30 quái** cùng lúc; wave nào đông hơn thì chờ bớt mới spawn tiếp.
- Quái mới spawn có ~0.5s "hiện hình" không bắn, không gây sát thương va chạm — tránh
  quái spawn ngay sát player rồi giết oan.

### Pickup (vàng / XP)

- Rớt ra dạng viên nhỏ, tự bay về player khi player vào **bán kính hút**
  (`magnetRadius`, chỉ số của player — item có thể tăng).
- Tự biến mất sau 10s nếu không nhặt.
- Nhặt → cộng vào `Wallet` / `PlayerLevel` qua event, không tham chiếu thẳng UI.

### Quái mẫu (dùng chung cho 3 màn, màn sau đông hơn)

| Tên | Movement | HP | Bắn | Vàng / XP |
|---|---|---|---|---|
| Dơi Bóng Đêm | Chase | 20 | Aimed 1 viên, thưa | 2 / 3 |
| Đom Đóm Lửa | Orbit | 30 | Aimed, chậm | 3 / 5 |
| Bí Ngô Bay | Strafe | 50 | Radial 8 viên | 5 / 8 |

| Màn | Wave 1 | Wave 2 | Wave 3 |
|---|---|---|---|
| 1 | 8 Dơi | 6 Dơi + 3 Đom Đóm | — |
| 2 | 10 Dơi + 3 Đom Đóm | 4 Bí Ngô + 8 Dơi | 5 Đom Đóm + 4 Bí Ngô |
| 3 | 14 Dơi + 5 Đom Đóm | 6 Bí Ngô + 6 Đom Đóm | 16 Dơi + 6 Bí Ngô + 6 Đom Đóm |

### Animation hình ảnh quái

- Sáu mẫu Dơi, Đom Đóm, Bí Ngô, Ma Trơi, Ma Thư và Sứa Trăng có vòng lặp bay/idle
  bốn frame nền trong suốt; mỗi loại một sheet 2x2 dưới `Art/Sprites/Enemies`.
- Frame phát theo thứ tự trên trái, trên phải, dưới trái, dưới phải. Tốc độ từng
  loại được cấu hình riêng, dùng thời gian gameplay để dừng khi pause.
- Animation chỉ thay sprite trên object hình ảnh; không thay đổi collider, AI,
  phần thưởng hoặc wave. Khi tái sử dụng từ pool, vòng lặp bắt đầu lại từ frame 0.
- Sáu loại đều có đạn riêng. Dơi thêm Aimed1 tốc độ3.2 mỗi2.5s; Ma Trơi bắn
  AimedFan3 góc30 độ, tốc độ2.6 mỗi2.3s; Ma Thư dùng AimedFan3 góc36 độ.
  Đom Đóm, Bí Ngô và Sứa giữ nhịp/tốc độ/số viên hiện có, chỉ gắn art.
- `EnemyDataSO` giữ `animationFrames` và `animationFramesPerSecond`; prefab quái chung
  có `EnemySpriteAnimation`. Mỗi lần spawn cấu hình lại bộ frame, không tạo visual mới.
  Quái không có frame vẫn dùng sprite tĩnh. Animation không ghi đè màu hiện hình.
- Màn 1 có wave mở đầu `Enemy_Showcase` (hai con mỗi loại, spawn cách 0.5s), sau đó
  giữ các wave cũ và boss. Setup lại không chèn trùng wave này.
- Ba loại mới dùng chỉ số thử nghiệm: Ma Trơi Chase, HP25/speed3/gold3/XP4;
  Ma Thư Orbit, HP45/speed1.5/radius3.5/gold5/XP7, bắn chùm3 viên tốc độ3 mỗi1.8s;
  Sứa Trăng Orbit, HP60/speed1/radius3/gold6/XP9, bắn Radial6 viên tốc độ1.8 mỗi2.5s.
  Collider lần lượt 0.2/0.24/0.25. Không có cơ chế vùng làm chậm mới ở bước này.

## Non-goals

- Không làm AI tìm đường / né chướng ngại — màn chơi là 1 arena trống.
- Không làm quái elite / quái có nhiều pha (việc đó là của boss).
- Chưa làm animation chết, hiệu ứng nổ (dùng placeholder như phần còn lại).

## Open questions

- Có giới hạn số quái tối đa cùng lúc trên màn hình không (để không quá tải khi wave
  lớn)? Đề xuất mặc định 30.
- Quái có rớt vật phẩm ngẫu nhiên (đồ trang bị) không, hay đồ chỉ mua ở shop?
  Đề xuất hiện tại: **chỉ mua ở shop**, cho đơn giản.
