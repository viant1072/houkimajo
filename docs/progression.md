# Progression

> Khớp với `Assets/_Game/Scripts/Progression/` — namespace `Houkimajo.Progression`

## Goal

Hệ thống phát triển nhân vật kiểu **RPG, lưu vĩnh viễn**: giết quái nhận XP và vàng,
lên cấp để mở khoá skill và chỉ số, dùng vàng để nâng cấp skill và mua trang bị. Tiến
trình được lưu xuống file, thoát game vào lại vẫn còn.

Nguyên tắc tách bạch: **XP → mở khoá / lựa chọn**, **Vàng → sức mạnh** (nâng cấp skill,
mua đồ). Như vậy không có chuyện tiền mua được thứ chỉ level mới mở.

## Requirements

### Shop hai lưới và nâng cấp Q/E/R

- Thay hai tab Cửa hàng/Túi đồ bằng hai bảng cùng hiện, lọc theo Vật phẩm/Trang bị/Nâng cấp.
  Trang bị chưa có dữ liệu; không tạo item asset. Bình HP được bán từng đơn vị với 50% giá mua.
- Nâng cấp Q/E/R đang lắp bằng vàng, lấy `upgradeCost` từ level kế; Lv5 hoặc thiếu vàng
  không trừ tiền. Chỉ tăng cấp instance/loadout, không sửa SkillSO gốc. Giữ lượt dùng,
  tỷ lệ hồi chiêu và năng lượng R; nếu tăng số lượt tối đa thì lượt mới phải hồi bình thường.
- Save v3 thêm danh sách id skill/cấp đã mua. Giữ toàn bộ tiền/túi/cờ seed từ v1/v2;
  tải lại stage hoặc đổi sang skill đã mua cấp trước đó sẽ áp dụng cấp đã lưu.
  Chưa làm mở khóa skill, nâng cấp SPACE hoặc trang bị.

### Prototype Bình HP — mốc đầu đã triển khai

- Chỉ một loại Bình HP: giá 60 vàng, hồi 40 HP, stack tối đa 9, cooldown dùng 2 giây.
  Cấu hình trực tiếp trên component prototype trong scene, chưa tạo ItemSO/icon/item asset.
- Shop mở lúc đầu stage để thử nhanh; phím B mở/đóng khi Playing/Shop, Escape đóng shop.
  GameState.Shop dừng gameplay, chuột và UGUI vẫn thao tác được. Checkpoint tự động
  giữa wave/trước boss, catalog tổng quát, equipment và các item khác để mốc sau.
- Hai bảng Cửa hàng/Túi đồ cùng hiện, Mua/Bán/Dùng 1, đóng bằng X/B/Escape; phím 1 ở HUD.
  HP đầy/đã chết/hết bình/đang cooldown thì không tiêu hao. Hồi kẹp max HP, không hồi sinh.
- Seed tài khoản thử lên ít nhất 10.000 vàng **một lần**, lưu cờ seed; không cấp lại
  khi mở shop/đổi stage/tải lại. Cấu hình bật/tắt và số vàng thử trên ShopController.
- Save v2 tối thiểu thêm số Bình HP, cooldown và cờ seed, giữ vàng/màn của save v1.
  Lưu ngay sau mua/dùng/đóng shop và khi cooldown kết thúc. Inventory tổng quát/ô nhanh
  ba loại/equipment migration chưa thuộc prototype. UI dùng màu/chữ tạm.

### Kế hoạch triển khai shop/items — 17/11/2026

**Trạng thái:** prototype một Bình HP phía trên đã triển khai; inventory tổng quát,
trang bị và danh mục mở rộng dưới đây vẫn là kế hoạch. Chưa tạo icon, model, prefab hay file
`.asset` cho item. Các số dưới đây là đề xuất balance, sẽ chỉnh trong Inspector
khi triển khai; không phải chỉ số đã được chốt cho gameplay.

