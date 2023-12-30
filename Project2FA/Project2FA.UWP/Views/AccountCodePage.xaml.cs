﻿using Prism.Ioc;
using Project2FA.Repository.Models;
using Project2FA.UWP.Controls;
using Project2FA.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using UNOversal.Services.Dialogs;
using CommunityToolkit.Mvvm.Input;
using Project2FA.Services;
using Windows.UI.Xaml.Controls.Primitives;

namespace Project2FA.UWP.Views
{
    public sealed partial class AccountCodePage : Page
    {
        AccountCodePageViewModel ViewModel => DataContext as AccountCodePageViewModel;

        public AccountCodePage()
        {
            this.InitializeComponent();
            this.Loaded += AccountCodePage_Loaded;
        }

        private void AccountCodePage_Loaded(object sender, RoutedEventArgs e)
        {
            App.ShellPageInstance.ShellViewInternal.Header = ViewModel;
            if (SettingsService.Instance.IsProVersion)
            {
                App.ShellPageInstance.ShellViewInternal.HeaderTemplate = ShellHeaderTemplatePro;
            }
            else
            {
                App.ShellPageInstance.ShellViewInternal.HeaderTemplate = ShellHeaderTemplate;
            }
        }

        /// <summary>
        /// Copy the 2fa code to clipboard and create a user dialog
        /// </summary>
        /// <param name="model"></param>
        private async Task<bool> Copy2FACodeToClipboard(TwoFACodeModel model)
        {
            try
            {
                DataPackage dataPackage = new DataPackage
                {
                    RequestedOperation = DataPackageOperation.Copy
                };
                dataPackage.SetText(model.TwoFACode);
                Clipboard.SetContent(dataPackage);
                return true;
            }
            catch (System.Exception)
            {
                ContentDialog dialog = new ContentDialog();
                dialog.Title = Strings.Resources.ErrorHandle;
                dialog.Content = Strings.Resources.ErrorClipboardTask;
                dialog.PrimaryButtonText = Strings.Resources.ButtonTextRetry;
                dialog.PrimaryButtonStyle = App.Current.Resources["AccentButtonStyle"] as Style;
                dialog.PrimaryButtonCommand = new AsyncRelayCommand(async () =>
                {
                    await Copy2FACodeToClipboard(model);
                });
                dialog.SecondaryButtonText = Strings.Resources.ButtonTextCancel;
                await App.Current.Container.Resolve<IDialogService>().ShowDialogAsync(dialog, new DialogParameters());
                return false;
            }
        }

        private void CreateTeachingTip(FrameworkElement element)
        {
            AutoCloseTeachingTip teachingTip = new AutoCloseTeachingTip
            {
                Target = element,
                Content = Strings.Resources.AccountCodePageCopyCodeTeachingTip,
                AutoCloseInterval = 1000,
                BorderBrush = new SolidColorBrush((Color)App.Current.Resources["SystemAccentColor"]),
                IsOpen = true,
            };
            MainGrid.Children.Add(teachingTip);
        }

        /// <summary>
        /// Copies the current generated TOTP of the entry into the clipboard
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BTN_CopyCode_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement).DataContext is TwoFACodeModel model)
            {
                if(await Copy2FACodeToClipboard(model))
                {
                    CreateTeachingTip(sender as FrameworkElement);
                }
            }
        }

        /// <summary>
        /// Copy the 2fa code to clipboard when click with 'right click' and create a user dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void TwoFACodeItem_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            e.Handled = true;
            if (e.PointerDeviceType != Windows.Devices.Input.PointerDeviceType.Touch)
            {
                if ((sender as FrameworkElement).DataContext is TwoFACodeModel model)
                {
                    if(await Copy2FACodeToClipboard(model))
                    {
                        CreateTeachingTip(sender as FrameworkElement);
                    }
                }
            }
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                ViewModel.SetSuggestionList(sender.Text);
            }
        }

        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args.SelectedItem is TwoFACodeModel item)
            {
                if (item.Label != Strings.Resources.AccountCodePageSearchNotFound)
                {
                    ViewModel.TwoFADataService.ACVCollection.Filter = x => ((TwoFACodeModel)x) == item;
                    ViewModel.SearchedAccountLabel = item.Label;
                }
                else
                {
                    ViewModel.SearchedAccountLabel = string.Empty;
                    ViewModel.TwoFADataService.ACVCollection.Filter = null;
                }
            }
            else
            {
                ViewModel.SearchedAccountLabel = string.Empty;
                ViewModel.TwoFADataService.ACVCollection.Filter = null;
            }
        }

        /// <summary>
        /// Copy the account code from selected item via Tab and Enter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void LV_AccountCollection_KeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (LV_AccountCollection.SelectedItem is TwoFACodeModel model)
                {
                    if (await Copy2FACodeToClipboard(model))
                    {
                        CreateTeachingTip(e.OriginalSource as FrameworkElement);
                    }
                }
            }
        }

        private void ABB_SearchFilter_Click(object sender, RoutedEventArgs e)
        {
            //if (sender is AppBarButton abbtn)
            //{
            //    CategoryFilterFlyout categoryFilterFlyout = new CategoryFilterFlyout();
            //    if (abbtn.Flyout is null)
            //    {
            //        abbtn.Flyout = new Flyout();
            //    }
                
            //    FlyoutBase.SetAttachedFlyout(categoryFilterFlyout, abbtn.Flyout);
            //    FlyoutBase.ShowAttachedFlyout(abbtn);
            //}
        }

        private void AutoSuggestBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ViewModel.SetSuggestionList(ViewModel.SearchedAccountLabel);
        }
    }
}