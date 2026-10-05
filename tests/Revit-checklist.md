# Kiểm tra trong Revit 2024

Dùng bản sao model và DWG có vị trí/góc đã biết.

- Host có CAD: nguồn mặc định là host, số CAD đúng, Type/Level đều thuộc host.
- Host không CAD, chỉ một link có CAD: tự chọn link; nhiều link có CAD thì chưa chọn nguồn.
- Cùng file RVT link hai lần ở vị trí và góc khác nhau: chọn từng instance và so sánh XY preview với CAD ở đúng instance.
- CAD import lệch gốc hoặc xoay: không áp transform hai lần; test cả import trực tiếp và qua link.
- Nhánh duct: cặp biên đã biết tạo đúng tim, đường kính và Level + Offset; chạy lại bỏ qua duct trùng.
- Thiết bị: block đơn ở 0°, 90°, block nested; chỉ chọn block trên layer mong muốn. Một block ngoài đã chọn không tạo thêm block con.
- Kiểm tra điểm chèn Family trùng điểm chèn CAD; góc bù 0°/90°; Level/Offset khác nhau; Family cần host không được liệt kê.
- Block explode/không giữ GeometryInstance, nghiêng/phản chiếu: không tạo tự suy đoán, kiểm tra chẩn đoán/không có kết quả.
- Lặp đặt cùng Type: bỏ qua vị trí trùng; hai điểm trong cùng lô trùng nhau cũng không đặt hai lần.
- Hủy ở bước 1/2/3 không thay đổi model. Lỗi tạo rollback cả lô. Một Undo khôi phục lần tạo.
- Không có cặp/block hợp lệ hoặc không có Type/Level: nút Tạo bị khóa.
- Link unloaded: không tiếp tục từ nguồn đó; linked document không bị thay đổi.

## Back, loading và thiết bị có sẵn

- Logo hiện trước lúc đọc model/link/layer; bước 1 mở sau khi đọc xong, không sửa model.
- Step 1 có hai thẻ nút, đổi lựa chọn làm đổi nền đỏ; không có Back.
- Step 2 Back về mode, Step 3 Back về source; chọn lại cùng nguồn giữ checkbox và Type/Offset.
- Step 4 Back về cấu hình giữ phần tử đã tạo; cột hiện diện cập nhật và không tạo trùng.
- Thiết bị hiện diện: cùng XYZ ±1 mm ở host; thử cùng Type/khác Type và khác Level/Offset. Thiết bị trong Revit Link không được tính vào cột host.
- Select all bỏ qua vị trí đã có; bỏ chọn tất cả khóa nút Tạo; chỉ tick một dòng thì chỉ tạo dòng đó.
- Chọn từng dòng CAD để xem đúng nét block, điểm chèn và góc model; block không có curve/polyline hiện thông báo không có preview.

## Layer, Category, Advance và host

- Bước 2 chỉ có model/CAD; bước 3 đổi layer phải đổi đúng bảng, preview và trạng thái tick theo layer.
- Category -> Family -> Type chỉ hiển thị các lựa chọn tương ứng, không giữ Type của category khác.
- Model không workshared hiển thị Không có WS; model workshared mặc định workset active, chọn WS khác phải gán đúng ELEM_PARTITION_PARAM.
- Comments phải được gán trên phần tử mới; không sửa Comments của phần tử đã có.
- Family cần host khóa Tạo khi chưa có host. Pick host xong quay lại đúng layer, Type, Offset và checkbox.
- Wall/floor/ceiling/roof-hosted: chọn host đúng loại; sai loại hoặc Revit di chuyển điểm khác CAD phải rollback, không để lại phần tử rời.
- Face-based: test mặt phẳng ngang và đứng, điểm nằm trong/ngoài biên mặt; Level/Offset sai cao độ; direction song song normal. Điểm ngoài mặt phải bị từ chối.
- Một lô nhiều block trên cùng host; đặt lần hai bỏ qua thiết bị trùng. Đổi Family yêu cầu host mới, cùng Family đổi Type có thể giữ host.
- Host thuộc Revit Link hoặc mặt cong không được pick. Adaptive/curve/view-based được báo chưa hỗ trợ.
- Hủy pick host bằng Esc giữ cấu hình. Lỗi tạo quay về cấu hình sau rollback.
