using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using TextAdventure.Core.Extensions;
using TextAdventure.Core.Helpers;
using TextAdventure.Core.Models;

namespace TextAdventureBuilder
{
    public partial class MainWindow : Window
    {
        public List<Screen> Screens { get; set; } = new List<Screen>();
        public List<Companion> Companions { get; set; } = new List<Companion>();
        public List<Item> Items { get; set; } = new List<Item>();
        public List<Shop> Shops { get; set; } = new List<Shop>();

        //Screen Variables
        private Screen _currentScreen;
        private TextOption _currentOption;
        private TextAdventure.Core.Models.Action _currentAction;
        private bool _loadingScreen = false;

        //Companion Variables
        private Companion _currentCompanion;

        //Item Variables
        private Item _currentItem;

        //Shop Variables
        private Shop _currentShop;

        //Save Timers
        DispatcherTimer saveTimer = new DispatcherTimer();
        int saveCounter = 0;

        private bool _exportingAll = false;

        public MainWindow()
        {
            InitializeComponent();

            var handler = new SaveHandler();
            InitializeScreens(handler);
            InitializeCompanions(handler);
            InitializeItems(handler);
            InitializeShops(handler);

            InitializeConditionTypes();
            InitializeActionTypes();

            saveTimer.Interval = TimeSpan.FromSeconds(0.5);
            saveTimer.Tick += SaveTimer_Tick;
        }

        #region Initialization

        private void InitializeScreens(SaveHandler handler)
        {
            Screens = (List<Screen>)handler
                .Load(typeof(List<Screen>), handler.ScreensFile);

            if (Screens == null)
                Screens = new List<Screen>();

            foreach (var obj in Screens)
            {
                var name = string.IsNullOrEmpty(obj.InternalName) ?
                    " - Screen" :
                    " - " + obj.InternalName;

                listScreens.Items.Add(new ListBoxItem
                {
                    Tag = obj.Id,
                    Content = obj.Id + name
                });
            }
        }

        private void InitializeCompanions(SaveHandler handler)
        {
            Companions = (List<Companion>)handler
                .Load(typeof(List<Companion>), handler.CompanionsFile);

            if (Companions == null)
                Companions = new List<Companion>();

            foreach (var obj in Companions)
            {
                var name = string.IsNullOrEmpty(obj.Name) ?
                        " - Companion" :
                        " - " + obj.Name;

                listCompanions.Items.Add(new ListBoxItem
                {
                    Tag = obj.Id,
                    Content = obj.Id + name
                });
            }
        }

        private void InitializeItems(SaveHandler handler)
        {
            Items = (List<Item>)handler
                .Load(typeof(List<Item>), handler.ItemsFile);

            if (Items == null)
                Items = new List<Item>();

            foreach (var obj in Items)
            {
                var name = string.IsNullOrEmpty(obj.Name) ?
                        " - Item" :
                        " - " + obj.Name;

                listItems.Items.Add(new ListBoxItem
                {
                    Tag = obj.Id,
                    Content = obj.Id + name
                });

                listAvailableItems.Items.Add(new ListBoxItem
                {
                    Tag = obj.Id,
                    Content = obj.Id + name
                });
            }
        }

        private void InitializeShops(SaveHandler handler)
        {
            Shops = (List<Shop>)handler
                .Load(typeof(List<Shop>), handler.ShopsFile);

            if (Shops == null)
                Shops = new List<Shop>();

            foreach (var obj in Shops)
            {
                var name = string.IsNullOrEmpty(obj.ShopName) ?
                        " - Shop" :
                        " - " + obj.ShopName;

                listShops.Items.Add(new ListBoxItem
                {
                    Tag = obj.Id,
                    Content = obj.Id + name
                });
            }
        }

        private void InitializeConditionTypes()
        {
            var conditionTypes = EnumUtil.GetValues<ConditionType>();
            foreach (var t in conditionTypes)
            {
                var display = t.ToString();
                var value = (int)t;
                ddlOptionConditionType.Items.Add(new ComboBoxItem
                {
                    Content = display,
                    Tag = value
                });
            }
        }

        private void InitializeActionTypes()
        {
            var actionTypes = EnumUtil.GetValues<ActionType>();
            foreach (var t in actionTypes)
            {
                var display = t.ToString();
                var value = (int)t;
                ddlActionTypes.Items.Add(new ComboBoxItem
                {
                    Content = display,
                    Tag = value
                });
            }
        }

        #endregion

