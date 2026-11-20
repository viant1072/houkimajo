# UI

> Khớp với `Assets/_Game/Scripts/UI/` — namespace `Houkimajo.UI`

## Goal

Hiển thị đủ thông tin để người chơi ra quyết định trong lúc né đạn (HP/MP, hồi chiêu,
năng lượng ultimate) mà không che arena, và cung cấp màn shop để quản lý skill/đồ.

## Requirements

### Tạm dừng và kho đồ

- Trong trận, Esc dừng thời gian và mở menu. Chọn Kho đồ hoặc Trang bị để mở bảng nhân vật với bộ lọc tương ứng. Esc trong bảng quay lại menu, Esc tại menu tiếp tục trận.
- Bên trái hiển thị chỉ số thực, ở giữa là avatar động, tám vị trí trang bị, vị trí pet và ba kỹ năng đang lắp. Bên phải là lưới đồ với ba bộ lọc Tất cả, Trang bị, Túi đồ. Bình HP dùng chung số lượng với cửa hàng. Các vị trí chưa có đồ hiển thị trống.

### HUD trong trận

- Góc trên trái: điểm, màn/wave và vàng. Level/XP chờ progression.
- Giữa phía trên: **thanh HP boss** (chỉ hiện khi boss xuất hiện) + tên pha.
- Thanh HUD ngang sát đáy theo mẫu bố cục người dùng: avatar tròn nhô lên bên trái,
  ô pet nhỏ cạnh avatar, bốn ô vuông LMB/Q/E/R ở giữa, HP đỏ và MP xanh bên phải.
  Tông mực tím/vàng cổ, thanh nền cao 76 px ở thiết kế 1280; avatar khoảng 106 px
  (giảm 40% so với bản 176 px), ô skill khoảng 59 px; HP/MP co còn 80%.
  Toàn bộ bám safe area, co đồng đều khi cửa sổ hẹp và giãn chiều ngang khi rộng.
  Pet hiện để trống vì chưa có hệ thống pet trang bị.
- Camera gameplay chừa dải HUD đặc phía dưới; avatar nhô vào mép vùng chơi như mẫu.
- Giữ icon, tên nhỏ có bóng, nhãn thuộc tính, phím và cấp số La Mã (I–V).
  Vòng hồi/số giây luôn nằm trong huy hiệu; cơ chế cooldown/nâng cấp không đổi.
- Q/E/R có lớp phủ quét tròn và số giây còn lại, flash viền khi cast/thất bại.
  Skill nhiều lượt vẫn sáng nếu còn lượt, hiện số lượt độc lập với tiến độ hồi.
- LMB dùng thanh nạp mảnh để phản ánh nhịp bắn nhanh, không nhấp nháy số mỗi 0.1s.
  Icon, tên, cấp, thuộc tính và chỉ số bắn đọc từ `BasicAttackSO` trên PlayerShooting.
- Ô chưa lắp hiển thị "Chưa trang bị". Ô R có skill hiển thị % năng lượng thật từ
  SkillCaster; đủ 100 sáng lên và dùng được. Không dùng cooldown giây cho R.
- Bắn thường có API đổi profile/nâng cấp; metadata thuộc tính dùng để trình bày HUD,
  chưa thêm hiệu ứng damage nguyên tố hay màn shop trong thay đổi này.
- Thông báo giữa màn (tự tắt sau 1.5s): "WAVE 2", "LEVEL UP!", "BOSS!".
- **Tâm ngắm** đi theo chuột (ẩn con trỏ hệ thống khi `Playing`, hiện lại khi ở shop/pause).
- HUD chỉ cập nhật khi nhận event (`OnGoldChanged`, `OnCooldownChanged`...), không poll
  mỗi frame — riêng lớp phủ hồi chiêu được phép cập nhật mỗi frame nhưng không tạo
  string mới (chỉ đổi số khi giây nguyên thay đổi).

### Màn Shop

