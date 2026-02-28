// Services/DatabaseSeeder.cs
using Microsoft.EntityFrameworkCore;
using SamarStoneQwen.Data;
using SamarStoneQwen.Models;

namespace SamarStoneQwen.Services
{
    public class DatabaseSeeder
    {
        private readonly ApplicationDbContext _context;

        public DatabaseSeeder(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SeedDatabaseAsync()
        {
            // Ensure the database is created
            await _context.Database.EnsureCreatedAsync();

            // Check if any data already exists to avoid re-seeding
            if (await _context.PurchaseOrders.AnyAsync())
            {
                return; // Data already exists, exit early
            }

            // Step 1: Seed Suppliers
            var suppliers = new List<Supplier>
            {
                new Supplier
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Italian Marble Importers",
                    ContactPerson = "Marco Rossi",
                    Email = "marco@italianmarble.it",
                    Phone = "+39 02 12345678",
                    Address = "Via del Marmo 123",
                    City = "Carrara",
                    Country = "Italy",
                    TaxNumber = "IT01234567890",
                    Notes = "Premium Italian marble supplier specializing in Carrara marble",
                    IsActive = true,
                    CreatedDate = DateTime.Now
                },
                new Supplier
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Spanish Stone Group",
                    ContactPerson = "Ana Garcia",
                    Email = "ana@spanishstone.es",
                    Phone = "+34 91 98765432",
                    Address = "Calle Piedra 456",
                    City = "Barcelona",
                    Country = "Spain",
                    TaxNumber = "ESB12345678",
                    Notes = "Spanish marble and granite supplier",
                    IsActive = true,
                    CreatedDate = DateTime.Now
                },
                new Supplier
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Turkish Natural Stone",
                    ContactPerson = "Ahmet Yilmaz",
                    Email = "ahmet@turkishstone.tr",
                    Phone = "+90 212 3456789",
                    Address = "Mermer Sokak 789",
                    City = "Afyonkarahisar",
                    Country = "Turkey",
                    TaxNumber = "TR12345678901",
                    Notes = "Turkish natural stone supplier",
                    IsActive = true,
                    CreatedDate = DateTime.Now
                }
            };

            await _context.Suppliers.AddRangeAsync(suppliers);
            await _context.SaveChangesAsync(); // Save suppliers and get their IDs