        //----------------------------------------
        //--SCREENS-------------------------------
        //----------------------------------------
        #region Screen Clicks

        private void BtnLoadScreen_Click(object sender, RoutedEventArgs e)
        {
            LoadSelectedScreen();
        }

        private void BtnSaveScreen_Click(object sender, RoutedEventArgs e)
        {
            SaveScreen();
        }

        private void BtnNewScreen_Click(object sender, RoutedEventArgs e)
        {
            NewScreen();
        }

        private void BtnDeleteScreen_Click(object sender, RoutedEventArgs e)
        {
            if (_currentScreen == null ||
                listScreens.SelectedItem == null ||
                !Screens.Any())
                return;

            Screens.RemoveAll(x => x.Id == _currentScreen.Id);
            listScreens.Items.Remove(listScreens.SelectedItem);

            _currentScreen = null;
            _currentOption = null;
            _currentAction = null;
            ClearScreen();
            RenumberScreens();
        }

        private void BtnLoadAction_Click(object sender, RoutedEventArgs e)
        {
            LoadSelectedAction();
        }

        private void BtnNewAction_Click(object sender, RoutedEventArgs e)
        {
            NewAction();
        }

        private void BtnDeleteAction_Click(object sender, RoutedEventArgs e)
        {
            if (_currentOption == null ||
                _currentAction == null ||
                ddlActions.SelectedItem == null ||
                !_currentOption.Actions.Any())
                return;

            _currentOption.Actions.RemoveAll(x => x.Id == _currentOption.Id);
            ddlActions.Items.Remove(ddlActions.SelectedItem);

            _currentAction = null;
            ClearAction();
            RenumberScreens();
        }

        private void BtnLoadOption_Click(object sender, RoutedEventArgs e)
        {
            LoadSelectedOption();
        }

        private void BtnNewOption_Click(object sender, RoutedEventArgs e)
        {
            NewOption();
        }

        private void BtnDeleteOption_Click(object sender, RoutedEventArgs e)
        {
            if (_currentScreen == null ||
                _currentOption == null ||
                listOptions.SelectedItem == null ||
                !_currentScreen.Options.Any())
                return;

            _currentScreen.Options.RemoveAll(x => x.Id == _currentOption.Id);
            listOptions.Items.Remove(listOptions.SelectedItem);

            _currentOption = null;
            _currentAction = null;
            ClearOption();
            RenumberScreens();
        }

        private void SaveScreenEvent(object sender, RoutedEventArgs e)
        {
            SaveScreen();
        }

        #endregion

        #region Screen Logic

        private void RenumberScreens()
        {
            int screenId = 1;
            bool refreshScreens = false, refreshOptions = false, refreshActions = false;
            foreach (var screen in Screens.OrderBy(x => x.Id))
            {
                if (screen.Id != screenId)
                    refreshScreens = true;

                screen.Id = screenId;
                screenId++;

                int optionId = 1;
                foreach (var option in screen.Options.OrderBy(x => x.Id))
                {
                    if (option.Id != optionId)
                        refreshOptions = true;

                    option.Id = optionId;
                    optionId++;

                    int actionId = 1;
                    foreach (var action in option.Actions)
                    {
                        if (action.Id != actionId)
                            refreshOptions = true;

                        action.Id = actionId;
                        actionId++;
                    }
                }
            }

            if (refreshScreens)
            {
                listScreens.Items.Clear();
                foreach (var obj in Screens)
                {
                    var name = string.IsNullOrEmpty(obj.InternalName) ?
                    " - Screen" :
                    " - " + obj.InternalName;

                    listScreens.Items.Add(new ListBoxItem
                    {
                        Tag = obj.Id,
                        Content = obj.Id + name
                    });
                }
            }
            else if (refreshOptions && _currentScreen != null)
            {
                listOptions.Items.Clear();
                foreach (var obj in _currentScreen.Options)
                {
                    listOptions.Items.Add(new ListBoxItem
                    {
                        Tag = obj.Id,
                        Content = "Object - " + obj.Id
                    });
                }
            }
            else if (refreshActions && _currentOption != null)
            {
                ddlActions.Items.Clear();
                foreach (var obj in _currentOption.Actions)
                {
                    ddlActions.Items.Add(new ComboBoxItem
                    {
                        Tag = obj.Id,
                        Content = "Action - " + obj.Id
                    });
                }
            }
        }

