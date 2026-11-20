# Enemy bullets v1

Generated with built-in image_gen, six separate transparent PNG assets.
Final prompts: `output/imagegen/enemy_bullets_prompts.json`.
Source paths, dimensions, PPU and sampled solid bounds: `enemy_bullets_manifest.json`.

| Quái | Sprite lưu trong project | Kiểu bắn mặc định | PPU |
|---|---|---|---|
| Dơi Bóng Đêm | `Assets/_Game/Art/Sprites/Bullets/bullet_enemy_shadow_bat_v1.png` | Aimed1 / 3.2 units/s / 2.5s | 2130 |
| Đom Đóm Lửa | `Assets/_Game/Art/Sprites/Bullets/bullet_enemy_ember_firefly_v1.png` | Aimed1 / 3 units/s / 1.6s | 2240 |
| Bí Ngô Bay | `Assets/_Game/Art/Sprites/Bullets/bullet_enemy_flying_pumpkin_v1.png` | Radial8 / 2.2 units/s / 1.8s | 2075 |
| Ma Trơi | `Assets/_Game/Art/Sprites/Bullets/bullet_enemy_wisp_ghost_v1.png` | AimedFan3 / 30° / 2.6 units/s / 2.3s | 2294 |
| Ma Thư | `Assets/_Game/Art/Sprites/Bullets/bullet_enemy_cursed_grimoire_v1.png` | AimedFan3 / 36° / 3 units/s / 1.8s | 2212 |
| Sứa Trăng | `Assets/_Game/Art/Sprites/Bullets/bullet_enemy_moon_jelly_v1.png` | Radial6 / 1.8 units/s / 2.5s | 1427 |

All PNGs preserve original generated RGBA; no raster edits or resampling.
Transparent background and visible body verified with 4px alpha sampling.
Solid longest edge targets 0.30–0.34 world units, excluding faint glow.
Enemy bullet collider stays radius0.08 and root scale stays1.

BulletPatternSO holds sprite/tint/orientation and AimedFan spreadAngle.
Every projectile uses the existing fixed-capacity BulletPool. Launch resets prefab
art/tint/rotation before applying a pattern, preventing cross-species/boss visual leaks.
Boss patterns without sprites keep the prefab appearance and original firing modes.

Mở Stage01 rồi Play để thử sáu loại trong wave mở đầu màn1.
Menu Houkimajo > Apply Enemy Bullet Art + Patterns cập nhật lại cấu hình; Setup Stages
cũng gọi bước này qua EnemyGameplayBuilder. Chỉ số hiện có được giữ, riêng Ma Thư
được migrate từ Aimed1 sang AimedFan3 một lần khi gắn art đầu tiên.

Validation details are recorded in enemy_bullets_validation.txt after Unity checks.
