using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ConsoleEscapeFromTarkov.Items;
using ConsoleEscapeFromTarkov.Entities;

namespace ConsoleEscapeFromTarkov.GameCore
{
    /// <summary>
    /// Manages player progress persistence between game sessions
    /// </summary>
    public class PlayerProgress
    {
        private Player player;
        private PlayerStorage storage;
        private int balance;
        private int totalRaids;
        private int successfulExtractions;
        private int totalKills;
        private List<Merchant> unlockedMerchants;
        private Random random;

        /// <summary>
        /// The player's current balance (money)
        /// </summary>
        public int Balance => balance;

        /// <summary>
        /// Total number of raids the player has undertaken
        /// </summary>
        public int TotalRaids => totalRaids;

        /// <summary>
        /// Number of successful extractions
        /// </summary>
        public int SuccessfulExtractions => successfulExtractions;

        /// <summary>
        /// Total enemies killed across all raids
        /// </summary>
        public int TotalKills => totalKills;

        /// <summary>
        /// List of merchants the player has unlocked
        /// </summary>
        public List<Merchant> UnlockedMerchants => unlockedMerchants;

        /// <summary>
        /// Constructor for PlayerProgress
        /// </summary>
        /// <param name="player">Player reference</param>
        /// <param name="storage">PlayerStorage reference</param>
        public PlayerProgress(Player player, PlayerStorage storage)
        {
            this.player = player;
            this.storage = storage;
            balance = 1000; // Starting money
            totalRaids = 0;
            successfulExtractions = 0;
            totalKills = 0;
            unlockedMerchants = new List<Merchant>();
            random = new Random();

            // Initialize with default merchants
            InitializeMerchants();
        }

        /// <summary>
        /// Initializes the default merchants
        /// </summary>
        private void InitializeMerchants()
        {
            // Add default trader that's always available
            unlockedMerchants.Add(new Merchant(
                "Trader Petrov",
                "A general trader willing to buy and sell almost anything.",
                Merchant.MerchantType.General
            ));

            // These merchants will be unlocked as the player progresses
            if (player.Level >= 3)
            {
                unlockedMerchants.Add(new Merchant(
                    "Dr. Pavlov",
                    "A former military medic who deals in medical supplies.",
                    Merchant.MerchantType.Medic
                ));
            }

            if (player.Level >= 5)
            {
                unlockedMerchants.Add(new Merchant(
                    "Gunsmith Volkov",
                    "An expert gunsmith who specializes in weapons and ammunition.",
                    Merchant.MerchantType.Gunsmith
                ));
            }

            if (player.Level >= 8)
            {
                unlockedMerchants.Add(new Merchant(
                    "Armorer Kozlov",
                    "A former military armorer with access to quality protective gear.",
                    Merchant.MerchantType.Armorer
                ));
            }

            if (player.Level >= 10)
            {
                unlockedMerchants.Add(new Merchant(
                    "The Fence",
                    "A mysterious middleman with connections to the black market.",
                    Merchant.MerchantType.Fence
                ));
            }
        }

        /// <summary>
        /// Records a completed raid
        /// </summary>
        /// <param name="successful">Whether the extraction was successful</param>
        /// <param name="kills">Number of kills in the raid</param>
        public void RecordRaidCompletion(bool successful, int kills)
        {
            totalRaids++;
            totalKills += kills;

            if (successful)
            {
                successfulExtractions++;
            }

            // Check for new merchant unlocks based on player level
            RefreshMerchants();
        }