        private void LoadSelectedScreen()
        {
            if (listScreens.SelectedItem == null)
            {
                _currentScreen = null;
                return;
            }

            _loadingScreen = true;
            ClearScreen();

            var selectedScreen = listScreens.SelectedItem as ListBoxItem;
            var screenId = Convert.ToInt32(selectedScreen.Tag);
            var screen = Screens.Single(x => x.Id == screenId);
            _currentScreen = screen;

            txtScreenName.Text = screen.InternalName;
            txtScreenIntro.Text = screen.ScreenIntroMessage;

            //Load Options
            listOptions.Items.Clear();
            foreach (var o in screen.Options)
            {
                listOptions.Items.Add(new ListBoxItem
                {
                    Tag = o.Id,
                    Content = "Option - " + o.Id
                });
            }

            if (screen.Options.Any())
            {
                listOptions.SelectedItem = listOptions.Items[0];
                LoadSelectedOption();
            }

            _loadingScreen = false;
        }

        private void LoadSelectedOption()
        {
            if (listOptions.SelectedItem == null || _currentScreen == null)
            {
                _currentOption = null;
                return;
            }

            _loadingScreen = true;
            ClearOption();

            var selectedOption = listOptions.SelectedItem as ListBoxItem;
            var optionId = Convert.ToInt32(selectedOption.Tag);
            if (optionId == -1)
                return;

            var option = _currentScreen.Options.Single(x => x.Id == optionId);
            _currentOption = option;

            txtOptionText.Text = option.Text;
            if (option.ShowCondition != null)
            {
                var conditionType = (int)option.ShowCondition.Type;
                if (conditionType != 0)
                {
                    ddlOptionConditionType.SelectedItem =
                        (from ComboBoxItem item in ddlOptionConditionType.Items
                         where item.Tag.ToString() == conditionType.ToString()
                         select item)
                        .First();
                    txtOptionConditionValue.Text = Convert.ToString(option.ShowCondition.Parameter);
                }
            }
            else
            {
                ddlOptionConditionType.SelectedItem = null;
                txtOptionConditionValue.Text = "";
            }

            //Load Actions
            ddlActions.Items.Clear();
            foreach (var a in option.Actions)
            {
                ddlActions.Items.Add(new ComboBoxItem
                {
                    Tag = a.Id,
                    Content = "Action - " + a.Id
                });
            }

            if (option.Actions.Any())
            {
                ddlActions.SelectedItem = ddlActions.Items[0];
                LoadSelectedAction();
            }

            _loadingScreen = false;
        }

        private void LoadSelectedAction()
        {
            if (ddlActions.SelectedItem == null || _currentOption == null || _currentScreen == null)
            {
                _currentAction = null;
                return;
            }
            _loadingScreen = true;
            ClearAction();

            var selectedAction = ddlActions.SelectedItem as ComboBoxItem;
            var actionId = Convert.ToInt32(selectedAction.Tag);
            var action = _currentOption.Actions.Single(x => x.Id == actionId);
            _currentAction = action;

            var actionType = (int)action.Type;
            if (actionType != 0)
            {
                ddlActionTypes.SelectedItem =
                    (from ComboBoxItem item in ddlActionTypes.Items
                     where item.Tag.ToString() == actionType.ToString()
                     select item)
                    .First();
            }
            txtActionParameter.Text = Convert.ToString(action.Parameter);
            _loadingScreen = false;
        }

        private void NewScreen()
        {
            var newScreen = new Screen();
            newScreen.Id = 1;
            if (Screens.Any())
            {
                newScreen.Id = Screens.Max(x => x.Id) + 1;
            }

            Screens.Add(newScreen);
            _currentScreen = newScreen;
            var newScreenListItem = new ListBoxItem
            {
                Tag = newScreen.Id,
                Content = newScreen.Id + " - Screen"
            };

            listScreens.Items.Add(newScreenListItem);
            foreach (ListBoxItem item in listScreens.Items)
            {
                var tag = (int)item.Tag;
                if (tag == newScreen.Id)
                {
                    listScreens.SelectedItem = item;
                }
            }

            LoadSelectedScreen();
            NewOption();
        }

        private void NewOption()
        {
            if (_currentScreen == null)
                return;

            var newOption = new TextOption();
            newOption.Id = 1;
            if (_currentScreen.Options.Any())
            {
                newOption.Id = _currentScreen.Options.Max(x => x.Id) + 1;
            }
            newOption.ShowCondition.Type = ConditionType.None;
            ddlOptionConditionType.SelectedItem = ddlOptionConditionType.Items[0];

            _currentScreen.Options.Add(newOption);
            _currentOption = newOption;
            var newOptionListItem = new ListBoxItem
            {
                Tag = newOption.Id,
                Content = "Option - " + newOption.Id
            };

            listOptions.Items.Add(newOptionListItem);
            foreach (ListBoxItem item in listOptions.Items)
            {
                var tag = (int)item.Tag;
                if (tag == newOption.Id)
                {
                    listOptions.SelectedItem = item;
                }
            }

            LoadSelectedOption();
            NewAction();
        }

