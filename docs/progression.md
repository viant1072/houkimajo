# Progression

> Khớp với `Assets/_Game/Scripts/Progression/` — namespace `Houkimajo.Progression`

## Goal

Hệ thống phát triển nhân vật kiểu **RPG, lưu vĩnh viễn**: giết quái nhận XP và vàng,
lên cấp để mở khoá skill và chỉ số, dùng vàng để nâng cấp skill và mua trang bị. Tiến
trình được lưu xuống file, thoát game vào lại vẫn còn.

Nguyên tắc tách bạch: **XP → mở khoá / lựa chọn**, **Vàng → sức mạnh** (nâng cấp skill,
mua đồ). Như vậy không có chuyện tiền mua được thứ chỉ level mới mở.

## Requirements

### Cấp độ (XP)

- `PlayerLevel`: giữ `level`, `xp`. XP cần lên cấp tiếp theo:
  `xpToNext = 50 * level^1.5` (làm tròn) — công thức nằm trong `ProgressionConfigSO`.
  Cấp tối đa: 30.
- Lên cấp **giữa trận** chỉ hiện thông báo "LEVEL UP!" — **không pause game**, vì
  chặn màn hình lúc né đạn sẽ giết người chơi oan. Lựa chọn được dồn lại và xử lý ở
  màn shop.
- Mỗi lần lên cấp nhận 1 **thẻ lựa chọn**: hiện 3 thẻ ngẫu nhiên, chọn 1:
  - Mở khoá 1 skill mới (trong các ô đã mở)
  - Hoặc 1 chỉ số bị động: +5% sát thương, +5% tốc độ bay, -4% hồi chiêu, +10% bán
    kính hút, +10% vàng nhận được...
- Mốc mở ô: E ở Lv3, R ở Lv6 (lên tới mốc → thẻ đầu tiên chắc chắn là chọn skill cho ô đó).

### Vàng

- `Wallet`: giữ `gold`, phát event `OnGoldChanged`. Mọi thao tác cộng/trừ đi qua
  `Wallet`, có kiểm tra đủ tiền (`TrySpend`).
- Nguồn vàng: quái (xem `enemies.md`), boss, đạn bị skill Hoá Giải xoá.

### Trang bị (Items)

- 3 ô trang bị: **Chổi**, **Mũ**, **Bùa** + túi **đồ tiêu hao** (tối đa 3 loại).
- `ItemSO` (Create → Houkimajo → Item): `id`, `displayName`, `icon`, `slot`, `price`,
  `description`, `modifiers[]` — `StatModifier { stat, type (Flat/Percent), value }`.
- Đồ tiêu hao: dùng tự động lúc vào stage (vd. Bình Máu = +1 mạng cho stage này),
  mất sau khi dùng.
- Bán lại đồ = 50% giá mua.

| Ô | Item | Hiệu ứng | Giá |
|---|---|---|---|
| Chổi | Chổi Gỗ Sồi | +10% tốc độ bay | 150 |
| Chổi | Chổi Lông Phượng | -25% hồi chiêu skill E | 400 |
| Mũ | Mũ Phù Thủy Cổ | -15% hồi chiêu tất cả skill | 450 |
| Mũ | Mũ Sao Đêm | +20% sát thương skill | 400 |
| Bùa | Bùa Hộ Mệnh | +1 mạng tối đa | 500 |
| Bùa | Kính Tinh Tú | +50% bán kính graze, +20% vàng nhận | 350 |
| Tiêu hao | Bình Máu | +1 mạng cho stage kế | 80 |
| Tiêu hao | Cuộn Phép | bắt đầu stage với 50 năng lượng ultimate | 120 |

### Chỉ số nhân vật

- `PlayerStats` gộp: chỉ số gốc (`PlayerBaseStatsSO`) + trang bị + thẻ bị động.
  Các chỉ số: `moveSpeed`, `focusSpeed`, `fireRate`, `bulletDamage`, `skillDamage%`,
  `cooldownReduction%`, `maxLives`, `invincibilityDuration`, `grazeRadius`,
  `magnetRadius`, `goldGain%`.
- Tính lại khi trang bị thay đổi, phát `OnStatsChanged` — **không** tính lại mỗi frame.

### Shop

- Mở **sau mỗi wave có cờ `openShopAfter`** và **luôn mở trước boss** (xem `systems.md`).
  Game dừng hẳn (`GameState.Shop`) trong lúc ở shop.
- 3 tab: **Lên cấp** (chọn các thẻ đang chờ), **Skill** (lắp skill vào ô, nâng cấp
  bằng vàng), **Cửa hàng** (mua/bán/lắp đồ). Nút **Tiếp tục** → vào wave/boss tiếp.
- Danh sách đồ bán lấy từ `ShopCatalogSO` (data-driven).

### Lưu game

- `SaveSystem` ghi JSON vào `Application.persistentDataPath/save.json`:
  level, xp, gold, skill đã mở + level từng skill, skill lắp ở Q/E/R, đồ đã sở hữu,
  đồ đang mặc, đồ tiêu hao, chỉ số bị động đã chọn, stage cao nhất đã qua.
- Lưu khi: rời shop, thắng boss, thua (game over). **Không lưu giữa wave.**
- Lưu theo `id` (string) của SO, không theo tên file — đổi tên asset không làm hỏng save.
- Có version trong file save để sau này đổi cấu trúc còn migrate được.
- **Thua**: giữ toàn bộ XP và vàng đã nhặt, chơi lại stage từ wave 1 (kiểu RPG, không
  phạt nặng).

## Non-goals

- Không làm nhiều slot save / nhiều nhân vật.
- Không có độ hiếm (rarity) hay chỉ số ngẫu nhiên trên đồ — mỗi item cố định.
- Không làm chế tạo, ghép đồ, giao dịch.
- Không mã hoá file save (chơi offline, sửa save thì kệ).

## Open questions

- Thua có nên mất một phần vàng (vd. 20%) để có chút áp lực không? Hiện tại: không mất.
- Có cần nút "reset tiến trình" trong menu không?
- Khi có nhiều stage: cho chọn chơi lại stage cũ để cày vàng/XP không?