**Nền tảng hiện có:** player 100 HP/100 MP, mất 20 HP/hit, hồi 5 MP/giây;
Q tốn 15 MP, E tốn 20, R dùng năng lượng riêng. Wallet đã có `TrySpend`;
`PlayerHealth.Heal` hồi HP không vượt max và không hồi sinh. Save v3 giữ Bình HP,
cooldown, cờ seed thử và cấp kỹ năng đã mua. `GameState.Shop` pause gameplay; shop mở đầu stage/B.
`openShopAfter` chưa được dùng. Chưa có inventory tổng quát, equipment, PlayerStats,
API hồi MP hay phân loại sát thương phép/vật lý. Không coi danh mục mở rộng là code đã có.

#### A. HUD cửa hàng và túi đồ

- Giữ tông tím mực/vàng cổ của HUD hiện tại; panel giữa màn, lớp nền tối phía sau.
- Header: tên cửa hàng, vàng hiện có, HP/MP hiện tại. Không tạo artwork trong đợt này;
  prototype dùng ô màu + chữ viết tắt và các thành phần UI sẵn có.
- Bố cục theo wireframe mới: ba nút trái **Vật phẩm / Trang bị / Nâng cấp** và hai bảng **Cửa hàng / Túi đồ** cùng hiện.
  Q/E/R nâng cấp bằng vàng đã có; thẻ XP và mở khóa skill là giai đoạn sau.
- Bên trái: lưới vật phẩm, tên, giá, số lượng đang có; chọn một ô để xem chi tiết.
  Bên phải: mô tả, tác dụng, ô trang bị, so sánh trước/sau và hành động phù hợp
  **Mua 1 / Dùng 1 / Trang bị / Tháo / Bán 1**. Không tự trang bị hoặc dùng ngay khi mua.
- Footer: Mua bên cửa hàng, Bán/Dùng bên túi; X/B/Escape tiếp tục trận. Nút mua bị khóa khi thiếu vàng,
  hết chỗ, đã sở hữu trang bị duy nhất; hiển thị lý do cụ thể.
- Trong gameplay, thêm ba ô item nhỏ riêng gần nhóm skill, phím **1/2/3**; mỗi ô
  có số lượng và hồi dùng chung. Không dùng ô pet làm ô item, không đổi phím Q/E/R.
  UI prototype cần kiểm tra khoảng cách với avatar và HP/MP của HUD thu gọn.

```text
┌ CỬA HÀNG                 Vàng: …       HP: …   MP: … ┐
│ [Cửa hàng] [Túi đồ]                                  │
│ [Tất cả] [Tiêu hao] [Trang bị]                        │
│ ┌ Danh sách item ──────────┐ ┌ Chi tiết đã chọn ─────┐│
│ │ Tên · Giá · Số đang có   │ │ Hiệu ứng / So sánh    ││
│ │ Ô màu/chữ tạm           │ │ [Mua 1]              ││
│ └─────────────────────────┘ └───────────────────────┘│
│ Chổi / Mũ / Bùa        Item nhanh 1 · 2 · 3 [Tiếp tục]│
└─────────────────────────────────────────────────────┘
```

#### B. Danh mục khởi đầu đề xuất

| Loại | Vật phẩm | Hiệu ứng đề xuất | Giá vàng |
|---|---|---|---:|
| Tiêu hao | Bình HP | Hồi ngay 40 HP, không vượt tối đa | 60 |
| Tiêu hao | Bình MP | Hồi ngay 50 MP, không vượt tối đa | 45 |
| Tiêu hao | Bình Sinh Lực | Hồi 30 HP và 30 MP | 90 |
| Tiêu hao | Dược Tấn Công | +20% sát thương thường/Q/E/R trong 10 giây gameplay | 100 |
| Tiêu hao | Dược Kháng Phép | +25 điểm phần trăm kháng phép trong 10 giây gameplay | 100 |
| Chổi | Chổi Gỗ Sồi | +10% tốc độ bay/focus | 150 |
| Chổi | Chổi Lông Phượng | -25% hồi chiêu E | 400 |
| Mũ | Mũ Sinh Lực | +20 HP tối đa | 180 |
| Mũ | Mũ Pháp Lực | +25 MP tối đa | 180 |
| Mũ | Mũ Phù Thủy Cổ | -15% hồi chiêu Q/E, không giảm yêu cầu năng lượng R | 450 |
| Bùa | Bùa Kháng Phép | +20 điểm phần trăm kháng phép | 250 |
| Bùa | Bùa Tấn Công | +15% sát thương thường/Q/E/R | 250 |

