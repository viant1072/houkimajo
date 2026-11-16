# Houkimajo — Dựng scene chơi thử

## Cách nhanh (khuyên dùng) — 1 click

Đã viết sẵn Editor script tự động dựng toàn bộ scene test (Player, đạn, pool, boss,
1 pattern đạn mẫu) bằng sprite hình khối màu tạm — chưa cần art thật.

1. Mở project trong Unity, đợi compile xong (thanh progress ở góc dưới phải biến mất)
2. Mở scene `Assets/_Game/Scenes/Stage01.unity`
3. Menu bar → **Houkimajo → Setup Playtest Scene (Placeholder Art)**
   (scene đã dựng từ trước → dùng **Houkimajo → Rebuild Playtest Scene** để xoá
   object cũ và dựng lại theo code mới nhất)
4. **Ctrl+S** để lưu scene — script không tự lưu
5. Xem Console: nếu không có lỗi đỏ, bấm **Play**

Điều khiển: WASD/mũi tên di chuyển, **chuột** ngắm, giữ **Space** = bắn liên tục,
giữ **Shift** = focus mode (bay chậm + hiện chấm hitbox trắng), **Q** = Tên Lửa Ma
Thuật, **E** = Lướt Chổi (lướt theo phím đang giữ, không giữ thì theo chuột).

Phím debug skill (chỉ trong Editor): **F1/F2/F3** đổi skill ở ô Q/E/R (lấy từ list
`Debug Skill Library` trên `SkillCaster`), **F4** nạp đầy R, **F5** tăng cấp mọi skill đang lắp (Lv5 → về Lv1).

Đã có scene dựng từ trước, chỉ muốn thêm skill mà không Rebuild: menu
**Houkimajo → Add Skills To Current Scene**, rồi Ctrl+S.

## Menu chính + 3 màn + quái thường

Với scene `Stage01` đã dựng (có Player), chạy **1 lần**:
**Houkimajo → Setup Stages + Main Menu (3 màn, quái, menu chính)**. Script sẽ:

- Tạo asset quái (`Data/Enemies`), wave (`Data/Waves`), pattern + pha boss màn 2–3,
  3 màn + `StageCatalog` (`Data/Stages`), prefab `Enemy` / `GoldPickup` (`Prefabs/Enemies`)
- Gắn `StageSystems` (StageDirector + EnemySpawner + 2 pool) và chữ thông báo màn vào
  HUD, **tự lưu** scene Stage01
- Tạo scene `MainMenu` và đặt Build Settings: MainMenu → Stage01

Chơi đủ vòng: mở `Assets/_Game/Scenes/MainMenu.unity` → Play → **Chơi mới**. Mỗi màn:
"MÀN n" → các wave quái từ 4 phía (giết rớt vàng, lại gần là hút) → "BOSS!" → hạ boss
→ tự sang màn kế sau 3s. Qua màn 3 → "PHÁ ĐẢO!" → về menu. Thua → Enter chơi lại,
Esc về menu. Play thẳng `Stage01` = test màn 1.

Save nằm ở `Application.persistentDataPath/save.json` (Windows:
`%USERPROFILE%\AppData\LocalLow\<Company>\houkimajo\save.json`) — xoá file này hoặc
bấm **Chơi mới** để chơi lại từ đầu. Chỉnh độ khó: sửa asset trong `Data/Waves`,
`Data/Enemies`, `Data/Stages` (script không ghi đè asset đã có).

Script tự tạo:
- `Player` (tag `Player`, có `PlayerController`/`PlayerHealth`/`PlayerAim`/
  `PlayerShooting`/`PlayerVisuals`; con `HitboxIndicator` và `GrazeZone` có
  `GrazeDetector` — đạn địch sượt qua vùng này được cộng 10 điểm)
- `Crosshair` (tâm ngắm xanh lá đi theo chuột, con trỏ hệ thống bị ẩn khi đang chơi)
- `SkillCaster` trên Player + `SkillPools` (pool projectile skill, tự tạo lúc chạy) +
  `SkillBarUI` (4 ô phép thuật nhỏ ở góc trái dưới: SPACE/Q/E/R, cấp, hồi chiêu và
  năng lượng R; nháy đỏ khi chưa dùng được). Asset skill nằm ở `Assets/_Game/Data/Skills/` — chỉnh chỉ số
  từng cấp trực tiếp trong Inspector, Rebuild không ghi đè.
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

Chạy lại **Setup** khi đã có Player/Boss trong scene sẽ bị chặn (tránh tạo trùng).
Muốn dựng lại từ đầu thì dùng **Rebuild**: chỉ xoá các object do script tạo ra
(Main Camera và object tự thêm vẫn giữ), có thể Ctrl+Z để hoàn tác.

## Animation nhân vật

- `Stage01` trên đĩa đã gắn vòng bay 4 frame vào object con `Player/Visual`.
- Nếu Unity đang mở scene cũ hoặc scene có chỉnh sửa chưa lưu, dùng **Houkimajo →
  Apply Player Flight Animation** để gắn vào Player hiện tại mà không cần Rebuild.
  Menu hỗ trợ Undo và không tạo trùng nếu đã có animation. Nhấn **Ctrl+S** sau khi gắn.
- Setup/Rebuild cũng tự dùng sheet `Art/Sprites/Player/player_witch_flight_v1.png`.
- Chọn `Player/Visual` để chỉnh `Frames Per Second` (mặc định 6), `Bob Amplitude`
  (0.025 đơn vị) và `Rock Angle` (1.5 độ). Focus tắt nhún/nghiêng; pause dừng vòng bay.
