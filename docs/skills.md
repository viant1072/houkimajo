# Skills

> Khớp với `Assets/_Game/Scripts/Skills/` — namespace `Houkimajo.Skills`

## Goal

Cho cô phù thủy 3 ô skill **Q / E / R**, mỗi ô có một "kho" skill riêng để chọn. Người
chơi mở khoá skill khi lên cấp, đổi skill giữa các skill đã mở ở màn shop, và nâng
cấp skill bằng vàng. Mục tiêu: mỗi người có một bộ skill khác nhau, tạo chiều sâu
build kiểu RPG mà vẫn giữ chất né đạn của bullet-hell.

## Requirements

### 1. Nguyên tắc thiết kế

1. **Mỗi ô một vai trò rõ ràng.** Q = gây sát thương, E = sống sót, R = lật ngược
   tình thế. Không có skill Q nào cứu mạng tốt hơn skill E, để lựa chọn có ý nghĩa.
2. **Đọc được giữa mưa đạn.** Mỗi skill có màu riêng, hiệu ứng rõ ràng, không che hitbox.
   Skill không bao giờ làm người chơi mất quyền điều khiển quá 0.2s.
3. **Ngắm là kỹ năng.** Skill có hướng thì đi theo chuột. Skill tự tìm mục tiêu
   (Tên Lửa, Sét Lan) bù lại bằng sát thương mỗi phát thấp hơn.
4. **Có cái giá.** Skill mạnh phải có điểm yếu: Tia Sao Băng làm player bay chậm,
   Cầu Lửa phải ngắm đúng chỗ, Đồng Hồ Cát không xoá đạn mà chỉ làm chậm.
5. **Nâng cấp cảm nhận được.** Mỗi cấp tăng ít nhất 1 con số nhìn thấy được (thêm đạn,
   vùng to hơn), không chỉ +5% sát thương. Lv5 có thêm một **hiệu ứng đặc biệt**.

### 2. Ngân sách sức mạnh (để cân bằng)

Mốc so sánh: **đạn thường = 10 sát thương × 10 viên/giây = 100 DPS**.

| Ô | Sát thương 1 lần dùng (Lv1) | Quy đổi | Hồi chiêu |
|---|---|---|---|
| Q | 60–110 | ≈ 0.6–1.1 giây bắn thường | 6–8s → còn 4.5–6s ở Lv5 |
| E | gần 0 (chỉ phòng thủ) | — | 6–14s → còn 3.5–10s ở Lv5 |
| R | 400–1000 hoặc phòng thủ mạnh | ≈ 4–10 giây bắn thường | tích năng lượng |

- Q dùng đều tay sẽ cộng thêm khoảng **15–20% DPS**: đáng bấm nhưng không thay thế
  được việc bắn thường.
- Mọi con số dưới đây là **giá trị khởi điểm**, nằm trong asset nên chỉnh được mà
  không phải sửa code.

### 3. Luật chung

- **Hồi chiêu** tính từ lúc bấm. Bấm khi chưa hồi xong → không có gì xảy ra, ô skill
  trên HUD nháy đỏ.
- **Global cooldown 0.15s** giữa 2 skill bất kỳ, tránh bấm Q+E+R cùng 1 frame làm
  hiệu ứng chồng lên nhau khó đọc.
- Dùng skill **không ngắt bắn thường** (vẫn giữ Space được), trừ Tia Sao Băng.
- Game dừng (shop, pause, game over) → hồi chiêu và hiệu ứng đứng yên theo
  (dùng `Time.deltaTime` có scale).
- Vào stage mới: mọi hồi chiêu về 0, năng lượng ultimate về 0 (trừ khi có Cuộn Phép).
- **Boss miễn nhiễm làm chậm.** Hiệu ứng làm chậm chỉ tác động lên quái thường.

### 4. Năng lượng ultimate (ô R)

| Nguồn | Năng lượng |
|---|---|
| Gây sát thương (đạn thường + skill) | +0.05 mỗi 1 sát thương |
| Graze 1 viên đạn | +1 |
| Giết 1 quái thường (khi có giai đoạn 2) | +2 |

