# Combat lighting and hit feedback

Đã gắn vào Stage01 và prefab, dùng lại sprite Q_MagicMissile_Impact trong atlas skillQ.
Không sinh thêm art hoặc chỉnh ảnh nguồn.

- PlayerBullet có child ArcaneLight với Point Light2D tím, intensity0.75, radius0.65.
  Đèn được tạo sẵn cùng đạn trong pool và tắt khi bullet GameObject về pool.
- PlayerShooting gọi PlayMuzzle khi bắn thành công: tia sao nhỏ0.055s và đèn cục bộ.
- DamageEvents.OnPlayerHit phát điểm chạm của damage hợp lệ; pool CombatFeedback
  tạo tia sao bung/fade0.18s cho BasicShot. Skills giữ VFX impact của riêng chúng.
- Quái/boss có HitFlash: overlay trắng/pink nhẹ0.07s, shader URP2D riêng chỉ đọc
  alpha của sprite. Không sửa tint/fade/animation gốc. Overlay tạo sẵn trong Awake.
- Pool48 slot, không cấp phát object khi bắn; đầy thì bỏ cosmetic. Hiệu ứng mới luôn
  có ít nhất một frame hiển thị, tránh mất flash khi FixedUpdate ở frame chậm.
- Pause/thoại/shop giữ tuổi hiệu ứng; hết thời gian hoặc disable trả/tắt toàn bộ.
  Bullet callback sau khi đã về pool bị bỏ qua để tránh damage và VFX trùng.

Source: Scripts/Systems/CombatFeedback.cs, HitFlash.cs, Art/Shaders/CombatHitFlash.shader.
Material: Assets/_Game/Art/Materials/CombatHitFlash.mat.
Menu Houkimajo > Apply Combat Lights + Hit Effects gắn/cập nhật; Ctrl+S để lưu scene.
Setup/Rebuild Stage cũng gọi builder, không tạo manager/light/flash trùng.

Chạy Stage01, bỏ qua/hoàn tất thoại và đóng shop rồi giữ chuột trái để thử.
Đã kiểm tra Play mode và render ảnh thật; chi tiết combat_feedback_validation.txt.
Ảnh so sánh: combat_feedback_before.png và combat_feedback_hit.png. Ảnh kiểm tra
zoom camera gần và giữ mục tiêu đứng yên; scene chính giữ camera/luồng game hiện có.
