using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfLib
{
    /// <summary>
    /// ColorDialog.xaml の相互作用ロジック
    /// カラー選択ダイヤログ
    /// 参考
    /// https://resanaplaza.com/2021/03/29/【wpf】色（カラー）一覧を取得してcomboboxやlistで選択す/
    /// </summary>
    public partial class ColorDialog : Window
    {
        /// <summary>
        /// 色と色名を保持するクラス
        /// </summary>
        public class MyColor
        {
            public Color Color { get; set; }
            public string Name { get; set; }
        }

        public Color mColor;
        public string mColorName;

        public ColorDialog()
        {
            InitializeComponent();

            DataContext = GetColorList();
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var mycolor = (MyColor)((ListBox)sender).SelectedItem;
            mColor = mycolor.Color;
            mColorName = mycolor.Name;

            Close();
        }

        /// <summary>
        /// すべての色を取得するメソッド
        /// </summary>
        /// <returns></returns>
        private MyColor[] GetColorList()
        {
            return typeof(Colors).GetProperties(BindingFlags.Public | BindingFlags.Static)
                .Select(i => new MyColor() { Color = (Color)i.GetValue(null), Name = i.Name }).ToArray();
        }
    }
}
