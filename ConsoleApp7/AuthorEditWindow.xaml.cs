using System.Windows;
using ConsoleApp7.Data;

namespace ConsoleApp7;

public partial class AuthorEditWindow : Window
{
    private readonly int? _authorId;

    public AuthorEditWindow(int? authorId)
    {
        _authorId = authorId;
        InitializeComponent();
        if (_authorId.HasValue)
            Title = "Редактирование автора";
        LoadData();
    }

    private void LoadData()
    {
        if (_authorId.HasValue)
        {
            using var db = DbContextFactory.Create();
            var a = db.Authors.Find(_authorId.Value);
            if (a != null)
            {
                FirstNameTextBox.Text = a.FirstName;
                LastNameTextBox.Text = a.LastName;
                BirthDatePicker.SelectedDate = a.BirthDate;
                CountryTextBox.Text = a.Country ?? "";
            }
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text) || string.IsNullOrWhiteSpace(LastNameTextBox.Text))
        {
            MessageBox.Show("Введите имя и фамилию.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        using var db = DbContextFactory.Create();
        if (_authorId.HasValue)
        {
            var a = db.Authors.Find(_authorId.Value);
            if (a != null)
            {
                a.FirstName = FirstNameTextBox.Text.Trim();
                a.LastName = LastNameTextBox.Text.Trim();
                a.BirthDate = BirthDatePicker.SelectedDate;
                a.Country = string.IsNullOrWhiteSpace(CountryTextBox.Text) ? null : CountryTextBox.Text.Trim();
                db.SaveChanges();
            }
        }
        else
        {
            db.Authors.Add(new Entities.Author
            {
                FirstName = FirstNameTextBox.Text.Trim(),
                LastName = LastNameTextBox.Text.Trim(),
                BirthDate = BirthDatePicker.SelectedDate,
                Country = string.IsNullOrWhiteSpace(CountryTextBox.Text) ? null : CountryTextBox.Text.Trim()
            });
            db.SaveChanges();
        }
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
