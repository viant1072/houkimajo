# Kiểm tra catalog

- Node.js kiểm tra cú pháp JavaScript thành công.
- Toàn bộ 443 đường dẫn file tồn tại; 77 ảnh và 128 vùng sprite nằm đúng trong kích thước PNG.
- Kiểm tra trình duyệt qua server localhost: tìm `q_fireball` trả đúng một ảnh,
  mở chi tiết, chọn vùng sprite, phát thử animation, chọn asset, mở danh sách chọn.
- Lựa chọn còn sau reload; xuất JSON thành công và đối chiếu đúng tên/đường dẫn asset.
- Console trình duyệt không có lỗi JavaScript trong các thao tác đã kiểm tra.
- Đã xem bố cục ở viewport desktop 1440×1000 và mobile 390×844.
- `catalog-preview.png` là ảnh chụp giao diện thật.
- Mở trực tiếp `file://` chưa được kiểm thử do browser tự động chặn giao thức này.
  Trang dùng script/data cục bộ và đường dẫn tương đối, không dùng fetch hay CDN.
- Server kiểm tra đã dừng; để sử dụng, mở `RawAssets/index.html` trong trình duyệt máy tính.
