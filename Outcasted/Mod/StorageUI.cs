using System;
using System.Collections.Generic;
using System.Text;
using ConsoleEscapeFromTarkov.Entities;
using ConsoleEscapeFromTarkov.Items;
using ConsoleEscapeFromTarkov.UI;

namespace ConsoleEscapeFromTarkov.GameCore
{
    /// <summary>
    /// UI handler for storage, market, and merchant interactions
    /// </summary>
    public class StorageUI
    {
        private Player player;
        private PlayerStorage storage;
        private PlayerProgress progress;
        private Market market;
        private List<Merchant> merchants;

        // UI state
        private int selectedStorageIndex;
        private int selectedInventoryIndex;
        private int selectedMarketIndex;
        private int selectedMerchantIndex;
        private int selectedMerchantItemIndex;
        private int currentMerchantTab;
        private int currentTab;
        private int storagePageOffset;
        private int maxItemsPerPage = 15;
        private Merchant activeMerchant;

        // UI constants
        private const int STORAGE_TAB = 0;
        private const int MARKET_TAB = 1;
        private const int MERCHANT_TAB = 2;
        private const int SELL_FROM_STORAGE_TAB = 3;

        /// <summary>
        /// Constructor for StorageUI
        /// </summary>
        /// <param name="player">Player reference</param>
        /// <param name="storage">Storage reference</param>
        /// <param name="progress">Progress reference</param>
        /// <param name="market">Market reference</param>
        public StorageUI(Player player, PlayerStorage storage, PlayerProgress progress, Market market)
        {
            this.player = player;
            this.storage = storage;
            this.progress = progress;
            this.market = market;
            this.merchants = progress.UnlockedMerchants;

            // Initialize state
            selectedStorageIndex = 0;
            selectedInventoryIndex = 0;
            selectedMarketIndex = 0;
            selectedMerchantIndex = 0;
            selectedMerchantItemIndex = 0;
            currentMerchantTab = 0;
            currentTab = STORAGE_TAB;
            storagePageOffset = 0;

            if (merchants.Count > 0)
            {
                activeMerchant = merchants[0];
            }
        }

        /// <summary>
        /// Renders the main menu storage UI
        /// </summary>
        public void RenderMainMenuStorageUI()
        {
            try
            {
                // Clear the screen
                Console.Clear();

                // Draw the cool storage title
                DrawStorageTitle();

                // Draw player info
                DrawPlayerInfoForMainMenu();

                // Draw storage contents 
                DrawStorageContentsForMainMenu();

                // Draw bottom controls
                DrawMainMenuStorageControls();
            }
            catch (Exception ex)
            {
                // Safety catch to prevent freezing
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error displaying Storage UI:");
                Console.WriteLine(ex.Message);
                Console.ResetColor();
                Console.WriteLine("\nPress any key to return to menu...");
            }
        }

        /// <summary>
        /// Draws a cool storage title banner
        /// </summary>
        private void DrawStorageTitle()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(@"
 ██████╗████████╗ ██████╗ ██████╗  █████╗  ██████╗ ███████╗
██╔════╝╚══██╔══╝██╔═══██╗██╔══██╗██╔══██╗██╔════╝ ██╔════╝
╚█████╗    ██║   ██║   ██║██████╔╝███████║██║  ███╗█████╗  
 ╚═══██╗   ██║   ██║   ██║██╔══██╗██╔══██║██║   ██║██╔══╝  
██████╔╝   ██║   ╚██████╔╝██║  ██║██║  ██║╚██████╔╝███████╗
╚═════╝    ╚═╝    ╚═════╝ ╚═╝  ╚═╝╚═╝  ╚═╝ ╚═════╝ ╚══════╝
                                                           ");
            Console.ResetColor();
        }

