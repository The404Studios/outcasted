using System;
using System.Diagnostics;
using System.Threading;
using ConsoleEscapeFromTarkov.Entities;
using ConsoleEscapeFromTarkov.Items;
using ConsoleEscapeFromTarkov.ObjectManagement;
using ConsoleEscapeFromTarkov.UI;
using ConsoleEscapeFromTarkov.Utils;

namespace ConsoleEscapeFromTarkov.GameCore
{
    /// <summary>
    /// Central manager for game state, loop, and component coordination
    /// </summary>
    public class GameManager
    {
        // Game components
        private World world;
        private Player player;
        private UIManager uiManager;
        private InputHandler inputHandler;
        private LootManager lootManager;
        private EnemyManager enemyManager;
        private ObjectManager objectManager;
        private MessageLog messageLog;
        private MissionManager missionManager;
        private WeatherSystem weatherSystem;

        // New persistence components
        private PlayerStorage playerStorage;
        private PlayerProgress playerProgress;
        private Market market;
        private StorageUI storageUI;
        private StarterGear starterGear;

        // Game state
        private bool isRunning;
        private GameState currentState;
        private GameState previousState;
        private Stopwatch frameTimer;
        private long targetFrameTimeMs = 50; // 20 FPS target (increased from 10 FPS to reduce input lag)
        private int gameTime; // In-game time counter
        private Random random;

        /// <summary>
        /// Game state enum defining the different screens/modes
        /// </summary>
        public enum GameState
        {
            MainMenu,
            Playing,
            Inventory,
            Looting,
            GameOver,
            Paused,
            Map,
            Character,
            Help,
            Storage,
            MainMenuStorage,
            GearSelection
        }

        /// <summary>
        /// Constructor for GameManager, initializes all game components
        /// </summary>
        public GameManager()
        {
            isRunning = true;
            currentState = GameState.MainMenu;
            previousState = GameState.MainMenu;
            frameTimer = new Stopwatch();
            gameTime = 0;
            random = new Random();

            // Calculate world size to fit in console window
            int worldWidth = Constants.WorldWidth;
            int worldHeight = Constants.WorldHeight;

            // Initialize object manager first for pooling
            objectManager = new ObjectManager();

            // Initialize message log
            messageLog = new MessageLog(10);

            // Initialize persistence components first
            playerStorage = new PlayerStorage(Constants.InitialStorageCapacity);
            market = new Market();

            // Initialize game components
            world = new World(worldWidth, worldHeight);
            lootManager = new LootManager(objectManager);
            player = new Player(10, 10, world, lootManager, objectManager, messageLog);
            enemyManager = new EnemyManager(world, player, lootManager, objectManager, messageLog);
            missionManager = new MissionManager(player, enemyManager, lootManager);
            weatherSystem = new WeatherSystem();
            inputHandler = new InputHandler();

            // Initialize remaining persistence components
            playerProgress = new PlayerProgress(player, playerStorage);
            starterGear = new StarterGear();

            // Initialize UI components last
            uiManager = new UIManager(world, player, lootManager, enemyManager, messageLog, missionManager, weatherSystem);
            uiManager.SetPersistenceComponents(playerStorage, playerProgress, market);
            storageUI = new StorageUI(player, playerStorage, playerProgress, market);

            // Initial market refresh
            market.RefreshMarket(player.Level);
            playerProgress.RefreshMerchantInventories(market);
        }

        /// <summary>
        /// Starts the game loop
        /// </summary>
        public void StartGame()
        {
            Console.Clear();

            // Initial delay to give the console time to stabilize
            Thread.Sleep(100);

            // Render once to initialize the screen
            uiManager.RenderBufferedGameUI();

            while (isRunning)
            {
                frameTimer.Restart();

                // Handle input based on current state
                HandleInput();

                // Update game state
                Update();

                // Render the current frame
                Render();

                // Wait to maintain stable frame rate
                frameTimer.Stop();
                int sleepTime = (int)(targetFrameTimeMs - frameTimer.ElapsedMilliseconds);
                if (sleepTime > 0)
                {
                    Thread.Sleep(sleepTime);
                }
            }
        }