- Đầy **100** mới dùng được R. Chỉ bắn thường thì mất khoảng 20 giây để đầy,
  graze nhiều thì nhanh hơn.
- Sát thương gây ra **bởi chính ultimate** không tích năng lượng, tránh vòng lặp
  dùng R → đầy R lại ngay.

### 5. Chi tiết từng skill

Ký hiệu `a → b` nghĩa là giá trị ở Lv1 → Lv5, các cấp giữa tăng đều.

#### Q – Tấn công (màu cam/đỏ/vàng)

**🔮 Tên Lửa Ma Thuật** *(tặng sẵn ở Lv1)*
- Bắn ra 1 chùm tên lửa tím, mỗi quả tự đuổi theo địch gần **con trỏ** nhất.
  Mục tiêu chết giữa đường thì tự chuyển sang mục tiêu khác.
- Số quả **5 → 9** · sát thương mỗi quả **15 → 27** · hồi chiêu **7 → 5s**
- Bay 8 đơn vị/giây, rẽ tối đa 360°/giây, tự huỷ sau 3 giây.
- **Lv5:** quả nào trúng mục tiêu sẽ nổ nhỏ (bán kính 0.5) ra xung quanh.
- Vai trò: skill khởi đầu dễ dùng, không cần ngắm.

**🔥 Cầu Lửa**
- 1 quả cầu lửa bay về phía chuột, **nổ khi chạm địch hoặc khi tới đúng vị trí con trỏ**.
  Nhờ vậy người chơi chủ động đặt vụ nổ ở đâu cũng được.
- Sát thương nổ **60 → 120** · bán kính **1.5 → 2.3** · hồi chiêu **8 → 6s**
- **Lv5:** để lại vùng lửa 2 giây, gây 20 sát thương/giây.
- Điểm yếu: phải ngắm, đặt sai chỗ là phí cả lượt.

**⚡ Sét Lan**
- Sét đánh tức thì vào địch gần con trỏ nhất (trong bán kính 4), rồi nhảy sang địch
  gần nhất kế tiếp (tầm nhảy 3). Mỗi địch chỉ bị đánh 1 lần mỗi lượt.
- Số lần nhảy **3 → 7** · sát thương mỗi lần **35 → 55** · hồi chiêu **6 → 4.5s**
- **Lv5:** địch bị sét đánh choáng 0.5 giây (boss không bị choáng).
- Mạnh khi đông quái, yếu khi chỉ có boss (chỉ đánh được 1 lần).

**❄️ Mưa Băng Tiễn**
- Quạt mũi băng xoè 60° về phía chuột, **xuyên qua** tối đa 3 địch.
- Số mũi **7 → 11** · sát thương mỗi mũi **12 → 20** · làm chậm 40% trong **2 → 4s** ·
  hồi chiêu **7 → 5s**
- **Lv5:** mũi băng trúng địch đang bị chậm thì gây gấp đôi sát thương.
- Đứng gần dồn cả quạt vào 1 mục tiêu thì sát thương rất cao. Đây là skill thưởng
  cho lối chơi liều.

#### E – Phòng thủ / di chuyển (màu xanh dương/trắng)

**🧹 Lướt Chổi**
- Lao nhanh **0.18 giây**, **bất tử** trong lúc lướt và thêm 0.1 giây sau đó.
- Hướng lướt: **theo phím di chuyển đang giữ**, không giữ phím nào thì lướt theo
  hướng chuột. *(Đổi so với bản trước, vốn chỉ lướt theo chuột: lướt theo hướng
  đang chạy tự nhiên hơn khi vừa né vừa ngắm ngược lại.)*
- Khoảng cách **3 → 4.5** · hồi chiêu **6 → 3.5s**
- **Lv5:** có **2 lượt lướt**, hồi từng lượt một.

**🛡️ Khiên Phép**
- Bật vòng khiên quanh người, đỡ được 1 số lần trúng đạn. Mỗi lần đỡ thì viên đạn
  biến mất và player được bất tử 0.5 giây.
