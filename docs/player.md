# Player

> Khớp với `Assets/_Game/Scripts/Player/` — namespace `Houkimajo.Player`

## Goal

Điều khiển cô phù thủy cưỡi chổi trong một **arena 1 màn hình**, di chuyển bằng bàn
phím, **ngắm bằng chuột** để bắn quái tới từ mọi phía, né đạn dày đặc. Cảm giác điều
khiển phải chính xác và "đọc" được: lúc nào an toàn, hitbox ở đâu.

## Requirements

### Điều khiển

| Phím | Chức năng |
|---|---|
| WASD / mũi tên | Di chuyển 8 hướng |
| Chuột | Ngắm — đạn thường và skill có hướng bay về phía con trỏ |
| **Space** (giữ) | Bắn liên tục |
| Shift (giữ) | Focus mode: bay chậm + hiện hitbox |
| Q / E / R | Skill (xem [`skills.md`](skills.md)) |

- Đọc input trực tiếp qua `Keyboard.current` / `Mouse.current` (giữ cách làm cũ, chưa
  dùng Input Actions asset).
- Vị trí chuột đổi sang world space bằng `Camera.ScreenToWorldPoint`, cache camera
  trong `Awake()`.
- Ẩn con trỏ hệ thống, thay bằng **tâm ngắm** (sprite) đi theo chuột.

### Di chuyển & arena

- Giới hạn di chuyển = toàn bộ vùng camera nhìn thấy trừ lề 0.3 đơn vị (không còn là cột
  dọc hẹp như Touhou). Tính từ camera lúc khởi tạo, không hardcode.
- Tốc độ lấy từ `PlayerStats` (`moveSpeed` thường, `focusSpeed` khi giữ Shift).

### Bắn

- `BasicAttackSO` cấu hình id/tên/icon/thuộc tính hiển thị/sprite đạn/tint và các cấp
  (damage, fire interval, speed). Profile mặc định Lv1 giữ 10 damage, 0.1s, 12 unit/s.
- `PlayerShooting.EquipAttack(profile, level)` và `SetAttackLevel(level)` phục vụ nâng
  cấp/đổi bắn thường về sau; HUD nghe event đổi profile/cấp và đọc nhịp hồi thực tế.
- Giữ các field cũ làm fallback cho scene chưa gán profile. Pause không bắn hoặc tick
  cooldown. Thay sprite/tint của viên đạn từ profile không scale hitbox.

- `PlayerShooting`: giữ Space → bắn theo `fireRate`, hướng = (chuột − vị trí player).
- Sát thương đạn lấy từ `PlayerStats.bulletDamage`. Đạn vẫn đi qua `BulletPool`.
- Sprite nhân vật (hoặc cây đũa) xoay/lật theo hướng chuột để người chơi biết đang ngắm đâu.

### Máu & bất tử

- Mạng (lives) tối đa từ `PlayerStats.maxLives` (gốc 3), hồi đầy khi bắt đầu 1 stage.
- Trúng đạn / đụng quái → mất 1 mạng → bất tử `invincibilityDuration` giây (gốc 2s).
- **Nhấp nháy** sprite trong lúc bất tử (hiện tại có logic bất tử nhưng không có
  phản hồi hình ảnh).
- Hitbox thật là 1 chấm nhỏ ở giữa người — hiện ra khi giữ Shift.

### Graze

- `GrazeDetector`: collider trigger lớn hơn hitbox (bán kính `PlayerStats.grazeRadius`).
  Đạn địch đi vào vùng này mà không trúng → tính 1 lần graze/viên, phát event
  `OnGraze` (dùng để tích năng lượng ultimate, cộng điểm).

### Animation bay (Stage01)

- Dùng sheet PNG RGBA 2×2 gồm 4 frame, thứ tự trái→phải, trên→dưới.
- `PlayerFlightAnimation` trên object con `Visual` phát vòng lặp 6 frame/giây;
  tóc/áo choàng đổi frame, hình nhún 0.025 đơn vị và nghiêng tối đa 1.5 độ.
- Chỉ dịch chuyển/xoay object hình ảnh: root, collider, điểm bắn và hitbox không bị
  animation tác động. Focus giảm nhún/nghiêng về 0 để hỗ trợ né chính xác.
- Dùng delta time có scale: pause/victory/game over dừng cả đổi frame và nhún.
- `PlayerVisuals` vẫn phụ trách lật trái/phải và nhấp nháy bất tử, tham chiếu renderer
  trên `Visual`. Không animate `enabled`, `flipX` hay màu của renderer.
- Setup/Rebuild scene dùng sheet nếu có đủ 4 frame, còn thiếu thì giữ placeholder.
- Scene Stage01 hiện có được gắn `Visual` và animation, giữ các thiết lập gameplay.

## Non-goals

- Không hỗ trợ tay cầm (gamepad) ở giai đoạn này.
- Chưa làm animation bắn, chết hay lướt riêng; vòng bay được dùng cả khi di chuyển.
- Không cho bắn tự động (auto-aim) — ngắm là kỹ năng của người chơi.

## Open questions

- Giữ hay bỏ focus mode (Shift)? Hiện tại: **giữ**, vì vẫn cần né đạn chính xác.
- Có cần tuỳ chọn "bắn bằng chuột trái" thay cho Space không?