        private void NewAction()
        {
            if (_currentOption == null)
                return;

            var newAction = new TextAdventure.Core.Models.Action();
            newAction.Id = 1;
            if (_currentOption.Actions.Any())
            {
                newAction.Id = _currentOption.Actions.Max(x => x.Id) + 1;
            }

            _currentOption.Actions.Add(newAction);
            _currentAction = newAction;
            var newActionListItem = new ComboBoxItem
            {
                Tag = newAction.Id,
                Content = "Action - " + newAction.Id
            };

            ddlActions.Items.Add(newActionListItem);
            foreach (ComboBoxItem item in ddlActions.Items)
            {
                var tag = (int)item.Tag;
                if (tag == newAction.Id)
                {
                    ddlActions.SelectedItem = item;
                }
            }

            LoadSelectedAction();
            SaveScreen();
        }

        private void SaveScreen()
        {
            if (_currentScreen == null || _loadingScreen)
                return;

            lblSaveSuccess.Visibility = Visibility.Collapsed;
            saveTimer.Stop();
            saveCounter = 0;

            var screen = Screens.Single(x => x.Id == _currentScreen.Id);
            screen.InternalName = txtScreenName.Text;
            screen.ScreenIntroMessage = txtScreenIntro.Text;

            if (_currentOption != null)
            {
                var option = screen.Options.Single(x => x.Id == _currentOption.Id);
                option.Text = txtOptionText.Text;
                if (ddlOptionConditionType.SelectedItem != null)
                {
                    var selectedConditionType = ddlOptionConditionType.SelectedItem as ComboBoxItem;
                    var typeId = Convert.ToInt32(selectedConditionType.Tag);
                    if (typeId > 0)
                    {
                        option.ShowCondition.Type = (ConditionType)typeId;
                        option.ShowCondition.Parameter = txtOptionConditionValue.Text;
                    }
                }

                if (_currentAction != null)
                {
                    var action = _currentOption.Actions.SingleOrDefault(x => x.Id == _currentAction.Id);
                    if (ddlActionTypes.SelectedItem != null && action != null)
                    {
                        var selectedActionType = ddlActionTypes.SelectedItem as ComboBoxItem;
                        var typeId = Convert.ToInt32(selectedActionType.Tag);
                        if (typeId > 0)
                        {
                            action.Type = (ActionType)typeId;
                            action.Parameter = txtActionParameter.Text;
                        }
                    }

                    _currentAction = action;
                }

                _currentOption = option;
            }

            _currentScreen = screen;

            lblSaveSuccess.Visibility = Visibility.Visible;
            saveTimer.Start();
            UpdateNames();
        }

        private void ClearScreen()
        {
            txtScreenName.Text = "";
            txtScreenIntro.Text = "";
            listOptions.Items.Clear();
            ClearOption();
        }

        private void ClearOption()
        {
            txtOptionText.Text = "";
            ddlOptionConditionType.SelectedIndex = 0;
            txtOptionConditionValue.Text = "";
            ddlActions.Items.Clear();
            ClearAction();
        }

        private void ClearAction()
        {
            ddlActionTypes.SelectedItem = null;
            txtActionParameter.Text = "";
        }

        #endregion

        //----------------------------------------
        //--COMPANIONS----------------------------
        //----------------------------------------
        #region Companion Clicks

        private void BtnLoadCompanion_Click(object sender, RoutedEventArgs e)
        {
            LoadCompanion();
        }

        private void BtnNewCompanion_Click(object sender, RoutedEventArgs e)
        {
            NewCompanion();
        }

        private void BtnDeleteCompanion_Click(object sender, RoutedEventArgs e)
        {
            DeleteCompanion();
        }

        private void BtnSaveCompanions_Click(object sender, RoutedEventArgs e)
        {
            SaveCompanions();
        }

        private void SaveCompanionEvent(object sender, RoutedEventArgs e)
        {
            SaveCompanions();
        }

        #endregion

        #region Companion Logic

