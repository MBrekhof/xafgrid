using DevExpress.ExpressApp;
using XafGrid.Module.BusinessObjects;

namespace XafGrid.Module.DatabaseUpdate;

/// <summary>Deterministic Northwind-lite seed: 50 customers, 8 employees, 30 products, 1 500 orders.</summary>
public static class DemoData {
    static readonly (string Country, string[] Cities)[] Places = {
        ("Netherlands", new[] { "Amsterdam", "Rotterdam", "Utrecht", "Eindhoven" }),
        ("Germany", new[] { "Berlin", "Hamburg", "München", "Köln" }),
        ("Belgium", new[] { "Antwerpen", "Gent", "Brussel" }),
        ("France", new[] { "Paris", "Lyon", "Marseille" }),
        ("United Kingdom", new[] { "London", "Manchester", "Bristol" }),
        ("Spain", new[] { "Madrid", "Barcelona", "Sevilla" }),
        ("Italy", new[] { "Milano", "Roma", "Torino" }),
        ("Denmark", new[] { "København", "Aarhus" }),
    };
    static readonly string[] FirstNames = { "Anna", "Bram", "Carla", "Daan", "Eva", "Finn", "Greet", "Hugo", "Iris", "Jasper", "Kim", "Lars", "Mila", "Noor", "Otto", "Pien", "Quinn", "Ruben", "Sara", "Tom" };
    static readonly string[] LastNames = { "Bakker", "Jansen", "de Vries", "Visser", "Smit", "Meijer", "Mulder", "Bos", "Vos", "Peters", "Hendriks", "Dekker", "Brouwer", "Dijkstra", "Koning", "Berg", "Schmidt", "Müller", "Dubois", "Rossi" };
    static readonly string[] CompanySuffix = { "BV", "GmbH", "SA", "Ltd", "SRL", "ApS", "& Sons", "Group" };
    static readonly string[] Titles = { "Sales Representative", "Sales Manager", "Inside Sales", "Account Manager" };
    static readonly (string Category, string[] Names)[] Catalog = {
        ("Beverages", new[] { "Chai", "Chang", "Guaraná Fantástica", "Sasquatch Ale", "Steeleye Stout", "Côte de Blaye" }),
        ("Dairy", new[] { "Queso Cabrales", "Gorgonzola Telino", "Mascarpone Fabioli", "Geitost", "Raclette Courdavault", "Camembert Pierrot" }),
        ("Grains", new[] { "Gustaf's Knäckebröd", "Tunnbröd", "Singaporean Hokkien Fried Mee", "Filo Mix", "Gnocchi di nonna Alice", "Ravioli Angelo" }),
        ("Seafood", new[] { "Ikura", "Konbu", "Carnarvon Tigers", "Nord-Ost Matjeshering", "Inlagd Sill", "Gravad lax" }),
        ("Produce", new[] { "Uncle Bob's Organic Dried Pears", "Tofu", "Rössle Sauerkraut", "Manjimup Dried Apples", "Longlife Tofu", "Boston Crab Meat" }),
    };

    public static void Seed(IObjectSpace os) {
        var rnd = new Random(42);
        T Pick<T>(T[] a) => a[rnd.Next(a.Length)];

        var customers = new List<Customer>();
        for(int i = 0; i < 50; i++) {
            var place = Pick(Places);
            var c = os.CreateObject<Customer>();
            c.Name = $"{Pick(LastNames)} {Pick(CompanySuffix)}";
            c.Country = place.Country;
            c.City = Pick(place.Cities);
            c.Rating = 1 + rnd.Next(5);
            c.Since = new DateTime(2015 + rnd.Next(10), 1 + rnd.Next(12), 1 + rnd.Next(28));
            customers.Add(c);
        }

        var employees = new List<Employee>();
        for(int i = 0; i < 8; i++) {
            var e = os.CreateObject<Employee>();
            e.Name = $"{Pick(FirstNames)} {Pick(LastNames)}";
            e.Title = Pick(Titles);
            employees.Add(e);
        }

        var products = new List<Product>();
        foreach(var (category, names) in Catalog) {
            foreach(var name in names) {
                var p = os.CreateObject<Product>();
                p.Name = name;
                p.Category = category;
                p.UnitPrice = Math.Round(2 + (decimal)rnd.NextDouble() * 120, 2);
                p.Discontinued = rnd.Next(10) == 0;
                p.SortOrder = products.Count;
                products.Add(p);
            }
        }

        var today = DateTime.Today;
        for(int i = 1; i <= 1500; i++) {
            var o = os.CreateObject<Order>();
            o.Number = $"ORD-{i:00000}";
            o.Customer = customers[rnd.Next(customers.Count)];
            o.Employee = employees[rnd.Next(employees.Count)];
            o.OrderDate = today.AddDays(-rnd.Next(730)).AddHours(8 + rnd.Next(10));
            var r = rnd.NextDouble();
            o.Status = r < 0.10 ? OrderStatus.New
                     : r < 0.25 ? OrderStatus.Confirmed
                     : r < 0.45 ? OrderStatus.Shipped
                     : r < 0.92 ? OrderStatus.Delivered
                     : OrderStatus.Cancelled;
            if(o.Status is OrderStatus.Shipped or OrderStatus.Delivered)
                o.ShippedDate = o.OrderDate.AddDays(1 + rnd.Next(10));

            int lineCount = 1 + rnd.Next(6);
            decimal total = 0;
            for(int j = 0; j < lineCount; j++) {
                var p = products[rnd.Next(products.Count)];
                var l = os.CreateObject<OrderLine>();
                l.Order = o;
                l.Product = p;
                l.Quantity = 1 + rnd.Next(20);
                l.UnitPrice = p.UnitPrice;
                l.Discount = rnd.Next(4) == 0 ? 0.1m * (1 + rnd.Next(3)) : 0m;
                l.LineTotal = l.Quantity * l.UnitPrice * (1 - l.Discount);
                total += l.LineTotal;
                o.Lines.Add(l);
            }
            o.Total = total;
        }
    }
}
