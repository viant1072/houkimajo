# Kho ảnh và assets Houkimajo

Bản sao tổng hợp từ project, được chia theo nội dung. Asset gameplay đang dùng vẫn
nằm tại `Assets/_Game/`; ảnh và tài liệu thiết kế gốc vẫn ở `output/`.

## Catalog ảnh

Mở [index.html](index.html) bằng trình duyệt (bấm đúp file). Trang chạy offline,
không cần cài thư viện hay chạy server.

- Tìm theo tên file, tên sprite hoặc nhóm; lọc ảnh/tất cả file/danh sách đã chọn.
- Bấm preview để xem lớn, đổi nền caro/tối/sáng và xem từng vùng sprite trong sheet.
- Các sheet animation có nút phát thử; động tác giơ tay phát một lần rồi giữ frame cuối.
- Tick chọn asset, mở **Đã chọn** và **Xuất danh sách JSON** để lưu lựa chọn kèm đường dẫn nguồn.
  Lựa chọn được lưu trong trình duyệt nếu trình duyệt cho phép localStorage.
- File cấu hình, shader, prefab và tài liệu nằm trong chế độ **Tất cả file**;
  file `.meta` được ẩn mặc định và có thể bật lại.
- Sau khi sync, Node.js (nếu có) tự cập nhật `catalog-data.js`. Có thể cập nhật riêng
  bằng `node RawAssets/build-catalog.mjs`. HTML/CSS/JS là giao diện, không sửa ảnh gốc.

| Thư mục | Nội dung |
| --- | --- |
| [01_Player](01_Player/) | Nhân vật chính, sprite gameplay, animation bay/giơ tay, cầu phép |
| [02_Enemies](02_Enemies/) | Sheet 6 loại quái và animation |
| [03_Bosses](03_Bosses/) | Sprite gameplay và animation 3 boss |
| [04_Bullets](04_Bullets/) | Đạn nhân vật và đạn quái, chia `Player/` và `Enemies/` |
| [05_Skills](05_Skills/) | Sheet kỹ năng, chia `Q/`, `E/`, `R/` |
| [06_Backgrounds](06_Backgrounds/) | 3 nền màn chơi và material nền |
| [07_UI](07_UI/) | Sprite giao diện |
| [08_Placeholders](08_Placeholders/) | Ảnh tạm đời đầu, tâm ngắm, hitbox, vàng |
| [09_Effects](09_Effects/) | Material, shader hiệu ứng; âm thanh nếu có |
| [10_Unity](10_Unity/) | Prefab, ScriptableObject data, scene, cấu hình render/input |
| [11_Concepts](11_Concepts/) | Ảnh concept boss đã lưu trong project |
| [12_Previews](12_Previews/) | Ảnh chụp gameplay/HUD/shop và ảnh xuất xem trước, giữ cấu trúc nguồn |
| [13_Documentation](13_Documentation/) | Prompt, catalog, manifest sản xuất, preview HTML, báo cáo kiểm tra và đặc tả game |

## Tra cứu và cập nhật

- `manifest.csv`: từng file có đường dẫn nguồn, đường dẫn bản sao, dung lượng và SHA256.
- `summary.json`: số lượng file, ảnh, tổng dung lượng và kết quả đối chiếu hash.
- Chạy `pwsh -File ./RawAssets/Sync-RawAssets.ps1` tại thư mục project để cập nhật.
  Script sao chép, xác minh byte bằng SHA256 và không xóa file.
- Các sheet giữ nguyên PNG và `.meta` để lưu pivot, PPU, vùng cắt sprite và GUID.
  Các sprite con/animation chạy bằng code vẫn nằm trong sheet, không xuất lại thành PNG rời.
- Kho này nằm ngoài `Assets/`, Unity không import trùng GUID. Khi khôi phục, đưa file
  về đúng vị trí nguồn trong manifest; prefab/scene còn tham chiếu script ở project gốc.
- HUD và một số hiệu ứng được vẽ bằng code, nên không có ảnh nguồn riêng; ảnh xem
  trước nằm trong `12_Previews/`. Hiện chưa có file âm thanh được tác giả thêm vào game.
- Chỉ tổng hợp tài nguyên hiện có trong project; không quét cache sinh ảnh ngoài project.
  Không chứa `Library/`, `Temp/`, bản recovery scene hay file build.
