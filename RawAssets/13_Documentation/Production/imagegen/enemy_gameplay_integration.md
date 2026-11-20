# Enemy gameplay integration

Sáu loại quái đã gắn sprite và bốn frame animation vào EnemyDataSO.
Prefab gameplay Enemy.prefab dùng chung, cache EnemySpriteAnimation ở Awake và
Configure bằng mảng từ data khi Initialize; không Instantiate visual khi spawn.

Mở Assets/_Game/Scenes/Stage01.unity rồi Play. Wave mở đầu màn1 gồm12 con, hai con
mỗi loại (Dơi, Đom Đóm, Bí Ngô, Ma Trơi, Ma Thư, Sứa Trăng), spawn cách0.5s.
Sau đó tiếp tục hai wave có sẵn và boss. Nếu mở MainMenu, chọn màn1 để xem bộ đủ6.

Menu Houkimajo > Apply Enemy Art + Six Enemy Showcase cập nhật lại art/prefab và
thêm wave nếu thiếu; giữ các chỉ số data hiện có, không chèn wave trùng.
StageBootstrapper cũng gọi bước này khi setup/rebuild.

Đã qua kiểm tra Unity6000.6.5f1 trong Play mode trên bản sao project:
- StageDirector/EnemySpawner sinh đủ6 loài có sprite mới và đổi frame thật.
- Pause thật dừng animation.
- Một instance trong pool lần lượt tái sử dụng cho cả6 loài, đúng frame0/frame1.
- Loại không có animation trở về sprite tĩnh, không giữ frame của loài trước.
- Animation giữ màu fade, collider và transform; chỉ số cũ không bị ghi đè.
- Gọi setup hai lần không thêm wave trùng.

Chi tiết: enemy_gameplay_validation.txt. Lỗi chỉ mục UnityEditor.Search.SearchDatabase
của editor bản sao được loại riêng theo stack; không có lỗi gameplay được ghi nhận.
Data/prefab đã kiểm tra được chép về sau khi xác nhận hash nguồn chưa đổi.
Scene Stage01 và save người chơi không bị ghi đè trong bước tích hợp.

Ba loài mới có chỉ số thử nghiệm trong docs/enemies.md. Sứa dùng đạn radial chậm;
chưa có cơ chế vùng cản đường riêng. Chưa thêm animation chết/tấn công/trúng đạn.
