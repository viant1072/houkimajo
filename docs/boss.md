# Boss

> Khớp với `Assets/_Game/Scripts/Boss/` — namespace `Houkimajo.Boss`

## Goal

Boss là thử thách cuối mỗi stage, xuất hiện sau khi hết các wave quái. Nhiều pha, mỗi
pha một kiểu bắn khác nhau, khó dần theo HP. Hạ boss nhận phần thưởng lớn vàng + XP.

## Requirements

- Giữ kiến trúc hiện có: `BossStateMachine` + list `BossPhaseSO`, chuyển pha theo % HP
  hoặc hết giờ pha. Thêm pha = thêm asset, không sửa code.
- `BossHealth` implement `IDamageable` (xem [`bullets.md`](bullets.md)).
- Boss có `RewardData { gold, xp }` cấu hình trong Inspector — Stage01 đề xuất 300 vàng / 200 XP.
- Boss **không** có sẵn trong scene ngay từ đầu: `StageDirector` kích hoạt boss sau
  shop trước-boss (xem [`systems.md`](systems.md)).
- Boss xuất hiện ở giữa phía trên arena. Vì player giờ có thể bay khắp màn, boss nên
  di chuyển chậm (lượn qua lại) để không đứng yên làm bia — kiểu di chuyển đặt trong
  `BossPhaseSO` (`Static` / `Hover` / `FollowPlayerX`).
- Va chạm thân boss = mất 1 mạng như trúng đạn.

## Non-goals

- Không làm nhiều boss trong 1 stage.
- Không làm "spell card" có tên/hiệu ứng riêng mỗi pha (có thể làm sau).

## Open questions

- Boss có nên triệu hồi thêm quái nhỏ ở một số pha không?
