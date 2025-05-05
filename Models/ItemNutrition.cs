using APIBase.Models.Enums;
using APIBase.Models.POS;

namespace APIBase.Models.CustomModels
{
    public class ItemSummary
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }
        public NutritionFacts NutritionFacts { get; set; }
    }

    public class NutritionFacts
    {
        public CoreNutrition Core { get; set; } = new CoreNutrition();
        public Fats Fats { get; set; } = new Fats();
        public Carbs Carbs { get; set; } = new Carbs();
        public List<AllergenType> Allergens { get; set; } = new List<AllergenType>();
    }

    public class CoreNutrition
    {
        public Nutrient Calories { get; set; }
        public Nutrient Protein { get; set; }
        public Nutrient Cholesterol { get; set; }
        public Nutrient Sodium { get; set; }
        public Nutrient Salt { get; set; }
        public Nutrient Caffein { get; set; }
    }

    public class Carbs
    {
        public Nutrient Carbohydrates { get; set; }
        public Nutrient Sugar { get; set; }
        public Nutrient AddedSugar { get; set; }
        public Nutrient Fiber { get; set; }
        public Nutrient Starch { get; set; }
        public Nutrient Polyols { get; set; }
    }

    public class Fats
    {
        public Nutrient Fat { get; set; }
        public Nutrient SaturatedFat { get; set; }
        public Nutrient MonounsaturatedFat { get; set; }
        public Nutrient PolyunsaturatedFat { get; set; }
        public Nutrient TransFat { get; set; }
    }

    public class Nutrient
    {
        //public string Name { get; set; }
        public double Amount { get; set; } = 0;
        public MeasurementUnit Unit { get; set; }
    }
}