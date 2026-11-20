# Boss gameplay sprites v1

Ba tạo hình boss đã duyệt được chuyển thành sprite gameplay bằng built-in image_gen.
Mỗi boss được sinh riêng từ concept tương ứng; PNG nguồn được chép nguyên bản vào
project, giữ alpha, không chỉnh pixel hoặc resize sau khi sinh.

| Boss | File trong project |
|---|---|
| Nữ Hoàng Bướm Đêm | `Assets/_Game/Art/Sprites/Bosses/boss_moth_queen_gameplay_v1.png` |
| Bá Tước Bí Ngô | `Assets/_Game/Art/Sprites/Bosses/boss_pumpkin_regent_gameplay_v1.png` |
| Phù Thuỷ Tinh Tú | `Assets/_Game/Art/Sprites/Bosses/boss_astral_archwitch_gameplay_v1.png` |

## Cấu hình

- Cả ba PNG RGBA kích thước 1254x1254, một pose idle, nền trong suốt.
- Kèm .meta: Sprite (2D and UI), Single, pivot giữa, PPU512, Bilinear, Clamp,
  alpha transparency, không mipmap, không nén. Cùng hệ PPU để có kích thước gần nhau.
- Ở scale1, thân và phụ kiện chiếm khoảng2.3–2.4 world units theo chiều lớn nhất.
  Có thể chỉnh scale/PPU khi tích hợp gameplay; chưa đặt collider cho bộ này.
- Giảm chi tiết cánh/lông, dây leo/viền áo và họa tiết váy; bỏ vòng sao rời của
  Phù Thuỷ Tinh Tú để phần thân rõ giữa mưa đạn.

## Kiểm tra

Đã xem trực tiếp cả ba ảnh để kiểm tra tạo hình, thân đầy đủ và loại bỏ khung/chữ.
Đã xác nhận PNG có alpha0 ở bốn góc. Lấy mẫu bước4px: có thân alpha>127, có nền alpha0,
dải8px dọc biên có alpha tối đa1, không có phần thân bị cắt ở cạnh ảnh.
Chi tiết kích thước, bounds và nguồn: `boss_gameplay_manifest_v1.json`.
Metadata được tạo theo schema sprite đơn có sẵn trong project; chưa chạy kiểm tra
import bộ boss này bằng Unity Editor.

## Phạm vi

Đây là bộ sprite tĩnh đã chuẩn bị để dùng trong Unity. Chưa animation, chưa thay
boss trong scene/stage và không thay đổi AI, phase, chỉ số hoặc hitbox.
Prompt đầy đủ: `boss_gameplay_prompts_v1.json`.

## Bộ animation bổ sung

Đã có ba sheet idle4frame và prefab hình ảnh riêng; xem boss_animation_assets_v1.md và boss_animation_preview_v1.html.