- Ưu tiên triển khai Bình HP trước, Bình MP kế tiếp; các item còn lại dùng cùng
  nền tảng dữ liệu/effect sau khi vòng mua/dùng/lưu đã ổn.
- Mỗi ô Chổi/Mũ/Bùa chỉ lắp một món: chọn kháng phép hoặc tấn công ở ô Bùa.
  Trang bị cố định, không random chỉ số; một item trang bị chỉ sở hữu một bản.
- Tiêu hao: tối đa 3 loại trong túi, mỗi loại stack tối đa 9; mua mới một loại
  khi đã đầy 3 loại bị chặn. Ba ô nhanh tham chiếu ba loại trong túi, không nhân bản item.
- Bán lại một đơn vị = floor(50% giá mua); chưa bán trực tiếp món đang trang bị,
  cần tháo trước. Shop đầu tiên có hàng cố định, chưa xoay hàng/random stock.

#### C. Luồng mua và sử dụng một item

1. Vào shop tại checkpoint an toàn. Chọn **Bình HP**, xem tác dụng và giá.
2. **Mua 1**: kiểm tra shop đang mở, id nằm trong catalog, giá hợp lệ, đủ vàng,
   túi/stack còn chỗ. Chỉ khi mọi điều kiện đúng mới trừ vàng và thêm đúng 1 bình.
   Một click là một giao dịch; chặn click lặp trong lúc đang xử lý.
3. Cập nhật vàng/số lượng bằng event và lưu snapshot vàng + inventory cùng nhau.
   Mua thất bại không thay đổi tiền/túi; không để UI quyết định giá hay hiệu ứng.
4. Gán vào ô nhanh trống đầu tiên; cho đổi thứ tự 1/2/3 trong túi đồ tại shop.
5. **Dùng 1** ở shop hoặc nhấn phím ô nhanh trong `Playing`: kiểm tra còn item,
   player còn sống, trạng thái cho phép, cooldown và hiệu ứng có tác dụng.
6. Ví dụ HP 35/100 + bình 40 → 75/100, giảm stack 1; HP 85 → 100 và vẫn tốn 1 bình.
   Nếu đã đầy HP thì không dùng, không mất bình và không bắt đầu cooldown.
7. HUD HP/MP, số lượng và cooldown cập nhật; lưu lượng tiêu hao còn lại ngay sau dùng.
   Hết stack thì ô trống; không tự nhét loại khác vào phím đang giữ.

- Phím 1/2/3 dùng theo cạnh nhấn, không lặp liên tục khi giữ. Tiêu hao dùng chung
  cooldown **2 giây**: dùng delta gameplay khi Playing, unscaled delta khi Shop để
  vẫn dùng được nhiều bình tại checkpoint; pause khác/GameOver giữ nguyên thời gian.
  Nút **Dùng 1** trong shop chịu cùng timer, không reset khi mở/đóng shop.
- Item hồi kép chỉ dùng khi ít nhất một tài nguyên còn thiếu; tài nguyên còn lại kẹp max.
- Buff cùng id không cộng dồn, dùng lại khi buff còn hiệu lực bị chặn để tránh phí đồ.
  Buff khác id cộng theo hệ thống stats và giới hạn chung; tooltip cho biết thời gian còn lại.
- GameOver/Victory/menu không nhận phím dùng item; việc dùng không kích hoạt skill/bắn thường.

#### D. Kháng phép, sát thương và chỉ số