- Số lần đỡ **1 → 3** · thời gian tồn tại **5 → 7s** · hồi chiêu **12 → 8s**
- **Lv5:** khiên vỡ thì nổ ra xoá đạn trong bán kính 1.5.
- Dễ dùng nhất trong kho E, hợp với người mới.

**💨 Hoá Giải**
- Xoá sạch đạn địch trong vùng tròn quanh người. Mỗi viên bị xoá đổi thành 1 xu
  (tối đa 30 xu mỗi lần dùng).
- Bán kính **2 → 3** · hồi chiêu **12 → 8s**
- **Lv5:** đẩy lùi quái thường trong vùng ra xa 2 đơn vị.
- Vừa cứu mạng vừa kiếm tiền, nhưng hồi chiêu dài.

**⏳ Đồng Hồ Cát**
- Tạo vùng tròn **đi theo player**: đạn địch bay vào vùng chậm đi 50%.
- Bán kính **2.5 → 3.5** · thời gian **3 → 5s** · hồi chiêu **14 → 10s**
- **Lv5:** đạn trong vùng chậm 70%, quái trong vùng chậm 30%.
- Không xoá đạn, chỉ giúp né dễ hơn. Bù lại thời gian tác dụng dài nhất.

**✨ Dịch Chuyển**
- Teleport tức thì tới vị trí con trỏ (nếu xa quá thì dừng ở tầm tối đa, luôn nằm
  trong arena). Bất tử 0.3 giây khi tới nơi.
- Tầm xa **4 → 6** · hồi chiêu **8 → 5s**
- **Lv5:** chỗ vừa rời đi nổ ra, gây 50 sát thương bán kính 1.
- Thoát hiểm xa nhất nhưng phải ngắm chỗ an toàn. Teleport vào giữa đạn là chết.

#### R – Ultimate (màu vàng kim/tím)

**☄️ Mưa Thiên Thạch**
- Thiên thạch rơi liên tục. 50% rơi trúng một địch ngẫu nhiên, 50% rơi ngẫu nhiên
  khắp arena. Mỗi quả có vòng đỏ cảnh báo 0.4 giây trước khi rơi, rơi xuống thì gây
  sát thương và xoá đạn trong vùng nổ.
- Thời gian **5 → 7s** · số quả mỗi giây **4 → 8** · sát thương **80 → 120** · bán kính 1
- **Lv5:** thiên thạch để lại vùng lửa nhỏ 1 giây.

**🐈‍⬛ Triệu Hồi Linh Thú**
- Gọi mèo đen bay vòng quanh player, tự bắn về phía chuột 5 phát/giây, mỗi phát
  8 sát thương. Linh thú không bị đạn địch làm hại.
- Số mèo **2 → 4** · thời gian **10 → 15s**
- **Lv5:** mèo còn tự chặn đạn địch chạm vào nó (mỗi con chặn tối đa 5 viên).
- Ultimate kéo dài lâu nhất, hợp lối chơi đánh lâu dài với boss.

**🌟 Tia Sao Băng**
- Phóng tia laser lớn theo hướng chuột (có thể xoay theo chuột trong lúc bắn).
  Gây 10 lần sát thương mỗi giây, **xoá mọi đạn địch** trên đường tia đi qua.
- Thời gian **2 → 3s** · độ rộng **0.8 → 1.4** · sát thương mỗi lần **30 → 45**
  (300 → 450 DPS)
- **Cái giá:** trong lúc bắn, player bay chậm 50% và không bắn thường được.
- **Lv5:** kết thúc bằng 1 vụ nổ ở đầu tia, gây 200 sát thương.
- Gây sát thương lên boss nhanh nhất.

**⏸️ Ngưng Đọng**
- Đóng băng **toàn bộ đạn địch** trên màn và **tạm dừng mọi nguồn bắn** (boss, quái).
  Player vẫn di chuyển và bắn bình thường. Hết giờ thì đạn bay tiếp.
