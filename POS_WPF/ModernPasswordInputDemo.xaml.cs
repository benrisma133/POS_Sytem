using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace POS_WPF
{
    public partial class ModernPasswordInputDemo : Window
    {
        public ModernPasswordInputDemo()
        {
            InitializeComponent();
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate all fields
            bool isBasicPasswordValid = BasicPassword.ValidateForce();

            // Validate strong password with custom validator
            bool isStrongPasswordValid = StrongPassword.ValidateForce(password =>
            {
                if (string.IsNullOrEmpty(password))
                    return null;

                // Check for uppercase letter
                if (!Regex.IsMatch(password, @"[A-Z]"))
                    return "Password must contain at least one uppercase letter.";

                // Check for lowercase letter
                if (!Regex.IsMatch(password, @"[a-z]"))
                    return "Password must contain at least one lowercase letter.";

                // Check for number
                if (!Regex.IsMatch(password, @"\d"))
                    return "Password must contain at least one number.";

                return null; // Valid
            });

            // Validate confirm password matches
            bool isConfirmPasswordValid = ConfirmPassword.ValidateForce(confirmPassword =>
            {
                if (string.IsNullOrEmpty(confirmPassword))
                    return null;

                if (confirmPassword != StrongPassword.Password)
                    return "Passwords do not match.";

                return null; // Valid
            });

            // Show result
            if (isBasicPasswordValid && isStrongPasswordValid && isConfirmPasswordValid)
            {
                ResultMessage.Text = "✓ All passwords are valid!";
                ResultMessage.Foreground = new SolidColorBrush(Color.FromRgb(34, 197, 94)); // Green
            }
            else
            {
                ResultMessage.Text = "✗ Please fix the validation errors above.";
                ResultMessage.Foreground = new SolidColorBrush(Color.FromRgb(239, 68, 68)); // Red
            }
        }
    }
}