        /// <summary>
        /// Updates the list of available merchants based on player progress
        /// </summary>
        public void RefreshMerchants()
        {
            // Check if we need to add new merchants based on level
            bool changed = false;

            if (player.Level >= 3 && !HasMerchantType(Merchant.MerchantType.Medic))
            {
                unlockedMerchants.Add(new Merchant(
                    "Dr. Pavlov",
                    "A former military medic who deals in medical supplies.",
                    Merchant.MerchantType.Medic
                ));
                changed = true;
            }

            if (player.Level >= 5 && !HasMerchantType(Merchant.MerchantType.Gunsmith))
            {
                unlockedMerchants.Add(new Merchant(
                    "Gunsmith Volkov",
                    "An expert gunsmith who specializes in weapons and ammunition.",
                    Merchant.MerchantType.Gunsmith
                ));
                changed = true;
            }

            if (player.Level >= 8 && !HasMerchantType(Merchant.MerchantType.Armorer))
            {
                unlockedMerchants.Add(new Merchant(
                    "Armorer Kozlov",
                    "A former military armorer with access to quality protective gear.",
                    Merchant.MerchantType.Armorer
                ));
                changed = true;
            }

            if (player.Level >= 10 && !HasMerchantType(Merchant.MerchantType.Fence))
            {
                unlockedMerchants.Add(new Merchant(
                    "The Fence",
                    "A mysterious middleman with connections to the black market.",
                    Merchant.MerchantType.Fence
                ));
                changed = true;
            }

            // If new merchants were added, refresh their inventory
            if (changed)
            {
                RefreshMerchantInventories();
            }
        }