- Thời gian **4 → 6s**
- **Lv5:** lúc hết giờ, đạn trong bán kính 3 quanh player tự biến mất.
- Ultimate phòng thủ: dùng để thoát pha đạn khó nhất của boss, hoặc để tranh thủ
  dồn sát thương.

### Asset mỹ thuật Q (bộ đầu tiên)

- Thư mục: `Assets/_Game/Art/Sprites/Skills/Q/`, mỗi skill một PNG RGBA 2×2.
- Thứ tự ô trái→phải, trên→dưới: `Icon`, `Projectile` (Sét Lan là `Arc`),
  `Impact`, `Mastery`. Unity import Multiple, 4 sub-sprite có tên riêng.
- Tên Lửa: tím/lavender, icon chùm tên lửa; projectile là phi tiêu phép lõi trắng;
  impact tia sao; mastery là nổ tròn nhỏ (Lv5, radius 0.5).
- Cầu Lửa: cam/đỏ/vàng, icon cầu lửa; projectile cầu lửa đuôi ngắn; impact vụ nổ;
  mastery vòng lửa có giữa thoáng (vùng lửa 2 giây ở Lv5).
- Sét Lan: vàng/trắng, icon sét nối 3 điểm; Arc là đoạn sét hướng +Y;
  impact tia điện; mastery vòng sao điện tượng trưng choáng 0.5 giây (không áp cho boss).
- Mưa Băng Tiễn: cyan/trắng, icon quạt 7 mũi; projectile tinh thể băng hướng +Y;
  impact mảnh băng; mastery vụ vỡ tinh thể để nhấn mạnh sát thương lên địch đang chậm.
- Tất cả phần đạn/tia hướng gốc +Y; pivot giữa ô; mỗi ô tương ứng 1×1 đơn vị world
  trước khi scale. Icon hiển thị qua UI; kích thước sprite không định nghĩa hitbox.
- Asset hiệu ứng ở bộ này là hình tĩnh để scale/fade bằng hệ thống VFX sau này,
  chưa phải sprite sheet animation. Tái sử dụng asset cho Lv1–5 bằng số lượng/kích thước.
- Giữ lõi sáng dễ đọc và glow gọn, không thêm flash phủ màn; vùng lửa/vụ nổ có thể
  giảm alpha để không che đạn địch. Hiệu ứng gameplay phải đi qua pool khi triển khai.
- Tích hợp: 4 Q có icon HUD, projectile/tia và VFX trúng/Lv5; F1 đổi Q, F5 tăng cấp.
- Prewarm cả thư viện debug và VFX; hết pool thì bỏ hiệu ứng/đạn, không tạo object giữa trận.
- Cầu Lửa nổ 1 lần khi chạm hoặc tới con trỏ; Lv5 vùng lửa 2s, 20 DPS, tick 0.25s.
- Sét chọn trước danh sách mục tiêu khác nhau để registry đổi khi địch chết không làm trúng lặp.
- Băng ghi nhớ 3 mục tiêu, không trúng lặp nhiều collider. `ISkillStatusTarget` dành cho
  quái thường nhận slow/stun; boss không implement nên miễn nhiễm. Stage01 chưa có quái
  thường nên chỉ kiểm tra được sát thương và hình ảnh trên boss.

### Asset mỹ thuật E / R (bộ đầu tiên)

- E: `Assets/_Game/Art/Sprites/Skills/E/`, mỗi skill một sheet RGBA 2×2, 4 phần:
  - Lướt Chổi: Icon, Trail, CastSpark, MasteryEcho (minh họa hai lượt, không thêm damage).
  - Khiên Phép: Icon, ShieldRing, Hit, MasteryBreak (vỡ/xóa đạn radius 1.5).
  - Hoá Giải: Icon, ClearRing, CoinSpark, MasteryPush (đẩy quái, không đẩy boss).
  - Đồng Hồ Cát: Icon, SlowField, TimeMote, MasteryField (làm chậm, không đóng băng).
  - Dịch Chuyển: Icon, Departure, Arrival, MasteryExplosion (nổ chỗ rời đi).
