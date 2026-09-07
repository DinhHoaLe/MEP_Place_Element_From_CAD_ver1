# Cấu trúc code — Create Duct From CAD

Mã nguồn đang sử dụng nằm trong **src/**. Mỗi class có một file riêng.
Namespace giữ nguyên CadLayerTools để không làm hỏng manifest và Add-In Manager.
Các file .cs cũ ở thư mục gốc chỉ còn lời dẫn, không được đưa vào build.

## Luồng chạy

ExportCadLayers.Execute
→ CreateDuctWorkflow.Run
→ SourceWindow
→ CadGeometryReader
→ MarkerMatcher
→ PairPreviewService / CenterlineService
→ ResultsWindow
→ DuctWindow
→ DuctCreationService
→ DuctDuplicateService / DuctDimensionService / DuctValidationService

## Sửa ở đâu?

| Thư mục / class | Trách nhiệm |
|---|---|
| Commands/ExportCadLayers | Điểm vào IExternalCommand, giữ tương thích với file .addin |
| Workflow/CreateDuctWorkflow | Điều phối các bước chọn CAD, đọc geometry, xem kết quả, tạo duct |
| Selection/CadFilter | Giới hạn đối tượng được chọn thành ImportInstance |
| Services/CadGeometryReader | Đọc Line/PolyLine, áp dụng transform, tách biên và marker trong block |
| Matching/MarkerMatcher | Ghép cặp bằng đoạn nối đầu mút; đây là thuật toán đang dùng |
| Services/CenterlineService | Tính hai điểm giữa, xử lý biên đảo chiều; dùng chung cho preview và tạo duct |
| Services/PairPreviewService | Chuyển cặp biên thành các hàng hiển thị |
| Services/DuctCreationService | Transaction tạo duct hàng loạt, không chứa giao diện |
| Services/DuctDimensionService | Gán Diameter |
| Services/DuctValidationService | Kiểm tra endpoints, length, Diameter sau khi regenerate |
| Services/DuctDuplicateService | Phát hiện tim đã có duct, không tạo chồng |
| Transactions/RollbackOnError | Hoàn tác transaction khi Revit báo lỗi |
| UI/ToolWindow | Màu sắc, styles WPF dùng chung, bố cục khung và helper input |
| UI/SourceWindow | Chọn layer, marker, dung sai |
| UI/ResultsWindow | Bảng kiểm tra và chẩn đoán |
| UI/DuctWindow | Duct Type, System Type, Level, Offset |
| Models/* | Dữ liệu hình học, cặp biên, hàng bảng, lựa chọn và kết quả tạo |

## Thuật toán cũ

Matching/BoundaryMatcher và Matching/UniqueLengthMatcher giữ lại để tham khảo và chạy regression tests.
Chúng KHÔNG được gọi trong workflow hiện tại. Projected, MatchResult hỗ trợ thuật toán cũ.
Không nối fitting, không thay đổi quy tắc nhận diện trong lần refactor này.

## Đơn vị và trách nhiệm

- V2, Boundary, Route và LengthPair: mm.
- Route.Width hiện đại diện cho Diameter khi tạo ống tròn.
- Chỉ chuyển mm sang feet tại biên gọi Revit API.
- DuctCreationService nhận các ElementId và cao độ Z bằng feet.
- UI không tạo/sửa phần tử Revit.
- Giữ helper liên quan trong cùng class; không tạo class riêng cho mỗi getter hay xử lý click.

## Build và kiểm tra

- Build.ps1: tự tìm toàn bộ src/**/*.cs, biên dịch DLL.
- Test.ps1: build, chạy các test matching và render thử ba cửa sổ WPF.
- CadLayerTools.csproj: chỉ compile src/**/*.cs; work/ không tham gia project.
- DLL: bin/Release/net48/CadLayerTools.dll.
- Bản sao trước refactor: work/before-refactor/*.txt.
- Chạy trong Revit: Add-In Manager (Manual), class CadLayerTools.ExportCadLayers.

Đã build và chạy lại các test, bao gồm render WPF.
Chưa kiểm thử tạo duct trực tiếp trong Revit sau refactor.