        #region Input Handling

        /// <summary>
        /// Routes input handling based on current game state
        /// </summary>
        private void HandleInput()
        {
            ConsoleKeyInfo? keyInfo = inputHandler.GetKeyPress();
            if (keyInfo.HasValue)
            {
                switch (currentState)
                {
                    case GameState.MainMenu:
                        HandleMainMenuInput(keyInfo.Value);
                        break;
                    case GameState.Playing:
                        HandleGameplayInput(keyInfo.Value);
                        break;
                    case GameState.Inventory:
                        HandleInventoryInput(keyInfo.Value);
                        break;
                    case GameState.Looting:
                        HandleLootingInput(keyInfo.Value);
                        break;
                    case GameState.GameOver:
                        HandleGameOverInput(keyInfo.Value);
                        break;
                    case GameState.Paused:
                        HandlePausedInput(keyInfo.Value);
                        break;
                    case GameState.Map:
                        HandleMapInput(keyInfo.Value);
                        break;
                    case GameState.Character:
                        HandleCharacterInput(keyInfo.Value);
                        break;
                    case GameState.Help:
                        HandleHelpInput(keyInfo.Value);
                        break;
                    case GameState.Storage:
                        HandleStorageInput(keyInfo.Value);
                        break;
                    case GameState.MainMenuStorage:
                        HandleMainMenuStorageInput(keyInfo.Value);
                        break;
                    case GameState.GearSelection:
                        HandleGearSelectionInput(keyInfo.Value);
                        break;
                }
            }
        }

