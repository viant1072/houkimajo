# Houkimajo (箒魔女)

Game 2D thể loại **bullet-hell / danmaku shooter** (phong cách Touhou): nhân vật
chính là một cô phù thủy nhỏ cưỡi chổi bay, dùng phép thuật bắn đạn tiêu diệt boss
bay, né tránh pattern đạn dày đặc của đối thủ.

## Công nghệ

- **Engine:** Unity 6.6 (6000.6.5f1), render pipeline Universal 2D (URP)
- **Ngôn ngữ:** C#
- **Editor code:** VS Code (`C# Dev Kit` + `Unity` extension)
- **Input:** `com.unity.inputsystem`

## Bắt đầu nhanh

1. Mở project bằng **Unity Hub** (bản `6000.6.5f1` hoặc tương thích)
2. Mở scene `Assets/_Game/Scenes/Stage01.unity`
3. Menu **Houkimajo → Setup Playtest Scene (Placeholder Art)** — tự động dựng
   Player, Boss, đạn, HUD... bằng sprite tạm (xem chi tiết tại
   [Assets/_Game/SETUP.md](Assets/_Game/SETUP.md))
4. Bấm **Play**

**Điều khiển:** WASD/mũi tên di chuyển · giữ **Shift** = focus mode (bay chậm,
né chính xác hơn) · giữ **Z** = bắn liên tục

## Cấu trúc project

```
Assets/_Game/
├── Scripts/        # Player, Bullets, Boss, Systems, UI — theo module
├── Prefabs/, Data/, Art/, Audio/, Scenes/
├── Editor/          # tool dựng scene playtest tự động
└── SETUP.md         # checklist/hướng dẫn chạy thử

docs/                # spec kỹ thuật từng module + coding-rules.md
CLAUDE.md             # bối cảnh dự án cho AI coding assistant
```

Xem [CLAUDE.md](CLAUDE.md) để biết các quyết định kiến trúc đã chốt (object
pooling cho đạn, pattern/pha boss qua ScriptableObject, boss state machine...),
và [docs/](docs/) cho spec chi tiết từng hệ thống.

## Trạng thái hiện tại

Vertical slice đã chạy được: di chuyển, bắn, boss 3 pha (Radial → Spiral →
Aimed) đổi kiểu bắn theo % HP, Game Over/Victory. Toàn bộ đang dùng sprite
placeholder — chưa có art/animation/âm thanh thật.