- R: `Assets/_Game/Art/Sprites/Skills/R/`, mỗi skill một sheet RGBA 3 cột × 2 hàng:
  - Mưa Thiên Thạch: Icon, Projectile, WarningRing, Impact, MasteryBurnZone, Ember.
  - Triệu Hồi Linh Thú: Icon, Familiar, Projectile, SummonRing, Impact, MasteryBlock.
  - Tia Sao Băng: Icon, BeamBody, BeamStart, BeamEnd, Impact, MasteryExplosion.
  - Ngưng Đọng: Icon, TimeSeal, FrozenBulletOverlay, CastRipple, ResumeBurst, MasteryClearRing.
- Thứ tự ô trái→phải, trên→dưới. Metadata tách sẵn sub-sprite; nền trong suốt, lề
  giữa các ô, icon đọc được ở 42–64 px. Mỗi ô vuông tương ứng 1×1 world unit trước scale.
- E dùng xanh/trắng, R dùng vàng/tím; WarningRing thiên thạch dùng đỏ theo spec.
  Mèo đen có viền sáng để đọc được trên arena tối. Đạn/tia/đuôi hướng gốc +Y.
- Vùng khiên/làm chậm/vòng cảnh báo để trống giữa để không che player, hitbox và đạn.
  Đồng Hồ Cát phân biệt Ngưng Đọng bằng hạt cát/mote, không dùng biểu tượng băng.
- Đây là sprite tĩnh để scale/fade/ghép hiệu ứng, không phải các frame animation.
  Runtime E/R dùng `SkillEffectController` trên Player, giữ trạng thái ở instance;
  các SkillSO chỉ giữ cấu hình. F2 đổi 5 E, F3 đổi 4 R, F4 nạp R, F5 tăng cấp.
- R cần 100 năng lượng: damage thường/skill +0.05 mỗi damage, graze +1; nguồn Ultimate
  (kể cả vùng lửa thiên thạch) không tự tích R. Đổi R giữ năng lượng, không nạp lại.
- Shield chặn hit + bất tử 0.5s; Hoá Giải cộng tối đa 30 vàng/lần vào Wallet của stage.
  Khi có progression sẽ nối Wallet này vào save/shop, chưa thêm shop ở bước này.
- Slow trường áp mỗi frame và tự hết khi ra vùng; Time Stop giữ riêng trạng thái freeze,
  không ghi đè SpeedScale. Tất cả emitter dùng enemy pool tạm dừng cùng freeze.
- Meteor báo 0.4s rồi rơi 0.18s; vùng lửa Lv5 dùng mặc định 20 DPS (tái dùng nhịp của
  Cầu Lửa, cấu hình được). Beam tick 10Hz, khoá bắn thường và giảm tốc 50%, luôn trả
  trạng thái khi hết/huỷ. Mèo prewarm 4 con và pool đạn, tối đa 5 lần chặn/con ở Lv5.
- Các trạng thái ảnh hưởng quái thường đi qua ISkillStatusTarget/IKnockbackTarget;
  Stage01 hiện chỉ có boss nên không thấy slow/stun/knockback trên boss.

### 6. Mở khoá & nâng cấp

- **Ô Q** mở từ đầu, tặng sẵn Tên Lửa Ma Thuật. **Ô E** mở ở **Lv3**, **ô R** ở **Lv6**.
  Tới mốc thì thẻ lên cấp đầu tiên chắc chắn là chọn skill cho ô vừa mở (xem
  [`progression.md`](progression.md)).
- Các skill khác trong kho được mở dần qua thẻ lên cấp.
- Giá nâng cấp (vàng): Lv2 **100** · Lv3 **250** · Lv4 **500** · Lv5 **1000**.
  Skill ultimate đắt gấp 1.5 lần.
- Skill đã nâng cấp giữ nguyên cấp khi tháo ra lắp lại, nên đổi build không bị phạt.

### 7. Gợi ý build (để kiểm tra các skill có hợp nhau không)