        private void LoadCompanion()
        {
            if (listCompanions.SelectedItem == null)
            {
                _currentCompanion = null;
                return;
            }

            ClearCompanion();

            var selectedCompanion = listCompanions.SelectedItem as ListBoxItem;
            var companionId = Convert.ToInt32(selectedCompanion.Tag);
            var companion = Companions.Single(x => x.Id == companionId);
            _currentCompanion = companion;

            txtCompanionName.Text = companion.Name;
        }

        private void NewCompanion()
        {
            var newCompanion = new Companion();
            newCompanion.Id = 1;
            if (Companions.Any())
            {
                newCompanion.Id = Companions.Max(x => x.Id) + 1;
            }

            Companions.Add(newCompanion);
            _currentCompanion = newCompanion;
            var newCompanionListItem = new ListBoxItem
            {
                Tag = newCompanion.Id,
                Content = newCompanion.Id + " - Companion"
            };

            listCompanions.Items.Add(newCompanionListItem);
            foreach (ListBoxItem item in listCompanions.Items)
            {
                var tag = (int)item.Tag;
                if (tag == newCompanion.Id)
                {
                    listCompanions.SelectedItem = item;
                }
            }

            LoadCompanion();
            SaveCompanions();
        }

        private void DeleteCompanion()
        {
            if (_currentCompanion == null ||
                listCompanions.SelectedItem == null ||
                !Companions.Any())
                return;

            Companions.RemoveAll(x => x.Id == _currentCompanion.Id);
            listCompanions.Items.Remove(listCompanions.SelectedItem);

            _currentCompanion = null;
            ClearCompanion();
            RenumberCompanions();
        }

        private void SaveCompanions()
        {
            if (_currentCompanion == null)
                return;

            lblSaveSuccess.Visibility = Visibility.Collapsed;
            saveTimer.Stop();
            saveCounter = 0;

            var companion = Companions.Single(x => x.Id == _currentCompanion.Id);
            companion.Name = txtCompanionName.Text;

            _currentCompanion = companion;

            lblSaveSuccess.Visibility = Visibility.Visible;
            saveTimer.Start();
            UpdateNames();
        }

        private void ClearCompanion()
        {
            txtCompanionName.Text = "";
        }

        private void RenumberCompanions()
        {
            int companionId = 1;
            bool refreshCompanions = false;
            foreach (var companion in Companions.OrderBy(x => x.Id))
            {
                if (companion.Id != companionId)
                    refreshCompanions = true;

                companion.Id = companionId;
                companionId++;
            }

            if (refreshCompanions)
            {
                listCompanions.Items.Clear();
                foreach (var obj in Companions)
                {
                    var name = string.IsNullOrEmpty(obj.Name) ?
                    " - Companion" :
                    " - " + obj.Name;

                    listCompanions.Items.Add(new ListBoxItem
                    {
                        Tag = obj.Id,
                        Content = obj.Id + name
                    });
                }
            }
        }

        #endregion

        //----------------------------------------
        //--SHOPS---------------------------------
        //----------------------------------------
        #region Shop Clicks

        private void BtnLoadShop_Click(object sender, RoutedEventArgs e)
        {
            LoadShop();
        }

        private void BtnNewShop_Click(object sender, RoutedEventArgs e)
        {
            NewShop();
        }

        private void BtnDeleteShop_Click(object sender, RoutedEventArgs e)
        {
            DeleteShop();
        }

        private void BtnSaveShops_Click(object sender, RoutedEventArgs e)
        {
            SaveShops();
        }

        private void SaveShopEvent(object sender, RoutedEventArgs e)
        {
            SaveShops();
        }

        private void BtnAddItemToShop_Click(object sender, RoutedEventArgs e)
        {
            AddItemToShop();
            SaveShops();
        }

        private void BtnRemoveItemFromShop_Click(object sender, RoutedEventArgs e)
        {
            RemoveItemFromShop();
            SaveShops();
        }

        #endregion

        #region Shop Logic

        private void LoadShop()
        {
            if (listShops.SelectedItem == null)
            {
                _currentShop = null;
                return;
            }

            ClearShop();

            var selectedShop = listShops.SelectedItem as ListBoxItem;
            var shopId = Convert.ToInt32(selectedShop.Tag);
            var shop = Shops.Single(x => x.Id == shopId);
            _currentShop = shop;

            foreach(var item in shop.Items)
            {
                var name = string.IsNullOrEmpty(item.Name) ?
                        " - Item" :
                        " - " + item.Name;

                listShopItems.Items.Add(new ListBoxItem
                {
                    Tag = item.Id,
                    Content = item.Id + name
                });
            }

            txtShopName.Text = shop.ShopName;
            txtWelcomeMessage.Text = shop.WelcomeMessage;
        }