        /// <summary>
        /// Draws player info for the main menu storage view
        /// </summary>
        private void DrawPlayerInfoForMainMenu()
        {
            Console.WriteLine("\n╔══════════════════════ PLAYER INFO ══════════════════════╗");
            Console.Write("║ Level: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{player.Level}".PadRight(10));
            Console.ResetColor();

            Console.Write(" XP: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{player.Experience}/{player.Level * 1000 + 1000}".PadRight(15));
            Console.ResetColor();

            Console.Write(" Money: ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{progress.Balance}₽".PadRight(10));
            Console.ResetColor();
            Console.WriteLine(" ║");

            Console.Write("║ Raids: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"{progress.TotalRaids}".PadRight(8));
            Console.ResetColor();

            Console.Write(" Extractions: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"{progress.SuccessfulExtractions}".PadRight(8));
            Console.ResetColor();

            Console.Write(" Kills: ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"{progress.TotalKills}".PadRight(8));
            Console.ResetColor();
            Console.WriteLine(" ║");

            Console.WriteLine("╚═════════════════════════════════════════════════════════╝");
        }

        /// <summary>
        /// Draws storage contents for main menu
        /// </summary>
        private void DrawStorageContentsForMainMenu()
        {
            int totalPages = (storage.StoredItems.Count + maxItemsPerPage - 1) / maxItemsPerPage;
            int currentPage = (storagePageOffset / maxItemsPerPage) + 1;

            Console.WriteLine($"\n╔═══════════════ STORAGE ({storage.ItemCount}/{storage.MaxCapacity}) - Page {currentPage}/{Math.Max(1, totalPages)} ════════════════╗");

            if (storage.StoredItems.Count == 0)
            {
                Console.WriteLine("║                    Storage is empty.                     ║");
                Console.WriteLine("║               Start a raid to collect items!             ║");
            }
            else
            {
                // Categorize items for better display
                var weapons = storage.GetWeapons();
                var armors = storage.GetArmor();
                var medkits = storage.GetMedKits();
                var ammo = storage.GetAmmo();
                var valuables = storage.GetValuables();

                Console.WriteLine("║                                                          ║");

                // Display weapons section
                Console.Write("║ ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("WEAPONS: ".PadRight(10));
                Console.ResetColor();

                string weaponText = weapons.Count > 0 ?
                    $"{weapons.Count} (DMG: {weapons.Sum(w => w.Damage)}, Total Value: {weapons.Count * 500}₽)" :
                    "None";
                Console.WriteLine(weaponText.PadRight(48) + " ║");

                // Display armor section
                Console.Write("║ ");
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write("ARMOR: ".PadRight(10));
                Console.ResetColor();

                string armorText = armors.Count > 0 ?
                    $"{armors.Count} (Protection: {armors.Sum(a => a.Protection)}, Total Value: {armors.Count * 300}₽)" :
                    "None";
                Console.WriteLine(armorText.PadRight(48) + " ║");

                // Display medkits section
                Console.Write("║ ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("MEDKITS: ".PadRight(10));
                Console.ResetColor();

                string medkitText = medkits.Count > 0 ?
                    $"{medkits.Count} (Healing: {medkits.Sum(m => m.HealAmount)}, Total Value: {medkits.Count * 150}₽)" :
                    "None";
                Console.WriteLine(medkitText.PadRight(48) + " ║");

                // Display ammo section
                Console.Write("║ ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("AMMO: ".PadRight(10));
                Console.ResetColor();

                string ammoText = ammo.Count > 0 ?
                    $"{ammo.Count} (Total Rounds: {ammo.Sum(a => a.Count)}, Total Value: {ammo.Sum(a => a.Count) * 5}₽)" :
                    "None";
                Console.WriteLine(ammoText.PadRight(48) + " ║");

                // Display valuables section
                Console.Write("║ ");
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write("VALUABLES: ".PadRight(10));
                Console.ResetColor();

                string valuableText = valuables.Count > 0 ?
                    $"{valuables.Count} (Total Value: {valuables.Sum(v => v.Value)}₽)" :
                    "None";
                Console.WriteLine(valuableText.PadRight(48) + " ║");

                Console.WriteLine("║                                                          ║");

                // Display detailed items with pagination
                Console.WriteLine("║ ---- DETAILED ITEM LIST ----                             ║");

                int startIdx = storagePageOffset;
                int endIdx = Math.Min(startIdx + maxItemsPerPage, storage.StoredItems.Count);

                for (int i = startIdx; i < endIdx; i++)
                {
                    Item item = storage.StoredItems[i];
                    string itemDesc = $"[{i + 1:D2}] {item.GetDescription()}";

                    // Colorize based on item type
                    Console.Write("║ ");
                    if (item is Weapon) Console.ForegroundColor = ConsoleColor.Red;
                    else if (item is Armor) Console.ForegroundColor = ConsoleColor.Blue;
                    else if (item is MedKit) Console.ForegroundColor = ConsoleColor.Green;
                    else if (item is Ammo) Console.ForegroundColor = ConsoleColor.Yellow;
                    else if (item is Valuable) Console.ForegroundColor = ConsoleColor.Magenta;

                    Console.Write(itemDesc.PadRight(58));
                    Console.ResetColor();
                    Console.WriteLine(" ║");
                }

                // Fill empty slots for consistent UI
                for (int i = endIdx; i < startIdx + maxItemsPerPage; i++)
                {
                    Console.WriteLine("║                                                          ║");
                }
            }

            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
        }

        /// <summary>
        /// Draws controls for main menu storage UI
        /// </summary>
        private void DrawMainMenuStorageControls()
        {
            Console.WriteLine("\n╔═════════════════════════ CONTROLS ════════════════════════╗");
            Console.WriteLine("║ [ENTER] Start Game with Gear    [ESC] Return to Main Menu ║");
            Console.WriteLine("║ [↑/↓]   Navigate Pages          [S]   Sort Items          ║");
            Console.WriteLine("║ [M]     Visit Merchants                                   ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
        }

        /// <summary>
        /// Renders the storage UI during gameplay
        /// </summary>
        public void RenderStorageUI()
        {
            try
            {
                // Clear the screen
                Console.Clear();

                // Draw header and tabs
                DrawHeader();

                // Draw content based on current tab
                switch (currentTab)
                {
                    case STORAGE_TAB:
                        DrawStorageTab();
                        break;
                    case MARKET_TAB:
                        DrawMarketTab();
                        break;
                    case MERCHANT_TAB:
                        DrawMerchantTab();
                        break;
                    case SELL_FROM_STORAGE_TAB:
                        DrawSellFromStorageTab();
                        break;
                }

                // Draw footer with controls
                DrawFooter();
            }
            catch (Exception ex)
            {
                // Safety catch to prevent freezing
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error displaying Storage UI:");
                Console.WriteLine(ex.Message);
                Console.ResetColor();
                Console.WriteLine("\nPress any key to return to game...");
            }
        }

        /// <summary>
        /// Draws the UI header with tabs
        /// </summary>
        private void DrawHeader()
        {
            // Draw top border
            Console.SetCursorPosition(0, 0);
            Console.Write('┌' + new string('─', 118) + '┐');

            // Draw header with player info
            Console.SetCursorPosition(0, 1);
            Console.Write('│');
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($" Player: Level {player.Level} | Balance: {progress.Balance}₽ ".PadRight(60));
            Console.ResetColor();
            Console.Write($"Health: {player.Health}/{player.MaxHealth} | Storage: {storage.ItemCount}/{storage.MaxCapacity} ".PadRight(58));
            Console.Write('│');

            // Draw tab headers
            Console.SetCursorPosition(0, 2);
            Console.Write('├' + new string('─', 118) + '┤');

            Console.SetCursorPosition(2, 3);
            if (currentTab == STORAGE_TAB)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("[Storage]");
                Console.ResetColor();
            }
            else
            {
                Console.Write(" Storage ");
            }

            Console.SetCursorPosition(15, 3);
            if (currentTab == MARKET_TAB)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("[Market]");
                Console.ResetColor();
            }
            else
            {
                Console.Write(" Market ");
            }

            Console.SetCursorPosition(27, 3);
            if (currentTab == MERCHANT_TAB)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("[Merchants]");
                Console.ResetColor();
            }
            else
            {
                Console.Write(" Merchants ");
            }

            Console.SetCursorPosition(42, 3);
            if (currentTab == SELL_FROM_STORAGE_TAB)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("[Sell From Storage]");
                Console.ResetColor();
            }
            else
            {
                Console.Write(" Sell From Storage ");
            }

            // Draw tab separator
            Console.SetCursorPosition(0, 4);
            Console.Write('├' + new string('─', 118) + '┤');
        }

        /// <summary>
        /// Draws the Storage tab UI
        /// </summary>
        private void DrawStorageTab()
        {
            // Draw storage section
            int storageY = 5;
            Console.SetCursorPosition(2, storageY);
            Console.Write($"Storage ({storage.ItemCount}/{storage.MaxCapacity}):");

            // Draw storage items
            int maxStorageItems = 15;
            for (int i = 0; i < maxStorageItems; i++)
            {
                Console.SetCursorPosition(2, storageY + 1 + i);

                if (i < storage.StoredItems.Count)
                {
                    if (i == selectedStorageIndex)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("► ");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.Write("  ");
                    }

                    Item item = storage.StoredItems[i];
                    if (item is Weapon) Console.ForegroundColor = ConsoleColor.Red;
                    else if (item is Armor) Console.ForegroundColor = ConsoleColor.Blue;
                    else if (item is MedKit) Console.ForegroundColor = ConsoleColor.Green;
                    else if (item is Ammo) Console.ForegroundColor = ConsoleColor.Yellow;
                    else if (item is Valuable) Console.ForegroundColor = ConsoleColor.Magenta;

                    Console.Write($"{i + 1:D2}. {storage.StoredItems[i].GetDescription()}");
                    Console.ResetColor();
                }
                else
                {
                    Console.Write("    ");
                }
            }

            // Draw inventory section
            int inventoryY = 5;
            Console.SetCursorPosition(60, inventoryY);
            Console.Write($"Inventory ({player.Inventory.Count}/{player.MaxInventorySize}):");

            // Draw inventory items
            int maxInventoryItems = 15;
            for (int i = 0; i < maxInventoryItems; i++)
            {
                Console.SetCursorPosition(60, inventoryY + 1 + i);

                if (i < player.Inventory.Count)
                {
                    if (i == selectedInventoryIndex)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("► ");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.Write("  ");
                    }

                    Item item = player.Inventory[i];
                    if (item is Weapon) Console.ForegroundColor = ConsoleColor.Red;
                    else if (item is Armor) Console.ForegroundColor = ConsoleColor.Blue;
                    else if (item is MedKit) Console.ForegroundColor = ConsoleColor.Green;
                    else if (item is Ammo) Console.ForegroundColor = ConsoleColor.Yellow;
                    else if (item is Valuable) Console.ForegroundColor = ConsoleColor.Magenta;

                    Console.Write($"{i + 1:D2}. {player.Inventory[i].GetDescription()}");
                    Console.ResetColor();
                }
                else
                {
                    Console.Write("    ");
                }
            }

            // Draw item details if selected
            int detailsY = 22;
            Console.SetCursorPosition(2, detailsY);
            Console.Write("Item Details:");

            if (currentTab == STORAGE_TAB)
            {
                if (selectedStorageIndex < storage.StoredItems.Count)
                {
                    Item item = storage.StoredItems[selectedStorageIndex];
                    DrawItemDetails(item, 2, detailsY + 1);
                }
            }
            else if (selectedInventoryIndex < player.Inventory.Count)
            {
                Item item = player.Inventory[selectedInventoryIndex];
                DrawItemDetails(item, 2, detailsY + 1);
            }
        }

        /// <summary>
        /// Draws the Market tab UI
        /// </summary>
        private void DrawMarketTab()
        {
            // Draw market listings
            int marketY = 5;
            Console.SetCursorPosition(2, marketY);
            Console.Write("Market Listings:");

            int maxListings = 20;
            for (int i = 0; i < maxListings; i++)
            {
                Console.SetCursorPosition(2, marketY + 1 + i);

                if (i < market.Listings.Count)
                {
                    if (i == selectedMarketIndex)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("► ");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.Write("  ");
                    }

                    MarketListing listing = market.Listings[i];

                    // Color based on item type
                    Item item = listing.Item;
                    if (item is Weapon) Console.ForegroundColor = ConsoleColor.Red;
                    else if (item is Armor) Console.ForegroundColor = ConsoleColor.Blue;
                    else if (item is MedKit) Console.ForegroundColor = ConsoleColor.Green;
                    else if (item is Ammo) Console.ForegroundColor = ConsoleColor.Yellow;
                    else if (item is Valuable) Console.ForegroundColor = ConsoleColor.Magenta;

                    Console.Write($"{i + 1:D2}. {listing.Item.GetDescription()}");
                    Console.ResetColor();
                    Console.Write(" - Price: ");

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($"{listing.Price}₽");
                    Console.ResetColor();
                }
                else
                {
                    Console.Write("    ");
                }
            }

            // Draw item details if selected
            int detailsY = 27;
            Console.SetCursorPosition(2, detailsY);
            Console.Write("Item Details:");

            if (selectedMarketIndex < market.Listings.Count)
            {
                Item item = market.Listings[selectedMarketIndex].Item;
                DrawItemDetails(item, 2, detailsY + 1);
            }

            // Draw inventory preview
            int inventoryY = 5;
            Console.SetCursorPosition(60, inventoryY);
            Console.Write($"Your Storage ({storage.ItemCount}/{storage.MaxCapacity}):");

            int maxInventoryItems = 10;
            for (int i = 0; i < maxInventoryItems; i++)
            {
                Console.SetCursorPosition(60, inventoryY + 1 + i);

                if (i < storage.StoredItems.Count)
                {
                    Item item = storage.StoredItems[i];
                    if (item is Weapon) Console.ForegroundColor = ConsoleColor.Red;
                    else if (item is Armor) Console.ForegroundColor = ConsoleColor.Blue;
                    else if (item is MedKit) Console.ForegroundColor = ConsoleColor.Green;
                    else if (item is Ammo) Console.ForegroundColor = ConsoleColor.Yellow;
                    else if (item is Valuable) Console.ForegroundColor = ConsoleColor.Magenta;

                    Console.Write($"{i + 1:D2}. {storage.StoredItems[i].GetDescription()}");
                    Console.ResetColor();
                }
            }

            // Show money
            Console.SetCursorPosition(60, inventoryY + 12);
            Console.Write($"Your Money: ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{progress.Balance}₽");
            Console.ResetColor();
        }

