using Microsoft.Build.Framework;

namespace APIBase.PublicAPIModels
{
#nullable disable

    public partial class Item
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }
        public string ItemGroupId { get; set; }
        public string Type { get; set; } //Product, Variant, Modifier
        public bool IsVariant { get; set; }
        public string VariantParentId { get; set; }
        public decimal Price { get; set; }
        public List<string> VariantLinks { get; set; }
        public List<string> ModifierLinks { get; set; }
    }

    public partial class ItemGroup
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }
    }

    public partial class DiningOption
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }
    }
}