        private void HandleMainMenuInput(ConsoleKeyInfo keyInfo)
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.Enter:
                    // Show gear selection instead of immediately starting game
                    SetGameState(GameState.GearSelection);
                    break;
                case ConsoleKey.S:
                    // Access storage from main menu
                    SetGameState(GameState.MainMenuStorage);
                    storageUI.RenderMainMenuStorageUI();
                    break;
                case ConsoleKey.H:
                    SetGameState(GameState.Help);
                    break;
                case ConsoleKey.Escape:
                    isRunning = false;
                    break;
            }
        }

        private void HandleGameplayInput(ConsoleKeyInfo keyInfo)
        {
            // Make sure player controls are only processed in gameplay state
            if (currentState != GameState.Playing)
                return;

            switch (keyInfo.Key)
            {
                case ConsoleKey.W:
                    player.Move(0, -1);
                    break;
                case ConsoleKey.S:
                    player.Move(0, 1);
                    break;
                case ConsoleKey.A:
                    player.Move(-1, 0);
                    break;
                case ConsoleKey.D:
                    player.Move(1, 0);
                    break;
                case ConsoleKey.Spacebar:
                    player.Shoot();
                    break;
                case ConsoleKey.I:
                    OpenInventory();
                    break;
                case ConsoleKey.E:
                    TryInteract();
                    break;
                case ConsoleKey.R:
                    player.Reload();
                    break;
                case ConsoleKey.C:
                    SetGameState(GameState.Character);
                    break;
                case ConsoleKey.M:
                    SetGameState(GameState.Map);
                    break;
                case ConsoleKey.H:
                    SetGameState(GameState.Help);
                    break;
                case ConsoleKey.O:
                    // New key for opening storage (easier than Ctrl+S)
                    OpenStorage();
                    break;
                case ConsoleKey.D1:
                case ConsoleKey.D2:
                case ConsoleKey.D3:
                case ConsoleKey.D4:
                case ConsoleKey.D5:
                    // Quick-use items from hotbar (1-5)
                    int slotIndex = (int)keyInfo.Key - (int)ConsoleKey.D1;
                    player.QuickUseItem(slotIndex);
                    break;
                case ConsoleKey.F:
                    // Extract if at extraction point
                    if (world.IsExtractionPoint(player.X, player.Y))
                    {
                        messageLog.AddMessage("Extracting from raid...");
                        Thread.Sleep(1000); // Simulate extraction time
                        EndMission(true);
                    }
                    break;
                case ConsoleKey.Tab:
                    // Cycle through weapon slots
                    player.CycleWeapons();
                    break;
                case ConsoleKey.P:
                    PauseGame();
                    break;
                case ConsoleKey.Escape:
                    currentState = GameState.MainMenu;
                    Console.Clear();
                    break;
            }
        }

        private void HandleInventoryInput(ConsoleKeyInfo keyInfo)
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.I:
                case ConsoleKey.Escape:
                    CloseInventory();
                    break;
                case ConsoleKey.UpArrow:
                    uiManager.MoveInventorySelector(-1);
                    uiManager.RenderInventory();
                    break;
                case ConsoleKey.DownArrow:
                    uiManager.MoveInventorySelector(1);
                    uiManager.RenderInventory();
                    break;
                case ConsoleKey.LeftArrow:
                    uiManager.SwitchInventoryTab(-1);
                    uiManager.RenderInventory();
                    break;
                case ConsoleKey.RightArrow:
                    uiManager.SwitchInventoryTab(1);
                    uiManager.RenderInventory();
                    break;
                case ConsoleKey.Enter:
                    player.UseSelectedItem(uiManager.SelectedInventoryIndex, uiManager.CurrentInventoryTab);
                    uiManager.RenderInventory();
                    break;
                case ConsoleKey.Delete:
                    player.DropSelectedItem(uiManager.SelectedInventoryIndex, uiManager.CurrentInventoryTab);
                    uiManager.RenderInventory();
                    break;
                case ConsoleKey.Tab:
                    player.EquipSelectedItem(uiManager.SelectedInventoryIndex, uiManager.CurrentInventoryTab);
                    uiManager.RenderInventory();
                    break;
                case ConsoleKey.D1:
                case ConsoleKey.D2:
                case ConsoleKey.D3:
                case ConsoleKey.D4:
                case ConsoleKey.D5:
                    // Assign to quickslot
                    int slotIndex = (int)keyInfo.Key - (int)ConsoleKey.D1;
                    player.AssignToQuickSlot(uiManager.SelectedInventoryIndex, uiManager.CurrentInventoryTab, slotIndex);
                    uiManager.RenderInventory();
                    break;
            }
        }

        private void HandleLootingInput(ConsoleKeyInfo keyInfo)
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.E:
                case ConsoleKey.Escape:
                    CloseLootingMenu();
                    break;
                case ConsoleKey.UpArrow:
                    uiManager.MoveLootSelector(-1);
                    uiManager.RenderLootingUI();
                    break;
                case ConsoleKey.DownArrow:
                    uiManager.MoveLootSelector(1);
                    uiManager.RenderLootingUI();
                    break;
                case ConsoleKey.Enter:
                    LootContainer container = player.GetNearbyLootContainer();
                    if (container != null && uiManager.SelectedLootIndex < container.Items.Count)
                    {
                        Item item = container.Items[uiManager.SelectedLootIndex];
                        bool taken = player.TakeLootItem(container, item);

                        if (taken)
                        {
                            messageLog.AddMessage($"Picked up {item.Name}");
                        }
                        else
                        {
                            messageLog.AddMessage("Inventory full!");
                        }

                        // Redraw looting UI to update items
                        uiManager.RenderLootingUI();

                        // If container is now empty, exit looting state
                        if (container.Items.Count == 0)
                        {
                            CloseLootingMenu();
                        }
                    }
                    break;
                case ConsoleKey.T:
                    // Take all items
                    LootContainer cont = player.GetNearbyLootContainer();
                    if (cont != null && cont.Items.Count > 0)
                    {
                        int takenCount = 0;

                        foreach (Item item in cont.Items.ToList())
                        {
                            if (player.TakeLootItem(cont, item))
                            {
                                takenCount++;
                            }
                            else
                            {
                                messageLog.AddMessage("Inventory full!");
                                break;
                            }
                        }

                        if (takenCount > 0)
                        {
                            messageLog.AddMessage($"Took {takenCount} items");
                        }

                        // If container is now empty, exit looting state
                        if (cont.Items.Count == 0)
                        {
                            CloseLootingMenu();
                        }
                        else
                        {
                            // Otherwise redraw looting UI
                            uiManager.RenderLootingUI();
                        }
                    }
                    break;
            }
        }

        private void HandleGameOverInput(ConsoleKeyInfo keyInfo)
        {
            if (keyInfo.Key == ConsoleKey.Enter)
            {
                StartNewGame();
            }
            else if (keyInfo.Key == ConsoleKey.Escape)
            {
                currentState = GameState.MainMenu;
                Console.Clear();
            }
        }

        private void HandlePausedInput(ConsoleKeyInfo keyInfo)
        {
            if (keyInfo.Key == ConsoleKey.P || keyInfo.Key == ConsoleKey.Escape)
            {
                ResumeGame();
            }
            else if (keyInfo.Key == ConsoleKey.S)
            {
                // Open storage from pause menu
                OpenStorage();
            }
            else if (keyInfo.Key == ConsoleKey.M)
            {
                // Exit to main menu
                SetGameState(GameState.MainMenu);
                Console.Clear();
            }
        }

        private void HandleMapInput(ConsoleKeyInfo keyInfo)
        {
            if (keyInfo.Key == ConsoleKey.M || keyInfo.Key == ConsoleKey.Escape)
            {
                CloseMap();
            }
        }

        private void HandleCharacterInput(ConsoleKeyInfo keyInfo)
        {
            if (keyInfo.Key == ConsoleKey.C || keyInfo.Key == ConsoleKey.Escape)
            {
                CloseCharacterScreen();
            }
        }

        private void HandleHelpInput(ConsoleKeyInfo keyInfo)
        {
            if (keyInfo.Key == ConsoleKey.H || keyInfo.Key == ConsoleKey.Escape)
            {
                CloseHelpScreen();
            }
        }

        private void HandleStorageInput(ConsoleKeyInfo keyInfo)
        {
            try
            {
                // Handle storage UI input
                bool shouldClose = storageUI.HandleInput(keyInfo);

                if (shouldClose)
                {
                    CloseStorage();
                }
                else
                {
                    // Redraw storage UI
                    storageUI.RenderStorageUI();
                }
            }
            catch (Exception ex)
            {
                // Fallback in case of error
                messageLog.AddMessage($"Error in storage UI: {ex.Message}");
                CloseStorage();
            }
        }

        private void HandleMainMenuStorageInput(ConsoleKeyInfo keyInfo)
        {
            try
            {
                // Handle main menu storage input
                string action = storageUI.HandleMainMenuInput(keyInfo);

                switch (action)
                {
                    case "START":
                        // Start game with gear selection
                        SetGameState(GameState.GearSelection);
                        break;

                    case "BACK":
                        // Return to main menu
                        SetGameState(GameState.MainMenu);
                        Console.Clear();
                        break;

                    case "MERCHANTS":
                        // Set the storage UI to merchant tab and render
                        SetGameState(GameState.Storage);
                        storageUI.RenderStorageUI();
                        break;

                    case "REFRESH":
                    default:
                        // Refresh the storage UI
                        storageUI.RenderMainMenuStorageUI();
                        break;
                }
            }
            catch (Exception ex)
            {
                // Fallback in case of error
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
                Console.WriteLine("\nPress any key to return to main menu...");
                Console.ReadKey(true);
                SetGameState(GameState.MainMenu);
            }
        }

        private void HandleGearSelectionInput(ConsoleKeyInfo keyInfo)
        {
            // Use the StarterGear system to handle selection
            GearLoadout selectedLoadout = null;

            if (keyInfo.Key == ConsoleKey.Escape)
            {
                // Cancel and return to main menu
                SetGameState(GameState.MainMenu);
                Console.Clear();
                return;
            }
            else if (keyInfo.Key == ConsoleKey.R)
            {
                // Select random loadout
                selectedLoadout = starterGear.GetRandomLoadout();
            }
            else if (keyInfo.KeyChar >= '1' && keyInfo.KeyChar <= '5')
            {
                // Select specific loadout
                int index = keyInfo.KeyChar - '1';
                selectedLoadout = starterGear.GetLoadoutByIndex(index);
            }

            if (selectedLoadout != null)
            {
                // Apply loadout and start game
                starterGear.ApplyLoadoutToPlayer(selectedLoadout, player, playerStorage);
                StartNewGameWithGear(GetPlayer());
            }
        }

        #endregion

        #region Game State Management

        /// <summary>
        /// Updates game state for the current frame
        /// </summary>
        private void Update()
        {
            // Only update game logic if we're in Playing state
            if (currentState == GameState.Playing)
            {
                // Update game time
                gameTime++;

                // Update weather conditions every 500 ticks
                if (gameTime % 500 == 0)
                {
                    weatherSystem.UpdateWeather();
                    messageLog.AddMessage($"Weather changed: {weatherSystem.CurrentWeather.ToString()}");
                }

                // Update mission objectives
                missionManager.Update();

                // Update all game objects
                objectManager.Update();

                // Update game entities
                enemyManager.Update();
                player.Update();

                // Check if player is dead
                if (player.Health <= 0)
                {
                    SetGameState(GameState.GameOver);
                    messageLog.AddMessage("You died!");

                    // Record failed raid
                    playerProgress.RecordRaidCompletion(false, player.KillCount);
                }

                // Spawn additional enemies periodically
                if (gameTime % 500 == 0 && enemyManager.Enemies.Count < 15)
                {
                    enemyManager.SpawnRandomEnemy();
                }

                // Refresh merchant inventories periodically
                if (gameTime % 1000 == 0)
                {
                    playerProgress.RefreshMerchantInventories(market);
                }

                // Refresh market periodically
                if (gameTime % Constants.MarketRefreshTimeInTicks == 0)
                {
                    market.RefreshMarket(player.Level);
                }
            }
        }

        /// <summary>
        /// Renders the current frame based on game state
        /// </summary>
        private void Render()
        {
            switch (currentState)
            {
                case GameState.MainMenu:
                    Console.Clear();
                    uiManager.RenderMainMenu();
                    break;
                case GameState.Playing:
                    // Instead of our buffer system, go back to direct rendering but optimize it
                    world.PrepareForRendering();

                    // Apply weather effects
                    weatherSystem.ApplyWeatherEffects(world);

                    // Render world entities
                    lootManager.Render(world);
                    enemyManager.Render(world);
                    player.Render(world);
                    objectManager.Render(world);

                    // Render map decorations and mission objectives
                    world.RenderMapFeatures();
                    missionManager.RenderObjectives(world);

                    // Use direct but optimized rendering
                    uiManager.RenderWorldDirectly();
                    uiManager.RenderGameUI();
                    break;
                case GameState.Inventory:
                    RenderInventoryScreen();
                    break;
                case GameState.Looting:
                    RenderLootingScreen();
                    break;
                case GameState.GameOver:
                    Console.Clear();
                    uiManager.RenderGameOver();
                    break;
                case GameState.Paused:
                    uiManager.RenderPausedScreen();
                    break;
                case GameState.Map:
                    uiManager.RenderMapScreen();
                    break;
                case GameState.Character:
                    uiManager.RenderCharacterScreen();
                    break;
                case GameState.Help:
                    uiManager.RenderHelpScreen();
                    break;
                case GameState.Storage:
                    // Storage UI handles its own rendering
                    break;
                case GameState.MainMenuStorage:
                    // Main menu storage UI handles its own rendering
                    break;
                case GameState.GearSelection:
                    // Render gear selection screen
                    starterGear.RenderLoadoutSelection();
                    break;
            }
        }

        private void RenderInventoryScreen()
        {
            // For inventory, we keep the world visible underneath
            if (previousState == GameState.Playing)
            {
                // First render the world underneath
                world.PrepareForRendering();
                weatherSystem.ApplyWeatherEffects(world);
                lootManager.Render(world);
                enemyManager.Render(world);
                player.Render(world);
                objectManager.Render(world);
                world.RenderMapFeatures();
                uiManager.RenderWorldEfficient();
            }

            // Then render inventory over top
            uiManager.RenderInventory();
        }

        private void RenderLootingScreen()
        {
            // For looting, we keep the world visible underneath
            if (previousState == GameState.Playing)
            {
                // First render the world underneath
                world.PrepareForRendering();
                weatherSystem.ApplyWeatherEffects(world);
                lootManager.Render(world);
                enemyManager.Render(world);
                player.Render(world);
                objectManager.Render(world);
                world.RenderMapFeatures();
                uiManager.RenderWorldEfficient();
            }

            // Then render looting UI over top
            uiManager.RenderLootingUI();
        }

        #endregion

        #region Game Actions

        /// <summary>
        /// Starts a new game, resetting everything
        /// </summary>
        private void StartNewGame()
        {
            // Reset game time
            gameTime = 0;

            // Reset message log
            messageLog.Clear();

            // Reset object pools
            objectManager.Reset();

            // Generate a new world
            world.Generate();

            // Reset player
            player.Reset(world.Width / 2, world.Height / 2);

            // Generate loot and enemies
            lootManager.GenerateLoot(world);
            enemyManager.GenerateEnemies(world, 10);

            // Set up missions
            missionManager.SetupMissions();

            // Reset weather
            weatherSystem.Reset();

            // Refresh market
            market.RefreshMarket(player.Level);

            // Refresh merchants
            playerProgress.RefreshMerchants();
            playerProgress.RefreshMerchantInventories(market);

            // Welcome message
            messageLog.AddMessage("Welcome to Console Escape from Tarkov!");
            messageLog.AddMessage("Find valuable loot and extract safely.");

            // Clear the screen before starting new game
            Console.Clear();

            SetGameState(GameState.Playing);
        }

        private Player GetPlayer()
        {
            return player;
        }

        /// <summary>
        /// Starts a new game with already selected gear
        /// </summary>
        private void StartNewGameWithGear(Player player)
        {
            // Base game initialization
            gameTime = 0;
            messageLog.Clear();
            objectManager.Reset();
            world.Generate();

            // Don't reset player here since we've already set up gear
            // Just set position
           // player.X = world.Width / 2;
           // player.Y = world.Height / 2;

            // Rest of initialization
            lootManager.GenerateLoot(world);
            enemyManager.GenerateEnemies(world, 10);
            missionManager.SetupMissions();
            weatherSystem.Reset();
            market.RefreshMarket(player.Level);
            playerProgress.RefreshMerchants();
            playerProgress.RefreshMerchantInventories(market);

            // Welcome messages
            messageLog.AddMessage("Welcome to Console Escape from Tarkov!");
            messageLog.AddMessage("Find valuable loot and extract safely.");
            messageLog.AddMessage("Your selected gear has been equipped!");

            Console.Clear();
            SetGameState(GameState.Playing);
        }

        /// <summary>
        /// Ends the current mission/raid
        /// </summary>
        /// <param name="success">Whether the player successfully extracted</param>
        private void EndMission(bool success)
        {
            if (success)
            {
                messageLog.AddMessage("Mission successful! You extracted with your loot.");
                player.AddExperience(500); // Base XP for extraction

                // Add bonus XP for completed objectives
                int completedObjectives = missionManager.GetCompletedObjectivesCount();
                if (completedObjectives > 0)
                {
                    int bonusXP = completedObjectives * 250;
                    player.AddExperience(bonusXP);
                    messageLog.AddMessage($"Completed {completedObjectives} objectives: +{bonusXP} XP");
                }

                // Keep track of statistics and successful extraction
                playerProgress.RecordRaidCompletion(true, player.KillCount);

                // Move all player items to storage automatically
                List<int> allItemIndices = new List<int>();
                for (int i = 0; i < player.Inventory.Count; i++)
                {
                    allItemIndices.Add(i);
                }

                int transferCount = playerProgress.TransferToStorage(allItemIndices);
                messageLog.AddMessage($"Transferred {transferCount} items to storage.");

                // Calculate money gain from valuables
                int moneyGain = 0;
                foreach (Item item in player.Inventory.ToList())
                {
                    if (item is Valuable valuable)
                    {
                        moneyGain += valuable.Value;
                    }
                }

                if (moneyGain > 0)
                {
                    playerProgress.ModifyBalance(moneyGain);
                    messageLog.AddMessage($"Gained {moneyGain}₽ from valuables!");
                }

                // Save player progress
                player.SaveProgress();
                playerProgress.SaveProgress();

                // Show storage UI after successful extraction
                OpenStorage();
            }
            else
            {
                messageLog.AddMessage("Mission failed. You lost all your loot.");
                // Player keeps base stats but loses mission items
                player.ResetMissionItems();

                // Record failed raid
                playerProgress.RecordRaidCompletion(false, player.KillCount);

                // Start a new raid
                StartNewGame();
            }
        }

        private void OpenInventory()
        {
            SetGameState(GameState.Inventory);
        }

        private void CloseInventory()
        {
            // Refresh the screen when returning to gameplay
            Console.Clear();
            SetGameState(GameState.Playing);
        }

        private void OpenStorage()
        {
            try
            {
                // Refresh merchant inventory when opening storage
                market.RefreshMarket(player.Level);
                playerProgress.RefreshMerchantInventories(market);

                SetGameState(GameState.Storage);

                // Render storage UI
                storageUI.RenderStorageUI();
            }
            catch (Exception ex)
            {
                // Log any errors
                messageLog.AddMessage($"Error opening storage: {ex.Message}");
                CloseStorage();
            }
        }

        private void CloseStorage()
        {
            // Refresh the screen when returning to gameplay
            Console.Clear();
            if (previousState == GameState.MainMenu || previousState == GameState.MainMenuStorage)
            {
                SetGameState(GameState.MainMenu);
            }
            else
            {
                SetGameState(GameState.Playing);
            }
        }

        private void TryInteract()
        {
            // Check for loot containers
            LootContainer container = player.GetNearbyLootContainer();
            if (container != null)
            {
                SetGameState(GameState.Looting);
                uiManager.SetActiveLootContainer(container);
                uiManager.ResetLootSelector();
                return;
            }

            // Check for interactable map features
            MapFeature feature = world.GetFeatureAt(player.X, player.Y);
            if (feature != null)
            {
                feature.Interact(player, messageLog);
                return;
            }

            // Check for extraction points
            if (world.IsExtractionPoint(player.X, player.Y))
            {
                messageLog.AddMessage("Press F to extract from the raid");
                return;
            }

            messageLog.AddMessage("Nothing to interact with here.");
        }

        private void CloseLootingMenu()
        {
            // Refresh the screen when returning to gameplay
            Console.Clear();
            SetGameState(GameState.Playing);
        }

        private void CloseMap()
        {
            Console.Clear();
            SetGameState(GameState.Playing);
        }

        private void CloseCharacterScreen()
        {
            Console.Clear();
            SetGameState(GameState.Playing);
        }

        private void CloseHelpScreen()
        {
            if (previousState == GameState.MainMenu)
            {
                Console.Clear();
                SetGameState(GameState.MainMenu);
            }
            else
            {
                Console.Clear();
                SetGameState(GameState.Playing);
            }
        }

        private void PauseGame()
        {
            SetGameState(GameState.Paused);
        }

        private void ResumeGame()
        {
            // Refresh the screen when returning to gameplay
            Console.Clear();
            SetGameState(GameState.Playing);
        }

        /// <summary>
        /// Changes the current game state and stores the previous state
        /// </summary>
        /// <param name="newState">The new game state</param>
        private void SetGameState(GameState newState)
        {
            previousState = currentState;
            currentState = newState;
        }

        #endregion
    }
}