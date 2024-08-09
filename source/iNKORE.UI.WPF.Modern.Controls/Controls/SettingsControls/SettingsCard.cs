// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel.Design;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using iNKORE.UI.WPF.Modern.Controls.Helpers;

namespace iNKORE.UI.WPF.Modern.Controls
{
    /// <summary>
    /// This is the base control to create consistent settings experiences, inline with the Windows 11 design language.
    /// A SettingsCard can also be hosted within a SettingsExpander.
    /// </summary>

    public partial class SettingsCard : ButtonBase
    {
        internal const string CommonStates = "CommonStates";
        internal const string NormalState = "Normal";
        internal const string MouseOverState = "PointerOver";
        internal const string PressedState = "Pressed";
        internal const string DisabledState = "Disabled";

        internal const string ContentAlignmentStates = "ContentAlignmentStates";
        internal const string RightState = "Right";
        internal const string RightWrappedState = "RightWrapped";
        internal const string RightWrappedNoIconState = "RightWrappedNoIcon";
        internal const string LeftState = "Left";
        internal const string VerticalState = "Vertical";

        internal const string ContentSpacingStates = "ContentSpacingStates";
        internal const string NoContentSpacingState = "NoContentSpacing";
        internal const string ContentSpacingState = "ContentSpacing";

        internal const string ActionIconPresenterHolder = "PART_ActionIconPresenterHolder";
        internal const string HeaderPresenter = "PART_HeaderPresenter";
        internal const string DescriptionPresenter = "PART_DescriptionPresenter";
        internal const string HeaderIconPresenterHolder = "PART_HeaderIconPresenterHolder";


        static SettingsCard()
        {
            ContentProperty.OverrideMetadata(typeof(SettingsCard), new FrameworkPropertyMetadata(null, ContentProperty_ValueChanged));

            DefaultStyleKeyProperty.OverrideMetadata(typeof(SettingsCard), new FrameworkPropertyMetadata(typeof(SettingsCard)));
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SettingsCard"/> class.
        /// </summary>
        public SettingsCard()
        {

        }

        /// <inheritdoc />
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            IsEnabledChanged -= OnIsEnabledChanged;
            OnActionIconChanged();
            OnHeaderChanged();
            OnHeaderIconChanged();
            OnDescriptionChanged();
            OnIsClickEnabledChanged();
            CheckInitialVisualState();
            SetAccessibleContentName();

            IsEnabledChanged += OnIsEnabledChanged;

            // RegisterPropertyChangedCallback(ContentProperty, OnContentChanged);
        }