            // Step 2: Seed Marble Types
            var marbleTypes = new List<MarbleType>
            {
                new MarbleType
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Carrara White",
                    Description = "Classic white marble from Carrara, Italy",
                    Color = "White",
                    OriginCountry = "Italy",
                    DefaultCostPerSqM = 65.00m,
                    QualityGrade = "A",
                    IsActive = true,
                    CreatedDate = DateTime.Now
                },
                new MarbleType
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Calacatta Gold",
                    Description = "Luxury white marble with golden veins",
                    Color = "White with Golden Veins",
                    OriginCountry = "Italy",
                    DefaultCostPerSqM = 120.00m,
                    QualityGrade = "A",
                    IsActive = true,
                    CreatedDate = DateTime.Now
                },
                new MarbleType
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Emperador Light",
                    Description = "Warm brown marble from Spain",
                    Color = "Beige/Brown",
                    OriginCountry = "Spain",
                    DefaultCostPerSqM = 45.00m,
                    QualityGrade = "B",
                    IsActive = true,
                    CreatedDate = DateTime.Now
                },
                new MarbleType
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Bianco Perlino",
                    Description = "Creamy white marble with subtle veining",
                    Color = "Cream/White",
                    OriginCountry = "Turkey",
                    DefaultCostPerSqM = 55.00m,
                    QualityGrade = "A",
                    IsActive = true,
                    CreatedDate = DateTime.Now
                },
                new MarbleType
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Travertino Classic",
                    Description = "Classic travertine with warm tones",
                    Color = "Tan/Beige",
                    OriginCountry = "Italy",
                    DefaultCostPerSqM = 40.00m,
                    QualityGrade = "B",
                    IsActive = true,
                    CreatedDate = DateTime.Now
                }
            };

            await _context.MarbleTypes.AddRangeAsync(marbleTypes);
            await _context.SaveChangesAsync(); // Save marble types

            // Step 3: Seed Customers
            var customers = new List<Customer>
            {
                new Customer
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Grand Hotel Alexandria",
                    Email = "info@grandhotelalex.com",
                    Phone = "+20 3 456 7890",
                    Address = "Corniche El Nil",
                    City = "Alexandria",
                    Country = "Egypt",
                    TaxNumber = "EG123456789",
                    Notes = "Luxury hotel chain requiring premium marble",
                    IsActive = true,
                    CreatedDate = DateTime.Now
                },
                new Customer
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Modern Architecture Co.",
                    Email = "contact@modernarch.eg",
                    Phone = "+20 2 345 6789",
                    Address = "Nasr City",
                    City = "Cairo",
                    Country = "Egypt",
                    TaxNumber = "EG987654321",
                    Notes = "Architectural firm specializing in modern designs",
                    IsActive = true,
                    CreatedDate = DateTime.Now
                },
                new Customer
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Royal Palace Resort",
                    Email = "reservations@royalpalace.com",
                    Phone = "+20 10 123 4567",
                    Address = "Red Sea Coast",
                    City = "Hurghada",
                    Country = "Egypt",
                    TaxNumber = "EG456789123",
                    Notes = "Luxury resort requiring high-end materials",
                    IsActive = true,
                    CreatedDate = DateTime.Now
                }
            };

            await _context.Customers.AddRangeAsync(customers);
            await _context.SaveChangesAsync(); // Save customers

            // Step 4: Seed Purchase Orders
            var purchaseOrders = new List<PurchaseOrder>
            {
                new PurchaseOrder
                {
                    Id = Guid.NewGuid().ToString(),
                    OrderNumber = "PO-2026-001",
                    Supplier = suppliers[0], // Use the saved supplier ID
                    OrderDate = DateTime.Now.AddDays(-30),
                    TotalAmountUSD = 45000m,
            
                    Currency = "USD",
                    Status = "Delivered",
                    ExpectedDeliveryDate = DateTime.Now.AddDays(-15),
                    Notes = "First shipment of premium Carrara marble",
                    CreatedDate = DateTime.Now.AddDays(-30)
                },
                new PurchaseOrder
                {
                    Id = Guid.NewGuid().ToString(),
                    OrderNumber = "PO-2026-002",
                    Supplier = suppliers[1], // Use the saved supplier ID
                    OrderDate = DateTime.Now.AddDays(-20),
                    TotalAmountUSD = 28000m,
                
                    Currency = "USD",
                    Status = "Paid",
                    ExpectedDeliveryDate = DateTime.Now.AddDays(-5),
                    Notes = "Emperador Light marble shipment",
                    CreatedDate = DateTime.Now.AddDays(-20)
                },
                new PurchaseOrder
                {
                    Id = Guid.NewGuid().ToString(),
                    OrderNumber = "PO-2026-003",
                    Supplier = suppliers[2], // Use the saved supplier ID
                    OrderDate = DateTime.Now.AddDays(-10),
                    TotalAmountUSD = 35000m,
              
                    Currency = "USD",
                    Status = "PartiallyPaid",
                    ExpectedDeliveryDate = DateTime.Now.AddDays(10),
                    Notes = "Bianco Perlino shipment",
                    CreatedDate = DateTime.Now.AddDays(-10)
                }
            };

            await _context.PurchaseOrders.AddRangeAsync(purchaseOrders);
            await _context.SaveChangesAsync(); // Save purchase orders

            // Step 5: Seed Containers
            var containers = new List<Container>
            {
                new Container
                {
                    Id = Guid.NewGuid().ToString(),
                    ContainerNumber = "MSCU1234567",
                    PurchaseOrder = purchaseOrders[0], // Use the saved purchase order ID
                    SizeInFeet = 40,
                    WeightInTons = 22.5m,
                    Status = "Unloaded",
                    ArrivalDate = DateTime.Now.AddDays(-18),
                    Notes = "Standard 40ft container with premium marble",
                    CreatedDate = DateTime.Now.AddDays(-30)
                },
                new Container
                {
                    Id = Guid.NewGuid().ToString(),
                    ContainerNumber = "MSCU7654321",
                    PurchaseOrder = purchaseOrders[1], // Use the saved purchase order ID
                    SizeInFeet = 20,
                    WeightInTons = 15.2m,
                    Status = "Arrived",
                    ArrivalDate = DateTime.Now.AddDays(-7),
                    Notes = "20ft container with Emperador Light",
                    CreatedDate = DateTime.Now.AddDays(-20)
                },
                new Container
                {
                    Id = Guid.NewGuid().ToString(),
                    ContainerNumber = "TCLU9876543",
                    PurchaseOrder = purchaseOrders[2], // Use the saved purchase order ID
                    SizeInFeet = 40,
                    WeightInTons = 24.8m,
                    Status = "Shipped",
                    ArrivalDate = null,
                    Notes = "Bianco Perlino shipment in transit",
                    CreatedDate = DateTime.Now.AddDays(-10)
                }
            };

            await _context.Containers.AddRangeAsync(containers);
            await _context.SaveChangesAsync(); // Save containers

            // Step 6: Seed Slabs
            var slabs = new List<Slab>
            {
                // Slabs for first container (Carrara White)
                new Slab
                {
                    Id = Guid.NewGuid().ToString(),
                    Container = containers[0], // Use the saved container ID
                    MarbleTypeNavigation = marbleTypes[0], // Use the saved marble type ID
                    SlabNumber = "SLB-CW-001",
                    Thickness = 2.0m,
                    Width = 120.0m,
                    Length = 240.0m,
                    CostPerSqM = 65.00m,
                    Status = "Available",
                    CreatedDate = DateTime.Now.AddDays(-25)
                },
                new Slab
                {
                    Id = Guid.NewGuid().ToString(),
                    Container = containers[0], // Use the saved container ID
                    MarbleTypeNavigation = marbleTypes[0], // Use the saved marble type ID
                    SlabNumber = "SLB-CW-002",
                    Thickness = 2.0m,
                    Width = 115.0m,
                    Length = 235.0m,
                    CostPerSqM = 65.00m,
                    Status = "Available",
                    CreatedDate = DateTime.Now.AddDays(-25)
                },
                new Slab
                {
                    Id = Guid.NewGuid().ToString(),
                    Container = containers[0], // Use the saved container ID
                    MarbleTypeNavigation = marbleTypes[0], // Use the saved marble type ID
                    SlabNumber = "SLB-CW-003",
                    Thickness = 2.0m,
                    Width = 130.0m,
                    Length = 250.0m,
                    CostPerSqM = 65.00m,
                    Status = "Sold",
                    CreatedDate = DateTime.Now.AddDays(-25)
                },
                // Slabs for second container (Emperador Light)
                new Slab
                {
                    Id = Guid.NewGuid().ToString(),
                    Container = containers[1], // Use the saved container ID
                    MarbleTypeNavigation = marbleTypes[2], // Use the saved marble type ID
                    SlabNumber = "SLB-EL-001",
                    Thickness = 2.0m,
                    Width = 125.0m,
                    Length = 245.0m,
                    CostPerSqM = 45.00m,
                    Status = "Available",
                    CreatedDate = DateTime.Now.AddDays(-15)
                },
                new Slab
                {
                    Id = Guid.NewGuid().ToString(),
                    Container = containers[1], // Use the saved container ID
                    MarbleTypeNavigation = marbleTypes[2], // Use the saved marble type ID
                    SlabNumber = "SLB-EL-002",
                    Thickness = 2.0m,
                    Width = 118.0m,
                    Length = 238.0m,
                    CostPerSqM = 45.00m,
                    Status = "Available",
                    CreatedDate = DateTime.Now.AddDays(-15)
                }
            };

            await _context.Slabs.AddRangeAsync(slabs);
            await _context.SaveChangesAsync(); // Save slabs

            // Step 7: Seed Sales
            var sales = new List<Sale>
            {
                new Sale
                {
                    Id = Guid.NewGuid().ToString(),
                    Customer = customers[0], // Use the saved customer ID
                    SaleDate = DateTime.Now.AddDays(-20),
                    Notes = "Premium marble for hotel lobby renovation",
                    CreatedDate = DateTime.Now.AddDays(-20)
                }
            };

            await _context.Sales.AddRangeAsync(sales);
            await _context.SaveChangesAsync(); // Save sales

            // Step 8: Create sale items for the sold slab
            var soldSlab = slabs.First(s => s.Status == "Sold");
            var saleItems = new List<SaleItem>
            {
                new SaleItem
                {
                    Id = Guid.NewGuid().ToString(),
                    Sale = sales[0], // Use the saved sale ID
                    Slab = soldSlab, // Use the saved slab ID
                    AreaSold = soldSlab.Area,
                    SellingPriceEGP = 15000.00m,
                    CostPerSqM = soldSlab.CostPerSqM
                }
            };

            await _context.SaleItems.AddRangeAsync(saleItems);
            await _context.SaveChangesAsync(); // Save sale items

            // Step 9: Seed Payments
            var payments = new List<Payment>
            {
                new Payment
                {
                    Id = Guid.NewGuid().ToString(),
                    PurchaseOrder = purchaseOrders[0], // Use the saved purchase order ID
                    AmountUSD = 15000m,
                    PaymentMethod = "MoneyTransfer",
                    PaymentDate = DateTime.Now.AddDays(-25),
                    TransactionReference = "MT-2026-001",
                    Description = "Initial payment for Carrara marble shipment",
                    CreatedDate = DateTime.Now.AddDays(-25)
                },
                new Payment
                {
                    Id = Guid.NewGuid().ToString(),
                    PurchaseOrder = purchaseOrders[0], // Use the saved purchase order ID
                    AmountUSD = 15000m,
                    PaymentMethod = "MoneyTransfer",
                    PaymentDate = DateTime.Now.AddDays(-15),
                    TransactionReference = "MT-2026-002",
                    Description = "Final payment for Carrara marble shipment",
                    CreatedDate = DateTime.Now.AddDays(-15)
                },
                new Payment
                {
                    Id = Guid.NewGuid().ToString(),
                    PurchaseOrder = purchaseOrders[1], // Use the saved purchase order ID
                    AmountUSD = 28000m,
                    PaymentMethod = "MoneyTransfer",
                    PaymentDate = DateTime.Now.AddDays(-18),
                    TransactionReference = "MT-2026-003",
                    Description = "Full payment for Emperador Light shipment",
                    CreatedDate = DateTime.Now.AddDays(-18)
                }
            };

            await _context.Payments.AddRangeAsync(payments);
            await _context.SaveChangesAsync(); // Save payments
        }
    }
}