# Houkimajo

Game 2D thể loại **bullet-hell / danmaku shooter** (phong cách Touhou): nhân vật chính
là một cô phù thủy nhỏ cưỡi chổi bay, dùng phép thuật bắn đạn tiêu diệt boss bay, né
tránh các pattern đạn dày đặc của đối thủ.

## Thông tin kỹ thuật

- **Engine:** Unity 6.6 (6000.6.5f1), render pipeline **Universal 2D (URP)**
- **Editor code:** VS Code (không dùng Visual Studio), extension `C# Dev Kit` + `Unity`
- **Input:** package `com.unity.inputsystem`, nhưng script đọc trực tiếp qua
  `Keyboard.current` (không dùng Input Actions asset) — đơn giản hoá cho prototype
- Chưa có automated test. Chạy thử bằng Play mode trong Unity Editor.

## Cấu trúc thư mục

```
Houkimajo/
├── Assets/_Game/           # toàn bộ code/asset tự viết, tách khỏi asset bên thứ 3
│   ├── Scripts/
│   │   ├── Player/         # di chuyển, bắn, máu người chơi
│   │   ├── Bullets/        # object pool đạn, pattern đạn (ScriptableObject)
│   │   ├── Boss/           # máu boss, state machine chuyển pha
│   │   ├── Systems/        # GameManager, ScoreManager (singleton xuyên scene)
│   │   └── UI/              # HUD
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
   `Houkimajo.Systems`, `Houkimajo.UI` — khớp tên thư mục `Scripts/`.
5. Gameplay core hiện tại (chưa có art/animation thật): di chuyển WASD/mũi tên, giữ
   **Shift** = focus mode (bay chậm, né đạn chính xác hơn — chuẩn Touhou), giữ **Z**
   = bắn liên tục.

## Convention

Quy tắc code đầy đủ: [`docs/coding-rules.md`](docs/coding-rules.md) (naming, hiệu
năng cho bullet-hell, data-driven qua ScriptableObject, giao tiếp qua event...).

Tóm tắt nhanh:
- Comment trong code: tiếng Việt, giải thích **tại sao**. Tên biến/hàm/class: tiếng Anh.
- File spec trong `docs/`: tiếng Việt, theo khung Goal/Requirements/Non-goals/Open
  questions.
- **Luôn qua `BulletPool`, không `Instantiate`/`Destroy` đạn trực tiếp.**

## Việc đang dở / chưa làm

Xem `Assets/_Game/SETUP.md` — checklist thao tác trong Unity Editor (tạo Player
GameObject, Bullet prefab, gán reference...) để có vertical slice chơi được lần đầu.
Chưa có: art/sprite thật, animation, âm thanh, UI menu chính, màn chơi ngoài
`Stage01` (test scene).