        private void NewShop()
        {
            var newShop = new Shop();
            newShop.Id = 1;
            if (Shops.Any())
            {
                newShop.Id = Shops.Max(x => x.Id) + 1;
            }

            Shops.Add(newShop);
            _currentShop = newShop;
            var newShopListItem = new ListBoxItem
            {
                Tag = newShop.Id,
                Content = newShop.Id + " - Shop"
            };

            listShops.Items.Add(newShopListItem);
            foreach (ListBoxItem item in listShops.Items)
            {
                var tag = (int)item.Tag;
                if (tag == newShop.Id)
                {
                    listShops.SelectedItem = item;
                }
            }

            LoadShop();
            SaveShops();
        }

        private void DeleteShop()
        {
            if (_currentShop == null ||
                listShops.SelectedItem == null ||
                !Shops.Any())
                return;

            Shops.RemoveAll(x => x.Id == _currentShop.Id);
            listShops.Items.Remove(listShops.SelectedItem);

            _currentShop = null;
            ClearShop();
            RenumberShops();
        }

        private void SaveShops()
        {
            if (_currentShop == null)
                return;

            lblSaveSuccess.Visibility = Visibility.Collapsed;
            saveTimer.Stop();
            saveCounter = 0;

            var shop = Shops.Single(x => x.Id == _currentShop.Id);
            shop.ShopName = txtShopName.Text;
            shop.WelcomeMessage = txtWelcomeMessage.Text;
            shop.Items = new List<Item>();
            foreach(ListBoxItem obj in listShopItems.Items)
            {
                var itemId = (int)obj.Tag;
                var item = Items.SingleOrDefault(x => x.Id == itemId);
                if(item != null)
                {
                    shop.Items.Add(item);
                }
            }

            _currentShop = shop;

            lblSaveSuccess.Visibility = Visibility.Visible;
            saveTimer.Start();
            UpdateNames();
        }

        private void ClearShop()
        {
            txtShopName.Text = "";
            txtWelcomeMessage.Text = "";
            listShopItems.Items.Clear();
        }

        private void RenumberShops()
        {
            int shopId = 1;
            bool refreshShops = false;
            foreach (var shop in Shops.OrderBy(x => x.Id))
            {
                if (shop.Id != shopId)
                    refreshShops = true;

                shop.Id = shopId;
                shopId++;
            }

            if (refreshShops)
            {
                listShops.Items.Clear();
                foreach (var obj in Shops)
                {
                    var name = string.IsNullOrEmpty(obj.ShopName) ?
                        " - Shop" :
                        " - " + obj.ShopName;

                    listShops.Items.Add(new ListBoxItem
                    {
                        Tag = obj.Id,
                        Content = obj.Id + name
                    });
                }
            }
        }

        private void AddItemToShop()
        {
            if (_currentShop == null)
                return;

            foreach(ListBoxItem item in listAvailableItems.SelectedItems)
            {
                listShopItems.Items.Add(new ListBoxItem
                {
                    Tag = item.Tag,
                    Content = item.Content
                });
            }

            listAvailableItems.UnselectAll();
        }

        private void RemoveItemFromShop()
        {
            if (_currentShop == null)
                return;

            var itemsToRemove = new List<ListBoxItem>();
            foreach (ListBoxItem item in listShopItems.SelectedItems)
            {
                itemsToRemove.Add(item);
            }

            foreach(var item in itemsToRemove)
            {
                listShopItems.Items.Remove(item);
            }

            listShopItems.UnselectAll();
        }

        #endregion

        //----------------------------------------
        //--ITEMS---------------------------------
        //----------------------------------------
        #region Item Clicks

        private void BtnLoadItem_Click(object sender, RoutedEventArgs e)
        {
            LoadItem();
        }

        private void BtnNewItem_Click(object sender, RoutedEventArgs e)
        {
            NewItem();
        }

        private void BtnDeleteItem_Click(object sender, RoutedEventArgs e)
        {
            DeleteItem();
        }

        private void BtnSaveItems_Click(object sender, RoutedEventArgs e)
        {
            SaveItems();
        }

        private void SaveItemEvent(object sender, RoutedEventArgs e)
        {
            SaveItems();
        }

        #endregion

        #region Item Logic

