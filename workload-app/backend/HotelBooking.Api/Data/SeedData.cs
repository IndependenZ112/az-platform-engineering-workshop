using HotelBooking.Api.Models;

namespace HotelBooking.Api.Data;

public static class SeedData
{
    public static List<Hotel> GetHotels() =>
    [
        new("h1", "The Grand Azure", "Luxury waterfront hotel with panoramic sea views and world-class dining.",
            "Barcelona", "Spain", "Passeig de la Barceloneta 42", 41.3784, 2.1925, 5,
            "https://images.unsplash.com/photo-1566073771259-6a8506099945?w=800",
            ["Pool", "Spa", "Restaurant", "Bar", "Gym", "Beach Access", "Room Service", "WiFi"],
            [
                new("r1-1", "h1", "Deluxe Sea View", "Spacious room with balcony overlooking the Mediterranean", 2, 320m, true),
                new("r1-2", "h1", "Presidential Suite", "Lavish suite with private terrace and jacuzzi", 4, 890m, true),
                new("r1-3", "h1", "Standard Double", "Comfortable room with city views", 2, 180m, true)
            ]),

        new("h2", "Alpine Lodge & Spa", "Charming mountain retreat nestled in the Swiss Alps.",
            "Zermatt", "Switzerland", "Bahnhofstrasse 15", 46.0207, 7.7491, 4,
            "https://images.unsplash.com/photo-1551882547-ff40c63fe5fa?w=800",
            ["Spa", "Restaurant", "Ski Storage", "Sauna", "WiFi", "Mountain View"],
            [
                new("r2-1", "h2", "Mountain View Room", "Cozy room with Matterhorn views", 2, 275m, true),
                new("r2-2", "h2", "Family Suite", "Spacious suite with separate kids area", 5, 450m, true),
                new("r2-3", "h2", "Chalet Room", "Authentic Swiss chalet-style room", 2, 220m, true)
            ]),

        new("h3", "Sakura Garden Hotel", "Traditional Japanese hospitality meets modern comfort.",
            "Kyoto", "Japan", "Higashiyama-ku, Gion 234", 35.0036, 135.7785, 4,
            "https://images.unsplash.com/photo-1542314831-068cd1dbfeeb?w=800",
            ["Onsen", "Restaurant", "Garden", "Tea Room", "WiFi", "Concierge"],
            [
                new("r3-1", "h3", "Tatami Suite", "Traditional tatami room with futon bedding", 2, 195m, true),
                new("r3-2", "h3", "Garden View Room", "Modern room overlooking the zen garden", 2, 165m, true),
                new("r3-3", "h3", "Imperial Suite", "Premium suite with private onsen bath", 3, 520m, true)
            ]),

        new("h4", "Manhattan Skyline Hotel", "Iconic New York hotel in the heart of Midtown.",
            "New York", "United States", "5th Avenue 789", 40.7549, -73.9840, 5,
            "https://images.unsplash.com/photo-1455587734955-081b22074882?w=800",
            ["Restaurant", "Bar", "Gym", "Business Center", "WiFi", "Room Service", "Valet Parking"],
            [
                new("r4-1", "h4", "City View King", "Elegant room with skyline views", 2, 410m, true),
                new("r4-2", "h4", "Penthouse Suite", "Top-floor suite with wraparound terrace", 4, 1200m, true),
                new("r4-3", "h4", "Standard Queen", "Well-appointed room in prime location", 2, 295m, true)
            ]),

        new("h5", "Riad Andalusia", "Enchanting traditional riad with courtyard pool.",
            "Marrakech", "Morocco", "Derb Sidi Bouloukat 18", 31.6295, -7.9811, 4,
            "https://images.unsplash.com/photo-1520250497591-112f2f40a3f4?w=800",
            ["Pool", "Restaurant", "Terrace", "Hammam", "WiFi", "Airport Transfer"],
            [
                new("r5-1", "h5", "Courtyard Room", "Beautiful room facing the inner courtyard", 2, 135m, true),
                new("r5-2", "h5", "Rooftop Suite", "Suite with private rooftop terrace", 2, 210m, true),
                new("r5-3", "h5", "Family Room", "Spacious room for families", 4, 185m, true)
            ]),

        new("h6", "Fjord View Resort", "Stunning resort perched above the Norwegian fjords.",
            "Bergen", "Norway", "Fjordveien 67", 60.3913, 5.3221, 4,
            "https://images.unsplash.com/photo-1564501049412-61c2a3083791?w=800",
            ["Restaurant", "Bar", "Hiking Trails", "Sauna", "WiFi", "Fjord Tours"],
            [
                new("r6-1", "h6", "Fjord View Room", "Room with breathtaking fjord panorama", 2, 245m, true),
                new("r6-2", "h6", "Cabin Suite", "Wooden cabin-style suite", 3, 380m, true)
            ]),

        new("h7", "The Royal Crescent", "Elegant Georgian townhouse hotel in historic Bath.",
            "London", "United Kingdom", "Royal Crescent 16", 51.3862, -2.3600, 5,
            "https://images.unsplash.com/photo-1571896349842-33c89424de2d?w=800",
            ["Spa", "Restaurant", "Bar", "Garden", "WiFi", "Afternoon Tea", "Concierge"],
            [
                new("r7-1", "h7", "Heritage Room", "Period-furnished room with garden views", 2, 350m, true),
                new("r7-2", "h7", "Duke Suite", "Grand suite with separate sitting room", 2, 580m, true),
                new("r7-3", "h7", "Classic Double", "Charming room with period details", 2, 250m, true)
            ]),

        new("h8", "Côte d'Azur Palace", "Glamorous beachfront palace on the French Riviera.",
            "Nice", "France", "Promenade des Anglais 124", 43.6947, 7.2653, 5,
            "https://images.unsplash.com/photo-1551016043-7a51f0de0846?w=800",
            ["Beach", "Pool", "Spa", "Restaurant", "Bar", "Casino", "WiFi", "Valet"],
            [
                new("r8-1", "h8", "Sea View Deluxe", "Luxurious room facing the Mediterranean", 2, 480m, true),
                new("r8-2", "h8", "Riviera Suite", "Opulent suite with private balcony", 3, 850m, true)
            ]),

        new("h9", "Bali Serenity Villa", "Private villa resort surrounded by rice terraces.",
            "Ubud", "Indonesia", "Jalan Raya Tegallalang 45", -8.4312, 115.2792, 4,
            "https://images.unsplash.com/photo-1582719508461-905c673771fd?w=800",
            ["Private Pool", "Spa", "Restaurant", "Yoga Studio", "WiFi", "Shuttle"],
            [
                new("r9-1", "h9", "Garden Villa", "Private villa with plunge pool", 2, 195m, true),
                new("r9-2", "h9", "Rice Terrace Villa", "Premium villa overlooking rice paddies", 2, 310m, true),
                new("r9-3", "h9", "Honeymoon Villa", "Romantic villa with outdoor bathtub", 2, 350m, true)
            ]),

        new("h10", "Cape Town Waterfront Hotel", "Modern hotel at the V&A Waterfront with Table Mountain views.",
            "Cape Town", "South Africa", "V&A Waterfront, Dock Road 22", -33.9036, 18.4203, 4,
            "https://images.unsplash.com/photo-1445019980597-93fa8acb246c?w=800",
            ["Pool", "Restaurant", "Bar", "Gym", "WiFi", "Tours Desk", "Parking"],
            [
                new("r10-1", "h10", "Mountain View Room", "Room with Table Mountain backdrop", 2, 175m, true),
                new("r10-2", "h10", "Waterfront Suite", "Suite overlooking the harbor", 3, 320m, true),
                new("r10-3", "h10", "Standard Room", "Comfortable room with modern amenities", 2, 125m, true)
            ]),

        // ── Wave 2: 90 additional hotels ──

        new("h11", "Palazzo Veneto", "Historic palazzo turned boutique hotel on the Grand Canal.",
            "Venice", "Italy", "San Marco 1234", 45.4342, 12.3388, 5,
            "https://images.unsplash.com/photo-1514890547357-a9ee288728e0?w=800",
            ["Restaurant", "Bar", "Concierge", "Water Taxi", "WiFi", "Room Service"],
            [
                new("r11-1", "h11", "Canal View Suite", "Ornate suite overlooking the Grand Canal", 2, 650m, true),
                new("r11-2", "h11", "Classic Venetian Room", "Period-decorated room with marble bath", 2, 380m, true),
                new("r11-3", "h11", "Doge Suite", "Opulent two-bedroom suite with frescoed ceilings", 4, 1100m, true)
            ]),

        new("h12", "Santorini Cliff House", "Whitewashed cave hotel perched on the caldera rim.",
            "Santorini", "Greece", "Oia Main Street 8", 36.4618, 25.3753, 4,
            "https://images.unsplash.com/photo-1570077188670-e3a8d69ac5ff?w=800",
            ["Infinity Pool", "Restaurant", "Sunset Terrace", "WiFi", "Airport Transfer"],
            [
                new("r12-1", "h12", "Cave Suite", "Traditional cave-carved suite with private terrace", 2, 290m, true),
                new("r12-2", "h12", "Honeymoon Cave", "Romantic cave room with outdoor jacuzzi", 2, 420m, true),
                new("r12-3", "h12", "Caldera View Room", "Bright room with volcano views", 2, 210m, true)
            ]),

        new("h13", "Prague Castle View", "Art nouveau hotel overlooking Prague Castle and Charles Bridge.",
            "Prague", "Czech Republic", "Mostecká 15", 50.0865, 14.4034, 4,
            "https://images.unsplash.com/photo-1551927336-09d50efd69cd?w=800",
            ["Restaurant", "Bar", "Spa", "WiFi", "Concierge", "Tour Desk"],
            [
                new("r13-1", "h13", "Castle View Deluxe", "Room with direct castle views", 2, 195m, true),
                new("r13-2", "h13", "Art Nouveau Suite", "Lavish suite with original period details", 2, 340m, true),
                new("r13-3", "h13", "Standard Twin", "Comfortable twin room in the old town", 2, 130m, true)
            ]),

        new("h14", "Dubai Marina Heights", "Ultra-modern skyscraper hotel with infinity pool on the 50th floor.",
            "Dubai", "UAE", "Marina Walk 500", 25.0762, 55.1332, 5,
            "https://images.unsplash.com/photo-1582719478250-c89cae4dc85b?w=800",
            ["Infinity Pool", "Spa", "Restaurant", "Bar", "Gym", "Beach Club", "WiFi", "Valet"],
            [
                new("r14-1", "h14", "Marina View King", "Floor-to-ceiling windows with marina views", 2, 380m, true),
                new("r14-2", "h14", "Sky Suite", "Corner suite on the 45th floor", 3, 720m, true),
                new("r14-3", "h14", "Royal Penthouse", "Two-floor penthouse with private pool", 6, 2500m, true)
            ]),

        new("h15", "Lisbon Alfama Charm", "Converted 18th-century townhouse in Lisbon's oldest quarter.",
            "Lisbon", "Portugal", "Rua de São Miguel 42", 38.7114, -9.1301, 3,
            "https://images.unsplash.com/photo-1596394516093-501ba68a0ba6?w=800",
            ["Terrace", "Restaurant", "WiFi", "Laundry", "Tour Desk"],
            [
                new("r15-1", "h15", "Tejo View Room", "Bright room with river glimpses", 2, 95m, true),
                new("r15-2", "h15", "Alfama Suite", "Suite with private balcony and rooftop access", 2, 155m, true),
                new("r15-3", "h15", "Budget Single", "Cozy single room in a historic building", 1, 65m, true)
            ]),

        new("h16", "Vienna Imperial", "Grand Ringstraße hotel in the tradition of Habsburg elegance.",
            "Vienna", "Austria", "Kärntner Ring 16", 48.2003, 16.3725, 5,
            "https://images.unsplash.com/photo-1568084680786-a84f91d1153c?w=800",
            ["Spa", "Restaurant", "Concert Hall", "Bar", "WiFi", "Concierge", "Room Service"],
            [
                new("r16-1", "h16", "Imperial Room", "Elegant room with chandelier and marble bath", 2, 420m, true),
                new("r16-2", "h16", "Mozart Suite", "Grand suite with Bösendorfer piano", 2, 780m, true),
                new("r16-3", "h16", "Classic Double", "Well-appointed room near the Staatsoper", 2, 310m, true)
            ]),

        new("h17", "Bangkok Riverside Retreat", "Tranquil riverside hotel with traditional Thai architecture.",
            "Bangkok", "Thailand", "Charoen Nakhon Road 88", 13.7228, 100.5100, 4,
            "https://images.unsplash.com/photo-1520250497591-112f2f40a3f4?w=800",
            ["Pool", "Spa", "Restaurant", "River Shuttle", "WiFi", "Yoga Deck"],
            [
                new("r17-1", "h17", "River View Room", "Room overlooking the Chao Phraya River", 2, 110m, true),
                new("r17-2", "h17", "Thai Heritage Suite", "Suite with traditional teak furnishings", 2, 195m, true),
                new("r17-3", "h17", "Garden Bungalow", "Private bungalow surrounded by tropical gardens", 3, 165m, true)
            ]),

        new("h18", "Edinburgh Old Town Inn", "Characterful boutique inn on the Royal Mile.",
            "Edinburgh", "United Kingdom", "Royal Mile 221", 55.9505, -3.1883, 3,
            "https://images.unsplash.com/photo-1566665797739-1674de7a421a?w=800",
            ["Restaurant", "Bar", "WiFi", "Fireplace Lounge", "Tour Desk"],
            [
                new("r18-1", "h18", "Castle View Room", "Room with Edinburgh Castle views", 2, 145m, true),
                new("r18-2", "h18", "Whisky Suite", "Suite with private whisky collection", 2, 235m, true),
                new("r18-3", "h18", "Cozy Single", "Compact room with exposed stone walls", 1, 85m, true)
            ]),

        new("h19", "Sydney Harbour Hotel", "Contemporary hotel with opera house and harbour bridge views.",
            "Sydney", "Australia", "Circular Quay West 10", -33.8568, 151.2153, 5,
            "https://images.unsplash.com/photo-1551882547-ff40c63fe5fa?w=800",
            ["Pool", "Spa", "Restaurant", "Bar", "Gym", "WiFi", "Concierge", "Harbor Cruise"],
            [
                new("r19-1", "h19", "Harbour View King", "Room with iconic opera house views", 2, 450m, true),
                new("r19-2", "h19", "Bridge Suite", "Suite facing the Harbour Bridge", 2, 720m, true),
                new("r19-3", "h19", "City Room", "Modern room with skyline views", 2, 320m, true)
            ]),

        new("h20", "Reykjavik Northern Lights Lodge", "Eco-friendly lodge with glass-roofed rooms for aurora viewing.",
            "Reykjavik", "Iceland", "Laugavegur 45", 64.1466, -21.9426, 4,
            "https://images.unsplash.com/photo-1520250497591-112f2f40a3f4?w=800",
            ["Glass Roof", "Hot Tub", "Restaurant", "Aurora Alerts", "WiFi", "Excursions"],
            [
                new("r20-1", "h20", "Aurora Room", "Glass-ceiling room for northern lights viewing", 2, 310m, true),
                new("r20-2", "h20", "Geothermal Suite", "Suite with private geothermal hot tub", 2, 480m, true),
                new("r20-3", "h20", "Cozy Cabin", "Warm cabin-style room with fireplace", 2, 220m, true)
            ]),

        new("h21", "Havana Colonial", "Restored colonial mansion in the heart of Old Havana.",
            "Havana", "Cuba", "Calle Obispo 155", 23.1365, -82.3590, 3,
            "https://images.unsplash.com/photo-1564501049412-61c2a3083791?w=800",
            ["Courtyard", "Restaurant", "Bar", "Live Music", "WiFi", "Cigar Lounge"],
            [
                new("r21-1", "h21", "Colonial Room", "Restored room with original tile floors", 2, 85m, true),
                new("r21-2", "h21", "Rooftop Suite", "Suite with private rooftop terrace", 2, 145m, true),
                new("r21-3", "h21", "Courtyard Room", "Ground-floor room facing the courtyard fountain", 2, 75m, true)
            ]),

        new("h22", "Munich Biergarten Hotel", "Bavarian-themed hotel near the Marienplatz.",
            "Munich", "Germany", "Maximilianstraße 33", 48.1371, 11.5754, 4,
            "https://images.unsplash.com/photo-1596394516093-501ba68a0ba6?w=800",
            ["Beer Garden", "Restaurant", "Sauna", "WiFi", "Bike Rental", "Parking"],
            [
                new("r22-1", "h22", "Bavarian Room", "Room with traditional Bavarian decor", 2, 175m, true),
                new("r22-2", "h22", "Royal Suite", "Spacious suite with Marienplatz views", 3, 320m, true),
                new("r22-3", "h22", "Economy Double", "Functional room at great value", 2, 120m, true)
            ]),

        new("h23", "Buenos Aires Tango Hotel", "Art deco hotel in the vibrant San Telmo neighborhood.",
            "Buenos Aires", "Argentina", "Defensa 820", -34.6197, -58.3716, 4,
            "https://images.unsplash.com/photo-1571896349842-33c89424de2d?w=800",
            ["Restaurant", "Tango Lounge", "Rooftop Pool", "Bar", "WiFi", "Tour Desk"],
            [
                new("r23-1", "h23", "Tango Suite", "Suite with art deco furnishings and balcony", 2, 155m, true),
                new("r23-2", "h23", "Standard Room", "Comfortable room in the heart of San Telmo", 2, 95m, true),
                new("r23-3", "h23", "Penthouse", "Rooftop penthouse with city panorama", 4, 310m, true)
            ]),

        new("h24", "Seoul Gangnam Tower", "Sleek high-rise hotel in the Gangnam business district.",
            "Seoul", "South Korea", "Teheran-ro 152", 37.5001, 127.0368, 5,
            "https://images.unsplash.com/photo-1566073771259-6a8506099945?w=800",
            ["Pool", "Spa", "Restaurant", "Sky Bar", "Gym", "Business Center", "WiFi"],
            [
                new("r24-1", "h24", "Executive King", "Business-class room with lounge access", 2, 280m, true),
                new("r24-2", "h24", "Sky Suite", "High-floor suite with panoramic city views", 2, 520m, true),
                new("r24-3", "h24", "Standard Double", "Modern room with efficient design", 2, 195m, true)
            ]),

        new("h25", "Maldives Overwater Paradise", "Luxury overwater bungalows in a private atoll.",
            "Malé", "Maldives", "North Malé Atoll", 4.1755, 73.5093, 5,
            "https://images.unsplash.com/photo-1582719508461-905c673771fd?w=800",
            ["Private Beach", "Overwater Spa", "Restaurant", "Diving Center", "WiFi", "Seaplane Transfer"],
            [
                new("r25-1", "h25", "Overwater Bungalow", "Bungalow with glass floor panels over lagoon", 2, 850m, true),
                new("r25-2", "h25", "Beach Villa", "Beachfront villa with private pool", 3, 1100m, true),
                new("r25-3", "h25", "Sunset Suite", "Premium overwater suite facing the sunset", 2, 1500m, true)
            ]),

        new("h26", "Toronto Lakefront Hotel", "Modern hotel on the shores of Lake Ontario.",
            "Toronto", "Canada", "Queens Quay West 300", 43.6391, -79.3824, 4,
            "https://images.unsplash.com/photo-1455587734955-081b22074882?w=800",
            ["Pool", "Restaurant", "Bar", "Gym", "WiFi", "Parking", "Bike Rental"],
            [
                new("r26-1", "h26", "Lake View Room", "Room overlooking Lake Ontario", 2, 225m, true),
                new("r26-2", "h26", "CN Tower Suite", "Suite with CN Tower views", 3, 395m, true),
                new("r26-3", "h26", "Standard Queen", "Comfortable room near the waterfront", 2, 165m, true)
            ]),

        new("h27", "Petra Rose Hotel", "Sandstone boutique hotel near the ancient city of Petra.",
            "Wadi Musa", "Jordan", "Tourism Street 12", 30.3285, 35.4444, 4,
            "https://images.unsplash.com/photo-1564501049412-61c2a3083791?w=800",
            ["Restaurant", "Terrace", "WiFi", "Guided Tours", "Horse Rides", "Pool"],
            [
                new("r27-1", "h27", "Desert View Room", "Room overlooking the Wadi Araba", 2, 120m, true),
                new("r27-2", "h27", "Nabatean Suite", "Suite inspired by ancient Nabatean design", 2, 210m, true),
                new("r27-3", "h27", "Family Room", "Spacious room for families visiting Petra", 4, 165m, true)
            ]),

        new("h28", "Amsterdam Canal House", "17th-century canal house converted into an intimate hotel.",
            "Amsterdam", "Netherlands", "Herengracht 341", 52.3702, 4.8856, 4,
            "https://images.unsplash.com/photo-1568084680786-a84f91d1153c?w=800",
            ["Garden", "Bar", "WiFi", "Concierge", "Bike Rental", "Canal View"],
            [
                new("r28-1", "h28", "Canal Room", "Charming room overlooking the Herengracht", 2, 245m, true),
                new("r28-2", "h28", "Garden Suite", "Suite with access to the private garden", 2, 380m, true),
                new("r28-3", "h28", "Attic Room", "Cozy room under the historic timber beams", 2, 175m, true)
            ]),

        new("h29", "Rio Copacabana Palace", "Legendary beachfront hotel on Copacabana Beach.",
            "Rio de Janeiro", "Brazil", "Avenida Atlântica 1702", -22.9649, -43.1729, 5,
            "https://images.unsplash.com/photo-1551016043-7a51f0de0846?w=800",
            ["Beach Access", "Pool", "Spa", "Restaurant", "Bar", "Tennis", "WiFi"],
            [
                new("r29-1", "h29", "Ocean View Deluxe", "Room with panoramic Copacabana views", 2, 390m, true),
                new("r29-2", "h29", "Presidential Suite", "Grand suite with private butler service", 4, 1400m, true),
                new("r29-3", "h29", "City Room", "Comfortable room near the beach", 2, 260m, true)
            ]),

        new("h30", "Singapore Marina Bay Sands View", "Boutique hotel with views of the iconic Marina Bay skyline.",
            "Singapore", "Singapore", "Collyer Quay 8", 1.2834, 103.8515, 4,
            "https://images.unsplash.com/photo-1542314831-068cd1dbfeeb?w=800",
            ["Rooftop Bar", "Pool", "Restaurant", "Gym", "WiFi", "Shuttle"],
            [
                new("r30-1", "h30", "Bay View Room", "Room overlooking Marina Bay", 2, 290m, true),
                new("r30-2", "h30", "Skyline Suite", "Suite with floor-to-ceiling bay windows", 2, 480m, true),
                new("r30-3", "h30", "Compact Single", "Efficient single room in prime location", 1, 195m, true)
            ]),

        new("h31", "Tuscany Vineyard Estate", "Converted farmhouse amid rolling vineyards in Chianti.",
            "Florence", "Italy", "Via Chiantigiana 45", 43.4643, 11.1535, 4,
            "https://images.unsplash.com/photo-1566073771259-6a8506099945?w=800",
            ["Vineyard Tours", "Wine Tasting", "Pool", "Restaurant", "WiFi", "Cooking Classes"],
            [
                new("r31-1", "h31", "Vineyard Room", "Room with views over the Chianti vineyards", 2, 185m, true),
                new("r31-2", "h31", "Tower Suite", "Suite in the medieval watchtower", 2, 320m, true),
                new("r31-3", "h31", "Family Apartment", "Two-bedroom apartment with kitchen", 5, 290m, true)
            ]),

        new("h32", "Istanbul Bosphorus Grand", "Ottoman-era palace hotel on the Bosphorus strait.",
            "Istanbul", "Turkey", "Çırağan Caddesi 32", 41.0476, 29.0229, 5,
            "https://images.unsplash.com/photo-1520250497591-112f2f40a3f4?w=800",
            ["Pool", "Spa", "Hammam", "Restaurant", "Bar", "WiFi", "Boat Tours", "Valet"],
            [
                new("r32-1", "h32", "Bosphorus View Room", "Room with strait and Asian shore views", 2, 340m, true),
                new("r32-2", "h32", "Sultan Suite", "Palatial suite with Ottoman decor", 3, 680m, true),
                new("r32-3", "h32", "Garden Room", "Peaceful room facing the palace gardens", 2, 240m, true)
            ]),

        new("h33", "Patagonia Wilderness Lodge", "Remote eco-lodge at the edge of Torres del Paine.",
            "Puerto Natales", "Chile", "Ruta 9 Norte Km 12", -51.7311, -72.5103, 3,
            "https://images.unsplash.com/photo-1551882547-ff40c63fe5fa?w=800",
            ["Hiking", "Restaurant", "Hot Tub", "Wildlife Tours", "WiFi", "Transfers"],
            [
                new("r33-1", "h33", "Mountain Cabin", "Cabin with views of Torres del Paine", 2, 195m, true),
                new("r33-2", "h33", "Lakeside Room", "Room overlooking the glacial lake", 2, 155m, true),
                new("r33-3", "h33", "Explorer Suite", "Spacious suite with private deck", 3, 280m, true)
            ]),

        new("h34", "Copenhagen Nyhavn Hotel", "Colorful boutique hotel in Copenhagen's iconic harbor.",
            "Copenhagen", "Denmark", "Nyhavn 71", 55.6802, 12.5891, 4,
            "https://images.unsplash.com/photo-1596394516093-501ba68a0ba6?w=800",
            ["Restaurant", "Bar", "Bike Rental", "WiFi", "Canal View", "Concierge"],
            [
                new("r34-1", "h34", "Harbor View Room", "Room overlooking Nyhavn canal", 2, 240m, true),
                new("r34-2", "h34", "Danish Design Suite", "Minimalist suite with designer furniture", 2, 380m, true),
                new("r34-3", "h34", "Compact Double", "Hygge-inspired compact room", 2, 170m, true)
            ]),

        new("h35", "Mexico City Zona Rosa", "Vibrant hotel in the cultural heart of Mexico City.",
            "Mexico City", "Mexico", "Paseo de la Reforma 222", 19.4270, -99.1676, 4,
            "https://images.unsplash.com/photo-1571896349842-33c89424de2d?w=800",
            ["Pool", "Restaurant", "Bar", "Spa", "WiFi", "Tour Desk", "Parking"],
            [
                new("r35-1", "h35", "Reforma View Room", "Room overlooking the grand boulevard", 2, 135m, true),
                new("r35-2", "h35", "Frida Suite", "Art-inspired suite with bold Mexican design", 2, 225m, true),
                new("r35-3", "h35", "Standard Twin", "Comfortable twin room near Chapultepec", 2, 95m, true)
            ]),

        new("h36", "Helsinki Design Hotel", "Award-winning design hotel in the Design District.",
            "Helsinki", "Finland", "Eerikinkatu 12", 60.1674, 24.9354, 4,
            "https://images.unsplash.com/photo-1566665797739-1674de7a421a?w=800",
            ["Sauna", "Restaurant", "Bar", "WiFi", "Gym", "Design Shop"],
            [
                new("r36-1", "h36", "Design Room", "Room curated by Finnish designers", 2, 205m, true),
                new("r36-2", "h36", "Marimekko Suite", "Suite with exclusive Marimekko textiles", 2, 340m, true),
                new("r36-3", "h36", "Studio Room", "Compact studio with workspace", 1, 155m, true)
            ]),

        new("h37", "Queenstown Alpine Resort", "Adventure resort near Milford Sound and ski fields.",
            "Queenstown", "New Zealand", "Lake Esplanade 5", -45.0311, 168.6626, 4,
            "https://images.unsplash.com/photo-1551882547-ff40c63fe5fa?w=800",
            ["Ski Storage", "Restaurant", "Bar", "Hot Tub", "WiFi", "Adventure Desk", "Parking"],
            [
                new("r37-1", "h37", "Lake View Room", "Room with Lake Wakatipu panorama", 2, 210m, true),
                new("r37-2", "h37", "Mountain Lodge Suite", "Two-level suite with fireplace", 4, 385m, true),
                new("r37-3", "h37", "Budget Bunk", "Affordable bunk room for adventurers", 2, 95m, true)
            ]),

        new("h38", "Cartagena Old City Hotel", "Colonial jewel within the UNESCO-listed walled city.",
            "Cartagena", "Colombia", "Calle de la Universidad 36", 10.4236, -75.5477, 4,
            "https://images.unsplash.com/photo-1564501049412-61c2a3083791?w=800",
            ["Pool", "Restaurant", "Rooftop Bar", "WiFi", "Spa", "Tour Desk"],
            [
                new("r38-1", "h38", "Colonial Room", "Room with exposed colonial-era stonework", 2, 125m, true),
                new("r38-2", "h38", "Rooftop Suite", "Suite with private rooftop plunge pool", 2, 245m, true),
                new("r38-3", "h38", "Standard Room", "Charming room in the historic center", 2, 90m, true)
            ]),

        new("h39", "Shanghai Bund Hotel", "Art deco landmark on the famous Bund waterfront.",
            "Shanghai", "China", "Zhongshan East 1st Road 20", 31.2397, 121.4918, 5,
            "https://images.unsplash.com/photo-1455587734955-081b22074882?w=800",
            ["Spa", "Restaurant", "Bar", "Pool", "Gym", "WiFi", "Business Center", "Valet"],
            [
                new("r39-1", "h39", "Bund View Room", "Room facing the Pudong skyline", 2, 350m, true),
                new("r39-2", "h39", "Heritage Suite", "Restored 1920s art deco suite", 2, 620m, true),
                new("r39-3", "h39", "Garden Room", "Quiet room facing the interior garden", 2, 260m, true)
            ]),

        new("h40", "Dubrovnik Old Town Stay", "Stone-walled hotel inside the UNESCO city walls.",
            "Dubrovnik", "Croatia", "Stradun 18", 42.6411, 18.1082, 4,
            "https://images.unsplash.com/photo-1568084680786-a84f91d1153c?w=800",
            ["Terrace", "Restaurant", "Bar", "WiFi", "Beach Access", "Concierge"],
            [
                new("r40-1", "h40", "Sea View Room", "Room with Adriatic Sea views", 2, 220m, true),
                new("r40-2", "h40", "City Wall Suite", "Suite built into the ancient walls", 2, 350m, true),
                new("r40-3", "h40", "Courtyard Room", "Cool stone room with courtyard access", 2, 155m, true)
            ]),

        new("h41", "Nairobi Safari Lodge", "Urban safari lodge on the edge of Nairobi National Park.",
            "Nairobi", "Kenya", "Langata Road 120", -1.3517, 36.7518, 4,
            "https://images.unsplash.com/photo-1566073771259-6a8506099945?w=800",
            ["Safari Drives", "Restaurant", "Pool", "WiFi", "Firepit", "Nature Walks"],
            [
                new("r41-1", "h41", "Safari Tent", "Luxury canvas tent with en-suite bathroom", 2, 180m, true),
                new("r41-2", "h41", "Bush Suite", "Suite overlooking a watering hole", 2, 310m, true),
                new("r41-3", "h41", "Family Lodge", "Two-room lodge for families", 5, 260m, true)
            ]),

        new("h42", "St. Petersburg Hermitage View", "Grand hotel overlooking the Winter Palace.",
            "St. Petersburg", "Russia", "Palace Embankment 6", 59.9398, 30.3146, 5,
            "https://images.unsplash.com/photo-1542314831-068cd1dbfeeb?w=800",
            ["Spa", "Restaurant", "Bar", "WiFi", "Concierge", "River Cruise", "Room Service"],
            [
                new("r42-1", "h42", "Palace View Room", "Room with views of the Winter Palace", 2, 290m, true),
                new("r42-2", "h42", "Tsarist Suite", "Opulent suite with gold-leaf details", 2, 550m, true),
                new("r42-3", "h42", "Nevsky Room", "Comfortable room near Nevsky Prospekt", 2, 195m, true)
            ]),

        new("h43", "Amalfi Coast Terrazza", "Clifftop boutique hotel with terraced gardens above the sea.",
            "Positano", "Italy", "Via Cristoforo Colombo 30", 40.6281, 14.4850, 4,
            "https://images.unsplash.com/photo-1570077188670-e3a8d69ac5ff?w=800",
            ["Pool", "Restaurant", "Terrace", "Beach Shuttle", "WiFi", "Boat Excursions"],
            [
                new("r43-1", "h43", "Sea View Room", "Room with sweeping Mediterranean views", 2, 280m, true),
                new("r43-2", "h43", "Terrazza Suite", "Suite with private terraced garden", 2, 450m, true),
                new("r43-3", "h43", "Lemon Grove Room", "Charming room amid lemon trees", 2, 195m, true)
            ]),

        new("h44", "Hanoi Old Quarter Hotel", "Boutique hotel in the bustling heart of Hanoi.",
            "Hanoi", "Vietnam", "Hàng Bạc 52", 21.0340, 105.8544, 3,
            "https://images.unsplash.com/photo-1566665797739-1674de7a421a?w=800",
            ["Restaurant", "Rooftop Bar", "WiFi", "Tour Desk", "Motorbike Rental"],
            [
                new("r44-1", "h44", "Lake View Room", "Room with Hoàn Kiếm Lake glimpses", 2, 65m, true),
                new("r44-2", "h44", "Heritage Suite", "Suite with Vietnamese lacquerware decor", 2, 110m, true),
                new("r44-3", "h44", "Budget Double", "Clean room in the Old Quarter", 2, 40m, true)
            ]),

        new("h45", "Moscow Red Square Hotel", "Luxury hotel steps from the Kremlin and Red Square.",
            "Moscow", "Russia", "Tverskaya Street 3", 55.7558, 37.6176, 5,
            "https://images.unsplash.com/photo-1455587734955-081b22074882?w=800",
            ["Spa", "Restaurant", "Bar", "Pool", "WiFi", "Business Center", "Concierge"],
            [
                new("r45-1", "h45", "Kremlin View Room", "Room with Red Square panorama", 2, 380m, true),
                new("r45-2", "h45", "Bolshoi Suite", "Lavish suite inspired by the Bolshoi Theatre", 2, 720m, true),
                new("r45-3", "h45", "Standard Room", "Well-appointed room near the center", 2, 250m, true)
            ]),

        new("h46", "Marrakech Desert Oasis", "Luxury desert camp in the Agafay desert near Marrakech.",
            "Marrakech", "Morocco", "Route d'Amizmiz Km 30", 31.4503, -8.1651, 4,
            "https://images.unsplash.com/photo-1582719508461-905c673771fd?w=800",
            ["Pool", "Restaurant", "Camel Rides", "Stargazing", "WiFi", "Spa Tent"],
            [
                new("r46-1", "h46", "Luxury Tent", "Air-conditioned tent with en-suite shower", 2, 195m, true),
                new("r46-2", "h46", "Royal Pavilion", "Private pavilion with plunge pool", 2, 380m, true),
                new("r46-3", "h46", "Berber Suite", "Two-room suite with traditional Berber decor", 4, 310m, true)
            ]),

        new("h47", "Bruges Medieval Inn", "Timber-framed inn overlooking the Bruges canals.",
            "Bruges", "Belgium", "Dijver 5", 51.2056, 3.2253, 3,
            "https://images.unsplash.com/photo-1568084680786-a84f91d1153c?w=800",
            ["Restaurant", "Bar", "WiFi", "Bike Rental", "Canal View", "Chocolate Shop"],
            [
                new("r47-1", "h47", "Canal Room", "Room with views over the Dijver canal", 2, 145m, true),
                new("r47-2", "h47", "Belfry Suite", "Suite with Belfry tower views", 2, 240m, true),
                new("r47-3", "h47", "Cozy Attic", "Small attic room with character", 1, 90m, true)
            ]),

        new("h48", "Lima Miraflores Hotel", "Modern hotel perched on the Miraflores clifftops.",
            "Lima", "Peru", "Malecón de la Reserva 615", -12.1221, -77.0306, 4,
            "https://images.unsplash.com/photo-1551016043-7a51f0de0846?w=800",
            ["Pool", "Restaurant", "Bar", "Gym", "WiFi", "Paragliding Desk", "Ocean View"],
            [
                new("r48-1", "h48", "Pacific View Room", "Room overlooking the Pacific Ocean", 2, 145m, true),
                new("r48-2", "h48", "Inca Suite", "Suite with pre-Columbian art collection", 2, 255m, true),
                new("r48-3", "h48", "Standard Double", "Comfortable room near Larcomar", 2, 105m, true)
            ]),

        new("h49", "Zanzibar Spice Island Resort", "Beachfront resort on the white sands of Zanzibar.",
            "Stone Town", "Tanzania", "Nungwi Beach Road", -5.7253, 39.2974, 4,
            "https://images.unsplash.com/photo-1520250497591-112f2f40a3f4?w=800",
            ["Beach", "Pool", "Spa", "Restaurant", "Diving", "WiFi", "Spice Tours"],
            [
                new("r49-1", "h49", "Beach Banda", "Thatched-roof bungalow on the beach", 2, 165m, true),
                new("r49-2", "h49", "Ocean Suite", "Suite with private infinity pool", 2, 310m, true),
                new("r49-3", "h49", "Garden Room", "Room surrounded by tropical gardens", 2, 120m, true)
            ]),

        new("h50", "Oslo Fjord Hotel", "Scandinavian design hotel on the Oslofjord waterfront.",
            "Oslo", "Norway", "Aker Brygge 24", 59.9110, 10.7275, 4,
            "https://images.unsplash.com/photo-1564501049412-61c2a3083791?w=800",
            ["Restaurant", "Bar", "Sauna", "WiFi", "Bike Rental", "Fjord View"],
            [
                new("r50-1", "h50", "Fjord Room", "Room with Oslofjord views", 2, 230m, true),
                new("r50-2", "h50", "Opera Suite", "Suite overlooking the Oslo Opera House", 2, 380m, true),
                new("r50-3", "h50", "Budget Single", "Minimalist single room", 1, 145m, true)
            ]),

        new("h51", "Seville Flamenco Palace", "Andalusian palace with a courtyard and fountain.",
            "Seville", "Spain", "Plaza de España 5", 37.3772, -5.9869, 4,
            "https://images.unsplash.com/photo-1571896349842-33c89424de2d?w=800",
            ["Pool", "Restaurant", "Flamenco Shows", "Garden", "WiFi", "Spa"],
            [
                new("r51-1", "h51", "Alcázar Room", "Room with Moorish-inspired decor", 2, 155m, true),
                new("r51-2", "h51", "Flamenco Suite", "Suite overlooking the courtyard stage", 2, 270m, true),
                new("r51-3", "h51", "Garden Room", "Room facing the orange tree garden", 2, 125m, true)
            ]),

        new("h52", "Taipei Skyview Hotel", "Glass-and-steel tower with Taipei 101 views.",
            "Taipei", "Taiwan", "Xinyi Road 88", 25.0330, 121.5654, 4,
            "https://images.unsplash.com/photo-1542314831-068cd1dbfeeb?w=800",
            ["Pool", "Restaurant", "Sky Bar", "Gym", "WiFi", "Business Center"],
            [
                new("r52-1", "h52", "101 View Room", "Room with Taipei 101 panorama", 2, 195m, true),
                new("r52-2", "h52", "Executive Suite", "Corner suite with wraparound views", 2, 350m, true),
                new("r52-3", "h52", "Standard Twin", "Modern twin room in Xinyi District", 2, 140m, true)
            ]),

        new("h53", "Jaipur Pink Palace", "Heritage hotel in a restored Rajasthani haveli.",
            "Jaipur", "India", "Johari Bazaar 87", 26.9185, 75.7883, 4,
            "https://images.unsplash.com/photo-1566073771259-6a8506099945?w=800",
            ["Pool", "Restaurant", "Rooftop", "WiFi", "Cooking Classes", "Elephant Tours"],
            [
                new("r53-1", "h53", "Maharaja Room", "Ornate room with stained glass windows", 2, 110m, true),
                new("r53-2", "h53", "Royal Suite", "Suite with private courtyard", 3, 220m, true),
                new("r53-3", "h53", "Heritage Room", "Restored haveli room with original frescoes", 2, 85m, true)
            ]),

        new("h54", "Vancouver Mountain Lodge", "West Coast lodge between mountains and Pacific Ocean.",
            "Vancouver", "Canada", "Coal Harbour Quay 1128", 49.2886, -123.1195, 4,
            "https://images.unsplash.com/photo-1551882547-ff40c63fe5fa?w=800",
            ["Pool", "Spa", "Restaurant", "Bar", "Gym", "WiFi", "Kayak Rental", "Ski Shuttle"],
            [
                new("r54-1", "h54", "Mountain View Room", "Room with North Shore mountains backdrop", 2, 240m, true),
                new("r54-2", "h54", "Pacific Suite", "Suite with ocean and mountain panorama", 2, 420m, true),
                new("r54-3", "h54", "Standard Queen", "Comfortable room near Stanley Park", 2, 175m, true)
            ]),

        new("h55", "Fez Medina Riad", "Intimate riad hidden in the ancient Fez medina.",
            "Fez", "Morocco", "Derb el Miter 7", 34.0654, -4.9735, 3,
            "https://images.unsplash.com/photo-1520250497591-112f2f40a3f4?w=800",
            ["Courtyard", "Restaurant", "Terrace", "Hammam", "WiFi", "Cooking Classes"],
            [
                new("r55-1", "h55", "Zellige Room", "Room decorated with traditional zellige tiles", 2, 75m, true),
                new("r55-2", "h55", "Terrace Suite", "Suite with private rooftop terrace", 2, 130m, true),
                new("r55-3", "h55", "Riad Room", "Simple room around the central courtyard", 2, 55m, true)
            ]),

        new("h56", "Athens Acropolis View", "Rooftop restaurant hotel with Parthenon views.",
            "Athens", "Greece", "Dionysiou Areopagitou 15", 37.9704, 23.7282, 4,
            "https://images.unsplash.com/photo-1568084680786-a84f91d1153c?w=800",
            ["Rooftop Restaurant", "Bar", "WiFi", "Concierge", "Pool", "Tour Desk"],
            [
                new("r56-1", "h56", "Acropolis Room", "Room with direct Parthenon views", 2, 210m, true),
                new("r56-2", "h56", "Olympian Suite", "Spacious suite with marble bathroom", 2, 350m, true),
                new("r56-3", "h56", "Economy Room", "Clean budget room in Plaka", 2, 115m, true)
            ]),

        new("h57", "Antigua Caribbean Inn", "Colorful beachfront inn on Dickenson Bay.",
            "St. John's", "Antigua", "Dickenson Bay Road 12", 17.1274, -61.8468, 3,
            "https://images.unsplash.com/photo-1582719508461-905c673771fd?w=800",
            ["Beach", "Pool", "Bar", "WiFi", "Snorkeling", "Airport Shuttle"],
            [
                new("r57-1", "h57", "Beach Room", "Room steps from the Caribbean Sea", 2, 145m, true),
                new("r57-2", "h57", "Sea View Suite", "Suite with wraparound sea views", 3, 250m, true),
                new("r57-3", "h57", "Garden Room", "Tropical garden-view room", 2, 105m, true)
            ]),

        new("h58", "Salzburg Mozart Hotel", "Baroque hotel in Mozart's birthplace.",
            "Salzburg", "Austria", "Getreidegasse 9", 47.8004, 13.0439, 4,
            "https://images.unsplash.com/photo-1596394516093-501ba68a0ba6?w=800",
            ["Restaurant", "Concert Tickets", "WiFi", "Bar", "Garden", "Tour Desk"],
            [
                new("r58-1", "h58", "Fortress View Room", "Room with Hohensalzburg fortress views", 2, 195m, true),
                new("r58-2", "h58", "Mozart Suite", "Suite where Mozart once performed", 2, 340m, true),
                new("r58-3", "h58", "Standard Double", "Comfortable room in the old town", 2, 145m, true)
            ]),

        new("h59", "Cusco Inca Hotel", "Stone-walled hotel built on Inca foundations.",
            "Cusco", "Peru", "Calle Loreto 115", -13.5200, -71.9785, 3,
            "https://images.unsplash.com/photo-1564501049412-61c2a3083791?w=800",
            ["Restaurant", "Oxygen Bar", "WiFi", "Tour Desk", "Coca Tea Service"],
            [
                new("r59-1", "h59", "Inca Room", "Room with original Inca stonework walls", 2, 85m, true),
                new("r59-2", "h59", "Conquistador Suite", "Colonial suite with courtyard views", 2, 155m, true),
                new("r59-3", "h59", "Budget Room", "Simple room for trekkers", 2, 50m, true)
            ]),

        new("h60", "Hong Kong Harbour Hotel", "Waterfront hotel with Victoria Harbour views.",
            "Hong Kong", "China", "Tsim Sha Tsui Promenade 18", 22.2950, 114.1722, 5,
            "https://images.unsplash.com/photo-1455587734955-081b22074882?w=800",
            ["Pool", "Spa", "Restaurant", "Sky Bar", "Gym", "WiFi", "Star Ferry Access"],
            [
                new("r60-1", "h60", "Harbour View Room", "Room with Victoria Harbour panorama", 2, 380m, true),
                new("r60-2", "h60", "Peak Suite", "Suite with views of Victoria Peak", 2, 650m, true),
                new("r60-3", "h60", "City Room", "Modern room in Kowloon", 2, 270m, true)
            ]),

        new("h61", "Paris Le Marais Boutique", "Chic boutique hotel in the fashionable Marais district.",
            "Paris", "France", "Rue des Francs-Bourgeois 25", 48.8566, 2.3603, 4,
            "https://images.unsplash.com/photo-1566665797739-1674de7a421a?w=800",
            ["Restaurant", "Bar", "WiFi", "Concierge", "Garden Courtyard"],
            [
                new("r61-1", "h61", "Parisian Room", "Elegant room with Haussmann details", 2, 280m, true),
                new("r61-2", "h61", "Eiffel Suite", "Suite with distant Eiffel Tower view", 2, 480m, true),
                new("r61-3", "h61", "Mansard Room", "Cozy room under the zinc rooftops", 2, 195m, true)
            ]),

        new("h62", "Chiang Mai Temple Lodge", "Teak-wood lodge near the old city temples.",
            "Chiang Mai", "Thailand", "Ratchadamnoen Road 78", 18.7877, 98.9931, 3,
            "https://images.unsplash.com/photo-1520250497591-112f2f40a3f4?w=800",
            ["Pool", "Restaurant", "Massage", "WiFi", "Bike Rental", "Cooking Classes"],
            [
                new("r62-1", "h62", "Temple View Room", "Room with views of Wat Chedi Luang", 2, 55m, true),
                new("r62-2", "h62", "Lanna Suite", "Suite with traditional Lanna decor", 2, 110m, true),
                new("r62-3", "h62", "Garden Bungalow", "Private bungalow in tropical garden", 2, 80m, true)
            ]),

        new("h63", "Berlin Kreuzberg Loft", "Industrial-chic loft hotel in hip Kreuzberg.",
            "Berlin", "Germany", "Oranienstraße 185", 52.4994, 13.4164, 3,
            "https://images.unsplash.com/photo-1551016043-7a51f0de0846?w=800",
            ["Bar", "Rooftop Terrace", "WiFi", "Bike Rental", "Co-Working Space"],
            [
                new("r63-1", "h63", "Loft Room", "Open-plan loft with exposed brick", 2, 115m, true),
                new("r63-2", "h63", "Artist Suite", "Street-art decorated suite", 2, 195m, true),
                new("r63-3", "h63", "Pod Room", "Capsule-style budget room", 1, 65m, true)
            ]),

        new("h64", "Luang Prabang Riverside", "French-colonial villa on the Mekong River bank.",
            "Luang Prabang", "Laos", "Khem Khong Road 42", 19.8876, 102.1349, 3,
            "https://images.unsplash.com/photo-1582719508461-905c673771fd?w=800",
            ["Restaurant", "Terrace", "WiFi", "Boat Trips", "Bike Rental", "Spa"],
            [
                new("r64-1", "h64", "Mekong Room", "Room overlooking the Mekong River", 2, 70m, true),
                new("r64-2", "h64", "Colonial Suite", "Suite in the restored colonial wing", 2, 125m, true),
                new("r64-3", "h64", "Garden Room", "Room facing the frangipani garden", 2, 50m, true)
            ]),

        new("h65", "Bordeaux Wine Hotel", "Vineyard hotel in the heart of Saint-Émilion.",
            "Bordeaux", "France", "Route de Saint-Émilion 15", 44.8946, -0.1585, 4,
            "https://images.unsplash.com/photo-1566073771259-6a8506099945?w=800",
            ["Wine Cellar", "Restaurant", "Pool", "WiFi", "Vineyard Tours", "Tasting Room"],
            [
                new("r65-1", "h65", "Vineyard Room", "Room with vineyard views", 2, 195m, true),
                new("r65-2", "h65", "Château Suite", "Suite in the original château", 2, 340m, true),
                new("r65-3", "h65", "Barrel Room", "Uniquely themed wine-barrel room", 2, 155m, true)
            ]),

        new("h66", "Doha Pearl Hotel", "Futuristic hotel on The Pearl-Qatar island.",
            "Doha", "Qatar", "Porto Arabia, The Pearl", 25.3684, 51.5510, 5,
            "https://images.unsplash.com/photo-1542314831-068cd1dbfeeb?w=800",
            ["Beach", "Pool", "Spa", "Restaurant", "Bar", "Gym", "WiFi", "Valet", "Yacht Marina"],
            [
                new("r66-1", "h66", "Marina View Room", "Room overlooking the yacht marina", 2, 340m, true),
                new("r66-2", "h66", "Pearl Suite", "Two-bedroom suite with butler service", 4, 780m, true),
                new("r66-3", "h66", "Deluxe King", "Luxurious room with Arabian Gulf views", 2, 260m, true)
            ]),

        new("h67", "Lake Como Villa", "Romantic lakeside villa with terraced gardens.",
            "Bellagio", "Italy", "Via Roma 1", 45.9893, 9.2613, 5,
            "https://images.unsplash.com/photo-1551882547-ff40c63fe5fa?w=800",
            ["Pool", "Spa", "Restaurant", "Boat Dock", "Garden", "WiFi", "Concierge"],
            [
                new("r67-1", "h67", "Lake View Suite", "Suite with wraparound lake panorama", 2, 520m, true),
                new("r67-2", "h67", "Garden Room", "Room in the terraced Italian gardens", 2, 310m, true),
                new("r67-3", "h67", "Villa Apartment", "Self-contained apartment in the villa wing", 4, 680m, true)
            ]),

        new("h68", "Marrakech Medina Palace", "Palatial hotel near Jemaa el-Fnaa square.",
            "Marrakech", "Morocco", "Avenue Mohammed V 44", 31.6258, -7.9892, 5,
            "https://images.unsplash.com/photo-1571896349842-33c89424de2d?w=800",
            ["Pool", "Spa", "Hammam", "Restaurant", "Bar", "WiFi", "Garden", "Airport Transfer"],
            [
                new("r68-1", "h68", "Medina Room", "Room with views of the medina rooftops", 2, 195m, true),
                new("r68-2", "h68", "Sultan Suite", "Luxurious suite with private hammam", 2, 420m, true),
                new("r68-3", "h68", "Family Riad", "Two-bedroom riad-style accommodation", 5, 350m, true)
            ]),

        new("h69", "Tallinn Medieval Hotel", "Gothic building in the UNESCO old town.",
            "Tallinn", "Estonia", "Viru 12", 59.4370, 24.7451, 3,
            "https://images.unsplash.com/photo-1596394516093-501ba68a0ba6?w=800",
            ["Restaurant", "Bar", "WiFi", "Sauna", "Tour Desk"],
            [
                new("r69-1", "h69", "Tower Room", "Room in the medieval watchtower", 2, 105m, true),
                new("r69-2", "h69", "Gothic Suite", "Vaulted-ceiling suite with period furniture", 2, 185m, true),
                new("r69-3", "h69", "Compact Room", "Small but characterful room", 1, 65m, true)
            ]),

        new("h70", "Lake Bled Castle Hotel", "Fairy-tale hotel with views of Lake Bled island.",
            "Bled", "Slovenia", "Veslaška promenada 6", 46.3683, 14.1146, 4,
            "https://images.unsplash.com/photo-1568084680786-a84f91d1153c?w=800",
            ["Pool", "Spa", "Restaurant", "Rowing Boats", "WiFi", "Hiking Guides"],
            [
                new("r70-1", "h70", "Lake View Room", "Room with views of the island church", 2, 175m, true),
                new("r70-2", "h70", "Castle Suite", "Suite overlooking Bled Castle", 2, 290m, true),
                new("r70-3", "h70", "Alpine Room", "Cozy room with mountain panorama", 2, 135m, true)
            ]),

        new("h71", "San Francisco Pacific Heights", "Victorian mansion hotel in a prestigious neighborhood.",
            "San Francisco", "United States", "Pacific Avenue 2100", 37.7929, -122.4330, 4,
            "https://images.unsplash.com/photo-1551016043-7a51f0de0846?w=800",
            ["Restaurant", "Garden", "WiFi", "Parking", "Concierge", "Wine Hour"],
            [
                new("r71-1", "h71", "Bay View Room", "Room with San Francisco Bay views", 2, 310m, true),
                new("r71-2", "h71", "Victorian Suite", "Restored Victorian suite with fireplace", 2, 490m, true),
                new("r71-3", "h71", "Standard Queen", "Comfortable room in a charming setting", 2, 225m, true)
            ]),

        new("h72", "Kathmandu Heritage Hotel", "Newari-style hotel in Kathmandu's Durbar Square area.",
            "Kathmandu", "Nepal", "Freak Street 15", 27.7050, 85.3068, 3,
            "https://images.unsplash.com/photo-1564501049412-61c2a3083791?w=800",
            ["Rooftop Restaurant", "WiFi", "Tour Desk", "Yoga", "Garden"],
            [
                new("r72-1", "h72", "Temple View Room", "Room overlooking ancient temples", 2, 45m, true),
                new("r72-2", "h72", "Himalayan Suite", "Suite with distant Himalayan views", 2, 95m, true),
                new("r72-3", "h72", "Backpacker Room", "Budget room for trekkers", 2, 25m, true)
            ]),

        new("h73", "Miami Art Deco Hotel", "Iconic pastel art deco hotel on Ocean Drive.",
            "Miami", "United States", "Ocean Drive 1234", 25.7806, -80.1300, 4,
            "https://images.unsplash.com/photo-1566665797739-1674de7a421a?w=800",
            ["Beach Access", "Pool", "Restaurant", "Bar", "WiFi", "Valet Parking"],
            [
                new("r73-1", "h73", "Ocean View Room", "Room facing the Atlantic", 2, 285m, true),
                new("r73-2", "h73", "Deco Suite", "Art deco suite with original neon accents", 2, 450m, true),
                new("r73-3", "h73", "Pool View Room", "Room overlooking the pool deck", 2, 210m, true)
            ]),

        new("h74", "Stockholm Archipelago Hotel", "Waterfront hotel in the Stockholm archipelago.",
            "Stockholm", "Sweden", "Strandvägen 7", 59.3334, 18.0785, 4,
            "https://images.unsplash.com/photo-1551882547-ff40c63fe5fa?w=800",
            ["Sauna", "Restaurant", "Bar", "WiFi", "Boat Excursions", "Bike Rental"],
            [
                new("r74-1", "h74", "Archipelago Room", "Room with island views", 2, 225m, true),
                new("r74-2", "h74", "Royal Suite", "Suite overlooking the royal palace", 2, 395m, true),
                new("r74-3", "h74", "Cabin Room", "Nordic cabin-inspired room", 2, 165m, true)
            ]),

        new("h75", "Siem Reap Temple Hotel", "Boutique hotel near the Angkor Wat temples.",
            "Siem Reap", "Cambodia", "Charles de Gaulle Road 44", 13.3633, 103.8600, 3,
            "https://images.unsplash.com/photo-1520250497591-112f2f40a3f4?w=800",
            ["Pool", "Restaurant", "Spa", "WiFi", "Tuk-Tuk Service", "Temple Tours"],
            [
                new("r75-1", "h75", "Temple Room", "Khmer-inspired room with garden", 2, 55m, true),
                new("r75-2", "h75", "Angkor Suite", "Spacious suite with Apsara carvings", 2, 110m, true),
                new("r75-3", "h75", "Budget Room", "Clean budget room near Pub Street", 2, 30m, true)
            ]),

        new("h76", "Scottish Highlands Castle", "Restored 16th-century castle amid heather moorlands.",
            "Inverness", "United Kingdom", "Castle Road 1", 57.4782, -4.2274, 5,
            "https://images.unsplash.com/photo-1566073771259-6a8506099945?w=800",
            ["Restaurant", "Bar", "Whisky Cellar", "Hunting", "Fishing", "WiFi", "Falconry"],
            [
                new("r76-1", "h76", "Laird's Room", "Room in the castle tower with loch views", 2, 350m, true),
                new("r76-2", "h76", "Royal Chamber", "Four-poster bed in the grand chamber", 2, 550m, true),
                new("r76-3", "h76", "Crofter's Cottage", "Cozy cottage room on the estate", 2, 225m, true)
            ]),

        new("h77", "Cairo Pyramids View Hotel", "Hotel with rooftop terrace facing the Great Pyramids.",
            "Cairo", "Egypt", "Al Haram Road 35", 29.9792, 31.1342, 4,
            "https://images.unsplash.com/photo-1542314831-068cd1dbfeeb?w=800",
            ["Rooftop Terrace", "Restaurant", "Pool", "WiFi", "Tour Desk", "Camel Rides"],
            [
                new("r77-1", "h77", "Pyramid View Room", "Room with direct views of the Great Pyramid", 2, 135m, true),
                new("r77-2", "h77", "Pharaoh Suite", "Luxurious suite with Egyptian-themed decor", 2, 255m, true),
                new("r77-3", "h77", "Standard Room", "Clean room near the Giza plateau", 2, 75m, true)
            ]),

        new("h78", "Zurich Lake Hotel", "Elegant lakeside hotel on Lake Zurich.",
            "Zurich", "Switzerland", "Utoquai 45", 47.3601, 8.5477, 5,
            "https://images.unsplash.com/photo-1455587734955-081b22074882?w=800",
            ["Spa", "Restaurant", "Bar", "WiFi", "Gym", "Lake View", "Concierge", "Valet"],
            [
                new("r78-1", "h78", "Lake View Room", "Room overlooking Lake Zurich", 2, 410m, true),
                new("r78-2", "h78", "Alpine Suite", "Suite with lake and Alps panorama", 2, 690m, true),
                new("r78-3", "h78", "Classic Double", "Well-appointed room in a prime location", 2, 310m, true)
            ]),

        new("h79", "Valletta Harbour Hotel", "Limestone hotel overlooking the Grand Harbour.",
            "Valletta", "Malta", "St. Ursula Street 25", 35.8989, 14.5146, 4,
            "https://images.unsplash.com/photo-1568084680786-a84f91d1153c?w=800",
            ["Rooftop Pool", "Restaurant", "Bar", "WiFi", "Tour Desk"],
            [
                new("r79-1", "h79", "Harbour View Room", "Room with Grand Harbour views", 2, 165m, true),
                new("r79-2", "h79", "Knights Suite", "Suite with Maltese cross motifs", 2, 280m, true),
                new("r79-3", "h79", "Budget Room", "Affordable room in the fortified city", 2, 95m, true)
            ]),

        new("h80", "Galway Bay Hotel", "Seaside hotel on Galway's promenade with bay views.",
            "Galway", "Ireland", "Salthill Promenade 12", 53.2590, -9.0820, 3,
            "https://images.unsplash.com/photo-1551016043-7a51f0de0846?w=800",
            ["Restaurant", "Bar", "Live Music", "WiFi", "Bike Rental", "Tours"],
            [
                new("r80-1", "h80", "Bay View Room", "Room overlooking Galway Bay", 2, 135m, true),
                new("r80-2", "h80", "Claddagh Suite", "Suite with Aran Islands views", 2, 225m, true),
                new("r80-3", "h80", "Standard Room", "Comfortable room near the promenade", 2, 95m, true)
            ]),

        new("h81", "Manila Bay Sunset Hotel", "Waterfront hotel with legendary Manila Bay sunsets.",
            "Manila", "Philippines", "Roxas Boulevard 1000", 14.5640, 120.9812, 4,
            "https://images.unsplash.com/photo-1566665797739-1674de7a421a?w=800",
            ["Pool", "Restaurant", "Bar", "Casino", "Spa", "WiFi", "Gym"],
            [
                new("r81-1", "h81", "Bay View Room", "Room facing the famous sunset", 2, 110m, true),
                new("r81-2", "h81", "Presidential Suite", "Grand suite with private dining room", 4, 350m, true),
                new("r81-3", "h81", "Standard Double", "Comfortable room in the city center", 2, 75m, true)
            ]),

        new("h82", "Bhutan Mountain Retreat", "Fortress-style hotel with Himalayan valley views.",
            "Paro", "Bhutan", "Paro Valley Road 3", 27.4305, 89.4127, 4,
            "https://images.unsplash.com/photo-1564501049412-61c2a3083791?w=800",
            ["Restaurant", "Hot Stone Bath", "Meditation", "WiFi", "Trekking", "Cultural Tours"],
            [
                new("r82-1", "h82", "Valley Room", "Room with Paro Valley panorama", 2, 195m, true),
                new("r82-2", "h82", "Dzong Suite", "Suite inspired by Bhutanese fortress design", 2, 340m, true),
                new("r82-3", "h82", "Monastery Room", "Simple room for mindful travelers", 1, 125m, true)
            ]),

        new("h83", "Warsaw Royal Hotel", "Neoclassical hotel on the Royal Route.",
            "Warsaw", "Poland", "Krakowskie Przedmieście 42", 52.2430, 21.0153, 4,
            "https://images.unsplash.com/photo-1571896349842-33c89424de2d?w=800",
            ["Spa", "Restaurant", "Bar", "WiFi", "Gym", "Concierge"],
            [
                new("r83-1", "h83", "Old Town Room", "Room near the reconstructed old town", 2, 125m, true),
                new("r83-2", "h83", "Chopin Suite", "Elegant suite with piano", 2, 240m, true),
                new("r83-3", "h83", "Standard Twin", "Clean modern twin room", 2, 90m, true)
            ]),

        new("h84", "Adelaide Hills Wine Hotel", "Vineyard estate in the Adelaide Hills wine region.",
            "Adelaide", "Australia", "Mount Lofty Summit Road 15", -34.9846, 138.7134, 4,
            "https://images.unsplash.com/photo-1520250497591-112f2f40a3f4?w=800",
            ["Wine Tasting", "Restaurant", "Pool", "Garden", "WiFi", "Hiking"],
            [
                new("r84-1", "h84", "Vineyard Room", "Room overlooking the vines", 2, 195m, true),
                new("r84-2", "h84", "Winemaker Suite", "Suite with private wine cellar", 2, 340m, true),
                new("r84-3", "h84", "Cottage Room", "Converted cottage in the estate", 3, 245m, true)
            ]),

        new("h85", "Bruges Chocolate Hotel", "Chocolate-themed hotel in the city of chocolate.",
            "Brussels", "Belgium", "Grand Place 15", 50.8467, 4.3525, 3,
            "https://images.unsplash.com/photo-1596394516093-501ba68a0ba6?w=800",
            ["Chocolate Workshop", "Restaurant", "Bar", "WiFi", "Tour Desk"],
            [
                new("r85-1", "h85", "Praline Room", "Room with chocolate-making amenities", 2, 135m, true),
                new("r85-2", "h85", "Truffle Suite", "Premium suite with daily chocolate service", 2, 225m, true),
                new("r85-3", "h85", "Budget Room", "Simple room near Grand Place", 2, 85m, true)
            ]),

        new("h86", "Kyoto Zen Temple Stay", "Authentic temple lodging experience in eastern Kyoto.",
            "Kyoto", "Japan", "Higashiyama-ku, Nanzenji 15", 35.0116, 135.7930, 3,
            "https://images.unsplash.com/photo-1542314831-068cd1dbfeeb?w=800",
            ["Zen Garden", "Meditation", "Vegetarian Meals", "Tea Ceremony", "WiFi"],
            [
                new("r86-1", "h86", "Zen Room", "Minimalist tatami room overlooking the garden", 2, 120m, true),
                new("r86-2", "h86", "Abbot's Quarter", "Premium room with private garden view", 2, 210m, true),
                new("r86-3", "h86", "Pilgrim Room", "Simple and serene accommodation", 1, 75m, true)
            ]),

        new("h87", "Split Palace Hotel", "Hotel built into the walls of Diocletian's Palace.",
            "Split", "Croatia", "Peristil Square 1", 43.5081, 16.4402, 4,
            "https://images.unsplash.com/photo-1551016043-7a51f0de0846?w=800",
            ["Restaurant", "Bar", "Terrace", "WiFi", "Tour Desk", "Beach Access"],
            [
                new("r87-1", "h87", "Palace Room", "Room within the ancient palace walls", 2, 185m, true),
                new("r87-2", "h87", "Emperor Suite", "Suite with Roman-era architectural features", 2, 320m, true),
                new("r87-3", "h87", "Waterfront Room", "Room near the Riva promenade", 2, 145m, true)
            ]),

        new("h88", "Tromsø Arctic Hotel", "Northern Norway hotel for whale watching and aurora hunting.",
            "Tromsø", "Norway", "Sjøgata 12", 69.6496, 18.9560, 3,
            "https://images.unsplash.com/photo-1551882547-ff40c63fe5fa?w=800",
            ["Northern Lights Tours", "Restaurant", "Sauna", "WiFi", "Whale Watching", "Dog Sledding"],
            [
                new("r88-1", "h88", "Arctic View Room", "Room with Arctic Cathedral views", 2, 175m, true),
                new("r88-2", "h88", "Aurora Suite", "Suite with skylight for aurora viewing", 2, 310m, true),
                new("r88-3", "h88", "Cozy Cabin", "Warm cabin room with fjord glimpses", 2, 130m, true)
            ]),

        new("h89", "Luxor Nile Hotel", "Riverside hotel near the Valley of the Kings.",
            "Luxor", "Egypt", "Corniche el-Nil 25", 25.6872, 32.6396, 4,
            "https://images.unsplash.com/photo-1582719508461-905c673771fd?w=800",
            ["Pool", "Restaurant", "Nile Terrace", "WiFi", "Temple Tours", "Felucca Rides"],
            [
                new("r89-1", "h89", "Nile View Room", "Room overlooking the Nile River", 2, 95m, true),
                new("r89-2", "h89", "Pharaoh Suite", "Suite with views toward the West Bank", 2, 185m, true),
                new("r89-3", "h89", "Standard Room", "Clean room near Luxor Temple", 2, 60m, true)
            ]),

        new("h90", "Porto Douro Wine Hotel", "Riverfront hotel in the Ribeira district.",
            "Porto", "Portugal", "Cais da Ribeira 32", 41.1406, -8.6132, 4,
            "https://images.unsplash.com/photo-1568084680786-a84f91d1153c?w=800",
            ["Wine Cellar", "Restaurant", "River Terrace", "WiFi", "Port Tasting", "Boat Cruises"],
            [
                new("r90-1", "h90", "Douro View Room", "Room overlooking the Douro River", 2, 155m, true),
                new("r90-2", "h90", "Port Wine Suite", "Suite with private port wine collection", 2, 270m, true),
                new("r90-3", "h90", "Economy Room", "Budget room in the heart of Ribeira", 2, 95m, true)
            ]),

        new("h91", "Lofoten Fisherman's Lodge", "Converted rorbuer on the Norwegian coast.",
            "Reine", "Norway", "Reine Harbor 5", 67.9338, 13.0843, 3,
            "https://images.unsplash.com/photo-1564501049412-61c2a3083791?w=800",
            ["Fishing", "Restaurant", "Sauna", "WiFi", "Kayaking", "Northern Lights"],
            [
                new("r91-1", "h91", "Rorbu Cabin", "Traditional red fisherman's cabin", 2, 155m, true),
                new("r91-2", "h91", "Panorama Suite", "Modern suite with midnight sun views", 2, 280m, true),
                new("r91-3", "h91", "Bunk Room", "Simple bunkbed room for hikers", 2, 85m, true)
            ]),

        new("h92", "Sorrento Cliffside Hotel", "Mediterranean hotel perched above the Bay of Naples.",
            "Sorrento", "Italy", "Via Correale 24", 40.6263, 14.3755, 4,
            "https://images.unsplash.com/photo-1570077188670-e3a8d69ac5ff?w=800",
            ["Pool", "Restaurant", "Lemon Garden", "WiFi", "Beach Elevator", "Boat Tours"],
            [
                new("r92-1", "h92", "Vesuvius View Room", "Room with Vesuvius and bay views", 2, 210m, true),
                new("r92-2", "h92", "Amalfi Suite", "Suite with private terrace and sea views", 2, 380m, true),
                new("r92-3", "h92", "Lemon Room", "Room surrounded by lemon groves", 2, 155m, true)
            ]),

        new("h93", "Kyoto Bamboo Inn", "Traditional inn adjacent to the Arashiyama bamboo grove.",
            "Kyoto", "Japan", "Saga-Tenryuji, Arashiyama 7", 35.0170, 135.6710, 3,
            "https://images.unsplash.com/photo-1566073771259-6a8506099945?w=800",
            ["Onsen", "Garden", "Restaurant", "WiFi", "Bike Rental", "Tea Ceremony"],
            [
                new("r93-1", "h93", "Bamboo Room", "Room overlooking the bamboo forest", 2, 145m, true),
                new("r93-2", "h93", "River View Room", "Room facing the Hozu River", 2, 175m, true),
                new("r93-3", "h93", "Family Tatami", "Large tatami room for families", 5, 220m, true)
            ]),

        new("h94", "Scottsdale Desert Resort", "Luxury desert resort in the Sonoran Desert.",
            "Scottsdale", "United States", "East Lincoln Drive 6000", 33.5310, -111.9480, 5,
            "https://images.unsplash.com/photo-1520250497591-112f2f40a3f4?w=800",
            ["Pool", "Spa", "Golf Course", "Restaurant", "Bar", "WiFi", "Tennis", "Hiking"],
            [
                new("r94-1", "h94", "Desert View Casita", "Private casita with desert panorama", 2, 395m, true),
                new("r94-2", "h94", "Canyon Suite", "Suite with canyon and saguaro views", 3, 650m, true),
                new("r94-3", "h94", "Standard Room", "Comfortable room with desert accents", 2, 280m, true)
            ]),

        new("h95", "Budapest Thermal Hotel", "Grand hotel built over natural thermal springs.",
            "Budapest", "Hungary", "Széchenyi István tér 5", 47.5001, 19.0460, 4,
            "https://images.unsplash.com/photo-1571896349842-33c89424de2d?w=800",
            ["Thermal Baths", "Spa", "Restaurant", "Bar", "WiFi", "Gym"],
            [
                new("r95-1", "h95", "Danube View Room", "Room overlooking the Danube River", 2, 165m, true),
                new("r95-2", "h95", "Thermal Suite", "Suite with private thermal bath", 2, 310m, true),
                new("r95-3", "h95", "Standard Room", "Room with thermal bath access included", 2, 120m, true)
            ]),

        new("h96", "Bora Bora Lagoon Resort", "Overwater bungalows in a turquoise lagoon.",
            "Bora Bora", "French Polynesia", "Matira Point", -16.5004, -151.7415, 5,
            "https://images.unsplash.com/photo-1582719508461-905c673771fd?w=800",
            ["Private Beach", "Spa", "Restaurant", "Diving", "WiFi", "Canoe", "Glass Floor"],
            [
                new("r96-1", "h96", "Overwater Bungalow", "Bungalow with glass floor over coral", 2, 950m, true),
                new("r96-2", "h96", "Royal Villa", "Beachfront villa with private pool", 4, 1800m, true),
                new("r96-3", "h96", "Garden Bungalow", "Bungalow in tropical gardens", 2, 650m, true)
            ]),

        new("h97", "Bruges Belfry Hotel", "Medieval hotel near the famous Belfry tower.",
            "Ghent", "Belgium", "Graslei 7", 51.0543, 3.7210, 3,
            "https://images.unsplash.com/photo-1551016043-7a51f0de0846?w=800",
            ["Restaurant", "Bar", "WiFi", "Bike Rental", "Canal Cruise"],
            [
                new("r97-1", "h97", "Canal Room", "Room overlooking the Graslei canal", 2, 125m, true),
                new("r97-2", "h97", "Belfry Suite", "Suite with castle views", 2, 210m, true),
                new("r97-3", "h97", "Standard Room", "Simple room in the old town", 2, 85m, true)
            ]),

        new("h98", "Dead Sea Spa Resort", "Mineral spa resort at the lowest point on Earth.",
            "Ein Bokek", "Israel", "Dead Sea Road 1", 31.2001, 35.3568, 4,
            "https://images.unsplash.com/photo-1566665797739-1674de7a421a?w=800",
            ["Spa", "Mud Baths", "Pool", "Restaurant", "WiFi", "Beach", "Medical Center"],
            [
                new("r98-1", "h98", "Sea View Room", "Room overlooking the Dead Sea", 2, 195m, true),
                new("r98-2", "h98", "Spa Suite", "Suite with private mineral bath", 2, 350m, true),
                new("r98-3", "h98", "Standard Room", "Room with spa access included", 2, 145m, true)
            ]),

        new("h99", "Queenstown Lakefront Lodge", "Intimate lodge on the shores of Lake Wakatipu.",
            "Queenstown", "New Zealand", "Marine Parade 18", -45.0312, 168.6627, 4,
            "https://images.unsplash.com/photo-1551882547-ff40c63fe5fa?w=800",
            ["Restaurant", "Bar", "Hot Tub", "WiFi", "Kayaking", "Bungy Booking"],
            [
                new("r99-1", "h99", "Lakefront Room", "Room directly on the lake shore", 2, 235m, true),
                new("r99-2", "h99", "Remarkables Suite", "Suite facing The Remarkables mountains", 2, 390m, true),
                new("r99-3", "h99", "Compact Single", "Budget room for solo travelers", 1, 130m, true)
            ]),

        new("h100", "Zanzibar Stone Town Hotel", "Omani-era merchant house in Stone Town.",
            "Stone Town", "Tanzania", "Kenyatta Road 45", -6.1622, 39.1921, 3,
            "https://images.unsplash.com/photo-1520250497591-112f2f40a3f4?w=800",
            ["Rooftop Restaurant", "Pool", "WiFi", "Spice Tours", "Dhow Cruise", "Diving"],
            [
                new("r100-1", "h100", "Sultan Room", "Room with carved Zanzibari door", 2, 85m, true),
                new("r100-2", "h100", "Rooftop Suite", "Suite with Indian Ocean sunset views", 2, 155m, true),
                new("r100-3", "h100", "Courtyard Room", "Simple room around the central courtyard", 2, 55m, true)
            ])
    ];
}
