# Houkimajo (箒魔女)

Game 2D **arena bullet-hell**: một cô phù thủy nhỏ cưỡi chổi, ngắm chuột để bắn phép, né đạn, vượt các wave quái và đối đầu boss nhiều pha. Project đang phát triển theo hướng shooter kết hợp RPG.

## Bắt đầu nhanh

1. Mở project bằng **Unity Hub**, dùng **6000.6.5f1** như trong `ProjectSettings/ProjectVersion.txt`.
2. Đợi Unity compile xong rồi nhấn **Play**. Mặc định Editor mở menu chính; chọn **Bắt đầu**, **Chơi mới** hoặc **Chơi tiếp** theo trạng thái save.
3. Muốn thử trực tiếp màn chơi: mở `Assets/_Game/Scenes/Stage01.unity`, bỏ chọn **Houkimajo > Start Play Mode From Main Menu**, rồi nhấn Play. Shop/thoại đầu màn có thể xuất hiện trước trận.

Scene đã có nội dung; không cần Rebuild để chạy thử. Hướng dẫn dựng lại và các menu gắn asset riêng nằm trong [SETUP.md](Assets/_Game/SETUP.md).

## Điều khiển

| Thao tác | Phím |
| --- | --- |
| Di chuyển | WASD / mũi tên |
| Ngắm | Chuột |
| Bắn phép thường | Giữ chuột trái |
| Focus: bay chậm, hiện hitbox | Giữ Left Control |
| Lướt | Left Shift |
| Dùng kỹ năng | Q / E / R |
| Mở / đóng shop | B |
| Dùng Bình HP | 1 |
| Pause / đóng giao diện hiện tại | Escape |

Trong **Unity Editor**: F1/F2/F3 đổi skill Q/E/R để thử, F4 nạp đầy năng lượng R, F5 tăng cấp skill đang lắp (Lv5 quay lại Lv1).

## Trạng thái hiện tại

- Menu chính và ba màn theo dữ liệu: **Rừng Đêm / Đầm Lầy Bí Ngô / Tháp Phù Thuỷ**, có background riêng, wave quái, boss nhiều pha và luồng thắng/thua/chuyển màn.
- Nhân vật có art và animation bay, giơ tay chưởng phép qua cầu tím. Giữ bắn sẽ giữ tư thế giơ tay, không lặp động tác.
- **6 loại quái**, **3 boss** có sprite/animation; đạn quái riêng, đạn nhân vật có animation, ánh sáng và hiệu ứng trúng mục tiêu.
- Skill Q/E/R có bộ art riêng, HUD phép thuật với hồi chiêu, HP/MP và năng lượng R.
- Shop prototype hỗ trợ Bình HP và nâng cấp Q/E/R bằng vàng; có giao diện pause/túi đồ, lưu tiến trình và thoại màn đầu.

**Đang phát triển:** XP/level, thẻ lên cấp, mở khóa skill, trang bị/inventory tổng quát, pet, âm thanh và cân bằng. Các mục này có trong thiết kế nhưng chưa hoàn thiện trong gameplay. Shop hiện có seed vàng thử một lần.

Save hiện lưu tiến độ màn, vàng, Bình HP/cooldown và cấp skill đã mua trong `Application.persistentDataPath/save.json`. **Chơi mới** khởi tạo lại tiến trình.

## Catalog ảnh và assets

Mở [RawAssets/index.html](RawAssets/index.html) bằng trình duyệt để xem catalog cục bộ:

- Preview ảnh, tìm kiếm và lọc theo nhân vật, quái, boss, đạn, Q/E/R, background, UI, concept và ảnh gameplay.
- Xem từng vùng sprite trong sheet, thử animation, đổi nền caro/tối/sáng.
- Đánh dấu asset và xuất danh sách lựa chọn ra JSON.
- Tra file nguồn, kích thước ảnh, PPU và tài nguyên Unity liên quan.

Xem [mục lục RawAssets](RawAssets/README.md) và [manifest nguồn/bản sao](RawAssets/manifest.csv). Kho này là bản sao để xem và quản lý; Unity vẫn dùng tài nguyên trong `Assets/_Game/`.

Cập nhật kho khi thêm/sửa assets (PowerShell 7; Node.js để tạo lại dữ liệu catalog):

```powershell
pwsh -File ./RawAssets/Sync-RawAssets.ps1
```

Chỉ cập nhật catalog từ manifest hiện có:

```powershell
node RawAssets/build-catalog.mjs
```

## Công nghệ và cấu trúc

Unity **6000.6.5f1** · Universal 2D (URP) · C# · Input System. Có thể dùng VS Code với C# Dev Kit và Unity extension.

```text
Assets/_Game/
├── Scripts/       # Player, Bullets, Boss, Enemies, Skills, Progression, Systems, UI
├── Editor/        # Tool dựng scene, gắn assets và kiểm tra Editor
├── Prefabs/       # Đạn, quái, visual boss và projectile skill
├── Data/          # ScriptableObject: skill, pattern, boss phase, quái, wave, stage
├── Art/           # Sprite, material và shader
├── Audio/         # Chỗ dành cho âm thanh
├── Scenes/        # MainMenu + Stage01; ba màn dùng chung gameplay scene
└── SETUP.md
RawAssets/         # Kho bản sao phân nhóm + catalog HTML
output/            # Prompt, manifest sản xuất, preview và validation
docs/              # Đặc tả kỹ thuật và cốt truyện
```

## Phát triển và kiểm tra

Đọc [AGENTS.md](AGENTS.md) cho hướng dẫn repository, [CLAUDE.md](CLAUDE.md) cho bối cảnh kỹ thuật và [docs/README.md](docs/README.md) để tìm spec. Quy tắc code nằm ở [docs/coding-rules.md](docs/coding-rules.md).

Đạn/quái/projectile skill/pickup dùng pooling; pattern, phase và nội dung gameplay dùng ScriptableObject. Commit `.meta` cùng asset để giữ GUID và cấu hình sprite.

Kiểm tra gameplay trong **Unity Play mode**, giữ Console sạch và thử các luồng liên quan. Có một số kiểm tra Editor, chưa có suite tự động thống nhất hay lệnh build CLI chung. Tạo bản build qua **File > Build Profiles**. Không commit `Library/`, `Temp/`, `Logs/`, build và file IDE sinh tự động.
