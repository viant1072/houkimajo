# docs/

Thư mục chứa spec kỹ thuật, viết trước khi triển khai code cho từng hệ thống.

## Quy ước

- **Mỗi file = 1 module**, khớp tên với thư mục trong `Assets/_Game/Scripts/`:
  - `player.md` ↔ `Scripts/Player/`
  - `bullets.md` ↔ `Scripts/Bullets/`
  - `boss.md` ↔ `Scripts/Boss/`
  - `systems.md` ↔ `Scripts/Systems/`
  - `ui.md` ↔ `Scripts/UI/`
- Thêm module code mới → thêm file spec mới cùng tên, theo cùng template.
- Mỗi file theo khung 4 mục: **Goal / Requirements / Non-goals / Open questions**.
- Spec viết trước khi code thay đổi lớn; code nhỏ/fix bug không bắt buộc phải có spec.

**Ngoại lệ:** [`coding-rules.md`](coding-rules.md) không theo quy ước 1 file/module —
đây là quy tắc code chung, áp dụng toàn dự án.
