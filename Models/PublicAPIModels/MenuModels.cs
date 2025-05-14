namespace APIBase.PublicAPIModels
{
    public class MenuResponse
    {
        public string branch_id { get; set; } = string.Empty;
        public List<item> items { get; set; } = new List<item>();
        public List<category> categories { get; set; } = new List<category>();
        public List<add_on> add_ons { get; set; } = new List<add_on>();
    }
    public class category
    {
        public string Id { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string arabic_name { get; set; } = string.Empty;
        public string sort_no { get; set; } = string.Empty;
    }
    public class item
    {
        public string Id { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string arabic_name { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public string description_arabic { get; set; } = string.Empty;
        public int min_prep_time { get; set; }
        public string picture { get; set; } = string.Empty;
        public string calories { get; set; } = string.Empty;
        public decimal unit_price { get; set; }
        public string status { get; set; } = string.Empty;
        public string category_id { get; set; } = string.Empty;
        public List<string> add_ons_ids = new List<string>();
    }

    public class add_on
    {
        public string Id { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string arabic_name { get; set; } = string.Empty;
        public decimal price { get; set; }
        public string sort_no { get; set; } = string.Empty;
        public string calaries { get; set; } = string.Empty;
    }
    //public class item_variant
    //{
    //    public string Id { get; set; } = string.Empty;
    //    public string name { get; set; } = string.Empty;
    //    public string arabic_name { get; set; } = string.Empty;
    //    public decimal price { get; set; }
    //    public string sort_no { get; set; } = string.Empty;
    //    public string calaries { get; set; } = string.Empty;
    //}
}
