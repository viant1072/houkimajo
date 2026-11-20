# Enemy gameplay sprites v1

## Goal

Chuyển sáu concept đã duyệt thành sprite idle nền alpha trong suốt, đồng bộ nét
chibi/viền tím đậm với nhân vật. Dùng trực tiếp làm Sprite trong Unity.

## Assets

- Atlas: `Assets/_Game/Art/Sprites/Enemies/enemies_gameplay_v1.png`
- Kích thước: 1536 x 1024, lưới 3 cột x 2 hàng; mỗi ô 512 x 512.
- `.meta`: Sprite (2D and UI), Multiple, PPU 512, pivot giữa, Bilinear,
  Wrap Clamp, alpha transparency, không mipmap; đã khai báo sáu sprite riêng.
- Mỗi sprite có một pose idle; chưa có animation, VFX chết hay AI/prefab.

| Ô (từ trên xuống, trái sang phải) | Unity sprite | Tên |
|---|---|---|
| 1 | Enemy_ShadowBat_Idle | Dơi Bóng Đêm |
| 2 | Enemy_EmberFirefly_Idle | Đom Đóm Lửa |
| 3 | Enemy_FlyingPumpkin_Idle | Bí Ngô Bay |
| 4 | Enemy_WispGhost_Idle | Ma Trơi |
| 5 | Enemy_CursedGrimoire_Idle | Ma Thư |
| 6 | Enemy_MoonJelly_Idle | Sứa Trăng |

Trong Project của Unity, mở rộng atlas để kéo từng sprite con vào SpriteRenderer.
PPU512 tương ứng ô sprite rộng 1 world unit; chỉnh scale theo kích thước quái mong muốn.
Kích thước collider cần bám thân quái, không lấy toàn bộ cánh/ánh sáng làm hitbox.

## Validation

PNG RGBA 1536x1024; xác nhận alpha=0 ở các góc và khoảng trống giữa các ô.
Lấy mẫu pixel theo bước 4px cho từng ô: có phần thân alpha>127, có nền alpha=0,
không có phần thân chạm dải biên 8px. Chi tiết ở `enemies_gameplay_alpha_check.json`.
Metadata dùng schema của atlas skill đã có, gồm sáu rect512, tên/ID riêng,
GUID asset/sprite mới. Chưa chạy xác nhận import bằng Unity cho bộ này.

## Non-goals

Chưa gắn quái vào gameplay, chưa sửa wave hoặc cân bằng chỉ số.
Ba mẫu Ma Trơi/Ma Thư/Sứa Trăng vẫn là thiết kế mở rộng, chưa có spec hành vi chốt.

## Sources

Concept đã duyệt: exec-0546f269-13ea-4740-90a3-d1ea50e389db.png
Generated gameplay atlas: exec-7c087318-64ac-4e15-a575-f6020f4db4ad.png
Nguồn gốc giữ tại thư mục generated_images của Codex; atlas được sao chép nguyên bản,
không dùng thuật toán xoá nền hoặc chỉnh pixel sau khi sinh.

## Animation bổ sung

Đã có bộ animation bay/idle 4 frame cho cả sáu loại; xem enemy_animation_assets.md và enemy_animation_preview.html.