        private void LoadItem()
        {
            if (listItems.SelectedItem == null)
            {
                _currentItem = null;
                return;
            }

            ClearItem();

            var selectedItem = listItems.SelectedItem as ListBoxItem;
            var itemId = Convert.ToInt32(selectedItem.Tag);
            var item = Items.Single(x => x.Id == itemId);
            _currentItem = item;

            txtItemName.Text = item.Name;
            txtItemCost.Text = item.Cost.ToString();
        }

        private void NewItem()
        {
            var newItem = new Item();
            newItem.Id = 1;
            if (Items.Any())
            {
                newItem.Id = Items.Max(x => x.Id) + 1;
            }

            Items.Add(newItem);
            _currentItem = newItem;
            var newItemListItem = new ListBoxItem
            {
                Tag = newItem.Id,
                Content = newItem.Id + " - Item"
            };

            var newAvailableItemListItem = new ListBoxItem
            {
                Tag = newItem.Id,
                Content = newItem.Id + " - Item"
            };

            listItems.Items.Add(newItemListItem);
            listAvailableItems.Items.Add(newAvailableItemListItem);
            foreach (ListBoxItem item in listItems.Items)
            {
                var tag = (int)item.Tag;
                if (tag == newItem.Id)
                {
                    listItems.SelectedItem = item;
                }
            }

            LoadItem();
            SaveItems();
        }

        private void DeleteItem()
        {
            if (_currentItem == null ||
                listItems.SelectedItem == null ||
                !Items.Any())
                return;

            Items.RemoveAll(x => x.Id == _currentItem.Id);
            listItems.Items.Remove(listItems.SelectedItem);

            ListBoxItem availableItemToRemove = null;
            foreach (ListBoxItem item in listAvailableItems.Items)
            {
                var itemId = (int)item.Tag;
                if (itemId == _currentItem.Id)
                    availableItemToRemove = item;
            }

            if (availableItemToRemove != null)
            {
                listAvailableItems.Items.Remove(availableItemToRemove);
            }

            _currentItem = null;
            ClearItem();
            RenumberItems();
        }

        private void SaveItems()
        {
            if (_currentItem == null)
                return;

            lblSaveSuccess.Visibility = Visibility.Collapsed;
            saveTimer.Stop();
            saveCounter = 0;

            var item = Items.Single(x => x.Id == _currentItem.Id);
            item.Name = txtItemName.Text;
            item.Cost = Convert.ToInt32(txtItemCost.Text);

            _currentItem = item;

            lblSaveSuccess.Visibility = Visibility.Visible;
            saveTimer.Start();
            UpdateNames();
        }

        private void ClearItem()
        {
            txtItemName.Text = "";
            txtItemCost.Text = "";
        }

        private void RenumberItems()
        {
            int itemId = 1;
            bool refreshItems = false;
            foreach (var item in Items.OrderBy(x => x.Id))
            {
                if (item.Id != itemId)
                    refreshItems = true;

                item.Id = itemId;
                itemId++;
            }

            if (refreshItems)
            {
                listItems.Items.Clear();
                listAvailableItems.Items.Clear();

                foreach (var obj in Items)
                {
                    var name = string.IsNullOrEmpty(obj.Name) ?
                            " - Item" :
                            " - " + obj.Name;

                    listItems.Items.Add(new ListBoxItem
                    {
                        Tag = obj.Id,
                        Content = obj.Id + name
                    });

                    listAvailableItems.Items.Add(new ListBoxItem
                    {
                        Tag = obj.Id,
                        Content = obj.Id + name
                    });
                }
            }
        }

        #endregion

        #region Exports

        private void BtnExportAll_Click(object sender, RoutedEventArgs e)
        {
            _exportingAll = true;
            var saveHandler = new SaveHandler();
            saveHandler.Save(Screens, typeof(List<Screen>), saveHandler.ScreensFile);
            saveHandler.Save(Companions, typeof(List<Companion>), saveHandler.CompanionsFile);
            saveHandler.Save(Items, typeof(List<Item>), saveHandler.ItemsFile);
            saveHandler.Save(Shops, typeof(List<Shop>), saveHandler.ShopsFile);
            _exportingAll = false;
            MessageBox.Show("Everything has been exported successfully!\n\nSee Files:\n\n" +
                saveHandler.ScreensFile + "\n" +
                saveHandler.CompanionsFile + "\n" +
                saveHandler.ItemsFile + "\n" +
                saveHandler.ShopsFile);
        }