| Build | Q | E | R | Lối chơi |
|---|---|---|---|---|
| Pháo Đài | Mưa Băng Tiễn | Khiên Phép | Ngưng Đọng | an toàn, hợp người mới |
| Sát Thủ Lướt | Sét Lan | Lướt Chổi | Tia Sao Băng | cơ động, dồn sát thương lên boss |
| Phù Thuỷ Buôn | Tên Lửa Ma Thuật | Hoá Giải | Mưa Thiên Thạch | dọn quái, cày vàng |
| Nhà Chiêm Tinh | Cầu Lửa | Dịch Chuyển | Triệu Hồi Linh Thú | ngắm chuẩn, đánh lâu dài |

### 8. Kiến trúc code

```
Scripts/Skills/
├── SkillSlot.cs              # enum Q, E, R
├── SkillLevelData.cs         # [Serializable] chỉ số 1 cấp (xem dưới)
├── SkillSO.cs                # abstract ScriptableObject
├── SkillContext.cs           # dữ liệu truyền vào lúc dùng skill
├── SkillCaster.cs            # trên Player: input Q/E/R, hồi chiêu, năng lượng R, event
├── SkillLoadout.cs           # skill đang lắp + cấp; tạm cấu hình qua Inspector
├── Definitions/              # 13 subclass SkillSO, mỗi hành vi 1 file
│   └── HomingMissileSkillSO.cs, FireballSkillSO.cs, DashSkillSO.cs...
└── Runtime/                  # component chạy hiệu ứng trong scene
    └── SkillProjectile.cs, ProjectilePool.cs, SlowField.cs, ShieldEffect.cs,
        Familiar.cs, StarBeam.cs, LightningArc.cs...
```

- `SkillSO`: `id` (string, dùng cho save), `displayName`, `icon`, `slot`, `color`,
  `description`, `levels[5]`, cùng hàm `abstract void Activate(SkillContext ctx, int level)`.
- `SkillLevelData`: `cooldown`, `damage`, `count`, `radius`, `duration`, `range`,
  `upgradeCost`. Field nào skill không dùng thì bỏ qua. Hiệu ứng Lv5 bật bằng
  `bool hasMasteryEffect` ở level cuối.
- **SkillSO không được giữ trạng thái lúc chạy.** Asset dùng chung và giữ nguyên giá
  trị giữa các lần Play. Mọi trạng thái (tia đang bắn, khiên còn mấy lần đỡ...) nằm ở
  component trong `Runtime/` do `Activate()` bật lên hoặc lấy ra từ pool.
- `SkillContext`: transform của player, `AimPoint`/`AimDirection` (từ `PlayerAim`),
  pool đạn địch (để xoá/đóng băng), các pool projectile, `PlayerHealth`/`PlayerController`
  (để bất tử, lướt), cùng hệ số từ trang bị (`damageMultiplier`, `cooldownMultiplier`).
  Trước khi có `PlayerStats` ở giai đoạn 3 thì hệ số mặc định là 1.
- `SkillCaster` phát event `OnCooldownChanged(slot, remaining, total)`,
  `OnUltChargeChanged(value)`, `OnSkillCast(slot)` cho HUD. Riêng hồi chiêu thì HUD
  đọc trực tiếp mỗi frame qua property, tránh bắn event mỗi frame.
- **Danh sách mục tiêu:** thêm `TargetRegistry` (static, trong `Bullets` cạnh
  `IDamageable`). Boss và quái tự đăng ký trong `OnEnable`, huỷ trong `OnDisable`.
  Skill tự tìm mục tiêu duyệt danh sách này, không `FindObjectsOfType`.
- **Projectile skill đi qua pool** (rule cứng). `SkillProjectile` là 1 component
  cấu hình được: tự đuổi mục tiêu, xuyên thấu N địch, nổ khi chạm hoặc khi tới điểm
  đích. Mỗi loại prefab có 1 pool riêng.
- Sát thương của skill phát qua cùng event với đạn thường (`OnPlayerDamageDealt`),
  kèm cờ `fromUltimate` để không tích năng lượng R.

**Phần sửa ở module khác:**

