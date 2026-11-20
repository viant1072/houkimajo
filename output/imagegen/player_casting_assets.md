# Player casting pose + magic orb

## Kết quả

Nhân vật giữ một tay trên chổi, giơ tay còn lại niệm phép khi bắn thường. Một cầu phép
riêng bay trước hướng ngắm, có pulse/ánh sáng tím; đạn và muzzle flash xuất phát từ đó.
Cả hai ảnh được tạo bằng built-in image_gen, giữ nguyên PNG nguồn và alpha sau khi sinh.
Prompt đầy đủ: player_casting_prompts.json.

- `Assets/_Game/Art/Sprites/Player/player_witch_cast_v1.png`:1254x1254, lưới2x2,
  bốn frame627x627, PPU627, pivot custom(0.61,0.42) khớp đúng bộ bay hiện có.
- `Assets/_Game/Art/Sprites/Player/player_cast_orb_v1.png`:1254x1254, Sprite Single,
  PPU1254. Cầu hiển thị ở scale0.36, pulse nhẹ, Light2D radius0.6.
- Frame cast phát10fps một lần rồi giữ frame cuối khi bắn liên tục; trở về bay0.18s
  sau phát cuối. Mỗi viên đạn không reset động tác giơ tay.
- Cầu cách root0.5 units dọc hướng ngắm. Pose chỉ lật trái/phải theo hệ sprite cũ;
  cầu/đạn vẫn ngắm đủ360 độ. Cầu không có collider và không cần tạo object khi bắn.
- Root/hitbox0.12, visual scale1.2, damage10, interval0.1 và speed12 không đổi.

## Cách dùng

Đã lưu vào Stage01. Hoàn tất/bỏ qua thoại, đóng shop và giữ chuột trái để bắn.
Thả chuột/khóa bắn sẽ trở về idle bay; pause/thoại/shop giữ frame và vị trí cầu.
Menu Houkimajo > Apply Player Casting Animation gắn/cập nhật pose và một cầu duy nhất;
Add Skills/Setup/Rebuild cũng gọi bước này.

## Runtime

PlayerFlightAnimation chuyển giữa mảng frames/castingFrames theo NotifyShot; chỉ refresh
hold time khi đang cast. PlayerCastingOrb cache aim, điều khiển renderer/light tạo sẵn.
PlayerShooting cache cả hai trong Awake, lấy điểm bắn từ orb và chỉ Notify sau khi lấy
được đạn từ pool. Click bị UI chặn, pool cạn hay bị skill khóa không tạo pose bắn giả.

## Kiểm tra

Đã kiểm tra Play mode bằng MouseState thật: cast liên tục qua nhiều frame, đổi hướng
trái/phải, pause, release, khóa bắn và điểm sinh đạn. Xem player_casting_validation.txt.
Ảnh render: player_casting_idle.png và player_casting_fire.png. Camera trong ảnh được
zoom gần nhân vật để xem pose, không đổi camera của scene đã lưu.
Kiểm tra alpha: player_casting_alpha_check.json. Mỗi frame có nền alpha0 và không có
thân chạm dải biên8px; orb có nền alpha0, một góc có alpha1/255 gần như trong suốt.

Nguồn: exec-2c3d6fcc-87e7-45dd-a2e3-1c8b7c761377.png (casting sheet),
exec-9e78a853-2649-497c-bf2e-70d2659f37ea.png (orb).
