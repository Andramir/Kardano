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
using System.Globalization;
using System.Security.Cryptography;

namespace Kordano
{
    public partial class MainWindow : Window
    {
        private Database db = new Database();

        public MainWindow()
        {
            InitializeComponent();
            OpenLogBoard();
        }

        public void ClearRegBoard()
        {
            NewLogin.Clear(); NewPass.Clear(); NewPassRepeat.Clear();
        }
        public void ClearLogBoard()
        {
            LoginAuth.Clear(); PasswordAuth.Clear();
        }

        public void Logout()
        {
            UserText.Text = "";
            MatrixDataGrid.Items.Clear(); MatrixDataGrid.Columns.Clear();
            FirstText.Clear(); EncodeText.Clear(); CryptoKeyText.Clear(); 
            OpenLogBoard();
        }

        public void OpenLogBoard()
        {
            ClearLogBoard();
            ClearRegBoard();
            TitleText.Visibility = Visibility.Visible;
            LoginBorder.Visibility = Visibility.Visible;

            RegisterBorder.Visibility = Visibility.Collapsed;
            AppBorder.Visibility = Visibility.Collapsed;

        }
        public void OpenRegBorder()
        {
            ClearLogBoard();
            ClearRegBoard();
            TitleText.Visibility = Visibility.Visible;
            RegisterBorder.Visibility = Visibility.Visible;

            LoginBorder.Visibility = Visibility.Collapsed;
            AppBorder.Visibility = Visibility.Collapsed;
        }
        public void OpenAppBorder()
        {
            ClearLogBoard();
            ClearRegBoard();
            AppBorder.Visibility = Visibility.Visible;

            TitleText.Visibility = Visibility.Collapsed;
            RegisterBorder.Visibility = Visibility.Collapsed;
            LoginBorder.Visibility = Visibility.Collapsed;
        }

        private void LogFormRegButton_Click(object sender, RoutedEventArgs e)    //RegisterButton from LogBorder
        {
            OpenRegBorder();
        }
        private void LogFormAuthButton_Click(object sender, RoutedEventArgs e)    //LoginButton from LogBorder
        {
            string res = db.CheckUser(LoginAuth.Text, PasswordAuth.Password);
            if (res == "auth")
            {
                UserText.Text = LoginAuth.Text;
                OpenAppBorder();
            }
            else
            {
                MessageBox.Show(caption: "Ошибка", messageBoxText: res);
            }
        }
        private void RegFormRegButton_Click(object sender, RoutedEventArgs e)    //RegisterButton  RegBoard
        {
            string login = NewLogin.Text.Trim();
            string pass = NewPass.Password.Trim();
            string passRepeat = NewPassRepeat.Password.Trim();

            if (login.Length < 5)
            {
                NewLogin.ToolTip = "Имя слишком короткое";
                NewLogin.Background = Brushes.IndianRed;
            }
            else if (pass.Length < 5)
            {
                NewPass.ToolTip = "Слишком короткий пароль";
                NewPass.Background = Brushes.IndianRed;

                NewLogin.ToolTip = "";
                NewLogin.Background = Brushes.Transparent;
            }
            else if (pass != passRepeat)
            {
                NewPassRepeat.ToolTip = "Пароль не совпадает";
                NewPassRepeat.Background = Brushes.IndianRed;

                NewPass.ToolTip = "";
                NewPass.Background = Brushes.Transparent;
            }
            else
            {
                NewPassRepeat.ToolTip = "";
                NewPassRepeat.Background = Brushes.Transparent;

                db.AddUser(login, pass);
                OpenLogBoard();
            }
        }
        private void RegFormBackButton_Click(object sender, RoutedEventArgs e)     //BackButton from RegBoard
        {
            OpenLogBoard();
        }
        private void LogoutButton_Click(object sender, RoutedEventArgs e)    //Logout
        {
            Logout();
        }

        private void OriginText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (FirstText.Text.Length > 999)
            {
                FirstText.Text = FirstText.Text.Substring(0, 999);
            }
        }
        private void CryptoKeyText_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        private void EncodeText_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        private void MatrixDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void EncodeButton_Click(object sender, RoutedEventArgs e)
        {
            EncodeText.Text = Encode(FirstText.Text, CryptoKeyText.Text);
        }
        private void DecodeButton_Click(object sender, RoutedEventArgs e)
        {
            FirstText.Text = Decode(EncodeText.Text, CryptoKeyText.Text);
        }

