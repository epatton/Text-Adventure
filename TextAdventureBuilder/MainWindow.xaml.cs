using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        //Save Timers
        DispatcherTimer screenSaveTimer = new DispatcherTimer();
        int screenSaveCounter = 0;

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

            screenSaveTimer.Interval = TimeSpan.FromSeconds(0.5);
            screenSaveTimer.Tick += ScreenSaveTimer_Tick;
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
                listScreens.Items.Add(new ListBoxItem
                {
                    Tag = obj.Id,
                    Content = "Screen - " + obj.Id
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
                //listScreens.Items.Add(new ListBoxItem
                //{
                //    Name = "Screen" + obj.Id,
                //    Content = "Screen - " + obj.Id
                //});
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
                //listScreens.Items.Add(new ListBoxItem
                //{
                //    Name = "Screen" + obj.Id,
                //    Content = "Screen - " + obj.Id
                //});
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
                //listScreens.Items.Add(new ListBoxItem
                //{
                //    Name = "Screen" + obj.Id,
                //    Content = "Screen - " + obj.Id
                //});
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
                    listScreens.Items.Add(new ListBoxItem
                    {
                        Tag = obj.Id,
                        Content = "Screen - " + obj.Id
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
        }

        private void LoadSelectedOption()
        {
            if (listOptions.SelectedItem == null || _currentScreen == null)
            {
                _currentOption = null;
                return;
            }

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
        }

        private void LoadSelectedAction()
        {
            if (ddlActions.SelectedItem == null || _currentOption == null || _currentScreen == null)
            {
                _currentAction = null;
                return;
            }

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
                Content = "Screen - " + newScreen.Id
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
            if (_currentScreen == null)
                return;

            lblScreenSaveSuccess.Visibility = Visibility.Collapsed;
            screenSaveTimer.Stop();
            screenSaveCounter = 0;

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
                    var action = _currentOption.Actions.Single(x => x.Id == _currentAction.Id);
                    if (ddlActionTypes.SelectedItem != null)
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

            lblScreenSaveSuccess.Visibility = Visibility.Visible;
            screenSaveTimer.Start();
        }

        private void ScreenSaveTimer_Tick(object sender, EventArgs e)
        {
            if (screenSaveCounter == 2)
            {
                lblScreenSaveSuccess.Visibility = Visibility.Collapsed;
                screenSaveTimer.Stop();
                screenSaveCounter = 0;
            }
            else
            {
                screenSaveCounter++;
            }
        }

        #endregion

        #region Helpers

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

        #region Exports

        private void BtnExportAll_Click(object sender, RoutedEventArgs e)
        {
            var saveHandler = new SaveHandler();
            saveHandler.Save(Screens, typeof(List<Screen>), saveHandler.ScreensFile);
            saveHandler.Save(Companions, typeof(List<Companion>), saveHandler.CompanionsFile);
            saveHandler.Save(Items, typeof(List<Item>), saveHandler.ItemsFile);
            saveHandler.Save(Shops, typeof(List<Shop>), saveHandler.ShopsFile);
        }

        private void BtnExportScreens_Click(object sender, RoutedEventArgs e)
        {
            var saveHandler = new SaveHandler();
            saveHandler.Save(Screens, typeof(List<Screen>), saveHandler.ScreensFile);
        }

        private void BtnExportCompanions_Click(object sender, RoutedEventArgs e)
        {
            var saveHandler = new SaveHandler();
            saveHandler.Save(Companions, typeof(List<Companion>), saveHandler.CompanionsFile);
        }

        private void BtnExportItems_Click(object sender, RoutedEventArgs e)
        {
            var saveHandler = new SaveHandler();
            saveHandler.Save(Items, typeof(List<Item>), saveHandler.ItemsFile);
        }

        private void BtnExportShops_Click(object sender, RoutedEventArgs e)
        {
            var saveHandler = new SaveHandler();
            saveHandler.Save(Shops, typeof(List<Shop>), saveHandler.ShopsFile);
        }

        #endregion

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
