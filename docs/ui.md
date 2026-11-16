# UI

> Khớp với `Assets/_Game/Scripts/UI/` — namespace `Houkimajo.UI`

## Goal

Hiển thị đủ thông tin để người chơi ra quyết định trong lúc né đạn (mạng, hồi chiêu,
năng lượng ultimate) mà không che arena, và cung cấp màn shop để quản lý skill/đồ.

## Requirements

### HUD trong trận

- Góc trên trái: **mạng**, **vàng**, **Level + thanh XP**.
- Giữa phía trên: **thanh HP boss** (chỉ hiện khi boss xuất hiện) + tên pha.
- Góc **trái dưới**: dock **SPACE / Q / E / R** rộng 420, cao 114 px ở scale 1
  (nhỏ hơn khoảng 30% so với bản 596×154). Neo theo safe area, lề 12 px.
- Bốn huy hiệu phép tròn nền mực tím, viền vàng cổ hai lớp, họa tiết trăng/nhánh lá,
  nối bằng dây vàng mảnh. Bỏ khung thẻ chữ nhật và tiêu đề kiểu bảng điều khiển.
- Giữ icon, tên nhỏ có bóng, nhãn thuộc tính, phím và cấp số La Mã (I–V).
  Vòng hồi/số giây luôn nằm trong huy hiệu; cơ chế cooldown/nâng cấp không đổi.
- Q/E/R có lớp phủ quét tròn và số giây còn lại, flash viền khi cast/thất bại.
  Skill nhiều lượt vẫn sáng nếu còn lượt, hiện số lượt độc lập với tiến độ hồi.
- SPACE dùng thanh nạp mảnh để phản ánh nhịp bắn nhanh, không nhấp nháy số mỗi 0.1s.
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

- 3 tab:
  - **Lên cấp**: hiện từng lượt thẻ đang chờ (3 thẻ, chọn 1). Có số lượt còn lại trên tab.
  - **Skill**: 3 ô Q/E/R, bấm vào ô → danh sách skill đã mở của ô đó để lắp; mỗi skill có
    nút **Nâng cấp (giá)**, xám đi nếu thiếu tiền hoặc đã Lv5.
  - **Cửa hàng**: danh sách đồ bán theo ô (Chổi/Mũ/Bùa/Tiêu hao), mua / bán / lắp.
- Rê chuột lên skill/item → hiện mô tả + chỉ số Lv hiện tại và Lv kế.
- Nút **Tiếp tục** ở góc dưới phải.

### Menu chính (scene `MainMenu`)

- Tên game to ở giữa, 2 nút: **Chơi tiếp — Màn n** (xám đi nếu chưa có save) và
  **Chơi mới** (có save thì đổi chữ thành "Chơi mới (xoá tiến trình)").
- Bấm chuột hoặc phím: **Enter** = Chơi tiếp (không có save thì Chơi mới).
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
- Chưa thay art HUD mạng/boss/shop; đợt này tập trung dock bốn kỹ năng.
- Không hiển thị số sát thương bay lên (damage numbers).

## Open questions

- Dùng UGUI (như HUD hiện tại) hay chuyển sang UI Toolkit cho màn shop nhiều bảng biểu?
  Đề xuất: **giữ UGUI** cho đồng bộ với code đang có.