- `PlayerVisuals` phụ trách lật/nhấp nháy; renderer phải trỏ tới `Visual`, không phải root.
- Sheet 1254×1254, 4 ô 627×627, Sprite Multiple, Bilinear, không mipmap/compression.
- `Visual` dùng scale 1.2 để nhân vật rõ hơn; root và collider giữ nguyên kích thước.

## Bộ skill Q và mỹ thuật

- HUD gồm 4 huy hiệu phép **SPACE / Q / E / R** nằm ở **góc trái dưới**, lề 12 px,
  kích thước gốc 420×114, tự co theo safe area. Viền vàng cổ, trăng lưỡi liềm, dây
  nối mảnh và cấp La Mã I–V thay cho thẻ vuông. Mỗi ô có phím, icon,
  tên, cấp và nhãn thuộc tính; Q/E/R có vòng hồi và số giây, SPACE có thanh nạp mảnh.
- Profile bắn thường mặc định: `Data/Skills/BasicAttack_Arcane.asset`. Mảng `levels`
  có damage, fireInterval, bulletSpeed cho từng cấp; thêm cấp trong asset khi thiết kế
  nâng cấp. Lv1 giữ 10 damage / 0.1s / 12 tốc độ như trước.
- Khi làm shop/trang bị, gọi `PlayerShooting.EquipAttack(profile, level)` hoặc
  `SetAttackLevel(level)`. HUD tự đổi theo event. Profile có icon, tên, nhãn/màu thuộc tính,
  sprite/tint đạn; hiệu ứng nguyên tố và giá nâng cấp chưa triển khai ở bước HUD này.
- Ô R hiện năng lượng thật 0→100%; **F4** nạp đầy để thử nhanh trong Editor.
  Sát thương thường/Q/E và graze nạp R, ultimate không tự nạp lại.
- Chạy lại **Add Skills To Current Scene** để cập nhật dock của scene cũ; Ctrl+S để lưu.

- Scene đã cấu hình skill: **Q** dùng skill, **F1** đổi vòng Tên Lửa → Cầu Lửa →
  Sét Lan → Băng Tiễn (chỉ trong Editor), **F5** tăng cấp 1→5 rồi quay về 1.
- **F2**: Lướt Chổi → Khiên Phép → Hoá Giải → Đồng Hồ Cát → Dịch Chuyển.
- **F3**: Mưa Thiên Thạch → Triệu Hồi Linh Thú → Tia Sao Băng → Ngưng Đọng.
  Nhấn **E/R** để dùng; đổi R giữ nguyên năng lượng. Mặc định R là Mưa Thiên Thạch.
- Lv5 có hai lượt lướt, vỡ khiên/xóa đạn, đẩy quái, tăng vùng cát, nổ chỗ teleport,
  vùng cháy thiên thạch, mèo chặn đạn, nổ cuối beam và xóa đạn cuối Ngưng Đọng.
  Boss miễn nhiễm các trạng thái chỉ dành cho quái thường. Vàng Hoá Giải lưu trong
  Wallet của player ở stage, chưa có màn shop/save progression.
- Nếu đang mở scene cũ, dùng **Houkimajo → Add Skills To Current Scene**, rồi **Ctrl+S**.
  Menu gắn art vào prefab/SkillSO có sẵn và giữ các chỉ số đã chỉnh.
- Icon Q đổi theo skill; lớp phủ tròn và chữ thể hiện hồi chiêu. Đạn và VFX dùng bộ
  sprite Q trong `Art/Sprites/Skills/Q`. VFX và projectile được tạo sẵn trong pool.
- Cầu Lửa nổ khi chạm hoặc tới con trỏ; Sét cần có mục tiêu trong tầm 4 quanh con trỏ.
  Chỉ có boss thì Sét đánh 1 lần. Băng xuyên tối đa 3 mục tiêu.
- Lv5 có nổ tên lửa, vùng lửa, VFX choáng và bonus băng. Boss miễn nhiễm slow/stun;
  cần quái thường implement `ISkillStatusTarget` để thấy hai trạng thái đó.
- Kiểm tra Unity batch trên **bản sao project** bằng
  `-batchmode -nographics -executeMethod Houkimajo.EditorTools.QSkillGameplayChecks.Run`.
  Lệnh tạo và lưu cấu hình Stage01 trong bản sao trước khi kiểm tra sát thương/pool/VFX.
- E/R có kiểm tra batch tương tự qua `Houkimajo.EditorTools.ERSkillGameplayChecks.Run`.
  Kiểm tra phím thật và vòng đời hiệu ứng trong Play mode dùng
  `Houkimajo.EditorTools.ERPlayModeSmokeChecks.Run` (không thêm `-quit`, script tự thoát).
  Các lệnh này chỉ chạy trên bản sao project vì chúng tạo scene và dữ liệu kiểm tra.

## Quái có sprite và animation

- Mở `Assets/_Game/Scenes/Stage01.unity` rồi Play để vào màn 1. Wave đầu có hai con
  mỗi loại: Dơi, Đom Đóm, Bí Ngô, Ma Trơi, Ma Thư, Sứa Trăng; sau đó tới wave cũ và boss.
- Sprite và nhịp animation nằm trên từng `EnemyDataSO` trong `Data/Enemies`.
  Prefab `Prefabs/Enemies/Enemy.prefab` dùng chung, nạp lại frame khi lấy từ pool.
- **Houkimajo > Apply Enemy Art + Six Enemy Showcase** gắn/cập nhật lại bộ art;
  chạy nhiều lần không chèn trùng wave và giữ chỉ số chiến đấu đã chỉnh.
- Animation dừng cùng pause, giữ nguyên fade lúc hiện hình và collider theo data.

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
