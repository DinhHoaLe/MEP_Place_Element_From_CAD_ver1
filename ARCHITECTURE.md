# place_element_from_cad

## Workflow

ExportCadLayers -> CreateDuctWorkflow. LoadingWindow đọc sources trên Revit UI thread, sau đó:

1. ModeWindow: equipment / duct.
2. SourceWindow: ModelSource và CadSource; lớp này không còn chọn layer.
3. EquipmentWindow hoặc DuctWindow chọn layer và cấu hình. Callback reader/analyzer do workflow cung cấp chạy đồng bộ trong modal command, trên cùng Revit UI thread; không gọi API từ worker thread.
4. Creation service -> CompletionWindow.

SourceSettings giữ model/CAD. CreationSettings giữ layer, marker/tolerance (duct), Type, Level, Offset, angle, Workset và Comments. Cache EquipmentPoint theo link/CAD/layer giữ trạng thái checkbox qua Back và pick host.

## Geometry / ownership

Resolve CAD, GraphicsStyle, Category, view và geometry trong document nguồn. Tích transform = link instance × CAD container × block. Preview dùng host XY (mm); API đổi sang feet. Z lấy từ host Level.ProjectElevation + Offset.

CadBlockReader chỉ nhận nested GeometryInstance đúng layer; preview lấy curve/polyline, tối đa 2.000 nét, không đoán từ exploded geometry. CadGeometryReader + MarkerMatcher dùng cho duct; ResultsWindow và thuật toán matcher cũ không được gọi trong workflow.

## Equipment

UiChoice có metadata category/family/placement kind để UI lọc Category -> Family -> Type. EquipmentCreationService.HostKind phân loại OneLevelBased, OneLevelBasedHosted, WorkPlaneBased và Unsupported.

EquipmentHostService pick host element / planar face trong document host. Host được giữ theo FamilyId. Khi pick, EquipmentWindow lưu settings và trả PickHostRequested; workflow pick ngoài modal rồi mở lại cửa sổ. Unsupported placement bị khóa trước khi tạo.

EquipmentCreationService dùng overload NewFamilyInstance tương ứng; xác minh vị trí và host. Face-based yêu cầu điểm trên mặt trong 0,1 mm và direction không song song normal. Không tự project xa hoặc đoán host. Workset và Comments gán trong cùng transaction; failure rollback cả lô.

EquipmentPresence tính khoảng cách 3D trên snapshot LocationPoint của thiết bị trong host; UI cập nhật theo Type/Level/Offset. Service kiểm tra lại ngay trước tạo.

## Layout / validation

ToolWindow.UseWorkspace dùng vùng nội dung co giãn trực tiếp, không đặt EquipmentWindow trong ScrollViewer toàn cột. Bảng cuộn riêng. ComboBox template dùng inset dọc 4 px để control thấp không cắt chữ.

Bộ test cục bộ (không đưa vào Git) build UI độc lập với Revit, kiểm tra step 2 chỉ chọn nguồn, layer switching, category/family cascade, host gating, Workset/Comments, Back, select/presence và layout với Advance mở.

Kiểm thử model thật vẫn cần thực hiện theo checklist cục bộ; test UI không chứng minh NewFamilyInstance/host/workset hoạt động đúng trên từng Family.
