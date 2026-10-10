CREATE DUCT FROM CAD now builds independently in ../MEP_Create_Duct_From_IFC_ver1.
This project's default build, solution and manifest expose PLACE ELEMENT FROM CAD only.
Legacy combined source remains for compatibility; it is not part of the default release.

# Place and Duct workflows

Dedicated entry points: PlaceFromCad -> EquipmentWorkflow; CreateDuctFromCad -> DuctWorkflow.
The legacy ExportCadLayers -> CreateDuctWorkflow selects a mode and delegates to either workflow.
SourceSets.json defines the dedicated compilation boundaries. Both Build.ps1 and the dedicated
.csproj files exclude the other tool's workflow/services/UI. CadLayerTools.csproj is the compatibility build.

## Placement session

SourceWindow resolves a CAD ImportInstance in the host or a direct Revit link.
EquipmentWorkflow reads CAD geometry once per source and caches block points per layer.
All document reads, placement callbacks and transaction work remain on the Revit command/UI thread.
EquipmentWindow invokes a placement callback without setting DialogResult or closing itself.
The service updates per-row progress and rejects duplicate XYZ positions within 1 mm.
Pending rows only become Placed after commit; failures rollback and reset all rows in the batch.
UI input/closing are disabled during placement. DispatcherPriority.Render updates progress visuals.
After commit, a fresh snapshot updates presence across subsequent layers. A Yes/No prompt offers the
Report step; either response returns to the same equipment configuration session.
Picking a host temporarily leaves the dialog and resumes with saved settings and cached CAD points.

## Duct session

DuctWindow uses the same persistent callback model and row/overall progress.
DuctWorkflow analyzes boundary pairs and connector segments through CadGeometryReader/MarkerMatcher.
DuctCreationService creates and validates a single transaction and checks duplicate routes.
The optional report returns to the same settings window so another layer can be processed.

## Geometry

CAD source transforms are composed with the direct Revit-link transform. Block previews preserve
CAD rotation/reflection while placement uses the insertion point, host Level.ProjectElevation and Offset.
GetSymbolGeometry uses parent x instance transform. GetInstanceGeometry already contains the
instance transform and is mapped with its parent only. Sparse symbols trigger the instance fallback.
Curves/polylines, face/solid edges and meshes are projected to XY, capped at 2,000 strokes.
CadAnnotationService optionally loads ignored local JSON from work/cad-annotations. The offline
CoreConsole exporter reads a DWG copy and exports top-level model-space INSERT attributes in mm.
Source name/hash and linked source path are checked; exact layer and unique transformed insertion
XY within 1 mm associate the labels. No coordinate match means no label is added. The service
uses the CAD-root transform, not the selected block transform, because attribute anchors are already
in drawing coordinates. Actual DWG font styles, ordinary text and nested attributes are not exported.
Revit CAD geometry may omit text/attributes; preview does not invent missing contents.
Family placement remains unrotated/unmirrored and validates the host, level and XYZ after regenerate.

## UI and validation

Dedicated tools have three steps: Source -> Configure/place or create -> optional Report.
ToolIdentity determines the correct window title from the executing assembly.
ModeArtwork embeds only the artwork needed by the selected tool.
Local tests cover continuous sessions, progress, duplicate presence, Back/host state, layout,
mirrored preview geometry and source-set boundaries. Test sources and diagnostics remain ignored.
Live creation/host/workset behavior must also be validated in Revit on representative models.