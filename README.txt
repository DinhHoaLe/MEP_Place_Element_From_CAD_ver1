CURRENT MODE: ONE-TO-ONE LINE PAIR STATISTICS
Select CAD, layer, length tolerance, minimum length, maximum separation.
Pairs are parallel with >=80% overlap, closest separation first; each extracted segment used once.
No duct creation in this step.

Geometry exclusions:
- Arc (including circles), Ellipse and other non-Line/non-PolyLine geometry are excluded.
- The outer GeometryInstance is treated as the CAD container.
- Nested GeometryInstances are skipped with their complete contents (block filtering).
- Blocks that were exploded/flattened during import cannot be identified reliably.
- Curves converted to ordinary polyline segments cannot be identified as original circles reliably.
- Line/PolyLine boundaries inside blocks are intentionally excluded.
The CAD/model is not deleted or modified; these exclusions apply only to analysis.

Built against Revit 2024. Runtime geometry hierarchy has not been verified against the user's DWG.
