# Coding Rules

> Áp dụng cho toàn bộ code trong `Assets/_Game/Scripts/`. Không khớp theo module như
> các file spec khác — đây là quy tắc chung.

## Naming

- **Class, Method, Property, Public field:** `PascalCase` — `PlayerController`, `TakeDamage()`
- **Private/protected field:** `camelCase`, không prefix `_` hay `m_` — `bulletSpeed`, không
  phải `_bulletSpeed`
- **Namespace khớp tên thư mục:** code trong `Scripts/Boss/` → `namespace Houkimajo.Boss`
- Tên phải nói lên ý nghĩa, không viết tắt tùy tiện: `bulletCount` chứ không `btCnt`

## Comment

- Comment giải thích **tại sao** (lý do thiết kế, quyết định đánh đổi), không lặp lại
  điều code đã tự nói lên. Ví dụ tốt: `// Kinematic để trigger hoạt động, không cần vật lý thật`
- Viết bằng **tiếng Việt**. Code identifier (tên biến/hàm/class) luôn **tiếng Anh**.
- Mọi class/field public quan trọng nên có `/// <summary>` hoặc `[Tooltip]` (xem bên dưới)

## MonoBehaviour & Inspector

- Field cần chỉnh trong Inspector: `[SerializeField] private`, **không** để `public`
  trừ khi thật sự cần script khác truy cập trực tiếp (ưu tiên property/method thay vì
  public field trần)
- Mọi `[SerializeField]` có đơn vị/ý nghĩa không hiển nhiên → thêm `[Tooltip("...")]`
  (xem `BulletPatternSO` làm ví dụ)
- Cache reference trong `Awake()`, không `GetComponent<T>()` lặp lại trong `Update()`
- Không `GameObject.Find()` hay `FindObjectOfType()` trong `Update()`/hot path — chỉ
  gọi 1 lần lúc khởi tạo, hoặc kéo reference thủ công qua Inspector

## Hiệu năng — quan trọng vì đây là bullet-hell

- **Không `Instantiate()`/`Destroy()` đạn hay enemy lúc runtime** — luôn qua
  `BulletPool` (hoặc pool tương tự nếu tạo thêm cho enemy/effect). Đây là rule cứng,
  vi phạm sẽ gây giật lag khi nhiều đạn trên màn hình.
- Tránh LINQ (`Where`, `Select`...) trong `Update()` — gây allocation mỗi frame, dồn
  lại thành GC spike. Dùng vòng `for`/`foreach` thường.
- Tránh nối chuỗi (`+`) để build UI text trong `Update()` — chỉ update text khi giá
  trị thật sự đổi (xem cách `HUDController` lắng nghe event thay vì poll mỗi frame).

## Data-driven

- Số liệu balance game (tốc độ đạn, sát thương, HP, ngưỡng chuyển pha...) → đưa vào
  `ScriptableObject` (`BulletPatternSO`, `BossPhaseSO`...), **không hardcode** trong
  script. Muốn thêm biến thể mới = tạo asset mới, không sửa code.
- Không thêm magic number trong logic (`if (hp < 500)`) — khai báo field có tên rõ
  nghĩa hoặc đưa vào ScriptableObject.

## Kiến trúc & giao tiếp giữa các hệ thống

- Giao tiếp qua **C# event** (`Action<T>`) khi 2 hệ thống không cần biết chi tiết
  lẫn nhau — ví dụ `ScoreManager.OnScoreChanged` để UI lắng nghe, không để UI tự
  poll điểm mỗi frame.
- Singleton (`Instance`) chỉ dùng cho các manager xuyên scene đã có
  (`GameManager`, `ScoreManager`). Không tự ý thêm singleton mới cho mọi thứ —
  cân nhắc truyền reference qua Inspector trước.
- Luôn null-check field kéo từ Inspector trước khi dùng (`if (bulletPool == null) return;`)
  — tránh `NullReferenceException` làm crash khi quên gán trong Editor.

## File & tổ chức

- Mỗi file 1 class/interface, tên file trùng tên class.
- Code tự viết nằm trong `Assets/_Game/`, không lẫn vào asset bên thứ 3.
- Thay đổi kiến trúc lớn (thêm hệ thống mới, đổi cách 2 module giao tiếp) → cập nhật
  file spec tương ứng trong `docs/` trước khi code.

## Git

- Không commit `Library/`, `Temp/`, `Logs/` (đã có trong `.gitignore`).
- Commit message mô tả **thay đổi gì**, không phải "update" chung chung.