        /// <summary>
        /// Draws the Merchant tab UI
        /// </summary>
        private void DrawMerchantTab()
        {
            // Draw merchant selection
            int merchantY = 5;
            Console.SetCursorPosition(2, merchantY);
            Console.Write("Available Merchants:");

            for (int i = 0; i < merchants.Count; i++)
            {
                Console.SetCursorPosition(2, merchantY + 1 + i);

                if (i == selectedMerchantIndex)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("► ");
                    Console.ResetColor();
                }
                else
                {
                    Console.Write("  ");
                }

                Merchant merchant = merchants[i];

                // Show merchant type with color
                Console.Write($"{merchant.Name} - ");

                Console.ForegroundColor = GetMerchantColor(merchant.Type);
                Console.Write($"{merchant.Type}");
                Console.ResetColor();
            }

            // Draw merchant details
            if (activeMerchant != null)
            {
                int detailsY = merchantY + merchants.Count + 2;
                Console.SetCursorPosition(2, detailsY);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(activeMerchant.GetRandomGreeting());
                Console.ResetColor();

                Console.SetCursorPosition(2, detailsY + 1);
                Console.Write(activeMerchant.Description);

                // Draw tabs for buy/sell
                Console.SetCursorPosition(2, detailsY + 3);
                if (currentMerchantTab == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("[Buy]");
                    Console.ResetColor();
                    Console.Write(" Sell ");
                }
                else
                {
                    Console.Write(" Buy ");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("[Sell]");
                    Console.ResetColor();
                }

                // Draw items based on current tab
                if (currentMerchantTab == 0) // Buy
                {
                    // Draw merchant inventory
                    int itemsY = detailsY + 5;
                    Console.SetCursorPosition(2, itemsY);
                    Console.Write("Merchant Items:");

                    int maxItems = 10;
                    for (int i = 0; i < maxItems; i++)
                    {
                        Console.SetCursorPosition(2, itemsY + 1 + i);

                        if (i < activeMerchant.SpecialInventory.Count)
                        {
                            if (i == selectedMerchantItemIndex)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.Write("► ");
                                Console.ResetColor();
                            }
                            else
                            {
                                Console.Write("  ");
                            }

                            Item item = activeMerchant.SpecialInventory[i];
                            if (item is Weapon) Console.ForegroundColor = ConsoleColor.Red;
                            else if (item is Armor) Console.ForegroundColor = ConsoleColor.Blue;
                            else if (item is MedKit) Console.ForegroundColor = ConsoleColor.Green;
                            else if (item is Ammo) Console.ForegroundColor = ConsoleColor.Yellow;
                            else if (item is Valuable) Console.ForegroundColor = ConsoleColor.Magenta;

                            Console.Write($"{i + 1:D2}. {item.GetDescription()}");
                            Console.ResetColor();
                            Console.Write(" - Price: ");

                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.Write($"{activeMerchant.CalculateBuyPrice(item, market)}₽");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.Write("    ");
                        }
                    }

                    // Show storage preview
                    int storageY = 5;
                    Console.SetCursorPosition(60, storageY);
                    Console.Write($"Your Storage ({storage.ItemCount}/{storage.MaxCapacity}):");

                    int maxStorageItems = 10;
                    for (int i = 0; i < maxStorageItems; i++)
                    {
                        Console.SetCursorPosition(60, storageY + 1 + i);

                        if (i < storage.StoredItems.Count)
                        {
                            Item item = storage.StoredItems[i];
                            if (item is Weapon) Console.ForegroundColor = ConsoleColor.Red;
                            else if (item is Armor) Console.ForegroundColor = ConsoleColor.Blue;
                            else if (item is MedKit) Console.ForegroundColor = ConsoleColor.Green;
                            else if (item is Ammo) Console.ForegroundColor = ConsoleColor.Yellow;
                            else if (item is Valuable) Console.ForegroundColor = ConsoleColor.Magenta;

                            Console.Write($"{i + 1:D2}. {storage.StoredItems[i].GetDescription()}");
                            Console.ResetColor();
                        }
                    }

                    // Show money
                    Console.SetCursorPosition(60, storageY + 12);
                    Console.Write($"Your Money: ");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($"{progress.Balance}₽");
                    Console.ResetColor();
                }
                else // Sell
                {
                    // Draw player inventory for selling
                    int itemsY = detailsY + 5;
                    Console.SetCursorPosition(2, itemsY);
                    Console.Write("Your Items:");

                    int maxItems = 10;
                    for (int i = 0; i < maxItems; i++)
                    {
                        Console.SetCursorPosition(2, itemsY + 1 + i);

                        if (i < player.Inventory.Count)
                        {
                            if (i == selectedMerchantItemIndex)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.Write("► ");
                                Console.ResetColor();
                            }
                            else
                            {
                                Console.Write("  ");
                            }

                            Item item = player.Inventory[i];
                            if (item is Weapon) Console.ForegroundColor = ConsoleColor.Red;
                            else if (item is Armor) Console.ForegroundColor = ConsoleColor.Blue;
                            else if (item is MedKit) Console.ForegroundColor = ConsoleColor.Green;
                            else if (item is Ammo) Console.ForegroundColor = ConsoleColor.Yellow;
                            else if (item is Valuable) Console.ForegroundColor = ConsoleColor.Magenta;

                            Console.Write($"{i + 1:D2}. {item.GetDescription()}");
                            Console.ResetColor();
                            Console.Write(" - Offer: ");

                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.Write($"{activeMerchant.CalculateSellPrice(item, market)}₽");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.Write("    ");
                        }
                    }

