# Bộ asset Q — Houkimajo

Nguồn yêu cầu: `docs/skills.md`, phần Q. Tạo bằng công cụ image_gen tích hợp.
Prompt gốc trong `skill_q_prompts.json`; tên sprite, GUID, fileID và rect trong
`skill_q_catalog.json` cùng thư mục.

## Danh mục

Mỗi PNG là atlas 2×2: icon ở trên trái, projectile/tia ở trên phải,
impact ở dưới trái, hiệu ứng Lv5 ở dưới phải. Các ô là asset khác nhau,
không phải các frame animation.

| Skill | File dưới Assets/_Game/Art/Sprites/Skills/Q/ | Các sub-sprite |
|---|---|---|
| Tên Lửa Ma Thuật | q_magic_missile_v1.png | Q_MagicMissile_Icon, Projectile, Impact, MasteryExplosion |
| Cầu Lửa | q_fireball_v1.png | Q_Fireball_Icon, Projectile, Impact, MasteryBurningZone |
| Sét Lan | q_chain_lightning_v1.png | Q_ChainLightning_Icon, Arc, Impact, MasteryStun |
| Mưa Băng Tiễn | q_ice_arrow_rain_v1.png | Q_IceArrowRain_Icon, Projectile, Impact, MasteryShatter |

Trong bảng, tiền tố của Icon cũng áp dụng cho các tên còn lại: ví dụ
`Q_Fireball_Projectile`, `Q_Fireball_Impact`, `Q_Fireball_MasteryBurningZone`.

## Import và sử dụng

- Cả 4 PNG: 1254×1254 RGBA; 4 ô 627×627; Sprite Multiple, 627 PPU,
  pivot giữa ô, Full Rect, Bilinear, Clamp, alpha transparency, không mipmap/compression.
- Mở rộng mũi tên cạnh PNG trong Project để kéo từng sub-sprite vào Inspector.
- Icon: kéo vào `SkillSO.icon` hoặc `UI.Image.sprite`; nên xem thử ở 48–64 px.
  HUD chữ hiện tại chưa tự hiển thị icon.
- Projectile hướng gốc +Y; `SkillProjectile.AlignToVelocity` hiện có đã xoay theo
  hướng này. Scale chỉ object hình ảnh nếu muốn giữ collider độc lập.
- Gợi ý chiều dài hình projectile ban đầu: tên lửa 0.45–0.6 world unit,
  cầu lửa 0.6–0.8, băng tiễn 0.5–0.7. Đây là gợi ý mỹ thuật, không đổi chỉ số skill.
- Arc sét: đặt giữa 2 mục tiêu, xoay trục +Y theo hướng nối và scale chiều dài.
  Artwork có lề trong ô nên cần căn hai đầu theo bounds phần tia thực tế;
  không dùng chiều dài toàn ô làm tầm nhảy gameplay.
- Impact: gợi ý scale/fade 0.12–0.25 giây; nổ lớn 0.25–0.4 giây.
  Dùng thời gian có scale và pool khi triển khai, tránh tạo/hủy mỗi lần trúng.
- Vòng nổ/vùng lửa có lề trong ô: căn kích thước hình theo radius gameplay,
  không lấy kích thước texture làm vùng sát thương. Giữ alpha vừa phải.
- Lv5: nổ tên lửa radius 0.5; vùng lửa 2 giây; choáng sét 0.5 giây
  chỉ trên quái thường; vỡ băng minh họa bonus damage, không mang nghĩa đóng băng.

## Phạm vi và kiểm tra

- Đã xem 4 ảnh kết quả, kiểm tra PNG vuông chẵn, góc nền alpha 0,
  đủ nội dung trong cả 16 ô và fileID/rect trong metadata.
- Đã lưu các PNG và `.meta` trong project, cập nhật spec asset ở `docs/skills.md`.
- Đã tích hợp bộ Q qua `SkillBootstrapper`: 4 SkillSO, 3 prefab projectile, icon HUD,
  VFX trúng và Lv5. F1 đổi vòng Q và F5 tăng cấp trong Editor.
- Hiệu ứng dùng pool riêng; vùng lửa có 32 chỗ dành riêng để hiệu ứng sáng không
  chiếm hết pool gây damage. Boss miễn nhiễm slow/stun, quái thường nối qua ISkillStatusTarget.
- Kiểm tra tích hợp batch qua `Houkimajo.EditorTools.QSkillGameplayChecks.Run` trên
  bản sao project. Kết quả cuối được ghi trong `skill_q_gameplay_validation.txt`.
