# Bộ asset E / R — Houkimajo

Đối chiếu `docs/skills.md`: 5 E và 4 R. Tạo bằng image_gen tích hợp, cùng phong cách
chibi/cel shading với bộ Q và tông huy hiệu phép của HUD. 9 PNG atlas, tổng 44 sprite.

## E — xanh dương / trắng

Thư mục `Assets/_Game/Art/Sprites/Skills/E/`. Mỗi PNG 1254×1254, lưới 2×2,
4 ô 627×627; thứ tự trái→phải, trên→dưới. Tên đầy đủ = tiền tố + `_` + tên phần.

| Skill | PNG | Tiền tố | 4 phần |
|---|---|---|---|
| Lướt Chổi | e_broom_dash_v1.png | E_BroomDash | Icon, Trail, CastSpark, MasteryEcho |
| Khiên Phép | e_magic_shield_v1.png | E_MagicShield | Icon, ShieldRing, Hit, MasteryBreak |
| Hoá Giải | e_dispel_v1.png | E_Dispel | Icon, ClearRing, CoinSpark, MasteryPush |
| Đồng Hồ Cát | e_hourglass_v1.png | E_Hourglass | Icon, SlowField, TimeMote, MasteryField |
| Dịch Chuyển | e_teleport_v1.png | E_Teleport | Icon, Departure, Arrival, MasteryExplosion |

- Lướt: Trail hướng +Y, xoay theo hướng lướt thực tế; fade khoảng 0.18s. MasteryEcho
  chỉ nhấn mạnh hai lượt Lv5, không thêm sát thương hay tự kích hoạt lượt thứ hai.
- Khiên: ShieldRing bao quanh player và để giữa thoáng; Hit khi đỡ đạn. MasteryBreak
  dùng lúc vỡ khiên Lv5, vùng xóa đạn radius 1.5 do gameplay quyết định.
- Hoá Giải: ClearRing mở rộng theo vùng xóa radius 2→3; CoinSpark là VFX chuyển xu,
  không thay pickup xu thật. Giữ giới hạn 30 xu/lần trong gameplay. MasteryPush hướng
  ra ngoài, áp cho quái thường theo spec.
- Đồng Hồ Cát: SlowField/MasteryField đi theo player, radius 2.5→3.5; TimeMote để
  rải thưa trên mép vùng. Hạt cát thể hiện làm chậm, không phải đóng băng hay xoá đạn.
- Dịch Chuyển: Departure ở chỗ rời đi, Arrival tại chỗ tới. MasteryExplosion nằm
  ở điểm rời đi (50 damage, radius 1), không ở điểm đến.

## R — vàng kim / tím

Thư mục `Assets/_Game/Art/Sprites/Skills/R/`. Mỗi PNG 1536×1024, 3 cột × 2 hàng,
6 ô 512×512; thứ tự trái→phải, trên→dưới.

| Skill | PNG | Tiền tố | 6 phần |
|---|---|---|---|
| Mưa Thiên Thạch | r_meteor_shower_v1.png | R_MeteorShower | Icon, Projectile, WarningRing, Impact, MasteryBurnZone, Ember |
| Triệu Hồi Linh Thú | r_familiar_summon_v1.png | R_FamiliarSummon | Icon, Familiar, Projectile, SummonRing, Impact, MasteryBlock |
| Tia Sao Băng | r_star_beam_v1.png | R_StarBeam | Icon, BeamBody, BeamStart, BeamEnd, Impact, MasteryExplosion |
| Ngưng Đọng | r_time_stop_v1.png | R_TimeStop | Icon, TimeSeal, FrozenBulletOverlay, CastRipple, ResumeBurst, MasteryClearRing |

- Thiên thạch: WarningRing đỏ hiện trước 0.4s, radius 1. Projectile hướng gốc +Y,
  xoay theo hướng rơi khi dùng; Impact khi rơi, Ember chỉ là hạt trang trí. Lv5 vùng
  lửa giữ 1s. Không dùng bloom/vùng sáng texture làm hitbox hoặc phạm vi xóa đạn.
- Linh thú: Familiar là mèo đen có viền lavender, hướng phải và có thể flipX.
  Đây là 1 tư thế tĩnh, chưa có animation bay/chớp mắt. Mỗi mèo bắn 5 phát/s như spec;
  MasteryBlock chỉ phát khi mèo Lv5 chặn đạn thành công, tối đa 5 viên/mèo.
- Tia Sao Băng: BeamBody hướng +Y, hai đầu phẳng, dùng kéo dài giữa BeamStart và
  BeamEnd. Các phần có lề trong ô: cần căn theo phần tia thật/mesh để không tạo khe
  hở giữa thân và cap. Giữ bề rộng gameplay 0.8→1.4, không scale đồng đều cả chiều dài
  và bề rộng. Impact ở mục tiêu, MasteryExplosion ở đầu tia khi kết thúc Lv5.
- Ngưng Đọng: TimeSeal/CastRipple minh họa thời gian dừng, không tint phủ màn và
  không dùng hình băng. FrozenBulletOverlay bao quanh đạn; không thay màu/silhouette
  gốc làm người chơi quên loại đạn. ResumeBurst khi hết dừng, MasteryClearRing là
  vùng xóa radius 3 quanh player lúc hết Lv5. Gameplay vẫn cho player di chuyển/bắn.

## Unity và phạm vi

- Sprite Multiple, pivot giữa, Full Rect, Bilinear, Clamp, không mipmap/compression,
  alpha transparency. PPU bằng cạnh ô: E = 627, R = 512; mỗi ô tương ứng 1×1 world unit.
- Mở rộng PNG trong Project để kéo từng sub-sprite vào Inspector. Icon phù hợp với ô
  HUD 42–64 px; không kéo nguyên Texture atlas vào `SkillSO.icon`.
- Đây là 44 thành phần tĩnh, không phải các frame animation. Scale/fade/di chuyển
  bằng VFX runtime và pool; không Instantiate/Destroy mỗi lần dùng skill.
- Các vòng có lề; hiệu chỉnh scale theo phần hình thực tế và radius gameplay.
  `skill_er_catalog.json` có rect, PPU, alpha mẫu và bounds gần đúng để hỗ trợ căn chỉnh.
- Một số phép có ánh sáng bán trong suốt mảnh hoặc hạt ở giữa; không dùng texturealpha
  để quyết định vùng sát thương/miễn nhiễm. Khi tích hợp, kiểm tra trên arena nhiều đạn.
- Đã gắn 5 E / 4 R vào gameplay bằng SkillEffectController và SkillBootstrapper:
  F2/F3 đổi skill, F4 nạp R, F5 tăng cấp. Asset đã nối vào icon, đạn, VFX chính và Lv5.

## Nguồn và kiểm tra

- `skill_er_prompts.json`: 9 prompt gốc và bước chỉnh lề Khiên Phép, công cụ image_gen.
- `skill_er_catalog.json`: tên, GUID, fileID, rect cho cả 44 sprite; ảnh không rỗng,
  kiểm tra lề tránh cắt phần opaque và alpha vùng giữa các vòng.
- `skill_er_import_validation.txt`: kết quả import thực tế bằng Unity 6000.6.5f1,
  đối chiếu tên và fileID trong bản sao project. Kết quả kiểm tra gameplay/Play mode ở
  `skill_er_gameplay_validation.txt`; hướng dẫn thử trong `Assets/_Game/SETUP.md`.
