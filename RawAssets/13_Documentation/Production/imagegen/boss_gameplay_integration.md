# Boss gameplay integration

Màn1 Rừng Đêm dùng Nữ Hoàng Bướm Đêm, màn2 Đầm Lầy Bí Ngô dùng Bá Tước Bí Ngô,
màn3 Tháp Phù Thuỷ dùng Phù Thuỷ Tinh Tú. Đã lưu Stage01 và cả ba StageSO.

StageSO giữ bossSprite, bossAnimationFrames[4], bossAnimationFramesPerSecond,
bossColliderRadius. FPS6/5/5; bán kính thân0.42/0.52/0.42 ở scale1. HP, phase,
pattern, phần thưởng và luồng wave/thoại/shop hiện có được giữ nguyên.

StageDirector cache bộ phát và collider trong Awake. RunBoss áp sprite/frame,
màu trắng và collider trước khi bật boss. Stage không có art dùng hình/tint scene
và xóa bộ frame trước đó. Bộ phát dùng Time.deltaTime, dừng khi pause/thoại/shop.

Mở Assets/_Game/Scenes/Stage01.unity rồi Play hoặc chọn màn từ MainMenu.
Boss xuất hiện ở cuối màn theo luồng hiện có. Các khung thoại dùng Space/Enter để
tiếp hoặc Esc để bỏ qua đoạn. Không thêm phím debug nhảy boss vào gameplay.

Houkimajo > Apply Boss Art To Gameplay gắn lại asset, sau đó Ctrl+S để lưu scene.
Setup/Rebuild Stage cũng gọi bước này. Chạy lại không thêm component trùng và giữ
nhịp animation/bán kính đã chỉnh trên data.

Kiểm tra Unity6000.6.5f1 actual Play mode trên bản sao, gọi đúng coroutine RunBoss
để tới nhanh cả ba boss: sprite/palette, animation, pause/thoại, đổi đủba phase,
hitbox, reset khi kích hoạt lại và fallback stage không art đều qua.
Chỉ bỏ qua wave/thoại trong bản kiểm tra; source giữ luồng chơi bình thường.
Chi tiết: boss_gameplay_validation.txt. Hash nguồn được kiểm tra trước khi chép
scene/data đã xác thực về. Settings/save/thay đổi runtime của bản thử không chép về.
