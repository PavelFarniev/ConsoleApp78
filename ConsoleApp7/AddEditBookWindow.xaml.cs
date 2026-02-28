using System.Windows;
using System.Windows.Controls;
using ConsoleApp7.Data;
using Microsoft.EntityFrameworkCore;

namespace ConsoleApp7;

public partial class AddEditBookWindow : Window
{
    private readonly int? _bookId;

    public AddEditBookWindow(int? bookId)
    {
        _bookId = bookId;
        InitializeComponent();
        if (_bookId.HasValue)
            Title = "Редактирование книги";
        LoadData();
    }

    private void LoadData()
    {
        using var db = DbContextFactory.Create();
        AuthorComboBox.ItemsSource = db.Authors.OrderBy(a => a.LastName).ToList();
        GenreComboBox.ItemsSource = db.Genres.OrderBy(g => g.Name).ToList();

        if (_bookId.HasValue)
        {
            var book = db.Books.Include(b => b.Author).Include(b => b.Genre).FirstOrDefault(b => b.Id == _bookId.Value);
            if (book != null)
            {
                TitleTextBox.Text = book.Title;
                YearTextBox.Text = book.PublishYear.ToString();
                IsbnTextBox.Text = book.ISBN ?? "";
                QuantityTextBox.Text = book.QuantityInStock.ToString();
                AuthorComboBox.SelectedItem = AuthorComboBox.Items.Cast<Entities.Author>().FirstOrDefault(a => a.Id == book.AuthorId);
                GenreComboBox.SelectedItem = GenreComboBox.Items.Cast<Entities.Genre>().FirstOrDefault(g => g.Id == book.GenreId);
            }
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
        {
            MessageBox.Show("Введите название книги.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (AuthorComboBox.SelectedItem is not Entities.Author author)
        {
            MessageBox.Show("Выберите автора.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (GenreComboBox.SelectedItem is not Entities.Genre genre)
        {
            MessageBox.Show("Выберите жанр.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (!int.TryParse(YearTextBox.Text, out var year) || year < 1000 || year > 2100)
        {
            MessageBox.Show("Введите корректный год издания.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (!int.TryParse(QuantityTextBox.Text, out var qty) || qty < 0)
        {
            MessageBox.Show("Введите корректное количество (0 или больше).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        using var db = DbContextFactory.Create();
        if (_bookId.HasValue)
        {
            var book = db.Books.Find(_bookId.Value);
            if (book != null)
            {
                book.Title = TitleTextBox.Text.Trim();
                book.AuthorId = author.Id;
                book.PublishYear = year;
                book.ISBN = string.IsNullOrWhiteSpace(IsbnTextBox.Text) ? null : IsbnTextBox.Text.Trim();
                book.GenreId = genre.Id;
                book.QuantityInStock = qty;
                db.SaveChanges();
            }
        }
        else
        {
            db.Books.Add(new Entities.Book
            {
                Title = TitleTextBox.Text.Trim(),
                AuthorId = author.Id,
                PublishYear = year,
                ISBN = string.IsNullOrWhiteSpace(IsbnTextBox.Text) ? null : IsbnTextBox.Text.Trim(),
                GenreId = genre.Id,
                QuantityInStock = qty
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