- Bổ sung loại sát thương **Magic / Physical**: đề xuất đạn địch/boss hiện tại là
  Magic, chạm thân quái là Physical. Thêm metadata vào cấu hình; không suy từ màu sprite.
- Phân biệt `DamageSource` (thường/skill/ultimate, dùng tính năng lượng) với loại
  sát thương; không đổi ý nghĩa enum nguồn đang dùng.
- Kháng phép chỉ giảm Magic: `damage = max(1, ceil(rawDamage * (1 - magicResistance)))`.
  Ví dụ 20 damage với 20% kháng → mất 16 HP; Physical vẫn mất 20 HP.
- Bất tử bỏ qua damage trước; khiên vẫn chặn một hit trước khi trừ HP. Resistance
  không làm giảm số lượt khiên, không thay lượng hồi HP/MP và không giảm chi phí MP.
- Kháng phép và giảm cooldown cộng theo điểm phần trăm, kẹp tối đa 60%.
  Không cho hồi chiêu/nhịp bắn về 0. R không dùng giảm cooldown vì đang tích năng lượng.
- `PlayerStats` tính lại từ base + equipment + buff khi có thay đổi, không sửa SO
  gốc và không cộng bonus lần nữa khi mở shop/tải lại. Đạn lấy sát thương khi được
  phóng, vùng DoT khi tạo; tooltip/bảng chỉ số đọc cùng nguồn với gameplay.
- Khi max HP/MP tăng do đổi đồ, giữ nguyên số hiện tại (không tặng hồi miễn phí);
  khi max giảm thì clamp. Bắt đầu stage mới vẫn đầy theo max sau khi tính trang bị.
- Buff dùng thời gian gameplay, hết khi chết/chuyển stage; không lưu buff tạm.
  Trang bị và item còn lại giữ qua chết/chuyển stage; bình đã dùng không được hoàn lại.

#### E. Phần cần xây dựng và thứ tự làm

| Mốc | Công việc | Điều kiện hoàn thành |
|---|---|---|
| 1 — Hợp đồng dữ liệu | ItemSO, effect/modifier, ShopCatalogSO, Inventory, id/stack/slot; thêm Wallet.TrySpend và Heal/RestoreMana | Có thể kiểm tra dữ liệu không cần icon; không lẫn state runtime vào SO |
| 2 — Một vòng Bình HP | Shop UI bằng ô màu/chữ, giao dịch mua, túi/ô nhanh, dùng một bình và phản hồi lỗi | Mua → dùng → HUD cập nhật; thiếu tiền/túi đầy/HP đầy không mất gì |
| 3 — Save và checkpoint | Migrate save v1→v2, lưu inventory/equipment/quick slots/cooldown cần thiết; mở shop theo StageDirector | Save cũ giữ vàng/màn; reload không nhân đôi đồ/vàng; đóng shop đúng wave/boss |
| 4 — Bình MP và trang bị | MP/hồi kép, PlayerStats, lắp/tháo/bán, modifiers tấn công/HP/MP/tốc độ/CDR | Đổi đồ thay chỉ số thật, không hồi máu miễn phí, không cộng dồn lại khi reopen |
| 5 — Kháng phép và buff | Damage type, resistance, buff tạm, hiệu lực/cooldown HUD | Magic/Physical nhận damage đúng; buff hết hạn/pause/chuyển stage đúng |
| 6 — Cân chỉnh và hoàn thiện UI | So sánh đồ, đọc phím, safe area, khóa nút/lý do lỗi, giá và nhịp mở shop | Chơi thử ba màn, kiểm tra cả chuột và bàn phím; sau đó mới thiết kế asset item |

- Các component đề xuất dưới `Scripts/Progression/`: `Inventory`, `Equipment`,
  `PlayerStats`, `ItemUseController`, `ShopController`, cùng định nghĩa SO.
  `ShopUI` dưới UI. Các item dùng chung pipeline, không viết một MonoBehaviour riêng
  cho từng bình. Kiểm tra id trùng/mất effect/giá âm khi authoring.