- Mỹ thuật cửa hàng theo sổ phép tím: bìa chuyển sắc, góc vát, viền vàng cổ mảnh,
  trăng/sao và huy hiệu đồng bộ HUD. Hai bảng là hai trang sổ, ô trống khắc chìm,
  ô chọn có dấu sao/ánh tím; nút ma thuật có hover/pressed/disabled rõ ràng.
  Fade mở shop 0.18s bằng thời gian unscaled. Khung/họa tiết là UGUI vector, chưa
  tạo art item; giữ bố cục và toàn bộ giao dịch/nâng cấp/lưu hiện có.

- Bố cục mới theo wireframe: ba nút loại ở cột trái **Vật phẩm / Trang bị / Nâng cấp**;
  hai lưới cạnh nhau **Cửa hàng / Túi đồ**, chọn ô để xem thông tin bên dưới.
  Nút Mua bên cửa hàng, Bán/Dùng bên túi; X góc trên và B/Escape đóng shop.
- Vật phẩm hiện có Bình HP: mua 60, bán 30, dùng vẫn hồi 40 HP/cooldown 2s.
  Trang bị hiển thị trống, chưa thêm item/art. Nâng cấp Q/E/R đang lắp dùng giá
  của cấp kế trong SkillSO, tối đa Lv5; bảng bên phải hiện kỹ năng đang lắp.
  Không bán skill, không reset năng lượng R hoặc tặng hồi chiêu khi nâng cấp.

- Shop mở đầu stage hoặc B; X/B/Escape đóng và tiếp tục trận. Ô HP riêng trên HUD,
  phím 1 dùng nhanh, số lượng/cooldown và vàng/HP thật. UI dùng màu/chữ tạm.
- Chưa có trang bị, MP potion, checkpoint shop hay inventory nhiều loại. Các phần
  mở rộng và chỉ số item đề xuất nằm trong `progression.md`; giữ bố cục ba loại/hai
  bảng này khi thêm nội dung. Thẻ XP/mở khóa skill để giai đoạn progression sau.
### Menu chính (scene `MainMenu`)

- Chưa có save: **Bắt đầu** vào màn 1, nút **Chơi tiếp** bị khóa.
- Có save: **Chơi tiếp — Màn n** dùng tiến trình đã lưu, **Chơi mới (xoá tiến
  trình)** vào lại màn 1.
- Bấm **Enter** để Chơi tiếp nếu có save hoặc Bắt đầu nếu chưa có save.
- Dòng nhỏ phía dưới: số vàng đang có, số màn đã qua.
- Dùng `EventSystem` + `InputSystemUIInputModule` (Input System mới) — **không** dùng
  `StandaloneInputModule` vì xung đột với Input System.

### Thông báo trong màn

- Chữ to giữa màn, tự tắt: "MÀN 1 — Rừng Đêm" (2s), "WAVE 2/3" (1.5s), "BOSS!" (1.5s).
- Góc trên trái dưới điểm: "Màn 1/3 · Wave 2/2 · Vàng 120", chỉ cập nhật khi đổi.

### Màn kết thúc

- Thắng màn: "HOÀN THÀNH MÀN n" + "Sang màn kế tiếp...". Tự chuyển sau 3s.
- Thắng màn cuối: "PHÁ ĐẢO!" + "Về menu...". Tự về menu sau 4s.
- Thua: "GAME OVER" + "Enter: chơi lại màn · Esc: về menu". Không tự chuyển.
- Phím tắt thay cho nút để màn gameplay không cần `EventSystem`.

## Non-goals

- Không làm màn cài đặt (âm lượng, phím) và nút thoát game ở giai đoạn này.
- Chưa có pause bằng Esc trong lúc chơi.
- Chưa làm pet trang bị; ô pet chỉ là vị trí dự phòng. Không thay HUD boss.
- Không hiển thị số sát thương bay lên (damage numbers).

## Open questions

- Dùng UGUI (như HUD hiện tại) hay chuyển sang UI Toolkit cho màn shop nhiều bảng biểu?
  Đề xuất: **giữ UGUI** cho đồng bộ với code đang có.
