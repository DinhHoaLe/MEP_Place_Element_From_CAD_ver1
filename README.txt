place_element_from_cad — Revit 2023 (mặc định)

FLOW
0. Logo CAD hiển thị khi đọc model/link/CAD.
1. Chọn chức năng: Đặt thiết bị hoặc Vẽ tuyến MEP. Hai hình minh họa AI nhúng trong DLL.
2. Chỉ chọn Model nguồn và CAD; không chọn layer ở bước này.
3. Chọn layer, xem trước và cấu hình. Đổi layer cập nhật danh sách ngay trong cửa sổ.
4. Tạo trong file hiện tại và hiển thị kết quả.
Back ở bước 2/3/4 giữ cấu hình; Back từ kết quả không undo phần tử đã tạo.

NGUỒN
Mặc định chọn file hiện tại, kể cả khi chưa có CAD. Back giữ lựa chọn nguồn trước đó.
Hỗ trợ CAD import/link trực tiếp hoặc trong Revit Link trực tiếp đang load; transform link áp vào geometry.
Chưa duyệt link lồng nhiều cấp. Dữ liệu CAD đọc từ document nguồn, phần tử được tạo trong host.

THIẾT BỊ / BƯỚC 3
- Chọn layer cùng hàng với icon chọn tất cả/bỏ chọn. Tick dòng muốn tạo.
- Category -> Family -> Type; hiện toàn bộ Category trong document, bỏ giới hạn danh mục MEP.
  Family/Type chỉ liệt kê những loại đã load. Category trống hoặc kiểu đặt chưa hỗ trợ không cho tạo.
  Chọn Level và Offset. X/Y lấy điểm chèn CAD; Z = Level + Offset. Không xoay Family theo góc CAD.
- Advance: Workset (Không có WS nếu model không workshared) và Comments.
- Preview lấy nét Curve/PolyLine thật của block, chiếu XY; dấu + là điểm chèn. Tối đa 2.000 nét/block.
- Bên cạnh symbol CAD là thumbnail Family Type từ Revit; báo rõ khi Type không có thumbnail.
- Hiện diện kiểm tra XYZ trong host, dung sai 1 mm; vị trí đã có thiết bị bị bỏ chọn, kể cả khác Type.
- Layout tự chia chiều cao, không cuộn toàn bộ hai cột; bảng nhiều dòng vẫn có scrollbar riêng.
- Một layer map tới một Family Type mỗi lần tạo. Chưa hỗ trợ ký hiệu explode/flatten, block nghiêng/phản chiếu.

FAMILY CẦN HOST
- OneLevelBased: đặt theo Level + Offset, không cần host.
- Gán Offset tương đối với Level, cập nhật Revit rồi đối chiếu Z tuyệt đối; nếu cần chỉ hiệu chỉnh Z cho Family không host. Giữ nguyên XY và hướng; vẫn kiểm tra tọa độ và Level trước khi chấp nhận.
- OneLevelBasedHosted: chọn phần tử host trong file gốc (tường/sàn/trần/mái phù hợp với Family).
- WorkPlaneBased: chọn mặt phẳng host trong file gốc.
- Nút Chọn host trong Revit đóng tạm cấu hình để pick, rồi quay lại cùng layer/cấu hình/checkbox. Esc hủy pick.
- Một host cho mỗi lần tạo; chọn các dòng CAD thuộc cùng host. Đổi Family phải chọn host tương ứng.
- Với face-based: điểm XY + Level/Offset phải nằm trên mặt đã chọn trong dung sai 0,1 mm; không tự dời điểm CAD tới host.
  Face-based dùng hướng tiếp tuyến cố định theo mặt host vì API yêu cầu; không dùng góc block CAD.
- Kiểm tra vị trí và liên kết host sau tạo; lỗi rollback cả lô và quay về cấu hình.
- Chưa hỗ trợ host trong Revit Link, mặt cong, adaptive/curve-based hoặc view-based. UI khóa Tạo cho kiểu đặt chưa hỗ trợ.

ỐNG GIÓ
Bước 3 chọn layer biên/đường nối và thông số nhận diện, rồi xem trước tuyến.
Giữ thuật toán ghép biên theo đoạn nối, tạo duct tròn, chưa nối fitting.

BUILD / TEST
.\Build.ps1
.\Test-SourceUi.ps1
.\Install.ps1 (restart Revit)
DLL Revit 2023: bin/Release/Revit2023/net48/CadLayerTools.dll
Không dùng DLL build với API 2024 trên Revit 2023: gây lỗi Method not found khi đặt Family.
Tool kiểm tra API major version trước khi chạy. Build/Install mặc định 2023.
Nếu cần 2024: .\Build.ps1 -RevitVersion 2024 (đầu ra riêng bin/Release/Revit2024/net48).
Entry point: CadLayerTools.ExportCadLayers
Test.ps1 cũ vẫn thiếu work/tests. Chưa kiểm thử tạo/host/Workset trực tiếp trên Revit model thực.

GIAO DIỆN
Phong cách FamilyWizardWindow: nền kem #F5F2EE, nút nâu #62543C, card trắng, thanh tiêu đề tùy chỉnh và checkbox đồng bộ. Giữ 4 bước và các chức năng CAD hiện có.
