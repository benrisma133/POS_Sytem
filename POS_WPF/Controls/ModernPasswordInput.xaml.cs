using System;
using System.Windows;
using System.Windows.Controls;

namespace POS_WPF.Controls
{
    public partial class ModernPasswordInput : UserControl
    {
        private bool hasInteracted = false;
        private bool isPasswordVisible = false;
        private Func<string, string> _externalValidator;

        public ModernPasswordInput()
        {
            InitializeComponent();
            IsValid = true;
        }

        #region Dependency Properties

        #region Height Property
        public static readonly DependencyProperty InputHeightProperty =
            DependencyProperty.Register(
                "InputHeight",
                typeof(double),
                typeof(ModernPasswordInput),
                new PropertyMetadata(44.0, OnInputHeightChanged)
            );

        public double InputHeight
        {
            get => (double)GetValue(InputHeightProperty);
            set => SetValue(InputHeightProperty, value);
        }

        private static void OnInputHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ModernPasswordInput)d;
            double newHeight = (double)e.NewValue;
            control.PasswordBoxBorder.Height = newHeight;
        }
        #endregion

        #region Label Property
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(ModernPasswordInput),
                new PropertyMetadata("Password", OnLabelChanged));

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        private static void OnLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ModernPasswordInput)d;
            control.LabelText.Text = e.NewValue?.ToString() ?? "Password";
        }
        #endregion

        #region Placeholder Property
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register("Placeholder", typeof(string), typeof(ModernPasswordInput),
                new PropertyMetadata("Enter password...", OnPlaceholderChanged));

        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        private static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ModernPasswordInput)d;
            control.PlaceholderText.Text = e.NewValue?.ToString() ?? "";
        }
        #endregion

        #region Password Property
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register("Password", typeof(string), typeof(ModernPasswordInput),
                new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPasswordChanged));

        public string Password
        {
            get => (string)GetValue(PasswordProperty);
            set => SetValue(PasswordProperty, value);
        }

        private static void OnPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ModernPasswordInput)d;
            string newValue = e.NewValue?.ToString() ?? "";

            // Update the appropriate control based on visibility state
            if (control.isPasswordVisible)
            {
                if (control.TextInput.Text != newValue)
                {
                    control.TextInput.Text = newValue;
                }
            }
            else
            {
                if (control.PasswordInput.Password != newValue)
                {
                    control.PasswordInput.Password = newValue;
                }
            }

            // Only validate if user has interacted
            if (control.hasInteracted)
            {
                control.Validate(live: true);
            }
        }
        #endregion

        #region Border Properties
        public static readonly DependencyProperty BorderRadiusProperty =
            DependencyProperty.Register("BorderRadius", typeof(CornerRadius), typeof(ModernPasswordInput),
                new PropertyMetadata(new CornerRadius(8), OnBorderRadiusChanged));

        public CornerRadius BorderRadius
        {
            get => (CornerRadius)GetValue(BorderRadiusProperty);
            set => SetValue(BorderRadiusProperty, value);
        }

        private static void OnBorderRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ModernPasswordInput)d;
            control.PasswordBoxBorder.CornerRadius = (CornerRadius)e.NewValue;
        }
        #endregion

        #region IsValid and ValidationMessage Properties
        public static readonly DependencyProperty IsValidProperty =
            DependencyProperty.Register("IsValid", typeof(bool), typeof(ModernPasswordInput),
                new PropertyMetadata(true));

        public bool IsValid
        {
            get => (bool)GetValue(IsValidProperty);
            set => SetValue(IsValidProperty, value);
        }

        public static readonly DependencyProperty ValidationMessageTextProperty =
            DependencyProperty.Register("ValidationMessageText", typeof(string), typeof(ModernPasswordInput),
                new PropertyMetadata("This field is required.", OnValidationMessageChanged));

        public string ValidationMessageText
        {
            get => (string)GetValue(ValidationMessageTextProperty);
            set => SetValue(ValidationMessageTextProperty, value);
        }

        private static void OnValidationMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ModernPasswordInput)d;
            control.ValidationMessage.Text = e.NewValue?.ToString() ?? "";
        }
        #endregion

        #region Validation Rule Properties
        public static readonly DependencyProperty IsRequiredProperty =
            DependencyProperty.Register("IsRequired", typeof(bool), typeof(ModernPasswordInput),
                new PropertyMetadata(false, OnValidationRuleChanged));

        public bool IsRequired
        {
            get => (bool)GetValue(IsRequiredProperty);
            set => SetValue(IsRequiredProperty, value);
        }

        public static readonly DependencyProperty MinLengthProperty =
            DependencyProperty.Register("MinLength", typeof(int), typeof(ModernPasswordInput),
                new PropertyMetadata(0, OnValidationRuleChanged));

        public int MinLength
        {
            get => (int)GetValue(MinLengthProperty);
            set => SetValue(MinLengthProperty, value);
        }

        public static readonly DependencyProperty MaxLengthProperty =
            DependencyProperty.Register("MaxLength", typeof(int), typeof(ModernPasswordInput),
                new PropertyMetadata(0, OnValidationRuleChanged));

        public int MaxLength
        {
            get => (int)GetValue(MaxLengthProperty);
            set
            {
                SetValue(MaxLengthProperty, value);
                PasswordInput.MaxLength = value > 0 ? value : int.MaxValue;
                TextInput.MaxLength = value > 0 ? value : 0;
            }
        }
        #endregion

        #region IsEnabled Property
        public new static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.Register("IsEnabled", typeof(bool), typeof(ModernPasswordInput),
                new PropertyMetadata(true, OnIsEnabledChanged));

        public new bool IsEnabled
        {
            get => (bool)GetValue(IsEnabledProperty);
            set => SetValue(IsEnabledProperty, value);
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ModernPasswordInput)d;
            bool isEnabled = (bool)e.NewValue;
            control.PasswordInput.IsEnabled = isEnabled;
            control.TextInput.IsEnabled = isEnabled;
            control.ToggleButton.IsEnabled = isEnabled;
            control.Opacity = isEnabled ? 1.0 : 0.5;
        }

        private static void OnValidationRuleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ModernPasswordInput)d;
            // Only validate if user has already interacted
            if (control.hasInteracted)
            {
                control.Validate();
            }
        }
        #endregion

        #endregion

        #region Event Handlers

        private void PasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            hasInteracted = true;
            Password = PasswordInput.Password;
            Validate(live: true);
            PasswordChanged?.Invoke(this, e);
        }

        private void TextInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            hasInteracted = true;
            Password = TextInput.Text;
            Validate(live: true);
            PasswordChanged?.Invoke(this, new RoutedEventArgs());
        }

        private void PasswordInput_GotFocus(object sender, RoutedEventArgs e)
        {
            hasInteracted = true;
        }

        private void PasswordInput_LostFocus(object sender, RoutedEventArgs e)
        {
            if (hasInteracted)
            {
                Validate();
            }
        }

        private void TextInput_GotFocus(object sender, RoutedEventArgs e)
        {
            hasInteracted = true;
        }

        private void TextInput_LostFocus(object sender, RoutedEventArgs e)
        {
            if (hasInteracted)
            {
                Validate();
            }
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            isPasswordVisible = !isPasswordVisible;

            if (isPasswordVisible)
            {
                // Show password as text
                TextInput.Text = PasswordInput.Password;
                TextInput.Visibility = Visibility.Visible;
                PasswordInput.Visibility = Visibility.Collapsed;

                // Show eye-off icon
                EyeIcon.Visibility = Visibility.Collapsed;
                EyeOffIcon.Visibility = Visibility.Visible;

                // Focus the text input
                TextInput.Focus();
                TextInput.CaretIndex = TextInput.Text.Length;
            }
            else
            {
                // Hide password
                PasswordInput.Password = TextInput.Text;
                PasswordInput.Visibility = Visibility.Visible;
                TextInput.Visibility = Visibility.Collapsed;

                // Show eye icon
                EyeIcon.Visibility = Visibility.Visible;
                EyeOffIcon.Visibility = Visibility.Collapsed;

                // Focus the password input
                PasswordInput.Focus();
            }
        }

        #endregion

        #region Validation

        public void Validate(bool live = false, Func<string, string> externalValidator = null)
        {
            // Store external validator
            if (externalValidator != null)
                _externalValidator = externalValidator;

            // Don't show validation errors if user hasn't interacted yet
            if (!hasInteracted && !live)
            {
                return;
            }

            string password = Password ?? string.Empty;
            bool wasValid = IsValid;

            IsValid = true;
            ValidationMessageText = string.Empty;

            // Required validation
            if (IsRequired && string.IsNullOrWhiteSpace(password))
            {
                IsValid = false;
                ValidationMessageText = $"{Label} is required.";
                return;
            }

            // Skip other validations if empty and not required
            if (string.IsNullOrWhiteSpace(password))
            {
                return;
            }

            // MinLength
            if (MinLength > 0 && password.Length < MinLength)
            {
                IsValid = false;
                ValidationMessageText = $"{Label} must be at least {MinLength} characters.";
                return;
            }

            // MaxLength
            if (MaxLength > 0 && password.Length > MaxLength)
            {
                IsValid = false;
                ValidationMessageText = $"{Label} must not exceed {MaxLength} characters.";
                return;
            }

            // Use stored external validator
            if (_externalValidator != null)
            {
                string error = _externalValidator(password);
                if (!string.IsNullOrEmpty(error))
                {
                    IsValid = false;
                    ValidationMessageText = error;
                }
            }
        }

        /// <summary>
        /// Force validation - useful for form submission
        /// This will validate even if the user hasn't interacted with the field
        /// </summary>
        public bool ValidateForce(Func<string, string> externalValidator = null)
        {
            hasInteracted = true;
            Validate(live: false, externalValidator: externalValidator);
            return IsValid;
        }

        /// <summary>
        /// Set external validation error (e.g., from server)
        /// </summary>
        public void SetExternalError(string message)
        {
            hasInteracted = true;
            IsValid = false;
            ValidationMessageText = message;
        }

        /// <summary>
        /// Reset the control to initial state
        /// </summary>
        public void Reset()
        {
            hasInteracted = false;
            Password = string.Empty;
            PasswordInput.Password = string.Empty;
            TextInput.Text = string.Empty;
            IsValid = true;
            ValidationMessageText = string.Empty;

            // Reset to password hidden state
            if (isPasswordVisible)
            {
                isPasswordVisible = false;
                PasswordInput.Visibility = Visibility.Visible;
                TextInput.Visibility = Visibility.Collapsed;
                EyeIcon.Visibility = Visibility.Visible;
                EyeOffIcon.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        public event RoutedEventHandler PasswordChanged;
    }
}