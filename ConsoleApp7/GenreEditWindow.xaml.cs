using System.Windows;
using ConsoleApp7.Data;

namespace ConsoleApp7;

public partial class GenreEditWindow : Window
{
    private readonly int? _genreId;

    public GenreEditWindow(int? genreId)
    {
        _genreId = genreId;
        InitializeComponent();
        if (_genreId.HasValue)
            Title = "Редактирование жанра";
        LoadData();
    }

    private void LoadData()
    {
        if (_genreId.HasValue)
        {
            using var db = DbContextFactory.Create();
            var g = db.Genres.Find(_genreId.Value);
            if (g != null)
            {
                NameTextBox.Text = g.Name;
                DescriptionTextBox.Text = g.Description ?? "";
            }
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameTextBox.Text))
        {
            MessageBox.Show("Введите название жанра.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        using var db = DbContextFactory.Create();
        if (_genreId.HasValue)
        {
            var g = db.Genres.Find(_genreId.Value);
            if (g != null)
            {
                g.Name = NameTextBox.Text.Trim();
                g.Description = string.IsNullOrWhiteSpace(DescriptionTextBox.Text) ? null : DescriptionTextBox.Text.Trim();
                db.SaveChanges();
            }
        }
        else
        {
            db.Genres.Add(new Entities.Genre
            {
                Name = NameTextBox.Text.Trim(),
                Description = string.IsNullOrWhiteSpace(DescriptionTextBox.Text) ? null : DescriptionTextBox.Text.Trim()
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
