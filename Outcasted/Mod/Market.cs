using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleEscapeFromTarkov.Items;

namespace ConsoleEscapeFromTarkov.GameCore
{
    /// <summary>
    /// Manages the marketplace where players can buy and sell items
    /// </summary>
    public class Market
    {
        private List<MarketListing> listings;
        private Random random;

        /// <summary>
        /// All active market listings
        /// </summary>
        public List<MarketListing> Listings => listings;

        /// <summary>
        /// Constructor for Market
        /// </summary>
        public Market()
        {
            listings = new List<MarketListing>();
            random = new Random();
        }

        /// <summary>
        /// Refreshes the market with new random items
        /// </summary>
        /// <param name="playerLevel">Player's level (affects item quality)</param>
        public void RefreshMarket(int playerLevel)
        {
            listings.Clear();

            // Generate weapons
            int weaponCount = 3 + random.Next(3);
            for (int i = 0; i < weaponCount; i++)
            {
                Weapon weapon = GenerateRandomWeapon(playerLevel);
                int price = CalculatePrice(weapon);
                listings.Add(new MarketListing(weapon, price));
            }

            // Generate armor
            int armorCount = 2 + random.Next(3);
            for (int i = 0; i < armorCount; i++)
            {
                Armor armor = GenerateRandomArmor(playerLevel);
                int price = CalculatePrice(armor);
                listings.Add(new MarketListing(armor, price));
            }

            // Generate medkits
            int medkitCount = 3 + random.Next(4);
            for (int i = 0; i < medkitCount; i++)
            {
                MedKit medkit = GenerateRandomMedkit();
                int price = CalculatePrice(medkit);
                listings.Add(new MarketListing(medkit, price));
            }

            // Generate ammo
            int ammoCount = 4 + random.Next(3);
            for (int i = 0; i < ammoCount; i++)
            {
                Ammo ammo = GenerateRandomAmmo();
                int price = CalculatePrice(ammo);
                listings.Add(new MarketListing(ammo, price));
            }
        }

        /// <summary>
        /// Calculates a buy price for an item
        /// </summary>
        /// <param name="item">Item to calculate price for</param>
        /// <returns>Price in game currency</returns>
        public int CalculatePrice(Item item)
        {
            if (item is Weapon weapon)
            {
                // Base price on damage, magazine size, and range
                return weapon.Damage * 100 + weapon.MagazineSize * 50 + weapon.Range * 20;
            }
            else if (item is Armor armor)
            {
                // Base price on protection value
                return armor.Protection * 200;
            }
            else if (item is MedKit medKit)
            {
                // Base price on heal amount
                return medKit.HealAmount * 30;
            }
            else if (item is Ammo ammo)
            {
                // Base price on count
                return ammo.Count * 20;
            }
            else if (item is Valuable valuable)
            {
                // Base price on value (with tax)
                return (int)(valuable.Value * 0.7f);
            }

            return 100; // Default price
        }

        /// <summary>
        /// Calculates a sell price for an item (lower than buy price)
        /// </summary>
        /// <param name="item">Item to calculate price for</param>
        /// <returns>Price in game currency</returns>
        public int CalculateSellPrice(Item item)
        {
            // Sell price is 70% of buy price
            return (int)(CalculatePrice(item) * 0.7f);
        }

        /// <summary>
        /// Purchases an item from the market
        /// </summary>
        /// <param name="listingIndex">Index of the listing to purchase</param>
        /// <returns>The purchased item, or null if invalid index</returns>
        public Item PurchaseItem(int listingIndex)
        {
            if (listingIndex < 0 || listingIndex >= listings.Count)
                return null;

            Item item = listings[listingIndex].Item;
            listings.RemoveAt(listingIndex);
            return item;
        }

        #region Item Generation Methods

        /// <summary>
        /// Generates a random weapon
        /// </summary>
        /// <param name="playerLevel">Player's level (affects quality)</param>
        /// <returns>A randomly generated weapon</returns>
        private Weapon GenerateRandomWeapon(int playerLevel)
        {
            string[] weaponTypes = { "Pistol", "SMG", "Rifle", "Shotgun", "Sniper Rifle" };
            string type = weaponTypes[random.Next(weaponTypes.Length)];

            int damage, magazineSize, range, fireRate, spread;

            // Scale weapon stats with player level
            int levelBonus = (int)(playerLevel * 0.5f);

            switch (type)
            {
                case "Pistol":
                    damage = 15 + random.Next(levelBonus);
                    magazineSize = 8 + random.Next(levelBonus / 2);
                    range = 15 + random.Next(levelBonus);
                    fireRate = 3;
                    spread = 0;
                    break;
                case "SMG":
                    damage = 8 + random.Next(levelBonus);
                    magazineSize = 30 + random.Next(levelBonus);
                    range = 15 + random.Next(levelBonus);
                    fireRate = 1;
                    spread = 0;
                    break;
                case "Rifle":
                    damage = 30 + random.Next(levelBonus);
                    magazineSize = 20 + random.Next(levelBonus / 2);
                    range = 20 + random.Next(levelBonus);
                    fireRate = 3;
                    spread = 0;
                    break;
                case "Shotgun":
                    damage = 40 + random.Next(levelBonus);
                    magazineSize = 6 + random.Next(levelBonus / 3);
                    range = 8 + random.Next(levelBonus / 2);
                    fireRate = 8;
                    spread = 1;
                    break;
                case "Sniper Rifle":
                    damage = 80 + random.Next(levelBonus * 2);
                    magazineSize = 5 + random.Next(levelBonus / 3);
                    range = 40 + random.Next(levelBonus);
                    fireRate = 10;
                    spread = 0;
                    break;
                default:
                    damage = 15;
                    magazineSize = 8;
                    range = 15;
                    fireRate = 3;
                    spread = 0;
                    break;
            }

            // Add quality prefix
            string[] qualities = { "", "Tactical ", "Military ", "Custom ", "Elite " };
            string quality = "";

            if (playerLevel > 5)
                quality = qualities[random.Next(qualities.Length)];

            return new Weapon(quality + type, damage, magazineSize, range, fireRate, spread);
        }

