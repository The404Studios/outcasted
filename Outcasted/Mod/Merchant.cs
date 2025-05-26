using System;
using System.Collections.Generic;
using ConsoleEscapeFromTarkov.Items;

namespace ConsoleEscapeFromTarkov.GameCore
{
    /// <summary>
    /// Represents a merchant character that buys and sells items
    /// </summary>
    public class Merchant
    {
        private string name;
        private string description;
        private MerchantType type;
        private float buyPriceModifier;  // How much to modify buy prices by (lower = cheaper for player)
        private float sellPriceModifier; // How much to modify sell prices by (higher = more money for player)
        private List<Item> specialInventory;
        private Random random;

        /// <summary>
        /// Types of merchants with different specialties
        /// </summary>
        public enum MerchantType
        {
            /// <summary>General trader who buys and sells everything</summary>
            General,
            /// <summary>Specializes in weapons and ammo</summary>
            Gunsmith,
            /// <summary>Specializes in medical supplies</summary>
            Medic,
            /// <summary>Specializes in armor and protective gear</summary>
            Armorer,
            /// <summary>Offers the best prices for valuables</summary>
            Fence
        }

        /// <summary>
        /// Name of the merchant
        /// </summary>
        public string Name => name;

        /// <summary>
        /// Description of the merchant
        /// </summary>
        public string Description => description;

        /// <summary>
        /// Type of the merchant
        /// </summary>
        public MerchantType Type => type;

        /// <summary>
        /// Special inventory unique to this merchant
        /// </summary>
        public List<Item> SpecialInventory => specialInventory;

        /// <summary>
        /// Constructor for Merchant
        /// </summary>
        /// <param name="name">Name of the merchant</param>
        /// <param name="description">Description of the merchant</param>
        /// <param name="type">Type of merchant</param>
        public Merchant(string name, string description, MerchantType type)
        {
            this.name = name;
            this.description = description;
            this.type = type;
            specialInventory = new List<Item>();
            random = new Random();

            // Set price modifiers based on merchant type
            switch (type)
            {
                case MerchantType.General:
                    buyPriceModifier = 1.0f;  // Standard prices
                    sellPriceModifier = 0.7f; // 70% of value
                    break;
                case MerchantType.Gunsmith:
                    buyPriceModifier = 0.9f;  // 10% discount on weapons/ammo
                    sellPriceModifier = 0.8f; // 80% of value for weapons/ammo
                    break;
                case MerchantType.Medic:
                    buyPriceModifier = 0.9f;  // 10% discount on medical items
                    sellPriceModifier = 0.8f; // 80% of value for medical items
                    break;
                case MerchantType.Armorer:
                    buyPriceModifier = 0.9f;  // 10% discount on armor
                    sellPriceModifier = 0.8f; // 80% of value for armor
                    break;
                case MerchantType.Fence:
                    buyPriceModifier = 1.1f;  // 10% markup on items
                    sellPriceModifier = 0.9f; // 90% of value for valuables
                    break;
                default:
                    buyPriceModifier = 1.0f;
                    sellPriceModifier = 0.7f;
                    break;
            }
        }

        /// <summary>
        /// Calculates buy price for an item (what player pays)
        /// </summary>
        /// <param name="item">The item to price</param>
        /// <param name="market">Market reference for base pricing</param>
        /// <returns>The price the player pays</returns>
        public int CalculateBuyPrice(Item item, Market market)
        {
            int basePrice = market.CalculatePrice(item);
            float modifier = buyPriceModifier;

            // Apply specialty discount if applicable
            if ((type == MerchantType.Gunsmith && (item is Weapon || item is Ammo)) ||
                (type == MerchantType.Medic && item is MedKit) ||
                (type == MerchantType.Armorer && item is Armor))
            {
                modifier = 0.9f; // Extra 10% discount for specialties
            }

            return (int)(basePrice * modifier);
        }