- Checkpoint: sau wave có `openShopAfter`, và trước boss; không mở hai lần nếu
  wave cuối đã mở shop. Dọn đạn, gom pickup về Wallet trước khi hiển thị số vàng.
  Shop phát đóng/mở qua event; director chờ đóng rồi mới tiếp tục intro/spawn.
- Dùng UGUI + EventSystem/InputSystemUIInputModule trong scene gameplay; UI hoạt động
  bằng thời gian unscaled khi `GameState.Shop` đặt timeScale=0. Gameplay input bị chặn.
  Không cho dùng phím debug đổi skill/nạp R trong shop; vào lại `Playing` mới nhận.
- Save mở rộng tương lai giữ id + số lượng/trang bị/ô nhanh; dữ liệu thiếu hoặc item id đã xóa được
  bỏ qua an toàn. Không làm mất trường vàng/màn khi save ở StageDirector. Ghi qua
  file tạm rồi thay file chính; giao dịch đã dùng item phải lưu số lượng còn lại.
  Cooldown dùng item không được reset nhờ reload; giữ thời gian còn lại theo gameplay.
  Không lưu HP/MP giữa wave ở mốc này, vẫn reset đầy khi tải stage như hiện tại.
- XP/thẻ lên cấp, mở khóa/nâng cấp skill và pet chưa cần để shop item đầu tiên chạy.

#### F. Checklist kiểm tra khi triển khai

- Mua thành công: vàng giảm đúng giá, stack tăng 1; click lặp không nhân đôi giao dịch.
- Thiếu vàng, túi đầy, stack đầy, catalog/id sai: tiền/túi không đổi.
- Dùng HP/MP ở mức thiếu, gần đầy, đầy, bằng 0; clamp và callback/HUD chính xác.
- Dùng hết bình, giữ phím, nhấn hai phím cùng frame, dùng lúc cooldown/pause/dead/shop.
- Lắp/tháo/đổi/bán đồ; base stats không đổi, tính lại không cộng dồn sai, max HP clamp.
- Kháng phép với đạn Magic, va chạm Physical, khiên và bất tử; kết hợp buff không vượt cap.
- Migrate save cũ, thoát sau mua/dùng, qua màn/chết/chơi tiếp; đồ tiêu hao không sống lại.
- Mở/đóng shop không tăng tốc spawn, không phát đạn hay cast skill ngoài ý muốn;
  UI không bị đứng theo timeScale, HUD nhỏ vẫn đọc được số lượng/cooldown.

### Cấp độ (XP)

- `PlayerLevel`: giữ `level`, `xp`. XP cần lên cấp tiếp theo:
  `xpToNext = 50 * level^1.5` (làm tròn) — công thức nằm trong `ProgressionConfigSO`.
  Cấp tối đa: 30.
- Lên cấp **giữa trận** chỉ hiện thông báo "LEVEL UP!" — **không pause game**, vì
  chặn màn hình lúc né đạn sẽ giết người chơi oan. Lựa chọn được dồn lại và xử lý ở
  màn shop.
- Mỗi lần lên cấp nhận 1 **thẻ lựa chọn**: hiện 3 thẻ ngẫu nhiên, chọn 1:
  - Mở khoá 1 skill mới (trong các ô đã mở)
  - Hoặc 1 chỉ số bị động: +5% sát thương, +5% tốc độ bay, -4% hồi chiêu, +10% bán
    kính hút, +10% vàng nhận được...
- Mốc mở ô: E ở Lv3, R ở Lv6 (lên tới mốc → thẻ đầu tiên chắc chắn là chọn skill cho ô đó).

### Vàng

- `Wallet`: giữ `gold`, phát event `OnGoldChanged`. Mọi thao tác cộng/trừ đi qua
  `Wallet`, có kiểm tra đủ tiền (`TrySpend`).
- Nguồn vàng: quái (xem `enemies.md`), boss, đạn bị skill Hoá Giải xoá.