        /// <summary>
        /// Generates random armor
        /// </summary>
        /// <param name="playerLevel">Player's level (affects quality)</param>
        /// <returns>A randomly generated armor</returns>
        private Armor GenerateRandomArmor(int playerLevel)
        {
            string[] armorTypes = { "Light Armor", "Medium Armor", "Heavy Armor", "Tactical Vest", "Helmet" };
            string type = armorTypes[random.Next(armorTypes.Length)];

            // Scale protection with player level
            int baseProtection = 0;
            switch (type)
            {
                case "Light Armor":
                    baseProtection = 10;
                    break;
                case "Medium Armor":
                    baseProtection = 20;
                    break;
                case "Heavy Armor":
                    baseProtection = 30;
                    break;
                case "Tactical Vest":
                    baseProtection = 15;
                    break;
                case "Helmet":
                    baseProtection = 25;
                    break;
            }

            int levelBonus = (int)(playerLevel * 0.5f);
            int protection = baseProtection + random.Next(levelBonus);

            // Add quality prefix
            string[] qualities = { "", "Reinforced ", "Military ", "Elite " };
            string quality = "";

            if (playerLevel > 3)
                quality = qualities[random.Next(qualities.Length)];

            return new Armor(quality + type, protection);
        }

        /// <summary>
        /// Generates a random medkit
        /// </summary>
        /// <returns>A randomly generated medkit</returns>
        private MedKit GenerateRandomMedkit()
        {
            string[] medkitTypes = { "Small Medkit", "Medium Medkit", "Large Medkit", "Combat Medkit", "Emergency Medkit" };
            string type = medkitTypes[random.Next(medkitTypes.Length)];

            int healAmount;
            switch (type)
            {
                case "Small Medkit":
                    healAmount = 30;
                    break;
                case "Medium Medkit":
                    healAmount = 50;
                    break;
                case "Large Medkit":
                    healAmount = 70;
                    break;
                case "Combat Medkit":
                    healAmount = 60;
                    break;
                case "Emergency Medkit":
                    healAmount = 100;
                    break;
                default:
                    healAmount = 30;
                    break;
            }

            return new MedKit(type, healAmount);
        }

        /// <summary>
        /// Generates random ammunition
        /// </summary>
        /// <returns>A randomly generated ammo</returns>
        private Ammo GenerateRandomAmmo()
        {
            string[] weaponTypes = { "Pistol", "SMG", "Rifle", "Shotgun", "Sniper Rifle" };
            string weaponType = weaponTypes[random.Next(weaponTypes.Length)];

            string ammoName;
            int count;

            switch (weaponType)
            {
                case "Pistol":
                    ammoName = "9mm Ammo";
                    count = 30 + random.Next(30);
                    break;
                case "SMG":
                    ammoName = "SMG Ammo";
                    count = 50 + random.Next(50);
                    break;
                case "Rifle":
                    ammoName = "Rifle Ammo";
                    count = 30 + random.Next(30);
                    break;
                case "Shotgun":
                    ammoName = "Shotgun Shells";
                    count = 20 + random.Next(20);
                    break;
                case "Sniper Rifle":
                    ammoName = "Sniper Ammo";
                    count = 15 + random.Next(15);
                    break;
                default:
                    ammoName = "Generic Ammo";
                    count = 30;
                    break;
            }

            return new Ammo(ammoName, weaponType, count);
        }

        #endregion
    }

    /// <summary>
    /// Represents an item listing in the market
    /// </summary>
    public class MarketListing
    {
        /// <summary>
        /// The item being sold
        /// </summary>
        public Item Item { get; private set; }

        /// <summary>
        /// The price of the item
        /// </summary>
        public int Price { get; private set; }

        /// <summary>
        /// Constructor for MarketListing
        /// </summary>
        /// <param name="item">Item being sold</param>
        /// <param name="price">Price of the item</param>
        public MarketListing(Item item, int price)
        {
            Item = item;
            Price = price;
        }
    }
}