        /// <summary>
        /// Calculates sell price for an item (what player receives)
        /// </summary>
        /// <param name="item">The item to price</param>
        /// <param name="market">Market reference for base pricing</param>
        /// <returns>The price the player receives</returns>
        public int CalculateSellPrice(Item item, Market market)
        {
            int basePrice = market.CalculateSellPrice(item);
            float modifier = sellPriceModifier;

            // Apply specialty bonus if applicable
            if ((type == MerchantType.Gunsmith && (item is Weapon || item is Ammo)) ||
                (type == MerchantType.Medic && item is MedKit) ||
                (type == MerchantType.Armorer && item is Armor) ||
                (type == MerchantType.Fence && item is Valuable))
            {
                modifier = modifier + 0.1f; // Extra 10% for specialties
            }

            return (int)(basePrice * modifier);
        }

        /// <summary>
        /// Refreshes the merchant's special inventory
        /// </summary>
        /// <param name="playerLevel">Player's level (affects quality)</param>
        /// <param name="market">Market reference for pricing</param>
        public void RefreshInventory(int playerLevel, Market market)
        {
            specialInventory.Clear();

            int itemCount = 3 + random.Next(5); // 3-7 special items

            for (int i = 0; i < itemCount; i++)
            {
                Item item = GenerateSpecialItem(playerLevel);
                if (item != null)
                {
                    specialInventory.Add(item);
                }
            }
        }

        /// <summary>
        /// Generates a special item based on merchant type
        /// </summary>
        /// <param name="playerLevel">Player's level (affects quality)</param>
        /// <returns>A special item</returns>
        private Item GenerateSpecialItem(int playerLevel)
        {
            // Generate special items based on merchant type
            switch (type)
            {
                case MerchantType.Gunsmith:
                    // Enhanced weapons with better stats
                    if (random.Next(2) == 0) // 50% chance for weapon
                    {
                        string[] weapons = { "Advanced Pistol", "Tactical SMG", "Military Rifle", "Combat Shotgun", "Precision Sniper" };
                        string weapon = weapons[random.Next(weapons.Length)];

                        // Enhanced stats for special weapons
                        int damage, magazineSize, range, fireRate, spread;

                        switch (weapon)
                        {
                            case "Advanced Pistol":
                                damage = 25 + playerLevel;
                                magazineSize = 12 + (playerLevel / 2);
                                range = 18;
                                fireRate = 2;
                                spread = 0;
                                break;
                            case "Tactical SMG":
                                damage = 12 + playerLevel;
                                magazineSize = 40 + playerLevel;
                                range = 18;
                                fireRate = 1;
                                spread = 0;
                                break;
                            case "Military Rifle":
                                damage = 35 + playerLevel;
                                magazineSize = 30 + (playerLevel / 2);
                                range = 25;
                                fireRate = 2;
                                spread = 0;
                                break;
                            case "Combat Shotgun":
                                damage = 45 + playerLevel;
                                magazineSize = 8 + (playerLevel / 4);
                                range = 10;
                                fireRate = 6;
                                spread = 1;
                                break;
                            case "Precision Sniper":
                                damage = 90 + (playerLevel * 2);
                                magazineSize = 8 + (playerLevel / 4);
                                range = 50;
                                fireRate = 8;
                                spread = 0;
                                break;
                            default:
                                damage = 20;
                                magazineSize = 10;
                                range = 18;
                                fireRate = 3;
                                spread = 0;
                                break;
                        }

                        return new Weapon(weapon, damage, magazineSize, range, fireRate, spread);
                    }
                    else // 50% chance for ammo
                    {
                        string[] ammoTypes = { "High-Powered Pistol Ammo", "Armor-Piercing Rifle Ammo", "Explosive Shotgun Shells", "Match-Grade Sniper Ammo" };
                        string ammoName = ammoTypes[random.Next(ammoTypes.Length)];

                        string weaponType;
                        int count;

                        if (ammoName.Contains("Pistol"))
                        {
                            weaponType = "Pistol";
                            count = 50 + (playerLevel * 5);
                        }
                        else if (ammoName.Contains("Rifle"))
                        {
                            weaponType = "Rifle";
                            count = 40 + (playerLevel * 4);
                        }
                        else if (ammoName.Contains("Shotgun"))
                        {
                            weaponType = "Shotgun";
                            count = 30 + (playerLevel * 3);
                        }
                        else // Sniper
                        {
                            weaponType = "Sniper Rifle";
                            count = 20 + (playerLevel * 2);
                        }

                        return new Ammo(ammoName, weaponType, count);
                    }

                case MerchantType.Medic:
                    // Enhanced medical items
                    string[] medItems = { "Military Medkit", "Combat Stimulant", "Trauma Kit", "Field Surgery Kit" };
                    string medItem = medItems[random.Next(medItems.Length)];

                    int healAmount = 50 + (playerLevel * 5);
                    return new MedKit(medItem, healAmount);

                case MerchantType.Armorer:
                    // Enhanced armor
                    string[] armorTypes = { "Ballistic Vest", "Combat Helmet", "Military-Grade Armor", "Tactical Plate Carrier" };
                    string armor = armorTypes[random.Next(armorTypes.Length)];

                    int protection = 30 + (playerLevel * 2);
                    return new Armor(armor, protection);

                case MerchantType.Fence:
                    // Rare valuables
                    string[] valuables = { "Gold Bar", "Diamond", "Ancient Coin", "Encrypted Data Drive", "Rare Gemstone" };
                    string valuable = valuables[random.Next(valuables.Length)];

                    int value = 5000 + (playerLevel * 1000) + random.Next(2000);
                    return new Valuable(valuable, value);

                case MerchantType.General:
                default:
                    // Random item of any type
                    int itemType = random.Next(4);
                    switch (itemType)
                    {
                        case 0: // Weapon
                            return new Weapon("Custom Weapon", 30 + random.Next(20), 20 + random.Next(15), 20 + random.Next(10), 3, 0);
                        case 1: // Armor
                            return new Armor("Custom Armor", 25 + random.Next(15));
                        case 2: // Medkit
                            return new MedKit("Premium Medkit", 60 + random.Next(40));
                        case 3: // Valuable
                            return new Valuable("Rare Item", 3000 + random.Next(7000));
                        default:
                            return new MedKit("Basic Medkit", 30);
                    }
            }
        }

