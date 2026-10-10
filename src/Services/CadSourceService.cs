using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
namespace CadLayerTools
{
    internal static class CadSourceService
    {
        public static List<ModelSource> Collect(Document host)
        {
            var sources = new List<ModelSource> {
                new ModelSource { Name = host.Title + " · Current model", Cads = ReadCads(host) }
            };
            // Keep each instance: the same linked document can have multiple placements.
            foreach (var link in new FilteredElementCollector(host).OfClass(typeof(RevitLinkInstance))
                .Cast<RevitLinkInstance>().OrderBy(l => l.Name).ThenBy(l => l.Id.ToString()))
            {
                var linkedDocument = link.GetLinkDocument();
                sources.Add(new ModelSource {
                    LinkId = link.Id,
                    Name = $"{link.Name} · ID {link.Id}" + (linkedDocument == null ? " · Unloaded" : ""),
                    UnavailableReason = linkedDocument == null ? "The Revit link is unloaded. Load it in Manage Links and run the tool again." : null,
                    Cads = linkedDocument == null ? new List<CadSource>() : ReadCads(linkedDocument)
                });
            }
            return sources;
        }
        private static List<CadSource> ReadCads(Document document)
        {
            return new FilteredElementCollector(document).OfClass(typeof(ImportInstance))
                .Cast<ImportInstance>().Select(instance => {
                    var categories = new Dictionary<string, Category>();
                    foreach (var parent in new[] { instance.Category, document.GetElement(instance.GetTypeId())?.Category })
                        if (parent != null) foreach (Category category in parent.SubCategories)
                            categories[category.Id.ToString()] = category;
                    string scope = instance.ViewSpecific ? document.GetElement(instance.OwnerViewId)?.Name ?? "View" : "Model";
                    return new CadSource {
                        Id = instance.Id,
                        Name = $"{document.GetElement(instance.GetTypeId())?.Name ?? instance.Name} · {(instance.IsLinked ? "Link" : "Import")} · {scope} · ID {instance.Id}",
                        Layers = categories.Values.OrderBy(c => c.Name).Select(c => new UiChoice { Id = c.Id, Name = c.Name }).ToList()
                    };
                }).OrderBy(c => c.Name).ToList();
        }
    }
}