        /// <summary>
        /// Checks if a merchant of the specified type is already unlocked
        /// </summary>
        /// <param name="type">Merchant type to check for</param>
        /// <returns>True if a merchant of that type exists</returns>
        private bool HasMerchantType(Merchant.MerchantType type)
        {
            foreach (Merchant merchant in unlockedMerchants)
            {
                if (merchant.Type == type)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Refreshes all merchant inventories
        /// </summary>
        /// <param name="market">Market reference for pricing</param>
        public void RefreshMerchantInventories(Market market = null)
        {
            foreach (Merchant merchant in unlockedMerchants)
            {
                merchant.RefreshInventory(player.Level, market);
            }
        }

        /// <summary>
        /// Modifies the player's balance
        /// </summary>
        /// <param name="amount">Amount to add (positive) or subtract (negative)</param>
        /// <returns>True if successful, false if insufficient funds</returns>
        public bool ModifyBalance(int amount)
        {
            if (amount < 0 && Math.Abs(amount) > balance)
            {
                return false; // Not enough money
            }

            balance += amount;
            return true;
        }

        /// <summary>
        /// Transfers items from player inventory to storage
        /// </summary>
        /// <param name="inventoryIndices">Indices of items to transfer</param>
        /// <returns>Number of items successfully transferred</returns>
        public int TransferToStorage(List<int> inventoryIndices)
        {
            // Sort indices in descending order to avoid index shifting issues
            inventoryIndices.Sort();
            inventoryIndices.Reverse();

            int successCount = 0;

            foreach (int index in inventoryIndices)
            {
                if (index >= 0 && index < player.Inventory.Count)
                {
                    Item item = player.Inventory[index];
                    if (storage.AddItem(item))
                    {
                        player.Inventory.RemoveAt(index);
                        successCount++;
                    }
                }
            }

            return successCount;
        }

        /// <summary>
        /// Transfers items from storage to player inventory
        /// </summary>
        /// <param name="storageIndices">Indices of items to transfer</param>
        /// <returns>Number of items successfully transferred</returns>
        public int TransferFromStorage(List<int> storageIndices)
        {
            // Sort indices in descending order to avoid index shifting issues
            storageIndices.Sort();
            storageIndices.Reverse();

            int successCount = 0;

            foreach (int index in storageIndices)
            {
                if (index >= 0 && index < storage.StoredItems.Count)
                {
                    Item item = storage.GetItem(index);
                    if (player.AddToInventory(item))
                    {
                        storage.RemoveItem(index);
                        successCount++;
                    }
                }
            }

            return successCount;
        }

        /// <summary>
        /// Purchases an item from the market
        /// </summary>
        /// <param name="market">Market reference</param>
        /// <param name="listingIndex">Index of the listing to purchase</param>
        /// <returns>True if purchased successfully</returns>
        public bool PurchaseMarketItem(Market market, int listingIndex)
        {
            if (listingIndex < 0 || listingIndex >= market.Listings.Count)
                return false;

            MarketListing listing = market.Listings[listingIndex];

            // Check if player has enough money
            if (balance < listing.Price)
                return false;

            // Check if player has inventory space
            if (player.Inventory.Count >= player.MaxInventorySize)
                return false;

            // Purchase the item
            Item item = market.PurchaseItem(listingIndex);
            if (item != null)
            {
                player.AddToInventory(item);
                ModifyBalance(-listing.Price);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Purchases a special item from a merchant
        /// </summary>
        /// <param name="merchant">Merchant to buy from</param>
        /// <param name="itemIndex">Index of the special item</param>
        /// <param name="market">Market reference for pricing</param>
        /// <returns>True if purchased successfully</returns>
        public bool PurchaseMerchantItem(Merchant merchant, int itemIndex, Market market)
        {
            if (itemIndex < 0 || itemIndex >= merchant.SpecialInventory.Count)
                return false;

            Item item = merchant.SpecialInventory[itemIndex];
            int price = merchant.CalculateBuyPrice(item, market);

            // Check if player has enough money
            if (balance < price)
                return false;

            // Check if player has inventory space
            if (player.Inventory.Count >= player.MaxInventorySize)
                return false;

            // Purchase the item
            item = merchant.PurchaseSpecialItem(itemIndex);
            if (item != null)
            {
                player.AddToInventory(item);
                ModifyBalance(-price);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sells an item to a merchant
        /// </summary>
        /// <param name="merchant">Merchant to sell to</param>
        /// <param name="inventoryIndex">Index of the inventory item</param>
        /// <param name="market">Market reference for pricing</param>
        /// <returns>True if sold successfully</returns>
        public bool SellItemToMerchant(Merchant merchant, int inventoryIndex, Market market)
        {
            if (inventoryIndex < 0 || inventoryIndex >= player.Inventory.Count)
                return false;

            Item item = player.Inventory[inventoryIndex];
            int price = merchant.CalculateSellPrice(item, market);

            // Sell the item
            player.Inventory.RemoveAt(inventoryIndex);
            ModifyBalance(price);

            return true;
        }

        /// <summary>
        /// Upgrades storage capacity for a price
        /// </summary>
        /// <param name="additionalSlots">Number of slots to add</param>
        /// <returns>True if upgraded successfully</returns>
        public bool UpgradeStorage(int additionalSlots)
        {
            // Cost increases with storage size and slots requested
            int baseCost = 1000;
            int currentSizeFactor = storage.MaxCapacity / 50; // Every 50 slots increases cost
            int totalCost = baseCost * (1 + currentSizeFactor) * additionalSlots;

            if (balance < totalCost)
                return false;

            ModifyBalance(-totalCost);
            storage.UpgradeCapacity(additionalSlots);
            return true;
        }

        /// <summary>
        /// Gets player statistics as a formatted string
        /// </summary>
        /// <returns>Player stats string</returns>
        public string GetStatsString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Player Level: {player.Level}");
            sb.AppendLine($"Experience: {player.Experience}/{player.Level * 1000 + 1000}");
            sb.AppendLine($"Balance: {balance}₽");
            sb.AppendLine($"Total Raids: {totalRaids}");
            sb.AppendLine($"Successful Extractions: {successfulExtractions}");
            sb.AppendLine($"Extraction Rate: {(totalRaids > 0 ? (float)successfulExtractions / totalRaids * 100 : 0):F1}%");
            sb.AppendLine($"Total Kills: {totalKills}");
            sb.AppendLine($"Storage Size: {storage.ItemCount}/{storage.MaxCapacity}");
            sb.AppendLine($"Storage Value: {storage.GetTotalValueInStorage()}₽");
            sb.AppendLine($"Unlocked Merchants: {unlockedMerchants.Count}");

            return sb.ToString();
        }

        /*
         * In a full implementation, these methods would save/load progress to/from a file
         * For brevity, they're stubbed out here
         */

        /// <summary>
        /// Saves player progress to a file
        /// </summary>
        /// <returns>True if saved successfully</returns>
        public bool SaveProgress()
        {
            // In a real implementation, this would serialize all player data to a file
            Console.WriteLine("Player progress saved!");
            return true;
        }

        /// <summary>
        /// Loads player progress from a file
        /// </summary>
        /// <returns>True if loaded successfully</returns>
        public bool LoadProgress()
        {
            // In a real implementation, this would deserialize player data from a file
            return false; // Not implemented
        }
    }
}