# Background previews v1

Ba background theo các màn trong `docs/systems.md`, tạo bằng built-in image_gen.
Ảnh đã được chép vào `Assets/_Game/Art/Sprites/Backgrounds/` để gắn vào StageSO
và scene gameplay. Thư mục này giữ bản gốc xem thử.

| File | Màn | Hướng mỹ thuật |
| --- | --- | --- |
| bg_stage01_night_forest_v1.png | Rừng Đêm | Chàm/xanh ngọc, rễ cổ thụ, nấm phát sáng, sương nhẹ |
| bg_stage02_pumpkin_marsh_v1.png | Đầm Lầy Bí Ngô | Nước tối, cây khô và bí ngô ánh hổ phách ở rìa |
| bg_stage03_witch_tower_v1.png | Tháp Phù Thuỷ | Sàn đá tím, viền vàng cũ, sách và pha lê quanh tháp |

Các ảnh dùng góc nhìn từ trên xuống, arena giữa thoáng, nền tối và điểm sáng tập
trung ở rìa để đạn/skill nổi rõ. Góc trái dưới dành cho HUD phép thuật. Đồ trang trí
chỉ là hình nền, không biểu diễn vật cản có collider.

Prompt đầy đủ: `../stage_background_prompts_v1.json`.
Runtime giữ tỷ lệ và cover/crop theo camera. Đây là ảnh nền nguyên tấm, chưa tách
lớp parallax. Độ đọc đạn trong các đợt bắn dày cần được đánh giá thêm khi chơi thử.