### Trang bị (Items)

- 3 ô trang bị: **Chổi**, **Mũ**, **Bùa** + túi **đồ tiêu hao** (tối đa 3 loại).
- `ItemSO` (Create → Houkimajo → Item): `id`, `displayName`, `icon`, `slot`, `price`,
  `description`, `modifiers[]` — `StatModifier { stat, type (Flat/Percent), value }`.
- Đồ tiêu hao dùng chủ động bằng nút trong shop hoặc ô nhanh 1/2/3 trong gameplay;
  mất một đơn vị khi dùng thành công, theo kế hoạch shop/items ở trên.
- Bán lại đồ = 50% giá mua.

Các món dưới đây là ý tưởng mở rộng sau danh mục khởi đầu, chưa triển khai:

| Ô | Item | Hiệu ứng | Giá đề xuất |
|---|---|---|---|
| Mũ | Mũ Sao Đêm | +20% sát thương skill | 400 |
| Bùa | Bùa Hộ Mệnh | +20 HP tối đa | 500 |
| Bùa | Kính Tinh Tú | +50% bán kính graze, +20% vàng nhận | 350 |
| Tiêu hao | Cuộn Phép | Nạp ngay 50 năng lượng R khi dùng, chặn khi đã đầy | 120 |

### Chỉ số nhân vật

- `PlayerStats` gộp: chỉ số gốc (`PlayerBaseStatsSO`) + trang bị + thẻ bị động.
  Các chỉ số: `moveSpeed`, `focusSpeed`, `fireRate`, `bulletDamage`, `skillDamage%`,
  `cooldownReduction%`, `maxHP`, `maxMP`, `manaRegen`, `magicResistance`, `invincibilityDuration`, `grazeRadius`,
  `magnetRadius`, `goldGain%`.
- Tính lại khi trang bị thay đổi, phát `OnStatsChanged` — **không** tính lại mỗi frame.

### Shop

- Mở **sau mỗi wave có cờ `openShopAfter`** và **luôn mở trước boss** (xem `systems.md`).
  Game dừng hẳn (`GameState.Shop`) trong lúc ở shop.
- Ba loại ở cột trái: **Vật phẩm / Trang bị / Nâng cấp**, hai bảng Cửa hàng/Túi đồ cùng hiện. Q/E/R đang lắp đã nâng cấp bằng vàng được; thẻ XP và mở khóa skill làm sau. X/B/Escape tiếp tục trận.
- Danh sách đồ bán lấy từ `ShopCatalogSO` (data-driven).

### Lưu game

- `SaveSystem` ghi JSON vào `Application.persistentDataPath/save.json`:
  level, xp, gold, skill đã mở + level từng skill, skill lắp ở Q/E/R, đồ đã sở hữu,
  đồ đang mặc, đồ tiêu hao, chỉ số bị động đã chọn, stage cao nhất đã qua.
- Lưu khi: rời shop, thắng boss, thua (game over). **Không lưu giữa wave.**
- Lưu theo `id` (string) của SO, không theo tên file — đổi tên asset không làm hỏng save.
- Có version trong file save để sau này đổi cấu trúc còn migrate được.
- **Thua**: giữ toàn bộ XP và vàng đã nhặt, chơi lại stage từ wave 1 (kiểu RPG, không
  phạt nặng).

## Non-goals

- Không làm nhiều slot save / nhiều nhân vật.
- Không có độ hiếm (rarity) hay chỉ số ngẫu nhiên trên đồ — mỗi item cố định.
- Không làm chế tạo, ghép đồ, giao dịch.
- Không mã hoá file save (chơi offline, sửa save thì kệ).

## Open questions

- Thua có nên mất một phần vàng (vd. 20%) để có chút áp lực không? Hiện tại: không mất.
- Có cần nút "reset tiến trình" trong menu không?
- Khi có nhiều stage: cho chọn chơi lại stage cũ để cày vàng/XP không?
