# Houkimajo — Bối cảnh cho coding assistant

Game 2D **arena bullet-hell**, hướng phát triển twin-stick shooter + RPG: cô phù thủy cưỡi chổi, di chuyển bằng bàn phím, ngắm chuột, đánh wave quái rồi boss nhiều pha. Tiến trình RPG đầy đủ vẫn đang phát triển.

Đọc [AGENTS.md](AGENTS.md) để biết quy tắc làm việc, [docs/coding-rules.md](docs/coding-rules.md) cho convention và [docs/README.md](docs/README.md) để tìm spec. Spec có cả kế hoạch tương lai; kiểm tra code trước khi khẳng định một hệ thống đã hoàn tất.

## Công nghệ và thư mục

- Unity **6000.6.5f1**, Universal 2D (URP), C#; phiên bản chính xác trong `ProjectSettings/ProjectVersion.txt`.
- Input System, đọc trực tiếp `Keyboard.current` / `Mouse.current`; không phụ thuộc Input Actions asset cho điều khiển hiện tại.
- VS Code với C# Dev Kit và Unity extension.
- `Assets/_Game/Scripts/`: Player, Bullets, Boss, Enemies, Skills, Progression, Systems, UI.
- `Assets/_Game/Editor/`: builder dựng scene/gắn art và các kiểm tra Editor.
- `Assets/_Game/Prefabs/`, `Data/`, `Art/`, `Audio/`, `Scenes/`: tài nguyên gameplay.
- `docs/`: đặc tả theo module, quy tắc code và cốt truyện trong `story.md`.
- `output/`: prompt, catalog sản xuất, ảnh preview và kết quả validation.
- `RawAssets/`: bản sao tổng hợp; `index.html` là catalog ảnh cục bộ.

## Chạy thử và điều khiển hiện tại

Mở project bằng Unity Hub đúng phiên bản. Nhấn Play mặc định đi qua menu chính; tắt **Houkimajo > Start Play Mode From Main Menu** để thử trực tiếp scene đang mở. Có hai scene chính `MainMenu` và `Stage01`; ba màn dùng chung gameplay scene và đổi dữ liệu `StageSO`.

| Thao tác | Phím |
| --- | --- |
| Di chuyển / ngắm | WASD hoặc mũi tên / chuột |
| Bắn thường | Giữ chuột trái |
| Focus: bay chậm, hiện hitbox | Giữ Left Control |
| Lướt mặc định | Left Shift |
| Kỹ năng đang lắp | Q / E / R |
| Mở/đóng shop | B |
| Dùng Bình HP | 1 |
| Pause / đóng lớp UI hiện tại | Escape |
| Đổi skill Q / E / R, chỉ Editor | F1 / F2 / F3 |
| Nạp R / đổi cấp skill, chỉ Editor | F4 / F5 |

Không dùng hướng dẫn cũ Z/Space để bắn hoặc Shift để focus.

## Những phần đã triển khai

- Menu chính, lưu tiến độ màn, ba cấu hình màn: Rừng Đêm, Đầm Lầy Bí Ngô, Tháp Phù Thuỷ; wave, quái và boss nhiều pha.
- Nhân vật có sprite bay, animation, pose giơ tay và cầu phép theo hướng ngắm. Động tác giơ tay **phát một lần rồi giữ frame cuối** khi bắn liên tục; thả bắn trở về bay.
- Sáu loại quái có sprite/animation và đạn riêng; ba boss có sprite/animation; ba background tự đổi theo màn.
- Đạn nhân vật có animation, ánh sáng đạn/cầu phép, muzzle và hiệu ứng trúng mục tiêu.
- Bộ skill Q/E/R cùng art, profile bắn thường và HUD phép thuật ở đáy với LMB/Q/E/R, HP/MP và năng lượng ultimate.
- Shop prototype, mua/bán/dùng Bình HP, nâng cấp Q/E/R bằng vàng và lưu cấp đã mua theo ID skill. Có pause/túi đồ giao diện và thoại cốt truyện màn đầu.
- Save JSON hiện giữ tiến độ màn, vàng, bình/cooldown, cờ seed tiền thử và cấp skill đã mua. Shop có seed vàng thử một lần; đây là cấu hình prototype.