        private static void ContentProperty_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SettingsCard control)
            {
                control.OnContentChanged(e.OldValue, e.NewValue);
            }
        }

        private void CheckInitialVisualState()
        {
            VisualStateManager.GoToState(this, IsEnabled ? NormalState : DisabledState, true);

            if (GetTemplateChild("ContentAlignmentStates") is VisualStateGroup contentAlignmentStatesGroup)
            {
                contentAlignmentStatesGroup.CurrentStateChanged -= this.ContentAlignmentStates_Changed;
                CheckVerticalSpacingState(contentAlignmentStatesGroup.CurrentState);
                contentAlignmentStatesGroup.CurrentStateChanged += this.ContentAlignmentStates_Changed;
            }
        }

        // We automatically set the AutomationProperties.Name of the Content if not configured.
        private void SetAccessibleContentName()
        {
            if (Header is string headerString && headerString != string.Empty)
            {
                // We don't want to override an AutomationProperties.Name that is manually set, or if the Content basetype is of type ButtonBase (the ButtonBase.Content will be used then)
                if (Content is UIElement element && string.IsNullOrEmpty(AutomationProperties.GetName(element)) && element.GetType().BaseType != typeof(ButtonBase) && element.GetType() != typeof(TextBlock))
                {
                    AutomationProperties.SetName(element, headerString);
                }
            }
        }

        private void EnableButtonInteraction()
        {
            DisableButtonInteraction();

            IsTabStop = true;
            PreviewKeyDown += Control_PreviewKeyDown;
            PreviewKeyUp += Control_PreviewKeyUp;
        }

        private void DisableButtonInteraction()
        {
            IsTabStop = false;
            PreviewKeyDown -= Control_PreviewKeyDown;
            PreviewKeyUp -= Control_PreviewKeyUp;
        }

        private void Control_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Space) //  || e.Key == Key.GamepadA
            {
                VisualStateManager.GoToState(this, NormalState, true);
            }
        }

        private void Control_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Space) //  || e.Key == Key.GamepadA
            {
                // Check if the active focus is on the card itself - only then we show the pressed state.
                if (this.IsFocused) // if (GetFocusedElement() is SettingsCard)
                {
                    VisualStateManager.GoToState(this, PressedState, true);
                }
            }
        }

        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            base.OnLostMouseCapture(e);
            VisualStateManager.GoToState(this, NormalState, true);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            VisualStateManager.GoToState(this, PressedState, true);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (IsClickEnabled)
            {
                base.OnMouseUp(e);
                VisualStateManager.GoToState(this, NormalState, true);
            }
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            VisualStateManager.GoToState(this, MouseOverState, true);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            VisualStateManager.GoToState(this, NormalState, true);
        }



        /// <summary>
        /// Creates AutomationPeer
        /// </summary>
        /// <returns>An automation peer for <see cref="SettingsCard"/>.</returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new SettingsCardAutomationPeer(this);
        }

        private void OnIsClickEnabledChanged()
        {
            OnActionIconChanged();
            if (IsClickEnabled)
            {
                EnableButtonInteraction();
            }
            else
            {
                DisableButtonInteraction();
            }
        }

        private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            VisualStateManager.GoToState(this, IsEnabled ? NormalState : DisabledState, true);
        }

        private void OnActionIconChanged()
        {
            if (GetTemplateChild(ActionIconPresenterHolder) is FrameworkElement actionIconPresenter)
            {
                if (IsClickEnabled && IsActionIconVisible)
                {
                    actionIconPresenter.Visibility = Visibility.Visible;
                }
                else
                {
                    actionIconPresenter.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void OnHeaderIconChanged()
        {
            if (GetTemplateChild(HeaderIconPresenterHolder) is FrameworkElement headerIconPresenter)
            {
                headerIconPresenter.Visibility = HeaderIcon != null
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        private void OnDescriptionChanged()
        {
            if (GetTemplateChild(DescriptionPresenter) is FrameworkElement descriptionPresenter)
            {
                descriptionPresenter.Visibility = IsNullOrEmptyString(Description)
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }

        }

        private void OnHeaderChanged()
        {
            if (GetTemplateChild(HeaderPresenter) is FrameworkElement headerPresenter)
            {
                headerPresenter.Visibility = IsNullOrEmptyString(Header)
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }

        }

        private void ContentAlignmentStates_Changed(object sender, VisualStateChangedEventArgs e)
        {
            CheckVerticalSpacingState(e.NewState);
        }

        private void CheckVerticalSpacingState(VisualState s)
        {
            // On state change, checking if the Content should be wrapped (e.g. when the card is made smaller or the ContentAlignment is set to Vertical). If the Content and the Header or Description are not null, we add spacing between the Content and the Header/Description.

            if (s != null && (s.Name == RightWrappedState || s.Name == RightWrappedNoIconState || s.Name == VerticalState) && (Content != null) && (!IsNullOrEmptyString(Header) || !IsNullOrEmptyString(Description)))
            {
                VisualStateManager.GoToState(this, ContentSpacingState, true);
            }
            else
            {
                VisualStateManager.GoToState(this, NoContentSpacingState, true);
            }
        }

        private FrameworkElement? GetFocusedElement()
        {
            //if (ControlHelpers.IsXamlRootAvailable && XamlRoot != null)
            //{
            //    return FocusManager.GetFocusedElement(XamlRoot) as FrameworkElement;
            //}
            //else
            //{
            //    return FocusManager.GetFocusedElement() as FrameworkElement;
            //}

            return FocusManager.GetFocusedElement(this) as FrameworkElement;
        }

        private static bool IsNullOrEmptyString(object obj)
        {
            if (obj == null)
            {
                return true;
            }

            if (obj is string objString && objString == string.Empty)
            {
                return true;
            }

            return false;
        }


        public static readonly DependencyProperty UseSystemFocusVisualsProperty =
            FocusVisualHelper.UseSystemFocusVisualsProperty.AddOwner(typeof(SettingsCard));

        public bool UseSystemFocusVisuals
        {
            get => (bool)GetValue(UseSystemFocusVisualsProperty);
            set => SetValue(UseSystemFocusVisualsProperty, value);
        }

        public static readonly DependencyProperty FocusVisualMarginProperty =
            FocusVisualHelper.FocusVisualMarginProperty.AddOwner(typeof(SettingsCard));

        public Thickness FocusVisualMargin
        {
            get => (Thickness)GetValue(FocusVisualMarginProperty);
            set => SetValue(FocusVisualMarginProperty, value);
        }
    }
}