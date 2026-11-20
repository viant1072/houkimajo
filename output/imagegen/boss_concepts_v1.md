# Boss concepts v1

Ba mẫu đề xuất để xem tạo hình, tạo bằng built-in image_gen. Chưa phải sprite nền
trong suốt, chưa animation và chưa gắn gameplay. Không thay docs/boss.md bằng ý tưởng
chưa được duyệt.

## Căn cứ

docs/boss.md quy định boss cuối stage, nhiều pha, xuất hiện phía trên arena và
di chuyển chậm (Static/Hover/FollowPlayerX). Tài liệu chưa chốt tên/tạo hình boss.
Ba chủ đề dưới đây là đề xuất theo tên màn hiện có trong Data/Stages, không phải
nhân vật đã được quy định trong spec. Không giả định boss triệu hồi quái vì còn là
open question; không thêm spell-card riêng từng pha.

| Mẫu | Màn đề xuất | Đặc điểm | File |
|---|---|---|---|
| Nữ Hoàng Bướm Đêm | Rừng Đêm | Bốn cánh, tím/xanh ngọc, vương miện trăng; thân trung tâm rõ | boss_moth_queen_concept_v1.png |
| Bá Tước Bí Ngô | Đầm Lầy Bí Ngô | Thân tròn, tay dây leo, áo choàng đỏ, lõi sáng | boss_pumpkin_regent_concept_v1.png |
| Phù Thuỷ Tinh Tú | Tháp Phù Thuỷ | Dáng cao, trượng sao, sách bay, áo xanh tím | boss_astral_archwitch_concept_v1.png |

Định hướng khi chuyển sang gameplay: giảm họa tiết, tách thân khỏi ánh sáng,
giữ hitbox bám thân, hạn chế glow; không tính toàn bộ cánh/áo choàng là hitbox.
Giữ một boss mỗi stage và tái sử dụng bộ khung phase hiện có. Phần thưởng/HP/pattern
chưa đổi trong lần thiết kế concept này.

Prompt đầy đủ: boss_concept_prompts_v1.json. Mỗi boss sinh bằng một lời gọi riêng;
tham chiếu phong cách từ player_witch_gameplay_v1.png. Cả ba file đã được xem trực tiếp
để kiểm tra full-body, dáng lơ lửng và sự khác nhau về hình dáng/màu sắc.
