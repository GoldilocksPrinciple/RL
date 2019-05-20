#pragma checksum "..\..\..\Pages\Login.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "65BD4C25F505560DD872C6A61E3C124E45F0091E"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Requiem_Network_Launcher;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace Requiem_Network_Launcher
{


    /// <summary>
    /// Login
    /// </summary>
    public partial class LoginPage : System.Windows.Controls.Page, System.Windows.Markup.IComponentConnector
    {


#line 12 "..\..\..\Pages\Login.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Requiem_Network_Launcher.LoadingSpinner LoadingSpinner;

#line default
#line hidden


#line 13 "..\..\..\Pages\Login.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button LoginButton;

#line default
#line hidden


#line 14 "..\..\..\Pages\Login.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button CreateAccountButton;

#line default
#line hidden


#line 22 "..\..\..\Pages\Login.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox LoginUsernameBox;

#line default
#line hidden


#line 24 "..\..\..\Pages\Login.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.PasswordBox LoginPasswordBox;

#line default
#line hidden


#line 25 "..\..\..\Pages\Login.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox RememberMeCheckBox;

#line default
#line hidden


#line 26 "..\..\..\Pages\Login.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock LoginNotificationBox;

#line default
#line hidden

        private bool _contentLoaded;

        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent()
        {
            if (_contentLoaded)
            {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/Requiem Network Launcher;component/pages/login.xaml", System.UriKind.Relative);

#line 1 "..\..\..\Pages\Login.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);

#line default
#line hidden
        }

        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal System.Delegate _CreateDelegate(System.Type delegateType, string handler)
        {
            return System.Delegate.CreateDelegate(delegateType, this, handler);
        }

        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target)
        {
            switch (connectionId)
            {
                case 1:

#line 8 "..\..\..\Pages\Login.xaml"
                    ((Requiem_Network_Launcher.Login)(target)).Loaded += new System.Windows.RoutedEventHandler(this.LoginPage_Loaded);

#line default
#line hidden
                    return;
                case 2:
                    this.LoadingSpinner = ((Requiem_Network_Launcher.LoadingSpinner)(target));
                    return;
                case 3:
                    this.LoginButton = ((System.Windows.Controls.Button)(target));

#line 13 "..\..\..\Pages\Login.xaml"
                    this.LoginButton.Click += new System.Windows.RoutedEventHandler(this.LoginButton_Click);

#line default
#line hidden
                    return;
                case 4:
                    this.CreateAccountButton = ((System.Windows.Controls.Button)(target));

#line 14 "..\..\..\Pages\Login.xaml"
                    this.CreateAccountButton.Click += new System.Windows.RoutedEventHandler(this.CreateAccountButton_Click);

#line default
#line hidden
                    return;
                case 5:
                    this.LoginUsernameBox = ((System.Windows.Controls.TextBox)(target));

#line 22 "..\..\..\Pages\Login.xaml"
                    this.LoginUsernameBox.KeyDown += new System.Windows.Input.KeyEventHandler(this.LoginUsernameBox_KeyDown);

#line default
#line hidden
                    return;
                case 6:
                    this.LoginPasswordBox = ((System.Windows.Controls.PasswordBox)(target));

#line 24 "..\..\..\Pages\Login.xaml"
                    this.LoginPasswordBox.KeyDown += new System.Windows.Input.KeyEventHandler(this.LoginPasswordBox_KeyDown);

#line default
#line hidden
                    return;
                case 7:
                    this.RememberMeCheckBox = ((System.Windows.Controls.CheckBox)(target));
                    return;
                case 8:
                    this.LoginNotificationBox = ((System.Windows.Controls.TextBlock)(target));
                    return;
            }
            this._contentLoaded = true;
        }
    }
}
