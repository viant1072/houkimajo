# Player bullet animation

Animation đạn bắn thường được tạo bằng built-in image_gen, từ sprite tím hiện có.
Prompt đầy đủ: `player_bullet_animation_prompt.json`.

## Assets

- Sheet: `Assets/_Game/Art/Sprites/Bullets/bullet_player_arcane_flight_v1.png`.
- PNG1254x1254, lưới2x2, bốn frame627x627 theo thứ tự trên trái → trên phải → dưới trái → dưới phải.
- Unity Sprite Multiple, pivot giữa, alpha transparency, Bilinear, Clamp, không mipmap,
  không nén. PPU1310 cân theo chiều dài thân sáng của sprite tĩnh PPU2560 hiện có.
- Chiều dài thân trung bình mới475/1310≈0.3626 world units; cũ928/2560≈0.3625.
  Các bounds đo theo alpha>127, lấy mẫu4px; xem `player_bullet_animation_geometry.json`.
- Nền alpha0 và không có phần thân chạm dải8px ở mép từng ô. Giữ PNG nguồn nguyên bản.
- Nguồn imagegen: exec-82a179e3-6c68-41cd-9135-90d0b0befadf.png.

## Gameplay

BasicAttackSO có projectileAnimationFrames và projectileAnimationFramesPerSecond;
profile BasicAttack_Arcane dùng bốn frame ở12fps. Không sửa icon hoặc sprite tĩnh fallback.
PlayerShooting nạp frame sau Launch/SetAppearance. BulletBase phát bằng renderer đã
cache, không thêm component hoặc tạo object khi bắn; chỉ mảng frame dùng chung từ SO.

Launch xóa animation/tint/rotation của viên trước và bắt đầu frame0 khi nạp profile.
Frame dừng khi pause/thoại/shop và khi pool TimeStop. Profile tĩnh không giữ frame cũ.
Root scale1, collider0.08, damage10, interval0.1s và speed12 ở Lv1 giữ nguyên.

Mở Stage01, hoàn tất/bỏ qua thoại và đóng shop, giữ chuột trái để bắn như hiện tại.
Menu Houkimajo > Apply Player Bullet Animation gắn lại bộ frame; Setup/Rebuild
và Add Skills cũng gọi bước này. Không cần thêm/sửa object trong scene.

Xem nhanh bằng `output/imagegen/player_bullet_animation_preview.html` (file tự chứa ảnh).
Kết quả kiểm tra runtime được ghi ở `player_bullet_animation_validation.txt`.
