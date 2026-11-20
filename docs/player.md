# Player

> Khớp với `Assets/_Game/Scripts/Player/` — namespace `Houkimajo.Player`

## Goal

Điều khiển cô phù thủy cưỡi chổi trong một **arena 1 màn hình**, di chuyển bằng bàn
phím, **ngắm bằng chuột** để bắn quái tới từ mọi phía, né đạn dày đặc. Cảm giác điều
khiển phải chính xác và "đọc" được: lúc nào an toàn, hitbox ở đâu.

## Requirements

### Điều khiển mới

- Giữ chuột trái để bắn; Space không bắn. Nhấn Left Shift lướt mặc định, giữ Left
  Control bay chậm/hiện hitbox; Right Shift/Right Control không thay các phím này.
- Lướt tách khỏi Q/E/R: không tốn MP, không chung global cooldown; giữ khoảng cách,
  thời gian, bất tử và hồi lượt từ cấu hình Lướt Chổi có sẵn. Hướng ưu tiên phím
  di chuyển, đứng yên thì theo chuột. Giữ Shift không tự lặp lướt.
- Cấp lướt đã mua trong save cũ được áp dụng cho động tác mặc định. Lướt không còn
  được lắp/nâng cấp ở ô E; E cũ là Lướt Chổi chuyển sang Khiên Phép cấp 1 (hoặc cấp
  Khiên Phép đã mua). F2 chỉ đổi bốn kỹ năng E còn lại.
- Shop/pause/hết HP chặn di chuyển/lướt/bắn và dừng hồi lướt. Không bắn khi con trỏ
  ở HUD/UI; giữ chuột qua lúc đóng shop phải thả rồi nhấn lại, tránh click xuyên UI.

### HP/MP (thay cơ chế mạng)

- Player có HP tối đa 100, MP tối đa 100, đầy khi bắt đầu stage. Không còn trừ mạng.
- Mỗi lần trúng đạn/chạm quái mặc định mất 20 HP; giữ bất tử sau hit 2 giây,
  khiên và bất tử do skill. HP về 0 chỉ phát sự kiện chết một lần.
- Q mặc định tốn 15 MP, E tốn 20 MP; chi phí chỉnh trên từng SkillSO trong Inspector.
  MP hồi liên tục 5/giây trong gameplay, dừng khi pause/game over. Bắn thường miễn phí.
  R vẫn dùng năng lượng ultimate 100, không trừ MP. Thiếu MP không trừ lượt/hồi chiêu.
- HUD nghe event HP/MP và hiển thị số hiện tại/tối đa; không dùng số giả từ ảnh mẫu.

### Điều khiển

| Phím | Chức năng |
|---|---|
| WASD / mũi tên | Di chuyển 8 hướng |
| Chuột | Ngắm — đạn thường và skill có hướng bay về phía con trỏ |
| **Chuột trái** (giữ) | Bắn liên tục |
| **Left Shift** (nhấn) | Lướt mặc định |
| **Left Control** (giữ) | Bay chậm + hiện hitbox |
| Q / E / R | Skill (xem [`skills.md`](skills.md)) |

- Đọc input trực tiếp qua `Keyboard.current` / `Mouse.current` (giữ cách làm cũ, chưa
  dùng Input Actions asset).
- Vị trí chuột đổi sang world space bằng `Camera.ScreenToWorldPoint`, cache camera
  trong `Awake()`.
- Ẩn con trỏ hệ thống, thay bằng **tâm ngắm** (sprite) đi theo chuột.

### Di chuyển & arena

- Giới hạn di chuyển = toàn bộ vùng camera nhìn thấy trừ lề 0.3 đơn vị (không còn là cột
  dọc hẹp như Touhou). Tính từ camera lúc khởi tạo, không hardcode.
- Tốc độ lấy từ `PlayerStats` (`moveSpeed` thường, `focusSpeed` khi giữ Left Control).

### Bắn

- `BasicAttackSO` cấu hình id/tên/icon/thuộc tính hiển thị/sprite đạn/tint và các cấp
  (damage, fire interval, speed). Profile mặc định Lv1 giữ 10 damage, 0.1s, 12 unit/s.
- `PlayerShooting.EquipAttack(profile, level)` và `SetAttackLevel(level)` phục vụ nâng
  cấp/đổi bắn thường về sau; HUD nghe event đổi profile/cấp và đọc nhịp hồi thực tế.
- Giữ các field cũ làm fallback cho scene chưa gán profile. Pause không bắn hoặc tick
  cooldown. Thay sprite/tint của viên đạn từ profile không scale hitbox.

- `PlayerShooting`: giữ chuột trái → bắn theo `fireRate`, hướng = (chuột − vị trí player).
- Sát thương đạn lấy từ `PlayerStats.bulletDamage`. Đạn vẫn đi qua `BulletPool`.
- Sprite nhân vật (hoặc cây đũa) xoay/lật theo hướng chuột để người chơi biết đang ngắm đâu.

### Máu & bất tử

- HP tối đa hiện cấu hình trên `PlayerHealth` (gốc 100), hồi đầy khi bắt đầu stage.
- Trúng đạn / đụng quái → mất 20 HP mặc định → bất tử `invincibilityDuration` giây (gốc 2s).
- **Nhấp nháy** sprite trong lúc bất tử (hiện tại có logic bất tử nhưng không có
  phản hồi hình ảnh).
- Hitbox thật là 1 chấm nhỏ ở giữa người — hiện ra khi giữ Left Control.

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

### Tạo hình lúc bắn thường

- Visual có thêm bốn frame `player_witch_cast_v1`: giơ tay và duỗi lòng bàn tay
  khi bắn. PlayerFlightAnimation dùng frame cast khi nhận NotifyShot từ phát bắn
  thành công, giữ pose0.18s sau phát cuối, rồi trở về bộ frame bay cũ.
- Giữ chuột bắn liên tục chỉ gia hạn thời gian giữ pose, không reset động tác mỗi
  viên. Động tác giơ tay phát một lần rồi giữ frame cuối đến khi ngừng bắn.
  Animation dùng thời gian gameplay, dừng trong pause/thoại/shop.
- Quả cầu phép tím riêng nằm phía trước hướng ngắm, cách root0.5 world units.
  Đạn và muzzle flash xuất phát từ tâm cầu. Cầu hiện/pulse trong lúc bắn, tắt khi
  hết hold, không có collider; được tạo sẵn trong scene, không Instantiate lúc bắn.
- Root/hitbox/tốc độ/damage và dáng bay cũ được giữ. Hướng giơ tay lật trái/phải
  theo sprite hiện có; quả cầu và đạn vẫn ngắm đủ360 độ. Skill khóa bắn/UI chặn click
  không duy trì cast nếu không có phát bắn thật.

## Non-goals

- Không hỗ trợ tay cầm (gamepad) ở giai đoạn này.
- Chưa làm animation bắn, chết hay lướt riêng; vòng bay được dùng cả khi di chuyển.
- Không cho bắn tự động (auto-aim) — ngắm là kỹ năng của người chơi.

## Open questions

- Giữ hay bỏ focus mode (Left Control)? Hiện tại: **giữ**, vì vẫn cần né đạn chính xác.
- Chuột trái đã là nút bắn mặc định; tùy chỉnh keybind để giai đoạn sau.