## Chưa hoàn tất

- Hệ XP/level và thẻ lên cấp, mở khóa skill, `PlayerStats`, trang bị và inventory tổng quát vẫn là kế hoạch trong `docs/progression.md`.
- Chưa có hệ pet hoạt động; vị trí pet trên HUD là chỗ chừa.
- Chưa có âm thanh được thêm vào thư mục Audio; cân bằng và nội dung production còn tiếp tục.
- Chưa có suite test tự động thống nhất hoặc lệnh build CLI chung. Có một số kiểm tra Editor và báo cáo Play mode trong `output/`.

## Kiến trúc cần giữ

1. Đạn người chơi/địch/boss qua `BulletPool`; quái, projectile skill và pickup cũng qua pool. Không Instantiate/Destroy mỗi phát bắn.
2. Pattern dùng `BulletPatternSO`; boss dùng `BossStateMachine` và `BossPhaseSO`; mở rộng nội dung bằng asset dữ liệu.
3. Skill, quái, wave và stage dùng ScriptableObject. Giữ ổn định ID để save không hỏng; `ItemSO` và các cấu hình RPG tương lai chưa phải hệ thống đã có.
4. Namespace theo thư mục module, comment tiếng Việt giải thích lý do, identifier tiếng Anh. UI ưu tiên cập nhật bằng event; tránh cấp phát trong Update/hot path.
5. Art/animation đặt trên visual con, không thay collider chỉ để sprite trông to hơn. Giữ GUID/pivot/PPU và tham chiếu sprite khi sửa asset.
6. Save nằm tại `Application.persistentDataPath/save.json`. Không ghi đè save thật bằng dữ liệu test.

## Dựng scene và cập nhật assets

Xem [Assets/_Game/SETUP.md](Assets/_Game/SETUP.md). Tên menu **Setup Playtest Scene (Placeholder Art)** là tên lịch sử; các builder hiện gắn cả art thật. Với scene đã được chỉnh, ưu tiên menu Apply tương ứng thay vì Rebuild toàn bộ. Lưu Ctrl+S sau thay đổi; một số builder cấp stage tự lưu scene.

`RawAssets/` là kho bản sao ngoài thư mục Unity import. Nguồn gameplay vẫn ở `Assets/_Game/`. Không chuyển cả kho vào `Assets/` vì `.meta` giữ GUID gốc.

```powershell
# PowerShell 7: sao chép asset, kiểm tra SHA256 và cập nhật catalog nếu có Node.js
pwsh -File ./RawAssets/Sync-RawAssets.ps1

# Chỉ tạo lại dữ liệu catalog
node RawAssets/build-catalog.mjs
node --check RawAssets/catalog.js
```

Mở [RawAssets/index.html](RawAssets/index.html) để xem ảnh, vùng sprite, animation và xuất lựa chọn. `manifest.csv` lưu nguồn/bản sao/hash; `catalog-data.js` được sinh tự động. Catalog dùng HTML/CSS/JS cục bộ, không cần framework hay CDN.

## Kiểm tra trước khi báo hoàn tất

Chạy kiểm tra phù hợp với phạm vi sửa. Với gameplay, thử trong Play mode, kiểm tra Console và các tình huống liên quan (pause, pooling, đổi skill, shop/save hoặc chuyển boss). Với hình ảnh, có screenshot/clip thực tế. Ghi rõ giới hạn kiểm chứng; đừng coi compile thành công là đã xác nhận gameplay. Giữ nguyên những thay đổi không thuộc nhiệm vụ trong workspace.