        /// <summary>
        /// Purchases an item from the merchant's special inventory
        /// </summary>
        /// <param name="index">Index of the item</param>
        /// <returns>The purchased item, or null if invalid index</returns>
        public Item PurchaseSpecialItem(int index)
        {
            if (index < 0 || index >= specialInventory.Count)
                return null;

            Item item = specialInventory[index];
            specialInventory.RemoveAt(index);
            return item;
        }

        /// <summary>
        /// Gets a random greeting from the merchant
        /// </summary>
        /// <returns>A greeting string</returns>
        public string GetRandomGreeting()
        {
            string[] generalGreetings = {
                $"Welcome to {name}'s shop, traveler!",
                "Looking to trade? You've come to the right place.",
                "What can I get for you today?",
                "Ah, a customer! Let's see what we can do for you.",
                "Got some rare items today, take a look!"
            };

            string[] specialGreetings = new string[0];

            // Add type-specific greetings
            switch (type)
            {
                case MerchantType.Gunsmith:
                    specialGreetings = new string[] {
                        "Need firepower? You're in luck!",
                        "Best weapons in the region, guaranteed.",
                        "I've got ammunition that'll tear through anything.",
                        "Every weapon tested personally, I guarantee quality."
                    };
                    break;
                case MerchantType.Medic:
                    specialGreetings = new string[] {
                        "You look like you could use some patching up.",
                        "Got everything you need to stay alive out there.",
                        "My medkits are the difference between life and death.",
                        "Stock up on supplies, you never know when you'll need them."
                    };
                    break;
                case MerchantType.Armorer:
                    specialGreetings = new string[] {
                        "Protection is my specialty, and business is booming.",
                        "This armor has saved countless lives - maybe yours next?",
                        "Don't skimp on protection, the best armor is worth every penny.",
                        "Got some new reinforced vests in, top quality stuff."
                    };
                    break;
                case MerchantType.Fence:
                    specialGreetings = new string[] {
                        "I pay top dollar for your valuables, no questions asked.",
                        "Found something shiny? I'm your buyer.",
                        "I have... connections. Best prices for your special finds.",
                        "Some call me a fence, I prefer 'alternative procurement specialist'."
                    };
                    break;
            }

            // Combine general and special greetings
            string[] allGreetings = new string[generalGreetings.Length + specialGreetings.Length];
            generalGreetings.CopyTo(allGreetings, 0);
            specialGreetings.CopyTo(allGreetings, generalGreetings.Length);

            return allGreetings[random.Next(allGreetings.Length)];
        }
    }
}