        private void BtnExportScreens_Click(object sender, RoutedEventArgs e)
        {
            var saveHandler = new SaveHandler();
            saveHandler.Save(Screens, typeof(List<Screen>), saveHandler.ScreensFile);

            if (!_exportingAll)
            {
                MessageBox.Show("Screens have been exported successfully!\n\nSee File:\n\n" + saveHandler.ScreensFile);
            }
        }

        private void BtnExportCompanions_Click(object sender, RoutedEventArgs e)
        {
            var saveHandler = new SaveHandler();
            saveHandler.Save(Companions, typeof(List<Companion>), saveHandler.CompanionsFile);

            if (!_exportingAll)
            {
                MessageBox.Show("Companions have been exported successfully!\n\nSee File:\n\n" + saveHandler.CompanionsFile);
            }
        }

        private void BtnExportItems_Click(object sender, RoutedEventArgs e)
        {
            var saveHandler = new SaveHandler();
            saveHandler.Save(Items, typeof(List<Item>), saveHandler.ItemsFile);

            if (!_exportingAll)
            {
                MessageBox.Show("Items have been exported successfully!\n\nSee File:\n\n" + saveHandler.ItemsFile);
            }
        }

        private void BtnExportShops_Click(object sender, RoutedEventArgs e)
        {
            var saveHandler = new SaveHandler();
            saveHandler.Save(Shops, typeof(List<Shop>), saveHandler.ShopsFile);

            if (!_exportingAll)
            {
                MessageBox.Show("Shops have been exported successfully!\n\nSee File:\n\n" + saveHandler.ShopsFile);
            }
        }

        #endregion

        private void SaveTimer_Tick(object sender, EventArgs e)
        {
            if (saveCounter == 2)
            {
                lblSaveSuccess.Visibility = Visibility.Collapsed;
                saveTimer.Stop();
                saveCounter = 0;
            }
            else
            {
                saveCounter++;
            }
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }

        private void UpdateNames()
        {
            // Screens
            foreach (ListBoxItem obj in listScreens.Items)
            {
                var screenId = Convert.ToInt32(obj.Tag);
                var screen = Screens.SingleOrDefault(x => x.Id == screenId);

                if (screen != null)
                {
                    var name = string.IsNullOrEmpty(screen.InternalName) ?
                        " - Screen" :
                        " - " + screen.InternalName;

                    obj.Content = screen.Id + name;
                }
            }

            // Companions
            foreach (ListBoxItem obj in listCompanions.Items)
            {
                var companionId = Convert.ToInt32(obj.Tag);
                var companion = Companions.SingleOrDefault(x => x.Id == companionId);

                if (companion != null)
                {
                    var name = string.IsNullOrEmpty(companion.Name) ?
                        " - Companion" :
                        " - " + companion.Name;

                    obj.Content = companion.Id + name;
                }
            }

            // Items
            foreach (ListBoxItem obj in listItems.Items)
            {
                var itemId = Convert.ToInt32(obj.Tag);
                var item = Items.SingleOrDefault(x => x.Id == itemId);

                if (item != null)
                {
                    var name = string.IsNullOrEmpty(item.Name) ?
                        " - Item" :
                        " - " + item.Name;

                    obj.Content = item.Id + name;
                }
            }

            // Available Items
            foreach (ListBoxItem obj in listAvailableItems.Items)
            {
                var itemId = Convert.ToInt32(obj.Tag);
                var item = Items.SingleOrDefault(x => x.Id == itemId);

                if (item != null)
                {
                    var name = string.IsNullOrEmpty(item.Name) ?
                        " - Item" :
                        " - " + item.Name;

                    obj.Content = item.Id + name;
                }
            }

            // Shop Items
            foreach (ListBoxItem obj in listShopItems.Items)
            {
                var itemId = Convert.ToInt32(obj.Tag);
                var item = Items.SingleOrDefault(x => x.Id == itemId);

                if (item != null)
                {
                    var name = string.IsNullOrEmpty(item.Name) ?
                        " - Item" :
                        " - " + item.Name;

                    obj.Content = item.Id + name;
                }
            }

            // Shops
            foreach (ListBoxItem obj in listShops.Items)
            {
                var itemId = Convert.ToInt32(obj.Tag);
                var item = Shops.SingleOrDefault(x => x.Id == itemId);

                if (item != null)
                {
                    var name = string.IsNullOrEmpty(item.ShopName) ?
                        " - Shop" :
                        " - " + item.ShopName;

                    obj.Content = item.Id + name;
                }
            }
        }
    }
}
