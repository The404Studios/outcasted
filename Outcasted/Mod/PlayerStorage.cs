using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleEscapeFromTarkov.Items;

namespace ConsoleEscapeFromTarkov.GameCore
{
    /// <summary>
    /// Manages persistent storage for the player's items between raids
    /// </summary>
    public class PlayerStorage
    {
        private List<Item> storedItems;
        private int maxStorageCapacity;

        /// <summary>
        /// The current number of items in storage
        /// </summary>
        public int ItemCount => storedItems.Count;

        /// <summary>
        /// The maximum capacity of the storage
        /// </summary>
        public int MaxCapacity => maxStorageCapacity;

        /// <summary>
        /// List of all items in storage
        /// </summary>
        public List<Item> StoredItems => storedItems;

        /// <summary>
        /// Constructor for PlayerStorage
        /// </summary>
        /// <param name="initialCapacity">Initial storage capacity</param>
        public PlayerStorage(int initialCapacity = 50)
        {
            maxStorageCapacity = initialCapacity;
            storedItems = new List<Item>();

            // Add some basic starter items to storage
            AddStarterItems();
        }

        /// <summary>
        /// Adds basic starter items to storage
        /// </summary>
        private void AddStarterItems()
        {
            // Add basic weapons
            storedItems.Add(new Weapon("Pistol", 15, 8, 15, 3));

            // Add basic ammunition
            storedItems.Add(new Ammo("9mm Ammo", "Pistol", 32));

            // Add basic medical supplies
            storedItems.Add(new MedKit("Small Medkit", 30));
            storedItems.Add(new MedKit("Small Medkit", 30));

            // Add basic armor
            storedItems.Add(new Armor("Light Vest", 10));

            // Add some valuables for starting money
            storedItems.Add(new Valuable("Scrap Metal", 50));
            storedItems.Add(new Valuable("Old Watch", 100));
        }

        /// <summary>
        /// Adds an item to storage
        /// </summary>
        /// <param name="item">Item to add</param>
        /// <returns>True if added successfully, false if storage is full</returns>
        public bool AddItem(Item item)
        {
            if (storedItems.Count >= maxStorageCapacity)
                return false;

            storedItems.Add(item);
            return true;
        }

        /// <summary>
        /// Adds multiple items to storage
        /// </summary>
        /// <param name="items">Items to add</param>
        /// <returns>Number of items successfully added</returns>
        public int AddItems(IEnumerable<Item> items)
        {
            int addedCount = 0;

            foreach (Item item in items)
            {
                if (AddItem(item))
                    addedCount++;
                else
                    break;
            }

            return addedCount;
        }

        /// <summary>
        /// Removes an item from storage
        /// </summary>
        /// <param name="index">Index of the item to remove</param>
        /// <returns>The removed item, or null if index is invalid</returns>
        public Item RemoveItem(int index)
        {
            if (index < 0 || index >= storedItems.Count)
                return null;

            Item item = storedItems[index];
            storedItems.RemoveAt(index);
            return item;
        }

        /// <summary>
        /// Gets an item from storage without removing it
        /// </summary>
        /// <param name="index">Index of the item</param>
        /// <returns>The item, or null if index is invalid</returns>
        public Item GetItem(int index)
        {
            if (index < 0 || index >= storedItems.Count)
                return null;

            return storedItems[index];
        }

        /// <summary>
        /// Clears all items from storage
        /// </summary>
        public void Clear()
        {
            storedItems.Clear();
        }

        /// <summary>
        /// Upgrades the maximum storage capacity
        /// </summary>
        /// <param name="additionalCapacity">Amount to increase capacity by</param>
        public void UpgradeCapacity(int additionalCapacity)
        {
            maxStorageCapacity += additionalCapacity;
        }

        /// <summary>
        /// Gets the total value of all valuables in storage
        /// </summary>
        /// <returns>Total value</returns>
        public int GetTotalValueInStorage()
        {
            return storedItems.OfType<Valuable>().Sum(v => v.Value);
        }

        /// <summary>
        /// Gets all weapons in storage
        /// </summary>
        /// <returns>List of weapons</returns>
        public List<Weapon> GetWeapons()
        {
            return storedItems.OfType<Weapon>().ToList();
        }

        /// <summary>
        /// Gets all armor in storage
        /// </summary>
        /// <returns>List of armor</returns>
        public List<Armor> GetArmor()
        {
            return storedItems.OfType<Armor>().ToList();
        }

        /// <summary>
        /// Gets all medkits in storage
        /// </summary>
        /// <returns>List of medkits</returns>
        public List<MedKit> GetMedKits()
        {
            return storedItems.OfType<MedKit>().ToList();
        }

        /// <summary>
        /// Gets all ammo in storage
        /// </summary>
        /// <returns>List of ammo</returns>
        public List<Ammo> GetAmmo()
        {
            return storedItems.OfType<Ammo>().ToList();
        }

        /// <summary>
        /// Gets all valuables in storage
        /// </summary>
        /// <returns>List of valuables</returns>
        public List<Valuable> GetValuables()
        {
            return storedItems.OfType<Valuable>().ToList();
        }

        /// <summary>
        /// Reorders the items in storage
        /// </summary>
        /// <param name="newOrder">New order of items</param>
        public void ReorderItems(List<Item> newOrder)
        {
            // Verify that the new order contains the same items
            if (newOrder.Count != storedItems.Count)
            {
                return;
            }

            // Replace the storage items with the new order
            storedItems.Clear();
            storedItems.AddRange(newOrder);
        }

        /// <summary>
        /// Gets the best weapon of each type in storage
        /// </summary>
        /// <returns>Dictionary of weapon type to best weapon</returns>
        public Dictionary<string, Weapon> GetBestWeapons()
        {
            Dictionary<string, Weapon> bestWeapons = new Dictionary<string, Weapon>();

            foreach (Weapon weapon in GetWeapons())
            {
                string type = GetWeaponType(weapon);

                if (!bestWeapons.ContainsKey(type) ||
                    bestWeapons[type].Damage < weapon.Damage)
                {
                    bestWeapons[type] = weapon;
                }
            }

            return bestWeapons;
        }

        /// <summary>
        /// Gets the best armor in storage
        /// </summary>
        /// <returns>The best armor, or null if none</returns>
        public Armor GetBestArmor()
        {
            List<Armor> armors = GetArmor();

            if (armors.Count == 0)
                return null;

            return armors.OrderByDescending(a => a.Protection).First();
        }

        /// <summary>
        /// Gets the weapon type from the weapon name
        /// </summary>
        /// <param name="weapon">Weapon to get type for</param>
        /// <returns>Type of the weapon</returns>
        private string GetWeaponType(Weapon weapon)
        {
            string name = weapon.Name.ToLower();

            if (name.Contains("pistol"))
                return "Pistol";
            else if (name.Contains("smg"))
                return "SMG";
            else if (name.Contains("shotgun"))
                return "Shotgun";
            else if (name.Contains("rifle"))
                return "Rifle";
            else if (name.Contains("sniper"))
                return "Sniper";
            else
                return "Other";
        }
    }
}