                    // Show advice about selling from storage
                    Console.SetCursorPosition(2, itemsY + 12);
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("TIP: Switch to the 'Sell From Storage' tab to sell items directly from storage!");
                    Console.ResetColor();
                }
            }
        }

        /// <summary>
        /// Draws the Sell From Storage tab UI
        /// </summary>
        private void DrawSellFromStorageTab()
        {
            if (activeMerchant == null && merchants.Count > 0)
            {
                activeMerchant = merchants[0];
                selectedMerchantIndex = 0;
            }

            if (activeMerchant == null)
            {
                Console.SetCursorPosition(2, 10);
                Console.Write("No merchants available. Complete more raids to unlock merchants.");
                return;
            }

            // Draw merchant info
            int merchantY = 5;
            Console.SetCursorPosition(2, merchantY);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"Selling to: {activeMerchant.Name} ({activeMerchant.Type})");
            Console.ResetColor();

            Console.SetCursorPosition(2, merchantY + 1);
            Console.Write(activeMerchant.Description);

            // Draw storage items
            int storageY = merchantY + 3;
            Console.SetCursorPosition(2, storageY);
            Console.Write($"Your Storage Items ({storage.ItemCount}/{storage.MaxCapacity}):");

            int maxItems = 15;
            for (int i = 0; i < maxItems; i++)
            {
                Console.SetCursorPosition(2, storageY + 1 + i);

                if (i < storage.StoredItems.Count)
                {
                    if (i == selectedStorageIndex)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("► ");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.Write("  ");
                    }

                    Item item = storage.StoredItems[i];
                    if (item is Weapon) Console.ForegroundColor = ConsoleColor.Red;
                    else if (item is Armor) Console.ForegroundColor = ConsoleColor.Blue;
                    else if (item is MedKit) Console.ForegroundColor = ConsoleColor.Green;
                    else if (item is Ammo) Console.ForegroundColor = ConsoleColor.Yellow;
                    else if (item is Valuable) Console.ForegroundColor = ConsoleColor.Magenta;

                    Console.Write($"{i + 1:D2}. {item.GetDescription()}");
                    Console.ResetColor();
                    Console.Write(" - Offer: ");

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($"{activeMerchant.CalculateSellPrice(item, market)}₽");
                    Console.ResetColor();
                }
                else
                {
                    Console.Write("    ");
                }
            }

            // Show merchant preferences
            int prefsY = storageY + 17;
            Console.SetCursorPosition(2, prefsY);
            Console.Write("Merchant Preferences:");

            Console.SetCursorPosition(2, prefsY + 1);
            switch (activeMerchant.Type)
            {
                case Merchant.MerchantType.Gunsmith:
                    Console.Write("This merchant offers better prices for ");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("weapons");
                    Console.ResetColor();
                    Console.Write(" and ");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("ammunition");
                    Console.ResetColor();
                    Console.Write(".");
                    break;

                case Merchant.MerchantType.Medic:
                    Console.Write("This merchant offers better prices for ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("medical supplies");
                    Console.ResetColor();
                    Console.Write(".");
                    break;

                case Merchant.MerchantType.Armorer:
                    Console.Write("This merchant offers better prices for ");
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write("armor and protective gear");
                    Console.ResetColor();
                    Console.Write(".");
                    break;

                case Merchant.MerchantType.Fence:
                    Console.Write("This merchant offers better prices for ");
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Write("valuable items");
                    Console.ResetColor();
                    Console.Write(".");
                    break;

                default:
                    Console.Write("This merchant offers standard prices for all items.");
                    break;
            }

            // Show total money
            Console.SetCursorPosition(60, merchantY);
            Console.Write($"Your Money: ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{progress.Balance}₽");
            Console.ResetColor();

            // Show selected item details
            if (selectedStorageIndex < storage.StoredItems.Count)
            {
                Item selectedItem = storage.StoredItems[selectedStorageIndex];
                int sellPrice = activeMerchant.CalculateSellPrice(selectedItem, market);

                Console.SetCursorPosition(60, merchantY + 2);
                Console.Write($"Selected Item:");

                Console.SetCursorPosition(60, merchantY + 3);
                if (selectedItem is Weapon) Console.ForegroundColor = ConsoleColor.Red;
                else if (selectedItem is Armor) Console.ForegroundColor = ConsoleColor.Blue;
                else if (selectedItem is MedKit) Console.ForegroundColor = ConsoleColor.Green;
                else if (selectedItem is Ammo) Console.ForegroundColor = ConsoleColor.Yellow;
                else if (selectedItem is Valuable) Console.ForegroundColor = ConsoleColor.Magenta;

                Console.Write($"{selectedItem.GetDescription()}");
                Console.ResetColor();

                Console.SetCursorPosition(60, merchantY + 4);
                Console.Write($"Sell Price: ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"{sellPrice}₽");
                Console.ResetColor();

                // Display item details
                DrawItemDetails(selectedItem, 60, merchantY + 6);
            }
        }

        /// <summary>
        /// Draws detailed information about an item
        /// </summary>
        /// <param name="item">Item to draw details for</param>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        private void DrawItemDetails(Item item, int x, int y)
        {
            if (item is Weapon weapon)
            {
                Console.SetCursorPosition(x, y);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"Weapon: {weapon.Name}");
                Console.ResetColor();

                Console.SetCursorPosition(x, y + 1);
                Console.Write($"Damage: {weapon.Damage}");

                Console.SetCursorPosition(x, y + 2);
                Console.Write($"Magazine: {weapon.CurrentAmmo}/{weapon.MagazineSize}");

                Console.SetCursorPosition(x, y + 3);
                Console.Write($"Range: {weapon.Range}");

                Console.SetCursorPosition(x, y + 4);
                Console.Write($"Fire Rate: {weapon.FireRate}");

                if (weapon.Spread > 0)
                {
                    Console.SetCursorPosition(x, y + 5);
                    Console.Write($"Spread: {weapon.Spread}");
                }
            }
            else if (item is Armor armor)
            {
                Console.SetCursorPosition(x, y);
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write($"Armor: {armor.Name}");
                Console.ResetColor();

                Console.SetCursorPosition(x, y + 1);
                Console.Write($"Protection: {armor.Protection}");
            }
            else if (item is MedKit medKit)
            {
                Console.SetCursorPosition(x, y);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"Medical: {medKit.Name}");
                Console.ResetColor();

                Console.SetCursorPosition(x, y + 1);
                Console.Write($"Heal Amount: {medKit.HealAmount}");
            }
            else if (item is Ammo ammo)
            {
                Console.SetCursorPosition(x, y);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"Ammo: {ammo.Name}");
                Console.ResetColor();

                Console.SetCursorPosition(x, y + 1);
                Console.Write($"Count: {ammo.Count}");

                Console.SetCursorPosition(x, y + 2);
                Console.Write($"For Weapon: {ammo.WeaponType}");
            }
            else if (item is Valuable valuable)
            {
                Console.SetCursorPosition(x, y);
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write($"Valuable: {valuable.Name}");
                Console.ResetColor();

                Console.SetCursorPosition(x, y + 1);
                Console.Write($"Value: {valuable.Value}₽");

                if (valuable.IsMissionItem)
                {
                    Console.SetCursorPosition(x, y + 2);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("Mission Item");
                    Console.ResetColor();
                }
            }
            else
            {
                Console.SetCursorPosition(x, y);
                Console.Write($"Item: {item.Name}");
            }
        }

        /// <summary>
        /// Draws the UI footer with controls
        /// </summary>
        private void DrawFooter()
        {
            // Draw separator line
            Console.SetCursorPosition(0, 36);
            Console.Write('├' + new string('─', 118) + '┤');

            // Draw controls based on current tab
            Console.SetCursorPosition(2, 37);
            Console.Write("Controls: ");

            switch (currentTab)
            {
                case STORAGE_TAB:
                    Console.Write("[←/→] Switch sections  [↑/↓] Navigate  [ENTER] Transfer item  [U] Upgrade storage");
                    Console.SetCursorPosition(2, 38);
                    Console.Write("[TAB] Switch tabs  [ESC] Return to game");
                    break;

                case MARKET_TAB:
                    Console.Write("[↑/↓] Navigate  [ENTER] Purchase item  [R] Refresh market");
                    Console.SetCursorPosition(2, 38);
                    Console.Write("[TAB] Switch tabs  [ESC] Return to game");
                    break;

                case MERCHANT_TAB:
                    if (currentMerchantTab == 0) // Buy tab
                    {
                        Console.Write("[↑/↓] Navigate  [ENTER] Select merchant/Buy item  [TAB] Switch to Sell");
                    }
                    else // Sell tab
                    {
                        Console.Write("[↑/↓] Navigate  [ENTER] Select merchant/Sell item  [TAB] Switch to Buy");
                    }
                    Console.SetCursorPosition(2, 38);
                    Console.Write("[←/→] Switch tabs  [ESC] Return to game");
                    break;

                case SELL_FROM_STORAGE_TAB:
                    Console.Write("[↑/↓] Navigate  [ENTER] Sell item  [M] Change merchant");
                    Console.SetCursorPosition(2, 38);
                    Console.Write("[←/→] Switch tabs  [ESC] Return to game");
                    break;
            }

            // Draw bottom border
            Console.SetCursorPosition(0, 39);
            Console.Write('└' + new string('─', 118) + '┘');
        }

        /// <summary>
        /// Handles keyboard input for the storage UI
        /// </summary>
        /// <param name="key">Key that was pressed</param>
        /// <returns>True if the UI should close</returns>
        public bool HandleInput(ConsoleKeyInfo key)
        {
            switch (key.Key)
            {
                case ConsoleKey.Tab:
                    // Switch tabs
                    currentTab = (currentTab + 1) % 4;  // Now includes SELL_FROM_STORAGE_TAB
                    return false;

                case ConsoleKey.LeftArrow:
                    if (currentTab == STORAGE_TAB)
                    {
                        // Switch focus between storage and inventory
                        SwitchStorageSections();
                    }
                    else
                    {
                        // Previous tab
                        currentTab = (currentTab + 3) % 4;  // Now includes SELL_FROM_STORAGE_TAB
                    }
                    return false;

                case ConsoleKey.RightArrow:
                    if (currentTab == STORAGE_TAB)
                    {
                        // Switch focus between storage and inventory
                        SwitchStorageSections();
                    }
                    else
                    {
                        // Next tab
                        currentTab = (currentTab + 1) % 4;  // Now includes SELL_FROM_STORAGE_TAB
                    }
                    return false;

                case ConsoleKey.UpArrow:
                    // Navigate up in the current list
                    NavigateUp();
                    return false;

                case ConsoleKey.DownArrow:
                    // Navigate down in the current list
                    NavigateDown();
                    return false;

                case ConsoleKey.Enter:
                    // Action for the current selection
                    HandleEnterKey();
                    return false;

                case ConsoleKey.R:
                    // Refresh market
                    if (currentTab == MARKET_TAB)
                    {
                        market.RefreshMarket(player.Level);
                        selectedMarketIndex = 0;
                    }
                    return false;

                case ConsoleKey.U:
                    // Upgrade storage
                    if (currentTab == STORAGE_TAB)
                    {
                        UpgradeStorage();
                    }
                    return false;

                case ConsoleKey.M:
                    // Change merchant in sell from storage tab
                    if (currentTab == SELL_FROM_STORAGE_TAB)
                    {
                        SelectNextMerchant();
                    }
                    return false;

                case ConsoleKey.Escape:
                    // Exit storage UI
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Handles input for the main menu storage view
        /// </summary>
        /// <param name="key">Key that was pressed</param>
        /// <returns>GameManager action to take</returns>
        public string HandleMainMenuInput(ConsoleKeyInfo key)
        {
            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                    // Navigate storage pages up
                    if (storagePageOffset >= maxItemsPerPage)
                    {
                        storagePageOffset -= maxItemsPerPage;
                    }
                    return "REFRESH";

                case ConsoleKey.DownArrow:
                    // Navigate storage pages down
                    if (storagePageOffset + maxItemsPerPage < storage.StoredItems.Count)
                    {
                        storagePageOffset += maxItemsPerPage;
                    }
                    return "REFRESH";

                case ConsoleKey.Enter:
                    // Start game with gear
                    return "START";

                case ConsoleKey.S:
                    // Sort items
                    SortStorageItems();
                    return "REFRESH";

                case ConsoleKey.M:
                    // Visit merchants
                    return "MERCHANTS";

                case ConsoleKey.Escape:
                    // Return to main menu
                    return "BACK";

                default:
                    return "REFRESH";
            }
        }

        /// <summary>
        /// Switches focus between storage and inventory in the storage tab
        /// </summary>
        private void SwitchStorageSections()
        {
            selectedInventoryIndex = 0;
            selectedStorageIndex = 0;
        }

        /// <summary>
        /// Navigates up in the current list
        /// </summary>
        private void NavigateUp()
        {
            switch (currentTab)
            {
                case STORAGE_TAB:
                    // Navigate storage or inventory
                    if (selectedStorageIndex > 0)
                    {
                        selectedStorageIndex--;
                    }
                    break;

                case MARKET_TAB:
                    // Navigate market listings
                    if (selectedMarketIndex > 0)
                    {
                        selectedMarketIndex--;
                    }
                    break;

                case MERCHANT_TAB:
                    // Navigate merchants or items
                    if (activeMerchant == null)
                    {
                        if (selectedMerchantIndex > 0)
                        {
                            selectedMerchantIndex--;
                        }
                    }
                    else
                    {
                        if (selectedMerchantItemIndex > 0)
                        {
                            selectedMerchantItemIndex--;
                        }
                    }
                    break;

                case SELL_FROM_STORAGE_TAB:
                    // Navigate storage items
                    if (selectedStorageIndex > 0)
                    {
                        selectedStorageIndex--;
                    }
                    break;
            }
        }

        /// <summary>
        /// Navigates down in the current list
        /// </summary>
        private void NavigateDown()
        {
            switch (currentTab)
            {
                case STORAGE_TAB:
                    // Navigate storage or inventory
                    if (selectedStorageIndex < storage.StoredItems.Count - 1)
                    {
                        selectedStorageIndex++;
                    }
                    break;

                case MARKET_TAB:
                    // Navigate market listings
                    if (selectedMarketIndex < market.Listings.Count - 1)
                    {
                        selectedMarketIndex++;
                    }
                    break;

                case MERCHANT_TAB:
                    // Navigate merchants or items
                    if (activeMerchant == null)
                    {
                        if (selectedMerchantIndex < merchants.Count - 1)
                        {
                            selectedMerchantIndex++;
                        }
                    }
                    else
                    {
                        if (currentMerchantTab == 0)
                        {
                            // Buy tab - navigate merchant inventory
                            if (selectedMerchantItemIndex < activeMerchant.SpecialInventory.Count - 1)
                            {
                                selectedMerchantItemIndex++;
                            }
                        }
                        else
                        {
                            // Sell tab - navigate player inventory
                            if (selectedMerchantItemIndex < player.Inventory.Count - 1)
                            {
                                selectedMerchantItemIndex++;
                            }
                        }
                    }
                    break;

                case SELL_FROM_STORAGE_TAB:
                    // Navigate storage items
                    if (selectedStorageIndex < storage.StoredItems.Count - 1)
                    {
                        selectedStorageIndex++;
                    }
                    break;
            }
        }

        /// <summary>
        /// Selects the next merchant in the list
        /// </summary>
        private void SelectNextMerchant()
        {
            if (merchants.Count > 0)
            {
                selectedMerchantIndex = (selectedMerchantIndex + 1) % merchants.Count;
                activeMerchant = merchants[selectedMerchantIndex];
            }
        }

        /// <summary>
        /// Sorts storage items by type
        /// </summary>
        private void SortStorageItems()
        {
            List<Item> sortedItems = new List<Item>();

            // First add weapons
            sortedItems.AddRange(storage.GetWeapons());

            // Then add armor
            sortedItems.AddRange(storage.GetArmor());

            // Then add medkits
            sortedItems.AddRange(storage.GetMedKits());

            // Then add ammo
            sortedItems.AddRange(storage.GetAmmo());

            // Finally add valuables
            sortedItems.AddRange(storage.GetValuables());

            // Replace the storage items with the sorted list
            storage.ReorderItems(sortedItems);
        }

        /// <summary>
        /// Handles the Enter key for the current selection
        /// </summary>
        private void HandleEnterKey()
        {
            switch (currentTab)
            {
                case STORAGE_TAB:
                    // Transfer item between storage and inventory
                    TransferItem();
                    break;

                case MARKET_TAB:
                    // Purchase item from market
                    PurchaseMarketItem();
                    break;

                case MERCHANT_TAB:
                    if (activeMerchant == null)
                    {
                        // Select merchant
                        if (selectedMerchantIndex >= 0 && selectedMerchantIndex < merchants.Count)
                        {
                            activeMerchant = merchants[selectedMerchantIndex];
                            selectedMerchantItemIndex = 0;
                        }
                    }
                    else
                    {
                        if (currentMerchantTab == 0)
                        {
                            // Buy from merchant
                            PurchaseMerchantItem();
                        }
                        else
                        {
                            // Sell to merchant
                            SellToMerchant();
                        }
                    }
                    break;

                case SELL_FROM_STORAGE_TAB:
                    // Sell item from storage
                    SellFromStorage();
                    break;
            }
        }

        /// <summary>
        /// Transfers an item between storage and inventory
        /// </summary>
        private void TransferItem()
        {
            if (selectedStorageIndex < storage.StoredItems.Count)
            {
                // Transfer from storage to inventory
                if (player.Inventory.Count < player.MaxInventorySize)
                {
                    Item item = storage.RemoveItem(selectedStorageIndex);
                    if (item != null)
                    {
                        player.AddToInventory(item);

                        // Adjust selected index if needed
                        if (selectedStorageIndex >= storage.StoredItems.Count)
                        {
                            selectedStorageIndex = Math.Max(0, storage.StoredItems.Count - 1);
                        }
                    }
                }
                else
                {
                    // Display inventory full message
                    Console.SetCursorPosition(2, 35);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("Inventory full!");
                    Console.ResetColor();
                }
            }
            else if (selectedInventoryIndex < player.Inventory.Count)
            {
                // Transfer from inventory to storage
                if (storage.ItemCount < storage.MaxCapacity)
                {
                    Item item = player.Inventory[selectedInventoryIndex];
                    if (storage.AddItem(item))
                    {
                        player.Inventory.RemoveAt(selectedInventoryIndex);

                        // Adjust selected index if needed
                        if (selectedInventoryIndex >= player.Inventory.Count)
                        {
                            selectedInventoryIndex = Math.Max(0, player.Inventory.Count - 1);
                        }
                    }
                }
                else
                {
                    // Display storage full message
                    Console.SetCursorPosition(2, 35);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("Storage full!");
                    Console.ResetColor();
                }
            }
        }

        /// <summary>
        /// Purchases an item from the market
        /// </summary>
        private void PurchaseMarketItem()
        {
            if (selectedMarketIndex >= 0 && selectedMarketIndex < market.Listings.Count)
            {
                MarketListing listing = market.Listings[selectedMarketIndex];

                // Check if player has enough money
                if (progress.Balance < listing.Price)
                {
                    // Display insufficient funds message
                    Console.SetCursorPosition(2, 35);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("Insufficient funds!");
                    Console.ResetColor();
                    return;
                }

                // Purchase the item directly to storage
                Item item = market.PurchaseItem(selectedMarketIndex);
                if (item != null)
                {
                    if (storage.AddItem(item))
                    {
                        // Deduct the cost
                        progress.ModifyBalance(-listing.Price);

                        // Display success message
                        Console.SetCursorPosition(2, 35);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write($"Item purchased and added to storage!");
                        Console.ResetColor();

                        // Adjust selected index if needed
                        if (selectedMarketIndex >= market.Listings.Count)
                        {
                            selectedMarketIndex = Math.Max(0, market.Listings.Count - 1);
                        }
                    }
                    else
                    {
                        // Storage is full
                        Console.SetCursorPosition(2, 35);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("Storage full! Cannot purchase item.");
                        Console.ResetColor();
                    }
                }
            }
        }

        /// <summary>
        /// Purchases an item from the merchant
        /// </summary>
        private void PurchaseMerchantItem()
        {
            if (activeMerchant != null &&
                selectedMerchantItemIndex >= 0 &&
                selectedMerchantItemIndex < activeMerchant.SpecialInventory.Count)
            {
                Item item = activeMerchant.SpecialInventory[selectedMerchantItemIndex];
                int price = activeMerchant.CalculateBuyPrice(item, market);

                // Check if player has enough money
                if (progress.Balance < price)
                {
                    // Display insufficient funds message
                    Console.SetCursorPosition(2, 35);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("Insufficient funds!");
                    Console.ResetColor();
                    return;
                }

                // Purchase directly to storage
                Item purchasedItem = activeMerchant.PurchaseSpecialItem(selectedMerchantItemIndex);
                if (purchasedItem != null)
                {
                    if (storage.AddItem(purchasedItem))
                    {
                        // Deduct the cost
                        progress.ModifyBalance(-price);

                        // Display success message
                        Console.SetCursorPosition(2, 35);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write($"Item purchased and added to storage!");
                        Console.ResetColor();

                        // Adjust selected index if needed
                        if (selectedMerchantItemIndex >= activeMerchant.SpecialInventory.Count)
                        {
                            selectedMerchantItemIndex = Math.Max(0, activeMerchant.SpecialInventory.Count - 1);
                        }
                    }
                    else
                    {
                        // Storage is full
                        Console.SetCursorPosition(2, 35);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("Storage full! Cannot purchase item.");
                        Console.ResetColor();
                    }
                }
            }
        }

        /// <summary>
        /// Sells an item to the merchant
        /// </summary>
        private void SellToMerchant()
        {
            if (activeMerchant != null &&
                selectedMerchantItemIndex >= 0 &&
                selectedMerchantItemIndex < player.Inventory.Count)
            {
                Item item = player.Inventory[selectedMerchantItemIndex];
                int price = activeMerchant.CalculateSellPrice(item, market);

                // Sell the item
                if (progress.SellItemToMerchant(activeMerchant, selectedMerchantItemIndex, market))
                {
                    // Display success message
                    Console.SetCursorPosition(2, 35);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($"Item sold for {price}₽!");
                    Console.ResetColor();

                    // Adjust selected index if needed
                    if (selectedMerchantItemIndex >= player.Inventory.Count)
                    {
                        selectedMerchantItemIndex = Math.Max(0, player.Inventory.Count - 1);
                    }
                }
            }
        }

        /// <summary>
        /// Sells an item directly from storage
        /// </summary>
        private void SellFromStorage()
        {
            if (activeMerchant != null &&
                selectedStorageIndex >= 0 &&
                selectedStorageIndex < storage.StoredItems.Count)
            {
                Item item = storage.StoredItems[selectedStorageIndex];
                int price = activeMerchant.CalculateSellPrice(item, market);

                // Remove item from storage
                storage.RemoveItem(selectedStorageIndex);

                // Add money
                progress.ModifyBalance(price);

                // Display success message
                Console.SetCursorPosition(2, 35);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"Item sold for {price}₽!");
                Console.ResetColor();

                // Adjust selected index
                if (selectedStorageIndex >= storage.StoredItems.Count)
                {
                    selectedStorageIndex = Math.Max(0, storage.StoredItems.Count - 1);
                }
            }
        }

        /// <summary>
        /// Upgrades the storage capacity
        /// </summary>
        private void UpgradeStorage()
        {
            // Calculate upgrade cost
            int additionalSlots = 10;
            int baseCost = 1000;
            int currentSizeFactor = storage.MaxCapacity / 50; // Every 50 slots increases cost
            int totalCost = baseCost * (1 + currentSizeFactor) * additionalSlots / 10;

            // Display confirmation
            Console.SetCursorPosition(2, 35);
            Console.Write($"Upgrade storage by {additionalSlots} slots for {totalCost}₽? (Y/N)");

            ConsoleKeyInfo key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Y)
            {
                // Attempt upgrade
                if (progress.UpgradeStorage(additionalSlots))
                {
                    // Display success message
                    Console.SetCursorPosition(2, 35);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($"Storage upgraded to {storage.MaxCapacity} slots!");
                    Console.ResetColor();
                }
                else
                {
                    // Display insufficient funds message
                    Console.SetCursorPosition(2, 35);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("Insufficient funds!");
                    Console.ResetColor();
                }
            }
            else
            {
                // Clear message
                Console.SetCursorPosition(2, 35);
                Console.Write(new string(' ', 60));
            }
        }

        /// <summary>
        /// Gets the appropriate color for a merchant type
        /// </summary>
        /// <param name="type">Merchant type</param>
        /// <returns>Color for the merchant type</returns>
        private ConsoleColor GetMerchantColor(Merchant.MerchantType type)
        {
            switch (type)
            {
                case Merchant.MerchantType.General:
                    return ConsoleColor.White;
                case Merchant.MerchantType.Gunsmith:
                    return ConsoleColor.Red;
                case Merchant.MerchantType.Medic:
                    return ConsoleColor.Green;
                case Merchant.MerchantType.Armorer:
                    return ConsoleColor.Blue;
                case Merchant.MerchantType.Fence:
                    return ConsoleColor.Magenta;
                default:
                    return ConsoleColor.Gray;
            }
        }
    }
}