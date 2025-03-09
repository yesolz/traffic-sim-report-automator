using Simulator.ViewModels;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Simulator.Views
{
	/// <summary>
	/// NetworkView.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class NetworkView : UserControl
    {
        private Storyboard myStoryBoard;
        public NetworkView()
        {
            myStoryBoard = new Storyboard();

            InitializeComponent();

			InitStoryBoard();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            myStoryBoard.Begin(this);
        }

        private void InitStoryBoard()
        {
            var myDoubleAnimation = new DoubleAnimation();

            myDoubleAnimation.From = 0;
            myDoubleAnimation.To = 1.0;
            myDoubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(3));
            myDoubleAnimation.AutoReverse = false;

            myStoryBoard= new Storyboard();
            myStoryBoard.Children.Add(myDoubleAnimation);

            Storyboard.SetTargetName(myDoubleAnimation, Window.Name);
            Storyboard.SetTargetProperty(myDoubleAnimation, new PropertyPath(NetworkView.OpacityProperty));

        }
    }
}