        public string Encode(string inputText, string cryptoKey)
        {
            string _inputText = inputText.Replace(" ", "");

            int matrixSize = (int)Math.Ceiling(Math.Sqrt(_inputText.Length * 10));
            char[,] matrix = new char[matrixSize, matrixSize];

            int key = GetStableHashCode(cryptoKey);  //создание и заполнение матрицы-маски
            Random random = new Random(key);
            int[] maskArray = GenerateMaskArray(matrixSize * matrixSize, random);

            int textIndex = 0;
            for (int i = 0; i < matrixSize; i++)  //заполнение матрицы
            {
                for (int j = 0; j < matrixSize; j++)
                {
                    if (textIndex < _inputText.Length && maskArray[i * matrixSize + j] == 1)
                    {
                        matrix[i, j] = _inputText[textIndex];
                        textIndex++;
                    }
                    else
                    {
                        matrix[i, j] = GetRandomRussianLetter(random);
                    }
                }
            }

            DisplayMatrixInDataGrid(matrix);

            string encodeText = "";

            textIndex = 0;
            for (int i = 0; i < matrixSize; i++)
            {
                for (int j = 0; j < matrixSize; j++)
                {
                    if (maskArray[i * matrixSize + j] == 1 || maskArray[i * matrixSize + j] == 0)
                    {
                        encodeText += matrix[i, j];
                        textIndex++;
                    }
                }
            }
            return encodeText;
        }
        public string Decode(string encodeText, string cryptoKey)
        {
            int matrixSize = (int)Math.Ceiling(Math.Sqrt(encodeText.Length)); // создание и заполнение матрицы 
            char[,] matrix = new char[matrixSize, matrixSize];

            int index = 0;
            for (int i = 0; i < matrixSize; i++)
            {
                for (int j = 0; j < matrixSize; j++)
                {
                    if (index < encodeText.Length)
                    {
                        matrix[i, j] = encodeText[index];
                        index++;
                    }
                    else
                    {
                        matrix[i, j] = ' '; // Заполнение пустыми символами, если текст короче размера матрицы
                    }
                }
            }

            int key = GetStableHashCode(cryptoKey);  //создание и заполнение матрицы-маски
            Random random = new Random(key);
            int[] maskArray = GenerateMaskArray(matrixSize * matrixSize, random);

            DisplayMatrixInDataGrid(matrix);

            string decodeText = "";

            int textIndex = 0;
            for (int i = 0; i < matrixSize; i++)
            {
                for (int j = 0; j < matrixSize; j++)
                {
                    if (maskArray[i * matrixSize + j] == 1)
                    {
                        decodeText += matrix[i, j];
                        textIndex++;
                    }
                }
            }
            return decodeText;
        }
        private static int GetStableHashCode(string str)
        {
            unchecked
            {
                int hash1 = (5381 << 16) + 5381;
                int hash2 = hash1;

                for (int i = 0; i < str.Length; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ str[i];
                    if (i == str.Length - 1)
                        break;
                    hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                }

                return hash1 + (hash2 * 1566083941);
            }
        }
        private static int[] GenerateMaskArray(int size, Random random)
        {
            int[] maskArray = new int[size];

            for (int i = 0; i < size; i++)
            {
                maskArray[i] = random.Next(2);
            }

            return maskArray;
        }
        private static char GetRandomRussianLetter(Random random)
        {
            int letterCode = random.Next(1040, 1104);
            return (char)letterCode;
        }
        private void DisplayMatrixInDataGrid(char[,] matrix)
        {
            MatrixDataGrid.Items.Clear();
            MatrixDataGrid.Columns.Clear();

            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            for (int j = 0; j < cols; j++)
            {
                DataGridTextColumn column = new DataGridTextColumn();
                column.Header = $"Column {j + 1}";

                // Создайте привязку данных для каждой ячейки столбца
                column.Binding = new System.Windows.Data.Binding($"[{j}]");

                // Установите стиль для ячеек, чтобы убрать отступы
                column.CellStyle = new Style(typeof(DataGridCell))
                {
                    Setters = {
                        new Setter(Control.PaddingProperty, new Thickness(0)),
                        new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center)
                    }
                };

                MatrixDataGrid.Columns.Add(column);
            }

            for (int i = 0; i < rows; i++)
            {
                var row = new object[cols];
                for (int j = 0; j < cols; j++)
                {
                    row[j] = matrix[i, j];
                }
                MatrixDataGrid.Items.Add(row);
            }
        }

        private void ClearOriginButton_Click(object sender, RoutedEventArgs e)
        {
            FirstText.Text = "";
        }
        private void ClearEncodeButton_Click(object sender, RoutedEventArgs e)
        {
            EncodeText.Text = "";
        }
    }
}
