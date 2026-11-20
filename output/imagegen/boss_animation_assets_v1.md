# Boss idle animations v1

Ba sheet tạo bằng built-in image_gen, chuyển từ sprite gameplay đã duyệt.
Prompt đầy đủ: `boss_animation_prompts_v1.json`.
Mỗi sheet1254x1254, lưới2x2, bốn frame627x627, nền alpha trong suốt.
PNG nguồn được chép nguyên bản, không sửa pixel/resize sau khi sinh.

| Boss | Sheet | Nhịp |
|---|---|---|
| Nữ Hoàng Bướm Đêm | Assets/_Game/Art/Sprites/Bosses/boss_moth_queen_idle_v1.png | Vỗ bốn cánh,6fps |
| Bá Tước Bí Ngô | Assets/_Game/Art/Sprites/Bosses/boss_pumpkin_regent_idle_v1.png | Dây leo/rễ/áo choàng chuyển động,5fps |
| Phù Thuỷ Tinh Tú | Assets/_Game/Art/Sprites/Bosses/boss_astral_archwitch_idle_v1.png | Tóc/vạt áo đung đưa,5fps |

## Xem thử

- Mở `output/imagegen/boss_animation_preview_v1.html` trong trình duyệt. File tự chứa
  ảnh, không cần server/mạng; nút Tạm dừng điều khiển cả ba vòng lặp.
- Trong Unity kéo prefab từ `Assets/_Game/Prefabs/Bosses/Visuals` vào scene rồi Play.
- `Boss_MothQueen_AnimatedVisual.prefab`, `Boss_PumpkinRegent_AnimatedVisual.prefab`,
  `Boss_AstralArchwitch_AnimatedVisual.prefab` đã gắn đủ frame và nhịp phát.
- Menu **Houkimajo > Create Boss Animated Visuals** tạo/cập nhật lại prefab; tốc độ
  và cấu hình renderer đã chỉnh trên prefab có sẵn được giữ.

## Cấu hình

Sprite Multiple, PPU256, pivot giữa, Bilinear, Clamp, alpha transparency, không mipmap.
Thứ tự frame: trên trái → trên phải → dưới trái → dưới phải → lặp.
PPU256 cho ô627 giữ cùng kích thước canvas world với sprite tĩnh1254/PPU512.
Prefab chỉ chứa SpriteRenderer và bộ phát frame `EnemySpriteAnimation` đã có;
không tạo thêm bộ phát runtime trùng chức năng. Time.deltaTime dừng animation khi
pause, OnEnable bắt đầu frame0. Bộ phát không đổi transform hay collider.

## Validation và phạm vi

Unity6000.6.5f1 đã import đủ12 sprite, biên dịch builder và tạo3 prefab.
Đã kiểm tra tham chiếu frame sau khi chép về project. Kiểm tra alpha lấy mẫu bước4px
không phát hiện thân chạm biên ô; xem `boss_animation_validation_v1.txt`.
Chưa chạy thêm Play-mode test cho boss trong lần tạo asset này.

Bộ này đã được gắn vào boss của cả ba stage qua StageSO/StageDirector; xem
`boss_gameplay_integration.md` và `boss_gameplay_validation.txt` cho phần kiểm tra runtime.
Chưa làm animation ra đòn/trúng đạn/chết. Manifest/nguồn ảnh: `boss_animation_manifest_v1.json`.