| Module | Thay đổi |
|---|---|
| `PlayerHealth` | thêm `GrantInvincibility(seconds)`, `AddShield(hits, duration)` |
| `PlayerController` | thêm `Dash(direction, distance, duration)`, `SpeedMultiplier` (Tia Sao Băng), hàm teleport kẹp trong arena |
| `PlayerShooting` | thêm cờ `CanFire` (Tia Sao Băng tắt bắn thường) |
| `BulletPool` | đã có `ClearInRadius`, `SpeedScale`; thêm `ClearAlongLine` |
| `BulletEmitter` | thêm `IsPaused` (Ngưng Đọng dừng nguồn bắn) |
| `BulletBase` | event sát thương thêm thông tin nguồn (đạn thường / skill / ultimate) |
| `UI` | thanh skill Q/E/R: icon, lớp phủ hồi chiêu, % năng lượng R |

### 9. Thứ tự triển khai

Mỗi bước xong đều chơi thử được. Mỗi bước cũng cập nhật `SceneBootstrapper` để tạo
sẵn asset skill và gắn `SkillCaster`.

| Bước | Nội dung | Xong khi |
|---|---|---|
| **4a** | Khung skill: `SkillSO`, `SkillCaster`, `SkillLoadout`, `TargetRegistry`, pool projectile; thanh skill dạng chữ trên HUD; **Tên Lửa Ma Thuật** + **Lướt Chổi** | bấm Q tên lửa đuổi theo boss, bấm E lướt qua đạn không mất mạng, HUD hiện hồi chiêu |
| **4b** | Năng lượng R + **Tia Sao Băng** + **Mưa Thiên Thạch** | bắn và graze thì R đầy dần, bấm R ra được 2 ultimate |
| **4c** | Q còn lại: **Cầu Lửa**, **Sét Lan**, **Mưa Băng Tiễn** (phần làm chậm chờ có quái) | đổi skill trong Inspector, cả 4 skill Q đều dùng được |
| **4d** | E còn lại: **Khiên Phép**, **Hoá Giải**, **Đồng Hồ Cát**, **Dịch Chuyển** | 5 skill E dùng được, Hoá Giải đếm số đạn đã xoá |
| **4e** | R còn lại: **Triệu Hồi Linh Thú**, **Ngưng Đọng** + toàn bộ hiệu ứng Lv5 | đủ 13 skill, chỉnh cấp 1–5 trong Inspector thấy khác biệt |

- **Phím debug khi test** (chỉ chạy trong Editor): **F1/F2/F3** đổi vòng skill ở ô
  Q/E/R, **F4** nạp đầy năng lượng R, **F5** tăng cấp tất cả skill thêm 1. Nhờ vậy
  test skill được trước khi có shop.
- Trước giai đoạn 3 (Progression), loadout cấu hình tay trong Inspector. Sau đó
  `Progression` sẽ nạp loadout từ file save.

## Non-goals

- Không làm cây kỹ năng (skill tree) phân nhánh, chỉ chọn skill và nâng cấp tuyến tính.
- Không làm combo giữa các skill (vd. băng + lửa = nổ hơi nước). Ngoại lệ duy nhất là
  hiệu ứng Lv5 của Mưa Băng Tiễn.
- Không dùng mana.
- Không có ô skill bị động riêng (bị động đến từ thẻ lên cấp và trang bị).
- Chưa làm sprite-sheet animation VFX hoàn chỉnh; Q/E/R dùng sprite trong suốt kết hợp chuyển động, scale và fade ở runtime.
- Không đổi skill giữa trận, chỉ đổi ở shop.

## Open questions

- **Làm skill trước hay quái trước?** Bước 4a–4b test được ngay với boss. Còn Sét Lan,
  làm chậm, Hoá Giải đẩy quái thì cần quái thường (giai đoạn 2) mới thấy hết tác dụng.
- Lướt Chổi theo **phím di chuyển** (đề xuất mới) hay theo **chuột** như bản cũ?
- Thiên thạch có nên gây sát thương lên cả player không (thêm rủi ro), hay luôn an toàn?
- Boss có nên miễn nhiễm choáng (Sét Lan Lv5) hoàn toàn, hay chỉ bị giảm thời gian?
