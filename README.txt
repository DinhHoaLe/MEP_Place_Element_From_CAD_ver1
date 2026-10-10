PLACE ELEMENT FROM CAD
Revit 2023 and 2024 (.NET Framework 4.8)

PLACE PROJECT AND MIGRATED DUCT TOOL
- PlaceFromCad.csproj -> PlaceFromCad.dll -> CadLayerTools.PlaceFromCad
  Places loaded Revit family types at CAD block insertion points.
  This DLL excludes the duct workflow, duct creation services and duct UI.
- CREATE DUCT FROM CAD has moved to the independent sibling project:
  D:\2_Revit\6_Working\00. Revit_API\Project\MEP_Create_Duct_From_IFC_ver1
  It has its own CreateDuctFromCad.csproj, source, assets, Build.ps1 and Install.ps1.
  Creates round ducts from CAD boundary pairs and connector segments.
  This DLL excludes the equipment workflow, equipment creation services and equipment UI.
- CadLayerTools.dll remains available for the legacy Add-In Manager entry
  CadLayerTools.ExportCadLayers. It offers the original mode selector.
  Use PlaceFromCad.dll here. Build/load CreateDuctFromCad.dll from the migrated project.

BUILD / INSTALL
.\Build.ps1 -RevitVersion 2023
.\Build.ps1 -RevitVersion 2024
Default build is Place only. -Tool All also builds Place only. -Tool Combined builds the legacy compatibility DLL.
Output: bin/Release/Revit<year>/net48/
.\Install.ps1 -RevitVersion 2023 (or 2024)
Restart Revit. This project registers PLACE ELEMENT FROM CAD only. Install the Duct tool from its new folder.
The installed manifest references PlaceFromCad.dll in this project folder.
Keep the folder in place. Do not load a DLL compiled for another Revit version.

WORKFLOW
1. Select source model and CAD import/link. Direct loaded Revit links are supported.
2. Select layer and settings, review the preview, then Place or Create.
   Progress appears in each row and in a horizontal overall bar.
   Processing remains on the Revit command/UI thread. Input and closing are blocked during creation.
   Rows remain pending until the transaction commits. A failed transaction rolls back the entire batch.
3. After success, choose whether to view the report. Yes opens the Report step.
   No keeps the settings window open. Continue/Back/Close on the report returns to the settings window.
   You can select another layer and continue without restarting the command.
   Back on settings returns to source selection. Cancel exits the tool.
   Already committed elements are kept when leaving; use Revit Undo to revert a batch.

CAD PREVIEW
Block previews use Curve/PolyLine strokes, Solid/Face edges and Mesh triangle edges.
If symbol geometry is sparse, instance geometry is checked without applying its transform twice.
The preview shows the actual geometry Revit exposes, projected onto host XY; the red + is insertion.
Nested blocks are not counted twice. Mirrored blocks are accepted; tilted/degenerate blocks are skipped.
At most 2,000 strokes are drawn per block. The preview reports symbol/instance stroke counts.
Revit may omit CAD text/attributes. Optional verified DWG attributes can supplement the preview.
Run .\scripts\Export-CadAnnotations.ps1 -DwgPath '<full DWG path>' with AutoCAD CoreConsole installed.
The exporter works on a copy and saves local JSON under ignored work/cad-annotations/.
Re-export after changing the DWG. The source file must remain accessible for hash verification.
The preview requires a matching CAD filename, unchanged DWG hash, layer and unique insertion XY
within 1 mm; linked CAD also requires the exact source path. Unmatched data is not attached.
The exporter currently assumes drawing coordinates/text height are millimetres and model-space,
top-level INSERT attributes. It does not export ordinary TEXT/MTEXT, nested attributes or font styles.
The JSON is local data, not shipped with the tool. Preview does not require running AutoCAD.
For HRDP_DSA_BD_EL_PL_16CT2_102.dwg, MEP-ELV-EQUIPMENT has eight blocks whose definition is
one rectangle plus an attribute A/B/C. AutoCAD selection grips are not part of the symbol.
Exploded symbols are not recognized as equipment blocks. Nested Revit links are not traversed.

PLACEMENT
X/Y use the transformed CAD insertion point. Z uses host Level.ProjectElevation + Offset.
Family orientation stays at its default; CAD rotation/mirroring is used for preview, not family orientation.
Category/Family/Type list loaded families; unsupported placement types cannot be placed.
OneLevelBased: level-based placement, with elevation validation.
OneLevelBasedHosted: pick a wall/floor/ceiling/roof in the host model.
WorkPlaneBased: pick a planar face in the host model. CAD points must lie on it within 0.1 mm.
Hosted placement does not automatically move the CAD insertion point onto a different face.
Linked hosts, curved faces, adaptive/curve-based and view-based families are not supported.
Workset and Comments are optional. Host/family/level/position are validated before accepting the batch.
Presence is checked at XYZ within 1 mm, including devices of another type. Existing rows are deselected.
After placement the snapshot is refreshed so later layers cannot duplicate the same position.

DUCT CREATION
Round ducts only; fittings are not connected.
Boundary/connector matching, dimension validation and duplicate detection are preserved.
Changing a layer or detection settings updates the route table immediately.

REPORT
Latest batch only: Element ID, Category, Family, Type, Level, XYZ (internal model coordinates in mm),
Workset, Mark and Comments. Routes use their start point. Export Excel writes an .xlsx workbook.

VALIDATION
Local UI/geometry/source-set tests and CAD diagnostics live in ignored tests/ and work/ folders.
They are not shipped or tracked in Git. Build/UI tests do not replace live Revit model validation.
All tool-owned labels, tooltips, validation messages and export sheet names use English.
Model/layer/family/type/workset names are displayed as supplied by the Revit model.