# Enemy animation v1

## Goal

Bộ vòng lặp bay/idle cho cả sáu mẫu quái đã duyệt, giữ nền alpha trong suốt.

## Assets

- Sáu sheet: `Assets/_Game/Art/Sprites/Enemies/enemy_*_flight_v1.png`.
- Mỗi sheet 1254x1254, lưới 2x2; mỗi frame 627x627. Giữ nguyên chất lượng ảnh nguồn.
- Unity Sprite Multiple, PPU627, pivot giữa, Bilinear, Clamp, không mipmap.
- Thứ tự frame: trên trái → trên phải → dưới trái → dưới phải → lặp.
- 6 prefab dùng ngay: `Assets/_Game/Prefabs/Enemies/Visuals/Enemy_*_AnimatedVisual.prefab`.
- Bộ phát: `EnemySpriteAnimation`, chỉ thay SpriteRenderer.sprite; không đổi transform,
  không cấp phát theo frame, dùng Time.deltaTime; reset frame0 khi tái kích hoạt.

| Quái | Chuyển động | Tốc độ |
|---|---|---|
| Dơi Bóng Đêm | Vỗ cánh | 6 fps |
| Đom Đóm Lửa | Vẫy cánh, râu chuyển động | 10 fps |
| Bí Ngô Bay | Vỗ cánh, dây leo đung đưa | 5 fps |
| Ma Trơi | Uốn đuôi và ngọn lửa | 5 fps |
| Ma Thư | Quạt cánh trang sách, dây đánh dấu đung đưa | 6 fps |
| Sứa Trăng | Co giãn thân và xúc tu | 4 fps |

## Cách xem

Mở `output/imagegen/enemy_animation_preview.html` bằng trình duyệt để xem cả sáu
vòng lặp; ảnh đã nhúng, không cần server hay kết nối mạng. Có nút tạm dừng.
Trong Unity, kéo một prefab từ `Prefabs/Enemies/Visuals` vào scene rồi Play.
Menu **Houkimajo > Create Enemy Animated Visuals** tạo/cập nhật lại prefab từ sheet,
giữ tốc độ và cấu hình renderer trên prefab đã tồn tại.

## Validation

Unity 6000.6.5f1 trên project kiểm tra tối giản riêng: import đủ 24 sprite,
kiểm tra sáu prefab, thứ tự frame, kích thước/PPU, thay frame, vòng lặp,
zero-delta pause, reset khi tái kích hoạt, transform không đổi. Đã qua.
Xem `enemy_animation_validation.txt`. Không gọi đây là full gameplay Play-mode test.
Kiểm tra alpha theo lưới lấy mẫu bước4px cho cả24 frame: có vùng thân, nền alpha0,
không có pixel thân alpha>127 trong dải biên8px; xem `enemy_animation_alpha_check.json`.

## Non-goals

Các prefab `Visuals` là prefab hình ảnh, không có EnemyController, collider hoặc AI.
Bước tích hợp gameplay đã bổ sung: EnemyDataSO giữ bộ frame, prefab quái chung nạp
animation theo data; wave mở đầu màn 1 có đủ sáu loại. Xem `docs/enemies.md`.
Chưa làm animation tấn công/trúng đạn/chết.
Manifest đường dẫn và nguồn ảnh: `enemy_animation_manifest.json`.
