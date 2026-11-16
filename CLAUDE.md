# Houkimajo

Game 2D **bullet-hell twin-stick shooter + RPG**: nhân vật chính là một cô phù thủy
nhỏ cưỡi chổi bay trong arena 1 màn hình, ngắm bằng chuột để bắn quái tới từ mọi phía
theo từng wave, rồi đánh boss nhiều pha với pattern đạn dày đặc. Giết quái nhận vàng
+ XP, lên cấp mở skill Q/E/R, mua trang bị và nâng cấp skill ở shop giữa các wave.
Tiến trình lưu vĩnh viễn.

## Thông tin kỹ thuật

- **Engine:** Unity 6.6 (6000.6.5f1), render pipeline **Universal 2D (URP)**
- **Editor code:** VS Code (không dùng Visual Studio), extension `C# Dev Kit` + `Unity`
- **Input:** package `com.unity.inputsystem`, nhưng script đọc trực tiếp qua
  `Keyboard.current` / `Mouse.current` (không dùng Input Actions asset) — đơn giản hoá cho prototype
- Chưa có automated test. Chạy thử bằng Play mode trong Unity Editor.

## Cấu trúc thư mục

```
Houkimajo/
├── Assets/_Game/           # toàn bộ code/asset tự viết, tách khỏi asset bên thứ 3
│   ├── Scripts/
│   │   ├── Player/         # di chuyển, bắn, máu người chơi
│   │   ├── Bullets/        # object pool đạn, pattern đạn (ScriptableObject)
│   │   ├── Boss/           # máu boss, state machine chuyển pha
│   │   ├── Enemies/        # quái thường, wave, spawner, pickup vàng/XP
│   │   ├── Skills/         # skill Q/E/R (SkillSO), SkillCaster, năng lượng ultimate
│   │   ├── Progression/    # level/XP, vàng, trang bị, shop, PlayerStats, lưu game
│   │   ├── Systems/        # GameManager, ScoreManager, StageDirector (luồng wave → boss)
│   │   └── UI/              # HUD, màn shop
│   ├── Prefabs/, Data/, Art/, Audio/, Scenes/
│   └── SETUP.md            # checklist thao tác Editor còn thiếu (tạo prefab, gán field...)
├── docs/                   # spec kỹ thuật, 1 file/module, viết TRƯỚC khi code thay đổi lớn
└── Assets/Scenes, Settings # còn lại do Unity template URP 2D tự sinh, không đụng vào
```

Mỗi file trong `docs/` khớp 1:1 với 1 thư mục trong `Scripts/` — xem `docs/README.md`
để biết quy ước.

## Quyết định kiến trúc đã chốt (đừng đổi mà không bàn trước)

1. **Object pooling cho đạn (`BulletPool`)** — bullet-hell spawn/destroy hàng trăm
   object/giây, không dùng `Instantiate`/`Destroy` trực tiếp. Mọi đạn mới (player lẫn
   enemy/boss) phải đi qua pool.
2. **Pattern đạn = ScriptableObject (`BulletPatternSO`)**, không hardcode trong code.
   Muốn thêm kiểu bắn mới → tạo asset mới trong `Assets/_Game/Data/BulletPatterns`,
   không sửa `BulletEmitter`.
3. **Boss nhiều pha = state machine (`BossStateMachine` + `BossPhaseSO`)**, chuyển
   pha dựa theo % HP còn lại (hoặc hết giờ). Thêm pha mới → thêm asset `BossPhaseSO`
   vào list trong Inspector, không sửa code.
4. **Namespace theo module**: `Houkimajo.Player`, `Houkimajo.Bullets`, `Houkimajo.Boss`,
   `Houkimajo.Enemies`, `Houkimajo.Skills`, `Houkimajo.Progression`, `Houkimajo.Systems`,
   `Houkimajo.UI` — khớp tên thư mục `Scripts/`.
5. **Điều khiển** (đổi ngày 16/11/2026, thay kiểu Touhou giữ Z bắn thẳng lên): WASD/mũi
   tên di chuyển, **chuột ngắm**, giữ **Space** bắn, giữ **Shift** = focus mode (bay
   chậm + hiện hitbox), **Q/E/R** = skill. Arena 1 màn hình, quái tới từ mọi phía.
6. **Tiến trình kiểu RPG, lưu vĩnh viễn** (JSON trong `persistentDataPath`), thua không
   mất XP/vàng. **XP → mở khoá/lựa chọn, vàng → sức mạnh** (nâng skill, mua đồ).
7. **Skill, item, quái, wave, stage đều là ScriptableObject** (`SkillSO`, `ItemSO`,
   `EnemyDataSO`, `WaveSO`, `StageSO`). Save lưu theo `id` của SO, không theo tên file.
8. **Shop mở giữa các wave và luôn mở trước boss**; lên cấp giữa trận không pause game,
   thẻ lựa chọn được xử lý ở shop.

## Convention

Quy tắc code đầy đủ: [`docs/coding-rules.md`](docs/coding-rules.md) (naming, hiệu
năng cho bullet-hell, data-driven qua ScriptableObject, giao tiếp qua event...).

Tóm tắt nhanh:
- Comment trong code: tiếng Việt, giải thích **tại sao**. Tên biến/hàm/class: tiếng Anh.
- File spec trong `docs/`: tiếng Việt, theo khung Goal/Requirements/Non-goals/Open
  questions.
- **Luôn qua pool, không `Instantiate`/`Destroy` đạn, quái, projectile skill hay pickup
  trực tiếp.**

## Việc đang dở / chưa làm

Đã có vertical slice kiểu cũ (Z bắn, chỉ có boss), dựng scene theo
`Assets/_Game/SETUP.md`. Đang chuyển sang thiết kế mới: spec đã viết xong trong `docs/`
(`player`, `bullets`, `enemies`, `skills`, `progression`, `systems`, `ui`). Code chia 6
giai đoạn: (1) điều khiển chuột + Space ✅, (2) quái + wave + StageDirector + menu chính
+ 3 màn tự chuyển ✅ (shop giữa wave chưa có), (3) PlayerStats
+ vàng + XP + lưu game, (4) skill Q/E/R chia bước 4a–4e trong `docs/skills.md`
(4a khung skill + Tên Lửa + Lướt Chổi ✅), (5) shop + trang bị + thẻ lên cấp,
(6) đủ skill + cân bằng.
Chưa có: art/sprite thật, animation, âm thanh, UI menu chính, màn chơi ngoài
`Stage01` (test scene).
